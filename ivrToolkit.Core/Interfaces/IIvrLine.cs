﻿// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 

using System;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Enums;
using ivrToolkit.Core.Util;

namespace ivrToolkit.Core.Interfaces;

/// <summary>
/// This interface exposes the main methods used to control the selected plugin.
/// </summary>
public interface IIvrLine : IIvrBaseLine, IPromptMethods, IPlayMethods
{

    /// <summary>
    /// The Status of the line.
    /// </summary>
    LineStatusTypes Status
    {
        get;
    }

    /// <summary>
    /// Use this within long computational methods to check if the line has hung-up or stopped.
    /// </summary>
    void CheckDispose();
    
    public void StartIncomingListener(Func<IIvrLine, CancellationToken, Task> callback, CancellationToken cancellationToken);

}