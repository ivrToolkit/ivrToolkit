// ReSharper disable InconsistentNaming

namespace ivrToolkit.Plugin.Dialogic.Common.DialogicDefs;

public class gcip_defs_h
{
    /* Defines for IP_ADDR and IP_ADDRP struct to identify version as IPv4 or IPv6: */
    public const int IPVER4 = 0; /* IPv4 address */
    public const int IPVER6 = 1; /* IPv6 address */
    public const uint IP_CFG_DEFAULT = uint.MaxValue; /* used to initialize IPCCLIB_START_DATA struct via memset indicate default values. */

    /* Defines for specifying Supplementary Service support in sup_serv_mask bitmask within IP_VIRTBOARD  */
    public const uint IP_SUP_SERV_DISABLED = 0x00; /* All Supplementary Services disabled (default) */
    public const uint IP_SUP_SERV_CALL_XFER = 0x01; /* Call Transfer enabled (blind and supervised)  */

    /* Defines for exposing additional message information support in 'sip_msginfo_mask' bitmask within IP_VIRTBOARD
     * (Defaults to all disabled...) */
    /* Mask to activate SIP standard headers */
    public const uint IP_SIP_MSGINFO_ENABLE = 0x01;
    /* Mask to activate SIP-T/MIME feature */
    public const uint IP_SIP_MIME_ENABLE = 0x02;

    /* Macros to set the terminal_type in the IP_VIRTBOARD data structure */
    public const ushort IP_TT_TERMINAL = 50;
    public const ushort IP_TT_GATEWAY = 60;

    public const string DEV_CLASS_IPT = "IPT";

    /******************************************************************
     *    Set and Parameter IDs                                       *
     ******************************************************************/
    public const int BASE_SETID = gccfgparm_h.GC_IP_TECH_SET_MIN; /* 0x3000 */

    /* Set Id for setting and getting parameter values
     * IPSET_CONFIG | This set Id is used for configuring general parameters.
     */
    public const int IPSET_CONFIG = BASE_SETID + 25;

    /* Set Id to for enabling or disabling of GCEV_EXTENSION unsolicited notification events.
     * To be used with parm ids GCPARM_GET_MSK, GCACT_SETMSK, GCACT_ADDMSK, and GCACT_SUBMSK and
     * associated bitmask value of size GC_VALUE_LONG (4 octets)
     */
    public const int IPSET_EXTENSIONEVT_MSK = BASE_SETID + 23;

    /* IPPARM_OPERATING_MODE | Operating mode for device can be automatic mode(default)
     * or manual mode. manual mode is required for T38 fax server
     */
    public const int IPPARM_OPERATING_MODE = 0x02;
    public const int IP_T38_AUTOMATIC_MODE = 0x01;
    public const int IP_T38_MANUAL_MODE = 0x02;                    /* switch thru GCEV_EXTENSION event */
    public const int IP_AUTOMATIC_MODE = IP_T38_AUTOMATIC_MODE;   /* old defines for compatibility */
    public const int IP_MANUAL_MODE = IP_T38_MANUAL_MODE;      /* old defines for compatibility */
    public const int IP_T38_MANUAL_MODIFY_MODE = 0x04; /* switch thru GCEV_REQ_MODIFY_CALL event */

    /*
     * bitmask settings for parm IDs GCPARM_GET_MSK, GCACT_SETMSK, GCACT_ADDMSK, and GCACT_SUBMSK
     */
    public const int EXTENSIONEVT_DTMF_USERINPUT_SIGNAL = 0x01;
    public const int EXTENSIONEVT_DTMF_RFC2833 = 0x02;
    public const int EXTENSIONEVT_DTMF_ALPHANUMERIC = 0x04;
    public const int EXTENSIONEVT_SIGNALING_STATUS = 0x08;
    public const int EXTENSIONEVT_STREAMING_STATUS = 0x10;
    public const int EXTENSIONEVT_T38_STATUS = 0x20;
    public const int EXTENSIONEVT_CALL_PROGRESS = 0x40;
    public const int EXTENSIONEVT_SIP_18X_RESPONSE = 0x80; /* FR5311 */

    /* Parameter value for specifying registration service in parameter
     * GCSET_SERVREQ:PARM_REQTYPE in functions gc_ReqService or gc_RespService
     */
    public const int IP_REQTYPE_REGISTRATION = 0x01;

