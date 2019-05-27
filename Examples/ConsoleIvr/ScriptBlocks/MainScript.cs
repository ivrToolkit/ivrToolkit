﻿// 
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
using ivrToolkit.Core.Util;

namespace ConsoleIvr.ScriptBlocks
{
    public class MainScript : AbstractScript
    {
        public override string Description
        {
            get { return "Main Script"; }
        }

        public override IScript Execute()
        {
            while (true)
            {
                string result = PromptFunctions.RegularPrompt(@"Voice Files\Press1234.wav");

                Line.PlayFile(@"Voice Files\YouPressed.wav");

                Line.PlayCharacters(result);

                if (result == "1234")
                {
                    Line.PlayFile(@"Voice Files\Correct.wav");
                }
                else
                {
                    Line.PlayFile(@"Voice Files\Incorrect.wav");
                }

                result = PromptFunctions.SingleDigitPrompt(@"Voice Files\TryAgain.wav", "12");
                if (result == "2") break;
            } // endless loop
            return new GoodbyeScript();
        }
    } // class
}
