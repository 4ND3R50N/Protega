#include "../stdafx.h"
#include "Data_Manager.h"

#pragma region PROTES_STATIC_DATA_CONFIG
char* Data_Manager::TARGET_ENVIORMENT_OWNER_NAME = "CodeZero";
char* Data_Manager::TARGET_ENVIORMENT_DATA_URL = "http://62.138.6.50:13011/CabalOnline/";
char* Data_Manager::TARGET_ENVIORMENT_HEURISTIC_DATA_FILE_NAME = "Heuristic_Data.csv.enc";
char* Data_Manager::TARGET_ENVIORMENT_VMP_FILE_NAME = "VMP_Addresses.csv.enc";

char Data_Manager::LOKAL_DATA_NEWLINE_DELIMITER = '#';
char Data_Manager::LOKAL_DATA_DELIMITER = ';';
char* Data_Manager::LOKAL_DATA_FOLDER = ".\\protega\\";

const char* Data_Manager::DATA_AES_KEY = "1234567890123456";
const char* Data_Manager::DATA_AES_IV = "bbbbbbbbbbbbbbbb";

std::string** Data_Manager::sHeuristicTable;
std::string** Data_Manager::sVMPTable;
#pragma endregion



//Public
bool Data_Manager::CollectDynamicProtesData()
{
	//Download data from web
	std::stringstream ssUrlCombiner;
	std::stringstream ssHeuristicFilePathCombiner;
	std::stringstream ssVMPFilePathCombiner;

	ssUrlCombiner << TARGET_ENVIORMENT_DATA_URL << TARGET_ENVIORMENT_HEURISTIC_DATA_FILE_NAME;
	ssHeuristicFilePathCombiner << LOKAL_DATA_FOLDER << TARGET_ENVIORMENT_HEURISTIC_DATA_FILE_NAME;
	Data_Gathering::DownloadWebFile(strdup(ssUrlCombiner.str().c_str()), strdup(ssHeuristicFilePathCombiner.str().c_str()));
	ssUrlCombiner.str(std::string());


	ssUrlCombiner << TARGET_ENVIORMENT_DATA_URL << TARGET_ENVIORMENT_OWNER_NAME << '/' << TARGET_ENVIORMENT_VMP_FILE_NAME;
	ssVMPFilePathCombiner << LOKAL_DATA_FOLDER << TARGET_ENVIORMENT_VMP_FILE_NAME;
	Data_Gathering::DownloadWebFile(strdup(ssUrlCombiner.str().c_str()), strdup(ssVMPFilePathCombiner.str().c_str()));
	ssUrlCombiner.str(std::string());

	//Read encrypted string from files
	std::ifstream isHeuristicFileReader(ssHeuristicFilePathCombiner.str());
	std::string sTempEncryptedHeuristicData((std::istreambuf_iterator<char>(isHeuristicFileReader)), std::istreambuf_iterator<char>());
	isHeuristicFileReader.close();
	std::ifstream isVMPFileReader(ssVMPFilePathCombiner.str());
	std::string sTempEncryptedVirtualMemoryData((std::istreambuf_iterator<char>(isVMPFileReader)), std::istreambuf_iterator<char>());

	//Delete files
	remove(ssHeuristicFilePathCombiner.str().c_str());
	remove(ssVMPFilePathCombiner.str().c_str());

	//Decrypt data
	std::string sTempDecryptedHeuristicData = CryptoPP_AES_Converter::Decrypt(DATA_AES_KEY, DATA_AES_IV, sTempEncryptedHeuristicData);
	std::string sTempDecryptedVirtualMemoryData = CryptoPP_AES_Converter::Decrypt(DATA_AES_KEY, DATA_AES_IV, sTempEncryptedVirtualMemoryData);

	//Convert data to lists
	sHeuristicTable = ConvertStringToMatrix(sTempDecryptedHeuristicData);
	sVMPTable = ConvertStringToMatrix(sTempDecryptedVirtualMemoryData);
	return true;
}

//Private
std::string ** Data_Manager::ConvertStringToMatrix(std::string sData)
{
	//Allocate memory for the matrix
	std::vector<std::string> vDataParts;
	//	important: delete the last '#\0' from string
	boost::split(vDataParts, sData.substr(0, sData.length() - 2), boost::is_any_of("#"));
	short iCurrentLocationInDataParts = 0;

	//	get count of cols we need
	size_t iColCount = std::count(vDataParts[0].begin(), vDataParts[0].end(), LOKAL_DATA_DELIMITER);
	//	get count of entries in csv
	size_t iMatrixRowCount = vDataParts.size();

	std::string** sDataMatrix = new std::string*[iMatrixRowCount];

	for (size_t iCurrentRow = 0; iCurrentRow < iMatrixRowCount; iCurrentRow++)
	{
		sDataMatrix[iCurrentRow] = new std::string[iColCount];
	}


	//Push data into matrix
	for (size_t iCurrentRow = 0; iCurrentRow < iMatrixRowCount; iCurrentRow++)
	{
		for (size_t iCurrentCol = 0; iCurrentCol < iColCount; iCurrentCol++)
		{
			std::vector<std::string> vCurrentDataLine;
			boost::split(vCurrentDataLine, vDataParts[iCurrentRow], boost::is_any_of(";"));
			sDataMatrix[iCurrentRow][iCurrentCol] = vCurrentDataLine[iCurrentCol];
			iCurrentLocationInDataParts++;
		}
	}
	return sDataMatrix;
}

//Getter Setter
std::string Data_Manager::GetTargetEnviormentDataUrl()
{
	return TARGET_ENVIORMENT_DATA_URL;
}

std::string Data_Manager::GetTargetEnviormentHeuristicDataFileName()
{
	return TARGET_ENVIORMENT_HEURISTIC_DATA_FILE_NAME;
}

