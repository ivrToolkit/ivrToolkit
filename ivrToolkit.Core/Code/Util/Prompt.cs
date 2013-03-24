/*
 * Copyright 2013 Troy Makaro
 *
 * This file is part of ivrToolkit, distributed under the GNU GPL. For full terms see the included COPYING file.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ivrToolkit.Core;
using ivrToolkit.Core.Exceptions;

namespace ivrToolkit.Core.Util
{
    /// <summary>
    /// This is a low level class defining prompts. You should use PromptFunctions instead however this is available if you want to design your own Prompt class.
    /// </summary>
    public class Prompt
    {
        private ILine line;

        private bool allowEmpty = true;

        /// <summary>
        /// Instatiate the class with the line out want to ask questions on.
        /// </summary>
        /// <param name="line">The voice line to use</param>
        public Prompt(ILine line)
        {
            this.line = line;
        }

        /// <summary>
        /// Lets blank be a valid answer. Only works if OnValidation = null. If you supply onValidation then you are in total
        /// control of the validation.
        /// </summary>
        public bool AllowEmpty
        {
            get { return allowEmpty; }
            set { allowEmpty = value; }
        }

        private string specialTerminator;
        /// <summary>
        /// You can define special terminator digits that will fire the OnSpecialTerminator event. For example a '*' could take you to a special option to control volume.
        /// </summary>
        public string SpecialTerminator
        {
            get { return specialTerminator; }
            set { specialTerminator = value; }
        }

        private string tooManyAttemptsMessage;
        /// <summary>
        /// A message to be played before throwing the TooManyAttemptsException. If null then no message will be played.
        /// </summary>
        public string TooManyAttemptsMessage
        {
            get { return tooManyAttemptsMessage; }
            set { tooManyAttemptsMessage = value; }
        }
        private string invalidAnswerMessage;
        /// <summary>
        /// A message to be played if the answer is inccorrect. If null then no message will be played.
        /// </summary>
        public string InvalidAnswerMessage
        {
            get { return invalidAnswerMessage; }
            set { invalidAnswerMessage = value; }
        }

        private string promptMessage;
        /// <summary>
        /// The prompt message to play before calling the GetDigits method
        /// </summary>
        public string PromptMessage
        {
            get { return promptMessage; }
            set { promptMessage = value; }
        }

        private bool catchTooManyAttempts = true;
        /// <summary>
        /// Set to false and the prompt will return a value of "". Set to true and TooManyAttemptsException will be thrown.
        /// </summary>
        public bool CatchTooManyAttempts
        {
            get { return catchTooManyAttempts; }
            set { catchTooManyAttempts = value; }
        }
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
        /// <param name="fileOrSentance">The wav file name or a sentance. A sentance is the 'data|code,data|code' format that PlayString uses</param>
        public delegate void PrePlayHandler(string fileOrSentance);
        /// <summary>
        /// Use this event to do something just before the prompt is asked.
        /// </summary>
        public event PrePlayHandler OnPrePlay;

        private int attempts = VoiceProperties.Current.PromptAttempts;
        /// <summary>
        /// The number of attempts before throwing the TooManyAttemptsException
        /// </summary>
        public int Attempts
        {
            get { return attempts; }
            set { attempts = value; }
        }

        private int numberOfDigits = 99;
        /// <summary>
        /// The number of digits the prompt can ask for before automatically terminating. The default is 99 so every prompt requires a pound key for termination. Set to 1 for menu prompts.
        /// </summary>
        public int NumberOfDigits
        {
            get { return numberOfDigits; }
            set { numberOfDigits = value; }
        }
        private string terminators = "#";
        /// <summary>
        /// List of one or more valid termination digits. The default is '#'. You can also use 'T' which will allow a timeout to be a valid termination key
        /// </summary>
        public string Terminators
        {
            get { return terminators; }
            set { terminators = value; }
        }
        private string answer;
        /// <summary>
        /// Calling Ask() returns the answer anyways but if you want you can also retrieve the keypresses with this property.
        /// </summary>
        public string Answer
        {
            get { return answer; }
            set { answer = value; }
        }
        /// <summary>
        /// Asks the question and returns an answer.
        /// </summary>
        public string Ask()
        {
            int count = 0;
            string myTerminators = terminators + (specialTerminator == null ? "" : specialTerminator);
            while (count < attempts)
            {
                try
                {
                    if (OnPrePlay != null)
                    {
                        OnPrePlay(promptMessage);
                    }
                    PlayFileOrPhrase(promptMessage);
                    answer = line.GetDigits(numberOfDigits, myTerminators+"t");
                    if (OnKeysEntered != null)
                    {
                        string term = line.LastTerminator;
                        OnKeysEntered(answer, term);
                    }
                    if (line.LastTerminator == "t")
                    {
                        throw new GetDigitsTimeoutException();
                    }
                    if (specialTerminator != null && line.LastTerminator == specialTerminator)
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
                            if (OnValidation(answer))
                            {
                                return answer;
                            }
                            else
                            {
                                if (invalidAnswerMessage != null)
                                {
                                    PlayFileOrPhrase(invalidAnswerMessage);
                                }
                            }
                        }
                        else
                        {
                            if (answer == "" && allowEmpty == false)
                            {
                                PlayFileOrPhrase(invalidAnswerMessage);
                            }
                            else
                            {
                                return answer;
                            }
                        }
                    }
                }
                catch (GetDigitsTimeoutException)
                {
                }
                count++;
            } // while

            // too many attempts
            if (!catchTooManyAttempts)
            {
                return "";
            }
            if  (tooManyAttemptsMessage != null) {
                PlayFileOrPhrase(tooManyAttemptsMessage);
            }
            throw new TooManyAttempts();
        }

        private void PlayFileOrPhrase(string fileNameOrPhrase)
        {
            if (fileNameOrPhrase.IndexOf("|") != -1)
            {
                line.PlayString(fileNameOrPhrase);
            }
            else
            {
                line.PlayFile(fileNameOrPhrase);
            }
        }

    } // class
}
