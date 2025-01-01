using System;
using System.Threading;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Plugin.Dialogic.Common.DialogicDefs;
using ivrToolkit.Plugin.Dialogic.Common.Exceptions;
using ivrToolkit.Plugin.Dialogic.Common.Extensions;
using Microsoft.Extensions.Logging;
// ReSharper disable StringLiteralTypo

namespace ivrToolkit.Plugin.Dialogic.Common.Listeners
{
    public class ThreadedEventListener : IEventListener
    {
        private readonly DialogicVoiceProperties _voiceProperties;
        private readonly int[] _handles;
        private readonly ILogger<ThreadedEventListener> _logger;
        private bool _disposed;
        private int _eventToWaitFor;

        private readonly AutoResetEvent _autoResetEvent = new(false);


        private const int DisposingEvent = 1;
        public event EventHandler<MetaEventArgs> OnMetaEvent;

        public ThreadedEventListener(ILoggerFactory loggerFactory, DialogicVoiceProperties voiceProperties, int[] handles)
        {
            _voiceProperties = voiceProperties;
            _handles = handles;
            _logger = loggerFactory.CreateLogger<ThreadedEventListener>();
            _logger.LogDebug("Ctr(ILoggerFactory, {0})", _handles);
        }

        public void Start()
        {
            _logger.LogDebug("Start");
            var thread = new Thread(Run);
            thread.Start();
        }

        public void Run()
        {
            _logger.LogDebug("Run()");

            var waitMilliseconds = _voiceProperties.BackgroundEventListenerTimeoutMilli;

            try
            {
                while (!_disposed)
                {
                    var metaEvent = WaitForAnyEvent(waitMilliseconds, _handles); // default is 5 minutes. It used to be -1 but
                                        // I wanted to see a heartbeat
                    switch (metaEvent.WaitEnum)
                    {
                        case EventWaitEnum.Success:
                            if (metaEvent.Event.evttype == DisposingEvent)
                            {
                                _disposed = true;
                                _autoResetEvent.Set(); // in case some other event is waiting for me.
                                _logger.LogDebug("Disposing event. Run() Completed");
                                return;
                            }
                            FireEvent(metaEvent.Event, metaEvent.EventHandle);
                            break;
                        case EventWaitEnum.Error:
                            _logger.LogError("EventListener failed");
                            return;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Run()");
                return;
            }

            _logger.LogDebug("Run() - completed");
        }

        private void FireEvent(METAEVENT metaEvt, int eventHandle)
        {
            _logger.LogDebug("FireEvent - {eventType}", metaEvt.evttype.EventTypeDescription());
            var raiseEvent = OnMetaEvent;
            raiseEvent?.Invoke(this, new MetaEventArgs(eventHandle, metaEvt));

            if (_eventToWaitFor == metaEvt.evttype)
            {
                _autoResetEvent.Set();
            }
        }

        private MetaEvent WaitForAnyEvent(int waitMilliSeconds, int[] handles, bool showDebug = true)
        {
            if (showDebug) _logger.LogDebug("*** Waiting for any event: waitMilliSeconds = {0} handles = {1}", waitMilliSeconds,
                string.Join(", ", handles));
            var eventHandle = 0;

            var result = srllib_h.sr_waitevtEx(handles, handles.Length, waitMilliSeconds, ref eventHandle);

            if (result == -1) return new MetaEvent { WaitEnum = EventWaitEnum.Expired };

            var metaEvt = new METAEVENT();
            result = gclib_h.gc_GetMetaEventEx(ref metaEvt, eventHandle);
            try
            {
                result.ThrowIfGlobalCallError();
            }
            catch (GlobalCallErrorException e)
            {
                if (e.GC_INFO != null)
                {
                    var error = e.GC_INFO.Value.ccValue;
                    if (error == 0x149)
                    {
                        _logger.LogDebug("GC_STOP detected. We are disposing");
                        return new MetaEvent
                            { WaitEnum = EventWaitEnum.Success, Event = new METAEVENT { evttype = DisposingEvent } };
                    }

                    throw;
                }
            }

            return new MetaEvent { WaitEnum = EventWaitEnum.Success, Event = metaEvt, EventHandle = eventHandle};
        }

        public void Dispose()
        {
            _logger.LogDebug("Dispose()");
            _disposed = true;
            var result = srllib_h.sr_putevt(_handles[0], DisposingEvent, 0, IntPtr.Zero, 0);

            var logLevel = result == -1 ? LogLevel.Error : LogLevel.Debug;
            _logger.Log(logLevel, "Dispose() result = {0}", result);
        }

        public void SetEventToWaitFor(int eventToWaitFor)
        {
            _logger.LogDebug("SetEventToWaitFor({0}:{1})", eventToWaitFor, eventToWaitFor.EventTypeDescription());
            _eventToWaitFor = eventToWaitFor;
            _autoResetEvent.Reset();
        }

        public EventWaitEnum WaitForEvent(int waitSeconds)
        {
            _logger.LogDebug("WaitForEvent({0} seconds)", waitSeconds);
            var result = _autoResetEvent.WaitOne(TimeSpan.FromSeconds(waitSeconds));
            _eventToWaitFor = 0;
            if (_disposed) throw new DisposingException();

            var waitEnum = result ? EventWaitEnum.Success : EventWaitEnum.Expired;
            _logger.LogDebug("WaitForEvent - {0}", waitEnum);
            return waitEnum;
        }

    }
}