    /* IPPARM_AUTHENTICATION_CONFIGURE | This parameter is used to add or
     * modify authentication configuration.
     * IPPARM_AUTHENTICATION_REMOVE | This parameter is used to remove
     * authentication configuration.
     */
    public const int IPPARM_AUTHENTICATION_CONFIGURE = 0x05;
    public const int IPPARM_AUTHENTICATION_REMOVE = 0x06;

    public const int IPPARM_IPMPARM = 0x07;

    /* Set Id to identify protocol in scenarios which support multiple protocols.
       The scope of this set and parameter ID pair apply to all subsequent parameters,
       i.e., GC_PARM_DATA structures, in the GC_PARM_BLK.
     */
    public const int IPSET_PROTOCOL = BASE_SETID + 24;
    /*
     * IPPARM_PROTOCOL_BITMASK |  Bitmask specifying intended protocol for registration information.
     */
    public const int IPPARM_PROTOCOL_BITMASK = 0x01;

    public const int IP_PROTOCOL_SIP = 0x01;
    public const int IP_PROTOCOL_H323 = 0x02;

    /* Set Id for the registration operation with a gatekeep (H.323) or registrar (SIP).
     */
    public const int IPSET_REG_INFO = BASE_SETID + 22;

    /* parm IDs defined under IPSET_REG_INFO identifying the registration operation
     *
     * IPPARM_OPERATION_REGISTER |  Register with the gatekeeper/registrar using the method
     *                              identified by the value field.
     *
     * IPPARM_OPERATION_DEREGISTER |  Deregister with the gatekeeper/registrar using the method
     *                                identified by the value field.
     */
    public const int IPPARM_OPERATION_REGISTER = 0x01;
    public const int IPPARM_OPERATION_DEREGISTER = 0x02;

    public const int IP_REG_SET_INFO = 0x01;  /* Register: Overwrite existing information */
    public const int IP_REG_ADD_INFO = 0x02;  /* Register: Append existing information */
    public const int IP_REG_DELETE_BY_VALUE = 0x03;  /* Register: Delete by value */
    public const int IP_REG_MAINTAIN_LOCAL_INFO = 0x04;  /* Deregister: Maintain local information */
    public const int IP_REG_DELETE_ALL = 0x05;  /* Deregister: Erase local information */
    public const int IP_REG_QUERY_INFO = 0x06; /* Register: Query current information */

    /*
     * IPPARM_REG_ADDRESS |  Registration address structure IP_REGISTER_ADDRESS (declared in.gcip.h)
     */
    public const int IPPARM_REG_ADDRESS = 0x03;

    public const int IP_REG_SERVER_ADDR_LENGTH = 64;            /* server address length in characters */
    public const int IP_REG_CLIENT_ADDR_LENGTH = 128; /* client address length in characters */

    /*
     * IPSET_LOCAL_ALIAS | Set id for setting local aliases
     *
     * IPSET_SUPPORTED_PREFIXES | Set id for setting local prefixes for Gateway
     * applications
     */
    public const int IPSET_LOCAL_ALIAS = BASE_SETID + 1;
    public const int IPSET_SUPPORTED_PREFIXES = BASE_SETID + 2;

    /* parm ids under IPSET_LOCAL_ALIAS and IPSET_SUPPORTED_PREFIXES */
    public const int IPPARM_ADDRESS_TRANSPARENT = 0x00;
    public const int IPPARM_ADDRESS_DOT_NOTATION = 0x01;
    public const int IPPARM_ADDRESS_H323_ID = 0x02;
    public const int IPPARM_ADDRESS_PHONE = 0x03;
    public const int IPPARM_ADDRESS_URL = 0x04;
    public const int IPPARM_ADDRESS_EMAIL = 0x05;

    /* Set Id for DTMF related parameters for suppressing or sending of DTMF digits.
     */
    public const int IPSET_DTMF = BASE_SETID + 14;

    /* parm IDs defined under IPSET_DTMF
     * IPPARM_SUPPORT_DTMF_BITMASK |  This parameter id is used to specify the unsigned char
     * bitmask which specifies which DTMF transmission methods are to be supported.
     */
    public const int IPPARM_SUPPORT_DTMF_BITMASK = 0x01;

