using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

namespace ivrToolkit.Dialogic.Common.DialogicDefs
{
    public class gclib_h
    {
        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        //public static extern int gc_Start(ref GC_START_STRUCT startp);
        public static extern int gc_Start(IntPtr startp);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_Stop();

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_CCLibStatus(
            [MarshalAs(UnmanagedType.LPStr)] string cclib_name,
            ref int cclib_infop); /* The cclib_name must be NULL terminated and in uppercase e.g "GC_ICAPI_LIB" */

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_OpenEx(ref int linedevp, [MarshalAs(UnmanagedType.LPStr)] string devicename,
            int mode, IntPtr usrattr);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_Close(int linedev);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_ErrorInfo(IntPtr a_Info);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_util_insert_parm_val(ref IntPtr parm_blkpp, ushort setID, ushort parmID,
            byte data_size, uint data);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void gc_util_delete_parm_blk(IntPtr parm_blk);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_SetConfigData(int target_type, int target_id, IntPtr target_datap,
            int time_out, int update_cond, ref int request_idp, uint mode);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_SetUsrAttr(int linedev, IntPtr usr_attr);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_util_insert_parm_ref(ref IntPtr parm_blkpp, ushort setID, ushort parmID,
            byte data_size, IntPtr datap);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_SetAuthenticationInfo(int target_type, int target_id, IntPtr target_datap);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_ReqService(int target_type, int target_id, ref uint pserviceID,
            IntPtr reqdatap, ref IntPtr respdatapp,
            uint mode);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_DropCall(int crn, int cause, uint mode);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_WaitCall(int linedev, IntPtr crnp, IntPtr waitcallp, int timeout, uint mode);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        //public static extern int gc_GetMetaEventEx(ref METAEVENT metaeventp, uint evt_handle);
        public static extern int gc_GetMetaEventEx(ref METAEVENT metaeventp, int evt_handle);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_GetCRN(ref int crn_ptr, ref METAEVENT metaeventp);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_SetUserInfo(int target_type, int target_id, IntPtr infoparmblkp, int duration);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_GetResourceH(int linedev, ref int resourcehp, int resourcetype);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_GetXmitSlot(int linedev, ref SC_TSINFO sctsinfop);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_Listen(int linedev, ref SC_TSINFO sctsinfop, uint mode);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_CallAck(int crn, ref GC_CALLACK_BLK callack_blkp, uint mode);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_AcceptCall(int crn, int rings, uint mode);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_AnswerCall(int crn, int rings, uint mode);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_GetCallState(int crn, ref int state_ptr);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_ResultInfo(ref METAEVENT a_Metaevent, IntPtr a_Info);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_ReleaseCallEx(int crn, uint mode);

        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gc_Extension(int target_type, int target_id, byte ext_id,
            IntPtr parmblkp, ref IntPtr retblkp, uint mode);



