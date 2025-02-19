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
    /// Speaks a file/phrase to a person on the call and then expects digits to be pressed.
    /// </summary>
    /// <param name="fileOrPhrase">The name of the file or the phrase string to speak out</param>
    /// <param name="promptOptions">Defines prompt options to override the default ones. See <see cref="PromptOptions"/></param>
    /// <returns>The answer to the prompt</returns>
    string Prompt(string fileOrPhrase, PromptOptions promptOptions = null);
    
    /// <summary>
    /// Speaks out the text-to-speech message using <see cref="ITextToSpeechCache"/>  and then expects digits to be pressed.
    /// </summary>
    /// <param name="textToSpeechCache">Used to generated TTS and possibly save it to a wav file for later use.
    /// Will only generate TTS if a wav file is not specified, missing or changed.</param>
    /// <param name="promptOptions">Defines prompt options to override the default ones. See <see cref="PromptOptions"/></param>
    /// <returns>The answer to the prompt</returns>
    string Prompt(ITextToSpeechCache textToSpeechCache, PromptOptions promptOptions = null);
    
    /// <summary>
    /// Asynchronously speaks a file/phrase to a person on the call and then expects digits to be pressed.
    /// </summary>
    /// <param name="fileOrPhrase">The name of the file or the phrase string to speak out</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="promptOptions">Defines prompt options to override the default ones.
    /// Leave as null to keep the default options. See <see cref="PromptOptions"/></param>
    /// <returns>The answer to the prompt</returns>
    Task<string> PromptAsync(string fileOrPhrase, CancellationToken cancellationToken,
        PromptOptions promptOptions = null);

    /// <summary>
    /// Asynchronously speaks out the text-to-speech message using <see cref="ITextToSpeechCache"/>  and then expects digits to be pressed.
    /// </summary>
    /// <param name="textToSpeechCache">Used to generated TTS and possibly save it to a wav file for later use.
    /// Will only generate TTS if a wav file is not specified, missing or changed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="promptOptions">Defines prompt options to override the default ones. See <see cref="PromptOptions"/></param>
    /// <returns>The answer to the prompt</returns>
    Task<string> PromptAsync(ITextToSpeechCache textToSpeechCache, CancellationToken cancellationToken,
        PromptOptions promptOptions = null);
    
    /// <summary>
    /// Same as <see cref="Prompt(string,ivrToolkit.Core.Util.PromptOptions)"/> but will ask x number of times before the answer is either accepted or there
    /// have been too many retries.
    /// </summary>
    /// <param name="fileOrPhrase">The name of the file or the phrase string to speak out</param>
    /// <param name="evaluator">Pass in a function that will validate the answer. Return true if it is correct or
    /// false if incorrect.</param>
    /// <param name="multiTryPromptOptions">Defines prompt options to override the default ones. See <see cref="MultiTryPromptOptions"/></param>
    /// <returns>The answer to the prompt</returns>
    string MultiTryPrompt(string fileOrPhrase, Func<string, bool> evaluator, MultiTryPromptOptions multiTryPromptOptions = null);

    /// <summary>
    /// Same as <see cref="Prompt(ITextToSpeechCache,ivrToolkit.Core.Util.PromptOptions)"/> but will ask x number of times before the answer is either accepted or there
    /// </summary>
    /// <param name="textToSpeechCache">Used to generated TTS and possibly save it to a wav file for later use.
    /// Will only generate TTS if a wav file is not specified, missing or changed.</param>
    /// <param name="evaluator">Pass in a function that will validate the answer. Return true if it is correct or
    /// false if incorrect.</param>
    /// <param name="multiTryPromptOptions">Defines prompt options to override the default ones. See <see cref="MultiTryPromptOptions"/></param>
    /// <returns>The answer to the prompt</returns>
    string MultiTryPrompt(ITextToSpeechCache textToSpeechCache, Func<string, bool> evaluator, MultiTryPromptOptions multiTryPromptOptions = null);
    
    /// <summary>
    /// Same as <see cref="Prompt(string,ivrToolkit.Core.Util.PromptOptions)"/> but will ask x number of times before the answer is either accepted or there
    /// </summary>
    /// <param name="fileOrPhrase">The name of the file or the phrase string to speak out</param>
    /// <param name="evaluator">Pass in a function that will validate the answer. Return true if it is correct or
    /// false if incorrect.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The answer to the prompt</returns>
    Task<string> MultiTryPromptAsync(string fileOrPhrase, Func<string, bool> evaluator, CancellationToken cancellationToken);

    /// <summary>
    /// Same as <see cref="Prompt(ITextToSpeechCache,ivrToolkit.Core.Util.PromptOptions)"/> but will ask x number of times before the answer is either accepted or there
    /// </summary>
    /// <param name="textToSpeechCache">Used to generated TTS and possibly save it to a wav file for later use.
    /// Will only generate TTS if a wav file is not specified, missing or changed.</param>
    /// <param name="evaluator">Pass in a function that will validate the answer. Return true if it is correct or
    /// false if incorrect.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The answer to the prompt</returns>
    Task<string> MultiTryPromptAsync(ITextToSpeechCache textToSpeechCache, Func<string, bool> evaluator, CancellationToken cancellationToken);

    /// <summary>
    /// Same as <see cref="Prompt(string,ivrToolkit.Core.Util.PromptOptions)"/> but will ask x number of times before the answer is either accepted or there
    /// </summary>
    /// <param name="fileOrPhrase">The name of the file or the phrase string to speak out</param>
    /// <param name="evaluator">Pass in a function that will validate the answer. Return true if it is correct or
    /// false if incorrect.</param>
    /// <param name="multiTryPromptOptions">Defines prompt options to override the default ones.
    /// Leave as null to keep the default options. See <see cref="MultiTryPromptOptions"/></param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The answer to the prompt</returns>
    Task<string> MultiTryPromptAsync(string fileOrPhrase, Func<string, bool> evaluator, MultiTryPromptOptions multiTryPromptOptions,
        CancellationToken cancellationToken);

    /// <summary>
    /// Same as <see cref="Prompt(ITextToSpeechCache,ivrToolkit.Core.Util.PromptOptions)"/> but will ask x number of times before the answer is either accepted or there
    /// </summary>
    /// <param name="textToSpeechCache">Used to generated TTS and possibly save it to a wav file for later use.
    /// Will only generate TTS if a wav file is not specified, missing or changed.</param>
    /// <param name="evaluator">Pass in a function that will validate the answer. Return true if it is correct or
    /// false if incorrect.</param>
    /// <param name="multiTryPromptOptions">Defines prompt options to override the default ones. See <see cref="MultiTryPromptOptions"/></param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The answer to the prompt</returns>
    Task<string> MultiTryPromptAsync(ITextToSpeechCache textToSpeechCache, Func<string, bool> evaluator, MultiTryPromptOptions multiTryPromptOptions,
        CancellationToken cancellationToken);
}