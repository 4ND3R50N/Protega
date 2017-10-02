#include "stdafx.h"
#include "Network_Manager.h"

Network_Manager::Network_Manager(string sIP, string iPort, std::function<void(string sMessage)> funcCallbackHandler)
{
	this->sIP = sIP;
	this->iPort = iPort;
	this->funcCallbackHandler = funcCallbackHandler;
}

Network_Manager::~Network_Manager()
{
}

bool Network_Manager::TestMessage_001()
{
	try
	{
		boost::asio::io_service IO_Service;
		tcp::resolver Resolver(IO_Service);
		tcp::resolver::query Query(sIP, iPort);
		tcp::resolver::iterator EndPointIterator = Resolver.resolve(Query);

		Tcp_Connector Test(IO_Service, EndPointIterator, std::bind(&Network_Manager::OnReceiveConverter, this, std::placeholders::_1));
		Test.Connect();
		Test.SendAndReceive("#001");
		Test.Close();
		Test.~Tcp_Connector();
	}
	catch (exception& e)
	{
		MessageBoxA(NULL, e.what(), "Protega antihack engine", NULL);
	}


	return true;
}

void Network_Manager::OnReceiveConverter(string sMessage)
{
	//Todo: Convert the string to a structure like a struct or list

	funcCallbackHandler(sMessage);

}



