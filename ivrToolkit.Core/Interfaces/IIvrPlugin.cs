using System;
using ivrToolkit.Core.Util;

namespace ivrToolkit.Core.Interfaces;

/// <summary>
/// Defines the common methods for different plugin implementations.
/// </summary>
public interface IIvrPlugin : IDisposable
{
    /// <summary>
    /// Gets an instance of the line.
    /// </summary>
    /// <param name="lineNumber">The line number you want to create a line for</param>
    protected internal IIvrBaseLine GetLine(int lineNumber);
    
    /// <summary>
    /// Access to the voiceProperties file
    /// </summary>
    VoiceProperties VoiceProperties { get; }

}