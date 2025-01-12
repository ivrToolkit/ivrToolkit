using System;
using ivrToolkit.Plugin.Dialogic.Common.DialogicDefs;
using ivrToolkit.Plugin.Dialogic.Common.Extensions;
using Microsoft.Extensions.Logging;
// ReSharper disable StringLiteralTypo

namespace ivrToolkit.Plugin.Dialogic.Sip;

public class DeviceInformation
{
    private readonly ILogger<DeviceInformation> _logger;

    public DeviceInformation(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<DeviceInformation>();
        _logger.LogDebug("ctr(ILoggerFactory)");
    }
    /**
    * Enumerate the HMP Device
    * This can be used to check the device name and numbers
    * That are part of an HMP License.
    * This function can help when a channel cannot be opened possibly due to
    * the name or numbers on the license.
    */
    internal void LogDeviceInformation()
    {
        _logger.LogInformation("LogDeviceInformation()");
        LogVoiceBoard();
        LogDtiBoard();
        LogMsiBoard();
        LogDcbBoard();
        LogIptBoard();
        LogIpmBoard();
    }

    internal void LogCclibsStatus()
    {
        var cclibStatus = 0;

        //GC_INFO gcErrorInfo; /* GlobalCall error information data */
        var result = gclib_h.gc_CCLibStatus("GC_DM3CC_LIB", ref cclibStatus);
        result.ThrowIfGlobalCallError();
        if (result == 0)
        {
            _logger.LogInformation("cclib {0} status:", "GC_DM3CC_LIB");
            _logger.LogInformation("   configured: {0}", (cclibStatus & gclib_h.GC_CCLIB_CONFIGURED) != 0 ? "yes" : "no");
            _logger.LogInformation("   available: {0}", (cclibStatus & gclib_h.GC_CCLIB_AVL) != 0 ? "yes" : "no");
            _logger.LogInformation("   failed: {0}", (cclibStatus & gclib_h.GC_CCLIB_FAILED) != 0 ? "yes" : "no");
            _logger.LogInformation("   stub: {0}", (cclibStatus & gclib_h.GC_CCLIB_STUB) != 0 ? "yes" : "no");
        }
    }


    private void LogIpmBoard()
    {
        var boardCount = 0;
        srllib_h.sr_getboardcnt("IPM", ref boardCount);
        _logger.LogInformation("    ipm board count={0}.", boardCount);
        for (var i = 1; i <= boardCount; i++)
        {
            var boardName = $":M_ipmB{i}";

            var handle = 0;

            var result = gclib_h.gc_OpenEx(ref handle, boardName, dtilib_h.EV_ASYNC, IntPtr.Zero);
            // can only get detail if gc started
            if (result != -1)
            {
                // always returns -1 and the same on the c++ version too. I think it is because gc_start needs to be done first
                _logger.LogInformation("        gc_openEx({0}, {1}) = {2}", handle, boardName, result);

                var subDevCount = srllib_h.ATDV_SUBDEVS(handle);
                _logger.LogInformation("        ipm board {0}(handle={1}) has {2} sub-devs.", i, handle, subDevCount);
                gclib_h.gc_Close(handle);
            }
        }
    }

    private void LogIptBoard()
    {
        var boardCount = 0;
        srllib_h.sr_getboardcnt(gcip_defs_h.DEV_CLASS_IPT, ref boardCount);
        _logger.LogInformation("    ipt board count={0}.", boardCount);
        for (var i = 1; i <= boardCount; i++)
        {
            var boardName = $":N_iptB{i}:P_IP";
            var handle = 0;
            var result = gclib_h.gc_OpenEx(ref handle, boardName, dtilib_h.EV_ASYNC, IntPtr.Zero);

            // can only get detail if gc started
            if (result != -1)
            {
                // always returns -1 and the same on the c++ version too. I think it is because gc_start needs to be done first
                _logger.LogInformation("        gc_openEx({0}, {1}) = {2}", handle, boardName, result);

                var subDevCount = srllib_h.ATDV_SUBDEVS(handle);
                _logger.LogInformation("        ipt board {0}(handle={1}) has {2} sub-devs.", i, handle, subDevCount);
                gclib_h.gc_Close(handle);
            }

        }
    }

