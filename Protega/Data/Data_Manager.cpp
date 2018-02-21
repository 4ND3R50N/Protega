#include "../stdafx.h"
#include "Data_Manager.h"

#pragma region PROTES_STATIC_DATA_CONFIG

int Data_Manager::SOFTWARE_VERSION = 100;

//Web data

char* Data_Manager::TARGET_ENVIORMENT_DATA_URL = "http://62.138.6.50:13011/CabalOnline/";
char* Data_Manager::TARGET_ENVIORMENT_HEURISTIC_MD5_FILENAME = "Heuristic_MD5.csv.enc";
char* Data_Manager::TARGET_ENVIORMENT_HEURISTIC_PROCESSNAME_FILENAME = "Heuristic_Process_Names.csv.enc";
char* Data_Manager::TARGET_ENVIORMENT_FTC_FILE_NAME = "Files_To_Check.csv.enc";

//Local data
std::string Data_Manager::LOCAL_HARDWARE_SID = "";
const char* Data_Manager::LOCAL_DATA_NEWLINE_DELIMITER = "~";
const char* Data_Manager::LOCAL_DATA_DELIMITER = ";";
const char* Data_Manager::LOCAL_DATA_FOLDER = "protega\\";
const char* Data_Manager::LOCAL_DATA_PROTEGA_IMAGE = "Protega_Logo.bmp";
std::string Data_Manager::LOCAL_DATA_PROTECTION_TARGET = "CabalMain.exe";

//Network data
const char* Data_Manager::NETWORK_SERVER_IP = "62.138.6.50";
int Data_Manager::NETWORK_SERVER_PORT = 13010;
int Data_Manager::NETWORK_MAX_SEND_RETRIES = 3;
std::string Data_Manager::NETWORK_PROTOCOL_DELIMITER = "~";
const char* Data_Manager::NETWORK_DATA_DELIMITER = ";";
const char* Data_Manager::DATA_AES_KEY = "1234567890123456";
const char* Data_Manager::DATA_AES_IV = "bbbbbbbbbbbbbbbb";



//Content data
std::vector<std::string> Data_Manager::vHeuristicMD5Values;
std::vector<std::wstring> Data_Manager::vHeuristicProcessNames;
std::pair<std::vector<std::string>, std::vector<std::string>> Data_Manager::pFilesToCheck;

//Protection data
double Data_Manager::PROTECTION_THREAD_RESPONSE_DELTA = 60.0;
int Data_Manager::PROTECTION_FP_MAX_DLL = 126;

//Exceptions
const char* Data_Manager::EXCEPTION_ERROR_FILE_NAME = "latest_protega_error.err";
const char* Data_Manager::EXCEPTION_CRASH_REPORTER_NAME = "CrashReporter.exe";

int Data_Manager::EXCEPTION_LOCAL_FILE_ERROR = 301;
int Data_Manager::EXCEPTION_WEB_DOWNLOAD_ERROR = 302;
int Data_Manager::EXCEPTION_DATA_CONVERSION_ERROR = 303;
int Data_Manager::EXCEPTION_VM_ERROR = 304;
int Data_Manager::EXCEPTION_FP_ERROR = 305;
int Data_Manager::EXCEPTION_THREAD_ERROR = 306;
int Data_Manager::EXCEPTION_NETWORK_ERROR = 307;
#pragma endregion


//Private

std::string Data_Manager::ConvertENCToDecryptedString(std::string sPathToEnc)
{
	//Read enc file
	std::string sEncodedString = "";
	std::string sCurrentLine;
	std::ifstream ifEncFile(sPathToEnc);

	while (getline(ifEncFile, sCurrentLine))  // same as: while (getline( myfile, line ).good())
	{
		sEncodedString += ((char)atoi(sCurrentLine.c_str()));
	}
	ifEncFile.close();
	remove(sPathToEnc.c_str());

	return CryptoPP_Converter::AESDecrypt(DATA_AES_KEY, DATA_AES_IV, sEncodedString);
}

std::vector<std::string> Data_Manager::ConvertStringToStringList(std::string sData)
{
	std::vector<std::string> vDataParts;
	std::vector<std::string> vTargetData;


	try
	{
		boost::split(vDataParts, sData.substr(0, sData.length() - 2), boost::is_any_of(LOCAL_DATA_NEWLINE_DELIMITER));
		boost::split(vTargetData, vDataParts[1].substr(0, sData.length() - 2), boost::is_any_of(LOCAL_DATA_DELIMITER));
	}
	catch (const std::exception&)
	{
		//This error case has to be handled in future!
		return vTargetData;
	}

	return vTargetData;
}

