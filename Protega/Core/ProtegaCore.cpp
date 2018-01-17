#include "../stdafx.h"
#include "ProtegaCore.h"

//Possible todos:
// - Implement a refresh logic for dynamic data (Download data again for each X mins




ProtegaCore::ProtegaCore()
{
	//Init classes	
	NetworkManager = new Network_Manager(Data_Manager::GetNetworkServerIP(), Data_Manager::GetNetworkServerPort(),
		Data_Manager::GetProtocolDelimiter(), Data_Manager::GetDataDelimiter(), std::bind(&ProtegaCore::ServerAnswer, this, std::placeholders::_1));
	//Set exception vars
	Exception_Manager::SetExeptionCaption(Data_Manager::GetExceptionCaption());
	Exception_Manager::SetTargetName(Data_Manager::GetLocalDataProtectionTarget().c_str());
}


ProtegaCore::~ProtegaCore()
{
}

void ProtegaCore::StartAntihack()
{
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
	
	//Show logo, wait for some time
	//	Convert const char* to CA2T (LPCTSTR)
	CA2T wt(ss.str().c_str());
	SplashDisplayer Splash(wt, RGB(128, 128, 128));
	Splash.ShowSplash();

	Sleep(1000);
	
	//Collect dynamic data
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
	//Dummy Lists
	std::vector<std::string> vBlackListWindowName;
	std::vector<std::string> vBlackListClassName;

	ProtectionManager = new Protection_Manager(std::bind(&ProtegaCore::ProtectionManagerAnswer, this, std::placeholders::_1), (int)GetCurrentProcessId(),
		Data_Manager::GetProtectionThreadResponseDelta(), Data_Manager::GetExceptionVmErrorNumber(), Data_Manager::GetExceptionThreadErrorNumber(), 
		Data_Manager::GetHeuristicProcessNames(), vBlackListWindowName, vBlackListClassName, Data_Manager::GetHeuristicMD5Values());

	ProtectionManager->CheckClocks(ProtectionManager->GetMainThreadClock());
	ProtectionManager->StartProtectionThreads();
	

	Splash.CloseSplash();
	Splash.~SplashDisplayer();
	Update();

}

//Private
void ProtegaCore::ServerAnswer(NetworkTelegram NetworkTelegramMessage)
{
	//Handles incoming telegrams


	MessageBoxA(NULL, NetworkTelegramMessage.lParameters[0].c_str(), "Protega antihack engine", NULL);
	int retval = ::_tsystem(_T("taskkill /F /T /IM CabalMain22.exe"));
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