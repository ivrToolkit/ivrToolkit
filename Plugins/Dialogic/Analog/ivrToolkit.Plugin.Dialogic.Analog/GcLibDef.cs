/**********@@@SOFT@@@WARE@@@COPY@@@RIGHT@@@**********************************
* DIALOGIC CONFIDENTIAL
*
* Copyright (C) 1990-2007 Dialogic Corporation. All Rights Reserved.
* The source code contained or described herein and all documents related
* to the source code ("Material") are owned by Dialogic Corporation or its
* suppliers or licensors. Title to the Material remains with Dialogic Corporation
* or its suppliers and licensors. The Material contains trade secrets and
* proprietary and confidential information of Dialogic or its suppliers and
* licensors. The Material is protected by worldwide copyright and trade secret
* laws and treaty provisions. No part of the Material may be used, copied,
* reproduced, modified, published, uploaded, posted, transmitted, distributed,
* or disclosed in any way without Dialogic's prior express written permission.
*
* No license under any patent, copyright, trade secret or other intellectual
* property right is granted to or conferred upon you by disclosure or delivery
* of the Materials, either expressly, by implication, inducement, estoppel or
* otherwise. Any license under such intellectual property rights must be
* express and approved by Dialogic in writing.
*
***********************************@@@SOFT@@@WARE@@@COPY@@@RIGHT@@@**********/
/**********************************************************************
*
*   C Header:       gclib.h
*   Description:    GlobalCall header file for application use
*   %date_created:  Thur June 29 2006 %
*
**********************************************************************/

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local

using System;
using System.Runtime.InteropServices;

namespace ivrToolkit.Plugin.Dialogic.Analog
{
    public class GcLibDef
    {

/*
 * Typedefs used throughout GlobalCall software, and application.
 */
        public const string GCLIB_DEBUG_FILE_NAME = "gclib.dbg"; /* controls where debug file list is kept */
        public const int MAX_BOARD_NAME_LENGTH = 100; /* Not including the trailing NULL */
        public const int MAX_CCLIB_NAME_LENGTH = 30; /* Not including the trailing NULL */
        public const int LIBID_GC = 0; /* GlobalCall lib's id */
        public const int GC_MAX_CRNS_PER_LINEDEV = 20;
        public const int GC_MAX_CCLIBNAME_LEN = 20; /* Maximum length of a call control library name */

/*gc1*/

        public struct LINEDEV
        {
            public int Linedev;
        }

/*gc2*/

        public struct CRN
        {
            public int crn;
        }

/*
-- bit mask for gc_GetCCLibInfo
*/
        public const int GC_CCLIB_AVL = 0x1;
        public const int GC_CCLIB_CONFIGURED = 0x2;
        public const int GC_CCLIB_FAILED = 0x4;
        public const int GC_CCLIB_STUB = 0x8;
        public const int GC_CCLIB_AVAILABLE = GC_CCLIB_AVL;

        /*
-- defines for gc_GetCallInfo()
*/
        /*#define CALLED_SUBS         0x5*/ /* In ISDN header files */
        /*#define U_IES               0x10*/ /* In ISDN header files */
        public const int CATEGORY_DIGIT = 0x100; /* Get category digit */
        public const int CONNECT_TYPE = 0x101; /* get callp connect type */
        public const int CALLNAME = 0x102; /* get caller name call ID */
        public const int CALLTIME = 0x103; /* get caller time call ID */
        public const int CALLINFOTYPE = 0x105; /* type of call (charge, no charge, etc.) */
        public const int PRESENT_RESTRICT = 0x106; /* origination presentation restriction */

        /* Defines to indicate type of information */
        /* for gc_CallAck(), gc_GetCallInfo(), gc_SendMoreInfo() and gc_ReqMoreInfo() */
        public const int ORIGINATION_ADDRESS = 0x107;
        public const int DESTINATION_ADDRESS = 0x108;

        /* Defines to retrieve protocol of incoming call via gc_GetCallInfo() */
        public const int CALLPROTOCOL = 0x109; /* protocol of current call */
        public const int CALLPROTOCOL_SIP = 0x01; /* current call is SIP protocol */
        public const int CALLPROTOCOL_H323 = 0x02; /* current call is H.323 protocol */

        /*
-- Defines for the connect types
*/
        public const int GCCT_NA = 0; /* media detection is not available (Orig. def.) */
        public const int GCCT_NOTENABLED = GCCT_NA; /* media detection never enabled (Preferred def.) */
        public const int GCCT_CAD = 1; /* connect due to cadence */
        public const int GCCT_LPC = 2; /* connect due to loop current */
        public const int GCCT_PVD = 3; /* connect due to positive voice detection */
        public const int GCCT_PAMD = 4; /* connect due to positive answering machine detection */
        public const int GCCT_FAX = GCCT_FAX1; /* for legacy re-compiles */
        public const int GCCT_INPROGRESS = 6; /* media detection not yet avail. */
        public const int GCCT_DISCARDED = 7; /* call disconnected by remote end or network */
        public const int GCCT_UNKNOWN = 8; /* unknown media type answered the call */
        public const int GCCT_FAX1 = 9; /* connect due to fax tone TID_FAX1  */
        public const int GCCT_FAX2 = 10; /* connect due to fax tone TID_FAX2 */


        /*gc3*/

        /*
 * Total number of Call Control Libraries supported by GlobalCall
 */
        public const int GC_TOTAL_CCLIBS = 10; /* Number of CC libraries suppported */

        /*
 * GlobalCall Library component
 */
        public const int GC_LIB = 0; /* GlobalCall library */

        /*
 * Call Control Libraries supported by GlobalCall
 */
        public const int GC_ALL_LIB = 0; /* All CC libraries */
        public const int GC_ICAPI_LIB = 1; /* ICAPI library */
        public const int GC_ISDN_LIB = 2; /* ISDN library */
        public const int GC_ANAPI_LIB = 3; /* ANAPI library */
        public const int GC_PDKRT_LIB = 4; /* PDKRT library */
        public const int GC_SS7_LIB = 5; /* SS7 library */
        public const int GC_DM3CC_LIB = 6; /* DM3CC library */
        public const int GC_IPM_LIB = 7; /* IP_Media library */
        public const int GC_H3R_LIB = 8; /* IP library */
        public const int GC_DIVAISDN_LIB = 9; /* Diva ISDN CC library */
        public const int GC_CUSTOM1_LIB = 100; /* Custom library # 1 */


/*
 * CCLib specific start structure, used by GC_START_STRUCT
 * Libraries should find its own structure by looking at the name field in the
 * array pointed to by GC_START_STRUCT
 */

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct CCLIB_START_STRUCT
        {
            /// char*
            [MarshalAs(UnmanagedType.LPStr)] public string cclib_name; /* Must match CCLib name in gcprod */

            public IntPtr cclib_data;
        }

/*
 * GC start structure, points to an array of CCLIB_START_STRUCT
 */

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GC_START_STRUCT
        {
            public int num_cclibs;
            public CCLIB_START_STRUCT cclib_list;
        }

/*
 * Custom CCLib start structure
 */

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GC_CUSTOMLIB_STRUCT
        {
            [MarshalAs(UnmanagedType.LPStr)] public string file_name; /* Name of library file */

            [MarshalAs(UnmanagedType.LPStr)] public string start_proc_name; /* Name of start procedure */
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GC_CCLIB_STATUS
        {
            public int num_avllibraries;
            public int num_configuredlibraries;
            public int num_failedlibraries;
            public int num_stublibraries;

            /*
    -- these are an array of strings, each string terminated with a NULL
    -- e.g. avl_libraries[0] = "GC_ICAPI_LIB"
    --        avl_libraries[1] = "GC_ISDN_LIB"
    --        avl_libraries[2] = "GC_ANAPI_LIB"
    */
            // TODO I don;t know how to marshal these!
            //public char** avllibraries;
            //public char** configuredlibraries;
            //public char** failedlibraries;
            //public char** stublibraries;
        }


/*
 * Data structure types
 */


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GC_CCLIB_STATE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = GC_MAX_CCLIBNAME_LEN)]
            public string name; /* name of the library. */
            public int state; /* state of the library */
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GC_CCLIB_STATUSALL
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = GC_TOTAL_CCLIBS)]
            public GC_CCLIB_STATE[] cclib_state;
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GETEVENT
        {
            public int poll_units; /* # of poll units before gc_GetEvent */
            /* should return */
            /* -1 = no limit */
            /* 0 is the same as 1 */
            /* NB - Only 1 or forever is currently */
            /* implemented */
            /* in the future, poll_units may be */
            /* either cycles or time */
            public int rfu1; /* reserved for future use */
            public int rfu2; /* reserved for future use */
        }

/*
-- for gc_Open() successful "termination event"
-- need to distinguish from normal BLOCKED/UNBLOCKED codes
*/
        public const int GCEV_OPEN_UNBLOCKED = -1; /* result of open being successful */
        /* a - number so not to conflict with */
        /* DTILIB codes */


/*
 * Data structure types
 */
        public const int GCME_UNKNOWN_STRUCT_TYPE = 0;
        public const int GCME_EVTBLK_STRUCT_TYPE = 1;
        public const int GCME_EVTDATA_STRUCT_TYPE = 2;


/*
 * Defines for GlobalCall API event codes
 */
        public const int DT_GC = 0x800;