        /* -- bit mask for gc_GetCCLibInfo */
        public const int GC_CCLIB_AVL = 0x1;
        public const int GC_CCLIB_CONFIGURED = 0x2;
        public const int GC_CCLIB_FAILED = 0x4;
        public const int GC_CCLIB_STUB = 0x8;
        public const int GC_CCLIB_AVAILABLE = GC_CCLIB_AVL;


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
 * Defines for GlobalCall API event codes
 */
        public const int DT_GC = 0x800;

/*gc4*/
        public const int GCEV_TASKFAIL = DT_GC | 0x01; /* Abnormal condition; state unchanged */
        public const int GCEV_ANSWERED = DT_GC | 0x02; /* Call answered and connected */
        public const int GCEV_CALLPROGRESS = DT_GC | 0x03;
        public const int GCEV_ACCEPT = DT_GC | 0x04; /* Call is accepted */
        public const int GCEV_DROPCALL = DT_GC | 0x05; /* gc_DropCall is completed */
        public const int GCEV_RESETLINEDEV = DT_GC | 0x06; /* Restart event */
        public const int GCEV_CALLINFO = DT_GC | 0x07; /* Info message received */
        public const int GCEV_REQANI = DT_GC | 0x08; /* gc_ReqANI() is completed */
        public const int GCEV_SETCHANSTATE = DT_GC | 0x09; /* gc_SetChanState() is completed */
        public const int GCEV_FACILITY_ACK = DT_GC | 0x0A;
        public const int GCEV_FACILITY_REJ = DT_GC | 0x0B;
        public const int GCEV_MOREDIGITS = DT_GC | 0x0C; /* cc_moredigits() is completed*/
        public const int GCEV_SETBILLING = DT_GC | 0x0E; /* gc_SetBilling() is completed */
        public const int GCEV_ATTACH = DT_GC | 0x0f; /* media device successfully attached */
        public const int GCEV_ATTACH_FAIL = DT_GC | 0x10; /* failed to attach media device */
        public const int GCEV_DETACH = DT_GC | 0x11; /* media device successfully detached */
        public const int GCEV_DETACH_FAIL = DT_GC | 0x12; /* failed to detach media device */
        public const int GCEV_MEDIA_REQ = DT_GC | 0x13; /* Remote end is requesting media channel */
        public const int GCEV_STOPMEDIA_REQ = DT_GC | 0x14; /* Remote end is requesting media streaming stop */
        public const int GCEV_MEDIA_ACCEPT = DT_GC | 0x15; /* Media channel established with peer */
        public const int GCEV_MEDIA_REJ = DT_GC | 0x16; /* Failed to established media channel with peer*/
        public const int GCEV_OPENEX = DT_GC | 0x17; /* Device Opened successfully */
        public const int GCEV_OPENEX_FAIL = DT_GC | 0x18; /* Device failed to Open */
        public const int GCEV_TRACEDATA = DT_GC | 0x19; /* Tracing data */

        public const int GCEV_ALERTING = DT_GC | 0x21; /* The destination telephone terminal
                                            * equipment has received connection
                                            * request(in ISDN accepted the
                                            * connection request.This event is
                                            * an unsolicited event
                                            */

        public const int GCEV_CONNECTED = DT_GC | 0x22; /* Destination answered the request */
        public const int GCEV_ERROR = DT_GC | 0x23; /* unexpected error event */
        public const int GCEV_OFFERED = DT_GC | 0x24; /* A connection request has been made */
        public const int GCEV_DISCONNECTED = DT_GC | 0x26; /* Remote end disconnected */

        public const int GCEV_PROCEEDING = DT_GC | 0x27; /* The call state has been changed to
                                            * the proceeding state */

        public const int GCEV_PROGRESSING = DT_GC | 0x28; /* A call progress message has been
                                            * received */

        public const int GCEV_USRINFO = DT_GC | 0x29; /* A user to user information event is
                                            * coming */

        public const int GCEV_FACILITY = DT_GC | 0x2A; /* Network facility indication */

        public const int GCEV_CONGESTION = DT_GC | 0x2B; /* Remote end is not ready to accept
                                            * incoming user information */

        public const int GCEV_D_CHAN_STATUS = DT_GC | 0x2E; /* Report D-channel status to the user */

        public const int GCEV_NOUSRINFOBUF = DT_GC | 0x30; /* User information element buffer is
                                            * not ready */

        public const int GCEV_NOFACILITYBUF = DT_GC | 0x31; /* Facility buffer is not ready */
        public const int GCEV_BLOCKED = DT_GC | 0x32; /* Line device is blocked */
        public const int GCEV_UNBLOCKED = DT_GC | 0x33; /* Line device is no longer blocked */
        public const int GCEV_ISDNMSG = DT_GC | 0x34;
        public const int GCEV_NOTIFY = DT_GC | 0x35; /* Notify message received */
        public const int GCEV_L2FRAME = DT_GC | 0x36;
        public const int GCEV_L2BFFRFULL = DT_GC | 0x37;
        public const int GCEV_L2NOBFFR = DT_GC | 0x38;
        public const int GCEV_SETUP_ACK = DT_GC | 0x39;

        public const int GCEV_REQMOREINFO = GCEV_SETUP_ACK; /* Received request for more information from network */

