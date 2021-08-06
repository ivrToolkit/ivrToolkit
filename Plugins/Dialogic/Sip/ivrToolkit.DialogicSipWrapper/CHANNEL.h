// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 
#pragma once
#include <gclib.h>
#include <boost/log/trivial.hpp>

namespace ivrToolkit::DialogicSipWrapper
{
	public class CHANNEL
	{
		long gc_dev;
		int ipm_dev;

		long ip_xslot;
		long vox_xslot;

		DV_TPT tpt;
		DX_XPB xpb;
		DX_IOTT vox_iott;

		int _id;
		int _lineNumber;
		int _offset;
		char device_name[64];

	public:
		BOOL fax_proceeding;
		CRN crn;
		BOOL already_connect_fax;
		int vox_dev;

		// constructor
		CHANNEL(int lineNumber, int offset);
		void open();
		char* get_device_name();
		long get_device_handle();
		int get_voice_handle();
		int get_media_device_handle();
		void connect_voice();
		void restore_voice();
		void set_dtmf();
		void wait_call();
		void print_call_status(METAEVENT meta_evt);
		void print_gc_error_info(const char* func_name, int func_return);
		void ack_call();
		void accept_call();
		void answer_call();
		void make_call(const char* ani, const char* dnis);
		void drop_call();
		void release_call();
		void play_wave_file(const char* file);
		void record_wave_file();
		void process_voice_done();
		void print_voice_done_terminal_reason();
		void send_audio_request();
		void send_t38_request();
		void response_codec_request(BOOL accept_call);
		void stop();
		void process_extension(METAEVENT meta_evt);
		void set_codec(int crn_or_chan);
		void close();
		int start_call_progress_analysis();
		void process_cpa_result(int cpa_result);
		int get_salutation_length();
		int voice_dx_deltones();
		int voice_dx_chgfreq(int tonetype, int fq1, int dv1, int fq2, int dv2);
		int voice_dx_chgdur(int typetype, int on, int ondv, int off, int offdv);
		int voice_dx_chgrepcnt(int tonetype, int repcount);
		int voice_dx_getdig();
		int globalcall_gc_GetCallState();
		void printDebug( const char* format, ...);
		void printError(const char* format, ...);
		void printInfo(const char* format, ...);
		void printTrace(const char* format, ...);
	};

}

#define USER_AGENT	"SRB_SIP_CLIENT"
#define USER_DISPLAY "SRB-Education"
#define HMP_SIP_PORT 5060

