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
    public class ScriptManager
    {
        private ILine line;
        private IScript _nextScript;

        public IScript nextScript
        {
            get { return _nextScript; }
            set { _nextScript = value; }
        }

        public ScriptManager(ILine line)
        {
            this.line = line;
        }

        public void execute()
        {
            _nextScript.line = line;
            _nextScript = _nextScript.execute();
        }

        public bool hasNext()
        {
            if (_nextScript != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
