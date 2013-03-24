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
    /// Thrown for any unknown ivrToolKit exception. See InnerException for the underlying cause.
    /// </summary>
    public class VoiceException : Exception
    {
        /// <inheritdoc/>
        public VoiceException()
            : base()
        {
        }
        /// <inheritdoc/>
        public VoiceException(string message)
            : base(message)
        {
        }
        /// <inheritdoc/>
        public VoiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
