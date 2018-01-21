#include "../stdafx.h"
#include "ProtegaCore.h"

//Possible todos:
// - Implement a refresh logic for dynamic data (Download data again for each X mins




ProtegaCore::ProtegaCore()
{
	//Init classes	
	NetworkManager = new Network_Manager(Data_Manager::GetNetworkServerIP(), Data_Manager::GetNetworkServerPort(),
		Data_Manager::GetProtocolDelimiter(), Data_Manager::GetDataDelimiter(), Data_Manager::GetNetworkMaxSendRetries(), Data_Manager::GetExceptionNetworkErrorNumber(),
		std::bind(&ProtegaCore::ServerAnswer, this, std::placeholders::_1));
	//Set exception vars
	Exception_Manager::SetExeptionCaption(Data_Manager::GetExceptionCaption());
	Exception_Manager::SetTargetName(Data_Manager::GetLocalDataProtectionTarget().c_str());
}


ProtegaCore::~ProtegaCore()
{
}

void ProtegaCore::StartAntihack()
{

#pragma region Check local protega files
	//Get logo file path
	std::stringstream ss;
	ss << Data_Manager::GetLocalDataFolder() << Data_Manager::GetLocalProtegaImage();

	//Check if logo/Other files are here
	if (!CheckProtegaFiles(ss.str().c_str()))
	{
		Exception_Manager::HandleProtegaStandardError(Data_Manager::GetExceptionLocalFileErrorNumber(),
			"Files are missing. Please restart the application or download the latest gamefiles again!");
		return;
	}

#pragma endregion
	
	//Show logo, wait for some time
	//	Convert const char* to CA2T (LPCTSTR)
	CA2T wt(ss.str().c_str());
	SplashDisplayer Splash(wt, RGB(128, 128, 128));
	Splash.ShowSplash();

	Sleep(1000);
	
#pragma region Authenticate to server
	//Collect necessary data (Hardware ID)
	NetworkManager->Authentication_500(Data_Manager::GenerateComputerID(), Data_Manager::GetSoftwareVersion(),
		Data_Manager::GetSoftwareArchitecture(), Data_Manager::GetSoftwareLanguage());
	do
	{
		Sleep(1000);
	} while (!NetworkManager->GetAuthentificationSuccessStatus());

#pragma endregion


#pragma region Collect Dynamic Data
	int iErrorCode = Data_Manager::CollectDynamicProtesData();
	if (iErrorCode == 1)
	{
		Exception_Manager::HandleProtegaStandardError(Data_Manager::GetExceptionWebDownloadErrorNumber(),
			"Data download failed. Please restart the application. If that problem accurs more often, check your internet connection!");
		return;
	}
	if (iErrorCode == 2)
	{
		Exception_Manager::HandleProtegaStandardError(Data_Manager::GetExceptionLocalFileErrorNumber(),
			"Was not able to open downloaded data. Please restart the application!");
		return;
	}
	if (iErrorCode == 3)
	{
		Exception_Manager::HandleProtegaStandardError(Data_Manager::GetExceptionDataConversionErrorNumber(),
			"Downloaded data is corrupt. Please check our internet connection and restart the application. If this problem accours more often, please contact an administrator!");
	}
#pragma endregion

#pragma region Start Protections
	//Dummy Lists
	std::vector<std::string> vBlackListWindowName;
	std::vector<std::string> vBlackListClassName;

	ProtectionManager = new Protection_Manager(std::bind(&ProtegaCore::ProtectionManagerAnswer, this, std::placeholders::_1), (int)GetCurrentProcessId(),
		Data_Manager::GetProtectionThreadResponseDelta(), Data_Manager::GetExceptionVmErrorNumber(), Data_Manager::GetExceptionFpErrorNumber(), Data_Manager::GetExceptionThreadErrorNumber(),
		Data_Manager::GetProtectionMaxFpDll(), Data_Manager::GetHeuristicProcessNames(), vBlackListWindowName, vBlackListClassName, Data_Manager::GetHeuristicMD5Values(),
		Data_Manager::GetFilesToCheckValues());

	ProtectionManager->CheckClocks(ProtectionManager->GetMainThreadClock());
	ProtectionManager->StartProtectionThreads();
#pragma endregion
	
	Splash.CloseSplash();
	Splash.~SplashDisplayer();


	Update();

}

//Private
void ProtegaCore::ServerAnswer(NetworkTelegram NetworkTelegramMessage)
{
	//Handles incoming telegrams

	switch (NetworkTelegramMessage.iTelegramNumber)
	{
		//Authentication Successfull
	case 200:
		Data_Manager::SetLocalHardwareSID(NetworkTelegramMessage.lParameters[0]);
		break;
		//Authentication Unsuccessfull
	case 201:

	default:
		break;
	}
}

void ProtegaCore::ProtectionManagerAnswer(std::list<std::wstring> wsDetectionInformation)
{
	std::wstringstream ss;
	std::list<std::wstring>::iterator wsIt;

	ss << "Hack Detect! Debug: ";
	for (wsIt = wsDetectionInformation.begin(); wsIt != wsDetectionInformation.end(); wsIt++)
	{
		std::wstring& wsItData(*wsIt);
		ss << wsItData << L" || ";
	}

	Exception_Manager::HandleProtegaStandardError(Data_Manager::GetExceptionLocalFileErrorNumber(),
		ss.str().c_str());

	//Send hack detect to server

}

void ProtegaCore::Update()
{
	do
	{
		//Ping to server. Check answer.

		ProtectionManager->CheckClocks(ProtectionManager->GetMainThreadClock());
		Sleep(1000);
	} while (true);
}

//Support functions
bool ProtegaCore::CheckProtegaFiles(const char* sImageFilePath)
{
	if (!boost::filesystem::exists(sImageFilePath))
	{
		return false;
	}
	return true;
}