/*gc4*/
        public const int GCEV_TASKFAIL = (DT_GC | 0x01); /* Abnormal condition; state unchanged */
        public const int GCEV_ANSWERED = (DT_GC | 0x02); /* Call answered and connected */
        public const int GCEV_CALLPROGRESS = (DT_GC | 0x03);
        public const int GCEV_ACCEPT = (DT_GC | 0x04); /* Call is accepted */
        public const int GCEV_DROPCALL = (DT_GC | 0x05); /* gc_DropCall is completed */
        public const int GCEV_RESETLINEDEV = (DT_GC | 0x06); /* Restart event */
        public const int GCEV_CALLINFO = (DT_GC | 0x07); /* Info message received */
        public const int GCEV_REQANI = (DT_GC | 0x08); /* gc_ReqANI() is completed */
        public const int GCEV_SETCHANSTATE = (DT_GC | 0x09); /* gc_SetChanState() is completed */
        public const int GCEV_FACILITY_ACK = (DT_GC | 0x0A);
        public const int GCEV_FACILITY_REJ = (DT_GC | 0x0B);
        public const int GCEV_MOREDIGITS = (DT_GC | 0x0C); /* cc_moredigits() is completed*/
        public const int GCEV_SETBILLING = (DT_GC | 0x0E); /* gc_SetBilling() is completed */
        public const int GCEV_ATTACH = (DT_GC | 0x0f); /* media device successfully attached */
        public const int GCEV_ATTACH_FAIL = (DT_GC | 0x10); /* failed to attach media device */
        public const int GCEV_DETACH = (DT_GC | 0x11); /* media device successfully detached */
        public const int GCEV_DETACH_FAIL = (DT_GC | 0x12); /* failed to detach media device */
        public const int GCEV_MEDIA_REQ = (DT_GC | 0x13); /* Remote end is requesting media channel */
        public const int GCEV_STOPMEDIA_REQ = (DT_GC | 0x14); /* Remote end is requesting media streaming stop */
        public const int GCEV_MEDIA_ACCEPT = (DT_GC | 0x15); /* Media channel established with peer */
        public const int GCEV_MEDIA_REJ = (DT_GC | 0x16); /* Failed to established media channel with peer*/
        public const int GCEV_OPENEX = (DT_GC | 0x17); /* Device Opened successfully */
        public const int GCEV_OPENEX_FAIL = (DT_GC | 0x18); /* Device failed to Open */
        public const int GCEV_TRACEDATA = (DT_GC | 0x19); /* Tracing data */

        public const int GCEV_ALERTING = (DT_GC | 0x21); /* The destination telephone terminal
    *

        equipment has received connection
                                            * request (in ISDN accepted the
                                            * connection request. This event is
                                            * an unsolicited event
                                            */

        public const int GCEV_CONNECTED = (DT_GC | 0x22); /* Destination answered the request */
        public const int GCEV_ERROR = (DT_GC | 0x23); /* unexpected error event */
        public const int GCEV_OFFERED = (DT_GC | 0x24); /* A connection request has been made */
        public const int GCEV_DISCONNECTED = (DT_GC | 0x26); /* Remote end disconnected */

        public const int GCEV_PROCEEDING = (DT_GC | 0x27); /* The call state has been changed to
                                            * the proceeding state */

        public const int GCEV_PROGRESSING = (DT_GC | 0x28); /* A call progress message has been
                                            * received */

        public const int GCEV_USRINFO = (DT_GC | 0x29); /* A user to user information event is
                                            * coming */

        public const int GCEV_FACILITY = (DT_GC | 0x2A); /* Network facility indication */

        public const int GCEV_CONGESTION = (DT_GC | 0x2B); /* Remote end is not ready to accept
                                            * incoming user information */

        public const int GCEV_D_CHAN_STATUS = (DT_GC | 0x2E); /* Report D-channel status to the user */

        public const int GCEV_NOUSRINFOBUF = (DT_GC | 0x30); /* User information element buffer is
                                            * not ready */

        public const int GCEV_NOFACILITYBUF = (DT_GC | 0x31); /* Facility buffer is not ready */
        public const int GCEV_BLOCKED = (DT_GC | 0x32); /* Line device is blocked */
        public const int GCEV_UNBLOCKED = (DT_GC | 0x33); /* Line device is no longer blocked */
        public const int GCEV_ISDNMSG = (DT_GC | 0x34);
        public const int GCEV_NOTIFY = (DT_GC | 0x35); /* Notify message received */
        public const int GCEV_L2FRAME = (DT_GC | 0x36);
        public const int GCEV_L2BFFRFULL = (DT_GC | 0x37);
        public const int GCEV_L2NOBFFR = (DT_GC | 0x38);
        public const int GCEV_SETUP_ACK = (DT_GC | 0x39);
        public const int GCEV_REQMOREINFO = GCEV_SETUP_ACK; /* Received request for more information from network */
        /* This is a replacement of GCEV_SETUP_ACK event */
        public const int GCEV_CALLSTATUS = (DT_GC | 0x3A); /* call status, e.g. busy */
        public const int GCEV_MEDIADETECTED = (DT_GC | 0x3B); /* Media detection completed */

/*gc5*/
/* these events only apply to those sites using ISDN DPNSS */
        public const int GCEV_DIVERTED = (DT_GC | 0x40);
        public const int GCEV_HOLDACK = (DT_GC | 0x41);
        public const int GCEV_HOLDCALL = (DT_GC | 0x42);
        public const int GCEV_HOLDREJ = (DT_GC | 0x43);
        public const int GCEV_RETRIEVEACK = (DT_GC | 0x44);
        public const int GCEV_RETRIEVECALL = (DT_GC | 0x45);
        public const int GCEV_RETRIEVEREJ = (DT_GC | 0x46);
        public const int GCEV_NSI = (DT_GC | 0x47);
        public const int GCEV_TRANSFERACK = (DT_GC | 0x48);
        public const int GCEV_TRANSFERREJ = (DT_GC | 0x49);
        public const int GCEV_TRANSIT = (DT_GC | 0x4A);
        public const int GCEV_RESTARTFAIL = (DT_GC | 0x4B);

/* end of ISDN DPNSS specific */

        public const int GCEV_ACKCALL = (DT_GC | 0x50); /* Termination event for gc_CallACK() */
        public const int GCEV_SETUPTRANSFER = (DT_GC | 0x51); /* Ready for making consultation call */
        public const int GCEV_COMPLETETRANSFER = (DT_GC | 0x52); /* Transfer completed successfully */
        public const int GCEV_SWAPHOLD = (DT_GC | 0x53); /* Call on hold swapped with active call */
        public const int GCEV_BLINDTRANSFER = (DT_GC | 0x54); /* Call transferred to consultation call */
        public const int GCEV_LISTEN = (DT_GC | 0x55); /* Channel (listen) connected to SCbus timeslot */
        public const int GCEV_UNLISTEN = (DT_GC | 0x56); /* Channel (listen) disconnected from SCbus timeslot */
        public const int GCEV_DETECTED = (DT_GC | 0x57); /* Incoming call detected */
        public const int GCEV_FATALERROR = (DT_GC | 0x58); /* Fatal error has occurred, see result value */
        public const int GCEV_RELEASECALL = (DT_GC | 0x59); /* Call Released */
        public const int GCEV_RELEASECALL_FAIL = (DT_GC | 0x5A); /* Failed to Release call*/

        public const int GCEV_DIALTONE = (DT_GC | 0x60); /* Call has transitioned to GCST_DialTone state */
        public const int GCEV_DIALING = (DT_GC | 0x61); /* Call has transitioned to GCST_Dialing state */
        public const int GCEV_ALARM = (DT_GC | 0x62); /* An alarm occurred */
        /* NOTE: this alarm is disabled by default */
        public const int GCEV_MOREINFO = (DT_GC | 0x63); /* Status of information requested\received */
        public const int GCEV_SENDMOREINFO = (DT_GC | 0x65); /* More information sent to the network */
        public const int GCEV_CALLPROC = (DT_GC | 0x66); /* Call acknowledged to indicate that the call is */
        /* now proceeding */
        public const int GCEV_NODYNMEM = (DT_GC | 0x67); /* No dynamic memory available */
        public const int GCEV_EXTENSION = (DT_GC | 0x68); /* Unsolicited extension event */
        public const int GCEV_EXTENSIONCMPLT = (DT_GC | 0x69); /* Termination event for gc_Extension() */
        public const int GCEV_GETCONFIGDATA = (DT_GC | 0x6A); /* Configuration data successfully */
        /* retrieved */
        public const int GCEV_GETCONFIGDATA_FAIL = (DT_GC | 0x6B); /* Failed to get (retrieve) the */
        /* configuration data */
        public const int GCEV_SETCONFIGDATA = (DT_GC | 0x6C); /* The configuration data successfully */
        /* updated (set) */
        public const int GCEV_SETCONFIGDATA_FAIL = (DT_GC | 0x6D); /* Failed to set (update) the */
        /* configuration data   */
        public const int GCEV_SERVICEREQ = (DT_GC | 0x6E); /* Service Request received */
        public const int GCEV_SERVICERESP = (DT_GC | 0x70); /* Service Response received */
        public const int GCEV_SERVICERESPCMPLT = (DT_GC | 0x71); /* Service Response sent */

        public const int GCEV_INVOKE_XFER_ACCEPTED = (DT_GC | 0x72); /* Invoke transfer accepted by the remote party */
        public const int GCEV_INVOKE_XFER_REJ = (DT_GC | 0x73); /* Invoke transfer rejected by the remote party */
        public const int GCEV_INVOKE_XFER = (DT_GC | 0x74); /* Successful completion of invoke transfer */
        public const int GCEV_INVOKE_XFER_FAIL = (DT_GC | 0x75); /* Failure in invoke transfer */
        public const int GCEV_REQ_XFER = (DT_GC | 0x76); /* Receiving a call transfer request */

        public const int GCEV_ACCEPT_XFER = (DT_GC | 0x77);
        /* Successfully accept the transfer request from remote party*/

        public const int GCEV_ACCEPT_XFER_FAIL = (DT_GC | 0x78);
        /* Failure to accept the transfer request from remote party*/

        public const int GCEV_REJ_XFER = (DT_GC | 0x79);
        /* Successfully reject the transfer request from remote party */

        public const int GCEV_REJ_XFER_FAIL = (DT_GC | 0x7A); /* Failure to reject the transfer request */

        public const int GCEV_XFER_CMPLT = (DT_GC | 0x7B);
        /* Successful completion of call transfer at the party receiving the request */

        public const int GCEV_XFER_FAIL = (DT_GC | 0x7C); /* Failure to reroute a transferred call  */
        public const int GCEV_INIT_XFER = (DT_GC | 0x7D); /* Successful completion of transfer initiate */
        public const int GCEV_INIT_XFER_REJ = (DT_GC | 0x7E); /* Transfer initiate rejected by the remote party */
        public const int GCEV_INIT_XFER_FAIL = (DT_GC | 0x7F); /* Failure in transfer initiate */
        public const int GCEV_REQ_INIT_XFER = (DT_GC | 0x80); /* Receiving a transfer initiate request */
        public const int GCEV_ACCEPT_INIT_XFER = (DT_GC | 0x81); /* Successfully accept the transfer initiate request */

        public const int GCEV_ACCEPT_INIT_XFER_FAIL = (DT_GC | 0x82);
        /* Failure to accept the transfer initiate request */

        public const int GCEV_REJ_INIT_XFER = (DT_GC | 0x83); /* Successfully reject the transfer initiate request */

        public const int GCEV_REJ_INIT_XFER_FAIL = (DT_GC | 0x84);
        /* Failure to reject the transfer initiate request  */

        public const int GCEV_TIMEOUT = (DT_GC | 0x85); /* Notification of generic time out  */
        public const int GCEV_REQ_MODIFY_CALL = (DT_GC | 0x86); /* received Modify request from remote party */

        public const int GCEV_REQ_MODIFY_CALL_UNSUPPORTED
            = (DT_GC | 0x87); /* unsupported inbound capability */

        public const int GCEV_MODIFY_CALL_ACK = (DT_GC | 0x88); /* acknowledge by remote party of accepting Modify */
        public const int GCEV_MODIFY_CALL_REJ = (DT_GC | 0x89); /* Rejection by remote party of Modify request */
        public const int GCEV_MODIFY_CALL_FAIL = (DT_GC | 0x8a); /* failure to send Modify request */
        public const int GCEV_MODIFY_CALL_CANCEL = (DT_GC | 0x8b); /* receipt of previous Modify cancellation request */
        /* from remote party */
        public const int GCEV_CANCEL_MODIFY_CALL = (DT_GC | 0x8c); /* successful cancellation of previous Modify */
        public const int GCEV_CANCEL_MODIFY_CALL_FAIL = (DT_GC | 0x8d); /* failure to cancel previous Modify */
        public const int GCEV_ACCEPT_MODIFY_CALL = (DT_GC | 0x8e); /* success in Modify request by remote end */
        public const int GCEV_ACCEPT_MODIFY_CALL_FAIL = (DT_GC | 0x8f); /* failure in Modify request by remote end */
        public const int GCEV_REJECT_MODIFY_CALL = (DT_GC | 0x90); /* rejection to Modify request by remote end */
        public const int GCEV_REJECT_MODIFY_CALL_FAIL = (DT_GC | 0x91); /* failure to Modify request by remote end */

        public const int GCEV_SIP_ACK = (DT_GC | 0x92); /* Receipt of ACK in responce to 200 OK on Invite transaction */
        public const int GCEV_SIP_ACK_OK = (DT_GC | 0x93); /* Sip ACK successfully sent.      */
        public const int GCEV_SIP_ACK_FAIL = (DT_GC | 0x94); /* Attempt to send Sip ACK failed. */
        public const int GCEV_SIP_200OK = (DT_GC | 0x95); /* A 200 OK was received on an Invite transaction. */

        public const int GCEV_SIP_PRACK = (DT_GC | 0x96); /* Sip PRACK received. */
        public const int GCEV_SIP_PRACK_RESPONSE = (DT_GC | 0x97); /* Sip PRACK Response received. */
        public const int GCEV_SIP_PRACK_OK = (DT_GC | 0x98); /* Sip PRACK request sent success. */
        public const int GCEV_SIP_PRACK_FAIL = (DT_GC | 0x99); /* Sip PRACK request send failure. */
        public const int GCEV_SIP_PRACK_RESPONSE_OK = (DT_GC | 0x9a); /* Sip PRACK Response request send success. */
        public const int GCEV_SIP_PRACK_RESPONSE_FAIL = (DT_GC | 0x9b); /* Sip PRACK Response request send failure. */

        public const int GCEV_SIP_SESSION_EXPIRES = (DT_GC | 0x9c); /* SIP session timer expires */

        public const int GCEV_SIP_SESSIONPROGRESS = (DT_GC | 0x9d); /* Sip Session Progress sent success. */

        public const int GCEV_TELEPHONY_EVENT = (DT_GC | 0x9e); /* Unsolicited telephony events */

        public const int GCEV_CALLUPDATE = (DT_GC | 0x9f); /* Sip UPDATE received */

        public const int GCEV_CANCELWAITCALL = (DT_GC | 0xA0); /* WaitCall Cancelled */

        public const int GCEV_FACILITYREQ = (DT_GC | 0xFF); /* A facility request is made by CO */

