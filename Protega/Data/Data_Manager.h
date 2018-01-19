#pragma once
#include <iostream>
#include <fstream>
#include <vector>
#include "../Tools/CryptoPP_Converter.h"
#include "Data_Gathering.h"

class Data_Manager
{
private:
	//Constructor
	Data_Manager(){}
	//Static Data Storage Vars
	static char* TARGET_ENVIORMENT_OWNER_NAME;
	static char* TARGET_ENVIORMENT_DATA_URL;
	static char* TARGET_ENVIORMENT_HEURISTIC_MD5_FILENAME;
	static char* TARGET_ENVIORMENT_HEURISTIC_PROCESSNAME_FILENAME;
	static char* TARGET_ENVIORMENT_FTC_FILE_NAME;

	static const char* LOCAL_DATA_FOLDER;
	static const char* LOCAL_DATA_PROTEGA_IMAGE;
	static const char* LOCAL_DATA_NEWLINE_DELIMITER;
	static const char* LOCAL_DATA_DELIMITER;
	static std::string LOCAL_DATA_PROTECTION_TARGET;

	static const char* NETWORK_SERVER_IP;
	static const char* NETWORK_SERVER_PORT;
	static const char* NETWORK_PROTOCOL_DELIMITER;
	static const char* NETWORK_DATA_DELIMITER;

	//INFO: Currently we have only 1 pair of AES values. We need 2 for Data and network later!
	static const char* DATA_AES_KEY;
	static const char* DATA_AES_IV;
	
	static double PROTECTION_THREAD_RESPONSE_DELTA;

	static const char* EXCEPTION_CAPTION;
	static int EXCEPTION_LOCAL_FILE_ERROR;
	static int EXCEPTION_WEB_DOWNLOAD_ERROR;
	static int EXCEPTION_DATA_CONVERSION_ERROR;
	static int EXCEPTION_VM_ERROR;
	static int EXCEPTION_FP_ERROR;
	static int EXCEPTION_THREAD_ERROR;

	//Dynamic Data Storage Vars
	static std::vector<std::string> vHeuristicMD5Values;
	static std::vector<std::wstring> vHeuristicProcessNames;
	static std::pair<std::vector<std::string>, std::vector<std::string>> pFilesToCheck;

	//Functions
	static std::string ConvertENCToDecryptedString(std::string sPathToEnc);
	static std::vector<std::string> ConvertStringToStringList(std::string sData);
	static std::pair<std::vector<std::string>, std::vector<std::string>> ConvertStringToPairOfStringLists(std::string sData);
	static std::vector<std::wstring> ConvertStringToWStringList(std::string sData);
	static void StringToWString(std::string sStringToConvert, std::wstring* wsOutput);

public:
	//Functions
	static int CollectDynamicProtesData();
	static std::string GenerateComputerID();
	//Getter
	static std::string GetTargetEnviormentDataUrl();
	static const char* GetNetworkServerIP();
	static const char* GetNetworkServerPort();
	static const char* GetProtocolDelimiter();
	static const char* GetDataDelimiter();
	static const char* GetNetworkAesKey();
	static const char* GetNetworkAesIV();
	static double GetProtectionThreadResponseDelta();
	static std::string GetLocalDataProtectionTarget();
	static const char* GetLocalDataFolder();
	static const char* GetLocalProtegaImage();
	static std::vector<std::wstring> GetHeuristicProcessNames();
	static std::vector<std::string> GetHeuristicMD5Values();
	static std::pair<std::vector<std::string>, std::vector<std::string>> GetFilesToCheckValues();

	static const char* GetExceptionCaption();
	static int GetExceptionLocalFileErrorNumber();
	static int GetExceptionWebDownloadErrorNumber();
	static int GetExceptionDataConversionErrorNumber();
	static int GetExceptionVmErrorNumber();
	static int GetExceptionFpErrorNumber();
	static int GetExceptionThreadErrorNumber();

};

