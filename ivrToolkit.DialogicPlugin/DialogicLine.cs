// 
// Copyright 2013 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the LESSER GNU GPL. For full terms see the included COPYING file and the COPYING.LESSER file.
// 
// 
using System;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Util;
using System.Threading;
using ivrToolkit.Core;

namespace ivrToolkit.DialogicPlugin
{
    public class DialogicLine : ILine
    {
        private readonly int _devh;
        private bool _hungup;
        private bool _stopped;

        internal DialogicLine(int devh, int lineNumber)
        {
            // can only instantiate this class from IVoice
            _devh = devh;
            LineNumber = lineNumber;
            SetDefaultFileType();
            DeleteCustomTones(); // uses dx_deltones() so I have to readd call progress tones. I also readd special tones
        }

        public string LastTerminator { get; private set; }

        public int LineNumber { get; private set; }

        private Dialogic.DX_XPB _currentXpb;

        public void WaitRings(int rings)
        {
            if (_stopped) ResetAndThrowStop();
            _status = LineStatusTypes.AcceptingCalls;
            Dialogic.WaitRings(_devh, rings);
            if (_stopped) ResetAndThrowStop();
        }

        public void Hangup()
        {
            _status = LineStatusTypes.OnHook;
            Dialogic.Hangup(_devh);
        }
        public void TakeOffHook()
        {
            _status = LineStatusTypes.OffHook;
            Dialogic.TakeOffHook(_devh);
        }

        public CallAnalysis Dial(string number, int answeringMachineLengthInMilliseconds)
        {
            if (_stopped) ResetAndThrowStop();

            Hangup();
            Thread.Sleep(2000);
            TakeOffHook();

            int dialToneTid = VoiceProperties.Current.DialTone.Tid;
            int noFreeLineTid = VoiceProperties.Current.NoFreeLineTone.Tid;

            bool dialToneEnabled = false;

            if (VoiceProperties.Current.PreTestDialTone)
            {
                dialToneEnabled = true;
                Dialogic.EnableTone(_devh, dialToneTid);
                int tid = Dialogic.ListenForCustomTones(_devh, 2);

                if (tid == 0)
                {
                    Dialogic.DisableTone(_devh, dialToneTid);
                    Hangup();
                    return CallAnalysis.NoDialTone;
                }
            }
            int index = number.IndexOf(',');
            if (VoiceProperties.Current.CustomOutboundEnabled && index != -1)
            {
                number = number.Substring(index+1).Replace(",",""); // there may be more than one comma

                if (!dialToneEnabled) Dialogic.EnableTone(_devh, dialToneTid);
                Dialogic.EnableTone(_devh, noFreeLineTid);

                // TODO send prefix

                // listen for tones
                var tid = Dialogic.ListenForCustomTones(_devh, 2);

                Dialogic.DisableTone(_devh, dialToneTid);
                Dialogic.DisableTone(_devh, noFreeLineTid);


                if (tid == 0)
                {
                    Hangup();
                    return CallAnalysis.NoDialTone;
                }
                if (tid == noFreeLineTid)
                {
                    Hangup();
                    return CallAnalysis.NoFreeLine;
                }
            }

            var result = Dialogic.DialWithCpa(_devh, number, answeringMachineLengthInMilliseconds);
            if (result == CallAnalysis.Stopped) ResetAndThrowStop();

            if (result == CallAnalysis.AnsweringMachine || result == CallAnalysis.Connected)
            {
                _status = LineStatusTypes.Connected;
            }
            else
            {
                Hangup();
            }
            return result;
        }
        public void Close()
        {
            if (_status != LineStatusTypes.OnHook)
            {
                Hangup();
            }
            Dialogic.Close(_devh);
        }

        private void SetDefaultFileType() {
            _currentXpb = new Dialogic.DX_XPB
                {
                    wFileFormat = Dialogic.FILE_FORMAT_WAVE,
                    wDataFormat = Dialogic.DATA_FORMAT_PCM,
                    nSamplesPerSec = Dialogic.DRT_8KHZ,
                    wBitsPerSample = 8
                };
        }

        public void PlayFile(string filename)
        {
            if (_stopped) ResetAndThrowStop();
            try
            {
                Dialogic.PlayFile(_devh, filename, "0123456789#*abcd", _currentXpb);
            }
            catch (StopException)
            {
                ResetAndThrowStop();
            }
            catch (HangupException)
            {
                ResetAndThrowHangup();
            }
        }

