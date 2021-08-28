using System;
using System.Threading;
using ivrToolkit.Core;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Plugin.Dialogic.Sip;
using Microsoft.Extensions.Logging;
using SipOutTest.ScriptBlocks;

namespace SipOutTest
{
    class Program
    {
        public static ILogger<Program> _logger;
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

            _logger = loggerFactory.CreateLogger<Program>();
            _logger.LogDebug("Starting the program!");

            var dialogicSipVoiceProperties = new DialogicSipVoiceProperties(loggerFactory, "voice.properties");

            var sipPlugin = new SipPlugin(loggerFactory, dialogicSipVoiceProperties);

            PluginManager pluginManager = null;

            try
            {
                pluginManager = new PluginManager(loggerFactory, sipPlugin);

                while (true)
                {
                    Thread.Sleep(3000);
                    Console.Write("Enter a line number: ");
                    var lineNumber = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(lineNumber)) return;

                    Console.Write("Enter a phone number to call: ");
                    var phoneNumber = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(phoneNumber)) return;

                    var ln = int.Parse(lineNumber);
                    var line = pluginManager.GetLine(ln);

                    _logger.LogDebug("Start Line {0}", ln);
                    var waitThread1 = new Thread(() => WaitCall(loggerFactory, dialogicSipVoiceProperties, line, phoneNumber));

                    waitThread1.Start();

                    _logger.LogDebug("All threads should be alive.");
                    Console.ReadLine();

                    _logger.LogDebug("Releasing all lines");
                    pluginManager.ReleaseAll();
                    waitThread1.Join();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
            finally
            {
                pluginManager?.ReleaseAll();
                pluginManager?.Dispose();
            }
        }


        static void WaitCall(ILoggerFactory loggerFactory, VoiceProperties dialogicVoiceProperties, ILine line, string phoneNumber)
        {
            var lineNumber = line.LineNumber;
            try
            {
                _logger.LogDebug("Dial: Line {0}: Got Line", lineNumber);
                while (true)
                {
                    _logger.LogDebug("Dial: Line {0}: Hang Up", lineNumber);
                    line.Hangup();
                    Thread.Sleep(1000);
                    _logger.LogDebug("Dial: Line {0}: dialing {1}...", lineNumber, phoneNumber);
                    var callAnalysis = line.Dial(phoneNumber, 3500);
                    _logger.LogDebug("callAnalysis is: {0}", callAnalysis );

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
                _logger.LogDebug("DisposingException on line {0}", lineNumber);
                line.Dispose();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception on line {0}", lineNumber);
            }
        }

    }
}