    /* Bits to enable support of DTMF method for IPPARM_SUPPORT_DTMF_BITMASK unsigned char bitmask:
     */
    public const int IP_DTMF_TYPE_USERINPUT_SIGNAL = 0x01;
    public const int IP_DTMF_TYPE_RFC_2833 = 0x02;
    public const int IP_DTMF_TYPE_INBAND_RTP = 0x04;
    public const int IP_DTMF_TYPE_ALPHANUMERIC = 0x08;


    /* parm IDs defined under IPSET_USERINPUTINDICATION
     *
     * IPPARM_UII_ALPHANUMERIC : User input is alphanumeric
     */
    public const int IPPARM_UII_ALPHANUMERIC = 0x15;
    public const int MAX_USER_INPUT_INDICATION_LENGTH = 256;


    /* parm IDs defined under IPSET_DTMF.  The data field for these parameters is char[] which
     * contains the digits to transmit to the remote endpoint.
     *
     * IPPARM_DTMF_RFC_2833 |  Used to initiate generating DTMF via RFC 2833 message.
     *
     * IPPARM_DTMF_ALPHANUMERIC | Used to initiate generating DTMF via H.245 User Input Indication
     * Alphanumeric message.
     *
     * IPPARM_DTMF_SIGNAL | Used to initiate generating DTMF via H.245 User Input Indication
     * Signal.
     *
     * IPPARM_DTMF_SIGNAL_UPDATE | Used to initiate generating DTMF via H.245 User Input Indication
     * Signal Update.
     *
     * IPPARM_DTMF_RFC2833_PAYLOAD_TYPE | Used to specify RFC2833 RTP payload type.  Data field is
     * unsigned char with a valid range of 96-127.
     */
    public const int IPPARM_DTMF_RFC_2833 = 0x02;
    public const int IPPARM_DTMF_ALPHANUMERIC = IPPARM_UII_ALPHANUMERIC; /* 0x15 */
    public const int IPPARM_DTMF_SIGNAL = 0x03;
    public const int IPPARM_DTMF_SIGNAL_UPDATE = 0x04;
    public const int IPPARM_DTMF_RFC2833_PAYLOAD_TYPE = 0x05;
    public const int IPPARM_TELEPHONY_EVENT_DTMF = 0x06;
    public const int IPPARM_TELEPHONY_EVENT_INFO = 0x07;

    /* IP_USE_STANDARD_PAYLOADTYPE | Use this value in the "payload_type" field of
     * the IP_xxx_CAPABILITY structures in case you want to use the standard
     * payload type for the selected coder, and not specify your own payload type
     */
    public const int IP_USE_STANDARD_PAYLOADTYPE = 0xff;
    /* Parameters to be used with GCSET_CHAN_CAPABILITY |
     *
     * IPPARM_LOCAL_CAPABILITY | This parameter id is used to pass the local capabilities
     * to be associated with a linedev, or a subset thereof (when being used to communicate
     * selected capabilities for a call. Value for this parameter id is an IP_CAPABILITY structure.
     */
    public const int IPPARM_LOCAL_TYPE = 0x01;
    public const int IPPARM_LOCAL_CAPABILITY = 0x02;

    /* Defines for Direction Codes
     * The following ID values are used for specifying or indicating direction for
     * the IP_CAPABILITY structure.
     */
    /* IP_CAP_DIR_LCLTRANSMIT | This value indicates a transmit capability for the
     * local end point.
     *
     * IP_CAP_DIR_LCLRECEIVE | This value indicates a receive capability for the
     * local end point.
     *
     * IP_CAP_DIR_LCLTXRX | This value indicates a bi-directional capability for
     * the local end point.
     *
     * IP_CAP_DIR_RMTTRANSMIT | This value indicates a transmit capability for the
     * remote peer.
     *
     * IP_CAP_DIR_RMTRECEIVE | This value indicates a receive capability for the
     * remote peer.
     *
     * IP_CAP_DIR_RMTTXRX | This value indicates a  bi-directional capability for
     * the remote peer.
     */

    public const int IP_CAP_DIR_LCLTRANSMIT = 0x01;
    public const int IP_CAP_DIR_LCLRECEIVE = 0x02;
    public const int IP_CAP_DIR_LCLTXRX = 0x03;
    public const int IP_CAP_DIR_LCLSENDONLY = 0x04;
    public const int IP_CAP_DIR_LCLRECVONLY = 0x05;
    public const int IP_CAP_DIR_LCLRTPINACTIVE = 0x06;
    public const int IP_CAP_DIR_LCLRTPRTCPINACTIVE = 0x07;

