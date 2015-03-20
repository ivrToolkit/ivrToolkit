using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private static bool exit = false;

        static void Main(string[] args)
        {
            Console.WriteLine("Start Line 1");

            //Thread waitThread = new Thread(new ThreadStart(WaitCall));
            Thread waitThread = new Thread(WaitCall);
            waitThread.Start();
            while (!waitThread.IsAlive) ;
            Thread.Sleep(10000);

            Console.WriteLine("Start Line 2");
            //Thread makeCallThread = new Thread(new ThreadStart(MakeCall));
            Thread makeCallThread = new Thread(MakeCall);
            makeCallThread.Start();
            while (!makeCallThread.IsAlive) ;
            Thread.Sleep(1);

            Console.WriteLine("All threads should be alive.");
            Console.ReadLine();
            exit = true;
            Thread.Sleep(1);

            waitThread.Abort();
            waitThread.Join();

            makeCallThread.Abort();
            makeCallThread.Join();

            Console.WriteLine("Threads should now be dead.");
            //TestLineManager();
        }

        static void MakeCall()
        {
            
            Console.WriteLine("MakeCall: Line 2: Get Line");
            ILine line2 = LineManager.GetLine(2);
            Console.WriteLine("################################MakeCall: Line 2: Got Line");
            while (!exit)
            {
                Thread.Sleep(30000);

                try
                {
                    // good idea to make sure the Line was hung up properly
                    line2.Hangup();
                    Console.WriteLine("MakeCall: Line 2: Hang Up");
                    Thread.Sleep(1000);
                    Console.WriteLine("MakeCall: Line 2: Dial");
                    CallAnalysis result = line2.Dial("7782320255", 3500);
                    line2.PlayFile("System Recordings\\Thursday.wav");

                    //line2.RecordToFile("C:\\ads\\data\\database\\E15226.wav");
                    line2.Hangup();

                }
                catch (ivrToolkit.Core.Exceptions.HangupException)
                {
                    line2.Hangup();

                }
            }
            Console.WriteLine("MakeCall: Line 2: End of Make Call");
            line2.Close();
            Console.WriteLine("MakeCall: Line 2: End of Make Call Line Closed");
        }
        /*
        static void TestLineManager() {
            line = LineManager.GetLine(1);
            line.Hangup();
            Console.WriteLine("Got Line 1");
            line2 = LineManager.GetLine(2);
            line2.Hangup();
            Console.WriteLine("Got Line 2");

            Console.WriteLine("Line 1 {0}", line.LineNumber);
            Console.WriteLine("Line 2 {0}", line2.LineNumber);
            //Console.WriteLine("BEFORE CLOSE Line 2");
            //line2.Close();
            //Console.WriteLine("BEFORE CLOSE Line 1");
            //line.Close();
            Console.WriteLine("BEFORE RELEASE ALL");
            LineManager.ReleaseAll();
        }
        */
        static void WaitCall()
        {
            Console.WriteLine("WaitCall: Line 1: Get Line");
           ILine line = LineManager.GetLine(1);
           Console.WriteLine("################################WaitCall: Line 1: Got Line");
            while (!exit)
            {
                Console.WriteLine("WaitCall: Line 1: Hang Up");
                line.Hangup();
                Thread.Sleep(1000);
                Console.WriteLine("WaitCall: Line 1: Wait Rings");
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
            Console.WriteLine("WaitCall: Line 1: End of Wait Call");
            line.Close();
            Console.WriteLine("WaitCall: Line 1: End of Wait Call Line Closed");
        }
    }
}
