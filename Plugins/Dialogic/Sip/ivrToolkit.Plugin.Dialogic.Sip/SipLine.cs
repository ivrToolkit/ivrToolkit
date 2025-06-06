using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using ivrToolkit.Core.Enums;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Core.Util;
using ivrToolkit.Plugin.Dialogic.Common;
using ivrToolkit.Plugin.Dialogic.Common.DialogicDefs;
using ivrToolkit.Plugin.Dialogic.Common.Exceptions;
using ivrToolkit.Plugin.Dialogic.Common.Extensions;
using Microsoft.Extensions.Logging;

// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

namespace ivrToolkit.Plugin.Dialogic.Sip
{
    public class SipLine : IIvrBaseLine, IIvrLineManagement
    {
        private readonly int _lineNumber;
        private readonly ILogger<SipLine> _logger;

        // keep track of state myself
        private StateProgress _stateProgress;

        // if I need to keep unmanaged memory in scope for the duration of this class
        private UnmanagedMemoryService _unmanagedMemoryService;

        // If I need to keep unmanaged memory in scope for the duration of the call
        private UnmanagedMemoryService _unmanagedMemoryServicePerCall;

        private readonly DialogicSipVoiceProperties _voiceProperties;

        private int _callReferenceNumber;
        private DX_XPB _currentXpb;
        private int _dxDev; // for device name = "dxxxB{boardId}C{channelId}"

        private int _gcDev; // for device name = ":P_SIP:N_iptB1T{_lineNumber}:M_ipmB1C{id}:V_dxxxB{boardId}C{channelId}"

        private int _ipmDev;
        private IntPtr _ipXslot;

        private int _volume;
        private IntPtr _voxXslot;
        private bool _waitCallSet;
        private readonly ILoggerFactory _loggerFactory;
        private bool _capDisplayed;
        private readonly EventWaiter _eventWaiter;
        private readonly ProcessExtension _processExtension;

        public IIvrLineManagement Management => this;
        public string LastTerminator { get; }


        public int LineNumber => _lineNumber;

        private bool _inCallProgressAnalysis;
        private bool _dropCallHappening;

        private enum HangupStateEnum
        {
            NotHungUp,
            Starting,
            Completed
        }

        private HangupStateEnum _hangupState = HangupStateEnum.NotHungUp;

        // ReSharper disable once InconsistentNaming
        private static readonly object _lockObject = new();

        public SipLine(ILoggerFactory loggerFactory, DialogicSipVoiceProperties voiceProperties, int lineNumber)
        {
            _voiceProperties = voiceProperties;
            _lineNumber = lineNumber;
            LastTerminator = "";
            _logger = loggerFactory.CreateLogger<SipLine>();
            _loggerFactory = loggerFactory;
            _logger.LogDebug("ctr(ILoggerFactory, VoiceProperties, {0})", lineNumber);

            _eventWaiter = new EventWaiter(_loggerFactory);
            _eventWaiter.OnMetaEvent += MetaEvent;

            _processExtension = new ProcessExtension(loggerFactory);

            _logger.LogDebug("{key} = {value}", DialogicSipVoiceProperties.SIP_IGNORE_CALL_STATE_CHECK_KEY, voiceProperties.IgnoreCallStateCheck);
            _logger.LogDebug("{key} = {value}", DialogicSipVoiceProperties.ATTEMPT_RECOVERY_START_POSITION, voiceProperties.AttemptRecoveryStartPosition);
            _logger.LogDebug("{key} = {value}", DialogicSipVoiceProperties.ATTEMPT_RECOVERY_RESETLINEDEV_SUCCESS, voiceProperties.AttemptRecoveryReturnOnResetLineDevSuccess);
            _logger.LogDebug("{key} = {value}", DialogicSipVoiceProperties.ATTEMPT_RECOVERY_TRY_REOPEN_ON, voiceProperties.AttemptRecoveryTryReopenOn);
            _logger.LogDebug("{key} = {value}", DialogicSipVoiceProperties.ATTEMPT_RECOVERY_THROW_FAILURE_ON, voiceProperties.AttemptRecoveryThrowFailureOn);
            Start();
        }

        private void Start()
        {
            _logger.LogDebug("Start() - Starting line: {0}", _lineNumber);

            _unmanagedMemoryService = new UnmanagedMemoryService(_loggerFactory, $"Lifetime of {nameof(SipLine)}");
            _unmanagedMemoryServicePerCall = new UnmanagedMemoryService(_loggerFactory, "Per Call");

            Open();
            SetDefaultFileType();
        }

        private void Open()
        {
            _logger.LogDebug("Open() - Opening line: {0}", _lineNumber);

            var id = _lineNumber + _voiceProperties.SipChannelOffset;

            var boardId = (id - 1) / 4 + 1;
            var channelId = id - (boardId - 1) * 4;

            var devName = $"dxxxB{boardId}C{channelId}";

            _dxDev = DXXXLIB_H.dx_open(devName, 0);
            _logger.LogDebug("get _dxDev = {0} = DXXXLIB_H.dx_open({1}, 0)", _dxDev, devName);

            var result = DXXXLIB_H.dx_setevtmsk(_dxDev, DXXXLIB_H.DM_RINGS | DXXXLIB_H.DM_DIGITS | DXXXLIB_H.DM_LCOF);
            result.ThrowIfStandardRuntimeLibraryError(_dxDev);

            devName = $":P_SIP:N_iptB1T{_lineNumber}:M_ipmB1C{id}:V_dxxxB{boardId}C{channelId}";

            _gcDev = 0;
            result = gclib_h.gc_OpenEx(ref _gcDev, devName, DXXXLIB_H.EV_SYNC, IntPtr.Zero);
            _logger.LogDebug("get _gcDev: result = {0} = gclib_h.gc_OpenEx(ref {1}, devName, {2}, IntPtr.Zero)", result,
                _gcDev, DXXXLIB_H.EV_SYNC);
            result.ThrowIfGlobalCallError();

            //Enabling GCEV_INVOKE_XFER_ACCEPTED Events
            var gcParmBlkPtr = IntPtr.Zero;

            //setting T.38 fax server operating mode: IP MANUAL mode
            result = gclib_h.gc_util_insert_parm_val(ref gcParmBlkPtr, gccfgparm_h.GCSET_CALLEVENT_MSK,
                gclib_h.GCACT_ADDMSK, sizeof(int), gclib_h.GCMSK_INVOKEXFER_ACCEPTED);
            result.ThrowIfGlobalCallError();

            var requestId = 0;
            result = gclib_h.gc_SetConfigData(gclib_h.GCTGT_GCLIB_CHAN, _gcDev, gcParmBlkPtr, 0,
                gclib_h.GCUPDATE_IMMEDIATE, ref requestId, DXXXLIB_H.EV_SYNC);
            result.ThrowIfGlobalCallError();

            gclib_h.gc_util_delete_parm_blk(gcParmBlkPtr);

            SetDtmf();
            ConnectVoice();
        }

        private void SetDefaultFileType()
        {
            _logger.LogDebug("SetDefaultFileType()");
            _currentXpb = new DX_XPB
            {
                wFileFormat = DXXXLIB_H.FILE_FORMAT_WAVE,
                wDataFormat = DXXXLIB_H.DATA_FORMAT_PCM,
                nSamplesPerSec = DXXXLIB_H.DRT_8KHZ,
                wBitsPerSample = 8
            };
        }
        /**
        * Set the channel to use DTMF for all calls.
        */
        private void SetDtmf()
        {
            _logger.LogDebug("SetDetm()");

            var gcParmBlkPtr = IntPtr.Zero;

            var result = gclib_h.gc_util_insert_parm_val(ref gcParmBlkPtr, gcip_defs_h.IPSET_DTMF,
                gcip_defs_h.IPPARM_SUPPORT_DTMF_BITMASK,
                sizeof(byte), gcip_defs_h.IP_DTMF_TYPE_RFC_2833);
            result.ThrowIfGlobalCallError();

            result = gclib_h.gc_util_insert_parm_val(ref gcParmBlkPtr, gcip_defs_h.IPSET_DTMF,
                gcip_defs_h.IPPARM_DTMF_RFC2833_PAYLOAD_TYPE,
                sizeof(byte), gcip_defs_h.IP_USE_STANDARD_PAYLOADTYPE);
            result.ThrowIfGlobalCallError();

            result = gclib_h.gc_SetUserInfo(gclib_h.GCTGT_GCLIB_CHAN, _gcDev, gcParmBlkPtr, gclib_h.GC_ALLCALLS);
            result.ThrowIfGlobalCallError();

            gclib_h.gc_util_delete_parm_blk(gcParmBlkPtr);
        }

        /**
        * Connect voice to the channel.
        */
        private void ConnectVoice()
        {
            _logger.LogDebug("ConnectVoice() - _dxDev = {0}, _gcDev = {1}", _dxDev, _gcDev);

            var scTsinfo = new SC_TSINFO();
            var result = gclib_h.gc_GetResourceH(_gcDev, ref _ipmDev, gclib_h.GC_MEDIADEVICE);
            result.ThrowIfGlobalCallError();

            _ipXslot = Marshal.AllocHGlobal(4);
            Marshal.WriteInt32(_ipXslot, 0);
            _unmanagedMemoryService.Push("_ipXslot", _ipXslot);

            scTsinfo.sc_numts = 1;
            scTsinfo.sc_tsarrayp = _ipXslot;

            result = gclib_h.gc_GetXmitSlot(_gcDev, ref scTsinfo);
            result.ThrowIfGlobalCallError();

            result = DXXXLIB_H.dx_listen(_dxDev, ref scTsinfo);
            result.ThrowIfStandardRuntimeLibraryError(_dxDev);

            _voxXslot = Marshal.AllocHGlobal(4);
            Marshal.WriteInt32(_voxXslot, 0);
            _unmanagedMemoryService.Push("_voxXslot", _voxXslot);


            scTsinfo.sc_numts = 1;
            scTsinfo.sc_tsarrayp = _voxXslot;

            result = DXXXLIB_H.dx_getxmitslot(_dxDev, ref scTsinfo);
            result.ThrowIfStandardRuntimeLibraryError(_dxDev);

            result = gclib_h.gc_Listen(_gcDev, ref scTsinfo, DXXXLIB_H.EV_SYNC);
            result.ThrowIfGlobalCallError();
        }

