/*
 * Copyright 2013 Troy Makaro
 *
 * This file is part of ivrToolkit, distributed under the GNU GPL. For full terms see the included COPYING file.
 */
namespace ivrToolkit.Core
{
    /// <summary>
    /// All Plugins for the ivrTool.Core engine must implement this Interface.
    /// </summary>
    /// <example>
    /// <code language="C#">
    ///    // pick the line number you want
    ///    ILine line = LineManager.getLine(1);
    /// </code>
    /// </example>
    public interface IVoice
    {
        /// <summary>
        /// Gets the ILine object for the requested line number.
        /// </summary>
        /// <param name="lineNumber">The line number you want to use</param>
        /// <returns>The ILine object representing the requested line number.</returns>
        /// <example>
        /// <code language="C#">
        ///    // pick the line number you want
        ///    ILine line = LineManager.getLine(1);
        /// </code>
        /// </example>
        ILine GetLine(int lineNumber);
    }
}
