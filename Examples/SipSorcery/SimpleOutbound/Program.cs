using System;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core;
using ivrToolkit.Core.Enums;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Plugin.SipSorcery;
using Microsoft.Extensions.Logging;

namespace SimpleOutbound;

class Program
{
    private const string WAV_FILE_LOCATION = "Voice Files";
    private static ILogger<Program> _logger;
    
    static async Task Main()
    {
        var loggerFactory = BuildLoggerFactory();
        
        var cancellationToken = CancellationToken.None;
        
        // this is one way to set up your properties, with a property file
        var sipVoiceProperties = new SipVoiceProperties(loggerFactory, @"c:\repos\Config\SipSorcery\voice.properties");
        
        // instantiate the plugin you want to use
        using var sipPlugin = new SipSorceryPlugin(loggerFactory, sipVoiceProperties);
        
        // create a line manager
        using var lineManager = new LineManager(loggerFactory, sipVoiceProperties, sipPlugin);

        // grab a line
        var line = lineManager.GetLine();
        
        while (true)
        {
            Console.Write("Enter a phone number to call: ");
            var phoneNumber = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(phoneNumber)) return;
            
            await MakeOutBoundCallAsync(phoneNumber, line, cancellationToken);
            await Task.Delay(1000, CancellationToken.None);
        }
    }

    private static async Task MakeOutBoundCallAsync(string phoneNumber, IIvrLine line, CancellationToken cancellationToken)
    {
        try
        {
            // make a call out
            var callAnalysis = await line.DialAsync(phoneNumber, 3500, cancellationToken);
            if (callAnalysis == CallAnalysis.Connected)
            {
                // play a wav file
                await line.PlayFileAsync($"{WAV_FILE_LOCATION}/ThankYou.wav", cancellationToken);

                // play a wav file and wait for digits to be pressed
                var result = await line.PromptAsync($"{WAV_FILE_LOCATION}/Press1234.wav", cancellationToken);

                // play another wav file
                await line.PlayFileAsync($"{WAV_FILE_LOCATION}/YouPressed.wav", cancellationToken);

                // speak out each digit of the result
                await line.PlayCharactersAsync(result, cancellationToken);

                // say Correct or Incorrect
                await line.PlayFileAsync(
                    result == "1234" ? $"{WAV_FILE_LOCATION}/Correct.wav" : @"Voice Files\Incorrect.wav",
                    cancellationToken);

                // Say goodbye
                await line.PlayFileAsync($"{WAV_FILE_LOCATION}/goodbye.wav", cancellationToken);

                // finally hang up
                line.Hangup();
            }
        }
        catch (HangupException)
        {
            _logger.LogDebug("The person that was called hung up");
        }
        catch (TooManyAttempts)
        {
            _logger.LogDebug("There have been too many attempts to answer the question");
            line.Hangup();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            line.Hangup();
        }
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