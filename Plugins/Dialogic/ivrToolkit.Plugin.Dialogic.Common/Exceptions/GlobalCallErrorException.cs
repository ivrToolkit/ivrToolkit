using System;
using System.Linq;
using ivrToolkit.Plugin.Dialogic.Common.DialogicDefs;

namespace ivrToolkit.Plugin.Dialogic.Common.Exceptions;

public class GlobalCallErrorException : Exception
{
    private GC_INFO? _gcInfo = null;

    public GlobalCallErrorException()
    {
    }

    public GlobalCallErrorException(GC_INFO gcInfo)
    {
        _gcInfo = gcInfo;
    }

    public override string Message
    {
        get
        {
            if (_gcInfo == null) return "Failed to retrieve global call error info";

            var info = _gcInfo.Value;
            return $"gcError: 0x{info.gcValue:x}, gcMessage: {info.gcMsg}, \n\tccLibName: {info.ccLibName}, \n\tccError: 0x{info.ccValue:x}, gcMessage: {info.ccMsg}, \n\tAdditional Info: {info.additionalInfo}\n";
        }
    }

    public GC_INFO? GC_INFO => _gcInfo;

    public override string StackTrace
    {
        get
        {
            var relevantStackFrames =
                base.StackTrace
                    .Split('\n')
                    .Skip(1)
                    .ToArray();
            return string.Join("\n", relevantStackFrames);
        }
    }
}