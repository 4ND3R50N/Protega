#pragma once
#include <list>
#include <windows.h>
#include <stdio.h>
#include <tchar.h>
#include <psapi.h>



class Heuristic_Scan_Engine
{
private:
	std::list<std::string> lProcessNames;
	std::list<std::string> lWindowNames;
	std::list<std::string> lClassNames;
	std::list<std::string> lMd5Values;

	void GetCurrentProcessIdentificators(DWORD* aProcesses, DWORD& cbNeeded);
	std::wstring GetProcessName(DWORD dwProcessID);


public:
	Heuristic_Scan_Engine();
	~Heuristic_Scan_Engine();

	//Main thread functions
	bool DoScanProcessNames();
	bool ScanWindowNames();
	bool ScanClassNames();
	bool ScanProcessMd5Hash();

};

