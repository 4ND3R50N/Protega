#pragma once
#include <functional>
#include <thread>
#include "Modules\Service_Cabal_Online_Collector.h"

class Service_Manager
{
private:
	unsigned int iAccountServiceTypeNumber = 1;
	std::thread* tAccountThread;

	//Classes
	Service_Cabal_Online* CboService;

	//Functions
	std::function<void(unsigned int iType, std::string sData)> funcCallbackHandler;

public:
	Service_Manager(std::function<void(unsigned int iType, std::string sData)> funcCallbackHandler, int iTargetApplicationId);

	void JobCallbackCurrentCabalAccount();

	std::string GetCurrentCabalAccount();
	~Service_Manager();
};

