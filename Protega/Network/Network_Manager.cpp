#include "../stdafx.h"
#include "Network_Manager.h"

Network_Manager::Network_Manager(std::string sIP, int iPort, std::string sProtocolDelimiter, const char* sDataDelimiter, int iMaxRetries, int iNetworkErrorCode,
	std::function<void(unsigned int iTelegramNumber, std::vector<std::string> lParameters)> funcCallbackHandler)
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

void Network_Manager::Authentication_500(std::string sHardwareID, int iVersion, 
	std::string sComputerArchitecture, std::string sLanguage, std::string sIP)
{
	iAuthenticationTries = 0;
	bAuthenticationSuccess = false;

	//Build protocol
	std::stringstream ss;
	// BF1426C6DA
	ss << iAuthenticationProtocolID << sDataDelimiter << sHardwareID << sDataDelimiter << std::to_string(iVersion) << sDataDelimiter << "D6D4ABB30s"
		<< sDataDelimiter << sComputerArchitecture << sDataDelimiter << sLanguage << sDataDelimiter << sIP;
	//Call SendAndGet threaded with parameters
	std::thread* th = new std::thread(&Network_Manager::SendAndGet, this, &bAuthenticationSuccess, &iAuthenticationTries, ss.str());

}

void Network_Manager::Ping_600(std::string sSessionID)
{
	iPingTries = 0;
	bPingSuccess = false;

	//Build protocol
	std::stringstream ss;
	ss << iPingProtocolID << sDataDelimiter << sSessionID;

	std::thread* th = new std::thread(&Network_Manager::SendAndGet, this, &bPingSuccess, &iPingTries, ss.str());
	//th.join();
}

void Network_Manager::HackDetection_HE_701(std::string sSessionID, unsigned int iHeSection, std::string sContent)
{
	//Build protocol
	std::stringstream ss;
	ss << iHackDetectionHeID << sDataDelimiter << sSessionID << sDataDelimiter << iHeSection << sDataDelimiter << sContent;

	std::thread* th = new std::thread(&Network_Manager::SendAndGet, this, &bHackDetectionSuccess, &iHackDetectionTries, ss.str());
	//th.join();
}

void Network_Manager::HackDetection_VMP_702(std::string sSessionID, std::string sBaseAddress, std::string sOffset, std::string sDetectedValue, std::string sDefaultValue)
{
	//Build protocol
	std::stringstream ss;
	ss << iHackDetectionVmpID << sDataDelimiter << sSessionID << sDataDelimiter << sBaseAddress << sDataDelimiter << sOffset << sDataDelimiter
		<< sDetectedValue << sDataDelimiter << sDefaultValue;

	std::thread* th = new std::thread(&Network_Manager::SendAndGet, this, &bHackDetectionSuccess, &iHackDetectionTries, ss.str());
	//th.join();
}

void Network_Manager::HackDetection_FP_703(std::string sSessionID, unsigned int iFpSection, std::string sContent)
{
	//Build protocol
	std::stringstream ss;
	ss << iHackDetectionFpID << sDataDelimiter << sSessionID << sDataDelimiter << iFpSection << sDataDelimiter << sContent;

	std::thread* th = new std::thread(&Network_Manager::SendAndGet, this, &bHackDetectionSuccess, &iHackDetectionTries, ss.str());
	//th.join();
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
bool Network_Manager::SendAndGet(bool * bActualProtocolSuccessVar, int * iActualProtocolTryVar, std::string sMessage)
{
	try
	{
		boost::asio::io_service IO_Service;
		tcp::resolver Resolver(IO_Service);
		tcp::resolver::query Query(sIP, std::to_string(iPort));
		tcp::resolver::iterator EndPointIterator = Resolver.resolve(Query);
		//AES Variablen in Network manager reinpacken!
		Tcp_Connector Tcp_C(IO_Service, EndPointIterator, std::bind(&Network_Manager::OnReceiveConverter, this, std::placeholders::_1), sProtocolDelimiter,"1234567890123456", "bbbbbbbbbbbbbbbb");
		Tcp_C.Connect();
		if (!Tcp_C.SendAndReceive(sMessage))
		{
			throw std::exception();
		}
		Tcp_C.Close();
		Tcp_C.~Tcp_Connector();
		*bActualProtocolSuccessVar = true;

	}
	catch (std::exception)
	{
		//if the send/get was not successfull, retry the process. Use the current iActualTryVar address of the specific int value of the protocol to compare it with the max tries.
		//if its lower, then retry, if not, error

		int iActualTries = *iActualProtocolTryVar;
		
		if (iActualTries <= iMaxRetries)
		{
			*iActualProtocolTryVar = *iActualProtocolTryVar + 1;
			SendAndGet(bActualProtocolSuccessVar, iActualProtocolTryVar, sMessage);
			return false;
		}
		else
		{
			Exception_Manager::HandleProtegaStandardError(iNetworkErrorCode,
				"Connection to server failed. Please restart the application and check your network connectivity! [1]");
			return false;
		}		
	}
	return true;
}

void Network_Manager::OnReceiveConverter(string sMessage)
{
	std::vector<std::string> lParameters;
	boost::split(lParameters, sMessage, boost::is_any_of(sDataDelimiter));

	int iTelegramNumber = atoi(lParameters[0].c_str());

	lParameters.erase(lParameters.begin());

	funcCallbackHandler(iTelegramNumber, lParameters);
}


