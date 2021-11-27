using System;
using System.Runtime.InteropServices;

// ReSharper disable CommentTypo

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo

namespace ivrToolkit.Plugin.Dialogic.Common.DialogicDefs
{
    public class DXXXLIB_H
    {
        public const string DEV_CLASS_VOICE = "Voice";

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dx_open([MarshalAs(UnmanagedType.LPStr)] string namep, int oflags);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dx_getfeaturelist(int chDev, ref FEATURE_TABLE feature_tablep);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dx_close(int dev);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dx_setevtmsk(int ChDev, uint mask);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dx_stopch(int ChDev, ushort mode);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dx_listen(int chDev, ref SC_TSINFO sc_tsinfop);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dx_getxmitslot(int chDev, ref SC_TSINFO sc_tsinfop);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dx_fileopen([In][MarshalAs(UnmanagedType.LPStr)] string filep, int flags, int pmode);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dx_fileopen([In][MarshalAs(UnmanagedType.LPStr)] string filep, int flags);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]

        public static extern int dx_fileerrno();

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_TERMMSK(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_STATE(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dx_playiottdata(int ChDev, ref DX_IOTT iottp, ref DV_TPT tptp, ref DX_XPB xpbp, ushort mode);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dx_reciottdata(int ChDev, ref DX_IOTT iottp, ref DV_TPT tptp, ref DX_XPB xpbp, ushort mode);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dx_fileclose(int handle);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dx_blddt(uint tid, uint freq1, uint fq1dev, uint freq2, uint fq2dev,
            uint mode);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dx_blddtcad(uint tid, uint freq1, uint fq1dev, uint freq2, uint fq2dev,
            uint ontime, uint ontdev, uint offtime, uint offtdev, uint repcnt);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dx_addtone(int chdev, byte digit, byte digitType);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dx_deltones(int chdev);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dx_distone(int chdev, int toneid, int evt_mask);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dx_adjsv(int chdev, ushort tabletype, ushort action, ushort adjsize);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dx_enbtone(int chdev, int toneid, int evt_mask);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dx_getevt(int chdev, ref DX_EBLK eblkp, int timeout);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dx_getdig(int chdev, ref DV_TPT tptp, IntPtr DV_DIGIT, ushort mode);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dx_clrdigbuf(int chdev);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_CPTERM(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_CONNTYPE(int SrlDevice);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void dx_clrcap(ref DX_CAP capp);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dx_dial(int chdev, string dialstring, ref DX_CAP capp, ushort mode);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDX_ANSRSIZ(int SrlDevice);

        /* FAX features */
        public const int FT_FAX = 0x0001;
        public const int FT_VFX40 = 0x0002;
        public const int FT_VFX40E = 0x0004;
        public const int FT_VFX40E_PLUS = 0x0008;
        public const int FT_FAX_EXT_TBL = 0x10;
        public const int FT_RS_SHARE = 0x20;
        public const int FT_FAX_T38UDP = 0x0040; /* Communication on T38 with UDP packets */

        /* E2P features */
        public const int FT_CALLP = 0x0002;
        public const int FT_MF_TONE_DETECT = 0x0004;
        public const int FT_DPD = 0x0020;
        public const int FT_SYNTELLECT = 0x0040;
        public const int FT_ECR = 0x0080;
        public const int FT_CSP = 0x0100;
        public const int FT_CONFERENCE = 0x0200;

        /*
         * D/4X Events and masks 
         */
        public const int DE_RINGS = 1;     /* Rings received */
        public const int DE_SILON = 2;     /* Silence on */
        public const int DE_SILOF = 3;     /* Silenec off */
        public const int DE_LCON = 4;     /* Loop current on */
        public const int DE_LCOF = 5;     /* Loop current off */
        public const int DE_WINK = 6;     /* Wink received */
        public const int DE_RNGOFF = 7;     /* Ring off event */
        public const int DE_DIGITS = 8;     /* Digit Received */
        public const int DE_DIGOFF = 9;     /* Digit tone off event */
        public const int DE_LCREV = 13;    /* Loop current reversal   */
        public const int DE_TONEON = 17;    /* Tone ON  Event Received */
        public const int DE_TONEOFF = 18;    /* Tone OFF Event Received */
        public const int DE_STOPRINGS = 19;    /* Stop ring detect state */
        public const int DE_VAD = 20;    /* Voice Energy detected */
        public const int DE_UNDERRUN = 21;    /* R4 Streaming to Board API FW underrun event. Improves streaming data to board */


        /* Alt. defines for DM_SILOF, DM_LCOF are DM_SILOFF and DM_LCOFF */
        /*
         * Event mask values
         */
        public const int DM_RINGS = 1 << (DE_RINGS - 1);
        public const int DM_SILON = 1 << (DE_SILON - 1);
        public const int DM_SILOF = 1 << (DE_SILOF - 1);
        public const int DM_LCON = 1 << (DE_LCON - 1);
        public const int DM_LCOF = 1 << (DE_LCOF - 1);
        public const int DM_LCREV = 1 << (DE_LCREV - 1);
        public const int DM_WINK = 1 << (DE_WINK - 1);
        public const int DM_RNGOFF = 1 << (DE_RNGOFF - 1);
        public const int DM_DIGITS = 1 << (DE_DIGITS - 1);
        public const int DM_DIGOFF = 1 << (DE_DIGOFF - 1);
        public const int DM_VADEVTS = 1 << (DE_VAD - 1);
        public const int DM_UNDERRUN = 1 << (DE_UNDERRUN - 1);

        /*
         * Defines for Sync mode and async mode
         */
        public const int EV_ASYNC = 0x8000;
        public const int EV_SYNC = 0x0000;

        /*
         * Masked DTMF termination/initiation equates
         */
        public const int DM_D = 0x0001;    /* Mask for DTMF d. */
        public const int DM_1 = 0x0002;    /* Mask for DTMF 1. */
        public const int DM_2 = 0x0004;    /* Mask for DTMF 2. */
        public const int DM_3 = 0x0008;    /* Mask for DTMF 3. */
        public const int DM_4 = 0x0010;    /* Mask for DTMF 4. */
        public const int DM_5 = 0x0020;    /* Mask for DTMF 5. */
        public const int DM_6 = 0x0040;    /* Mask for DTMF 6. */
        public const int DM_7 = 0x0080;    /* Mask for DTMF 7. */
        public const int DM_8 = 0x0100;    /* Mask for DTMF 8. */
        public const int DM_9 = 0x0200;    /* Mask for DTMF 9. */
        public const int DM_0 = 0x0400;    /* Mask for DTMF 0. */
        public const int DM_S = 0x0800;    /* Mask for DTMF *. */
        public const int DM_P = 0x1000;    /* Mask for DTMF #. */
        public const int DM_A = 0x2000;    /* Mask for DTMF a. */
        public const int DM_B = 0x4000;    /* Mask for DTMF b. */
        public const int DM_C = 0x8000; /* Mask for DTMF c. */

        // Event Types 
        public const int TDX_PLAY = 0x81; /* Play Completed */
        public const int TDX_RECORD = 0x82; /* Record Completed */
        public const int TDX_GETDIG = 0x83; /* Get Digits Completed */
        public const int TDX_DIAL = 0x84; /* Dial Completed */
        public const int TDX_CALLP = 0x85; /* Call Progress Completed */
        public const int TDX_CST = 0x86; /* CST Event Received */
        public const int TDX_SETHOOK = 0x87; /* SetHook Completed */
        public const int TDX_WINK = 0x88; /* Wink Completed */
        public const int TDX_ERROR = 0x89; /* Error Event */
        public const int TDX_PLAYTONE = 0x8A; /* Play Tone Completed */
        public const int TDX_GETR2MF = 0x8B; /* Get R2MF completed */
        public const int TDX_BARGEIN = 0x8C; /* Barge in completed */
        public const int TDX_NOSTOP = 0x8D; /* No Stop needed to be Issued */
        public const int TDX_UNKNOWN = 1000;


        /*
         * Wave file support defines
         */
        /*
         * File formats
         */
        public const int FILE_FORMAT_VOX = 1;     /* Dialogic VOX format */
        public const int FILE_FORMAT_WAVE = 2;     /* Microsoft Wave format */
        public const int FILE_FORMAT_NONE = 3;     /* No file being used */

        /*
         * Sampling rate
         */
        public const int DRT_6KHZ = 0x30;  /* 6KHz */
        public const int DRT_8KHZ = 0x40;  /* 8KHz */
        public const int DRT_11KHZ = 0x58;  /* 11KHz */

        /*
         * Data format
         */
        public const int DATA_FORMAT_DIALOGIC_ADPCM = 0x1;		/* OKI ADPCM */
        public const int DATA_FORMAT_ALAW = 0x3;		/* alaw PCM  */
        public const int DATA_FORMAT_G726 = 0x4;		/* G.726     */
        public const int DATA_FORMAT_MULAW = 0x7;		/* mulaw PCM */
        public const int DATA_FORMAT_PCM = 0x8;		/* PCM       */
        public const int DATA_FORMAT_G729A = 0x0C;	/* CELP coder */
        public const int DATA_FORMAT_GSM610 = 0x0D;	/* Microsoft GSM (backward compatible*/
        public const int DATA_FORMAT_GSM610_MICROSOFT = 0x0D;	/* Microsoft GSM */
        public const int DATA_FORMAT_GSM610_ETSI = 0x0E;	/* ETSI standard framing */
        public const int DATA_FORMAT_GSM610_TIPHON = 0x0F;	/* ETSI TIPHON bit order */
        public const int DATA_FORMAT_LC_CELP = 0x10;		/* Lucent CELP Coder */
        public const int DATA_FORMAT_TRUESPEECH = 0x10;		/* TRUESPEECH Coder */
        public const int DATA_FORMAT_G711_ALAW = DATA_FORMAT_ALAW;
        public const int DATA_FORMAT_G711_ALAW_8BIT_REV = 0x11;
        public const int DATA_FORMAT_G711_ALAW_16BIT_REV = 0x12;
        public const int DATA_FORMAT_G711_MULAW = DATA_FORMAT_MULAW;
        public const int DATA_FORMAT_G711_MULAW_8BIT_REV = 0x13;
        public const int DATA_FORMAT_G711_MULAW_16BIT_REV = 0x14;
        public const int DATA_FORMAT_G721 = 0x15;
        public const int DATA_FORMAT_G721_8BIT_REV = 0x16;
        public const int DATA_FORMAT_G721_16BIT_REV = 0x17;
        public const int DATA_FORMAT_G721_16BIT_REV_NIBBLE_SWAP = 0x18;
        public const int DATA_FORMAT_IMA_ADPCM = 0x19;
        public const int DATA_FORMAT_RAW = 0x1A;


        public const int DATA_FORMAT_FFT = 0xFF; /* fft data  */

        /*
         * GTD Defines
         */
        public const int DM_TONEON = 0x01;  /* Tone ON Mask */
        public const int DM_TONEOFF = 0x02;  /* Tone OFF Mask */

        public const uint TONEALL = 0xFFFFFFFF;   /* Enable/Disable All Tone ID's */

        public const int TN_SINGLE = 0;     /* Single Tone */
        public const int TN_DUAL = 1;     /* Dual Tone */

        /*
         * Template Modes and Frequency for GTD
         */
        public const int TN_FREQDEV = 5;     /* Frequency Deviation */

        public const int TN_CADENCE = 0x01;  /* Cadence Detection */
        public const int TN_LEADING = 0x02;  /* Leading Edge Detection */
        public const int TN_TRAILING = 0x04;  /* Trailing Edge Detection */

        //defines for Call Progress Analysis function
        public const int CA_SIT = 0x01;		// look for a previously defined SIT tone
        public const int CA_PAMD = 0x02;		// use a previously defined PAMD template
        public const int CA_PVD = 0x04; // use a previously defined PVD template.

        /*
         * Error codes returned by ATDV_LASTERR()
         */
        public const int EDX_NOERROR = 0;     /* No Errors */
        public const int EDX_SYSTEM = 1;     /* System Error */
        public const int EDX_FWERROR = 2;     /* Firmware Error */
        public const int EDX_TIMEOUT = 3;     /* Function Timed Out */
        public const int EDX_BADIOTT = 4;     /* Invalid Entry in the DX_IOTT */
        public const int EDX_BADTPT = 5;     /* Invalid Entry in the DX_TPT */
        public const int EDX_BADPARM = 6;     /* Invalid Parameter in Function Call */
        public const int EDX_BADDEV = 7;     /* Invalid Device Descriptor */
        public const int EDX_BADPROD = 8;     /* Func. Not Supported on this Board */
        public const int EDX_BUSY = 9;    /* Device is Already Busy */
        public const int EDX_IDLE = 10;    /* Device is Idle */
        public const int EDX_STOPRINGS = 11;    /* Stop waitrings (MT only) */
        public const int EDX_WTRINGSTOP = 11;    /* Wait for Rings stopped by user */
        public const int EDX_BADWAVEFILE = 12;    /* Bad/Unsupported WAV file */
        public const int EDX_XPBPARM = 13;    /* Bad XPB structure */
        public const int EDX_NOSUPPORT = 14;    /* Data format not supported */
        public const int EDX_NOTIMP = 15;    /* Function not implemented */
        public const int EDX_BADSUBCOMMAND = 16;
        public const int EDX_BADCHANNELNUMBER = 17;
        public const int EDX_BADRESOURCEID = 18;
        public const int EDX_NORESOURCE = 19;    /* No Resources */
        public const int EDX_DSPERROR = 20;    /* Resource DSP error */
        public const int EDX_INUSE = 21;
        public const int EDX_HOOKSTATETRANSITIONERROR = 25;    /* dx_sethook() unable to transition hookstate */

        public const ushort SV_SPEEDTBL = 0x01;    /* Modify Speed */
        public const ushort SV_VOLUMETBL = 0x02;   /* Modify Volume */

        public const ushort SV_ABSPOS = 0x00;      /* Absolute Position */
        public const ushort SV_RELCURPOS = 0x10;   /* Relative to Current Position */
        public const ushort SV_TOGGLE = 0x20;      /* Toggle */

        public const ushort SV_WRAPMOD = 0x0010;
        public const ushort SV_SETDEFAULT = 0x0020;
        public const ushort SV_LEVEL = 0x0100;
        public const ushort SV_BEGINPLAY = 0x0200;

        public const ushort SV_TOGORIGIN = 0x00;    /* Toggle Between Origin and Last Modified Position */
        public const ushort SV_CURORIGIN = 0x01;    /* Reset Current Position to Origin */
        public const ushort SV_CURLASTMOD = 0x02;   /* Reset Current Position to Last Modified Position */
        public const ushort SV_RESETORIG = 0x03;    /* Reset Current Position and Last Modified State to Origin */

        /*
         * Defines for channel state values
         */
        public const int CS_IDLE = 1;     /* Channel is idle */
        public const int CS_PLAY = 2;     /* Channel is playing back */
        public const int CS_RECD = 3;     /* Channel is recording */
        public const int CS_DIAL = 4;     /* Channel is dialing */
        public const int CS_GTDIG = 5;     /* Channel is getting digits */
        public const int CS_TONE = 6;     /* Channel is generating a tone */
        public const int CS_STOPD = 7;     /* Operation has terminated */
        public const int CS_SENDFAX = 8;     /* Channel is sending a fax */
        public const int CS_RECVFAX = 9;     /* Channel is receiving a fax */
        public const int CS_CALL = 13;    /* Channel is Call Progress Mode */
        public const int CS_GETR2MF = 14;    /* Channel is Getting R2MF */
        public const int CS_BLOCKED = 16;    /* Channel is blocked */

        public const int CS_RECDPREPARE = 17;    /* Channel is preparing record and driver has not yet sent record 

        /* 
         * This is a complex state composed of one of the 
         * above states and faxmode. 
         */

        public const int CS_FAXIO = 10;    /* Channel is between fax pages */

        /*
         * Define a channel state for the remaining blocking commands
         */
        public const int CS_HOOK = 11;    /* A change in hookstate is in progress */
        public const int CS_WINK = 12;    /* A wink operation is in progress */
        public const int CS_RINGS = 15; /* Call status Rings state */

        /*
         * Channel Mode values
         */
        public const int MD_ADPCM = 0x0000;    /* ADPCM data (the default) */
        public const int MD_PCM = 0x0100;    /* Mu-Law PCM data */
        public const int MD_FFT = 0x0200;    /* FFT data (debugging) */
        public const int MD_GAIN = 0x0000;    /* AGC on */
        public const int MD_NOGAIN = 0x1000;    /* AGC off */
        public const int PM_TONE = 0x0001;    /* Tone initiated play/record */
        public const int RM_TONE = PM_TONE;
        public const int PM_SR6 = 0x2000;    /* 6KHz sampling rate (digitization) */
        public const int PM_SR8 = 0x4000;    /* 8KHz sampling rate (digitization) */
        public const int RM_SR6 = PM_SR6;
        public const int RM_SR8 = PM_SR8;
        public const int PM_ALAW = 0x0020;    /* Play A-Law data         */
        public const int RM_ALAW = PM_ALAW;   /* Record data using A-Law */
        public const int PM_DTINIT = 0x0002;    /* Play with DTMF init */
        public const int RM_DTINIT = PM_DTINIT; /* Record with DTMF init */
        public const int PM_DTINITSET = 0x0010 | PM_DTINIT; /* Play with DTMF init set */
        public const int RM_DTINITSET = PM_DTINITSET;   /* Record with DTMF init set */
        public const int R2_COMPELDIG = 0x0400;    /* R2MF Compelled signalling */
        public const int RM_USERTONE = 0x0040;
        public const int RM_NOTIFY = 0x0004;    /* record notification beep tone must be generated */
        public const int RM_VADNOTIFY = 0x0008;
        public const int RM_ISCR = 0x0080;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DX_EBLK
    {
        public ushort ev_event; /* Event that occured */

        public ushort ev_data; /* Event-specific data */

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] ev_rfu; /* RFU for packing-independence */
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DX_XPB
    {
        public ushort wFileFormat;
        public ushort wDataFormat;
        public uint nSamplesPerSec;
        public ushort wBitsPerSample;

        public override string ToString()
        {
            return $"DX_XPB({wFileFormat},{wDataFormat},{nSamplesPerSec},{wBitsPerSample})";
        }
    }

    /*
     * FEATURE_TABLE structure
     */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct FEATURE_TABLE
    {
        public ushort ft_play;
        public ushort ft_record;
        public ushort ft_tone;
        public ushort ft_e2p_brd_cfg;
        public ushort ft_fax;
        public ushort ft_front_end;
        public ushort ft_misc;
        public ushort ft_send;
        public ushort ft_receive;
        public uint ft_play_ext;
        public uint ft_record_ext;
        public ushort ft_device;
        public ushort ft_rfu;
    }
}