/*
 * MASK defines which may be modified by gc_SetEvtMsk();.
 * These masks are used to mask or unmask their corresponding events,
 * GCEV_xxxx.
 */
        public const int GCMSK_ALERTING = 0x01;
        public const int GCMSK_PROCEEDING = 0x02;
        public const int GCMSK_PROGRESS = 0x04;
        public const int GCMSK_NOFACILITYBUF = 0x08;
        public const int GCMSK_NOUSRINFO = 0x10;
        public const int GCMSK_BLOCKED = 0x20;
        public const int GCMSK_UNBLOCKED = 0x40;
        public const int GCMSK_PROC_SEND = 0x80;
        public const int GCMSK_SETUP_ACK = 0x100;
        public const int GCMSK_DETECTED = 0x200;
        public const int GCMSK_DIALTONE = 0x400;
        public const int GCMSK_DIALING = 0x800;
        public const int GCMSK_FATALERROR = 0x1000;
        public const int GCMSK_REQMOREINFO = 0x2000;
        public const int GCMSK_INVOKEXFER_ACCEPTED = 0x4000; /* Event mask for GCEV_INVOKE_XFER_ACCEPTED */
        public const int GCMSK_SIP_ACK = 0x8000;
        public const int GCMSK_200_OK = 0x10000;


/*
 * Event Mask Action values
 *
 */

        public const int GCACT_SETMSK = 0x01; /* Enable notification of events
                                         * specified in bitmask and disable
                                         * notification of previously set
                                         * events */

        public const int GCACT_ADDMSK = 0x02; /* Enable notification of events
                                         * specified in bitmask in addition
                                         * to previously set events. */

        public const int GCACT_SUBMSK = 0x03; /* Disable notification of events
                                         * specified  in bitmask. */

/*
 * BUFFER sizes
 */
        public const int GC_BILLSIZE = 0x60; /* For storing billing info */
        public const int GC_ADDRSIZE = 128; /* For storing ANI or DNIS digits. */


/*
 * Components supported for gc_GetVer()
 */
        public const int GCGV_LIB = GC_LIB; /* GlobalCall library */
        public const int ICGV_LIB = GC_ICAPI_LIB; /* ICAPI library */
        public const int ISGV_LIB = GC_ISDN_LIB; /* ISDN library */
        public const int ANGV_LIB = GC_ANAPI_LIB; /* ANAPI library */
        public const int PDGV_LIB = GC_PDKRT_LIB; /* PDKRT library */
        public const int SS7GV_LIB = GC_SS7_LIB; /* SS7 library */
        public const int DM3CCGV_LIB = GC_DM3CC_LIB; /* DM3 library */
        public const int IPMGV_LIB = GC_IPM_LIB; /* IP Media library */
        public const int IPH3RGV_LIB = GC_H3R_LIB; /* IP Protocol library */
        public const int DIVAISDNGV_LIB = GC_DIVAISDN_LIB; /* Diva ISDN library */


/*
 * Cause definitions for dropping a call
 */
        public const int GC_UNASSIGNED_NUMBER = 0x01; /* Number unassigned / unallocated */
        public const int GC_NORMAL_CLEARING = 0x10; /* Call dropped under normal conditions*/
        public const int GC_CHANNEL_UNACCEPTABLE = 0x06;
        public const int GC_USER_BUSY = 0x11; /* End user is busy */
        public const int GC_CALL_REJECTED = 0x15; /* Call was rejected */
        public const int GC_DEST_OUT_OF_ORDER = 0x19; /* Destination is out of order */
        public const int GC_NETWORK_CONGESTION = 0x2a;
        public const int GC_REQ_CHANNEL_NOT_AVAIL = 0x2c; /* Requested channel is not available */
        public const int GC_SEND_SIT = 0x300; /* send Special Info. Tone (SIT) */

/*
 * RATE types for gc_SetBilling()
 */
        public const int GCR_CHARGE = 0x0000; /* Charge call (default) */
        public const int GCR_NOCHARGE = 0x0100; /* Do not charge call */


/*
 * Defines for 'parm' parameter of gc_SetParm() and gc_GetParm()
 * gc6
 */
        public const int GCPR_ALARM = 1; /* Enable or disable alarm handling */
        public const int GCPR_WAITIDLE = 2; /* Change wait for idle time-out */
        public const int GCPR_LOADTONES = 4; /* Enable or disable loading tone */
        public const int GCPR_RINGBACKID = 5; /* GTD id for ring back tone */
        public const int GCPR_OUTGUARD = 6; /* maximum time for call progress */
        public const int GCPR_MINDIGITS = 7; /* min # of digits */
        public const int GCPR_CALLINFOTYPE = 8; /* type of call (charge, no charge, etc.) */
        public const int GCPR_RECEIVE_INFO_BUF = 9; /* size of cyclic buffers to store sig. msgs */
        public const int GCPR_MEDIADETECT = 10; /* Enable or disable media detection */
        public const int GCPR_PROTVER = 11; /* Retrieve protocol version            */
        public const int GCPR_CALLPROGRESS = 12; /* Enable or disable call progress */
        public const int GCPR_CALLINGPARTY = 0x4001; /* set or get terminal phone number */
        public const int GCPR_CATEGORY = 0x104; /* request caller category              */
        public const int GCPR_CONNECT_TYPE = 0x106; /* request perfectcall connect type */

        public const int GCPV_ENABLE = 1; /* enable feature */
        public const int GCPV_DISABLE = 0; /* disable feature */

/*
 * Call States
 */
/*
 * The call states defined for basic services   
 */
        public const int GCST_NULL = 0x00;
        public const int GCST_ACCEPTED = 0x01;
        public const int GCST_ALERTING = 0x02;
        public const int GCST_CONNECTED = 0x04;
        public const int GCST_OFFERED = 0x08;
        public const int GCST_DIALING = 0x10;
        public const int GCST_IDLE = 0x20;
        public const int GCST_DISCONNECTED = 0x40;
        public const int GCST_DIALTONE = 0x80;
        public const int GCST_ONHOLDPENDINGTRANSFER = 0x100;
        public const int GCST_ONHOLD = 0x200;
        public const int GCST_DETECTED = 0x400;

        public const int GCST_PROCEEDING = 0x800;
        public const int GCST_SENDMOREINFO = 0x1000;
        public const int GCST_GETMOREINFO = 0x2000;
        public const int GCST_CALLROUTING = 0x4000;

/*
 * The call states defined for supplementary services  
 */
        public const int GCSUPP_CALLSTBASE = 0x40000000; /* Call state base for GC supplementary services */
        public const int GCST_INVOKE_XFER_ACCEPTED = (GCSUPP_CALLSTBASE | 0x1);
        public const int GCST_INVOKE_XFER = (GCSUPP_CALLSTBASE | 0x2);
        public const int GCST_REQ_XFER = (GCSUPP_CALLSTBASE | 0x4);
        public const int GCST_ACCEPT_XFER = (GCSUPP_CALLSTBASE | 0x8);
        public const int GCST_XFER_CMPLT = (GCSUPP_CALLSTBASE | 0x10);
        public const int GCST_REQ_INIT_XFER = (GCSUPP_CALLSTBASE | 0x20);
        public const int GCST_HOLD = (GCSUPP_CALLSTBASE | 0x40);
        public const int GCST_HELD = (GCSUPP_CALLSTBASE | 0x80);



