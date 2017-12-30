#pragma once
#include <boost/asio.hpp>
#include <boost/array.hpp>
#include <boost/bind.hpp>
#include <boost/algorithm/string.hpp>
#include <memory>
#include <iostream>
#include "../Tools/CryptoPP_Converter.h"

using boost::asio::ip::tcp;
using namespace std;

class Tcp_Connector
{

private:
	//Asio Objects
	boost::asio::io_service& m_IOService;
	tcp::socket m_Socket;
	tcp::endpoint EndPoint;
	tcp::resolver::iterator EndPointIter;

	//Vars
	string m_SendBuffer;
	boost::array<char, 128> m_ReceiveBuffer;
	boost::system::error_code m_Error;
	const char* sProtocolDelimiter;

	bool bEncryptedNetworking = false;
	const char* sAesKey;
	const char* sAesIV;

	//Functions
	std::function<void(string sMessage)> funcCallbackHandler;

	//Events
	void DoClose();
	
public:
	Tcp_Connector(boost::asio::io_service & IO_Service, tcp::resolver::iterator EndPointIter, std::function<void(string sMessage)> funcCallbackHandler, const char * sProtocolDelimiter);
	Tcp_Connector(boost::asio::io_service & IO_Service, tcp::resolver::iterator EndPointIter, std::function<void(string sMessage)> funcCallbackHandler, const char * sProtocolDelimiter,
		const char* sAesKey, const char* sAesIV);
	~Tcp_Connector();
	bool Connect();
	bool SendAndReceive(string sMessage);
	void Close();
};

