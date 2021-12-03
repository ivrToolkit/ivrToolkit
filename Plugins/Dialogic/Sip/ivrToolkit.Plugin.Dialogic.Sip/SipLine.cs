using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using ivrToolkit.Core.Enums;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Core.Util;
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
        private const int SYNC_WAIT_INFINITE = -1;
        private const int SYNC_WAIT_EXPIRED = -2;
        private const int SYNC_WAIT_ERROR = -1;
        private const int SYNC_WAIT_SUCCESS = 1;
        private readonly int _lineNumber;
        private readonly ILogger<SipLine> _logger;

        // if I need to keep unmanaged memory in scope for the duration of this class
        private UnmanagedMemoryService _unmanagedMemoryService;

        // If I need to keep unmanaged memory in scope for the duration of the call
        private UnmanagedMemoryService _unmanagedMemoryServicePerCall;

        private readonly DialogicSipVoiceProperties _voiceProperties;

        private static object _lockObject = new object();
        private static int _boardDev; // for board device = ":N_iptB1:P_IP"

        private int _callReferenceNumber;
        private DX_XPB _currentXpb;
        private int _devh; // for device name = "dxxxB{boardId}C{channelId}"

        private int
            _gcDev; // for device name = ":P_SIP:N_iptB1T{_lineNumber}:M_ipmB1C{id}:V_dxxxB{boardId}C{channelId}"

        private int _ipmDev;
        private IntPtr _ipXslot;

        private int _volume;
        private IntPtr _voxXslot;
        private bool _waitCallSet;
        private readonly ILoggerFactory _loggerFactory;
        private bool _disposeTriggerActivated;
        private bool _capDisplayed;

        public SipLine(ILoggerFactory loggerFactory, DialogicSipVoiceProperties voiceProperties, int lineNumber)
        {
            _voiceProperties = voiceProperties;
            _lineNumber = lineNumber;
            _logger = loggerFactory.CreateLogger<SipLine>();
            _loggerFactory = loggerFactory;
            _logger.LogDebug("ctr(ILoggerFactory, VoiceProperties, {0})", lineNumber);

            Start();
        }

        private void Start()
        {
            _logger.LogDebug("Start() - Starting line: {0}", _lineNumber);

            _unmanagedMemoryService = new UnmanagedMemoryService(_loggerFactory, $"Lifetime of {nameof(SipLine)}");
            _unmanagedMemoryServicePerCall = new UnmanagedMemoryService(_loggerFactory, "Per Call");

            lock (_lockObject)
            {
                Open();
                // See my comments in the method. the old c code never worked either
                Register();
                SetDefaultFileType();
                DeleteCustomTones(); // uses dx_deltones() so I have to readd call progress tones. I also readd special tones
            }
        }

        public IIvrLineManagement Management => this;


        public string LastTerminator { get; set; }

        public int LineNumber => _lineNumber;

        public void WaitRings(int rings)
        {
            _logger.LogDebug("WaitRings({0})", rings);

            _unmanagedMemoryServicePerCall.Dispose();

            var crnPtr = IntPtr.Zero;


            CheckCallState();
            int result;

            if (!_waitCallSet)
            {
                // this method only needs to be called once unless the line is reset or closed.
                _waitCallSet = true;
                result = gclib_h.gc_WaitCall(_gcDev, crnPtr, IntPtr.Zero, 0, DXXXLIB_H.EV_ASYNC);
                result.ThrowIfGlobalCallError();
            }

            // asynchronously start waiting for a call to come in
            result = WaitForEventIndefinitely(gclib_h.GCEV_ANSWERED);
            switch (result)
            {
                case SYNC_WAIT_SUCCESS:
                    _logger.LogDebug("The WaitRings method received the GCEV_ANSWERED event");
                    break;
                case SYNC_WAIT_ERROR:
                    var message = "The WaitRings method failed waiting for the GCEV_ANSWERED event";
                    _logger.LogError(message);
                    throw new VoiceException(message);
            }

            if (result == SYNC_WAIT_ERROR)
            {
                throw new VoiceException("WaitRings threw an exception");
            }
        }

        public void Hangup()
        {
            _logger.LogDebug("Hangup(); - crn = {0}", _callReferenceNumber);

            var result = DXXXLIB_H.dx_stopch(_devh, DXXXLIB_H.EV_SYNC);
            result.ThrowIfStandardRuntimeLibraryError(_devh);

            if (_callReferenceNumber == 0) return; // line is not in use

            _logger.LogDebug(
                "gclib_h.gc_DropCall(_callReferenceNumber, gclib_h.GC_NORMAL_CLEARING, DXXXLIB_H.EV_ASYNC);");
            result = gclib_h.gc_DropCall(_callReferenceNumber, gclib_h.GC_NORMAL_CLEARING, DXXXLIB_H.EV_ASYNC);
            try
            {
                result.ThrowIfGlobalCallError();
            }
            catch (GlobalCallErrorException e)
            {
                // for now I will let this go while I find out more about the proper way to hangup and drop.
                _logger.LogWarning(e, null);
            }

            result = WaitForEvent(gclib_h.GCEV_DROPCALL, 5); // 5 second wait

            switch (result)
            {
                case SYNC_WAIT_SUCCESS:
                    _logger.LogDebug("The hangup method received the dropcall event");
                    break;
                case SYNC_WAIT_EXPIRED:
                    _logger.LogWarning("The hangup method did not receive the dropcall event");
                    break;
                case SYNC_WAIT_ERROR:
                    _logger.LogError("The hangup method failed waiting for the dropcall event");
                    break;
            }

            try
            {
                // okay, now lets wait for the release call event
                result = WaitForEvent(gclib_h.GCEV_RELEASECALL, 5); // Should fire a hangup exception

                switch (result)
                {
                    case SYNC_WAIT_SUCCESS:
                        // this should never happen!
                        _logger.LogError("The hangup method received the releaseCall event but it should have immediately fired a hangup exception");
                        break;
                    case SYNC_WAIT_EXPIRED:
                        _logger.LogWarning("The hangup method did not receive the releaseCall event");
                        break;
                    case SYNC_WAIT_ERROR:
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
            _logger.LogDebug("Dial({0}, {1})", number, answeringMachineLengthInMilliseconds);
            _unmanagedMemoryServicePerCall.Dispose();

            var dialToneTid = _voiceProperties.DialTone.Tid;

            var dialToneEnabled = false;

            if (_voiceProperties.PreTestDialTone)
            {
                _logger.LogDebug("We are pre-testing the dial tone");
                dialToneEnabled = true;
                EnableTone(_devh, dialToneTid);
                var tid = ListenForCustomTones(_devh, 2);

                if (tid == 0)
                {
                    _logger.LogDebug("No tone was detected");
                    DisableTone(_devh, dialToneTid);
                    Hangup();
                    return CallAnalysis.NoDialTone;
                }
            }

            if (dialToneEnabled) DisableTone(_devh, dialToneTid);

            _logger.LogDebug("about to dial: {0}", number);
            return DialWithCpa(_devh, number, answeringMachineLengthInMilliseconds);
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

            var ani = _voiceProperties.SipAlias + "@" + _voiceProperties.SipProxyIp;
            var dnis = number + "@" + _voiceProperties.SipProxyIp;

            SetAuthenticationInfo();
            MakeCall(ani, dnis);

            _logger.LogDebug("DialWithCpa: Syncronous Make call Completed starting call process analysis");

            var result = DXXXLIB_H.dx_dial(devh, "", ref cap, DXCALLP_H.DX_CALLP | DXXXLIB_H.EV_ASYNC);
            result.ThrowIfStandardRuntimeLibraryError(devh);

            result = WaitForEvent(DXXXLIB_H.TDX_CALLP, 60); // 60 seconds
            switch (result)
            {
                case SYNC_WAIT_SUCCESS:
                    _logger.LogDebug("The DialWithCpa method received the TDX_CALLP event");
                    break;
                case SYNC_WAIT_EXPIRED:
                    var message = "The DialWithCpa method timed out waiting for the TDX_CALLP event";
                    _logger.LogError(message);
                    throw new VoiceException(message);
                case SYNC_WAIT_ERROR:
                    message = "The DialWithCpa method failed waiting for the TDX_CALLP event";
                    _logger.LogError(message);
                    throw new VoiceException(message);
            }

            result = DXXXLIB_H.ATDX_CPTERM(devh);

            _logger.LogDebug("Call Progress Analysius Result {0}", result);
            switch (result)
            {
                case DXCALLP_H.CR_BUSY:
                    return CallAnalysis.Busy;
                case DXCALLP_H.CR_CEPT:
                    return CallAnalysis.OperatorIntercept;
                case DXCALLP_H.CR_CNCT:
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
                    return CallAnalysis.Error;
                case DXCALLP_H.CR_FAXTONE:
                    return CallAnalysis.FaxTone;
                case DXCALLP_H.CR_NOANS:
                    return CallAnalysis.NoAnswer;
                case DXCALLP_H.CR_NODIALTONE:
                    return CallAnalysis.NoDialTone;
                case DXCALLP_H.CR_NORB:
                    return CallAnalysis.NoRingback;
                case DXCALLP_H.CR_STOPD:
                    // calling method will check and throw the stopException
                    return CallAnalysis.Stopped;
            }

            throw new VoiceException("Unknown dail response: " + result);
        }

        /**
        * Make a call.
        * Please note,
        * The call header sets the USER_DISPLAY.
        * The USER_DISPLAY is blocekd by carriers (Fido, Telus, etc.)
        * The USER_DISPLAY can also be set using the PBX.
        */
        private void MakeCall(string ani, string dnis)
        {
            _logger.LogDebug("MakeCall({0}, {1})", ani, dnis);

            var gcParmBlkp = IntPtr.Zero;
            var sipHeader = $"User-Agent: {_voiceProperties.SipUserAgent}";
            _logger.LogDebug("SipHeader is: {0}", sipHeader);

            var dataSize = (uint)(sipHeader.Length + 1);
            var pSipHeader1 = _unmanagedMemoryServicePerCall.StringToHGlobalAnsi("pSipHeader1", sipHeader);
            var result = gclib_h.gc_util_insert_parm_ref_ex(ref gcParmBlkp, gcip_defs_h.IPSET_SIP_MSGINFO,
                gcip_defs_h.IPPARM_SIP_HDR, dataSize, pSipHeader1);
            result.ThrowIfGlobalCallError();

            sipHeader = $"From: {_voiceProperties.SipFrom}<sip:{ani}>"; //From header
            _logger.LogDebug("SipHeader is: {0}", sipHeader);
            dataSize = (uint)(sipHeader.Length + 1);
            var pSipHeader2 = _unmanagedMemoryServicePerCall.StringToHGlobalAnsi("pSipHeader2", sipHeader);
            result = gclib_h.gc_util_insert_parm_ref_ex(ref gcParmBlkp, gcip_defs_h.IPSET_SIP_MSGINFO,
                gcip_defs_h.IPPARM_SIP_HDR, dataSize, pSipHeader2);
            result.ThrowIfGlobalCallError();

            sipHeader = $"Contact: {_voiceProperties.SipConctact}<sip:{ani}:{_voiceProperties.SipSignalingPort}>"; //Contact header
            _logger.LogDebug("SipHeader is: {0}", sipHeader);
            dataSize = (uint)(sipHeader.Length + 1);
            var pSipHeader3 = _unmanagedMemoryServicePerCall.StringToHGlobalAnsi("pSipHeader3", sipHeader);
            result = gclib_h.gc_util_insert_parm_ref_ex(ref gcParmBlkp, gcip_defs_h.IPSET_SIP_MSGINFO,
                gcip_defs_h.IPPARM_SIP_HDR, dataSize, pSipHeader3);
            result.ThrowIfGlobalCallError();


            result = gclib_h.gc_SetUserInfo(gclib_h.GCTGT_GCLIB_CHAN, _gcDev, gcParmBlkp, gclib_h.GC_SINGLECALL);
            result.ThrowIfGlobalCallError();
            gclib_h.gc_util_delete_parm_blk(gcParmBlkp);

            _unmanagedMemoryServicePerCall.Free(pSipHeader1);
            _unmanagedMemoryServicePerCall.Free(pSipHeader2);
            _unmanagedMemoryServicePerCall.Free(pSipHeader3);

            gcParmBlkp = IntPtr.Zero;
            result = gclib_h.gc_util_insert_parm_val(ref gcParmBlkp, gcip_defs_h.IPSET_PROTOCOL,
                gcip_defs_h.IPPARM_PROTOCOL_BITMASK, sizeof(int), gcip_defs_h.IP_PROTOCOL_SIP);
            result.ThrowIfGlobalCallError();

            var gclibMkBlk = new GCLIB_MAKECALL_BLK();

            gclibMkBlk.origination.address = ani;
            gclibMkBlk.origination.address_type = gclib_h.GCADDRTYPE_TRANSPARENT;

            gclibMkBlk.ext_datap = gcParmBlkp;

            var pGclib = _unmanagedMemoryServicePerCall.Create(nameof(GC_MAKECALL_BLK), gclibMkBlk);
            var gcMkBlk = new GC_MAKECALL_BLK
            {
                cclib = IntPtr.Zero,
                gclib = pGclib
            };

            SetCodec(gclib_h.GCTGT_GCLIB_CHAN);

            result = gclib_h.gc_MakeCall(_gcDev, ref _callReferenceNumber, dnis, ref gcMkBlk, 30, DXXXLIB_H.EV_ASYNC);
            result.ThrowIfGlobalCallError();
            gclib_h.gc_util_delete_parm_blk(gcParmBlkp);
            _unmanagedMemoryServicePerCall.Free(pGclib);

            result = WaitForEvent(gclib_h.GCEV_ALERTING, 40); // 40 seconds - 10 seconds was sometimes too short

            string message;
            switch (result)
            {
                case SYNC_WAIT_EXPIRED:
                    message = "The MakeCall method timed out waiting for the GCEV_ALERTING event";
                    _logger.LogError(message);
                    throw new VoiceException(message);
                case SYNC_WAIT_SUCCESS:
                    _logger.LogDebug("The MakeCall method received the GCEV_ALERTING event");
                    break;
                case SYNC_WAIT_ERROR:
                    message = "The MakeCall method failed waiting for the GCEV_ALERTING event";
                    _logger.LogError(message);
                    throw new VoiceException(message);
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

            var result = DXXXLIB_H.dx_stopch(_devh, DXXXLIB_H.EV_SYNC);
            result.ThrowIfStandardRuntimeLibraryError(_devh);
            _disposeTriggerActivated = true;
        }

        #endregion

        public void Dispose()
        {
            _logger.LogDebug("Dispose() - Disposing of the line");

            try
            {
                _waitCallSet = false;
                var result = DXXXLIB_H.dx_close(_devh);
                result.ThrowIfStandardRuntimeLibraryError(_devh);

            }
            finally
            {
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
            CheckCallState();
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
            if (DXXXLIB_H.dx_reciottdata(_devh, ref iott, ref tpt[0], ref xpb, DXXXLIB_H.RM_TONE | DXXXLIB_H.EV_ASYNC) == -1)
            {
                var errPtr = srllib_h.ATDV_ERRMSGP(_devh);
                var err = errPtr.IntPtrToString();
                DXXXLIB_H.dx_fileclose(iott.io_fhandle);
                throw new VoiceException(err);
            }

            try
            {
                var waitResult = WaitForEvent(DXXXLIB_H.TDX_RECORD, 180); // 3 minutes
                switch (waitResult)
                {
                    case SYNC_WAIT_SUCCESS:
                        _logger.LogDebug("The RecordToFile method received the TDX_RECORD event");
                        break;
                    case SYNC_WAIT_EXPIRED:
                        var message = "The RecordToFile method timed out waiting for the TDX_RECORD event";
                        _logger.LogError(message);
                        throw new VoiceException(message);
                    case SYNC_WAIT_ERROR:
                        message = "The RecordToFile method failed waiting for the TDX_RECORD event";
                        _logger.LogError(message);
                        throw new VoiceException(message);
                }
            }
            catch (HangupException)
            {
                ClearEventBuffer(_devh, 2000); // Did not get the TDX_RECORD event so clear the buffer so it isn't captured later
                DXXXLIB_H.dx_fileclose(iott.io_fhandle);
                _logger.LogDebug(
                    "Hangup Exception : The file handle has been closed because the call has been hung up.");
                throw;
            }
            if (DXXXLIB_H.dx_fileclose(iott.io_fhandle) == -1)
            {
                var errPtr = srllib_h.ATDV_ERRMSGP(_devh);
                var err = errPtr.IntPtrToString();
                throw new VoiceException(err);
            }

            var reason = DXXXLIB_H.ATDX_TERMMSK(_devh);
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
            return GetDigits(_devh, numberOfDigits, terminators);
        }

        public string FlushDigitBuffer()
        {
            _logger.LogDebug("FlushDigitBuffer()");

            var all = "";
            try
            {
                // add "T" so that I can get all the characters.
                all = GetDigits(_devh, DXDIGIT_H.DG_MAXDIGS, "T", 100);
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
                var result = DXXXLIB_H.dx_adjsv(_devh, DXXXLIB_H.SV_VOLUMETBL, DXXXLIB_H.SV_ABSPOS, adjsize);
                result.ThrowIfStandardRuntimeLibraryError(_devh);
                _volume = value;
            }
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

        public void DeleteCustomTones()
        {
            _logger.LogDebug("DeleteCustomTones()");
            //Dialogic.DeleteTones(LineNumber);
            //Dialogic.DeleteTones(_devh);
            AddSpecialCustomTones();
        }

        private void AddSpecialCustomTones()
        {
            _logger.LogDebug("AddSpecialCustomTones()");
            AddCustomTone(_voiceProperties.DialTone);
        }


        public void AddCustomTone(CustomTone tone)
        {
            _logger.LogDebug("AddCustomTone()");

            if (tone.ToneType == CustomToneType.Single)
            {
                // TODO
            }
            else if (tone.ToneType == CustomToneType.Dual)
            {
                AddDualTone(_devh, tone.Tid, tone.Freq1, tone.Frq1Dev, tone.Freq2, tone.Frq2Dev, tone.Mode);
            }
            else if (tone.ToneType == CustomToneType.DualWithCadence)
            {
                AddDualToneWithCadence(_devh, tone.Tid, tone.Freq1, tone.Frq1Dev, tone.Freq2, tone.Frq2Dev, tone.Ontime,
                    tone.Ontdev, tone.Offtime,
                    tone.Offtdev, tone.Repcnt);
            }

            DisableTone(_devh, tone.Tid);
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
            result.ThrowIfStandardRuntimeLibraryError(_devh);
        }

        // ReSharper disable once UnusedMember.Local
        private void EnableTone(int devh, int tid)
        {
            _logger.LogDebug("EnableTone({0}, {1})", devh, tid);

            var result = DXXXLIB_H.dx_enbtone(devh, tid, DXXXLIB_H.DM_TONEON | DXXXLIB_H.DM_TONEOFF);
            result.ThrowIfStandardRuntimeLibraryError(_devh);
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

        private void Open()
        {
            _logger.LogDebug("Open() - Opening line: {0}", _lineNumber);

            if (_boardDev == 0)
            {
                var boardResult = gclib_h.gc_OpenEx(ref _boardDev, ":N_iptB1:P_IP", DXXXLIB_H.EV_SYNC, IntPtr.Zero);
                _logger.LogDebug(
                    "get _boardDev: result = {0} = gc_openEx([ref]{1}, :N_iptB1:P_IP, EV_SYNC, IntPtr.Zero)...", boardResult,
                    _boardDev);
                boardResult.ThrowIfGlobalCallError();
                SetupGlobalCallParameterBlock();
            }

            var id = _lineNumber + _voiceProperties.SipChannelOffset;

            var boardId = (id - 1) / 4 + 1;
            var channelId = id - (boardId - 1) * 4;

            var devName = $"dxxxB{boardId}C{channelId}";

            _devh = DXXXLIB_H.dx_open(devName, 0);
            _logger.LogDebug("get _devh = {0} = DXXXLIB_H.dx_open({1}, 0)", _devh, devName);

            var result = DXXXLIB_H.dx_setevtmsk(_devh, DXXXLIB_H.DM_RINGS | DXXXLIB_H.DM_DIGITS | DXXXLIB_H.DM_LCOF);
            result.ThrowIfStandardRuntimeLibraryError(_devh);

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
            _logger.LogDebug("ConnectVoice() - _devh = {0}, _gcDev = {1}", _devh, _gcDev);

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

            result = DXXXLIB_H.dx_listen(_devh, ref scTsinfo);
            result.ThrowIfStandardRuntimeLibraryError(_devh);

            _voxXslot = Marshal.AllocHGlobal(4);
            Marshal.WriteInt32(_voxXslot, 0);
            _unmanagedMemoryService.Push("_voxXslot", _voxXslot);


            scTsinfo.sc_numts = 1;
            scTsinfo.sc_tsarrayp = _voxXslot;

            result = DXXXLIB_H.dx_getxmitslot(_devh, ref scTsinfo);
            result.ThrowIfStandardRuntimeLibraryError(_devh);

            result = gclib_h.gc_Listen(_gcDev, ref scTsinfo, DXXXLIB_H.EV_SYNC);
            result.ThrowIfGlobalCallError();
        }


        private void Register()
        {
            _logger.LogDebug("Register() - Registering line: {0}", _lineNumber);

            var proxy = _voiceProperties.SipProxyIp;
            var local = _voiceProperties.SipLocalIp;
            var alias = _voiceProperties.SipAlias;

            var regServer = $"{proxy}"; // Request-URI
            var regClient = $"{alias}@{proxy}"; // To header field
            var contact = $"sip:{alias}@{local}"; // Contact header field

            _logger.LogDebug("Register() - regServer = {0}, regClient = {1}, contact = {2}", regServer,
                regClient, contact);

            SetAuthenticationInfo();

            var gcParmBlkPtr = IntPtr.Zero;

            var result = gclib_h.gc_util_insert_parm_val(ref gcParmBlkPtr, gccfgparm_h.GCSET_SERVREQ,
                gccfgparm_h.PARM_REQTYPE, sizeof(byte), gcip_defs_h.IP_REQTYPE_REGISTRATION);
            result.ThrowIfGlobalCallError();

            result = gclib_h.gc_util_insert_parm_val(ref gcParmBlkPtr, gccfgparm_h.GCSET_SERVREQ, gccfgparm_h.PARM_ACK,
                sizeof(byte), gcip_defs_h.IP_REQTYPE_REGISTRATION);
            result.ThrowIfGlobalCallError();

            result = gclib_h.gc_util_insert_parm_val(ref gcParmBlkPtr, gcip_defs_h.IPSET_PROTOCOL,
                gcip_defs_h.IPPARM_PROTOCOL_BITMASK, sizeof(byte), gcip_defs_h.IP_PROTOCOL_SIP);
            result.ThrowIfGlobalCallError();

            result = gclib_h.gc_util_insert_parm_val(ref gcParmBlkPtr, gcip_defs_h.IPSET_REG_INFO,
                gcip_defs_h.IPPARM_OPERATION_REGISTER, sizeof(byte), gcip_defs_h.IP_REG_SET_INFO);
            result.ThrowIfGlobalCallError();

            var ipRegisterAddress = new IP_REGISTER_ADDRESS
            {
                reg_client = regClient,
                reg_server = regServer,
                time_to_live = 3600,
                max_hops = 30
            };

            var dataSize = (byte)Marshal.SizeOf<IP_REGISTER_ADDRESS>();

            var pData = _unmanagedMemoryService.Create(nameof(IP_REGISTER_ADDRESS), ipRegisterAddress);

            result = gclib_h.gc_util_insert_parm_ref(ref gcParmBlkPtr, gcip_defs_h.IPSET_REG_INFO,
                gcip_defs_h.IPPARM_REG_ADDRESS,
                dataSize, pData);
            result.ThrowIfGlobalCallError();

            dataSize = (byte)(contact.Length + 1);

            var pContact = _unmanagedMemoryService.StringToHGlobalAnsi("pContact", contact);

            result = gclib_h.gc_util_insert_parm_ref(ref gcParmBlkPtr, gcip_defs_h.IPSET_LOCAL_ALIAS,
                gcip_defs_h.IPPARM_ADDRESS_TRANSPARENT, dataSize, pContact);
            result.ThrowIfGlobalCallError();

            uint serviceId = 1;

            var respDataPp = IntPtr.Zero;

            _logger.LogDebug("Register() - about to call gc_ReqService asynchronously");
            result = gclib_h.gc_ReqService(gclib_h.GCTGT_CCLIB_NETIF, _boardDev, ref serviceId, gcParmBlkPtr,
                ref respDataPp,
                DXXXLIB_H.EV_ASYNC);
            result.ThrowIfGlobalCallError();
            _logger.LogDebug("Register() - called gc_ReqService asynchronously");
            gclib_h.gc_util_delete_parm_blk(gcParmBlkPtr);

            result = WaitForEvent(gclib_h.GCEV_SERVICERESP, 10); // wait for 10 seconds 
            _logger.LogDebug("Result for gc_ReqService is {0}", result);

            _unmanagedMemoryService.Free(pContact);
            _unmanagedMemoryService.Free(pData);

        }

        private void SetAuthenticationInfo()
        {
            _logger.LogDebug("SetAuthenticationInfo()");
            var proxy = _voiceProperties.SipProxyIp;
            var alias = _voiceProperties.SipAlias;

            var password = _voiceProperties.SipPassword;
            var realm = _voiceProperties.SipRealm;

            var identity = $"sip:{alias}@{proxy}";

            _logger.LogDebug("SetAuthenticationInfo() - proxy = {0}, alias = {1}, Password ****, Realm = {2}, identity = {3}", proxy,
                alias, realm, identity);

            var auth = new IP_AUTHENTICATION
            {
                version = gcip_h.IP_AUTHENTICATION_VERSION,
                realm = realm,
                identity = identity,
                username = alias,
                password = password
            };

            var gcParmBlkPtr = IntPtr.Zero;
            var dataSize = (byte)Marshal.SizeOf<IP_AUTHENTICATION>();

            var pData = _unmanagedMemoryService.Create(nameof(IP_AUTHENTICATION), auth);

            var result = gclib_h.gc_util_insert_parm_ref(ref gcParmBlkPtr, gcip_defs_h.IPSET_CONFIG,
                gcip_defs_h.IPPARM_AUTHENTICATION_CONFIGURE, dataSize, pData);
            result.ThrowIfGlobalCallError();

            result = gclib_h.gc_SetAuthenticationInfo(gclib_h.GCTGT_CCLIB_NETIF, _boardDev, gcParmBlkPtr);
            result.ThrowIfGlobalCallError();

            gclib_h.gc_util_delete_parm_blk(gcParmBlkPtr);
            _unmanagedMemoryService.Free(pData);
        }

        private void SetupGlobalCallParameterBlock()
        {
            _logger.LogDebug("SetupGlobalCallParameterBlock() - _boardDev = {0}", _boardDev);
            var gcParmBlkPtr = IntPtr.Zero;

            //setting T.38 fax server operating mode: IP MANUAL mode
            var result = gclib_h.gc_util_insert_parm_val(ref gcParmBlkPtr, gcip_defs_h.IPSET_CONFIG,
                gcip_defs_h.IPPARM_OPERATING_MODE, sizeof(int), gcip_defs_h.IP_MANUAL_MODE);
            result.ThrowIfGlobalCallError();

            //Enabling and Disabling Unsolicited Notification Events
            result = gclib_h.gc_util_insert_parm_val(ref gcParmBlkPtr, gcip_defs_h.IPSET_EXTENSIONEVT_MSK,
                gclib_h.GCACT_ADDMSK, sizeof(int),
                gcip_defs_h.EXTENSIONEVT_DTMF_ALPHANUMERIC | gcip_defs_h.EXTENSIONEVT_SIGNALING_STATUS |
                gcip_defs_h.EXTENSIONEVT_STREAMING_STATUS | gcip_defs_h.EXTENSIONEVT_T38_STATUS);
            result.ThrowIfGlobalCallError();

            var requestId = 0;
            result = gclib_h.gc_SetConfigData(gclib_h.GCTGT_CCLIB_NETIF, _boardDev, gcParmBlkPtr, 0,
                gclib_h.GCUPDATE_IMMEDIATE, ref requestId, DXXXLIB_H.EV_ASYNC);
            result.ThrowIfGlobalCallError();

            gclib_h.gc_util_delete_parm_blk(gcParmBlkPtr);

            result = WaitForEvent(gclib_h.GCEV_SETCONFIGDATA, 10); // wait for 10 seconds
            switch (result)
            {
                case SYNC_WAIT_EXPIRED:
                    _logger.LogError("gc_SetConfigData has expired");
                    break;
                case SYNC_WAIT_SUCCESS:
                    _logger.LogDebug("gc_SetConfigData was a success");
                    break;
                case SYNC_WAIT_ERROR:
                    _logger.LogError("gc_SetConfigData has failed");
                    break;
            }
        }

        /**
    * Process a metaevent extension block.
    */
        private void ProcessExtension(METAEVENT metaEvt)
        {
            // todo this mess needs to be written better :)
            _logger.LogDebug("ProcessExtension(METAEVENT metaEvt)");

            var gcParmBlkp = metaEvt.extevtdatap + 1;
            var parmDatap = IntPtr.Zero;

            parmDatap = gcip_h.gc_util_next_parm(gcParmBlkp, parmDatap);

            while (parmDatap != IntPtr.Zero)
            {
                var parmData = Marshal.PtrToStructure<GC_PARM_DATA>(parmDatap);

                switch (parmData.set_ID)
                {
                    case gcip_defs_h.IPSET_SWITCH_CODEC:
                        _logger.LogDebug("IPSET_SWITCH_CODEC:");
                        switch (parmData.parm_ID)
                        {
                            case gcip_defs_h.IPPARM_AUDIO_REQUESTED:
                                _logger.LogDebug("  IPPARM_AUDIO_REQUESTED:");
                                ResponseCodecRequest(true);
                                break;
                            case gcip_defs_h.IPPARM_READY:
                                _logger.LogDebug("  IPPARM_READY:");
                                break;
                            default:
                                _logger.LogError("  Got unknown extension parmID {0}", parmData.parm_ID);
                                break;
                        }

                        break;
                    case gcip_defs_h.IPSET_MEDIA_STATE:
                        _logger.LogDebug("IPSET_MEDIA_STATE:");
                        switch (parmData.parm_ID)
                        {
                            case gcip_defs_h.IPPARM_TX_CONNECTED:
                                _logger.LogDebug("  IPPARM_TX_CONNECTED");
                                break;
                            case gcip_defs_h.IPPARM_TX_DISCONNECTED:
                                _logger.LogDebug("  IPPARM_TX_DISCONNECTED");
                                break;
                            case gcip_defs_h.IPPARM_RX_CONNECTED:
                                _logger.LogDebug("  IPPARM_RX_CONNECTED");
                                break;
                            case gcip_defs_h.IPPARM_RX_DISCONNECTED:
                                _logger.LogDebug("  IPPARM_RX_DISCONNECTED");
                                break;
                            default:
                                _logger.LogError("  Got unknown extension parmID {0}", parmData.parm_ID);
                                break;
                        }

                        if (parmData.value_size == Marshal.SizeOf<IP_CAPABILITY>())
                        {
                            var ptr = parmDatap + 5;
                            var ipCapp = Marshal.PtrToStructure<IP_CAPABILITY>(ptr);

                            _logger.LogDebug(
                                "    stream codec infomation for TX: capability({0}), dir({1}), frames_per_pkt({2}), VAD({3})",
                                ipCapp.capability, ipCapp.direction, ipCapp.extra.audio.frames_per_pkt,
                                ipCapp.extra.audio.VAD);
                        }

                        break;
                    case gcip_defs_h.IPSET_IPPROTOCOL_STATE:
                        _logger.LogDebug("IPSET_IPPROTOCOL_STATE:");
                        switch (parmData.parm_ID)
                        {
                            case gcip_defs_h.IPPARM_SIGNALING_CONNECTED:
                                _logger.LogDebug("  IPPARM_SIGNALING_CONNECTED");
                                break;
                            case gcip_defs_h.IPPARM_SIGNALING_DISCONNECTED:
                                _logger.LogDebug("  IPPARM_SIGNALING_DISCONNECTED");
                                break;
                            case gcip_defs_h.IPPARM_CONTROL_CONNECTED:
                                _logger.LogDebug("  IPPARM_CONTROL_CONNECTED");
                                break;
                            case gcip_defs_h.IPPARM_CONTROL_DISCONNECTED:
                                _logger.LogDebug("  IPPARM_CONTROL_DISCONNECTED");
                                break;
                            default:
                                _logger.LogError("  Got unknown extension parmID {0}", parmData.parm_ID);
                                break;
                        }

                        break;
                    case gcip_defs_h.IPSET_RTP_ADDRESS:
                        _logger.LogDebug("IPSET_RTP_ADDRESS:");
                        switch (parmData.parm_ID)
                        {
                            case gcip_defs_h.IPPARM_LOCAL:
                                _logger.LogDebug("IPPARM_LOCAL: size = {0}", parmData.value_size);
                                var ptr = parmDatap + 5;
                                var ipAddr = Marshal.PtrToStructure<RTP_ADDR>(ptr);
                                _logger.LogDebug("  IPPARM_LOCAL: address:{0}, port {1}", GetIp(ipAddr.u_ipaddr.ipv4),
                                    ipAddr.port);
                                break;
                            case gcip_defs_h.IPPARM_REMOTE:
                                _logger.LogDebug("IPPARM_REMOTE: size = {0}", parmData.value_size);
                                var ptr2 = parmDatap + 5;
                                var ipAddr2 = Marshal.PtrToStructure<RTP_ADDR>(ptr2);
                                _logger.LogDebug("  IPPARM_REMOTE: address:{0}, port {1}", GetIp(ipAddr2.u_ipaddr.ipv4),
                                    ipAddr2.port);
                                break;
                            default:
                                _logger.LogError("  Got unknown extension parmID {0}", parmData.parm_ID);
                                break;
                        }

                        break;
                    /* Set ID for SIP message types handed by GCEV_EXTENSION
                     * IPSET_MSG_SIP | This Set ID is used to set or get the SIP message type.
                     */
                    case gcip_defs_h.IPSET_MSG_SIP:
                        _logger.LogInformation("IPSET_MSG_SIP: {0}", parmData.parm_ID);
                        switch (parmData.parm_ID)
                        {
                            case gcip_defs_h.IPPARM_MSGTYPE:
                                var messType = parmData.value_buf;
                                _logger.LogDebug("  value_size = {0}, value_buf = {1}", parmData.value_size, parmData.value_buf);

                                // TODO I don;t think this is done properly at all.
                                switch (messType)
                                {
                                    case gcip_defs_h.IP_MSGTYPE_SIP_INFO_OK:
                                        _logger.LogDebug("  IP_MSGTYPE_SIP_INFO_OK");
                                        break;
                                    case gcip_defs_h.IP_MSGTYPE_SIP_INFO_FAILED:
                                        _logger.LogDebug("  IP_MSGTYPE_SIP_INFO_FAILED");
                                        break;
                                }
                                break;
                            case gcip_defs_h.IPPARM_MSG_SIP_RESPONSE_CODE:
                                _logger.LogDebug(" value_size = {0}, value_buf = {1}", parmData.value_size, parmData.value_buf);
                                var responseCode = parmData.value_buf;
                                break;
                        }
                        _logger.LogInformation("  IPSET_MSG_SIP:: size = {0}", parmData.value_size);
                        break;
                    case gcip_defs_h.IPSET_SIP_MSGINFO:
                        _logger.LogInformation("IPSET_SIP_MSGINFO:");
                        var str = GetStringFromPtr(parmDatap + 5, parmData.value_size);
                        _logger.LogDebug("  {0}: {1}", GetSipMsgInfo(parmData.parm_ID), str);
                        break;
                    default:
                        _logger.LogError("Got unknown set_ID({0}).", parmData.set_ID);
                        break;
                }

                parmDatap = gcip_h.gc_util_next_parm(gcParmBlkp, parmDatap);
            }
        }

        private string GetSipMsgInfo(ushort parm_ID)
        {
            switch (parm_ID)
            {
                case gcip_defs_h.IPPARM_REQUEST_URI:
                    return "IPPARM_REQUEST_URI";
                case gcip_defs_h.IPPARM_CONTACT_URI:
                    return "IPPARM_CONTACT_URI";
                case gcip_defs_h.IPPARM_FROM_DISPLAY:
                    return "IPPARM_FROM_DISPLAY";
                case gcip_defs_h.IPPARM_TO_DISPLAY:
                    return "IPPARM_TO_DISPLAY";
                case gcip_defs_h.IPPARM_CONTACT_DISPLAY:
                    return "IPPARM_CONTACT_DISPLAY";
                case gcip_defs_h.IPPARM_REFERRED_BY:
                    return "IPPARM_REFERRED_BY";
                case gcip_defs_h.IPPARM_REPLACES:
                    return "IPPARM_REPLACES";
                case gcip_defs_h.IPPARM_CONTENT_DISPOSITION:
                    return "IPPARM_CONTENT_DISPOSITION";
                case gcip_defs_h.IPPARM_CONTENT_ENCODING:
                    return "IPPARM_CONTENT_ENCODING";
                case gcip_defs_h.IPPARM_CONTENT_LENGTH:
                    return "IPPARM_CONTENT_LENGTH";
                case gcip_defs_h.IPPARM_CONTENT_TYPE:
                    return "IPPARM_CONTENT_TYPE";
                case gcip_defs_h.IPPARM_REFER_TO:
                    return "IPPARM_REFER_TO";
                case gcip_defs_h.IPPARM_DIVERSION_URI:
                    return "IPPARM_DIVERSION_URI";
                case gcip_defs_h.IPPARM_EVENT_HDR:
                    return "IPPARM_EVENT_HDR";
                case gcip_defs_h.IPPARM_EXPIRES_HDR:
                    return "IPPARM_EXPIRES_HDR";
                case gcip_defs_h.IPPARM_CALLID_HDR:
                    return "IPPARM_CALLID_HDR";
                case gcip_defs_h.IPPARM_SIP_HDR:
                    return "IPPARM_SIP_HDR";
                case gcip_defs_h.IPPARM_FROM:
                    return "IPPARM_FROM";
                case gcip_defs_h.IPPARM_TO:
                    return "IPPARM_TO";
                case gcip_defs_h.IPPARM_SIP_HDR_REMOVE:
                    return "IPPARM_SIP_HDR_REMOVE";
                case gcip_defs_h.IPPARM_SIP_VIA_HDR_REPLACE:
                    return "IPPARM_SIP_VIA_HDR_REPLACE";
            }
            return $"unknown {parm_ID}";
        }

        private string GetIp(uint ip)
        {
            try
            {
                //var netorderIp = IPAddress.HostToNetworkOrder(shit);
                var ipAddress = new IPAddress(ip);
                return ipAddress.ToString();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to convert to an IP");
                return ip.ToString();
            }
        }

        /**
        * Process a codec request.
        */
        private void ResponseCodecRequest(bool acceptCall)
        {
            _logger.LogDebug("response_codec_request({0})...", acceptCall ? "accept" : "reject");
            var gcParmBlkPtr = IntPtr.Zero;
            var result = gclib_h.gc_util_insert_parm_val(ref gcParmBlkPtr, gcip_defs_h.IPSET_SWITCH_CODEC,
                (ushort)(acceptCall ? gcip_defs_h.IPPARM_ACCEPT : gcip_defs_h.IPPARM_REJECT), sizeof(int), 0);
            result.ThrowIfGlobalCallError();

            var returnParamPtr = IntPtr.Zero;

            result = gclib_h.gc_Extension(gclib_h.GCTGT_GCLIB_CRN, _callReferenceNumber, gcip_defs_h.IPEXTID_CHANGEMODE,
                gcParmBlkPtr, ref returnParamPtr, DXXXLIB_H.EV_ASYNC);
            result.ThrowIfGlobalCallError();

            gclib_h.gc_util_delete_parm_blk(gcParmBlkPtr);
        }

        private string TranslateEventId(int eventId)
        {
            return GetEventTypeDescription(eventId)?? gcmsg_h.GCEV_MSG(eventId);
        }


        private int WaitForEventIndefinitely(int waitForEvent)
        {
            _logger.LogDebug("*** Waiting for event: {0}: {1}, waitSeconds = indefinitely", waitForEvent, TranslateEventId(waitForEvent));

            int result;
            while ((result = WaitForEvent(waitForEvent, 5, false)) == SYNC_WAIT_EXPIRED) // wait  5 seconds
            {
                if (result == -SYNC_WAIT_EXPIRED) _logger.LogTrace("Wait for call exhausted. Will try again");
                CheckDisposing();
            }

            return result;
        }

        private int WaitForEvent(int waitForEvent, int waitSeconds, bool showDebug = true)
        {
            var handles = new[] { _gcDev, _boardDev, _devh };
            return WaitForEvent(waitForEvent, waitSeconds, handles, showDebug);
        }

        private int WaitForEvent(int waitForEvent, int waitSeconds, int[] handles, bool showDebug = true)
        {
            if (showDebug) _logger.LogDebug("*** Waiting for event: {0}: {1}, waitSeconds = {2}", waitForEvent, TranslateEventId(waitForEvent), waitSeconds);

            var eventThrown = -1;
            var count = 0;
            var eventHandle = 0;

            do
            {
                var result = srllib_h.sr_waitevtEx(handles, handles.Length, 1000, ref eventHandle);
                var timedOut = IsTimeout(result, _devh);
                if (!timedOut)
                {
                    eventThrown = ProcessEvent(eventHandle);
                    if (eventThrown == waitForEvent) break;
                }
                CheckDisposing();

                count++;
            } while (LoopAgain(eventThrown, waitForEvent, count, waitSeconds));

            if (eventThrown == waitForEvent)
            {
                return SYNC_WAIT_SUCCESS;
            }

            if (HasExpired(count, waitSeconds))
            {
                return SYNC_WAIT_EXPIRED;
            }

            return SYNC_WAIT_ERROR;
        }

        // SR_waitEvtEx() returns -1 for both an error and a timeout so need to check for an error.
        private bool IsTimeout(int error, int devHandle)
        {
            _logger.LogTrace("IsTimeout({0}, {1})", error, devHandle);

            if (error != -1) return false;

            // here is where we need to tell if -1 is a timeout or not
            var result = srllib_h.ATDV_LASTERR(devHandle);
            if (result == -1) throw new VoiceException($"Unable to get failure for: {devHandle}");

            if (result == srllib_h.ESR_NOERR) return true; // no error so it must have timed out.

            // i am getting an error of 1 sometimes for some reason so I am going to do another check just to make sure
            var eventType = srllib_h.sr_getevttype((uint)devHandle);
            if (eventType == srllib_h.SR_TMOUTEVT) return true;

            _logger.LogError("WTH: error is: {0}", result);
            return true; // was false but I can't get the error!!!!
        }

        private bool HasExpired(int count, int waitSeconds)
        {
            _logger.LogTrace("HasExpired({0}, {1})", count, waitSeconds);

            if (waitSeconds == SYNC_WAIT_INFINITE)
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

        private int ProcessEvent(int eventHandle)
        {
            _logger.LogDebug("ProcessEvent({0})", eventHandle);

            var metaEvt = new METAEVENT();

            var result = gclib_h.gc_GetMetaEventEx(ref metaEvt, eventHandle);
            result.ThrowIfGlobalCallError();

            _logger.LogDebug(
                "evt_type = {0}:{1}, evt_dev = {2}, evt_flags = {3}, board_dev = {4}, line_dev = {5} ",
                metaEvt.evttype, TranslateEventId(metaEvt.evttype), metaEvt.evtdev, metaEvt.flags, _boardDev,
                metaEvt.linedev);

            if ((metaEvt.flags & gclib_h.GCME_GC_EVENT) == gclib_h.GCME_GC_EVENT)
            {
                //for register
                if (metaEvt.evtdev == _boardDev && metaEvt.evttype == gclib_h.GCEV_SERVICERESP)
                {
                    HandleRegisterStuff(metaEvt);
                }
                else
                {
                    HandleGcEvents(metaEvt);
                }
            } else
            {
                HandleOtherEvents((uint)eventHandle, metaEvt);
            }

            return metaEvt.evttype;
        }

        private void HandleOtherEvents(uint eventHandle, METAEVENT metaEvt)
        {
            switch (metaEvt.evttype)
            {
                case DXXXLIB_H.TDX_CST: // a call status transition event.
                    var ptr = srllib_h.sr_getevtdatap(eventHandle);

                    var dxCst = Marshal.PtrToStructure<DX_CST>(ptr);
                    _logger.LogDebug("Call status transition event = {0}: {1}", dxCst.cst_event, CstDescription(dxCst.cst_event));
                    break;

            }
        }

        private void HandleRegisterStuff(METAEVENT metaEvt)
        {
            _logger.LogDebug("HandleRegisterStuff(METAEVENT metaEvt)");
            var gcParmBlkp = metaEvt.extevtdatap;
            var parmDatap = IntPtr.Zero;

            parmDatap = gcip_h.gc_util_next_parm(gcParmBlkp, parmDatap);

            while (parmDatap != IntPtr.Zero)
            {
                var parmData = Marshal.PtrToStructure<GC_PARM_DATA>(parmDatap);

                switch (parmData.set_ID)
                {
                    case gcip_defs_h.IPSET_REG_INFO:
                        switch (parmData.parm_ID)
                        {
                            case gcip_defs_h.IPPARM_REG_STATUS:
                            {
                                var value = GetValueFromPtr(parmDatap + 5, parmData.value_size);
                                switch (value)
                                {
                                    case gcip_defs_h.IP_REG_CONFIRMED:
                                        _logger.LogDebug("    IPSET_REG_INFO/IPPARM_REG_STATUS: IP_REG_CONFIRMED");
                                        break;
                                    case gcip_defs_h.IP_REG_REJECTED:
                                        _logger.LogDebug("    IPSET_REG_INFO/IPPARM_REG_STATUS: IP_REG_REJECTED");
                                        break;
                                }

                                break;
                            }
                            case gcip_defs_h.IPPARM_REG_SERVICEID:
                            {
                                var value = GetValueFromPtr(parmDatap + 5, parmData.value_size);
                                _logger.LogDebug("    IPSET_REG_INFO/IPPARM_REG_SERVICEID: 0x{0:X}", value);
                                break;
                            }
                            default:
                                _logger.LogDebug(
                                    "    Missed one: set_ID = IPSET_REG_INFO, parm_ID = {1:X}, bytes = {2}",
                                    parmData.parm_ID, parmData.value_size);
                                break;
                        }

                        break;
                    case gcip_defs_h.IPSET_PROTOCOL:
                        var value2 = GetValueFromPtr(parmDatap + 5, parmData.value_size);
                        _logger.LogDebug("    IPSET_PROTOCOL value: {0}", value2);
                        break;
                    case gcip_defs_h.IPSET_LOCAL_ALIAS:
                    {
                        var localAlias = GetStringFromPtr(parmDatap + 5, parmData.value_size);
                        _logger.LogDebug("    IPSET_LOCAL_ALIAS value: {0}", localAlias);
                        break;
                    }
                    case gcip_defs_h.IPSET_SIP_MSGINFO:
                    {
                        var msgInfo = GetStringFromPtr(parmDatap + 5, parmData.value_size);
                        _logger.LogDebug("    IPSET_SIP_MSGINFO value: {0}", msgInfo);
                        break;
                    }
                    default:
                        _logger.LogDebug("    Missed one: set_ID = {0:X}, parm_ID = {1:X}, bytes = {2}",
                            parmData.set_ID, parmData.parm_ID, parmData.value_size);
                        break;
                }

                parmDatap = gcip_h.gc_util_next_parm(gcParmBlkp, parmDatap);
            }

            _logger.LogDebug("HandleRegisterStuff(METAEVENT metaEvt) - done!");
        }

        private string GetStringFromPtr(IntPtr ptr, int size)
        {
            return Marshal.PtrToStringAnsi(ptr, size);
        }

        private int GetValueFromPtr(IntPtr ptr, byte size)
        {
            int value;
            switch (size)
            {
                case 1:
                    value = Marshal.ReadByte(ptr);
                    break;
                case 2:
                    value = Marshal.ReadInt16(ptr);
                    break;
                case 4:
                    value = Marshal.ReadInt32(ptr);
                    break;
                default:
                    throw new VoiceException($"Unable to get value from ptr. Size is {size}");
            }

            return value;
        }

        private void HandleGcEvents(METAEVENT metaEvt)
        {
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

                #region Handle hangup detection
                case gclib_h.GCEV_DISCONNECTED:
                    _logger.LogDebug("GCEV_DISCONNECTED");
                    DropCall(); // will block for gcev_dropcall
                    break;
                case gclib_h.GCEV_DROPCALL:
                    _logger.LogDebug("GCEV_DROPCALL");
                    ReleaseCall(); // will block for gcev_releasecall
                    break;
                case gclib_h.GCEV_RELEASECALL:
                    _logger.LogDebug("GCEV_RELEASECALL - set crn = 0");
                    _callReferenceNumber = 0;
                    throw new HangupException();
                #endregion

                case gclib_h.GCEV_EXTENSIONCMPLT:
                    _logger.LogDebug("GCEV_EXTENSIONCMPLT - we do nothing with this event");
                    break;
                case gclib_h.GCEV_EXTENSION:
                    _logger.LogDebug("GCEV_EXTENSION");
                    ProcessExtension(metaEvt);
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
                    break;
                case gclib_h.GCEV_ATTACH:
                    _logger.LogDebug("GCEV_ATTACH - we do nothing with this event");
                    break;
                default:
                    _logger.LogInformation("gc_Unknown type - {0}", metaEvt.evttype);
                    break;
            }
        }

        private void LogWarningMessage(METAEVENT metaEvt)
        {
            var callStatusInfo = new GC_INFO();

            var ptr = _unmanagedMemoryService.Create($"{nameof(GC_INFO)} for LogWarningMessage", callStatusInfo);

            var result = gclib_h.gc_ResultInfo(ref metaEvt, ptr);
            try
            {
                result.ThrowIfGlobalCallError();

                callStatusInfo = Marshal.PtrToStructure<GC_INFO>(ptr);
                _unmanagedMemoryService.Free(ptr);

                var ex = new GlobalCallErrorException(callStatusInfo);
                _logger.LogWarning(ex.Message);
            }
            catch (GlobalCallErrorException e)
            {
                // for now we will just log an error if we get one
                _logger.LogError(e, null);
            }
        }

        private void DropCall()
        {
            _logger.LogDebug("DropCall() - {0}", _callReferenceNumber);

            if (_callReferenceNumber == 0) return; // line is idle

            var result = gclib_h.gc_DropCall(_callReferenceNumber, gclib_h.GC_NORMAL_CLEARING, DXXXLIB_H.EV_ASYNC);
            result.ThrowIfGlobalCallError();

            // don't include _devh right now because we want to finish the drop call first
            result = WaitForEvent(gclib_h.GCEV_DROPCALL, 10, new []{ _gcDev, _boardDev}); // 10 seconds
        }

        /**
        * Release a call.
        */
        private void ReleaseCall()
        {
            _logger.LogDebug("ReleaseCall()");
            var result = gclib_h.gc_ReleaseCallEx(_callReferenceNumber, DXXXLIB_H.EV_ASYNC);
            result.ThrowIfGlobalCallError();

            // don't include _devh right now because we want to finish the drop call first
            result = WaitForEvent(gclib_h.GCEV_RELEASECALL, 10, new[] { _gcDev, _boardDev }); // 10 seconds
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

            gclib_h.gc_util_delete_parm_blk(parmblkp);
            foreach (var ptr in pointers)
            {
                _unmanagedMemoryServicePerCall.Free(ptr);
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
            CheckCallState();

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

            // todo there should be no reason to clear the event buffer. therefor I set messages to warn
            ClearEventBuffer(_devh); 

            var state = DXXXLIB_H.ATDX_STATE(_devh);
            _logger.LogDebug("About to play: {0} state: {1}", filename, state);
            if (!File.Exists(filename))
            {
                var err = $"File {filename} does not exist so it cannot be played, call will be droped.";
                _logger.LogError(err);
                throw new VoiceException(err);
            }

            /* Now play the file */
            if (DXXXLIB_H.dx_playiottdata(_devh, ref iott, ref tpt[0], ref _currentXpb, DXXXLIB_H.EV_ASYNC) == -1)
            {
                _logger.LogError("Tried to play: {0} state: {1}", filename, state);

                var err = srllib_h.ATDV_ERRMSGP(_devh);
                var message = err.IntPtrToString();
                DXXXLIB_H.dx_fileclose(iott.io_fhandle);
                throw new VoiceException(message);
            }

            try
            {
                var waitResult = WaitForEvent(DXXXLIB_H.TDX_PLAY, 60); // 1 minute
                switch (waitResult)
                {
                    case SYNC_WAIT_SUCCESS:
                        _logger.LogDebug("The PlaySipFile method received the TDX_PLAY event");
                        break;
                    case SYNC_WAIT_EXPIRED:
                        var message = "The PlaySipFile method timed out waiting for the TDX_PLAY event";
                        _logger.LogError(message);
                        throw new VoiceException(message);
                    case SYNC_WAIT_ERROR:
                        message = "The PlaySipFile method failed waiting for the TDX_PLAY event";
                        _logger.LogError(message);
                        throw new VoiceException(message);
                }
            }
            catch (HangupException)
            {
                ClearEventBuffer(_devh, 2000); // Did not get the TDX_PLAY event so clear the buffer so it isn't captured later
                DXXXLIB_H.dx_fileclose(iott.io_fhandle);
                _logger.LogDebug(
                    "Hangup Exception : The file handle has been closed because the call has been hung up.");
                throw;
            }

            // make sure the file is closed
            var result = DXXXLIB_H.dx_fileclose(iott.io_fhandle);
            result.ThrowIfStandardRuntimeLibraryError(_devh);

            var reason = DXXXLIB_H.ATDX_TERMMSK(_devh);

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
         * Checks the call state.
         * If the call is no longer connected (call_state == 4) 
         * then drop the call.
         */
        private void CheckCallState()
        {
            _logger.LogDebug("CheckCallState() - crn = {0}", _callReferenceNumber);
            if (_callReferenceNumber == 0) return;

            var callState = GetCallState();
            _logger.LogDebug("CheckCallState: Call State {0}", GetCallStateDescription(callState));
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

        private string GetCallStateDescription(int callState)
        {
            _logger.LogDebug("GetCallStateDescription({0})", callState);
            switch (callState)
            {
                case gclib_h.GCST_NULL:
                    return "GCST_NULL";
                case gclib_h.GCST_ACCEPTED:
                    return "GCST_ACCEPTED";
                case gclib_h.GCST_ALERTING:
                    return "GCST_ALERTING";
                case gclib_h.GCST_CONNECTED:
                    return "GCST_CONNECTED";
                case gclib_h.GCST_OFFERED:
                    return "GCST_OFFERED";
                case gclib_h.GCST_DIALING:
                    return "GCST_DIALING";
                case gclib_h.GCST_IDLE:
                    return "GCST_IDLE";
                case gclib_h.GCST_DISCONNECTED:
                    return "GCST_DISCONNECTED";
                case gclib_h.GCST_DIALTONE:
                    return "GCST_DIALTONE";
                case gclib_h.GCST_ONHOLDPENDINGTRANSFER:
                    return "GCST_ONHOLDPENDINGTRANSFER";
                case gclib_h.GCST_ONHOLD:
                    return "GCST_ONHOLD";
                case gclib_h.GCST_DETECTED:
                    return "GCST_DETECTED";
                case gclib_h.GCST_PROCEEDING:
                    return "GCST_PROCEEDING";
                case gclib_h.GCST_SENDMOREINFO:
                    return "GCST_SENDMOREINFO";
                case gclib_h.GCST_GETMOREINFO:
                    return "GCST_GETMOREINFO";
                case gclib_h.GCST_CALLROUTING:
                    return "GCST_CALLROUTING";
            }

            return callState.ToString();
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
                var handles = new[] { _devh };
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
                _logger.LogDebug("ClearEventBuffer: Type = {0} = {1}, Reason = {2} = {3}", type, GetEventTypeDescription(type),
                    reason, GetReasonDescription(reason));
            } while (true);
        }

        private string GetEventTypeDescription(int type)
        {
            _logger.LogDebug("GetEventTypeDescription({0})", type);
            switch (type)
            {
                case DXXXLIB_H.TDX_PLAY:
                    return "Play Completed";
                case DXXXLIB_H.TDX_RECORD:
                    return "Record Complete";
                case DXXXLIB_H.TDX_GETDIG:
                    return "Get Digits Completed";
                case DXXXLIB_H.TDX_DIAL:
                    return "Dial Completed";
                case DXXXLIB_H.TDX_CALLP:
                    return "Call Progress Completed";
                case DXXXLIB_H.TDX_CST:
                    return "CST Event Received";
                case DXXXLIB_H.TDX_SETHOOK:
                    return "SetHook Completed";
                case DXXXLIB_H.TDX_WINK:
                    return "Wink Completed";
                case DXXXLIB_H.TDX_ERROR:
                    return "Error Event";
                case DXXXLIB_H.TDX_PLAYTONE:
                    return "Play Tone Completed";
                case DXXXLIB_H.TDX_GETR2MF:
                    return "Get R2MF completed";
                case DXXXLIB_H.TDX_BARGEIN:
                    return "Barge in completed";
                case DXXXLIB_H.TDX_NOSTOP:
                    return "No Stop needed to be Issued";
                case DXXXLIB_H.TDX_UNKNOWN:
                    return "TDX_UNKNOWN";
            }

            return null;
        }

        private string CstDescription(int type)
        {
            switch (type)
            {
                case DXXXLIB_H.DE_DIGITS:
                    return "Digit Received";
                case DXXXLIB_H.DE_DIGOFF:
                    return "Digit tone off event";
                case DXXXLIB_H.DE_LCOF:
                    return "Loop current off";
                case DXXXLIB_H.DE_LCON:
                    return "Loop current on";
                case DXXXLIB_H.DE_LCREV:
                    return "Loop current reversal";
                case DXXXLIB_H.DE_RINGS:
                    return "Rings received";
                case DXXXLIB_H.DE_RNGOFF:
                    return "Ring off event";
                case DXXXLIB_H.DE_SILOF:
                    return "Silenec off";
                case DXXXLIB_H.DE_SILON:
                    return "Silence on";
                case DXXXLIB_H.DE_STOPRINGS:
                    return "Stop ring detect state";
                case DXXXLIB_H.DE_TONEOFF:
                    return "Tone OFF Event Received";
                case DXXXLIB_H.DE_TONEON:
                    return "Tone ON Event Received";
                case DXXXLIB_H.DE_UNDERRUN:
                    return "R4 Streaming to Board API FW underrun event. Improves streaming data to board";
                case DXXXLIB_H.DE_VAD:
                    return "Voice Energy detected";
                case DXXXLIB_H.DE_WINK:
                    return "Wink received";
            }

            return "unknown";
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
            CheckCallState();

            var state = DXXXLIB_H.ATDX_STATE(devh);
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("state: {0}", GetChannelStateDescription(state));
            }

            /*
             * If number of digits is 99 this will fail on SIP.
             * An invalid tpt error will be thrown.
             * I hacked this in place just to keep going with development.
             */
            if (numberOfDigits >= 15) numberOfDigits = 15;

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
                throw new VoiceException(message);
            }

            int waitResult;
            try
            {
                waitResult = WaitForEvent(DXXXLIB_H.TDX_GETDIG, 60); // 1 minute
            }
            catch (HangupException)
            {
                ClearEventBuffer(_devh, 2000); // Did not get the TDX_GETDIG event so clear the buffer so it isn't captured later
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
                case SYNC_WAIT_SUCCESS:
                    _logger.LogDebug("The GetDigits method received the TDX_GETDIG event");
                    break;
                case SYNC_WAIT_EXPIRED:
                    var message = "The GetDigits method timed out waiting for the TDX_GETDIG event";
                    _logger.LogError(message);
                    _unmanagedMemoryServicePerCall.Free(digitPtr);
                    throw new VoiceException(message);
                case SYNC_WAIT_ERROR:
                    message = "The GetDigits method failed waiting for the TDX_GETDIG event";
                    _logger.LogError(message);
                    _unmanagedMemoryServicePerCall.Free(digitPtr);
                    throw new VoiceException(message);
            }

            digit = Marshal.PtrToStructure<DV_DIGIT>(digitPtr);
            _unmanagedMemoryServicePerCall.Free(digitPtr);

            var reason = DXXXLIB_H.ATDX_TERMMSK(devh);
            CheckCallState();

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
            ClearDigits(devh); // not sure if this is necessary and perhaps only needed for getDigitsTimeoutException?
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

            // todo should not be needed!
            ClearEventBuffer(devh);

            return answer;
        }

        private void ClearDigits(int devh)
        {
            _logger.LogDebug("ClearDigits({0})", devh);

            var result = DXXXLIB_H.dx_clrdigbuf(devh);
            result.ThrowIfStandardRuntimeLibraryError(devh);
        }


        private string GetChannelStateDescription(int channelState)
        {
            _logger.LogDebug("GetChannelStateDescription({0})", channelState);

            switch (channelState)
            {
                case DXXXLIB_H.CS_IDLE:
                    return "Channel is idle";
                case DXXXLIB_H.CS_PLAY:
                    return "Channel is playing back";
                case DXXXLIB_H.CS_RECD:
                    return "Channel is recording";
                case DXXXLIB_H.CS_DIAL:
                    return "Channel is dialing";
                case DXXXLIB_H.CS_GTDIG:
                    return "Channel is getting digits";
                case DXXXLIB_H.CS_TONE:
                    return "Channel is generating a tone";
                case DXXXLIB_H.CS_STOPD:
                    return "Operation has terminated";
                case DXXXLIB_H.CS_SENDFAX:
                    return "Channel is sending a fax";
                case DXXXLIB_H.CS_RECVFAX:
                    return "Channel is receiving a fax";
                case DXXXLIB_H.CS_FAXIO:
                    return "Channel is between fax pages";
                case DXXXLIB_H.CS_HOOK:
                    return "A change in hookstate is in progress";
                case DXXXLIB_H.CS_WINK:
                    return "A wink operation is in progress";
                case DXXXLIB_H.CS_CALL:
                    return "Channel is Call Progress Mode";
                case DXXXLIB_H.CS_GETR2MF:
                    return "Channel is Getting R2MF";
                case DXXXLIB_H.CS_RINGS:
                    return "Call status Rings state";
                case DXXXLIB_H.CS_BLOCKED:
                    return "Channel is blocked";
                case DXXXLIB_H.CS_RECDPREPARE:
                    return "Channel is preparing record and driver has not yet sent record";
            }

            return $"Unknown channel: {channelState}";
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