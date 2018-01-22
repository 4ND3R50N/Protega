#include "../stdafx.h"
#include "Network_Manager.h"

Network_Manager::Network_Manager(std::string sIP, std::string sPort, const char* sProtoclDelimiter, const char* sDataDelimiter, int iMaxRetries, int iNetworkErrorCode, 
	std::function<void(NetworkTelegram Telegram)> funcCallbackHandler)
{
	this->sIP = sIP;
	this->iPort = iPort;
	this->sProtocolDelimiter = sProtocolDelimiter;
	this->sDataDelimiter = sDataDelimiter;
	this->iMaxRetries = iMaxRetries;
	this->iNetworkErrorCode = iNetworkErrorCode;
	this->funcCallbackHandler = funcCallbackHandler;
}

Network_Manager::~Network_Manager()
{
}

//Public
bool Network_Manager::TestMessage_001()
{	
	/*if (!SendAndGet("#001"))
	{
		return false;
	}	*/
	return true;
}

void Network_Manager::Authentication_500(std::string sHardwareID, std::string sVersion, 
	std::string sComputerArchitecture, std::string sLanguage)
{
	iAuthenticationTries = 0;
	bAuthenticationSuccess = false;

	//Build protocol
	std::stringstream ss;
	ss << iAuthenticationProtocolID << sDataDelimiter << sHardwareID << sDataDelimiter << sVersion 
		<< sDataDelimiter << sComputerArchitecture << sDataDelimiter << sLanguage;
	
	//Call SendAndGet threaded with parameters
	std::thread th(&Network_Manager::SendAndGet, this, &bPingSuccess, &iAuthenticationTries, ss.str().c_str());
	th.join();
}

void Network_Manager::Ping_600(std::string sSessionID)
{
	iPingTries = 0;
	bPingSuccess = false;

	//Build protocol
	std::stringstream ss;
	ss << iPingProtocolID;

	std::thread th(&Network_Manager::SendAndGet, this, &bPingSuccess, &iPingTries, ss.str().c_str());
	th.join();
}

//Getter
bool Network_Manager::GetAuthentificationSuccessStatus()
{
	return bAuthenticationSuccess;
}

bool Network_Manager::GetPingSuccessStatus()
{
	return bPingSuccess;
}


//Private
bool Network_Manager::SendAndGet(bool * bActualProtocolSuccessVar, int * iActualProtocolTryVar, const char * sMessage)
{
	try
	{
		boost::asio::io_service IO_Service;
		tcp::resolver Resolver(IO_Service);
		tcp::resolver::query Query(sIP, iPort);
		tcp::resolver::iterator EndPointIterator = Resolver.resolve(Query);

		Tcp_Connector Tcp_C(IO_Service, EndPointIterator, std::bind(&Network_Manager::OnReceiveConverter, this, std::placeholders::_1), sProtocolDelimiter);
		Tcp_C.Connect();
		Tcp_C.SendAndReceive(sMessage);
		Tcp_C.Close();
		Tcp_C.~Tcp_Connector();
		*bActualProtocolSuccessVar = true;

	}
	catch (const std::exception&)
	{
		//if the send/get was not successfull, retry the process. Use the current iActualTryVar address of the specific int value of the protocol to compare it with the max tries.
		//if its lower, then retry, if not, error
		int iActualTries = *iActualProtocolTryVar;

		if (iActualTries <= iMaxRetries)
		{
			SendAndGet(bActualProtocolSuccessVar, iActualProtocolTryVar, sMessage);
			return false;
		}
		else
		{
			Exception_Manager::HandleProtegaStandardError(iNetworkErrorCode,
				"Connection to server failed. Please restart the application and check your network connectivity!");
			return false;
		}		
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


