#pragma once
#include "Tcp_Connector.h"
#include <boost/thread.hpp>
#include <boost/algorithm/string.hpp>
#include <thread>

#pragma region Web Config
#define TELEGRAM_SPLIT_CHAR ";"
#pragma endregion

struct NetworkTelegram {
	int iTelegramNumber;
	std::vector<std::string> lParameters;
};

class Network_Manager
{
private:
	//Vars
	string sIP;
	string iPort;
	NetworkTelegram NetworkTelegramMessage;
	//Functions
	bool SendAndGet(const char* sMessage);
	void OnReceiveConverter(string sMessage);
	std::function<void(NetworkTelegram Telegram)> funcCallbackHandler;
public:
	Network_Manager(string sIP, string iPort, std::function<void(NetworkTelegram Telegram)> funcCallbackHandler);
	~Network_Manager();
	bool TestMessage_001();
};