        /* This is a replacement of GCEV_SETUP_ACK event */
        public const int GCEV_CALLSTATUS = DT_GC | 0x3A; /* call status, e.g. busy */
        public const int GCEV_MEDIADETECTED = DT_GC | 0x3B; /* Media detection completed */

        /*gc5*/
        /* these events only apply to those sites using ISDN DPNSS */
        public const int GCEV_DIVERTED = DT_GC | 0x40;
        public const int GCEV_HOLDACK = DT_GC | 0x41;
        public const int GCEV_HOLDCALL = DT_GC | 0x42;
        public const int GCEV_HOLDREJ = DT_GC | 0x43;
        public const int GCEV_RETRIEVEACK = DT_GC | 0x44;
        public const int GCEV_RETRIEVECALL = DT_GC | 0x45;
        public const int GCEV_RETRIEVEREJ = DT_GC | 0x46;
        public const int GCEV_NSI = DT_GC | 0x47;
        public const int GCEV_TRANSFERACK = DT_GC | 0x48;
        public const int GCEV_TRANSFERREJ = DT_GC | 0x49;
        public const int GCEV_TRANSIT = DT_GC | 0x4A;
        public const int GCEV_RESTARTFAIL = DT_GC | 0x4B;

        /* end of ISDN DPNSS specific */

        public const int GCEV_ACKCALL = DT_GC | 0x50; /* Termination event for gc_CallACK() */
        public const int GCEV_SETUPTRANSFER = DT_GC | 0x51; /* Ready for making consultation call */
        public const int GCEV_COMPLETETRANSFER = DT_GC | 0x52; /* Transfer completed successfully */
        public const int GCEV_SWAPHOLD = DT_GC | 0x53; /* Call on hold swapped with active call */
        public const int GCEV_BLINDTRANSFER = DT_GC | 0x54; /* Call transferred to consultation call */
        public const int GCEV_LISTEN = DT_GC | 0x55; /* Channel (listen) connected to SCbus timeslot */
        public const int GCEV_UNLISTEN = DT_GC | 0x56; /* Channel (listen) disconnected from SCbus timeslot */
        public const int GCEV_DETECTED = DT_GC | 0x57; /* Incoming call detected */
        public const int GCEV_FATALERROR = DT_GC | 0x58; /* Fatal error has occurred, see result value */
        public const int GCEV_RELEASECALL = DT_GC | 0x59; /* Call Released */
        public const int GCEV_RELEASECALL_FAIL = DT_GC | 0x5A; /* Failed to Release call*/

        public const int GCEV_DIALTONE = DT_GC | 0x60; /* Call has transitioned to GCST_DialTone state */
        public const int GCEV_DIALING = DT_GC | 0x61; /* Call has transitioned to GCST_Dialing state */

        public const int GCEV_ALARM = DT_GC | 0x62; /* An alarm occurred */

        /* NOTE: this alarm is disabled by default */
        public const int GCEV_MOREINFO = DT_GC | 0x63; /* Status of information requested\received */
        public const int GCEV_SENDMOREINFO = DT_GC | 0x65; /* More information sent to the network */

        public const int GCEV_CALLPROC = DT_GC | 0x66; /* Call acknowledged to indicate that the call is */

        /* now proceeding */
        public const int GCEV_NODYNMEM = DT_GC | 0x67; /* No dynamic memory available */
        public const int GCEV_EXTENSION = DT_GC | 0x68; /* Unsolicited extension event */
        public const int GCEV_EXTENSIONCMPLT = DT_GC | 0x69; /* Termination event for gc_Extension() */

        public const int GCEV_GETCONFIGDATA = DT_GC | 0x6A; /* Configuration data successfully */

        /* retrieved */
        public const int GCEV_GETCONFIGDATA_FAIL = DT_GC | 0x6B; /* Failed to get (retrieve) the */

        /* configuration data */
        public const int GCEV_SETCONFIGDATA = DT_GC | 0x6C; /* The configuration data successfully */

