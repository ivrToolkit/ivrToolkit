// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 
using System;
namespace ivrToolkit.Core.Exceptions
{
    /// <summary>
    /// Thrown if there is a Task fail event
    /// </summary>
    public class RecoveryFailedException : VoiceException
    {
        /// <inheritdoc/>
        public RecoveryFailedException()
        {
        }
        /// <inheritdoc/>
        public RecoveryFailedException(string message)
            : base(message)
        {
        }
        /// <inheritdoc/>
        public RecoveryFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

}
