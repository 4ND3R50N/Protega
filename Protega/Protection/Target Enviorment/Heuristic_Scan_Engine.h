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
#include <codecvt>
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
	std::vector<std::wstring> vBlackListProcessNames;
	std::vector<std::string> vBlackListWindowNames;
	std::vector<std::string> vBlackListClassNames;
	std::vector<std::string> vBlackListMd5Values;

	bool bIsAtStart = true;
	std::list<DWORD> lCurrentProcessIDsTmp;
	std::list<DWORD>::iterator itDwProcessID;

	std::function<void(std::string sSection, std::string sDetectedValue) > funcErrorCallbackHandler;

	void GetCurrentProcessNamesAndPIDs(std::list<std::wstring>& lProcessNames, std::list<DWORD>& lProcessIDs);
	std::string GetMD5Hash(const char* sFilePath);
	

public:
	Heuristic_Scan_Engine(std::vector<std::wstring> lBlackListProcessNames,
		std::vector<std::string> lBlackListWindowNames,
		std::vector<std::string> lBlackListClassNames,
		std::vector<std::string> lBlackListMd5Values,
		std::function<void(std::string sSection, std::string sDetectedValue) > funcErrorCallbackHandler);
	~Heuristic_Scan_Engine();

	//Main thread functions
	bool DetectBlacklistedProcessNames();
	bool DetectBlacklistedWindowNames();
	bool DetectBlacklistedClassNames();
	bool DetectBlacklistedProcessMd5Hash();

};

