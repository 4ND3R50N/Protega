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
	static char* TARGET_ENVIORMENT_VMP_FILE_NAME;

	static char* LOKAL_DATA_FOLDER;
	static char LOKAL_DATA_NEWLINE_DELIMITER;
	static char LOKAL_DATA_DELIMITER;


	static const char* DATA_AES_KEY;
	static const char* DATA_AES_IV;
	
	//Dynamic Data Storage Vars
	static std::string** sHeuristicTable;
	static std::string** sVMPTable;

	//Functions
	static std::string** ConvertStringToMatrix(std::string sData);

public:
	//Functions
	static bool CollectDynamicProtesData();
	static std::string GenerateComputerID();
	//Getter
	static std::string GetTargetEnviormentDataUrl();
	static std::string GetTargetEnviormentHeuristicDataFileName();
	
		


};

