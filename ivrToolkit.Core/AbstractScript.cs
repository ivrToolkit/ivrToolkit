// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 
using ivrToolkit.Core.Util;

namespace ivrToolkit.Core
{
    /// <summary>
    /// An implementation of the IScript interface that implements the line property to save you some time.
    /// </summary>
    public abstract class AbstractScript : IScript
    {
        private ILine _line;
        private IPromptFunctions _promptFunctions;

        /// <summary>
        /// Used within your script block to handle prompts
        /// </summary>
        protected IPromptFunctions PromptFunctions
        {
            get
            {
                return _promptFunctions;
            }
        }

        /// <inheritdoc/>
        public ILine Line
        {
            get
            { return _line; }
            set
            {
                _line = value;
                _promptFunctions = new PromptFunctions(_line);
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