std::pair<std::vector<std::string>, std::vector<std::string>> Data_Manager::ConvertStringToPairOfStringLists(std::string sData)
{
	std::vector<std::string> vDataParts;
	std::vector<std::string> vFileNames;
	std::vector<std::string> vMd5Hashes;
	std::pair<std::list<std::string>, std::list<std::string>> lTargetData;

	try
	{
		boost::split(vDataParts, sData.substr(0, sData.length() - 2), boost::is_any_of(LOCAL_DATA_NEWLINE_DELIMITER));
		boost::split(vFileNames, vDataParts[1].substr(0, sData.length() - 2), boost::is_any_of(LOCAL_DATA_DELIMITER));

		int iMd5HashlistCounter = 0;
		for (unsigned int i = 1; i <= vFileNames.size(); i++)
		{
			vMd5Hashes.push_back(vFileNames[i]);
			vFileNames.erase(std::remove(vFileNames.begin(), vFileNames.end(), vFileNames[i]), vFileNames.end());
			iMd5HashlistCounter++;
		}
	}
	catch (const std::exception&)
	{
		return std::pair<std::vector<std::string>, std::vector<std::string>>();
	}

	return std::make_pair(vFileNames, vMd5Hashes);
}

std::vector<std::wstring> Data_Manager::ConvertStringToWStringList(std::string sData)
{
	std::wstring wsConvertedString;
	StringToWString(sData, &wsConvertedString);
	std::vector<std::wstring> vDataParts;
	std::vector<std::wstring> vTargetData;

	try
	{
		boost::split(vDataParts, wsConvertedString.substr(0, wsConvertedString.length() - 1), boost::is_any_of(LOCAL_DATA_NEWLINE_DELIMITER));
		boost::split(vTargetData, vDataParts[1].substr(0, wsConvertedString.length() - 1), boost::is_any_of(LOCAL_DATA_DELIMITER));
	}
	catch (const std::exception&)
	{
		return vTargetData;
	}

	return vTargetData;
}

void Data_Manager::StringToWString(std::string sStringToConvert, std::wstring * wsOutput)
{
	std::wstring ws(sStringToConvert.size(), L' '); // Overestimate number of code points.
	ws.resize(std::mbstowcs(&ws[0], sStringToConvert.c_str(), sStringToConvert.size())); // Shrink to fit.
	*wsOutput = ws;
}


//Public

//Note: Maybe optimize the function. Drag out the base model of downloading + decripting
int Data_Manager::CollectDynamicProtesData()
{
	//Download data from web
	std::stringstream ssUrlCombiner;
	std::stringstream ssLocalPathCombiner;
	std::string sLocalFolderPath = GetProgramFolderPath();

	//Get FP decrypted string

	

	ssUrlCombiner << TARGET_ENVIORMENT_DATA_URL << TARGET_ENVIORMENT_FTC_FILE_NAME;
	ssLocalPathCombiner << sLocalFolderPath << LOCAL_DATA_FOLDER << TARGET_ENVIORMENT_FTC_FILE_NAME;
	
	if (!Data_Gathering::DownloadWebFile(_strdup(ssUrlCombiner.str().c_str()), _strdup(ssLocalPathCombiner.str().c_str())))
	{
		return 1;
	}

	std::string sTempDecryptedFilesToCheckData = ConvertENCToDecryptedString(ssLocalPathCombiner.str());
		
	ssUrlCombiner.str("");
	ssLocalPathCombiner.str("");

	//Get heuristic md5 decrypted string

	ssUrlCombiner << TARGET_ENVIORMENT_DATA_URL << TARGET_ENVIORMENT_HEURISTIC_MD5_FILENAME;
	ssLocalPathCombiner << sLocalFolderPath << LOCAL_DATA_FOLDER << TARGET_ENVIORMENT_HEURISTIC_MD5_FILENAME;

	if (!Data_Gathering::DownloadWebFile(_strdup(ssUrlCombiner.str().c_str()), _strdup(ssLocalPathCombiner.str().c_str())))
	{
		return 1;
	}

	std::string sTempDecryptedMD5Data = ConvertENCToDecryptedString(ssLocalPathCombiner.str());

	ssUrlCombiner.str("");
	ssLocalPathCombiner.str("");

	//Get heuristic process name

	ssUrlCombiner << TARGET_ENVIORMENT_DATA_URL << TARGET_ENVIORMENT_HEURISTIC_PROCESSNAME_FILENAME;
	ssLocalPathCombiner << sLocalFolderPath << LOCAL_DATA_FOLDER << TARGET_ENVIORMENT_HEURISTIC_PROCESSNAME_FILENAME;

	if (!Data_Gathering::DownloadWebFile(_strdup(ssUrlCombiner.str().c_str()), _strdup(ssLocalPathCombiner.str().c_str())))
	{
		return 1;
	}

	std::string sTempDecryptedProcessNameData = ConvertENCToDecryptedString(ssLocalPathCombiner.str());
	
	if (sTempDecryptedFilesToCheckData == "" || sTempDecryptedMD5Data == "" || sTempDecryptedProcessNameData == "")
	{
		return 3;
	}

	vHeuristicMD5Values = ConvertStringToStringList(sTempDecryptedMD5Data);
	vHeuristicProcessNames = ConvertStringToWStringList(sTempDecryptedProcessNameData);
	pFilesToCheck = ConvertStringToPairOfStringLists(sTempDecryptedFilesToCheckData);

	return 0;
}

