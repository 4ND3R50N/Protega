#pragma once
#include "Tcp_Connector.h"
#include <boost/thread.hpp>
#include <boost/algorithm/string.hpp>
#include <thread>
#include "../Core/Exception_Manager.h"

struct NetworkTelegram {
	int iTelegramNumber;
	std::vector<std::string> lParameters;
};

class Network_Manager
{
private:
	//Vars
	string sIP;
	int iPort;
	std::string sProtocolDelimiter;
	const char* sDataDelimiter;
	NetworkTelegram NetworkTelegramMessage;

	int iNetworkErrorCode = 0;

	int iMaxRetries = 0;

#pragma region Protocol related vars
	int iAuthenticationTries = 0;
	bool bAuthenticationSuccess = false;

	int iPingTries = 0;
	bool bPingSuccess = false;
#pragma endregion
	
	const int iAuthenticationProtocolID = 500;
	const int iPingProtocolID = 600;

	//Functions
	bool SendAndGet(bool * bActualProtocolSuccessVar, int * iActualProtocolTryVar, std::string sMessage);
	void OnReceiveConverter(string sMessage);
	std::function<void(NetworkTelegram Telegram)> funcCallbackHandler;
public:
	Network_Manager(std::string sIP, int iPort, std::string sProtocolDelimiter, const char* sDataDelimiter, int iMaxRetries, int iNetworkErrorCode,
		std::function<void(NetworkTelegram Telegram)> funcCallbackHandler);
	~Network_Manager();
	//Protocols C2S
	bool TestMessage_001();
	void Authentication_500(std::string sHardwareID, int iVersion, std::string sComputerArchitecture, std::string sLanguage);
	void Ping_600(std::string sSessionID);

	//Getter
	bool GetAuthentificationSuccessStatus();
	bool GetPingSuccessStatus();
};

