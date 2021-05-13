// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using ivrToolkit.Core;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Util;

namespace ivrToolkit.SimulatorPlugin
{
    public delegate void HangupDelegate(object sender, EventArgs args);

    public class SimulatorLine : ILine
    {
        private int _ringsGot;
        private bool _hungup;
        private bool _stopped;

        //Thread signal.
        private static readonly ManualResetEvent AllDone = new ManualResetEvent(false);


        private LineStatusTypes _status = LineStatusTypes.OnHook;

        public LineStatusTypes Status
        {
            get { return _status; }
        }

        public string LastTerminator { get; private set; }

        private readonly int _lineNumber;

        public int LineNumber
        {
            get { return _lineNumber; }
        }

        List<char> _digits = new List<char>();

        private readonly object _lockObject = new object();

        public SimulatorLine(int lineNumber) {
            _lineNumber = lineNumber;
        } 

        // tell listeners that the software has manually hung up the phone
        public void Hangup()
        {
            _status = LineStatusTypes.OnHook;
            DispatchHangupEvent(); // tell virtual phone that the program performed a hangup
        }

        public void TakeOffHook()
        {
            _status = LineStatusTypes.OffHook;
        }

        public CallAnalysis Dial(string number, int answeringMachineLengthInMilliseconds)
        {
            if (_stopped) ResetAndThrowStop();
            if (_hungup) ResetAndThrowHangup();

            var phone = Phone.GetPhone(number);
            if (phone == null)
            {
                return CallAnalysis.NoAnswer;
            }
            var result = phone.Dial(this);
            if (result == CallAnalysis.AnsweringMachine || result == CallAnalysis.Connected)
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
            _ringsGot = 0;

            lock (_lockObject)
            {
                while (true)
                {
                    if (_stopped) ResetAndThrowStop();
                    if (_hungup)
                    {
                        _ringsGot = 0;
                        Reset();
                        _status = LineStatusTypes.AcceptingCalls;
                    }
                    if (_ringsGot >= rings)
                    {
                        _status = LineStatusTypes.Connected;
                        // tell the thread that called sendRing method that it is ok to continue
                        AllDone.Set();
                        return;
                    }

                    // tell the thread that called sendRing method that it is ok to continue
                    AllDone.Set();

                    Monitor.Wait(_lockObject); // wait indefinetly
                }
            }
        }
        private void PlayFinished(object sender, EventArgs args)
        {
            //object obj = DateTime.Now - test;
            lock (_lockObject)
            {
                //Console.WriteLine(obj);
                _isPlayFinished = true;
                Monitor.Pulse(_lockObject);
            }
        }
        //private DateTime test;

        private bool _isPlayFinished;

        public void PlayFile(string filename)
        {
            lock (_lockObject)
            {
                if (_stopped) ResetAndThrowStop();
                if (_hungup) ResetAndThrowHangup();
                if (_digits.Count != 0) return;

                _isPlayFinished = false;
                using (var player = new WavPlayer())
                {
                    player.SubscribeFinished(PlayFinished);
                    //test = DateTime.Now;
                    //Console.WriteLine(test + "|" + filename);
                    player.Play(filename);

                    while (true)
                    {
                        if (_stopped)
                        {
                            player.Stop();
                            ResetAndThrowStop();
                        }
                        if (_hungup)
                        {
                            player.Stop();
                            ResetAndThrowHangup();
                        }
                        if (_isPlayFinished)
                        {
                            //Console.WriteLine("finished");
                            return;
                        }
                        if (_digits.Count != 0)
                        {
                            //Console.WriteLine("stopped");
                            player.Stop();
                            return;
                        }

                        Monitor.Wait(_lockObject);
                    } // loop
                }
            } // lock
        }

        public void RecordToFile(string filename)
        {
            if (_stopped) ResetAndThrowStop();
            if (_hungup) ResetAndThrowHangup();

            throw new NotImplementedException();
        }

        public void RecordToFile(string filename, int timeoutMilliseconds)
        {
            if (_stopped) ResetAndThrowStop();
            if (_hungup) ResetAndThrowHangup();

            throw new NotImplementedException();
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
            _digits = new List<char>();
            _status = LineStatusTypes.OnHook;
        }

