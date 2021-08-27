using System.Runtime.InteropServices;
// ReSharper disable CommentTypo

namespace ivrToolkit.Plugin.Dialogic.Common.DialogicDefs
{
    public class DXCALLP_H
    {
        public const int DX_CALLP = 1;   /* Turn on Call Analysis */
        /*
         * Call Analysis Errors as returned by ATDX_CPERROR()
         */
        public const int CR_BUSY = 7; /* Line busy */
        public const int CR_NOANS = 8; /* No answer */
        public const int CR_NORB = 9; /* No ringback */
        public const int CR_CNCT = 10; /* Call connected */
        public const int CR_CEPT = 11; /* Operator intercept */
        public const int CR_STOPD = 12; /* Call analysis stopped */
        public const int CR_NODIALTONE = 17; /* No dialtone detected */
        public const int CR_FAXTONE = 18; /* Fax tone detected */
        public const int CR_ERROR = 0x100; /* Call analysis error */

        /*
         * Connection types ( ATDX_CONNTYPE() )
         */
        public const int CON_CAD = 1;  /* Cadence Break */
        public const int CON_LPC = 2;  /* Loop Current Drop */
        public const int CON_PVD = 3;  /* Positive Voice Detect */
        public const int CON_PAMD = 4;  /* Positive Answering Machine Detect */
        public const int CON_DIGITAL = 5; /* connect to pbx */

    }

    /*
     * DX_CAP
     *
     * Call Analysis parameters
     * [NOTE: All user-accessible structures must be defined so as to be
     *        unaffected by structure packing.]
     */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DX_CAP
    {
        public ushort ca_nbrdna;     /* # of rings before no answer. */
        public ushort ca_stdely;     /* Delay after dialing before analysis. */
        public ushort ca_cnosig;     /* Duration of no signal time out delay. */
        public short ca_lcdly;      /* Delay after dial before lc drop connect */
        public ushort ca_lcdly1;     /* Delay after lc drop con. before msg. */
        public ushort ca_hedge;      /* Edge of answer to send connect message. */
        public ushort ca_cnosil;     /* Initial continuous noise timeout delay. */
        public ushort ca_lo1tola;    /* % acceptable pos. dev of short low sig. */
        public ushort ca_lo1tolb;    /* % acceptable neg. dev of short low sig. */
        public ushort ca_lo2tola;    /* % acceptable pos. dev of long low sig. */
        public ushort ca_lo2tolb;    /* % acceptable neg. dev of long low sig. */
        public ushort ca_hi1tola;    /* % acceptable pos. dev of high signal. */
        public ushort ca_hi1tolb;    /* % acceptable neg. dev of high signal. */
        public ushort ca_lo1bmax;    /* Maximum interval for shrt low for busy. */
        public ushort ca_lo2bmax;    /* Maximum interval for long low for busy. */
        public ushort ca_hi1bmax;    /* Maximum interval for 1st high for busy */
        public ushort ca_nsbusy;     /* Num. of highs after nbrdna busy check. */
        public ushort ca_logltch;    /* Silence deglitch duration. */
        public ushort ca_higltch;    /* Non-silence deglitch duration. */
        public ushort ca_lo1rmax;    /* Max. short low  dur. of double ring. */
        public ushort ca_lo2rmin;    /* Min. long low  dur. of double ring. */
        public ushort ca_intflg;     /* Operator intercept mode. */
        public ushort ca_intfltr;    /* Minimum signal to qualify freq. detect. */
        public ushort rfu1;          /* -- unsigned int pvd_qtemp */
        public ushort rfu2;          /* -- */
        public ushort rfu3;          /* -- unsigned int pamd_qtemp  */
        public ushort rfu4;          /* -- */
        public ushort ca_hisiz;      /* Used to determine which lowmax to use. */
        public ushort ca_alowmax;    /* Max. low before con. if high >hisize. */
        public ushort ca_blowmax;    /* Max. low before con. if high <hisize. */
        public ushort ca_nbrbeg;     /* Number of rings before analysis begins. */
        public ushort ca_hi1ceil;    /* Maximum 2nd high dur. for a retrain. */
        public ushort ca_lo1ceil;    /* Maximum 1st low dur. for a retrain. */
        public ushort ca_lowerfrq;   /* Lower allowable frequency in hz. */
        public ushort ca_upperfrq;   /* Upper allowable frequency in hz. */
        public ushort ca_timefrq;    /* Total duration of good signal required. */
        public ushort ca_rejctfrq;   /* Allowable % of bad signal. */
        public ushort ca_maxansr;    /* Maximum duration of answer. */
        public ushort ca_ansrdgl;    /* Silence deglitching value for answer. */
        public ushort ca_mxtimefrq;  /* max time for 1st freq to remain in bounds */
        public ushort ca_lower2frq;  /* lower bound for second frequency */
        public ushort ca_upper2frq;  /* upper bound for second frequency */
        public ushort ca_time2frq;   /* min time for 2nd freq to remains in bounds */
        public ushort ca_mxtime2frq; /* max time for 2nd freq to remain in bounds */
        public ushort ca_lower3frq;  /* lower bound for third frequency */
        public ushort ca_upper3frq;  /* upper bound for third frequency */
        public ushort ca_time3frq;   /* min time for 3rd freq to remains in bounds */
        public ushort ca_mxtime3frq; /* max time for 3rd freq to remain in bounds */
        public ushort ca_dtn_pres;   /* Length of a valid dial tone (def=1sec) */
        public ushort ca_dtn_npres;  /* Max time to wait for dial tone (def=3sec)*/
        public ushort ca_dtn_deboff; /* The dialtone off debouncer (def=100ms) */
        public ushort ca_pamd_failtime; /* Wait for AMD/PVD after cadence break(default=4sec)*/
        public ushort ca_pamd_minring;  /* min allowable ring duration (def=1.9sec)*/
        public byte ca_pamd_spdval; /* Set to 2 selects quick decision (def=1) */
        public byte ca_pamd_qtemp;  /* The Qualification template to use for PAMD - Not Used on HMP*/
        public ushort ca_noanswer;   /* time before no answer after first ring (default=30sec) */
        public ushort ca_maxintering;   /* Max inter ring delay before connect (8 sec) */
    }
}
