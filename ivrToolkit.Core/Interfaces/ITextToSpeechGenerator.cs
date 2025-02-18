namespace ivrToolkit.Core.Interfaces;

/// <summary>
/// Used to generate an instance of the ITextToSpeechBuilder.
/// <code>
/// // example:
/// var ttsBuilder = ttsGenerator.GetTextToSpeechBuilder(text, wavFileName);
/// </code>
/// You can then pass ttsBuilder to PlayFile or PlayTextToSpeech
/// </summary>
public interface ITextToSpeechGenerator
{
    /// <summary>
    /// Creates an instance of the TextToSpeechBuilder with text and a wav file name for use later
    /// in PlayFile or PlayTextToSpeech
    /// </summary>
    /// <param name="text">The text to be converted to a wav stream later on</param>
    /// <param name="wavFileName">The name of the wav file to generate later on. If null,
    /// no wav file will be generated</param>
    ITextToSpeechBuilder GetTextToSpeechBuilder(string text, string wavFileName = null);
}