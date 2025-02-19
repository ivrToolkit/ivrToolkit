#nullable enable
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Extensions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Core.TTS;

public class TextToSpeechCache : ITextToSpeechCache
{
    private readonly ITextToSpeech? _textToSpeech;
    private readonly string _text;
    private readonly string? _wavFileName;
    private readonly IFileHandler _fileHandler;

    private readonly ILogger<ITextToSpeechCache> _logger;

    public bool FileExists { get; private set; }

    public TextToSpeechCache(ILoggerFactory loggerFactory, ITextToSpeech? textToSpeech, 
        string text, 
        string? wavFileName,
        IFileHandler fileHandler)
    {
        loggerFactory.ThrowIfNull(nameof(loggerFactory));
        _text = text.ThrowIfNull(nameof(text));
        _logger = loggerFactory.CreateLogger<TextToSpeechCache>();
        _fileHandler = fileHandler.ThrowIfNull(nameof(fileHandler));
        
        // these two are allowed to be null
        _textToSpeech = textToSpeech;
        _wavFileName = wavFileName;
    }
    
    public async Task<WavStream> GetOrGenerateCacheAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("{method}()", nameof(GetOrGenerateCacheAsync));
        
        // see if there is a wav file existing and the text hasn't changed
        if (await IsNotChangedAsync(_text, _wavFileName, cancellationToken))
        {
            FileExists = true;
            return await GetWavFileDataAsync(_wavFileName);
        }

        if (_textToSpeech == null)
        {
            throw new VoiceException("Missing text to speech engine. Cannot convert text to Speech.");
        }
        var audioStream = await _textToSpeech.TextToSpeechAsync(_text, cancellationToken);

        if (_wavFileName is null) return audioStream;
        
        // write out the wav file
        await _fileHandler.WriteAllBytesAsync(_wavFileName, audioStream.ToArray(), cancellationToken);

        // now write out the txt file
        var wavFileNameWithoutExtension = Path.GetFileNameWithoutExtension(_wavFileName);
        var txtFileName = $"{wavFileNameWithoutExtension}.txt";
        var fullPath = Path.Combine(Path.GetDirectoryName(_wavFileName) ?? string.Empty, txtFileName);
        
        await _fileHandler.WriteAllTextAsync(fullPath, _text, cancellationToken);
        FileExists = true;
        return audioStream;
    }

    public async Task GenerateCacheAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("{method}()", nameof(GetOrGenerateCacheAsync));
        if (_wavFileName is null)
        {
            throw new VoiceException("Requires fileName.");
        }
        
        // see if there is a wav file existing and the text hasn't changed
        if (await IsNotChangedAsync(_text, _wavFileName, cancellationToken))
        {
            FileExists = true;
            return;
        }

        if (_textToSpeech == null)
        {
            throw new VoiceException("Missing text to speech engine. Cannot convert text to Speech.");
        }
        var audioStream = await _textToSpeech.TextToSpeechAsync(_text, cancellationToken);

        // write out the wav file
        await _fileHandler.WriteAllBytesAsync(_wavFileName, audioStream.ToArray(), cancellationToken);

        // now write out the txt file
        var wavFileNameWithoutExtension = Path.GetFileNameWithoutExtension(_wavFileName);
        var txtFileName = $"{wavFileNameWithoutExtension}.txt";
        var fullPath = Path.Combine(Path.GetDirectoryName(_wavFileName) ?? string.Empty, txtFileName);
        
        await _fileHandler.WriteAllTextAsync(fullPath, _text, cancellationToken);
        FileExists = true;
    }

    public string? GetCacheFileName()
    {
        return _wavFileName;
    }

    private async Task<bool> IsNotChangedAsync(string text, string? wavFileName, CancellationToken cancellationToken)
    {
        if (wavFileName is not null && _fileHandler.Exists(wavFileName))
        {
            var wavFileNameWithoutExtension = Path.GetFileNameWithoutExtension(wavFileName);
            var txtFileName = $"{wavFileNameWithoutExtension}.txt";
            var fullPath = Path.Combine(Path.GetDirectoryName(wavFileName) ?? string.Empty, txtFileName);
            
            if (!_fileHandler.Exists(fullPath)) return false;
            
            var savedText = await _fileHandler.ReadAllTextAsync(fullPath, cancellationToken);
            if (text == savedText)
            {
                // nothing has changed
                return true;
            }
        }

        return false;
    }

    private async Task<WavStream> GetWavFileDataAsync(string? wavFileName)
    {
        if (string.IsNullOrWhiteSpace(wavFileName) || !_fileHandler.Exists(wavFileName))
        {
            throw new VoiceException($"Missing wav file: {wavFileName}");
        }
        
        await using var fs = _fileHandler.GetFileStream(wavFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var ms = new MemoryStream();
        
        await fs.CopyToAsync(ms);
        
        return new WavStream(ms.ToArray());
    }
}