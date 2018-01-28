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
	char *cTmpJunk;
	char *cJunk;

	std::string sDecryptedMessage;
	//INFO: This "for" runs the entire time
	for (;;)
	{
		//NOTE: This block waits forever, if nothing gets send back
		size_t len = m_Socket.read_some(boost::asio::buffer(m_ReceiveBuffer), m_Error);
		
		cTmpJunk = strstr(m_ReceiveBuffer.c_array(), sProtocolDelimiter.c_str());

		if (cTmpJunk == NULL)
			return false; // Connection closed cleanly by peer.

		while ((cTmpJunk = strstr(cTmpJunk, sProtocolDelimiter.c_str())) != NULL)
		{
			cJunk = cTmpJunk;
			++cTmpJunk;
		}			
				
		if (m_Error)
			//INFO: Error must be called in the core class
			throw boost::system::system_error(m_Error); // Some other error.

		//Delete the junk after the eof + pass the answer to the upper layer
		std::string sFinalData(m_ReceiveBuffer.begin(), m_ReceiveBuffer.end());
		boost::erase_all(sFinalData, cJunk);

		//todo: ~ -> Global
		if (bEncryptedNetworking)
		{
			sDecryptedMessage = CryptoPP_Converter::AESDecrypt(sAesKey, sAesIV, sFinalData);
		}
		else
		{
			sDecryptedMessage = sFinalData;
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
