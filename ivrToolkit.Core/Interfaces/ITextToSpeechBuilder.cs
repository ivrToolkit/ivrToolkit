#nullable enable
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Util;

namespace ivrToolkit.Core.Interfaces;

/// <summary>
/// Passed to various LineWrapper methods in order to use TTS. Holds a text message and optionally a wav file name
/// for later parsing.
/// Has the ability to save the TTS as a wav file and a related txt file that holds the text message that generated it.
/// If a wav file exists and the related txt file matches the text message, then the TTS conversion is skipped and the
/// wav file is used instead.
/// If a wav file was not specified, TTS will happen every time.
/// </summary>
public interface ITextToSpeechBuilder
{

    /// <summary>
    /// Gets a <see cref="WavStream"/> object representing the text message or wav file.
    /// This method generates a wav file and a txt file if the wav file name exists.
    /// Will not do TTS and save the file unless
    /// the file doesn't yet exist, or it does exist but the txt file representing the last text message
    /// is now different from the text message. TTS happens always if the wav file name is null.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task<WavStream> GetWavStreamAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Generates a wav file from the text message. Will not do TTS and save the file unless
    /// the file doesn't yet exist, or it does exist but the txt file representing the last text message
    /// is now different from the text message. Will throw a VoiceException if the file name wasn't specified.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task GenerateWavFileAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the name of the wav file or null if one wasn't specified.
    /// </summary>
    string? GetFileName();
}