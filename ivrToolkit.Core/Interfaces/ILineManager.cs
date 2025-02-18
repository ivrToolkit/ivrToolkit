using System;
using ivrToolkit.Core.Util;

namespace ivrToolkit.Core.Interfaces;

/// <summary>
/// The LineManager keeps track of the lines in use.
/// </summary>
public interface ILineManager : IDisposable
{
    /// <summary>
    /// Gets the line class that will do the line manipulation.
    /// </summary>
    /// 
    /// <param name="lineNumber">The line number to connect to starting at 1</param>
    /// <returns>A class that represents the phone line</returns>
    IIvrLine GetLine(int lineNumber);

    /// <summary>
    /// Gets an available line. With SipSorcery, there is no need to specify a line number however,
    /// the line manager still uses them internally to manage them.
    /// </summary>
    /// 
    /// <returns>A class that represents the phone line</returns>
    IIvrLine GetLine();

    /// <summary>
    /// Returns the voice properties definition.
    /// </summary>
    VoiceProperties VoiceProperties { get; }

    /// <summary>
    /// Releases a voice line and removes it from the list of used lines.
    /// </summary>
    /// <param name="lineNumber">The line number to release</param>
    void ReleaseLine(int lineNumber);

    /// <summary>
    /// Releases all the voice lines.
    /// </summary>
    void ReleaseAll();
}