        /* updated (set) */
        public const int GCEV_SETCONFIGDATA_FAIL = DT_GC | 0x6D; /* Failed to set (update) the */

        /* configuration data   */
        public const int GCEV_SERVICEREQ = DT_GC | 0x6E; /* Service Request received */
        public const int GCEV_SERVICERESP = DT_GC | 0x70; /* Service Response received */
        public const int GCEV_SERVICERESPCMPLT = DT_GC | 0x71; /* Service Response sent */

        public const int GCEV_INVOKE_XFER_ACCEPTED = DT_GC | 0x72; /* Invoke transfer accepted by the remote party */
        public const int GCEV_INVOKE_XFER_REJ = DT_GC | 0x73; /* Invoke transfer rejected by the remote party */
        public const int GCEV_INVOKE_XFER = DT_GC | 0x74; /* Successful completion of invoke transfer */
        public const int GCEV_INVOKE_XFER_FAIL = DT_GC | 0x75; /* Failure in invoke transfer */
        public const int GCEV_REQ_XFER = DT_GC | 0x76; /* Receiving a call transfer request */

        public const int
            GCEV_ACCEPT_XFER = DT_GC | 0x77; /* Successfully accept the transfer request from remote party*/

        public const int
            GCEV_ACCEPT_XFER_FAIL = DT_GC | 0x78; /* Failure to accept the transfer request from remote party*/

        public const int GCEV_REJ_XFER = DT_GC | 0x79; /* Successfully reject the transfer request from remote party */
        public const int GCEV_REJ_XFER_FAIL = DT_GC | 0x7A; /* Failure to reject the transfer request */

        public const int
            GCEV_XFER_CMPLT =
                DT_GC | 0x7B; /* Successful completion of call transfer at the party receiving the request */

        public const int GCEV_XFER_FAIL = DT_GC | 0x7C; /* Failure to reroute a transferred call  */
        public const int GCEV_INIT_XFER = DT_GC | 0x7D; /* Successful completion of transfer initiate */
        public const int GCEV_INIT_XFER_REJ = DT_GC | 0x7E; /* Transfer initiate rejected by the remote party */
        public const int GCEV_INIT_XFER_FAIL = DT_GC | 0x7F; /* Failure in transfer initiate */
        public const int GCEV_REQ_INIT_XFER = DT_GC | 0x80; /* Receiving a transfer initiate request */
        public const int GCEV_ACCEPT_INIT_XFER = DT_GC | 0x81; /* Successfully accept the transfer initiate request */

        public const int
            GCEV_ACCEPT_INIT_XFER_FAIL = DT_GC | 0x82; /* Failure to accept the transfer initiate request */

        public const int GCEV_REJ_INIT_XFER = DT_GC | 0x83; /* Successfully reject the transfer initiate request */
        public const int GCEV_REJ_INIT_XFER_FAIL = DT_GC | 0x84; /* Failure to reject the transfer initiate request  */
        public const int GCEV_TIMEOUT = DT_GC | 0x85; /* Notification of generic time out  */
        public const int GCEV_REQ_MODIFY_CALL = DT_GC | 0x86; /* received Modify request from remote party */
        public const int GCEV_REQ_MODIFY_CALL_UNSUPPORTED = DT_GC | 0x87; /* unsupported inbound capability */
        public const int GCEV_MODIFY_CALL_ACK = DT_GC | 0x88; /* acknowledge by remote party of accepting Modify */
        public const int GCEV_MODIFY_CALL_REJ = DT_GC | 0x89; /* Rejection by remote party of Modify request */
        public const int GCEV_MODIFY_CALL_FAIL = DT_GC | 0x8a; /* failure to send Modify request */

        public const int GCEV_MODIFY_CALL_CANCEL = DT_GC | 0x8b; /* receipt of previous Modify cancellation request */

