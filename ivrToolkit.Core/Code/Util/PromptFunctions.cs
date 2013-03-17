/*
 * Copyright 2013 Troy Makaro
 *
 * This file is part of ivrToolkit, distributed under the GNU GPL. For full terms see the included COPYING file.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ivrToolkit.Core.Properties;
using ivrToolkit.Core;

namespace ivrToolkit.Core.Util
{
    public class PromptFunctions
    {
        private ILine line;

        public PromptFunctions(ILine line)
        {
            this.line = line;
        }

        /// <summary>
        /// Gets the default prompt object suitable for a multi digit press. The attempts are set from 'prompt.attempts' property
        /// in ads.properties
        /// </summary>
        public virtual Prompt getRegularStylePrompt()
        {
            Prompt p = new Prompt(line);
            p.numberOfDigits = 99;
            p.terminators = "#";
            p.attempts = getAttempts();
            return p;
        }

        /// <summary>
        /// Gets the default prompt object suitable for a single digit press. The attempts are set from 'prompt.attempts' property
        /// in ads.properties
        /// </summary>
        public virtual Prompt getMenuStylePrompt()
        {
            Prompt p = new Prompt(line);
            p.numberOfDigits = 1;
            p.terminators = "";
            p.attempts = getAttempts();
            return p;
        }

        private int getAttempts()
        {
            return VoiceProperties.current.promptAttempts;
        }

        /// <summary>
        /// Gets a single digit response.
        /// The attempts are set from 'prompt.attempts' property in ads.properties.
        /// </summary>
        /// <param name="promptMessage">The name of the file or the phrase string to speak out</param>
        /// <param name="allowed">This list of digits that are acceptable</param>
        /// <returns>The digit pressed that is within the allowed string.</returns>
        public string singleDigitPrompt(string promptMessage, string allowed)
        {
            Prompt p = getMenuStylePrompt();
            p.promptMessage = promptMessage;
            p.onValidation += delegate(string answer)
            {
                if (allowed.IndexOf(answer) != -1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            };
            return p.ask();
        }

        public string singleDigitPrompt(string promptMessage, string allowed, int attemptsOverride, int timeoutOverride, bool catchTooManyAttempts)
        {
            Prompt p = getMenuStylePrompt();            
            p.promptMessage = promptMessage;
            p.onValidation += delegate(string answer)
            {
                if (allowed.IndexOf(answer) != -1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            };

            p.attempts = 1;
            p.catchTooManyAttempts = false;

            return p.ask();
        }

        /// <summary>
        /// Gets a multi digit response from the user.
        /// The attempts are set from 'prompt.attempts' property in ads.properties.
        /// </summary>
        /// <param name="promptMessage">The name of the file or the phrase string to speak out</param>
        /// <returns>A response of any size including empty</returns>
        public string regularPrompt(string promptMessage)
        {
            return regularPrompt(promptMessage, null);
        }

        /// <summary>
        /// Gets a multi digit response from the user.
        /// The attempts are set from 'prompt.attempts' property in ads.properties.
        /// </summary>
        /// <param name="promptMessage">The name of the file or the phrase string to speak out</param>
        /// <param name="validAnswers">A string array of valid answers.</param>
        /// <returns>A response matching on of the validAnswers[] string</returns>
        public string regularPrompt(string promptMessage, string[] validAnswers)
        {
            Prompt.ValidationHandler customHandler = delegate(string answer)
            {
                if (validAnswers == null) return true;

                foreach (string s in validAnswers)
                {
                    if (answer == s)
                    {
                        return true;
                    }
                }
                return false;
            };
            return customValidationPrompt(promptMessage, customHandler);
        }

        /// <summary>
        /// Gets a multi digit response from the user.
        /// The attempts are set from 'prompt.attempts' property in ads.properties.
        /// </summary>
        /// <param name="promptMessage">The name of the file or the phrase string to speak out</param>
        /// <param name="customHandler">Prompt.ValidationHandler custom answer validator</param>
        /// <returns></returns>
        public string customValidationPrompt(string promptMessage, Prompt.ValidationHandler customHandler)
        {
            Prompt p = getRegularStylePrompt();
            p.promptMessage = promptMessage;
            p.onValidation += customHandler;
            return p.ask();
        }
    } // class
}
