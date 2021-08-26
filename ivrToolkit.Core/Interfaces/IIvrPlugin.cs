﻿using System;

namespace ivrToolkit.Core.Interfaces
{
    public interface IIvrPlugin : IDisposable
    {
        /// <summary>
        /// Gets an instance of the line.
        /// </summary>
        /// <param name="lineNumber">The line number you want to create a line for</param>
        ILine GetLine(int lineNumber);

    }
}