        public void WaitRings(int rings)
        {
            _logger.LogDebug("(SIP) - WaitRings({0})", rings);

            _stateProgress = new StateProgress();
            _hangupState = HangupStateEnum.NotHungUp;

            // a hangup can interupt a TDX_PLAY,TDX_RECORD or TDX_GETDIG. I try and clear the buffer then but the event doesn't always happen in time
            // so this is one more attempt to clear _dxDev events. Not that it really matters because I don't action on those events anyways. 
            ClearStopPlayRecordGetDigitEvents(_dxDev, 1000);

            ClearDigits(_dxDev); // make sure we are starting with an empty digit buffer

            _unmanagedMemoryServicePerCall.Dispose(); // there is unlikely anything to free. This is just a fail safe.

            var crnPtr = IntPtr.Zero;


            int result;

            if (!_waitCallSet)
            {
                // this method only needs to be called once unless the line is reset or closed.
                _waitCallSet = true;
                TraceCallStateChange(() =>
                {
                    result = gclib_h.gc_WaitCall(_gcDev, crnPtr, IntPtr.Zero, 0, DXXXLIB_H.EV_ASYNC);
                    result.ThrowIfGlobalCallError();
                }, "gc_WaitCall");
            }
            var eventWaitEnum = _eventWaiter.WaitForEventIndefinitely(gclib_h.GCEV_OFFERED, new[] { _dxDev, _gcDev });
            switch (eventWaitEnum)
            {
                case EventWaitEnum.Success:
                    _logger.LogDebug("(SIP) - The WaitRings method received the GCEV_OFFERED event");
                    break;
                case EventWaitEnum.Error:
                    var message = "(SIP) - The WaitRings method failed waiting for the GCEV_OFFERED event";
                    _logger.LogError(message);
                    throw new VoiceException(message);
            }

            // Now that a call is offered, we wait for a finite amount of time to answer
            eventWaitEnum = _eventWaiter.WaitForEvent(gclib_h.GCEV_ANSWERED, 15, new[] { _dxDev, _gcDev });
            switch (eventWaitEnum)
            {
                case EventWaitEnum.Success:
                    _logger.LogDebug("(SIP) - The WaitRings method received the GCEV_ANSWERED event");
                    break;
                case EventWaitEnum.Expired:
                    _logger.LogWarning("(SIP) - The WaitRings method did not receive the GCEV_ANSWERED event within 15 seconds.");
                    break;
                case EventWaitEnum.Error:
                    var message = "(SIP) - The WaitRings method failed waiting for the GCEV_ANSWERED event";
                    _logger.LogError(message);
                    throw new VoiceException(message);
            }
            HandlePossibleHangupInProgress(forceHangup: true); // check if a hangup is in progress
        }

        private void TraceCallStateChange(Action operation, string operationName)
        {

            var preChannelState = DXXXLIB_H.ATDX_STATE(_dxDev);

            var preCallState = GetCallState();

            try
            {
                _logger.LogDebug(
                    "(SIP) - Begin: {operationName} - call state[channel state]: {preCallState}[{preChannelState}]",
                    operationName,
                    preCallState.CallStateDescription(),
                    preChannelState.ChannelStateDescription());
                operation();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "From TraceCallStateChange");
            }
            finally
            {
                var postChannelState = DXXXLIB_H.ATDX_STATE(_dxDev); // put this here because I am hoping it will work always
                var postCallState = GetCallState();
                _logger.LogDebug("(SIP) -   End: {operationName} - call state[channel state]: {postCallState}[{postChannelState}]", operationName,
                    postCallState.CallStateDescription(),
                    postChannelState.ChannelStateDescription());
            }
        }

        public void Hangup()
        {
            _logger.LogDebug("(SIP) - Hangup(); - crn = {0}", _callReferenceNumber);

            // this hangup method never happens during a play, record or getdigits so it should be safe
            StopPlayRecordGetDigitsImmediately("Hangup");

            if (_callReferenceNumber == 0) return; // line is not in use

            if (_dropCallHappening)
            {
                _logger.LogDebug("(SIP) - A gc_dropCall is already in progress. Can't hangup twice");
                return;
            }
            _dropCallHappening = true;


            TraceCallStateChange(() =>
            {
                var result = gclib_h.gc_DropCall(_callReferenceNumber, gclib_h.GC_NORMAL_CLEARING, DXXXLIB_H.EV_ASYNC);
                result.ThrowIfGlobalCallError();
            }, "gc_DropCall (from hangup method)");

            // okay, now lets wait for the release call event
            // 70 seconds should be way overkill but I want to see if longer times solves my hangup problem
            var eventWaitEnum = _eventWaiter.WaitForEvent(gclib_h.GCEV_RELEASECALL, 70, new[] { _dxDev, _gcDev }); // Should fire a hangup exception
            _logger.LogDebug("(SIP) - The result of the wait for GCEV_RELEASECALL(70 seconds) = {0}", eventWaitEnum);

            switch (eventWaitEnum)
            {
                case EventWaitEnum.Success:
                    // this should never happen!
                    _logger.LogDebug("(SIP) - The hangup method completed as expected.");
                    break;
                case EventWaitEnum.Expired:
                    _logger.LogWarning("(SIP) - The hangup method did not receive the releaseCall event");
                    AttemptRecovery();
                    break;
                case EventWaitEnum.Error:
                    _logger.LogError("(SIP) - The hangup method failed waiting for the releaseCall event");
                    break;
            }
        }

        public void TakeOffHook()
        {
            /*
             * Sip Does not need to take the receiver off the hook
             */
        }

        public CallAnalysis Dial(string number, int answeringMachineLengthInMilliseconds)
        {
            _inCallProgressAnalysis = true;
            try
            {
                _logger.LogDebug("Dial({0}, {1})", number, answeringMachineLengthInMilliseconds);
                _unmanagedMemoryServicePerCall.Dispose(); // there is unlikely anything to release, this is just a failsafe.

                // should never be in this state but clear it just in case
                StopPlayRecordGetDigitsImmediately("Dial",true);
                ClearStopPlayRecordGetDigitEvents(_dxDev, 1000);

                _stateProgress = new StateProgress();
                _hangupState = HangupStateEnum.NotHungUp;

                ClearDigits(_dxDev); // make sure we are starting with an empty digit buffer

                return DialWithCpa(number, answeringMachineLengthInMilliseconds);

            }
            finally
            {
                _inCallProgressAnalysis = false;
            }

        }

        public CallStateProgressEnum GetCallStateProgress()
        {
            return _stateProgress.GetCallStateProgress();
        }

        /// <summary>
        /// Dials a phone number using call progress analysis.
        /// </summary>
        /// <param name="number">The phone number to dial.</param>
        /// <param name="answeringMachineLengthInMilliseconds">Answering machine length in milliseconds</param>
        /// <returns>CallAnalysis Enum</returns>
        private CallAnalysis DialWithCpa(string number, int answeringMachineLengthInMilliseconds)
        {
            _logger.LogDebug("(SIP) - DialWithCpa({0}, {1})", number, answeringMachineLengthInMilliseconds);

            var cap = GetCap();

            var ani = $"{_voiceProperties.SipAlias}@{_voiceProperties.SipProxyIp}"; // automatic number identification (from)
            var dnis = $"{number}@{_voiceProperties.SipProxyIp}"; // dialed number identification service (to)

            MakeCall(ani, dnis);

            // check the CPA
            var startTime = DateTimeOffset.Now;


            TraceCallStateChange(() =>
            {
                var result = DXXXLIB_H.dx_dial(_dxDev, "", ref cap, DXCALLP_H.DX_CALLP | DXXXLIB_H.EV_ASYNC);
                result.ThrowIfStandardRuntimeLibraryError(_dxDev);
            }, "dx_dial");

            var eventWaitEnum = _eventWaiter.WaitForEvent(DXXXLIB_H.TDX_CALLP, 60, new[] { _dxDev, _gcDev }); // 60 seconds
            if (eventWaitEnum == EventWaitEnum.Success) _inCallProgressAnalysis = false;

            switch (eventWaitEnum)
            {
                case EventWaitEnum.Success:
                    _logger.LogDebug("(SIP) - Check CPA duration = {0} seconds. Received TDX_CALLP", (DateTimeOffset.Now - startTime).TotalSeconds);
                    break;
                case EventWaitEnum.Expired:
                    var message = $"(SIP) - Check CPA duration = {(DateTimeOffset.Now - startTime).TotalSeconds} seconds. Timed out waiting for TDX_CALLP";
                    _logger.LogError(message);
                    AttemptRecovery();
                    return CallAnalysis.Error;
                case EventWaitEnum.Error:
                    message = $"(SIP) - Check CPA duration = {(DateTimeOffset.Now - startTime).TotalSeconds} seconds. Failed waiting for TDX_CALLP";
                    _logger.LogError(message);
                    AttemptRecovery();
                    return CallAnalysis.Error;
            }
            
            _logger.LogDebug("(SIP) - Last event was: {lastEventState}, last call state was: {lastCallState}", 
                _stateProgress.LastEventState.EventTypeDescription(), _stateProgress.LastCallState.CallStateDescription());

            var callState = GetCallState();
            var channelState = DXXXLIB_H.ATDX_STATE(_dxDev);

            // get the CPA result
            var callProgressResult = DXXXLIB_H.ATDX_CPTERM(_dxDev);

            _logger.LogDebug("(SIP) - Call Progress Analysis Result - {callResult} - {callState}[{channelState}] - ({events})", 
                callProgressResult.CallProgressDescription(),
                callState.CallStateDescription(), channelState.ChannelStateDescription(), _stateProgress.GetCallStateProgress());


            if (_hangupState == HangupStateEnum.Starting)
            {
                // we should attempt to finish the hangup here
                try
                {
                    // I have seen this scenario:
                    //  - gc_releaseCallEx executed
                    //  - CPA finishes before receiving GCEV_RELEASECALL
                    // 
                    // Without this next step, call state is probably alerting and I would return NoAnswer.
                    //    then a hangup would occur which tries to do a gc_dropCall which will fail and probably
                    //    trigger a AttemptRecovery() when it wasn't needed.
                    HandlePossibleHangupInProgress(ignoreStateCheck: true); // ignore state check like alerting for example
                }
                catch (HangupException)
                {
                    _logger.LogDebug("Hangup caught during CPA");
                    return CallAnalysis.NoAnswer;
                }
            }


            switch (callProgressResult)
            {
                case DXCALLP_H.CR_BUSY:
                    return CallAnalysis.Busy;
                case DXCALLP_H.CR_CEPT:
                    return CallAnalysis.OperatorIntercept;
                case DXCALLP_H.CR_CNCT:
                    return HandleCallProgressConnected(callState);
                case DXCALLP_H.CR_ERROR:
                    return CallAnalysis.Error;
                case DXCALLP_H.CR_FAXTONE:
                    return CallAnalysis.FaxTone;
                case DXCALLP_H.CR_NOANS:
                    return CallAnalysis.NoAnswer;
                case DXCALLP_H.CR_NODIALTONE:
                    return CallAnalysis.NoDialTone;
                case DXCALLP_H.CR_NORB:
                    // see went throught the dialing, proceeding, alerting and connected states
                    if (_stateProgress.IsRegularDial())
                    {
                        // at this point, there was a proper dialing handshake but the CPA wasn't able to complete within
                        // 40 seconds.
                        // I've been able to recreate this by simplying answering the phone and saying nothing.
                        
                        _logger.LogDebug("(SIP) - CPA result = NoRingback but dialStateProgress is regular dial. One way this happens if the callee picks up the phone and says nothing for 40 seconds. Will treat this as a NoAnswer");
                        return CallAnalysis.NoAnswer;
                    }
                    return CallAnalysis.NoRingback;
                case DXCALLP_H.CR_STOPD:
                    // calling method will check and throw the stopException
                    return CallAnalysis.Stopped;
            }

            throw new VoiceException($"Unknown dail response: {callProgressResult}");
        }