/*
 * Call State Masks
 */
        public const int GCMSK_ALERTING_STATE = GCST_ALERTING;
        public const int GCMSK_DETECTED_STATE = GCST_DETECTED;
        public const int GCMSK_CALLROUTING_STATE = GCST_CALLROUTING;
        public const int GCMSK_GETMOREINFO_STATE = GCST_GETMOREINFO;
        public const int GCMSK_PROCEEDING_STATE = GCST_PROCEEDING;
        public const int GCMSK_SENDMOREINFO_STATE = GCST_SENDMOREINFO;


/*
 * Channel states which may be set using gc_SetChanState()
 */
        public const int GCLS_INSERVICE = 0; /* Set channel to in service */
        public const int GCLS_MAINTENANCE = 1; /* Set channel to maintenance state */
        public const int GCLS_OUT_OF_SERVICE = 2; /* Set channel to out of service */


/*
 * Defines for gc_CallACK() when getting more digits.
 */

        public const int GCIF_DDI = 1; /* get additional DDI digits */


/*gc7*/
        public const int GCDG_COMPLETE = 0x0000; /* No more digits after that */
        public const int GCDG_PARTIAL = 0x0100; /* Maybe more digits after that */
        public const int GCDG_NDIGIT = 0x00FF; /* Get infinite string of digits */
        public const int GCDG_MAXDIGIT = 0x00E; /* maximum # of DDI digits which can be collected */

/*
-- Defines for gc_GetLinedevState
*/
        public const int GCGLS_BCHANNEL = 0x0; /* B channel (ISDN) */
        public const int GCGLS_DCHANNEL = 0x1; /* D channel (ISDN) */

        public const int GC_MAXNFACNETWORKID = 251; /* Maximum non-facility network ID */

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct METAEVENT
        {
/*
-- Note: structure is ordered with longest fields 1st
-- to improve access time with some compilers
*/
            public long magicno; /* for internal validity check */

            /* application calls gc_GetMetaEvent() to fill in these fields */
            public uint flags; /* flags field */
            /* - possibly event data structure type */
            /* i.e. evtdata_struct_type */
            public IntPtr evtdatap; /* pointer to the event data block */
            /* other libraries to be determined */
            /* sr_getevtdatap() */
            public int evtlen; /* event length */
            /* sr_getevtlen */
            public int evtdev; /* sr_getevtdev */
            public int evttype; /* Event type */

            /* linedev & crn are only valid for GlobalCall events */
            public LINEDEV linedev; /* linedevice */
            public CRN crn; /* crn - if 0 then no crn for this event */
            public IntPtr extevtdatap; /* pointer to abstract data buffer */
            public IntPtr usrattr; /* user attribute */
            public int cclibid; /* ID of CCLib that event is associated with */
            /* + = CCLib ID number */
            /* -1 = unknown */
            public int rfu1; /* for future use only */

        }

/* define(s) for flags field within METAEVENT structure */

        public const int GCME_GC_EVENT =
            0x1; /* Event is a GlobalCall event */

/*gc8*/
        public const int MAXPHONENUM = 32; /* Maximum length of a phone number */
        public const int MAX_ADDRESS_LEN = GC_ADDRSIZE; /* Maximum length of an address */


/* define(s) for type field within GC_CALLACK_BLK structure */
        public const int GCACK_SERVICE_DNIS = 0x1;
        public const int GCACK_SERVICE_ISDN = 0x2;
        public const int GCACK_SERVICE_PROC = 0x3;
        public const int GCACK_SERVICE_INFO = 0x4;



        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GC_CALLACK_BLK
        {
            public uint type; /* type of a structure inside following union */
            public long rfu; /* will be used for common functionality */

            // TODO I don't know what a unitn is
            //union {
            //    struct {
            //        int accept;
            //    } dnis;
            //    struct {
            //        int acceptance;
            //        /* 0x0000 proceding with the same B chan */
            //        /* 0x0001 proceding with the new B chan */
            //        /* 0x0002 setup ACK */
            //        LINEDEV linedev;
            //    } isdn;
            //    struct {          
            //        int info_len;
            //        int info_type; 
            //    } info;
            //    struct {
            //        long gc_private[4];
            //    } gc_private;
            //} service;               /* what kind of service is requested */
            //                         /* related to type field */
        }

        // TODO this was initially a union so probably is done incorrectly
        public struct GC_PARM
        {
            public short shortvalue;
            public uint ulongvalue;
            public int longvalue;
            public int intvalue;
            public uint uintvalue;
            public byte charvalue;
            [MarshalAs(UnmanagedType.LPStr)] public string paddress;
            public IntPtr pstruct;
        }

/*
 * For GC_PARM_DATA and GC_PARM_BLK, the value_buf and the parm_data_buf
 * parameters, respectively, are used as placeholders to access the data in the
 * contagious memory block, which is what GC_PARM_BLK was designed for.
 * Applications should be aware that the one byte allocated should be used for
 * the data.  Applications, however, are strongly recommended to use the gc_util
 * functions for manipulation of the GC_PARM_BLK to avoid having to deal with
 * these issues.
 */

/* 
    GC_PARM_DATA data structure used in GC_PARM_BLK
*/

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GC_PARM_DATA
        {
            public ushort set_ID; /* Set ID (two bytes long)*/
            public ushort parm_ID; /* Parameter ID (two bytes long) */
            public byte value_size; /* Size of value_buf in bytes */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public byte[] value_buf; /* Address to the parm value buffer */
        }

/* 
    Generic GC_PARM_BLK data structure
*/

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GC_PARM_BLK
        {
            public ushort parm_data_size; /* Size of parm_data_buf in bytes */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public byte[] parm_data_buf; /* Address to the parm data buffer,
                                            * a chain of GC_PARM_DATA */
        }


/*

 * An extended version of GC_PARM_DATA, called GC_PARM_DATA_EXT, has been added  

 * to enable applications access to parm data greater than 255 bytes. This structure

 * must be initialized by calling INIT_GC_PARM_DATA_EXT() prior to initial use.

 * Applications are strongly recommended to use gc_util_next_parm_ex() and 

 * gc_util_find_parm_ex() in conjunction with GC_PARM_DATA_EXT when retrieve  

 * GC_PARM_BLK data that can potentially contain parm data greater than 255 bytes.

 */

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GC_PARM_DATA_EXT

        {

            public uint version; /* Current version of this structure */

            public IntPtr pInternal; /* For internal use only */

            public uint set_ID; /* Set ID (four bytes long)*/

            public uint parm_ID; /* Parameter ID (four bytes long)*/

            public uint data_size; /* Size of data pointed to by pData (four bytes long)*/

            public IntPtr pData; /* Data pointer(four bytes long)*/

        }



/*

 * Define for GC_PARM_DATA_EXT version field.

 */

        public const int GC_PARM_DATA_EXT_VERSION = 0x100; /* Current GC_PARM_DATA_EXT version */



/*

 * Define used to initialize the GC_PARM_DATA_EXT structure.

 */

        public const int GC_CFG_DEFAULT = -1; /* Default value */



/*

 * GC_PARM_DATA_EXT initialization macro.

 */

//static __inline void INIT_GC_PARM_DATA_EXT(GC_PARM_DATA_EXTP pGCParmDataExt) {

//  pGCParmDataExt->version   = GC_PARM_DATA_EXT_VERSION;     

//  pGCParmDataExt->pInternal = NULL;      

//  pGCParmDataExt->set_ID    = (unsigned long)GC_CFG_DEFAULT;      

//  pGCParmDataExt->parm_ID   = (unsigned long)GC_CFG_DEFAULT;    

//  pGCParmDataExt->data_size = (unsigned long)GC_CFG_DEFAULT;

//  pGCParmDataExt->pData     = NULL;

//}                           


/*
 List of target type for gc_GetConfigData and gc_SetConfigData
*/

        public const int GCTGT_GCLIB_SYSTEM = 0; /* GCLib target object followed by */
        /* target ID = 0 */
        public const int GCTGT_CCLIB_SYSTEM = 1; /* CCLib target object followed by */
        /* target ID = CCLib ID */
        public const int GCTGT_PROTOCOL_SYSTEM = 2; /* Protocol target object followed by */
        /* target ID = Protocol ID */
        public const int GCTGT_FIRMWARE_SYSTEM = 3; /* Firware target object followed by */
        /* target ID = Firmware ID */
        public const int GCTGT_GCLIB_BOARD = 4; /* Physical Board target object in GlobalCall */
        /* Library followed by target ID = Board ID */
        public const int GCTGT_CCLIB_BOARD = 5; /* Physical Board target object in Call Control */
        /* Library followed by target ID = Board ID */
        public const int GCTGT_FIRMWARE_BOARD = 7; /* Physical Board target object in Firmware */
        /* followed by ID = Board ID */
        public const int GCTGT_GCLIB_NETIF = 8; /* Network Interface board target object in */
        /* GlobalCall Library followed by */
        /* target ID = Line Device ID */
        public const int GCTGT_CCLIB_NETIF = 9; /* Network Interface board target object in */
        /* Call Control Library followed by */
        /* target ID = Line Device ID */
        public const int GCTGT_PROTOCOL_NETIF = 10; /* Network Interface board target object in */
        /* Protocol module followed by target ID = Line Device ID */
        public const int GCTGT_FIRMWARE_NETIF = 11; /* Network Interface board target object in */
        /* Firmware module followed by target ID = Line Device ID */
        public const int GCTGT_GCLIB_CHAN = 12; /* Time Slot target object in GlobalCall Library */
        /* followed by target ID = Line Device ID */
        public const int GCTGT_CCLIB_CHAN = 13; /* Time Slot target object in Call Control Library */
        /* followed by target ID = Line Device ID */
        public const int GCTGT_PROTOCOL_CHAN = 14; /* Time Slot target object in Protocol module */
        /* followed by target ID = Line Device ID */
        public const int GCTGT_FIRMWARE_CHAN = 15; /* Time Slot target object in Firmware followed */
        /* by target ID = Line Device ID */
        public const int GCTGT_GCLIB_CRN = 16; /* Call target object in GlobalCall Library */
        /* followed by target ID = CRN */
        public const int GCTGT_CCLIB_CRN = 17; /* Call target object in Call Control Library */
        /* followed by target ID = CRN */
        public const int GCTGT_UNASSIGNED = 100; /* Any target object not assigned in GC yet */
        /* followed by target ID = any long value */


