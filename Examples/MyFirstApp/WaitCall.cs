using System;
using System.Threading;
using ivrToolkit.Core;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Plugin.Dialogic.Common;
using Microsoft.Extensions.Logging;
using MyFirstApp.ScriptBlocks;

namespace MyFirstApp
{
    public class WaitCall
    {
        private readonly ILogger<WaitCall> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly DialogicVoiceProperties _voiceProperties;
        private readonly IIvrLine _line;

        public WaitCall(ILoggerFactory loggerFactory, IIvrLine line, DialogicVoiceProperties voiceProperties)
        {
            _voiceProperties = voiceProperties;
            _loggerFactory = loggerFactory;
            _line = line;
            _logger = loggerFactory.CreateLogger<WaitCall>();
        }

        public void Run()
        {
            var lineNumber = _line.LineNumber;
            try
            {
                _logger.LogInformation("WaitCall: Line {0}: Got Line", lineNumber);
                while (true)
                {
                    _logger.LogInformation("WaitCall: Line {0}: Hang Up", lineNumber);
                    _line.Hangup();
                    Thread.Sleep(1000);
                    _logger.LogInformation("WaitCall: Line {0}: Wait Rings", lineNumber);
                    _line.WaitRings(1);

                    try
                    {
                        var manager = new ScriptManager(_loggerFactory, new WelcomeScript(_loggerFactory, _voiceProperties, _line));

                        while (manager.HasNext())
                        {
                            // execute the next script
                            manager.Execute();
                        }

                        _line.Hangup();
                    }
                    catch (HangupException)
                    {
                        _line.Hangup();
                    }

                }
            }
            catch (DisposingException)
            {
                _logger.LogDebug("DisposingException on line {0}", lineNumber);
                _line.Dispose();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception on line {0}: {1}", lineNumber, e.Message);
            }
        }
    }
}