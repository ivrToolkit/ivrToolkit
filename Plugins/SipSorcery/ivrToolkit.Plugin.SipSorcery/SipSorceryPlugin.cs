using System.Net;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Extensions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging;
using SIPSorcery.SIP;
using SIPSorcery.SIP.App;

namespace ivrToolkit.Plugin.SipSorcery;

public class SipSorceryPlugin : IIvrPlugin
{
    private readonly ILogger<SipSorceryPlugin> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly SipVoiceProperties _voiceProperties;
    private bool _disposed;
    private readonly SIPTransport _sipTransport;
    private readonly InviteManager _inviteManager;
    private SIPRegistrationUserAgent? _sipRegistrar;

    public SipSorceryPlugin(ILoggerFactory loggerFactory, SipVoiceProperties voiceProperties)
    {
        loggerFactory.ThrowIfNull(nameof(loggerFactory));
        voiceProperties.ThrowIfNull(nameof(voiceProperties));
        
        _inviteManager = new InviteManager(loggerFactory);

        _loggerFactory = loggerFactory;
        _voiceProperties = voiceProperties;
        _logger = loggerFactory.CreateLogger<SipSorceryPlugin>();

        _logger.LogDebug("ctr()");

        var localEndpoint = _voiceProperties.SipLocalEndpoint.Split(":");
        if (localEndpoint.Length != 2) throw new VoiceException("LocalEndpoint is invalid");
        if (localEndpoint[0] == "0.0.0.0") localEndpoint[0] = "";

        if (!int.TryParse(localEndpoint[1], out var portNumber))
        {
            throw new VoiceException("LocalEndpoint has an invalid port number");
        }
        
        var address = !string.IsNullOrWhiteSpace(localEndpoint[0])
            ? IPAddress.Parse(localEndpoint[0])
            : IPAddress.Any;
        
        
        var sipTransport = new SIPTransport();
        sipTransport.AddSIPChannel(new SIPUDPChannel(new IPEndPoint(address, portNumber)));
        _sipTransport = sipTransport;
        
        if (voiceProperties.SipTransportEnableTraceLogs) sipTransport.EnableTraceLogs();

        WatchTransport();
        RegisterUser();
    }

    public VoiceProperties VoiceProperties => _voiceProperties;
    
    private void WatchTransport()
    {
        _sipTransport.SIPTransportRequestReceived += async (_, _, sipRequest) =>
        {
            switch (sipRequest.Method)
            {
                case SIPMethodsEnum.OPTIONS:
                case SIPMethodsEnum.NOTIFY:
                    _logger.LogDebug("StatusLine = {statusLine}", sipRequest.StatusLine);

                    SIPResponse response;
                    if (sipRequest.StatusLine.Contains($"sip:{_voiceProperties.SipUsername}@"))
                    {
                        response = SIPResponse.GetResponse(sipRequest, SIPResponseStatusCodesEnum.Ok, null);
                    }
                    else
                    {

                        response = SIPResponse.GetResponse(sipRequest, SIPResponseStatusCodesEnum.Decline, null);
                    }
                    await _sipTransport.SendResponseAsync(response);
                    break;
            }
        };
    }
    
    private void RegisterUser()
    {
        _logger.LogDebug("{method}()", nameof(RegisterUser));

        var registerComplete = false;
        var success = false;
        
        _sipRegistrar = new SIPRegistrationUserAgent(
            _sipTransport,
            _voiceProperties.SipUsername,
            _voiceProperties.SipPassword,
            $"sip:{_voiceProperties.SipServer}",
            3600, sendUsernameInContactHeader: true);

        _sipRegistrar.RegistrationFailed += (_, _, message) =>
        {
            registerComplete = true;
            success = false;
            _logger.LogError("Registration failed: {message}", message);
        };
        _sipRegistrar.RegistrationSuccessful += (sipuri, _) =>
        {
            registerComplete = true;
            success = true;
            _logger.LogDebug("Registration successful for {sipuri}", sipuri);
        };
        _sipRegistrar.Start();

        while (!registerComplete)
        {
            Task.Delay(200).Wait();
        }
        if (!success) throw new VoiceException("Registration failed");
        
    }
    
    IIvrBaseLine IIvrPlugin.GetLine(int lineNumber)
    {
        _logger.LogDebug("GetLine({lineNumber})", lineNumber);
        lineNumber.ThrowIfLessThanOrEqualTo(0, nameof(lineNumber));

        if (_disposed) throw new DisposedException("You cannot get a line from a disposed plugin");

        return new SipSorceryLine(_loggerFactory, _voiceProperties, lineNumber, _sipTransport, _inviteManager);
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
        
        // unregister and give time to complete
        _sipRegistrar?.Stop();
        Task.Delay(1000).Wait();
        
        _sipTransport.Dispose();
    }
}