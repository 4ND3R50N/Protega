#pragma once
#include <iostream>
#include <fstream>
#include <vector>
#include "../Tools/CryptoPP_AES_Converter.h"
#include "Data_Gathering.h"

class Data_Manager
{
private:
	//Constructor
	Data_Manager(){}
	//Static Data Storage Vars
	static char* TARGET_ENVIORMENT_OWNER_NAME;
	static char* TARGET_ENVIORMENT_DATA_URL;
	static char* TARGET_ENVIORMENT_HEURISTIC_DATA_FILE_NAME;
	static char* TARGET_ENVIORMENT_FTC_FILE_NAME;

	static char* LOKAL_DATA_FOLDER;
	static char LOKAL_DATA_NEWLINE_DELIMITER;
	static char LOKAL_DATA_DELIMITER;

	static const char* NETWORK_SERVER_IP;
	static const char* NETWORK_SERVER_PORT;
	static const char* NETWORK_PROTOCOL_DELIMITER;
	static const char* NETWORK_DATA_DELIMITER;
	//INFO: Currently we have only 1 pair of AES values. We need 2 for Data and network later!
	static const char* DATA_AES_KEY;
	static const char* DATA_AES_IV;
	
	//Dynamic Data Storage Vars
	static std::string** sHeuristicTable;
	static std::string** sFTCTable;

	//Functions
	static std::string** ConvertStringToMatrix(std::string sData);

public:
	//Functions
	static bool CollectDynamicProtesData();
	static std::string GenerateComputerID();
	//Getter
	static std::string GetTargetEnviormentDataUrl();
	static std::string GetTargetEnviormentHeuristicDataFileName();
	static const char* GetNetworkServerIP();
	static const char* GetNetworkServerPort();
	static const char* GetProtocolDelimiter();
	static const char* GetDataDelimiter();
	static const char* GetNetworkAesKey();
	static const char* GetNetworkAesIV();
		


};

