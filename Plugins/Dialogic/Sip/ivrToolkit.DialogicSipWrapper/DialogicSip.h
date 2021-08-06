// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 
// DialogicSipWrapper.h
#pragma once

#include "DialogicFunctions.h"

using namespace System;

namespace ivrToolkit::DialogicSipWrapper {
	/*
	* This class only provides methods that are syncronous.  This helps prevent any confusion between 
	* using syncornous and asyncronous features of the wrapper.
	*/
	public ref class DialogicSip
	{
	public:
		// constructor
		DialogicSip();
		// destructor cleans up managed resources
		~DialogicSip();
		//finalizer cleans up unmanaged resources
		!DialogicSip();

		//wrapper methods
		int WStartLibraries(int h323_signaling_port, int sip_signaling_port, int maxCalls, int logLevel);
		void WStopLibraries();
		void WOpen(int lineNumber, int offset);
		void WClose();
		void WStop();
		void WRegister(String^ proxy_ip, String^ local_ip, String^ alias, String^ password, String^ realm);
		void WUnregister();
		int WMakeCall(String^ ani, String^ dnis);

		void WWaitCallAsync();
		int WWaitForCallEventSync(int wait_time);

		void WDropCall();
		String^ WGetDeviceName();
		int WGetVoiceHandle();
		int WGetCallState();

	private:
		DialogicFunctions *dialogicFunctions;
		int channel_index;

	};
}