/* 
List of possible value for update condition argument in gc_SetConfigData()
*/
        public const int GCUPDATE_IMMEDIATE = 0;
        public const int GCUPDATE_ATNULL = 1;

/*
*  The following data structure is used in evtdatap field of GC METAEVENT for RTCM  
*/

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GC_RTCM_EVTDATA
        {
            public int request_ID; /* The RTCM request ID */
            public int gc_result; /* GC result value for this event */
            public int cclib_result; /* CCLib result value for this event */
            public int cclib_ID; /* CCLib ID for the result */

            [MarshalAs(UnmanagedType.LPStr)]
            public string additional_msg; /* Additional message for this event */

            // TODO HELP! i don't know what to do with this one?????
            //public GC_PARM_BLKP retrieved_parmblkp; /* Retrieved GC_PARM_BLK -- */
            /* used for gc_GetConfigData() in */
            /* asynchronous mode */
            public int target_type; /* Target type */
            public int target_id; /* Target ID */
        }

/* structure for gc_GetDeviceNameInfo */

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GC_DEVICENAME_INFO
        {
            public int cclibid;
            public long rfu;
        }


/* 
 *    This structure is used for providing address information about the 
 *    origination and destination parties in the makecall block.
 */

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GCLIB_ADDRESS_BLK
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ADDRESS_LEN)]
            public string address; /* address data element */

            public byte address_type; /* address type parameter */
            public byte address_plan; /* address plan parameter */

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ADDRESS_LEN)]
            public string sub_address; /* address data element */

            public byte sub_address_type; /* sub-address data element */
            public byte sub_address_plan; /* sub-address plan data element */
        }

/*
 *   This structure provides information about the medium e.g. timeslot to be
 *   used for the call. See the Technology User's Guide for support information.  
 */

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GCLIB_CHAN_BLK
        {
            public byte medium_id; /* select timeslot/port */
            public byte medium_sel; /* Preferred or exclusive */
        }

/*
 *   This structure provides information about the call e.g. category of the
 *   call. See the Technology User's Guide for support information.  
 */

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GCLIB_CALL_BLK
        {
            public byte category; /* Category of the call */
            public byte address_info; /* Indicates if address is */
            /* complete or overlap mode */
        }

/*
 *   This is the Generic GC makecall block to be used for all technologies.
 */

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GCLIB_MAKECALL_BLK
        {
            public GCLIB_ADDRESS_BLK destination; /* Called party information */
            public GCLIB_ADDRESS_BLK origination; /* Calling party information */
            public GCLIB_CHAN_BLK chan_info; /* Channel information */
            public GCLIB_CALL_BLK call_info; /* Call information */
            // TODO HELP! i don't know what to do with this one?????
            //public GC_PARM_BLKP ext_datap; /* Extended parameters */
        }


/*
 *    This structure is used for passing the GC generic makecall block or 
 *    a technology specific makecall block.
 */

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GC_MAKECALL_BLK
        {
            public GCLIB_MAKECALL_BLK gclib; /* Generic makecall block */
            public IntPtr cclib; /* cclib specific portion */
        }



//typedef union {
//  struct {
//     long       cents;
//  } ATT, *ATT_PTR;
//} GC_RATE_U,  *GC_RATE_U_PTR;


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GCLIB_WAITCALL_BLK
        {
            public long flags;
            public long rfu;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GC_WAITCALL_BLK
        {
            public GCLIB_WAITCALL_BLK gclib; /* GlobalCall specific portion */
            public IntPtr cclib; /* cclib specific portion */
        }



/*
-- structures for gc_SndMsg
-- This structure is an rfu
*/

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GCLIB_IE_BLK
        {
            public long flags;
            public long rfu;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GC_IE_BLK
        {
            public GCLIB_IE_BLK gclib; /* GlobalCall specific portion */
            public IntPtr cclib; /* cclib specific portion */
        }



/*
 * Type definition for SCBus Timeslots information structure.
 * NOTE: All GlobalCall libraries/applications should include this header file in order to 
 * access this structure, i.e. this structure must not be accessed from another header file 
 * (e.g. dtilib.h/msilib.h/dxxxlib.h)
 */
//#ifndef __SC_TSINFO__
//#define __SC_TSINFO__
//typedef struct sc_tsinfo {
//    unsigned long         sc_numts;
//    long                  *sc_tsarrayp;
//} SC_TSINFO;
//#endif



/*
-- structures for gc_SndFrame
-- This structure is an rfu
*/

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GCLIB_L2_BLK
        {
            public long flags;
            public long rfu;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GC_L2_BLK
        {
            public GCLIB_L2_BLK gclib; /* GlobalCall specific portion */
            public IntPtr cclib; /* cclib specific portion */
        }



/*
-- Alarm support data and structures
*/
/*************************************/
/* maximum # of alarm source objects */
/*************************************/
        public const int MAX_ASOS = 25; /* maximum # of alarm source objects */
/*
-- alarm list structure
-- used to pass information between layers with a list of alarms
*/

        public const int MAX_NUMBER_OF_ALARMS = 32; /* Max # of alarms can have */
        /* Caution: implementation assumes */
        /* 32 = unsigned long */
        /* if ever change this, change the */
        /* implementation also!!!! */

/**********************************************/
/* description of one field within ALARM_LIST */
/**********************************************/

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct ALARM_FIELD
        {
            public long alarm_number; /* the alarm number */

            /* Note: type of data is assumed to be known by the context */
            public GC_PARM alarm_data; /* data for the alarm */
            /* field is typically ignored */
            /* if transmitting or stopping transmission */
            /* of an alarm */
            public GC_PARM rfu; /* reserved for future use, must be 0 */
        }

/********************************/
/* description of an alarm list */
/********************************/

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct ALARM_LIST
        {
            public int n_alarms; /* # of alarms in this list */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_NUMBER_OF_ALARMS)]
            public ALARM_FIELD[] alarm_fields;
        }


        public const int MAX_NUMBER_OF_ALARM_PARMS = 32; /* maximum number of alarm parms   */
        /* that can be set/get in one call */

/****************************************************************/
/* ALARM_PARM_FIELD represents one entry in the ALARM_PARM_LIST */
/****************************************************************/

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct ALARM_PARM_FIELD
        {
            /****************************************************/
            /* a union since different ASOs may have different  */
            /* formats for the size                             */
            /****************************************************/
            public GC_PARM alarm_parm_number;

            /* Note: type of data is assumed to be known by the context */
            public GC_PARM alarm_parm_data;

            public GC_PARM rfu; /* resserved for future use, must be 0 */
        }

/**********************************************************/
/* ALARM_PARM_LIST represents a list of alarms parameters */
/**********************************************************/

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct ALARM_PARM_LIST
        {
            public int n_parms; /* # of alarm parameters */
            /* in this list */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_NUMBER_OF_ALARM_PARMS)]
            public ALARM_PARM_FIELD[] alarm_parm_fields;
        }



/*
-- alarm event structure
-- used by GC to pass information to the application
-- about an individual alarm
*/

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GC_ALARM_EVENT
        {
            public uint alarm_source_ID; /* source of the alarm */
            public int alarm_number; /* e.g if an E1 alarm, DTE1_LOS */
        }


        /*****************************/
        /* structures for alarm list */
        /*****************************/

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct ALARM_SOURCE_OBJECT_FIELD
        {
            public GC_PARM aso_data; /* type is assumed to be known by the context */
            /* typically will be char * or unsigned long */
            public int rfu; /* reserved for future use, must be 0 */
        }

        public const int ASO_LIST_BY_NAME = 0;
        public const int ASO_LIST_BY_ID = 1;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct ALARM_SOURCE_OBJECT_LIST
        {
            public int n_asos; /* # of asos in this data structure */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_ASOS)]
            public ALARM_SOURCE_OBJECT_FIELD[] aso_fields;
        }

        /***********************************/
        /* defines used by alarm functions */
        /***********************************/
/*
-- set & get configuration values 
*/
        public const int ALARM_CONFIG_BLOCKING = 0; /* set alarm configuration to be */
        /* blocking or non-blocking */
        public const int ALARM_CONFIG_NOTIFY = 1; /* set alarm configuration for */
        /* notify/no notify */
        public const int ALARM_CONFIG_NAME = 2; /* name of alarms in English */
        public const int ALARM_CONFIG_STATUS = 3; /* current status of the alarm - off or on */
        public const int ALARM_CONFIG_STATUS_BLOCKING = 4; /* current status of the blocking alarms - */
        /* off or on */

/*
-- What is configured when call gc_SetAlarmConfiguration()/gc_SetAlarmNotifyAll()
-- Defaults are as follows:
--      If configuring alarm source object, only configure the alarm source object
--      If configuring board, only configure the board
--
-- These flags allow the application the following options:
--      If configuring alarm source object, optionally do one of the following:
--          1. configure all all boards associated with this alarm source object
--          2. configure all all boards and timeslots associated with this alarm source object
--      It is not legal to configure all timeslots associated with this alarm source object
--      without also configuring all boards
--
--      If configuring board, optionally configure all timeslots associated with
--      this board
*/
/* the next two defines are valid when configuring an alarm source object */
        public const int ALARM_CONFIGURE_BOARDS = 0x100;
        public const int ALARM_CONFIGURE_BOARDS_AND_TIMESLOTS = 0x200;

/* the next define is only valid when configuring a board */
        public const int ALARM_CONFIGURE_TIMESLOTS = 0x400;

//public const int          ALARM_CONFIGURE_FLAGS (ALARM_CONFIGURE_BOARDS              
//                        | ALARM_CONFIGURE_BOARDS_AND_TIMESLOTS                    
//                        | ALARM_CONFIGURE_TIMESLOTS);


/*
-- ALARM_CONFIG_BLOCKING values
*/
        public const int ALARM_NONBLOCKING = 0; /* this alarm is non-blocking */
        public const int ALARM_BLOCKING = 1; /* this alarm is blocking */

/*
-- ALARM_CONFIG_NOTIFY values
*/
        public const int ALARM_NONOTIFY = 0; /* Do notify on this alarm */
        public const int ALARM_NOTIFY = 1; /* notify on this alarm */
/*
-- ALARM_CONFIG_STATUS, ALARM_CONFIG_STATUS_BLOCKING values
*/
        public const int ALARM_ON = 1; /* alarm is on */
        public const int ALARM_OFF = 0; /* alarm is off */

