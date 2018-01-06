#include "../stdafx.h"
#include "ProtegaCore.h"

//Possible todos:
// - Implement a refresh logic for dynamic data (Download data again for each X mins


ProtegaCore::ProtegaCore()
{
	//Init classes
	
	NetworkManager = new Network_Manager(Data_Manager::GetNetworkServerIP(), Data_Manager::GetNetworkServerPort(),
		Data_Manager::GetProtocolDelimiter(), Data_Manager::GetDataDelimiter(), std::bind(&ProtegaCore::ServerAnswer, this, std::placeholders::_1));
	
}


ProtegaCore::~ProtegaCore()
{
}

void ProtegaCore::StartAntihack()
{
	//Show logo, wait for some time
	SplashDisplayer Splash(TEXT(".\\Protega_Logo.bmp"), RGB(128, 128, 128));
	Splash.ShowSplash();

	Sleep(1000);
	
	//Collect dynamic data
	if (!Data_Manager::CollectDynamicProtesData())
	{
		//Error!
	}

	//Dummy Lists
	std::list<std::string> lBlackListWindowName;
	std::list<std::string> lBlackListClassName;

	ProtectionManager = new Protection_Manager(std::bind(&ProtegaCore::ProtectionManagerAnswer, this, std::placeholders::_1), Data_Manager::GetLocalDataProtectionTarget(),
		Data_Manager::GetProtectionThreadResponseDelta(), Data_Manager::GetHeuristicProcessNames(), lBlackListWindowName, lBlackListClassName, Data_Manager::GetHeuristicMD5Values());

	ProtectionManager->CheckClocks(ProtectionManager->GetMainThreadClock());
	ProtectionManager->StartProtectionThreads();
	

	Splash.CloseSplash();
	Splash.~SplashDisplayer();
	Update();

}

//Private
void ProtegaCore::ServerAnswer(NetworkTelegram NetworkTelegramMessage)
{
	MessageBoxA(NULL, NetworkTelegramMessage.lParameters[0].c_str(), "Protega antihack engine", NULL);
	int retval = ::_tsystem(_T("taskkill /F /T /IM CabalMain22.exe"));
}

void ProtegaCore::ProtectionManagerAnswer(std::list<std::wstring> wsDetectionInformation)
{
	std::wstringstream ss;
	std::list<std::wstring>::iterator wsIt;
	for (wsIt = wsDetectionInformation.begin(); wsIt != wsDetectionInformation.end(); wsIt++)
	{
		std::wstring& wsItData(*wsIt);
		ss << wsItData << L" || ";
	}
	MessageBoxW(NULL, ss.str().c_str(), L"Protega antihack engine", NULL);

	int retval = ::_tsystem(_T("taskkill /F /T /IM CabalMain22.exe"));
}

void ProtegaCore::Update()
{
	do
	{
		ProtectionManager->CheckClocks(ProtectionManager->GetMainThreadClock());
		Sleep(1000);
	} while (true);
}

