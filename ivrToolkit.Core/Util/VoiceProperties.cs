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

    public VoiceProperties(ILoggerFactory loggerFactory, string fileName) : base (loggerFactory, fileName)
    {
        _logger = loggerFactory.CreateLogger<VoiceProperties>();
        _logger.LogDebug("ctr(ILoggerFactory, {0})", fileName);
    }

    /// <summary>
    /// Total number of attempts to try at a prompt before 'TooManyAttemptsException' is thrown. Default is '99'.
    /// </summary>
    public int PromptAttempts => int.Parse(GetProperty("prompt.attempts", "99"));

    /// <summary>
    /// Number of blank entry attempts to try at a prompt before 'TooManyAttemptsException' is thrown. Default is '5'.
    /// </summary>
    public int PromptBlankAttempts => int.Parse(GetProperty("prompt.blankAttempts", "5"));

    /// <summary>
    /// Number of milliseconds between keypress before it considers it to be a prompt attempt. Default is '5000'.
    /// </summary>
    public int DigitsTimeoutInMilli => int.Parse(GetProperty("getDigits.timeoutInMilliseconds", "5000"));
    
    public new void Dispose()
    {
        _logger.LogDebug("Dispose()");
        base.Dispose();
    }
}