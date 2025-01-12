using ivrToolkit.Core;
using ivrToolkit.Core.Enums;
using Microsoft.Extensions.Logging;
using System;
using ivrToolkit.Core.Util;

// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

namespace ivrToolkit.Plugin.Dialogic.Common;

public class DialogicVoiceProperties : VoiceProperties, IDisposable
{
    private readonly ILogger _logger;
    public DialogicVoiceProperties(ILoggerFactory loggerFactory, string fileName) : base(loggerFactory, fileName)
    {
        _logger = loggerFactory.CreateLogger<DialogicVoiceProperties>();
        _logger.LogDebug("Ctr(ILoggerFactory loggerFactory, {0})", fileName);
    }

    /// <summary>
    /// The device name pattern. Default is ':P_pdk_na_an_io:V_dxxxB{board}C{channel}' for gx and 'dxxxB{board}C{channel}' for dx.
    /// </summary>
    public string DeviceNamePattern => GetProperty("voice.deviceNamePattern", UseGc ? ":P_pdk_na_an_io:V_dxxxB{board}C{channel}" : "dxxxB{board}C{channel}");

    /// <summary>
    /// True to use GC_OpenEx instead of DX_OPEN
    /// </summary>
    public bool UseGc => ToBool(GetProperty("voice.useGC", "false"));

    /// <summary>
    /// Resets the event listener for the board every 5 minutes. It used to be -1 but this will show a debug log entry every x milliseconds.
    /// </summary>
    public int BackgroundEventListenerTimeoutMilli => int.Parse(GetProperty("voice.backgroundEventListenerTimeoutMilli", "300000"));

    /// <summary>
    /// Dial tone detection.  Default is 'L' for Leading.
    /// </summary>
    public string DialToneType => GetProperty("dial.cpa.dialTone.type", "L");


    /// <summary>
    /// Number of milliseconds between keypress before it considers it to be a prompt attempt. Default is '5000'.
    /// </summary>
    public int DigitsTimeoutInMilli => int.Parse(GetProperty("getDigits.timeoutInMilliseconds", "5000"));

    /// <summary>
    /// if true then the noFreeLine tone will be enabled
    /// </summary>
    public bool CustomOutboundEnabled => ToBool(GetProperty("dial.customOutbound.enabled", "true"));

    /// <summary>
    /// if true then the dial method will check for dial tone after picking up the receiver/dialing the number.
    /// </summary>
    public bool PreTestDialTone => ToBool(GetProperty("dial.preTestDialTone", "false"));



    /// <summary>
    /// The definition of dial tone. Default is '350,20,440,20,L'. The default Tone Id is '306'.
    /// </summary>
    public CustomTone DialTone
    {
        get
        {
            var tone = new CustomTone(GetProperty("customTone.dialTone", "350,20,440,20,L"))
            {
                Tid = int.Parse(GetProperty("customTone.dialTone.tid", "306"))
            };
            return tone;
        }
    }
    /// <summary>
    /// The definition of the no free line dial tone. Usually is a fast busy signal. Default is '480,30,620,40,25,5,25,5,2' with a tone id of '305'.
    /// </summary>
    public CustomTone NoFreeLineTone
    {
        get
        {
            var tone = new CustomTone(GetProperty("dial.customOutbound.noFreeLineTone", "480,30,620,40,25,5,25,5,2"))
            {
                Tid = int.Parse(GetProperty("dial.customOutbound.noFreeLineTone.tid", "305"))
            };
            return tone;
        }
    }

    /// <summary>
    /// Check Call Progress Analysis if an hangup is detected during the dial. Default is false.
    /// </summary>
    public bool CheckCpaOnHangupDuringDial => ToBool(GetProperty("dial.checkCpaOnHangupError", "false"));

    public new void Dispose()
    {
        _logger.LogDebug("Dispose()");
        base.Dispose();
    }
}