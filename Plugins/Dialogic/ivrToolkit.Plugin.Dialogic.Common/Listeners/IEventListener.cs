using System;

namespace ivrToolkit.Plugin.Dialogic.Common.Listeners;

public interface IEventListener : IDisposable
{
    public void Start();
    public event EventHandler<MetaEventArgs> OnMetaEvent;
    public void SetEventToWaitFor(int eventToWaitFor);
    public EventWaitEnum WaitForEvent(int waitSeconds);
}