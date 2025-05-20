using System;

namespace ivrToolkit.Core.Exceptions
{
    public class DisposedException : VoiceException
    {
        public DisposedException()
        {
        }

        public DisposedException(string message) : base(message)
        {
        }

        public DisposedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
