// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 

using System;
using System.Collections.Generic;
using System.IO;
using ivrToolkit.Core;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Util;
using ivrToolkit.DialogicSipWrapper;
using NLog;
//Please note that the dll must exist in order for this using to work correctly.

namespace ivrToolkit.Plugin.Dialogic.Sip
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Dialogic : IVoice
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly object LockObject = new object();
        private static bool _isLibaryStarted;
        private static bool _libraryFailedToStart;

        public ILine GetLine(int lineNumber, object data = null)
        {
            var sip = new DialogicSip();

            var offset = VoiceProperties.Current.SipChannelOffset;
            Logger.Debug("lineNumber = {0}, offset = {1}",lineNumber, offset);

            var channel = lineNumber + offset;

            lock (LockObject)
            {
                if (_libraryFailedToStart) throw new Exception("SIP library previously failed to start");
                var lineCount = LineManager.GetLineCount();
                if (!_isLibaryStarted)
                {
                    var h323SignalingPort = VoiceProperties.Current.SipH323SignalingPort;
                    var sipSignalingPort = VoiceProperties.Current.SipSignalingPort;
                    var maxCalls = VoiceProperties.Current.MaxCalls;
                    var cppLogLevel = VoiceProperties.Current.CppLogLevel;
                    var logPath = Path.Combine(TenantSingleton.Instance.TenantDirectory, "logs","ADS_CPP_%N.log");

                    Logger.Info("Starting the Dialogic Libraries isLibraryStarted {0}, lineCount {1}", _isLibaryStarted, lineCount);
                    int result = sip.WStartLibraries(logPath, cppLogLevel, h323SignalingPort, sipSignalingPort, maxCalls);
                    if (result < 0)
                    {
                        _libraryFailedToStart = true;
                        var message = "The SIP driver has failed to initialize";
                        Logger.Error(message);
                        throw new Exception(message);
                    }
                    _isLibaryStarted = true;
                }
                
                sip.WOpen(lineNumber, offset);
            }

            var proxy = VoiceProperties.Current.SipProxyIp;
            var local = VoiceProperties.Current.SipLocalIp;
            var alias = VoiceProperties.Current.SipAlias;
            var password = VoiceProperties.Current.SipPassword;
            var realm = VoiceProperties.Current.SipRealm;
            /*
             * Please note that if the SIP Client is already registered
             * it will take 10 seconds to return
             */
            sip.WRegister(proxy, local, alias, password, realm);
            Logger.Debug("Registration Complete Channel {0}, Line {1}", channel, lineNumber);

            var devh = sip.WGetVoiceHandle();
            /*
             * This returns the line number not the channel.
             * There is no place for channel in the DialogicLine class.
             * This is irrelevant so long as the correct channel is opened above
             * using the sip.WOpen method.
             * 
             * This means that while the program will report line 1 it will in fact
             * correspond to the line + the offset for the channel.
             * 
             */
            return new DialogicLine(devh, lineNumber, sip);
        }

        internal static void Stop(int devh, DialogicSip sip)
        {
            Logger.Debug("Stop({0})",devh);
            if (dx_stopch(devh, EV_SYNC) == -1)
            {
                Logger.Debug("Got an error");
                var err = ATDV_ERRMSGP(devh);
                Logger.Debug("Error is {0}",err);
                throw new VoiceException(err);
            }

            Logger.Debug("Continuing with Stop({0})",devh);
            //Get Line Manager Count 
            //If Line Manager Count is less then or equal to 0
            //Then Stop the Dialogic Libraries
            lock (LockObject)
            {
                var lineCount = LineManager.GetLineCount();
                Logger.Debug("Line count is {0}", lineCount);
                /*
                 * The if statment below checks if the lineCount is 0 
                 * because the line is removed from the lineCount
                 * before this stop method is invoked.
                 */
                if (_isLibaryStarted && lineCount == 0)
                {
                    Logger.Info("Stopping the Dialogic Libraries isLibraryStarted {0}, lineCount {1}", _isLibaryStarted, lineCount);
                    sip.WStopLibraries();
                    _isLibaryStarted = false;
                }
            }
            Logger.Debug("Hopefully will be stopped ({0})",devh);
        }

        /// <summary>
        /// Puts the line on hook.
        /// </summary>
        /// <param name="devh">The handle for the Dialogic line.</param>
        /// <param name="sip"></param>
        internal static void Hangup(int devh, DialogicSip sip)
        {
            dx_stopch(devh, EV_SYNC);
            sip.WDropCall();
        }

        internal static void SetVolume(int devh, int size)
        {
            if (size < -10 || size > 10)
            {
                throw new VoiceException("size must be between -10 to 10");
            }
            var adjsize = (ushort)size;
            var result = dx_adjsv(devh, SV_VOLUMETBL, SV_ABSPOS, adjsize);
            if (result <= -1)
            {
                var error = ATDV_ERRMSGP(devh);
                throw new VoiceException(error);
            }
        }

        /// <summary>
        /// Dials a phone number using call progress analysis.
        /// </summary>
        /// <param name="devh">The handle for the Dialogic line.</param>
        /// <param name="number">The phone number to dial.</param>
        /// <param name="sip"></param>
        internal static void Dial(int devh, string number, DialogicSip sip)
        {
            var makeCallResult = sip.WMakeCall("ani", "dnis");
            Logger.Debug("Dial: Syncronous Make call Completed starting call process analysis");
            if (makeCallResult <= -1)
            {
                throw new VoiceException("Dialogic.Dial Failed");
            }
        }


        /// <summary>
        /// Dials a phone number using call progress analysis.
        /// </summary>
        /// <param name="devh">The handle for the Dialogic line.</param>
        /// <param name="number">The phone number to dial.</param>
        /// <param name="answeringMachineLengthInMilliseconds">Answering machine length in milliseconds</param>
        /// <param name="sip"></param>
        /// <returns>CallAnalysis Enum</returns>
        internal static Core.CallAnalysis DialWithCpa(int devh, string number, int answeringMachineLengthInMilliseconds, DialogicSip sip)
        {

            var cap = GetCap(devh);

            var proxy = VoiceProperties.Current.SipProxyIp;
            var alias = VoiceProperties.Current.SipAlias;
            var ani = alias + "@" + proxy;
            var dnis = number + "@" + proxy;

            sip.WMakeCall(ani, dnis);
            Logger.Debug("DialWithCpa: Syncronous Make call Completed starting call process analysis");

            var result = dx_dial(devh, "", ref cap, DX_CALLP | EV_SYNC);
            if (result <= -1)
            {
                var error = ATDV_ERRMSGP(devh);
                throw new VoiceException(error);
            }
            Logger.Debug("Call Progress Analysius Result {0}", result);
            var c = (CallAnalysis)result;
            switch (c)
            {
                case CallAnalysis.CR_BUSY:
                    return Core.CallAnalysis.Busy;
                case CallAnalysis.CR_CEPT:
                    return Core.CallAnalysis.OperatorIntercept;
                case CallAnalysis.CR_CNCT:
                    var connType = ATDX_CONNTYPE(devh);
                    switch (connType)
                    {
                        case CON_CAD:
                            Logger.Debug("Connection due to cadence break ");
                            break;
                        case CON_DIGITAL:
                            Logger.Debug("con_digital");
                            break;
                        case CON_LPC:
                            Logger.Debug("Connection due to loop current");
                            break;
                        case CON_PAMD:
                            Logger.Debug("Connection due to Positive Answering Machine Detection");
                            break;
                        case CON_PVD:
                            Logger.Debug("Connection due to Positive Voice Detection");
                            break;
                    }
                    var len = GetSalutationLength(devh);
                    if (len > answeringMachineLengthInMilliseconds)
                    {
                        return Core.CallAnalysis.AnsweringMachine;
                    }
                    return Core.CallAnalysis.Connected;
                case CallAnalysis.CR_ERROR:
                    return Core.CallAnalysis.Error;
                case CallAnalysis.CR_FAXTONE:
                    return Core.CallAnalysis.FaxTone;
                case CallAnalysis.CR_NOANS:
                    return Core.CallAnalysis.NoAnswer;
                case CallAnalysis.CR_NODIALTONE:
                    return Core.CallAnalysis.NoDialTone;
                case CallAnalysis.CR_NORB:
                    return Core.CallAnalysis.NoRingback;
                case CallAnalysis.CR_STOPD:
                    // calling method will check and throw the stopException
                    return Core.CallAnalysis.Stopped;
            }
            throw new VoiceException("Unknown dail response: "+result);
        }

        private static DX_CAP GetCap(int devh)
        {
            var cap = new DX_CAP();

            var result = dx_clrcap(ref cap);
            if (result <= -1)
            {
                var err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }

            var capType = typeof(DX_CAP);

            object boxed = cap;

            var caps = VoiceProperties.Current.GetKeyPrefixMatch("cap.");
            foreach (var capName in caps)
            {
                var info = capType.GetField(capName);
                if (info == null)
                {
                    throw new Exception("Could not find dx_cap."+capName);
                }
                var obj = info.GetValue(cap);
                if (obj is ushort)
                {
                    var value = ushort.Parse(VoiceProperties.Current.GetProperty("cap."+capName));
                    info.SetValue(boxed, value);
                }
                else if (obj is byte)
                {
                    var value = byte.Parse(VoiceProperties.Current.GetProperty("cap."+capName));
                    info.SetValue(boxed, value);
                }
            }

            return (DX_CAP)boxed;
        }

        /// <summary>
        /// Gets the greeting time in milliseconds.
        /// </summary>
        /// <param name="devh">The handle for the Dialogic line.</param>
        /// <returns>The greeting time in milliseconds.</returns>
        private static int GetSalutationLength(int devh)
        {
            var result = ATDX_ANSRSIZ(devh);
            if (result <= -1)
            {
                var err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
            return result * 10;
        }

        /// <summary>
        /// Closes the board line.
        /// </summary>
        /// <param name="devh">The handle for the Dialogic line.</param>
        /// <param name="sip"></param>
        internal static void Close(int devh, DialogicSip sip)
        {
            var result = dx_close(devh, 0);
            if (result <= -1)
            {
                var err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
            sip.WClose();
        }


        /// <summary>
        /// Returns every character including the terminator
        /// </summary>
        /// <param name="devh">The handle for the Dialogic line.</param>
        /// <param name="sip"></param>
        /// <returns>All the digits in the buffer including terminators</returns>
        internal static string FlushDigitBuffer(int devh, DialogicSip sip)
        {
            var all = "";
            try
            {
                // add "T" so that I can get all the characters.
                all = GetDigits(devh, 99, "T", 100, sip);
                // strip off timeout terminator if there is once
                if (all.EndsWith("T"))
                {
                    all = all.Substring(0, all.Length - 1);
                }
            }
            catch (GetDigitsTimeoutException)
            {
            }
            return all;
        }

        /// <summary>
        /// Keep prompting for digits until number of digits is pressed or a terminator digit is pressed.
        /// </summary>
        /// <param name="devh">The handle for the Dialogic line.</param>
        /// <param name="numberOfDigits">Maximum number of digits allowed in the buffer.</param>
        /// <param name="terminators">Terminator keys</param>
        /// <param name="sip"></param>
        /// <returns>Returns the digits pressed not including the terminator if there was one</returns>
        internal static string GetDigits(int devh, int numberOfDigits, string terminators, DialogicSip sip)
        {
            var timeout = VoiceProperties.Current.DigitsTimeoutInMilli;
            return GetDigits(devh, numberOfDigits, terminators, timeout, sip);
        }

        internal static string GetDigits(int devh, int numberOfDigits, string terminators, int timeout, DialogicSip sip)
        {
            Logger.Debug("NumberOfDigits: {0} terminators: {1} timeout: {2}",
                numberOfDigits, terminators, timeout);

            var state = ATDX_STATE(devh);
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("state: {0}", GetChannelStateDescription(state));
            }

            /*
             * If number of digits is 99 this will fail on SIP.
             * An invalid tpt error will be thrown.
             * I hacked this in place just to keep going with development.
             */
            if (numberOfDigits >= 15) numberOfDigits = 15;

            var tpt = GetTerminationConditions(numberOfDigits, terminators, timeout);

            DV_DIGIT digit;

            // Note: async does not work becaues digit is marshalled out immediately after dx_getdig is complete
            // not when event is found. Would have to use DV_DIGIT* and unsafe code. or another way?
            //var result = dx_getdig(devh, ref tpt[0], out digit, EV_SYNC);
            var result = dx_getdig(devh, ref tpt[0], out digit, EV_SYNC);
            if (result == -1)
            {
                var err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);

            }

            CheckCallState(sip);

            var reason = ATDX_TERMMSK(devh);
            Logger.Debug("Type = TDX_GETDIG, Reason = {0} = {1}", reason, GetReasonDescription(reason));

            if ((reason & TM_ERROR) == TM_ERROR)
            {
                throw new VoiceException("TM_ERROR");
            }
            if ((reason & TM_USRSTOP) == TM_USRSTOP)
            {
                throw new StopException();
            }
            if ((reason & TM_LCOFF) == TM_LCOFF)
            {
                throw new HangupException();
            }


            var answer = digit.dg_value;
            ClearDigits(devh); // not sure if this is necessary and perhaps only needed for getDigitsTimeoutException?
            if ((reason & TM_IDDTIME) == TM_IDDTIME)
            {
                if (terminators.IndexOf("t", StringComparison.Ordinal) != -1)
                {
                    answer += 't';
                }
                else
                {
                    throw new GetDigitsTimeoutException();
                }
            }


            ClearEventBuffer(devh);

            return answer;
        }

        private static void ClearDigits(int devh)
        {
            if (dx_clrdigbuf(devh) == -1)
            {
                Logger.Error("ClearDigits: Error");
                var err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
        }
        /*
         * Clear Out the Event Buffer by consuming all the events until the timeout is thrown to indicate that
         * no events are left in the event buffer.
         * 
         * This is the only way I found to reliably clear out the event buffer.  
         * This consumes events for the device until I receive an event timout.  
         * This ensures that no events are left in the buffer before I need to consume 
         * an event in another method (syncronously or asyncronously).
         */
        private static void ClearEventBuffer(int devh)
        {
            Logger.Debug("ClearEventBuffer()");
            var handler = 0;
            do
            {
                if (sr_waitevtEx(ref devh, 1, 50, ref handler) == -1)
                {
                    Logger.Trace("ClearEventBuffer: Timeout");
                    return;
                }

                /*
                 * Get the event
                 */
                var type = sr_getevttype((uint)handler);
                var reason = ATDX_TERMMSK(devh);
                Logger.Debug("ClearEventBuffer: Type = {0}, Reason = {1} = {2}", GetEventTypeDescription(type), reason, GetReasonDescription(reason));
            } while (true);
        }

        private static string GetEventTypeDescription(int type)
        {
            switch (type)
            {
                case TDX_PLAY:
                    return "Play Completed";
                case TDX_RECORD:
                    return "Record Complete";
                case TDX_GETDIG:
                    return "Get Digits Completed";
                case TDX_DIAL:
                    return "Dial Completed";
                case TDX_CALLP:
                    return "Call Progress Completed";
                case TDX_CST:
                    return "CST Event Received";
                case TDX_SETHOOK:
                    return "SetHook Completed";
                case TDX_WINK:
                    return "Wink Completed";
                case TDX_ERROR:
                    return "Error Event";
                case TDX_PLAYTONE:
                    return "Play Tone Completed";
                case TDX_GETR2MF:
                    return "Get R2MF completed";
                case TDX_BARGEIN:
                    return "Barge in completed";
                case TDX_NOSTOP:
                    return "No Stop needed to be Issued";
                case TDX_UNKNOWN:
                    return "TDX_UNKNOWN";
            }

            return type.ToString();
        }

        private static string GetReasonDescription(int reason)
        {
            List<string> list = new List<string>();
            if ((reason & TM_NORMTERM) == TM_NORMTERM) list.Add("Normal Termination");
            if ((reason & TM_MAXDTMF) == TM_MAXDTMF) list.Add("Max Number of Digits Recd");
            if ((reason & TM_MAXSIL) == TM_MAXSIL) list.Add("Max Silence");
            if ((reason & TM_MAXNOSIL) == TM_MAXNOSIL) list.Add("Max Non-Silence");
            if ((reason & TM_LCOFF) == TM_LCOFF) list.Add("Loop Current Off");
            if ((reason & TM_IDDTIME) == TM_IDDTIME) list.Add("Inter Digit Delay");
            if ((reason & TM_MAXTIME) == TM_MAXTIME) list.Add("Max Function Time Exceeded");
            if ((reason & TM_DIGIT) == TM_DIGIT) list.Add("Digit Mask or Digit Type Term.");
            if ((reason & TM_PATTERN) == TM_PATTERN) list.Add("Pattern Match Silence Off");
            if ((reason & TM_USRSTOP) == TM_USRSTOP) list.Add("Function Stopped by User");
            if ((reason & TM_EOD) == TM_EOD) list.Add("End of Data Reached on Playback");
            if ((reason & TM_TONE) == TM_TONE) list.Add("Tone On/Off Termination");
            if ((reason & TM_BARGEIN) == TM_BARGEIN) list.Add("Play terminated due to Barge-in");
            if ((reason & TM_ERROR) == TM_ERROR) list.Add("I/O Device Error");
            if ((reason & TM_MAXDATA) == TM_MAXDATA) list.Add("Max Data reached for FSK");
            return string.Join("|",list.ToArray());
        }


        /*
         * Checks the call state.
         * If the call is no longer connected (call_state == 4) 
         * then drop the call.
         */
        private static void CheckCallState(DialogicSip sip)
        {
            var callState = sip.WGetCallState();
            Logger.Debug("CheckCallState: Call State {0}", GetCallStateDescription(callState));
            if (callState != 4)
            {
                Logger.Debug("CheckCallState: The call has been hang up.");
                throw new HangupException();

            }
        }

        private static string GetChannelStateDescription(int channelState)
        {
            switch (channelState)
            {
                case 1:
                    return "Channel is idle";
                case 2:
                    return "Channel is playing back";
                case 3:
                    return "Channel is recording";
                case 4:
                    return "Channel is dialing";
                case 5:
                    return "Channel is getting digits";
                case 6:
                    return "Channel is generating a tone";
                case 7:
                    return "Operation has terminated";
                case 8:
                    return "Channel is sending a fax";
                case 9:
                    return "Channel is receiving a fax";
                case 10:
                    return "Channel is between fax pages";
                case 11:
                    return "A change in hookstate is in progress";
                case 12:
                    return "A wink operation is in progress";
                case 13:
                    return "Channel is Call Progress Mode";
                case 14:
                    return "Channel is Getting R2MF";
                case 15:
                    return "Call status Rings state";
                case 16:
                    return "Channel is blocked";
                case 17:
                    return "Channel is preparing record and driver has not yet sent record";

            }

            return $"Unknown channel: {channelState}";
        }

        // todo There are defined in the GcLibDef.cs file in the analog plugin. Need to consider solidation

        private static string GetCallStateDescription(int callState)
        {
            switch (callState)
            {
                case 0x00:
                    return "GCST_NULL";
                case 0x01:
                    return "GCST_ACCEPTED";
                case 0x02:
                    return "GCST_ALERTING";
                case 0x04:
                    return "GCST_CONNECTED";
                case 0x08:
                    return "GCST_OFFERED";
                case 0x10:
                    return "GCST_DIALING";
                case 0x20:
                    return "GCST_IDLE";
                case 0x40:
                    return "GCST_DISCONNECTED";
                case 0x80:
                    return "GCST_DIALTONE";
                case 0x100:
                    return "GCST_ONHOLDPENDINGTRANSFER";
                case 0x200:
                    return "GCST_ONHOLD";
                case 0x400:
                    return "GCST_DETECTED";
                case 0x800:
                    return "GCST_PROCEEDING";
                case 0x1000:
                    return "GCST_SENDMOREINFO";
                case 0x2000:
                    return "GCST_GETMOREINFO";
                case 0x4000:
                    return "GCST_CALLROUTING";
            }

            return callState.ToString();
        }

        private static DV_TPT[] GetTerminationConditions(int numberOfDigits, string terminators, int timeoutInMilliseconds)
        {
            var tpts = new List<DV_TPT>();

            var tpt = new DV_TPT
                {
                    tp_type = IO_CONT,
                    tp_termno = DX_MAXDTMF,
                    tp_length = (ushort) numberOfDigits,
                    tp_flags = TF_MAXDTMF,
                    tp_nextp = IntPtr.Zero
                };
            tpts.Add(tpt);

            var bitMask = DefineDigits(terminators);
            if (bitMask != 0)
            {
                tpt = new DV_TPT
                    {
                        tp_type = IO_CONT,
                        tp_termno = DX_DIGMASK,
                        tp_length = (ushort) bitMask,
                        tp_flags = TF_DIGMASK,
                        tp_nextp = IntPtr.Zero
                    };
                tpts.Add(tpt);
            }
            if (timeoutInMilliseconds != 0)
            {
                tpt = new DV_TPT
                    {
                        tp_type = IO_CONT,
                        tp_termno = DX_IDDTIME,
                        tp_length = (ushort) (timeoutInMilliseconds/100),
                        tp_flags = TF_IDDTIME,
                        tp_nextp = IntPtr.Zero
                    };
                tpts.Add(tpt);
            }

            tpt = new DV_TPT
                {
                    tp_type = IO_EOT,
                    tp_termno = DX_LCOFF,
                    tp_length = 3,
                    tp_flags = TF_LCOFF | TF_10MS,
                    tp_nextp = IntPtr.Zero
                };
            tpts.Add(tpt);

            return tpts.ToArray();
        }

        private static int DefineDigits(string digits)
        {
            var result = 0;

            if (digits == null) digits = "";

            var all = digits.Trim().ToLower();
            var chars = all.ToCharArray();
            foreach (var c in chars)
            {
                switch (c)
                {
                    case '0':
                        result = result | DM_0;
                        break;
                    case '1':
                        result = result | DM_1;
                        break;
                    case '2':
                        result = result | DM_2;
                        break;
                    case '3':
                        result = result | DM_3;
                        break;
                    case '4':
                        result = result | DM_4;
                        break;
                    case '5':
                        result = result | DM_5;
                        break;
                    case '6':
                        result = result | DM_6;
                        break;
                    case '7':
                        result = result | DM_7;
                        break;
                    case '8':
                        result = result | DM_8;
                        break;
                    case '9':
                        result = result | DM_9;
                        break;
                    case 'a':
                        result = result | DM_A;
                        break;
                    case 'b':
                        result = result | DM_B;
                        break;
                    case 'c':
                        result = result | DM_C;
                        break;
                    case 'd':
                        result = result | DM_D;
                        break;
                    case '#':
                        result = result | DM_P;
                        break;
                    case '*':
                        result = result | DM_S;
                        break;
                }
            }
            return result;
        }


        /// <summary>
        /// Play a vox or wav file.
        /// </summary>
        /// <param name="devh">The handle for the Dialogic line.</param>
        /// <param name="filename">The name of the file to play.</param>
        /// <param name="terminators">Terminator keys</param>
        /// <param name="xpb">The format of the vox or wav file.</param>
        /// <param name="sip"></param>
        internal static void PlayFile(int devh, string filename, string terminators, DX_XPB xpb, DialogicSip sip)
        {

            /* set up DV_TPT */
            var tpt = GetTerminationConditions(10, terminators,0);

            var iott = new DX_IOTT {io_type = IO_DEV | IO_EOT, io_bufp = null, io_offset = 0, io_length = -1};
            /* set up DX_IOTT */
            if ((iott.io_fhandle = dx_fileopen(filename, _O_RDONLY | _O_BINARY)) == -1)
            {
                var fileErr = dx_fileerrno();

                var err = "";

                switch (fileErr)
                {
                    case EACCES:
                        err = "Tried to open read-only file for writing, file's sharing mode does not allow specified operations, or given path is directory.";
                        break;
                    case EEXIST:
                        err = "_O_CREAT and _O_EXCL flags specified, but filename already exists.";
                        break;
                    case EINVAL:
                        err = "Invalid oflag or pmode argument.";
                        break;
                    case EMFILE:
                        err = "No more file descriptors available (too many open files).";
                        break;
                    case ENOENT:
                        err = "File or path not found.";
                        break;
                }
                err += " File: |"+filename+"|";

                //I don't think this is needed when we get an error opening a file
                //dx_fileclose(iott.io_fhandle);

                throw new VoiceException(err);
            }
            /*
             * It appears as if digits or something else is still in the buffer and the play file is getting skipped.
             * This did nothing.
             */
            ClearEventBuffer(devh);
            /*
             * This might have been the fix for the digits problem.
             */
            //ClearDigits(devh);

            var state = ATDX_STATE(devh);
            Logger.Debug("About to play: {0} state: {1}",filename,state);
            //Double Check this code tomorrow.
            if (!File.Exists(filename)){
                var err = $"File {filename} does not exist so it cannot be played, call will be droped.";
                Logger.Error(err);
                sip.WDropCall();
                throw new VoiceException(err);
            }

            /* Now play the file */
            if (dx_playiottdata(devh, ref iott, ref tpt[0], ref xpb, EV_ASYNC) == -1)
            {
                Logger.Error("Tried to play: {0} state: {1}", filename, state);

                var err = ATDV_ERRMSGP(devh);
                dx_fileclose(iott.io_fhandle);
                throw new VoiceException(err);
            }
            /*
             * Clear Digits Buffer 2
             * This might have been the fix for the digits problem.
             * I am unsure if I need to do this after I play a file or if doing it (Clear Digits Buffer 1) before play file is sufficent.
             * Further testing tomorrow will resolve this question.
             */
            //ClearDigits(devh);

            var handler = 0;

            while (true)
            {
                // This code has a timeout so that if the user hangs up while playing a file it can be detected.
                sr_waitevtEx(ref devh, 1, 5000, ref handler);

                //Check if the call is still connected
                try
                {
                    CheckCallState(sip);
                }
                catch (HangupException)
                {
                    dx_fileclose(iott.io_fhandle);
                    Logger.Debug("Hangup Exception : The file handle has been closed because the call has been hung up.");
                    throw new HangupException("Hangup Exception call has been hungup.");
                }

                var type = sr_getevttype((uint)handler);
                //Ignore events (including timeout events) that are not of they type we want.
                //Double Check this code tomorrow.
                if (type != TDX_PLAY)
                {
                    continue;
                }

                // make sure the file is closed
                if (dx_fileclose(iott.io_fhandle) == -1)
                {
                    var err = ATDV_ERRMSGP(devh);
                    throw new VoiceException(err);
                }

                var reason = ATDX_TERMMSK(devh);

                Logger.Debug("Type = TDX_PLAY, Reason = {0} = {1}", reason, GetReasonDescription(reason));
                if ((reason & TM_ERROR) == TM_ERROR)
                {
                    throw new VoiceException("TM_ERROR");
                }
                if ((reason & TM_USRSTOP) == TM_USRSTOP)
                {
                    throw new StopException();
                }
                if ((reason & TM_LCOFF) == TM_LCOFF)
                {
                    throw new HangupException();
                }
                return;
            } // while

        }



        internal static void AddDualTone(int devh, int tid, int freq1, int fq1Dev, int freq2, int fq2Dev,
            ToneDetection mode)
        {
            var dialogicMode = mode == ToneDetection.Leading ? TN_LEADING : TN_TRAILING;

            if (dx_blddt((uint)tid, (uint)freq1, (uint)fq1Dev, (uint)freq2, (uint)fq2Dev, dialogicMode) == -1)
            {
                throw new VoiceException("unable to build dual tone");
            }
            if (dx_addtone(devh, 0, 0) == -1)
            {
                var err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
        }
        //T5=480,30,620,40,25,5,25,5,2 fast busy
        //T6=350,20,440,20,L dial tone

        internal static void AddDualToneWithCadence(int devh, int tid, int freq1, int fq1Dev, int freq2, int fq2Dev,
            int ontime, int ontdev, int offtime, int offtdev, int repcnt)
        {
            if (dx_blddtcad((uint)tid, (uint)freq1, (uint)fq1Dev, (uint)freq2, (uint)fq2Dev, (uint)ontime, (uint)ontdev, (uint)offtime, (uint)offtdev, (uint)repcnt) == -1)
            {
                throw new VoiceException("unable to build dual tone cadence");
            }
            if (dx_addtone(devh, 0, 0) == -1)
            {
                var err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
        }

        internal static void DisableTone(int devh, int tid)
        {
            if (dx_distone(devh, tid, DM_TONEON | DM_TONEOFF) == -1)
            {
                var err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
        }

        internal static void EnableTone(int devh, int tid)
        {
            if (dx_enbtone(devh, tid, DM_TONEON | DM_TONEOFF) == -1)
            {
                var err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
        }

        internal static int ListenForCustomTones(int devh, int timeoutSeconds)
        {
            var eblk = new DX_EBLK();
            if (dx_getevt(devh, ref eblk, timeoutSeconds) == -1)
            {
                if (ATDV_LASTERR(devh) == EDX_TIMEOUT)
                {
                    return 0;
                }
                var err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
            if (eblk.ev_event == DE_TONEON || eblk.ev_event == DE_TONEOFF)
            {
                return eblk.ev_data;
            }
            return 0;
        }

        /// <summary>
        /// Record a vox or wav file.
        /// </summary>
        /// <param name="devh">The handle for the Dialogic line.</param>
        /// <param name="filename">The name of the file to play.</param>
        /// <param name="terminators">Terminator keys</param>
        /// <param name="xpb">The format of the vox or wav file.</param>
        /// <param name="timeoutMilli">Number of milliseconds before timeout</param>
        /// <param name="sip"></param>
        internal static void RecordToFile(int devh, string filename, string terminators, DX_XPB xpb, int timeoutMilli, DialogicSip sip)
        {

            FlushDigitBuffer(devh, sip);

            /* set up DV_TPT */
            var tpt = GetTerminationConditions(1, terminators, timeoutMilli);

            var iott = new DX_IOTT {io_type = IO_DEV | IO_EOT, io_bufp = null, io_offset = 0, io_length = -1};
            /* set up DX_IOTT */
            if ((iott.io_fhandle = dx_fileopen(filename, _O_CREAT | _O_BINARY | _O_RDWR, _S_IWRITE)) == -1)
            {
                var fileErr = dx_fileerrno();

                var err = "";

                switch (fileErr)
                {
                    case EACCES:
                        err = "Tried to open read-only file for writing, file's sharing mode does not allow specified operations, or given path is directory.";
                        break;
                    case EEXIST:
                        err = "_O_CREAT and _O_EXCL flags specified, but filename already exists.";
                        break;
                    case EINVAL:
                        err = "Invalid oflag or pmode argument.";
                        break;
                    case EMFILE:
                        err = "No more file descriptors available (too many open files).";
                        break;
                    case ENOENT:
                        err = "File or path not found.";
                        break;
                }

                dx_fileclose(iott.io_fhandle);

                throw new VoiceException(err);
            }

            /* Now record the file */
            if (dx_reciottdata(devh, ref iott, ref tpt[0], ref xpb, RM_TONE | EV_ASYNC) == -1)
            {
                var err = ATDV_ERRMSGP(devh);
                dx_fileclose(iott.io_fhandle);
                throw new VoiceException(err);
            }

            var handler = 0;

            while (true)
            {
                // This code has a timeout so that if the user hangs up while recording there name it can be detected.
                sr_waitevtEx(ref devh, 1, 5000, ref handler);

                //Check if the call is still connected
                CheckCallState(sip);

                var type = sr_getevttype((uint) handler);
                //Ignore events that are not of they type we want.
                if (type != TDX_RECORD)
                {
                    continue;
                }

                if (dx_fileclose(iott.io_fhandle) == -1)
                {
                    var err = ATDV_ERRMSGP(devh);
                    throw new VoiceException(err);
                }

                var reason = ATDX_TERMMSK(devh);
                Logger.Debug("Type = TDX_RECORD, Reason = {0} = {1}", reason, GetReasonDescription(reason));
                if ((reason & TM_ERROR) == TM_ERROR)
                {
                    throw new VoiceException("TM_ERROR");
                }

                if ((reason & TM_USRSTOP) == TM_USRSTOP)
                {
                    throw new StopException();
                }

                if ((reason & TM_LCOFF) == TM_LCOFF)
                {
                    throw new HangupException();
                }

                FlushDigitBuffer(devh, sip);
                return;
            }

        }
    } // class
} // namespace
