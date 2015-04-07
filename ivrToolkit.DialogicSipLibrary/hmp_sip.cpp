// hmp_sip.cpp : Defines the entry point for the application.
//
/**
* hmp_sip provides a number of fucntions to easily open a HMP device,
* make a call, wait for a call, and various other call features.
* 
* Features have been broken down into three catagories
* Syncronous : Do not use with Asyncronous features
* Asyncronous : Do not use with Syncronous features
* Shared : Shared between Syncronous and Asyncronous features
*
* The Syncronous features were only created to support the syncronous
* calls made by the IVR toolkit.  Dialogic does not recommend using the 
* global call API in syncronous (SYNC) mode.  Some Dialogic functions are 
* not supported at all in (SYNC) mode while other will have issues if you 
* 10-20 channels.  In almost all cases the global call API invocations 
* are made using (ASYNC) mode and the events are wrapped by another 
* function so that they return syncronously even though the Dialogic 
* functions where invoked aysncronously.
*
* Dialogic recommends using its Asyncronous features as it reduces 
* CPU overhead, memory overhead, threading problems, etc.
*
* Please note that this code originally had feature for fax as well and 
* this has been removed to decrease code complexity.
*
* Original code provide by Dialogic as an example of how to use their APIs
*
*/

#pragma once
#include <stdio.h>
#include <conio.h>
#include <fcntl.h>
#include <process.h>

#include <srllib.h>
#include <dxxxlib.h>
#include <dtilib.h>
#include <msilib.h>
#include <dcblib.h>
//#include <scxlib.h>
#include <gcip_defs.h>
#include <gcip.h>
#include "hmp_sip.h"

/**
* The CHANNEL class provides all channel realted functions.
* It holds onto the required handles so that the consumming application
* does not have to hold onto the global call device handle (gc_dev), or 
* the voice handle (vox_dev).
*
* An instance of channel must have an index that starts at 1 
* If holding an instance of channel in an array the index of 
* the array and the channel number will always be off by 1.
*/
//void authentication(const char* proxy_ip, const char* alias, const char* password, const char* realm);

class CHANNEL{
public:
	long gc_dev;
	int vox_dev;
	int ipm_dev;

	long ip_xslot;
	long vox_xslot;

	DV_TPT tpt;
	DX_XPB xpb;
	DX_IOTT vox_iott;

	CRN crn;

	int id;
	BOOL fax_proceeding;
	BOOL already_connect_fax;
	char device_name[64];

