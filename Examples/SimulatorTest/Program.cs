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
            line = LineManager.GetLine(1);

            // A utility class to define some common prompt definitions
            PromptFunctions functions = new PromptFunctions(line);

            // wait for an incomming call
            line.WaitRings(2);

            // say Thank You
            line.PlayFile(@"Voice Files\ThankYou.wav");

            while (true)
            {
                string result = functions.RegularPrompt(@"Voice Files\Press1234.wav");

                line.PlayFile(@"Voice Files\YouPressed.wav");

                line.PlayCharacters(result);

                if (result == "1234")
                {
                    line.PlayFile(@"Voice Files\Correct.wav");
                }
                else
                {
                    line.PlayFile(@"Voice Files\Incorrect.wav");
                }

                result = functions.SingleDigitPrompt(@"Voice Files\TryAgain.wav", "12");
                if (result == "2") break;
            }
            line.PlayFile(@"Voice Files\Goodbye.wav");
            line.Hangup();
        }
    } // class
}