        // callProgressResult = connected. Doesn't mean the call state is connected.
        private CallAnalysis HandleCallProgressConnected(int callState)
        {
            var connType = DXXXLIB_H.ATDX_CONNTYPE(_dxDev);
            // display the reason for the connection from CPA
            LogHowConnected(connType);

            if (callState == gclib_h.GCST_CONNECTED)
            {
                return connType == DXCALLP_H.CON_PAMD ? CallAnalysis.AnsweringMachine : CallAnalysis.Connected;
            }

            // callProgressResult result is connected but the call state is not connected
            if (_callReferenceNumber == 0)
            {
                // this can happen if someone picks up and then immediately hangs up. Treat as a noAnswer
                // the software will hang up even before call progress has completed
                _logger.LogDebug(
                    "(SIP) - CPA result = connected but crn = 0. This happens if a hangup is detected before call progress has completed. Will treat this as a NoAnswer");
                return CallAnalysis.NoAnswer;
            }

            if (callState == gclib_h.GCST_ALERTING)
            {
                _logger.LogDebug("(SIP) - CPA result = connected but state = alerting.");
                // this can happen if the callee rings and then immediately disconnectes
                // it leaves the state in alerting
                return CallAnalysis.NoAnswer;
            }

            _logger.LogWarning(
                "(SIP) - CPA result = connected but state = {0}. Hangup and return CallAnalysis.Error",
                callState.CallStateDescription());
            return CallAnalysis.Error;
        }

        private void LogHowConnected(int connType)
        {
            switch (connType)
            {
                case DXCALLP_H.CON_CAD:
                    _logger.LogDebug("Connection due to cadence break ");
                    break;
                case DXCALLP_H.CON_DIGITAL:
                    _logger.LogDebug("con_digital");
                    break;
                case DXCALLP_H.CON_LPC:
                    _logger.LogDebug("Connection due to loop current");
                    break;
                case DXCALLP_H.CON_PAMD: // ca_intflg = 8 PAMD + OPTEN
                    _logger.LogDebug("Connection due to Positive Answering Machine Detection");
                    break;
                case DXCALLP_H.CON_PVD:
                    _logger.LogDebug("Connection due to Positive Voice Detection");
                    break;
                default:
                    _logger.LogDebug("Unknown connection type: {connType}", connType);
                    break;
            }
        }

        /**
        * Make a call.
        * Please note,
        * The call header sets the USER_DISPLAY.
        * The USER_DISPLAY is blocked by carriers (Fido, Telus, etc.)
        * The USER_DISPLAY can also be set using the PBX.
        */

        // from = ani = automatic number identification. Example: "200@192.168.1.40"
        // to = dnis = dialed number identification service. Example: "2348675309@192.168.1.40"
        private void MakeCall(string from, string to)
        {
            _logger.LogDebug("MakeCall({0}, {1})", from, to);
            DisplayCallState();

            if (_callReferenceNumber != 0)
            {
                // This is the one, that I want to try absolutely everything and then throw an error if it doesn't work
                AttemptRecovery(true); // signal it is for gc_makeCall
            }

            var gcParmBlkp = IntPtr.Zero;

            InsertSipHeader(ref gcParmBlkp, $"Contact: <sip:{_voiceProperties.SipContact}>");
            SetUserInfo(ref gcParmBlkp); // set user info and delete the parameter block

            int result;
            try
            {
                gcParmBlkp = IntPtr.Zero;
                result = gclib_h.gc_util_insert_parm_val(ref gcParmBlkp, gcip_defs_h.IPSET_PROTOCOL,
                    gcip_defs_h.IPPARM_PROTOCOL_BITMASK, sizeof(int), gcip_defs_h.IP_PROTOCOL_SIP);
                result.ThrowIfGlobalCallError();
            }
            catch
            {
                if (gcParmBlkp != IntPtr.Zero) gclib_h.gc_util_delete_parm_blk(gcParmBlkp);
                throw;
            }

            var gclibMakeCallBlk = new GCLIB_MAKECALL_BLK();
            gclibMakeCallBlk.origination.address = from;
            gclibMakeCallBlk.origination.address_type = gclib_h.GCADDRTYPE_TRANSPARENT;
            gclibMakeCallBlk.ext_datap = gcParmBlkp;


            var gclibMkBlkPtr = _unmanagedMemoryServicePerCall.Create(nameof(GC_MAKECALL_BLK), gclibMakeCallBlk);
            try
            {
                var gcMakeCallBlk = new GC_MAKECALL_BLK
                {
                    cclib = IntPtr.Zero,
                    gclib = gclibMkBlkPtr
                };

                SetCodec(gclib_h.GCTGT_GCLIB_CHAN);
                
                TraceCallStateChange(() =>
                {
                    result = gclib_h.gc_MakeCall(_gcDev, ref _callReferenceNumber, to, ref gcMakeCallBlk, 70, DXXXLIB_H.EV_ASYNC);
                    result.ThrowIfGlobalCallError();
                }, "gc_MakeCall");
            }
            finally
            {
                if (gcParmBlkp != IntPtr.Zero) gclib_h.gc_util_delete_parm_blk(gcParmBlkp);
                _unmanagedMemoryServicePerCall.Free(gclibMkBlkPtr);
                DisplayCallState();
            }
        }

        // we were about to make a call but the call state is incorrect. Try and recover from it.
        private void AttemptRecovery(bool makeCall = false)
        {
            var callSateDescription = GetCallState().CallStateDescription();
            _logger.LogDebug("(SIP) - AttemptRecovery() - call State: {callState}", callSateDescription);

            if (_voiceProperties.AttemptRecoveryStartPosition == AttemptRecoveryStartPositions.Disabled)
            {
                _logger.LogDebug("AttemptRecovery disabled");
                return;
            }

            int result;
            
            // first lets try and stop the channel
            TraceCallStateChange(() =>
            {
                result = DXXXLIB_H.dx_stopch(_dxDev, DXXXLIB_H.EV_SYNC);
                result.LogIfStandardRuntimeLibraryError(_dxDev, _logger);
            }, "dx_stopch (from AttemptRecovery)");
            
            if (AttemptRecoveryGeneric(AttemptRecoveryStartPositions.DropCall)) return;
            if (AttemptRecoveryGeneric(AttemptRecoveryStartPositions.ReleaseCall)) return;
            if (AttemptRecoveryResetLineDev()) return;

            var doit = DecideWhen(_voiceProperties.AttemptRecoveryTryReopenOn, makeCall);
            if (doit)
            {
                _logger.LogWarning("Last ditch attempt to recover the line. I am going to dispose and recreate it.");
                try
                {
                    Dispose();
                    Start();
                    return; // I think it worked
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to dispose and restart the line!");
                    throw new RecoveryFailedException(
                        $"AttemptRecovery() - Failed to dispose and restart. This line is now hooped. {ex.Message}");
                }

            }

            // dang, at this point everything has failed!!!
            doit = DecideWhen(_voiceProperties.AttemptRecoveryThrowFailureOn, makeCall);
            if (doit)
                throw new RecoveryFailedException(
                    "AttemptRecovery() - Failed to recover. " +
                    "The line is in an unknown state. Please reset the line or restart the application.");
        }

        private bool DecideWhen(AttemptRecoveryWhen attemptRecoveryWhen, bool makeCall)
        {
            var doit = false;
            switch (_voiceProperties.AttemptRecoveryTryReopenOn)
            {
                case AttemptRecoveryWhen.MakeCall:
                    doit = makeCall; // just for MakeCall
                    break;
                case AttemptRecoveryWhen.All:
                    doit = true; // for everyone
                    break;
            }
            return doit;
        }

        private bool AttemptRecoveryGeneric(AttemptRecoveryStartPositions startOn)
        {
            _logger.LogDebug("(SIP) - AttemptRecovery() - Waiting 5 seconds for GCEV_RELEASECALL event");
            var eventResult = _eventWaiter.WaitForEvent(gclib_h.GCEV_RELEASECALL, 5, new[] { _dxDev, _gcDev }); // wait for the release call event
            if (eventResult == EventWaitEnum.Success)
            {
                _logger.LogDebug("(SIP) - AttemptRecovery() - Finally received the GCEV_RELEASECALL.");
                return true;
            }

            if (_voiceProperties.AttemptRecoveryStartPosition > startOn)
            {
                _logger.LogDebug("skipping {step}", startOn);
                return false;
            }

            // we can't do a drop call without a call reference number
            if (_callReferenceNumber == 0)
            {
                _logger.LogDebug("(SIP) - AttemptRecovery() - No call reference number. Can't drop call or release Call.");
                return false;
            }

            var result = 0;
            TraceCallStateChange(() =>
            {
                switch (startOn)
                {
                    case AttemptRecoveryStartPositions.DropCall:
                        result = gclib_h.gc_DropCall(_callReferenceNumber, gclib_h.GC_NORMAL_CLEARING, DXXXLIB_H.EV_ASYNC);
                        break;
                    case AttemptRecoveryStartPositions.ReleaseCall:
                        result = gclib_h.gc_ReleaseCallEx(_callReferenceNumber, DXXXLIB_H.EV_ASYNC);
                        break;
                    default:
                        throw new Exception($"Unsupported StartOn phase: {startOn}");
                }
                result.LogIfGlobalCallError(_logger);
            }, $"{startOn} (from AttemptRecovery)");

            _logger.LogDebug("(SIP) - AttemptRecovery() - Waiting 10 seconds for GCEV_RELEASECALL event");
            eventResult = _eventWaiter.WaitForEvent(gclib_h.GCEV_RELEASECALL, 10, new[] { _dxDev, _gcDev }); // wait for the release call event
            if (eventResult == EventWaitEnum.Success)
            {
                _logger.LogDebug("(SIP) - AttemptRecovery() - Successfully released the call.");
                return true;
            }
            return false;
        }

