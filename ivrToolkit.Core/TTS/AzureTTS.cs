using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Extensions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Core.Util;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Core.TTS;

// ReSharper disable once InconsistentNaming
public class AzureTTS : ITextToSpeech, IDisposable
{
    private readonly ILogger<AzureTTS> _logger;
    private readonly SpeechSynthesizer _synthesizer;
    private readonly WavConverter _wavConverter;

    /// <summary>
    /// An Azure TTS implementation
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="synthesizer"></param>
    public AzureTTS(ILoggerFactory loggerFactory, SpeechSynthesizer synthesizer)
    {
        _synthesizer = synthesizer.ThrowIfNull(nameof(synthesizer));
        loggerFactory.ThrowIfNull(nameof(loggerFactory));
        _logger = loggerFactory.CreateLogger<AzureTTS>();
        _wavConverter = new WavConverter(loggerFactory);
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

        // it worked, lets convert to the correct format
        return await _wavConverter.ConvertToPCM16Bit8000hz(new MemoryStream(result.AudioData));
    }

    private async Task<MemoryStream> SpeakTextAsync(string text)
    {
        var ssml = $@"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>
                            <voice name='en-US-JennyNeural'>
                                {text}
                            </voice>
                        </speak>";
        
        using var result = await _synthesizer.SpeakSsmlAsync(ssml);
        if (result.Reason != ResultReason.SynthesizingAudioCompleted)
        {
            throw new VoiceException(result.Reason.ToString());
        }

        // it worked, lets convert to the correct format
        return await _wavConverter.ConvertToPCM16Bit8000hz(new MemoryStream(result.AudioData));
    }


    public void Dispose()
    {
        _synthesizer?.Dispose();
    }
}