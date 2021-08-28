// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 

using ivrToolkit.Core;
using ivrToolkit.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace SipOutTest.ScriptBlocks
{
    public class WelcomeScript : AbstractScript
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly VoiceProperties _voiceProperties;
        private readonly ILogger<WelcomeScript> _logger;

        public WelcomeScript(ILoggerFactory loggerFactory, VoiceProperties voiceProperties) : base(loggerFactory, voiceProperties)
        {
            _loggerFactory = loggerFactory;
            _voiceProperties = voiceProperties;
            _logger = loggerFactory.CreateLogger<WelcomeScript>();
            _logger.LogDebug("Ctr()");
        }

        public override string Description => "Welcome";

        public override IScript Execute()
        {
            _logger.LogDebug("Execute()");
            // say My welcome message
            Line.PlayFile(@"Voice Files\ThankYou.wav");
            return new MainScript(_loggerFactory, _voiceProperties);
        }

    } // class
}
