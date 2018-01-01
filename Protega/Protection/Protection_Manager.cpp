#include "../stdafx.h"
#include "Protection_Manager.h"

//Debug constructor
Protection_Manager::Protection_Manager()
{
	this->dThreadResponseDelta = 1000;
}

Protection_Manager::Protection_Manager(std::string sTargetApplication,
	double dThreadResponseDelta,
	std::list<std::wstring> lBlackListProcessNames, 
	std::list<std::string> lBlackListWindowNames, 
	std::list<std::string> lBlackListClassNames, 
	std::list<std::string> lBlackListMd5Values)
{
	this->dThreadResponseDelta = dThreadResponseDelta;
	//Get Target process id
	iTargetProcessId = GetProcessIdByName(strdup(sTargetApplication.c_str()));

	//Build protection classes
	//	HE
	HE = new Heuristic_Scan_Engine(lBlackListProcessNames, lBlackListWindowNames, lBlackListClassNames, lBlackListMd5Values, 
		std::bind(&Protection_Manager::HE_Callback, this, std::placeholders::_1));
	//	VMP
	VMP = new Virtual_Memory_Protection_Cabal_Online(iTargetProcessId,
		std::bind(&Protection_Manager::VMP_Callback, this, std::placeholders::_1, std::placeholders::_2, std::placeholders::_3, std::placeholders::_4));
	//	File
	FP = new File_Protection_Engine();

}
//Public
bool Protection_Manager::StartProtectionThreads()
{
	iProtectionIsRunning = true;
	//Create threads
	tTestThread = new std::thread(&Protection_Manager::HE_Thread, this);
	tTestThread->join();
	return false;
}

Protection_Manager::~Protection_Manager()
{
}


//Private
//	Threads
void Protection_Manager::VMP_Thread()
{

}

void Protection_Manager::HE_Thread()
{
	do
	{
		MessageBoxA(0, "tTestThread", "ctHeResponse", MB_OK);
		CheckClocks(&ctHeResponse);
		Sleep(500);
	} while (iProtectionIsRunning);
}

void Protection_Manager::FP_Thread()
{

}
//	Callbacks
void Protection_Manager::HE_Callback(std::wstring sDetectionValue)
{

}

void Protection_Manager::VMP_Callback(std::string sDetectedBaseAddress, std::string sDetectedOffset, std::string sDetectedValue, std::string sDefaultValue)
{

}

bool Protection_Manager::CheckClocks(std::clock_t* ctOwnClock)
{
	double dCurrentDuration = 0.0;
	//Reset own clock
	*ctOwnClock = std::clock();

	//Check all clocks
	//	HE
	dCurrentDuration = (std::clock() - ctHeResponse) / (double)CLOCKS_PER_SEC;

	if (dCurrentDuration > dThreadResponseDelta)
	{
		return false;
	}

	//	VMP

	//	FP


	return true;
}
//	Normal functions
int Protection_Manager::GetProcessIdByName(char* ProcName) {
	PROCESSENTRY32 pe32;
	HANDLE hSnapshot = NULL;
	pe32.dwSize = sizeof(PROCESSENTRY32);
	hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);

	if (Process32First(hSnapshot, &pe32)) {
		do {
			_bstr_t sPe32ExeFile(pe32.szExeFile);
			if (strcmp(sPe32ExeFile, ProcName) == 0)
				break;
		} while (Process32Next(hSnapshot, &pe32));
	}

	if (hSnapshot != INVALID_HANDLE_VALUE)
		CloseHandle(hSnapshot);

	return pe32.th32ProcessID;
}