        /* from remote party */
        public const int GCEV_CANCEL_MODIFY_CALL = DT_GC | 0x8c; /* successful cancellation of previous Modify */
        public const int GCEV_CANCEL_MODIFY_CALL_FAIL = DT_GC | 0x8d; /* failure to cancel previous Modify */
        public const int GCEV_ACCEPT_MODIFY_CALL = DT_GC | 0x8e; /* success in Modify request by remote end */
        public const int GCEV_ACCEPT_MODIFY_CALL_FAIL = DT_GC | 0x8f; /* failure in Modify request by remote end */
        public const int GCEV_REJECT_MODIFY_CALL = DT_GC | 0x90; /* rejection to Modify request by remote end */
        public const int GCEV_REJECT_MODIFY_CALL_FAIL = DT_GC | 0x91; /* failure to Modify request by remote end */

        public const int GCEV_SIP_ACK = DT_GC | 0x92; /* Receipt of ACK in responce to 200 OK on Invite transaction */
        public const int GCEV_SIP_ACK_OK = DT_GC | 0x93; /* Sip ACK successfully sent.      */
        public const int GCEV_SIP_ACK_FAIL = DT_GC | 0x94; /* Attempt to send Sip ACK failed. */
        public const int GCEV_SIP_200OK = DT_GC | 0x95; /* A 200 OK was received on an Invite transaction. */

        public const int GCEV_SIP_PRACK = DT_GC | 0x96; /* Sip PRACK received. */
        public const int GCEV_SIP_PRACK_RESPONSE = DT_GC | 0x97; /* Sip PRACK Response received. */
        public const int GCEV_SIP_PRACK_OK = DT_GC | 0x98; /* Sip PRACK request sent success. */
        public const int GCEV_SIP_PRACK_FAIL = DT_GC | 0x99; /* Sip PRACK request send failure. */
        public const int GCEV_SIP_PRACK_RESPONSE_OK = DT_GC | 0x9a; /* Sip PRACK Response request send success. */
        public const int GCEV_SIP_PRACK_RESPONSE_FAIL = DT_GC | 0x9b; /* Sip PRACK Response request send failure. */

        public const int GCEV_SIP_SESSION_EXPIRES = DT_GC | 0x9c; /* SIP session timer expires */

        public const int GCEV_SIP_SESSIONPROGRESS = DT_GC | 0x9d; /* Sip Session Progress sent success. */

        public const int GCEV_TELEPHONY_EVENT = DT_GC | 0x9e; /* Unsolicited telephony events */

        public const int GCEV_CALLUPDATE = DT_GC | 0x9f; /* Sip UPDATE received */

        public const int GCEV_CANCELWAITCALL = DT_GC | 0xA0; /* WaitCall Cancelled */

        public const int GCEV_FACILITYREQ = DT_GC | 0xFF; /* A facility request is made by CO */

        /* define(s) for flags field within METAEVENT structure */
        public const int GCME_GC_EVENT = 0x1; /* Event is a GlobalCall event */

        /* duration field values for gc_SetUserInfo( ) */
        public const int GC_SINGLECALL = 0;
        public const int GC_ALLCALLS = 1;
        public const int GC_NEXT_OUTBOUND_MSG = 2;
        public const int GC_SINGLE_SIP_SESSION = 3;

        /* Defines to indicate type of resource device */
        public const int GC_NETWORKDEVICE = 1;
        public const int GC_VOICEDEVICE = 2;
        public const int GC_MEDIADEVICE = 3;
        public const int GC_NET_GCLINEDEVICE = 4;

        /* define(s) for type field within GC_CALLACK_BLK structure */
        public const int GCACK_SERVICE_DNIS = 0x1;
        public const int GCACK_SERVICE_ISDN = 0x2;
        public const int GCACK_SERVICE_PROC = 0x3;
        public const int GCACK_SERVICE_INFO = 0x4;