        private bool AttemptRecoveryResetLineDev()
        {
            if (_voiceProperties.AttemptRecoveryStartPosition > AttemptRecoveryStartPositions.ResetLineDev)
            {
                _logger.LogDebug("skipping {step}", AttemptRecoveryStartPositions.ResetLineDev);
                return false;
            }

            var result = 0;
            TraceCallStateChange(() =>
            {
                result = gclib_h.gc_ResetLineDev(_gcDev, DXXXLIB_H.EV_ASYNC);
                result.LogIfGlobalCallError(_logger);
            }, "gc_ResetLineDev (from AttemptRecovery)");
            
            if (result < 0)
            {
                _logger.LogError("(SIP) - AttemptRecovery() - Failed to reset the line device.");
                return false;
            }
            _logger.LogDebug("(SIP) - AttemptRecovery() - Waiting 20 seconds for GCEV_RESETLINEDEV event");
            // ignore any other events until after the GCEV_RESETLINEDEV event is received.
            // Global Call API Library Reference page 270
            var releaseCallResult = _eventWaiter.WaitForThisEventOnly(gclib_h.GCEV_RESETLINEDEV, 20, new[] { _dxDev, _gcDev });
            switch (releaseCallResult) {
                case EventWaitEnum.Success:
                    _logger.LogDebug("(SIP) - AttemptRecovery() - Successfully reset the line device.");
                    _dropCallHappening = false;
                    _hangupState = HangupStateEnum.Completed;
                    _callReferenceNumber = 0;
                    _waitCallSet = false;
                    break;
                case EventWaitEnum.Expired:
                    _logger.LogError("(SIP) - AttemptRecovery() - Failed to reset the line device. Timeout waiting for GCEV_RESETLINEDEV");
                    return false;
                case EventWaitEnum.Error:
                    _logger.LogError("(SIP) - AttemptRecovery() - Failed to reset the line device. Error waiting for GCEV_RESETLINEDEV");
                    return false;
            }

            // since we already received this event, this just provides a timeout before the next call
            releaseCallResult = _eventWaiter.WaitForEvent(gclib_h.GCEV_RESETLINEDEV, 5, new[] { _dxDev, _gcDev });
            return _voiceProperties.AttemptRecoveryReturnOnResetLineDevSuccess;
        }

        private void SetUserInfo(ref IntPtr gcParmBlkp)
        {
            try
            {
                var result = gclib_h.gc_SetUserInfo(gclib_h.GCTGT_GCLIB_CHAN, _gcDev, gcParmBlkp, gclib_h.GC_SINGLECALL);
                result.ThrowIfGlobalCallError();
            }
            finally
            {
                if (gcParmBlkp != IntPtr.Zero) gclib_h.gc_util_delete_parm_blk(gcParmBlkp);
            }
        }

        private void InsertSipHeader(ref IntPtr gcParmBlkp, string header)
        {
            _logger.LogDebug("SipHeader is: {0}", header);
            var pSipHeader = _unmanagedMemoryServicePerCall.StringToHGlobalAnsi("pSipHeader", header);
            try
            {
                var result = gclib_h.gc_util_insert_parm_ref_ex(ref gcParmBlkp, gcip_defs_h.IPSET_SIP_MSGINFO, 
                    gcip_defs_h.IPPARM_SIP_HDR, (uint)(header.Length + 1), pSipHeader);
                result.ThrowIfGlobalCallError();
            }
            finally
            {
                _unmanagedMemoryServicePerCall.Free(pSipHeader);
            }
        }