        public void RecordToFile(string filename)
        {
            if (_stopped) ResetAndThrowStop();
            try {
                Dialogic.RecordToFile(_devh, filename, "0123456789#*abcd", _currentXpb);
            }
            catch (StopException)
            {
                ResetAndThrowStop();
            }
            catch (HangupException)
            {
                ResetAndThrowHangup();
            }
        }

        /// <summary>
        /// Keep prompting for digits until number of digits is pressed or a terminator digit is pressed.
        /// </summary>
        /// <param name="numberOfDigits">Maximum number of digits allowed in the buffer.</param>
        /// <param name="terminators">Terminators</param>
        /// <returns>Returns the digits pressed not including the terminator if there was one</returns>
        public string GetDigits(int numberOfDigits, string terminators)
        {
            if (_stopped) ResetAndThrowStop();
            try {
                string answer = Dialogic.GetDigits(_devh, numberOfDigits, terminators);
                return StripOffTerminator(answer, terminators);
            }
            catch (StopException)
            {
                ResetAndThrowStop();
            }
            catch (HangupException)
            {
                ResetAndThrowHangup();
            }
            return null; // will never get here
        }

        /// <summary>
        /// Returns every character including the terminator
        /// </summary>
        /// <returns>All the digits in the buffer including terminators</returns>
        public string FlushDigitBuffer()
        {
            if (_stopped) ResetAndThrowStop();
            return Dialogic.FlushDigitBuffer(_devh);
        }

        private string StripOffTerminator(string answer, string terminators)
        {
            LastTerminator = "";
            if (answer.Length >= 1)
            {
                string lastDigit = answer.Substring(answer.Length - 1, 1);
                if (terminators != null & terminators != "")
                {
                    if (terminators.IndexOf(lastDigit, StringComparison.Ordinal) != -1)
                    {
                        LastTerminator = lastDigit;
                        answer = answer.Substring(0, answer.Length - 1);
                    }
                }
            }
            return answer;
        }
        private LineStatusTypes _status = LineStatusTypes.OnHook;
        public LineStatusTypes Status
        {
            get { return _status; }
        }

        public void CheckStop()
        {
            if (_stopped) ResetAndThrowStop();
            if (_hungup) ResetAndThrowHangup();
        }
        public void Stop()
        {
            _stopped = true;
            Dialogic.Stop(_devh);
        }

        private void ResetAndThrowStop()
        {
            Reset();
            throw new StopException();
        }
        private void ResetAndThrowHangup()
        {
            Reset();
            throw new HangupException();
        }
        private void Reset()
        {
            _hungup = false;
            _stopped = false;
            _status = LineStatusTypes.OnHook;
        }

        public void DeleteCustomTones()
        {
            Dialogic.DeleteTones(_devh);
            Dialogic.InitCallProgress(_devh);
            AddSpecialCustomTones();
        }

        private void AddSpecialCustomTones()
        {
            AddCustomTone(VoiceProperties.Current.DialTone);
            if (VoiceProperties.Current.CustomOutboundEnabled)
            {
                AddCustomTone(VoiceProperties.Current.NoFreeLineTone);
            }
        }


        public void AddCustomTone(CustomTone tone)
        {
            if (tone.ToneType == CustomToneType.Single)
            {
                // TODO
            }
            else if (tone.ToneType == CustomToneType.Dual)
            {
                Dialogic.AddDualTone(_devh, tone.Tid, tone.Freq1, tone.Frq1Dev, tone.Freq2, tone.Frq2Dev, tone.Mode);
            }
            else if (tone.ToneType == CustomToneType.DualWithCadence)
            {
                Dialogic.AddDualToneWithCadence(_devh, tone.Tid, tone.Freq1, tone.Frq1Dev, tone.Freq2, tone.Frq2Dev, tone.Ontime, tone.Ontdev, tone.Offtime,
                    tone.Offtdev, tone.Repcnt);
            }
            Dialogic.DisableTone(_devh, tone.Tid);
        }

        public void DisableTone(int tid)
        {
            Dialogic.DisableTone(_devh, tid);
        }

        public void EnableTone(int tid)
        {
            Dialogic.EnableTone(_devh, tid);
        }

    } // class
}
