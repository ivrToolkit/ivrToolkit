// ReSharper disable InconsistentNaming

using System;
using System.Runtime.InteropServices;

// ReSharper disable CommentTypo
// ReSharper disable UnusedMember.Global

namespace ivrToolkit.Dialogic.Common.DialogicDefs
{
    public class gcip_h
    {
        [DllImport("libgc.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gc_util_next_parm(IntPtr parm_blk, IntPtr cur_parm);


        /* Use INIT_IPCCLIB_START_DATA function to initialize the IPCCLIB_START_DATA structure.
         *
         * Applications must use this function to inititialize the IPCCLIB_START_DATA data 
         * structures to maintain forward binary compatability.
         */
        public static IPCCLIB_START_DATA CreateAndInitIpcclibStartData()
        {
            return new IPCCLIB_START_DATA
            {
                version = 0x201,
                delimiter = Convert.ToByte(','),
                max_parm_data_size = uint.MaxValue,
                media_operational_mode = enumIPCCLIBMediaMode.MEDIA_OPERATIONAL_MODE_1PCC,
            };
        }

        public const ushort MIME_MEM_VERSION = 0x100;

        public static U_ipaddr CreateAndInitUiPAddr()
        {
            return new U_ipaddr
            {
                ipv4 = uint.MaxValue,
                ipv6_1 = uint.MaxValue,
                ipv6_2 = uint.MaxValue,
                ipv6_3 = uint.MaxValue,
                ipv6_4 = uint.MaxValue
            };
        }
        public static IP_ADDR CreateAndInitIpAddr()
        {
            return new IP_ADDR
            {
                ip_ver = byte.MaxValue,
                u_ipaddr = CreateAndInitUiPAddr()
            };
        }
        public static MIME_MEM CreateAndInitMimeMem()
        {
            return new MIME_MEM
            {
                number = uint.MaxValue,
                size = uint.MaxValue,
                version = MIME_MEM_VERSION
            };
        }

        // Its very important that these objects be initialized with FFs where I can!
        public static IP_VIRTBOARD CreateAndInitIpVirtboard()
        {
            return new IP_VIRTBOARD
            {
                localIP = CreateAndInitIpAddr(),
                E_SIP_Add_rport_to_Via_Header = EnumSIP_Enabled.ENUM_Disabled,    /* FR6234 */
                E_SIP_DefaultTransport = EnumSIP_TransportProtocol.ENUM_UDP,
                E_SIP_GetUnregisteredHdrs_Enabled = EnumSIP_Enabled.ENUM_Disabled, /* XMS-3858 */
                E_SIP_IPv6 = EnumSIP_Enabled.ENUM_Disabled,
                E_SIP_MESSAGE_Access = EnumSIP_Enabled.ENUM_Disabled,   /* FR6192 Application access to SIP MESSAGE method is disabled by default. */
                E_SIP_OOD_NOTIFY_Access = EnumSIP_Enabled.ENUM_Enabled,  /* FR6280 unsubscribed NOTIFY" outside a dialog, enabled by default (CR45699) */
                E_SIP_OPTIONS_Access = EnumSIP_Enabled.ENUM_Disabled,
                E_SIP_OutboundProxyTransport = EnumSIP_TransportProtocol.ENUM_UDP,
                E_SIP_Overlapped_INFO_Enabled = EnumSIP_Enabled.ENUM_Disabled,  /* XMS-3174 */
                E_SIP_Persistence = EnumSIP_Persistence.ENUM_PERSISTENCE_TRANSACT_USER,
                E_SIP_PrackEnabled = EnumSIP_Enabled.ENUM_Disabled,
                E_SIP_PreConditions_Enabled = EnumSIP_Enabled.ENUM_Disabled,  /* XMS-1773 */
                E_SIP_RequestRetry = EnumSIP_RequestRetry.ENUM_REQUEST_RETRY_ALL,
                E_SIP_SessionTimer_Enabled = EnumSIP_Enabled.ENUM_Disabled,
                E_SIP_UPDATE_Access = EnumSIP_Enabled.ENUM_Disabled,
                E_SIP_dynamic_outbound_proxy_enabled = EnumSIP_Enabled.ENUM_Disabled,
                E_SIP_tcpenabled = EnumSIP_Enabled.ENUM_Disabled,
                SIP_SessionTimer_MinSE = 90,
                SIP_SessionTimer_SessionExpires = 1800,
                SIP_maxUDPmsgLen = 0,
                SIP_registrar_registrations = -1,
                h323_max_calls = uint.MaxValue,
                h323_msginfo_mask = 0,
                h323_signaling_port = ushort.MaxValue,
                localIPv6 = CreateAndInitIpAddr(),
                localIPv6_iface_name = null,
                outbound_proxy_IP = CreateAndInitIpAddr(),
                outbound_proxy_hostname = null,
                outbound_proxy_port = ushort.MaxValue,
                reserved = IntPtr.Zero,
                sip_max_calls = uint.MaxValue,
                sip_max_subscription = 0, /* if 0, stack sets it to (max call leg + 7 (extra calls) + 20) */
                sip_mime_mem = CreateAndInitMimeMem(),
                sip_msginfo_mask = 0,
                sip_signaling_port = ushort.MaxValue,
                sip_stack_cfg = IntPtr.Zero,
                sip_tls_engine = IntPtr.Zero,
                size = (ushort)Marshal.SizeOf<IP_VIRTBOARD>(),
                sup_serv_mask = gcip_defs_h.IP_SUP_SERV_DISABLED,
                terminal_type = gcip_defs_h.IP_TT_GATEWAY,
                total_max_calls = uint.MaxValue,
                version = IP_VIRTBOARD_VERSION
            };
        }
        
        //public const int DEFAULT_MAXUDPMSGLEN(0)
        public const int VIRTBOARD_VERSION_TCP_SUPPORT = 0x106; /*this value subject to change*/
        //public const int VIRTBOARD_SIP_NOUDPMSGSIZECHECK(0)

        public const int VIRTBOARD_VERSION_REQUEST_RETRY = 0x107;
        public const int VIRTBOARD_VERSION_OPTIONS_SUPPORT = 0x108;
        public const int VIRTBOARD_VERSION_SIP_REGISTRAR = 0x109;
        public const int VIRTBOARD_VERSION_TLS_SUPPORT = 0x10a;
        public const int VIRTBOARD_VERSION_SIP_PRACK = 0x10b;
        public const int VIRTBOARD_VERSION_SESSION_TIMER_SUPPORT = 0x10c;
        public const int VIRTBOARD_VERSION_SIP_STACK_CFG = 0x10d;
        public const int VIRTBOARD_VERSION_UPDATE_SUPPORT = 0x10e;
        public const int VIRTBOARD_VERSION_DYNAMIC_OUTBOUND_PROXY_SUPPORT = 0x10f;
        public const int VIRTBOARD_VERSION_IPV6_SUPPORT = 0x110;
        public const int VIRTBOARD_VERSION_MAX_SUBSCRIPTION = 0x111;
        public const int VIRTBOARD_VERSION_MESSAGE_SUPPORT = 0x112;       /* FR6192 */
        public const int VIRTBOARD_VERSION_OOD_NOTIFY_SUPPORT = 0x113;    /* FR6280 */
        public const int VIRTBOARD_VERSION_RPORT_IN_VIA_HEADER = 0x114;   /* FR6234 */
        public const int VIRTBOARD_VERSION_PRECONDITION_SUPPORT = 0x115;   /* XMS1773 */
        public const int VIRTBOARD_VERSION_OVERLAPPED_INFO_SUPPORT = 0x116;   /* XMS3174 */
        public const int VIRTBOARD_VERSION_UNREGISTERED_HDRS_SUPPORT = 0x117;  /* XMS3858 */


        public const int IP_VIRTBOARD_VERSION = VIRTBOARD_VERSION_UNREGISTERED_HDRS_SUPPORT; /* XMS-3858 */

        public const int IP_AUTHENTICATION_VERSION = (0x100 & 0xFFFF) |  (20 << 16); // IP_AUTHENTICATION size is 20


    }
    /* IPCCLIB_START_DATA | Used in CCLIB_START_STRUCT to configure GC_H3R_LIB library 
     *
     * version : version of structure - set to 0x100 for this release
     *                                - set to 0x101 for Request-URI feature
     *                                - set to 0x200 for get/set access of GC parm data
     *                                  greater than 255 bytes. 
     *
     * delimiter : ANSI character used to change default address string delimiter of ","
     *
     * num_boards : number of virtual boards specifed in board_info array element.
     *
     * board_info: array of virtual board board elements of type IP_VIRTBOARD
     *
     * media_operational_mode: 1PCC Embedded media or 3PCC Split Media library operation 
     *
     */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct IPCCLIB_START_DATA
    {
        public ushort version;
        public byte delimiter;
        public byte num_boards;
        public IntPtr board_list;

        // todo unsigned long should be CULong in .net 6 but we aren't there yet
        public uint max_parm_data_size; // was ulong in c. currently works on windows only until switch to CULong

        public enumIPCCLIBMediaMode media_operational_mode;
    }

    /* New version number and structure member must be added to 5.1 line as well 
       to preserve compatibility with 5.2 line  */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct IP_VIRTBOARD
    {
        public ushort version; /* library use only. do not change value */
        public uint total_max_calls;
        public uint h323_max_calls;
        public uint sip_max_calls;
        public IP_ADDR localIP;
        public ushort h323_signaling_port;
        public ushort sip_signaling_port;
        public IntPtr reserved; /* (was (void*))library use only. do not change value */
        public ushort size; /* library use only. do not change value */
        public uint sip_msginfo_mask; /* added in version 0x0101               */
        public uint sup_serv_mask; /* added in version 0x0102               */
        public uint h323_msginfo_mask; /* added in version 0x0103               */
        public MIME_MEM sip_mime_mem; /* added in version 0x0104               */
        public ushort terminal_type; /* added in version 0x0104               */
        public IP_ADDR outbound_proxy_IP; /* added in version 0x0105 */
        public ushort outbound_proxy_port; /* added in version 0x0105 */

        [MarshalAs(UnmanagedType.LPStr)]
        public string outbound_proxy_hostname; /* added in version 0x0105 */

        /* the following added for VIRTBOARD_VERSION_TCP_SUPPORT */
        public EnumSIP_Enabled E_SIP_tcpenabled;
        public EnumSIP_TransportProtocol E_SIP_OutboundProxyTransport;
        public EnumSIP_Persistence E_SIP_Persistence;
        public ushort SIP_maxUDPmsgLen;

        public EnumSIP_TransportProtocol E_SIP_DefaultTransport;
        /* end VIRTBOARD_VERSION_TCP_SUPPORT additions*/

        /* the following added for VIRTBOARD_VERSION_REQUEST_RETRY */
        public EnumSIP_RequestRetry E_SIP_RequestRetry;

        /* end VIRTBOARD_VERSION_REQUEST_RETRY additions*/
        public EnumSIP_Enabled E_SIP_OPTIONS_Access;


        /* Begin VIRTBOARD_VERSION_SIP_REGISTRAR additions */
        public int SIP_registrar_registrations;
        /* End VIRTBOARD_VERSION_SIP_REGISTRAR additions   */

        /* The following is added for VIRTBOARD_VERSION_TLS_SUPPORT support */
        public IntPtr sip_tls_engine; // todo is this the correct way
        /* end VIRTBOARD_VERSION_TLS_SUPPORT additions */

        /* Begin PRACK related additions. */
        public EnumSIP_Enabled E_SIP_PrackEnabled;
        /* End PRACK related additions. */

        /* The following is added for VIRTBOARD_VERSION_SESSION_TIMER_SUPPORT support */
        public EnumSIP_Enabled E_SIP_SessionTimer_Enabled;
        public uint SIP_SessionTimer_SessionExpires;

        public uint SIP_SessionTimer_MinSE;
        /* end VIRTBOARD_VERSION_SESSION_TIMER_SUPPORT additions */

        /* The following is added for VIRTBOARD_VERSION_SIP_STACK_CFG support */
        public IntPtr sip_stack_cfg; // todo is this the correct way or should I be using IntPtr?
        /* end VIRTBOARD_VERSION_SIP_STACK_CFG additions */

        /* Begin UPDATE related additions */
        public EnumSIP_Enabled E_SIP_UPDATE_Access;
        /* End UPDATE related additions */

        /* FR3468 - Dynamic Outbound Proxy Selection */
        public EnumSIP_Enabled E_SIP_dynamic_outbound_proxy_enabled;

        public EnumSIP_Enabled E_SIP_IPv6; /* Enable IPv6 Support */
        public IP_ADDR localIPv6; /* Local IP Address for IPv6 */

        [MarshalAs(UnmanagedType.LPStr)]
        public string localIPv6_iface_name; /* Name of the local IP address interface */

        /* (for IPv6 Link-Local Scope Only) */
        public uint sip_max_subscription;
        public EnumSIP_Enabled E_SIP_MESSAGE_Access; /* FR6192 SIP MESSAGE method support */
        public EnumSIP_Enabled E_SIP_OOD_NOTIFY_Access; /* FR6280 SIP out of dialog NOTIFY support */
        public EnumSIP_Enabled E_SIP_Add_rport_to_Via_Header; /* FR6234 - rport support */
        public EnumSIP_Enabled E_SIP_PreConditions_Enabled; /* XMS-1773 */
        public EnumSIP_Enabled E_SIP_Overlapped_INFO_Enabled; /* XMS-3174 */
        public EnumSIP_Enabled E_SIP_GetUnregisteredHdrs_Enabled; /* XMS-3858 */
    }

    /* IP_ADDR |
     *
     * ip_ver : IP version as IPVER4 or IPVER6
     *
     * u_ipaddr : IP address as 32 bit IPv4 or 128 bit IPv6 (union)
     *            or IP_RETRIEVE_HOST_IPADDR to internally retrieve host IP address
     */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct IP_ADDR
    {
        public byte ip_ver;
        public U_ipaddr u_ipaddr;
    }

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi, Size = 16, Pack = 1)]
    public struct U_ipaddr
    {
        [FieldOffset(0)]
        public uint ipv4;

