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
    public abstract class BaseEventListener : IDisposable
    {
        private readonly int[] _handles;
        private readonly ILogger<BoardEventListener> _logger;
        private bool _disposed;
        private int _eventToWaitFor;

        private AutoResetEvent _autoResetEvent;

        private const int DisposingEvent = 1;

        protected BaseEventListener(ILoggerFactory loggerFactory, int[] handles)
        {
            _handles = handles;
            _logger = loggerFactory.CreateLogger<BoardEventListener>();
            _logger.LogDebug("Ctr(ILoggerFactory, {0})", _handles);
        }

        public void Run()
        {
            _logger.LogDebug("Run()");

            try
            {
                while (!_disposed)
                {
                    var metaEvent = WaitForAnyEvent(-1, _handles);
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
                            FireEvent(metaEvent.Event);
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

        private void FireEvent(METAEVENT metaEvt)
        {
            _logger.LogDebug(
                "evt_type = {0}:{1}, evt_dev = {2}, evt_flags = {3},  line_dev = {4} ",
                metaEvt.evttype, metaEvt.evttype.EventTypeDescription(), metaEvt.evtdev, metaEvt.flags,
                metaEvt.linedev);

            if (_eventToWaitFor == metaEvt.evttype)
            {
                _autoResetEvent.Set();
            }
            HandleEvent(metaEvt);
        }

        public MetaEvent WaitForAnyEvent(int waitMilliSeconds, int[] handles, bool showDebug = true)
        {
            if (showDebug) _logger.LogDebug("*** Waiting for any event: waitMilliSeconds = {0}", waitMilliSeconds);
            var eventHandle = 0;

            var result = srllib_h.sr_waitevtEx(handles, handles.Length, waitMilliSeconds, ref eventHandle);
            if (result == -1) return new MetaEvent { WaitEnum = EventWaitEnum.Error };

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

            return new MetaEvent { WaitEnum = EventWaitEnum.Success, Event = metaEvt};
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
            _autoResetEvent = new AutoResetEvent(false);
        }

        public EventWaitEnum WaitForEvent(int waitSeconds)
        {
            _logger.LogDebug("WaitForEvent({0} seconds)", waitSeconds);
            var result = _autoResetEvent.WaitOne(TimeSpan.FromSeconds(waitSeconds));
            _eventToWaitFor = 0;
            if (_disposed) throw new DisposingException();
            return result? EventWaitEnum.Success : EventWaitEnum.Expired;
        }

        protected abstract void HandleEvent(METAEVENT metaEvt);
    }
}
