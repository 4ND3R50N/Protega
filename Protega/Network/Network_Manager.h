#pragma once
#include "Tcp_Connector.h"
#include <boost/thread.hpp>
#include <boost/algorithm/string.hpp>
#include <thread>
#include "../Core/Exception_Manager.h"

class Network_Manager
{
private:
	//Vars
	string sIP;
	int iPort;
	std::string sProtocolDelimiter;
	const char* sDataDelimiter;


	int iNetworkErrorCode = 0;

	int iMaxRetries = 0;

	int iAuthenticationTries = 0;
	bool bAuthenticationSuccess = false;

	int iPingTries = 0;
	bool bPingSuccess = false;

	//Only one pair for 3 protocols.
	int iHackDetectionTries = 0;
	bool bHackDetectionSuccess = false;

	const int iAuthenticationProtocolID = 500;
	const int iPingProtocolID = 600;

	const int iHackDetectionHeID = 701;
	const int iHackDetectionVmpID = 702;
	const int iHackDetectionFpID = 703;

	//Functions
	bool SendAndGet(bool * bActualProtocolSuccessVar, int * iActualProtocolTryVar, std::string sMessage);
	void OnReceiveConverter(string sMessage);
	std::function<void(unsigned int iTelegramNumber, std::vector<std::string> lParameters)> funcCallbackHandler;
public:
	Network_Manager(std::string sIP, int iPort, std::string sProtocolDelimiter, const char* sDataDelimiter, int iMaxRetries, int iNetworkErrorCode,
		std::function<void(unsigned int iTelegramNumber, std::vector<std::string> lParameters)> funcCallbackHandler);
	~Network_Manager();
	//Protocols C2S
	bool TestMessage_001();
	void Authentication_500(std::string sHardwareID, int iVersion, std::string sComputerArchitecture, std::string sLanguage, std::string sIP);
	void Ping_600(std::string sSessionID);
	void HackDetection_HE_701(std::string sSessionID, unsigned int iHeSection, std::string sContent);
	void HackDetection_VMP_702(std::string sSessionID, std::string sBaseAddress, std::string sOffset, std::string sDetectedValue, 
		std::string sDefaultValue);
	void HackDetection_FP_703(std::string sSessionID, unsigned int iFpSection, std::string sContent);

	//Getter
	bool GetAuthentificationSuccessStatus();
	bool GetPingSuccessStatus();
};

