#include <Windows.h>
#include <string>
#include <sstream>
#include <algorithm>
#include <map>
#include "Common.h"
#include "PayloadHelper.h"

using namespace std;

int main(int argc, char** argv)
{
	//for (int i = 0; i < argc; i++)
	//	printf("argv[%d]: %s\n", i, argv[i]);

	BYTE isDebug = atoi(argv[1]);
	string clientPath = string(argv[2]);
	string clientArgs = string(argv[3]);
	string libraryPath = string(argv[4]);

	string libraryRedirectIP = string(argv[5]);
	WORD libraryRedirectPort = atoi(argv[6]);
	BYTE libraryRealAddressCount = atoi(argv[7]);
	vector<string> libraryRealAddresses;
	for (size_t i = 0; i < libraryRealAddressCount; i++)
		libraryRealAddresses.push_back(string(argv[8 + i]));

	WORD libraryRealPort = atoi(argv[8 + libraryRealAddressCount]);

	stringstream commandLine;
	commandLine << "\"" << clientPath << "\" " << clientArgs;

	STARTUPINFOA si = { 0 };
	PROCESS_INFORMATION pi = { 0 };
	si.cb = sizeof(STARTUPINFOA);
	bool result = CreateProcessA(0, (LPSTR)commandLine.str().c_str(), 0, NULL, FALSE, CREATE_SUSPENDED, NULL, NULL, &si, &pi);
	if (result == false)
	{
		MessageBoxA(0, "Could not start \"sro_client.exe\".", "Fatal Error", MB_ICONERROR);
		return 0;
	}

	stringstream payloadPath;
	payloadPath << getenv("TMP") << "\\rsLoader_" << pi.dwProcessId << ".dat";

	ofstream stream(payloadPath.str(), ofstream::binary);

	PayloadWrite(stream, isDebug);
	PayloadWriteString(stream, libraryRedirectIP);
	PayloadWrite(stream, libraryRedirectPort);
	PayloadWrite(stream, libraryRealAddressCount);
	for (size_t i = 0; i < libraryRealAddressCount; i++)
		PayloadWriteString(stream, libraryRealAddresses[i]);
	PayloadWrite(stream, libraryRealPort);

	stream.flush();
	stream.close();

	InjectDLL(pi.hProcess, GetFileEntryPoint(clientPath.c_str()), libraryPath.c_str(), "Initialize");

	ResumeThread(pi.hThread);
	ResumeThread(pi.hProcess);

	return pi.dwProcessId;
}