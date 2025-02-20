using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.TextToSpeech.V1;
using ivrToolkit.Core.Extensions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Core.TTS;

// ReSharper disable once InconsistentNaming
/// <inheritdoc cref="ivrToolkit.Core.Interfaces.ITextToSpeech" />
public class GoogleTTS : ITextToSpeech, IDisposable
{
    private readonly VoiceProperties _voiceProperties;
    private readonly ILogger<AzureTTS> _logger;
    private readonly TextToSpeechClient _ttsClient;

    /// <summary>
    /// An Azure TTS implementation
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="voiceProperties"></param>
    /// <param name="ttsClient"></param>
    public GoogleTTS(ILoggerFactory loggerFactory, VoiceProperties voiceProperties, TextToSpeechClient ttsClient)
    {
        _voiceProperties = voiceProperties.ThrowIfNull(nameof(voiceProperties));
        _ttsClient = ttsClient.ThrowIfNull(nameof(ttsClient));
        loggerFactory.ThrowIfNull(nameof(loggerFactory));
        _logger = loggerFactory.CreateLogger<AzureTTS>();
    }

    /// <inheritdoc />
    public WavStream TextToSpeech(string text)
    {
        _logger.LogDebug("{method}({text})", nameof(TextToSpeech), text);
        return TextToSpeechAsync(text, CancellationToken.None).GetAwaiter().GetResult();
    }

    public async Task<WavStream> TextToSpeechAsync(string text, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({text})", nameof(TextToSpeechAsync), text);
        return text.StartsWith("<speak") ? await SpeakSsmlAsync(text) : await SpeakTextAsync(text);
    }

    private async Task<WavStream> SpeakSsmlAsync(string text)
    {
        // Prepare text input
        var input = new SynthesisInput { Ssml  = text };

        // Select voice parameters
        var voice = new VoiceSelectionParams
        {
            LanguageCode = _voiceProperties.TtsGoogleLanguageCode,
            SsmlGender = _voiceProperties.TtsGoogleGender
        };
        if (!string.IsNullOrWhiteSpace(_voiceProperties.TtsGoogleLanguageCode))
        {
            voice.Name = _voiceProperties.TtsGoogleName;
        }

        // Configure audio output format (16-bit PCM WAV, 8000 Hz or 16000 Hz)
        var audioConfig = new AudioConfig
        {
            AudioEncoding = AudioEncoding.Linear16, // WAV format
            SampleRateHertz = _voiceProperties.DefaultWavSampleRate
        };

        // Generate speech
        var response = await _ttsClient.SynthesizeSpeechAsync(input, voice, audioConfig);
        return new WavStream(response.AudioContent.ToByteArray());
    }

    private async Task<WavStream> SpeakTextAsync(string text)
    {
        var ssml = $@"<speak>
                        {text}
                    </speak>";

        return await SpeakSsmlAsync(ssml);
    }


    /// <inheritdoc />
    public void Dispose()
    {
        // _ttsClient doesn't have a dispose method
    }
}