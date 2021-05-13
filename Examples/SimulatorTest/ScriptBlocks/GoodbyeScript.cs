// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ivrToolkit.Core;

namespace SimulatorTest.ScriptBlocks
{
    public class GoodbyeScript : AbstractScript
    {
        public override string Description
        {
            get { return "Goodbye"; }
        }

        public override IScript Execute()
        {
            // say my goodbye message
            Line.PlayFile(@"Voice Files\Goodbye.wav");
            return null; // signal the end
        }
    } // class
}
