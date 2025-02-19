#nullable enable
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Util;

namespace ivrToolkit.Core.Interfaces;

/// <summary>
/// A caching mechanism for TTS. TTS generated will be stored as a wav file so that future calls will
/// not have to call the TTS process again.
/// Two files are created. A wav file and a text file. The wav file contains the TTS wav stream and the text
/// contains the text message.
///
/// In order to skip TTS generation, the wav file must exist and the text in the text file must match the text message.
///
/// If a wav file path is not specified, no caching will happen and TTS will run every time.
/// </summary>
public interface ITextToSpeechCache
{

    /// <summary>
    /// Gets or generates the TTS audio and caches it.
    /// This method will use the wav file that is cached if it hasn't changed, or it will generate the
    /// TTS audio and cache it.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The <see cref="WavStream"/> from the cache.</returns>
    Task<WavStream> GetOrGenerateCacheAsync(CancellationToken cancellationToken);

    /// <summary>
    /// If the cache has changed, will run the TTS and cache the new value.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task GenerateCacheAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the name of the cached wav file or null if one wasn't specified.
    /// </summary>
    string? GetCacheFileName();
}