/*
-- Flow control. Used to prevent flooding
*/
        public const int ALARM_FLOW_ALWAYS = 0; /* send all alarm events to the app */
        public const int ALARM_FLOW_FIRST_AND_LAST_BLOCKING = 1; /* send the first & last */
        /* blocking alarm */
        /* events to the app */
        public const int ALARM_FLOW_FIRST_AND_LAST = 2; /* send first & last alarm events */
        /* to the app */
        public const int ALARM_FLOW_ALWAYS_BLOCKING = 3; /* send all blocking alarms to the app */
        /* but do not send non-blocking */
/*
-- alarm source ID
*/
        public const int ALARM_SOURCE_ID_NA = 0; /* Not applicable - needed internally */
        public const int ALARM_SOURCE_ID_SPRINGWARE_E1 = 1; /* Springware E1 */
        public const int ALARM_SOURCE_ID_SPRINGWARE_T1 = 2; /* Springware T1 */
        public const int ALARM_SOURCE_ID_DM3_E1 = 3; /* DM3 E1 */
        public const int ALARM_SOURCE_ID_DM3_T1 = 4; /* DM3 T1 */
        public const int ALARM_SOURCE_ID_LOGICAL_ALARM = 5; /* logical alarms - internal use only */
        public const uint ALARM_SOURCE_ID_NETWORK_ID = 0xFFFFFFFF; /* network alarm source object ID */
        /* may be used instead of actual */
        /* network ID */
/* where the dynamic assigned start */
        public const int ALARM_SOURCE_ID_DYNAMIC_START = (ALARM_SOURCE_ID_LOGICAL_ALARM + 1);

/* defines parameter set IDs for alarm source objects supported by GC */
        public const int ParmSetID_dt_alarm = 1; /* dt_setalrm() */
        public const int ParmSetID_parm = 2; /* dt_get/setparm */

/* duration field values for gc_SetUserInfo( ) */
        public const int GC_SINGLECALL = 0;
        public const int GC_ALLCALLS = 1;
        public const int GC_NEXT_OUTBOUND_MSG = 2;
        public const int GC_SINGLE_SIP_SESSION = 3;

/* EXTENSIONEVTBLK passed in GCEV_EXTENSION events for feature transparency and extension info: */

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct EXTENSIONEVTBLK
        {
            public byte ext_id;
            public GC_PARM_BLK parmblk;
        }

/*
 * The following data structure defines the rerouting information, e.g., used as event data for 
 * GCEV_REQ_XFER and GCEV_INIT_XFER
 */

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GC_REROUTING_INFO
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string rerouting_num; /* Rerouting number, terminated with '\0' */
            public GCLIB_ADDRESS_BLK rerouting_addrblkp; /* Rerouting address block    */
            public GC_PARM_BLK parm_blkp; /* Additional parameters associated */
        }

/*
 * The following data structure defines the error or result information 
 */

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GC_INFO
        {
            public int gcValue;

            [MarshalAs(UnmanagedType.LPStr)]
            public string gcMsg;

            public int ccLibId;

            [MarshalAs(UnmanagedType.LPStr)]
            public string ccLibName;

            public int ccValue;

            [MarshalAs(UnmanagedType.LPStr)]
            public string ccMsg;

            [MarshalAs(UnmanagedType.LPStr)]
            public string additionalInfo;
        }

/*
 * The following data structure defines the tracing data for GCEV_TRACEDATA 
 */
        public const int GCVAL_TRACEDATA_SIZE = 2048; /* Maximum size of tracing data in memory buffer */

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GC_TRACEDATA
        {
            public uint seq_no; /* Sequence number  of tracing data*/
            public ushort data_size; /* Size of tracing data */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = GCVAL_TRACEDATA_SIZE)]
            public byte[] data_buf; /* Memory buffer for tracing data */
        }

/* The data structure defines for CAS transition, CAS pulse, and CAS Train to allow user to tune / retrieve the defintion of
 * CAS signals.  The sequence of data fields in these structures strictly follows CAS definition in CDP file.  
 * The ABCD code is a character string, terminated by '\0'. The unit of all time-related fields is millisecond.  Not all data fields are applied to DM3
*/

        public const int GCVAL_CAS_CODE_SIZE = 0x5; /* The "ABCD" code size of CAS Signal in character string */

/* Data structure for CAS Transition signal */

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GC_CASPROT_TRANS
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = GCVAL_CAS_CODE_SIZE)]
            public string PreTransCode; /* ABCD pre-transition code */
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = GCVAL_CAS_CODE_SIZE)]
            public string PostTransCode; /* ABCD post-transition code */

            public ushort PreTransInterval; /* The minimum time for the duration of the pre-transition (in msec)*/
            public ushort PostTransInterval; /* The minimum time for the duration of the post-transition (in msec)*/
            public ushort PreTransIntervalNom;
            /* The nominal time for the duration of the pre-transition (in msec). Ignored in DM3: always 0 */

            public ushort PostTransIntervalNom;
            /* The nominal time for the duration of the post-transition (in msec).Ignored in DM3: always 0 */
        }

/* Data structure of CAS Pulse signal */

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GC_CASPROT_PULSE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = GCVAL_CAS_CODE_SIZE)]
            public string OffPulseCode; /* ABCD pulse off code */
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = GCVAL_CAS_CODE_SIZE)]
            public string OnPulseCode; /* ABCD pulse on code */
            public ushort PrePulseInterval; /* The minimum time for the duration of the pre-pulse (in msec) */
            public ushort PostPulseInterval; /* The minimum time for the duration of the post-pulse (in msec) */
            public ushort PrePulseIntervalNom;
            /* The nominal time for the duration of the pre-pulse. Ignored in DM3: always 0 */

            public ushort PostPulseIntervalNom;
            /* The nominal time for the duration of the post-pulse (in msec). Ignored in DM3: always 0 */

            public ushort PulseIntervalMin; /* The minimum time for the duration of the pulse interval (in msec) */
            public ushort PulseIntervalNom; /* The nominal time for the duration of the pulse interval (in msec) */
            public ushort PulseIntervalMax; /* The maximum time for the duration of the pulse interval (in msec) */
        }

/* Data structure of CAS Train signal. */

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GC_CASPROT_TRAIN
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = GCVAL_CAS_CODE_SIZE)]
            public string OffPulseCode; /* ABCD pulse off code */
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = GCVAL_CAS_CODE_SIZE)]
            public string OnPulseCode; /* ABCD pulse on code */

            public ushort PreTrainInterval; /* The minimum time for the duration of the pre-train (in msec) */
            public ushort PostTrainInterval; /* The minimum time for the duration of the post-train (in msec) */
            public ushort PreTrainIntervalNom;
            /* The nominal time for the duration of the pre-train. Ignored in DM3: always 0 */

            public ushort PostTrainIntervalNom;
            /* The nominal time for the duration of the post-train (in msec). Ignored in DM3: always 0 */

            public ushort PulseIntervalMin; /* The minimum time for the duration of the pulse interval (in msec)*/
            public ushort PulseIntervalNom; /* The nominal time for the duration of the pulse interval (in msec)*/
            public ushort PulseIntervalMax; /* The maximum time for the duration of the pulse interval (in msec)*/
            public ushort InterPulseIntervalMin;
            /* The minimum time for the duration of inter-pulse interval (in msec)*/

            public ushort InterPulseIntervalNom;
            /* The nominal time for the duration of inter-pulse interval (in msec)*/

            public ushort InterPulseIntervalMax;
            /* The maximum time for the duration of inter-pulse interval (in msec) */
        }


/**************************************************************************/
/*                                  GC Values                             */
/**************************************************************************/


/* 
 * List of possible values for address_type parameter in the 
 * GCLIB_ADDRESS_BLK structure.
 */
        public const int GCADDRTYPE_TRANSPARENT = 1; /* Transparent Number*/
        public const int GCADDRTYPE_NAT = 2; /* National Number */
        public const int GCADDRTYPE_INTL = 3; /* International Number */
        public const int GCADDRTYPE_LOC = 4; /* Local Number */
        public const int GCADDRTYPE_IP = 5; /* Internet Protocol Address) */
        public const int GCADDRTYPE_URL = 6; /* URL Address */
        public const int GCADDRTYPE_DOMAIN = 7; /* Domain address) */
        public const int GCADDRTYPE_EMAIL = 8; /* Email Address */
        public const int GCADDRTYPE_NET_SPECIFIC = 9; /* Network specific address */
        public const int GCADDRTYPE_ABBREVIATED = 10; /* Abbreviated address */

/* 
 * List of possible values for address_plan field in the 
 * GCLIB_ADDRESS_BLK structure.
 */
        public const int GCADDRPLAN_UNKNOWN = 1; /* Address plan - Unknown */
        public const int GCADDRPLAN_ISDN = 2; /* Address plan - ISDN plan */
        public const int GCADDRPLAN_TELEPHONY = 3; /* Address plan - Telephony plan */
        public const int GCADDRPLAN_PRIVATE = 4; /* Address plan - Private plan */
        public const int GCADDRPLAN_DATA_X121 = 5; /* Address plan - X121 plan */
        public const int GCADDRPLAN_TELEX_F69 = 6; /* Address plan - Telex F69 plan */

/* 
 * List of possible values for sub_address_type field in the 
 * GCLIB_ADDRESS_BLK structure.
 */
        public const int GCSUBADDR_UNKNOWN = 1; /* Address type - Unknown type */
        public const int GCSUBADDR_OSI = 2; /* Address type - OSI type */
        public const int GCSUBADDR_USER = 3; /* Address type - User type */
        public const int GCSUBADDR_IA5 = 4; /* Address type - IA5 characters */

/* 
 * List of possible values for medium_sel field in the 
 * GCLIB_CHAN_BLK structure.
 */
        public const int GCMEDSEL_MEDIUM_PREF = 1; /* Timeslot selected is preferred */
        public const int GCMEDSEL_MEDIUM_EXCL = 2; /* Timeslot selected is exclusive */


/* 
 * List of possible values for category field in the 
 * GCLIB_CALL_BLK structure.
 */
        public const int GCCAT_SUB_NOPRIOR = 1; /* Subscriber without priority */
        public const int GCCAT_SUB_PRIOR = 2; /* Subscriber with priority */
        public const int GCCAT_MAINT_EQUIP = 3; /* Maintenance equipment */
        public const int GCCAT_COIN_BOX = 4; /* Coinbox or subscriber with chargemetering */
        public const int GCCAT_OPERATOR = 5; /* Operator */
        public const int GCCAT_DATA = 6; /* Data transmission */
        public const int GCCAT_CPTP = 7; /* C.P.T.P. */
        public const int GCCAT_SPECIAL = 8; /* Special Line */
        public const int GCCAT_MOBILE = 9; /* Mobile users */
        public const int GCCAT_VPN = 10; /* Virtual private network line */

