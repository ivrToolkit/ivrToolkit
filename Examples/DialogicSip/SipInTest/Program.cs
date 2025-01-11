using System;
using System.Threading;
using ivrToolkit.Core;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Plugin.Dialogic.Sip;
using Microsoft.Extensions.Logging;
using SipInTest.ScriptBlocks;

namespace SipInTest
{
    class Program
    {
        static void Main()
        {
            var loggerFactory =
                LoggerFactory.Create(builder =>
                    builder.AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = true;
                        options.SingleLine = true;
                        options.TimestampFormat = "hh:mm:ss ";
                    }).AddFilter("*", LogLevel.Debug));

            var logger = loggerFactory.CreateLogger<Program>();
            logger.LogDebug("Starting the program!");

            var dialogicSipVoiceProperties = new DialogicSipVoiceProperties(loggerFactory, "voice.properties");

            var sipPlugin = new SipPlugin(loggerFactory, dialogicSipVoiceProperties);

            LineManager lineManager = null;

            try
            {
                lineManager = new LineManager(loggerFactory.CreateLogger<LineManager>(), sipPlugin);


                var line = lineManager.GetLine(1);

                Console.WriteLine("Start Line {0}", 1);
                var waitThread1 = new Thread(() => WaitCall(loggerFactory, dialogicSipVoiceProperties, line));

                waitThread1.Start();

                Console.WriteLine("All threads should be alive.");
                Console.ReadLine();

                Console.WriteLine("Releasing all lines");
                lineManager.ReleaseAll();
                waitThread1.Join();
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
            }
            finally
            {
                lineManager?.ReleaseAll();
                lineManager?.Dispose();
            }
        }


        static void WaitCall(ILoggerFactory loggerFactory, VoiceProperties dialogicVoiceProperties, IIvrLine line)
        {
            var lineNumber = line.LineNumber;
            try
            {
                Console.WriteLine("WaitCall: Line {0}: Got Line", lineNumber);
                while (true)
                {
                    Console.WriteLine("WaitCall: Line {0}: Hang Up", lineNumber);
                    line.Hangup();
                    Thread.Sleep(1000);
                    Console.WriteLine("WaitCall: Line {0}: Wait Rings", lineNumber);
                    line.WaitRings(2);

                    try
                    {
                        var manager = new ScriptManager(loggerFactory, new WelcomeScript(loggerFactory, dialogicVoiceProperties, line));

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
            }
            catch (DisposingException)
            {
                Console.WriteLine("DisposingException on line {0}", lineNumber);
                line.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception on line {0}: {1}", lineNumber, e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

    }
}
