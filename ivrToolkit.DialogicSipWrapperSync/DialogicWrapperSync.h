// DialogicWrapperSync.h
#pragma once

#include "../ivrToolkit.DialogicSipLibrary/hmp_sip.h"
#include "../ivrToolkit.DialogicSipLibrary/hmp_sip.cpp"

using namespace System;

namespace DialogicWrapperSync {
	/*
	* This class only provides methods that are syncronous.  This helps prevent any confusion between 
	* using syncornous and asyncronous features of the wrapper.
	*/
	public ref class DialogicSIPSync
	{
	public:
		// constructor
		DialogicSIPSync();
		// destructor cleans up managed resources
		~DialogicSIPSync();
		//finalizer cleans up unmanaged resources
		!DialogicSIPSync();

		//wrapper methods
		void WStartLibraries(int h323_signaling_port, int sip_signaling_port);
		void WStopLibraries();
		void WOpen(int channel_index);
		void WClose();
		void WStop();
		void WRegister(String^ proxy_ip, String^ local_ip, String^ alias, String^ password, String^ realm);
		void WUnregister();
		void WStatus();
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