/* 
 * List of possible values for address_info field in the 
 * GCLIB_CALL_BLK structure.
 */
        public const int GCADDRINFO_ENBLOC = 1; /* Address is complete */
        public const int GCADDRINFO_OVERLAP = 2; /* Address is not complete */

/* 
 * If an optional field in the makecall block is not set, then it must
 * be intialized to GCMKCALLBLK_DEFAULT.
 */
        public const int GCMKCALLBLK_DEFAULT = 0xFF; /* To initialize fields that are not used */


/* 
 * List of possible values of board open status
 */
        public const int GCBOARD_OPEN_IMPLICIT = 0;
        public const int GCBOARD_OPEN_EXPLICIT = 1;

/* 
 * List of possible query IDs
 */
        public const int GCQUERY_LD_NAME_TO_ID = 1; /* query line device ID from its name */
        public const int GCQUERY_BOARD_NAME_TO_STATUS = 2; /* query board status from its name */
        public const int GCQUERY_PROTOCOL_NAME_TO_ID = 3; /* query protocol name from its name */
        public const int GCQUERY_PARM_NAME_TO_ID = 4; /* query parm ID from its name */

/* 
 * Data structure for GC_PARM_ID
 */

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GC_PARM_ID
        {
            public ushort set_ID; /* Set ID */
            public ushort parm_ID; /* Parm ID */
            public byte value_type; /* Value type, its defines listed in gccfgparm.h */
            public byte update_perm; /* Update permission, its defines listed in gccfgparm.h */
        }

/* Pre-determined sizes of parm_data_buf, used when inserting parms.  When
 * using the insert_parm functions, initially the smallest size will be
 * created, and increased to next sizes as needed.  As 
 * one can see, the max size of any block is therefore 8192 bytes */
        public const int PARM_BLK_SMALL = 0x100; /* 256 bytes */
        public const int PARM_BLK_MEDIUM = 0x200; /* 512 bytes */
        public const int PARM_BLK_LARGE = 0x400; /* 1024 bytes */
        public const int PARM_BLK_HUGE = 0x2000; /* 8192 bytes */

/* Error codes for insert_parm */
        public const int SUCCESS = 0;
        public const int INVPARM = -1;
        public const int MAX_SIZE_REACHED = -2;
        public const int MEMORY_ERR = -3;

/* Defines to indicate type of resource device */
        public const int GC_NETWORKDEVICE = 1;
        public const int GC_VOICEDEVICE = 2;
        public const int GC_MEDIADEVICE = 3;
        public const int GC_NET_GCLINEDEVICE = 4;

/* Value defines of the rejection reason supplementary services (it is intended 
 * to select the same values as the result values for rejection event)
 */
        public const int GCVAL_REJREASON_UNSPECIFIED = 0x569; /* Not given */
        public const int GCVAL_REJREASON_UNAVAIL = 0x56A; /* The service is not available */
        public const int GCVAL_REJREASON_INVADDR = 0x56B; /* Invalid number or address */
        public const int GCVAL_REJREASON_NOTALLOWED = 0x56C; /* The service is not allowed */
        public const int GCVAL_REJREASON_INSUFFINFO = 0x56D; /* Insufficient information */
        public const int GCVAL_REJREASON_NOTSUBSCRIBED = 0x56E; /* User is not subscribed */


/*
 *  The following value must be ORed with the number of rings parameter in
 *  gc_AnswerCall() to trigger double answer signaling for R2 protocols
 *  that support it. Refer to the Global Call Country Dependent Parameters
 *  for PDK Protocol Configuration Guide for double answer support.
 */
        public const int GC_DBL_ANSWER = 0x100;

/*
 * GlobalCall Function Prototypes
 * Note: New functions will need to be addeded twice: once for
 * _MSC_VER ... & once for not
 */
//#ifndef GC_CDECL
//#ifdef _WIN32
//public int GC_CDECL	__cdecl
//#else
//public int GC_CDECL
//#endif
//#endif

//#if ( defined (_MSC_VER) || defined (__STDC__) || defined (__cplusplus) || defined (__BORLANDC__) || defined (CLIB) )

//#if defined (__cplusplus)
//extern "C" {
//#endif  /* __cplusplus */











//int GC_CDECL gc_AcceptCall(CRN crn, int rings, unsigned long mode);
//int GC_CDECL gc_AcceptModifyCall(CRN crn, GC_PARM_BLK *parmblkp, unsigned long mode);
//int GC_CDECL gc_AlarmName(METAEVENT *metaeventp ,char **alarm_name);
//int GC_CDECL gc_AlarmNumber(METAEVENT *metaeventp,long *alarm_number);
//int GC_CDECL gc_AlarmNumberToName(unsigned long aso_id, long alarm_number, char **name);
//int GC_CDECL gc_AlarmSourceObjectID(METAEVENT *metaeventp, unsigned long *aso_id);
//int GC_CDECL gc_AlarmSourceObjectIDToName(unsigned long aso_id, char **aso_name);
//int GC_CDECL gc_AlarmSourceObjectName(METAEVENT *metaeventp, char **aso_name);
//int GC_CDECL gc_AlarmSourceObjectNameToID(char *aso_name, unsigned long *aso_id);
//int GC_CDECL gc_AnswerCall(CRN crn, int rings, unsigned long mode);
//int GC_CDECL gc_Attach(LINEDEV linedev, int voiceh, unsigned long mode);
//int GC_CDECL gc_AttachResource(LINEDEV  linedev, int resourceh, GC_PARM_BLKP resourceattrp,
//                      GC_PARM_BLKP *retblkpp, int resourcetype, unsigned long mode);
//int GC_CDECL gc_BlindTransfer(CRN activecall, char *numberstr, GC_MAKECALL_BLK *makecallp,
//                     int timeout, unsigned long mode);                               
//int GC_CDECL gc_CallAck(CRN crn, GC_CALLACK_BLK *callack_blkp, unsigned long mode);
//int GC_CDECL gc_CallProgress(CRN crn, int indicator);
//int GC_CDECL gc_CCLibIDToName(int cclibid, char **lib_name);
//int GC_CDECL gc_CCLibNameToID(char *lib_name, int *cclibidp);  /* The lib_name must be NULL terminated */
//	                                                      /* and in uppercase e.g "GC_ICAPI_LIB" */
//int GC_CDECL gc_CCLibStatus(char *cclib_name, int *cclib_infop); /* The cclib_name must be NULL terminated */
//                                                        /* and in uppercase e.g "GC_ICAPI_LIB" */
//int GC_CDECL gc_CCLibStatusAll(GC_CCLIB_STATUS *cclib_status);

        /* The cclib_name must be NULL terminated */
        /* and in uppercase e.g "GC_ICAPI_LIB" */
        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_CCLibStatusEx(string cclib_name, ref GC_CCLIB_STATUSALL cclibstuff);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_Close(int chdev);

//int GC_CDECL gc_CompleteTransfer(CRN callonhold, CRN consultationcall, unsigned long mode);  
//int GC_CDECL gc_CRN2LineDev(CRN crn, LINEDEV *linedevp);
//int GC_CDECL gc_Detach(LINEDEV linedev, int voiceh, unsigned long mode);
//int GC_CDECL gc_DropCall(CRN crn, int cause, unsigned long mode);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_ErrorInfo(ref GC_INFO a_infop);


//int GC_CDECL gc_ErrorValue(int *gc_errorp, int *cclibidp, long *cclib_errorp);
//int GC_CDECL gc_GetAlarmConfiguration(LINEDEV linedev, unsigned long aso_id,
//                             ALARM_LIST *alarm_list, int alarm_config_type);
//int GC_CDECL gc_GetAlarmFlow(unsigned long aso_id, int *flow); 
//int GC_CDECL gc_GetAlarmParm(LINEDEV linedev, unsigned long aso_id, int ParmSetID,
//                    ALARM_PARM_LIST * alarm_parm_list, unsigned long mode);
//int GC_CDECL gc_GetAlarmSourceObjectList(LINEDEV linedev, int ByNameOrById,
//                                ALARM_SOURCE_OBJECT_LIST *aso_list);
//int GC_CDECL gc_GetAlarmSourceObjectNetworkID(LINEDEV linedev, unsigned long *aso_networkID);
//int GC_CDECL gc_GetANI(CRN crn, char * anibuf);
//int GC_CDECL gc_GetBilling(CRN crn, char *billing_buf);
//int GC_CDECL gc_GetCallInfo(CRN crn,int info_id, char *valuep);
//int GC_CDECL gc_GetCallProgressParm(LINEDEV linedev, DX_CAP *parmp);
//int GC_CDECL gc_GetCallState(CRN crn, int *state_ptr);
//int GC_CDECL gc_GetConfigData(int target_type, long target_id, GC_PARM_BLKP target_datap, int time_out, 
//                     long * request_idp, unsigned long mode);
//int GC_CDECL gc_GetCRN(CRN *crn_ptr, METAEVENT *metaeventp);
//int GC_CDECL gc_GetCTInfo(LINEDEV linedev, CT_DEVINFO *ct_devinfop);                         
//int GC_CDECL gc_GetDeviceNameInfo(char *DeviceName, GC_DEVICENAME_INFOP devicename_infop);
//int GC_CDECL gc_GetDlgerrValue( LINEDEV linedev, int *dlgerrp);
//int GC_CDECL gc_GetDNIS(CRN crn, char *dnis);
//int GC_CDECL gc_GetFrame(LINEDEV linedev, GC_L2_BLK *l2_blkp);
//int GC_CDECL gc_GetInfoElem(LINEDEV linedev, GC_IE_BLK *iep);
//int GC_CDECL gc_GetLineDev(LINEDEV *linedevp, METAEVENT *metaeventp);
//int GC_CDECL gc_GetLinedevState(LINEDEV linedev, int type, int *state_buf);
//int GC_CDECL gc_GetMetaEvent(METAEVENT *metaeventp);
//#ifdef _WIN32
//int GC_CDECL gc_GetMetaEventEx(METAEVENT *metaeventp, unsigned long evt_handle );
//#endif  /* _WIN32 */
//int GC_CDECL gc_GetNetworkH(LINEDEV linedev, int *networkhp);
//int GC_CDECL gc_GetParm(LINEDEV linedev, int parm_id, GC_PARM *valuep);

//int GC_CDECL gc_GetResourceH(LINEDEV linedev, int * resourcehp, int resourcetype);
        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_GetResourceH(int linedev, ref int resourcehp, int resourcetype);

        
