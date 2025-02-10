// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Enums;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Extensions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Plugin.Dialogic.Common;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Plugin.Dialogic.Analog;

public class AnalogLine : IIvrBaseLine, IIvrLineManagement
{
    private readonly int _voiceh;
    private readonly DialogicVoiceProperties _voiceProperties;
    private readonly ILogger<AnalogLine> _logger;

    private readonly int _devh;
    private int _volume;

    public AnalogLine(ILoggerFactory loggerFactory, DialogicVoiceProperties voiceProperties, int devh, int voiceh, int lineNumber)
    {
        _logger = loggerFactory.CreateLogger<AnalogLine>();
        // can only instantiate this class from IVoice
        _voiceProperties = voiceProperties;
        _devh = devh;
        _voiceh = voiceh;
        LineNumber = lineNumber;
        SetDefaultFileType();
        DeleteCustomTones(); // uses dx_deltones() so I have to re-add call progress tones. I also re-add special tones
    }

    public IIvrLineManagement Management => this;

    public string LastTerminator { get; set; } = string.Empty;

    public int LineNumber { get; }

    private DialogicDef.DX_XPB _currentXpb;

    public void WaitRings(int rings)
    {
        WaitRings(_voiceh, rings);
    }

    public Task WaitRingsAsync(int rings, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    void IIvrBaseLine.StartIncomingListener(Func<IIvrLine, CancellationToken, Task> callback, IIvrLine line, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void Hangup()
    {
        Hangup(_voiceh);
    }
    public void TakeOffHook()
    {
        TakeOffHook(_voiceh);
    }

    public CallAnalysis Dial(string number, int answeringMachineLengthInMilliseconds)
    {
        TakeOffHook();
        _logger.LogDebug("Line is now off hook");

        var dialToneTid = _voiceProperties.DialTone.Tid;
        var noFreeLineTid = _voiceProperties.NoFreeLineTone.Tid;

        var dialToneEnabled = false;

        if (_voiceProperties.PreTestDialTone)
        {
            _logger.LogDebug("We are pre-testing the dial tone");
            dialToneEnabled = true;
            EnableTone(_voiceh, dialToneTid);
            var tid = ListenForCustomTones(_voiceh, 2);

            if (tid == 0)
            {
                _logger.LogDebug("No tone was detected");
                DisableTone(_voiceh, dialToneTid);
                Hangup();
                return CallAnalysis.NoDialTone;
            }
        }
        var index = number.IndexOf(',');
        if (_voiceProperties.CustomOutboundEnabled && index != -1)
        {
            _logger.LogDebug("Custom dial-9 logic");
            var prefix = number.Substring(0, index);

            number = number.Substring(index + 1).Replace(",", ""); // there may be more than one comma

            if (!dialToneEnabled) EnableTone(_voiceh, dialToneTid);
            EnableTone(_voiceh, noFreeLineTid);

            // send prefix (usually a 9)
            Dial(_voiceh, prefix);

            // listen for tones
            var tid = ListenForCustomTones(_voiceh, 2);

            DisableTone(_voiceh, dialToneTid);
            DisableTone(_voiceh, noFreeLineTid);


            if (tid == 0)
            {
                Hangup();
                return CallAnalysis.NoDialTone;
            }
            if (tid == noFreeLineTid)
            {
                Hangup();
                return CallAnalysis.NoFreeLine;
            }
        }
        else
        {
            if (dialToneEnabled) DisableTone(_voiceh, dialToneTid);
        }

        _logger.LogDebug("about to dial: {0}",number);
        return DialWithCpa(_voiceh, number, answeringMachineLengthInMilliseconds);

    }

    public Task<CallAnalysis> DialAsync(string phoneNumber, int answeringMachineLengthInMilliseconds, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private void SetDefaultFileType() {
        _currentXpb = new DialogicDef.DX_XPB
        {
            wFileFormat = DialogicDef.FILE_FORMAT_WAVE,
            wDataFormat = DialogicDef.DATA_FORMAT_PCM,
            nSamplesPerSec = DialogicDef.DRT_8KHZ,
            wBitsPerSample = 8
        };
    }

    public void PlayFile(string filename)
    {
        PlayFile(_voiceh, filename, "0123456789#*abcd", _currentXpb);
    }

    public void PlayWavStream(MemoryStream wavStream)
    {
        throw new NotImplementedException();
    }

    public Task PlayWavStreamAsync(MemoryStream wavStream, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task PlayFileAsync(string filename, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void RecordToFile(string filename)
    {
        RecordToFile(filename,60000*5); // default timeout of 5 minutes
    }

    public Task RecordToFileAsync(string filename, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void RecordToFile(string filename, int timeoutMilliseconds)
    {
        RecordToFile(_voiceh, filename, "0123456789#*abcd", _currentXpb, timeoutMilliseconds);
    }

    public Task RecordToFileAsync(string filename, int timeoutMilliseconds, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Keep prompting for digits until number of digits is pressed or a terminator digit is pressed.
    /// </summary>
    /// <param name="numberOfDigits">Maximum number of digits allowed in the buffer.</param>
    /// <param name="terminators">Terminators</param>
    /// <returns>Returns the digits pressed not including the terminator if there was one</returns>
    public string GetDigits(int numberOfDigits, string terminators, int timeoutMilliSeconds = 0)
    {
        return GetDigits(_voiceh, numberOfDigits, terminators);
    }

    public Task<string> GetDigitsAsync(int numberOfDigits, string terminators, CancellationToken cancellationToken, int timeoutMilliSeconds = 0)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns every character including the terminator
    /// </summary>
    /// <returns>All the digits in the buffer including terminators</returns>
    public string FlushDigitBuffer()
    {
        return FlushDigitBuffer(_voiceh);
    }

    public int Volume
    {
        get
        {
            return _volume;
        }
        set
        {
            SetVolume(_voiceh,value);
            _volume = value;
        }
    }

    public void DeleteCustomTones()
    {
        DeleteTones(_voiceh);
        InitCallProgress(_voiceh);
        AddSpecialCustomTones();
    }

    private void AddSpecialCustomTones()
    {
        AddCustomTone(_voiceProperties.DialTone);
        if (_voiceProperties.CustomOutboundEnabled)
        {
            AddCustomTone(_voiceProperties.NoFreeLineTone);
        }
    }


    public void AddCustomTone(CustomTone tone)
    {
        if (tone.ToneType == CustomToneType.Single)
        {
            // TODO - legacy todo statement
        }
        else if (tone.ToneType == CustomToneType.Dual)
        {
            AddDualTone(_voiceh, tone.Tid, tone.Freq1, tone.Frq1Dev, tone.Freq2, tone.Frq2Dev, tone.Mode);
        }
        else if (tone.ToneType == CustomToneType.DualWithCadence)
        {
            AddDualToneWithCadence(_voiceh, tone.Tid, tone.Freq1, tone.Frq1Dev, tone.Freq2, tone.Frq2Dev, tone.Ontime, tone.Ontdev, tone.Offtime,
                tone.Offtdev, tone.Repcnt);
        }
        DisableTone(_voiceh, tone.Tid);
    }

    public void DisableTone(int tid)
    {
        DisableTone(_voiceh, tid);
    }

    public void EnableTone(int tid)
    {
        EnableTone(_voiceh, tid);
    }

    public void Dispose()
    {
        _logger.LogDebug("Dispose()");
        var result = DialogicDef.dx_close(_voiceh, 0);
        if (result <= -1)
        {
            var err = DialogicDef.ATDV_ERRMSGP(_devh);
            throw new VoiceException(err);
        }
        if (_voiceProperties.UseGc)
        {
            result = GcLibDef.gc_Close(_devh);
            if (result != 0)
            {
                ThrowError("Close().gc_Close");
            }
        }
    }

    void IIvrLineManagement.TriggerDispose()
    {
        if (DialogicDef.dx_stopch(_devh, DialogicDef.EV_SYNC) == -1)
        {
            var err = DialogicDef.ATDV_ERRMSGP(_devh);
            throw new VoiceException(err);
        }
    }

    #region OldDialogicCrap
    // This is just temporary while I figure out the best place to put the code.
    // There may be some shared methods that can be used between SIP and ANALOG.
    // For now I just want the thing to compile.




    private void WaitRings(int devh, int rings)
    {
        if (DialogicDef.dx_wtring(devh, rings, (int)DialogicDef.HookState.OFF_HOOK, -1) == -1)
        {
            var err = DialogicDef.ATDV_ERRMSGP(devh);
            throw new VoiceException(err);
        }
    }

    /// <summary>
    /// Puts the line on hook.
    /// </summary>
    /// <param name="devh">The handle for the Dialogic line.</param>
    private void Hangup(int devh)
    {
        DialogicDef.dx_stopch(devh, DialogicDef.EV_SYNC);

        var result = DialogicDef.dx_sethook(devh, (int)DialogicDef.HookState.ON_HOOK, DialogicDef.EV_SYNC);
        if (result <= -1)
        {
            var err = DialogicDef.ATDV_ERRMSGP(devh);
            throw new VoiceException(err);
        }
    }

    /// <summary>
    /// Takes the line off hook.
    /// </summary>
    /// <param name="devh">The handle for the Dialogic line.</param>
    private void TakeOffHook(int devh)
    {
        var result = DialogicDef.dx_sethook(devh, (int)DialogicDef.HookState.OFF_HOOK, DialogicDef.EV_SYNC);
        if (result <= -1)
        {
            var err = DialogicDef.ATDV_ERRMSGP(devh);
            throw new VoiceException(err);
        }
    }

    private void SetVolume(int devh, int size)
    {
        if (size < -10 || size > 10)
        {
            throw new VoiceException("size must be between -10 to 10");
        }
        var adjsize = (ushort)size;
        var result = DialogicDef.dx_adjsv(devh, DialogicDef.SV_VOLUMETBL, DialogicDef.SV_ABSPOS, adjsize);
        if (result <= -1)
        {
            var error = DialogicDef.ATDV_ERRMSGP(devh);
            throw new VoiceException(error);
        }
    }

    /// <summary>
    /// Dials a phone number using call progress analysis.
    /// </summary>
    /// <param name="devh">The handle for the Dialogic line.</param>
    /// <param name="number">The phone number to dial.</param>
    private void Dial(int devh, string number)
    {
        var cap = GetCap(devh);

        var result = DialogicDef.dx_dial(devh, number, ref cap, DialogicDef.EV_SYNC);
        if (result <= -1)
        {
            var error = DialogicDef.ATDV_ERRMSGP(devh);
            throw new VoiceException(error);
        }
    }


    /// <summary>
    /// Dials a phone number using call progress analysis.
    /// </summary>
    /// <param name="devh">The handle for the Dialogic line.</param>
    /// <param name="number">The phone number to dial.</param>
    /// <param name="answeringMachineLengthInMilliseconds">Answering machine length in milliseconds</param>
    /// <returns>CallAnalysis Enum</returns>
    private CallAnalysis DialWithCpa(int devh, string number, int answeringMachineLengthInMilliseconds)
    {

        var cap = GetCap(devh);

        var fullNumber = _voiceProperties.DialToneType + number;
        var result = DialogicDef.dx_dial(devh, fullNumber, ref cap, DialogicDef.DX_CALLP | DialogicDef.EV_SYNC);
        if (result <= -1)
        {
            var error = DialogicDef.ATDV_ERRMSGP(devh);
            throw new VoiceException(error);
        }
        var c = (DialogicDef.CallAnalysis)result;
        switch (c)
        {
            case DialogicDef.CallAnalysis.CR_BUSY:
                return CallAnalysis.Busy;
            case DialogicDef.CallAnalysis.CR_CEPT:
                return CallAnalysis.OperatorIntercept;
            case DialogicDef.CallAnalysis.CR_CNCT:
                var connType = DialogicDef.ATDX_CONNTYPE(devh);
                switch (connType)
                {
                    case DialogicDef.CON_CAD:
                        _logger.LogDebug("Connection due to cadence break ");
                        break;
                    case DialogicDef.CON_DIGITAL:
                        _logger.LogDebug("con_digital");
                        break;
                    case DialogicDef.CON_LPC:
                        _logger.LogDebug("Connection due to loop current");
                        break;
                    case DialogicDef.CON_PAMD:
                        _logger.LogDebug("Connection due to Positive Answering Machine Detection");
                        break;
                    case DialogicDef.CON_PVD:
                        _logger.LogDebug("Connection due to Positive Voice Detection");
                        break;
                }
                var len = GetSalutationLength(devh);
                if (len > answeringMachineLengthInMilliseconds)
                {
                    return CallAnalysis.AnsweringMachine;
                }
                return CallAnalysis.Connected;
            case DialogicDef.CallAnalysis.CR_ERROR:
                return CallAnalysis.Error;
            case DialogicDef.CallAnalysis.CR_FAXTONE:
                return CallAnalysis.FaxTone;
            case DialogicDef.CallAnalysis.CR_NOANS:
                return CallAnalysis.NoAnswer;
            case DialogicDef.CallAnalysis.CR_NODIALTONE:
                return CallAnalysis.NoDialTone;
            case DialogicDef.CallAnalysis.CR_NORB:
                return CallAnalysis.NoRingback;
            case DialogicDef.CallAnalysis.CR_STOPD:
                // calling method will check and throw the stopException
                return CallAnalysis.Stopped;
        }
        throw new VoiceException("Unknown dial response: " + result);
    }

    private int GetTid(string tidName)
    {
        tidName.ThrowIfNull(nameof(tidName));
        if (int.TryParse(tidName, out var value))
        {
            return value;
        }

        try
        {
            return (int)Enum.Parse<DialogicDef.ToneTypes>(tidName, true);
        }
        catch (Exception)
        {
            throw new Exception("tid name is not found: " + tidName);
        }
    }

    private void InitCallProgress(int devh)
    {
        var toneParams = _voiceProperties.GetValuePrefixMatch("cpa.tone.");

        foreach (var tone in toneParams)
        {
            var part = tone.Split(',');
            var t = new DialogicDef.Tone_T
            {
                str = part[0].Trim(),
                tid = GetTid(part[1].Trim()),
                freq1 = new DialogicDef.Freq_T
                {
                    freq = int.Parse(part[2].Trim()),
                    deviation = int.Parse(part[3].Trim())
                },
                freq2 = new DialogicDef.Freq_T
                {
                    freq = int.Parse(part[4].Trim()),
                    deviation = int.Parse(part[5].Trim())
                },
                @on = new DialogicDef.State_T
                {
                    time = int.Parse(part[6].Trim()),
                    deviation = int.Parse(part[7].Trim())
                },
                off = new DialogicDef.State_T
                {
                    time = int.Parse(part[8].Trim()),
                    deviation = int.Parse(part[9].Trim())
                },
                repcnt = int.Parse(part[10].Trim())
            };

            DialogicDef.dx_chgfreq(t.tid,
                t.freq1.freq,
                t.freq1.deviation,
                t.freq2.freq,
                t.freq2.deviation);

            DialogicDef.dx_chgdur(t.tid,
                t.on.time,
                t.on.deviation,
                t.off.time,
                t.off.deviation);

            DialogicDef.dx_chgrepcnt(t.tid,
                t.repcnt);
        } // foreach

        // initialize
        var result = DialogicDef.dx_initcallp(devh);
        if (result <= -1)
        {
            var err = DialogicDef.ATDV_ERRMSGP(devh);
            throw new VoiceException(err);
        }
    }

    private DialogicDef.DX_CAP GetCap(int devh)
    {
        var cap = new DialogicDef.DX_CAP();

        var result = DialogicDef.dx_clrcap(ref cap);
        if (result <= -1)
        {
            var err = DialogicDef.ATDV_ERRMSGP(devh);
            throw new VoiceException(err);
        }

        var capType = typeof(DialogicDef.DX_CAP);

        object boxed = cap;

        var caps = _voiceProperties.GetKeyPrefixMatch("cap.");
        foreach (var capName in caps)
        {
            var info = capType.GetField(capName);
            if (info == null)
            {
                throw new Exception("Could not find dx_cap." + capName);
            }
            var obj = info.GetValue(cap);
            if (obj is ushort)
            {
                var value = ushort.Parse(_voiceProperties.GetProperty("cap." + capName));
                info.SetValue(boxed, value);
            }
            else if (obj is byte)
            {
                var value = byte.Parse(_voiceProperties.GetProperty("cap." + capName));
                info.SetValue(boxed, value);
            }
        }

        return (DialogicDef.DX_CAP)boxed;
    }

    private void DeleteTones(int devh)
    {
        _logger.LogDebug("in delete tones");
        if (DialogicDef.dx_deltones(devh) == -1)
        {
            var err = DialogicDef.ATDV_ERRMSGP(devh);
            throw new VoiceException(err);
        }
    }

    /// <summary>
    /// Gets the greeting time in milliseconds.
    /// </summary>
    /// <param name="devh">The handle for the Dialogic line.</param>
    /// <returns>The greeting time in milliseconds.</returns>
    private int GetSalutationLength(int devh)
    {
        var result = DialogicDef.ATDX_ANSRSIZ(devh);
        if (result <= -1)
        {
            var err = DialogicDef.ATDV_ERRMSGP(devh);
            throw new VoiceException(err);
        }
        return result * 10;
    }

    private void ThrowError(string from)
    {
        var info = new GcLibDef.GC_INFO();
        _logger.LogDebug("About to find error for: {0}", from);
        var status = GcLibDef.gc_ErrorInfo(ref info);
        if (status != 0) throw new VoiceException($"Unknown error from {from}"); // should not happen

        var message =
            $"{info.gcValue}|{info.gcMsg}\r\n{info.ccValue}|{info.ccMsg}\r\n{info.ccLibId}|{info.ccLibName}";
        _logger.LogError(message);
        throw new VoiceException(message);

    }

    /// <summary>
    /// Returns every character including the terminator
    /// </summary>
    /// <param name="devh">The handle for the Dialogic line.</param>
    /// <returns>All the digits in the buffer including terminators</returns>
    private string FlushDigitBuffer(int devh)
    {
        var all = "";
        try
        {
            // add "T" so that I can get all the characters.
            all = GetDigits(devh, 99, "T", 100);
            // strip off timeout terminator if there is once
            if (all.EndsWith("T"))
            {
                all = all.Substring(0, all.Length - 1);
            }
        }
        catch (GetDigitsTimeoutException)
        {
        }
        return all;
    }

    /// <summary>
    /// Keep prompting for digits until number of digits is pressed or a terminator digit is pressed.
    /// </summary>
    /// <param name="devh">The handle for the Dialogic line.</param>
    /// <param name="numberOfDigits">Maximum number of digits allowed in the buffer.</param>
    /// <param name="terminators">Terminator keys</param>
    /// <returns>Returns the digits pressed not including the terminator if there was one</returns>
    private string GetDigits(int devh, int numberOfDigits, string terminators)
    {
        var timeout = _voiceProperties.DigitsTimeoutInMilli;
        return GetDigits(devh, numberOfDigits, terminators, timeout);
    }

    private string GetDigits(int devh, int numberOfDigits, string terminators, int timeout)
    {

        var tpt = GetTerminationConditions(numberOfDigits, terminators, timeout);

        DialogicDef.DV_DIGIT digit;

        // Note: async does not work becaues digit is marshalled out immediately after dx_getdig is complete
        // not when event is found. Would have to use DV_DIGIT* and unsafe code. or another way?
        var result = DialogicDef.dx_getdig(devh, ref tpt[0], out digit, DialogicDef.EV_SYNC);
        if (result == -1)
        {
            var err = DialogicDef.ATDV_ERRMSGP(devh);
            throw new VoiceException(err);
        }

        var reason = DialogicDef.ATDX_TERMMSK(devh);
        if ((reason & DialogicDef.TM_ERROR) == DialogicDef.TM_ERROR)
        {
            throw new VoiceException("TM_ERROR");
        }
        if ((reason & DialogicDef.TM_USRSTOP) == DialogicDef.TM_USRSTOP)
        {
            throw new DisposingException();
        }
        if ((reason & DialogicDef.TM_LCOFF) == DialogicDef.TM_LCOFF)
        {
            throw new HangupException();
        }
        if ((reason & DialogicDef.TM_BARGEIN) == DialogicDef.TM_BARGEIN) Console.WriteLine("TM_BARGEIN");
        //if ((reason & TM_DIGIT) == TM_DIGIT) Console.WriteLine("TM_DIGIT");
        //if ((reason & TM_EOD) == TM_EOD) Console.WriteLine("TM_EOD");
        if ((reason & DialogicDef.TM_MAXDATA) == DialogicDef.TM_MAXDATA) Console.WriteLine("TM_MAXDATA");
        //if ((reason & TM_MAXDTMF) == TM_MAXDTMF) Console.WriteLine("TM_MAXDTMF");
        if ((reason & DialogicDef.TM_MAXNOSIL) == DialogicDef.TM_MAXNOSIL) Console.WriteLine("TM_MTAXNOSIL");
        if ((reason & DialogicDef.TM_MAXSIL) == DialogicDef.TM_MAXSIL) Console.WriteLine("TM_MAXSIL");
        //if ((reason & TM_NORMTERM) == TM_NORMTERM) Console.WriteLine("TM_NORMTERM");
        if ((reason & DialogicDef.TM_PATTERN) == DialogicDef.TM_PATTERN) Console.WriteLine("TM_PATTERN");
        if ((reason & DialogicDef.TM_TONE) == DialogicDef.TM_TONE) Console.WriteLine("TM_TONE");


        var answer = digit.dg_value;
        ClearDigits(devh); // not sure if this is necessary and perhaps only needed for getDigitsTimeoutException?
        if ((reason & DialogicDef.TM_IDDTIME) == DialogicDef.TM_IDDTIME)
        {
            if (terminators.IndexOf("t", StringComparison.Ordinal) != -1)
            {
                answer += 't';
            }
            else
            {
                throw new GetDigitsTimeoutException();
            }
        }
        return answer;
    }

    private void ClearDigits(int devh)
    {
        if (DialogicDef.dx_clrdigbuf(devh) == -1)
        {
            var err = DialogicDef.ATDV_ERRMSGP(devh);
            throw new VoiceException(err);
        }
    }

    private DialogicDef.DV_TPT[] GetTerminationConditions(int numberOfDigits, string terminators, int timeoutInMilliseconds)
    {
        var tpts = new List<DialogicDef.DV_TPT>();

        var tpt = new DialogicDef.DV_TPT
        {
            tp_type = DialogicDef.IO_CONT,
            tp_termno = DialogicDef.DX_MAXDTMF,
            tp_length = (ushort)numberOfDigits,
            tp_flags = DialogicDef.TF_MAXDTMF,
            tp_nextp = IntPtr.Zero
        };
        tpts.Add(tpt);

        var bitMask = DefineDigits(terminators);
        if (bitMask != 0)
        {
            tpt = new DialogicDef.DV_TPT
            {
                tp_type = DialogicDef.IO_CONT,
                tp_termno = DialogicDef.DX_DIGMASK,
                tp_length = (ushort)bitMask,
                tp_flags = DialogicDef.TF_DIGMASK,
                tp_nextp = IntPtr.Zero
            };
            tpts.Add(tpt);
        }
        if (timeoutInMilliseconds != 0)
        {
            tpt = new DialogicDef.DV_TPT
            {
                tp_type = DialogicDef.IO_CONT,
                tp_termno = DialogicDef.DX_IDDTIME,
                tp_length = (ushort)(timeoutInMilliseconds / 100),
                tp_flags = DialogicDef.TF_IDDTIME,
                tp_nextp = IntPtr.Zero
            };
            tpts.Add(tpt);
        }

        tpt = new DialogicDef.DV_TPT
        {
            tp_type = DialogicDef.IO_EOT,
            tp_termno = DialogicDef.DX_LCOFF,
            tp_length = 3,
            tp_flags = DialogicDef.TF_LCOFF | DialogicDef.TF_10MS,
            tp_nextp = IntPtr.Zero
        };
        tpts.Add(tpt);

        return tpts.ToArray();
    }

    private int DefineDigits(string digits)
    {
        var result = 0;

        if (digits == null) digits = "";

        var all = digits.Trim().ToLower();
        var chars = all.ToCharArray();
        foreach (var c in chars)
        {
            switch (c)
            {
                case '0':
                    result = result | DialogicDef.DM_0;
                    break;
                case '1':
                    result = result | DialogicDef.DM_1;
                    break;
                case '2':
                    result = result | DialogicDef.DM_2;
                    break;
                case '3':
                    result = result | DialogicDef.DM_3;
                    break;
                case '4':
                    result = result | DialogicDef.DM_4;
                    break;
                case '5':
                    result = result | DialogicDef.DM_5;
                    break;
                case '6':
                    result = result | DialogicDef.DM_6;
                    break;
                case '7':
                    result = result | DialogicDef.DM_7;
                    break;
                case '8':
                    result = result | DialogicDef.DM_8;
                    break;
                case '9':
                    result = result | DialogicDef.DM_9;
                    break;
                case 'a':
                    result = result | DialogicDef.DM_A;
                    break;
                case 'b':
                    result = result | DialogicDef.DM_B;
                    break;
                case 'c':
                    result = result | DialogicDef.DM_C;
                    break;
                case 'd':
                    result = result | DialogicDef.DM_D;
                    break;
                case '#':
                    result = result | DialogicDef.DM_P;
                    break;
                case '*':
                    result = result | DialogicDef.DM_S;
                    break;
            }
        }
        return result;
    }


    /// <summary>
    /// Play a vox or wav file.
    /// </summary>
    /// <param name="devh">The handle for the Dialogic line.</param>
    /// <param name="filename">The name of the file to play.</param>
    /// <param name="terminators">Terminator keys</param>
    /// <param name="xpb">The format of the vox or wav file.</param>
    private void PlayFile(int devh, string filename, string terminators, DialogicDef.DX_XPB xpb)
    {

        /* set up DV_TPT */
        var tpt = GetTerminationConditions(10, terminators, 0);

        var iott = new DialogicDef.DX_IOTT { io_type = DialogicDef.IO_DEV | DialogicDef.IO_EOT, io_bufp = null, io_offset = 0, io_length = -1 };
        /* set up DX_IOTT */
        if ((iott.io_fhandle = DialogicDef.dx_fileopen(filename, DialogicDef._O_RDONLY | DialogicDef._O_BINARY)) == -1)
        {
            var fileErr = DialogicDef.dx_fileerrno();

            var err = "";

            switch (fileErr)
            {
                case DialogicDef.EACCES:
                    err = "Tried to open read-only file for writing, file's sharing mode does not allow specified operations, or given path is directory.";
                    break;
                case DialogicDef.EEXIST:
                    err = "_O_CREAT and _O_EXCL flags specified, but filename already exists.";
                    break;
                case DialogicDef.EINVAL:
                    err = "Invalid oflag or pmode argument.";
                    break;
                case DialogicDef.EMFILE:
                    err = "No more file descriptors available (too many open files).";
                    break;
                case DialogicDef.ENOENT:
                    err = "File or path not found.";
                    break;
            }
            err += " File: |" + filename + "|";

            //I don't think this is needed when we get an error opening a file
            //dx_fileclose(iott.io_fhandle);

            throw new VoiceException(err);
        }


        var state = DialogicDef.ATDX_STATE(devh);
        _logger.LogDebug("About to play: {0} state: {1}", filename, state);

        /* Now play the file */
        if (DialogicDef.dx_playiottdata(devh, ref iott, ref tpt[0], ref xpb, DialogicDef.EV_ASYNC) == -1)
        {
            _logger.LogError("Tried to play: {0} state: {1}", filename, state);

            var err = DialogicDef.ATDV_ERRMSGP(devh);
            DialogicDef.dx_fileclose(iott.io_fhandle);
            throw new VoiceException(err);
        }

        var handler = 0;

        while (true)
        {
            if (DialogicDef.sr_waitevtEx(ref devh, 1, -1, ref handler) == -1)
            {
                var err = DialogicDef.ATDV_ERRMSGP(devh);
                DialogicDef.dx_fileclose(iott.io_fhandle);
                throw new VoiceException(err);
            }
            // make sure the file is closed
            if (DialogicDef.dx_fileclose(iott.io_fhandle) == -1)
            {
                var err = DialogicDef.ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }
            var type = DialogicDef.sr_getevttype((uint)handler);
            if (type == DialogicDef.TDX_PLAY)
            {
                var reason = DialogicDef.ATDX_TERMMSK(devh);
                if ((reason & DialogicDef.TM_ERROR) == DialogicDef.TM_ERROR)
                {
                    throw new VoiceException("TM_ERROR");
                }
                if ((reason & DialogicDef.TM_USRSTOP) == DialogicDef.TM_USRSTOP)
                {
                    throw new DisposingException();
                }
                if ((reason & DialogicDef.TM_LCOFF) == DialogicDef.TM_LCOFF)
                {
                    throw new HangupException();
                }
                if ((reason & DialogicDef.TM_MAXTIME) == DialogicDef.TM_MAXTIME) _logger.LogDebug("TM_MAXTIME");

                if ((reason & DialogicDef.TM_BARGEIN) == DialogicDef.TM_BARGEIN) _logger.LogDebug("TM_BARGEIN");
                //                    if ((reason & TM_DIGIT) == TM_DIGIT) _logger.LogDebug("TM_DIGIT");
                //                    if ((reason & TM_EOD) == TM_EOD) _logger.LogDebug("TM_EOD"); // This is how I know they listend to full message
                if ((reason & DialogicDef.TM_IDDTIME) == DialogicDef.TM_IDDTIME) _logger.LogDebug("TM_IDDTIME");
                if ((reason & DialogicDef.TM_MAXDATA) == DialogicDef.TM_MAXDATA) _logger.LogDebug("TM_MAXDATA");
                //                    if ((reason & TM_MAXDTMF) == TM_MAXDTMF) _logger.LogDebug("TM_MAXDTMF");
                if ((reason & DialogicDef.TM_MAXNOSIL) == DialogicDef.TM_MAXNOSIL) _logger.LogDebug("TM_MTAXNOSIL");
                if ((reason & DialogicDef.TM_MAXSIL) == DialogicDef.TM_MAXSIL) _logger.LogDebug("TM_MAXSIL");
                //                    if ((reason & TM_NORMTERM) == TM_NORMTERM) _logger.LogDebug("TM_NORMTERM");
                if ((reason & DialogicDef.TM_PATTERN) == DialogicDef.TM_PATTERN) _logger.LogDebug("TM_PATTERN");
                if ((reason & DialogicDef.TM_TONE) == DialogicDef.TM_TONE) _logger.LogDebug("TM_TONE");
            }
            else
            {
                _logger.LogError("got here: {0}", type);
            }
            return;
        } // while

    }



    private void AddDualTone(int devh, int tid, int freq1, int fq1Dev, int freq2, int fq2Dev,
        ToneDetection mode)
    {
        var dialogicMode = mode == ToneDetection.Leading ? DialogicDef.TN_LEADING : DialogicDef.TN_TRAILING;

        if (DialogicDef.dx_blddt((uint)tid, (uint)freq1, (uint)fq1Dev, (uint)freq2, (uint)fq2Dev, dialogicMode) == -1)
        {
            throw new VoiceException("unable to build dual tone");
        }
        if (DialogicDef.dx_addtone(devh, 0, 0) == -1)
        {
            var err = DialogicDef.ATDV_ERRMSGP(devh);
            throw new VoiceException(err);
        }
    }
    //T5=480,30,620,40,25,5,25,5,2 fast busy
    //T6=350,20,440,20,L dial tone

    private void AddDualToneWithCadence(int devh, int tid, int freq1, int fq1Dev, int freq2, int fq2Dev,
        int ontime, int ontdev, int offtime, int offtdev, int repcnt)
    {
        if (DialogicDef.dx_blddtcad((uint)tid, (uint)freq1, (uint)fq1Dev, (uint)freq2, (uint)fq2Dev, (uint)ontime, (uint)ontdev, (uint)offtime, (uint)offtdev, (uint)repcnt) == -1)
        {
            throw new VoiceException("unable to build dual tone cadence");
        }
        if (DialogicDef.dx_addtone(devh, 0, 0) == -1)
        {
            var err = DialogicDef.ATDV_ERRMSGP(devh);
            throw new VoiceException(err);
        }
    }

    private void DisableTone(int devh, int tid)
    {
        if (DialogicDef.dx_distone(devh, tid, DialogicDef.DM_TONEON | DialogicDef.DM_TONEOFF) == -1)
        {
            var err = DialogicDef.ATDV_ERRMSGP(devh);
            throw new VoiceException(err);
        }
    }

    private void EnableTone(int devh, int tid)
    {
        if (DialogicDef.dx_enbtone(devh, tid, DialogicDef.DM_TONEON | DialogicDef.DM_TONEOFF) == -1)
        {
            var err = DialogicDef.ATDV_ERRMSGP(devh);
            throw new VoiceException(err);
        }
    }

    private int ListenForCustomTones(int devh, int timeoutSeconds)
    {
        var eblk = new DialogicDef.DX_EBLK();
        if (DialogicDef.dx_getevt(devh, ref eblk, timeoutSeconds) == -1)
        {
            if (DialogicDef.ATDV_LASTERR(devh) == DialogicDef.EDX_TIMEOUT)
            {
                return 0;
            }
            var err = DialogicDef.ATDV_ERRMSGP(devh);
            throw new VoiceException(err);
        }
        if (eblk.ev_event == DialogicDef.DE_TONEON || eblk.ev_event == DialogicDef.DE_TONEOFF)
        {
            return eblk.ev_data;
        }
        return 0;
    }

    /// <summary>
    /// Record a vox or wav file.
    /// </summary>
    /// <param name="devh">The handle for the Dialogic line.</param>
    /// <param name="filename">The name of the file to play.</param>
    /// <param name="terminators">Terminator keys</param>
    /// <param name="xpb">The format of the vox or wav file.</param>
    /// <param name="timeoutMilli">Number of milliseconds before timeout</param>
    private void RecordToFile(int devh, string filename, string terminators, DialogicDef.DX_XPB xpb, int timeoutMilli)
    {

        FlushDigitBuffer(devh);

        /* set up DV_TPT */
        var tpt = GetTerminationConditions(1, terminators, timeoutMilli);

        var iott = new DialogicDef.DX_IOTT { io_type = DialogicDef.IO_DEV | DialogicDef.IO_EOT, io_bufp = null, io_offset = 0, io_length = -1 };
        /* set up DX_IOTT */
        if ((iott.io_fhandle = DialogicDef.dx_fileopen(filename, DialogicDef._O_CREAT | DialogicDef._O_BINARY | DialogicDef._O_RDWR, DialogicDef._S_IWRITE)) == -1)
        {
            var fileErr = DialogicDef.dx_fileerrno();

            var err = "";

            switch (fileErr)
            {
                case DialogicDef.EACCES:
                    err = "Tried to open read-only file for writing, file's sharing mode does not allow specified operations, or given path is directory.";
                    break;
                case DialogicDef.EEXIST:
                    err = "_O_CREAT and _O_EXCL flags specified, but filename already exists.";
                    break;
                case DialogicDef.EINVAL:
                    err = "Invalid oflag or pmode argument.";
                    break;
                case DialogicDef.EMFILE:
                    err = "No more file descriptors available (too many open files).";
                    break;
                case DialogicDef.ENOENT:
                    err = "File or path not found.";
                    break;
            }

            DialogicDef.dx_fileclose(iott.io_fhandle);

            throw new VoiceException(err);
        }

        /* Now record the file */
        if (DialogicDef.dx_reciottdata(devh, ref iott, ref tpt[0], ref xpb, DialogicDef.RM_TONE | DialogicDef.EV_ASYNC) == -1)
        {
            var err = DialogicDef.ATDV_ERRMSGP(devh);
            DialogicDef.dx_fileclose(iott.io_fhandle);
            throw new VoiceException(err);
        }

        var handler = 0;

        while (true)
        {
            if (DialogicDef.sr_waitevtEx(ref devh, 1, -1, ref handler) == -1)
            {
                var err = DialogicDef.ATDV_ERRMSGP(devh);
                DialogicDef.dx_fileclose(iott.io_fhandle);
                throw new VoiceException(err);
            }
            if (DialogicDef.dx_fileclose(iott.io_fhandle) == -1)
            {
                var err = DialogicDef.ATDV_ERRMSGP(devh);
                throw new VoiceException(err);
            }

            var type = DialogicDef.sr_getevttype((uint)handler);
            if (type == DialogicDef.TDX_RECORD)
            {
                var reason = DialogicDef.ATDX_TERMMSK(devh);
                if ((reason & DialogicDef.TM_ERROR) == DialogicDef.TM_ERROR)
                {
                    throw new VoiceException("TM_ERROR");
                }
                if ((reason & DialogicDef.TM_USRSTOP) == DialogicDef.TM_USRSTOP)
                {
                    throw new DisposingException();
                }
                if ((reason & DialogicDef.TM_LCOFF) == DialogicDef.TM_LCOFF)
                {
                    throw new HangupException();
                }
                if ((reason & DialogicDef.TM_MAXTIME) == DialogicDef.TM_MAXTIME) Console.WriteLine("TM_MAXTIME");

                if ((reason & DialogicDef.TM_BARGEIN) == DialogicDef.TM_BARGEIN) Console.WriteLine("TM_BARGEIN");
                if ((reason & DialogicDef.TM_DIGIT) == DialogicDef.TM_DIGIT) Console.WriteLine("TM_DIGIT");
                if ((reason & DialogicDef.TM_EOD) == DialogicDef.TM_EOD) Console.WriteLine("TM_EOD");
                if ((reason & DialogicDef.TM_IDDTIME) == DialogicDef.TM_IDDTIME) Console.WriteLine("TM_IDDTIME");
                if ((reason & DialogicDef.TM_MAXDATA) == DialogicDef.TM_MAXDATA) Console.WriteLine("TM_MAXDATA");
                if ((reason & DialogicDef.TM_MAXDTMF) == DialogicDef.TM_MAXDTMF) Console.WriteLine("TM_MAXDTMF");
                if ((reason & DialogicDef.TM_MAXNOSIL) == DialogicDef.TM_MAXNOSIL) Console.WriteLine("TM_MTAXNOSIL");
                if ((reason & DialogicDef.TM_MAXSIL) == DialogicDef.TM_MAXSIL) Console.WriteLine("TM_MAXSIL");
                if ((reason & DialogicDef.TM_NORMTERM) == DialogicDef.TM_NORMTERM) Console.WriteLine("TM_NORMTERM");
                if ((reason & DialogicDef.TM_PATTERN) == DialogicDef.TM_PATTERN) Console.WriteLine("TM_PATTERN");
                if ((reason & DialogicDef.TM_TONE) == DialogicDef.TM_TONE) Console.WriteLine("TM_TONE");
            }
            else
            {
                Console.WriteLine("got here: " + type);
            }
            FlushDigitBuffer(devh);
            return;
        }

    }

    public void Reset()
    {
        throw new NotImplementedException();
    }





    #endregion
} // class