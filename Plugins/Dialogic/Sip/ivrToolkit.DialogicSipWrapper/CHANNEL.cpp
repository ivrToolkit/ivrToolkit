// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 
// ReSharper disable CppDeprecatedEntity
#include "CHANNEL.h"

#include <fcntl.h>
#include <gcip.h>
#include <gcip_defs.h>
#include <gclib.h>

#include <boost/log/core.hpp>
#include <boost/log/trivial.hpp>
#include <boost/log/expressions.hpp>
#include <boost/log/sinks/text_file_backend.hpp>
#include <boost/log/utility/setup/file.hpp>
#include <boost/log/utility/setup/common_attributes.hpp>
#include <boost/log/sources/severity_logger.hpp>
#include <boost/log/sources/record_ostream.hpp>
#include <boost/format.hpp>

namespace logging = boost::log;
namespace src = boost::log::sources;
namespace sinks = boost::log::sinks;
namespace keywords = boost::log::keywords;



	ivrToolkit::DialogicSipWrapper::CHANNEL::CHANNEL(int lineNumber, int offset): gc_dev(0), ipm_dev(0), ip_xslot(0), vox_xslot(0), tpt(), xpb(), vox_iott(),
	                                                             device_name{},
	                                                             crn(0),
	                                                             vox_dev(0)
	{
		_lineNumber = lineNumber;
		_offset = offset;
		_id = _lineNumber + _offset;
		printDebug("CHANNEL(%i, %i)", lineNumber , offset);
	}

