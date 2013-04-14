/*
 * Copyright 2013 Troy Makaro
 *
 * This file is part of ivrToolkit, distributed under the GNU GPL. For full terms see the included COPYING file.
 */
using System;

namespace ivrToolkit.Core.Exceptions
{
    /// <summary>
    /// Thrown if the line has been stopped.
    /// </summary>
    public class StopException : VoiceException
    {
        /// <inheritdoc/>
        public StopException()
        {
        }
        /// <inheritdoc/>
        public StopException(string message)
            : base(message)
        {
        }
        /// <inheritdoc/>
        public StopException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
