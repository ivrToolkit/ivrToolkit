namespace ivrToolkit.Core.Interfaces;

/// <summary>
/// Used to generate an instance of ITextToSpeechCache. This provides a way of caching Text to speech
/// as a wav file so that later use will not have to rerun the TTS process.
/// <code>
/// // example:
/// var ttsCache = ttsCacheFactory.Create(text, wavFileName);
/// </code>
/// You can then pass ttsCache to PlayFile or PlayTextToSpeech
/// </summary>
public interface ITextToSpeechCacheFactory
{
    /// <summary>
    /// Creates an instance of ITextToSpeechCache with text and a wav file name for use
    /// in PlayFile or PlayTextToSpeech
    /// </summary>
    /// <param name="text">The text to be converted to a wav stream later on</param>
    /// <param name="wavFileName">The name of the wav file to generate later on. If null,
    /// no wav file will be generated</param>
    ITextToSpeechCache Create(string text, string wavFileName = null);
}