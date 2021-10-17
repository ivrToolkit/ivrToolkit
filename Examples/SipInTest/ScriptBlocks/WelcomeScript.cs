// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 

using ivrToolkit.Core;
using ivrToolkit.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace SipInTest.ScriptBlocks
{
    public class WelcomeScript : AbstractScript
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly VoiceProperties _voiceProperties;
        private readonly IIvrLine _line;
        private readonly ILogger<WelcomeScript> _logger;

        public WelcomeScript(ILoggerFactory loggerFactory, VoiceProperties voiceProperties, IIvrLine line) : base(loggerFactory, voiceProperties, line)
        {
            _loggerFactory = loggerFactory;
            _voiceProperties = voiceProperties;
            _line = line;
            _logger = loggerFactory.CreateLogger<WelcomeScript>();
            _logger.LogDebug("Ctr()");
        }

        public override string Description => "Welcome";

        public override IScript Execute()
        {
            _logger.LogDebug("Execute()");
            // say My welcome message
            Line.PlayFile(@"Voice Files\ThankYou.wav");
            return new MainScript(_loggerFactory, _voiceProperties, _line);
        }

    } // class
}
