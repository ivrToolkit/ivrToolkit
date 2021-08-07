// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 

#include "DialogicSip.h"

#include "DialogicFunctions.h"
using namespace Runtime::InteropServices;

#include <iostream>

//Default Constructor
ivrToolkit::DialogicSipWrapper::DialogicSip::DialogicSip(): channel_index(0)
{	
	//Fill in consturctor logic
	dialogicFunctions = new DialogicFunctions();
}

// destructor cleans up managed resources
ivrToolkit::DialogicSipWrapper::DialogicSip::~DialogicSip(){
	// clean up code to release managed resource (at present there are none)
	// to avoid code duplication 
	// call finalizer to release unmanaged resources
	this->!DialogicSip();
}
//finalizer cleans up unmanaged resources
ivrToolkit::DialogicSipWrapper::DialogicSip::!DialogicSip(){
	delete dialogicFunctions;
}

int ivrToolkit::DialogicSipWrapper::DialogicSip::WStartLibraries(String^ logPath, int logLevel, int h323_signaling_port, int sip_signaling_port, int maxCalls ){
	IntPtr p = Marshal::StringToHGlobalAnsi(logPath);
	char* char_logPath = static_cast<char*>(p.ToPointer());
	return dialogicFunctions->DialogicStartSync(char_logPath, logLevel, h323_signaling_port, sip_signaling_port, maxCalls);
}
void ivrToolkit::DialogicSipWrapper::DialogicSip::WStopLibraries(){
	dialogicFunctions->DialogicStopSync();
}

void ivrToolkit::DialogicSipWrapper::DialogicSip::WOpen(int lineNumber, int offset){
	channel_index = lineNumber + offset;
	dialogicFunctions->DialogicOpenSync(lineNumber, offset);
}
void ivrToolkit::DialogicSipWrapper::DialogicSip::WClose(){
	dialogicFunctions->DialogicCloseSync(channel_index);
}
void ivrToolkit::DialogicSipWrapper::DialogicSip::WStop(){
	dialogicFunctions->DialogicStop(channel_index);
}
void ivrToolkit::DialogicSipWrapper::DialogicSip::WRegister(String^ proxy_ip, String^ local_ip, String^ alias, String^ password, String^ realm){

	IntPtr p = Marshal::StringToHGlobalAnsi(proxy_ip);
	char* char_proxy_ip = static_cast<char*>(p.ToPointer());

	p = Marshal::StringToHGlobalAnsi(local_ip);
	char* char_local_ip = static_cast<char*>(p.ToPointer());

	p = Marshal::StringToHGlobalAnsi(alias);
	char* char_alias = static_cast<char*>(p.ToPointer());

	p = Marshal::StringToHGlobalAnsi(password);
	char* char_password = static_cast<char*>(p.ToPointer());

	p = Marshal::StringToHGlobalAnsi(realm);
	char* char_realm = static_cast<char*>(p.ToPointer());

	dialogicFunctions->DialogicRegisterSync(channel_index, char_proxy_ip, char_local_ip, char_alias, char_password, char_realm);
}
void ivrToolkit::DialogicSipWrapper::DialogicSip::WUnregister(){
	dialogicFunctions->DialogicUnregisterSync(channel_index);
}

int ivrToolkit::DialogicSipWrapper::DialogicSip::WMakeCall(String^ ani, String^ dnis){

	IntPtr p = Marshal::StringToHGlobalAnsi(ani);
	char* char_ani = static_cast<char*>(p.ToPointer());

	p = Marshal::StringToHGlobalAnsi(dnis);
	char* char_dnis = static_cast<char*>(p.ToPointer());

	return dialogicFunctions->DialogicMakeCallSync(channel_index, char_ani, char_dnis);
}

void ivrToolkit::DialogicSipWrapper::DialogicSip::WWaitCallAsync(){
	dialogicFunctions->DialogicWaitCallAsync(channel_index);
}

int ivrToolkit::DialogicSipWrapper::DialogicSip::WWaitForCallEventSync(int wait_time) {
	return dialogicFunctions->DialogicWaitForCallEventSync(channel_index, wait_time);
}

void ivrToolkit::DialogicSipWrapper::DialogicSip::WDropCall(){
	dialogicFunctions->DialogicDropCallSync(channel_index);
}

int ivrToolkit::DialogicSipWrapper::DialogicSip::WGetVoiceHandle(){
	return dialogicFunctions->DialogicGetVoiceHandle(channel_index);
}
int ivrToolkit::DialogicSipWrapper::DialogicSip::WGetCallState(){
	return dialogicFunctions->DialogicGetCallState(channel_index);
}