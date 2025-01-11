// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 

using System.Threading;
using System.Threading.Tasks;
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

        protected AbstractScript(ILoggerFactory loggerFactory, VoiceProperties voiceProperties, IIvrLine line)
        {
            _loggerFactory = loggerFactory;
            _voiceProperties = voiceProperties;
            Line = line;
            PromptFunctions = new PromptFunctions(_loggerFactory, _voiceProperties, line);
        }


        /// <summary>
        /// Used within your script block to handle prompts
        /// </summary>
        protected IPromptFunctions PromptFunctions { get; }

        /// <inheritdoc/>
        public IIvrLine Line { get; }

        /// <inheritdoc/>
        public abstract string Description
        {
            get;
        }

        /// <inheritdoc/>
        public virtual IScript Execute()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual Task<IScript> ExecuteAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    } // class
}
