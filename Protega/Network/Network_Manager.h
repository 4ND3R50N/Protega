#pragma once
#include "Tcp_Connector.h"
#include <boost/thread.hpp>
#include <boost/algorithm/string.hpp>
#include <thread>

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
	const char* sProtocolDelimiter;
	const char* sDataDelimiter;
	NetworkTelegram NetworkTelegramMessage;
	//Functions
	bool SendAndGet(const char* sMessage);
	void OnReceiveConverter(string sMessage);
	std::function<void(NetworkTelegram Telegram)> funcCallbackHandler;
public:
	Network_Manager(std::string sIP, std::string iPort, const char* sProtocolDelimiter, const char* sDataDelimiter, std::function<void(NetworkTelegram Telegram)> funcCallbackHandler);
	~Network_Manager();
	bool TestMessage_001();
};

