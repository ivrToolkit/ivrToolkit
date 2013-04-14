/*
 * Copyright 2013 Troy Makaro
 *
 * This file is part of ivrToolkit, distributed under the GNU GPL. For full terms see the included COPYING file.
 */

using System;
using System.Linq;

namespace ivrToolkit.Core.Util
{
    /// <summary>
    /// This is the preferred class to use for speaking prompts. The Prompt class is more low level.
    /// </summary>
    public class PromptFunctions : IPromptFunctions
    {
        private readonly ILine _line;
        /// <summary>
        /// Initiates the class with a voice line
        /// </summary>
        /// <param name="line">The voice line to ask the questions on</param>
        public PromptFunctions(ILine line)
        {
            _line = line;
        }

        /// <summary>
        /// Gets the default prompt object suitable for a multi digit press. The attempts are set from 'prompt.attempts' property
        /// in voice.properties
        /// </summary>
        public virtual Prompt GetRegularStylePrompt()
        {
            var p = new Prompt(_line) {NumberOfDigits = 99, Terminators = "#", Attempts = GetAttempts()};
            return p;
        }
        
        /// <summary>
        /// Gets the default prompt object suitable for a single digit press. The attempts are set from 'prompt.attempts' property
        /// in voice.properties
        /// </summary>
        /// <returns></returns>
        public virtual Prompt GetMenuStylePrompt()
        {
            var p = new Prompt(_line) {NumberOfDigits = 1, Terminators = "", Attempts = GetAttempts()};
            return p;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetAttempts()
        {
            return VoiceProperties.Current.PromptAttempts;
        }

        /// <summary>
        /// Gets a single digit response.
        /// The attempts are set from 'prompt.attempts' property in ads.properties.
        /// </summary>
        /// <param name="promptMessage">The name of the file or the phrase string to speak out</param>
        /// <param name="allowed">This list of digits that are acceptable</param>
        /// <returns>The digit pressed that is within the allowed string.</returns>
        public string SingleDigitPrompt(string promptMessage, string allowed)
        {
            var p = GetMenuStylePrompt();
            p.PromptMessage = promptMessage;
            p.OnValidation += delegate(string answer)
                {
                    return allowed.IndexOf(answer, StringComparison.Ordinal) != -1;
                };
            return p.Ask();
        }

        /// <summary>
        /// Gets a multi digit response from the user.
        /// The attempts are set from 'prompt.attempts' property in ads.properties.
        /// </summary>
        /// <param name="promptMessage">The name of the file or the phrase string to speak out</param>
        /// <returns>A response of any size including empty</returns>
        public string RegularPrompt(string promptMessage)
        {
            return RegularPrompt(promptMessage, null);
        }

        /// <summary>
        /// Gets a multi digit response from the user.
        /// The attempts are set from 'prompt.attempts' property in ads.properties.
        /// </summary>
        /// <param name="promptMessage">The name of the file or the phrase string to speak out</param>
        /// <param name="validAnswers">A string array of valid answers.</param>
        /// <returns>A response matching on of the validAnswers[] string</returns>
        public string RegularPrompt(string promptMessage, string[] validAnswers)
        {
            Prompt.ValidationHandler customHandler = delegate(string answer)
            {
                if (validAnswers == null) return true;

                return validAnswers.Any(s => answer == s);
            };
            return CustomValidationPrompt(promptMessage, customHandler);
        }

        /// <summary>
        /// Gets a multi digit response from the user.
        /// The attempts are set from 'prompt.attempts' property in ads.properties.
        /// </summary>
        /// <param name="promptMessage">The name of the file or the phrase string to speak out</param>
        /// <param name="customHandler">Prompt.ValidationHandler custom answer validator</param>
        /// <returns></returns>
        public string CustomValidationPrompt(string promptMessage, Prompt.ValidationHandler customHandler)
        {
            Prompt p = GetRegularStylePrompt();
            p.PromptMessage = promptMessage;
            p.OnValidation += customHandler;
            return p.Ask();
        }
    } // class
}
