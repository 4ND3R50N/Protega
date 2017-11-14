#include "../stdafx.h"
#include "Data_Manager.h"

#pragma region PROTES_STATIC_DATA_CONFIG
char* Data_Manager::TARGET_ENVIORMENT_OWNER_NAME = "CodeZero";
char* Data_Manager::TARGET_ENVIORMENT_DATA_URL = "http://62.138.6.50:13011/CabalOnline/";
char* Data_Manager::TARGET_ENVIORMENT_HEURISTIC_DATA_FILE_NAME = "Heuristic_Data.csv.enc";
char* Data_Manager::TARGET_ENVIORMENT_VMP_FILE_NAME = "VMP_Addresses.csv.enc";
char* Data_Manager::LOKAL_DATA_FOLDER = "protega/";
const char* Data_Manager::DATA_AES_KEY = "1234567890123456";
const char* Data_Manager::DATA_AES_IV = "bbbbbbbbbbbbbbbb";
#pragma endregion



//Public
std::string::size_type matching_characters(std::string s1, std::string s2) {
	sort(begin(s1), end(s1));
	sort(begin(s2), end(s2));
	std::string intersection;
	std::set_intersection(begin(s1), end(s1), begin(s2), end(s2),
		back_inserter(intersection));
	return intersection.size();
}

bool Data_Manager::CollectDynamicProtesData()
{
	//Download data from web
	std::stringstream ssUrlCombiner;
	std::stringstream ssHeuristicFilePathCombiner;
	std::stringstream ssVMPFilePathCombiner;

	ssUrlCombiner << TARGET_ENVIORMENT_DATA_URL << TARGET_ENVIORMENT_HEURISTIC_DATA_FILE_NAME;
	ssHeuristicFilePathCombiner << LOKAL_DATA_FOLDER << TARGET_ENVIORMENT_HEURISTIC_DATA_FILE_NAME;
	Data_Manager::DownloadWebFile(strdup(ssUrlCombiner.str().c_str()), strdup(ssHeuristicFilePathCombiner.str().c_str()));
	ssUrlCombiner.str(std::string());


	ssUrlCombiner << TARGET_ENVIORMENT_DATA_URL << TARGET_ENVIORMENT_OWNER_NAME << '/' << TARGET_ENVIORMENT_VMP_FILE_NAME;
	ssVMPFilePathCombiner << LOKAL_DATA_FOLDER << TARGET_ENVIORMENT_VMP_FILE_NAME;
	Data_Manager::DownloadWebFile(strdup(ssUrlCombiner.str().c_str()), strdup(ssVMPFilePathCombiner.str().c_str()));
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

	return true;
}

std::string Data_Manager::GetTargetEnviormentDataUrl()
{
	return TARGET_ENVIORMENT_DATA_URL;
}

std::string Data_Manager::GetTargetEnviormentHeuristicDataFileName()
{
	return TARGET_ENVIORMENT_HEURISTIC_DATA_FILE_NAME;
}

static size_t WriteCallbackForString(void *contents, size_t size, size_t nmemb, void *userp)
{
	((std::string*)userp)->append((char*)contents, size * nmemb);
	return size * nmemb;
}

static size_t WriteCallbackForFile(void *contents, size_t size, size_t nmemb, FILE *userp)
{
	size_t written = fwrite(contents, size, nmemb, userp);
	return written;
}

bool Data_Manager::DownloadWebFile(char* sTarget, char sDestination[FILENAME_MAX])
{
	CURL *curl;
	FILE *fp;
	CURLcode res;

	curl = curl_easy_init();
	if (curl) {
		fp = fopen(sDestination, "wb");
		curl_easy_setopt(curl, CURLOPT_URL, sTarget);
		curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallbackForFile);
		curl_easy_setopt(curl, CURLOPT_WRITEDATA, fp);
		res = curl_easy_perform(curl);
		/* always cleanup */
		curl_easy_cleanup(curl);
		fclose(fp);
	}
	else
	{
		return false;
	}
	return true;
}

std::string Data_Manager::GetWebFileAsString(const char* sTargetURL)
{
	CURL *curl;
	CURLcode res;
	std::string readBuffer;

	curl = curl_easy_init();
	if (curl) {
		curl_easy_setopt(curl, CURLOPT_URL, sTargetURL);
		curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallbackForString);
		curl_easy_setopt(curl, CURLOPT_WRITEDATA, &readBuffer);
		res = curl_easy_perform(curl);
		curl_easy_cleanup(curl);
		//
	}

	return readBuffer;
}
