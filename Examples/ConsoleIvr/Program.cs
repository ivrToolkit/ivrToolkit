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

            int offset = Int32.Parse(VoiceProperties.Current.GetProperty("sip.channel_offset"));

            //Thread waitThread = new Thread(new ThreadStart(WaitCall));
            int LineNumber = 1 + offset;
            Console.WriteLine("Start Line {0}", LineNumber);
            Thread waitThread = new Thread(() => WaitCall(LineNumber));
            waitThread.Start();
            while (!waitThread.IsAlive) ;
            Thread.Sleep(10000);

            LineNumber = 2 + offset;
            Console.WriteLine("Start Line {0}", LineNumber);
            //Thread makeCallThread = new Thread(new ThreadStart(MakeCall));
            Thread makeCallThread = new Thread(() => MakeCall(LineNumber));
            makeCallThread.Start();
            while (!makeCallThread.IsAlive) ;
            Thread.Sleep(1);

            //LineNumber = 10;
            //Console.WriteLine("Start Line {0}", LineNumber);
            //Thread waitThreadNine = new Thread(() => WaitCall(LineNumber));
            //waitThreadNine.Start();
            //while (!waitThreadNine.IsAlive) ;
            //Thread.Sleep(1);


            Console.WriteLine("All threads should be alive.");
            Console.ReadLine();
            exit = true;
            Thread.Sleep(1);

            //waitThreadNine.Abort();
            //waitThreadNine.Join();

            waitThread.Abort();
            waitThread.Join();

            makeCallThread.Abort();
            makeCallThread.Join();

            Console.WriteLine("Before Line Manager Release All");
            LineManager.ReleaseAll();

            Console.WriteLine("Threads should now be dead.");
            //TestLineManager();
        }



        static void MakeCall(int LineNumber)
        {

            Console.WriteLine("MakeCall: Line {0}: Get Line", LineNumber);
            ILine line2 = LineManager.GetLine(LineNumber);
            Console.WriteLine("################################MakeCall: Line {0}: Got Line", LineNumber);
            while (!exit)
            {
               

                try
                {
                    // good idea to make sure the Line was hung up properly
                    line2.Hangup();
                    Console.WriteLine("MakeCall: Line {0}: Hang Up", LineNumber);
                    Thread.Sleep(1000);
                    Console.WriteLine("MakeCall: Line {0}: Dial", LineNumber);
                    CallAnalysis result = line2.Dial("7782320255", 3500);
                    if (LineNumber >= 4) {
                        line2.PlayFile("System Recordings\\Monday.wav");
                    }
                    else
                    {
                        line2.PlayFile("System Recordings\\Thursday.wav");
                    }

                    //line2.RecordToFile("C:\\ads\\data\\database\\E15226.wav");
                    line2.Hangup();
                    Thread.Sleep(60000);

                }
                catch (ivrToolkit.Core.Exceptions.HangupException)
                {
                    line2.Hangup();

                }
                Thread.Sleep(300000);
            }
            Console.WriteLine("MakeCall: Line {0}: End of Make Call", LineNumber);
            line2.Close();
            Console.WriteLine("MakeCall: Line {0}: End of Make Call Line Closed", LineNumber);
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
        static void WaitCall(int LineNumber)
        {
            Console.WriteLine("WaitCall: Line {0}: Get Line", LineNumber);
            ILine line = LineManager.GetLine(LineNumber);
            Console.WriteLine("################################WaitCall: Line {0}: Got Line", LineNumber);
            while (!exit)
            {
                Console.WriteLine("WaitCall: Line {0}: Hang Up", LineNumber);
                line.Hangup();
                Thread.Sleep(1000);
                Console.WriteLine("WaitCall: Line {0}: Wait Rings", LineNumber);
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
            Console.WriteLine("WaitCall: Line {0}: End of Wait Call", LineNumber);
            line.Close();
            Console.WriteLine("WaitCall: Line {0}: End of Wait Call Line Closed", LineNumber);
        }
    }
}
