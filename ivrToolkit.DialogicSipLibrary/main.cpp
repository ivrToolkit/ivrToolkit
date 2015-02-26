#include "hmp_sip.h"

int main(int argc, char* argv[])
{
	DialogicFunctions *dialogicFunctions = new DialogicFunctions();

	//dialogicFunctions->HelloWorld();
	/*
	* Testing out Asyncronous features
	*/
	dialogicFunctions->DialogicOpenAll();
	dialogicFunctions->DialogicStartAsyncEventThread();
	dialogicFunctions->DialogicCloseAll ();
	

	/*
	* Testing out Syncronous Features
	#
	# sip parameters
	#
	sip.proxy_ip=10.143.102.42
	sip.local_ip=10.143.102.220
	sip.alias=Developer1
	sip.password=password
	sip.realm=
	*/
	//int lineNumber = 0;
	//dialogicFunctions->DialogicOpenSync(lineNumber);

	//dialogicFunctions->DialogicRegisterSync("10.143.102.42","10.143.102.220","Developer1","password","");
	//dialogicFunctions->DialogicUnregisterSync();
	//dialogicFunctions->DialogicCloseSync(lineNumber);

	free(dialogicFunctions);

	return 0;
}