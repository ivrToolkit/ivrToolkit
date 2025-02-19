#nullable enable
using ivrToolkit.Core.Extensions;
using ivrToolkit.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Core.TTS;

public class TextToSpeechCacheFactory : ITextToSpeechCacheFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ITextToSpeech? _textToSpeech;
    private readonly IFileHandler _fileHandler;
    private readonly ILogger<TextToSpeechCacheFactory> _logger;

    public TextToSpeechCacheFactory(ILoggerFactory loggerFactory, ITextToSpeech? textToSpeech, IFileHandler fileHandler)
    {
        _loggerFactory = loggerFactory.ThrowIfNull(nameof(loggerFactory));
        _logger = loggerFactory.CreateLogger<TextToSpeechCacheFactory>();
        _textToSpeech = textToSpeech;
        _fileHandler = fileHandler;
    }

    public ITextToSpeechCache Create(string text, string? wavFileName = null)
    {
        _logger.LogDebug("{method}({text}, {wavFileName})", nameof(Create), text, wavFileName);
        return new TextToSpeechCache(_loggerFactory, _textToSpeech, text, wavFileName, _fileHandler);
    }
}