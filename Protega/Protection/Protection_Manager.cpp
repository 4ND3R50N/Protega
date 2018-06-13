#include "../stdafx.h"
#include "Protection_Manager.h"

Protection_Manager::Protection_Manager(std::function<void(unsigned int iType, std::vector<std::string> lDetectionInformation)> funcCallbackHandler,
	int iTargetApplicationId,
	double dThreadResponseDelta,
	int iVMErrorCode,
	int iFPErrorCode,
	int iThreadErrorCode,
	int iFPMaxDlls,
	std::string sBaseFolder,
	std::vector<std::wstring> vBlackListProcessNames,
	std::vector<std::string> vBlackListWindowNames,
	std::vector<std::string> vBlackListClassNames,
	std::vector<std::string> vBlackListMd5Values, 
	std::pair<std::vector<std::string>, std::vector<std::string>> pFilesAndMd5)
{
	this->dThreadResponseDelta = dThreadResponseDelta;
	this->iVMErrorCode = iVMErrorCode;
	this->iFPErrorCode = iFPErrorCode;
	this->iThreadErrorCode = iThreadErrorCode;
	this->funcCallbackHandler = funcCallbackHandler;
	//Get Target process id
	iTargetProcessId = iTargetApplicationId;

	if (iTargetProcessId == 0)
	{
		Exception_Manager::HandleProtegaStandardError(iVMErrorCode,
			"Not able to get access to the target Process. Please restart the application as admin. If this problem accours more often, please contact the administrator! [1]");
	}

	//Build protection classes
	//	HE
	HE = new Heuristic_Scan_Engine(vBlackListProcessNames, vBlackListWindowNames, vBlackListClassNames, vBlackListMd5Values, 
		std::bind(&Protection_Manager::HE_Callback, this, std::placeholders::_1, std::placeholders::_2));
	//	VMP
	VMP = new Virtual_Memory_Protection_Cabal_Online(iTargetProcessId,
		std::bind(&Protection_Manager::VMP_Callback, this, std::placeholders::_1, std::placeholders::_2, std::placeholders::_3, std::placeholders::_4));
	//	File
	FP = new File_Protection_Engine(iTargetProcessId, sBaseFolder,
		std::bind(&Protection_Manager::FP_Callback, this, std::placeholders::_1, std::placeholders::_2), pFilesAndMd5, iFPMaxDlls);

}

Protection_Manager::~Protection_Manager()
{
}


//Public
//	Classless thread starter


bool Protection_Manager::StartProtectionThreads()
{
	bProtectionIsRunning = true;

	//VMP -> Connect to process
	if (!VMP->OpenProcessInstance())
	{
		Exception_Manager::HandleProtegaStandardError(iVMErrorCode,
			"Not able to get access to the target Process. Please restart the application as admin. If this problem accours more often, please contact the administrator! [2]");
		return false;
	}

	//Create threads
	tHeThread = new std::thread(&Protection_Manager::HE_Thread, this);
	tVmpIfThread = new std::thread(&Protection_Manager::VMP_IF_Thread, this);
	tVmpNifThread = new std::thread(&Protection_Manager::VMP_NIF_Thread, this);
	tFpThread = new std::thread(&Protection_Manager::FP_Thread, this);

	//Set clocks
	ctHeResponse = std::clock();
	ctVmpIfResponse = std::clock();
	ctVmpNifResponse = std::clock();
	ctFpResponse = std::clock();

	return true;
}

bool Protection_Manager::ProtectionIsRunning()
{
	return bProtectionIsRunning;
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
		bProtectionIsRunning = false;
		Exception_Manager::HandleProtegaStandardError(iThreadErrorCode,
			"Thread Error [M]. Please restart the application!");
		return false;
	}
	//	HE
	dCurrentDuration = (std::clock() - ctHeResponse) / (double)CLOCKS_PER_SEC;

	if (dCurrentDuration > dThreadResponseDelta)
	{
		bProtectionIsRunning = false;
		Exception_Manager::HandleProtegaStandardError(iThreadErrorCode,
			"Thread Error [HE]. Please restart the application!");
		return false;
	}
	//	VMP 1&2

	dCurrentDuration = (std::clock() - ctVmpIfResponse) / (double)CLOCKS_PER_SEC;

	if (dCurrentDuration > dThreadResponseDelta)
	{
		bProtectionIsRunning = false;
		Exception_Manager::HandleProtegaStandardError(iThreadErrorCode,
			"Thread Error [VM_IF]. Please restart the application!");
		return false;
	}

	dCurrentDuration = (std::clock() - ctVmpNifResponse) / (double)CLOCKS_PER_SEC;

	if (dCurrentDuration > dThreadResponseDelta)
	{
		bProtectionIsRunning = false;
		Exception_Manager::HandleProtegaStandardError(iThreadErrorCode,
			"Thread Error [VM_NIF]. Please restart the application!");
		return false;
	}
	//	FP
	dCurrentDuration = (std::clock() - ctFpResponse) / (double)CLOCKS_PER_SEC;

	if (dCurrentDuration > dThreadResponseDelta)
	{
		bProtectionIsRunning = false;
		Exception_Manager::HandleProtegaStandardError(iThreadErrorCode,
			"Thread Error [FP]. Please restart the application!");
		return false;
	}

	return true;
}

