// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 

using ivrToolkit.Core;
using ivrToolkit.Core.Extensions;
using ivrToolkit.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace SipInTest.ScriptBlocks
{
    public class MainScript : AbstractScript
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly VoiceProperties _voiceProperties;
        private readonly ILine _line;
        private readonly ILogger<MainScript> _logger;

        public MainScript(ILoggerFactory loggerFactory, VoiceProperties voiceProperties, ILine line) : base(loggerFactory, voiceProperties, line)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<MainScript>();
            _logger.LogDebug("Ctr()");
            _voiceProperties = voiceProperties;
            _line = line;
        }

        public override string Description => "Main Script";

        public override IScript Execute()
        {
            _logger.LogDebug("Execute()");
            while (true)
            {
                string result = PromptFunctions.RegularPrompt(@"Voice Files\Press1234.wav");

                Line.PlayFile(@"Voice Files\YouPressed.wav");

                Line.PlayCharacters(result);

                Line.PlayFile(result == "1234" ? @"Voice Files\Correct.wav" : @"Voice Files\Incorrect.wav");

                result = PromptFunctions.SingleDigitPrompt(@"Voice Files\TryAgain.wav", "12");
                if (result == "2") break;
            } // endless loop
            return new GoodbyeScript(_loggerFactory, _voiceProperties, _line);
        }
    } // class
}
