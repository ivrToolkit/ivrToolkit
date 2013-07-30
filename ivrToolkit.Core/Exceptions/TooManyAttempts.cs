// 
// Copyright 2013 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the LESSER GNU GPL. For full terms see the included COPYING file and the COPYING.LESSER file.
// 
// 
using System;

namespace ivrToolkit.Core.Exceptions
{
    /// <summary>
    /// Thrown if There have been too many attempts at a prompt. The default is '5' attempts.
    /// </summary>
    public class TooManyAttempts : VoiceException
    {
        /// <inheritdoc/>
        public TooManyAttempts()
        {
        }
        /// <inheritdoc/>
        public TooManyAttempts(string message)
            : base(message)
        {
        }
        /// <inheritdoc/>
        public TooManyAttempts(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
