#include "../../stdafx.h"
#include "File_Protection_Engine.h"

File_Protection_Engine::File_Protection_Engine(int iTargetApplicationId, 
	std::function<void(std::string sFile, std::string sMd5, bool bInjection) > funcDetectCallbackHandler,
	std::pair<std::vector<std::string>, std::vector<std::string>> pFileAndMd5, int iMaxPossibleDlls)
{
	this->pFileAndMd5 = pFileAndMd5;
	this->iTargetApplicationId = iTargetApplicationId;
	this->iMaxPossibleDlls = iMaxPossibleDlls;
	this->funcDetectCallbackHandler = funcDetectCallbackHandler;
}

//Public
int File_Protection_Engine::DetectLocalFileChange()
{
	for (unsigned int i = 0; i < pFileAndMd5.first.size(); i++)
	{
		std::string sActualMd5 = CryptoPP_Converter::GetMD5ofFile(pFileAndMd5.first[i].c_str());

		if (sActualMd5 == "") {
			return 1;
		}

		if (sActualMd5 != pFileAndMd5.second[i])
		{
			funcDetectCallbackHandler(pFileAndMd5.first[i], pFileAndMd5.second[i], false);
			return 2;
		}
	}
	return 0;
}

int File_Protection_Engine::DetectInjection()
{
	int iModuleCounter = 0;
	HMODULE hMods[1024];
	DWORD cbNeeded;
	HANDLE hProcess = OpenProcess(PROCESS_QUERY_INFORMATION |
	PROCESS_VM_READ,
	FALSE, iTargetApplicationId);
	if (hProcess == NULL)
	{
		return 1;
	}

	if (EnumProcessModules(hProcess, hMods, sizeof(hMods), &cbNeeded))
	{
		for (unsigned int i = 0; i < (cbNeeded / sizeof(HMODULE)); i++)
		{
			TCHAR szModName[MAX_PATH];
			// Get the full path to the module's file.
			if (GetModuleFileNameEx(hProcess, hMods[i], szModName,
				sizeof(szModName) / sizeof(TCHAR)))
			{
				// Print the module name and handle value.
				iModuleCounter++;
			}
		}
	}
	
	if (iModuleCounter > iMaxPossibleDlls)
	{
		/*std::stringstream ss;
		ss << "Number of ddls: " << iModuleCounter << " || " << "allowed are_: " << iMaxPossibleDlls;
		MessageBoxA(0, ss.str().c_str(), "MessageBox caption", MB_OK);*/
		funcDetectCallbackHandler("Main Application", "-", true);
		return 2;
	}
	

	return 0;
}

bool File_Protection_Engine::DetectInjection(int iTargetApplicationId)
{
	return false;
}

File_Protection_Engine::~File_Protection_Engine()
{
}
