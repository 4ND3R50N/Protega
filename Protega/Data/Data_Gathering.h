#pragma once
#include <iostream>
#include <curl\curl.h>
#include <boost\algorithm/string.hpp>
#include <Windows.h>
#include <tchar.h>
#include <intrin.h> 

#define INFO_BUFFER_SIZE 32767

class Data_Gathering
{
private:
	//Vars
	typedef BOOL(WINAPI *LPFN_ISWOW64PROCESS) (HANDLE, PBOOL);

	//Functions
	static void TCharToChar(const wchar_t* Src, char* Dest, int Size);
	Data_Gathering() {}
public:
	//Http
	static bool DownloadWebFile(char* sTarget, char sDestination[FILENAME_MAX]);
	static std::string GetWebFileAsString(const char* sTargetURL);

	//Hardware
	static bool is64BitOS();
	static uint16_t getVolumeHash();
	static uint16_t getCpuHash();
	static std::string getMachineName();
	
};
