#pragma once
#include "../Network/Network_Manager.h"
#include "../Data/Data_Manager.h"
#include "../Protection/Protection_Manager.h"
#include "../Tools/SplashDisplayer.h"
#include "Exception_Manager.h"
#include <atlbase.h>
#include <boost/filesystem.hpp>

class ProtegaCore
{
private:
	Network_Manager* NetworkManager;
	Protection_Manager* ProtectionManager;


	//Main functions
	//	Callbacks
	void ServerAnswer(unsigned int iTelegramNumber, std::vector<std::string> vInformation);
	void ProtectionManagerAnswer(unsigned int iType, std::vector<std::string> vDetectionInformation);
	//	Thread routine after start
	void Update();
	
	//Support functions
	bool CheckProtegaFiles(const char* sImageFilePath);
public:
	ProtegaCore();
	~ProtegaCore();

	void StartAntihack();
};

