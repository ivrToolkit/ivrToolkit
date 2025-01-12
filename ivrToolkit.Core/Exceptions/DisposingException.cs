// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 
using System;

namespace ivrToolkit.Core.Exceptions;

/// <summary>
/// Thrown if another thread calls the InitiateDispose method.
/// </summary>
public class DisposingException : VoiceException
{
    /// <inheritdoc/>
    public DisposingException()
    {
    }
    /// <inheritdoc/>
    public DisposingException(string message)
        : base(message)
    {
    }
    /// <inheritdoc/>
    public DisposingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}