using ivrToolkit.Core.TTS;
using ivrToolkit.Plugin.SipSorcery;
using Microsoft.Extensions.Logging;

var loggerFactory = BuildLoggerFactory();
var logger = loggerFactory.CreateLogger<Program>();
        
var cancellationToken = CancellationToken.None;

// this is one way to set up your properties, with a property file
var sipVoiceProperties = new SipVoiceProperties(loggerFactory, @"c:\repos\Config\SipSorcery\voice.properties")
{
    DefaultWavSampleRate = 8000,
    TtsAzureVoice = "en-US-JennyNeural"
};

// i'm using azure TTS
var ttsFactory = new AzureTtsFactory(loggerFactory, sipVoiceProperties);
var ttsEngine = ttsFactory.Create();

Directory.CreateDirectory(sipVoiceProperties.TtsAzureVoice);

while (true)
{
    Console.Write("Enter text to be processed: ");
    var text = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(text)) return;

    Console.Write("Enter output wav file name: ");
    var output = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(output)) return;
    
    if (!output.ToLower().EndsWith(".wav")) output += ".wav";

    Console.Write("Go? [Y/N]: <N>");
    var go = Console.ReadLine();

    if (go?.ToLower() == "y")
    {
        Convert(text, output);
    }

}

ILoggerFactory BuildLoggerFactory()
{
    var loggerFactoryInner =
        LoggerFactory.Create(builder =>
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "hh:mm:ss ";
            }).AddFilter("*", LogLevel.Debug));

    SIPSorcery.LogFactory.Set(loggerFactoryInner);
    return loggerFactoryInner;
}


void Convert(string text, string output)
{
    var audioStream = ttsEngine.TextToSpeech($"{text} <break time='100ms'/>");
    File.WriteAllBytes($"{sipVoiceProperties.TtsAzureVoice}\\{output}", audioStream.ToArray());
}

