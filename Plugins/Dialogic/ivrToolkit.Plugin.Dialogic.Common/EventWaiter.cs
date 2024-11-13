using ivrToolkit.Core.Exceptions;
using ivrToolkit.Plugin.Dialogic.Common.DialogicDefs;
using ivrToolkit.Plugin.Dialogic.Common.Extensions;
using Microsoft.Extensions.Logging;
using System;

namespace ivrToolkit.Plugin.Dialogic.Common
{

    // Define a class to hold custom event info
    public class MetaEventArgs : EventArgs
    {
        public MetaEventArgs(int eventHandle, METAEVENT metaEvent)
        {
            EventHandle = eventHandle;
            MetaEvent = metaEvent;
        }

        public METAEVENT MetaEvent { get; set; }
        public int EventHandle { get; set; }
    }

    public enum  EventWaitEnum
    {
        Expired = -2,
        Error = -1,
        Success = 1
    }

    public class EventWaiter
    {
        public const int SyncWaitInfinite = -1;

        private ILogger<EventWaiter> _logger;
        private bool _disposeTriggerActivated;

        public event EventHandler<MetaEventArgs> OnMetaEvent;

        public bool DisposeTriggerActivated
        {
            set => _disposeTriggerActivated = value;
        }

        public EventWaiter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<EventWaiter>();
            _logger.LogDebug("Ctr()");
        }

        public EventWaitEnum WaitForEventIndefinitely(int waitForEvent, int[] handles)
        {
            _logger.LogDebug("*** Waiting for event: {0}: {1}, waitSeconds = indefinitely", waitForEvent, waitForEvent.EventTypeDescription());

            EventWaitEnum result;
            while ((result = WaitForEvent(waitForEvent, 5, handles, false)) == EventWaitEnum.Expired) // wait  5 seconds
            {
                _logger.LogTrace("Wait for call exhausted. Will try again");
                CheckDisposing();
            }

            return result;
        }

        public EventWaitEnum WaitForThisEventOnly(int waitForEvent, int waitSeconds, int[] handles, bool showDebug = true)
        {
            return WaitForEvent(waitForEvent, waitSeconds, handles, showDebug, true);
        }

        public EventWaitEnum WaitForEvent(int waitForEvent, int waitSeconds, int[] handles, bool showDebug = true, bool waitExact = false)
        {
            if (showDebug) _logger.LogDebug("*** Waiting for event: {0}: {1}, waitSeconds = {2}", waitForEvent, waitForEvent.EventTypeDescription(), waitSeconds);

            var eventThrown = -1;
            var count = 0;
            var eventHandle = 0;

            do
            {
                var result = srllib_h.sr_waitevtEx(handles, handles.Length, 1000, ref eventHandle);
                var timedOut = result == -1;
                if (!timedOut)
                {
                    var metaEvt = GetEvent(eventHandle);
                    eventThrown = metaEvt.evttype;

                    if (waitExact)
                    {
                        // wait for the exact event only. Will fire extension events too since they are only informational
                        if (eventThrown == waitForEvent || eventThrown == gclib_h.GCEV_EXTENSION)
                        {
                            FireEvent(eventHandle, metaEvt);
                        }
                        else
                        {
                            _logger.LogDebug("Skipping event {0}:{1}", eventThrown, eventThrown.EventTypeDescription());
                        }
                    }
                    else
                    {
                        FireEvent(eventHandle, metaEvt);
                    }
                    if (eventThrown == waitForEvent) break;
                }
                CheckDisposing();

                count++;
            } while (LoopAgain(eventThrown, waitForEvent, count, waitSeconds));

            if (eventThrown == waitForEvent)
            {
                return EventWaitEnum.Success;
            }

            if (HasExpired(count, waitSeconds))
            {
                return EventWaitEnum.Expired;
            }

            return EventWaitEnum.Error;
        }

        private METAEVENT GetEvent(int eventHandle)
        {
            _logger.LogDebug("GetEvent({0})", eventHandle);
            var metaEvt = new METAEVENT();

            var result = gclib_h.gc_GetMetaEventEx(ref metaEvt, eventHandle);
            result.ThrowIfGlobalCallError();
            return metaEvt;
        }

        private void FireEvent(int eventHandle, METAEVENT metaEvt)
        {
            _logger.LogDebug("FireEvent({0})", eventHandle);

            var result = gclib_h.gc_GetMetaEventEx(ref metaEvt, eventHandle);
            result.ThrowIfGlobalCallError();

            _logger.LogDebug(
                "evt_type = {0}:{1}, evt_dev = {2}, evt_flags = {3},  line_dev = {4} ",
                metaEvt.evttype, metaEvt.evttype.EventTypeDescription(), metaEvt.evtdev, metaEvt.flags,
                metaEvt.linedev);

            EventHandler<MetaEventArgs> raiseEvent = OnMetaEvent;
            if (raiseEvent != null)
            {
                raiseEvent(this, new MetaEventArgs(eventHandle, metaEvt));
            }

            return;
        }

        private bool HasExpired(int count, int waitSeconds)
        {
            _logger.LogTrace("HasExpired({0}, {1})", count, waitSeconds);

            if (waitSeconds == SyncWaitInfinite)
            {
                return false;
            }

            if (count > waitSeconds)
            {
                return true;
            }

            return false;
        }

        private bool LoopAgain(int eventThrown, int waitForEvent, int count, int waitSeconds)
        {
            _logger.LogTrace("LoopAgain({0}, {1}, {2}, {3})", eventThrown, waitForEvent, count, waitSeconds);

            var hasEventThrown = false;
            var hasExpired = false;

            if (eventThrown == waitForEvent)
            {
                hasEventThrown = true;
            }

            if (HasExpired(count, waitSeconds))
            {
                hasExpired = true;
            }

            if (hasEventThrown || hasExpired)
            {
                return false;
            }

            return true;
        }

        private void CheckDisposing()
        {
            if (_disposeTriggerActivated) ThrowDisposingException();
        }

        private void ThrowDisposingException()
        {
            _logger.LogDebug("ThrowDisposingException()");
            _disposeTriggerActivated = false;
            throw new DisposingException();
        }
    }
}
