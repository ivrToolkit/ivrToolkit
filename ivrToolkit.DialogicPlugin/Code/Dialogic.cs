/*
 * Copyright 2013 Troy Makaro
 *
 * This file is part of ivrToolkit, distributed under the GNU GPL. For full terms see the included COPYING file.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using ivrToolkit.Core;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Util;
using System.Reflection;

namespace ivrToolkit.DialogicPlugin
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Dialogic : IVoice
    {
        /// <summary>
        /// Defines the api required to access Dialogic Springware boards
        /// </summary>
        public Dialogic()
        {
        }

        /// <summary>
        /// Opens the board line.
        /// </summary>
        /// <param name="devname">Name of the board line. For example: dxxxB1C1</param>
        /// <returns>The device handle</returns>
        public static int openDevice(string devname)
        {
            int devh = -1;

            devh = dx_open(devname, 0);
            if (devh <= -1)
            {
                string err = string.Format("Could not get device handle for device {0}", devname);
                throw new VoiceException(err);
            }
            return devh;
        }

        public static void waitRings(int devh, int rings)
        {
            if (dx_wtring(devh, rings, (int)HookState.OFF_HOOK, -1) == -1)
            {
                string err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
        }

        public static void stop(int devh)
        {
            if (dx_stopch(devh, EV_SYNC) == -1)
            {
                string err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
        }

        /// <summary>
        /// Puts the line on hook.
        /// </summary>
        /// <param name="devh">The handle for the Dialogic line.</param>
        public static void hangup(int devh)
        {
            dx_stopch(devh, EV_SYNC);

            int result = dx_sethook(devh, (int)HookState.ON_HOOK, EV_SYNC);
            if (result <= -1)
            {
                string err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
        }

        /// <summary>
        /// Takes the line off hook.
        /// </summary>
        /// <param name="devh">The handle for the Dialogic line.</param>
        public static void takeOffHook(int devh)
        {
            int result = dx_sethook(devh, (int)HookState.OFF_HOOK, EV_SYNC);
            if (result <= -1)
            {
                string err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
        }

        /// <summary>
        /// Dials a phone number using call progress analysis.
        /// </summary>
        /// <param name="devh">The handle for the Dialogic line.</param>
        /// <param name="number">The phone number to dial.</param>
        /// <returns>CallAnalysis Enum</returns>
        public static ivrToolkit.Core.CallAnalysis dialWithCPA(int devh, string number, int answeringMachineLengthInMilliseconds)
        {

            DX_CAP cap = Dialogic.getCap(devh);

            String fullNumber = VoiceProperties.current.dialToneType + number;
            int result = dx_dial(devh, fullNumber, ref cap, DX_CALLP | EV_SYNC);
            if (result <= -1)
            {
                string error = ATDV_ERRMSGP(devh);
                throw new VoiceException(error);
            }
            CallAnalysis c = (CallAnalysis)result;
            switch (c)
            {
                case CallAnalysis.CR_BUSY:
                    return ivrToolkit.Core.CallAnalysis.busy;
                case CallAnalysis.CR_CEPT:
                    return ivrToolkit.Core.CallAnalysis.operatorIntercept;
                case CallAnalysis.CR_CNCT:
                    int connType = ATDX_CONNTYPE(devh);
                    switch (connType)
                    {
                        case CON_CAD:
                            Console.WriteLine("Connection due to cadence break ");
                            break;
                        case CON_DIGITAL:
                            Console.WriteLine("con_digital");
                            break;
                        case CON_LPC:
                            Console.WriteLine("Connection due to loop current");
                            break;
                        case CON_PAMD:
                            Console.WriteLine("Connection due to Positive Answering Machine Detection");
                            break;
                        case CON_PVD:
                            Console.WriteLine("Connection due to Positive Voice Detection");
                            break;
                    }
                    int len = getSalutationLength(devh);
                    if (len > answeringMachineLengthInMilliseconds)
                    {
                        return ivrToolkit.Core.CallAnalysis.answeringMachine;
                    }
                    else
                    {
                        return ivrToolkit.Core.CallAnalysis.connected;
                    }
                case CallAnalysis.CR_ERROR:
                    return ivrToolkit.Core.CallAnalysis.error;
                case CallAnalysis.CR_FAXTONE:
                    return ivrToolkit.Core.CallAnalysis.faxTone;
                case CallAnalysis.CR_NOANS:
                    return ivrToolkit.Core.CallAnalysis.noAnswer;
                case CallAnalysis.CR_NODIALTONE:
                    return ivrToolkit.Core.CallAnalysis.noDialTone;
                case CallAnalysis.CR_NORB:
                    return ivrToolkit.Core.CallAnalysis.noRingback;
                case CallAnalysis.CR_STOPD:
                    // calling method will check and throw the stopException
                    return ivrToolkit.Core.CallAnalysis.stopped;
            }
            throw new VoiceException("Unknown dail response: "+result);
        }

        private static int getTid(string tidName)
        {
            int value = 0;
            if (int.TryParse(tidName, out value))
            {
                return value;
            }
            else
            {
                FieldInfo fi =
                typeof(Dialogic).GetField(tidName, BindingFlags.Static | BindingFlags.NonPublic);
                if (fi != null)
                {
                    object obj = fi.GetValue(null);
                    if (obj is Int32)
                    {
                        return (int)obj;
                    }
                }
                throw new Exception("tid name is not found: "+tidName);
            }
        }

        public static void initCallProgress(int devh)
        {
            string[] toneParams = VoiceProperties.current.getPrefixMatch("cpa.tone.");

            foreach (String tone in toneParams)
            {
                string[] part = tone.Split(',');
                Tone_T t = new Tone_T()
                {
                    str = part[0].Trim(),
                    tid = getTid(part[1].Trim()),
                    freq1 = new Freq_T()
                    {
                        freq = int.Parse(part[2].Trim()),
                        deviation = int.Parse(part[3].Trim())
                    },
                    freq2 = new Freq_T()
                    {
                        freq = int.Parse(part[4].Trim()),
                        deviation = int.Parse(part[5].Trim())
                    },
                    on = new State_T()
                    {
                        time = int.Parse(part[6].Trim()),
                        deviation = int.Parse(part[7].Trim())
                    },
                    off = new State_T()
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
            int result = dx_initcallp(devh);
            if (result <= -1)
            {
                string err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
        }

        private static DX_CAP getCap(int devh)
        {
            string err = null;
            DX_CAP cap = new DX_CAP();

            int result = dx_clrcap(ref cap);
            if (result <= -1)
            {
                err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }

            Type capType = typeof(DX_CAP);

            object boxed = cap;

            string[] caps = VoiceProperties.current.getKeyPrefixMatch("cap.");
            foreach (string capName in caps)
            {
                FieldInfo info = capType.GetField(capName);
                if (info == null)
                {
                    throw new Exception("Could not find dx_cap."+capName);
                }
                else
                {
                    object obj = info.GetValue(cap);
                    if (obj is ushort)
                    {
                        ushort value = ushort.Parse(VoiceProperties.current.getProperty("cap."+capName));
                        info.SetValue(boxed, value);
                    }
                    else if (obj is byte)
                    {
                        byte value = byte.Parse(VoiceProperties.current.getProperty("cap."+capName));
                        info.SetValue(boxed, value);
                    }
                }
            }

            return (DX_CAP)boxed;
        }

        public static void deleteTones(int devh)
        {
            if (dx_deltones(devh) == -1)
            {
                string err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
        }

        /// <summary>
        /// Gets the greeting time in milliseconds.
        /// </summary>
        /// <param name="devh">The handle for the Dialogic line.</param>
        /// <returns>The greeting time in milliseconds.</returns>
        private static int getSalutationLength(int devh) {
            int result = ATDX_ANSRSIZ(devh);
            if (result <= -1)
            {
                string err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
            return result * 10;
        }

        /// <summary>
        /// Closes the board line.
        /// </summary>
        /// <param name="devh">The handle for the Dialogic line.</param>
        public static void close(int devh)
        {
            int result = dx_close(devh, 0);
            if (result <= -1)
            {
                string err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
        }

        /// <summary>
        /// Returns every character including the terminator
        /// </summary>
        /// <param name="devh">The handle for the Dialogic line.</param>
        /// <returns>All the digits in the buffer including terminators</returns>
        public static string flushDigitBuffer(int devh)
        {
            string all = "";
            try
            {
                // add "T" so that I can get all the characters. There must be a better way.
                all = getDigits(devh, 99, "T", 100);
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
        /// <returns>Returns the digits pressed not including the terminator if there was one</returns>
        public static string getDigits(int devh, int numberOfDigits, string terminators)
        {
            int timeout = VoiceProperties.current.digitsTimeoutInMilli;
            return getDigits(devh, numberOfDigits, terminators, timeout);
        }

        public static string getDigits(int devh, int numberOfDigits, string terminators, int timeout)
        {

            DV_TPT[] tpt = getTerminationConditions(numberOfDigits, terminators, timeout);

            DV_DIGIT digit = new DV_DIGIT();

            // Note: async does not work becaues digit is marshalled out immediately after dx_getdig is complete
            // not when event is found. Would have to use DV_DIGIT* and unsafe code. or another way?
            int result = dx_getdig(devh, ref tpt[0], out digit, EV_SYNC);
            if (result == -1)
            {
                string err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }

            int reason = ATDX_TERMMSK(devh);
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


            string answer = digit.dg_value;
            clearDigits(devh); // not sure if this is necessary and perhaps only needed for getDigitsTimeoutException?
            if ((reason & TM_IDDTIME) == TM_IDDTIME)
            {
                if (terminators.IndexOf("T") != -1 && answer.Length != 0)
                {
                    // terminator is allowed as long as there is at least one key pressed
                    answer += 'T';
                }
                else
                {
                    if (terminators.IndexOf("t") != -1)
                    {
                        answer += 't';
                    }
                    else
                    {
                        throw new GetDigitsTimeoutException();
                    }
                }
            }
            return answer;
        }

        private static void clearDigits(int devh)
        {
            if (dx_clrdigbuf(devh) == -1)
            {
                string err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
        }

        private static DV_TPT[] getTerminationConditions(int numberOfDigits, string terminators, int timeoutInMilliseconds)
        {
            List<DV_TPT> tpts = new List<DV_TPT>();

            DV_TPT tpt = new DV_TPT();
            tpt.tp_type = IO_CONT;
            tpt.tp_termno = DX_MAXDTMF; // Maximum digits
            tpt.tp_length = (ushort)numberOfDigits; // terminate on max digit
            tpt.tp_flags = TF_MAXDTMF;
            tpt.tp_nextp = IntPtr.Zero;
            tpts.Add(tpt);

            int bitMask = defineDigits(terminators);
            if (bitMask != 0)
            {
                tpt = new DV_TPT();
                tpt.tp_type = IO_CONT;
                tpt.tp_termno = DX_DIGMASK; // digit mask termination
                tpt.tp_length = (ushort)bitMask;
                tpt.tp_flags = TF_DIGMASK;
                tpt.tp_nextp = IntPtr.Zero;
                tpts.Add(tpt);
            }
            if (timeoutInMilliseconds != 0)
            {
                tpt = new DV_TPT();
                tpt.tp_type = IO_CONT;
                tpt.tp_termno = DX_IDDTIME; // Function out
                tpt.tp_length = (ushort)(timeoutInMilliseconds/100) ; // x millseconds (100 ms resolution * timer)
                tpt.tp_flags = TF_IDDTIME; // edge triggered
                tpt.tp_nextp = IntPtr.Zero;
                tpts.Add(tpt);
            }

            tpt = new DV_TPT();
            tpt.tp_type = IO_EOT;
            tpt.tp_termno = DX_LCOFF; // Loop current off
            tpt.tp_length = 3; // Use 30 ms (10 ms resolution * timer)
            tpt.tp_flags = TF_LCOFF | TF_10MS;
            tpt.tp_nextp = IntPtr.Zero;
            tpts.Add(tpt);

            return tpts.ToArray();
        }

        private static int defineDigits(string digits)
        {
            int result = 0;

            if (digits == null) digits = "";

            string all = digits.Trim().ToLower();
            char[] chars = all.ToCharArray();
            for (int index = 0; index < chars.Length; index++)
            {
                char c = chars[index];
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
        /// <param name="xpb">The format of the vox or wav file.</param>
        public static void playFile(int devh, string filename, string terminators, DX_XPB xpb)
        {

            /* set up DV_TPT */
            DV_TPT[] tpt = getTerminationConditions(10, terminators,0);

            DX_IOTT iott = new DX_IOTT();
            /* set up DX_IOTT */
            iott.io_type = IO_DEV | IO_EOT;
            iott.io_bufp = null;
            iott.io_offset = 0;
            iott.io_length = -1; /* play till end of file */
            if ((iott.io_fhandle = dx_fileopen(filename, _O_RDONLY | _O_BINARY)) == -1)
            {
                int fileErr = dx_fileerrno();

                string err = "";

                switch (fileErr)
                {
                    case Dialogic.EACCES:
                        err = "Tried to open read-only file for writing, file's sharing mode does not allow specified operations, or given path is directory.";
                        break;
                    case Dialogic.EEXIST:
                        err = "_O_CREAT and _O_EXCL flags specified, but filename already exists.";
                        break;
                    case Dialogic.EINVAL:
                        err = "Invalid oflag or pmode argument.";
                        break;
                    case Dialogic.EMFILE:
                        err = "No more file descriptors available (too many open files).";
                        break;
                    case Dialogic.ENOENT:
                        err = "File or path not found.";
                        break;
                }

                dx_fileclose(iott.io_fhandle);

                throw new VoiceException(err);
            }

            /* Now play the file */
            if (dx_playiottdata(devh, ref iott, ref tpt[0], ref xpb, EV_ASYNC) == -1)
            {
                string err = ATDV_ERRMSGP(devh);
                dx_fileclose(iott.io_fhandle);
                throw new VoiceException(err);
            }

            int handler = 0;

            while (true)
            {
                if (sr_waitevtEx(ref devh, 1, -1, ref handler) == -1)
                {
                    string err = ATDV_ERRMSGP(devh);
                    dx_fileclose(iott.io_fhandle);
                    throw new VoiceException(err);
                }
                // make sure the file is closed
                if (dx_fileclose(iott.io_fhandle) == -1)
                {
                    string err = ATDV_ERRMSGP(devh);
                    throw new VoiceException(err);
                }
                int type = sr_getevttype((uint)handler);
                if (type == TDX_PLAY)
                {
                    int reason = ATDX_TERMMSK(devh);
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
//                    if ((reason & TM_DIGIT) == TM_DIGIT) Console.WriteLine("TM_DIGIT");
//                    if ((reason & TM_EOD) == TM_EOD) Console.WriteLine("TM_EOD"); // This is how I know they listend to full message
                    if ((reason & TM_IDDTIME) == TM_IDDTIME) Console.WriteLine("TM_IDDTIME");
                    if ((reason & TM_MAXDATA) == TM_MAXDATA) Console.WriteLine("TM_MAXDATA");
//                    if ((reason & TM_MAXDTMF) == TM_MAXDTMF) Console.WriteLine("TM_MAXDTMF");
                    if ((reason & TM_MAXNOSIL) == TM_MAXNOSIL) Console.WriteLine("TM_MTAXNOSIL");
                    if ((reason & TM_MAXSIL) == TM_MAXSIL) Console.WriteLine("TM_MAXSIL");
//                    if ((reason & TM_NORMTERM) == TM_NORMTERM) Console.WriteLine("TM_NORMTERM");
                    if ((reason & TM_PATTERN) == TM_PATTERN) Console.WriteLine("TM_PATTERN");
                    if ((reason & TM_TONE) == TM_TONE) Console.WriteLine("TM_TONE");
                }
                else
                {
                    Console.WriteLine("got here: " + type);
                }
                return;
            }

        }



        public static void addDualTone(int devh, int tid, int freq1, int fq1dev, int freq2, int fq2dev,
            ToneDetection mode)
        {
            uint dialogicMode;
            if (mode == ToneDetection.leading)
            {
                dialogicMode = TN_LEADING;
            }
            else
            {
                dialogicMode = TN_TRAILING;
            }

            if (dx_blddt((uint)tid, (uint)freq1, (uint)fq1dev, (uint)freq2, (uint)fq2dev, dialogicMode) == -1)
            {
                throw new VoiceException("unable to build dual tone");
            }
            if (dx_addtone(devh, 0, 0) == -1)
            {
                string err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
        }
        //T5=480,30,620,40,25,5,25,5,2 fast busy
        //T6=350,20,440,20,L dial tone

        public static void addDualToneWithCadence(int devh, int tid, int freq1, int fq1dev, int freq2, int fq2dev,
            int ontime, int ontdev, int offtime, int offtdev, int repcnt)
        {
            if (dx_blddtcad((uint)tid, (uint)freq1, (uint)fq1dev, (uint)freq2, (uint)fq2dev, (uint)ontime, (uint)ontdev, (uint)offtime, (uint)offtdev, (uint)repcnt) == -1)
            {
                throw new VoiceException("unable to build dual tone cadence");
            }
            if (dx_addtone(devh, 0, 0) == -1)
            {
                string err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
        }

        public static void disableTone(int devh, int tid)
        {
            if (dx_distone(devh, tid, DM_TONEON | DM_TONEOFF) == -1)
            {
                string err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
        }

        public static void enableTone(int devh, int tid)
        {
            if (dx_enbtone(devh, tid, DM_TONEON | DM_TONEOFF) == -1)
            {
                string err = ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
        }

        public static int listenForCustomTones(int devh, int timeoutSeconds)
        {
            DX_EBLK eblk = new DX_EBLK();
            if (dx_getevt(devh, ref eblk, timeoutSeconds) == -1)
            {
                if (ATDV_LASTERR(devh) == EDX_TIMEOUT)
                {
                    return 0;
                }
                string err = ATDV_ERRMSGP(devh);
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
        /// <param name="xpb">The format of the vox or wav file.</param>
        public static void recordToFile(int devh, string filename, string terminators, DX_XPB xpb)
        {

            flushDigitBuffer(devh);

            /* set up DV_TPT */
            DV_TPT[] tpt = getTerminationConditions(1, terminators,0);

            DX_IOTT iott = new DX_IOTT();
            /* set up DX_IOTT */
            iott.io_type = IO_DEV | IO_EOT;
            iott.io_bufp = null;
            iott.io_offset = 0;
            iott.io_length = -1;
            if ((iott.io_fhandle = dx_fileopen(filename, _O_CREAT | _O_BINARY | _O_RDWR, _S_IWRITE)) == -1)
            {
                int fileErr = dx_fileerrno();

                string err = "";

                switch (fileErr)
                {
                    case Dialogic.EACCES:
                        err = "Tried to open read-only file for writing, file's sharing mode does not allow specified operations, or given path is directory.";
                        break;
                    case Dialogic.EEXIST:
                        err = "_O_CREAT and _O_EXCL flags specified, but filename already exists.";
                        break;
                    case Dialogic.EINVAL:
                        err = "Invalid oflag or pmode argument.";
                        break;
                    case Dialogic.EMFILE:
                        err = "No more file descriptors available (too many open files).";
                        break;
                    case Dialogic.ENOENT:
                        err = "File or path not found.";
                        break;
                }

                dx_fileclose(iott.io_fhandle);

                throw new VoiceException(err);
            }

            /* Now record the file */
            if (dx_reciottdata(devh, ref iott, ref tpt[0], ref xpb, RM_TONE | EV_ASYNC) == -1)
            {
                string err = ATDV_ERRMSGP(devh);
                dx_fileclose(iott.io_fhandle);
                throw new VoiceException(err);
            }

            int handler = 0;

            while (true)
            {
                if (sr_waitevtEx(ref devh, 1, -1, ref handler) == -1)
                {
                    string err = ATDV_ERRMSGP(devh);
                    dx_fileclose(iott.io_fhandle);
                    throw new VoiceException(err);
                }
                if (dx_fileclose(iott.io_fhandle) == -1)
                {
                    string err = ATDV_ERRMSGP(devh);
                    throw new VoiceException(err);
                }

                int type = sr_getevttype((uint)handler);
                if (type == TDX_RECORD)
                {
                    int reason = ATDX_TERMMSK(devh);
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
                flushDigitBuffer(devh);
                return;
            }

        }



        private static void display(string name, object obj) {
            byte[] bytes = StructureToByteArray(obj);
            System.Diagnostics.Debug.Write(name+" length = "+ bytes.Length + ", bytes = ");
            for (int index = 0; index < bytes.Length; index++)
            {
                System.Diagnostics.Debug.Write(bytes[index] + "|");
            }
            System.Diagnostics.Debug.WriteLine("");
        }

        private static byte[] StructureToByteArray(object obj)
        {

            int len = Marshal.SizeOf(obj);

            byte[] arr = new byte[len];

            IntPtr ptr = Marshal.AllocHGlobal(len);

            Marshal.StructureToPtr(obj, ptr, true);

            Marshal.Copy(ptr, arr, 0, len);

            Marshal.FreeHGlobal(ptr);

            return arr;

        }

        public ILine getLine(int lineNumber)
        {
            // TODO fix this
            int devh = Dialogic.openDevice("dxxxB1C" + lineNumber.ToString());
            return new DialogicLine(devh,lineNumber);
        }
    } // class
} // namespace
