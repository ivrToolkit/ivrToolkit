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
    public class GetDigitsTimeoutException : VoiceException
    {
        public GetDigitsTimeoutException()
            : base()
        {
        }
        public GetDigitsTimeoutException(string message)
            : base(message)
        {
        }
        public GetDigitsTimeoutException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
