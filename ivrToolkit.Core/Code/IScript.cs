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

namespace ivrToolkit.Core
{
    public interface IScript
    {
        ILine line
        {
            get;
            set;
        }
        string description
        {
            get;
        }
        IScript execute();
    }
}