        [FieldOffset(0)]
        public uint ipv6_1;

        [FieldOffset(4)]
        public uint ipv6_2;

        [FieldOffset(8)]
        public uint ipv6_3;

        [FieldOffset(12)]
        public uint ipv6_4;

    }

    /* SIP_TLS_ENGINE | Used in IP_VIRTBOARD to specify virtual board settings 
     *
     * Use INIT_SIP_TLS_ENGINE function to initialize the structure to default values
     *
     * version: version of structure 
     *
     * sip_tls_port: TLS port number GC will listen, default port number is 5061
     *
     * E_sip_tls_method: indicates the version of SSL to use: currently only TLSv1 is supported
     *
     * local_rsa_private_key_filename: file containing TLS RSA private key of local certificate, must be PEM 
     * (base64 encoded) X509 format, in plain text or encrypted.  default is NULL
     *
     * local_rsa_private_key_password: password string to read TLS RSA private key of local certificate, 
     * if it is encrypted.  default is NULL
     *
     * local_rsa_cert_filename: PEM file containing TLS RSA certificate representing local identity, must be PEM 
     * (base64 encoded) X509 format, in plain text or encrypted. default is NULL
     *
     * local_dss_private_key_filename: file containing TLS DSS private key of local certificate, must be PEM 
     * (base64 encoded) X509 format, in plain text or encrypted.  default is NULL
     *
     * local_dss_private_key_password: password string to read TLS DSS private key of local certificate, 
     * if it is encrypted.  default is NULL
     *
     * local_dss_cert_filename: PEM file containing TLS DSS certificate representing local identity, must be PEM 
     * (base64 encoded) X509 format, in plain text defaultis NULL
     *
     * ca_cert_number:  number of trusted certificates. TLS engine can trust zero, 
     * one or more root certificates. Once an engine trusts a root certificate, it will 
     * approve all valid certificates issued by that root certificate. Trusted certificates 
     * are (usually) root certificates. Default is 0.
     *
     * ca_cert_filename: array of trusted certificates filenames, must be PEM 
     * (base64 encoded) X509 format, in plain text. The size of array is 
     * ca_cert_number. Default is NULL.
     *
     * chain_cert_number: number of  chained certificates. An engine may hold a 
     * certificate that is not issued directly by a root certificate, but by a certificate 
     * authority delegated by that root certificate. To add this intermediate certificate
     * to the chain of certificates that the engine will present during a handshake, specify 
     * the chained certificates number here. Default is 0.
     *
     * chain_cert_filename: array of chained certificates filenames, must be PEM 
     * (base64 encoded) X509 format, in plain text. The size of array is 
     * chain_cert_number. Default is NULL.
     *
     * crl_number: number of CRL files. An engine may look up certificate revocation list
     * while examine the incoming certficates. To add one or more CRL files, specifiy 
     * the number of the CRL files here.Default is 0.
     *
     * crl_filename: array of CRL filenames, must be PEM format in plain text. The size of array is 
     * crl_number. Default is NULL.
     *
     * local_cipher_suite: the list of ciphers is specified by a specially formatted string
     * defined by OPENSSL. OPENSSL allows for several keywords in the elist, which are shortcuts
     * for sets of ciphers. Default is NULL which uses OPENSSL default string.
     *
     * dh_param_512_filename: filename containing DH parameter with 512 bit key length. 
     * Default is NULL which uses pre-built DH parameter with 512 bit key length. 
     *
     * dh_param_1024_filename: filename containing DH parameter with 1024 bit key length. 
     * Default is NULL which uses pre-built DH parameter with 1024 bit key length. 
     *
     * session_id: If session id is set, session caching is enabled on the server side.
     * session id will be provided to client during handshake so that client may reuse
     * the session for future connection. Default is NULL and session caching is disabled.
     *
     * E_client_cert_required: if set to ENUM_Enabled, TLS server will require client's certificate 
     * for mutual authentication. Default is ENUM_Disabled.
     *
     * E_block_udp_port: if set to ENUM_Enabled, UDP port will be disabled to prevent downgrade
     * attack. Both send and receive on UDP port will be rejected. Default is ENUM_Disabled.
     * 
     * E_block_tcp_port: if set to ENUM_Enabled, TCP port will be disabled to prevent downgrade
     * attack. Both send and receive on TCP port will be rejected. Default is ENUM_Disabled.
     * 
     * verify_cert_CB: if not NULL, it expects a pointer to C-style function for application 
     * X.509 certification verification callback
     *
     * post_connect_CB: if not NULL, it expects a pointer to C-style function for 
     * application post-connection hostname assertion override callback
     *
     */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SIP_TLS_ENGINE
    {
        public uint version; /* (was long)version set by INIT_SIP_TLS_ENGINE */
        public ushort sip_tls_port; /*  TLS port number GC will listen */
        public EnumSIP_TLS_METHOD E_sip_tls_method; /* TLS method*/

        [MarshalAs(UnmanagedType.LPStr)]
        public string local_rsa_private_key_filename; /* TLS local RSA private key file name */
        [MarshalAs(UnmanagedType.LPStr)]
        public string local_rsa_private_key_password; /* TLS local RSA private key password */
        [MarshalAs(UnmanagedType.LPStr)]
        public string local_rsa_cert_filename; /* TLS local RSA certificate file name */
        [MarshalAs(UnmanagedType.LPStr)]
        public string local_dss_private_key_filename; /* TLS local DSS private key file name */
        [MarshalAs(UnmanagedType.LPStr)]
        public string local_dss_private_key_password; /* TLS local DSS private key password */
        [MarshalAs(UnmanagedType.LPStr)]
        public string local_dss_cert_filename; /* TLS local DSS certificate file name */
        public uint ca_cert_number; /* number of CA certificates */
    public IntPtr ca_cert_filename; /* CA certificate file names */
        public uint chain_cert_number; /* chained certificate number*/ 
    public IntPtr chain_cert_filename; /* chained certificate file names */
        public uint crl_number; /* number of CRL files */
    public IntPtr crl_filename; /* CRL file names containing certificate revocation list*/
        [MarshalAs(UnmanagedType.LPStr)]
        public string local_cipher_suite; /* local cipher suite list string*/
        [MarshalAs(UnmanagedType.LPStr)]
        public string dh_param_512_filename; /* DH parameter file name with 512 bit key length */
        [MarshalAs(UnmanagedType.LPStr)]
        public string dh_param_1024_filename; /* DH parameter file name with 1024 bit key length*/
        [MarshalAs(UnmanagedType.LPStr)]
        public string session_id; /* session identifier*/
        public EnumSIP_Enabled E_client_cert_required; /* client certificate is required */
        public EnumSIP_Enabled E_block_udp_port; /* block UDP port to prevent downgrade attack */
        public EnumSIP_Enabled E_block_tcp_port; /* block TCP port to prevent downgrade attack */
    public IntPtr verify_cert_CB; /* CB to a server certificate callback */
    public IntPtr post_connect_CB; /* CB to a post-connect callback */
    }

