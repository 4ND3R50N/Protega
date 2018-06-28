#include "../stdafx.h"
#include "ProtegaCore.h"

//Possible todos:
// - Implement a refresh logic for dynamic data (Download data again for each X mins


ProtegaCore::ProtegaCore()
{
	//Init classes	
	NetworkManager = new Network_Manager(Data_Manager::GetNetworkServerIP(), Data_Manager::GetNetworkServerPort(),
		Data_Manager::GetProtocolDelimiter(), Data_Manager::GetDataDelimiter(), Data_Manager::GetNetworkMaxSendRetries(), Data_Manager::GetExceptionNetworkErrorNumber(),
		std::bind(&ProtegaCore::ServerAnswer, this, std::placeholders::_1, std::placeholders::_2));
	//Set exception vars
	Exception_Manager::SetErrorFileName(Data_Manager::GetExceptionErrorFileName());
	Exception_Manager::SetCrashReporterName(Data_Manager::GetExceptionCrashReporterName());
	Exception_Manager::SetBaseFolder(Data_Manager::GetProgramFolderPath());
}


ProtegaCore::~ProtegaCore()
{
}

void ProtegaCore::StartAntihack()
{
#pragma region Check local protega files
	//Get logo file path
	std::stringstream ss;
	ss << Data_Manager::GetProgramFolderPath() << Data_Manager::GetLocalDataFolder() << Data_Manager::GetLocalProtegaImage();

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
		Data_Manager::GetSoftwareArchitecture(), Data_Manager::GetSoftwareLanguage(), Data_Manager::GetCurrentWanIP());
	do
	{
		Sleep(1000);
	} while(!NetworkManager->GetAuthentificationSuccessStatus());
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

	ProtectionManager = new Protection_Manager(std::bind(&ProtegaCore::ProtectionManagerAnswer, this, std::placeholders::_1, std::placeholders::_2), (int)GetCurrentProcessId(),
		Data_Manager::GetProtectionThreadResponseDelta(), Data_Manager::GetExceptionVmErrorNumber(), Data_Manager::GetExceptionFpErrorNumber(), Data_Manager::GetExceptionThreadErrorNumber(),
		Data_Manager::GetProtectionMaxFpDll(), Data_Manager::GetProgramFolderPath(), Data_Manager::GetHeuristicProcessNames(), vBlackListWindowName, vBlackListClassName, Data_Manager::GetHeuristicMD5Values(),
		Data_Manager::GetFilesToCheckValues());

	ProtectionManager->CheckClocks(ProtectionManager->GetMainThreadClock());
	ProtectionManager->StartProtectionThreads();
#pragma endregion

	Splash.CloseSplash();
	Splash.~SplashDisplayer();

	Update();
}

//Private
void ProtegaCore::ServerAnswer(unsigned int iTelegramNumber, std::vector<std::string> lParameters)
{
	//Handles incoming telegrams

	switch (iTelegramNumber)
	{		
	case 200: //Authentication Successfull
		//Overwrites the Computer ID with the session id from the server
		
		Data_Manager::SetLocalHardwareSID(lParameters[0]);
		break;		
	case 201: //Authentication Unsuccessfull
		//Writes the problem as a exception
		Exception_Manager::HandleProtegaStandardError(atoi(lParameters[0].c_str()), lParameters[1].c_str());
		break;
	case 300: //Ping without message
		//Nothing happens here. The ping must not fail, that is the only condition. If the ping failes, there
		//is a error message in Network_Manager::SendAndGet anyway.
		break;
	case 301: //Ping with message
		
	case 400: //Hack detect successfull
		Exception_Manager::HandleProtegaStandardError(atoi(lParameters[0].c_str()),
			"Hack detected!");
		break;
	case 401: //Hack detect not successfull
		Exception_Manager::HandleProtegaStandardError(atoi(lParameters[0].c_str()),
			"Hack detected!");
		break;
	default:
		break;
	}
}

void ProtegaCore::ProtectionManagerAnswer(unsigned int iType, std::vector<std::string> vDetectionInformation)
{
	switch (iType)
	{
		//HE
	case 1:
		NetworkManager->HackDetection_HE_701(Data_Manager::GetLocalHardwareSID(), atoi(vDetectionInformation[0].c_str()),
			vDetectionInformation[1]);
		break;
		//VMP
	case 2:
		NetworkManager->HackDetection_VMP_702(Data_Manager::GetLocalHardwareSID(), vDetectionInformation[0], vDetectionInformation[1],
			vDetectionInformation[2], vDetectionInformation[3]);
		break;
		//FP
	case 3:
		NetworkManager->HackDetection_FP_703(Data_Manager::GetLocalHardwareSID(), atoi(vDetectionInformation[0].c_str()), vDetectionInformation[1]);
		break;
	default:
		break;
	}

}

void ProtegaCore::Update()
{
	do
	{
		//Ping to server. Check answer.
		if (ProtectionManager->ProtectionIsRunning())
		{
			NetworkManager->Ping_600(Data_Manager::GetLocalHardwareSID());
		}		
		
		ProtectionManager->CheckClocks(ProtectionManager->GetMainThreadClock());
		Sleep(3000);
	} while (ProtectionManager->ProtectionIsRunning());

	//Temporary
	Sleep(10000);
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