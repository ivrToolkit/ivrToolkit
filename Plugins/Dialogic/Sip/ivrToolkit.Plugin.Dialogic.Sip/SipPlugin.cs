using System;
using ivrToolkit.Core;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Extensions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Core.Util;
using ivrToolkit.Plugin.Dialogic.Common.DialogicDefs;
using ivrToolkit.Plugin.Dialogic.Common.Extensions;
using Microsoft.Extensions.Logging;

// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

namespace ivrToolkit.Plugin.Dialogic.Sip
{
    public class SipPlugin : IIvrPlugin
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly DialogicSipVoiceProperties _voiceProperties;
        private UnmanagedMemoryService _unmanagedMemoryService;
        private readonly ILogger<SipPlugin> _logger;
        private bool _disposed;

        public SipPlugin(ILoggerFactory loggerFactory, DialogicSipVoiceProperties voiceProperties)
        {
            loggerFactory.ThrowIfNull(nameof(loggerFactory));
            voiceProperties.ThrowIfNull(nameof(voiceProperties));

            _loggerFactory = loggerFactory;
            _voiceProperties = voiceProperties;
            _logger = loggerFactory.CreateLogger<SipPlugin>();
            _logger.LogDebug("ctr()");

            Start();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                _logger.LogWarning("Dispose() - Already Disposed");
                return;
            }
            _logger.LogDebug("Dispose()");

            try
            {
                var result = gclib_h.gc_Stop();
                result.ThrowIfGlobalCallError();

                _disposed = false;
            }
            finally
            {
                _unmanagedMemoryService?.Dispose();
                _unmanagedMemoryService = null;
            }
        }

        public ILine GetLine(int lineNumber)
        {
            lineNumber.ThrowIfLessThanOrEqualTo(0, nameof(lineNumber));

            if (_disposed) throw new VoiceException("You cannot get a line from a disposed plugin");

            _logger.LogDebug("GetLine({0})", lineNumber);
            var line = new SipLine(_loggerFactory, _voiceProperties, lineNumber);
            return new LineWrapper(_loggerFactory, lineNumber, line);
        }

        public VoiceProperties VoiceProperties => _voiceProperties;

        private void Start()
        {
            _logger.LogDebug("Start()");

            _unmanagedMemoryService = new UnmanagedMemoryService(_loggerFactory);

            // show some info
            var deviceInformation = new DeviceInformation(_loggerFactory);
            deviceInformation.LogCclibsStatus();
            deviceInformation.LogDeviceInformation();

            // some profile options need to be read in
            var h323SignalingPort = _voiceProperties.SipH323SignalingPort;
            var sipSignalingPort = _voiceProperties.SipSignalingPort;
            var maxCalls = _voiceProperties.MaxCalls;

            // define the virt boards
            var ipVirtboard = new[] { gcip_h.CreateAndInitIpVirtboard() };

            ipVirtboard[0].localIP.ip_ver = gcip_defs_h.IPVER4; // must be set to IPVER4
            ipVirtboard[0].localIP.u_ipaddr.ipv4 = gcip_defs_h.IP_CFG_DEFAULT; // or specify host NIC IP address

            ipVirtboard[0].h323_signaling_port = h323SignalingPort; // or application defined port for H.323 
            ipVirtboard[0].sip_signaling_port = sipSignalingPort; // or application defined port for SIP
            ipVirtboard[0].sup_serv_mask = gcip_defs_h.IP_SUP_SERV_CALL_XFER; // Enable SIP Transfer Feature
            ipVirtboard[0].sip_msginfo_mask = gcip_defs_h.IP_SIP_MSGINFO_ENABLE; // Enable SIP header
            ipVirtboard[0].reserved = IntPtr.Zero; // must be set to NULL

            ipVirtboard[0].sip_max_calls = maxCalls;
            ipVirtboard[0].h323_max_calls = maxCalls;
            ipVirtboard[0].total_max_calls = maxCalls;


            var ipcclibStartData = gcip_h.CreateAndInitIpcclibStartData();
            ipcclibStartData.max_parm_data_size = 4096;

            ipcclibStartData.num_boards = 1;
            ipcclibStartData.board_list = _unmanagedMemoryService.Create(ipVirtboard[0]);

            CCLIB_START_STRUCT[] ccLibStartStructs =
            {
                new() {cclib_name = "GC_DM3CC_LIB", cclib_data = IntPtr.Zero},
                new() {cclib_name = "GC_H3R_LIB", cclib_data = _unmanagedMemoryService.Create(ipcclibStartData)},
                new() {cclib_name = "GC_IPM_LIB", cclib_data = IntPtr.Zero}
            };

            var gclibStart = new GC_START_STRUCT
            {
                num_cclibs = ccLibStartStructs.Length,
                cclib_list = _unmanagedMemoryService.Create(ccLibStartStructs)
            };

            _logger.LogDebug("Calling gclib_h.gc_Start()...");
            var result = gclib_h.gc_Start(_unmanagedMemoryService.Create(gclibStart));
            result.ThrowIfGlobalCallError();

            // show some info
            deviceInformation.LogDeviceInformationAfterGcStart();
        }

    }

}
