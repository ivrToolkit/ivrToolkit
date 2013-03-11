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
    public class Prompt
    {
        private ILine line;

        private bool _allowEmpty = true;

        /// <summary>
        /// Lets blank be a valid answer. Only works if onValidation = null. If you supply onValidation then you are in total
        /// control of the validation.
        /// </summary>
        public bool allowEmpty
        {
            get { return _allowEmpty; }
            set { _allowEmpty = value; }
        }

        private string _specialTerminator;

        public string specialTerminator
        {
            get { return _specialTerminator; }
            set { _specialTerminator = value; }
        }

        private string _tooManyAttemptsMessage;

        public string tooManyAttemptsMessage
        {
            get { return _tooManyAttemptsMessage; }
            set { _tooManyAttemptsMessage = value; }
        }

        private string _invalidAnswerMessage;

        public string invalidAnswerMessage
        {
            get { return _invalidAnswerMessage; }
            set { _invalidAnswerMessage = value; }
        }

        private string _promptMessage;

        public string promptMessage
        {
            get { return _promptMessage; }
            set { _promptMessage = value; }
        }

        private bool _catchTooManyAttempts = true;

        public bool catchTooManyAttempts
        {
            get { return _catchTooManyAttempts; }
            set { _catchTooManyAttempts = value; }
        }

        public delegate bool ValidationHandler(string answer);
        public event ValidationHandler onValidation;

        public delegate void SpecialTerminatorHandler();
        public event SpecialTerminatorHandler onSpecialTerminator;

        public delegate void KeysEnteredHandler(string keys, string terminator);
        public event KeysEnteredHandler onKeysEntered;

        public delegate void PrePlayHandler(string fileOrSentance);
        public event PrePlayHandler onPrePlay;

        private int _attempts = 5;

        public int attempts
        {
            get { return _attempts; }
            set { _attempts = value; }
        }

        private int _numberOfDigits = 99;

        public int numberOfDigits
        {
            get { return _numberOfDigits; }
            set { _numberOfDigits = value; }
        }
        private string _terminators = "#";

        public string terminators
        {
            get { return _terminators; }
            set { _terminators = value; }
        }
        private string _answer;

        public string answer
        {
            get { return _answer; }
            set { _answer = value; }
        }

        public Prompt(ILine line)
        {
            this.line = line;
        }

        public string ask()
        {
            int count = 0;
            string myTerminators = _terminators + (_specialTerminator == null ? "" : _specialTerminator);
            while (count < _attempts)
            {
                try
                {
                    if (onPrePlay != null)
                    {
                        onPrePlay(_promptMessage);
                    }
                    playFileOrPhrase(_promptMessage);
                    _answer = line.getDigits(_numberOfDigits, myTerminators+"t");
                    if (onKeysEntered != null)
                    {
                        string term = line.lastTerminator;
                        onKeysEntered(_answer, term);
                    }
                    if (line.lastTerminator == "t")
                    {
                        throw new GetDigitsTimeoutException();
                    }
                    if (_specialTerminator != null && line.lastTerminator == _specialTerminator)
                    {
                        if (onSpecialTerminator != null)
                        {
                            onSpecialTerminator();
                        }
                        count--; // give them another chance
                    }
                    else
                    {
                        if (onValidation != null)
                        {
                            if (onValidation(_answer))
                            {
                                return _answer;
                            }
                            else
                            {
                                if (_invalidAnswerMessage != null)
                                {
                                    playFileOrPhrase(_invalidAnswerMessage);
                                }
                            }
                        }
                        else
                        {
                            if (_answer == "" && _allowEmpty == false)
                            {
                                playFileOrPhrase(_invalidAnswerMessage);
                            }
                            else
                            {
                                return _answer;
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
            if (!_catchTooManyAttempts)
            {
                return "";
            }
            if  (_tooManyAttemptsMessage != null) {
                playFileOrPhrase(_tooManyAttemptsMessage);
            }
            throw new TooManyAttempts();
        }

        private void playFileOrPhrase(string fileNameOrPhrase)
        {
            if (fileNameOrPhrase.IndexOf("|") != -1)
            {
                line.playString(fileNameOrPhrase);
            }
            else
            {
                line.playFile(fileNameOrPhrase);
            }
        }

    } // class
}