    /* MIME_MEM | used in IP_VERTBOARD to specify MIME memory pool
     * Use INIT_MIME_MEM function to initialize the structure to defautl values
     *
     * version : 0x104 or up
     *
     * size : size of MIME buffer
     *
     * number: number of MIME buffer
     */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct MIME_MEM
    {
        public ushort version; /* library use only. do not change value */
        public uint size; /* size of MIME buffer */
        public uint number; /* number of MIME buffer */
    }

    /* SIP_STACK_CFG | Used in IP_VIRTBOARD to specify virtual board settings 
     *
     * Use INIT_SIP_STACK_CFG function to initialize the structure to default values
     *
     * version: version of structure 
     *
     * retransmissionT1
     * T1 determines several timers as defined in RFC3261. For example, When an
     * unreliable transport protocol is used, a Client Invite transaction retransmits
     * requests at an interval that starts at T1 seconds and doubles after every
     * retransmission. A Client General transaction retransmits requests at an interval
     * that starts at T1 and doubles until it reaches T2.
     *
     * retransmissionT2
     * Determines the maximum retransmission interval as defined in RFC 3261. For
     * example, when an unreliable transport protocol is used, general requests are
     * retransmitted at an interval which starts at T1 and doubles until it reaches T2. If
     * a provisional response is received, retransmissions continue but at an interval of
     * T2.The parameter value cannot be less than 4000.
     *
     * retransmissionT4
     * T4 represents the amount of time the network takes to clear messages between
     * client and server transactions as defined in RFC 3261. For example, when
     * working with an unreliable transport protocol, T4 determines the time that a
     * UAS waits after receiving an ACK message and before terminating the
     * transaction.
     *
     * generalLingerTimer
     * After a server sends a final response, the server cannot be sure that the client has
     * received the response message. The server should be able to retransmit the
     * response upon receiving retransmissions of the request for generalLingerTimer
     * milliseconds.
     * 
     * inviteLingerTimer
     * After sending an ACK for an INVITE final response, a client cannot be sure that
     * the server has received the ACK message. The client should be able to
     * retransmit the ACK upon receiving retransmissions of the final response for
     * inviteLingerTimer milliseconds.
     * 
     * provisionalTimer
     * The provisionalTimer is set when receiving a provisional response on an Invite
     * transaction. The transaction will stop retransmissions of the Invite request and
     * will wait for a final response until the provisionalTimer expires. If you set the
     * provisionalTimer to zero (0), no timer is set. The Invite transaction will wait
     * indefinitely for the final response.
     * 
     * cancelGeneralNoResponseTimer
     * When sending a CANCEL request on a General transaction, the User Agent
     * waits cancelGeneralNoResponseTimer milliseconds before timeout termination
     * if there is no response for the cancelled transaction.
     * 
     * cancelInviteNoResponseTimer
     * When sending a CANCEL request on an Invite request, the User Agent waits
     * cancelInviteNoResponseTimer milliseconds before timeout termination if there
     * is no response for the cancelled transaction.
     * 
     * generalRequestTimeoutTimer
     * After sending a General request, the User Agent waits for a final response
     * generalRequestTimeoutTimer milliseconds before timeout termination (in this
     * time the User Agent retransmits the request every T1, 2*T1, ... , T2, ...
     * milliseconds)
     * 
     * forked1xxTimerTimeout
     * Sets the timeout value for the forked-1xx-timer which is set by a forked call-leg
     * after receiving the first 1xx response. This timer is released when the call-leg 
     * receives a 2xx response. If the timer expires before 2xx reception, the call-leg 
     * is terminated. This timeout value defines how long the call-leg will wait for a 
     * 2xx response before termination.
     * 
     * removeOldAuth 
     * This Enum indicates whether to remove old authentication challenges that were 
     * received from the same realm. If a new challenge is received from the server (401/407) 
     * and this field is enabled, the Stack will check whether an old challenge from the same 
     * realm exists. If so, the old challenge will be removed.
     * 
     */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SIP_STACK_CFG
    {
        public uint version; /* (was unsigned long)version set by INIT_SIP_STACK_CFG */
        public int retransmissionT1;
        public int retransmissionT2;
        public int retransmissionT4;
        public int generalLingerTimer;
        public int inviteLingerTimer;
        public int provisionalTimer;
        public int cancelGeneralNoResponseTimer;
        public int cancelInviteNoResponseTimer;
        public int generalRequestTimeoutTimer;
        public int forked1xxTimerTimeout; /* added in version 0x101               */
        public EnumSIP_Enabled removeOldAuth; /* added in version 0x102               */
    };

