// ReSharper disable CommentTypo
// ReSharper disable InconsistentNaming
namespace ivrToolkit.Plugin.Dialogic.Common.DialogicDefs;

public class gccfgparm_h
{
    /* The IP technology must be from 0x3000h to 0x31FFh (16384-12799) */
    public const int GC_IP_TECH_SET_MIN = 0x3000;

    /*
     *
     * The following set ID is for call event mask of a time slot
     *
     */
    public const int GCSET_CALLEVENT_MSK = 0x8; /* Call event mask set*/
    public const int GCPARM_GET_MSK = 0x0; /* generic parm */
    /* Other parm IDs that support set ID = GCSET_CALLEVENT_MSK include */
    /* GCACT_SETMSK, GCACT_ADDMSK, and GCACT_SUBMSK */

    /*
     *
     *  The following set ID is for configuring the service request parameters
     *
     */
    public const int GCSET_SERVREQ = 0x1006;  /* GCSR parameters */

    /* Parameter IDs for GCSET_SERVREQ */
    public const int PARM_SERVICEID = 1;       /* Request ID */
    public const int PARM_REQTYPE = 2;       /* Request Type */
    public const int PARM_ACK = 3;       /* Request Acknowledgement */

    /* Possible values for PARM_ACK parameter in GCSET_SERVREQ */
    public const int GC_NACK = 0;       /* No ack required or rejection */
    public const int GC_ACK = 1; /* Ack required or confirmation */

    /*
     *
     *  The following set ID is for setting the channel capabilities
     *
     */
    public const int GCSET_CHAN_CAPABILITY = 0x1004;  /* MakeCall block - Channel capability parameters */

    /* Parameter IDs for GCSET_CHAN_CAPABILITY */
    public const int GCPARM_TYPE = 1;    /* Type of capability */
    public const int GCPARM_CAPABILITY = 2;    /* Capability of the channel */
    public const int GCPARM_RATE = 3;    /* Information transfer rate */ 

    /* Possible values for GCPARM_TYPE parameter in the GCSET_CHAN_CAPABILITY set.*/
    public const int GCCAPTYPE_VIDEO = 1;    /* Video */
    public const int GCCAPTYPE_AUDIO = 2;    /* Audio */
    public const int GCCAPTYPE_3KHZ_AUDIO = 3;    /* 3Khz Audio */
    public const int GCCAPTYPE_7KHZ_AUDIO = 4;    /* 7Khz Audio */
    public const int GCCAPTYPE_RDATA = 5;  /* Raw data */
    public const int GCCAPTYPE_UDATA = 6;  /* User data */
    public const int GCCAPTYPE_UNDEFINED = 7;  /* Undefined capability type */
    public const int GCCAPTYPE_MUX = 8; /* Multiplexed capability */
    public const int GCCAPTYPE_DTMF = 9;  /* DTMF capability */

    /* Possible values for GCPARM_CAPABILITY parameter in the GCSET_CHAN_CAPABILITY set.*/
    /* Data Capabilites */
    public const int GCCAP_DATA_t120 = (int)eIPMEDIA_CODEC_TYPE.CODEC_DATA_T120; /* 1: T.120 data protocol chosen */
    public const int GCCAP_DATA_dsm_cc = (int)eIPMEDIA_CODEC_TYPE.CODEC_DATA_DSC_CC;    /* 2: DSM-cc data protocol chosen */
    public const int GCCAP_DATA_usrData = (int)eIPMEDIA_CODEC_TYPE.CODEC_DATA_USRDATA;     /* 3: User data protocol chosen */
    public const int GCCAP_DATA_t84 = (int)eIPMEDIA_CODEC_TYPE.CODEC_DATA_T84;         /* 4: T.84 data protocol chosen. */
    public const int GCCAP_DATA_t434 = (int)eIPMEDIA_CODEC_TYPE.CODEC_DATA_T434;       /* 5: T.434 data protocol chosen. */
    public const int GCCAP_DATA_h224 = (int)eIPMEDIA_CODEC_TYPE.CODEC_DATA_H224;       /* 6: H.224 data protocol chosen. */
    public const int GCCAP_DATA_nlpd = (int)eIPMEDIA_CODEC_TYPE.CODEC_DATA_NLPD;       /* 7: NLPD data protocol chosen. */
    public const int GCCAP_DATA_dsvdControl = (int)eIPMEDIA_CODEC_TYPE.CODEC_DATA_DSVDCONTROL;  /* 8: DSVD control data protocol chosen. */
    public const int GCCAP_DATA_h222 = (int)eIPMEDIA_CODEC_TYPE.CODEC_DATA_H222;      /* 9: H.222 data protocol chosen */
    public const int GCCAP_DATA_t30Fax = (int)eIPMEDIA_CODEC_TYPE.CODEC_DATA_T30FAX;     /* 10: T.30 Fax protocol chosen */
    public const int GCCAP_DATA_t140 = (int)eIPMEDIA_CODEC_TYPE.CODEC_DATA_T140;       /* 11: T.140 data protocol chosen */
    public const int GCCAP_DATA_t38UDPFax = (int)eIPMEDIA_CODEC_TYPE.CODEC_DATA_T38FAX;    /* 12: T.38 Fax protocol over UDP chosen */
    public const int GCCAP_DATA_CCITTV110 = (int)eIPMEDIA_CODEC_TYPE.CODEC_DATA_CCITTV110;  /* 13: CCITT V.110 standard */
    public const int GCCAP_DATA_CCITTV120 = (int)eIPMEDIA_CODEC_TYPE.CODEC_DATA_CCITTV120;    /* 14: CCITT V.120 standard */
    public const int GCCAP_DATA_CCITTX31 = (int)eIPMEDIA_CODEC_TYPE.CODEC_DATA_CCITTX31;   /* 15: CCITT X.31 standard */
    public const int GCCAP_DATA_nonStandard = (int)eIPMEDIA_CODEC_TYPE.CODEC_DATA_NONSTANDARD; /* 16: non-Standard Data capability */

