#pragma once
#include <iostream>
#include <stdio.h>
#include <stdlib.h>
#include <curl\curl.h>
#include <fstream>
#include "../Tools/CryptoPP_AES_Converter.h"
#include <vector>

class Data_Manager
{
private:
	//Constructor
	Data_Manager(){}
	//Static Data Storage Vars
	static char* TARGET_ENVIORMENT_OWNER_NAME;
	static char* TARGET_ENVIORMENT_DATA_URL;
	static char* TARGET_ENVIORMENT_HEURISTIC_DATA_FILE_NAME;
	static char* TARGET_ENVIORMENT_VMP_FILE_NAME;

	static char* LOKAL_DATA_FOLDER;
	static char LOKAL_DATA_DELIMITER;

	static const char* DATA_AES_KEY;
	static const char* DATA_AES_IV;
	
	//Dynamic Data Storage Vars
	static std::string** sHeuristicTable;
	static std::string** sVMPTable;
	//Functions

public:
	//Functions
	static bool CollectDynamicProtesData();
	//Getter
	static std::string GetTargetEnviormentDataUrl();
	static std::string GetTargetEnviormentHeuristicDataFileName();

	//Support functions
	static bool DownloadWebFile(char* sTarget, char sDestination[FILENAME_MAX]);
	static std::string GetWebFileAsString(const char* sTargetURL);
};

