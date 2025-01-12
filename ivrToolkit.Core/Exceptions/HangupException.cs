// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 
using System;

namespace ivrToolkit.Core.Exceptions;

/// <summary>
/// Thrown if the person on the end of the line hangs up.
/// </summary>
public class HangupException : VoiceException
{
    /// <inheritdoc/>
    public HangupException()
    {
    }
    /// <inheritdoc/>
    public HangupException(string message)
        : base(message)
    {
    }
    /// <inheritdoc/>
    public HangupException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}