    /* Audio Capabilities */
    public const int GCCAP_AUDIO_nonstandard = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_NONSTANDARD; /* 100: Non standard audio codec chosen */
    public const int GCCAP_AUDIO_g711Alaw64k = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_G711ALAW64K; /* 101: G.711 audio, A-law, 64k */
    public const int GCCAP_AUDIO_g711Alaw56k = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_G711ALAW56K;/* 102: G.711 audio, A-law, 56k */
    public const int GCCAP_AUDIO_g711Ulaw64k = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_G711ULAW64K; /* 103: G.711 audio, U-law, 64k */
    public const int GCCAP_AUDIO_g711Ulaw56k = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_G711ULAW56K; /* 104: G.711 audio, U-law, 56k */
    public const int GCCAP_AUDIO_G721ADPCM = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_G721ADPCM;  /* 105: ADPCM */
    public const int GCCAP_AUDIO_g722_48k = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_G722_48K;  /* 106: G.722 audio 48k */
    public const int GCCAP_AUDIO_g722_56k = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_G722_56K;   /* 107: G.722 audio 56k */
    public const int GCCAP_AUDIO_g722_64k = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_G722_64K;  /* 108: G.722 audio 64k */
    public const int GCCAP_AUDIO_g7231 = GCCAP_AUDIO_g7231_6_3k;  /* 111: G.723.1 at received bit rate */
    public const int GCCAP_AUDIO_g7231_5_3k = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_G7231_5_3K; /* 109: G.723.1 transmit at 5.3 kbit/s */
    public const int GCCAP_AUDIO_g7231_6_3k = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_G7231_6_3K; /* 110: G.723.1 transmit at 6.3 kbit/s */

    public const int GCCAP_AUDIO_g726_16k = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_G726_16K;   /* 111: G.726 at 16 kbit/s. Mapped to
    nonstandard coder in H.245 as G.726.
                                                                  is not yet specified in H.245 */

    public const int GCCAP_AUDIO_g726_24k = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_G726_24K;   /* 112: G.726 at 24 kbit/s. Mapped to
                                                                  nonstandard coder in H.245 as G.726.
                                                                  is not yet specified in H.245 */

    public const int GCCAP_AUDIO_g726_32k = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_G726_32K;    /* 113: G.726 at 32 kbit/s. Mapped to
                                                                  nonstandard coder in H.245 as G.726
                                                                  is not yet specified in H.245 */

    public const int GCCAP_AUDIO_g726_40k = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_G726_40K;    /* 114: G.726 at 40 kbit/s. Mapped to
                                                                  nonstandard coder in H.245 as G.726
                                                                  is not yet specified in H.245 */

    public const int GCCAP_AUDIO_g728 = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_G728;      /* 115: G.728 audio at 16 kbit/s */
    public const int GCCAP_AUDIO_g729 = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_G729;        /* 116: G.729 audio at 8 kbit/s */
    public const int GCCAP_AUDIO_g729AnnexA = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_G729ANNEXA;  /* 118: G.729AnnexA audio at 8 kbit/s */

    public const int GCCAP_AUDIO_g729wAnnexB = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_G729WANNEXB; /* 119: G.729 audio at 8 kbit/s with silence
                                                                  suppression as in Annex B */

