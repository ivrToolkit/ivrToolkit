// 
// Copyright 2013 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the LESSER GNU GPL. For full terms see the included COPYING file and the COPYING.LESSER file.
// 
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ivrToolkit.Core;

namespace ConsoleIvr.ScriptBlocks
{
    public class WelcomeScript : AbstractScript
    {
        public override string Description
        {
            get { return "Welcome"; }
        }

        public override IScript Execute()
        {
            // say My welcome message
            Line.PlayFile(@"Voice Files\ThankYou.wav");
            return new MainScript();
        }
    } // class
}
