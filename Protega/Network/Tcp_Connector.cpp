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
	boost::system::error_code error = boost::asio::error::host_not_found;
	m_Socket.connect(EndPoint, error);
	return true;
}


bool Tcp_Connector::SendAndReceive(string sMessage)
{
	//Try Catch?
	m_SendBuffer = sMessage;
	m_Socket.send(boost::asio::buffer(m_SendBuffer.c_str(), m_SendBuffer.length() + 1));
	char *cJunk;
	for (;;)
	{
		size_t len = m_Socket.read_some(boost::asio::buffer(m_ReceiveBuffer), m_Error);
		
		//todo: ~ -> Global
		cJunk = strstr(m_ReceiveBuffer.c_array(), "~");
		
		if (cJunk != NULL)
			break; // Connection closed cleanly by peer.
		else if (m_Error)
			throw boost::system::system_error(m_Error); // Some other error.
	}
	
	//Delete the junk after the eof + pass the answer to the upper layer
	std::string sFinalData(m_ReceiveBuffer.begin(), m_ReceiveBuffer.end());
	boost::erase_all(sFinalData, cJunk);
	funcCallbackHandler(sFinalData);
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
