namespace ivrToolkit.Core.Interfaces;

/// <summary>
/// Factory to used to generate text-to-speech engines. Passed into the LineManager <see cref="LineManager"/>
/// </summary>
public interface ITextToSpeechFactory
{
    /// <summary>
    /// Instantiate an implementation of a TTS engine
    /// </summary>
    ITextToSpeech Create();
}