        private void ResetLineDev()
        {
            _logger.LogDebug("(SIP) - ResetLineDev()");

            try
            {
                StopPlayRecordGetDigitsImmediately("ResetLineDev");

                TraceCallStateChange(() =>
                {
                    var result = gclib_h.gc_ResetLineDev(_gcDev, DXXXLIB_H.EV_ASYNC);
                    result.ThrowIfGlobalCallError();
                }, "gc_ResetLineDev");

                // ignore any other events until after the GCEV_RESETLINEDEV event is received.
                // Global Call API Library Reference page 270
                var eventWaitEnum = _eventWaiter.WaitForThisEventOnly(gclib_h.GCEV_RESETLINEDEV, 60, new[] { _dxDev, _gcDev }); // 60 seconds
                switch (eventWaitEnum)
                {
                    case EventWaitEnum.Error:
                        _logger.LogError("(SIP) - Failed to Reset the line. Failed");
                        _dropCallHappening = false; // let the hangup try again.
                        break;
                    case EventWaitEnum.Expired:
                        _dropCallHappening = false; // let the hangup try again.
                        _logger.LogError("(SIP) - Failed to Reset the line. Timeout waiting for GCEV_RESETLINEDEV");
                        break;
                    case EventWaitEnum.Success:
                        _callReferenceNumber = 0;
                        _dropCallHappening = false;
                        _waitCallSet = false;
                        break;
                }
                DisplayCallState();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "(SIP) - Failed to Reset the line");
            }
        }

        private DX_CAP GetCap()
        {
            _logger.LogDebug("GetCap()");
            var dxCap = new DX_CAP();

            var capType = typeof(DX_CAP);

            object boxed = dxCap;

            var caps = _voiceProperties.GetPairPrefixMatch("cap.");
            foreach (var cap in caps)
            {
                var fieldInfo = capType.GetField(cap.Key);
                if (fieldInfo == null)
                {
                    throw new Exception($"cap.{cap.Key} does not exist in DX_CAP");
                }

                var obj = fieldInfo.GetValue(dxCap);
                if (obj is ushort)
                {
                    var value = ushort.Parse(cap.Value);
                    fieldInfo.SetValue(boxed, value);
                }
                else if (obj is short)
                {
                    var value = short.Parse(cap.Value);
                    fieldInfo.SetValue(boxed, value);
                }
                else if (obj is byte)
                {
                    var value = byte.Parse(cap.Value);
                    fieldInfo.SetValue(boxed, value);
                }
            }

            if (_logger.IsEnabled(LogLevel.Debug) && !_capDisplayed)
            {
                _capDisplayed = true;
                var fields = capType.GetFields();
                foreach (var field in fields)
                {
                    _logger.LogDebug($"{field.Name} = {field.GetValue(boxed)}");
                }
            }

            return (DX_CAP)boxed;
        }


        #region ILineManagement region

        void IIvrLineManagement.TriggerDispose()
        {
            _logger.LogDebug("ILineManagement.TriggerDispose() for line: {0}", _lineNumber);

            TraceCallStateChange(() =>
            {
                var result = DXXXLIB_H.dx_stopch(_dxDev, DXXXLIB_H.EV_SYNC);
                result.ThrowIfStandardRuntimeLibraryError(_dxDev);
            }, "dx_stopch (from TriggerDispose)");
            _eventWaiter.DisposeTriggerActivated = true;
        }

        #endregion

        public void Dispose()
        {
            _logger.LogDebug("Dispose() - Disposing of the line");

            try
            {
                var result = gclib_h.gc_Close(_gcDev);
                result.LogIfStandardRuntimeLibraryError(_gcDev, _logger);

                result = DXXXLIB_H.dx_close(_dxDev);
                result.LogIfStandardRuntimeLibraryError(_dxDev, _logger);

            }
            finally
            {
                _waitCallSet = false;
                _inCallProgressAnalysis = false;
                _dropCallHappening = false;
                _capDisplayed = false;
                _callReferenceNumber = 0;


                _unmanagedMemoryServicePerCall?.Dispose();
                _unmanagedMemoryServicePerCall = null;
                _unmanagedMemoryService?.Dispose();
                _unmanagedMemoryService = null;
            }
        }

        public void PlayFile(string filename)
        {
            _logger.LogDebug("PlayFile({0})", filename);
            PlaySipFile(filename, "0123456789#*abcd");
        }


        public void RecordToFile(string filename)
        {
            RecordToFile(filename, 60000 * 5); // default timeout of 5 minutes
        }

        public void RecordToFile(string filename, int timeoutMilliseconds)
        {
            _logger.LogDebug("RecordToFile({0}, {1})", filename, timeoutMilliseconds);
            RecordToFile(filename, "0123456789#*abcd", _currentXpb, timeoutMilliseconds);
        }

        /// <summary>
        /// Record a vox or wav file.
        /// </summary>
        /// <param name="filename">The name of the file to play.</param>
        /// <param name="terminators">Terminator keys</param>
        /// <param name="xpb">The format of the vox or wav file.</param>
        /// <param name="timeoutMilli">Number of milliseconds before timeout</param>
        private void RecordToFile(string filename, string terminators, DX_XPB xpb, int timeoutMilli)
        {
            _logger.LogDebug("RecordToFile({0}, {1}, {2}, {3})", filename, terminators, xpb, timeoutMilli);
            HandlePossibleHangupInProgress();
            DisplayCallState();
            FlushDigitBuffer();

            /* set up DV_TPT */
            var tpt = GetTerminationConditions(1, terminators, timeoutMilli);

            var iott = new DX_IOTT { io_type = DXTABLES_H.IO_DEV | DXTABLES_H.IO_EOT, io_bufp = null, io_offset = 0, io_length = -1 };
            /* set up DX_IOTT */
            if ((iott.io_fhandle = DXXXLIB_H.dx_fileopen(filename, fcntl_h._O_CREAT | fcntl_h._O_BINARY | fcntl_h._O_RDWR, stat_h._S_IWRITE)) == -1)
            {
                var fileErr = DXXXLIB_H.dx_fileerrno();

                var err = "";

                switch (fileErr)
                {
                    case DXTABLES_H.EACCES:
                        err = "Tried to open read-only file for writing, file's sharing mode does not allow specified operations, or given path is directory.";
                        break;
                    case DXTABLES_H.EEXIST:
                        err = "_O_CREAT and _O_EXCL flags specified, but filename already exists.";
                        break;
                    case DXTABLES_H.EINVAL:
                        err = "Invalid oflag or pmode argument.";
                        break;
                    case DXTABLES_H.EMFILE:
                        err = "No more file descriptors available (too many open files).";
                        break;
                    case DXTABLES_H.ENOENT:
                        err = "File or path not found.";
                        break;
                }

                DXXXLIB_H.dx_fileclose(iott.io_fhandle);

                throw new VoiceException(err);
            }

            /* Now record the file */
            if (DXXXLIB_H.dx_reciottdata(_dxDev, ref iott, ref tpt[0], ref xpb, DXXXLIB_H.RM_TONE | DXXXLIB_H.EV_ASYNC) == -1)
            {
                var errPtr = srllib_h.ATDV_ERRMSGP(_dxDev);
                var err = errPtr.IntPtrToString();
                DXXXLIB_H.dx_fileclose(iott.io_fhandle);
                throw new VoiceException(err);
            }

            try
            {
                var waitResult = _eventWaiter.WaitForEvent(DXXXLIB_H.TDX_RECORD, 180, new[] { _dxDev, _gcDev }); // 3 minutes
                HandlePossibleHangupInProgress();
                switch (waitResult)
                {
                    case EventWaitEnum.Success:
                        _logger.LogDebug("The RecordToFile method received the TDX_RECORD event");
                        break;
                    case EventWaitEnum.Expired:
                        var message = "The RecordToFile method timed out waiting for the TDX_RECORD event";
                        _logger.LogError(message);
                        throw new VoiceException(message);
                    case EventWaitEnum.Error:
                        message = "The RecordToFile method failed waiting for the TDX_RECORD event";
                        _logger.LogError(message);
                        throw new VoiceException(message);
                }
            }
            catch (HangupException)
            {
                _logger.LogDebug("(SIP) - Hangup exception caught from RecordToFile");
                DXXXLIB_H.dx_fileclose(iott.io_fhandle);
                _logger.LogDebug(
                    "(SIP) - Hangup Exception : The file handle has been closed because the call has been hung up.");
                throw;
            }
            if (DXXXLIB_H.dx_fileclose(iott.io_fhandle) == -1)
            {
                var errPtr = srllib_h.ATDV_ERRMSGP(_dxDev);
                var err = errPtr.IntPtrToString();
                throw new VoiceException(err);
            }

            var reason = DXXXLIB_H.ATDX_TERMMSK(_dxDev);
            _logger.LogDebug("Type = TDX_RECORD, Reason = {0} = {1}", reason, GetReasonDescription(reason));
            if ((reason & DXTABLES_H.TM_ERROR) == DXTABLES_H.TM_ERROR)
            {
                throw new VoiceException("TM_ERROR");
            }

            if ((reason & DXTABLES_H.TM_USRSTOP) == DXTABLES_H.TM_USRSTOP)
            {
                throw new DisposingException();
            }

            if ((reason & DXTABLES_H.TM_LCOFF) == DXTABLES_H.TM_LCOFF)
            {
                throw new HangupException();
            }

            FlushDigitBuffer();
        }

        public string GetDigits(int numberOfDigits, string terminators)
        {
            _logger.LogDebug("GetDigits({0}, {1})", numberOfDigits, terminators);
            return GetDigits(_dxDev, numberOfDigits, terminators);
        }

        public string FlushDigitBuffer()
        {
            _logger.LogDebug("FlushDigitBuffer()");

            var all = "";
            try
            {
                // add "T" so that I can get all the characters.
                all = GetDigits(_dxDev, DXDIGIT_H.DG_MAXDIGS, "T", 100);
                // strip off timeout terminator if there is once
                if (all.EndsWith("T"))
                {
                    all = all.Substring(0, all.Length - 1);
                }
            }
            catch (GetDigitsTimeoutException)
            {
                // surpress this error
            }
            return all;
        }

        public int Volume
        {
            get => _volume;
            set
            {
                if (value < -10 || value > 10)
                {
                    throw new VoiceException("size must be between -10 to 10");
                }

                var adjsize = (ushort)value;
                var result = DXXXLIB_H.dx_adjsv(_dxDev, DXXXLIB_H.SV_VOLUMETBL, DXXXLIB_H.SV_ABSPOS, adjsize);
                result.ThrowIfStandardRuntimeLibraryError(_dxDev);
                _volume = value;
            }
        }

        public void AddCustomTone(CustomTone tone)
        {
            _logger.LogDebug("AddCustomTone()");

            // Note from Dialogic Voice API:
            // When using this function in a multi-threaded application, use critical sections or a semaphore
            // around the function call to ensure a thread-safe application.Failure to do so will result in “Bad
            // Tone Template ID” errors.

            // and I have had "Bad Tone Template ID" errors.

            lock (_lockObject)
            {
                if (tone.ToneType == CustomToneType.Single)
                {
                    // TODO
                }
                else if (tone.ToneType == CustomToneType.Dual)
                {
                    AddDualTone(_dxDev, tone.Tid, tone.Freq1, tone.Frq1Dev, tone.Freq2, tone.Frq2Dev, tone.Mode);
                }
                else if (tone.ToneType == CustomToneType.DualWithCadence)
                {
                    AddDualToneWithCadence(_dxDev, tone.Tid, tone.Freq1, tone.Frq1Dev, tone.Freq2, tone.Frq2Dev, tone.Ontime,
                        tone.Ontdev, tone.Offtime,
                        tone.Offtdev, tone.Repcnt);
                }

                DisableTone(_dxDev, tone.Tid);
            }
        }

        private void AddDualTone(int devh, int tid, int freq1, int fq1Dev, int freq2, int fq2Dev,
            ToneDetection mode)
        {
            _logger.LogDebug("AddDualTone({0}, {1}, {2}, {3}, {4}, {5}, {6})", devh, tid, freq1, fq1Dev, freq2, fq2Dev,
                mode);
            var dialogicMode = mode == ToneDetection.Leading ? DXXXLIB_H.TN_LEADING : DXXXLIB_H.TN_TRAILING;

            var result = DXXXLIB_H.dx_blddt((uint)tid, (uint)freq1, (uint)fq1Dev, (uint)freq2, (uint)fq2Dev,
                (uint)dialogicMode);
            result.ThrowIfStandardRuntimeLibraryError(devh);

            result = DXXXLIB_H.dx_addtone(devh, 0, 0);
            result.ThrowIfStandardRuntimeLibraryError(devh);
        }
        //T5=480,30,620,40,25,5,25,5,2 fast busy
        //T6=350,20,440,20,L dial tone

        private void AddDualToneWithCadence(int devh, int tid, int freq1, int fq1Dev, int freq2, int fq2Dev,
            int ontime, int ontdev, int offtime, int offtdev, int repcnt)
        {
            _logger.LogDebug("AddDualToneWithCadence({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10})",
                devh, tid, freq1, fq1Dev, freq2, fq2Dev, ontime, ontdev, offtime, offtdev, repcnt);

            var result = DXXXLIB_H.dx_blddtcad((uint)tid, (uint)freq1, (uint)fq1Dev, (uint)freq2, (uint)fq2Dev,
                (uint)ontime,
                (uint)ontdev, (uint)offtime, (uint)offtdev, (uint)repcnt);
            result.ThrowIfStandardRuntimeLibraryError(devh);

            result = DXXXLIB_H.dx_addtone(devh, 0, 0);
            result.ThrowIfStandardRuntimeLibraryError(devh);
        }

        private void DisableTone(int devh, int tid)
        {
            _logger.LogDebug("DisableTone({0}, {1})", devh, tid);

            var result = DXXXLIB_H.dx_distone(devh, tid, DXXXLIB_H.DM_TONEON | DXXXLIB_H.DM_TONEOFF);
            result.ThrowIfStandardRuntimeLibraryError(_dxDev);
        }

        // ReSharper disable once UnusedMember.Local
        private void EnableTone(int devh, int tid)
        {
            _logger.LogDebug("EnableTone({0}, {1})", devh, tid);

            var result = DXXXLIB_H.dx_enbtone(devh, tid, DXXXLIB_H.DM_TONEON | DXXXLIB_H.DM_TONEOFF);
            result.ThrowIfStandardRuntimeLibraryError(_dxDev);
        }

        // ReSharper disable once UnusedMember.Local
        private int ListenForCustomTones(int devh, int timeoutSeconds)
        {
            _logger.LogDebug("ListenForCustomTones({0}, {1})", devh, timeoutSeconds);

            var eblk = new DX_EBLK();
            if (DXXXLIB_H.dx_getevt(devh, ref eblk, timeoutSeconds) == -1)
            {
                if (srllib_h.ATDV_LASTERR(devh) == DXXXLIB_H.EDX_TIMEOUT)
                {
                    return 0;
                }

                var err = srllib_h.ATDV_ERRMSGP(devh);
                var message = err.IntPtrToString();
                throw new VoiceException(message);
            }

            if (eblk.ev_event == DXXXLIB_H.DE_TONEON || eblk.ev_event == DXXXLIB_H.DE_TONEOFF)
            {
                return eblk.ev_data;
            }

            return 0;
        }

        private void MetaEvent(object sender, MetaEventArgs e)
        {
            var eventHandle = e.EventHandle;
            var metaEvt = e.MetaEvent;

            _logger.LogDebug(
                "evt_type = {0}:{1}, evt_dev = {2}, evt_flags = {3},  line_dev = {4} ",
                metaEvt.evttype, metaEvt.evttype.EventTypeDescription(), metaEvt.evtdev, metaEvt.flags,
                metaEvt.linedev);

            if ((metaEvt.flags & gclib_h.GCME_GC_EVENT) == gclib_h.GCME_GC_EVENT)
            {
                HandleGcEvents(metaEvt);
            }
            else
            {
                HandleOtherEvents((uint)eventHandle, metaEvt);
            }
        }

        private void HandleOtherEvents(uint eventHandle, METAEVENT metaEvt)
        {
            var reason = DXXXLIB_H.ATDX_TERMMSK(_dxDev);

            _logger.LogDebug("HandleOtherEvents() - {0}: {1}| {2}: {3}", 
                metaEvt.evttype, 
                metaEvt.evttype.EventTypeDescription(),
                reason, GetReasonDescription(reason));
            switch (metaEvt.evttype)
            {
                case DXXXLIB_H.TDX_CST: // a call status transition event.
                    var ptr = srllib_h.sr_getevtdatap(eventHandle);

                    var dxCst = Marshal.PtrToStructure<DX_CST>(ptr);
                    _logger.LogDebug("Call status transition event = {0}: {1}", dxCst.cst_event, dxCst.cst_event.CstDescription());
                    break;
            }
        }


        private void HandleGcEvents(METAEVENT metaEvt)
        {
            var callState = GetCallState();
            _logger.LogDebug("(SIP) - HandleGcEvents() - {0}: {1} - {2}", metaEvt.evttype, metaEvt.evttype.EventTypeDescription(),
                callState.CallStateDescription());

            if (metaEvt.evttype != gclib_h.GCEV_EXTENSION && metaEvt.evttype != gclib_h.GCEV_EXTENSIONCMPLT)
            {
                // keep track of the state
                _stateProgress?.SetState(metaEvt.evttype, GetCallState());
            }
            
            switch (metaEvt.evttype)
            {
                case gclib_h.GCEV_OPENEX:
                    throw new Exception("Async OpenEx not used anymore and should not be an event for it!");
                case gclib_h.GCEV_OFFERED:
                    var result = gclib_h.gc_GetCRN(ref _callReferenceNumber, ref metaEvt);
                    _logger.LogDebug("crn = {0}", _callReferenceNumber);
                    result.ThrowIfGlobalCallError();

                    AcknowledgeCallAsync();
                    break;
                case gclib_h.GCEV_CALLPROC:
                    AcceptCallAsync();
                    break;
                case gclib_h.GCEV_ACCEPT:
                    AnswerCallAsync();
                    break;

                case gclib_h.GCEV_DISCONNECTED:
                    _hangupState = HangupStateEnum.Starting;
                    StopPlayRecordGetDigitsImmediately("GCEV_DISCONNECTED");
                    
                    DisconnectedEvent();
                    break;
                case gclib_h.GCEV_DROPCALL:
                    _hangupState = HangupStateEnum.Starting;
                    StopPlayRecordGetDigitsImmediately("GCEV_DROPCALL");

                    TraceCallStateChange(() =>
                    {
                        var releaseCallResult = gclib_h.gc_ReleaseCallEx(_callReferenceNumber, DXXXLIB_H.EV_ASYNC);
                        releaseCallResult.LogIfGlobalCallError(_logger);
                    }, "gc_ReleaseCallEx");
                    
                    break;
                case gclib_h.GCEV_RELEASECALL:

                    _callReferenceNumber = 0;
                    _dropCallHappening = false;
                    _hangupState = HangupStateEnum.Completed;

                    StopPlayRecordGetDigitsImmediately("GCEV_RELEASECALL");
                    break;

                case gclib_h.GCEV_EXTENSION:
                    var hangupStarting = _processExtension.HandleExtension(metaEvt);
                    if (hangupStarting)
                    {
                        _hangupState = HangupStateEnum.Starting;
                        StopPlayRecordGetDigitsImmediately("GCEV_EXTENSION");
                    }
                    break;
                case gclib_h.GCEV_TASKFAIL:
                    LogWarningMessage(metaEvt);
                    break;
                case gclib_h.GCEV_ALERTING:
                case gclib_h.GCEV_UNBLOCKED:
                case gclib_h.GCEV_ANSWERED:
                case gclib_h.GCEV_CALLSTATUS:
                case gclib_h.GCEV_CONNECTED:
                case gclib_h.GCEV_RESETLINEDEV:
                case gclib_h.GCEV_EXTENSIONCMPLT:
                case gclib_h.GCEV_SETCONFIGDATA:
                case gclib_h.GCEV_PROCEEDING:
                case gclib_h.GCEV_ATTACH:
                    break;
                default:
                    _logger.LogWarning("NotExpecting event - {0}: {1}", metaEvt.evttype, metaEvt.evttype.EventTypeDescription());
                    break;
            }
        }
        
        private void StopPlayRecordGetDigitsImmediately(string identifier, bool force = false)
        {
            _logger.LogDebug("StopPlayRecordGetDigitsImmediately({identifier}, {force})", identifier, force);
            if (_inCallProgressAnalysis && force == false)
            {
                // we are in call progress analysis and don't want to stop CPA
                _logger.LogDebug("We are in call progress analysis and don't want to stop CPA");
                return;
            }

            TraceCallStateChange(() =>
            {
                var result = DXXXLIB_H.dx_stopch(_dxDev, DXXXLIB_H.EV_SYNC);
                result.ThrowIfStandardRuntimeLibraryError(_dxDev);
            }, $"dx_stopch ({identifier})");

        }

        private void LogWarningMessage(METAEVENT metaEvt)
        {
            var callStatusInfo = new GC_INFO();

            var ptr = _unmanagedMemoryService.Create($"{nameof(GC_INFO)} for LogWarningMessage", callStatusInfo);

            try
            {
                var result = gclib_h.gc_ResultInfo(ref metaEvt, ptr);
                try
                {
                    result.ThrowIfGlobalCallError();

                    callStatusInfo = Marshal.PtrToStructure<GC_INFO>(ptr);

                    var ex = new GlobalCallErrorException(callStatusInfo);
                    _logger.LogWarning($"(SIP) - {ex.Message}");
                }
                catch (GlobalCallErrorException e)
                {
                    // for now we will just log an error if we get one
                    _logger.LogError(e, "(SIP) - Was not expecting this!");
                }

            }
            finally
            {
                _unmanagedMemoryService.Free(ptr);
            }

        }

        // incoming disconnected event
        private void DisconnectedEvent()
        {
            _logger.LogDebug("DisconnectedEvent() - {0}", _callReferenceNumber);

            if (_callReferenceNumber == 0) return; // line is idle

            if (_dropCallHappening)
            {
                _logger.LogDebug("A disconnect is already in progress. Can't hangup twice");
                return;
            }
            _dropCallHappening = true;

            TraceCallStateChange(() =>
            {
                var result = gclib_h.gc_DropCall(_callReferenceNumber, gclib_h.GC_NORMAL_CLEARING, DXXXLIB_H.EV_ASYNC);
                result.ThrowIfGlobalCallError();
            }, "gc_DropCall (from disconnected event)");
        }
        

        /**
        * Acknowlage a call.
        */
        private void AcknowledgeCallAsync()
        {
            _logger.LogDebug("AcknowledgeCallAsync()");

            var gcCallackBlk = new GC_CALLACK_BLK
            {
                type = gclib_h.GCACK_SERVICE_PROC
            };

            TraceCallStateChange(() =>
            {
                var result = gclib_h.gc_CallAck(_callReferenceNumber, ref gcCallackBlk, DXXXLIB_H.EV_ASYNC);
                result.LogIfGlobalCallError(_logger);
            }, "gc_CallAck");
        }

        /**
        * Accept a call.
        */
        private void AcceptCallAsync()
        {
            _logger.LogDebug("AcceptCallAsync()");

            TraceCallStateChange(() =>
            {
                var result = gclib_h.gc_AcceptCall(_callReferenceNumber, 2, DXXXLIB_H.EV_ASYNC);
                result.LogIfGlobalCallError(_logger);
            }, "gc_AcceptCall");
        }

        /**
	    * Answer a call.
	    */
        private void AnswerCallAsync()
        {
            _logger.LogDebug("AnswerCallAsync()");
            SetCodec(gclib_h.GCTGT_GCLIB_CRN);

            TraceCallStateChange(() =>
            {
                var result = gclib_h.gc_AnswerCall(_callReferenceNumber, 2, DXXXLIB_H.EV_ASYNC);
                result.LogIfGlobalCallError(_logger);
            }, "gc_AnswerCall");
        }

        /**
        * set supported codecs.
        */
        private void SetCodec(int crnOrChan)
        {
            _logger.LogDebug("set_codec({0})", crnOrChan);

            var ipCap = new IP_CAPABILITY[3];

            ipCap[0] = new IP_CAPABILITY
            {
                capability = gccfgparm_h.GCCAP_AUDIO_g711Ulaw64k,
                type = gccfgparm_h.GCCAPTYPE_AUDIO,
                direction = gcip_defs_h.IP_CAP_DIR_LCLTRANSMIT,
                payload_type = gcip_defs_h.IP_USE_STANDARD_PAYLOADTYPE,
                extra = new IP_CAPABILITY_UNION
                {
                    audio = new IP_AUDIO_CAPABILITY
                    {
                        frames_per_pkt = 20,
                        VAD = gclib_h.GCPV_DISABLE
                    }
                }
            };

            ipCap[1] = new IP_CAPABILITY
            {
                capability = gccfgparm_h.GCCAP_AUDIO_g711Ulaw64k,
                type = gccfgparm_h.GCCAPTYPE_AUDIO,
                direction = gcip_defs_h.IP_CAP_DIR_LCLRECEIVE,
                payload_type = gcip_defs_h.IP_USE_STANDARD_PAYLOADTYPE,
                extra = new IP_CAPABILITY_UNION
                {
                    audio = new IP_AUDIO_CAPABILITY
                    {
                        frames_per_pkt = 20,
                        VAD = gclib_h.GCPV_DISABLE
                    }
                }
            };

            ipCap[2] = new IP_CAPABILITY
            {
                capability = gccfgparm_h.GCCAP_DATA_t38UDPFax,
                type = gccfgparm_h.GCCAPTYPE_RDATA,
                direction = gcip_defs_h.IP_CAP_DIR_LCLTXRX,
                payload_type = 0,
                extra = new IP_CAPABILITY_UNION
                {
                    data = new IP_DATA_CAPABILITY
                    {
                        max_bit_rate = 144
                    }
                }
            };

            var pointers = new List<IntPtr>();
            int result;
            var parmblkp = IntPtr.Zero;

            try
            {
                for (var i = 0; i < 3; i++)
                {
                    var ipCapPtr = _unmanagedMemoryServicePerCall.Create($"ipCap[{i}]", ipCap[i]);
                    pointers.Add(ipCapPtr);
                    result = gclib_h.gc_util_insert_parm_ref(ref parmblkp, gccfgparm_h.GCSET_CHAN_CAPABILITY,
                        gcip_defs_h.IPPARM_LOCAL_CAPABILITY,
                        (byte)Marshal.SizeOf<IP_CAPABILITY>(), ipCapPtr);
                    result.ThrowIfGlobalCallError();
                }

                if (crnOrChan == gclib_h.GCTGT_GCLIB_CRN)
                {
                    result = gclib_h.gc_SetUserInfo(gclib_h.GCTGT_GCLIB_CRN, _callReferenceNumber, parmblkp,
                        gclib_h.GC_SINGLECALL);
                    result.ThrowIfGlobalCallError();
                }
                else
                {
                    result = gclib_h.gc_SetUserInfo(gclib_h.GCTGT_GCLIB_CHAN, _gcDev, parmblkp, gclib_h.GC_SINGLECALL);
                    result.ThrowIfGlobalCallError();
                }
            }
            finally
            {
                if (parmblkp != IntPtr.Zero) gclib_h.gc_util_delete_parm_blk(parmblkp);
                foreach (var ptr in pointers)
                {
                    _unmanagedMemoryServicePerCall.Free(ptr);
                }
            }

        }

        private DV_TPT[] GetTerminationConditions(int numberOfDigits, string terminators, int timeoutInMilliseconds)
        {
            _logger.LogDebug("GetTerminationConditions({0}, {1}, {2})", numberOfDigits, terminators,
                timeoutInMilliseconds);
            var tpts = new List<DV_TPT>();

            var tpt = new DV_TPT
            {
                tp_type = srltpt_h.IO_CONT,
                tp_termno = DXTABLES_H.DX_MAXDTMF,
                tp_length = (ushort)numberOfDigits,
                tp_flags = DXTABLES_H.TF_MAXDTMF,
                tp_nextp = IntPtr.Zero
            };
            tpts.Add(tpt);

            var bitMask = DefineDigits(terminators);
            if (bitMask != 0)
            {
                tpt = new DV_TPT
                {
                    tp_type = srltpt_h.IO_CONT,
                    tp_termno = DXTABLES_H.DX_DIGMASK,
                    tp_length = (ushort)bitMask,
                    tp_flags = DXTABLES_H.TF_DIGMASK,
                    tp_nextp = IntPtr.Zero
                };
                tpts.Add(tpt);
            }

            if (timeoutInMilliseconds != 0)
            {
                tpt = new DV_TPT
                {
                    tp_type = srltpt_h.IO_CONT,
                    tp_termno = DXTABLES_H.DX_IDDTIME,
                    tp_length = (ushort)(timeoutInMilliseconds / 100),
                    tp_flags = DXTABLES_H.TF_IDDTIME,
                    tp_nextp = IntPtr.Zero
                };
                tpts.Add(tpt);
            }

            tpt = new DV_TPT
            {
                tp_type = srltpt_h.IO_EOT,
                tp_termno = DXTABLES_H.DX_LCOFF,
                tp_length = 3,
                tp_flags = DXTABLES_H.TF_LCOFF | DXTABLES_H.TF_10MS,
                tp_nextp = IntPtr.Zero
            };
            tpts.Add(tpt);

            return tpts.ToArray();
        }

        private int DefineDigits(string digits)
        {
            _logger.LogDebug("DefineDigits({0})", digits);

            var result = 0;

            digits ??= "";

            var all = digits.Trim().ToLower();
            var chars = all.ToCharArray();
            foreach (var c in chars)
            {
                switch (c)
                {
                    case '0':
                        result = result | DXXXLIB_H.DM_0;
                        break;
                    case '1':
                        result = result | DXXXLIB_H.DM_1;
                        break;
                    case '2':
                        result = result | DXXXLIB_H.DM_2;
                        break;
                    case '3':
                        result = result | DXXXLIB_H.DM_3;
                        break;
                    case '4':
                        result = result | DXXXLIB_H.DM_4;
                        break;
                    case '5':
                        result = result | DXXXLIB_H.DM_5;
                        break;
                    case '6':
                        result = result | DXXXLIB_H.DM_6;
                        break;
                    case '7':
                        result = result | DXXXLIB_H.DM_7;
                        break;
                    case '8':
                        result = result | DXXXLIB_H.DM_8;
                        break;
                    case '9':
                        result = result | DXXXLIB_H.DM_9;
                        break;
                    case 'a':
                        result = result | DXXXLIB_H.DM_A;
                        break;
                    case 'b':
                        result = result | DXXXLIB_H.DM_B;
                        break;
                    case 'c':
                        result = result | DXXXLIB_H.DM_C;
                        break;
                    case 'd':
                        result = result | DXXXLIB_H.DM_D;
                        break;
                    case '#':
                        result = result | DXXXLIB_H.DM_P;
                        break;
                    case '*':
                        result = result | DXXXLIB_H.DM_S;
                        break;
                }
            }

            return result;
        }

        private void PlaySipFile(string filename, string terminators)
        {
            _logger.LogDebug("PlaySipFile({0}, {1})", filename, terminators);
            HandlePossibleHangupInProgress();
            DisplayCallState();

            /* set up DV_TPT */
            var tpt = GetTerminationConditions(10, terminators, 0);

            var iott = new DX_IOTT
            { io_type = DXTABLES_H.IO_DEV | DXTABLES_H.IO_EOT, io_bufp = null, io_offset = 0, io_length = -1 };
            /* set up DX_IOTT */
            if ((iott.io_fhandle = DXXXLIB_H.dx_fileopen(filename, fcntl_h._O_RDONLY | fcntl_h._O_BINARY)) == -1)
            {
                var fileErr = DXXXLIB_H.dx_fileerrno();

                var err = "";

                switch (fileErr)
                {
                    case DXTABLES_H.EACCES:
                        err =
                            "Tried to open read-only file for writing, file's sharing mode does not allow specified operations, or given path is directory.";
                        break;
                    case DXTABLES_H.EEXIST:
                        err = "_O_CREAT and _O_EXCL flags specified, but filename already exists.";
                        break;
                    case DXTABLES_H.EINVAL:
                        err = "Invalid oflag or pmode argument.";
                        break;
                    case DXTABLES_H.EMFILE:
                        err = "No more file descriptors available (too many open files).";
                        break;
                    case DXTABLES_H.ENOENT:
                        err = "File or path not found.";
                        break;
                }

                err += " File: |" + filename + "|";

                throw new VoiceException(err);
            }

            var state = DXXXLIB_H.ATDX_STATE(_dxDev);
            _logger.LogDebug("About to play: {0} state: {1}", filename, state);
            if (!File.Exists(filename))
            {
                var err = $"File {filename} does not exist so it cannot be played, call will be droped.";
                _logger.LogError(err);
                throw new VoiceException(err);
            }

            /* Now play the file */
            if (DXXXLIB_H.dx_playiottdata(_dxDev, ref iott, ref tpt[0], ref _currentXpb, DXXXLIB_H.EV_ASYNC) == -1)
            {
                _logger.LogError("Tried to play: {0} state: {1}", filename, state);

                var err = srllib_h.ATDV_ERRMSGP(_dxDev);
                var message = err.IntPtrToString();
                DXXXLIB_H.dx_fileclose(iott.io_fhandle);
                throw new VoiceException(message);
            }

            try
            {
                var waitResult = _eventWaiter.WaitForEvent(DXXXLIB_H.TDX_PLAY, 300, new[] { _dxDev, _gcDev }); // 5 minutes
                HandlePossibleHangupInProgress();
                switch (waitResult)
                {
                    case EventWaitEnum.Success:
                        _logger.LogDebug("The PlaySipFile method received the TDX_PLAY event");
                        break;
                    case EventWaitEnum.Expired:
                        var message = "The PlaySipFile method timed out waiting for the TDX_PLAY event";
                        _logger.LogError(message);
                        throw new VoiceException(message);
                    case EventWaitEnum.Error:
                        message = "The PlaySipFile method failed waiting for the TDX_PLAY event";
                        _logger.LogError(message);
                        throw new VoiceException(message);
                }
            }
            catch (HangupException)
            {
                _logger.LogDebug("(SIP) - Hangup exception caught from PlaySipFile");
                DXXXLIB_H.dx_fileclose(iott.io_fhandle);
                _logger.LogDebug(
                    "(SIP) - Hangup Exception : The file handle has been closed because the call has been hung up.");
                throw;
            }

            // make sure the file is closed
            var result = DXXXLIB_H.dx_fileclose(iott.io_fhandle);
            result.ThrowIfStandardRuntimeLibraryError(_dxDev);

            var reason = DXXXLIB_H.ATDX_TERMMSK(_dxDev);

            _logger.LogDebug("Type = TDX_PLAY, Reason = {0} = {1}", reason, GetReasonDescription(reason));
            if ((reason & DXTABLES_H.TM_ERROR) == DXTABLES_H.TM_ERROR)
            {
                throw new VoiceException("TM_ERROR");
            }

            if ((reason & DXTABLES_H.TM_USRSTOP) == DXTABLES_H.TM_USRSTOP)
            {
                throw new DisposingException();
            }

            if ((reason & DXTABLES_H.TM_LCOFF) == DXTABLES_H.TM_LCOFF)
            {
                throw new HangupException();
            }
        }

        private void HandlePossibleHangupInProgress(bool forceHangup = false, bool ignoreStateCheck = false)
        {
            _logger.LogDebug("HandlePossibleHangup()");

            var callState = GetCallState();
            var channelState = DXXXLIB_H.ATDX_STATE(_dxDev);

            if (_callReferenceNumber == 0 || _hangupState == HangupStateEnum.Completed)
            {
                _logger.LogDebug("(SIP) - call completed the hangup. callState[channelState] = {callState}[{channelState}]",
                    callState.CallStateDescription(),
                    channelState.ChannelStateDescription());
                throw new HangupException();
            }
            if (_hangupState == HangupStateEnum.Starting)
            {
                _logger.LogDebug("(SIP) - Waiting for the hangup to finish. callState[channelState] = {callState}[{channelState}]",
                    callState.CallStateDescription(),
                    channelState.ChannelStateDescription());

                var waitResult = _eventWaiter.WaitForEvent(gclib_h.GCEV_RELEASECALL, 70, new[] { _dxDev, _gcDev }); // 70 seconds                                                                                                                        

                switch (waitResult)
                {
                    case EventWaitEnum.Success:
                        // this should never happen!
                        _logger.LogDebug("(SIP) - The hangup method completed as expected.");
                        break;
                    case EventWaitEnum.Expired:
                        _logger.LogWarning("(SIP) - The hangup method did not receive the releaseCall event");
                        AttemptRecovery();
                        break;
                    case EventWaitEnum.Error:
                        _logger.LogError("(SIP) - The hangup method failed waiting for the releaseCall event");
                        break;
                }
                throw new HangupException();
            }

            // This code that checks to see if the state is connected was recently added, and
            // now I am concerned that the state may not be accurate. It might be better to
            // wait for disconnect events instead of checking the state.
            // therefor I am going to turn this off by default for now.
            if (_voiceProperties.IgnoreCallStateCheck) return;
            
            // Depending on how my test goes, I may delete this block of code
            if (ignoreStateCheck == false && callState != gclib_h.GCST_CONNECTED)
            {
                _logger.LogWarning("(SIP - Line isn't connected. Going to hang up. callState[channelState] = {callState}[{channelState}]",
                    callState.CallStateDescription(), 
                    channelState.ChannelStateDescription());

                // it's possible that we got here because of a waitforevent exipiry. In which case we did not get the TDX_ event.
                StopPlayRecordGetDigitsImmediately("Line isn't connected");
                ClearStopPlayRecordGetDigitEvents(_dxDev, 1000);

                if (forceHangup) Hangup(); // on incoming lines, I want to force a hangup. On outgoing lines, I don't need to because
                                           // a hangup is always done at the end of the call.
                throw new HangupException();
            }
        }

        /*
         * Displays the call state.
         */
        private void DisplayCallState()
        {
            if (_callReferenceNumber == 0)
            {
                _logger.LogDebug("DisplayCallState() - crn = 0");
                return;
            }

            var callState = 0; /* current state of call */
            var result = gclib_h.gc_GetCallState(_callReferenceNumber, ref callState);
            result.ThrowIfGlobalCallError();

            _logger.LogDebug("DisplayCallState() - crn = {0} - {1}", _callReferenceNumber, callState.CallStateDescription());
        }

        private int GetCallState()
        {
            if (_callReferenceNumber == 0) return -1; // indicates that crn = 0;

            var callState = 0; /* current state of call */
            var result = gclib_h.gc_GetCallState(_callReferenceNumber, ref callState);
            result.LogIfGlobalCallError(_logger);
            return callState;
        }

        /*
         * Clears out all events related to stop, play, record and getDigits. Happens right after dx_stopch is called.
         */
        private void ClearStopPlayRecordGetDigitEvents(int devh, int timeoutMilli = 50)
        {
            _logger.LogDebug("ClearStopPlayRecordGetDigitEvents({0}, {1})", devh, timeoutMilli);
            var handler = 0;
            do
            {
                var handles = new[] { _dxDev };
                if (srllib_h.sr_waitevtEx(handles, handles.Length, timeoutMilli, ref handler) == -1)
                {
                    // todo -1 doesn't always mean timeout!
                    _logger.LogTrace("ClearEventBuffer: Timeout");
                    return;
                }

                /*
                 * Get the event
                 */
                var type = srllib_h.sr_getevttype((uint)handler);
                var reason = DXXXLIB_H.ATDX_TERMMSK(devh);
                _logger.LogDebug("ClearEventBuffer: Type = {0} = {1}, Reason = {2} = {3}", type, type.EventTypeDescription(),
                    reason, GetReasonDescription(reason));
            } while (true);
        }

        private string GetReasonDescription(int reason)
        {
            _logger.LogDebug("GetReasonDescription({0})", reason);

            var list = new List<string>();
            if ((reason & DXTABLES_H.TM_NORMTERM) == DXTABLES_H.TM_NORMTERM) list.Add("Normal Termination");
            if ((reason & DXTABLES_H.TM_MAXDTMF) == DXTABLES_H.TM_MAXDTMF) list.Add("Max Number of Digits Recd");
            if ((reason & DXTABLES_H.TM_MAXSIL) == DXTABLES_H.TM_MAXSIL) list.Add("Max Silence");
            if ((reason & DXTABLES_H.TM_MAXNOSIL) == DXTABLES_H.TM_MAXNOSIL) list.Add("Max Non-Silence");
            if ((reason & DXTABLES_H.TM_LCOFF) == DXTABLES_H.TM_LCOFF) list.Add("Loop Current Off");
            if ((reason & DXTABLES_H.TM_IDDTIME) == DXTABLES_H.TM_IDDTIME) list.Add("Inter Digit Delay");
            if ((reason & DXTABLES_H.TM_MAXTIME) == DXTABLES_H.TM_MAXTIME) list.Add("Max Function Time Exceeded");
            if ((reason & DXTABLES_H.TM_DIGIT) == DXTABLES_H.TM_DIGIT) list.Add("Digit Mask or Digit Type Term.");
            if ((reason & DXTABLES_H.TM_PATTERN) == DXTABLES_H.TM_PATTERN) list.Add("Pattern Match Silence Off");
            if ((reason & DXTABLES_H.TM_USRSTOP) == DXTABLES_H.TM_USRSTOP) list.Add("Function Stopped by User");
            if ((reason & DXTABLES_H.TM_EOD) == DXTABLES_H.TM_EOD) list.Add("End of Data Reached on Playback");
            if ((reason & DXTABLES_H.TM_TONE) == DXTABLES_H.TM_TONE) list.Add("Tone On/Off Termination");
            if ((reason & DXTABLES_H.TM_BARGEIN) == DXTABLES_H.TM_BARGEIN) list.Add("Play terminated due to Barge-in");
            if ((reason & DXTABLES_H.TM_ERROR) == DXTABLES_H.TM_ERROR) list.Add("I/O Device Error");
            if ((reason & DXTABLES_H.TM_MAXDATA) == DXTABLES_H.TM_MAXDATA) list.Add("Max Data reached for FSK");
            return string.Join("|", list.ToArray());
        }

        private string GetDigits(int devh, int numberOfDigits, string terminators)
        {
            _logger.LogDebug("GetDigits({0}, {1}, {2})", devh, numberOfDigits, terminators);
            var timeout = _voiceProperties.DigitsTimeoutInMilli;
            return GetDigits(devh, numberOfDigits, terminators, timeout);
        }

        private string GetDigits(int devh, int numberOfDigits, string terminators, int timeout)
        {
            _logger.LogDebug("NumberOfDigits: {0} terminators: {1} timeout: {2}",
                numberOfDigits, terminators, timeout);
            HandlePossibleHangupInProgress();
            DisplayCallState();

            var state = DXXXLIB_H.ATDX_STATE(devh);
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("state: {0}", state.ChannelStateDescription());
            }

            // don't go over the max number of digits
            if (numberOfDigits >= DXDIGIT_H.DG_MAXDIGS) numberOfDigits = DXDIGIT_H.DG_MAXDIGS;

            var tpt = GetTerminationConditions(numberOfDigits, terminators, timeout);

            var digit = new DV_DIGIT();
            var digitPtr = _unmanagedMemoryServicePerCall.Create(nameof(DV_DIGIT), digit);



            // Note: async does not work becaues digit is marshalled out immediately after dx_getdig is complete
            // not when event is found. Would have to use DV_DIGIT* and unsafe code. or another way?
            // var result = dx_getdig(devh, ref tpt[0], out digit, EV_SYNC);
            var result = DXXXLIB_H.dx_getdig(devh, ref tpt[0], digitPtr, DXXXLIB_H.EV_ASYNC);
            if (result == -1)
            {
                var err = srllib_h.ATDV_ERRMSGP(devh);
                var message = err.IntPtrToString();
                _unmanagedMemoryServicePerCall.Free(digitPtr);
                throw new VoiceException(message);
            }

            EventWaitEnum waitResult;
            try
            {
                waitResult = _eventWaiter.WaitForEvent(DXXXLIB_H.TDX_GETDIG, 60, new[] { _dxDev, _gcDev }); // 1 minute
                HandlePossibleHangupInProgress();
            }
            catch (HangupException)
            {
                _logger.LogDebug("(SIP) - Hangup exception caught from GetDigits");
                _unmanagedMemoryServicePerCall.Free(digitPtr);
                throw;
            }
            catch (Exception)
            {
                _unmanagedMemoryServicePerCall.Free(digitPtr);
                throw;
            }
            switch (waitResult)
            {
                case EventWaitEnum.Success:
                    _logger.LogDebug("The GetDigits method received the TDX_GETDIG event");
                    break;
                case EventWaitEnum.Expired:
                    var message = "The GetDigits method timed out waiting for the TDX_GETDIG event";
                    _logger.LogError(message);
                    _unmanagedMemoryServicePerCall.Free(digitPtr);
                    throw new VoiceException(message);
                case EventWaitEnum.Error:
                    message = "The GetDigits method failed waiting for the TDX_GETDIG event";
                    _logger.LogError(message);
                    _unmanagedMemoryServicePerCall.Free(digitPtr);
                    throw new VoiceException(message);
            }

            digit = Marshal.PtrToStructure<DV_DIGIT>(digitPtr);
            _unmanagedMemoryServicePerCall.Free(digitPtr);

            var reason = DXXXLIB_H.ATDX_TERMMSK(devh);
            DisplayCallState();

            _logger.LogDebug("Type = TDX_GETDIG, Reason = {0} = {1}", reason, GetReasonDescription(reason));

            if ((reason & DXTABLES_H.TM_ERROR) == DXTABLES_H.TM_ERROR)
            {
                throw new VoiceException("TM_ERROR");
            }

            if ((reason & DXTABLES_H.TM_USRSTOP) == DXTABLES_H.TM_USRSTOP)
            {
                throw new DisposingException();
            }

            if ((reason & DXTABLES_H.TM_LCOFF) == DXTABLES_H.TM_LCOFF)
            {
                throw new HangupException();
            }

            var answer = digit.dg_value;
            if ((reason & DXTABLES_H.TM_IDDTIME) == DXTABLES_H.TM_IDDTIME)
            {
                if (terminators.IndexOf("t", StringComparison.Ordinal) != -1)
                {
                    answer += 't';
                }
                else
                {
                    throw new GetDigitsTimeoutException();
                }
            }

            return answer;
        }

        private void ClearDigits(int devh)
        {
            _logger.LogDebug("ClearDigits({0})", devh);

            var result = DXXXLIB_H.dx_clrdigbuf(devh);
            result.ThrowIfStandardRuntimeLibraryError(devh);
        }

        public void Reset()
        {
            _logger.LogDebug("Reset()");
            try
            {
                ResetLineDev();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to reset the line");
            }
            Dispose();
            Start();
        }
    }
}