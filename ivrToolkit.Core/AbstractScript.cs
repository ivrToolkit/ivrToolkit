// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 

using ivrToolkit.Core.Interfaces;
using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Core
{
    /// <summary>
    /// An implementation of the IScript interface that implements the line property to save you some time.
    /// </summary>
    public abstract class AbstractScript : IScript
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly VoiceProperties _voiceProperties;

        protected AbstractScript(ILoggerFactory loggerFactory, VoiceProperties voiceProperties)
        {
            _loggerFactory = loggerFactory;
            _voiceProperties = voiceProperties;
        }

        private ILine _line;

        /// <summary>
        /// Used within your script block to handle prompts
        /// </summary>
        protected IPromptFunctions PromptFunctions { get; private set; }

        /// <inheritdoc/>
        public ILine Line
        {
            get => _line;
            set
            {
                _line = value;
                PromptFunctions = new PromptFunctions(_loggerFactory, _voiceProperties, _line);
            }
        }
        /// <inheritdoc/>
        public abstract string Description
        {
            get;
        }

        /// <inheritdoc/>
        public abstract IScript Execute();

    } // class
}
