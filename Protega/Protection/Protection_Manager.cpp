#include "../stdafx.h"
#include "Protection_Manager.h"

//Debug constructor
Protection_Manager::Protection_Manager(std::string sTargetApplication, double dThreadResponseDelta)
{
	this->dThreadResponseDelta = dThreadResponseDelta;
	//Get Target process id
	iTargetProcessId = GetProcessIdByName(strdup(sTargetApplication.c_str()));
	//	VMP
	VMP = new Virtual_Memory_Protection_Cabal_Online(iTargetProcessId,
		std::bind(&Protection_Manager::VMP_Callback, this, std::placeholders::_1, std::placeholders::_2, std::placeholders::_3, std::placeholders::_4));
}

Protection_Manager::Protection_Manager(std::function<void(std::list<std::wstring> lDetectionInformation)> funcCallbackHandler, 
	std::string sTargetApplication,
	double dThreadResponseDelta,
	std::list<std::wstring> lBlackListProcessNames, 
	std::list<std::string> lBlackListWindowNames, 
	std::list<std::string> lBlackListClassNames, 
	std::list<std::string> lBlackListMd5Values)
{
	this->dThreadResponseDelta = dThreadResponseDelta;
	this->funcCallbackHandler = funcCallbackHandler;
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


Protection_Manager::~Protection_Manager()
{
}


//Public
bool Protection_Manager::StartProtectionThreads()
{
	iProtectionIsRunning = true;
	//Create threads
	tHeThread = new std::thread(&Protection_Manager::HE_Thread, this);
	tVmpThread = new std::thread(&Protection_Manager::VMP_Thread, this);
	//tFpThread = new std::thread(&Protection_Manager::FP_Thread, this);

	//Set clocks
	ctHeResponse = std::clock();
	ctVmpResponse = std::clock();
	//ctFpResponse = std::clock();

	//Start threads
	tHeThread->join();
	tVmpThread->join();
	//tFpThread->join();
	return true;
}

std::clock_t * Protection_Manager::GetMainThreadClock()
{
	return &ctMainThreadResponse;
}

bool Protection_Manager::CheckClocks(std::clock_t* ctOwnClock)
{
	double dCurrentDuration = 0.0;
	//Reset own clock
	*ctOwnClock = std::clock();

	//Check all clocks
	//	Main Thread
	dCurrentDuration = (std::clock() - ctMainThreadResponse) / (double)CLOCKS_PER_SEC;

	if (dCurrentDuration > dThreadResponseDelta)
	{
		return false;
	}
	//	HE
	dCurrentDuration = (std::clock() - ctHeResponse) / (double)CLOCKS_PER_SEC;

	if (dCurrentDuration > dThreadResponseDelta)
	{
		return false;
	}
	//	VMP
	dCurrentDuration = (std::clock() - ctVmpResponse) / (double)CLOCKS_PER_SEC;

	if (dCurrentDuration > dThreadResponseDelta)
	{
		return false;
	}
	//	FP
	/*dCurrentDuration = (std::clock() - ctFpResponse) / (double)CLOCKS_PER_SEC;

	if (dCurrentDuration > dThreadResponseDelta)
	{
	return false;
	}*/

	return true;
}

//Private
//	Threads
void Protection_Manager::VMP_Thread()
{
	VMP->OpenProcessInstance();
	do
	{
		if (VMP->CheckAllVmpFunctions() == true)
		{
			VMP->CloseProcessInstance();
			iProtectionIsRunning = false;
			return;
		}
		CheckClocks(&ctVmpResponse);
		Sleep(500);
	} while (iProtectionIsRunning);
}

void Protection_Manager::HE_Thread()
{
	do
	{
		HE->DoScanProcessNames();
		CheckClocks(&ctHeResponse);
		HE->ScanProcessMd5Hash();
		CheckClocks(&ctHeResponse);
		Sleep(500);
	} while (iProtectionIsRunning);
}

void Protection_Manager::FP_Thread()
{
	do
	{
		MessageBoxA(0, "FP_Thread", "ctFpResponse", MB_OK);
		CheckClocks(&ctFpResponse);
		Sleep(500);
	} while (iProtectionIsRunning);
}

//	Callbacks
void Protection_Manager::HE_Callback(std::wstring sDetectionValue)
{
	std::list<std::wstring> lDetectionInformation;
	lDetectionInformation.push_back(L"HE");
	lDetectionInformation.push_back(sDetectionValue);
	funcCallbackHandler(lDetectionInformation);
}

void Protection_Manager::VMP_Callback(std::string sDetectedBaseAddress, std::string sDetectedOffset, std::string sDetectedValue, std::string sDefaultValue)
{
	std::wstring wsDetectedBaseAddress;
	std::wstring wsDetectedOffset;
	std::wstring wsDetectedValue;
	std::wstring wsDefaultValue;

	StringToWString(sDetectedBaseAddress, &wsDetectedBaseAddress);
	StringToWString(sDetectedOffset, &wsDetectedOffset);
	StringToWString(sDetectedValue, &wsDetectedValue);
	StringToWString(sDefaultValue, &wsDefaultValue);

	std::list<std::wstring> lDetectionInformation;
	lDetectionInformation.push_back(L"VMP");	
	lDetectionInformation.push_back(wsDetectedBaseAddress);
	lDetectionInformation.push_back(wsDetectedOffset);
	lDetectionInformation.push_back(wsDetectedValue);
	lDetectionInformation.push_back(wsDefaultValue);

	funcCallbackHandler(lDetectionInformation);
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

void Protection_Manager::StringToWString(std::string sStringToConvert, std::wstring* wsOutput)
{
	std::wstring ws(sStringToConvert.size(), L' '); // Overestimate number of code points.
	ws.resize(std::mbstowcs(&ws[0], sStringToConvert.c_str(), sStringToConvert.size())); // Shrink to fit.
	*wsOutput = ws;
}

