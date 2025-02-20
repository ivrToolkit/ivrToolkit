using System;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core;
using ivrToolkit.Core.Enums;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Core.TTS;
using ivrToolkit.Plugin.SipSorcery;
using Microsoft.Extensions.Logging;

namespace TextToSpeech1;

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

        var ttsFactory = new AzureTtsFactory(loggerFactory, sipVoiceProperties);
        //var ttsFactory = new GoogleTtsFactory(loggerFactory, sipVoiceProperties);
        
        // create a line manager
        using var lineManager = new LineManager(loggerFactory, sipVoiceProperties, sipPlugin, ttsFactory);

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
        var ttsCacheFactory = line.TextToSpeechCacheFactory;
        
        try
        {
            // make a call out
            var callAnalysis = await line.DialAsync(phoneNumber, 3500, cancellationToken);
            if (callAnalysis == CallAnalysis.Connected)
            {
                // play TTS
                var ttsCache = ttsCacheFactory.Create("Thank you for using the <say-as interpret-as='characters'>IVR</say-as> Toolkit.", $"{WAV_FILE_LOCATION}/ThankYou.wav");
                await line.PlayFileAsync(ttsCache, cancellationToken);

                // play tts and wait for digits to be pressed
                var message = "For this simple demonstration, press <say-as interpret-as='characters'>1234</say-as> followed by the pound key.";
                ttsCache = ttsCacheFactory.Create(message, $"{WAV_FILE_LOCATION}/Press1234.wav");
                var result = await line.MultiTryPromptAsync(ttsCache, 
                    value => !string.IsNullOrEmpty(value),
                    cancellationToken);

                ttsCache = ttsCacheFactory.Create("You pressed", $"{WAV_FILE_LOCATION}/YouPressed.wav");
                await line.PlayFileAsync(ttsCache, cancellationToken);
                
                // speak out each digit of the result
                await line.PlayCharactersAsync(result, cancellationToken);

                // say Correct or Incorrect
                if (result == "1234")
                {
                    ttsCache = ttsCacheFactory.Create("Which is correct.", $"{WAV_FILE_LOCATION}/correct.wav");
                }
                else
                {
                    ttsCache = ttsCacheFactory.Create("Which is incorrect.", $"{WAV_FILE_LOCATION}/Incorrect.wav");
                }
                await line.PlayTextToSpeechAsync(ttsCache, cancellationToken);

                // Say goodbye
                ttsCache = ttsCacheFactory.Create("Goodbye.", $"{WAV_FILE_LOCATION}/goodbye.wav");
                await line.PlayTextToSpeechAsync(ttsCache, cancellationToken);

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