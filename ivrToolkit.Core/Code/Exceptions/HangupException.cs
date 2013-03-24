/*
 * Copyright 2013 Troy Makaro
 *
 * This file is part of ivrToolkit, distributed under the GNU GPL. For full terms see the included COPYING file.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ivrToolkit.Core.Exceptions
{
    /// <summary>
    /// Thrown if the person on the end of the line hangs up.
    /// </summary>
    public class HangupException : VoiceException
    {
        /// <inheritdoc/>
        public HangupException()
            : base()
        {
        }
        /// <inheritdoc/>
        public HangupException(string message)
            : base(message)
        {
        }
        /// <inheritdoc/>
        public HangupException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
