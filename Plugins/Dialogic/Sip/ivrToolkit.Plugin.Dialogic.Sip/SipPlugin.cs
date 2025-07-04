﻿using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Extensions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Core.Util;
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
    private readonly ProcessExtension _processExtension;
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

        _boardEventListener = new ThreadedEventListener(_loggerFactory, _voiceProperties, new[] { _boardDev });

        _boardEventListener.OnMetaEvent += _boardEventListener_OnMetaEvent;
        _boardEventListener.Start();

        SetupGlobalCallParameterBlock();
        SetAuthenticationInfo();
        Register(3600); // 1 hour
    }

    private void _boardEventListener_OnMetaEvent(object sender, MetaEventArgs e)
    {
        var metaEvt = e.MetaEvent;
        _logger.LogDebug(
            "evt_type = {0}:{1}, evt_dev = {2}, evt_flags = {3},  line_dev = {4} ",
            metaEvt.evttype, metaEvt.evttype.EventTypeDescription(), metaEvt.evtdev, metaEvt.flags,
            metaEvt.linedev);

        switch (metaEvt.evttype)
        {
            case gclib_h.GCEV_SERVICERESP:
                HandleRegisterStuff(metaEvt);
                break;
            case gclib_h.GCEV_EXTENSION:
                _processExtension.HandleExtension(metaEvt); // todo some or all of this may not be for board events.
                break;
        }
    }

    IIvrBaseLine IIvrPlugin.GetLine(int lineNumber)
    {
        _logger.LogDebug("GetLine({0})", lineNumber);
        lineNumber.ThrowIfLessThanOrEqualTo(0, nameof(lineNumber));

        if (_disposed) throw new DisposedException("You cannot get a line from a disposed plugin");

        return new SipLine(_loggerFactory, _voiceProperties, lineNumber);
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
            if (_boardEventListener != null)
            {
                Register(0); // unregister
                Task.Delay(2000).GetAwaiter().GetResult(); // give it time to complete
                _boardEventListener.Dispose();
            }
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

        _logger.LogDebug("Deleting parameter block: 0x{paramBlock:X}", gcParmBlkPtr);
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

        _logger.LogDebug("Deleting the parameter block 0x{paramBlock:X}", gcParmBlkPtr);
        gclib_h.gc_util_delete_parm_blk(gcParmBlkPtr);
        _unmanagedMemoryService.Free(pData);
    }

    private void Register(int timeout)
    {
        _logger.LogDebug("Register({timeout})", timeout);

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

        if (timeout == 0)
        {
            // for unregister
            result = gclib_h.gc_util_insert_parm_val(ref gcParmBlkPtr, gcip_defs_h.IPSET_REG_INFO,
                gcip_defs_h.IPPARM_OPERATION_DEREGISTER, sizeof(byte), gcip_defs_h.IP_REG_DELETE_ALL);
        }
        else
        {
            // for register
            result = gclib_h.gc_util_insert_parm_val(ref gcParmBlkPtr, gcip_defs_h.IPSET_REG_INFO,
                gcip_defs_h.IPPARM_OPERATION_REGISTER, sizeof(byte), gcip_defs_h.IP_REG_SET_INFO);
        }
        result.ThrowIfGlobalCallError();

        var ipRegisterAddress = new IP_REGISTER_ADDRESS
        {
            reg_client = regClient, // me. example: "200@192.168.1.40"
            reg_server = regServer, // FreePBX. example: "192.168.1.40"
            time_to_live = timeout,
            max_hops = 30
        };

        var dataSizeRegister = (byte)Marshal.SizeOf<IP_REGISTER_ADDRESS>();

        var pDataRegister = _unmanagedMemoryService.Create(nameof(IP_REGISTER_ADDRESS), ipRegisterAddress);

        result = gclib_h.gc_util_insert_parm_ref(ref gcParmBlkPtr, gcip_defs_h.IPSET_REG_INFO,
            gcip_defs_h.IPPARM_REG_ADDRESS,
            dataSizeRegister, pDataRegister);
        result.ThrowIfGlobalCallError();
        _unmanagedMemoryService.Free(pDataRegister);

        // set up the contact
        var contact = $"{_voiceProperties.SipContact}\0"; // contact. example: {alias}@{proxy_ip}:{sip_signaling_port}
        var pContact = _unmanagedMemoryService.StringToHGlobalAnsi("pContact", contact);
        var dataSize = (byte)contact.Length;

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
        _logger.LogDebug("Deleting the parameter block 0x{paramBlock:X}", gcParmBlkPtr);
        gclib_h.gc_util_delete_parm_blk(gcParmBlkPtr);

        var eventWaitEnum = _boardEventListener.WaitForEvent(10); // wait for 10 seconds 
        _logger.LogDebug("Result for gc_ReqService is {0}", eventWaitEnum);

        _unmanagedMemoryService.Free(pContact);
    }
    
    private void HandleRegisterStuff(METAEVENT metaEvt)
    {
        _logger.LogDebug("HandleRegisterStuff(METAEVENT metaEvt)");
        var gcParmBlkp = metaEvt.extevtdatap;

        var parmData = gcip_h.CreateAndInitGcParmDataExt();

        var result = gclib_h.gc_util_next_parm_ex(gcParmBlkp, ref parmData);

        while (result == GcErr_h.GC_SUCCESS)
        {
            switch (parmData.set_ID)
            {
                case gcip_defs_h.IPSET_REG_INFO:
                    switch (parmData.parm_ID)
                    {
                        case gcip_defs_h.IPPARM_REG_STATUS:
                            {
                                var value = GetValueFromPtr(parmData.pData, parmData.data_size);
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
                                var value = GetValueFromPtr(parmData.pData, parmData.data_size);
                                _logger.LogDebug("    IPSET_REG_INFO/IPPARM_REG_SERVICEID: 0x{0:X}", value);
                                break;
                            }
                        default:
                            _logger.LogDebug(
                                "    Missed one: set_ID = IPSET_REG_INFO, parm_ID = {1:X}, bytes = {2}",
                                parmData.parm_ID, parmData.data_size);
                            break;
                    }

                    break;
                case gcip_defs_h.IPSET_PROTOCOL:
                    var value2 = GetValueFromPtr(parmData.pData, parmData.data_size);
                    _logger.LogDebug("    IPSET_PROTOCOL value: {0}", value2);
                    break;
                case gcip_defs_h.IPSET_LOCAL_ALIAS:
                    {
                        var localAlias = GetStringFromPtr(parmData.pData, (int)parmData.data_size);
                        _logger.LogDebug("    IPSET_LOCAL_ALIAS value: {0}", localAlias);
                        break;
                    }
                case gcip_defs_h.IPSET_SIP_MSGINFO:
                    {
                        var msgInfo = GetStringFromPtr(parmData.pData, (int)parmData.data_size);
                        _logger.LogDebug("    IPSET_SIP_MSGINFO value: {0}", msgInfo);
                        break;
                    }
                default:
                    _logger.LogDebug("    Missed one: set_ID = {0:X}, parm_ID = {1:X}, bytes = {2}",
                        parmData.set_ID, parmData.parm_ID, parmData.data_size);
                    break;
            }

            result = gclib_h.gc_util_next_parm_ex(gcParmBlkp, ref parmData);
        }
        if (result != GcErr_h.EGC_NO_MORE_PARMS)
        {
            result.LogIfGlobalCallError(_logger);
        }
    }

    private string GetStringFromPtr(IntPtr ptr, int size)
    {
        return Marshal.PtrToStringAnsi(ptr, size).TrimEnd('\0');
    }

    private int GetValueFromPtr(IntPtr ptr, uint size)
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