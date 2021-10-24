using System;
using System.IO;
using ivrToolkit.Plugin.Dialogic.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MyFirstApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            return Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((_, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddSingleton(provider =>
                        new DialogicVoiceProperties(provider.GetService<ILoggerFactory>(), "voice.properties"));
                });
        }
    }
}