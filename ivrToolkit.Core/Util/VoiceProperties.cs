// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 
using System;
using System.Globalization;
using Google.Cloud.TextToSpeech.V1;
using ivrToolkit.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Core.Util;

/// <summary>
/// Holds the voice.properties in a Properties class.
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
    
    private const string AM_SILENCE_AMPLITUDE_KEY = "answeringMachine.silenceThresholdAmplitude";
    private const string AM_SILENCE_AMPLITUDE_DEFAULT = "0.1";
    
    private const string AM_MAX_START_SILENCE_SECONDS_KEY = "answeringMachine.maxStartSilenceSeconds";
    private const string AM_MAX_START_SILENCE_SECONDS_DEFAULT = "3.0";
    
    private const string AM_END_SPEECH_SILENCE_SECONDS_KEY = "answeringMachine.endSpeechSilenceDurationSeconds";
    private const string AM_END_SPEECH_SILENCE_SECONDS_DEFAULT = "1.5";
    
    private const string AM_GIVE_UP_AFTER_SECONDS_KEY = "answeringMachine.giveUpAfterSeconds";
    private const string AM_GIVE_UP_AFTER_SECONDS_DEFAULT = "10.0";
    
    private const string SYSTEM_RECORDING_SUBFOLDER_KEY = "system.recording.subfolder";
    private const string SYSTEM_RECORDING_SUBFOLDER_DEFAULT = "en-US-JennyNeural";
    
    private const string TTS_GOOGLE_CREDENTIALS_PATH_KEY = "tts.google.credentials.path";
    
    private const string TTS_GOOGLE_LANGUAGE_CODE_KEY = "tts.google.languageCode";
    private const string TTS_GOOGLE_LANGUAGE_CODE_DEFAULT = "en-US";
    
    private const string TTS_GOOGLE_NAME_KEY = "tts.google.name";
    private const string TTS_GOOGLE_NAME_DEFAULT = "";
    
    private const string TTS_GOOGLE_GENDER_KEY = "tts.google.gender";
    private const string TTS_GOOGLE_GENDER_DEFAULT = "Female";
    
    
    /// <summary>
    /// Constructs a VoiceProperties object given the text file that contains the definitions.
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="fileName">The text file that contains the definitions</param>
    protected VoiceProperties(ILoggerFactory loggerFactory, string fileName) : base (loggerFactory, fileName)
    {
        _logger = loggerFactory.CreateLogger<VoiceProperties>();
        _logger.LogDebug("ctr(ILoggerFactory, {0})", fileName);
    }

    /// <summary>
    /// Construct a VoiceProperties object without a text file for the defaults.
    /// </summary>
    /// <param name="loggerFactory"></param>
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

    /// <summary>
    /// The Google application credentials path for use with TTS
    /// </summary>
    public string TtsGoogleApplicationCredentialsPath
    {
        get => GetProperty(TTS_GOOGLE_CREDENTIALS_PATH_KEY, "");
        init => SetProperty(TTS_GOOGLE_CREDENTIALS_PATH_KEY, value);
    }

    public string TtsGoogleLanguageCode
    {
        get => GetProperty(TTS_GOOGLE_LANGUAGE_CODE_KEY, TTS_GOOGLE_LANGUAGE_CODE_DEFAULT);
        init => SetProperty(TTS_GOOGLE_LANGUAGE_CODE_KEY, value);
    }
    
    public string TtsGoogleName
    {
        get => GetProperty(TTS_GOOGLE_NAME_KEY, TTS_GOOGLE_NAME_DEFAULT);
        init => SetProperty(TTS_GOOGLE_NAME_KEY, value);
    }


    public SsmlVoiceGender TtsGoogleGender
    {
        get
        {
            var input = GetProperty(TTS_GOOGLE_GENDER_KEY, TTS_GOOGLE_GENDER_DEFAULT);
            if (Enum.TryParse<SsmlVoiceGender>(input, out var status))
            {
                return status;
            }

            throw new VoiceException($"Invalid voice gender: {input}");
        }
        init => SetProperty(TTS_GOOGLE_GENDER_KEY, value.ToString());
    }
    
    /// <summary>
    /// The silence threshold amplitude. The higher the number the more sound it thinks is background noise
    /// and should be considered silence. Default is 0.1
    /// </summary>
    public float AnsweringMachineSilenceThresholdAmplitude
    {
        get => float.Parse(GetProperty(AM_SILENCE_AMPLITUDE_KEY, AM_SILENCE_AMPLITUDE_DEFAULT));
        init => SetProperty(AM_SILENCE_AMPLITUDE_KEY, value.ToString(CultureInfo.InvariantCulture));
    }
    
    /// <summary>
    /// The maximum amount of time to wait for speech to begin in seconds. Default is 3.0 seconds. After which,
    /// the software will give up waiting for speech to happen and just return call analysis as connected.
    /// </summary>
    public double AnsweringMachineMaxStartSilenceSeconds
    {
        get => double.Parse(GetProperty(AM_MAX_START_SILENCE_SECONDS_KEY, AM_MAX_START_SILENCE_SECONDS_DEFAULT));
        init => SetProperty(AM_MAX_START_SILENCE_SECONDS_KEY, value.ToString(CultureInfo.InvariantCulture));
    }
    
    /// <summary>
    /// The amount of time between spoken words in seconds. Used to calculate when the person on the call
    /// stops speaking. This will be used to calculate the duration of the greeting. Default is 1.5 seconds.
    /// </summary>
    public double AnsweringMachineEndSpeechSilenceDurationSeconds
    {
        get => double.Parse(GetProperty(AM_END_SPEECH_SILENCE_SECONDS_KEY, AM_END_SPEECH_SILENCE_SECONDS_DEFAULT));
        init => SetProperty(AM_END_SPEECH_SILENCE_SECONDS_KEY, value.ToString(CultureInfo.InvariantCulture));
    }
    
    /// <summary>
    /// The amount of time before the answering machine calculation gives up in seconds. Default is 10.0 seconds. This
    /// could happen if the user rambles on and on. I think this is unlikely but there needs to be a time to stop.
    /// </summary>
    public double AnsweringMachineGiveUpAfterSeconds
    {
        get => double.Parse(GetProperty(AM_GIVE_UP_AFTER_SECONDS_KEY, AM_GIVE_UP_AFTER_SECONDS_DEFAULT));
        init => SetProperty(AM_GIVE_UP_AFTER_SECONDS_KEY, value.ToString(CultureInfo.InvariantCulture));
    }
    
    /// <inheritdoc />
    public new void Dispose()
    {
        _logger.LogDebug("{method}()", nameof(Dispose));
        base.Dispose();
    }
}