    public const int IP_CAP_DIR_RMTTRANSMIT = 0x10;
    public const int IP_CAP_DIR_RMTRECEIVE = 0x11;
    public const int IP_CAP_DIR_RMTTXRX = 0x12;
    public const int IP_CAP_DIR_RMTSENDONLY = 0x13;
    public const int IP_CAP_DIR_RMTRECVONLY = 0x14;
    public const int IP_CAP_DIR_RMTRTPINACTIVE = 0x15;
    public const int IP_CAP_DIR_RMTRTPRTCPINACTIVE = 0x16;


    public const int IP_CAP_DIR_FR1677_STOP = 0x2000;
    public const int IP_CAP_DIR_FR1677_RESTART = 0x2001;

    /* Set ID for fax over IP
     * IPSET_FOIP | This Set ID is used to set or get T38 related parameters
     */
    public const int IPSET_FOIP = BASE_SETID + 28;

    /* parm IDs defined under IPSET_FOIP for configuration
     * IPPARM_T38_CONNECT | connect media handle with fax handle
     * IPPARM_T38_DISCONNECT | disconnect media handle from fax handle
     * IPPARM_T38_OFFERED | T38 only call is offered
     */
    public const int IPPARM_T38_CONNECT = 0x1;
    public const int IPPARM_T38_DISCONNECT = 0x2;
    public const int IPPARM_T38_OFFERED = 0x3;

    /* Set Id for notification of the status and configuration information of transmit or receive
     * directions of media streaming. Notification is via GCEV_EXTENSION event of extension ID
     * IPEXTID_MEDIAINFO.
     */
    public const int IPSET_MEDIA_STATE = BASE_SETID + 16;

    /* Set ID for codec switch during a call
     * IPSET_SWITCH_CODEC | This Set ID is used to configure codec switch
     */
    public const int IPSET_SWITCH_CODEC = BASE_SETID + 29;

    /* parm IDs defined under IPSET_SWITCH_CODEC for configuration
     * IPPARM_T38_INITIATE | initiate T38 switch from audio
     * IPPARM_AUDIO_INITIATE | initiate audio switch from t38
     * IPPARM_T38_REQUESTED | incoming t38 switch request from audio
     * IPPARM_AUDIO_REQUESTED  | incoming audio switch request from t38
     * IPPARM_READY         | media is ready
     * IPPARM_ACCEPT        | accept incoming t38/audio switch request
     * IPPARM_REJECT        | reject incoming t38/audio switch request
     */
    public const int IPPARM_T38_INITIATE = 0x1;
    public const int IPPARM_AUDIO_INITIATE = 0x2;
    public const int IPPARM_T38_REQUESTED = 0x3;
    public const int IPPARM_AUDIO_REQUESTED = 0x4;
    public const int IPPARM_READY = 0x5;
    public const int IPPARM_ACCEPT = 0x6;
    public const int IPPARM_REJECT = 0x7;

    /* parm IDs defined under IPSET_MEDIA_STATE.  For XXX_CONNECTED parm IDs, the datatype of the
     * parameter is IP_CAPABILITY and consists of the coder configuration which resulted
     * from the capability exchange with the remote peer. For XXX_DISCONNECTED parm IDs, the data
     * value field is unused.
     *
     * IPPARM_TX_CONNECTED |  Streaming initiated in transmit direction.  The datatype of the
     * parameter is IP_CAPABILITY and consists of the coder configuration which resulted
     * from the capability exchange with the remote peer.
     *
     * IPPARM_TX_DISCONNECTED |  Streaming terminated in transmit direction. The data
     * value field is unused.
     *
     * IPPARM_RX_CONNECTED |  Streaming initiated in receive direction.  The datatype of the
     * parameter is IP_CAPABILITY and consists of the coder configuration which resulted
     * from the capability exchange with the remote peer.
     *
     * IPPARM_RX_DISCONNECTED |  Streaming termintated in receive direction. The data
     * value field is unused.
     */
    public const int IPPARM_TX_CONNECTED = 0x01;
    public const int IPPARM_TX_DISCONNECTED = 0x02;
    public const int IPPARM_RX_CONNECTED = 0x03;
    public const int IPPARM_RX_DISCONNECTED = 0x04;
    public const int IPPARM_TX_SENDONLY = 0x05;
    public const int IPPARM_TX_INACTIVE = 0x06;
    public const int IPPARM_RX_RECVONLY = 0x07;
    public const int IPPARM_RX_INACTIVE = 0x08;

