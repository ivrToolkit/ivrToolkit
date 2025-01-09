using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using ivrToolkit.Core.Enums;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Interfaces;
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
        private ProcessExtension _processExtension;

        public IIvrLineManagement Management => this;

        public string LastTerminator { get; set; }

        public int LineNumber => _lineNumber;

        private bool _inCallProgressAnalysis;
        private bool _disconnectionHappening;

        private static readonly object _lockObject = new object();

        public SipLine(ILoggerFactory loggerFactory, DialogicSipVoiceProperties voiceProperties, int lineNumber)
        {
            _voiceProperties = voiceProperties;
            _lineNumber = lineNumber;
            _logger = loggerFactory.CreateLogger<SipLine>();
            _loggerFactory = loggerFactory;
            _logger.LogDebug("ctr(ILoggerFactory, VoiceProperties, {0})", lineNumber);

            _eventWaiter = new EventWaiter(_loggerFactory);
            _eventWaiter.OnMetaEvent += MetaEvent;

            _processExtension = new ProcessExtension(loggerFactory);

            Start();
        }

        private void Start()
        {
            _logger.LogDebug("Start() - Starting line: {0}", _lineNumber);

            _unmanagedMemoryService = new UnmanagedMemoryService(_loggerFactory, $"Lifetime of {nameof(SipLine)}");
            _unmanagedMemoryServicePerCall = new UnmanagedMemoryService(_loggerFactory, "Per Call");

            Open();
            SetDefaultFileType();

            // I don't think anyone uses this with SIP.
            if (_voiceProperties.PreTestDialTone)
            {
                AddCustomTone(_voiceProperties.DialTone); // adds it and then disables it
            }
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
            _logger.LogDebug("WaitRings({0})", rings);

            // a hangup can interupt a TDX_PLAY,TDX_RECORD or TDX_GETDIG. I try and clear the buffer then but the event doesn't always happen in time
            // so this is one more attempt to clear _dxDev events. Not that it really matters because I don't action on those events anyways. 
            ClearEventBuffer(_dxDev, 1000);

            ClearDigits(_dxDev); // make sure we are starting with an empty digit buffer

            _unmanagedMemoryServicePerCall.Dispose(); // there is unlikely anything to free. This is just a fail safe.

            var crnPtr = IntPtr.Zero;


            DisplayCallState();
            int result;

            if (!_waitCallSet)
            {
                // this method only needs to be called once unless the line is reset or closed.
                _waitCallSet = true;
                result = gclib_h.gc_WaitCall(_gcDev, crnPtr, IntPtr.Zero, 0, DXXXLIB_H.EV_ASYNC);
                result.ThrowIfGlobalCallError();
            }

            // asynchronously start waiting for a call to come in
            var eventWaitEnum = _eventWaiter.WaitForEventIndefinitely(gclib_h.GCEV_ANSWERED, new[] { _dxDev, _gcDev });
            switch (eventWaitEnum)
            {
                case EventWaitEnum.Success:
                    _logger.LogDebug("The WaitRings method received the GCEV_ANSWERED event");
                    break;
                case EventWaitEnum.Error:
                    var message = "The WaitRings method failed waiting for the GCEV_ANSWERED event";
                    _logger.LogError(message);
                    throw new VoiceException(message);
            }
        }

        public void Hangup()
        {
            _logger.LogDebug("Hangup(); - crn = {0}", _callReferenceNumber);
            if (_callReferenceNumber == 0) return; // line is not in use

            var result = DXXXLIB_H.dx_stopch(_dxDev, DXXXLIB_H.EV_SYNC);
            try
            {
                result.ThrowIfStandardRuntimeLibraryError(_dxDev);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Hangup() - dx_stopch");
            }

            DisplayCallState();

            if (_disconnectionHappening)
            {
                _logger.LogDebug("A disconnect is already in progress. Can't hangup twice");
                return;
            }
            _disconnectionHappening = true;

            _logger.LogDebug(
                "gclib_h.gc_DropCall(_callReferenceNumber, gclib_h.GC_NORMAL_CLEARING, DXXXLIB_H.EV_ASYNC);");
            result = gclib_h.gc_DropCall(_callReferenceNumber, gclib_h.GC_NORMAL_CLEARING, DXXXLIB_H.EV_ASYNC);
            result.LogIfGlobalCallError(_logger);

            // note:When the GCEV_DROPCALL is caught it automatically calls ReleaseCall and waits for GCEV_RELEASECALL. Thus the call we want to wait for is GCEV_RELEASECALL.
            try
            {
                // okay, now lets wait for the release call event
                var eventWaitEnum = _eventWaiter.WaitForEvent(gclib_h.GCEV_RELEASECALL, 5, new[] { _dxDev, _gcDev }); // Should fire a hangup exception

                switch (eventWaitEnum)
                {
                    case EventWaitEnum.Success:
                        // this should never happen!
                        _logger.LogError("The hangup method received the releaseCall event but it should have immediately fired a hangup exception");
                        break;
                    case EventWaitEnum.Expired:
                        _logger.LogWarning("The hangup method did not receive the releaseCall event");
                        break;
                    case EventWaitEnum.Error:
                        _logger.LogError("The hangup method failed waiting for the releaseCall event");
                        break;
                }
            }
            catch (HangupException)
            {
                _logger.LogDebug("The hangup method completed as expected.");
            }

        }

        public void TakeOffHook()
        {
            /*
             * Sip Does not need to take the received off the hook
             */
        }

        public CallAnalysis Dial(string number, int answeringMachineLengthInMilliseconds)
        {
            _inCallProgressAnalysis = true;
            try
            {
                _logger.LogDebug("Dial({0}, {1})", number, answeringMachineLengthInMilliseconds);
                _unmanagedMemoryServicePerCall.Dispose(); // there is unlikely anything to release, this is just a failsafe.

                // a hangup can interupt a TDX_PLAY,TDX_RECORD or TDX_GETDIG. I try and clear the buffer then but the event doesn't always happen in time
                // so this is one more attempt to clear _dxDev events. Not that it really matters because I don't action on those events anyways. 
                ClearEventBuffer(_dxDev);

                ClearDigits(_dxDev); // make sure we are starting with an empty digit buffer

                var dialToneTid = _voiceProperties.DialTone.Tid;

                var dialToneEnabled = false;

                if (_voiceProperties.PreTestDialTone)
                {
                    _logger.LogDebug("We are pre-testing the dial tone");
                    dialToneEnabled = true;
                    EnableTone(_dxDev, dialToneTid);
                    var tid = ListenForCustomTones(_dxDev, 2);

                    if (tid == 0)
                    {
                        _logger.LogDebug("No tone was detected");
                        DisableTone(_dxDev, dialToneTid);
                        Hangup();
                        return CallAnalysis.NoDialTone;
                    }
                }

                if (dialToneEnabled) DisableTone(_dxDev, dialToneTid);

                _logger.LogDebug("about to dial: {0}", number);
                return DialWithCpa(_dxDev, number, answeringMachineLengthInMilliseconds);

            }
            finally
            {
                _inCallProgressAnalysis = false;
            }

        }

        /// <summary>
        /// Dials a phone number using call progress analysis.
        /// </summary>
        /// <param name="devh">The handle for the Dialogic line.</param>
        /// <param name="number">The phone number to dial.</param>
        /// <param name="answeringMachineLengthInMilliseconds">Answering machine length in milliseconds</param>
        /// <returns>CallAnalysis Enum</returns>
        private CallAnalysis DialWithCpa(int devh, string number, int answeringMachineLengthInMilliseconds)
        {
            _logger.LogDebug("DialWithCpa({0}, {1}, {2})", devh, number, answeringMachineLengthInMilliseconds);

            var cap = GetCap();

            var ani = $"{_voiceProperties.SipAlias}@{_voiceProperties.SipProxyIp}"; // automatic number identification (from)
            var dnis = $"{number}@{_voiceProperties.SipProxyIp}"; // dialed number identification service (to)

            MakeCall(ani, dnis);

            // check the CPA
            var startTime = DateTimeOffset.Now;

            var result = DXXXLIB_H.dx_dial(devh, "", ref cap, DXCALLP_H.DX_CALLP | DXXXLIB_H.EV_ASYNC);
            result.ThrowIfStandardRuntimeLibraryError(devh);
            DisplayCallState();

            var eventWaitEnum = _eventWaiter.WaitForEvent(DXXXLIB_H.TDX_CALLP, 60, new[] { _dxDev, _gcDev }); // 60 seconds
            switch (eventWaitEnum)
            {
                case EventWaitEnum.Success:
                    _logger.LogDebug("Check CPA duration = {0} seconds. Received TDX_CALLP", (DateTimeOffset.Now - startTime).TotalSeconds);
                    break;
                case EventWaitEnum.Expired:
                    var message = $"Check CPA duration = {(DateTimeOffset.Now - startTime).TotalSeconds} seconds. Timed out waiting for TDX_CALLP";
                    _logger.LogError(message);
                    ResetLineDev();
                    return CallAnalysis.Error;
                case EventWaitEnum.Error:
                    message = $"Check CPA duration = {(DateTimeOffset.Now - startTime).TotalSeconds} seconds. Failed waiting for TDX_CALLP";
                    _logger.LogError(message);
                    ResetLineDev();
                    return CallAnalysis.Error;
            }
            DisplayCallState();

            // get the CPA result
            var callProgressResult = DXXXLIB_H.ATDX_CPTERM(devh);

            _logger.LogDebug("Call Progress Analysis Result {0}:{1}", callProgressResult, callProgressResult.CallProgressDescription());
            switch (callProgressResult)
            {
                case DXCALLP_H.CR_BUSY:
                    return CallAnalysis.Busy;
                case DXCALLP_H.CR_CEPT:
                    return CallAnalysis.OperatorIntercept;
                case DXCALLP_H.CR_CNCT:
                    var callState = GetCallState();
                    if (callState != gclib_h.GCST_CONNECTED)
                    {
                        // i've seen cpa say "connected" but the call state was stuck on "alerting"
                        // note, this may have been because I didn't start cpa until recieving the alerting event.
                        //       I now start cpa immediately. Note: Old cpp SIP version did this too.
                        _logger.LogWarning("Expected CONNECTED state but we are in {0}", callState.CallStateDescription());
                        ResetLineDev();
                        // the old cpp SIP version never used to check state here but it would catch it on playfile or getdigits and
                        // hangup, so I now hangup to be the same as the old version. Ultimately, I would like to get confirmation
                        // from dialogic as to why this state happens.
                        throw new HangupException();
                    }
                    var connType = DXXXLIB_H.ATDX_CONNTYPE(devh);
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
                            return CallAnalysis.AnsweringMachine;
                        case DXCALLP_H.CON_PVD:
                            _logger.LogDebug("Connection due to Positive Voice Detection");
                            break;
                    }

                    // TODO this code doesn't work with SIP
                    var len = GetSalutationLength(devh);
                    _logger.LogDebug("Salutation length is: {0}", len);
                    if (len > answeringMachineLengthInMilliseconds)
                    {
                        return CallAnalysis.AnsweringMachine;
                    }

                    return CallAnalysis.Connected;
                case DXCALLP_H.CR_ERROR:
                    ResetLineDev();
                    return CallAnalysis.Error;
                case DXCALLP_H.CR_FAXTONE:
                    return CallAnalysis.FaxTone;
                case DXCALLP_H.CR_NOANS:
                    return CallAnalysis.NoAnswer;
                case DXCALLP_H.CR_NODIALTONE:
                    ResetLineDev();
                    return CallAnalysis.NoDialTone;
                case DXCALLP_H.CR_NORB:
                    ResetLineDev();
                    return CallAnalysis.NoRingback;
                case DXCALLP_H.CR_STOPD:
                    // calling method will check and throw the stopException
                    return CallAnalysis.Stopped;
            }

            throw new VoiceException("Unknown dail response: " + callProgressResult);
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
            var startTime = DateTimeOffset.Now;
            _logger.LogDebug("MakeCall({0}, {1})", from, to);
            DisplayCallState();

            var gcParmBlkp = IntPtr.Zero;

            InsertSipHeader(ref gcParmBlkp, $"Contact: <sip:{_voiceProperties.SipContact}>");
            SetUserInfo(ref gcParmBlkp); // set user info and delete the parameter block

            var result = 0;
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

                result = gclib_h.gc_MakeCall(_gcDev, ref _callReferenceNumber, to, ref gcMakeCallBlk, 30, DXXXLIB_H.EV_ASYNC);
                result.ThrowIfGlobalCallError();
            }
            finally
            {
                if (gcParmBlkp != IntPtr.Zero) gclib_h.gc_util_delete_parm_blk(gcParmBlkp);
                _unmanagedMemoryServicePerCall.Free(gclibMkBlkPtr);
                DisplayCallState();
            }
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
            _logger.LogDebug("ResetLineDev()");
            try
            {
                if (!_inCallProgressAnalysis)
                {
                    var result2 = DXXXLIB_H.dx_stopch(_dxDev, DXXXLIB_H.EV_SYNC);
                    result2.LogIfStandardRuntimeLibraryError(_dxDev, _logger);
                }

                var result = gclib_h.gc_ResetLineDev(_gcDev, DXXXLIB_H.EV_ASYNC);
                result.ThrowIfGlobalCallError();

                var eventWaitEnum = _eventWaiter.WaitForThisEventOnly(gclib_h.GCEV_RESETLINEDEV, 60, new[] { _dxDev, _gcDev }); // 60 seconds
                switch (eventWaitEnum)
                {
                    case EventWaitEnum.Error:
                        _logger.LogError("Failed to Reset the line. Failed");
                        break;
                    case EventWaitEnum.Expired:
                        _logger.LogError("Failed to Reset the line. Timeout waiting for GCEV_RESETLINEDEV");
                        break;
                    case EventWaitEnum.Success:
                        _callReferenceNumber = 0;
                        _disconnectionHappening = false;
                        _waitCallSet = false;
                        break;
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to Reset the line");
            }
        }

        /// <summary>
        /// Gets the greeting time in milliseconds.
        /// </summary>
        /// <param name="devh">The handle for the Dialogic line.</param>
        /// <returns>The greeting time in milliseconds.</returns>
        private static int GetSalutationLength(int devh)
        {
            var result = DXXXLIB_H.ATDX_ANSRSIZ(devh);
            result.ThrowIfStandardRuntimeLibraryError(devh);

            return result * 10;
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

            var result = DXXXLIB_H.dx_stopch(_dxDev, DXXXLIB_H.EV_SYNC);
            result.ThrowIfStandardRuntimeLibraryError(_dxDev);
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
                _disconnectionHappening = false;
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
                ClearEventBuffer(_dxDev, 2000); // Did not get the TDX_RECORD event so clear the buffer so it isn't captured later
                DXXXLIB_H.dx_fileclose(iott.io_fhandle);
                _logger.LogDebug(
                    "Hangup Exception : The file handle has been closed because the call has been hung up.");
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
            _logger.LogDebug("HandleOtherEvents() - {0}: {1}", metaEvt.evttype, metaEvt.evttype.EventTypeDescription());
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
            _logger.LogDebug("HandleGcEvents() - {0}: {1}", metaEvt.evttype, metaEvt.evttype.EventTypeDescription());
            switch (metaEvt.evttype)
            {
                case gclib_h.GCEV_ALERTING:
                    _logger.LogDebug("GCEV_ALERTING - handled by call to WaitForEvent");
                    break;
                case gclib_h.GCEV_OPENEX:
                    _logger.LogDebug("GCEV_OPENEX - This no longer does anything and should never get here!");
                    throw new Exception("Async OpenEx not used anymore and should not be an event for it!");
                case gclib_h.GCEV_UNBLOCKED:
                    _logger.LogDebug("GCEV_UNBLOCKED - we do nothing with this event");
                    break;
                case gclib_h.GCEV_OFFERED:
                    _logger.LogDebug("GCEV_OFFERED");

                    var result = gclib_h.gc_GetCRN(ref _callReferenceNumber, ref metaEvt);
                    _logger.LogDebug("crn = {0}", _callReferenceNumber);
                    result.ThrowIfGlobalCallError();

                    AcknowledgeCallAsync();
                    break;
                case gclib_h.GCEV_CALLPROC:
                    _logger.LogDebug("GCEV_CALLPROC");
                    AcceptCallAsync();
                    break;
                case gclib_h.GCEV_ACCEPT:
                    _logger.LogDebug("GCEV_ACCEPT");
                    AnswerCallAsync();
                    break;
                case gclib_h.GCEV_ANSWERED:
                    _logger.LogDebug("GCEV_ANSWERED - handled by call to WaitForEvent");
                    break;
                case gclib_h.GCEV_CALLSTATUS:
                    _logger.LogDebug("GCEV_CALLSTATUS - we do nothing with this event");
                    break;
                case gclib_h.GCEV_CONNECTED:
                    _logger.LogDebug("GCEV_CONNECTED - we do nothing with this event");
                    break;
                case gclib_h.GCEV_RESETLINEDEV:
                    _logger.LogDebug("GCEV_RESETLINEDEV - we do nothing with this event");
                    break;

                #region Handle hangup detection
                case gclib_h.GCEV_DISCONNECTED:
                    _logger.LogDebug("GCEV_DISCONNECTED");
                    DisconnectedEvent(); // will block for gcev_dropcall
                    break;
                case gclib_h.GCEV_DROPCALL:
                    _logger.LogDebug("GCEV_DROPCALL");
                    ReleaseCall(); // will block for gcev_releasecall
                    break;
                case gclib_h.GCEV_RELEASECALL:
                    _logger.LogDebug("GCEV_RELEASECALL - set crn = 0");
                    _callReferenceNumber = 0;
                    _disconnectionHappening = false;

                    // need to let call analysis finish
                    if (!_inCallProgressAnalysis) throw new HangupException();

                    break;
                #endregion

                case gclib_h.GCEV_EXTENSIONCMPLT:
                    // i've never received this event before
                    _logger.LogDebug("GCEV_EXTENSIONCMPLT - we do nothing with this event");
                    break;
                case gclib_h.GCEV_EXTENSION:
                    _logger.LogDebug("GCEV_EXTENSION");
                    _processExtension.HandleExtension(metaEvt);
                    break;
                case gclib_h.GCEV_SETCONFIGDATA:
                    _logger.LogDebug("GCEV_SETCONFIGDATA - handled by call to WaitForEvent");
                    break;
                case gclib_h.GCEV_PROCEEDING:
                    _logger.LogDebug("GCEV_PROCEEDING - we do nothing with this event");
                    break;
                case gclib_h.GCEV_TASKFAIL:
                    _logger.LogWarning("GCEV_TASKFAIL");
                    LogWarningMessage(metaEvt);
                    ResetLineDev();
                    // todo should I move the line reset here and handle
                    throw new TaskFailException();
                case gclib_h.GCEV_ATTACH:
                    _logger.LogDebug("GCEV_ATTACH - we do nothing with this event");
                    break;
                default:
                    _logger.LogWarning("NotExpecting event - {0}: {1}", metaEvt.evttype, metaEvt.evttype.EventTypeDescription());
                    break;
            }
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
                    _logger.LogWarning(ex.Message);
                }
                catch (GlobalCallErrorException e)
                {
                    // for now we will just log an error if we get one
                    _logger.LogError(e, "Was not expecting this!");
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

            if (_disconnectionHappening)
            {
                _logger.LogDebug("A disconnect is already in progress. Can't hangup twice");
                return;
            }
            _disconnectionHappening = true;

            DisplayCallState();

            // we don't want to stop call progress analysis
            if (!_inCallProgressAnalysis)
            {
                var stopResult = DXXXLIB_H.dx_stopch(_dxDev, DXXXLIB_H.EV_SYNC);
                stopResult.LogIfStandardRuntimeLibraryError(_dxDev, _logger);
            }

            var result = gclib_h.gc_DropCall(_callReferenceNumber, gclib_h.GC_NORMAL_CLEARING, DXXXLIB_H.EV_ASYNC);
            result.ThrowIfGlobalCallError();

            var eventWaitEnum = _eventWaiter.WaitForEvent(gclib_h.GCEV_DROPCALL, 10, new[] { _dxDev, _gcDev }); // 10 seconds
            _logger.LogDebug("_eventWaiter.WaitForEvent(gclib_h.GCEV_DROPCALL, 10, _dxDev, _gcDev ) = {0}", eventWaitEnum);
        }

        /**
        * Release a call.
        */
        private void ReleaseCall()
        {
            _logger.LogDebug("ReleaseCall()");
            var result = gclib_h.gc_ReleaseCallEx(_callReferenceNumber, DXXXLIB_H.EV_ASYNC);
            result.ThrowIfGlobalCallError();

            var eventWaitEnum = _eventWaiter.WaitForEvent(gclib_h.GCEV_RELEASECALL, 10, new[] { _dxDev, _gcDev }); // 10 seconds
            _logger.LogDebug("_eventWaiter.WaitForEvent(gclib_h.GCEV_RELEASECALL, 10, _dxDev, _gcDev ) = {0}", eventWaitEnum);
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
            var result = gclib_h.gc_CallAck(_callReferenceNumber, ref gcCallackBlk, DXXXLIB_H.EV_ASYNC);
            result.ThrowIfGlobalCallError();
        }

        /**
        * Accept a call.
        */
        private void AcceptCallAsync()
        {
            _logger.LogDebug("AcceptCallAsync()");
            var result = gclib_h.gc_AcceptCall(_callReferenceNumber, 2, DXXXLIB_H.EV_ASYNC);
            result.ThrowIfGlobalCallError();
        }

        /**
	    * Answer a call.
	    */
        private void AnswerCallAsync()
        {
            _logger.LogDebug("AnswerCallAsync()");
            SetCodec(gclib_h.GCTGT_GCLIB_CRN);
            var result = gclib_h.gc_AnswerCall(_callReferenceNumber, 2, DXXXLIB_H.EV_ASYNC);
            result.ThrowIfGlobalCallError();
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
                var waitResult = _eventWaiter.WaitForEvent(DXXXLIB_H.TDX_PLAY, 60, new[] { _dxDev, _gcDev }); // 1 minute
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
                ClearEventBuffer(_dxDev, 2000); // Did not get the TDX_PLAY event so clear the buffer so it isn't captured later
                DXXXLIB_H.dx_fileclose(iott.io_fhandle);
                _logger.LogDebug(
                    "Hangup Exception : The file handle has been closed because the call has been hung up.");
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

        /*
         * Displays the call state.
         */
        private void DisplayCallState()
        {
            _logger.LogDebug("DisplayCallState() - crn = {0}", _callReferenceNumber);
            if (_callReferenceNumber == 0) return;

            var callState = GetCallState();
            _logger.LogDebug("DisplayCallState: Call State {0}", callState.CallStateDescription());
        }

        private int GetCallState()
        {
            _logger.LogDebug("GetCallState() - crn = {0}", _callReferenceNumber);
            if (_callReferenceNumber == 0) return 0;

            var callState = 0; /* current state of call */
            var result = gclib_h.gc_GetCallState(_callReferenceNumber, ref callState);
            result.ThrowIfGlobalCallError();
            return callState;
        }

        /*
         * Clear Out the Event Buffer by consuming all the events until the timeout is thrown to indicate that
         * no events are left in the event buffer.
         * 
         * This is the only way I found to reliably clear out the event buffer.  
         * This consumes events for the device until I receive an event timout.  
         * This ensures that no events are left in the buffer before I need to consume 
         * an event in another method (syncronously or asyncronously).
         */
        private void ClearEventBuffer(int devh, int timeoutMilli = 50)
        {
            _logger.LogDebug("ClearEventBuffer({0}, {1})", devh, timeoutMilli);
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
            //var result = dx_getdig(devh, ref tpt[0], out digit, EV_SYNC);
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
            }
            catch (HangupException)
            {
                ClearEventBuffer(_dxDev, 2000); // Did not get the TDX_GETDIG event so clear the buffer so it isn't captured later
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