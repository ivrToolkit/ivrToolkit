using System;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core;
using ivrToolkit.Core.Enums;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Plugin.SipSorcery;
using Microsoft.Extensions.Logging;

namespace AnsweringMachineDetection;

class Program
{
    private const string WAV_FILE_LOCATION = "Voice Files";
    private static ILogger<Program> _logger;
    
    static async Task Main()
    {
        var loggerFactory = BuildLoggerFactory();
        
        var cancellationToken = CancellationToken.None;
        
        // this is one way to set up your properties, with a property file
        var sipVoiceProperties = new SipVoiceProperties(loggerFactory, @"c:\repos\Config\SipSorcery\voice.properties")
        {
            // Used to cut out background noise. They higher the number, the more noise it will cut out.
            // this is likely more than high enough.
            AnsweringMachineSilenceThresholdAmplitude = 0.1f,
            // Wait for up to 3.0 seconds to hear the start of a voice. Stop the software from waiting forever if nobody
            // says anything.
            AnsweringMachineMaxStartSilenceSeconds = 3.0,
            // this one is probably unlikely to happen. Someone rambling on and on.
            AnsweringMachineGiveUpAfterSeconds = 10.0,
            // too long and people start to thing they have to say hello again. To short, and it might
            // detect a silence in the middle of the answering machine message (thus thinking it is not an answering machine)
            AnsweringMachineEndSpeechSilenceDurationSeconds = 1.5
        };
        
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
            
            // 0 - no answering machine detection
            // otherwise answering machine detection will occur. I recommend 3000 to 3500. (3 to 3.5 seconds)
            Console.Write("Answering Machine MilliSeconds (0 for no detection): ");
            var answer = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(answer)) return;
            
            var answeringMachineMilli = int.Parse(answer);
            
            await MakeOutBoundCallAsync(phoneNumber, answeringMachineMilli, line, cancellationToken);
            await Task.Delay(1000, CancellationToken.None);
        }
    }

    private static async Task MakeOutBoundCallAsync(string phoneNumber, int answeringMachineMilli, IIvrLine line,
        CancellationToken cancellationToken)
    {
        try
        {
            // make a call out
            var callAnalysis = await line.DialAsync(phoneNumber, answeringMachineMilli, cancellationToken);
            
            switch (callAnalysis)
            {
                case CallAnalysis.Connected:
                        await line.PlayFileAsync($"{WAV_FILE_LOCATION}/Person.wav", cancellationToken);
                    break;
                case CallAnalysis.AnsweringMachine:
                    await line.PlayFileAsync($"{WAV_FILE_LOCATION}/AnsweringMachine.wav", cancellationToken);
                    break;
                default:
                    return;
            }

            // finally hang up
            line.Hangup();
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