using System;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Util;

namespace ivrToolkit.Core.Interfaces;

/// <summary>
/// This interface defines the different prompt methods that are available through the line.
/// 
/// A prompt is a file/message to play followed by asking for digits.
/// A multiTryPrompt is one that will repeat x number of times until you get what you expect.
/// </summary>
public interface IPromptMethods
{
    /// <summary>
    /// Speaks a file/phrase to a person on the call and then expexts digits to be pressed.
    /// </summary>
    /// <param name="fileOrPhrase">The name of the file or the phrase string to speak out</param>
    /// <param name="promptOptions">Defines prompt options to override the default ones. See <see cref="PromptOptions"/></param>
    /// <returns>The answer to the prompt</returns>
    string Prompt(string fileOrPhrase, PromptOptions promptOptions = null);
    
    /// <summary>
    /// Speaks out the text-to-speech message. 
    /// </summary>
    /// <param name="textToSpeech">The text message to speak out</param>
    /// <param name="fileName">If the fileName exists, it will play the fileName instead of doing TTS.
    /// Will save the text-to-speech to the filename for use the next time. Leave null if you want to just
    /// speak the TTS</param>
    /// <param name="promptOptions">Defines prompt options to override the default ones. See <see cref="PromptOptions"/></param>
    /// <returns>The answer to the prompt</returns>
    string Prompt(string textToSpeech, string fileName, PromptOptions promptOptions = null);
    
    /// <summary>
    /// A asynchronous version of <see cref="Prompt"/>
    /// </summary>
    /// <param name="fileOrPhrase">The name of the file or the phrase string to speak out</param>
    /// <param name="cancellationToken">Allows method to cancel itself gracefully</param>
    /// <param name="promptOptions">Defines prompt options to override the default ones.
    /// Leave as null to keep the default options. See <see cref="PromptOptions"/></param>
    /// <returns>The answer to the prompt</returns>
    Task<string> PromptAsync(string fileOrPhrase, CancellationToken cancellationToken,
        PromptOptions promptOptions = null);

    /// <summary>
    /// Speaks out the text-to-speech message asynchronously. 
    /// </summary>
    /// <param name="textToSpeech">The text message to speak out</param>
    /// <param name="fileName">If the fileName exists, it will play the fileName instead of doing TTS.
    /// Will save the text-to-speech to the filename for use the next time. Leave null if you want to just
    /// speak the TTS</param>
    /// <param name="cancellationToken">The cancellation Token</param>
    /// <param name="promptOptions">Defines prompt options to override the default ones. See <see cref="PromptOptions"/></param>
    /// <returns>The answer to the prompt</returns>
    Task<string> PromptAsync(string textToSpeech, string fileName, CancellationToken cancellationToken,
        PromptOptions promptOptions = null);
    
    /// <summary>
    /// Same as <see cref="Prompt"/> but repeats x number of times until evaluator is satisfied.
    /// </summary>
    /// <param name="fileOrPhrase">The name of the file or the phrase string to speak out</param>
    /// <param name="evaluator">Pass in a function that will validate the answer. Return true if it is correct or
    /// false if incorrect.</param>
    /// <param name="multiTryPromptOptions">Defines prompt options to override the default ones. See <see cref="MultiTryPromptOptions"/></param>
    /// <returns>The answer to the prompt</returns>
    string MultiTryPrompt(string fileOrPhrase, Func<string, bool> evaluator, MultiTryPromptOptions multiTryPromptOptions = null);

    /// <summary>
    /// Speaks out the text-to-speech message but repeats x number of times until evaluator is satisfied. 
    /// </summary>
    /// <param name="textToSpeech">The text message to speak out</param>
    /// <param name="fileName">If the fileName exists, it will play the fileName instead of doing TTS.
    /// Will save the text-to-speech to the filename for use the next time. Leave null if you want to just
    /// speak the TTS</param>
    /// <param name="evaluator">Pass in a function that will validate the answer. Return true if it is correct or
    /// false if incorrect.</param>
    /// <param name="multiTryPromptOptions">Defines prompt options to override the default ones. See <see cref="MultiTryPromptOptions"/></param>
    /// <returns>The answer to the prompt</returns>
    string MultiTryPrompt(string textToSpeech, string fileName, Func<string, bool> evaluator, MultiTryPromptOptions multiTryPromptOptions = null);
    
    /// <summary>
    /// Same as <see cref="PromptAsync"/> but repeats x number of time until evaluator is satisfied.
    /// </summary>
    /// <param name="fileOrPhrase">The name of the file or the phrase string to speak out</param>
    /// <param name="evaluator">Pass in a function that will validate the answer. Return true if it is correct or
    /// false if incorrect.</param>
    /// <param name="cancellationToken">Allows method to cancel itself gracefully</param>
    /// <returns>The answer to the prompt</returns>
    Task<string> MultiTryPromptAsync(string fileOrPhrase, Func<string, bool> evaluator, CancellationToken cancellationToken);

    /// <summary>
    /// Speaks out the text-to-speech message asynchronously but repeats x number of times until evaluator is satisfied. 
    /// </summary>
    /// <param name="textToSpeech">The text message to speak out</param>
    /// <param name="fileName">If the fileName exists, it will play the fileName instead of doing TTS.
    /// Will save the text-to-speech to the filename for use the next time. Leave null if you want to just
    /// speak the TTS</param>
    /// <param name="evaluator">Pass in a function that will validate the answer. Return true if it is correct or
    /// false if incorrect.</param>
    /// <param name="cancellationToken">Allows method to cancel itself gracefully</param>
    /// <returns>The answer to the prompt</returns>
    Task<string> MultiTryPromptAsync(string textToSpeech, string fileName, Func<string, bool> evaluator, CancellationToken cancellationToken);

    /// <summary>
    /// Same as <see cref="PromptAsync"/> but repeats x number of time until evaluator is satisfied.
    /// </summary>
    /// <param name="fileOrPhrase">The name of the file or the phrase string to speak out</param>
    /// <param name="evaluator">Pass in a function that will validate the answer. Return true if it is correct or
    /// false if incorrect.</param>
    /// <param name="multiTryPromptOptions">Defines prompt options to override the default ones.
    /// Leave as null to keep the default options. See <see cref="MultiTryPromptOptions"/></param>
    /// <param name="cancellationToken">Allows method to cancel itself gracefully</param>
    /// <returns>The answer to the prompt</returns>
    Task<string> MultiTryPromptAsync(string fileOrPhrase, Func<string, bool> evaluator, MultiTryPromptOptions multiTryPromptOptions,
        CancellationToken cancellationToken);

    /// <summary>
    /// Speaks out the text-to-speech message asynchronously but repeats x number of times until evaluator is satisfied. 
    /// </summary>
    /// <param name="textToSpeech">The text message to speak out</param>
    /// <param name="fileName">If the fileName exists, it will play the fileName instead of doing TTS.
    /// Will save the text-to-speech to the filename for use the next time. Leave null if you want to just
    /// speak the TTS</param>
    /// <param name="evaluator">Pass in a function that will validate the answer. Return true if it is correct or
    /// false if incorrect.</param>
    /// <param name="multiTryPromptOptions">Defines prompt options to override the default ones. See <see cref="MultiTryPromptOptions"/></param>
    /// <param name="cancellationToken">Allows method to cancel itself gracefully</param>
    /// <returns>The answer to the prompt</returns>
    Task<string> MultiTryPromptAsync(string textToSpeech, string fileName, Func<string, bool> evaluator, MultiTryPromptOptions multiTryPromptOptions,
        CancellationToken cancellationToken);
}