        public const int GCPV_ENABLE = 1;      /* enable feature */
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
        public const int GCSUPP_CALLSTBASE = 0x40000000;				/* Call state base for GC supplementary services */
        public const int GCST_INVOKE_XFER_ACCEPTED = GCSUPP_CALLSTBASE | 0x1;
        public const int GCST_INVOKE_XFER = GCSUPP_CALLSTBASE | 0x2;
        public const int GCST_REQ_XFER = GCSUPP_CALLSTBASE | 0x4;
        public const int GCST_ACCEPT_XFER = GCSUPP_CALLSTBASE | 0x8;
        public const int GCST_XFER_CMPLT = GCSUPP_CALLSTBASE | 0x10;
        public const int GCST_REQ_INIT_XFER = GCSUPP_CALLSTBASE | 0x20;
        public const int GCST_HOLD = GCSUPP_CALLSTBASE | 0x40;
        public const int GCST_HELD = GCSUPP_CALLSTBASE | 0x80;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct METAEVENT
    {
        /*
        -- Note: structure is ordered with longest fields 1st
        -- to improve access time with some compilers
        */
        public int magicno; /* for internal validity check */

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
        public int linedev; /* linedevice */
        public int crn; /* crn - if 0 then no crn for this event */
        public IntPtr extevtdatap; /* pointer to abstract data buffer */
        public IntPtr usrattr; /* user attribute */

        public int cclibid; /* ID of CCLib that event is associated with */

        /* + = CCLib ID number */
        /* -1 = unknown */
        public int rfu1; /* for future use only */

    }



    /*
     * The following data structure defines the error or result information 
     */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct GC_INFO
    {
        public int gcValue;
        [MarshalAs(UnmanagedType.LPStr)] public string gcMsg;
        public int ccLibId;
        [MarshalAs(UnmanagedType.LPStr)] public string ccLibName;
        public int ccValue; // todo was long
        [MarshalAs(UnmanagedType.LPStr)] public string ccMsg;
        [MarshalAs(UnmanagedType.LPStr)] public string additionalInfo;
    }

