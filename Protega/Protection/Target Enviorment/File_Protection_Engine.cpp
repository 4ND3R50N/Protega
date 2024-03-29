#include "../../stdafx.h"
#include "File_Protection_Engine.h"

File_Protection_Engine::File_Protection_Engine(int iTargetApplicationId, std::string sBaseFolder,
	std::function<void(std::string s, std::string sMd5) > funcDetectCallbackHandler,
	std::pair<std::vector<std::string>, std::vector<std::string>> pFileAndMd5, int iMaxPossibleDlls)
{
	this->pFileAndMd5 = pFileAndMd5;
	this->iTargetApplicationId = iTargetApplicationId;
	this->iMaxPossibleDlls = iMaxPossibleDlls;
	this->funcDetectCallbackHandler = funcDetectCallbackHandler;
	this->sBaseFolder = sBaseFolder;
}

//Public
int File_Protection_Engine::DetectLocalFileChange()
{
	if (bIsAtStart)
	{
		sItFile = pFileAndMd5.first.begin();
		sItMd5 = pFileAndMd5.second.begin();
		bIsAtStart = false;
	}

	std::string& sFile(*sItFile);
	std::string& sMd5(*sItMd5);

	std::stringstream ss;

	ss << sBaseFolder << sFile;

	std::string sActualMd5 = CryptoPP_Converter::GetMD5ofFile(ss.str().c_str());

	if (sActualMd5 == "") {
		return 1;
	}

	if (sActualMd5 != sMd5)
	{
		//funcDetectCallbackHandler(pFileAndMd5.first[i], pFileAndMd5.second[i], false);
		funcDetectCallbackHandler("1", sMd5);
		return 2;
	}

	++sItFile;
	++sItMd5;

	if (sItFile == pFileAndMd5.first.end() || sItMd5 == pFileAndMd5.second.end())
	{
		bIsAtStart = true;
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
		funcDetectCallbackHandler("2", std::to_string(iModuleCounter));
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
