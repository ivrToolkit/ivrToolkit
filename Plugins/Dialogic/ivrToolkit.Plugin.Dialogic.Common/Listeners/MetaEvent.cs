using ivrToolkit.Plugin.Dialogic.Common.DialogicDefs;

namespace ivrToolkit.Plugin.Dialogic.Common.Listeners;

public class MetaEvent
{
    public EventWaitEnum WaitEnum { get; set; }
    public METAEVENT Event { get; set; }
    public int EventHandle { get; set; }
}