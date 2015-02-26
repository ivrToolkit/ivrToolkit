using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ivrToolkit.Core;
using ivrToolkit.Core.Util;
using ConsoleIvr.ScriptBlocks;

namespace ConsoleIvr
{
    class Program
    {

        private static ILine line;

        static void Main(string[] args)
        {

            WaitCall();
            //MakeCall();
        }

        static void MakeCall()
        {
            line = LineManager.GetLine(1);
            CallAnalysis result = line.Dial("7782320255", 3500);
            line.PlayFile("System Recordings\\Thursday.wav");
            line.Hangup();
            Console.WriteLine("End of Program");
        }

        static void WaitCall()
        {


            line = LineManager.GetLine(1);
            Console.WriteLine("Got a Line");

            line.WaitRings(2);

            try
            {

                ScriptManager manager = new ScriptManager(line, new WelcomeScript());

                while (manager.HasNext())
                {
                    // execute the next script
                    manager.Execute();
                }

                line.Hangup();

            }
            catch (ivrToolkit.Core.Exceptions.HangupException)
            {
                line.Hangup();

            }
            line.Close();
        }
    }
}
