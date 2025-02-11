using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Extensions;
using ivrToolkit.Core.Interfaces;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Core.TTS;

// ReSharper disable once InconsistentNaming
public class AzureTTS : ITextToSpeech, IDisposable
{
    private readonly ILogger<AzureTTS> _logger;
    private readonly SpeechSynthesizer _synthesizer;
    private readonly string _voiceName;

    /// <summary>
    /// An Azure TTS implementation
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="voiceName"></param>
    /// <param name="synthesizer"></param>
    public AzureTTS(ILoggerFactory loggerFactory, string voiceName, SpeechSynthesizer synthesizer)
    {
        _synthesizer = synthesizer.ThrowIfNull(nameof(synthesizer));
        loggerFactory.ThrowIfNull(nameof(loggerFactory));
        _voiceName = voiceName.ThrowIfNull(nameof(voiceName));
        _logger = loggerFactory.CreateLogger<AzureTTS>();
    }
    
    public MemoryStream TextToSpeech(string text)
    {
        _logger.LogDebug("{method}({text})", nameof(TextToSpeech), text);
        return TextToSpeechAsync(text, CancellationToken.None).GetAwaiter().GetResult();
    }
    
    public async Task<MemoryStream> TextToSpeechAsync(string text, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({text})", nameof(TextToSpeechAsync), text);
        return text.StartsWith("<speak") ? await SpeakSsmlAsync(text) : await SpeakTextAsync(text);
    }

    private async Task<MemoryStream> SpeakSsmlAsync(string text)
    {
        using var result = await _synthesizer.SpeakSsmlAsync(text);
        if (result.Reason != ResultReason.SynthesizingAudioCompleted)
        {
            throw new VoiceException(result.Reason.ToString());
        }
        return new MemoryStream(result.AudioData);
    }

    private async Task<MemoryStream> SpeakTextAsync(string text)
    {
        var ssml = $@"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>
                            <voice name='{_voiceName}'>
                                {text}
                            </voice>
                        </speak>";
        
        using var result = await _synthesizer.SpeakSsmlAsync(ssml);
        if (result.Reason != ResultReason.SynthesizingAudioCompleted)
        {
            throw new VoiceException(result.Reason.ToString());
        }
        return new MemoryStream(result.AudioData);
    }


    public void Dispose()
    {
        _synthesizer?.Dispose();
    }
}