    public const int GCCAP_AUDIO_g729AnnexAwAnnexB = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_G729ANNEXAWANNEXB; /* 120: G.729AnnexA audio at
                                                                                     8 kbit/s with silence
                                                                                     suppression as in
                                                                                     Annex B */

    public const int GCCAP_AUDIO_g7231AnnexCCapability = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_G7231ANNEXCCAP; /* 121: G.723.1 with Annex C */

    public const int GCCAP_AUDIO_gsmFullRate = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_GSMFULLRATE;   /* 122: Full-rate speech transcoding
    (GSM 06.10) */

    public const int GCCAP_AUDIO_gsmHalfRate = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_GSMHALFRATE;    /* 123: Half-rate speech transcoding
                                                                                 (GSM 06.20) */

    public const int GCCAP_AUDIO_gsmEnhancedFullRate = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_GSMEFULLRATE;   /* 124: Enhanced Full Rate (EFR)
                                                                                  speech transoding(GSM 06.60) */

    public const int GCCAP_AUDIO_gsmAdaptiveMultiRate = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_GSMADAPTIVEMULTIRATE;  /* 125: GSM 06.90 AdaptiveMultRate.
                                                                                         Mapped to nonstandard coder
                                                                                         in H.245 as GSM AMR not
                                                                                         yet specified in H.245 */

    public const int GCCAP_AUDIO_is11172 = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_IS11172;     /* 126: is11172AudioCapability_chosen */
    public const int GCCAP_AUDIO_is13818 = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_IS13818;      /* 127: is13818AudioCapability_chosen */

    public const int GCCAP_AUDIO_is127EnhancedVariableRate = (int)eIPMEDIA_CODEC_TYPE.CODEC_AUDIO_IS127EVARIABLERATE;  /* 128: TIA/EIA standard IS-127
                                                                                       transcoding.Mapped to nonstandard
                                                                                      coder in H.245 as IS-127 EVRC
                                                                                       is not yet specified in H.245.*/

    public const int GCCAP_AUDIO_disabled = 129;                        /* fax or data call only - no initial audio */
    public const int GCCAP_AUDIO_AMRNB_4_75k = 130;                       /* 130: CODEC_AUDIO_AMRNB_4_75k - GSM AMR rate is 4.75k*/
    public const int GCCAP_AUDIO_AMRNB_5_15k = 131;                       /* 131: CODEC_AUDIO_AMRNB_5_15k - GSM AMR rate is 5.15kk*/
    public const int GCCAP_AUDIO_AMRNB_5_9k = 132;                       /* 132: CODEC_AUDIO_AMRNB_5_9k - GSM AMR rate is 5.9k*/
    public const int GCCAP_AUDIO_AMRNB_6_7k = 133;                       /* 133: CODEC_AUDIO_AMRNB_6_7k - GSM AMR rate is 6.7k*/
    public const int GCCAP_AUDIO_AMRNB_7_4k = 134;                      /* 134: CODEC_AUDIO_AMRNB_7_4k - GSM AMR rate is 7.4k*/
    public const int GCCAP_AUDIO_AMRNB_7_95k = 135;                       /* 135: CODEC_AUDIO_AMRNB_7_95k - GSM AMR rate is 7.95k*/
    public const int GCCAP_AUDIO_AMRNB_10_2k = 136;                       /* 136: CODEC_AUDIO_AMRNB_10_2k - GSM AMR rate is 10.2k*/
    public const int GCCAP_AUDIO_AMRNB_12_2k = 137;                        /* 137: CODEC_AUDIO_AMRNB_12_2k - GSM AMR rate is 12.2k*/

