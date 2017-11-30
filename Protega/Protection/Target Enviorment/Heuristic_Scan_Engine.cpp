#include "../../stdafx.h"
#include "Heuristic_Scan_Engine.h"




Heuristic_Scan_Engine::Heuristic_Scan_Engine()
{
	//Convert content upper case?

}


Heuristic_Scan_Engine::~Heuristic_Scan_Engine()
{

}



bool Heuristic_Scan_Engine::DoScanProcessNames()
{
	// Get the list of process identifiers.  
	DWORD dwProcessIdContainer[1024], cbNeeded, dwProcessAmount;
	unsigned int i;

	//This returns a list of handles to processes running on the system as an array.
	if (!EnumProcesses(dwProcessIdContainer, sizeof(dwProcessIdContainer), &cbNeeded))
		return false;

	// Calculate how many process identifiers were returned.  
	dwProcessAmount = cbNeeded / sizeof(DWORD);

	
	for (int iProcessIndex = 0; iProcessIndex < dwProcessAmount; iProcessIndex++)
	{
		if (dwProcessIdContainer[iProcessIndex] != 0)
		{
			//Get the process name of the process
			std::wstring wsCurrentProcessName = GetProcessName(dwProcessIdContainer[iProcessIndex]);

			//Check if the name is in the process name black list
		}
	}
	return true;
}

bool Heuristic_Scan_Engine::ScanWindowNames()
{
	return false;
}

bool Heuristic_Scan_Engine::ScanClassNames()
{
	return false;
}

bool Heuristic_Scan_Engine::ScanProcessMd5Hash()
{
	return false;
}

//Private
void Heuristic_Scan_Engine::GetCurrentProcessIdentificators(DWORD* aProcesses, DWORD& cProcessAmount)
{
	DWORD cbNeeded;

	if (!EnumProcesses(aProcesses, sizeof(aProcesses), &cbNeeded))
	{
		return;
	}

	cProcessAmount = cbNeeded / sizeof(DWORD);
}

std::wstring Heuristic_Scan_Engine::GetProcessName(DWORD dwProcessID)
{
	TCHAR szProcessName[MAX_PATH] = TEXT("<unknown>");

	//Get a handle to the process.

	HANDLE hProcess = OpenProcess(PROCESS_QUERY_INFORMATION |
		PROCESS_VM_READ,
		FALSE, dwProcessID);

	//Get the process name.
	if (NULL != hProcess)
	{
		HMODULE hMod;
		DWORD cbNeeded;

		if (EnumProcessModules(hProcess, &hMod, sizeof(hMod),
			&cbNeeded))
		{
			GetModuleBaseName(hProcess, hMod, szProcessName,
				sizeof(szProcessName) / sizeof(TCHAR));
		}
	}
	CloseHandle(hProcess);
	//Possible return of dwProcessID
	return szProcessName;
}


