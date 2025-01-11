// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Core.Util
{
    /// <summary>
    /// This is the preferred class to use for speaking prompts. The Prompt class is more low level.
    /// </summary>
    public class PromptFunctions : IPromptFunctions
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly VoiceProperties _voiceProperties;
        private readonly IIvrLine _line;
        private readonly ILogger<PromptFunctions> _logger;

        /// <summary>
        /// Initiates the class with a voice line
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="voiceProperties"></param>
        /// <param name="line">The voice line to ask the questions on</param>
        public PromptFunctions(ILoggerFactory loggerFactory, VoiceProperties voiceProperties, IIvrLine line)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<PromptFunctions>();
            _logger.LogDebug("Ctr()");
            _voiceProperties = voiceProperties;
            _line = line;
        }

        /// <summary>
        /// Gets the default prompt object suitable for a multi digit press. The attempts are set from 'prompt.attempts' property
        /// in voice.properties
        /// </summary>
        public virtual Prompt GetRegularStylePrompt()
        {
            _logger.LogDebug("GetRegularStylePrompt()");
            const int dgMaxdigs = 31;
            var p = new Prompt(_loggerFactory, _voiceProperties, _line) {NumberOfDigits = dgMaxdigs, Terminators = "#", Attempts = GetAttempts(), BlankAttempts = GetBlankAttempts()};
            return p;
        }
        
        /// <summary>
        /// Gets the default prompt object suitable for a single digit press. The attempts are set from 'prompt.attempts' property
        /// in voice.properties
        /// </summary>
        /// <returns></returns>
        public virtual Prompt GetMenuStylePrompt()
        {
            _logger.LogDebug("GetMenuStylePrompt()");
            var p = new Prompt(_loggerFactory, _voiceProperties, _line) { NumberOfDigits = 1, Terminators = "", Attempts = GetAttempts(), BlankAttempts = GetBlankAttempts() };
            return p;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetAttempts()
        {
            _logger.LogDebug("GetAttempts()");
            return _voiceProperties.PromptAttempts;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetBlankAttempts()
        {
            _logger.LogDebug("GetBlankAttempts()");
            return _voiceProperties.PromptBlankAttempts;
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
            return SingleDigitPromptInternalAsync(promptMessage,allowed,
                (p) => { var result = p.Ask(); return Task.FromResult(result); }
                ).GetAwaiter().GetResult();
        }

        public async Task<string> SingleDigitPromptAsync(string promptMessage, string allowed, CancellationToken cancellationToken)
        {
            return await SingleDigitPromptInternalAsync(promptMessage,allowed,
                async (p) => await p.AskAsync(cancellationToken));
        }

        private async Task<string> SingleDigitPromptInternalAsync(string promptMessage, string allowed,
             Func<Prompt, Task<string>> ask)
        {
            _logger.LogDebug("SingleDigitPromptInternalAsync()");
            var p = GetMenuStylePrompt();
            p.PromptMessage = promptMessage;
            p.OnValidation += answer => allowed.IndexOf(answer, StringComparison.Ordinal) != -1;
            return await ask(p);
        }
        
        /// <summary>
        /// Gets a multi digit response from the user.
        /// The attempts are set from 'prompt.attempts' property in ads.properties.
        /// </summary>
        /// <param name="promptMessage">The name of the file or the phrase string to speak out</param>
        /// <returns>A response of any size including empty</returns>
        public string RegularPrompt(string promptMessage)
        {
            _logger.LogDebug("RegularPrompt({0})", promptMessage );
            return RegularPrompt(promptMessage, null);
        }
        
        public async Task<string> RegularPromptAsync(string promptMessage, CancellationToken cancellationToken)
        {
            _logger.LogDebug("RegularPromptAsync({0})", promptMessage );
            return await RegularPromptAsync(promptMessage, null, cancellationToken);
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
            _logger.LogDebug("RegularPrompt({0}, {1})", promptMessage, validAnswers);

            bool CustomHandler(string answer)
            {
                if (validAnswers == null) return true;
                return validAnswers.Any(s => answer == s);
            }
            return CustomValidationPrompt(promptMessage, CustomHandler);
        }

        public async Task<string> RegularPromptAsync(string promptMessage, string[] validAnswers, CancellationToken cancellationToken)
        {
            _logger.LogDebug("RegularPromptAsync({0}, {1})", promptMessage, validAnswers);

            bool CustomHandler(string answer)
            {
                if (validAnswers == null) return true;
                return validAnswers.Any(s => answer == s);
            }
            return await CustomValidationPromptAsync(promptMessage, CustomHandler, cancellationToken);
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
            _logger.LogDebug("CustomValidationPrompt({0}, Prompt.ValidationHandler)", promptMessage);
            return CustomValidationPromptInternalAsync(promptMessage, customHandler,
                (p) => { var result = p.Ask(); return Task.FromResult(result); }
            ).GetAwaiter().GetResult();
        }
        
        public async Task<string> CustomValidationPromptAsync(string promptMessage, Prompt.ValidationHandler customHandler, 
            CancellationToken cancellationToken)
        {
            return await CustomValidationPromptInternalAsync(promptMessage,customHandler,
                async (p) => await p.AskAsync(cancellationToken));
        }

        private async Task<string> CustomValidationPromptInternalAsync(string promptMessage, Prompt.ValidationHandler customHandler,
            Func<Prompt, Task<string>> ask)
        {
            _logger.LogDebug("CustomValidationPromptInternalAsync()");
            var prompt = GetRegularStylePrompt();
            prompt.PromptMessage = promptMessage;
            prompt.OnValidation += customHandler;
            return await ask(prompt);
        }
        
    } // class
}
