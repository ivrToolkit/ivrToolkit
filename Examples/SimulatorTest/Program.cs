using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ivrToolkit.Core;
using ivrToolkit.Core.Util;

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

            // A utility class to define some common prompt definitions
            PromptFunctions functions = new PromptFunctions(line);

            // wait for an incomming call
            line.waitRings(2);

            // say Thank You
            line.playFile(@"Voice Files\ThankYou.wav");

            while (true)
            {
                string result = functions.regularPrompt(@"Voice Files\Press1234.wav");

                line.playFile(@"Voice Files\YouPressed.wav");

                line.playCharacters(result);

                if (result == "1234")
                {
                    line.playFile(@"Voice Files\Correct.wav");
                }
                else
                {
                    line.playFile(@"Voice Files\Incorrect.wav");
                }

                result = functions.singleDigitPrompt(@"Voice Files\TryAgain.wav", "12");
                if (result == "2") break;
            }
            line.playFile(@"Voice Files\Goodbye.wav");
            line.hangup();
        }
    } // class
}
