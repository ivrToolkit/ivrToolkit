using System;
using System.Runtime.Serialization;

namespace ivrToolkit.Core.Exceptions
{
    public class DisposedException : VoiceException
    {
        public DisposedException()
        {
        }

        protected DisposedException(SerializationInfo info, StreamingContext context) : base(info, context)
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
