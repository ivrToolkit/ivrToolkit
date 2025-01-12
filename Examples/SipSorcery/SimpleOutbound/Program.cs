using System;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core;
using ivrToolkit.Core.Enums;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Extensions;
using ivrToolkit.Plugin.SipSorcery;
using Microsoft.Extensions.Logging;

namespace SimpleOutbound;

class Program
{
    private static ILogger<Program> _logger;
    static async Task Main()
    {
        var loggerFactory = BuildLoggerFactory();
        
        Console.Write("Enter a phone number to call: ");
        var phoneNumber = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(phoneNumber)) return;
        
        
        
        var sipVoiceProperties = new SipVoiceProperties(loggerFactory, @"c:\repos\Config\SipSorcery\voice.properties");
        var sipPlugin = new SipSorceryPlugin(loggerFactory, sipVoiceProperties);

        var cancellationToken = CancellationToken.None;

        LineManager lineManager = null;
        try
        {
            lineManager = new LineManager(loggerFactory.CreateLogger<LineManager>(), sipPlugin);
            
            var line = lineManager.GetLine(1);

            var callAnalysis = await line.DialAsync(phoneNumber, 3500, cancellationToken);
            if (callAnalysis == CallAnalysis.Connected)
            {
                await line.PlayFileAsync(@"Voice Files\ThankYou.wav", cancellationToken);
                
                // single attempt prompt
                var result = await line.PromptAsync(@"Voice Files\Press1234.wav", cancellationToken);
                
                await line.PlayFileAsync(@"Voice Files\YouPressed.wav", cancellationToken);

                await line.PlayCharactersAsync(result, cancellationToken);

                await line.PlayFileAsync(result == "1234" ? @"Voice Files\Correct.wav" : @"Voice Files\Incorrect.wav", cancellationToken);
                await line.PlayFileAsync(@"Voice Files\goodbye.wav", cancellationToken);
                line.Hangup();
            }

        }
        catch (HangupException e)
        {
            _logger.LogError(e, e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
        finally
        {
            lineManager?.ReleaseAll();
            lineManager?.Dispose();
        }
        
        
        Console.Write("Press any key to continue...: ");
        Console.ReadKey();
    }

    private static ILoggerFactory BuildLoggerFactory()
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
        return loggerFactory;
    }



}