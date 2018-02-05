#include "../stdafx.h"
#include "Tcp_Connector.h"

Tcp_Connector::Tcp_Connector(boost::asio::io_service & IO_Service, tcp::resolver::iterator EndPointIter,
	std::function<void(string sMessage)> funcCallbackHandler, std::string sProtocolDelimiter) : m_IOService(IO_Service), m_Socket(IO_Service), m_SendBuffer("")
{ 
	EndPoint = *EndPointIter;
	this->EndPointIter = EndPointIter;
	this->funcCallbackHandler = funcCallbackHandler;
	this->sProtocolDelimiter = sProtocolDelimiter;
}

Tcp_Connector::Tcp_Connector(boost::asio::io_service & IO_Service, tcp::resolver::iterator EndPointIter, 
	std::function<void(string sMessage)> funcCallbackHandler, std::string sProtocolDelimiter, const char * sAesKey, const char * sAesIV) : m_IOService(IO_Service), m_Socket(IO_Service), m_SendBuffer("")
{
	EndPoint = *EndPointIter;
	this->EndPointIter = EndPointIter;
	this->funcCallbackHandler = funcCallbackHandler;
	this->sProtocolDelimiter = sProtocolDelimiter;
	this->sAesKey = sAesKey;
	this->sAesIV = sAesIV; 
	this->bEncryptedNetworking = true;
}

Tcp_Connector::~Tcp_Connector()
{

}

bool Tcp_Connector::Connect()
{
	//INFO: Try catch!!!
	boost::system::error_code error = boost::asio::error::host_not_found;
	m_Socket.connect(EndPoint, error);
	return true;
}

bool Tcp_Connector::SendAndReceive(string sMessage)
{
	//Try Catch?
	if (bEncryptedNetworking)
	{
		m_SendBuffer = CryptoPP_Converter::AESEncrypt(sAesKey, sAesIV, sMessage);
	}
	else
	{
		m_SendBuffer = sMessage;
	}
	
	m_Socket.send(boost::asio::buffer(m_SendBuffer.c_str(), m_SendBuffer.length() + 1));

	std::string sDecryptedMessage;
	//INFO: This "for" runs the entire time
	for (;;)
	{
		//NOTE: This block waits forever, if nothing gets send back
		size_t len = m_Socket.read_some(boost::asio::buffer(m_ReceiveBuffer), m_Error);
	
		if (m_Error)
			//INFO: Error must be called in the core class
			throw boost::system::system_error(m_Error); // Some other error.
		
				
		//Delete the junk after the eof + pass the answer to the upper layer
		std::string sFullEncryptedProtocol(m_ReceiveBuffer.begin(), m_ReceiveBuffer.end());
		unsigned int iProtLength = atoi(sFullEncryptedProtocol.substr(0, 3).c_str());

		std::string sEncryptedRealProtocol = sFullEncryptedProtocol.substr(3, iProtLength);

		/*ofstream test("Release.log");
		std::stringstream ss;
		ss << "Vor dem erase: " << sEncryptedRealProtocol << endl;
		ss << "Der Junk: " << cJunk << endl;
		boost::erase_all(sEncryptedRealProtocol, cJunk);
		ss << "Nach dem erase: " << sEncryptedRealProtocol << endl;
		test << ss.str();
		test.close();*/

		//todo: ~ -> Global
		if (bEncryptedNetworking)
		{
			sDecryptedMessage = CryptoPP_Converter::AESDecrypt(sAesKey, sAesIV, sEncryptedRealProtocol);
		}
		else
		{
			sDecryptedMessage = sEncryptedRealProtocol;
		}

		break;
	}
	
	funcCallbackHandler(sDecryptedMessage);
	return true;
}

void Tcp_Connector::Close()
{
	m_IOService.post(
		boost::bind(&Tcp_Connector::DoClose, this));
}

void Tcp_Connector::DoClose()
{
	m_Socket.close();
}