    private void LogDcbBoard()
    {
        var boardCount = 0;
        srllib_h.sr_getboardcnt(dcblib_h.DEV_CLASS_DCB, ref boardCount);
        _logger.LogInformation("    dcb board count={0}.", boardCount);
        for (var i = 1; i <= boardCount; i++)
        {
            var boardName = $"dcbB{i}";
            var handle = dcblib_h.dcb_open(boardName, 0);
            var subDevCount = srllib_h.ATDV_SUBDEVS(handle);
            _logger.LogInformation("        dcb board {0} has {1} sub-devs(DSP).", i, subDevCount);
            for (var j = 1; j <= subDevCount; j++)
            {
                var devName = $"{boardName}D{j}";
                var devHandle = dcblib_h.dcb_open(devName, 0);
                var dspResourceCount = 0;
                dcblib_h.dcb_dsprescount(devHandle, ref dspResourceCount);
                _logger.LogInformation("            DSP {0} has {1} conference resource.", devName, dspResourceCount);
                dcblib_h.dcb_close(devHandle);
            }
            dcblib_h.dcb_close(handle);
        }
    }

    private void LogMsiBoard()
    {
        var boardCount = 0;
        srllib_h.sr_getboardcnt(msilib_h.DEV_CLASS_MSI, ref boardCount);
        _logger.LogInformation("    msi board count={0}.", boardCount);
        for (var i = 1; i <= boardCount; i++)
        {
            var boardName = $"msiB{i}";
            var handle = msilib_h.ms_open(boardName, 0);
            var subDevCount = srllib_h.ATDV_SUBDEVS(handle);

            _logger.LogInformation("        msi board {0} has {1} sub-devs.", i, subDevCount);
            msilib_h.ms_close(handle);
        }
    }

    private void LogDtiBoard()
    {
        var boardCount = 0;
        srllib_h.sr_getboardcnt(dtilib_h.DEV_CLASS_DTI, ref boardCount);
        _logger.LogInformation("    dti board count={0}.", boardCount);
        for (var i = 1; i <= boardCount; i++)
        {
            var boardName = $"dtiB{i}";
            var handle = dtilib_h.dt_open(boardName, 0);
            var subDevCount = srllib_h.ATDV_SUBDEVS(handle);

            _logger.LogInformation("        dti board {0} has {1} sub-devs.", i, subDevCount);
            dtilib_h.dt_close(handle);
        }
    }

    private void LogVoiceBoard()
    {
        var featureTable = new FEATURE_TABLE();
        var boardCount = 0;
        srllib_h.sr_getboardcnt(DXXXLIB_H.DEV_CLASS_VOICE, ref boardCount);
        _logger.LogInformation("    voice board count={0}.", boardCount);

        for (var i = 1; i <= boardCount; i++)
        {
            var boardName = $"dxxxB{i}";
            var handle = DXXXLIB_H.dx_open(boardName, 0);
            var subDevCount = srllib_h.ATDV_SUBDEVS(handle);

            _logger.LogInformation("        voice board {0} has {1} sub-devs.", i, subDevCount);
            for (var j = 1; j <= subDevCount; j++)
            {
                var devName = $"dxxxB{i}C{j}";
                var devHandle = DXXXLIB_H.dx_open(devName, 0);
                DXXXLIB_H.dx_getfeaturelist(devHandle, ref featureTable);

                _logger.LogInformation("            {0} {1}support fax, {2}support T38 fax, {3}support CSP.", devName,
                    (featureTable.ft_fax & DXXXLIB_H.FT_FAX) != 0 ? "" : "NOT ",
                    (featureTable.ft_fax & DXXXLIB_H.FT_FAX_T38UDP) != 0 ? "" : "NOT ",
                    (featureTable.ft_e2p_brd_cfg & DXXXLIB_H.FT_CSP) != 0 ? "" : "NOT ");

                DXXXLIB_H.dx_close(devHandle);
            }
            DXXXLIB_H.dx_close(handle);
        }
    }
}