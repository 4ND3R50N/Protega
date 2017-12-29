#pragma once
#include "../Network/Network_Manager.h"
#include "../Data/Data_Manager.h"

class ProtegaCore
{
private:
	void ServerAnswer(NetworkTelegram NetworkTelegramMessage);
public:
	ProtegaCore();
	~ProtegaCore();

	void StartAntihack();
};

