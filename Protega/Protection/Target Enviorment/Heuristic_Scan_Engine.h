#pragma once
#include <psapi.h>
#include <tchar.h>
#include <iostream>
#include <sstream>
#include <list>
#include "TlHelp32.h"
#include <boost/algorithm/string.hpp>
#include <algorithm>
#include <comdef.h> 
#include "../../Tools/CryptoPP_Converter.h"


#ifdef _UNICODE
#define tcout wcout
#define tcerr wcerr
#else
#define tcout cout
#define tcerr cerr
#endif

class Heuristic_Scan_Engine
{
private:
	std::list<DWORD> lProcessIDs;
	std::list<std::wstring> lBlackListProcessNames;
	std::list<std::string> lBlackListWindowNames;
	std::list<std::string> lBlackListClassNames;
	std::list<std::string> lBlackListMd5Values;

	std::function<void(std::wstring sDetectedValue) > funcErrorCallbackHandler;

	void GetCurrentProcessNamesAndPIDs(std::list<std::wstring>& lProcessNames, std::list<DWORD>& lProcessIDs);
	std::string GetMD5Hash(const char* sFilePath);
	

public:
	Heuristic_Scan_Engine(std::list<std::wstring> lBlackListProcessNames,
		std::list<std::string> lBlackListWindowNames,
		std::list<std::string> lBlackListClassNames,
		std::list<std::string> lBlackListMd5Values,
		std::function<void(std::wstring sDetectedValue) > funcErrorCallbackHandler);
	~Heuristic_Scan_Engine();

	//Main thread functions
	bool DoScanProcessNames();
	bool ScanWindowNames();
	bool ScanClassNames();
	bool ScanProcessMd5Hash();

};

