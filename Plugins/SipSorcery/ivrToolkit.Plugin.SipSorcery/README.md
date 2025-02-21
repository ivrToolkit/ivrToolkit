This is the SipSorcery plugin used with the ivrToolkit.Core package. SipSorcery is a 100% c# implementation of
SIP. You will need two packages

- ivrToolkit.Core
- ivrToolkit.Plugin.SipSorcery

Example:
```
// instantiate the plugin you want to use
using var sipPlugin = new SipSorceryPlugin(loggerFactory, sipVoiceProperties);

// create a line manager
using var lineManager = new LineManager(loggerFactory, sipVoiceProperties, sipPlugin);

// grab a line
var line = lineManager.GetLine();

```