using System;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Extensions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging;
using Google.Cloud.TextToSpeech.V1;

namespace ivrToolkit.Core.TTS;

/// <inheritdoc />
public class GoogleTtsFactory : ITextToSpeechFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<GoogleTtsFactory> _logger;
    private readonly VoiceProperties _voiceProperties;

    /// <summary>
    /// An Azure TTS implementation
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="voiceProperties">The properties file</param>
    public GoogleTtsFactory(ILoggerFactory loggerFactory, VoiceProperties voiceProperties)
    {
        _loggerFactory = loggerFactory.ThrowIfNull(nameof(loggerFactory));
        _voiceProperties = voiceProperties.ThrowIfNull(nameof(voiceProperties));

        var defaultWavSampleRate = voiceProperties.DefaultWavSampleRate;
        if (defaultWavSampleRate != 8000 && defaultWavSampleRate != 16000)
        {
            throw new VoiceException("Default wav sample rate must be either 8000 or 16000");
        }
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", voiceProperties.TtsGoogleApplicationCredentialsPath);
        
        _logger = loggerFactory.CreateLogger<GoogleTtsFactory>();
    }

    /// <inheritdoc />
    public ITextToSpeech Create()
    {
        _logger.LogDebug("{method}()", nameof(Create));
        
        var client = TextToSpeechClient.Create();
        return new GoogleTTS(_loggerFactory, _voiceProperties, client);
    }
}