using System;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core;
using ivrToolkit.Core.Enums;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Plugin.SipSorcery;
using Microsoft.Extensions.Logging;

namespace RecordToFile;

class Program
{
    private const string WAV_FILE_LOCATION = "Voice Files";
    private static ILogger<Program> _logger;
    
    static async Task Main()
    {
        var loggerFactory = BuildLoggerFactory();
        
        Console.Write("Enter a phone number to call: ");
        var phoneNumber = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(phoneNumber)) return;
        
        var cancellationToken = CancellationToken.None;
        
        // this is one way to set up your properties, with a property file
        var propertiesFromFile = new SipVoiceProperties(loggerFactory, @"c:\repos\Config\SipSorcery\voice.properties");

        var sipServer = propertiesFromFile.SipServer;
        var sipUsername = propertiesFromFile.SipUsername;
        var password = propertiesFromFile.SipPassword;
        var sipLocalEndpoint = propertiesFromFile.SipLocalEndpoint;
        
        // otherwise, you can just set up your properties this way
        var sipVoiceProperties = new SipVoiceProperties(loggerFactory)
        {
            SipServer = sipServer,
            SipUsername = sipUsername,
            SipPassword = password,
            SipLocalEndpoint = sipLocalEndpoint,
        };
        
        // instantiate the plugin you want to use
        using var sipPlugin = new SipSorceryPlugin(loggerFactory, sipVoiceProperties);
        
        // create a line manager
        using var lineManager = new LineManager(loggerFactory, sipVoiceProperties, sipPlugin);
        
        // grab a line
        var line = lineManager.GetLine();
        
        try
        {
            // make a call out
            var callAnalysis = await line.DialAsync(phoneNumber, 0, cancellationToken);
            if (callAnalysis == CallAnalysis.Connected)
            {
                // play a wav file
                await line.PlayFileAsync($"{WAV_FILE_LOCATION}/ThankYou.wav", cancellationToken);

                // play a wav file
                await line.PlayFileAsync($"{WAV_FILE_LOCATION}/RecordMessage.wav", cancellationToken);
                
                // record a message - 5 minutes max
                await line.RecordToFileAsync("myRecording.wav", cancellationToken);
                
                // play another wav file
                await line.PlayFileAsync($"{WAV_FILE_LOCATION}/YouRecorded.wav", cancellationToken);
                
                // speak out the recorded message
                await line.PlayFileAsync("myRecording.wav", cancellationToken);
                
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