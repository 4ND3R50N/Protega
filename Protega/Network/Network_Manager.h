#pragma once
#include "Tcp_Connector.h"
#include <boost/thread.hpp>
#include <thread>

class Network_Manager
{
private:
	//Vars
	string sIP;
	string iPort;
	void OnReceiveConverter(string sMessage);
	void Send(string sMessage);
	//Functions
	std::function<void(string sMessage)> funcCallbackHandler;
public:
	Network_Manager(string sIP, string iPort, std::function<void(string sMessage)> funcCallbackHandler);
	~Network_Manager();
	bool TestMessage_001();
};

