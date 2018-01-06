#include "../stdafx.h"
#include "Data_Manager.h"

#pragma region PROTES_STATIC_DATA_CONFIG
//Web data
char* Data_Manager::TARGET_ENVIORMENT_OWNER_NAME = "CodeZero";
char* Data_Manager::TARGET_ENVIORMENT_DATA_URL = "http://62.138.6.50:13011/CabalOnline/";
char* Data_Manager::TARGET_ENVIORMENT_HEURISTIC_MD5_FILENAME = "Heuristic_MD5.csv.enc";
char* Data_Manager::TARGET_ENVIORMENT_HEURISTIC_PROCESSNAME_FILENAME = "Heuristic_Process_Names.csv.enc";
char* Data_Manager::TARGET_ENVIORMENT_FTC_FILE_NAME = "Files_To_Check.csv.enc";

//Local data
const char* Data_Manager::LOCAL_DATA_NEWLINE_DELIMITER = "~";
const char* Data_Manager::LOCAL_DATA_DELIMITER = ";";
char* Data_Manager::LOCAL_DATA_FOLDER = ".\\protega\\";
std::string Data_Manager::LOCAL_DATA_PROTECTION_TARGET = "CabalMain22.exe";

//Network data
const char* Data_Manager::NETWORK_SERVER_IP = "62.138.6.50";
const char* Data_Manager::NETWORK_SERVER_PORT = "13001";
const char* Data_Manager::NETWORK_PROTOCOL_DELIMITER = "~";
const char* Data_Manager::NETWORK_DATA_DELIMITER = ";";
const char* Data_Manager::DATA_AES_KEY = "1234567890123456";
const char* Data_Manager::DATA_AES_IV = "bbbbbbbbbbbbbbbb";

//Content data
std::list<std::string> Data_Manager::lHeuristicMD5Values;
std::list<std::wstring> Data_Manager::lHeuristicProcessNames;

//Protection data
double Data_Manager::PROTECTION_THREAD_RESPONSE_DELTA = 20.0;
#pragma endregion



//Public
bool Data_Manager::CollectDynamicProtesData()
{
	//Download data from web
	std::stringstream ssUrlCombiner;
	std::stringstream ssLocalPathCombiner;


	ssUrlCombiner << TARGET_ENVIORMENT_DATA_URL << TARGET_ENVIORMENT_HEURISTIC_MD5_FILENAME;
	ssLocalPathCombiner << LOCAL_DATA_FOLDER << TARGET_ENVIORMENT_HEURISTIC_MD5_FILENAME;
	Data_Gathering::DownloadWebFile(_strdup(ssUrlCombiner.str().c_str()), _strdup(ssLocalPathCombiner.str().c_str()));
	std::ifstream isHeuristicMD5FileReader(ssLocalPathCombiner.str());
	std::string sTempEncryptedMD5Data((std::istreambuf_iterator<char>(isHeuristicMD5FileReader)), std::istreambuf_iterator<char>());
	isHeuristicMD5FileReader.close();
	ssUrlCombiner.str("");
	ssLocalPathCombiner.str("");

	ssUrlCombiner << TARGET_ENVIORMENT_DATA_URL << TARGET_ENVIORMENT_HEURISTIC_PROCESSNAME_FILENAME;
	ssLocalPathCombiner << LOCAL_DATA_FOLDER << TARGET_ENVIORMENT_HEURISTIC_PROCESSNAME_FILENAME;
	Data_Gathering::DownloadWebFile(_strdup(ssUrlCombiner.str().c_str()), _strdup(ssLocalPathCombiner.str().c_str()));
	std::ifstream isHeuristicProcessNameFileReader(ssLocalPathCombiner.str());
	std::string sTempEncryptedProcessNameData((std::istreambuf_iterator<char>(isHeuristicProcessNameFileReader)), std::istreambuf_iterator<char>());
	isHeuristicProcessNameFileReader.close();
	ssUrlCombiner.str("");
	ssLocalPathCombiner.str("");

	//Delete files
	//remove(ssHeuristicFilePathCombiner.str().c_str());
	//remove(ssFTCFilePathCombiner.str().c_str());

	//Decrypt data
	std::string sTempDecryptedMD5Data = CryptoPP_Converter::AESDecrypt(DATA_AES_KEY, DATA_AES_IV, sTempEncryptedMD5Data);
	std::string sTempDecryptedProcessNameData = CryptoPP_Converter::AESDecrypt(DATA_AES_KEY, DATA_AES_IV, sTempEncryptedProcessNameData);

	//Convert data to lists
	lHeuristicMD5Values = ConvertStringToStringList(sTempDecryptedMD5Data);
	lHeuristicProcessNames = ConvertStringToWStringList(sTempDecryptedProcessNameData);
	return true;
}

