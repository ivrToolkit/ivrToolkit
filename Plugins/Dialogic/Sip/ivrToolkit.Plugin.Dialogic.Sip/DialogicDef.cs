// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 

using System;
using System.Runtime.InteropServices;

namespace ivrToolkit.Plugin.Dialogic.Sip
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Local
    // ReSharper disable NotAccessedField.Local
    // ReSharper disable FieldCanBeMadeReadOnly.Local
    // ReSharper disable MemberCanBePrivate.Local
    #pragma warning disable 169
    #pragma warning disable 649

    public partial class Dialogic
    {
        public const int CON_CAD = 1;
        public const int CON_LPC = 2;
        public const int CON_PVD = 3;
        public const int CON_PAMD = 4;
        public const int CON_DIGITAL = 5;

/* Error Codes */

        public const int EPERM = 1;
        public const int ENOENT = 2;
        public const int ESRCH = 3;
        public const int EINTR = 4;
        public const int EIO = 5;
        public const int ENXIO = 6;
        public const int E2BIG = 7;
        public const int ENOEXEC = 8;
        public const int EBADF = 9;
        public const int ECHILD = 10;
        public const int EAGAIN = 11;
        public const int ENOMEM = 12;
        public const int EACCES = 13;
        public const int EFAULT = 14;
        public const int EBUSY = 16;
        public const int EEXIST = 17;
        public const int EXDEV = 18;
        public const int ENODEV = 19;
        public const int ENOTDIR = 20;
        public const int EISDIR = 21;
        public const int ENFILE = 23;
        public const int EMFILE = 24;
        public const int ENOTTY = 25;
        public const int EFBIG = 27;
        public const int ENOSPC = 28;
        public const int ESPIPE = 29;
        public const int EROFS = 30;
        public const int EMLINK = 31;
        public const int EPIPE = 32;
        public const int EDOM = 33;
        public const int EDEADLK = 36;
        public const int ENAMETOOLONG = 38;
        public const int ENOLCK = 39;
        public const int ENOSYS = 40;
        public const int ENOTEMPTY = 41;

        public const int EINVAL = 22;
        public const int ERANGE = 34;
        public const int EILSEQ = 42;
        public const int STRUNCATE = 80;

        public const int _S_IFMT = 0xF000;          /* file type mask */
        public const int _S_IFDIR = 0x4000;          /* directory */
        public const int _S_IFCHR = 0x2000;          /* character special */
        public const int _S_IFIFO = 0x1000;          /* pipe */
        public const int _S_IFREG = 0x8000;          /* regular */
        public const int _S_IREAD = 0x0100;          /* read permission, owner */
        public const int _S_IWRITE = 0x0080;          /* write permission, owner */
        public const int _S_IEXEC = 0x0040;          /* execute/search permission, owner */

        public const int _O_RDONLY = 0; // 0x0000
        public const int _O_WRONLY = 1; // 0x0001
        public const int _O_RDWR = 2; // 0x0002
        public const int _O_APPEND = 8; // 0x0008
        public const int _O_CREAT = 256; // 0x0100
        public const int _O_TRUNC = 512; // 0x0200
        public const int _O_EXCL = 1024; // 0x0400
        public const int _O_TEXT = 16384; // 0x4000
        public const int _O_BINARY = 32768; // 0x8000

        public const int FILE_FORMAT_VOX = 1;
        public const int FILE_FORMAT_WAVE = 2;
        public const int FILE_FORMAT_NONE = 3;

        public const int DRT_6KHZ = 48; // 0x30
        public const int DRT_8KHZ = 64; // 0x40
        public const int DRT_11KHZ = 88; // 0x58

        public const int DATA_FORMAT_DIALOGIC_ADPCM = 1; // 0x1
        public const int DATA_FORMAT_ALAW = 3; // 0x3
        public const int DATA_FORMAT_G726 = 4; // 0x4
        public const int DATA_FORMAT_MULAW = 7; // 0x7
        public const int DATA_FORMAT_PCM = 8; // 0x8
        public const int DATA_FORMAT_G729A = 12; // 0x0C
        public const int DATA_FORMAT_GSM610 = 13; // 0x0D
        public const int DATA_FORMAT_FFT = 255; // 0xFF


/*
 * Error codes returned by ATDV_LASTERR()
 */
        public const int EDX_NOERROR = 0;     /* No Errors */
        public const int EDX_SYSTEM   =   1;     /* System Error */
        public const int EDX_FWERROR  =   2;     /* Firmware Error */
        public const int EDX_TIMEOUT  =   3;     /* Function Timed Out */
        public const int EDX_BADIOTT  =   4;     /* Invalid Entry in the DX_IOTT */
        public const int EDX_BADTPT   =   5;     /* Invalid Entry in the DX_TPT */
        public const int EDX_BADPARM  =   6;     /* Invalid Parameter in Function Call */
        public const int EDX_BADDEV   =   7;     /* Invalid Device Descriptor */
        public const int EDX_BADPROD  =   8;     /* Func. Not Supported on this Board */
        public const int EDX_BUSY     =   9;    /* Device is Already Busy */
        public const int EDX_IDLE     =   10;    /* Device is Idle */
        public const int EDX_STOPRINGS =  11;    /* Stop waitrings (MT only) */
        public const int EDX_WTRINGSTOP = 11;    /* Wait for Rings stopped by user */
        public const int EDX_BADWAVEFILE =12;    /* Bad/Unsupported WAV file */
        public const int EDX_XPBPARM     =13;    /* Bad XPB structure */
        public const int EDX_NOSUPPORT   =14;    /* Data format not supported */
        public const int EDX_NOTIMP		=15;    /* Function not implemented */
        public const int EDX_BADSUBCOMMAND		=		16;
        public const int EDX_BADCHANNELNUMBER	=		17;
        public const int EDX_BADRESOURCEID		=		18;
        public const int EDX_NORESOURCE			=		19;    /* No Resources */
        public const int EDX_DSPERROR			=		20;    /* Resource DSP error */
        public const int EDX_INUSE				=		21;
        public const int EDX_HOOKSTATETRANSITIONERROR	=25;    /* dx_sethook() unable to transition hookstate */



        [StructLayout(LayoutKind.Sequential)]
        public struct DX_XPB
        {
            public ushort wFileFormat;
            public ushort wDataFormat;
            public uint nSamplesPerSec;
            public ushort wBitsPerSample;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DV_DIGIT
        {

            /// char[]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=100)]
            public string dg_value;

            /// char[]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=100)]
            public string dg_type;
        }

        private enum HookState
        {
            ON_HOOK = 0,
            OFF_HOOK = 1
        }

        private const int DM_TONEON = 0x01;  /* Tone ON Mask */
        private const int DM_TONEOFF = 0x02;  /* Tone OFF Mask */

        private const int EV_ASYNC = 0x8000;
        private const int EV_SYNC = 0x0000;

        private struct Freq_T
        {
            public int freq;          // frequency Hz
            public int deviation;     // deviation in Hz
        }

        private struct State_T
        {
            public int time;          // time in 10ms
            public int deviation;     // deviation in ms
        }

        private struct Tone_T
        {
            public string str;        // informational string

            public int tid;        // tone id
            public Freq_T freq1;      // frequency 1
            public Freq_T freq2;      // frequency 2
            public State_T on;         // on time
            public State_T off;        // off time
            public int repcnt;     // repitition count
        }
        // call analysis 
        private enum CallAnalysis
        {
            CR_BUSY = 7, /* Line busy */
            CR_NOANS = 8, /* No answer */
            CR_NORB = 9, /* No ringback */
            CR_CNCT = 10, /* Call connected */
            CR_CEPT = 11, /* Operator intercept */
            CR_STOPD = 12, /* Call analysis stopped */
            CR_NODIALTONE = 17, /* No dialtone detected */
            CR_FAXTONE = 18, /* Fax tone detected */
            CR_ERROR = 0x100 /* Call analysis error */
        }
        private enum DeviceType
        {
            SC_VOX = 0x01,
            SC_LSI = 0x02
        }

        private enum DuplexMode
        {
            SC_FULLDUP = 0x00,
            SC_HALFDUP = 0x01
        }

        private const ushort SV_SPEEDTBL = 0x01;    /* Modify Speed */
        private const ushort SV_VOLUMETBL = 0x02;   /* Modify Volume */

        private const ushort SV_ABSPOS = 0x00;      /* Absolute Position */
        private const ushort SV_RELCURPOS = 0x10;   /* Relative to Current Position */
        private const ushort SV_TOGGLE = 0x20;      /* Toggle */

        private const ushort SV_WRAPMOD = 0x0010;
        private const ushort SV_SETDEFAULT = 0x0020;
        private const ushort SV_LEVEL = 0x0100;
        private const ushort SV_BEGINPLAY = 0x0200;

        private const ushort SV_TOGORIGIN = 0x00;    /* Toggle Between Origin and Last Modified Position */
        private const ushort SV_CURORIGIN = 0x01;    /* Reset Current Position to Origin */
        private const ushort SV_CURLASTMOD = 0x02;   /* Reset Current Position to Last Modified Position */
        private const ushort SV_RESETORIG = 0x03;    /* Reset Current Position and Last Modified State to Origin */

        [StructLayout(LayoutKind.Sequential)]
        public struct DX_CST
        {
            public ushort cst_event;
            public ushort cst_data;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DV_TPT
        {
            public ushort tp_type; /* Flags Describing this Entry */
            public ushort tp_termno; /* Termination Parameter Number */
            public ushort tp_length; /* Length of Terminator */
            public ushort tp_flags; /* Termination Parameter Attributes Flag */
            public ushort tp_data; /* Optional Additional Data */
            public ushort rfu; /* Reserved */


            /// DV_TPT*
            public IntPtr tp_nextp; /* Ptr to next DV_TPT if IO_LINK set */
            //public DV_TPT* tp_nextp;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DX_IOTT
        {
            public ushort io_type; /* Transfer type */
            public ushort rfu; /* reserved */
            public int io_fhandle; /* File descriptor */

            /// char*
            [MarshalAs(UnmanagedType.LPStr)]
            public string io_bufp; /* Pointer to base memory */

            public uint io_offset; /* File/Buffer offset */
            public int io_length; /* Length of data */

            /// DX_IOTT*
            public IntPtr io_nextp; /* Pointer to next DX_IOTT if IO_LINK */

            /// DX_IOTT*
            public IntPtr io_prevp; /* (optional) Pointer to previous DX_IOTT */
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct DX_CAP
        {
            public ushort ca_nbrdna; /* # of rings before no answer. */
            public ushort ca_stdely; /* Delay after dialing before analysis. */
            public ushort ca_cnosig; /* Duration of no signal time out delay. */
            public ushort ca_lcdly; /* Delay after dial before lc drop connect */
            public ushort ca_lcdly1; /* Delay after lc drop con. Before msg. */
            public ushort ca_hedge; /* Edge of answer to send connect message. */
            public ushort ca_cnosil; /* Initial continuous noise timeout delay. */
            public ushort ca_lo1tola; /* % acceptable pos. dev of short low sig. */
            public ushort ca_lo1tolb; /* % acceptable neg. dev of short low sig. */
            public ushort ca_lo2tola; /* % acceptable pos. dev of long low sig. */
            public ushort ca_lo2tolb; /* % acceptable neg. dev of long low sig. */
            public ushort ca_hi1tola; /* % acceptable pos. dev of high signal. */
            public ushort ca_hi1tolb; /* % acceptable neg. dev of high signal. */
            public ushort ca_lo1bmax; /* Maximum interval for shrt low for busy. */
            public ushort ca_lo2bmax; /* Maximum interval for long low for busy. */
            public ushort ca_hi1bmax; /* Maximum interval for 1st high for busy */
            public ushort ca_nsbusy; /* Num. of highs after nbrdna busy check. */
            public ushort ca_logltch; /* Silence deglitch duration. */
            public ushort ca_higltch; /* Non-silence deglitch duration. */
            public ushort ca_lo1rmax; /* Max. short low dur. of double ring. */
            public ushort ca_lo2rmin; /* Min. long low dur. of double ring. */
            public ushort ca_intflg; /* Operator intercept mode. */
            public ushort ca_intfltr; /* Minimum signal to qualify freq. detect. */
            public ushort rfu1; /* reserved for future use */
            public ushort rfu2; /* reserved for future use */
            public ushort rfu3; /* reserved for future use */
            public ushort rfu4; /* reserved for future use */
            public ushort ca_hisiz; /* Used to determine which lowmax to use. */
            public ushort ca_alowmax; /* Max. low before con. if high >hisize. */
            public ushort ca_blowmax; /* Max. low before con. if high */
            public ushort ca_nbrbeg; /* Number of rings before analysis begins. */
            public ushort ca_hi1ceil; /* Maximum 2nd high dur. for a retrain. */
            public ushort ca_lo1ceil; /* Maximum 1st low dur. for a retrain. */
            public ushort ca_lowerfrq; /* Lower allowable frequency in hz. */
            public ushort ca_upperfrq; /* Upper allowable frequency in hz. */
            public ushort ca_timefrq; /* Total duration of good signal required. */
            public ushort ca_rejctfrq; /* Allowable % of bad signal. */
            public ushort ca_maxansr; /* Maximum duration of answer. */
            public ushort ca_ansrdgl; /* Silence deglitching value for answer. */
            public ushort ca_mxtimefrq; /* max time for 1st freq to remain in bounds */
            public ushort ca_lower2frq; /* lower bound for second frequency */
            public ushort ca_upper2frq; /* upper bound for second frequency */
            public ushort ca_time2frq; /* min time for 2nd freq to remains in bounds */
            public ushort ca_mxtime2frq; /* max time for 2nd freq to remain in bounds */
            public ushort ca_lower3frq; /* lower bound for third frequency */
            public ushort ca_upper3frq; /* upper bound for third frequency */
            public ushort ca_time3frq; /* min time for 3rd freq to remains in bounds */
            public ushort ca_mxtime3frq; /* max time for 3rd freq to remain in bounds */
            public ushort ca_dtn_pres; /* Length of a valid dial tone (def=1sec) */
            public ushort ca_dtn_npres; /* Max time to wait for dial tone (def=3sec)*/
            public ushort ca_dtn_deboff; /* The dialtone off debouncer (def=100ms) */
            public ushort ca_pamd_failtime; /* Wait for AMD/PVD after cadence break(dfault=4s)*/
            public ushort ca_pamd_minring; /* min allowable ring duration (def=1.9sec)*/
            public byte ca_pamd_spdval; /* Set to 2 selects quick decision (def=1) */
            public byte ca_pamd_qtemp; /* The Qualification template to use for PAMD */
            public ushort ca_noanswer; /* time before no answer after 1st ring (deflt=30s) */
            public ushort ca_maxintering; /* Max inter ring delay before connect (8 sec) */
        }
        /*
         * Tone ID types
         */
        private const int TID_FIRST = 250;
        private const int TID_DIAL_LCL = 250;
        private const int TID_DIAL_INTL = 251;
        private const int TID_DIAL_XTRA = 252;
        private const int TID_BUSY1 = 253;
        private const int TID_RNGBK1 = 254;
        private const int TID_BUSY2 = 255;
        private const int TID_RNGBK2 = 256;

        private const int TID_DISCONNECT = 257;
        private const int TID_FAX1 = 258;
        private const int TID_FAX2 = 259;
        private const int TID_LAST = 259;  /* last in springware */

        private const int TID_SIT_NC = 260;
        private const int TID_SIT_IC = 261;
        private const int TID_SIT_VC = 262;

        private const int TID_SIT_RO = 263;
        private const int TID_SIT_ANY = 264;

        private const int TID_SIT_NC_INTERLATA = 265;
        private const int TID_SIT_RO_INTERLATA = 266;
        private const int TID_SIT_IO = 267;


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int dx_eventhandler(uint parm);


        private const uint EV_ANYEVT = 0xFFFFFFFF;
        private const int EV_ANYDEV = -1;


        private const int USR_EXIT = -1;

        private const int INB = 3;
        private const int OUTB = 4;
        private const int TDX_USR_TIMER = TDX_UNKNOWN + 100;

        /* 
         * Termination mask defines for use with ATDX_TERMMSK( )
         */
        private const int TM_NORMTERM = 0x00000;     /* Normal Termination */
        private const int TM_MAXDTMF = 0x00001;     /* Max Number of Digits Recd */
        private const int TM_MAXSIL = 0x00002;     /* Max Silence */
        private const int TM_MAXNOSIL = 0x00004;     /* Max Non-Silence */
        private const int TM_LCOFF = 0x00008;     /* Loop Current Off */
        private const int TM_IDDTIME = 0x00010;     /* Inter Digit Delay */
        private const int TM_MAXTIME = 0x00020;     /* Max Function Time Exceeded */
        private const int TM_DIGIT = 0x00040;     /* Digit Mask or Digit Type Term. */
        private const int TM_PATTERN = 0x00080;     /* Pattern Match Silence Off */
        private const int TM_USRSTOP = 0x00100;     /* Function Stopped by User */
        private const int TM_EOD = 0x00200;     /* End of Data Reached on Playback */
        private const int TM_TONE = 0x02000;     /* Tone On/Off Termination */
        private const int TM_BARGEIN = 0x08000;     /* Play terminated due to Barge-in */
        private const int TM_ERROR = 0x80000;     /* I/O Device Error */
        private const int TM_MAXDATA = 0x100000;	  /* Max Data reached for FSK */


        // Event Types 
        private const int TDX_PLAY = 0x81; /* Play Completed */
        private const int TDX_RECORD = 0x82; /* Record Completed */
        private const int TDX_GETDIG = 0x83; /* Get Digits Completed */
        private const int TDX_DIAL = 0x84; /* Dial Completed */
        private const int TDX_CALLP = 0x85; /* Call Progress Completed */
        private const int TDX_CST = 0x86; /* CST Event Received */
        private const int TDX_SETHOOK = 0x87; /* SetHook Completed */
        private const int TDX_WINK = 0x88; /* Wink Completed */
        private const int TDX_ERROR = 0x89; /* Error Event */
        private const int TDX_PLAYTONE = 0x8A; /* Play Tone Completed */
        private const int TDX_GETR2MF = 0x8B; /* Get R2MF completed */
        private const int TDX_BARGEIN = 0x8C; /* Barge in completed */
        private const int TDX_NOSTOP = 0x8D; /* No Stop needed to be Issued */
        private const int TDX_UNKNOWN = 1000;

        private const int DE_RINGS = 1; /* Rings received */
        private const int DE_SILON = 2; /* Silence on */
        private const int DE_SILOFF = 3; /* Silenec off */
        private const int DE_LCON = 4; /* Loop current on */
        private const int DE_LCOFF = 5; /* Loop current off */
        private const int DE_WINK = 6; /* Wink received */
        private const int DE_RNGOFF = 7; /* Ring off event */
        private const int DE_DIGITS = 8; /* Digit Received */
        private const int DE_DIGOFF = 9; /* Digit tone off event */
        private const int DE_LCREV = 13; /* Loop current reversal */
        private const int DE_TONEON = 17; /* Tone ON Event Received */
        private const int DE_TONEOFF = 18; /* Tone OFF Event Received */
        private const int DE_STOPRINGS = 19; /* Stop ring detect state */
        private const int DE_VAD = 20; /* Voice Energy detected */

        private const int DX_CALLP = 1;

        private int DM_RINGS = (1 << (DE_RINGS - 1));

        private const int DX_OPTEN = 1; /* Enable Operator Intercept with Connect */
        private const int DX_OPTDIS = 2; /* Disable Operator Intercept */
        private const int DX_OPTNOCON = 3; /* Enable Operator Intercept w/o Connect */
        private const int DX_PVDENABLE = 4; /* Enable PVD */
        private const int DX_PVDOPTEN = 5; /* Enable PVD with OPTEN */
        private const int DX_PVDOPTNOCON = 6; /* Enable PVD with OPTNOCON */
        private const int DX_PAMDENABLE = 7; /* Enable PAMD */
        private const int DX_PAMDOPTEN = 8; /* Enable PAMD with OPTEN */

        private const int RLS_SILENCE = 0x80; /* Sil Bit in Raw Line Status */
        private const int RLS_DTMF = 0x40; /* DTMF Signal Bit in Raw Line Status */
        private const int RLS_LCSENSE = 0x20; /* Loop Current Sense Bit in Raw Line Status */
        private const int RLS_RING = 0x10; /* Ring Detect Bit in Raw Line Status */
        private const int RLS_HOOK = 0x08; /* Hook Switch Status Bit in Raw Line Status */
        private const int RLS_RINGBK = 0x04; /* Audible Ringback Detect Bit in Raw Line Status */

        private const int SR_STASYNC = 0; /* Single threaded async model */
        private const int SR_MTASYNC = 1; /* Multithreaded asynchronous model */
        private const int SR_MTSYNC = 2; /* Multithreaded synchronous model */

        private const int IO_CONT = 0x01; /* Next TPT is contiguous in memory */
        private const int IO_LINK = 0x02; /* Next TPT found thru tp_nextp ptr */
        private const int IO_EOT = 0x04; /* End of the Termination Parameters */
        private const int IO_DEV = 0x00; /* play/record from a file */
        private const int IO_MEM = 0x08; /* play/record from memory */
        private const int IO_UIO = 0x10; /* play/record using user I/O functions */
        private const int IO_STREAM = 0x20; /* End of the Termination for R4 Streaming API */
        private const int IO_CACHED = 0x40; /* play from cache */
        private const int IO_USEOFFSET = 0x80; /* use io_offset and io_length for non-VOX */
        private const int IO_UNIT_TIME = 0x200; /* io_offset and io_length in milliseconds */ 


        // Defines for the TPT 

        /// <summary>
        /// Maximum Number of Digits Received
        /// </summary>
        private const int DX_MAXDTMF = 1;

        /// <summary>
        /// Maximum Silence
        /// </summary>
        private const int DX_MAXSIL = 2;

        /// <summary>
        /// Maximum Non-Silence
        /// </summary>
        private const int DX_MAXNOSIL = 3;

        /// <summary>
        /// Loop Current Off
        /// </summary>
        private const int DX_LCOFF = 4;

        /// <summary>
        /// Inter-Digit Delay
        /// </summary>
        private const int DX_IDDTIME = 5;

        /// <summary>
        /// Function Time
        /// </summary>
        private const int DX_MAXTIME = 6;

        /// <summary>
        /// Digit Mask Termination
        /// </summary>
        private const int DX_DIGMASK = 7;
        /// <summary>
        /// Pattern Match Silence On
        /// </summary>
        private const int DX_PMOFF = 8;
        /// <summary>
        /// Pattern Match Silence Off
        /// </summary>
        private const int DX_PMON = 9;

        /// <summary>
        /// Digit Type Termination
        /// </summary>
        private const int DX_DIGTYPE = 11;

        /// <summary>
        /// Tone On/Off Termination
        /// </summary>
        private const int DX_TONE = 12;

        /// <summary>
        /// Maximum bytes for ADSI data
        /// </summary>
        private const int DX_MAXDATA = 13;

        private const int EVFL_SENDSELF = 0x01; /* Send event to self process */
        private const int EVFL_SENDOTHERS = 0x02; /* Send event to other processes */
        private const int EVFL_SENDALL = 0x03; /* Send event to all processes */



        







        /*
         * Defines for TPT Termination Flags
         */
        public const int TF_EDGE = 0x00;
        public const int TF_LEVEL = 0x01;
        public const int TF_CLREND = 0x02;
        public const int TF_CLRBEG = 0x04;
        public const int TF_USE = 0x08;
        public const int TF_SETINIT = 0x10;
        public const int TF_10MS = 0x20;
        public const int TF_FIRST = TF_CLREND;

        public const int TF_MAXDTMF = (TF_LEVEL | TF_USE);
        public const int TF_MAXSIL = (TF_EDGE | TF_USE);
        public const int TF_MAXNOSIL = (TF_EDGE | TF_USE);
        public const int TF_LCOFF = (TF_LEVEL | TF_USE | TF_CLREND);
        public const int TF_IDDTIME = (TF_EDGE);
        public const int TF_MAXTIME = (TF_EDGE);
        public const int TF_DIGMASK = (TF_LEVEL);
        public const int TF_PMON = (TF_EDGE);
        public const int TF_DIGTYPE = (TF_LEVEL);
        public const int TF_TONE = (TF_LEVEL | TF_USE | TF_CLREND);
        public const int TF_MAXDATA = 0;

        /*
         * Masked DTMF termination/initiation equates
         */
        private const int DM_D = 0x0001;    /* Mask for DTMF d. */
        private const int DM_1 = 0x0002;    /* Mask for DTMF 1. */
        private const int DM_2 = 0x0004;    /* Mask for DTMF 2. */
        private const int DM_3 = 0x0008;    /* Mask for DTMF 3. */
        private const int DM_4 = 0x0010;    /* Mask for DTMF 4. */
        private const int DM_5 = 0x0020;    /* Mask for DTMF 5. */
        private const int DM_6 = 0x0040;    /* Mask for DTMF 6. */
        private const int DM_7 = 0x0080;    /* Mask for DTMF 7. */
        private const int DM_8 = 0x0100;    /* Mask for DTMF 8. */
        private const int DM_9 = 0x0200;    /* Mask for DTMF 9. */
        private const int DM_0 = 0x0400;    /* Mask for DTMF 0. */
        private const int DM_S = 0x0800;    /* Mask for DTMF *. */
        private const int DM_P = 0x1000;    /* Mask for DTMF #. */
        private const int DM_A = 0x2000;    /* Mask for DTMF a. */
        private const int DM_B = 0x4000;    /* Mask for DTMF b. */
        private const int DM_C = 0x8000;    /* Mask for DTMF c. */


        // Channel Mode values 
        private const int MD_ADPCM = 0x0000; /* ADPCM data (the default) */
        private const int MD_PCM = 0x0100; /* Mu-Law PCM data */
        private const int MD_FFT = 0x0200; /* FFT data (debugging) */
        private const int MD_GAIN = 0x0000; /* AGC on */
        private const int MD_NOGAIN = 0x1000; /* AGC off */
        private const int PM_TONE = 0x0001; /* Tone initiated play/record */
        private const int RM_TONE = 0x0001;
        private const int PM_SR6 = 0x2000; /* 6KHz sampling rate (digitization) */
        private const int PM_SR8 = 0x4000; /* 8KHz sampling rate (digitization) */
        private const int RM_SR6 = 0x2000;
        private const int RM_SR8 = 0x4000;
        private const int PM_ALAW = 0x0020; /* Play A-Law data */
        private const int RM_ALAW = 0x0020; /* Record data using A-Law */
        private const int PM_DTINIT = 0x0002; /* Play with DTMF init */
        private const int RM_DTINIT = 0x0002; /* Record with DTMF init */
        private const int PM_DTINITSET = 0x0010 | 0x0002; /* Play with DTMF init set */
        private const int RM_DTINITSET = 0x0010; /* Record with DTMF init set */
        private const int R2_COMPELDIG = 0x0400; /* R2MF Compelled signalling */


        private const int PAMD_ACCU = 3;

        private const int SR_USERCONTEXT = 0x06;
        private const int SR_MODELTYPE = 0x05;
        private const int SR_POLLMODE = 0;

        private const int SRL_DEVICE = 0;

        private struct CT_DEVINFO
        {
            public int ct_prodid;
            public byte ct_devfamily;
            public byte ct_devmode;
            public byte ct_nettype;
            public byte ct_busmode;
            public byte ct_busencoding;
            public byte[] ct_rfu;
        }

        private struct DX_EBLK
        {
            public ushort ev_event; /* Event that occured */

            public ushort ev_data; /* Event-specific data */

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] ev_rfu; /* RFU for packing-independence */
        }

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_clrtpt(ref DV_TPT tptp, int size);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_recf(int chDev, string fNamep, ref DV_TPT tptp, int mode);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_fileerrno();

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_getevt(int chdev, ref DX_EBLK eblkp, int timeout);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_getdig(int chdev, ref DV_TPT tptp, out DV_DIGIT digitp, ushort mode);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_wtring(int ChDev, int numRings, int HookState, int timeout);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_initcallp(int dev);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_setparm(int dev, int par, int val);

        [DllImport("libsrlmt.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sr_getevtdev(uint evt_handle);


        [DllImport("libsrlmt.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sr_getevttype(uint evt_handle);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_setevtmsk(int dev, int mask);

        [DllImport("libsrlmt.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sr_dishdlr(int dev, uint evt_type, dx_eventhandler handler);

        [DllImport("libsrlmt.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sr_enbhdlr(int dev, uint evt_type, dx_eventhandler handler);

        [DllImport("libsrlmt.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private  static extern int sr_waitevtEx(ref int handlep, int count, int tmout, ref int handler);

        [DllImport("libsrlmt.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sr_waitevt(int tmout);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_clrcap(ref DX_CAP dx_cap);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_dial(int chdev, string dialstring, ref DX_CAP dx_cap, int flag);




        private const uint TN_LEADING = 0x02;
        private const uint TN_TRAILING = 0x04;

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_blddt(uint tid, uint freq1, uint fq1dev, uint freq2, uint fq2dev,
            uint mode);



        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_blddtcad( uint tid, uint freq1, uint fq1dev, uint freq2, uint fq2dev, 
            uint ontime, uint ontdev, uint offtime, uint offtdev, uint repcnt);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_addtone(int chdev, byte digit, byte digitType);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_deltones(int chdev);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_distone(int chdev, int toneid, int evt_mask);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_adjsv( int chdev, ushort tabletype, ushort action, ushort adjsize );


        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_enbtone(int chdev, int toneid, int evt_mask);
        
        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_play(int ChDev, ref DX_IOTT iottp, ref DV_TPT tptp, ushort mode);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_playiottdata(int ChDev, ref DX_IOTT iottp, ref DV_TPT tptp, ref DX_XPB xpbp, ushort mode);
        
        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_reciottdata(int ChDev, ref DX_IOTT iottp, ref DV_TPT tptp, ref DX_XPB xpbp, ushort mode);
                
        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_clrdigbuf(int chdev);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_fileopen([In] [MarshalAs(UnmanagedType.LPStr)] string filep, int flags, int pmode);
        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_fileopen([In] [MarshalAs(UnmanagedType.LPStr)] string filep, int flags);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_fileclose(int handle);


        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int gc_ResetLineDev(int linedev, int flag);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int gc_Close(int chdev);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_sethook(int chdev, int hookstate, int flag);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_getctinfo(int chdev, ref CT_DEVINFO ct_devinfop);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ag_getctinfo(int chdev, ref CT_DEVINFO ct_devinfop);

        [DllImport("libsrlmt.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sr_getboardcnt(string class_namep, ref int boardcntp);

        [DllImport("libsrlmt.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern String ATDV_ERRMSGP(int chdev);

        [DllImport("libsrlmt.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ATDV_LASTERR(int chdev);

        [DllImport("libsrlmt.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ATDV_SUBDEVS(int bddev);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_stopch(int chdev, int flag);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_close(int chdev, int flag);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_open(string chdevname, int flag);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_unlisten(int chdev);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ag_unlisten(int chdev);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_chgfreq(int tonetype, int fq1, int dv1, int fq2, int dv2);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_chgdur(int typetype, int on, int ondv, int off, int offdv);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int dx_chgrepcnt(int tonetype, int repcount);

        //----------------------------

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ATDX_BDNAMEP(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ATDX_CHNAMES(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_BDTYPE(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_BUFDIGS(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_CHNUM(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_DEVTYPE(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_DBMASK(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_EVTCNT(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_FWVER(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_HOOKST(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_LINEST(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_NUMCHAN(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_PHYADDR(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_PRODUCT(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_RINGCNT(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_DIGBUFMODE(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_STATE(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_TERMMSK(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_TONEID(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_TRCOUNT(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_XFERBUFSIZE(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_RXDATABUFSIZE(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_ANSRSIZ(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_CONNTYPE(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_CPERROR(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_CPTERM(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_FRQDUR(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_FRQDUR2(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_FRQDUR3(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_FRQHZ(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_FRQHZ2(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_FRQHZ3(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_FRQOUT(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_LONGLOW(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_SHORTLOW(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_SIZEHI(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_DTNFAIL(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_CRTNID(int SrlDevice);
    } // class

} // namespace
