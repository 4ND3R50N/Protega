#include "../../stdafx.h"
#include "Heuristic_Scan_Engine.h"

Heuristic_Scan_Engine::Heuristic_Scan_Engine(std::list<std::wstring> lBlackListProcessNames,
	std::list<std::string> lBlackListWindowNames,
	std::list<std::string> lBlackListClassNames,
	std::list<std::string> lBlackListMd5Values,
	std::function<void(std::wstring sDetectedValue) > funcErrorCallbackHandler)
{
	//Set data
	this->lBlackListProcessNames = lBlackListProcessNames;
	this->lBlackListWindowNames = lBlackListWindowNames;
	this->lBlackListClassNames = lBlackListClassNames;
	this->lBlackListMd5Values = lBlackListMd5Values;

	this->funcErrorCallbackHandler = funcErrorCallbackHandler;

	//Convert the current process names uppercase
	std::list<std::wstring>::iterator wsIt;
	std::list<std::string>::iterator sIt;
	//Iterate through all char related blacklists
	// lBlackListProcessNames
	for (wsIt = this->lBlackListProcessNames.begin(); wsIt != this->lBlackListProcessNames.end(); wsIt++)
	{
		std::wstring& wsItData(*wsIt);
		boost::to_upper(wsItData);
	}
	// lBlackListWindowNames
	for (sIt = this->lBlackListWindowNames.begin(); sIt != this->lBlackListWindowNames.end(); sIt++)
	{
		std::string& sItData(*sIt);
		boost::to_upper(sItData);
	}
	// lBlackListClassNames
	for (sIt = this->lBlackListClassNames.begin(); sIt != this->lBlackListClassNames.end(); sIt++)
	{
		std::string& sItData(*sIt);
		boost::to_upper(sItData);
	}
}


Heuristic_Scan_Engine::~Heuristic_Scan_Engine()
{

}



bool Heuristic_Scan_Engine::DoScanProcessNames()
{
	//get current process names
	std::list<std::wstring> lCurrentProcessNames;
	std::list<DWORD> lCurrentProcessIDsTmp;
	GetCurrentProcessNamesAndPIDs(lCurrentProcessNames, lCurrentProcessIDsTmp);

	//Convert the current process names uppercase
	std::list<std::wstring>::iterator wsIt;
	int iForCounter = 0;
	//Iterate through all current running processes
	for (wsIt = lBlackListProcessNames.begin(); wsIt != lBlackListProcessNames.end(); wsIt++)
	{
		std::wstring& wsItBlackListEntry(*wsIt);

		//Compare entries of the current process names with the entries of the blacklists
		bool bEntryFound = (std::find(lCurrentProcessNames.begin(), lCurrentProcessNames.end(), wsItBlackListEntry) != lCurrentProcessNames.end());
		

		if (bEntryFound)
		{
			//Detection handling
			std::list<std::string> lOtherInformation;
			std::list<std::string>::iterator sIt = std::next(lBlackListWindowNames.begin(), iForCounter);	
		
			//Get other data
			lOtherInformation.push_back(sIt->c_str());
			sIt = std::next(lBlackListClassNames.begin(), iForCounter);
			lOtherInformation.push_back(sIt->c_str());
			sIt = std::next(lBlackListMd5Values.begin(), iForCounter);
			lOtherInformation.push_back(sIt->c_str());
			//Send them to the protection manager
			funcErrorCallbackHandler(wsItBlackListEntry);
			return true;
		}
		iForCounter++;
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
	//get current process names
	std::stringstream ss;
	std::list<std::wstring> lCurrentProcessNames;
	std::list<DWORD> lCurrentProcessIDsTmp;
	GetCurrentProcessNamesAndPIDs(lCurrentProcessNames, lCurrentProcessIDsTmp);
	
	//
	HANDLE hProcessHandle;
	wchar_t wtFilePath[MAX_PATH];

	std::list<DWORD>::iterator itDwProcessID;

	for (itDwProcessID = lCurrentProcessIDsTmp.begin(); itDwProcessID != lCurrentProcessIDsTmp.end(); ++itDwProcessID) {

		hProcessHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, 0, *itDwProcessID);

		if (hProcessHandle)
		{
			GetModuleFileNameEx(hProcessHandle, 0, wtFilePath, MAX_PATH);
			std::string sMD5Hash = "";
			_bstr_t bstrFilePath(wtFilePath);

			try
			{
				sMD5Hash = GetMD5Hash(bstrFilePath);
			}
			catch (const std::exception& e)
			{
				continue;
			}
			
			bool bEntryFound = (std::find(lBlackListMd5Values.begin(), lBlackListMd5Values.end(), sMD5Hash) != lBlackListMd5Values.end());
			
			if (bEntryFound)
			{
				// Overestimate number of code points.
				std::wstring wsMD5Hash(sMD5Hash.size(), L' '); 
				// Shrink to fit.
				wsMD5Hash.resize(std::mbstowcs(&wsMD5Hash[0], sMD5Hash.c_str(), sMD5Hash.size()));
				funcErrorCallbackHandler(wsMD5Hash);
			}

			CloseHandle(hProcessHandle);
		}
	}
	return false;
}

//Private
void Heuristic_Scan_Engine::GetCurrentProcessNamesAndPIDs(std::list<std::wstring>& lProcessNames, std::list<DWORD>& lProcessIDs)
{
	HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
	
	if (hSnapshot) {
		PROCESSENTRY32 pe32;
		pe32.dwSize = sizeof(PROCESSENTRY32);
		if (Process32First(hSnapshot, &pe32)) {
			do {
				WCHAR* wDummy = pe32.szExeFile;
				boost::to_upper(wDummy);

				//Fill lists
				lProcessIDs.push_back(pe32.th32ProcessID);
				lProcessNames.push_back(wDummy);
			} while (Process32Next(hSnapshot, &pe32));
		}
		CloseHandle(hSnapshot);
	}

}

std::string Heuristic_Scan_Engine::GetMD5Hash(const char * sFilePath)
{
	return CryptoPP_Converter::GetMD5ofFile(sFilePath);
}