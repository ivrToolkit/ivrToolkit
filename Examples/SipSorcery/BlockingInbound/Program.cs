using System;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Plugin.SipSorcery;
using Microsoft.Extensions.Logging;

namespace BlockingInbound;

/// <summary>
/// This program shows how to use the legacy WaitRings method. I recommend using <see cref="IIvrLine.StartIncomingListener"/>  instead.
/// However, WaitRings does provide a way to migrating any Dialogic apps that use WaitRings without having to
/// make code changes.
/// </summary>
class Program
{
    private const string WAV_FILE_LOCATION = "Voice Files";
    private static ILogger<Program> _logger;
    
    static async Task Main()
    {
        var loggerFactory = BuildLoggerFactory();
        
        var cancellationToken = CancellationToken.None;
        
        // this is one way to set up your properties, with a property file
        var propertiesFromFile = new SipVoiceProperties(loggerFactory, @"c:\repos\Config\SipSorcery\voice.properties")
        {
            // normally this is 5 but I needed the line to stay busy while I call in with another phone.
            PromptBlankAttempts = 99
        };
        
        // instantiate the plugin you want to use
        using var sipPlugin = new SipSorceryPlugin(loggerFactory, propertiesFromFile);
        
        // create a line manager
        using var lineManager = new LineManager(loggerFactory, propertiesFromFile, sipPlugin);
        
        // grab a line
        var line1 = lineManager.GetLine();

        // block until a call is answered.
        await line1.WaitRingsAsync(3, cancellationToken);
        
        var task = HandleIncomingCallAsync(line1, cancellationToken);
        
        Console.Write("Press any key to dispose...: ");
        Console.ReadKey();
        
        // dispose of the line
        lineManager.ReleaseAll();

        // wait until the call gets disconnected which should be immediately because of ReleaseAll()
        await task;
        
        Console.Write("Press any key to continue...: ");
        Console.ReadKey();
    }

    private static async Task HandleIncomingCallAsync(IIvrLine line, CancellationToken cancellationToken)
    {
        _logger.LogDebug("line #{lineNo}", line.LineNumber);
        try
        {
            // play a wav file
            await line.PlayFileAsync($"{WAV_FILE_LOCATION}/ThankYou.wav", cancellationToken);

            // play a wav file and wait for digits to be pressed
            var result = await line.MultiTryPromptAsync($"{WAV_FILE_LOCATION}/Press1234.wav", 
                value => !string.IsNullOrEmpty(value),
                cancellationToken);

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
        catch (HangupException)
        {
            _logger.LogDebug("The person hung up");
        }
        catch (TooManyAttempts)
        {
            _logger.LogDebug("There have been too many attempts");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Exception occured");
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
        return loggerFactory;
    }
}