std::string Data_Manager::GenerateComputerID()
{
	std::stringstream ssComputerID;
	ssComputerID << Data_Gathering::GetCpuHash() << "-" << Data_Gathering::GetVolumeHash();
	LOCAL_HARDWARE_SID = ssComputerID.str();
	return LOCAL_HARDWARE_SID;
}

std::string Data_Manager::GetSoftwareArchitecture()
{
	if (Data_Gathering::Is64BitOS())
	{
		return "x64";
	}
	else
	{
		return "x86";
	}
}

std::string Data_Manager::GetSoftwareLanguage()
{
	std::wstring wsLanguageCode = Data_Gathering::GetLanguage();
	std::string sLanguageCode = "";
	using convert_type = std::codecvt_utf8<wchar_t>;
	std::wstring_convert<convert_type, wchar_t> converter;

	//use converter (.to_bytes: wstr->str, .from_bytes: str->wstr)
	sLanguageCode = converter.to_bytes(wsLanguageCode);

	return sLanguageCode;
}

std::string Data_Manager::GetProgramFolderPath()
{
	std::string sFilePath = Data_Gathering::GetApplicationFilePath();
	std::string sTmpLocalDataProtectionTarget = LOCAL_DATA_PROTECTION_TARGET;
	boost::to_upper(sTmpLocalDataProtectionTarget);
	boost::to_upper(sFilePath);
	std::vector<std::string> vFolderPath;
	boost::algorithm::split_regex(vFolderPath, sFilePath, boost::regex(sTmpLocalDataProtectionTarget));
	return vFolderPath[0];
}


//Getter 

int Data_Manager::GetSoftwareVersion()
{
	return SOFTWARE_VERSION;
}

std::string Data_Manager::GetTargetEnviormentDataUrl()
{
	return TARGET_ENVIORMENT_DATA_URL;
}

const char * Data_Manager::GetNetworkServerIP()
{
	return NETWORK_SERVER_IP;
}

int Data_Manager::GetNetworkServerPort()
{
	return NETWORK_SERVER_PORT;
}

int Data_Manager::GetNetworkMaxSendRetries()
{
	return NETWORK_MAX_SEND_RETRIES;
}

std::string Data_Manager::GetProtocolDelimiter()
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

int Data_Manager::GetProtectionMaxFpDll()
{
	return PROTECTION_FP_MAX_DLL;
}

std::string Data_Manager::GetLocalHardwareSID()
{
	return LOCAL_HARDWARE_SID;
}


const char * Data_Manager::GetLocalDataFolder()
{
	return LOCAL_DATA_FOLDER;
}

const char * Data_Manager::GetLocalProtegaImage()
{
	return LOCAL_DATA_PROTEGA_IMAGE;
}

std::vector<std::wstring> Data_Manager::GetHeuristicProcessNames()
{
	return vHeuristicProcessNames;
}

std::vector<std::string> Data_Manager::GetHeuristicMD5Values()
{
	return vHeuristicMD5Values;
}

std::pair<std::vector<std::string>, std::vector<std::string>> Data_Manager::GetFilesToCheckValues()
{
	return pFilesToCheck;
}

const char * Data_Manager::GetExceptionErrorFileName()
{
	return EXCEPTION_ERROR_FILE_NAME;
}

const char * Data_Manager::GetExceptionCrashReporterName()
{
	return EXCEPTION_CRASH_REPORTER_NAME;
}

int Data_Manager::GetExceptionLocalFileErrorNumber()
{
	return EXCEPTION_LOCAL_FILE_ERROR;
}

int Data_Manager::GetExceptionWebDownloadErrorNumber()
{
	return EXCEPTION_WEB_DOWNLOAD_ERROR;
}

int Data_Manager::GetExceptionDataConversionErrorNumber()
{
	return EXCEPTION_DATA_CONVERSION_ERROR;
}

int Data_Manager::GetExceptionVmErrorNumber()
{
	return EXCEPTION_VM_ERROR;
}

int Data_Manager::GetExceptionFpErrorNumber()
{
	return EXCEPTION_FP_ERROR;
}

int Data_Manager::GetExceptionThreadErrorNumber()
{
	return EXCEPTION_THREAD_ERROR;
}

int Data_Manager::GetExceptionNetworkErrorNumber()
{
	return EXCEPTION_NETWORK_ERROR;
}

//Setter

void Data_Manager::SetLocalHardwareSID(std::string sSID)
{
	LOCAL_HARDWARE_SID = sSID;
}
