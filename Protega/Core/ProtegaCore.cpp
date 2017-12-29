#include "../stdafx.h"
#include "ProtegaCore.h"

//Possible todos:
// - Implement a refresh logic for dynamic data (Download data again for each X mins


ProtegaCore::ProtegaCore()
{
}


ProtegaCore::~ProtegaCore()
{
}

void ProtegaCore::StartAntihack()
{
	//Todo: Just for testing reasons! Globallize the Network_manager object + store data in a data class
	Network_Manager *NetworkTestManager = new Network_Manager(Data_Manager::GetNetworkServerIP(), Data_Manager::GetNetworkServerPort(),
		Data_Manager::GetProtocolDelimiter(), Data_Manager::GetDataDelimiter(), std::bind(&ProtegaCore::ServerAnswer, this, std::placeholders::_1));
	NetworkTestManager->TestMessage_001();
}

void ProtegaCore::ServerAnswer(NetworkTelegram NetworkTelegramMessage)
{
	MessageBoxA(NULL, NetworkTelegramMessage.lParameters[0].c_str(), "Protega antihack engine", NULL);
}