    /* Set Id for notification of intermediate protocol states, i.e. Q.931 and H.245
     * connections and disconnections.  Notification is via GCEV_EXTENSION event of extension
     * ID IPEXTID_IPPROTOCOL_STATE.
     */
    public const int IPSET_IPPROTOCOL_STATE = BASE_SETID + 15;

    /* parm IDs defined under IPSET_IPPROTOCOL_STATE.  The data value field for all
     * parameters in this set will be unused.
     *
     * IPPARM_SIGNALING_CONNECTED |  Q.931 or SIP signaling connection established.
     * In case of Q.931, CONNECT or CONNECTACK received at caller or callee, respectively.
     *
     * IPPARM_SIGNALING_DISCONNECTED |  Q.931 or SIP signaling session disconnected.
     * In case of Q.931, RELEASE or RELEASECOMEPLETE received at terminator or its peer,
     * respectively.
     *
     * IPPARM_CONTROL_CONNECTED |  H.245 connection established. Logical Channels operations
     * can be performed (i.e. Request Mode).
     *
     * IPPARM_CONTROL_DISCONNECTED |  H.245 session disconnected, i.e. EndSession received or sent.
     *
     * IPPARM_EST_CONTROL_FAILED |  H.245 connection establishment failed. H.245 channel is not
     * available for duration of call.
     *
     */
    public const int IPPARM_SIGNALING_CONNECTED = 0x01;
    public const int IPPARM_SIGNALING_DISCONNECTED = 0x02;
    public const int IPPARM_CONTROL_CONNECTED = 0x03;
    public const int IPPARM_CONTROL_DISCONNECTED = 0x04;
    public const int IPPARM_EST_CONTROL_FAILED = 0x05;

    /*for reporting RTP addresses of endpoints on connect */
    public const int IPSET_RTP_ADDRESS = BASE_SETID + 34;
    public const int IPPARM_LOCAL = 0;
    public const int IPPARM_REMOTE = 1;
    public const int IPPARM_LOCAL_ENUM = 2;


    /* parm IDs defined under IPSET_MIME for configuration
     * IPPARM_MIME_PART                 | MIME part pointer to GC_PARM_BLK
     * IPPARM_MIME_PART_BODY_SIZE       | MIME part body size
     * IPPARM_MIME_PART_TYPE            | MIME part type string
     * IPPARM_MIME_PART_HEADER          | MIME part header string
     * IPPARM_MIME_PART_BODY            | MIME part pointer to memory buffer
     */
    public const int IPPARM_MIME_PART = 0x1;
    public const int IPPARM_MIME_PART_BODY_SIZE = 0x2;
    public const int IPPARM_MIME_PART_TYPE = 0x3;
    public const int IPPARM_MIME_PART_HEADER = 0x4;
    public const int IPPARM_MIME_PART_BODY = 0x5;

    /*
     * IPEXTID_SENDMSG | This extension is used for sending Q931 and
     * nonstandard messages. The sets supported are IPSET_MSG_H245,
     * IPSET_MSG_Q931 and IPSET_MSG_RAS.
     *
     * IPEXTID_GETINFO | This extension is used for retrieving call
     * related information
     *
     * IPEXTID_MEDIAINFO | This extension is used in GCEV_EXTENSION events
     * for notification of the initiation and termiation of of media streaming
     * in transmit or receive directions  In the case of media streaming
     * connection notification, the datatype of the parameter is IP_CAPABILITY
     * and consists of the coder configuration which resulted from the
     * capability exchange with the remote peer.
     *
     * IPEXTID_SEND_DTMF | This extension is used for sending DTMF digits.
     *
     * IPEXTID_RECEIVE_DTMF | This extension is used in GCEV_EXTENSION events
     * upon detection of DTMF digits.
     *
     * IPEXTID_IPPROTOCOL_STATE | This extension is used in GCEV_EXTENSION events
     * for notification of intermediate protocol states, i.e. Q.931 and H.245
     * session connections and disconnections, and the initation and termination
     * of media streaming.
     *
     * IPEXTID_FOIP | This extension is used in GCEV_EXTENSION events
     * for notification of information related to fax.
     *
     * IPEXTID_RECEIVEMSG | This extension is used when Q931, H245, RAS and
     * nonstandard messages are received.
     *
     * IPEXTID_CHANGEMODE | This extension is used to change call mode including
     * codec switch for both H323 and SIP
     * nonstandard messages are received.
     *
     * IPEXTID_LOCAL_MEDIA_ADDRESS |
     *
     * IPEXTID_RECEIVED_18X_RESPONSE | This extension is used in GCEV_EXTENSION events
     * upon reception of 18x SIP provisional response
     *
     * IPEXTID_SIP_STATS | This extension is used in GCEV_EXTENSIONCMPLT event
     * upon reception of SIP call statistics
     */
    public const int IPEXTID_SENDMSG = 0x01;
    public const int IPEXTID_GETINFO = 0x02;
    public const int IPEXTID_MEDIAINFO = 0x03;
    public const int IPEXTID_SEND_DTMF = 0x04;
    public const int IPEXTID_RECEIVE_DTMF = 0x05;
    public const int IPEXTID_IPPROTOCOL_STATE = 0x06;
    public const int IPEXTID_FOIP = 0x07;
    public const int IPEXTID_RECEIVEMSG = 0x08;
    public const int IPEXTID_CHANGEMODE = 0x09;
    public const int IPEXTID_LOCAL_MEDIA_ADDRESS = 0x0a;
    public const int IPEXTID_RECEIVED_18X_RESPONSE = 0x0b;
    public const int IPEXTID_GETCALLINFOUPDATE = 0x0c;
    public const int IPEXTID_SIP_STATS = 0x0d;

