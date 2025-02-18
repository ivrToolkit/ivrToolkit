// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 

using System;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Enums;

namespace ivrToolkit.Core.Interfaces;

/// <summary>
/// This interface exposes the main methods used to control the selected plugin.
/// </summary>
public interface IIvrLine : IIvrBaseLine, IPromptMethods, IPlayMethods
{

    /// <summary>
    /// The Status of the line.
    /// </summary>
    LineStatusTypes Status { get; }

    /// <summary>
    /// Gets the Text To Speech generator being used.
    /// </summary>
    ITextToSpeechGenerator TextToSpeechGenerator { get; }

    /// <summary>
    /// Use this within long computational methods to check if the line has hung-up or stopped.
    /// </summary>
    void CheckDispose();
    
    /// <summary>
    /// Starts an incoming call listener with a callback function.
    /// </summary>
    /// <param name="callback">The function to execute when a call is received.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public void StartIncomingListener(Func<IIvrLine, CancellationToken, Task> callback, CancellationToken cancellationToken);

}