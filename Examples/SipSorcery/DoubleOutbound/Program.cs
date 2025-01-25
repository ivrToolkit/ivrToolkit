using System;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core;
using ivrToolkit.Core.Enums;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Plugin.SipSorcery;
using Microsoft.Extensions.Logging;

namespace DoubleOutbound;

class Program
{
    private const string WAV_FILE_LOCATION = "Voice Files";
    private static ILogger<Program> _logger;
    
    static async Task Main()
    {
        var loggerFactory = BuildLoggerFactory();
        
        Console.Write("Enter a phone number to call: ");
        var phoneNumber1 = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(phoneNumber1)) return;

        Console.Write($"Enter a second phone number to call - [{phoneNumber1}]: ");
        var phoneNumber2 = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(phoneNumber2)) phoneNumber2 = phoneNumber1;
        

        Console.Write($"Seconds to wait before second call starts: ");
        var secondsString = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(secondsString)) return;

        var worked = int.TryParse(secondsString, out var seconds);
        if (!worked) return;
        
        var cancellationToken = CancellationToken.None;
        
        // this is one way to set up your properties, with a property file
        var propertiesFromFile = new SipVoiceProperties(loggerFactory, @"c:\repos\Config\SipSorcery\voice.properties");
        
        var sipServer = propertiesFromFile.SipServer;
        var sipUsername = propertiesFromFile.SipUsername;
        var password = propertiesFromFile.SipPassword;
        
        // otherwise, you can just set up your properties this way
        var sipVoiceProperties = new SipVoiceProperties(loggerFactory)
        {
            SipServer = sipServer,
            SipUsername = sipUsername,
            SipPassword = password
        };
        
        // instantiate the plugin you want to use
        using var sipPlugin = new SipSorceryPlugin(loggerFactory, sipVoiceProperties);
        
        // create a line manager
        using var lineManager = new LineManager(loggerFactory, sipVoiceProperties, sipPlugin);
        
        // grab a line
        var line1 = lineManager.GetLine();
        // grab another line
        var line2 = lineManager.GetLine();

        // begin the first call
        var task1 = HandleLineAsync(line1, phoneNumber1, cancellationToken);

        // begin the second call
        await Task.Delay(seconds * 1000, cancellationToken);
        var task2 = HandleLineAsync(line2, phoneNumber2, cancellationToken);
        
        // wait for both to complete
        Task.WaitAll([task1, task2], cancellationToken);
        
        Console.Write("Press any key to continue...: ");
        Console.ReadKey();
    }


    private static async Task HandleLineAsync(IIvrLine line, string phoneNumber, CancellationToken cancellationToken)
    {
        try
        {
            // make a call out
            var callAnalysis = await line.DialAsync(phoneNumber, 3500, cancellationToken);
            if (callAnalysis == CallAnalysis.Connected)
            {
                // play a wav file
                await line.PlayFileAsync($"{WAV_FILE_LOCATION}/ThankYou.wav", cancellationToken);
            
                // a user has to enter some digit or it is considered false and will ask again.
                var result = await line.MultiTryPromptAsync($"{WAV_FILE_LOCATION}/Press1234.wav",
                    value => value != "", 
                    cancellationToken);
            
                // play another wav file
                await line.PlayFileAsync($"{WAV_FILE_LOCATION}/YouPressed.wav", cancellationToken);

                // speak out each digit of the result
                await line.PlayCharactersAsync(result, cancellationToken);
            
                // say Correct or Incorrect
                await line.PlayFileAsync(result == "1234" ? $"{WAV_FILE_LOCATION}/Correct.wav" : @"Voice Files\Incorrect.wav", cancellationToken);

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