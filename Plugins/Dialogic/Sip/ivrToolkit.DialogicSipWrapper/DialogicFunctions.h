// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 
#pragma once
#include <stdexcept>

#define MAX_CHANNELS					2

#define H323_SIP_PORT					1720
#define SYNC_WAIT_INFINITE -1
#define SYNC_WAIT_EXPIRED -2
#define SYNC_WAIT_ERROR -1
#define SYNC_WAIT_SUCCESS 1


class DialogicFunctions
{
public:

	/*
	* Syncronous Functions
	* Syncronous functions are wrapper with their own event processor.  
	* It is strongly discourged from using Asyncronous and Syncronous functions 
	* together as this could result in race conditions.
	* 
	*/
	/*Start Functions*/
	int DialogicStartSync(int h323_signaling_port, int sip_signaling_port, int maxCalls, int logLevel);
	void DialogicStopSync();
	/*Channel Functions*/
	void DialogicOpenSync(int lineNumber, int offset);
	void DialogicCloseSync(int channel_index);
	/*Registration Functions*/
	void DialogicRegisterSync(int channel_index, const char* proxy_ip, const char* local_ip, const char* alias, const char* password, const char* realm);
	void DialogicUnregisterSync(int channel_index);
	/*Call Functions*/
	void DialogicStopSync(int channel_index);
	int DialogicMakeCallSync(int channel_index, const char* ani, const char* dnis);
	int DialogicDropCallSync(int channel_index);


	void DialogicWaitCallAsync(int channel_index);
	int DialogicWaitForCallEventSync(int channel_index, int wait_time);

	/*
	* Asyncronous Functions
	* ASyncornous Functions must have an Async Event Thread running in order to get event results.
	*/
	/*Registration Functions*/
	void DialogicUnregister();
	/*Call Functions*/
	void DialogicStatus();
	void DialogicPlayFile(int channel_index, const char* filename);
	void DialogicRecordFile(int channel_index);
	void DialogicStop(int channel_index);
	void DialogicMakeCall(int channel_index, const char* ani, const char* dnis);
	void DialogicDropCall(int channel_index);

	/*
	* Shared Functions
	* These functions are syncronous but they can be used interchangably with sycnronous and asyncronous funtions
	*/
	char* DialogicGetDeviceName(int channel_index);
	long DialogicGetDeviceHandle(int channel_index);
	int DialogicGetVoiceHandle(int channel_index);
	int DialogicDeleteTones(int channel_index);
	int DialogicChangeFrequency(int channel_index, int tonetype, int fq1, int dv1, int fq2, int dv2);
	int DialogicChangeDuration(int channel_index, int typetype, int on, int ondv, int off, int offdv);
	int DialogicChangeRepititionCount(int channel_index, int tonetype, int repcount);
	int DialogicGetDigits(int channel_index);
	int DialogicGetCallState(int channel_index);
};
