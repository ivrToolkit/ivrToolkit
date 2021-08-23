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
		CRN crn;
		int vox_dev;

		// constructor
		CHANNEL(int lineNumber, int offset);
		void open();
		char* get_device_name();
		long get_device_handle() const;
		int get_voice_handle() const;
		int get_media_device_handle() const;
		void connect_voice();
		void restore_voice();
		void set_dtmf() const;
		void wait_call() const;
		void print_call_status(METAEVENT meta_evt) const;
		void print_gc_error_info(const char* func_name, int func_return) const;
		void ack_call() const;
		void accept_call() const;
		void answer_call() const;
		void make_call(const char* ani, const char* dnis);
		void drop_call() const;
		void release_call() const;
		void play_wave_file(const char* file);
		void record_wave_file();
		void process_voice_done();
		void print_voice_done_terminal_reason() const;
		void send_audio_request() const;
		void send_t38_request() const;
		void response_codec_request(BOOL accept_call) const;
		void stop() const;
		void process_extension(METAEVENT meta_evt) const;
		void set_codec(int crn_or_chan) const;
		void close() const;
		int start_call_progress_analysis() const;
		void process_cpa_result(int cpa_result) const;
		int get_salutation_length() const;
		int voice_dx_deltones() const;
		static int voice_dx_chgfreq(int tonetype, int fq1, int dv1, int fq2, int dv2);
		static int voice_dx_chgdur(int typetype, int on, int ondv, int off, int offdv);
		static int voice_dx_chgrepcnt(int tonetype, int repcount);
		int voice_dx_getdig() const;
		int globalcall_gc_GetCallState() const;
		void printDebug( const char* format, ...) const;
		void printError(const char* format, ...) const;
		void printInfo(const char* format, ...) const;
		void printTrace(const char* format, ...) const;
	};

}

#define USER_AGENT	"SRB_SIP_CLIENT"
#define USER_DISPLAY "SRB-Education"
#define HMP_SIP_PORT 5060