//int GC_CDECL gc_GetSigInfo(LINEDEV linedev, char *valuep, int info_id, METAEVENT *metaeventp);
//int GC_CDECL gc_GetUsrAttr(LINEDEV linedev, void  **usr_attrp);
//int GC_CDECL gc_GetVer(LINEDEV linedev, unsigned int *releasenump,
//              unsigned int *intnump, long component);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_GetVoiceH(int linedevp, ref int voiceh);


//int GC_CDECL gc_GetXmitSlot(LINEDEV linedev, SC_TSINFO *sctsinfop);                          
//int GC_CDECL gc_HoldCall(CRN crn, unsigned long mode);
//int GC_CDECL gc_HoldACK(CRN crn);
//int GC_CDECL gc_HoldRej(CRN crn, int cause);
//int GC_CDECL gc_LibInit(void);
//int GC_CDECL gc_LinedevToCCLIBID(LINEDEV linedev, int *cclibid);
//int GC_CDECL gc_Listen(LINEDEV linedev, SC_TSINFO *sctsinfop, unsigned long mode);           
//int GC_CDECL gc_LoadDxParm(LINEDEV linedev, char *pathp, char *errmsgp, int err_length);
//int GC_CDECL gc_MakeCall(LINEDEV linedev, CRN *crnp, char *numberstr,
//                GC_MAKECALL_BLK *makecallp, int timeout, unsigned long mode);
//int GC_CDECL gc_GetNetCRV(CRN crn, int *netcrvp);
//int GC_CDECL gc_Open(LINEDEV *linedevp, char *devicename, int rfu);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_OpenEx(ref int linedevp, string devicename, int mode, [MarshalAs(UnmanagedType.AsAny)] Object usrattr);

//int GC_CDECL gc_QueryConfigData(int target_type, long target_id, GC_PARM * source_datap, 
//                       long query_id, GC_PARM * response_datap);
//int GC_CDECL gc_ReleaseCall(CRN crn);
//int GC_CDECL gc_ReleaseCallEx(CRN crn, unsigned long mode);
//int GC_CDECL gc_RejectModifyCall(CRN crn, unsigned long reason, unsigned long mode);
//int GC_CDECL gc_ReqANI(CRN crn, char *anibuf, int req_type, unsigned long mode);
//int GC_CDECL gc_ReqMoreInfo(CRN crn, int info_id, int info_len, int timeout, unsigned long mode);
//int GC_CDECL gc_ReqModifyCall(CRN crn, GC_PARM_BLK *parmblkp, unsigned long mode);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_ResetLineDev(int linedev, int flag);

//int GC_CDECL gc_ResultInfo(METAEVENT *a_Metaevent, GC_INFO *a_Info);
//int GC_CDECL gc_ResultMsg(int cclibid, long result_code, char **msg);
//int GC_CDECL gc_ResultValue(METAEVENT *metaeventp, int *gc_result, int *cclibidp,
//                   long *cclib_resultp);
//int GC_CDECL gc_RetrieveCall(CRN crn, unsigned long mode);
//int GC_CDECL gc_RetrieveAck(CRN crn);
//int GC_CDECL gc_RetrieveRej(CRN crn, int cause);
//int GC_CDECL gc_SendMoreInfo(CRN crn, int info_id, GC_PARM *info_ptr, unsigned long mode);
//int GC_CDECL gc_SetAlarmConfiguration(LINEDEV linedev, unsigned long aso_id,
//                             ALARM_LIST *alarm_list, int alarm_config_type);
//int GC_CDECL gc_SetAlarmFlow(unsigned long aso_id, int flow);
//int GC_CDECL gc_SetAlarmNotifyAll(LINEDEV linedev, unsigned long aso_id, int value);
//int GC_CDECL gc_SetAlarmParm(LINEDEV linedev, unsigned long aso_id, int ParmSetID,
//                    ALARM_PARM_LIST *alarm_parm_list, unsigned long mode);
//int GC_CDECL gc_SetAuthenticationInfo(int target_type, long target_id, GC_PARM_BLKP target_datap);
//
//int GC_CDECL gc_SetBilling(CRN crn,int rate_type, GC_RATE_U *ratep, unsigned long mode);
//int GC_CDECL gc_SetCallingNum(LINEDEV linedev, char *calling_num);
//int GC_CDECL gc_SetCallProgressParm(LINEDEV linedev, DX_CAP *parmp);
//int GC_CDECL gc_SetChanState(LINEDEV linedev, int chanstate, unsigned long mode);
//int GC_CDECL gc_SetConfigData(int target_type, long target_id, GC_PARM_BLKP target_datap,
//                     int time_out, int update_cond, long * request_idp, unsigned long mode);
//int GC_CDECL gc_SetDlgerrValue(LINEDEV linedev, int dlgerr);
//int GC_CDECL gc_SetEvtMsk(LINEDEV linedev, unsigned long mask, int action);
//int GC_CDECL gc_SetInfoElem(LINEDEV linedev, GC_IE_BLK *iep);
//int GC_CDECL gc_SetParm(LINEDEV linedev, int parm_id, GC_PARM value);
//int GC_CDECL gc_SetupTransfer(CRN calltohold, CRN *consultationcall, unsigned long mode); 
//int GC_CDECL gc_SetUsrAttr(LINEDEV linedev, void *usr_attr);
//int GC_CDECL gc_SipAck(CRN crn, GC_PARM_BLK *parmblkp, unsigned long mode);
//int GC_CDECL gc_SipPrack(CRN crn, GC_PARM_BLK *parmblkp, unsigned long mode);
//int GC_CDECL gc_SipPrackResponse(CRN crn, GC_PARM_BLK *parmblkp, unsigned long mode);
//int GC_CDECL gc_SndFrame(LINEDEV linedev, GC_L2_BLK* l2_blkp);
//int GC_CDECL gc_SndMsg(LINEDEV linedev, CRN crn, int msg_type, GC_IE_BLK *sndmsgptr);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_Start([MarshalAs(UnmanagedType.AsAny)] Object usrattr);

//int GC_CDECL gc_StartTrace(LINEDEV linedev, char *tracefilename);
        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_Stop();

//int GC_CDECL gc_StopTrace(LINEDEV linedev);
//int GC_CDECL gc_StopTransmitAlarms(LINEDEV linedev, unsigned long aso_id,
//                          ALARM_LIST *alarm_list, unsigned long mode);
//int GC_CDECL gc_SwapHold(CRN activecall, CRN callonhold, unsigned long mode);                
//int GC_CDECL gc_TransmitAlarms(LINEDEV linedev, unsigned long aso_id,
//                      ALARM_LIST *alarm_list, unsigned long mode);
//int GC_CDECL gc_UnListen(LINEDEV linedev, unsigned long mode);                               
//int GC_CDECL gc_WaitCall(LINEDEV linedev, CRN *crnp, GC_WAITCALL_BLK *waitcallp,
//                int timeout, unsigned long mode);
//int GC_CDECL gc_Extension(int target_type, long target_id, unsigned char ext_id, 
//                 GC_PARM_BLKP parmblkp, GC_PARM_BLKP *retblkp, unsigned long mode);
//int GC_CDECL gc_SetUserInfo(int target_type, long target_id, GC_PARM_BLKP infoparmblkp, int duration);
//int GC_CDECL gc_GetUserInfo (int target_type, long target_id, GC_PARM_BLKP *infoparmblkp);
//int GC_CDECL gc_ReqService(int target_type, long target_id, unsigned long *pserviceID,
//                    GC_PARM_BLKP reqdatap, GC_PARM_BLKP *respdatapp,
//                    unsigned long mode);
//int GC_CDECL gc_RespService(int target_type, long target_id, GC_PARM_BLKP datap, 
//                    unsigned long mode);
//int GC_CDECL gc_InvokeXfer(CRN crn, CRN extracrn, char *numberstr, 
//					GC_MAKECALL_BLK *makecallp, int timeout, unsigned long mode);
//int GC_CDECL gc_AcceptXfer(CRN crn, GC_PARM_BLK *parmblkp, unsigned long mode);
//int GC_CDECL gc_RejectXfer(CRN crn, unsigned long reason, GC_PARM_BLK *parmblkp, unsigned long mode);
//int GC_CDECL gc_InitXfer(CRN crn, GC_PARM_BLK *parmblkp, GC_REROUTING_INFO ** ret_rerouting_infopp, unsigned long mode);
//int GC_CDECL gc_AcceptInitXfer(CRN crn, GC_REROUTING_INFO *rerouting_infop, unsigned long mode);
//int GC_CDECL gc_RejectInitXfer(CRN crn, unsigned long reason, GC_PARM_BLK *parmblkp, unsigned long mode);
//
//* The following GC utility functions provide the convenience when using GC_PARM_BLK and
// GC_PARM_DATA */
//int GC_CDECL gc_util_insert_parm_ref(GC_PARM_BLKP *parm_blkpp, unsigned short setID,
//                            unsigned short parmID, unsigned char data_size,
//                            void *datap);
//int GC_CDECL gc_util_insert_parm_val(GC_PARM_BLKP *parm_blkpp, unsigned short setID,
//                            unsigned short parmID, unsigned char data_size,
//                            unsigned long data);
//GC_PARM_DATAP GC_CDECL gc_util_next_parm(GC_PARM_BLKP parm_blk, GC_PARM_DATAP cur_parm);
//GC_PARM_DATAP GC_CDECL  gc_util_find_parm(GC_PARM_BLKP parm_blk, unsigned short setID,
//                                unsigned short parmID);
//void GC_CDECL gc_util_delete_parm_blk(GC_PARM_BLKP parm_blk);
//int GC_CDECL gc_util_insert_parm_ref_ex(GC_PARM_BLKP *parm_blkpp, unsigned long setID,
//
//                                        unsigned long parmID, unsigned long data_size,
//
//                                        void *datap);
//
//int GC_CDECL gc_util_next_parm_ex(GC_PARM_BLKP parm_blk, GC_PARM_DATA_EXTP parm);
//
//int GC_CDECL gc_util_find_parm_ex(GC_PARM_BLKP parm_blk, unsigned long setID, 
//
//                                  unsigned long parmID, GC_PARM_DATA_EXTP parm);
//
//int GC_CDECL gc_util_copy_parm_blk(GC_PARM_BLKP *parm_blkpp, const GC_PARM_BLKP parm_blk);
//
//int GC_CDECL gc_SipSessionProgress(CRN crn, unsigned long mode);
//int GC_CDECL gc_CancelWaitCall(LINEDEV linedev, unsigned long mode);


    }
}
