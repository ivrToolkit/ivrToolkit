using System;
using System.Runtime.Serialization;

namespace ivrToolkit.Core.Exceptions;

public class VoiceException : Exception
{
    public VoiceException()
    {
    }

    protected VoiceException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public VoiceException(string message) : base(message)
    {
    }

    public VoiceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}