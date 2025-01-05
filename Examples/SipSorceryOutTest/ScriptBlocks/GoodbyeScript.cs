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
    public class GoodbyeScript : AbstractScript
    {
        private readonly ILogger<GoodbyeScript> _logger;

        public GoodbyeScript(ILoggerFactory loggerFactory, VoiceProperties voiceProperties, IIvrLine line) : base(loggerFactory, voiceProperties, line)
        {
            _logger = loggerFactory.CreateLogger<GoodbyeScript>();
            _logger.LogDebug("Ctr()");
        }

        public override string Description => "Goodbye";

        public override IScript Execute()
        {
            _logger.LogDebug("Execute");
            // say my goodbye message
            Line.PlayFile(@"Voice Files\Goodbye.wav");
            return null; // signal the end
        }
    } // class
}
