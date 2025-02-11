using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ivrToolkit.Core.Interfaces;

/// <summary>
/// Represents the TTS engine.
/// </summary>
public interface ITextToSpeech
{
    /// <summary>
    /// Converts the text to Speech and returns a wav audio stream including the wav header.
    /// </summary>
    /// <param name="text">The text to convert to a wav stream</param>
    /// <returns>A wav stream representing the text. Includes the wav header too.</returns>
    MemoryStream TextToSpeech(string text);
    
    /// <summary>
    /// Converts the text to Speech and returns a wav audio stream including the wav header.
    /// </summary>
    /// <param name="text">The text to convert to a wav stream</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A wav stream representing the text. Includes the wav header too.</returns>
    Task<MemoryStream> TextToSpeechAsync(string text, CancellationToken cancellationToken);
}