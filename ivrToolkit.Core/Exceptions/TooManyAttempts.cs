// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 
using System;

namespace ivrToolkit.Core.Exceptions;

/// <summary>
/// Thrown if There have been too many attempts at a prompt. The default is '5' attempts.
/// </summary>
public class TooManyAttempts : VoiceException
{
    /// <inheritdoc/>
    public TooManyAttempts()
    {
    }
    /// <inheritdoc/>
    public TooManyAttempts(string message)
        : base(message)
    {
    }
    /// <inheritdoc/>
    public TooManyAttempts(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}