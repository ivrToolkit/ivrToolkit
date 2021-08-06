// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 
#pragma once
#include <stdio.h>
#include <conio.h>
#include <process.h>

#include <srllib.h>
#include <dxxxlib.h>
#include <dtilib.h>
#include <msilib.h>
#include <dcblib.h>
#include <gcip_defs.h>
#include <gcip.h>
#include "CHANNEL.h"
#include "DialogicFunctions.h"





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





long board_dev = 0; //Board Device Handle
BOOL registered = FALSE; //Registered with PBX
ivrToolkit::DialogicSipWrapper::CHANNEL* channls[MAX_CHANNELS] = { 0 };  //A channel array 
bool exitFlag = false;				// Process SRL events until ExitFlag = TRUE
HANDLE hThread; //The thread that is used to process events asyncronously.
BOOL started = FALSE;

/*Authentication variables*/
const char* auth_proxy_ip;
const char* auth_alias;
const char* auth_password;
const char* auth_realm;


/**
* Print error information.  This information is commonly used for
* debugging an error when an API function returns does not return
* as SUCCESS (1).
*/
void print_gc_error_info(const char *func_name, int func_return) {
	GC_INFO gc_error_info;
	if (GC_ERROR == func_return) {
		gc_ErrorInfo(&gc_error_info);

		BOOST_LOG_TRIVIAL(error) << boost::format("%s return %d, GC ErrorValue:0x%hx-%s,\n  CCLibID:%i-%s, CC ErrorValue:0x%lx-%s,\n  Additional Info:%s") %
			func_name % func_return % gc_error_info.gcValue % gc_error_info.gcMsg %
			gc_error_info.ccLibId % gc_error_info.ccLibName %
			gc_error_info.ccValue % gc_error_info.ccMsg %
			gc_error_info.additionalInfo;
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
	int i;
	int j;
	int board_count = 0;
	int sub_dev_count = 0;
	int dsp_resource_count = 0;
	long handle = 0;
	long dev_handle;
	char board_name[20] = "";
	char dev_name[20] = "";
	FEATURE_TABLE ft = { 0 };

	BOOST_LOG_TRIVIAL(info) << "  enum_dev_information():\n";

	sr_getboardcnt(DEV_CLASS_VOICE, &board_count);
	BOOST_LOG_TRIVIAL(info) << boost::format("    voice board count=%d.\n") % board_count;
	for (i = 1; i <= board_count; i++) {
		sprintf(board_name, "dxxxB%d", i);
		handle = dx_open(board_name, 0);
		sub_dev_count = ATDV_SUBDEVS(handle);
		
		BOOST_LOG_TRIVIAL(info) << boost::format("        voice board %d has %d sub-devs.\n") % i % sub_dev_count;
		for (j = 1; j <= sub_dev_count; j++) {
			sprintf(dev_name, "dxxxB%dC%d", i, j);
			dev_handle = dx_open(dev_name, 0);
			dx_getfeaturelist(dev_handle, &ft);

			BOOST_LOG_TRIVIAL(info) << boost::format("            %s %ssupport fax, %ssupport T38 fax, %ssupport CSP.\n") % dev_name %
				(ft.ft_fax & FT_FAX ? "" : "NOT ") %
				(ft.ft_fax & FT_FAX_T38UDP ? "" : "NOT ") %
				(ft.ft_e2p_brd_cfg & FT_CSP ? "" : "NOT ");
			
			dx_close(dev_handle);
		}
		dx_close(handle);
	}

	sr_getboardcnt(DEV_CLASS_DTI, &board_count);
	BOOST_LOG_TRIVIAL(info) << boost::format("    dti board count=%d.\n") % board_count;
	for (i = 1; i <= board_count; i++) {
		sprintf(board_name, "dtiB%d", i);
		handle = dt_open(board_name, 0);
		sub_dev_count = ATDV_SUBDEVS(handle);
		
		BOOST_LOG_TRIVIAL(info) << boost::format("        dti board %d has %d sub-devs.\n") % i % sub_dev_count;
		dt_close(handle);
	}

	sr_getboardcnt(DEV_CLASS_MSI, &board_count);
	BOOST_LOG_TRIVIAL(info) << boost::format("    msi board count=%d.\n") % board_count;
	for (i = 1; i <= board_count; i++) {
		sprintf(board_name, "msiB%d", i);
		handle = ms_open(board_name, 0);
		sub_dev_count = ATDV_SUBDEVS(handle);
		BOOST_LOG_TRIVIAL(info) << boost::format("        msi board %d has %d sub-devs.\n") % i % sub_dev_count;
		ms_close(handle);
	}

	sr_getboardcnt(DEV_CLASS_DCB, &board_count);
	BOOST_LOG_TRIVIAL(info) << boost::format("    dcb board count=%d.\n") % board_count;
	for (i = 1; i <= board_count; i++) {
		sprintf(board_name, "dcbB%d", i);
		handle = dcb_open(board_name, 0);
		sub_dev_count = ATDV_SUBDEVS(handle);
		BOOST_LOG_TRIVIAL(info) << boost::format("        dcb board %d has %d sub-devs(DSP).\n") % i % sub_dev_count;
		for (j = 1; j <= sub_dev_count; j++) {
			sprintf(dev_name, "%sD%d", board_name, j);
			dev_handle = dcb_open(dev_name, 0);
			dcb_dsprescount(dev_handle, &dsp_resource_count);
			BOOST_LOG_TRIVIAL(info) << boost::format("            DSP %s has %d conference resource.\n") % dev_name % dsp_resource_count;
			dcb_close(dev_handle);
		}
		dcb_close(handle);
	}

	//	DEV_CLASS_SCX
	//	DEV_CLASS_AUDIO_IN	

	sr_getboardcnt(DEV_CLASS_IPT, &board_count);
	BOOST_LOG_TRIVIAL(info) << boost::format("    ipt board count=%d.\n") % board_count;
	for (i = 1; i <= board_count; i++) {
		sprintf(board_name, ":N_iptB%d:P_IP", i);
		gc_OpenEx(&handle, board_name, EV_SYNC, NULL);
		sub_dev_count = ATDV_SUBDEVS(handle);
		BOOST_LOG_TRIVIAL(info) << boost::format("        ipt board %d(handle=%d) has %d sub-devs.\n") % i % handle % sub_dev_count;
		gc_Close(handle);
	}

	sr_getboardcnt("IPM", &board_count);
	BOOST_LOG_TRIVIAL(info) << boost::format("    ipm board count=%d.\n") % board_count;
	for (i = 1; i <= board_count; i++) {
		sprintf(board_name, ":M_ipmB%d", i);
		gc_OpenEx(&handle, board_name, EV_SYNC, NULL);
		sub_dev_count = ATDV_SUBDEVS(handle);
		BOOST_LOG_TRIVIAL(info) << boost::format("        ipm board %d(handle=%d) has %d sub-devs.\n") % i % handle % sub_dev_count;
		gc_Close(handle);
	}

	BOOST_LOG_TRIVIAL(info) << "  enum_dev_information done.\n";
}
/**
* Set authentication information for the PBX
* @param proxy_ip This can be (based on circumstance) the local IP or the PBX IP address.  Ussually the PBX IP address.
* @param alias The alias name to connect as on the PBX
* @param password The password for the alias to connect to the PBX with.
* @param realm The realm for the alias to connect to the PBX with.
*/
void authentication(int channel_index, const char* proxy_ip, const char* alias, const char* password, const char* realm)
{
	GC_PARM_BLKP gc_parm_blkp = NULL;
	IP_AUTHENTICATION auth;
	char identity[GC_ADDRSIZE] = "";
	BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: authentication()...\n") % channel_index;
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
void registration(int channel_index, const char* proxy_ip, const char* local_ip, const char* alias, const char* password, const char* realm)
{
	GC_PARM_BLKP gc_parm_blkp = NULL;
	IP_REGISTER_ADDRESS register_address;
	unsigned long serviceID = 1;
	char contact[250] = "";

	if (!registered) {
		authentication(channel_index, proxy_ip, alias, password, realm);

		BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: registration()...\n") % channel_index;

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
		BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i:   serviceID is 0x%x.\n") % channel_index % serviceID;

		registered = TRUE;
	}
}
/**
* Unregister from the SIP PBX
*/
void unregistration(int channel_index)
{
	GC_PARM_BLKP gc_parm_blkp = NULL;
	unsigned long serviceID = 1;

	if (registered) {
		BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: unregistration()...\n") %channel_index;
		gc_util_insert_parm_val(&gc_parm_blkp, IPSET_REG_INFO, IPPARM_OPERATION_DEREGISTER, sizeof(char), IP_REG_DELETE_ALL);
		gc_util_insert_parm_val(&gc_parm_blkp, GCSET_SERVREQ, PARM_REQTYPE, sizeof(unsigned char), IP_REQTYPE_REGISTRATION);
		gc_util_insert_parm_val(&gc_parm_blkp, GCSET_SERVREQ, PARM_ACK, sizeof(unsigned char), IP_REQTYPE_REGISTRATION);
		gc_util_insert_parm_val(&gc_parm_blkp, IPSET_PROTOCOL, IPPARM_PROTOCOL_BITMASK, sizeof(char), IP_PROTOCOL_SIP);
		gc_ReqService(GCTGT_CCLIB_NETIF, board_dev, &serviceID, gc_parm_blkp, NULL, EV_ASYNC);
		gc_util_delete_parm_blk(gc_parm_blkp);
		BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i:   serviceID is 0x%x.\n") % channel_index % serviceID;

		registered = FALSE;
	}
}

void print_all_cclibs_status()
{
	int cclib_status;
	GC_INFO gc_error_info; /* GlobalCall error information data */
	int result = gc_CCLibStatus("GC_DM3CC_LIB", &cclib_status);
	if (result == GC_SUCCESS)
	{
		BOOST_LOG_TRIVIAL(info) << boost::format("  cclib %s status:\n") % "GC_DM3CC_LIB";
		BOOST_LOG_TRIVIAL(info) << boost::format("   configured: %s\n") % (cclib_status & GC_CCLIB_CONFIGURED ? "yes" : "no");
		BOOST_LOG_TRIVIAL(info) << boost::format("   available: %s\n") % (cclib_status & GC_CCLIB_AVL ? "yes" : "no");
		BOOST_LOG_TRIVIAL(info) << boost::format("   failed: %s\n") % (cclib_status & GC_CCLIB_FAILED ? "yes" : "no");
		BOOST_LOG_TRIVIAL(info) << boost::format("   stub: %s\n") % (cclib_status & GC_CCLIB_STUB ? "yes" : "no");
	} else {
		/* error handling */
		gc_ErrorInfo(&gc_error_info);
		BOOST_LOG_TRIVIAL(error) << boost::format("Error: gc_CCLibStatusEx(), lib_name: %s, GC ErrorValue: 0x%hx - %s, CCLibID: %i - %s, CC ErrorValue : 0x % lx - %s\n") %
				 "GC_ALL_LIB" % gc_error_info.gcValue % gc_error_info.gcMsg %
				 gc_error_info.ccLibId % gc_error_info.ccLibName %
				 gc_error_info.ccValue % gc_error_info.ccMsg;
	}
}

/**
* Start the Dialogic Global Call API and initalize the libraries.
*/
int global_call_start(int h323_signaling_port, int sip_signaling_port, int maxCalls)
{
	GC_START_STRUCT	gclib_start;
	IPCCLIB_START_DATA ipcclibstart;
	IP_VIRTBOARD ip_virtboard[1];

	BOOST_LOG_TRIVIAL(info) << boost::format("global_call_start(h323_signaling_port=%i, sip_signaling_port=%i, maxCalls=%i)...\n") % h323_signaling_port % sip_signaling_port % maxCalls;
	
	print_all_cclibs_status();

	enum_dev_information();


	memset(&ipcclibstart, 0, sizeof(IPCCLIB_START_DATA));
	memset(ip_virtboard, 0, sizeof(IP_VIRTBOARD)*1);
	
	INIT_IPCCLIB_START_DATA(&ipcclibstart, 1, ip_virtboard);
	INIT_IP_VIRTBOARD(&ip_virtboard[0]);

	ipcclibstart.delimiter = ',';
	ipcclibstart.num_boards = 1;
	ipcclibstart.board_list = ip_virtboard;
	ipcclibstart.max_parm_data_size = 4096;

	ip_virtboard[0].localIP.ip_ver = IPVER4;					// must be set to IPVER4
	ip_virtboard[0].localIP.u_ipaddr.ipv4 = IP_CFG_DEFAULT;	// or specify host NIC IP address
	//ip_virtboard[0].localIP.u_ipaddr.ipv4 = 0x66409892;     //146.152.64.102
	//ip_virtboard[0].localIP.u_ipaddr.ipv4 = 0x5003A8C0;     //192.168.3.80 !!! Worked on my workstation!
	//ip_virtboard[0].localIP.u_ipaddr.ipv4 = 0x64638F0A;     //
	// 192 = C0
	// 168 = A8
	// 3 = 03
	// 80 = 50

	// 10 = 0A
	// 143 = 8F
	// 99 = 63
	// 100 = 64
	
	ip_virtboard[0].h323_signaling_port = h323_signaling_port;	// or application defined port for H.323 
	ip_virtboard[0].sip_signaling_port = sip_signaling_port;		// or application defined port for SIP
	ip_virtboard[0].sup_serv_mask = IP_SUP_SERV_CALL_XFER;	// Enable SIP Transfer Feature
	ip_virtboard[0].sip_msginfo_mask = IP_SIP_MSGINFO_ENABLE;// Enable SIP header
	ip_virtboard[0].reserved = NULL;							// must be set to NULL

	ip_virtboard[0].sip_max_calls = maxCalls;
	ip_virtboard[0].h323_max_calls = maxCalls;
	ip_virtboard[0].total_max_calls = maxCalls;


	CCLIB_START_STRUCT cc_Lib_Start[] = {
		{ "GC_DM3CC_LIB", NULL },
		{ "GC_H3R_LIB", &ipcclibstart },
		{ "GC_IPM_LIB", NULL } };
	gclib_start.num_cclibs = 3;
	gclib_start.cclib_list = cc_Lib_Start;
	
	int result = gc_Start(&gclib_start);
	
	if (result < 0){
		BOOST_LOG_TRIVIAL(error) << "Error Global Call Libraries could not be started. \n";
		print_gc_error_info("gc_Start", result);
	}
	else{
		started = TRUE;
		BOOST_LOG_TRIVIAL(info) << "global_call_start() done.\n";
	}
	return result;
}


/**
* Open a single channel.
*/
void open_channel(int lineNumber, int offset)
{

	if (started) {
		int channel_index = lineNumber + offset;
		BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: open_channel(%i, %i).  \n") % channel_index % lineNumber % offset;

		long request_id = 0;
		GC_PARM_BLKP gc_parm_blk_p = NULL;


		//enum_dev_information();

		int result = gc_OpenEx(&board_dev, ":N_iptB1:P_IP", EV_SYNC, NULL);
		BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: result for gc_OpenEx() = %i\n") % channel_index % result;
		BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: board_dev = %d\n") % channel_index % board_dev;

		//setting T.38 fax server operating mode: IP MANUAL mode
		gc_util_insert_parm_val(&gc_parm_blk_p, IPSET_CONFIG, IPPARM_OPERATING_MODE, sizeof(long), IP_MANUAL_MODE);

		//Enabling and Disabling Unsolicited Notification Events
		gc_util_insert_parm_val(&gc_parm_blk_p, IPSET_EXTENSIONEVT_MSK, GCACT_ADDMSK, sizeof(long),
			EXTENSIONEVT_DTMF_ALPHANUMERIC | EXTENSIONEVT_SIGNALING_STATUS | EXTENSIONEVT_STREAMING_STATUS | EXTENSIONEVT_T38_STATUS);
		gc_SetConfigData(GCTGT_CCLIB_NETIF, board_dev, gc_parm_blk_p, 0, GCUPDATE_IMMEDIATE, &request_id, EV_ASYNC);
		gc_util_delete_parm_blk(gc_parm_blk_p);

		GC_PARM_BLKP pParmBlock = 0;
		int frc = GC_SUCCESS;

		int channel = lineNumber + offset;
		channls[channel] = new ivrToolkit::DialogicSipWrapper::CHANNEL(lineNumber, offset);
		channls[channel]->open();

		int device_handle = channls[channel]->get_device_handle();
		gc_SetUsrAttr(device_handle, channls[channel]);
	}


}
/**
* Close all open channels to MAX_CHANNELS
* todo not used. I think there is a bug here?
*/ 
void close_channels(int channel_index)
{
	BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: close_channels()...\n") %channel_index;
	unregistration(channel_index);
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
int ProcessEventSync(int wait_event, long event_handle, int channel, int dev_handle)
{
	BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: ProcessEventSync(wait_event=%i, event_handle=%d, channel=%i) \n") % channel % wait_event % event_handle % channel;
	int timeout = 0;
	//long event_handle = 0;
	int evt_dev = 0;
	int evt_code = 0;
	int evt_len = 0;
	void* evt_datap = NULL;
	METAEVENT meta_evt;
	ivrToolkit::DialogicSipWrapper::CHANNEL* pch = NULL;
	GC_PARM_BLKP gc_parm_blkp = NULL;
	GC_PARM_DATAP gc_parm_datap = NULL;
	int value = 0;

	gc_GetMetaEventEx(&meta_evt, event_handle);
	//gc_GetMetaEvent(&meta_evt);

	evt_code = (int)meta_evt.evttype;
	evt_dev = (int)meta_evt.evtdev;

	BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: evt_code = %i, evt_dev = %i, evt_flags = %d, board_dev = %d, evt_type = %d, line_dev = %d, \n") % channel % evt_code % evt_dev % meta_evt.flags % board_dev % meta_evt.evttype % meta_evt.linedev;

	if (meta_evt.flags & GCME_GC_EVENT) {

		//for register
		if (evt_dev == board_dev && GCEV_SERVICERESP == meta_evt.evttype) {
			gc_parm_blkp = (GC_PARM_BLKP)(meta_evt.extevtdatap);
			gc_parm_datap = gc_util_next_parm(gc_parm_blkp, gc_parm_datap);

			while (NULL != gc_parm_datap) {
				if (IPSET_REG_INFO == gc_parm_datap->set_ID) {
					if (IPPARM_REG_STATUS == gc_parm_datap->parm_ID) {
						value = (int)gc_parm_datap->value_buf[0];
						switch (value) {
						case IP_REG_CONFIRMED:
							BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: IPSET_REG_INFO/IPPARM_REG_STATUS: IP_REG_CONFIRMED\n") % channel;
							break;
						case IP_REG_REJECTED:
							BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: IPSET_REG_INFO/IPPARM_REG_STATUS: IP_REG_REJECTED\n") % channel;
							break;
						default:
							break;
						}
						return value;
					}
					else if (IPPARM_REG_SERVICEID == gc_parm_datap->parm_ID) {
						value = (int)(gc_parm_datap->value_buf[0]);
						BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: IPSET_REG_INFO/IPPARM_REG_SERVICEID: 0x%x\n") % channel % value;
					}

				}
				else if (IPSET_LOCAL_ALIAS == gc_parm_datap->set_ID){
					char * localAlias = new char[gc_parm_datap->value_size + 1];
					localAlias = (char*)&gc_parm_datap->value_buf;
					BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: IPSET_LOCAL_ALIAS value: %s\n") % channel % localAlias;
				}
				else if (IPSET_SIP_MSGINFO == gc_parm_datap->set_ID){
					char * msgInfo = new char[gc_parm_datap->value_size + 1];
					msgInfo = (char*)&gc_parm_datap->value_buf;
					BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: IPSET_SIP_MSGINFO value: %s\n") % channel % msgInfo;
				}
				gc_parm_datap = gc_util_next_parm(gc_parm_blkp, gc_parm_datap);
			}
			//continue;
			return -1;
		}

		BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: gc_GetUsrAttr(meta_evt.linedev = %d)\n") % channel % meta_evt.linedev;
		gc_GetUsrAttr(meta_evt.linedev, (void**)&pch);
		
		if (NULL == pch) return -1;


		pch->printDebug("CHECK CHANNEL %i got GC event : %s", channel, GCEV_MSG(evt_code));
		gc_GetCRN(&pch->crn, &meta_evt);

		switch (evt_code)
		{
		case GCEV_ALERTING:
			BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: ##########ALERTING###########") % channel;
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
			pch->printDebug( "got voice event : TDX_PLAY");
			pch->process_voice_done();
			break;
		case TDX_RECORD:
			pch->printDebug("got voice event : TDX_RECORD");
			pch->process_voice_done();
			break;
		case TDX_CST:
			pch->printDebug("got voice event : TDX_CST");
			if (DE_DIGITS == ((DX_CST*)evt_datap)->cst_event) {
				pch->printDebug("DE_DIGITS: [%c]", (char)((DX_CST*)evt_datap)->cst_data);
			}
			break;

		default:
			pch->printError("unexcepted R4 event(0x%x)", evt_code);
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
	if (count > wait_time){
		return true;
	}
	
	return false;
}
/**
* Checks to see if the syncronous wrapper should loop again.
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
	BOOST_LOG_TRIVIAL(trace) << boost::format("Channel %i: WaitForEventSync(channel=%i, wait_for_event=%i, wait_time=%i)\n") % channel % channel % wait_for_event % wait_time;

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

	do
	{
		int result = sr_waitevtEx(hdls, hdlcnt, 100, &event_handle);
		//	Wait one tenth of a second for an event
		if (result != -1)
		{
			// If the event is valid, process it
			event_thrown = ProcessEventSync(wait_for_event, event_handle, channel, dev_handle);
			if (event_thrown != -1)
			{
				BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: event_thrown = %i\n") % channel % event_thrown;
			}
			if (event_thrown == wait_for_event) break;

		}
		count++;
	} while (loopAgain(event_thrown,wait_for_event,count, wait_time));

	if (event_thrown == wait_for_event){
		return SYNC_WAIT_SUCCESS;
	}
	if (hasExpired(count, wait_time)){
		return SYNC_WAIT_EXPIRED;
	}
	return SYNC_WAIT_ERROR;
}

void init_logging(int logLevel)
{
	logging::register_simple_formatter_factory<logging::trivial::severity_level, char>("Severity");

	logging::add_file_log
	(
		keywords::file_name = "ADS_CPP_LOG_%N.log",
		keywords::rotation_size = 70 * 1024 * 1024,
		keywords::format = "[%TimeStamp%] [%ThreadID%] [%Severity%] %Message%",
		keywords::auto_flush = true
	);

	logging::core::get()->set_filter
	(
		logging::trivial::severity >= logLevel
	);

	logging::add_common_attributes();
}

/**
* Syncronous Functions
*/

/**
* Starts Dialogic Libraries syncronously
*/
int DialogicFunctions::DialogicStartSync(int h323_signaling_port, int sip_signaling_port, int maxCalls, int logLevel){
	if (!started){
		init_logging(logLevel);
		return global_call_start(h323_signaling_port, sip_signaling_port, maxCalls);
	}
	return 0;
}
/**
* Stops Dialogic Libraries syncronously
*/
void DialogicFunctions::DialogicStopSync(){
	if (started){
		BOOST_LOG_TRIVIAL(info) << "DialogicFunctions::DialogicStopSync\n";
		int result = gc_Stop();
		print_gc_error_info("gc_Stop", result);
	}
}

/**
* Open a channel syncronously
* @param channel_index The channel to open.
*/
void DialogicFunctions::DialogicOpenSync(int lineNumber, int offset){
	int channel_index = lineNumber + offset;
	BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: DialogicFunctions::DialogicOpenSync\n") % channel_index;
	open_channel(lineNumber, offset);

	/*
	* I wait forever as if I cannot open the device this will keep it open
	* to show that an error is occuring.  I might want to put 
	* more logic in this and return an ERROR or SUCCESS in the future.
	*/

	if (WaitForEventSync(channel_index, GCEV_UNBLOCKED, SYNC_WAIT_INFINITE) == SYNC_WAIT_SUCCESS){
		//printf("GCEV_UNBLOCKED\n");
	}

}
/**
* Close a channel syncronously
* @param channel_index The channel to close.
*/
void DialogicFunctions::DialogicCloseSync(int channel_index){
	BOOST_LOG_TRIVIAL(info) << boost::format("Channel %i: DialogicCloseSync (%i) \n") % channel_index % channel_index;
	/*
	Due to the threading of this application never unregester until then.  Otherwise this could
	prevent calls from getting through.
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
		registration(channel_index, proxy_ip, local_ip, alias, password, realm);
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
	unregistration(channel_index);
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
	authentication(channel_index, auth_proxy_ip, auth_alias, auth_password, auth_realm);
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
		BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: status IDLE.\n") % channel_index;
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
* Wait for a call Asyncronously.
* @ param channel_index The channel to use to wait for a call.
*/
void DialogicFunctions::DialogicWaitCallAsync(int channel_index){
	//printf("DialogicWaitCallSync\n");

	channls[channel_index]->wait_call(); 
}

/**
* Wait for a event syncronously.
* @ param channel_index The channel to use to wait for.
* @ param wait_time The time to wait in 1/10 of a second increments
*/
int DialogicFunctions::DialogicWaitForCallEventSync(int channel_index, int wait_time) {
	//printf("DialogicWaitForCallEventSync\n");
	return WaitForEventSync(channel_index, GCEV_ANSWERED, wait_time);
}

/*
* ASyncronous Functions
*/

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

/*
* Shared Functions
* These functions are syncronous but they can be used interchangably with sycnronous and asyncronous funtions
*/

/**
* Get the Global Call Device name.
* @param channel_index The channel to get the name for.
*/
char* DialogicFunctions::DialogicGetDeviceName(int channel_index){
	BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: DialogicGetDeviceName\n") % channel_index;
	return channls[channel_index]->get_device_name();
}
/**
* Get the Global Call Device handle.
* Avoid using this feature unless you absolutly have to.
* It would be better to put the code inside the CHANNEL class in this code.
* @param channel_index The channel to get the global call device handle for.
*/
long DialogicFunctions::DialogicGetDeviceHandle(int channel_index){
	BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: DialogicGetDeviceHandle\n") % channel_index;
	return channls[channel_index]->get_device_handle();
}
/** 
* Get the Voice Device handle.
* Avoid using this feature unless you absolutly have to.
* It would be better to put the code inside the CHANNEL class in this code.
* @param channel_index The channel to get the voice device handle for.
*/
int DialogicFunctions::DialogicGetVoiceHandle(int channel_index){
	BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: DialogicGetVoiceHandle\n") % channel_index;
	return channls[channel_index]->get_voice_handle();
}
/**
* Delete Tones
* @param channel_index The channel to use.
*/
int DialogicFunctions::DialogicDeleteTones(int channel_index){
	BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: DialogicDeleteTones\n") % channel_index;
	return channls[channel_index]->voice_dx_deltones();
}
/**
* Change tone frequency.
* @param channel_index The channel to use.
*/
int DialogicFunctions::DialogicChangeFrequency(int channel_index, int tonetype, int fq1, int dv1, int fq2, int dv2){
	BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: DialogicChangeFrequency\n") % channel_index;
	return channls[channel_index]->voice_dx_chgfreq(tonetype, fq1, dv1, fq2, dv2);
}
/**
* Change duration.
* @param channel_index The channel to use.
*/
int DialogicFunctions::DialogicChangeDuration(int channel_index, int typetype, int on, int ondv, int off, int offdv){
	BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: DialogicChangeDuration\n") % channel_index;
	return channls[channel_index]->voice_dx_chgdur(typetype, on, ondv, off, offdv);
}
/**
* Change repitition count.
* @param channel_index The channel to use.
*/
int DialogicFunctions::DialogicChangeRepititionCount(int channel_index, int tonetype, int repcount){
	BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: DialogicChangeRepititionCount\n") % channel_index;
	return channls[channel_index]->voice_dx_chgrepcnt(tonetype, repcount);
}
/**
* Get digits.
* @param channel_index The channel to use.
*/
int DialogicFunctions::DialogicGetDigits(int channel_index){
	BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: DialogicGetDigits\n") % channel_index;
	return channls[channel_index]->voice_dx_getdig();
}
/**
* Get call state.
* @param channel_index The channel to use.
*/
int DialogicFunctions::DialogicGetCallState(int channel_index){
	BOOST_LOG_TRIVIAL(debug) << boost::format("Channel %i: DialogicGetCallState\n") % channel_index;
	return channls[channel_index]->globalcall_gc_GetCallState();
}

