// 
// Copyright 2013 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the LESSER GNU GPL. For full terms see the included COPYING file and the COPYING.LESSER file.
// 
// 
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices.ComTypes;
using ivrToolkit.Core;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Util;
using System.Reflection;
using NLog;

namespace ivrToolkit.DialogicPlugin
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Dialogic : IVoice
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ILine GetLine(int lineNumber)
        {
            // TODO fix this
            var devh = OpenDevice("dxxxB1C" + lineNumber.ToString(CultureInfo.InvariantCulture));
            return new DialogicLine(devh, lineNumber);
        }

        /// <summary>
        /// Opens the board line.
        /// </summary>
        /// <param name="devname">Name of the board line. For example: dxxxB1C1</param>
        /// <returns>The device handle</returns>
        private static int OpenDevice(string devname)
        {
            var devh = dx_open(devname, 0);
            if (devh <= -1)
            {
                var err = string.Format("Could not get device handle for device {0}", devname);
                throw new VoiceException(err);
            }
            return devh;
        }

        internal static void WaitRings(int devh, int rings)
        {
            if (dx_wtring(devh, rings, (int)HookState.OFF_HOOK, -1) == -1)
            {
                var err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
        }

        internal static void Stop(int devh)
        {
            if (dx_stopch(devh, EV_SYNC) == -1)
            {
                var err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
        }

        /// <summary>
        /// Puts the line on hook.
        /// </summary>
        /// <param name="devh">The handle for the Dialogic line.</param>
        internal static void Hangup(int devh)
        {
            dx_stopch(devh, EV_SYNC);

            var result = dx_sethook(devh, (int)HookState.ON_HOOK, EV_SYNC);
            if (result <= -1)
            {
                var err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
        }

        /// <summary>
        /// Takes the line off hook.
        /// </summary>
        /// <param name="devh">The handle for the Dialogic line.</param>
        internal static void TakeOffHook(int devh)
        {
            var result = dx_sethook(devh, (int)HookState.OFF_HOOK, EV_SYNC);
            if (result <= -1)
            {
                var err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
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
        internal static void Dial(int devh, string number)
        {
            var cap = GetCap(devh);

            var result = dx_dial(devh, number, ref cap, EV_SYNC);
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
        /// <param name="answeringMachineLengthInMilliseconds">Answering machine length in milliseconds</param>
        /// <returns>CallAnalysis Enum</returns>
        internal static Core.CallAnalysis DialWithCpa(int devh, string number, int answeringMachineLengthInMilliseconds)
        {

            var cap = GetCap(devh);

            var fullNumber = VoiceProperties.Current.DialToneType + number;
            var result = dx_dial(devh, fullNumber, ref cap, DX_CALLP | EV_SYNC);
            if (result <= -1)
            {
                var error = ATDV_ERRMSGP(devh);
                throw new VoiceException(error);
            }
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

        private static int GetTid(string tidName)
        {
            int value;
            if (int.TryParse(tidName, out value))
            {
                return value;
            }
            var fi =
                typeof(Dialogic).GetField(tidName, BindingFlags.Static | BindingFlags.NonPublic);
            if (fi != null)
            {
                var obj = fi.GetValue(null);
                if (obj is Int32)
                {
                    return (int)obj;
                }
            }
            throw new Exception("tid name is not found: "+tidName);
        }

        internal static void InitCallProgress(int devh)
        {
            var toneParams = VoiceProperties.Current.GetPrefixMatch("cpa.tone.");

            foreach (var tone in toneParams)
            {
                var part = tone.Split(',');
                var t = new Tone_T
                {
                    str = part[0].Trim(),
                    tid = GetTid(part[1].Trim()),
                    freq1 = new Freq_T
                    {
                        freq = int.Parse(part[2].Trim()),
                        deviation = int.Parse(part[3].Trim())
                    },
                    freq2 = new Freq_T
                        {
                        freq = int.Parse(part[4].Trim()),
                        deviation = int.Parse(part[5].Trim())
                    },
                    on = new State_T
                    {
                        time = int.Parse(part[6].Trim()),
                        deviation = int.Parse(part[7].Trim())
                    },
                    off = new State_T
                    {
                        time = int.Parse(part[8].Trim()),
                        deviation = int.Parse(part[9].Trim())
                    },
                    repcnt = int.Parse(part[10].Trim())
                };

                dx_chgfreq(t.tid,
                           t.freq1.freq,
                           t.freq1.deviation,
                           t.freq2.freq,
                           t.freq2.deviation);

                dx_chgdur(t.tid,
                          t.on.time,
                          t.on.deviation,
                          t.off.time,
                          t.off.deviation);

                dx_chgrepcnt(t.tid,
                             t.repcnt);
            } // foreach

            // initialize
            var result = dx_initcallp(devh);
            if (result <= -1)
            {
                var err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
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

        internal static void DeleteTones(int devh)
        {
            if (dx_deltones(devh) == -1)
            {
                var err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
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
        internal static void Close(int devh)
        {
            var result = dx_close(devh, 0);
            if (result <= -1)
            {
                var err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
        }

        /// <summary>
        /// Returns every character including the terminator
        /// </summary>
        /// <param name="devh">The handle for the Dialogic line.</param>
        /// <returns>All the digits in the buffer including terminators</returns>
        internal static string FlushDigitBuffer(int devh)
        {
            var all = "";
            try
            {
                // add "T" so that I can get all the characters.
                all = GetDigits(devh, 99, "T", 100);
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
        /// <returns>Returns the digits pressed not including the terminator if there was one</returns>
        internal static string GetDigits(int devh, int numberOfDigits, string terminators)
        {
            var timeout = VoiceProperties.Current.DigitsTimeoutInMilli;
            return GetDigits(devh, numberOfDigits, terminators, timeout);
        }

        internal static string GetDigits(int devh, int numberOfDigits, string terminators, int timeout)
        {

            var tpt = GetTerminationConditions(numberOfDigits, terminators, timeout);

            DV_DIGIT digit;

            // Note: async does not work becaues digit is marshalled out immediately after dx_getdig is complete
            // not when event is found. Would have to use DV_DIGIT* and unsafe code. or another way?
            var result = dx_getdig(devh, ref tpt[0], out digit, EV_SYNC);
            if (result == -1)
            {
                var err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }

            var reason = ATDX_TERMMSK(devh);
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
            if ((reason & TM_BARGEIN) == TM_BARGEIN) Console.WriteLine("TM_BARGEIN");
            //if ((reason & TM_DIGIT) == TM_DIGIT) Console.WriteLine("TM_DIGIT");
            //if ((reason & TM_EOD) == TM_EOD) Console.WriteLine("TM_EOD");
            if ((reason & TM_MAXDATA) == TM_MAXDATA) Console.WriteLine("TM_MAXDATA");
            //if ((reason & TM_MAXDTMF) == TM_MAXDTMF) Console.WriteLine("TM_MAXDTMF");
            if ((reason & TM_MAXNOSIL) == TM_MAXNOSIL) Console.WriteLine("TM_MTAXNOSIL");
            if ((reason & TM_MAXSIL) == TM_MAXSIL) Console.WriteLine("TM_MAXSIL");
            //if ((reason & TM_NORMTERM) == TM_NORMTERM) Console.WriteLine("TM_NORMTERM");
            if ((reason & TM_PATTERN) == TM_PATTERN) Console.WriteLine("TM_PATTERN");
            if ((reason & TM_TONE) == TM_TONE) Console.WriteLine("TM_TONE");


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
            return answer;
        }

        private static void ClearDigits(int devh)
        {
            if (dx_clrdigbuf(devh) == -1)
            {
                var err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
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
        internal static void PlayFile(int devh, string filename, string terminators, DX_XPB xpb)
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


            var state = ATDX_STATE(devh);
            Logger.Debug("About to play: {0} state: {1}",filename,state);

            /* Now play the file */
            if (dx_playiottdata(devh, ref iott, ref tpt[0], ref xpb, EV_ASYNC) == -1)
            {
                Logger.Error("Tried to play: {0} state: {1}", filename, state);

                var err = ATDV_ERRMSGP(devh);
                dx_fileclose(iott.io_fhandle);
                throw new VoiceException(err);
            }

            var handler = 0;

            while (true)
            {
                if (sr_waitevtEx(ref devh, 1, -1, ref handler) == -1)
                {
                    var err = ATDV_ERRMSGP(devh);
                    dx_fileclose(iott.io_fhandle);
                    throw new VoiceException(err);
                }
                // make sure the file is closed
                if (dx_fileclose(iott.io_fhandle) == -1)
                {
                    var err = ATDV_ERRMSGP(devh);
                    throw new VoiceException(err);
                }
                var type = sr_getevttype((uint)handler);
                if (type == TDX_PLAY)
                {
                    var reason = ATDX_TERMMSK(devh);
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
                    if ((reason & TM_MAXTIME) == TM_MAXTIME) Logger.Debug("TM_MAXTIME");

                    if ((reason & TM_BARGEIN) == TM_BARGEIN) Logger.Debug("TM_BARGEIN");
                    //                    if ((reason & TM_DIGIT) == TM_DIGIT) Logger.Debug("TM_DIGIT");
                    //                    if ((reason & TM_EOD) == TM_EOD) Logger.Debug("TM_EOD"); // This is how I know they listend to full message
                    if ((reason & TM_IDDTIME) == TM_IDDTIME) Logger.Debug("TM_IDDTIME");
                    if ((reason & TM_MAXDATA) == TM_MAXDATA) Logger.Debug("TM_MAXDATA");
                    //                    if ((reason & TM_MAXDTMF) == TM_MAXDTMF) Logger.Debug("TM_MAXDTMF");
                    if ((reason & TM_MAXNOSIL) == TM_MAXNOSIL) Logger.Debug("TM_MTAXNOSIL");
                    if ((reason & TM_MAXSIL) == TM_MAXSIL) Logger.Debug("TM_MAXSIL");
                    //                    if ((reason & TM_NORMTERM) == TM_NORMTERM) Logger.Debug("TM_NORMTERM");
                    if ((reason & TM_PATTERN) == TM_PATTERN) Logger.Debug("TM_PATTERN");
                    if ((reason & TM_TONE) == TM_TONE) Logger.Debug("TM_TONE");
                }
                else
                {
                    Logger.Error("got here: {0}",type);
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
        internal static void RecordToFile(int devh, string filename, string terminators, DX_XPB xpb)
        {

            FlushDigitBuffer(devh);

            /* set up DV_TPT */
            var tpt = GetTerminationConditions(1, terminators,0);

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
                if (sr_waitevtEx(ref devh, 1, -1, ref handler) == -1)
                {
                    var err = ATDV_ERRMSGP(devh);
                    dx_fileclose(iott.io_fhandle);
                    throw new VoiceException(err);
                }
                if (dx_fileclose(iott.io_fhandle) == -1)
                {
                    var err = ATDV_ERRMSGP(devh);
                    throw new VoiceException(err);
                }

                var type = sr_getevttype((uint)handler);
                if (type == TDX_RECORD)
                {
                    var reason = ATDX_TERMMSK(devh);
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
                    if ((reason & TM_MAXTIME) == TM_MAXTIME) Console.WriteLine("TM_MAXTIME");

                    if ((reason & TM_BARGEIN) == TM_BARGEIN) Console.WriteLine("TM_BARGEIN");
                    if ((reason & TM_DIGIT) == TM_DIGIT) Console.WriteLine("TM_DIGIT");
                    if ((reason & TM_EOD) == TM_EOD) Console.WriteLine("TM_EOD");
                    if ((reason & TM_IDDTIME) == TM_IDDTIME) Console.WriteLine("TM_IDDTIME");
                    if ((reason & TM_MAXDATA) == TM_MAXDATA) Console.WriteLine("TM_MAXDATA");
                    if ((reason & TM_MAXDTMF) == TM_MAXDTMF) Console.WriteLine("TM_MAXDTMF");
                    if ((reason & TM_MAXNOSIL) == TM_MAXNOSIL) Console.WriteLine("TM_MTAXNOSIL");
                    if ((reason & TM_MAXSIL) == TM_MAXSIL) Console.WriteLine("TM_MAXSIL");
                    if ((reason & TM_NORMTERM) == TM_NORMTERM) Console.WriteLine("TM_NORMTERM");
                    if ((reason & TM_PATTERN) == TM_PATTERN) Console.WriteLine("TM_PATTERN");
                    if ((reason & TM_TONE) == TM_TONE) Console.WriteLine("TM_TONE");
                }
                else
                {
                    Console.WriteLine("got here: " + type);
                }
                FlushDigitBuffer(devh);
                return;
            }

        }
    } // class
} // namespace