	CHANNEL(int index) { id = index; }
	/**
	* Open the Voice Device
	* Open the Global Call Device
	*/
	void open() {
		print("open()...");
		int board_id = ((id - 1) / 4) + 1;
		int channel_id = (id - ((board_id - 1) * 4));
		long request_id = 0;
		GC_PARM_BLKP gc_parm_blkp = NULL;
		char dev_name[64] = "";
		sprintf(dev_name, "dxxxB%dC%d",board_id, channel_id);
		vox_dev = dx_open(dev_name, NULL);
		dx_setevtmsk(vox_dev, DM_RINGS | DM_DIGITS | DM_LCOF);
		//sprintf(dev_name, "dxxxB2C%d", id);
		sprintf(dev_name, ":N_iptB%dT%d:P_SIP:M_ipmB%dC%d", board_id, channel_id, board_id, channel_id);
		sprintf(device_name, dev_name);
		print(dev_name);
		print_gc_error_info("gc_OpenEx",gc_OpenEx(&gc_dev, dev_name, EV_ASYNC, (void*)this));

		//Enabling GCEV_INVOKE_XFER_ACCEPTED Events
		gc_util_insert_parm_val(&gc_parm_blkp, GCSET_CALLEVENT_MSK, GCACT_ADDMSK, sizeof(long), GCMSK_INVOKEXFER_ACCEPTED);
		gc_SetConfigData(GCTGT_GCLIB_CHAN, gc_dev, gc_parm_blkp, 0, GCUPDATE_IMMEDIATE, &request_id, EV_SYNC);
		gc_util_delete_parm_blk(gc_parm_blkp);
		print("end of open()...");
	}
	/**
	* Get the global call device name, no checks are made if the device has been opened.
	*/
	char* get_device_name(){
		return device_name;
	}
	/**
	* Get the global call device handle, no checks are made if the device has been opened.
	*/
	long get_device_handle(){
		return gc_dev;
	}
	/**
	* Get the voice device handle, no checks are made if the device has been opened.
	*/
	int get_voice_handle(){
		return vox_dev;
	}
	/**
	* Get the media device handle, no checks are made if the device has been opened.
	*/
	int get_media_device_handle(){
		return ipm_dev;
	}
	/**
	* Connect voice to the channel.
	*/
	void connect_voice() {
		print("connect_voice()...");
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
	void restore_voice() {
		print("restore_voice()...");
		IP_CONNECT ip_connect;
		GC_PARM_BLKP gc_parm_blkp = NULL;
		SC_TSINFO sc_tsinfo;
		sc_tsinfo.sc_numts = 1;
		sc_tsinfo.sc_tsarrayp = &vox_xslot;
		gc_Listen(gc_dev, &sc_tsinfo, EV_SYNC);
		sc_tsinfo.sc_numts = 1;
		sc_tsinfo.sc_tsarrayp = &ip_xslot;
		dx_listen(vox_dev, &sc_tsinfo);
		ip_connect.version = 0x100;
		ip_connect.mediaHandle = ipm_dev;
		gc_util_insert_parm_ref(&gc_parm_blkp, IPSET_FOIP, IPPARM_T38_DISCONNECT, sizeof(IP_CONNECT), (void*)(&ip_connect));
		gc_SetUserInfo(GCTGT_GCLIB_CRN, crn, gc_parm_blkp, GC_SINGLECALL);
		gc_util_delete_parm_blk(gc_parm_blkp);
	}
	/**
	* Set the channel to use DTMF for all calls.
	*/
	void set_dtmf() {
		print("set_dtmf()...");
		GC_PARM_BLKP parmblkp = NULL;
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
	void wait_call() {
		print("wait_call()...");
		print_gc_error_info("gc_WaitCall", gc_WaitCall(gc_dev, NULL, NULL, 0, EV_ASYNC));
	}
	/**
	* Print informaiton on an incomming call.
	*/
	void print_offer_info(METAEVENT meta_evt) {
		char ani[255] = "";
		char dnis[255] = "";
		int protocol = CALLPROTOCOL_H323;
		GC_PARM_BLKP parm_blkp = &(((EXTENSIONEVTBLK*)(meta_evt.extevtdatap))->parmblk);
		GC_PARM_DATAP parm_datap = NULL;
		CRN secondary_crn = 0;
		char transferring_addr[GC_ADDRSIZE] = "";

		gc_GetCallInfo(crn, ORIGINATION_ADDRESS, ani);
		gc_GetCallInfo(crn, DESTINATION_ADDRESS, dnis);
		gc_GetCallInfo(crn, CALLPROTOCOL, (char*)&protocol);
		print("number %s, got %s offer from %s",
			dnis, protocol == CALLPROTOCOL_H323 ? "H323" : "SIP", ani);

		while (parm_datap = gc_util_next_parm(parm_blkp, parm_datap)) {
			switch (parm_datap->parm_ID) {
			case GCPARM_SECONDARYCALL_CRN:
				memcpy(&secondary_crn, parm_datap->value_buf, parm_datap->value_size);
				print("GCPARM_SECONDARYCALL_CRN: 0x%x", secondary_crn);
				break;
			case GCPARM_TRANSFERRING_ADDR:
				memcpy(transferring_addr, parm_datap->value_buf, parm_datap->value_size);
				print("GCPARM_TRANSFERRING_ADDR: %s", transferring_addr);
				break;
			default:
				break;
			}
		}
	}
	/**
	* Print call status infromation.
	*/
	void print_call_status(METAEVENT meta_evt) {
		GC_INFO call_status_info = { 0 };
		gc_ResultInfo(&meta_evt, &call_status_info);
		print("CALLSTATUS Info: \n GC InfoValue:0x%hx-%s,\n CCLibID:%i-%s, CC InfoValue:0x%lx-%s,\n Additional Info:%s",
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
	void print_gc_error_info(const char *func_name, int func_return) {
		GC_INFO gc_error_info;
		if (GC_ERROR == func_return) {
			gc_ErrorInfo(&gc_error_info);
			print("%s return %d, GC ErrorValue:0x%hx-%s,\n  CCLibID:%i-%s, CC ErrorValue:0x%lx-%s,\n  Additional Info:%s",
				func_name, func_return, gc_error_info.gcValue, gc_error_info.gcMsg,
				gc_error_info.ccLibId, gc_error_info.ccLibName,
				gc_error_info.ccValue, gc_error_info.ccMsg,
				gc_error_info.additionalInfo);
		}
	}
	/**
	* Acknowlage a call.
	*/
	void ack_call() {
		GC_CALLACK_BLK gc_callack_blk;
		memset(&gc_callack_blk, 0, sizeof(GC_CALLACK_BLK));
		gc_callack_blk.type = GCACK_SERVICE_PROC;
		print("ack_call()...");
		print_gc_error_info("gc_CallAck", gc_CallAck(crn, &gc_callack_blk, EV_ASYNC));
	}
	/**
	* Accept a call.
	*/
	void accept_call() {
		print("accept_call()...");
		print_gc_error_info("gc_AcceptCall", gc_AcceptCall(crn, 2, EV_ASYNC));
	}
	/**
	* Answer a call.
	*/
	void answer_call() {
		print("answer_call()...");
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
	void make_call(const char* ani, const char* dnis) {
		print("make_call(%s -> %s)...", ani, dnis);
		GC_PARM_BLKP gc_parm_blkp = NULL;
		GC_MAKECALL_BLK gc_mk_blk;
		GCLIB_MAKECALL_BLK gclib_mk_blk = { 0 };
		gc_mk_blk.cclib = NULL;
		gc_mk_blk.gclib = &gclib_mk_blk;

		/*Added Authentication */
		/*
		const char *proxy_ip = "10.143.102.42";
		const char *alias = "Developer1";
		const char *password = "password";
		const char *realm = "";
		authentication(proxy_ip, alias, password, realm);
		*/

		strcpy(gc_mk_blk.gclib->origination.address, ani);
		gc_mk_blk.gclib->origination.address_type = GCADDRTYPE_TRANSPARENT;

		//char* call_from_address = "Developer1@10.143.102.42";

		char sip_header[1024] = "";
		sprintf(sip_header, "User-Agent: %s", USER_AGENT); //proprietary header
		gc_util_insert_parm_ref_ex(&gc_parm_blkp, IPSET_SIP_MSGINFO, IPPARM_SIP_HDR, (unsigned long)(strlen(sip_header) + 1), sip_header);

		sprintf(sip_header, "From: %s<sip:%s>", USER_DISPLAY, ani); //From header
		gc_util_insert_parm_ref_ex(&gc_parm_blkp, IPSET_SIP_MSGINFO, IPPARM_SIP_HDR, (unsigned long)(strlen(sip_header) + 1), sip_header);

		sprintf(sip_header, "Contact: %s<sip:%s:%d>", USER_DISPLAY, ani, HMP_SIP_PORT); //Contact header
		gc_util_insert_parm_ref_ex(&gc_parm_blkp, IPSET_SIP_MSGINFO, IPPARM_SIP_HDR, (unsigned long)(strlen(sip_header) + 1), sip_header);

		gc_SetUserInfo(GCTGT_GCLIB_CHAN, gc_dev, gc_parm_blkp, GC_SINGLECALL);
		gc_util_delete_parm_blk(gc_parm_blkp);
		/**/
		gc_util_insert_parm_val(&gc_parm_blkp, IPSET_PROTOCOL, IPPARM_PROTOCOL_BITMASK, sizeof(int), IP_PROTOCOL_SIP);

		gclib_mk_blk.ext_datap = gc_parm_blkp;
		set_codec(GCTGT_GCLIB_CHAN);
		print_gc_error_info("gc_MakeCall", gc_MakeCall(gc_dev, &crn, (char*)dnis, &gc_mk_blk, 30, EV_ASYNC));
		gc_util_delete_parm_blk(gc_parm_blkp);
	}
	/**
	* Drop a call in progress.
	*/
	void drop_call() {
		print("drop_call()...");
		if (already_connect_fax)
			restore_voice();
		print_gc_error_info("gc_DropCall", gc_DropCall(crn, GC_NORMAL_CLEARING, EV_ASYNC));
	}
	/**
	* Release a call.
	*/
	void release_call() {
		print("release_call()...");
		print_gc_error_info("gc_ReleaseCallEx", gc_ReleaseCallEx(crn, EV_ASYNC));
	}
	/**
	* Play a wave file
	* @param file The file path to the wave file.
	*/
	void play_wave_file(const char* file) {
		print("play_wave_file()...");
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
		vox_iott.io_bufp = 0;
		vox_iott.io_offset = 0;
		vox_iott.io_length = -1;
		dx_clrdigbuf(vox_dev);
		dx_playiottdata(vox_dev, &vox_iott, &tpt, &xpb, EV_ASYNC);
	}
	/**
	* Record a wave file.
	*/
	void record_wave_file() {
		char file[MAX_PATH] = "";
		SYSTEMTIME t;
		print("record_wave_file()...");
		tpt.tp_type = IO_EOT;
		tpt.tp_termno = DX_DIGMASK;
		tpt.tp_length = 0xFFFF; // any digit
		tpt.tp_flags = TF_DIGMASK;
		xpb.wFileFormat = FILE_FORMAT_WAVE;
		xpb.wDataFormat = DATA_FORMAT_MULAW;
		xpb.nSamplesPerSec = DRT_8KHZ;
		xpb.wBitsPerSample = 8;
		GetSystemTime(&t);
		sprintf(file, "record_wave_ch%d_timeD%02dH%02dM%02dS%02d.%04d.wav", id, t.wDay, t.wHour, t.wMinute, t.wSecond, t.wMilliseconds);
		vox_iott.io_fhandle = dx_fileopen(file, _O_RDWR | _O_BINARY | _O_CREAT | _O_TRUNC, 0666);
		vox_iott.io_type = IO_DEV | IO_EOT;
		vox_iott.io_bufp = 0;
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
	void process_voice_done() {
		print_voice_done_terminal_reason();
		dx_clrtpt(&tpt, 1);
		dx_fileclose(vox_iott.io_fhandle);
	}
	/**
	* Print the voice event termination reason.
	*/
	void print_voice_done_terminal_reason() {
		int term_reason = ATDX_TERMMSK(vox_dev);
		if (TM_DIGIT == term_reason)
			print("print_voice_done_terminal_reason: TM_DIGIT");
		else if (TM_EOD == term_reason)
			print("print_voice_done_terminal_reason: TM_EOD");
		else if (TM_USRSTOP == term_reason)
			print("print_voice_done_terminal_reason: TM_USRSTOP");
		else
			print("print_voice_done_terminal_reason: 0x%x", term_reason);
	}
	/**
	* Send a request to start audio .
	*/
	void send_audio_request() {
		print("send_audio_request()...");
		GC_PARM_BLKP gc_parm_blkp = NULL;
		gc_util_insert_parm_ref(&gc_parm_blkp, IPSET_SWITCH_CODEC, IPPARM_AUDIO_INITIATE, sizeof(int), NULL);
		gc_Extension(GCTGT_GCLIB_CRN, crn, IPEXTID_CHANGEMODE, gc_parm_blkp, NULL, EV_ASYNC);
		gc_util_delete_parm_blk(gc_parm_blkp);
	}
	/**
	* Send a reques to start fax.
	* Please note that most fax features have been removed fromt he code.
	*/
	void send_t38_request() {
		print("send_t38_request()...");
		GC_PARM_BLKP gc_parm_blkp = NULL;
		gc_util_insert_parm_ref(&gc_parm_blkp, IPSET_SWITCH_CODEC, IPPARM_T38_INITIATE, sizeof(int), NULL);
		gc_Extension(GCTGT_GCLIB_CRN, crn, IPEXTID_CHANGEMODE, gc_parm_blkp, NULL, EV_ASYNC);
		gc_util_delete_parm_blk(gc_parm_blkp);
	}
	/**
	* Process a codec request.
	*/
	void response_codec_request(BOOL accept_call) {
		print("response_codec_request(%s)...", accept_call ? "accept" : "reject");
		GC_PARM_BLKP gc_parm_blkp = NULL;
		gc_util_insert_parm_val(&gc_parm_blkp, IPSET_SWITCH_CODEC, accept_call ? IPPARM_ACCEPT : IPPARM_REJECT, sizeof(int), NULL);
		gc_Extension(GCTGT_GCLIB_CRN, crn, IPEXTID_CHANGEMODE, gc_parm_blkp, NULL, EV_ASYNC);
		gc_util_delete_parm_blk(gc_parm_blkp);
	}
	/**
	* stop a voice device.
	*/
	void stop() {
		print("stop()...");
		dx_stopch(vox_dev, EV_ASYNC);
	}
	/**
	* Process a metaevent extension block.
	*/
	void process_extension(METAEVENT meta_evt) {
		GC_PARM_BLKP gc_parm_blkp = &(((EXTENSIONEVTBLK*)(meta_evt.extevtdatap))->parmblk);
		GC_PARM_DATA* parm_datap = NULL;
		IP_CAPABILITY* ip_capp = NULL;
		RTP_ADDR rtp_addr;
		struct in_addr ip_addr;

		while (parm_datap = gc_util_next_parm(gc_parm_blkp, parm_datap)) {
			switch (parm_datap->set_ID) {
			case IPSET_SWITCH_CODEC:
				print("IPSET_SWITCH_CODEC:");
				switch (parm_datap->parm_ID) {
				case IPPARM_AUDIO_REQUESTED:
					print("  IPPARM_AUDIO_REQUESTED:");
					response_codec_request(TRUE);
					break;
				case IPPARM_READY:
					print("  IPPARM_READY:");
					break;
				default:
					print("  Got unknown extension parmID %d", parm_datap->parm_ID);
					break;
				}
				break;
			case IPSET_MEDIA_STATE:
				print("IPSET_MEDIA_STATE:");
				switch (parm_datap->parm_ID) {
				case IPPARM_TX_CONNECTED:
					print("  IPPARM_TX_CONNECTED");
					break;
				case IPPARM_TX_DISCONNECTED:
					print("  IPPARM_TX_DISCONNECTED");
					break;
				case IPPARM_RX_CONNECTED:
					print("  IPPARM_RX_CONNECTED");
					break;
				case IPPARM_RX_DISCONNECTED:
					print("  IPPARM_RX_DISCONNECTED");
					break;
				default:
					print("  Got unknown extension parmID %d", parm_datap->parm_ID);
					break;
				}
				if (sizeof(IP_CAPABILITY) == parm_datap->value_size) {
					ip_capp = (IP_CAPABILITY*)(parm_datap->value_buf);
					print("    stream codec infomation for TX: capability(%d), dir(%d), frames_per_pkt(%d), VAD(%d)",
						ip_capp->capability, ip_capp->direction, ip_capp->extra.audio.frames_per_pkt, ip_capp->extra.audio.VAD);
				}
				break;
			case IPSET_IPPROTOCOL_STATE:
				print("IPSET_IPPROTOCOL_STATE:");
				switch (parm_datap->parm_ID) {
				case IPPARM_SIGNALING_CONNECTED:
					print("  IPPARM_SIGNALING_CONNECTED");
					break;
				case IPPARM_SIGNALING_DISCONNECTED:
					print("  IPPARM_SIGNALING_DISCONNECTED");
					break;
				case IPPARM_CONTROL_CONNECTED:
					print("  IPPARM_CONTROL_CONNECTED");
					break;
				case IPPARM_CONTROL_DISCONNECTED:
					print("  IPPARM_CONTROL_DISCONNECTED");
					break;
				default:
					print("  Got unknown extension parmID %d", parm_datap->parm_ID);
					break;
				}
				break;
			case IPSET_RTP_ADDRESS:
				print("IPSET_RTP_ADDRESS:");
				switch (parm_datap->parm_ID) {
				case IPPARM_LOCAL:
					memcpy(&rtp_addr, parm_datap->value_buf, parm_datap->value_size);
					ip_addr.S_un.S_addr = rtp_addr.u_ipaddr.ipv4;
					print("  IPPARM_LOCAL: address:%s, port %d", inet_ntoa(ip_addr), rtp_addr.port);
					break;
				case IPPARM_REMOTE:
					memcpy(&rtp_addr, parm_datap->value_buf, parm_datap->value_size);
					ip_addr.S_un.S_addr = rtp_addr.u_ipaddr.ipv4;
					print("  IPPARM_REMOTE: address:%s, port %d", inet_ntoa(ip_addr), rtp_addr.port);
					break;
				default:
					print("  Got unknown extension parmID %d", parm_datap->parm_ID);
					break;
				}
				break;
			default:
				print("Got unknown set_ID(%d).", parm_datap->set_ID);
				break;
			}
		}
	}
	/**
	* set supported codecs.
	*/
	void set_codec(int crn_or_chan) {
		print("set_codec(g711Ulaw64k/t38UDPFax)...");
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
		GC_PARM_BLKP parmblkp = NULL;
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
	void close() {
		print("close()...");
		dx_close(vox_dev);
		gc_Close(gc_dev);
	}
	/**
	* Print information.  All channel related prints statments should go through this print.
	*/
	void print(const char *format, ...) {
		char buf[1024] = "";
		SYSTEMTIME t;
		va_list argptr;
		va_start(argptr, format);
		_vsnprintf(buf, 1023, format, argptr);
		buf[1023] = '\0';
		va_end(argptr);
		GetSystemTime(&t);
		printf("%02d:%02d:%02d.%04d CH %d: %s\n", t.wHour, t.wMinute, t.wSecond, t.wMilliseconds, id, buf);
	}

	/**
	* Start Dialogic Call Progress Analysis
	*/
	int start_call_progress_analysis(){
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
	void process_cpa_result(int cpa_result){
		//if (cpa_result != 0)
		//{
		//	printf("Error during start_call_progress_analysis\n");
		//	return;
		// handle error
		//}
		print("cpa_result: %i\n", cpa_result);
		switch (cpa_result){
		case CR_BUSY:
			print("Call progress analysis: CR_BUSY\n");
			break;
		case CR_CEPT:
			print("Call progress analysis: CR_CEPT\n");
			break;
		case CR_CNCT:
			print("Call progress analysis: CR_CNCT\n");
			//int cpa_cnct_type = ATDX_CONNTYPE(vox_dev);
			switch (ATDX_CONNTYPE(vox_dev)){
			case CON_CAD:
				print("Call connection type: CON_CAD\n");
				break;
			case CON_DIGITAL:
				print("Call connection type: CON_DIGITAL\n");
				break;
			case CON_LPC:
				print("Call connection type: CON_LPC\n");
				break;
			case CON_PAMD:
				print("Call connection type: CON_PAMD\n");
				break;
			case CON_PVD:
				/*
				* If someone delibarly hangs up the call is still considered connected and then hung up.  
				*/
				print("Call connection type: CON_PVD\n");
				break;
			}
			break;
		case CR_ERROR:
			print("Call progress analysis: CR_ERROR\n");
			break;
		case CR_FAXTONE:
			print("Call progress analysis: CR_FAXTONE\n");
			break;
		case CR_NOANS:
			print("Call progress analysis: CR_NOANS\n");
			break;
		case CR_NORB:
			print("Call progress analysis: CR_NORB\n");
			break;
		case CR_STOPD:
			print("Call progress analysis: CR_STOPD\n");
			break;
		}
	}

	/**
	* Gets the greeting time in milliseconds.
	* This might be used to check if an answering maching 
	* answer the call instead of a person.
	*/
	int get_salutation_length()
	{
		int result = ATDX_ANSRSIZ(vox_dev);
		return result * 10;
	}
	/**
	* Invoke the Dialogic Voice API dx_deltones function via the Channel.
	*/
	int voice_dx_deltones(){
		return dx_deltones(vox_dev);
	}
	/**
	* Invoke the Dialogic Voice API dx_chgfreq function via the Channel.
	*/
	int voice_dx_chgfreq(int tonetype, int fq1, int dv1, int fq2, int dv2){
		return dx_chgfreq(tonetype, fq1, dv1, fq2, dv2);
	}
	/**
	* Invoke the Dialogic Voice API dx_chgdur function via the Channel.
	*/
	int voice_dx_chgdur(int typetype, int on, int ondv, int off, int offdv){
		return dx_chgdur(typetype, on, ondv, off, offdv);
	}

	/**
	* Invoke the Dialogic Voice API dx_chgrepcnt function via the Channel.
	*/
	int voice_dx_chgrepcnt(int tonetype, int repcount){
		return dx_chgrepcnt(tonetype, repcount);
	}
	/*
	* Invoke the Dialogic Voice API dx_getdig function via the Channel.
	* This is a sample function and is not ready for a real program to use. 
	*/
	int voice_dx_getdig(){
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
			printf("dx_getdig error %d, %s\n", ATDV_LASTERR(vox_dev), ATDV_ERRMSGP(vox_dev));
		}
		else {
			/* display digits received and digit type */
			printf("Received \"%s\"\n", dig.dg_value);
			printf("Digit type is ");
			/*
			* digit types have 0x30 ORed with them strip it off
			* so that we can use the DG_xxx equates from the header files
			*/
			switch ((dig.dg_type[0] & 0x000f)) {
			case DG_DTMF:
				printf("DTMF\n");
				break;
			case DG_DPD:
				printf("DPD\n");
				break;
			default:
				printf("Unknown, %d\n", (dig.dg_type[0] & 0x000f));
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
	int globalcall_gc_GetCallState(){
		int call_state; /* current state of call */
		print_gc_error_info("gc_GetCallState", gc_GetCallState(crn, &call_state));
		return call_state;
	}
};

long board_dev = 0; //Board Device Handle
BOOL registered = FALSE; //Registered with PBX
CHANNEL* channls[MAX_CHANNELS] = { 0 };  //A channel array 
bool exitFlag = false;				// Process SRL events until ExitFlag = TRUE
HANDLE hThread; //The thread that is used to process events asyncronously.
BOOL started = FALSE;

/*Authentication variables*/
const char* auth_proxy_ip;
const char* auth_alias;
const char* auth_password;
const char* auth_realm;

/**
* Print error informaiton.  This information is commonly used for
* debugging an error when an API function returns does not return
* as SUCCESS (1).
*/
void print_gc_error_info(const char *func_name, int func_return) {
	GC_INFO gc_error_info;
	if (GC_ERROR == func_return) {
		gc_ErrorInfo(&gc_error_info);
		printf("%s return %d, GC ErrorValue:0x%hx-%s,\n  CCLibID:%i-%s, CC ErrorValue:0x%lx-%s,\n  Additional Info:%s",
			func_name, func_return, gc_error_info.gcValue, gc_error_info.gcMsg,
			gc_error_info.ccLibId, gc_error_info.ccLibName,
			gc_error_info.ccValue, gc_error_info.ccMsg,
			gc_error_info.additionalInfo);
	}
}

/**
* Enumerate the HMP Device
* This can be used to check the device name and numbers
* That are part of an HMP License.
* This function can help when a channel cannot be opened possibly due to
* the name or numbers on the license.
*/
void enum_dev_information()
{
	int i = 0;
	int j = 0;
	int board_count = 0;
	int sub_dev_count = 0;
	int dsp_resource_count = 0;
	long handle = 0;
	long dev_handle = 0;
	char board_name[20] = "";
	char dev_name[20] = "";
	FEATURE_TABLE ft = { 0 };

	printf("enum_dev_information()...\n");

	sr_getboardcnt(DEV_CLASS_VOICE, &board_count);
	printf("voice board count=%d.\n", board_count);
	for (i = 1; i <= board_count; i++) {
		sprintf(board_name, "dxxxB%d", i);
		handle = dx_open(board_name, 0);
		sub_dev_count = ATDV_SUBDEVS(handle);
		printf("\tvoice board %d has %d sub-devs.\n", i, sub_dev_count);
		for (j = 1; j <= sub_dev_count; j++) {
			sprintf(dev_name, "dxxxB%dC%d", i, j);
			dev_handle = dx_open(dev_name, 0);
			dx_getfeaturelist(dev_handle, &ft);
			printf("\t\t%s %ssupport fax, %ssupport T38 fax, %ssupport CSP.\n", dev_name,
				(ft.ft_fax & FT_FAX) ? "" : "NOT ",
				(ft.ft_fax & FT_FAX_T38UDP) ? "" : "NOT ",
				(ft.ft_e2p_brd_cfg & FT_CSP) ? "" : "NOT ");
			dx_close(dev_handle);
		}
		dx_close(handle);
	}

	sr_getboardcnt(DEV_CLASS_DTI, &board_count);
	printf("dti board count=%d.\n", board_count);
	for (i = 1; i <= board_count; i++) {
		sprintf(board_name, "dtiB%d", i);
		handle = dt_open(board_name, 0);
		sub_dev_count = ATDV_SUBDEVS(handle);
		printf("\tdti board %d has %d sub-devs.\n", i, sub_dev_count);
		dt_close(handle);
	}

	sr_getboardcnt(DEV_CLASS_MSI, &board_count);
	printf("msi board count=%d.\n", board_count);
	for (i = 1; i <= board_count; i++) {
		sprintf(board_name, "msiB%d", i);
		handle = ms_open(board_name, 0);
		sub_dev_count = ATDV_SUBDEVS(handle);
		printf("\tmsi board %d has %d sub-devs.\n", i, sub_dev_count);
		ms_close(handle);
	}

	sr_getboardcnt(DEV_CLASS_DCB, &board_count);
	printf("dcb board count=%d.\n", board_count);
	for (i = 1; i <= board_count; i++) {
		sprintf(board_name, "dcbB%d", i);
		handle = dcb_open(board_name, 0);
		sub_dev_count = ATDV_SUBDEVS(handle);
		printf("\tdcb board %d has %d sub-devs(DSP).\n", i, sub_dev_count);
		for (j = 1; j <= sub_dev_count; j++) {
			sprintf(dev_name, "%sD%d", board_name, j);
			dev_handle = dcb_open(dev_name, 0);
			dcb_dsprescount(dev_handle, &dsp_resource_count);
			printf("\t\tDSP %s has %d conference resource.\n", dev_name, dsp_resource_count);
			dcb_close(dev_handle);
		}
		dcb_close(handle);
	}

	//	DEV_CLASS_SCX
	//	DEV_CLASS_AUDIO_IN	

	sr_getboardcnt(DEV_CLASS_IPT, &board_count);
	printf("ipt board count=%d.\n", board_count);
	for (i = 1; i <= board_count; i++) {
		sprintf(board_name, ":N_iptB%d:P_IP", i);
		gc_OpenEx(&handle, board_name, EV_SYNC, NULL);
		sub_dev_count = ATDV_SUBDEVS(handle);
		printf("\tipt board %d(handle=%d) has %d sub-devs.\n", i, handle, sub_dev_count);
		gc_Close(handle);
	}

	sr_getboardcnt("IPM", &board_count);
	printf("ipm board count=%d.\n", board_count);
	for (i = 1; i <= board_count; i++) {
		sprintf(board_name, ":M_ipmB%d", i);
		gc_OpenEx(&handle, board_name, EV_SYNC, NULL);
		sub_dev_count = ATDV_SUBDEVS(handle);
		printf("\tipm board %d(handle=%d) has %d sub-devs.\n", i, handle, sub_dev_count);
		gc_Close(handle);
	}

	printf("enum_dev_information done.\n");
}
/**
* Set authentication information for the PBX
* @param proxy_ip This can be (based on circumstance) the local IP or the PBX IP address.  Ussually the PBX IP address.
* @param alias The alias name to connect as on the PBX
* @param password The password for the alias to connect to the PBX with.
* @param realm The realm for the alias to connect to the PBX with.
*/
void authentication(const char* proxy_ip, const char* alias, const char* password, const char* realm)
{
	GC_PARM_BLKP gc_parm_blkp = NULL;
	IP_AUTHENTICATION auth;
	char identity[GC_ADDRSIZE] = "";
	printf("authentication()...\n");
	sprintf(identity, "sip:%s@%s", alias, proxy_ip);
	INIT_IP_AUTHENTICATION(&auth);
	auth.realm = (char *)realm;
	auth.identity = (char *)identity;
	auth.username = (char *)alias;
	auth.password = (char *)password;
	gc_util_insert_parm_ref(&gc_parm_blkp, IPSET_CONFIG, IPPARM_AUTHENTICATION_CONFIGURE, (unsigned char)(sizeof(IP_AUTHENTICATION)), &auth);
	gc_SetAuthenticationInfo(GCTGT_CCLIB_NETIF, board_dev, gc_parm_blkp);
	gc_util_delete_parm_blk(gc_parm_blkp);
}
/**
* Set registration information for the PBX
* @param proxy_ip This can be (based on circumstance) the local IP or the PBX IP address.  Ussually the PBX IP address.
* @param local_ip The local IP for the SIP client.  (Where this SIP program is runnning)
* @param alias The alias name to connect as on the PBX
* @param password The password for the alias to connect to the PBX with.
* @param realm The realm for the alias to connect to the PBX with.
*/
void registration(const char* proxy_ip, const char* local_ip, const char* alias, const char* password, const char* realm)
{
	GC_PARM_BLKP gc_parm_blkp = NULL;
	IP_REGISTER_ADDRESS register_address;
	unsigned long serviceID = 1;
	char contact[250] = "";

	if (!registered) {
		authentication(proxy_ip, alias, password, realm);

		printf("registration()...\n");

		gc_util_insert_parm_val(&gc_parm_blkp, GCSET_SERVREQ, PARM_REQTYPE, sizeof(unsigned char), IP_REQTYPE_REGISTRATION);
		gc_util_insert_parm_val(&gc_parm_blkp, GCSET_SERVREQ, PARM_ACK, sizeof(unsigned char), IP_REQTYPE_REGISTRATION);
		gc_util_insert_parm_val(&gc_parm_blkp, IPSET_PROTOCOL, IPPARM_PROTOCOL_BITMASK, sizeof(char), IP_PROTOCOL_SIP);
		gc_util_insert_parm_val(&gc_parm_blkp, IPSET_REG_INFO, IPPARM_OPERATION_REGISTER, sizeof(char), IP_REG_SET_INFO);

		memset((void*)&register_address, 0, sizeof(IP_REGISTER_ADDRESS));
		sprintf(register_address.reg_server, "%s", proxy_ip);// Request-URI
		sprintf(register_address.reg_client, "%s@%s", alias, proxy_ip);// To header field
		sprintf(contact, "sip:%s@%s", alias, local_ip);// Contact header field
		register_address.time_to_live = 3600;
		register_address.max_hops = 30;
		


		gc_util_insert_parm_ref(&gc_parm_blkp, IPSET_REG_INFO, IPPARM_REG_ADDRESS, (unsigned char)sizeof(IP_REGISTER_ADDRESS), &register_address);
		gc_util_insert_parm_ref(&gc_parm_blkp, IPSET_LOCAL_ALIAS, IPPARM_ADDRESS_TRANSPARENT, (unsigned char)strlen(contact) + 1, (void *)contact);
		gc_ReqService(GCTGT_CCLIB_NETIF, board_dev, &serviceID, gc_parm_blkp, NULL, EV_ASYNC);
		gc_util_delete_parm_blk(gc_parm_blkp);
		printf("  serviceID is 0x%x.\n", serviceID);

		registered = TRUE;
	}
}
/**
* Unregister from the SIP PBX
*/
void unregistration()
{
	GC_PARM_BLKP gc_parm_blkp = NULL;
	unsigned long serviceID = 1;

	if (registered) {
		printf("unregistration()...\n");
		gc_util_insert_parm_val(&gc_parm_blkp, IPSET_REG_INFO, IPPARM_OPERATION_DEREGISTER, sizeof(char), IP_REG_DELETE_ALL);
		gc_util_insert_parm_val(&gc_parm_blkp, GCSET_SERVREQ, PARM_REQTYPE, sizeof(unsigned char), IP_REQTYPE_REGISTRATION);
		gc_util_insert_parm_val(&gc_parm_blkp, GCSET_SERVREQ, PARM_ACK, sizeof(unsigned char), IP_REQTYPE_REGISTRATION);
		gc_util_insert_parm_val(&gc_parm_blkp, IPSET_PROTOCOL, IPPARM_PROTOCOL_BITMASK, sizeof(char), IP_PROTOCOL_SIP);
		gc_ReqService(GCTGT_CCLIB_NETIF, board_dev, &serviceID, gc_parm_blkp, NULL, EV_ASYNC);
		gc_util_delete_parm_blk(gc_parm_blkp);
		printf("  serviceID is 0x%x.\n", serviceID);

		registered = FALSE;
	}
}
/**
* Print the current status of the channels.
*/
void print_sys_status()
{
	int status = 0;
	printf("print_sys_status()...\n");
	for (unsigned index = 1; index<=MAX_CHANNELS; index++) {
		if (0 == channls[index]->crn)
			printf("  channel %d status IDEL.\n", index);
		else {
			gc_GetCallState(channls[index]->crn, &status);
			printf("  channel %d status BUSY(0x%x).\n", index, status);
		}
	}
	printf("  %sregistered.\n", TRUE == registered ? "" : "NOT ");
}

int print_all_cclibs_status(void)
{
	int i;
	char str[100], str1[100];
	GC_CCLIB_STATUSALL cclib_status_all;
	GC_CCLIB_STATUS cclib_status;
	GC_INFO gc_error_info; /* GlobalCall error information data */
	//if (gc_CCLibStatusEx("GC_ALL_LIB", &cclib_status_all) != GC_SUCCESS) {
	if (gc_CCLibStatusEx("GC_DM3CC_LIB", &cclib_status) != GC_SUCCESS) {
		/* error handling */
		gc_ErrorInfo(&gc_error_info);
		printf("Error: gc_CCLibStatusEx(), lib_name: %s, GC ErrorValue: 0x%hx - %s, CCLibID: %i - %s, CC ErrorValue : 0x % lx - %s\n",
				 "GC_ALL_LIB", gc_error_info.gcValue, gc_error_info.gcMsg,
				 gc_error_info.ccLibId, gc_error_info.ccLibName,
				 gc_error_info.ccValue, gc_error_info.ccMsg);
		return (gc_error_info.gcValue);
	}
	strcpy(str, " Call Control Library Status:\n");
	int avalible = cclib_status.num_avllibraries;
	int configured = cclib_status.num_configuredlibraries;
	int fail = cclib_status.num_failedlibraries;

	printf("Avalible %i \n", avalible);
	printf("Configured %i \n", configured);
	printf("Fail %i \n", fail);
	char ** avalible_libs = cclib_status.avllibraries;
	for (i = 0; i < avalible; i++) {
		//printf(avalible_libs[i]);
		
	}
	/*
	for (i = 0; i < GC_TOTAL_CCLIBS; i++) {
		switch (cclib_status_all.cclib_state[i].state) {
		case GC_CCLIB_CONFIGURED:
			sprintf(str1, "%s - configured\n", cclib_status_all.cclib_state[i].name);
			break;
		case GC_CCLIB_AVAILABLE:
			sprintf(str1, "%s - available\n", cclib_status_all.cclib_state[i].name);
			break;
		case GC_CCLIB_FAILED:
			sprintf(str1, "%s - is not available for use\n",
				cclib_status_all.cclib_state[i].name);
			break;
		default:
			sprintf(str1, "%s - unknown CCLIB status\n",
				cclib_status_all.cclib_state[i].name);
			break;
		}
		strcat(str, str1);
	}
	*/

	printf(str);
	return (0);
}

/**
* Start the Dialogic Global Call API and initalize the libraries.
*/
void global_call_start()
{
	GC_START_STRUCT	gclib_start;
	IPCCLIB_START_DATA cclib_start_data;
	IP_VIRTBOARD virt_boards[1];

	print_all_cclibs_status();

	printf("global_call_start()...\n");

	memset(&cclib_start_data, 0, sizeof(IPCCLIB_START_DATA));
	memset(virt_boards, 0, sizeof(IP_VIRTBOARD));
	INIT_IPCCLIB_START_DATA(&cclib_start_data, 1, virt_boards);
	INIT_IP_VIRTBOARD(&virt_boards[0]);

	cclib_start_data.delimiter = ',';
	cclib_start_data.num_boards = 1;
	cclib_start_data.board_list = virt_boards;

	virt_boards[0].localIP.ip_ver = IPVER4;					// must be set to IPVER4
	virt_boards[0].localIP.u_ipaddr.ipv4 = IP_CFG_DEFAULT;	// or specify host NIC IP address
	virt_boards[0].h323_signaling_port = IP_CFG_DEFAULT;	// or application defined port for H.323 
	virt_boards[0].sip_signaling_port = IP_CFG_DEFAULT;		// or application defined port for SIP
	virt_boards[0].sup_serv_mask = IP_SUP_SERV_CALL_XFER;	// Enable SIP Transfer Feature
	virt_boards[0].sip_msginfo_mask = IP_SIP_MSGINFO_ENABLE;// Enable SIP header
	virt_boards[0].reserved = NULL;							// must be set to NULL

	CCLIB_START_STRUCT cclib_start[] = { { "GC_DM3CC_LIB", NULL }, { "GC_H3R_LIB", &cclib_start_data }, { "GC_IPM_LIB", NULL } };
	gclib_start.num_cclibs = 3;
	gclib_start.cclib_list = cclib_start;
	if (gc_Start(&gclib_start) != -1){
		started = TRUE;
		printf("global_call_start() done.\n");
	}
	else{
		printf("Error Global Call Libraries could not be started. \n");
		print_gc_error_info("gc_Start", -1);
	}
	//print_gc_error_info_cox("gc_Start", gc_Start(NULL));
	//print_all_cclibs_status();
	
}


/**
* Open All Channels to MAX_CHANNELS
*/
void open_channels()
{

	printf("Hello from open_channels. \n");

	long request_id = 0;
	GC_PARM_BLKP gc_parm_blk_p = NULL;

	global_call_start();
	printf("global_call_start() done.\n");

	//enum_dev_information();

	gc_OpenEx(&board_dev, ":N_iptB1:P_IP", EV_SYNC, NULL);

	//setting T.38 fax server operating mode: IP MANUAL mode
	gc_util_insert_parm_val(&gc_parm_blk_p, IPSET_CONFIG, IPPARM_OPERATING_MODE, sizeof(long), IP_MANUAL_MODE);

	//Enabling and Disabling Unsolicited Notification Events
	gc_util_insert_parm_val(&gc_parm_blk_p, IPSET_EXTENSIONEVT_MSK, GCACT_ADDMSK, sizeof(long),
		EXTENSIONEVT_DTMF_ALPHANUMERIC | EXTENSIONEVT_SIGNALING_STATUS | EXTENSIONEVT_STREAMING_STATUS | EXTENSIONEVT_T38_STATUS);
	gc_SetConfigData(GCTGT_CCLIB_NETIF, board_dev, gc_parm_blk_p, 0, GCUPDATE_IMMEDIATE, &request_id, EV_ASYNC);
	gc_util_delete_parm_blk(gc_parm_blk_p);

	GC_PARM_BLKP pParmBlock = 0;
	int frc = GC_SUCCESS;


	for (int i = 1; i <= MAX_CHANNELS; i++) {
		channls[i] = new CHANNEL(i);
		channls[i]->open();
	}

}
/**
* Open a single channel.
*/
void open_channel(int channel)
{

	if (started){
		printf("open_channel.  \n");

		long request_id = 0;
		GC_PARM_BLKP gc_parm_blk_p = NULL;


		//enum_dev_information();

		gc_OpenEx(&board_dev, ":N_iptB1:P_IP", EV_SYNC, NULL);

		//setting T.38 fax server operating mode: IP MANUAL mode
		gc_util_insert_parm_val(&gc_parm_blk_p, IPSET_CONFIG, IPPARM_OPERATING_MODE, sizeof(long), IP_MANUAL_MODE);

		//Enabling and Disabling Unsolicited Notification Events
		gc_util_insert_parm_val(&gc_parm_blk_p, IPSET_EXTENSIONEVT_MSK, GCACT_ADDMSK, sizeof(long),
			EXTENSIONEVT_DTMF_ALPHANUMERIC | EXTENSIONEVT_SIGNALING_STATUS | EXTENSIONEVT_STREAMING_STATUS | EXTENSIONEVT_T38_STATUS);
		gc_SetConfigData(GCTGT_CCLIB_NETIF, board_dev, gc_parm_blk_p, 0, GCUPDATE_IMMEDIATE, &request_id, EV_ASYNC);
		gc_util_delete_parm_blk(gc_parm_blk_p);

		GC_PARM_BLKP pParmBlock = 0;
		int frc = GC_SUCCESS;

		channls[channel] = new CHANNEL(channel);
		channls[channel]->open();
	}


}
/**
* Close all open channels to MAX_CHANNELS
*/
void close_channels()
{
	printf("close_channels()...\n");
	unregistration();
	gc_Close(board_dev);
	for (int i = 1; i<=MAX_CHANNELS; i++) {
		channls[i]->close();
	}
	gc_Stop();
}
/**
* Process Event for a Syncronous repsonse.
* This is part of the sycnronous wrapper for Dialogic ASYNC mode.
* @param wait_event The Dialogic event that this process is waiting for.
*/
int ProcessEventSync(int wait_event, long event_handle, int channel)
{
	printf("ProcessEventSync \n");
	int timeout = 0;
	//long event_handle = 0;
	int evt_dev = 0;
	int evt_code = 0;
	int evt_len = 0;
	void* evt_datap = NULL;
	METAEVENT meta_evt;
	CHANNEL* pch = NULL;
	GC_PARM_BLKP gc_parm_blkp = NULL;
	GC_PARM_DATAP gc_parm_datap = NULL;
	int value = 0;

	/*
	* This has been removed as this loop now executes inside a seperate thread.
	* This is way easier to understand as now I do not have all these weird loops in the code.
	* Unfortunatlly I had to replace the continue statements with return statements as the loops
	* were using continue statements to avoid code before variables had been initalized.
	* This is a hack and I will need to clean up the variables later.
	PRINT_CLI_HELP_MSG;

	while (TRUE) {
	do {
	timeout = sr_waitevt(50);
	if (FALSE == analyse_cli()) {
	return;
	}
	Sleep(1000);
	} while (timeout == SR_TMOUT);
	*/

	//evt_dev = (int)sr_getevtdev(event_handle);
	//evt_code = (int)sr_getevttype(event_handle);
	//evt_len = (int)sr_getevtlen(event_handle);
	//evt_datap = (void*)sr_getevtdatap(event_handle);

	gc_GetMetaEventEx(&meta_evt, event_handle);
	//gc_GetMetaEvent(&meta_evt);

	evt_code = (int)meta_evt.evttype;
	evt_dev = (int)meta_evt.evtdev;


	if (meta_evt.flags & GCME_GC_EVENT) {

		//for register
		if (evt_dev == board_dev && GCEV_SERVICERESP == meta_evt.evttype) {
			gc_parm_blkp = (GC_PARM_BLKP)(meta_evt.extevtdatap);
			gc_parm_datap = gc_util_next_parm(gc_parm_blkp, gc_parm_datap);

			while (NULL != gc_parm_datap) {
				if (IPSET_REG_INFO == gc_parm_datap->set_ID) {
					if (IPPARM_REG_STATUS == gc_parm_datap->parm_ID) {
						value = (int)(gc_parm_datap->value_buf[0]);
						switch (value) {
						case IP_REG_CONFIRMED:
							printf("IPSET_REG_INFO/IPPARM_REG_STATUS: IP_REG_CONFIRMED\n");
							break;
						case IP_REG_REJECTED:
							printf("IPSET_REG_INFO/IPPARM_REG_STATUS: IP_REG_REJECTED\n");
							break;
						default:
							break;
						}
						return value;
					}
					else if (IPPARM_REG_SERVICEID == gc_parm_datap->parm_ID) {
						value = (int)(gc_parm_datap->value_buf[0]);
						printf("IPSET_REG_INFO/IPPARM_REG_SERVICEID: 0x%x\n", value);
					}

				}
				else if (IPSET_LOCAL_ALIAS == gc_parm_datap->set_ID){
					char * localAlias = new char[gc_parm_datap->value_size + 1];
					localAlias = (char*)&gc_parm_datap->value_buf;
					printf("\tIPSET_LOCAL_ALIAS value: %s\n", localAlias);
				}
				else if (IPSET_SIP_MSGINFO == gc_parm_datap->set_ID){
					char * msgInfo = new char[gc_parm_datap->value_size + 1];
					msgInfo = (char*)&gc_parm_datap->value_buf;
					printf("\tIPSET_SIP_MSGINFO value: %s\n", msgInfo);
				}
				gc_parm_datap = gc_util_next_parm(gc_parm_blkp, gc_parm_datap);
			}
			//continue;
			return -1;
		}

		gc_GetUsrAttr(meta_evt.linedev, (void**)&pch);
		if (NULL == pch)
			return -1;
		//continue;



		pch->print("CHECK CHANNEL %i got GC event : %s", channel, GCEV_MSG(evt_code));
		gc_GetCRN(&pch->crn, &meta_evt);

		switch (evt_code)
		{
		case GCEV_ALERTING:
			printf("##########ALERTING###########");
			//pch->start_call_progress_analysis();
			break;
		case GCEV_OPENEX:
			pch->set_dtmf();
			pch->connect_voice();
			break;
		case GCEV_UNBLOCKED:
			//pch->wait_call();
			break;
		case GCEV_OFFERED:
			pch->print_offer_info(meta_evt);
			pch->ack_call();
			break;
		case GCEV_CALLPROC:
			pch->accept_call();
			break;
		case GCEV_ACCEPT:
			pch->answer_call();
			break;
		case GCEV_ANSWERED:
			//pch->do_fax(DF_TX);
			break;
		case GCEV_CALLSTATUS:
			pch->print_call_status(meta_evt);
			break;
		case GCEV_CONNECTED:
			//pch->do_fax(DF_RX);
			break;
		case GCEV_DROPCALL:
			pch->release_call();
			break;
		case GCEV_DISCONNECTED:
			pch->print_call_status(meta_evt);
			pch->restore_voice();
			pch->drop_call();
			break;
		case GCEV_EXTENSIONCMPLT:
		case GCEV_EXTENSION:
			pch->process_extension(meta_evt);
			break;
		case GCEV_RELEASECALL:
			pch->already_connect_fax = FALSE;
			pch->fax_proceeding = FALSE;
			pch->crn = 0;
			break;
		case GCEV_TASKFAIL:
			pch->print_call_status(meta_evt);
			if (TRUE == pch->fax_proceeding)
				pch->restore_voice();
			break;
		default:
			break;
		}
	}
	else {
		/*
		Please note that if I want to use syncronous calls to process
		these events in the future I will need to uncomment the code below and
		pass in the channel that I want to check the event against.
		*/
		//for (int i = 0; i<MAX_CHANNELS; i++) {
		//	if (channls[i]->vox_dev == evt_dev)
		//		pch = channls[i];
		//}
		if (NULL == pch)
			return -1;
		//continue;

		switch (evt_code)
		{
		case TDX_PLAY:
			pch->print("got voice event : TDX_PLAY");
			pch->process_voice_done();
			break;
		case TDX_RECORD:
			pch->print("got voice event : TDX_RECORD");
			pch->process_voice_done();
			break;
		case TDX_CST:
			pch->print("got voice event : TDX_CST");
			if (DE_DIGITS == ((DX_CST*)evt_datap)->cst_event) {
				pch->print("DE_DIGITS: [%c]", (char)((DX_CST*)evt_datap)->cst_data);
			}
			break;

		default:
			pch->print("unexcepted R4 event(0x%x)", evt_code);
			break;
		}
	}
	return evt_code;
}
/**
* Checks to see if the syncronous wrapper has expired.
* It was easier to understand the lgoic if it was seperated into 
* this smaller code block.
* This is part of the sycnronous wrapper for Dialogic ASYNC mode.
*
* A wait_time of SYNC_WAIT_INFINITE will force hasExpired to never expire.
*
* @param count The number of times a Dialogic wait event has looped.
* @param wait_time The number of times to allow a Dialogic wait event to loop
*/
bool hasExpired(int count, int wait_time){
	if (wait_time == SYNC_WAIT_INFINITE)	{
		return false;
	}
	else if (count > wait_time){
		return true;
	}
	
	return false;
}
/**
* Checks to see if the syncronous wrapper shoudl loop again.
* It was easier to understand the lgoic if it was seperated into
* this smaller code block.
* This is part of the sycnronous wrapper for Dialogic ASYNC mode.
*
* A wait_time of SYNC_WAIT_INFINITE will force hasExpired to never expire.
*
* @param event_thrown The event that has just been thrown.
* @param wait_for_event The event that we are waiting to occur.
* @param count The number of times a Dialogic wait event has looped.
* @param wait_time The number of times to allow a Dialogic wait event to loop
*/
bool loopAgain(int event_thrown, int wait_for_event, int count, int wait_time){

	bool has_event_thrown = false;
	bool has_expired = false;

	if (event_thrown == wait_for_event){
		has_event_thrown = true;
	}

	if (hasExpired(count, wait_time)){
		has_expired = true;
	}

	if (has_event_thrown || has_expired){
		return false;
	}
	return true;
}

/**
* This is the main loop for the syncronous wrapper for Dialogic ASYNC mode.
*
* A wait_time of SYNC_WAIT_INFINITE will force hasExpired to never expire.
* Please note that wait_time is a tenth of a second.
*
* @param wait_for_event The event that we are waiting to occur.
* @param wait_time The number of times to allow a Dialogic wait event to loop
*/
int WaitForEventSync(int channel, int wait_for_event, int wait_time){

	int event_thrown = -1;
	int count = 0;
	long event_handle = 0;

	/*
	* Do SRL event processing
	*/
	long hdlcnt = 1;
	long hdls[1];
	long dev_handle = channls[channel]->get_device_handle();
	hdls[0] = dev_handle;
	//hdls[1] = channls[channel]->get_voice_handle();
	//printf(" dev_handle = %d \n", dev_handle);

	do
	{
		//	Wait one tenth of a second for an event
		if (sr_waitevtEx(hdls, hdlcnt,100,&event_handle) != -1)
		//if (sr_waitevt(100) != -1)
		{
			// If the event is valid, process it
			event_thrown = ProcessEventSync(wait_for_event, event_handle, channel);
			if (event_thrown == wait_for_event) break;

		}
		count++;
	} while (loopAgain(event_thrown,wait_for_event,count, wait_time));

	if (event_thrown == wait_for_event){
		return SYNC_WAIT_SUCCESS;
	}
	else if (hasExpired(count, wait_time)){
		return SYNC_WAIT_EXPIRED;
	}
	return SYNC_WAIT_ERROR;
}

/**
* This is the logic for prcoessing an event in Dialogic ASYNC mode.
*/
void ProcessEvent()
{

	int timeout = 0;
	long event_handle = 0;
	int evt_dev = 0;
	int evt_code = 0;
	int evt_len = 0;
	void* evt_datap = NULL;
	METAEVENT meta_evt;
	CHANNEL* pch = NULL;
	GC_PARM_BLKP gc_parm_blkp = NULL;
	GC_PARM_DATAP gc_parm_datap = NULL;
	int value = 0;

	/*
	* This has been removed as this loop now executes inside a seperate thread.  
	* This is way easier to understand as now I do not have all these weird loops in the code.
	* Unfortunatlly I had to replace the continue statements with return statements as the loops 
	* were using continue statements to avoid code before variables had been initalized.
	* This is a hack and I will need to clean up the variables later.
	PRINT_CLI_HELP_MSG;

	while (TRUE) {
		do {
			timeout = sr_waitevt(50);
			if (FALSE == analyse_cli()) {
				return;
			}
			Sleep(1000);
		} while (timeout == SR_TMOUT);
		*/

		evt_dev = (int)sr_getevtdev(event_handle);
		evt_code = (int)sr_getevttype(event_handle);
		evt_len = (int)sr_getevtlen(event_handle);
		evt_datap = (void*)sr_getevtdatap(event_handle);

		gc_GetMetaEventEx(&meta_evt, event_handle);


		if (meta_evt.flags & GCME_GC_EVENT) {

			//for register
			if (evt_dev == board_dev && GCEV_SERVICERESP == meta_evt.evttype) {
				gc_parm_blkp = (GC_PARM_BLKP)(meta_evt.extevtdatap);
				gc_parm_datap = gc_util_next_parm(gc_parm_blkp, gc_parm_datap);

				while (NULL != gc_parm_datap) {
					if (IPSET_REG_INFO == gc_parm_datap->set_ID) {
						if (IPPARM_REG_STATUS == gc_parm_datap->parm_ID) {
							value = (int)(gc_parm_datap->value_buf[0]);
							switch (value) {
							case IP_REG_CONFIRMED:
								printf("IPSET_REG_INFO/IPPARM_REG_STATUS: IP_REG_CONFIRMED\n");
								break;
							case IP_REG_REJECTED:
								printf("IPSET_REG_INFO/IPPARM_REG_STATUS: IP_REG_REJECTED\n");
								break;
							default:
								break;
							}
						}
						else if (IPPARM_REG_SERVICEID == gc_parm_datap->parm_ID) {
							value = (int)(gc_parm_datap->value_buf[0]);
							printf("IPSET_REG_INFO/IPPARM_REG_SERVICEID: 0x%x\n", value);
						}

					}
					else if (IPSET_LOCAL_ALIAS == gc_parm_datap->set_ID){
						char * localAlias = new char[gc_parm_datap->value_size + 1];
						localAlias = (char*)&gc_parm_datap->value_buf;
						printf("\tIPSET_LOCAL_ALIAS value: %s\n", localAlias);
					}
					else if (IPSET_SIP_MSGINFO == gc_parm_datap->set_ID){
						char * msgInfo = new char[gc_parm_datap->value_size + 1];
						msgInfo = (char*)&gc_parm_datap->value_buf;
						printf("\tIPSET_SIP_MSGINFO value: %s\n", msgInfo);
					}
					gc_parm_datap = gc_util_next_parm(gc_parm_blkp, gc_parm_datap);
				}
				//continue;
				return;
			}
			
			gc_GetUsrAttr(meta_evt.linedev, (void**)&pch);
			if (NULL == pch)
				return;
				//continue;
				

			
			pch->print("got GC event : %s", GCEV_MSG(evt_code));
			gc_GetCRN(&pch->crn, &meta_evt);

			switch (evt_code)
			{
			case GCEV_ALERTING:
				printf("##########ALERTING###########");
				pch->start_call_progress_analysis();
				break;
			case GCEV_OPENEX:
				pch->set_dtmf();
				pch->connect_voice();
				break;
			case GCEV_UNBLOCKED:
				pch->wait_call();
				break;
			case GCEV_OFFERED:
				pch->print_offer_info(meta_evt);
				pch->ack_call();
				break;
			case GCEV_CALLPROC:
				pch->accept_call();
				break;
			case GCEV_ACCEPT:
				pch->answer_call();
				break;
			case GCEV_ANSWERED:
				//pch->do_fax(DF_TX);
				break;
			case GCEV_CALLSTATUS:
				pch->print_call_status(meta_evt);
				break;
			case GCEV_CONNECTED:
				//pch->do_fax(DF_RX);
				break;
			case GCEV_DROPCALL:
				pch->release_call();
				break;
			case GCEV_DISCONNECTED:
				pch->print_call_status(meta_evt);
				pch->restore_voice();
				pch->drop_call();
				break;
			case GCEV_EXTENSIONCMPLT:
			case GCEV_EXTENSION:
				pch->process_extension(meta_evt);
				break;
			case GCEV_RELEASECALL:
				pch->already_connect_fax = FALSE;
				pch->fax_proceeding = FALSE;
				pch->crn = 0;
				break;
			case GCEV_TASKFAIL:
				pch->print_call_status(meta_evt);
				if (TRUE == pch->fax_proceeding)
					pch->restore_voice();
				break;
			default:
				break;
			}
		}
		else {

			for (int i = 0; i<MAX_CHANNELS; i++) {
				if (channls[i]->vox_dev == evt_dev)
					pch = channls[i];
			}
			if (NULL == pch)
				return;
				//continue;

			switch (evt_code)
			{
			case TDX_PLAY:
				pch->print("got voice event : TDX_PLAY");
				pch->process_voice_done();
				break;
			case TDX_RECORD:
				pch->print("got voice event : TDX_RECORD");
				pch->process_voice_done();
				break;
			case TDX_CST:
				pch->print("got voice event : TDX_CST");
				if (DE_DIGITS == ((DX_CST*)evt_datap)->cst_event) {
					pch->print("DE_DIGITS: [%c]", (char)((DX_CST*)evt_datap)->cst_data);
				}
				break;
			
			default:
				pch->print("unexcepted R4 event(0x%x)", evt_code);
				break;
			}
		}

	//}

}

/**
* The wait event loop for the async event handling thread function
*
* @param parm case to the index in the ExtendedAsyncInfo array
*/
void WaitEvent(void* parm)
{
	// wait for, and process, events untill application exits
	printf("[%4d] WaitEvent Thread started\n", GetCurrentThreadId());

	do
	{
		//	Wait one second for an event
		if (sr_waitevt(1000) != -1)
		{
			// If the event is valid, process it
			ProcessEvent();
		}
	} while (!exitFlag);
	printf("[%4d] WaitEvent Thread stopping\n", GetCurrentThreadId());
}

/**
* Syncronous Functions
*/

/**
* Starts Dialogic Libraries syncronously
*/
void DialogicFunctions::DialogicStartSync(){
	if (!started){
		global_call_start();
	}

}
/**
* Stops Dialogic Libraries syncronously
*/
void DialogicFunctions::DialogicStopSync(){
	if (started){
		gc_Stop();
	}
}

/**
* Open a channel syncronously
* @param channel_index The channel to open.
*/
void DialogicFunctions::DialogicOpenSync(int channel_index){
	//printf("DialogicOpenSync\n");
	open_channel(channel_index);

	/*
	* I wait forever as if I cannot open the device this will keep it open
	* to show that an error is occuring.  I might want to put 
	* more logic in this and return an ERROR or SUCCESS in the future.
	*/

	if (WaitForEventSync(channel_index, GCEV_UNBLOCKED, SYNC_WAIT_INFINITE) == SYNC_WAIT_SUCCESS){
		//printf("GCEV_UNBLOCKED\n");
	}

	//channel_index++;


	//open_channel(channel_index);
	//if (WaitForEventSync(channel_index, GCEV_UNBLOCKED, SYNC_WAIT_INFINITE) == SYNC_WAIT_SUCCESS){
		//printf("GCEV_UNBLOCKED\n");
	//}


}
/**
* Close a channel syncronously
* @param channel_index The channel to close.
*/
void DialogicFunctions::DialogicCloseSync(int channel_index){
	printf("DialogicCloseSync %i \n", channel_index);
	/*
	Due to the threading of this applicaiton never unregester until then.  Otherwise this could
	prevent calls from getting through.
	if (registered) {
		unregistration();
		
		// I wait or timeout as even if I cannot close the device I still want to close
		// the code.
		

		if (WaitForEventSync(channel_index, IP_REG_CONFIRMED, 100) == SYNC_WAIT_SUCCESS){
			//printf("Unregistered\n");
		}
	}
	*/
	gc_Close(board_dev);
	channls[channel_index]->close();

}
/**
* Register the sip client syncronously
* Set registration information for the PBX
* @param proxy_ip This can be (based on circumstance) the local IP or the PBX IP address.  Ussually the PBX IP address.
* @param local_ip The local IP for the SIP client.  (Where this SIP program is runnning)
* @param alias The alias name to connect as on the PBX
* @param password The password for the alias to connect to the PBX with.
* @param realm The realm for the alias to connect to the PBX with.
*/
void DialogicFunctions::DialogicRegisterSync(int channel_index, const char* proxy_ip, const char* local_ip, const char* alias, const char* password, const char* realm){
	//printf("DialogicRegisterSync %i\n", channel_index);

	auth_proxy_ip = proxy_ip;
	auth_alias = alias;
	auth_password = password;
	auth_realm = realm;

	if (!registered) {
		registration(proxy_ip, local_ip, alias, password, realm);
		if (WaitForEventSync(channel_index, IP_REG_CONFIRMED, 100) == SYNC_WAIT_SUCCESS){
			//printf("Registered\n");
		}
	}
}
/**
* Unregister the sip client syncronously.
*/
void DialogicFunctions::DialogicUnregisterSync(int channel_index){
	//printf("DialogicUnregisterSync %i\n", channel_index);
	unregistration();
	if (WaitForEventSync(channel_index, IP_REG_CONFIRMED, 100) == SYNC_WAIT_SUCCESS){
		//printf("IP_REG_CONFIRMED\n");
	}
}
/**
* Stop voice functions (Play, Record, etc.) for the channel syncronously.
* @param channel_index The channel to stop voice functions.
*/
void DialogicFunctions::DialogicStopSync(int channel_index){
	//printf("DialogicStopSync\n");
	return channls[channel_index]->stop();
}

/**
* Make a call syncronously.
* @param channel_index The channel to use to make a call.
* @param ani The call origin formatted as alias@proxy_ip Example: Developer1@127.0.0.1
* @param dnis The call desintation formatted as number@proxy_ip Example: 5554443333@127.0.0.1
*/
int DialogicFunctions::DialogicMakeCallSync(int channel_index, const char* ani, const char* dnis){
	//printf("DialogicMakeCallSync\n");
	authentication(auth_proxy_ip, auth_alias, auth_password, auth_realm);
	channls[channel_index]->make_call(ani, dnis);

	// TODO: There is no point waiting forrever if a timeout occurs I can drop the call as this is an error.
	// I can fix this later.
	if (WaitForEventSync(channel_index, GCEV_ALERTING, 2000) == SYNC_WAIT_SUCCESS){
		//printf("###################GCEV_ALERTING\n");
		//return channls[channel_index]->start_call_progress_analysis();
		return 1;
	}

	return -1;

}
/**
* Make a drop call syncronously.
* @param channel_index The channel to use to drop a call.
*/
int DialogicFunctions::DialogicDropCallSync(int channel_index){
	//printf("DialogicDropCallSync\n");

	/*
	* If the channel is IDLE there is no call to drop.
	*/
	if (0 == channls[channel_index]->crn){
		printf("Channel %d status IDLE.\n", channel_index);
		return 1;
	}

	/*
	* The channel is not IDLE so drop the call and wait for the event
	* so that the call can be released.
	* After the call has been released the channel is IDLE
	* and a new call can begin.
	*/

	channls[channel_index]->drop_call();

	int result = WaitForEventSync(channel_index, GCEV_RELEASECALL, 100);

	/*
	* If a timeout event occurs for drop call I can assume that
	* the call has already ended 
	*/
	if (result == SYNC_WAIT_SUCCESS || result == SYNC_WAIT_EXPIRED){
		//printf("GCEV_RELEASECALL\n");
		return 1;
	}

	return -1;
}
/**
* Wait for a call syncronously.
* @ param channel_index The channel to use to wait for a call.
*/
int DialogicFunctions::DialogicWaitCallSync(int channel_index){
	//printf("DialogicWaitCallSync\n");

	channls[channel_index]->wait_call();
	if (WaitForEventSync(channel_index, GCEV_ANSWERED, SYNC_WAIT_INFINITE) == SYNC_WAIT_SUCCESS){
		return 1;
	}

	return -1;
}

/*
* ASyncronous Functions
*/

/**
* Open all channels to MAX_CHANNEL
* TODO: Make MAX_CHANNEL a parameter.
*/
void DialogicFunctions::DialogicOpenAll(){
	//printf("DialogicStart\n");
	open_channels();
}
/**
* Start the thread that handles Dialogic Asycnronous events.
*/
void DialogicFunctions::DialogicStartAsyncEventThread(){
	//printf("[%4d] starting...\n", GetCurrentThreadId());
	hThread = (HANDLE)_beginthread(WaitEvent, 0, (void*)NULL);
}
/**
* Stop the thread that handles Dialogic Asycnronous events.
*/
void DialogicFunctions::DialogicStopAsyncEventThread(){
	//printf("[%4d] stopping...\n", GetCurrentThreadId());
	exitFlag = true;
	CloseHandle(hThread);
	WaitForSingleObject(hThread, INFINITE);
}
/**
* Close all channels to MAX_CHANNEL
* Close the Async event thread if it has not been stopped already.
* TODO: Remove duplicate code for stop thread.
*/
void DialogicFunctions::DialogicCloseAll(){
	//printf("DialogicCloseAll\n");
	close_channels();
	exitFlag = true;
	CloseHandle(hThread);
	WaitForSingleObject(hThread, INFINITE);
}
/**
* Register the sip client asyncronously
* Set registration information for the PBX
* @param proxy_ip This can be(based on circumstance) the local IP or the PBX IP address.Ussually the PBX IP address.
* @param local_ip The local IP for the SIP client.  (Where this SIP program is runnning)
* @param alias The alias name to connect as on the PBX
* @param password The password for the alias to connect to the PBX with.
* @param realm The realm for the alias to connect to the PBX with.
*/
void DialogicFunctions::DialogicRegister(const char* proxy_ip, const char* local_ip, const char* alias, const char* password, const char* realm){
	//printf("DialogicRegister\n");
	registration(proxy_ip, local_ip, alias, password, realm);
}
/**
* Stop voice functions for the channel.
* @param channel_index The channel to stop the voice functions.
*/
void DialogicFunctions::DialogicStop(int channel_index){
	//printf("DialogicStop\n");
	channls[channel_index]->stop();
}
/**
* Play a file using the Dialogic Voice API for the channel.
* Please note file must conform to the wave encoding in this.
* @param channel_index The channel to use to play the file.
* @param filename The filename (full path if not in lcoal executable directory) for the wave file to play.
*/
void DialogicFunctions::DialogicPlayFile(int channel_index, const char* filename){
	//printf("DialogicPlayFile\n");
	channls[channel_index]->play_wave_file(filename);
}
/**
* Record a file using the Dialogic Voice API for the channel.
* @param channel_index The channel to use to record the file.
*/
void DialogicFunctions::DialogicRecordFile(int channel_index){
	//printf("DialogicRecordFile\n");
	channls[channel_index]->record_wave_file();
}
/**
* Make a call asyncronously.
* @param channel_index The channel to use to make a call.
* @param ani The call origin formatted as alias@proxy_ip Example: Developer1@127.0.0.1
* @param dnis The call desintation formatted as number@proxy_ip Example: 5554443333@127.0.0.1
*/
void DialogicFunctions::DialogicMakeCall(int channel_index, const char* ani, const char* dnis){
	//printf("DialogicMakeCall\n");
	channls[channel_index]->make_call(ani, dnis);
}
/**
* Drop a call asyncronously.
* @param channel_index.  The channel to use to drop a call.
*/
void DialogicFunctions::DialogicDropCall(int channel_index){
	//printf("DialogicDropCall\n");
	channls[channel_index]->drop_call();
}
/**
* Print the call status.
*/
void DialogicFunctions::DialogicStatus(){
	//printf("DialogicStatus\n");
	print_sys_status();
}
/**
* Unregister the sip client.
*/
void DialogicFunctions::DialogicUnregister(){
	//printf("DialogicUnregister\n");
	unregistration();
}


/*
* Shared Functions
* These functions are syncronous but they can be used interchangably with sycnronous and asyncronous funtions
*/

/**
* CLR test method to make sure teh CLR wrapping code was working correctly.
* TODO: Remove this method.
*/
void DialogicFunctions::HelloWorld(){
	printf("HelloWorld\n");
}
/**
* Get the Global Call Device name.
* @param channel_index The channel to get the name for.
*/
char* DialogicFunctions::DialogicGetDeviceName(int channel_index){
	printf("DialogicGetDeviceName\n");
	return channls[channel_index]->get_device_name();
}
/**
* Get the Global Call Device handle.
* Avoid using this feature unless you absolutly have to.
* It would be better to put the code inside the CHANNEL class in this code.
* @param channel_index The channel to get the global call device handle for.
*/
long DialogicFunctions::DialogicGetDeviceHandle(int channel_index){
	printf("DialogicGetDeviceHandle\n");
	return channls[channel_index]->get_device_handle();
}
/** 
* Get the Voice Device handle.
* Avoid using this feature unless you absolutly have to.
* It would be better to put the code inside the CHANNEL class in this code.
* @param channel_index The channel to get the voice device handle for.
*/
int DialogicFunctions::DialogicGetVoiceHandle(int channel_index){
	printf("DialogicGetVoiceHandle\n");
	return channls[channel_index]->get_voice_handle();
}
/**
* Delete Tones
* @param channel_index The channel to use.
*/
int DialogicFunctions::DialogicDeleteTones(int channel_index){
	printf("DialogicDeleteTones\n");
	return channls[channel_index]->voice_dx_deltones();
}
/**
* Change tone frequency.
* @param channel_index The channel to use.
*/
int DialogicFunctions::DialogicChangeFrequency(int channel_index, int tonetype, int fq1, int dv1, int fq2, int dv2){
	printf("DialogicChangeFrequency\n");
	return channls[channel_index]->voice_dx_chgfreq(tonetype, fq1, dv1, fq2, dv2);
}
/**
* Change duration.
* @param channel_index The channel to use.
*/
int DialogicFunctions::DialogicChangeDuration(int channel_index, int typetype, int on, int ondv, int off, int offdv){
	printf("DialogicChangeDuration\n");
	return channls[channel_index]->voice_dx_chgdur(typetype, on, ondv, off, offdv);
}
/**
* Change repitition count.
* @param channel_index The channel to use.
*/
int DialogicFunctions::DialogicChangeRepititionCount(int channel_index, int tonetype, int repcount){
	printf("DialogicChangeRepititionCount\n");
	return channls[channel_index]->voice_dx_chgrepcnt(tonetype, repcount);
}
/**
* Get digits.
* @param channel_index The channel to use.
*/
int DialogicFunctions::DialogicGetDigits(int channel_index){
	printf("DialogicGetDigits\n");
	return channls[channel_index]->voice_dx_getdig();
}
/**
* Get call state.
* @param channel_index The channel to use.
*/
int DialogicFunctions::DialogicGetCallState(int channel_index){
	return channls[channel_index]->globalcall_gc_GetCallState();
}

