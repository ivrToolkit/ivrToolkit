using System;
using System.Runtime.InteropServices;
using ivrToolkit.Core;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Extensions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Plugin.Dialogic.Common;
using ivrToolkit.Plugin.Dialogic.Common.DialogicDefs;
using ivrToolkit.Plugin.Dialogic.Common.Extensions;
using ivrToolkit.Plugin.Dialogic.Common.Listeners;
using Microsoft.Extensions.Logging;

// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

namespace ivrToolkit.Plugin.Dialogic.Sip;

public class SipPlugin : IIvrPlugin
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly DialogicSipVoiceProperties _voiceProperties;
    private UnmanagedMemoryService _unmanagedMemoryService;
    private readonly ILogger<SipPlugin> _logger;
    private bool _disposed;
    private int _boardDev;
    private IEventListener _boardEventListener;
    private ProcessExtension _processExtension;
    public VoiceProperties VoiceProperties => _voiceProperties;

    public SipPlugin(ILoggerFactory loggerFactory, DialogicSipVoiceProperties voiceProperties)
    {
        loggerFactory.ThrowIfNull(nameof(loggerFactory));
        voiceProperties.ThrowIfNull(nameof(voiceProperties));

        _loggerFactory = loggerFactory;
        _voiceProperties = voiceProperties;
        _logger = loggerFactory.CreateLogger<SipPlugin>();
        _processExtension = new ProcessExtension(loggerFactory);

        _logger.LogDebug("ctr()");

        Start();
    }

    private void Start()
    {
        // show some info
        var deviceInformation = new DeviceInformation(_loggerFactory);
        deviceInformation.LogCclibsStatus();
        deviceInformation.LogDeviceInformation();

        StartSip();
        OpenBoard();

        // todo replace this with a factory to choose ThreadedEventListener vs SynchronousEventListener
        //_boardEventListener = new SynchronousEventListener(_loggerFactory, new[] { _boardDev });
        _boardEventListener = new ThreadedEventListener(_loggerFactory, _voiceProperties, new[] { _boardDev });

        _boardEventListener.OnMetaEvent += _boardEventListener_OnMetaEvent;
        _boardEventListener.Start();

        SetupGlobalCallParameterBlock();
        SetAuthenticationInfo();
        Register();
    }

    private void _boardEventListener_OnMetaEvent(object sender, MetaEventArgs e)
    {
        var metaEvt = e.MetaEvent;
        _logger.LogDebug(
            "evt_type = {0}:{1}, evt_dev = {2}, evt_flags = {3},  line_dev = {4} ",
            metaEvt.evttype, metaEvt.evttype.EventTypeDescription(), metaEvt.evtdev, metaEvt.flags,
            metaEvt.linedev);
        HandleEvent(metaEvt);
    }

    public IIvrLine GetLine(int lineNumber)
    {
        _logger.LogDebug("GetLine({0})", lineNumber);
        lineNumber.ThrowIfLessThanOrEqualTo(0, nameof(lineNumber));

        if (_disposed) throw new DisposedException("You cannot get a line from a disposed plugin");

        var line = new SipLine(_loggerFactory, _voiceProperties, lineNumber);
        return new LineWrapper(_loggerFactory, _voiceProperties, lineNumber, line);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            _logger.LogWarning("Dispose() - Already Disposed");
            return;
        }
        _logger.LogDebug("Dispose()");
        _disposed = true;


        try
        {
            _boardEventListener?.Dispose();
            var result = gclib_h.gc_Close(_boardDev);
            result.ThrowIfGlobalCallError();

        }
        finally
        {
            _unmanagedMemoryService?.Dispose();
            _unmanagedMemoryService = null;
        }
    }

    private void StartSip()
    {
        var sipSignalingPort = _voiceProperties.SipSignalingPort;
        var maxCalls = _voiceProperties.MaxCalls;

        _logger.LogDebug("Start() - sipSignalingPort = {0}, maxCalls = {1}", sipSignalingPort, maxCalls);

        _unmanagedMemoryService = new UnmanagedMemoryService(_loggerFactory, $"Lifetime of {nameof(SipPlugin)}");

        // define the virt boards
        var ipVirtboard = new[] { gcip_h.CreateAndInitIpVirtboard() };

        ipVirtboard[0].localIP.ip_ver = gcip_defs_h.IPVER4; // must be set to IPVER4
        ipVirtboard[0].localIP.u_ipaddr.ipv4 = gcip_defs_h.IP_CFG_DEFAULT; // or specify host NIC IP address

        ipVirtboard[0].sip_signaling_port = sipSignalingPort; // or application defined port for SIP
        ipVirtboard[0].sup_serv_mask = gcip_defs_h.IP_SUP_SERV_CALL_XFER; // Enable SIP Transfer Feature
        ipVirtboard[0].sip_msginfo_mask = gcip_defs_h.IP_SIP_MSGINFO_ENABLE | gcip_defs_h.IP_SIP_MIME_ENABLE; // Enable SIP header
        ipVirtboard[0].reserved = IntPtr.Zero; // must be set to NULL

        ipVirtboard[0].sip_max_calls = maxCalls;
        ipVirtboard[0].h323_max_calls = 0;
        ipVirtboard[0].total_max_calls = maxCalls;


        var ipcclibStartData = gcip_h.CreateAndInitIpcclibStartData();
        ipcclibStartData.max_parm_data_size = 4096;

        ipcclibStartData.num_boards = 1;
        ipcclibStartData.board_list = _unmanagedMemoryService.Create(nameof(IP_VIRTBOARD), ipVirtboard[0]);

        CCLIB_START_STRUCT[] ccLibStartStructs =
        {
            new() {cclib_name = "GC_DM3CC_LIB", cclib_data = IntPtr.Zero},
            new() {cclib_name = "GC_H3R_LIB", cclib_data = _unmanagedMemoryService.Create(nameof(IPCCLIB_START_DATA), ipcclibStartData)},
            new() {cclib_name = "GC_IPM_LIB", cclib_data = IntPtr.Zero}
        };

        var gclibStart = new GC_START_STRUCT
        {
            num_cclibs = ccLibStartStructs.Length,
            cclib_list = _unmanagedMemoryService.Create(nameof(CCLIB_START_STRUCT), ccLibStartStructs)
        };

        _logger.LogDebug("Calling gclib_h.gc_Start()...");
        var result = gclib_h.gc_Start(_unmanagedMemoryService.Create(nameof(GC_START_STRUCT), gclibStart));
        result.ThrowIfGlobalCallError();

    }

    private void OpenBoard()
    {
        _logger.LogDebug("Calling gclib_h.gc_OpenEx...");
        var result = gclib_h.gc_OpenEx(ref _boardDev, ":N_iptB1:P_IP", DXXXLIB_H.EV_SYNC, IntPtr.Zero);
        _logger.LogDebug(
            "get _boardDev: result = {0} = gc_openEx([ref]{1}, :N_iptB1:P_IP, EV_SYNC, IntPtr.Zero)...", result,
            _boardDev);
        result.ThrowIfGlobalCallError();
    }

    private void SetupGlobalCallParameterBlock()
    {
        _logger.LogDebug("SetupGlobalCallParameterBlock() - _boardDev = {0}", _boardDev);
        var gcParmBlkPtr = IntPtr.Zero;

        //setting T.38 fax server operating mode: IP MANUAL mode
        var result = gclib_h.gc_util_insert_parm_val(ref gcParmBlkPtr, gcip_defs_h.IPSET_CONFIG,
            gcip_defs_h.IPPARM_OPERATING_MODE, sizeof(int), gcip_defs_h.IP_MANUAL_MODE);
        result.ThrowIfGlobalCallError();

        //Enabling and Disabling Unsolicited Notification Events
        result = gclib_h.gc_util_insert_parm_val(ref gcParmBlkPtr, gcip_defs_h.IPSET_EXTENSIONEVT_MSK,
            gclib_h.GCACT_ADDMSK, sizeof(int),
            gcip_defs_h.EXTENSIONEVT_DTMF_ALPHANUMERIC | gcip_defs_h.EXTENSIONEVT_SIGNALING_STATUS |
            gcip_defs_h.EXTENSIONEVT_STREAMING_STATUS | gcip_defs_h.EXTENSIONEVT_T38_STATUS);
        result.ThrowIfGlobalCallError();

        _boardEventListener.SetEventToWaitFor(gclib_h.GCEV_SETCONFIGDATA);

        var requestId = 0;
        result = gclib_h.gc_SetConfigData(gclib_h.GCTGT_CCLIB_NETIF, _boardDev, gcParmBlkPtr, 0,
            gclib_h.GCUPDATE_IMMEDIATE, ref requestId, DXXXLIB_H.EV_ASYNC);
        result.ThrowIfGlobalCallError();

        gclib_h.gc_util_delete_parm_blk(gcParmBlkPtr);

        var eventWait = _boardEventListener.WaitForEvent(10); // wait for 10 seconds
        switch (eventWait)
        {
            case EventWaitEnum.Expired:
                _logger.LogError("gc_SetConfigData has expired");
                break;
            case EventWaitEnum.Success:
                _logger.LogDebug("gc_SetConfigData was a success");
                break;
            case EventWaitEnum.Error:
                _logger.LogError("gc_SetConfigData has failed");
                break;
        }
    }

    private void SetAuthenticationInfo()
    {
        _logger.LogDebug("SetAuthenticationInfo()");
        var proxy = _voiceProperties.SipProxyIp;
        var alias = _voiceProperties.SipAlias;

        var password = _voiceProperties.SipPassword;
        var realm = _voiceProperties.SipRealm;

        var identity = $"sip:{alias}@{proxy}";

        _logger.LogDebug("SetAuthenticationInfo() - proxy = {0}, alias = {1}, Password ****, Realm = {2}, identity = {3}", proxy,
            alias, realm, identity);

        var auth = new IP_AUTHENTICATION
        {
            version = gcip_h.IP_AUTHENTICATION_VERSION,
            realm = realm,
            identity = identity,
            username = alias,
            password = password
        };

        var gcParmBlkPtr = IntPtr.Zero;
        var dataSize = (byte)Marshal.SizeOf<IP_AUTHENTICATION>();

        var pData = _unmanagedMemoryService.Create(nameof(IP_AUTHENTICATION), auth);

        var result = gclib_h.gc_util_insert_parm_ref(ref gcParmBlkPtr, gcip_defs_h.IPSET_CONFIG,
            gcip_defs_h.IPPARM_AUTHENTICATION_CONFIGURE, dataSize, pData);
        result.ThrowIfGlobalCallError();

        result = gclib_h.gc_SetAuthenticationInfo(gclib_h.GCTGT_CCLIB_NETIF, _boardDev, gcParmBlkPtr);
        result.ThrowIfGlobalCallError();

        gclib_h.gc_util_delete_parm_blk(gcParmBlkPtr);
        _unmanagedMemoryService.Free(pData);
    }

    private void Register()
    {
        _logger.LogDebug("Register()");

        var proxy = _voiceProperties.SipProxyIp;
        var alias = _voiceProperties.SipAlias;

        var regServer = $"{proxy}"; // Request-URI
        var regClient = $"{alias}@{proxy}";

        _logger.LogDebug("Register() - regServer = {0}, regClient = {1}", regServer,
            regClient);

        var gcParmBlkPtr = IntPtr.Zero;

        var result = gclib_h.gc_util_insert_parm_val(ref gcParmBlkPtr, gccfgparm_h.GCSET_SERVREQ,
            gccfgparm_h.PARM_REQTYPE, sizeof(byte), gcip_defs_h.IP_REQTYPE_REGISTRATION);
        result.ThrowIfGlobalCallError();

        result = gclib_h.gc_util_insert_parm_val(ref gcParmBlkPtr, gccfgparm_h.GCSET_SERVREQ, gccfgparm_h.PARM_ACK,
            sizeof(byte), gcip_defs_h.IP_REQTYPE_REGISTRATION);
        result.ThrowIfGlobalCallError();

        result = gclib_h.gc_util_insert_parm_val(ref gcParmBlkPtr, gcip_defs_h.IPSET_PROTOCOL,
            gcip_defs_h.IPPARM_PROTOCOL_BITMASK, sizeof(byte), gcip_defs_h.IP_PROTOCOL_SIP);
        result.ThrowIfGlobalCallError();

        result = gclib_h.gc_util_insert_parm_val(ref gcParmBlkPtr, gcip_defs_h.IPSET_REG_INFO,
            gcip_defs_h.IPPARM_OPERATION_REGISTER, sizeof(byte), gcip_defs_h.IP_REG_SET_INFO);
        result.ThrowIfGlobalCallError();

        var ipRegisterAddress = new IP_REGISTER_ADDRESS
        {
            reg_client = regClient, // me. example: "200@192.168.1.40"
            reg_server = regServer, // FreePBX. example: "192.168.1.40"
            time_to_live = 3600, // 1 hour
            max_hops = 30
        };

        var dataSize = (byte)Marshal.SizeOf<IP_REGISTER_ADDRESS>();

        var pData = _unmanagedMemoryService.Create(nameof(IP_REGISTER_ADDRESS), ipRegisterAddress);

        result = gclib_h.gc_util_insert_parm_ref(ref gcParmBlkPtr, gcip_defs_h.IPSET_REG_INFO,
            gcip_defs_h.IPPARM_REG_ADDRESS,
            dataSize, pData);
        result.ThrowIfGlobalCallError();

        // set up the contact
        var contact = $"{_voiceProperties.SipContact}\0"; // contact. example: {alias}@{proxy_ip}:{sip_signaling_port}
        var pContact = _unmanagedMemoryService.StringToHGlobalAnsi("pContact", contact);
        dataSize = (byte)contact.Length;

        result = gclib_h.gc_util_insert_parm_ref(ref gcParmBlkPtr, gcip_defs_h.IPSET_LOCAL_ALIAS,
            gcip_defs_h.IPPARM_ADDRESS_TRANSPARENT, dataSize, pContact);
        result.ThrowIfGlobalCallError();

        uint serviceId = 1;

        var respDataPp = IntPtr.Zero;

        _boardEventListener.SetEventToWaitFor(gclib_h.GCEV_SERVICERESP);
        _logger.LogDebug("Register() - about to call gc_ReqService asynchronously");
        result = gclib_h.gc_ReqService(gclib_h.GCTGT_CCLIB_NETIF, _boardDev, ref serviceId, gcParmBlkPtr,
            ref respDataPp,
            DXXXLIB_H.EV_ASYNC);
        result.ThrowIfGlobalCallError();
        _logger.LogDebug("Register() - called gc_ReqService asynchronously");
        gclib_h.gc_util_delete_parm_blk(gcParmBlkPtr);

        var eventWaitEnum = _boardEventListener.WaitForEvent(10); // wait for 10 seconds 
        _logger.LogDebug("Result for gc_ReqService is {0}", eventWaitEnum);

        _unmanagedMemoryService.Free(pContact);
        _unmanagedMemoryService.Free(pData);

    }

    private void HandleEvent(METAEVENT metaEvt)
    {
        switch (metaEvt.evttype)
        {
            case gclib_h.GCEV_SERVICERESP:
                _logger.LogDebug("GCEV_SERVICERESP");
                HandleRegisterStuff(metaEvt);
                break;
            case gclib_h.GCEV_EXTENSION:
                _logger.LogDebug("GCEV_EXTENSION");
                _processExtension.HandleExtension(metaEvt);
                break;
            case gclib_h.GCEV_EXTENSIONCMPLT:
                _logger.LogDebug("GCEV_EXTENSIONCMPLT");
                _processExtension.HandleExtension(metaEvt);
                break;
        }
    }

    private void HandleRegisterStuff(METAEVENT metaEvt)
    {
        _logger.LogDebug("HandleRegisterStuff(METAEVENT metaEvt)");
        var gcParmBlkp = metaEvt.extevtdatap;
        var parmDatap = IntPtr.Zero;

        parmDatap = gclib_h.gc_util_next_parm(gcParmBlkp, parmDatap);

        while (parmDatap != IntPtr.Zero)
        {
            var parmData = Marshal.PtrToStructure<GC_PARM_DATA>(parmDatap);

            switch (parmData.set_ID)
            {
                case gcip_defs_h.IPSET_REG_INFO:
                    switch (parmData.parm_ID)
                    {
                        case gcip_defs_h.IPPARM_REG_STATUS:
                            {
                                var value = GetValueFromPtr(parmDatap + 5, parmData.value_size);
                                switch (value)
                                {
                                    case gcip_defs_h.IP_REG_CONFIRMED:
                                        _logger.LogDebug("    IPSET_REG_INFO/IPPARM_REG_STATUS: IP_REG_CONFIRMED");
                                        break;
                                    case gcip_defs_h.IP_REG_REJECTED:
                                        _logger.LogDebug("    IPSET_REG_INFO/IPPARM_REG_STATUS: IP_REG_REJECTED");
                                        break;
                                    default:
                                        _logger.LogDebug("    IPSET_REG_INFO/IPPARM_REG_STATUS: UNKNOWN {value}", value);
                                        break;
                                }

                                break;
                            }
                        case gcip_defs_h.IPPARM_REG_SERVICEID:
                            {
                                var value = GetValueFromPtr(parmDatap + 5, parmData.value_size);
                                _logger.LogDebug("    IPSET_REG_INFO/IPPARM_REG_SERVICEID: 0x{0:X}", value);
                                break;
                            }
                        default:
                            _logger.LogDebug(
                                "    Missed one: set_ID = IPSET_REG_INFO, parm_ID = {1:X}, bytes = {2}",
                                parmData.parm_ID, parmData.value_size);
                            break;
                    }

                    break;
                case gcip_defs_h.IPSET_PROTOCOL:
                    var value2 = GetValueFromPtr(parmDatap + 5, parmData.value_size);
                    _logger.LogDebug("    IPSET_PROTOCOL value: {0}", value2);
                    break;
                case gcip_defs_h.IPSET_LOCAL_ALIAS:
                    {
                        var localAlias = GetStringFromPtr(parmDatap + 5, parmData.value_size);
                        _logger.LogDebug("    IPSET_LOCAL_ALIAS value: {0}", localAlias);
                        break;
                    }
                case gcip_defs_h.IPSET_SIP_MSGINFO:
                    {
                        var msgInfo = GetStringFromPtr(parmDatap + 5, parmData.value_size);
                        _logger.LogDebug("    IPSET_SIP_MSGINFO value: {0}", msgInfo);
                        break;
                    }
                default:
                    _logger.LogDebug("    Missed one: set_ID = {0:X}, parm_ID = {1:X}, bytes = {2}",
                        parmData.set_ID, parmData.parm_ID, parmData.value_size);
                    break;
            }

            parmDatap = gclib_h.gc_util_next_parm(gcParmBlkp, parmDatap);
        }
        gclib_h.gc_util_delete_parm_blk(gcParmBlkp);
    }

    private string GetStringFromPtr(IntPtr ptr, int size)
    {
        return Marshal.PtrToStringAnsi(ptr, size).TrimEnd('\0'); ;
    }

    private int GetValueFromPtr(IntPtr ptr, byte size)
    {
        int value;
        switch (size)
        {
            case 1:
                value = Marshal.ReadByte(ptr);
                break;
            case 2:
                value = Marshal.ReadInt16(ptr);
                break;
            case 4:
                value = Marshal.ReadInt32(ptr);
                break;
            default:
                throw new VoiceException($"Unable to get value from ptr. Size is {size}");
        }

        return value;
    }

}