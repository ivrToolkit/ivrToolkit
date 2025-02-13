// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 
using System;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Core.Util;

/// <summary>
/// Holds the voice.properties in a static Properties class.
/// </summary>
public class VoiceProperties : Properties, IDisposable
{
    private readonly ILogger<VoiceProperties> _logger;

    private const string PROMPT_ATTEMPTS_KEY = "prompt.attempts";
    private const string DEFAULT_PROMPT_ATTEMPTS = "99";

    private const string PROMPT_BLANK_ATTEMPTS_KEY = "prompt.blankAttempts";
    private const string DEFAULT_PROMPT_BLANK_ATTEMPTS = "5";

    private const string DIGITS_TIMEOUT_KEY = "getDigits.timeoutInMilliseconds";
    private const string DEFAULT_DIGITS_TIMEOUT = "5000";

    private const string TTS_AZURE_SUBSCRIPTION_KEY = "tts.azure.subscriptionKey";
    private const string TTS_AZURE_REGION_KEY = "tts.azure.region";
    
    private const string TTS_AZURE_VOICE_KEY = "tts.azure.voice";
    private const string TTS_AZURE_VOICE_DEFAULT = "en-US-JennyNeural";
    
    private const string WAV_SAMPLE_RATE_KEY = "wav.sampleRate";
    private const string WAV_SAMPLE_RATE_DEFAULT = "8000";
    
    private const string SYSTEM_RECORDING_SUBFOLDER_KEY = "system.recording.subfolder";
    private const string SYSTEM_RECORDING_SUBFOLDER_DEFAULT = "en-US-JennyNeural";
    
    protected VoiceProperties(ILoggerFactory loggerFactory, string fileName) : base (loggerFactory, fileName)
    {
        _logger = loggerFactory.CreateLogger<VoiceProperties>();
        _logger.LogDebug("ctr(ILoggerFactory, {0})", fileName);
    }

    public VoiceProperties(ILoggerFactory loggerFactory) : base (loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<VoiceProperties>();
        _logger.LogDebug("ctr(ILoggerFactory)");
    }

    /// <summary>
    /// The sub-folder location under "System Recordings" that contains the system recordings.
    /// Current options are "Original" or "en-US-JennyNeural"
    /// </summary>
    public string SystemRecordingSubfolder
    {
        get => GetProperty(SYSTEM_RECORDING_SUBFOLDER_KEY, SYSTEM_RECORDING_SUBFOLDER_DEFAULT);
        init => SetProperty(SYSTEM_RECORDING_SUBFOLDER_KEY, value);
    }
    
    /// <summary>
    /// Total number of attempts to try at a prompt before 'TooManyAttemptsException' is thrown. Default is '99'.
    /// </summary>
    public int PromptAttempts
    {
        get => int.Parse(GetProperty(PROMPT_ATTEMPTS_KEY, DEFAULT_PROMPT_ATTEMPTS));
        init => SetProperty(PROMPT_ATTEMPTS_KEY, value.ToString());
    }

    /// <summary>
    /// Number of blank entry attempts to try at a prompt before 'TooManyAttemptsException' is thrown. Default is '5'.
    /// </summary>
    public int PromptBlankAttempts
    {
        get => int.Parse(GetProperty(PROMPT_BLANK_ATTEMPTS_KEY, DEFAULT_PROMPT_BLANK_ATTEMPTS));
        init => SetProperty(PROMPT_BLANK_ATTEMPTS_KEY, value.ToString());
    }

    /// <summary>
    /// Number of milliseconds between keypress before it considers it to be a prompt attempt. Default is '5000'.
    /// </summary>
    public int DigitsTimeoutInMilli
    {
        get => int.Parse(GetProperty(DIGITS_TIMEOUT_KEY, DEFAULT_DIGITS_TIMEOUT));
        init => SetProperty(DIGITS_TIMEOUT_KEY, value.ToString());
    }
    
    
    /// <summary>
    /// The subscription key for Azure Text-To-Speech
    /// </summary>
    public string TtsAzureSubscriptionKey
    {
        get => GetProperty(TTS_AZURE_SUBSCRIPTION_KEY, "");
        init => SetProperty(TTS_AZURE_SUBSCRIPTION_KEY, value);
    }
    
    /// <summary>
    /// The region for Azure Text-To-Speech
    /// </summary>
    public string TtsAzureRegion
    {
        get => GetProperty(TTS_AZURE_REGION_KEY, "");
        init => SetProperty(TTS_AZURE_REGION_KEY, value);
    }

    /// <summary>
    /// The region for Azure Text-To-Speech
    /// </summary>
    public string TtsAzureVoice
    {
        get => GetProperty(TTS_AZURE_VOICE_KEY, TTS_AZURE_VOICE_DEFAULT);
        init => SetProperty(TTS_AZURE_VOICE_KEY, value);
    }
    
    /// <summary>
    /// The default wav sample rate to use when writing out wav files with TTS. 8000 or 16000 only.
    /// Default is 8000
    /// </summary>
    public int DefaultWavSampleRate
    {
        get => int.Parse(GetProperty(WAV_SAMPLE_RATE_KEY, WAV_SAMPLE_RATE_DEFAULT));
        init => SetProperty(WAV_SAMPLE_RATE_KEY, value.ToString());
    }
    
    
    public new void Dispose()
    {
        _logger.LogDebug("{method}()", nameof(Dispose));
        base.Dispose();
    }
}