    /* 
        GC_PARM_DATA data structure used in GC_PARM_BLK
    */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct GC_PARM_DATA
    {
        public ushort set_ID; /* Set ID (two bytes long)*/
        public ushort parm_ID; /* Parameter ID (two bytes long) */
        public byte value_size; /* Size of value_buf in bytes */
        public byte value_buf; /* Address to the parm value buffer */
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct GCLIB_WAITCALL_BLK
    {
        public int flags;
        public int rfu;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct GC_WAITCALL_BLK
    {
        public IntPtr gclib; /* GlobalCall specific portion */
        public IntPtr cclib; /* cclib specific portion */
    }

    /*
    * CCLib specific start structure, used by GC_START_STRUCT
    * Libraries should find its own structure by looking at the name field in the
    * array pointed to by GC_START_STRUCT
    */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CCLIB_START_STRUCT
    {
        /// char*
        [MarshalAs(UnmanagedType.LPStr)] public string cclib_name; /* Must match CCLib name in gcprod */

        public IntPtr cclib_data;
    }


    /*
     * GC start structure, points to an array of CCLIB_START_STRUCT
     */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct GC_START_STRUCT
    {
        public int num_cclibs;
        public IntPtr cclib_list;
    }

    /* 
        Generic GC_PARM_BLK data structure
    */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct GC_PARM_BLK
    {
        public ushort parm_data_size; /* Size of parm_data_buf in bytes */
        public byte byte1;
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DNIS{
        public int accept;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ISDN
    {
        public int acceptance;

        /* 0x0000 proceding with the same B chan */
        /* 0x0001 proceding with the new B chan */
        /* 0x0002 setup ACK */
        public int linedev;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct INFO
    {
        public int info_len;
        public int info_type;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct GC_PRIVATE {
        public int gc_private1;
        public int gc_private2;
        public int gc_private3;
        public int gc_private4;
    }

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi, Pack = 1)]
    public struct GC_CALLACK_BLK
    {
        [FieldOffset(0)]
        public uint type; /* type of a structure inside following union */
        [FieldOffset(4)]
        public int rfu; /* will be used for common functionality */

        //    union {
        [FieldOffset(8)]
        public DNIS dnis;
        [FieldOffset(8)]
        public ISDN isdn;
        [FieldOffset(8)]
        public INFO info;
        [FieldOffset(8)]
        public GC_PRIVATE gc_private;
    }

    /* IP_AUDIO_CAPABILITY : This structure is used to allow some minimum set of information
     * to be exchanged along with the audio codec identifier.
     * 
     * frames_per_pkt : When bundling more than one audio frame into a single transport packet, 
     * this value should represent the maximum number of frames per packet which will 
     * be sent on the wire. If set to zero, it indicates either that the exact number 
     * of frames per packet is not known which means it could be anything or it indicates
     * that this data is not applicable.
     *
     * VAD : For audio algorithms that support the concept of voice activity detection (VAD, 
     * also known as silence suppression), this value will be either GCPV_ENABLE or 
     * GCPV_DISABLE.  This parameter is ignored.for algorithms which do not support VAD. 
     * See product documentation for details.
     */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct IP_AUDIO_CAPABILITY
    {
        public uint frames_per_pkt;
        public ushort VAD;
    }

    /* IP_VIDEO_CAPABILITY | This structure is used to allow some minimum set of information
     * to be exchanged along with the video codec identifier.
     *
     * mean_pict_intrvl : This field is used to indicate the maximum frame rate for 
     * the video stream. The frame rate maps to MPI through the following formula:  
     * fps = 29.97/MPI.
     */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct IP_VIDEO_CAPABILITY
    {
        public uint mean_pict_intrvl;
    }

    /* IP_DATA_CAPABILITY | This structure is used to allow some minimum set of information
     * to be exchanged along with the data capability.
     *
     * max_bit_rate : This field is used to indicate the maximum bit rate which should be 
     * used for the data channel.  The bit rate/ should be specified in 100's of bit/sec.
     */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct IP_DATA_CAPABILITY
    {
        public uint max_bit_rate;
    }


    /* IP_CAPABILITY_UNION | This union simply allows a way for the different capability
     * categories to define their own additional parameters of interest.
     *
     * audio : This is a structure which represents audio capability
     *
     * video : This is a structure which represents video capability.
     *
     * data : This is a structure which represents data capability.
     */
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi, Pack = 1)]
    public struct IP_CAPABILITY_UNION
    {
        [FieldOffset(0)]
        public IP_AUDIO_CAPABILITY audio;
        [FieldOffset(0)]
        public IP_VIDEO_CAPABILITY video;
        [FieldOffset(0)]
        public IP_DATA_CAPABILITY data;
    }

    /* IP_CAPABILITY | This structure is intended to be a very simplified capability 
     * representation. Obviously it will not contain all the flexibility of the H.245 
     * terminal capability structure, but hopefully it will provide what might be 
     * classified as the first level of useful information beyond simply the capability 
     * or codec identifier.
     *
     * capability : This is the media capability for this structure.
     *
     * type : This is the capability category of capability specified in this structure.  
     * It indicates which member of the union is being used.
     *
     * direction : This is the direction for this capability. 
     *
     * payload_type : This field only applies to dynamic payload types.  
     *                It is ignored for well-known static payload type transcoding.
     *
     * extra : The contents of this IP_CAPABILITY_UNION will be indicated by the 
     * type field. 
     * 
     * rfu   : Reserved for future use. Must be set to zero when not used.
     *
     *
     */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct IP_CAPABILITY
    {
        public int capability;
        public int type;
        public int direction;
        public int payload_type;
        public IP_CAPABILITY_UNION extra;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] rfu;
    }
    /* IP_CONNECT | Used in gc_SetUserInfo() for set media device connection
     * in T38 fax server application.The data structure should support necessary
     * fields required by dev_connect() and dev_disconnect() functions.
     *
     * version :      version of this structure. currently supported is 0x100
     * mediaHandle:   media device handle required by dev_connect and dev_disconnect
     * faxHandle:     fax device handle requried by dev_connect
     * connectType:   connection type, full or half duplex defined eIPConnecType_e
     * 
     */

    public enum eIPConnectType_e
    {
        IP_FULLDUP = 1,
        IP_HALFDUP
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct IP_CONNECT
    {
        public ushort version;
        public int mediaHandle;
        public int faxHandle;
        public eIPConnectType_e connectType;
    }

}
