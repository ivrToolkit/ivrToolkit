#nullable enable
using ivrToolkit.Core.Extensions;
using ivrToolkit.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Core.TTS;

public class TextToSpeechGenerator : ITextToSpeechGenerator
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ITextToSpeech? _textToSpeech;
    private readonly IFileHandler _fileHandler;
    private readonly ILogger<TextToSpeechGenerator> _logger;

    public TextToSpeechGenerator(ILoggerFactory loggerFactory, ITextToSpeech? textToSpeech, IFileHandler fileHandler)
    {
        _loggerFactory = loggerFactory.ThrowIfNull(nameof(loggerFactory));
        _logger = loggerFactory.CreateLogger<TextToSpeechGenerator>();
        _textToSpeech = textToSpeech;
        _fileHandler = fileHandler;
    }

    public ITextToSpeechBuilder GetTextToSpeechBuilder(string text, string? wavFileName = null)
    {
        _logger.LogDebug("{method}({text}, {wavFileName})", nameof(GetTextToSpeechBuilder), text, wavFileName);
        return new TextToSpeechBuilder(_loggerFactory, _textToSpeech, text, wavFileName, _fileHandler);
    }
}