//Private

//	Threads

void Protection_Manager::HE_Thread()
{
	do
	{
		if (HE->DetectBlacklistedProcessNames())
		{
			bProtectionIsRunning = false;
			return;
		}
		CheckClocks(&ctHeResponse);

		if (HE->DetectBlacklistedProcessMd5Hash())
		{
			bProtectionIsRunning = false;
			return;
		}
		CheckClocks(&ctHeResponse);

		Sleep(500);
	} while (bProtectionIsRunning);
}

void Protection_Manager::VMP_IF_Thread()
{
	do
	{
		if (VMP->IterativeFunctions_DetectManipulatedMemory())
		{
			VMP->CloseProcessInstance();
			bProtectionIsRunning = false;
			return;
		}
		CheckClocks(&ctVmpIfResponse);
		Sleep(5);
	} while (bProtectionIsRunning);
}


void Protection_Manager::VMP_NIF_Thread()
{

	do
	{
		if (VMP->NoIterativeFunctions_DetectManipulatedMemory())
		{
			VMP->CloseProcessInstance();
			bProtectionIsRunning = false;
			return;
		}
		CheckClocks(&ctVmpNifResponse);
		Sleep(500);
	} while (bProtectionIsRunning);
}

void Protection_Manager::FP_Thread()
{
	do
	{
		int iStatus = FP->DetectLocalFileChange();

		if (iStatus == 1)
		{
			bProtectionIsRunning = false;
			Exception_Manager::HandleProtegaStandardError(iFPErrorCode,
				"Not able to read game files. Please restart the application. If this problem continues, please contact the administrator! [1]");

			return;
		}
		if (iStatus == 1)
		{
			bProtectionIsRunning = false;
			return;
		}
		CheckClocks(&ctFpResponse);

		iStatus = FP->DetectInjection();

		if (iStatus == 1)
		{
			bProtectionIsRunning = false;
			Exception_Manager::HandleProtegaStandardError(iFPErrorCode,
				"Not able to read game files. Please restart the application. If this problem continues, please contact the administrator! [2]");

			return;
		}

		if (iStatus == 2)
		{
			bProtectionIsRunning = false;
			return;
		}
		CheckClocks(&ctFpResponse);
		Sleep(500);
	} while (bProtectionIsRunning);
}

//	Callbacks
void Protection_Manager::HE_Callback(std::string sSection, std::string sDetectionValue)
{
	bProtectionIsRunning = false;
	std::vector<std::string> lDetectionInformation;
	lDetectionInformation.push_back(sSection);
	lDetectionInformation.push_back(sDetectionValue);
	funcCallbackHandler(1, lDetectionInformation);
}

void Protection_Manager::VMP_Callback(std::string sDetectedBaseAddress, std::string sDetectedOffset, std::string sDetectedValue, std::string sDefaultValue)
{
	bProtectionIsRunning = false;

#pragma region DEBUG-OUTPUT
	//std::ofstream filestr;
	//filestr.open(".\\detect.txt", std::fstream::in | std::fstream::out | std::fstream::app);

	//clock_t start = std::clock();

	//filestr << "OS: " << sDetectedOffset << " DV: " << sDetectedValue << " DEV: " << sDefaultValue << std::endl;
	//filestr.close();
#pragma endregion

	std::vector<std::string> lDetectionInformation;	
	lDetectionInformation.push_back(sDetectedBaseAddress);
	lDetectionInformation.push_back(sDetectedOffset);
	lDetectionInformation.push_back(sDetectedValue);
	lDetectionInformation.push_back(sDefaultValue);

	funcCallbackHandler(2, lDetectionInformation);
}

void Protection_Manager::FP_Callback(std::string sSection, std::string sDetectionValue)
{
	bProtectionIsRunning = false;

	std::string sInjection = "";

	std::vector<std::string> lDetectionInformation;
	lDetectionInformation.push_back(sSection);
	lDetectionInformation.push_back(sDetectionValue);

	funcCallbackHandler(3, lDetectionInformation);
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