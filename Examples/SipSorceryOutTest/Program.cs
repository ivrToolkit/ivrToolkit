using System;
using System.Threading;
using System.Threading.Tasks;
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
        private static ILogger<Program> _logger;
        static async Task Main()
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

            var sipVoiceProperties = new SipVoiceProperties(loggerFactory, @"c:\repos\Config\SipSorcery\voice.properties");

            var sipPlugin = new SipSorceryPlugin(loggerFactory, sipVoiceProperties);

            LineManager lineManager = null;
            
            var cancellationToken = CancellationToken.None;

            try
            {
                lineManager = new LineManager(loggerFactory.CreateLogger<LineManager>(), sipPlugin);

                while (true)
                {
                    Console.Write("Enter a phone number to call: ");
                    var phoneNumber = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(phoneNumber)) return;

                    var ln = 1;
                    var line = lineManager.GetLine(ln);

                    _logger.LogDebug("Start Line {0}", ln);
                    await WaitCallAsync(loggerFactory, sipVoiceProperties, line, phoneNumber, cancellationToken);

                    _logger.LogDebug("Releasing all lines");
                    lineManager.ReleaseAll();
                    Console.WriteLine();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
            finally
            {
                _logger.LogDebug("In finally of main thread.");
                lineManager?.ReleaseAll();
                lineManager?.Dispose();
            }
        }


        static async Task WaitCallAsync(ILoggerFactory loggerFactory, VoiceProperties dialogicVoiceProperties, 
            IIvrLine line, string phoneNumber, CancellationToken cancellationToken)
        {
            var lineNumber = line.LineNumber;
            try
            {
                _logger.LogDebug("Dial: Line {0}: Got Line", lineNumber);
                while (true)
                {
                    _logger.LogDebug("Dial: Line {0}: Hang Up", lineNumber);
                    line.Hangup();
                    await Task.Delay(1000);

                    _logger.LogDebug("Dial: Line {0}: dialing {1}...", lineNumber, phoneNumber);
                    var callAnalysis = await line.DialAsync(phoneNumber, 3500, cancellationToken);
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
                            //line.Hangup();
                            return;
                    }

                    _logger.LogDebug("callAnalysis is: {0}", callAnalysis );

                    try
                    {
                        var manager = new ScriptManager(loggerFactory, new WelcomeScript(loggerFactory, dialogicVoiceProperties, line));
                        await manager.ExecuteScriptAsync(cancellationToken);

                        _logger.LogDebug("scripts are done so hanging up.");
                        line.Hangup();
                    }
                    catch (HangupException)
                    {
                        _logger.LogDebug("Hangup Detected");
                        line.Hangup();
                    }
                    _logger.LogDebug("Disposing of line");
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
