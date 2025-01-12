// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 

using System;
using System.Collections.Generic;
using ivrToolkit.Core.Extensions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Core;

/// <summary>
/// The LineManager keeps track of the lines in use.
/// </summary>
public class LineManager : IDisposable
{
    private readonly Dictionary<int, IIvrLine> _lines = new();
    private readonly object _lockObject = new();

    private readonly IIvrPlugin _ivrPlugin;
    private readonly ILogger<LineManager> _logger;

    public LineManager(ILogger<LineManager> logger, IIvrPlugin ivrPlugin)
    {
        _logger = logger.ThrowIfNull(nameof(logger));
        _ivrPlugin = ivrPlugin.ThrowIfNull(nameof(ivrPlugin));

        _logger.LogDebug("ctr()");
    }

    /// <summary>
    /// Gets the line class that will do the line manipulation.
    /// </summary>
    /// 
    /// <param name="lineNumber">The line number to connect to starting at 1</param>
    /// <returns>A class that represents the phone line</returns>
    public IIvrLine GetLine(int lineNumber)
    {
        lineNumber.ThrowIfLessThanOrEqualTo(0, nameof(lineNumber));

        _logger.LogDebug("GetLine({0})", lineNumber);
        lock (_lockObject)
        {
            var line = _ivrPlugin.GetLine(lineNumber);
            _lines.Add(line.LineNumber, line);
            return line;
        }
    }

    public VoiceProperties VoiceProperties => _ivrPlugin.VoiceProperties;

    /// <summary>
    /// Releases a voice line and removes it from the list of used lines.
    /// </summary>
    /// <param name="lineNumber">The line number to release</param>
    public void ReleaseLine(int lineNumber)
    {
        lineNumber.ThrowIfLessThanOrEqualTo(0, nameof(lineNumber));

        _logger.LogDebug("ReleaseLine({0})", lineNumber);
        lock (_lockObject)
        {
            try
            {
                var line = _lines[lineNumber];
                _lines.Remove(lineNumber);
                // some other thread is handling this line and it will get the DisposingException and handle the stopping of the line.

                line.Management.TriggerDispose();
            }
            catch (KeyNotFoundException)
            {
                _logger.LogDebug("line not found");
            }
        }
    }

    /// <summary>
    /// Releases all the voice lines.
    /// </summary>
    public void ReleaseAll()
    {
        _logger.LogDebug("ReleaseAll()");
        lock (_lockObject)
        {
            foreach (var keyValue in _lines)
            {
                var line = keyValue.Value;
                ReleaseLine(line.LineNumber);
            }
        }
    }

    public void Dispose()
    {
        ReleaseAll();
        _logger.LogDebug("Dispose()");
        _ivrPlugin?.Dispose();
    }
}