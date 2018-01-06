#pragma once
#include "../Network/Network_Manager.h"
#include "../Data/Data_Manager.h"
#include "../Protection/Protection_Manager.h"
#include "../Tools/SplashDisplayer.h"

class ProtegaCore
{
private:
	Network_Manager* NetworkManager;
	Protection_Manager* ProtectionManager;



	void ServerAnswer(NetworkTelegram NetworkTelegramMessage);
	void ProtectionManagerAnswer(std::list<std::wstring> wsDetectionInformation);
	void Update();

public:
	ProtegaCore();
	~ProtegaCore();

	void StartAntihack();
};

