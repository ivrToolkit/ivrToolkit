// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 

using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Util;

namespace ivrToolkit.Core.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPromptFunctions
    {
        /// <summary>
        /// Gets the default prompt object suitable for a multi digit press. The attempts are set from 'prompt.attempts' property
        /// in voice.properties
        /// </summary>
        Prompt GetRegularStylePrompt();

        /// <summary>
        /// Gets the default prompt object suitable for a single digit press. The attempts are set from 'prompt.attempts' property
        /// in voice.properties
        /// </summary>
        /// <returns></returns>
        Prompt GetMenuStylePrompt();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetAttempts();

        /// <summary>
        /// Gets a single digit response.
        /// The attempts are set from 'prompt.attempts' property in ads.properties.
        /// </summary>
        /// <param name="promptMessage">The name of the file or the phrase string to speak out</param>
        /// <param name="allowed">This list of digits that are acceptable</param>
        /// <returns>The digit pressed that is within the allowed string.</returns>
        string SingleDigitPrompt(string promptMessage, string allowed);
        Task<string> SingleDigitPromptAsync(string promptMessage, string allowed, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a multi digit response from the user.
        /// The attempts are set from 'prompt.attempts' property in ads.properties.
        /// </summary>
        /// <param name="promptMessage">The name of the file or the phrase string to speak out</param>
        /// <returns>A response of any size including empty</returns>
        string RegularPrompt(string promptMessage);
        Task<string> RegularPromptAsync(string promptMessage, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a multi digit response from the user.
        /// The attempts are set from 'prompt.attempts' property in ads.properties.
        /// </summary>
        /// <param name="promptMessage">The name of the file or the phrase string to speak out</param>
        /// <param name="validAnswers">A string array of valid answers.</param>
        /// <returns>A response matching on of the validAnswers[] string</returns>
        string RegularPrompt(string promptMessage, string[] validAnswers);
        Task<string> RegularPromptAsync(string promptMessage, string[] validAnswers, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a multi digit response from the user.
        /// The attempts are set from 'prompt.attempts' property in ads.properties.
        /// </summary>
        /// <param name="promptMessage">The name of the file or the phrase string to speak out</param>
        /// <param name="customHandler">Prompt.ValidationHandler custom answer validator</param>
        /// <returns></returns>
        string CustomValidationPrompt(string promptMessage, Prompt.ValidationHandler customHandler);
        Task<string> CustomValidationPromptAsync(string promptMessage, Prompt.ValidationHandler customHandler, CancellationToken cancellationToken);

    }
}