    public enum EnumSIP_Enabled
    {
        ENUM_Disabled = 0,
        ENUM_Enabled = 1
    }

    public enum EnumSIP_TransportProtocol
    {
        ENUM_UDP = 0,
        ENUM_TCP = 1,
        ENUM_TLS = 2,
        ENUM_TCP_RETRY = 0x101
    }

    /* IP_VIRTBOARD | Used in IPCCLIB_START_DATA to specify virtual board settings 
     *
     * Use INIT_IP_VIRTBOARD function to initialize the structure to default values
     * the set any non-default parameters in structure.
     *
     * version : version of structure 
     *
     * total_max_calls : maximum total number of H.323 and SIP calls supported: 
     * possible values are 1-LIMIT_MAX_CALLS or IP_CFG_MAX_AVAILABLE_CALLS
     *
     * h323_max_calls : maximum number of H.323 current calls supported: 
     * possible values are 1-LIMIT_MAX_CALLS or IP_CFG_NO_CALLS or IP_CFG_MAX_AVAILABLE_CALLS
     *
     * sip_max_calls : maximum number of SIP current calls supported: 
     * possible values are 1-LIMIT_MAX_CALLS or IP_CFG_NO_CALLS or IP_CFG_MAX_AVAILABLE_CALLS
     *
     * localIP : local IP address of type IP_ADDR 
     *
     * h323_signaling_port : H.323 call signaling port or IP_CFG_DEFAULT
     *
     * sip_signaling_port : SIP call signaling port or IP_CFG_DEFAULT
     *
     * reserved : must be set to NULL
     *
     * size - size of pack(1) structure.
     *
     * sip_msginfo_mask : bitmask to enable/disable access to SIP message fields
     *
     * sup_serv_mask : bitmask to enable/disable supplementary services
     * possible bits are IP_SUP_SERV_DISABLED or IP_SUP_SERV_CALL_XFER
     *
     * h323_msginfo_mask: bitmask to enable/disable information elements
     * exposure. Possible bits are IP_INFOELEMENT_DISABLED or
     * IP_INFOELEMENT_ENABLED
     *
     * terminal_type : Parameter representing h.323 and h.245 terminal type used
     *                 in GateKeeper registration and MSD respectivly.
     */
    public enum EnumSIP_Persistence
    {
        ENUM_PERSISTENCE_NONE = -1,
        ENUM_PERSISTENCE_TRANSACT,
        ENUM_PERSISTENCE_TRANSACT_USER
    }