        public string GetDigits(int numberOfDigits, string terminators)
        {
            var timeout = VoiceProperties.Current.DigitsTimeoutInMilli;
            lock (_lockObject)
            {
                LastTerminator = "";
                while (true)
                {
                    if (_stopped) ResetAndThrowStop();
                    if (_hungup) ResetAndThrowHangup();
                    if (HaveResult(numberOfDigits, terminators)) return TheDigits(numberOfDigits, terminators);

                    if (!Monitor.Wait(_lockObject, timeout))
                    { // handle the timeout
                        if (terminators.IndexOf("T", StringComparison.Ordinal) != -1 && _digits.Count != 0)
                        {
                            _digits.Add('T'); // handle special timeout terminator
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
            if (_stopped) ResetAndThrowStop();
            if (_hungup) ResetAndThrowHangup();

            var myDigits = TheDigits();
            _digits = new List<char>();
            return myDigits;
        }

        public void CheckStop()
        {
            if (_stopped) ResetAndThrowStop();
            if (_hungup) ResetAndThrowHangup();
        }

        public void Stop()
        {
            if (LineManager.GetLineCount() == 0)
            {
                SimulatorListener.Singleton.Stop();
            }
            SendStop();
        }

        //=========================================================

        private bool HaveResult(int numberOfDigits, string terminators)
        {
            if (_digits.Count == numberOfDigits) return true;
            return _digits.Any(c => terminators.IndexOf(c) != -1);
        }

        private void PullOff(int count)
        {
            for (var intT = 0; intT < count; intT++)
            {
                _digits.RemoveAt(0);
            }
        }
        private string TheDigits(int numberOfDigits, string terminators)
        {
            var result = "";
            var index = 0;
            foreach (var c in _digits)
            {
                index++;
                if (terminators.IndexOf(c) != -1)
                {
                    LastTerminator = c.ToString(CultureInfo.InvariantCulture);
                    PullOff(index);
                    return result;
                }
                result += c.ToString(CultureInfo.InvariantCulture);
                if (index == numberOfDigits)
                {
                    PullOff(index);
                    return result;
                }
            }
            return result;
        }

        private string TheDigits()
        {
            var result = _digits.Aggregate("", (current, c) => current + c.ToString(CultureInfo.InvariantCulture));
            _digits.Clear();
            return result;
        }

        // used by the simulator client Phone.cs to send a digit to to the software monitoring the line
        public LineStatusTypes SendRing()
        {
            lock (_lockObject)
            {
                // Set the event to nonsignaled state.
                AllDone.Reset();

                _ringsGot++;
                Monitor.Pulse(_lockObject); // wake up thread

            }
            AllDone.WaitOne(); // wait for the waitRings method to tell us it is ok to continue
            return _status;
        }

        // used by the simulator client Phone.cs to send a digit to to the software monitoring the line
        public void SendDigit(char digit)
        {
            lock (_lockObject)
            {
                _digits.Add(digit);
                Monitor.Pulse(_lockObject);
            }
        }

        // used by the simulator client Phone.cs to send a digit to to the software monitoring the line
        public void SendHangup()
        {
            lock (_lockObject)
            {
                _hungup = true;
                Monitor.Pulse(_lockObject);
            }
        }

        // used by the simulator client Phone.cs to send a digit to to the software monitoring the line
        public void SendStop()
        {
            lock (_lockObject)
            {
                _stopped = true;
                Monitor.Pulse(_lockObject);
            }
        }


        private event HangupDelegate OnHangup;

        public void SubscribeToHangup(HangupDelegate eventHandler)
        {
            OnHangup += eventHandler;
        }
        public void UnsubscribeToHangup(HangupDelegate eventHandler)
        {
            OnHangup -= eventHandler;
        }
        private void DispatchHangupEvent()
        {
            // make sure the are some delegates in the invocation list 
            if (OnHangup != null)
            {
                OnHangup(this, null);
            }
        }

        public int Volume { get; set; }
    } // class
}
