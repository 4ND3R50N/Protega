#include "../stdafx.h"
#include "Service_Manager.h"

Service_Manager::Service_Manager(std::function<void(unsigned int iType, std::string sData)> funcCallbackHandler, int iTargetApplicationId)
{
	this->funcCallbackHandler = funcCallbackHandler;
	CboService = new Service_Cabal_Online(iTargetApplicationId);
}

Service_Manager::~Service_Manager()
{
}

//Private


//Public

void Service_Manager::JobCallbackCurrentCabalAccount()
{
	//Start thread to receive the current account per change
	std::string sCurrentAccount = "";

	std::thread tAccountWatcher([&] 
	{		
		if (sCurrentAccount.c_str() != GetCurrentCabalAccount())
		{
			sCurrentAccount = GetCurrentCabalAccount();
			funcCallbackHandler(iAccountServiceTypeNumber, sCurrentAccount);
		}
	});
	tAccountWatcher.join();
}

std::string Service_Manager::GetCurrentCabalAccount()
{
	return CboService->GetCabalAccountName();
}

