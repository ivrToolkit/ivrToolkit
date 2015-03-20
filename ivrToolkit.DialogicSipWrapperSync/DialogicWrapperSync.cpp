// This is the main DLL file.

#include "stdafx.h"
#include "../ivrToolkit.DialogicSipLibrary/hmp_sip.h"
#include "../ivrToolkit.DialogicSipLibrary/hmp_sip.cpp"
#include "DialogicWrapperSync.h"
using namespace System::Runtime::InteropServices;

//Default Constructor
DialogicWrapperSync::DialogicSIPSync::DialogicSIPSync(){
//Fill in consturctor logic
	dialogicFunctions = new DialogicFunctions();
}
// destructor cleans up managed resources
DialogicWrapperSync::DialogicSIPSync::~DialogicSIPSync(){
	// clean up code to release managed resource (at present there are none)
	// to avoid code duplication 
	// call finalizer to release unmanaged resources
	this->!DialogicSIPSync();
}
//finalizer cleans up unmanaged resources
DialogicWrapperSync::DialogicSIPSync::!DialogicSIPSync(){
	delete dialogicFunctions;
}

void DialogicWrapperSync::DialogicSIPSync::WStartLibraries(){
	dialogicFunctions->DialogicStartSync();
}
void DialogicWrapperSync::DialogicSIPSync::WStopLibraries(){
	dialogicFunctions->DialogicStopSync();
}

void DialogicWrapperSync::DialogicSIPSync::WOpen(int num_channel_index){
	channel_index = num_channel_index;
	dialogicFunctions->DialogicOpenSync(channel_index);
}
void DialogicWrapperSync::DialogicSIPSync::WClose(){
	dialogicFunctions->DialogicCloseSync(channel_index);
}
void DialogicWrapperSync::DialogicSIPSync::WStop(){
	dialogicFunctions->DialogicStop(channel_index);
}
void DialogicWrapperSync::DialogicSIPSync::WRegister(String^ proxy_ip, String^ local_ip, String^ alias, String^ password, String^ realm){

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
void DialogicWrapperSync::DialogicSIPSync::WUnregister(){
	dialogicFunctions->DialogicUnregisterSync(channel_index);
}
void DialogicWrapperSync::DialogicSIPSync::WStatus(){
	printf("WStatus %i \n", channel_index);
	printf("WFeature not yet implemented\n");
	//dialogicFunctions->DialogicStatus();
}
int DialogicWrapperSync::DialogicSIPSync::WMakeCall(String^ ani, String^ dnis){

	IntPtr p = Marshal::StringToHGlobalAnsi(ani);
	char* char_ani = static_cast<char*>(p.ToPointer());

	p = Marshal::StringToHGlobalAnsi(dnis);
	char* char_dnis = static_cast<char*>(p.ToPointer());

	return dialogicFunctions->DialogicMakeCallSync(channel_index, char_ani, char_dnis);
}
int DialogicWrapperSync::DialogicSIPSync::WWaitCall(){
	return dialogicFunctions->DialogicWaitCallSync(channel_index);
}

void DialogicWrapperSync::DialogicSIPSync::WDropCall(){
	dialogicFunctions->DialogicDropCallSync(channel_index);
}
String^ DialogicWrapperSync::DialogicSIPSync::WGetDeviceName(){
	const char* device_name = dialogicFunctions->DialogicGetDeviceName(channel_index);
	String^ clistr = gcnew String(device_name);
	return clistr;
}
int DialogicWrapperSync::DialogicSIPSync::WGetVoiceHandle(){
	return dialogicFunctions->DialogicGetVoiceHandle(channel_index);
}
int DialogicWrapperSync::DialogicSIPSync::WGetCallState(){
	return dialogicFunctions->DialogicGetCallState(channel_index);
}