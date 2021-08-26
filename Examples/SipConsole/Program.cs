using System;
using System.Threading;
using ivrToolkit.Core;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Plugin.Dialogic.Sip;
using Microsoft.Extensions.Logging;
using SipConsole.ScriptBlocks;

namespace SipConsole
{
    class Program
    {
        static void Main()
        {
            using ILoggerFactory loggerFactory =
                LoggerFactory.Create(builder =>
                    builder.AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = true;
                        options.SingleLine = true;
                        options.TimestampFormat = "hh:mm:ss ";
                    }).AddFilter("*", LogLevel.Debug));

            var logger = loggerFactory.CreateLogger<Program>();
            logger.LogDebug("Starting the program!");

            var dialogicVoiceProperties = new VoiceProperties(loggerFactory, "voice.properties");

            // there are two ways to setup the PluginManager.
            // 1) instantiate the plugin and inject it into the PluginManager
            // 2) Have the PluginManager instantiate the plugin from voice.properties (original way)

            var sipPlugin = new SipPlugin(loggerFactory, dialogicVoiceProperties);

            PluginManager pluginManager = null;

            // Optionally setup the plugin from voice.properties
            //pluginManager = new PluginManager(loggerFactory, dialogicVoiceProperties);
            try
            {
                pluginManager = new PluginManager(loggerFactory, sipPlugin);


                var line = pluginManager.GetLine(1);

                Console.WriteLine("Start Line {0}", 1);
                var waitThread1 = new Thread(() => WaitCall(loggerFactory, dialogicVoiceProperties, line));

                waitThread1.Start();

                Console.WriteLine("All threads should be alive.");
                Console.ReadLine();

                Console.WriteLine("Before Line Manager Release All");
                pluginManager.ReleaseAll();
                Console.WriteLine("After Line Manager Release All");

                waitThread1.Join();

                Console.WriteLine("Threads should now be dead.");
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
            }
            finally
            {
                pluginManager?.ReleaseAll();
                pluginManager?.Dispose();
            }
        }


        static void WaitCall(ILoggerFactory loggerFactory, VoiceProperties dialogicVoiceProperties, ILine line)
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
                        var manager = new ScriptManager(loggerFactory, line, new WelcomeScript(loggerFactory, dialogicVoiceProperties));

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