    /*
     * IPPARM_REG_STATUS |  Status of registration operation, being confirmed or rejected.
     *                      Notification is via GCEV_SERVICERESP event.
     */
    public const int IPPARM_REG_STATUS = 0x04;

    public const int IP_REG_REJECTED = 0x00;
    public const int IP_REG_CONFIRMED = 0x01;

    /* This parameter ID, used in conjunction with the IPSET_REG_INFO set ID, allows applications to be
       notified of the Service ID that a particular GCEV_SERVICERESP or GCEV_TASKFAIL event is
       associated with. */
    public const int IPPARM_REG_SERVICEID = 0x06;

    /* Set ID for SIP message information
     * IPSET_SIP_MSGINFO | This Set ID is used configuration and retrieval
     * of SIP related message information (e.g. Request URI).
     */
    public const int IPSET_SIP_MSGINFO = BASE_SETID + 27;
    public const int IPSET_MIME = BASE_SETID + 30;
    public const int IPSET_MIME_200OK_TO_BYE = BASE_SETID + 31;
    public const int IPSET_SIP_RESPONSE_CODE = BASE_SETID + 32;

    public const int IPSET_SIP_REQUEST_ERROR = BASE_SETID + 37;
    public const int IPSET_SIP_SESSION_TIMER = BASE_SETID + 40;
    public const int IPSET_SIP_STATS = BASE_SETID + 44;

    /* Different Parm IDs that can be used with SetID IPSET_SIP_MSGINFO */

    public const int IPPARM_REQUEST_URI = 0x01;
    public const int IPPARM_CONTACT_URI = 0x02;
    public const int IPPARM_FROM_DISPLAY = 0x03;
    public const int IPPARM_TO_DISPLAY = 0x04;
    public const int IPPARM_CONTACT_DISPLAY = 0x05;
    public const int IPPARM_REFERRED_BY = 0x06;
    public const int IPPARM_REPLACES = 0x07;
    public const int IPPARM_CONTENT_DISPOSITION = 0x08;
    public const int IPPARM_CONTENT_ENCODING = 0x09;
    public const int IPPARM_CONTENT_LENGTH = 0x0a;
    public const int IPPARM_CONTENT_TYPE = 0x0b;
    public const int IPPARM_REFER_TO = 0x0c;
    public const int IPPARM_DIVERSION_URI = 0x0d;
    public const int IPPARM_EVENT_HDR = 0x0e;
    public const int IPPARM_EXPIRES_HDR = 0x0f;
    public const int IPPARM_CALLID_HDR = 0x10;
    public const int IPPARM_SIP_HDR = 0x11;
    public const int IPPARM_FROM = 0x12;
    public const int IPPARM_TO = 0x13;
    public const int IPPARM_SIP_HDR_REMOVE = 0x14;
    public const int IPPARM_SIP_VIA_HDR_REPLACE = 0x15;

