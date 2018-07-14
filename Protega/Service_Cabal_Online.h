#pragma once
#include <functional>
#include <vector>

class Service_Cabal_Online
{
private:
	std::function<void(std::string sInformation)> funcCallbackHandler;


public:
	Service_Cabal_Online(std::function<void(std::string sInformation)> funcCallbackHandler);
	~Service_Cabal_Online();
};