/**
	* Open the Voice Device
	* Open the Global Call Device
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::open() {
		_id = _lineNumber + _offset;
		
		printDebug("open(i=%i)...", _id);
		const int board_id = (_id - 1) / 4 + 1;
		const int channel_id = _id - (board_id - 1) * 4;
		long request_id = 0;
		GC_PARM_BLKP gc_parm_blkp = nullptr;
		char dev_name[64] = "";
		sprintf(dev_name, "dxxxB%dC%d", board_id, channel_id);
		vox_dev = dx_open(dev_name, NULL);

		dx_setevtmsk(vox_dev, DM_RINGS | DM_DIGITS | DM_LCOF);
		sprintf(dev_name, ":P_SIP:N_iptB1T%d:M_ipmB1C%d:V_dxxxB%dC%d", _lineNumber, _id, board_id, channel_id);
		sprintf(device_name, dev_name);
		printDebug("Device name = %s", dev_name);
		const int result = gc_OpenEx(&gc_dev, dev_name, EV_ASYNC, nullptr);
		printDebug("result for gc_OpenEx() = %i", result);
		printDebug("gc_dev = %d", gc_dev);
		print_gc_error_info("gc_OpenEx", result);

		//Enabling GCEV_INVOKE_XFER_ACCEPTED Events
		gc_util_insert_parm_val(&gc_parm_blkp, GCSET_CALLEVENT_MSK, GCACT_ADDMSK, sizeof(long), GCMSK_INVOKEXFER_ACCEPTED);
		gc_SetConfigData(GCTGT_GCLIB_CHAN, gc_dev, gc_parm_blkp, 0, GCUPDATE_IMMEDIATE, &request_id, EV_SYNC);
		gc_util_delete_parm_blk(gc_parm_blkp);
		printDebug("end of open()...");
	}
	/**
	* Get the global call device name, no checks are made if the device has been opened.
	*/
	char* ivrToolkit::DialogicSipWrapper::CHANNEL::get_device_name() {
		return device_name;
	}
	/**
	* Get the global call device handle, no checks are made if the device has been opened.
	*/
	long ivrToolkit::DialogicSipWrapper::CHANNEL::get_device_handle() const
	{
		return gc_dev;
	}
	/**
	* Get the voice device handle, no checks are made if the device has been opened.
	*/
	int ivrToolkit::DialogicSipWrapper::CHANNEL::get_voice_handle() const
	{
		return vox_dev;
	}
	/**
	* Get the media device handle, no checks are made if the device has been opened.
	*/
	int ivrToolkit::DialogicSipWrapper::CHANNEL::get_media_device_handle() const
	{
		return ipm_dev;
	}
	/**
	* Connect voice to the channel.
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::connect_voice() {
		printDebug("connect_voice()...");
		printDebug("vox_dev = %d", vox_dev);
		printDebug("gc_dev = %d", gc_dev);
		SC_TSINFO sc_tsinfo;
		gc_GetResourceH(gc_dev, &ipm_dev, GC_MEDIADEVICE);
		sc_tsinfo.sc_numts = 1;
		sc_tsinfo.sc_tsarrayp = &ip_xslot;
		gc_GetXmitSlot(gc_dev, &sc_tsinfo);

		dx_listen(vox_dev, &sc_tsinfo);

		sc_tsinfo.sc_numts = 1;
		sc_tsinfo.sc_tsarrayp = &vox_xslot;
		dx_getxmitslot(vox_dev, &sc_tsinfo);
		gc_Listen(gc_dev, &sc_tsinfo, EV_SYNC);
	}
	/**
	* Restore voice to the channel.  Voice might have been removed from the channel
	* when sending a fax or doing other non voice operations.
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::restore_voice() {
		printDebug("restore_voice()...");
		IP_CONNECT ip_connect;
		GC_PARM_BLKP gc_parm_blkp = nullptr;
		SC_TSINFO sc_tsinfo;
		sc_tsinfo.sc_numts = 1;
		sc_tsinfo.sc_tsarrayp = &vox_xslot;
		gc_Listen(gc_dev, &sc_tsinfo, EV_SYNC);
		sc_tsinfo.sc_numts = 1;
		sc_tsinfo.sc_tsarrayp = &ip_xslot;
		dx_listen(vox_dev, &sc_tsinfo);
		ip_connect.version = 0x100;
		ip_connect.mediaHandle = ipm_dev;
		gc_util_insert_parm_ref(&gc_parm_blkp, IPSET_FOIP, IPPARM_T38_DISCONNECT, sizeof(IP_CONNECT), &ip_connect);
		gc_SetUserInfo(GCTGT_GCLIB_CRN, crn, gc_parm_blkp, GC_SINGLECALL);
		gc_util_delete_parm_blk(gc_parm_blkp);
	}
	/**
	* Set the channel to use DTMF for all calls.
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::set_dtmf() const
	{
		printDebug("set_dtmf()...");
		GC_PARM_BLKP parmblkp = nullptr;
		gc_util_insert_parm_val(&parmblkp, IPSET_DTMF, IPPARM_SUPPORT_DTMF_BITMASK,
			sizeof(char), IP_DTMF_TYPE_RFC_2833);
		gc_util_insert_parm_val(&parmblkp, IPSET_DTMF, IPPARM_DTMF_RFC2833_PAYLOAD_TYPE,
			sizeof(char), IP_USE_STANDARD_PAYLOADTYPE);
		gc_SetUserInfo(GCTGT_GCLIB_CHAN, gc_dev, parmblkp, GC_ALLCALLS);
		gc_util_delete_parm_blk(parmblkp);
	}

	/**
	* Set the channel to wait for a call Asyncronously.
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::wait_call() const
	{
		printDebug("wait_call()...");
		print_gc_error_info("gc_WaitCall", gc_WaitCall(gc_dev, nullptr, nullptr, 0, EV_ASYNC));
	}

	/**
	* Print call status infromation.
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::print_call_status(METAEVENT meta_evt) const
	{
		GC_INFO call_status_info = { 0 };
		gc_ResultInfo(&meta_evt, &call_status_info);
		printDebug("CALLSTATUS Info: \n GC InfoValue:0x%hx-%s,\n CCLibID:%i-%s, CC InfoValue:0x%lx-%s,\n Additional Info:%s",
			call_status_info.gcValue, call_status_info.gcMsg,
			call_status_info.ccLibId, call_status_info.ccLibName,
			call_status_info.ccValue, call_status_info.ccMsg,
			call_status_info.additionalInfo);
	}
	/**
	* Print error informaiton.  This information is commonly used for
	* debugging an error when an API function returns does not return
	* as SUCCESS (1).
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::print_gc_error_info(const char* func_name, int func_return) const
	{
		GC_INFO gc_error_info;
		if (GC_ERROR == func_return) {
			gc_ErrorInfo(&gc_error_info);
			printError("%s return %d, GC ErrorValue:0x%hx-%s,\n  CCLibID:%i-%s, CC ErrorValue:0x%lx-%s,\n  Additional Info:%s",
				func_name , func_return, gc_error_info.gcValue, gc_error_info.gcMsg,
				gc_error_info.ccLibId, gc_error_info.ccLibName,
				gc_error_info.ccValue, gc_error_info.ccMsg,
				gc_error_info.additionalInfo);
		}
	}
	/**
	* Acknowlage a call.
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::ack_call() const
	{
		GC_CALLACK_BLK gc_callack_blk;
		memset(&gc_callack_blk, 0, sizeof(GC_CALLACK_BLK));
		gc_callack_blk.type = GCACK_SERVICE_PROC;
		printDebug("ack_call()...");
		print_gc_error_info("gc_CallAck", gc_CallAck(crn, &gc_callack_blk, EV_ASYNC));
	}
	/**
	* Accept a call.
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::accept_call() const
	{
		printDebug("accept_call()...");
		print_gc_error_info("gc_AcceptCall", gc_AcceptCall(crn, 2, EV_ASYNC));
	}
	/**
	* Answer a call.
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::answer_call() const
	{
		printDebug("answer_call()...");
		set_codec(GCTGT_GCLIB_CRN);
		print_gc_error_info("gc_AnswerCall", gc_AnswerCall(crn, 2, EV_ASYNC));
	}
	/**
	* Make a call.
	* Please note,
	* The call header sets the USER_DISPLAY.
	* The USER_DISPLAY is blocekd by carriers (Fido, Telus, etc.)
	* The USER_DISPLAY can also be set using the PBX.
	* As far as I can tell invoking the gc_makecall functuion in SYNC mode is
	* not supported.  Dialogic has not been able to provide me with any examples
	* of this function working in SIP with SYNC mode.
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::make_call(const char* ani, const char* dnis) {
		printDebug("make_call(%s -> %s)...", ani, dnis);
		GC_PARM_BLKP gc_parm_blkp = nullptr;
		GC_MAKECALL_BLK gc_mk_blk;
		GCLIB_MAKECALL_BLK gclib_mk_blk = { 0 };
		gc_mk_blk.cclib = nullptr;
		gc_mk_blk.gclib = &gclib_mk_blk;

		strcpy(gc_mk_blk.gclib->origination.address, ani);
		gc_mk_blk.gclib->origination.address_type = GCADDRTYPE_TRANSPARENT;

		//char* call_from_address = "Developer1@10.143.102.42";

		char sip_header[1024] = "";
		sprintf(sip_header, "User-Agent: %s", USER_AGENT); //proprietary header
		gc_util_insert_parm_ref_ex(&gc_parm_blkp, IPSET_SIP_MSGINFO, IPPARM_SIP_HDR, static_cast<unsigned long>(strlen(sip_header) + 1), sip_header);

		sprintf(sip_header, "From: %s<sip:%s>", USER_DISPLAY, ani); //From header
		gc_util_insert_parm_ref_ex(&gc_parm_blkp, IPSET_SIP_MSGINFO, IPPARM_SIP_HDR, static_cast<unsigned long>(strlen(sip_header) + 1), sip_header);

		sprintf(sip_header, "Contact: %s<sip:%s:%d>", USER_DISPLAY, ani, HMP_SIP_PORT); //Contact header
		gc_util_insert_parm_ref_ex(&gc_parm_blkp, IPSET_SIP_MSGINFO, IPPARM_SIP_HDR, static_cast<unsigned long>(strlen(sip_header) + 1), sip_header);

		gc_SetUserInfo(GCTGT_GCLIB_CHAN, gc_dev, gc_parm_blkp, GC_SINGLECALL);
		gc_util_delete_parm_blk(gc_parm_blkp);
		/**/
		gc_util_insert_parm_val(&gc_parm_blkp, IPSET_PROTOCOL, IPPARM_PROTOCOL_BITMASK, sizeof(int), IP_PROTOCOL_SIP);

		gclib_mk_blk.ext_datap = gc_parm_blkp;
		set_codec(GCTGT_GCLIB_CHAN);
		print_gc_error_info("gc_MakeCall", gc_MakeCall(gc_dev, &crn, const_cast<char*>(dnis), &gc_mk_blk, 30, EV_ASYNC));
		gc_util_delete_parm_blk(gc_parm_blkp);
	}
	/**
	* Drop a call in progress.
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::drop_call() const
	{
		printDebug("drop_call()...");
		print_gc_error_info("gc_DropCall", gc_DropCall(crn, GC_NORMAL_CLEARING, EV_ASYNC));
	}
	/**
	* Release a call.
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::release_call() const
	{
		printDebug("release_call()...");
		print_gc_error_info("gc_ReleaseCallEx", gc_ReleaseCallEx(crn, EV_ASYNC));
	}
	/**
	* Play a wave file
	* @param file The file path to the wave file.
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::play_wave_file(const char* file) {
		printDebug("play_wave_file()...");
		tpt.tp_type = IO_EOT;
		tpt.tp_termno = DX_DIGMASK;
		tpt.tp_length = 0xFFFF; // any digit
		tpt.tp_flags = TF_DIGMASK;
		xpb.wFileFormat = FILE_FORMAT_WAVE;
		xpb.wDataFormat = DATA_FORMAT_MULAW;
		xpb.nSamplesPerSec = DRT_8KHZ;
		xpb.wBitsPerSample = 8;
		vox_iott.io_fhandle = dx_fileopen(file, _O_RDONLY | _O_BINARY);
		vox_iott.io_type = IO_DEV | IO_EOT;
		vox_iott.io_bufp = nullptr;
		vox_iott.io_offset = 0;
		vox_iott.io_length = -1;
		dx_clrdigbuf(vox_dev);
		dx_playiottdata(vox_dev, &vox_iott, &tpt, &xpb, EV_ASYNC);
	}
	/**
	* Record a wave file.
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::record_wave_file() {
		char file[MAX_PATH] = "";
		SYSTEMTIME t;
		printDebug("record_wave_file()...");
		tpt.tp_type = IO_EOT;
		tpt.tp_termno = DX_DIGMASK;
		tpt.tp_length = 0xFFFF; // any digit
		tpt.tp_flags = TF_DIGMASK;
		xpb.wFileFormat = FILE_FORMAT_WAVE;
		xpb.wDataFormat = DATA_FORMAT_MULAW;
		xpb.nSamplesPerSec = DRT_8KHZ;
		xpb.wBitsPerSample = 8;
		GetSystemTime(&t);
		sprintf(file, "record_wave_ch%d_timeD%02dH%02dM%02dS%02d.%04d.wav", _id, t.wDay, t.wHour, t.wMinute, t.wSecond, t.wMilliseconds);
		vox_iott.io_fhandle = dx_fileopen(file, _O_RDWR | _O_BINARY | _O_CREAT | _O_TRUNC, 0666);
		vox_iott.io_type = IO_DEV | IO_EOT;
		vox_iott.io_bufp = nullptr;
		vox_iott.io_offset = 0;
		vox_iott.io_length = -1;
		dx_clrdigbuf(vox_dev);
		dx_reciottdata(vox_dev, &vox_iott, &tpt, &xpb, EV_ASYNC | RM_TONE);
	}
	/**
	* Process a voice event terminition reason
	* Print the reason.
	* Clear the digit buffer.
	* Close the file handle.
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::process_voice_done() {
		print_voice_done_terminal_reason();
		dx_clrtpt(&tpt, 1);
		dx_fileclose(vox_iott.io_fhandle);
	}
	/**
	* Print the voice event termination reason.
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::print_voice_done_terminal_reason() const
	{
		if (const int term_reason = ATDX_TERMMSK(vox_dev); term_reason == TM_DIGIT)
			printDebug("print_voice_done_terminal_reason: TM_DIGIT");
		else if (term_reason == TM_EOD)
			printDebug("print_voice_done_terminal_reason: TM_EOD");
		else if (term_reason == TM_USRSTOP)
			printDebug("print_voice_done_terminal_reason: TM_USRSTOP");
		else
			printError("print_voice_done_terminal_reason: 0x%x", term_reason);
	}
	/**
	* Send a request to start audio .
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::send_audio_request() const
	{
		printDebug("send_audio_request()...");
		GC_PARM_BLKP gc_parm_blkp = nullptr;
		gc_util_insert_parm_ref(&gc_parm_blkp, IPSET_SWITCH_CODEC, IPPARM_AUDIO_INITIATE, sizeof(int), nullptr);
		gc_Extension(GCTGT_GCLIB_CRN, crn, IPEXTID_CHANGEMODE, gc_parm_blkp, nullptr, EV_ASYNC);
		gc_util_delete_parm_blk(gc_parm_blkp);
	}
	/**
	* Send a reques to start fax.
	* Please note that most fax features have been removed fromt he code.
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::send_t38_request() const
	{
		printDebug("send_t38_request()...");
		GC_PARM_BLKP gc_parm_blkp = nullptr;
		gc_util_insert_parm_ref(&gc_parm_blkp, IPSET_SWITCH_CODEC, IPPARM_T38_INITIATE, sizeof(int), nullptr);
		gc_Extension(GCTGT_GCLIB_CRN, crn, IPEXTID_CHANGEMODE, gc_parm_blkp, nullptr, EV_ASYNC);
		gc_util_delete_parm_blk(gc_parm_blkp);
	}
	/**
	* Process a codec request.
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::response_codec_request(BOOL accept_call) const
	{
		printDebug("response_codec_request(%s)...", accept_call ? "accept" : "reject");
		GC_PARM_BLKP gc_parm_blkp = nullptr;
		gc_util_insert_parm_val(&gc_parm_blkp, IPSET_SWITCH_CODEC, accept_call ? IPPARM_ACCEPT : IPPARM_REJECT, sizeof(int), NULL);
		gc_Extension(GCTGT_GCLIB_CRN, crn, IPEXTID_CHANGEMODE, gc_parm_blkp, nullptr, EV_ASYNC);
		gc_util_delete_parm_blk(gc_parm_blkp);
	}
	/**
	* stop a voice device.
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::stop() const
	{
		printInfo("stop()...");
		dx_stopch(vox_dev, EV_ASYNC);
	}
	/**
	* Process a metaevent extension block.
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::process_extension(METAEVENT meta_evt) const
	{
		const GC_PARM_BLKP gc_parm_blkp = &static_cast<EXTENSIONEVTBLK*>(meta_evt.extevtdatap)->parmblk;
		GC_PARM_DATA* parm_datap = nullptr;
		RTP_ADDR rtp_addr;
		struct in_addr ip_addr;

		while ((parm_datap = gc_util_next_parm(gc_parm_blkp, parm_datap))) {
			switch (parm_datap->set_ID) {
			case IPSET_SWITCH_CODEC:
				printDebug("IPSET_SWITCH_CODEC:");
				switch (parm_datap->parm_ID) {
				case IPPARM_AUDIO_REQUESTED:
					printDebug("  IPPARM_AUDIO_REQUESTED:");
					response_codec_request(TRUE);
					break;
				case IPPARM_READY:
					printDebug("  IPPARM_READY:");
					break;
				default:
					printError("  Got unknown extension parmID %d", parm_datap->parm_ID);
					break;
				}
				break;
			case IPSET_MEDIA_STATE:
				printDebug( "IPSET_MEDIA_STATE:");
				switch (parm_datap->parm_ID) {
				case IPPARM_TX_CONNECTED:
					printDebug( "  IPPARM_TX_CONNECTED");
					break;
				case IPPARM_TX_DISCONNECTED:
					printDebug("  IPPARM_TX_DISCONNECTED");
					break;
				case IPPARM_RX_CONNECTED:
					printDebug( "  IPPARM_RX_CONNECTED");
					break;
				case IPPARM_RX_DISCONNECTED:
					printDebug("  IPPARM_RX_DISCONNECTED");
					break;
				default:
					printError("  Got unknown extension parmID %d", parm_datap->parm_ID);
					break;
				}
				if (sizeof(IP_CAPABILITY) == parm_datap->value_size) {
					IP_CAPABILITY* ip_capp = reinterpret_cast<IP_CAPABILITY*>(parm_datap->value_buf);
					printDebug("    stream codec infomation for TX: capability(%d), dir(%d), frames_per_pkt(%d), VAD(%d)",
						ip_capp->capability , ip_capp->direction , ip_capp->extra.audio.frames_per_pkt , ip_capp->extra.audio.VAD);
				}
				break;
			case IPSET_IPPROTOCOL_STATE:
				printDebug("IPSET_IPPROTOCOL_STATE:");
				switch (parm_datap->parm_ID) {
				case IPPARM_SIGNALING_CONNECTED:
					printDebug("  IPPARM_SIGNALING_CONNECTED");
					break;
				case IPPARM_SIGNALING_DISCONNECTED:
					printDebug("  IPPARM_SIGNALING_DISCONNECTED");
					break;
				case IPPARM_CONTROL_CONNECTED:
					printDebug("  IPPARM_CONTROL_CONNECTED");
					break;
				case IPPARM_CONTROL_DISCONNECTED:
					printDebug("  IPPARM_CONTROL_DISCONNECTED");
					break;
				default:
					printError("  Got unknown extension parmID %d", parm_datap->parm_ID);
					break;
				}
				break;
			case IPSET_RTP_ADDRESS:
				printDebug( "IPSET_RTP_ADDRESS:");
				switch (parm_datap->parm_ID) {
				case IPPARM_LOCAL:
					memcpy(&rtp_addr, parm_datap->value_buf, parm_datap->value_size);
					ip_addr.S_un.S_addr = rtp_addr.u_ipaddr.ipv4;
					printDebug("  IPPARM_LOCAL: address:%s, port %d", inet_ntoa(ip_addr), rtp_addr.port);
					break;
				case IPPARM_REMOTE:
					memcpy(&rtp_addr, parm_datap->value_buf, parm_datap->value_size);
					ip_addr.S_un.S_addr = rtp_addr.u_ipaddr.ipv4;
					printDebug("  IPPARM_REMOTE: address:%s, port %d", inet_ntoa(ip_addr) , rtp_addr.port);
					break;
				default:
					printError("  Got unknown extension parmID %d", parm_datap->parm_ID);
					break;
				}
				break;
			default:
				printError("Got unknown set_ID(%d).", parm_datap->set_ID);
				break;
			}
		}
	}
	/**
	* set supported codecs.
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::set_codec(int crn_or_chan) const
	{
		printDebug("set_codec(g711Ulaw64k/t38UDPFax)...");
		IP_CAPABILITY ip_cap[3];
		ip_cap[0].capability = GCCAP_AUDIO_g711Ulaw64k;
		ip_cap[0].type = GCCAPTYPE_AUDIO;
		ip_cap[0].direction = IP_CAP_DIR_LCLTRANSMIT;
		ip_cap[0].payload_type = IP_USE_STANDARD_PAYLOADTYPE;
		ip_cap[0].extra.audio.frames_per_pkt = 20;
		ip_cap[0].extra.audio.VAD = GCPV_DISABLE;
		ip_cap[1].capability = GCCAP_AUDIO_g711Ulaw64k;
		ip_cap[1].type = GCCAPTYPE_AUDIO;
		ip_cap[1].direction = IP_CAP_DIR_LCLRECEIVE;
		ip_cap[1].payload_type = IP_USE_STANDARD_PAYLOADTYPE;
		ip_cap[1].extra.audio.frames_per_pkt = 20;
		ip_cap[1].extra.audio.VAD = GCPV_DISABLE;
		ip_cap[2].capability = GCCAP_DATA_t38UDPFax;
		ip_cap[2].type = GCCAPTYPE_RDATA;
		ip_cap[2].direction = IP_CAP_DIR_LCLTXRX;
		ip_cap[2].payload_type = 0;
		ip_cap[2].extra.data.max_bit_rate = 144;
		GC_PARM_BLKP parmblkp = nullptr;
		for (int i = 0; i < 3; i++)
			gc_util_insert_parm_ref(&parmblkp, GCSET_CHAN_CAPABILITY, IPPARM_LOCAL_CAPABILITY, sizeof(IP_CAPABILITY), &ip_cap[i]);
		if (GCTGT_GCLIB_CRN == crn_or_chan)
			gc_SetUserInfo(GCTGT_GCLIB_CRN, crn, parmblkp, GC_SINGLECALL);
		else
			gc_SetUserInfo(GCTGT_GCLIB_CHAN, gc_dev, parmblkp, GC_SINGLECALL);
		gc_util_delete_parm_blk(parmblkp);
	}

	/**
	* close the voice device and the global call device.
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::close() const
	{
		printInfo("close()...");
		dx_close(vox_dev);
		gc_Close(gc_dev);
	}

	/**
	* Print information.  All channel related prints statments should go through this print.
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::printDebug(const char* format, ...) const
	{
		char buf[1024] = "";
		va_list argptr;
		va_start(argptr, format);
		_vsnprintf_s(buf, 1023, format, argptr);
		buf[1023] = '\0';
		va_end(argptr);

		BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %d: %s") % _id % buf;
	}

	/**
	* Print information.  All channel related prints statments should go through this print.
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::printInfo(const char* format, ...) const
	{
		char buf[1024] = "";
		va_list argptr;
		va_start(argptr, format);
		_vsnprintf_s(buf, 1023, format, argptr);
		buf[1023] = '\0';
		va_end(argptr);

		BOOST_LOG_TRIVIAL(info) << boost::format("Channel %d: %s") % _id % buf;
	}

	/**
	* Print information.  All channel related prints statments should go through this print.
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::printError(const char* format, ...) const
	{
		char buf[1024] = "";
		va_list argptr;
		va_start(argptr, format);
		_vsnprintf_s(buf, 1023, format, argptr);
		buf[1023] = '\0';
		va_end(argptr);

		BOOST_LOG_TRIVIAL(error) << boost::format("Channel %d: %s") % _id % buf;
	}

	/**
	* Print information.  All channel related prints statments should go through this print.
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::printTrace(const char* format, ...) const
	{
		char buf[1024] = "";
		va_list argptr;
		va_start(argptr, format);
		_vsnprintf_s(buf, 1023, format, argptr);
		buf[1023] = '\0';
		va_end(argptr);

		BOOST_LOG_TRIVIAL(trace) << boost::format("Channel %d: %s") % _id % buf;
	}

/**
	* Start Dialogic Call Progress Analysis
	*/
	int ivrToolkit::DialogicSipWrapper::CHANNEL::start_call_progress_analysis() const
	{
		DX_CAP cap; // Voice call analysis & call progress structure
		dx_clrcap(&cap);

		cap.ca_dtn_pres = 100;
		cap.ca_dtn_npres = 300;
		cap.ca_dtn_deboff = 10;
		cap.ca_noanswer = 3000;
		cap.ca_intflg = 4;
		cap.ca_ansrdgl = 50;
		cap.ca_hedge = 2;
		cap.ca_maxansr = 1000;

		cap.ca_intflg = DX_PAMDOPTEN;
		cap.ca_pamd_spdval = PAMD_QUICK;
		return dx_dial(vox_dev, "", &cap, DX_CALLP | EV_SYNC);
	}

	/**
	* Check the Call Progress Analysis Result.
	*/
	void ivrToolkit::DialogicSipWrapper::CHANNEL::process_cpa_result(int cpa_result) const
	{
		printDebug("cpa_result: %i", cpa_result);
		switch (cpa_result) {
		case CR_BUSY:
			printDebug("Call progress analysis: CR_BUSY");
			break;
		case CR_CEPT:
			printDebug("Call progress analysis: CR_CEPT");
			break;
		case CR_CNCT:
			printDebug("Call progress analysis: CR_CNCT");
			//int cpa_cnct_type = ATDX_CONNTYPE(vox_dev);
			switch (ATDX_CONNTYPE(vox_dev)) {
			case CON_CAD:
				printDebug("Call connection type: CON_CAD");
				break;
			case CON_DIGITAL:
				printDebug("Call connection type: CON_DIGITAL");
				break;
			case CON_LPC:
				printDebug("Call connection type: CON_LPC");
				break;
			case CON_PAMD:
				printDebug("Call connection type: CON_PAMD");
				break;
			case CON_PVD:
				/*
				* If someone delibarly hangs up the call is still considered connected and then hung up.
				*/
				printDebug("Call connection type: CON_PVD");
				break;
			default: 
				break;
			}
			break;
		case CR_ERROR:
			printDebug("Call progress analysis: CR_ERROR");
			break;
		case CR_FAXTONE:
			printDebug("Call progress analysis: CR_FAXTONE");
			break;
		case CR_NOANS:
			printDebug("Call progress analysis: CR_NOANS");
			break;
		case CR_NORB:
			printDebug("Call progress analysis: CR_NORB");
			break;
		case CR_STOPD:
			printDebug("Call progress analysis: CR_STOPD");
			break;
		default: 
			break;
		}
	}

	/**
	* Gets the greeting time in milliseconds.
	* This might be used to check if an answering maching
	* answer the call instead of a person.
	*/
	int ivrToolkit::DialogicSipWrapper::CHANNEL::get_salutation_length() const
	{
		const int result = ATDX_ANSRSIZ(vox_dev);
		return result * 10;
	}
	/**
	* Invoke the Dialogic Voice API dx_deltones function via the Channel.
	*/
	int ivrToolkit::DialogicSipWrapper::CHANNEL::voice_dx_deltones() const
	{
		return dx_deltones(vox_dev);
	}
	/**
	* Invoke the Dialogic Voice API dx_chgfreq function via the Channel.
	*/
	int ivrToolkit::DialogicSipWrapper::CHANNEL::voice_dx_chgfreq(int tonetype, int fq1, int dv1, int fq2, int dv2) {
		return dx_chgfreq(tonetype, fq1, dv1, fq2, dv2);
	}
	/**
	* Invoke the Dialogic Voice API dx_chgdur function via the Channel.
	*/
	int ivrToolkit::DialogicSipWrapper::CHANNEL::voice_dx_chgdur(int typetype, int on, int ondv, int off, int offdv) {
		return dx_chgdur(typetype, on, ondv, off, offdv);
	}

	/**
	* Invoke the Dialogic Voice API dx_chgrepcnt function via the Channel.
	*/
	int ivrToolkit::DialogicSipWrapper::CHANNEL::voice_dx_chgrepcnt(int tonetype, int repcount) {
		return dx_chgrepcnt(tonetype, repcount);
	}
	/*
	* Invoke the Dialogic Voice API dx_getdig function via the Channel.
	* This is a sample function and is not ready for a real program to use.
	*/
	int ivrToolkit::DialogicSipWrapper::CHANNEL::voice_dx_getdig() const
	{
		DV_DIGIT dig;
		DV_TPT tpt;
		/* set up TPT to wait for 3 digits and terminate */
		dx_clrtpt(&tpt, 1);
		tpt.tp_type = IO_EOT;
		tpt.tp_termno = DX_MAXDTMF;
		tpt.tp_length = 3;
		tpt.tp_flags = TF_MAXDTMF;

		/* enable DPD and DTMF digits */
		dx_setdigtyp(vox_dev, D_DPDZ | D_DTMF);
		/* clear the digit buffer */
		dx_clrdigbuf(vox_dev);
		/* collect 3 digits from the user */
		if (dx_getdig(vox_dev, &tpt, &dig, EV_SYNC) == -1) {
			/* error, display error message */
			printDebug("dx_getdig error %d, %s", ATDV_LASTERR(vox_dev) , ATDV_ERRMSGP(vox_dev));
		}
		else {
			/* display digits received and digit type */
			printDebug("Received \"%s\"", dig.dg_value);
			/*
			* digit types have 0x30 ORed with them strip it off
			* so that we can use the DG_xxx equates from the header files
			*/
			switch (dig.dg_type[0] & 0x000f) {
			case DG_DTMF:
				printDebug("Digit type is DTMF");
				break;
			case DG_DPD:
				printDebug("Digit type is DPD");
				break;
			default:
				printError("Digit type is Unknown, %d", dig.dg_type[0] & 0x000f);
			}
		}
		/*
		* continue processing call
		*/
		return 1;
	}

	/**
	* Invoke the Dialogic Global Call API gc_GetCallState function via the Channel.
	*/
	int ivrToolkit::DialogicSipWrapper::CHANNEL::globalcall_gc_GetCallState() const
	{
		int call_state; /* current state of call */
		print_gc_error_info("gc_GetCallState", gc_GetCallState(crn, &call_state));
		return call_state;
	}

