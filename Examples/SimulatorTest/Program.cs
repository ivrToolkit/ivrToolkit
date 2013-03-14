using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ivrToolkit.Core;

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
            line = LineManager.getLine(1);

            // wait for an incomming call
            line.waitRings(2);
            line.hangup();
        }
    } // class
}
