/*
 * Copyright 2013 Troy Makaro
 *
 * This file is part of ivrToolkit, distributed under the GNU GPL. For full terms see the included COPYING file.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ivrToolkit.Core;

namespace ivrToolkit.SimulatorPlugin
{
    public class Simulator : IVoice
    {
        private static Dictionary<string, SimulatorLine> lines = new Dictionary<string, SimulatorLine>();

        public ILine GetLine(int lineNumber)
        {
            // make sure the simulator thread is started and listening for a connection
            SimulatorListener.singleton.start();

            try
            {
                return lines[lineNumber.ToString()];
            }
            catch (KeyNotFoundException)
            {
                SimulatorLine line = new SimulatorLine(lineNumber);
                lines.Add(lineNumber.ToString(), line);
                return line;
            }
        }
        public static void releaseLine(int lineNumber)
        {
            lines.Remove(lineNumber.ToString());
        }
    }
}
