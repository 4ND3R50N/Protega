#include "../stdafx.h"
#include "ProtegaCore.h"


ProtegaCore::ProtegaCore()
{
}


ProtegaCore::~ProtegaCore()
{
}

void ProtegaCore::StartAntihack()
{
	//Todo: Just for testing reasons! Globallize the Network_manager object + store data in a data class
	Network_Manager *NetworkTestManager = new Network_Manager("62.138.6.50", "13001", std::bind(&ProtegaCore::ServerAnswer, this, std::placeholders::_1));
	NetworkTestManager->TestMessage_001();
}

void ProtegaCore::ServerAnswer(NetworkTelegram NetworkTelegramMessage)
{
	MessageBoxA(NULL, NetworkTelegramMessage.lParameters[0].c_str(), "Protega antihack engine", NULL);
}