std::string Data_Manager::GenerateComputerID()
{
	std::stringstream ssComputerID;
	ssComputerID << Data_Gathering::GetCpuHash() << "-" << Data_Gathering::GetVolumeHash();
	return ssComputerID.str();
}


//Private
std::list<std::string> Data_Manager::ConvertStringToStringList(std::string sData)
{

	////Allocate memory for the matrix
	//std::vector<std::string> vDataParts;
	////	important: delete the last '#\0' from string
	//boost::split(vDataParts, sData.substr(0, sData.length() - 2), boost::is_any_of("#"));
	//short iCurrentLocationInDataParts = 0;

	////	get count of cols we need
	//size_t iColCount = std::count(vDataParts[0].begin(), vDataParts[0].end(), LOCAL_DATA_DELIMITER);
	////	get count of entries in csv
	//size_t iMatrixRowCount = vDataParts.size();

	//std::string** sDataMatrix = new std::string*[iMatrixRowCount];

	//for (size_t iCurrentRow = 0; iCurrentRow < iMatrixRowCount; iCurrentRow++)
	//{
	//	sDataMatrix[iCurrentRow] = new std::string[iColCount];
	//}


	////Push data into matrix
	//for (size_t iCurrentRow = 0; iCurrentRow < iMatrixRowCount; iCurrentRow++)
	//{
	//	for (size_t iCurrentCol = 0; iCurrentCol < iColCount; iCurrentCol++)
	//	{
	//		std::vector<std::string> vCurrentDataLine;
	//		boost::split(vCurrentDataLine, vDataParts[iCurrentRow], boost::is_any_of(";"));
	//		sDataMatrix[iCurrentRow][iCurrentCol] = vCurrentDataLine[iCurrentCol];
	//		iCurrentLocationInDataParts++;
	//	}
	//}
	std::vector<std::string> vDataParts;
	std::vector<std::string> vTargetData;
	std::list<std::string> lTargetData;
	boost::split(vDataParts, sData.substr(0, sData.length() - 2), boost::is_any_of(LOCAL_DATA_NEWLINE_DELIMITER));
	boost::split(vTargetData, vDataParts[1].substr(0, sData.length() - 2), boost::is_any_of(LOCAL_DATA_DELIMITER));

	std::copy(vTargetData.begin(), vTargetData.end(), std::back_inserter(lTargetData));
	return lTargetData;
}

std::list<std::wstring> Data_Manager::ConvertStringToWStringList(std::string sData)
{
	std::wstring wsConvertedString;
	StringToWString(sData, &wsConvertedString);
	std::vector<std::wstring> vDataParts;
	std::vector<std::wstring> vTargetData;
	std::list<std::wstring> lTargetData;
	boost::split(vDataParts, wsConvertedString.substr(0, wsConvertedString.length() - 1), boost::is_any_of(LOCAL_DATA_NEWLINE_DELIMITER));
	boost::split(vTargetData, vDataParts[1].substr(0, wsConvertedString.length() - 1), boost::is_any_of(LOCAL_DATA_DELIMITER));
	std::copy(vTargetData.begin(), vTargetData.end(), std::back_inserter(lTargetData));
	return lTargetData;
}

void Data_Manager::StringToWString(std::string sStringToConvert, std::wstring * wsOutput)
{
	std::wstring ws(sStringToConvert.size(), L' '); // Overestimate number of code points.
	ws.resize(std::mbstowcs(&ws[0], sStringToConvert.c_str(), sStringToConvert.size())); // Shrink to fit.
	*wsOutput = ws;
}

//Getter Setter
std::string Data_Manager::GetTargetEnviormentDataUrl()
{
	return TARGET_ENVIORMENT_DATA_URL;
}

const char * Data_Manager::GetNetworkServerIP()
{
	return NETWORK_SERVER_IP;
}

const char * Data_Manager::GetNetworkServerPort()
{
	return NETWORK_SERVER_PORT;
}

const char * Data_Manager::GetProtocolDelimiter()
{
	return NETWORK_PROTOCOL_DELIMITER;
}

const char * Data_Manager::GetDataDelimiter()
{
	return NETWORK_DATA_DELIMITER;
}

const char * Data_Manager::GetNetworkAesKey()
{
	return DATA_AES_KEY;
}

const char * Data_Manager::GetNetworkAesIV()
{
	return DATA_AES_IV;
}

double Data_Manager::GetProtectionThreadResponseDelta()
{
	return PROTECTION_THREAD_RESPONSE_DELTA;
}

std::string Data_Manager::GetLocalDataProtectionTarget()
{
	return LOCAL_DATA_PROTECTION_TARGET;
}

std::list<std::wstring> Data_Manager::GetHeuristicProcessNames()
{
	return lHeuristicProcessNames;
}

std::list<std::string> Data_Manager::GetHeuristicMD5Values()
{
	return lHeuristicMD5Values;
}


