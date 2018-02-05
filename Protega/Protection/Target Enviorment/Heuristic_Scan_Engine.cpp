#include "../../stdafx.h"
#include "Heuristic_Scan_Engine.h"

Heuristic_Scan_Engine::Heuristic_Scan_Engine(std::vector<std::wstring> lBlackListProcessNames,
	std::vector<std::string> lBlackListWindowNames,
	std::vector<std::string> lBlackListClassNames,
	std::vector<std::string> lBlackListMd5Values,
	std::function<void(std::string sSection, std::string sDetectedValue) > funcErrorCallbackHandler)
{
	//Set data
	this->vBlackListProcessNames = lBlackListProcessNames;
	this->vBlackListWindowNames = lBlackListWindowNames;
	this->vBlackListClassNames = lBlackListClassNames;
	this->vBlackListMd5Values = lBlackListMd5Values;

	this->funcErrorCallbackHandler = funcErrorCallbackHandler;

	//Convert the current process names uppercase
	std::vector<std::wstring>::iterator wsIt;
	std::vector<std::string>::iterator sIt;
	//Iterate through all char related blacklists
	// lBlackListProcessNames
	for (wsIt = this->vBlackListProcessNames.begin(); wsIt != this->vBlackListProcessNames.end(); wsIt++)
	{
		std::wstring& wsItData(*wsIt);
		boost::to_upper(wsItData);
	}
	// lBlackListWindowNames
	for (sIt = this->vBlackListWindowNames.begin(); sIt != this->vBlackListWindowNames.end(); sIt++)
	{
		std::string& sItData(*sIt);
		boost::to_upper(sItData);
	}
	// lBlackListClassNames
	for (sIt = this->vBlackListClassNames.begin(); sIt != this->vBlackListClassNames.end(); sIt++)
	{
		std::string& sItData(*sIt);
		boost::to_upper(sItData);
	}
	// lBlackListMD5Values
	for (sIt = this->vBlackListMd5Values.begin(); sIt != this->vBlackListMd5Values.end(); sIt++)
	{
		std::string& sItData(*sIt);
		boost::to_upper(sItData);
	}
}


Heuristic_Scan_Engine::~Heuristic_Scan_Engine()
{

}


bool Heuristic_Scan_Engine::DetectBlacklistedProcessNames()
{
	//get current process names
	std::list<std::wstring> lCurrentProcessNames;
	std::list<DWORD> lCurrentProcessIDsTmp;
	GetCurrentProcessNamesAndPIDs(lCurrentProcessNames, lCurrentProcessIDsTmp);

	//Convert the current process names uppercase
	std::vector<std::wstring>::iterator wsIt;
	int iForCounter = 0;
	//Iterate through all current running processes
	for (wsIt = vBlackListProcessNames.begin(); wsIt != vBlackListProcessNames.end(); wsIt++)
	{
		std::wstring& wsItBlackListEntry(*wsIt);

		//Compare entries of the current process names with the entries of the blacklists
		bool bEntryFound = (std::find(lCurrentProcessNames.begin(), lCurrentProcessNames.end(), wsItBlackListEntry) != lCurrentProcessNames.end());
		

		if (bEntryFound)
		{
			//Commented out due change of handling things
			//Detection handling
			//std::list<std::string> lOtherInformation;
			
			//std::list<std::string>::iterator sIt = std::next(lBlackListWindowNames.begin(), iForCounter);	
		
			////Get other data
			//lOtherInformation.push_back(sIt->c_str());
			//sIt = std::next(lBlackListClassNames.begin(), iForCounter);
			//lOtherInformation.push_back(sIt->c_str());
			//sIt = std::next(lBlackListMd5Values.begin(), iForCounter);
			//lOtherInformation.push_back(sIt->c_str());
			//Send them to the protection manager

			std::string sItBlackListEntry = "";
			using convert_type = std::codecvt_utf8<wchar_t>;
			std::wstring_convert<convert_type, wchar_t> converter;

			//use converter (.to_bytes: wstr->str, .from_bytes: str->wstr)
			sItBlackListEntry = converter.to_bytes(wsItBlackListEntry);
			funcErrorCallbackHandler("1", sItBlackListEntry);
			return true;
		}
		iForCounter++;
	}
	return false;
}

bool Heuristic_Scan_Engine::DetectBlacklistedWindowNames()
{
	return false;
}

bool Heuristic_Scan_Engine::DetectBlacklistedClassNames()
{
	return false;
}

bool Heuristic_Scan_Engine::DetectBlacklistedProcessMd5Hash()
{
	//get current process names
	std::stringstream ss;
	std::list<std::wstring> lCurrentProcessNames;
		
	//
	HANDLE hProcessHandle;
	wchar_t wtFilePath[MAX_PATH];

	if (bIsAtStart)
	{
		lCurrentProcessIDsTmp.clear();
		lCurrentProcessNames.clear();
		GetCurrentProcessNamesAndPIDs(lCurrentProcessNames, lCurrentProcessIDsTmp);
		itDwProcessID = lCurrentProcessIDsTmp.begin();
		bIsAtStart = false;
	}

	hProcessHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, 0, *itDwProcessID);

	if (hProcessHandle)
	{
		bool bMd5Success = false;
		GetModuleFileNameEx(hProcessHandle, 0, wtFilePath, MAX_PATH);
		std::string sMD5Hash = "";
		_bstr_t bstrFilePath(wtFilePath);

		try
		{
			sMD5Hash = GetMD5Hash(bstrFilePath);
			boost::to_upper(sMD5Hash);
			bMd5Success = true;
		}
		catch (const std::exception&)
		{
			bMd5Success = false;
		}

		if (bMd5Success)
		{
			bool bEntryFound = (std::find(vBlackListMd5Values.begin(), vBlackListMd5Values.end(), sMD5Hash) != vBlackListMd5Values.end());

			if (bEntryFound)
			{
				funcErrorCallbackHandler("2", sMD5Hash);
			}
		}
		
		CloseHandle(hProcessHandle);			
	}

	++itDwProcessID;

	if (itDwProcessID == lCurrentProcessIDsTmp.end())
	{
		bIsAtStart = true;
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