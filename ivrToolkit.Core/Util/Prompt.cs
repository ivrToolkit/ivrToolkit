// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 
using System;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Extensions;
using ivrToolkit.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Core.Util
{
    /// <summary>
    /// This is a low level class defining prompts. You should use PromptFunctions instead however this is available if you want to design your own Prompt class.
    /// </summary>
    public class Prompt
    {
        private readonly IIvrLine _line;
        private readonly ILogger<Prompt> _logger;

        /// <summary>
        /// Instantiate the class with the line out want to ask questions on.
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="voiceProperties"></param>
        /// <param name="line">The voice line to use</param>
        public Prompt(ILoggerFactory loggerFactory, VoiceProperties voiceProperties, IIvrLine line)
        {
            _logger = loggerFactory.CreateLogger<Prompt>();
            _logger.LogDebug("Ctr()");

            _line = line;
            Attempts = voiceProperties.PromptAttempts;
            BlankAttempts = voiceProperties.PromptBlankAttempts;
        }

        /// <summary>
        /// Lets blank be a valid answer. Only works if OnValidation = null. If you supply onValidation then you are in total
        /// control of the validation.
        /// </summary>
        public bool AllowEmpty { get; set; } = true;

        /// <summary>
        /// You can define special terminator digits that will fire the OnSpecialTerminator event. For example a '*' could take you to a special option to control volume.
        /// </summary>
        public string SpecialTerminator { get; set; }

        /// <summary>
        /// A message to be played before throwing the TooManyAttemptsException. If null then no message will be played.
        /// </summary>
        public string TooManyAttemptsMessage { get; set; }

        /// <summary>
        /// A message to be played if the answer is incorrect. If null then no message will be played.
        /// </summary>
        public string InvalidAnswerMessage { get; set; }

        /// <summary>
        /// The prompt message to play before calling the GetDigits method
        /// </summary>
        public string PromptMessage { get; set; }

        /// <summary>
        /// Set to false and the prompt will return a value of "". Set to true and TooManyAttemptsException will be thrown.
        /// </summary>
        public bool CatchTooManyAttempts { get; set; } = true;

        /// <summary>
        /// Method signature for use with the OnValidation event
        /// </summary>
        /// <param name="answer">The string to be validated</param>
        /// <returns>true if valid. false if invalid</returns>
        public delegate bool ValidationHandler(string answer);

        /// <summary>
        /// Use this event to define your own validation.
        /// </summary>
        public event ValidationHandler OnValidation;

        /// <summary>
        /// Method signature for use with the OnSpecialTerminator event
        /// </summary>
        public delegate void SpecialTerminatorHandler();

        /// <summary>
        /// Use this event to define what you want to do if a special terminator digit was pressed.
        /// </summary>
        public event SpecialTerminatorHandler OnSpecialTerminator;

        /// <summary>
        /// Method signature for the OnKeysEntered event
        /// </summary>
        /// <param name="keys">The digits pressed</param>
        /// <param name="terminator">The terminator digit pressed</param>
        public delegate void KeysEnteredHandler(string keys, string terminator);

        /// <summary>
        /// Use this event to track what keys have been pressed. The intended use is for logging keypresses not for validation.
        /// </summary>
        public event KeysEnteredHandler OnKeysEntered;

        /// <summary>
        /// Method signature for the OnPrePlay event
        /// </summary>
        /// <param name="fileOrSentance">The wav file name or a sentence. A sentence is the 'data|code,data|code' format that PlayString uses</param>
        public delegate void PrePlayHandler(string fileOrSentance);

        /// <summary>
        /// Use this event to do something just before the prompt is asked.
        /// </summary>
        public event PrePlayHandler OnPrePlay;


        /// <summary>
        /// The number of attempts before throwing the TooManyAttemptsException
        /// </summary>
        public int Attempts { get; set; }

        /// <summary>
        /// The number of blank attempts before throwing the TooManyAttemptsException
        /// </summary>
        public int BlankAttempts { get; set; }

        /// <summary>
        /// The number of digits the prompt can ask for before automatically terminating. The default is DG_MAXDIGS = 31 so every prompt requires a pound key for termination. Set to 1 for menu prompts.
        /// </summary>
        public int NumberOfDigits { get; set; } = 31;

        /// <summary>
        /// List of one or more valid termination digits. The default is '#'. You can also use 'T' which will allow a timeout to be a valid termination key
        /// </summary>
        public string Terminators { get; set; } = "#";

        /// <summary>
        /// Calling Ask() returns the answer anyways but if you want you can also retrieve the keypresses with this property.
        /// </summary>
        public string Answer { get; set; }


        /// <summary>
        /// Asks the question and returns an answer.
        /// </summary>
        public string Ask()
        {
            _logger.LogDebug("Ask()");
            return AskInternalAsync(
                content =>
                {
                    PlayFileOrPhrase(content); return Task.CompletedTask;
                },
                (numberOfDigits, terminators) =>
                {
                    var result = _line.GetDigits(numberOfDigits, terminators); return Task.FromResult(result);
                }).GetAwaiter().GetResult();
        }
        
        public async Task<string> AskAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("AskAsync()");
            return await AskInternalAsync(
                async content => await PlayFileOrPhraseAsync(content, cancellationToken),
                async (numberOfDigits, terminators) => await _line.GetDigitsAsync(numberOfDigits, terminators, cancellationToken));
        }

        private async Task<string> AskInternalAsync(
            Func<string, Task> playFileOrPhrase,
            Func<int, string, Task<string>> getDigits)
        {
            _logger.LogDebug("AskInternal()");
            var count = 0;
            var blankCount = 0;
            var myTerminators = Terminators + (SpecialTerminator ?? "");
            while (count < Attempts)
            {
                if (blankCount >= BlankAttempts)
                {
                    break;
                }

                try
                {
                    OnPrePlay?.Invoke(PromptMessage);

                    await playFileOrPhrase(PromptMessage);
                    Answer = await getDigits(NumberOfDigits, myTerminators + "t");
                    //Answer = _line.GetDigits(NumberOfDigits, myTerminators + "t");
                    if (OnKeysEntered != null)
                    {
                        var term = _line.LastTerminator;
                        OnKeysEntered(Answer, term);
                    }

                    if (_line.LastTerminator == "t")
                    {
                        if (myTerminators.IndexOf("t", StringComparison.Ordinal) == -1)
                        {
                            throw new GetDigitsTimeoutException();
                        }

                        if (!AllowEmpty && Answer == "")
                        {
                            throw new GetDigitsTimeoutException();
                        }
                    }

                    if (SpecialTerminator != null && _line.LastTerminator == SpecialTerminator)
                    {
                        if (OnSpecialTerminator != null)
                        {
                            OnSpecialTerminator();
                        }

                        count--; // give them another chance
                    }
                    else
                    {
                        if (OnValidation != null)
                        {
                            if (OnValidation(Answer))
                            {
                                return Answer;
                            }
                        }
                        else
                        {
                            if (Answer != "" || AllowEmpty) return Answer;
                        }

                        if (InvalidAnswerMessage != null)
                        {
                            await playFileOrPhrase(InvalidAnswerMessage);
                        }
                    }
                }
                catch (GetDigitsTimeoutException)
                {
                }

                // increment counters
                blankCount++;
                if (!string.IsNullOrEmpty(Answer)) blankCount = 0;
                count++;

            } // while

            // too many attempts
            if (!CatchTooManyAttempts)
            {
                return "";
            }

            if (TooManyAttemptsMessage != null)
            {
                await playFileOrPhrase(TooManyAttemptsMessage);
            }

            throw new TooManyAttempts();
        }

        private void PlayFileOrPhrase(string fileNameOrPhrase)
        {
            _logger.LogDebug("PlayFileOrPhrase({0})", fileNameOrPhrase);
            if (fileNameOrPhrase.IndexOf("|", StringComparison.Ordinal) != -1)
            {
                _line.PlayString(fileNameOrPhrase);
            }
            else
            {
                _line.PlayFile(fileNameOrPhrase);
            }
        }
        
        private async Task PlayFileOrPhraseAsync(string fileNameOrPhrase, CancellationToken cancellationToken)
        {
            _logger.LogDebug("PlayFileOrPhraseAsync({0})", fileNameOrPhrase);
            if (fileNameOrPhrase.IndexOf("|", StringComparison.Ordinal) != -1)
            {
                await _line.PlayStringAsync(fileNameOrPhrase, cancellationToken);
            }
            else
            {
                await _line.PlayFileAsync(fileNameOrPhrase, cancellationToken);
            }
        }

    } // class
}
