using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Extensions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Core.Util;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Core.TTS;

public class AzureTtsFactory : ITextToSpeechFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly string _subscriptionKey;
    private readonly string _region;
    private readonly ILogger<AzureTtsFactory> _logger;
    private readonly int _defaultWavSampleRate;
    private readonly string _voiceName;

    /// <summary>
    /// An Azure TTS implementation
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="voiceProperties">The properties file</param>
    public AzureTtsFactory(ILoggerFactory loggerFactory, VoiceProperties voiceProperties)
    {
        _loggerFactory = loggerFactory.ThrowIfNull(nameof(loggerFactory));
        voiceProperties.ThrowIfNull(nameof(voiceProperties));
        _subscriptionKey = voiceProperties.TtsAzureSubscriptionKey;
        _region = voiceProperties.TtsAzureRegion;
        _voiceName = voiceProperties.TtsAzureVoice;

        _defaultWavSampleRate = voiceProperties.DefaultWavSampleRate;
        if (_defaultWavSampleRate != 8000 && _defaultWavSampleRate != 16000)
        {
            throw new VoiceException("Default wav sample rate must be either 8000 or 16000");
        }
        
        _logger = loggerFactory.CreateLogger<AzureTtsFactory>();
    }

    public ITextToSpeech Create()
    {
        _logger.LogDebug("{method}()", nameof(Create));
        var speechConfig = SpeechConfig.FromSubscription(_subscriptionKey, _region);

        speechConfig.SetSpeechSynthesisOutputFormat(_defaultWavSampleRate == 8000
            ? SpeechSynthesisOutputFormat.Riff8Khz16BitMonoPcm
            : SpeechSynthesisOutputFormat.Riff16Khz16BitMonoPcm);
        
        var synthesizer = new SpeechSynthesizer(speechConfig, null);
        return new AzureTTS(_loggerFactory, _voiceName, synthesizer);
    }
}