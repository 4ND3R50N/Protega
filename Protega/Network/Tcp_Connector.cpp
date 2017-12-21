#include "../stdafx.h"
#include "Tcp_Connector.h"

Tcp_Connector::Tcp_Connector(boost::asio::io_service & IO_Service, tcp::resolver::iterator EndPointIter,
	std::function<void(string sMessage)> funcCallbackHandler) : m_IOService(IO_Service), m_Socket(IO_Service), m_SendBuffer("")
{ 
	EndPoint = *EndPointIter;
	this->EndPointIter = EndPointIter;
	this->funcCallbackHandler = funcCallbackHandler;
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
	m_SendBuffer = CryptoPP_AES_Converter::Encrypt("1234567890123456", "bbbbbbbbbbbbbbbb",sMessage);
	m_Socket.send(boost::asio::buffer(m_SendBuffer.c_str(), m_SendBuffer.length() + 1));
	char *cJunk;
	std::string sDecryptedMessage;
	//INFO: This "for" runs the entire time
	for (;;)
	{
		size_t len = m_Socket.read_some(boost::asio::buffer(m_ReceiveBuffer), m_Error);
		
		//INFO: Globallize the delimiter
		cJunk = strstr(m_ReceiveBuffer.c_array(), "~");

		if (cJunk == NULL)
			return false; // Connection closed cleanly by peer.
		if (m_Error)
			//INFO: Error must be called in the core class
			throw boost::system::system_error(m_Error); // Some other error.

		//Delete the junk after the eof + pass the answer to the upper layer
		std::string sFinalData(m_ReceiveBuffer.begin(), m_ReceiveBuffer.end());
		boost::erase_all(sFinalData, cJunk);

		//todo: ~ -> Global
		sDecryptedMessage  = CryptoPP_AES_Converter::Decrypt("1234567890123456", "bbbbbbbbbbbbbbbb", sFinalData);
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
