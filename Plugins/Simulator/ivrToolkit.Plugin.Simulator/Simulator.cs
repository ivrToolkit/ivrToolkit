// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 

using System.Collections.Generic;
using System.Globalization;
using ivrToolkit.Core;

namespace ivrToolkit.Plugin.Simulator
{
    public class Simulator : IVoice
    {
        private static readonly Dictionary<string, SimulatorLine> Lines = new Dictionary<string, SimulatorLine>();

        public ILine GetLine(int lineNumber, object data = null)
        {
            // make sure the simulator thread is started and listening for a connection
            SimulatorListener.Singleton.Start();

            try
            {
                return Lines[lineNumber.ToString(CultureInfo.InvariantCulture)];
            }
            catch (KeyNotFoundException)
            {
                var line = new SimulatorLine(lineNumber);
                Lines.Add(lineNumber.ToString(CultureInfo.InvariantCulture), line);
                return line;
            }
        }

        public static void ReleaseLine(int lineNumber)
        {
            Lines.Remove(lineNumber.ToString(CultureInfo.InvariantCulture));
        }
    }
}
