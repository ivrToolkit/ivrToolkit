// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Core.Util;
using ivrToolkit.Plugin.Dialogic.Common;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Plugin.Dialogic.Analog;

/// <summary>
/// 
/// </summary>
public class AnalogPlugin : IIvrPlugin
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly DialogicVoiceProperties _voiceProperties;
    private readonly object _lockObject = new();
    private bool _initialized;
    private readonly ILogger<AnalogPlugin> _logger;

    public AnalogPlugin(ILoggerFactory loggerFactory, DialogicVoiceProperties voiceProperties)
    {
        _loggerFactory = loggerFactory;
        _voiceProperties = voiceProperties;
        _logger = loggerFactory.CreateLogger<AnalogPlugin>();
    }

    private void InitForDx()
    {
        _initialized = true;
    }

    private void InitForGc()
    {
        var result = GcLibDef.gc_Start(IntPtr.Zero);
        _logger.LogDebug("gc_start: {0}", result);
        if (result != 0)
        {
            ThrowError("Init().gc_start");
        }
        _initialized = true;

        var cclibStates = new GcLibDef.GC_CCLIB_STATUSALL();
        result = GcLibDef.gc_CCLibStatusEx("GC_ALL_LIB", ref cclibStates);
        _logger.LogDebug("gc_CCLibStatusEx: {0}", result);

        foreach (var cclibState in cclibStates.cclib_state)
        {
            _logger.LogDebug("{0}|{1}", cclibState.name, cclibState.state);
        }
            
    }

    private void ThrowError(string from)
    {
        var info = new GcLibDef.GC_INFO();
        _logger.LogDebug("About to find error for: {0}", from);
        var status = GcLibDef.gc_ErrorInfo(ref info);
        if (status != 0) throw new VoiceException($"Unknown error from {from}"); // should not happen

        var message =
            $"{info.gcValue}|{info.gcMsg}\r\n{info.ccValue}|{info.ccMsg}\r\n{info.ccLibId}|{info.ccLibName}";
        _logger.LogError(message);
        throw new VoiceException(message);
            
    }

    IIvrBaseLine IIvrPlugin.GetLine(int lineNumber)
    {
        lock (_lockObject)
        {
            if (!_initialized)
            {
                if (_voiceProperties.UseGc)
                {
                    InitForGc();
                }
                else
                {
                    InitForDx();
                }
            }
        }

        var deviceName = GetDeviceName(lineNumber);

        var handles = _voiceProperties.UseGc ? OpenDeviceWithGc(deviceName): OpenDeviceWithDx(deviceName);
        return new AnalogLine(_loggerFactory, _voiceProperties, handles.Devh, handles.Voiceh, lineNumber);
    }

    public event Func<IIvrBaseLine, CancellationToken, Task> OnInboundCall;

    public VoiceProperties VoiceProperties => _voiceProperties;

    private string GetDeviceName(int lineNumber)
    {
        var pattern = _voiceProperties.DeviceNamePattern;

        var board = ((lineNumber - 1) / 4) + 1;
        var channel = (lineNumber - (board - 1) * 4);

        pattern = pattern.Replace("{board}", board.ToString(CultureInfo.InvariantCulture))
            .Replace("{channel}", channel.ToString(CultureInfo.InvariantCulture))
            .Replace("{line}",lineNumber.ToString(CultureInfo.InvariantCulture));

        return pattern;
    }

    private class Handles
    {
        public int Devh;
        public int Voiceh;
    }

    /// <summary>
    /// Opens the board line.
    /// </summary>
    /// <param name="devname">Name of the board line. For example: dxxxB1C1</param>
    /// <returns>The device handle</returns>
    private Handles OpenDeviceWithDx(string devname)
    {
        _logger.LogDebug("OpenDeviceWithDx({0})", devname);
        var devh = DialogicDef.dx_open(devname, 0);
        if (devh <= -1)
        {
            //var err = string.Format("Could not get device handle for device {0}", devname);
            var err = DialogicDef.ATDV_ERRMSGP(devh);
            _logger.LogDebug("Error is: {0}", err);
            throw new VoiceException(err);
        }
        return new Handles
        {
            Devh = devh,
            Voiceh = devh
        };
    }

    private Handles OpenDeviceWithGc(string devname)
    {
        _logger.LogDebug("OpenDeviceWithGc({0})", devname);
        var devh = 0;
        var result = GcLibDef.gc_OpenEx(ref devh, devname, DialogicDef.EV_SYNC, IntPtr.Zero);

        if (result != 0)
        {
            ThrowError("OpenDevice().gc_OpenEx");
        }


        var voiceh = 0;
        result = GcLibDef.gc_GetResourceH(devh, ref voiceh, GcLibDef.GC_VOICEDEVICE);

        if (result != 0)
        {
            ThrowError("gOpenDevice().gc_GetResourceH");
        }

        return new Handles
        {
            Devh = devh,
            Voiceh = voiceh
        };
    }


    public void Dispose()
    {
        _logger.LogDebug("Dispose()");
    }

} // class
// namespace
