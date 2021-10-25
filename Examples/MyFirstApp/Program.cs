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
            // Make the working directory the location of the EXE file
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            return Host.CreateDefaultBuilder(args)

                // set this up as a windows service
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