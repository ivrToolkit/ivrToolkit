/*
 * Copyright 2013 Troy Makaro
 *
 * This file is part of ivrToolkit, distributed under the GNU GPL. For full terms see the included COPYING file.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Util;
using System.Threading;
using ivrToolkit.Core;

namespace ivrToolkit.DialogicPlugin
{
    public class DialogicLine : ILine
    {
        private int devh;
        private bool hungup = false;
        private bool stopped = false;
        private string _lastTerminator;

        public string lastTerminator
        {
            get { return _lastTerminator; }
        }
        private int _lineNumber;
        public int lineNumber
        {
            get
            {
                return _lineNumber;
            }
        }

        private Dialogic.DX_XPB currentXPB;

        public DialogicLine(int devh, int lineNumber)
        {
            this.devh = devh;
            this._lineNumber = lineNumber;
            setDefaultFileType();
            deleteCustomTones(); // uses dx_deltones() so I have to readd call progress tones. I also readd special tones
        }

        public void waitRings(int rings)
        {
            if (stopped) resetAndThrowStop();
            _status = LineStatusTypes.acceptingCalls;
            Dialogic.waitRings(devh, rings);
            if (stopped) resetAndThrowStop();
        }

        public void hangup()
        {
            _status = LineStatusTypes.onHook;
            Dialogic.hangup(devh);
        }
        public void takeOffHook()
        {
            _status = LineStatusTypes.offHook;
            Dialogic.takeOffHook(devh);
        }

        public CallAnalysis dial(string number, int answeringMachineLengthInMilliseconds)
        {
            if (stopped) resetAndThrowStop();

            hangup();
            Thread.Sleep(2000);
            takeOffHook();

            int dialToneTid = VoiceProperties.current.dialTone.tid;
            int noFreeLineTid = VoiceProperties.current.noFreeLineTone.tid;

            bool dialToneEnabled = false;

            if (VoiceProperties.current.preTestDialTone)
            {
                dialToneEnabled = true;
                Dialogic.enableTone(devh, dialToneTid);
                int tid = Dialogic.listenForCustomTones(devh, 2);

                if (tid == 0)
                {
                    Dialogic.disableTone(devh, dialToneTid);
                    hangup();
                    return CallAnalysis.noDialTone;
                }
            }
            int index = number.IndexOf(',');
            if (VoiceProperties.current.customOutboundEnabled && index != -1)
            {
                string prefix = number.Substring(0, index);
                number = number.Substring(index+1).Replace(",",""); // there may be more than one comma

                if (!dialToneEnabled) Dialogic.enableTone(devh, dialToneTid);
                Dialogic.enableTone(devh, noFreeLineTid);

                // TODO send prefix

                // listen for tones
                int tid = Dialogic.listenForCustomTones(devh, 2);

                Dialogic.disableTone(devh, dialToneTid);
                Dialogic.disableTone(devh, noFreeLineTid);


                if (tid == 0)
                {
                    hangup();
                    return CallAnalysis.noDialTone;
                }
                if (tid == noFreeLineTid)
                {
                    hangup();
                    return CallAnalysis.noFreeLine;
                }
            }

            CallAnalysis result = Dialogic.dialWithCPA(devh, number, answeringMachineLengthInMilliseconds);
            if (result == CallAnalysis.stopped) resetAndThrowStop();

            if (result == CallAnalysis.answeringMachine || result == CallAnalysis.connected)
            {
                _status = LineStatusTypes.connected;
            }
            else
            {
                hangup();
            }
            return result;
        }
        public void close()
        {
            if (_status != LineStatusTypes.onHook)
            {
                hangup();
            }
            Dialogic.close(devh);
        }

        private void setDefaultFileType() {
            currentXPB = new Dialogic.DX_XPB();
            currentXPB.wFileFormat = Dialogic.FILE_FORMAT_WAVE;
            currentXPB.wDataFormat = Dialogic.DATA_FORMAT_PCM;
            currentXPB.nSamplesPerSec = Dialogic.DRT_8KHZ;
            currentXPB.wBitsPerSample = 8;
        }

        public void playFile(string filename)
        {
            if (stopped) resetAndThrowStop();
            try
            {
                Dialogic.playFile(devh, filename, "0123456789#*abcd", currentXPB);
            }
            catch (StopException)
            {
                resetAndThrowStop();
            }
            catch (HangupException)
            {
                resetAndThrowHangup();
            }
        }

        public void recordToFile(string filename)
        {
            if (stopped) resetAndThrowStop();
            try {
                Dialogic.recordToFile(devh, filename, "0123456789#*abcd", currentXPB);
            }
            catch (StopException)
            {
                resetAndThrowStop();
            }
            catch (HangupException)
            {
                resetAndThrowHangup();
            }
        }

        /// <summary>
        /// Keep prompting for digits until number of digits is pressed or a terminator digit is pressed.
        /// </summary>
        /// <param name="numberOfDigits">Maximum number of digits allowed in the buffer.</param>
        /// <returns>Returns the digits pressed not including the terminator if there was one</returns>
        public string getDigits(int numberOfDigits, string terminators)
        {
            if (stopped) resetAndThrowStop();
            try {
                string answer = Dialogic.getDigits(devh, numberOfDigits, terminators);
                return stripOffTerminator(answer, terminators);
            }
            catch (StopException)
            {
                resetAndThrowStop();
            }
            catch (HangupException)
            {
                resetAndThrowHangup();
            }
            return null; // will never get here
        }

        /// <summary>
        /// Returns every character including the terminator
        /// </summary>
        /// <returns>All the digits in the buffer including terminators</returns>
        public string flushDigitBuffer()
        {
            if (stopped) resetAndThrowStop();
            return Dialogic.flushDigitBuffer(devh);
        }

        private string stripOffTerminator(string answer, string terminators)
        {
            _lastTerminator = "";
            if (answer.Length >= 1)
            {
                string lastDigit = answer.Substring(answer.Length - 1, 1);
                if (terminators != null & terminators != "")
                {
                    if (terminators.IndexOf(lastDigit) != -1)
                    {
                        _lastTerminator = lastDigit;
                        answer = answer.Substring(0, answer.Length - 1);
                    }
                }
            }
            return answer;
        }
        private LineStatusTypes _status = LineStatusTypes.onHook;
        public LineStatusTypes status
        {
            get { return _status; }
        }

        public void checkStop()
        {
            if (stopped) resetAndThrowStop();
            if (hungup) resetAndThrowHangup();
        }
        public void stop()
        {
            stopped = true;
            Dialogic.stop(devh);
        }

        private void resetAndThrowStop()
        {
            reset();
            throw new StopException();
        }
        private void resetAndThrowHangup()
        {
            reset();
            throw new HangupException();
        }
        private void reset()
        {
            hungup = false;
            stopped = false;
            _status = LineStatusTypes.onHook;
        }

        public void deleteCustomTones()
        {
            Dialogic.deleteTones(devh);
            Dialogic.initCallProgress(devh);
            addSpecialCustomTones();
        }

        private void addSpecialCustomTones()
        {
            addCustomTone(VoiceProperties.current.dialTone);
            if (VoiceProperties.current.customOutboundEnabled)
            {
                addCustomTone(VoiceProperties.current.noFreeLineTone);
            }
        }


        public void addCustomTone(CustomTone tone)
        {
            if (tone.toneType == CustomToneType.single)
            {
                // TODO
            }
            else if (tone.toneType == CustomToneType.dual)
            {
                Dialogic.addDualTone(devh, tone.tid, tone.freq1, tone.frq1dev, tone.freq2, tone.frq2dev, tone.mode);
            }
            else if (tone.toneType == CustomToneType.dualWithCadence)
            {
                Dialogic.addDualToneWithCadence(devh, tone.tid, tone.freq1, tone.frq1dev, tone.freq2, tone.frq2dev, tone.ontime, tone.ontdev, tone.offtime,
                    tone.offtdev, tone.repcnt);
            }
            Dialogic.disableTone(devh, tone.tid);
        }

        public void disableTone(int tid)
        {
            Dialogic.disableTone(devh, tid);
        }

        public void enableTone(int tid)
        {
            Dialogic.enableTone(devh, tid);
        }

    } // class
}
