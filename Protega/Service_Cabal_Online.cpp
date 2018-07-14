#include "stdafx.h"
#include "Service_Cabal_Online.h"

Service_Cabal_Online::Service_Cabal_Online(std::function<void(std::string sInformation)> funcCallbackHandler)
{
	this->funcCallbackHandler = funcCallbackHandler;
}

Service_Cabal_Online::~Service_Cabal_Online()
{
}
