using System;
using ivrToolkit.Core;
using ivrToolkit.Core.Util;
using ConsoleIvr.ScriptBlocks;
using System.Threading;

namespace ConsoleIvr
{
    class Program
    {

        //private static ILine line;
        //private static ILine line2;
        private static bool _exit;


        static void Main()
        {
            //Thread waitThread = new Thread(new ThreadStart(WaitCall));
            var lineNumber = 1;
            Console.WriteLine("Start Line {0}", lineNumber);
            Thread waitThread = new Thread(() => WaitCall(lineNumber));
            waitThread.Start();

            Console.WriteLine("All threads should be alive.");
            Console.ReadLine();
            _exit = true;
            Thread.Sleep(1);

            waitThread.Abort();
            waitThread.Join();

            Console.WriteLine("Before Line Manager Release All");
            LineManager.ReleaseAll();

            Console.WriteLine("Threads should now be dead.");
        }

        static void WaitCall(int lineNumber)
        {
            Console.WriteLine("WaitCall: Line {0}: Get Line", lineNumber);
            var line = LineManager.GetLine(lineNumber);
            Console.WriteLine("################################WaitCall: Line {0}: Got Line", lineNumber);
            while (!_exit)
            {
                Console.WriteLine("WaitCall: Line {0}: Hang Up", lineNumber);
                line.Hangup();
                Thread.Sleep(1000);
                Console.WriteLine("WaitCall: Line {0}: Wait Rings", lineNumber);
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

            }
            Console.WriteLine("WaitCall: Line {0}: End of Wait Call", lineNumber);
            line.Close();
            Console.WriteLine("WaitCall: Line {0}: End of Wait Call Line Closed", lineNumber);
        }
    }
}
