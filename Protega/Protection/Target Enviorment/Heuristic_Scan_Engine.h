#pragma once
#include <sstream>
#include <list>
#include "TlHelp32.h"
#include <boost/algorithm/string.hpp>
#include <algorithm>

class Heuristic_Scan_Engine
{
private:
	std::list<DWORD> lProcessIDs;
	std::list<std::wstring> lBlackListProcessNames;
	std::list<std::string> lBlackListWindowNames;
	std::list<std::string> lBlackListClassNames;
	std::list<std::string> lBlackListMd5Values;

	std::function<void(std::wstring sDetectedValue, std::list<std::string> lOtherInformation) > funcErrorCallbackHandler;

	void GetCurrentProcessNamesAndPIDs(std::list<std::wstring>& lProcessNames, std::list<DWORD>& lProcessIDs);

	

public:
	Heuristic_Scan_Engine(std::list<std::wstring> lBlackListProcessNames,
		std::list<std::string> lBlackListWindowNames,
		std::list<std::string> lBlackListClassNames,
		std::list<std::string> lBlackListMd5Values,
		std::function<void(std::wstring sDetectedValue, std::list<std::string> lOtherInformation) > funcErrorCallbackHandler);
	~Heuristic_Scan_Engine();

	//Main thread functions
	bool DoScanProcessNames();
	bool ScanWindowNames();
	bool ScanClassNames();
	bool ScanProcessMd5Hash();

};

