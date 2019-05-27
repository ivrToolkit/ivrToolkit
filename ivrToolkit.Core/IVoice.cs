// 
// Copyright 2013 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the LESSER GNU GPL. For full terms see the included COPYING file and the COPYING.LESSER file.
// 
// 
namespace ivrToolkit.Core
{
    delegate void TestDelegate<T>(T s);
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
        /// <param name="data">Optional parameter for passing in extra data. The dialogic plugin can take a string that represents the device name</param>
        /// <returns>The ILine object representing the requested line number.</returns>
        /// <example>
        /// <code language="C#">
        ///    // pick the line number you want
        ///    ILine line = LineManager.getLine(1);
        /// </code>
        /// </example>
        ILine GetLine(int lineNumber, object data = null);

    }
}
