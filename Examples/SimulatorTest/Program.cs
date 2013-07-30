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
using ivrToolkit.Core.Util;
using SimulatorTest.ScriptBlocks;

namespace SimulatorTest
{
    public class Program
    {
        private ILine line;

        public static void Main(string[] args)
        {
            Program prog = new Program(args);
            prog.go();
        }

        public Program(string[] args)
        {
        }

        public void go()
        {
            // pick the line number you want
            line = LineManager.GetLine(1);


            // wait for an incomming call
            line.WaitRings(2);

            ScriptManager manager = new ScriptManager(line, new WelcomeScript());

            while (manager.HasNext())
            {
                // execute the next script
                manager.Execute(); 
            }

            line.Hangup();
        }
    } // class
}
