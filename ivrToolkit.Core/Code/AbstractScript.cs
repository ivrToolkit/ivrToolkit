/*
 * Copyright 2013 Troy Makaro
 *
 * This file is part of ivrToolkit, distributed under the GNU GPL. For full terms see the included COPYING file.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ivrToolkit.Core.Util;
using System.Threading;

namespace ivrToolkit.Core
{
    /// <summary>
    /// An implementation of the IScript interface that implements the line property to save you some time.
    /// </summary>
    public abstract class AbstractScript : IScript
    {
        private ILine line;

        /// <inheritdoc/>
        public ILine Line
        {
            get
            { return line; }
            set { line = value; }
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
