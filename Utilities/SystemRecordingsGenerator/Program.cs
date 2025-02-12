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

File.Copy(Path.Combine("System Recordings", "beep.wav"), Path.Combine(sipVoiceProperties.TtsAzureVoice, "beep.wav"), true);
File.Copy(Path.Combine("System Recordings", "pause.wav"), Path.Combine(sipVoiceProperties.TtsAzureVoice, "pause.wav"), true);

Convert("0", "0.wav");
Convert("hundred hours", "00 hours.wav");
Convert("1", "1.wav");
Convert("10", "10.wav");
Convert("100", "100.wav");
Convert("11", "11.wav");
Convert("12", "12.wav");
Convert("13", "13.wav");
Convert("14", "14.wav");
Convert("15", "15.wav");
Convert("16", "16.wav");
Convert("17", "17.wav");
Convert("18", "18.wav");
Convert("19", "19.wav");
Convert("2", "2.wav");
Convert("20", "20.wav");
Convert("200", "200.wav");
Convert("3", "3.wav");
Convert("30", "30.wav");
Convert("300", "300.wav");
Convert("4", "4.wav");
Convert("40", "40.wav");
Convert("400", "400.wav");
Convert("5", "5.wav");
Convert("50", "50.wav");
Convert("500", "500.wav");
Convert("6", "6.wav");
Convert("60", "60.wav");
Convert("600", "600.wav");
Convert("7", "7.wav");
Convert("70", "70.wav");
Convert("700", "700.wav");
Convert("8", "8.wav");
Convert("80", "80.wav");
Convert("800", "800.wav");
Convert("9", "9.wav");
Convert("90", "90.wav");
Convert("900", "900.wav");
Convert("a", "a.wav");
Convert("<say-as interpret-as='characters'>am</say-as>", "am.wav");
Convert("and", "and.wav");
Convert("April", "April.wav");
Convert("August", "August.wav");
Convert("b", "b.wav");
Convert("billion", "billion.wav");
Convert("c", "c.wav");
Convert("cent", "cent.wav");
Convert("cents", "cents.wav");
Convert("d", "d.wav");
Convert("December", "December.wav");
Convert("dollar", "dollar.wav");
Convert("dollars", "dollars.wav");
Convert("e", "e.wav");
Convert("f", "f.wav");
Convert("February", "February.wav");
Convert("Friday", "Friday.wav");
Convert("g", "g.wav");
Convert("h", "h.wav");
Convert("i", "i.wav");
Convert("j", "j.wav");
Convert("January", "January.wav");
Convert("July", "July.wav");
Convert("June", "June.wav");
Convert("k", "k.wav");
Convert("l", "l.wav");
Convert("m", "m.wav");
Convert("March", "March.wav");
Convert("May", "May.wav");
Convert("million", "million.wav");
Convert("Monday", "Monday.wav");
Convert("n", "n.wav");
Convert("negative", "negative.wav");
Convert("November", "November.wav");
Convert("o", "o.wav");
Convert("October", "October.wav");
Convert("oh", "oh.wav");
Convert("1st", "ord1.wav");
Convert("10th", "ord10.wav");
Convert("11th", "ord11.wav");
Convert("12th", "ord12.wav");
Convert("13th", "ord13.wav");
Convert("14th", "ord14.wav");
Convert("15th", "ord15.wav");
Convert("16th", "ord16.wav");
Convert("17th", "ord17.wav");
Convert("18th", "ord18.wav");
Convert("19th", "ord19.wav");
Convert("2nd", "ord2.wav");
Convert("20th", "ord20.wav");
Convert("21st", "ord21.wav");
Convert("22nd", "ord22.wav");
Convert("23rd", "ord23.wav");
Convert("24th", "ord24.wav");
Convert("25th", "ord25.wav");
Convert("26th", "ord26.wav");
Convert("27th", "ord27.wav");
Convert("28th", "ord28.wav");
Convert("29th", "ord29.wav");
Convert("3rd", "ord3.wav");
Convert("30th", "ord30.wav");
Convert("31st", "ord31.wav");
Convert("4th", "ord4.wav");
Convert("5th", "ord5.wav");
Convert("6th", "ord6.wav");
Convert("7th", "ord7.wav");
Convert("8th", "ord8.wav");
Convert("9th", "ord9.wav");
Convert("p", "p.wav");
Convert("<say-as interpret-as='characters'>pm</say-as>", "pm.wav");
Convert("point", "point.wav");
Convert("pound", "pound.wav");
Convert("q", "q.wav");
Convert("r", "r.wav");
Convert("s", "s.wav");
Convert("Saturday", "Saturday.wav");
Convert("second", "second.wav");
Convert("seconds", "seconds.wav");
Convert("September", "September.wav");
Convert("star", "star.wav");
Convert("Sunday", "Sunday.wav");
Convert("t", "t.wav");
Convert("thousand", "thousand.wav");
Convert("Thursday", "Thursday.wav");
Convert("Tuesday", "Tuesday.wav");
Convert("u", "u.wav");
Convert("v", "v.wav");
Convert("w", "w.wav");
Convert("Wednesday", "Wednesday.wav");
Convert("x", "x.wav");
Convert("y", "y.wav");
Convert("zed", "z.wav");


return;

ILoggerFactory BuildLoggerFactory()
{
    var loggerFactory =
        LoggerFactory.Create(builder =>
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "hh:mm:ss ";
            }).AddFilter("*", LogLevel.Debug));

    SIPSorcery.LogFactory.Set(loggerFactory);
    return loggerFactory;
}


void Convert(string text, string output)
{
    var audioStream = ttsEngine.TextToSpeech($"{text} <break time='100ms'/>");
    File.WriteAllBytes($"{sipVoiceProperties.TtsAzureVoice}\\{output}", audioStream.ToArray());
}

