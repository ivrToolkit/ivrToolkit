// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 

using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace ivrToolkit.Core
{
    /// <summary>
    /// Holds the voice.properties in a static Properties class.
    /// </summary>
    public class VoiceProperties : Properties, IDisposable
    {
        private readonly ILogger<VoiceProperties> _logger;

        public VoiceProperties(ILoggerFactory loggerFactory, string fileName) : base (loggerFactory, fileName)
        {
            _logger = loggerFactory.CreateLogger<VoiceProperties>();
            _logger.LogDebug("ctr(ILoggerFactory, {0})", fileName);
        }

        /// <summary>
        /// Total number of attempts to try at a prompt before 'TooManyAttemptsException' is thrown. Default is '99'.
        /// </summary>
        public int PromptAttempts => int.Parse(GetProperty("prompt.attempts", "99"));

        /// <summary>
        /// Number of blank entry attempts to try at a prompt before 'TooManyAttemptsException' is thrown. Default is '5'.
        /// </summary>
        public int PromptBlankAttempts => int.Parse(GetProperty("prompt.blankAttempts", "5"));

        public new void Dispose()
        {
            _logger.LogDebug("Dispose()");
            base.Dispose();
        }
    }
}