    public enum EnumSIP_TLS_METHOD
    {
        /* SSL versions are not required
           ENUM_TLS_METHOD_SSL_V2 = 0,
           ENUM_TLS_METHOD_SSL_V3, */
        ENUM_TLS_METHOD_TLS_V1 = 2,
        ENUM_TLS_METHOD_TLS_V1_1,
        ENUM_TLS_METHOD_TLS_V1_2
    }

    /* IP_AUTHENTICATION | This structure is to configure SIP Digest Authentication 
     * authentication quadruplet.
     * 
     * version: version number of this structure
     *
     * realm : string that defines the protected domain
     *
     * identity: SIP URI string that uniquely identify the user in the realm.
     *
     * username : The user's name string in the specified realm
     *
     * password :  The user's password string associated to the user 
     *
     * The string pointer must not be NULL for realm, identity, username and password.
     */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct IP_AUTHENTICATION
    {
        public uint version; /* library use only, do not change value */
        [MarshalAs(UnmanagedType.LPStr)]
        public string realm; /* must be null terminated string */
        [MarshalAs(UnmanagedType.LPStr)]
        public string identity; /* must be null terminated string */
        [MarshalAs(UnmanagedType.LPStr)]
        public string username; /* must be null terminated string, ignored during remove */
        [MarshalAs(UnmanagedType.LPStr)]
        public string password; /* must be null terminated string, ignored during remove */
    }

    /* IP_REGISTER_ADDRESS | Used in gc_ReqService to specify address of gatekeeper
     *                       (H.323) or registrar (SIP). 
     * reg_client : 128 character local address of registering host
     *
     * reg_server : 64 character local address of gatekeeper (H.323) or registrar (SIP)
     *
     * time_to_live : unicast TTL in seconds
     * 
     * max_hops : multicast TTL in hops
     */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct IP_REGISTER_ADDRESS
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = gcip_defs_h.IP_REG_CLIENT_ADDR_LENGTH)]
        public string reg_client;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = gcip_defs_h.IP_REG_SERVER_ADDR_LENGTH)]
        public string reg_server;
        public int time_to_live;
        public int max_hops;
    }

    /*RTP_ADDR : for passing endpoint addresses (port and IP address) to app
    */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct RTP_ADDR
    {
        public int version;
        public ushort port;
        public byte ip_ver;
        public U_ipaddr u_ipaddr;
    }

    /* EXTENSIONEVTBLK passed in GCEV_EXTENSION events for feature transparency and extension info: */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct EXTENSIONEVTBLK
    {
        public byte ext_id;
        public GC_PARM_BLK parmblk;
    }


}
