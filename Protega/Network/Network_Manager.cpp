#include "../stdafx.h"
#include "Network_Manager.h"

Network_Manager::Network_Manager(std::string sIP, std::string sPort, const char* sProtoclDelimiter, const char* sDataDelimiter, std::function<void(NetworkTelegram Telegram)> funcCallbackHandler)
{
	this->sIP = sIP;
	this->iPort = iPort;
	this->sProtocolDelimiter = sProtocolDelimiter;
	this->sDataDelimiter = sDataDelimiter;
	this->funcCallbackHandler = funcCallbackHandler;
}

Network_Manager::~Network_Manager()
{
}

//Public
bool Network_Manager::TestMessage_001()
{	
	if (!SendAndGet("#001"))
	{
		return false;
	}	
	return true;
}


//Private
bool Network_Manager::SendAndGet(const char * sMessage)
{
	try
	{
		boost::asio::io_service IO_Service;
		tcp::resolver Resolver(IO_Service);
		tcp::resolver::query Query(sIP, iPort);
		tcp::resolver::iterator EndPointIterator = Resolver.resolve(Query);

		Tcp_Connector Test(IO_Service, EndPointIterator, std::bind(&Network_Manager::OnReceiveConverter, this, std::placeholders::_1), sProtocolDelimiter);
		Test.Connect();
		Test.SendAndReceive(sMessage);
		Test.Close();
		Test.~Tcp_Connector();
	}
	catch (const std::exception&e)
	{
		MessageBoxA(NULL, e.what(), "Protega antihack engine", NULL);
		return false;
	}
	return true;
}

void Network_Manager::OnReceiveConverter(string sMessage)
{
	std::vector<std::string> lParameters;
	boost::split(lParameters, sMessage, boost::is_any_of(sDataDelimiter));
	NetworkTelegramMessage.iTelegramNumber = atoi(lParameters[0].c_str());
	lParameters.erase(lParameters.begin());
	NetworkTelegramMessage.lParameters = lParameters;

	funcCallbackHandler(NetworkTelegramMessage);

}


