using System;
using ivrToolkit.Core;
using ConsoleIvr.ScriptBlocks;
using System.Threading;
using ivrToolkit.Core.Exceptions;

namespace ConsoleIvr
{
    class Program
    {

        //private static ILine line;
        //private static ILine line2;
        private static bool _exit;


        static void Main()
        {
            try
            {
                //Thread waitThread = new Thread(new ThreadStart(WaitCall));
                Console.WriteLine("Start Line {0}", 1);
                Thread waitThread1 = new Thread(() => WaitCall(1));

                Console.WriteLine("Start Line {0}", 2);
                Thread waitThread2 = new Thread(() => WaitCall(2));
                waitThread1.Start();
                waitThread2.Start();

                Console.WriteLine("All threads should be alive.");
                Console.ReadLine();
                //_exit = true;

                Console.WriteLine("Before Line Manager Release All");
                LineManager.ReleaseAll();
                Console.WriteLine("After Line Manager Release All");

                waitThread1.Join();
                waitThread2.Join();

                Console.WriteLine("Threads should now be dead.");

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static void WaitCall(int lineNumber)
        {
            try
            {
                Console.WriteLine("WaitCall: Line {0}: Get Line", lineNumber);
                var line = LineManager.GetLine(lineNumber);
                Console.WriteLine("WaitCall: Line {0}: Got Line", lineNumber);
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
                    catch (HangupException)
                    {
                        line.Hangup();
                    }

                }

                Console.WriteLine("WaitCall: Line {0}: End of Wait Call", lineNumber);
                line.Close();
                Console.WriteLine("WaitCall: Line {0}: End of Wait Call Line Closed", lineNumber);
            }
            catch (StopException)
            {
                Console.WriteLine("StopException on line {0}", lineNumber);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception on line {0}: {1}", lineNumber, e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
