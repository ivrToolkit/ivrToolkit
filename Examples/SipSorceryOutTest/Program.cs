using System;
using System.Threading;
using ivrToolkit.Core;
using ivrToolkit.Core.Enums;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Plugin.SipSorcery;
using Microsoft.Extensions.Logging;
using SipOutTest.ScriptBlocks;

namespace SipSorceryOutTest
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
            SIPSorcery.LogFactory.Set(loggerFactory);
            _logger.LogDebug("Starting the program!");

            var SipVoiceProperties = new SipVoiceProperties(loggerFactory, @"c:\repos\Config\SipSorcery\voice.properties");

            var sipPlugin = new SipPlugin(loggerFactory, SipVoiceProperties);

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
                    var waitThread = new Thread(() => WaitCall(loggerFactory, SipVoiceProperties, line, phoneNumber));

                    waitThread.Start();

                    _logger.LogDebug("Thread should be alive.");

                    // wait for the thread to end.
                    waitThread.Join(); 

                    _logger.LogDebug("Releasing all lines");
                    pluginManager.ReleaseAll();
                    Console.WriteLine();
                } // while
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
            finally
            {
                _logger.LogDebug("In finally of main thread.");
                pluginManager?.ReleaseAll();
                pluginManager?.Dispose();
            }
        }


        static void WaitCall(ILoggerFactory loggerFactory, VoiceProperties dialogicVoiceProperties, IIvrLine line, string phoneNumber)
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
                    switch (callAnalysis)
                    {
                        case CallAnalysis.Busy:
                        case CallAnalysis.Error:
                        case CallAnalysis.FaxTone:
                        case CallAnalysis.NoAnswer:
                        case CallAnalysis.NoDialTone:
                        case CallAnalysis.NoFreeLine:
                        case CallAnalysis.NoRingback:
                        case CallAnalysis.OperatorIntercept:
                        case CallAnalysis.Stopped:
                            return;
                        case CallAnalysis.Connected:
                            break;
                        case CallAnalysis.AnsweringMachine:
                            line.Hangup();
                            return;
                    }

                    _logger.LogDebug("callAnalysis is: {0}", callAnalysis );

                    try
                    {
                        var manager = new ScriptManager(loggerFactory, new WelcomeScript(loggerFactory, dialogicVoiceProperties, line));

                        while (manager.HasNext())
                        {
                            // execute the next script
                            manager.Execute();
                        }
                        _logger.LogDebug("scripts are done so hanging up.");
                        line.Hangup();
                    }
                    catch (HangupException)
                    {
                        _logger.LogDebug("Hangup Detected");
                        line.Hangup();
                    }
                    _logger.LogDebug("Disposing of line");
                    Thread.Sleep(5000); // todo remove me
                    line.Dispose();
                    _logger.LogDebug("Line is now disposed");
                    return;
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