    /* Set ID for SIP message types handed by GCEV_EXTENSION
     * IPSET_MSG_SIP | This Set ID is used to set or get the SIP message type.
     */
    public const int IPSET_MSG_SIP = BASE_SETID + 35;
    public const int IPPARM_MSG_SIP_RESPONSE_CODE = 1;

    public const int IPPARM_MSGTYPE = 0x00;



    /*
     * IP_MSGTYPE_SIP_SUBSCRIBE           | SIP Subscribe message.
     * IP_MSGTYPE_SIP_SUBSCRIBE_ACCEPT    | Response to SIP Subscribe message with a 200 OK.
     * IP_MSGTYPE_SIP_SUBSCRIBE_REJECT    | Response to SIP Subscribe message with a failure.
     * IP_MSGTYPE_SIP_SUBSCRIBE_EXPIRE    | Expiration of Subscribe
     * IP_MSGTYPE_SIP_NOTIFY              | SIP Notify message.
     * IP_MSGTYPE_SIP_NOTIFY_ACCEPT       | Response to SIP Notify message with a 200 OK.
     * IP_MSGTYPE_SIP_NOTIFY_REJECT       | Response to SIP Notify message with a failure.
     * IP_MSGTYPE_SIP_INFO                | SIP Info message
     * IP_MSGTYPE_SIP_INFO_ACCEPT         | Response to SIP Info message with a 200 OK.
     * IP_MSGTYPE_SIP_INFO_REJECT         | Response to SIP Info message with a failure.
     * IP_MSGTYPE_SIP_REINVITE_ACCEPT     | Response to SIP Re-INVITE message (RFC-4579)
     * IP_MSGTYPE_SIP_REINVITE_REJECT     | Response to SIP Re-INVITE message (RFC-4579)
     */
    public const int IP_MSGTYPE_SIP_SUBSCRIBE = 0x1;
    public const int IP_MSGTYPE_SIP_SUBSCRIBE_ACCEPT = 0x2;
    public const int IP_MSGTYPE_SIP_SUBSCRIBE_REJECT = 0x3;
    public const int IP_MSGTYPE_SIP_SUBSCRIBE_EXPIRE = 0x4;
    public const int IP_MSGTYPE_SIP_NOTIFY = 0x5;
    public const int IP_MSGTYPE_SIP_NOTIFY_ACCEPT = 0x6;
    public const int IP_MSGTYPE_SIP_NOTIFY_REJECT = 0x7;
    public const int IP_MSGTYPE_SIP_INFO = 0x8;
    public const int IP_MSGTYPE_SIP_INFO_OK = 0x9;
    public const int IP_MSGTYPE_SIP_INFO_FAILED = 0xa;
    /* Options support  */
    public const int IP_MSGTYPE_SIP_OPTIONS = 0xb;
    public const int IP_MSGTYPE_SIP_OPTIONS_OK = 0xc;
    public const int IP_MSGTYPE_SIP_OPTIONS_FAILED = 0xd;

    /* Re-INVITE support */
    public const int IP_MSGTYPE_SIP_CANCEL = 0xe;

    /* UPDATE support */
    public const int IP_MSGTYPE_SIP_UPDATE = 0xf;
    public const int IP_MSGTYPE_SIP_UPDATE_OK = 0x10;
    public const int IP_MSGTYPE_SIP_UPDATE_FAILED = 0x11;

    /* MESSAGE support */
    public const int IP_MSGTYPE_SIP_MESSAGE = 0x12;
    public const int IP_MSGTYPE_SIP_MESSAGE_OK = 0x13;
    public const int IP_MSGTYPE_SIP_MESSAGE_FAILED = 0x14;

    /* RFC4579 support */
    public const int IP_MSGTYPE_SIP_REINVITE_ACCEPT = 0x15;
    public const int IP_MSGTYPE_SIP_REINVITE_REJECT = 0x16;


}



/*
 * enumIPCCLIBMediaMode used in structure IPCCLIB_START_DATA
 * to configure the libraries media behavior.
 */
public enum enumIPCCLIBMediaMode
{
    MEDIA_OPERATIONAL_MODE_1PCC,
    MEDIA_OPERATIONAL_MODE_3PCC
}

public enum EnumSIP_RequestRetry
{
    ENUM_REQUEST_RETRY_UNDEFINED = 0,
    ENUM_REQUEST_RETRY_NONE,
    ENUM_REQUEST_RETRY_DNS,
    ENUM_REQUEST_RETRY_FORCEDTCP,
    ENUM_REQUEST_RETRY_ALL
}