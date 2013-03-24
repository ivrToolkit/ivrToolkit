/*
 * Copyright 2013 Troy Makaro
 *
 * This file is part of ivrToolkit, distributed under the GNU GPL. For full terms see the included COPYING file.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Threading;
using ivrToolkit.Core;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Util;

namespace ivrToolkit.SimulatorPlugin
{
    public delegate void HangupDelegate(object sender, EventArgs args);

    public class SimulatorLine : ILine
    {
        private int ringsGot = 0;
        private bool hungup = false;
        private bool stopped = false;
        private string _lastTerminator;

        //Thread signal.
        private static ManualResetEvent allDone = new ManualResetEvent(false);


        private LineStatusTypes _status = LineStatusTypes.OnHook;

        public LineStatusTypes Status
        {
            get { return _status; }
        }
        public string LastTerminator
        {
            get { return _lastTerminator; }
        }

        private int _lineNumber;

        public int LineNumber
        {
            get { return _lineNumber; }
        }

        List<char> digits = new List<char>();

        private object lockObject = new object();

        public SimulatorLine(int lineNumber) {
            this._lineNumber = lineNumber;
        } 

        // tell listeners that the software has manually hung up the phone
        public void Hangup()
        {
            _status = LineStatusTypes.OnHook;
            dispatchHangupEvent(); // tell virtual phone that the program performed a hangup
        }

        public void TakeOffHook()
        {
            _status = LineStatusTypes.OffHook;
        }

        public CallAnalysis Dial(string number, int answeringMachineLengthInMilliseconds)
        {
            if (stopped) resetAndThrowStop();
            if (hungup) resetAndThrowHangup();

            Phone phone = Phone.getPhone(number);
            if (phone == null)
            {
                return CallAnalysis.noAnswer;
            }
            CallAnalysis result = phone.dial(this);
            if (result == CallAnalysis.answeringMachine || result == CallAnalysis.connected)
            {
                _status = LineStatusTypes.Connected;
            }
            return result;
        }

        public void Close()
        {
            if (_status != LineStatusTypes.OnHook)
            {
                Hangup();
            }
        }

        public void WaitRings(int rings)
        {

            _status = LineStatusTypes.AcceptingCalls;
            ringsGot = 0;

            lock (lockObject)
            {
                while (true)
                {
                    if (stopped) resetAndThrowStop();
                    if (hungup)
                    {
                        ringsGot = 0;
                        reset();
                        _status = LineStatusTypes.AcceptingCalls;
                    }
                    if (ringsGot >= rings)
                    {
                        _status = LineStatusTypes.Connected;
                        // tell the thread that called sendRing method that it is ok to continue
                        allDone.Set();
                        return;
                    }

                    // tell the thread that called sendRing method that it is ok to continue
                    allDone.Set();

                    Monitor.Wait(lockObject); // wait indefinetly
                }
            }
        }
        private void playFinished(object sender, EventArgs args)
        {
            //object obj = DateTime.Now - test;
            lock (lockObject)
            {
                //Console.WriteLine(obj);
                isPlayFinished = true;
                Monitor.Pulse(lockObject);
            }
        }
        //private DateTime test;

        private bool isPlayFinished;

        public void PlayFile(string filename)
        {
            lock (lockObject)
            {
                if (stopped) resetAndThrowStop();
                if (hungup) resetAndThrowHangup();
                if (digits.Count != 0) return;

                isPlayFinished = false;
                using (WavPlayer player = new WavPlayer())
                {
                    player.subscribeFinished(new EventHandler(playFinished));
                    //test = DateTime.Now;
                    //Console.WriteLine(test + "|" + filename);
                    player.play(filename);

                    while (true)
                    {
                        if (stopped)
                        {
                            player.stop();
                            resetAndThrowStop();
                        }
                        if (hungup)
                        {
                            player.stop();
                            resetAndThrowHangup();
                        }
                        if (isPlayFinished)
                        {
                            //Console.WriteLine("finished");
                            return;
                        }
                        if (digits.Count != 0)
                        {
                            //Console.WriteLine("stopped");
                            player.stop();
                            return;
                        }

                        Monitor.Wait(lockObject);
                    } // loop
                }
            } // lock
        }

        public void RecordToFile(string filename)
        {
            if (stopped) resetAndThrowStop();
            if (hungup) resetAndThrowHangup();

            throw new NotImplementedException();
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
            digits = new List<char>();
            _status = LineStatusTypes.OnHook;
        }

        public string GetDigits(int numberOfDigits, string terminators)
        {
            int timeout = VoiceProperties.Current.DigitsTimeoutInMilli;
            lock (lockObject)
            {
                _lastTerminator = "";
                while (true)
                {
                    if (stopped) resetAndThrowStop();
                    if (hungup) resetAndThrowHangup();
                    if (haveResult(numberOfDigits, terminators)) return theDigits(numberOfDigits, terminators);

                    if (!Monitor.Wait(lockObject, timeout))
                    { // handle the timeout
                        if (terminators.IndexOf("T") != -1 && digits.Count != 0)
                        {
                            digits.Add('T'); // handle special timeout terminator
                        }
                        else
                        {
                            // there may be digits in the buffer that aren't terminators so flush them.
                            FlushDigitBuffer();
                            throw new GetDigitsTimeoutException();
                        }
                    }
                } // while
            } // lock
        }

        public string FlushDigitBuffer()
        {
            if (stopped) resetAndThrowStop();
            if (hungup) resetAndThrowHangup();

            string myDigits = theDigits();
            digits = new List<char>();
            return myDigits;
        }

        public void CheckStop()
        {
            if (stopped) resetAndThrowStop();
            if (hungup) resetAndThrowHangup();
        }

        public void Stop()
        {
            if (LineManager.GetLineCount() == 0)
            {
                SimulatorListener.singleton.stop();
            }
            sendStop();
        }

        //=========================================================

        private bool haveResult(int numberOfDigits, string terminators)
        {
            if (digits.Count == numberOfDigits) return true;
            foreach (char c in digits)
            {
                if (terminators.IndexOf(c) != -1) return true; 
            }
            return false;
        }

        private void pullOff(int count)
        {
            for (int intT = 0; intT < count; intT++)
            {
                digits.RemoveAt(0);
            }
        }
        private string theDigits(int numberOfDigits, string terminators)
        {
            string result = "";
            int index = 0;
            foreach (char c in digits)
            {
                index++;
                if (terminators.IndexOf(c) != -1)
                {
                    _lastTerminator = c.ToString();
                    pullOff(index);
                    return result;
                }
                result += c.ToString();
                if (index == numberOfDigits)
                {
                    pullOff(index);
                    return result;
                }
            }
            return result;
        }

        private string theDigits()
        {
            string result = "";
            foreach (char c in digits)
            {
                result += c.ToString();
            }
            digits.Clear();
            return result;
        }

        // used by the simulator client Phone.cs to send a digit to to the software monitoring the line
        public LineStatusTypes sendRing()
        {
            lock (lockObject)
            {
                // Set the event to nonsignaled state.
                allDone.Reset();

                ringsGot++;
                Monitor.Pulse(lockObject); // wake up thread

            }
            allDone.WaitOne(); // wait for the waitRings method to tell us it is ok to continue
            return _status;
        }

        // used by the simulator client Phone.cs to send a digit to to the software monitoring the line
        public void sendDigit(char digit)
        {
            lock (lockObject)
            {
                digits.Add(digit);
                Monitor.Pulse(lockObject);
            }
        }

        // used by the simulator client Phone.cs to send a digit to to the software monitoring the line
        public void sendHangup()
        {
            lock (lockObject)
            {
                hungup = true;
                Monitor.Pulse(lockObject);
            }
        }

        // used by the simulator client Phone.cs to send a digit to to the software monitoring the line
        public void sendStop()
        {
            lock (lockObject)
            {
                stopped = true;
                Monitor.Pulse(lockObject);
            }
        }

        #region handle hangup event
        private event HangupDelegate onHangup;

        public void subscribeToHangup(HangupDelegate eventHandler)
        {
            onHangup += eventHandler;
        }
        public void unsubscribeToHangup(HangupDelegate eventHandler)
        {
            onHangup -= eventHandler;
        }
        private void dispatchHangupEvent()
        {
            // make sure the are some delegates in the invocation list 
            if (onHangup != null)
            {
                onHangup(this, null);
            }
        }
 
	    #endregion

    } // class
}
