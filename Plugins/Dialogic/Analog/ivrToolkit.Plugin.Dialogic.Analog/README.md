This is the Dialogic analog plugin used with the ivrToolkit.Core package. 
It does require a Dialogic driver. See https://ivrtoolkit.com/Documentation/GetStarted/Requirements/

Here are the ivrToolkit nuget packages you need:
- ivrToolkit.Core
- ivrToolkit.Plugin.Dialogic.Common
- ivrToolkit.Plugin.Dialogic.Analog

Example:
```csharp
// instantiate the plugin you want to use
using var sipPlugin = new AnalogPlugin(loggerFactory, sipVoiceProperties);

// create a line manager
using var lineManager = new LineManager(loggerFactory, sipVoiceProperties, sipPlugin);

// grab a line
var line = lineManager.GetLine(1);
```