    public const int GCCAP_AUDIO_AMRWB_6_6k = 143;                      /* 143: CODEC_AUDIO_AMRWB_6_6K - WB AMR rate is 6.6k*/
    public const int GCCAP_AUDIO_AMRWB_8_85k = 144;                       /* 144: CODEC_AUDIO_AMRWB_8_85K - WB AMR rate is 8.85k*/
    public const int GCCAP_AUDIO_AMRWB_12_65k = 145;                       /* 145: CODEC_AUDIO_AMRWB_12_65K - WB AMR rate is 12.65k*/
    public const int GCCAP_AUDIO_AMRWB_14_25k = 146;                       /* 146: CODEC_AUDIO_AMRWB_14_25K - WB AMR rate is 14.25k*/
    public const int GCCAP_AUDIO_AMRWB_15_85k = 147;                       /* 147: CODEC_AUDIO_AMRWB_15_85K - WB AMR rate is 15.85k*/
    public const int GCCAP_AUDIO_AMRWB_18_25k = 148;                       /* 148: CODEC_AUDIO_AMRWB_18_25K - WB AMR rate is 18.25k*/
    public const int GCCAP_AUDIO_AMRWB_19_85k = 149;                       /* 149: CODEC_AUDIO_AMRWB_19_85K - WB AMR rate is 19.85k*/
    public const int GCCAP_AUDIO_AMRWB_23_05k = 150;                       /* 150: CODEC_AUDIO_AMRWB_23_05K - WB AMR rate is 23.05k*/
    public const int GCCAP_AUDIO_AMRWB_23_85k = 151;                      /* 151: CODEC_AUDIO_AMRWB_23_85K - WM AMR rate is 23.85k*/

/* Video Capabilites */
    public const int GCCAP_VIDEO_nonstandard = (int)eIPMEDIA_CODEC_TYPE.CODEC_VIDEO_NONSTANDARD;     /* 200: Non standard video codec chosen */
    public const int GCCAP_VIDEO_h261 = (int)eIPMEDIA_CODEC_TYPE.CODEC_VIDEO_H261;          /* 201: H.261 video codec */
    public const int GCCAP_VIDEO_h262 = (int)eIPMEDIA_CODEC_TYPE.CODEC_VIDEO_H262;          /* 202: H.261 video codec */
    public const int GCCAP_VIDEO_h263 = (int)eIPMEDIA_CODEC_TYPE.CODEC_VIDEO_H263;          /* 203: H.263 video codec */
    public const int GCCAP_VIDEO_is11172 = (int)eIPMEDIA_CODEC_TYPE.CODEC_VIDEO_IS11172;       /* 204: IS11172 video codec */   

/* Other Capabilities */
    public const int GCCAP_txEncryption = (int)eIPMEDIA_CODEC_TYPE.CODEC_OTHER_TXENCRYPTION;    /* 300: Transmit enxryption capability */
    public const int GCCAP_rxEncryption = (int)eIPMEDIA_CODEC_TYPE.CODEC_OTHER_RXENCRYPTION;  /* 301: Receive enxryption capability */
    public const int GCCAP_conference = (int)eIPMEDIA_CODEC_TYPE.CODEC_OTHER_CONFERENCE;    /* 302: Conference capability */
    public const int GCCAP_h235Security = (int)eIPMEDIA_CODEC_TYPE.CODEC_OTHER_H235SECURITY;  /* 303: H.235 security capability */
    public const int GCCAP_clientUserInput = (int)eIPMEDIA_CODEC_TYPE.CODEC_OTHER_CLIENTUSERINPUT; /* 304: Client user input, used for DTMF tones */
    public const int GCCAP_muxNonStandard = (int)eIPMEDIA_CODEC_TYPE.CODEC_OTHER_MUXNONSTANDARD;  /* 305: Non standard Mux capability */
    public const int GCCAP_muxH222 = (int)eIPMEDIA_CODEC_TYPE.CODEC_OTHER_MUXH222;     /* 306: H.222 Mux capability */
    public const int GCCAP_muxH223 = (int)eIPMEDIA_CODEC_TYPE.CODEC_OTHER_MUXH223;       /* 307: H.223 Mux capability */
    public const int GCCAP_muxVgMux = (int)eIPMEDIA_CODEC_TYPE.CODEC_OTHER_MUXVGMUX;      /* 308: VG Mux capability */
    public const int GCCAP_muxH2250 = (int)eIPMEDIA_CODEC_TYPE.CODEC_OTHER_MUXH2250;    /* 309: H.225.0 Mux capability */
    public const int GCCAP_muxH223AnnexA = (int)eIPMEDIA_CODEC_TYPE.CODEC_OTHER_MUXH223ANNEXA;  /* 310: H.223 Annex A Mux capability */
    public const int GCCAP_unknown = (int)eIPMEDIA_CODEC_TYPE.CODEC_OTHER_UNKNOWN;      /* 311: unknown capability */
    public const int GCCAP_noChange = (int)eIPMEDIA_CODEC_TYPE.CODEC_OTHER_NOCHANGE;      /* 312: use previous capability */
    public const int GCCAP_dontCare = (int)eIPMEDIA_CODEC_TYPE.CODEC_OTHER_DONTCARE;     /* 313: use any capability */
    public const int GCCAP_nonStandard = (int)eIPMEDIA_CODEC_TYPE.CODEC_OTHER_NONSTANDARD; /* 314: non-Standard capability */

    /* The defines for Parm ID GCPARM_CONSDROP_HKFLASH_OVERRIDE*/
    public const int GCPV_DISABLED = 0;
    public const int GCPV_DBL_HKFLASH = 1;
    public const int GCPV_SINGLE_HKFLASH = 2;


}