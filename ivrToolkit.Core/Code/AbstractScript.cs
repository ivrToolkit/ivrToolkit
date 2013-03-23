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
    /// Documentation goes here
    /// </summary>
    public abstract class AbstractScript : IScript
    {
        private ILine _line;

        // set by scriptmanager
        public ILine line
        {
            get
            { return _line; }
            set { _line = value; }
        }

        public abstract string description
        {
            get;
        }

        public abstract IScript execute();

    } // class
}
