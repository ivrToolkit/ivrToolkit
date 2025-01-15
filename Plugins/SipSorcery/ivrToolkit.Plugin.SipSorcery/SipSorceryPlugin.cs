﻿using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Extensions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging;
using SIPSorcery.SIP;

namespace ivrToolkit.Plugin.SipSorcery;

public class SipSorceryPlugin : IIvrPlugin
{
    private readonly ILogger<SipSorceryPlugin> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly SipVoiceProperties _voiceProperties;
    private bool _disposed;
    private readonly SIPTransport _sipTransport;

    public SipSorceryPlugin(ILoggerFactory loggerFactory, SipVoiceProperties voiceProperties)
    {
        loggerFactory.ThrowIfNull(nameof(loggerFactory));
        voiceProperties.ThrowIfNull(nameof(voiceProperties));

        _loggerFactory = loggerFactory;
        _voiceProperties = voiceProperties;
        _logger = loggerFactory.CreateLogger<SipSorceryPlugin>();

        _logger.LogDebug("ctr()");

        var sipTransport = new SIPTransport();
        if (voiceProperties.SipTransportEnableTraceLogs) sipTransport.EnableTraceLogs();

        _sipTransport = sipTransport;
    }

    public VoiceProperties VoiceProperties => _voiceProperties;

    IIvrBaseLine IIvrPlugin.GetLine(int lineNumber)
    {
        _logger.LogDebug("GetLine({0})", lineNumber);
        lineNumber.ThrowIfLessThanOrEqualTo(0, nameof(lineNumber));

        if (_disposed) throw new DisposedException("You cannot get a line from a disposed plugin");

        return new SipSorceryLine(_loggerFactory, _voiceProperties, lineNumber, _sipTransport);
        //return new LineWrapper(_loggerFactory, _voiceProperties, lineNumber, line);
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
        _sipTransport.Dispose();
    }
}