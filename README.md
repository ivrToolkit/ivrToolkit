ivrToolkit
==========

An IVR toolkit library for dotNet developers wanting to write telephony applications. 
Plugins include a SipSorcery plugin, a Dialogic SIP plugin and a Dialogic analog board plugin.
I recommend the ivrToolkit.Plugin.SipSorcery plugin because it is 100% c# and doesn't require any drivers.

https://www.ivrToolkit.com

[![.NET Core Desktop](https://github.com/ivrToolkit/ivrToolkit/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/ivrToolkit/ivrToolkit/actions/workflows/dotnet-desktop.yml)

Example
-------
```csharp
// this is one way to set up your properties, with a property file
var sipVoiceProperties = new SipVoiceProperties(loggerFactory, "voice.properties");

// instantiate the plugin you want to use
using var sipPlugin = new SipSorceryPlugin(loggerFactory, sipVoiceProperties);

// choose a TTS Engine (not a requirement, you could just play wav files)
var ttsFactory = new AzureTtsFactory(loggerFactory, sipVoiceProperties);

// create a line manager
using var lineManager = new LineManager(loggerFactory, sipVoiceProperties, sipPlugin, ttsFactory);

// grab a line
var line = lineManager.GetLine();

// dial out
var callAnalysis = await line.DialAsync(phoneNumber, 3500, cancellationToken);
if (callAnalysis == CallAnalysis.Connected)
{
    // say something
    await line.PlayTextToSpeech("Hello World!", cancellationToken);
}
```

License
-------
[![GitHub](https://img.shields.io/badge/license-Apache--2.0-blue)](https://github.com/ivrToolkit/ivrToolkit/blob/develop/LICENSE)

Documentation
-------------
https://ivrtoolkit.com/Documentation/GetStarted/Intro/

Support 
-------
https://github.com/ivrToolkit/ivrToolkit/issues

Nuget
-----

https://nuget.org/profiles/ivrToolkit

Source Code
-----------
https://github.com/ivrToolkit/ivrToolkit

Example Source Code
-------------------
https://github.com/ivrToolkit/ivrToolkit/tree/develop/Examples
