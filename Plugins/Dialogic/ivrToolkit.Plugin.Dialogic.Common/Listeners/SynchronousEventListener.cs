using System;
using ivrToolkit.Plugin.Dialogic.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Plugin.Dialogic.Common.Listeners
{
    public class SynchronousEventListener : IEventListener
    {
        private readonly int[] _handles;
        private readonly ILogger<ThreadedEventListener> _logger;
        private readonly EventWaiter _eventWaiter;
        private int _eventToWaitFor;

        public SynchronousEventListener(ILoggerFactory loggerFactory, int[] handles)
        {
            _handles = handles;
            _logger = loggerFactory.CreateLogger<ThreadedEventListener>();
            _logger.LogDebug("Ctr(ILoggerFactory, {0})", _handles);

            _eventWaiter = new EventWaiter(loggerFactory);
        }

        public void Dispose()
        {
            _logger.LogDebug("Dispose()");
        }

        public void Start()
        {
            _logger.LogDebug("Start()");
            _eventWaiter.OnMetaEvent += _eventWaiter_OnMetaEvent;
        }

        private void _eventWaiter_OnMetaEvent(object sender, MetaEventArgs e)
        {
            var raiseEvent = OnMetaEvent;
            raiseEvent?.Invoke(this, new MetaEventArgs(e.EventHandle, e.MetaEvent));
        }

        public event EventHandler<MetaEventArgs> OnMetaEvent;

        public void SetEventToWaitFor(int eventToWaitFor)
        {
            _logger.LogDebug("SetEventToWaitFor({0}:{1})", eventToWaitFor, eventToWaitFor.EventTypeDescription());
            _eventToWaitFor = eventToWaitFor;
        }

        public EventWaitEnum WaitForEvent(int waitSeconds)
        {
            _logger.LogDebug("WaitForEvent({0} seconds)", waitSeconds);
            if (waitSeconds == -1)
            {
                return _eventWaiter.WaitForEvent(_eventToWaitFor, waitSeconds, _handles);
            }

            return _eventWaiter.WaitForEventIndefinitely(_eventToWaitFor, _handles);
        }
    }
}
