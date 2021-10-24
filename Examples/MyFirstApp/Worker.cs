using System;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core;
using ivrToolkit.Plugin.Dialogic.Analog;
using ivrToolkit.Plugin.Dialogic.Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MyFirstApp
{
    public class Worker : BackgroundService
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly DialogicVoiceProperties _voiceProperties;
        private readonly ILogger<Worker> _logger;
        private PluginManager _pluginManager;

        public Worker(ILoggerFactory loggerFactory, DialogicVoiceProperties voiceProperties)
        {
            _loggerFactory = loggerFactory;
            _voiceProperties = voiceProperties;
            _logger = loggerFactory.CreateLogger<Worker>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() =>
            {
                _logger.LogInformation("Starting the windows service.");
                try
                {
                    _logger.LogDebug("Starting the Dialogic Analog Plugin");
                    var ivrPlugin = new AnalogPlugin(_loggerFactory, _voiceProperties);
                    _pluginManager = new PluginManager(_loggerFactory, ivrPlugin);

                    _logger.LogDebug("Getting line 1");
                    var line1 = _pluginManager.GetLine(1);
                    var waitCall1 = new WaitCall(_loggerFactory, line1, _voiceProperties);
                    var thread1 = new Thread(waitCall1.Run);

                    _logger.LogDebug("Getting line 2");
                    var line2 = _pluginManager.GetLine(2);
                    var waitCall2 = new WaitCall(_loggerFactory, line2, _voiceProperties);
                    var thread2 = new Thread(waitCall2.Run);

                    thread1.Start();
                    thread2.Start();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                }
            }, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                _pluginManager?.ReleaseAll();
                _pluginManager?.Dispose();
            }, cancellationToken);
        }
    }

}
