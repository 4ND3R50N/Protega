#include "../stdafx.h"
#include "Data_Gathering.h"

//Callbacks for curl operations
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

//public static
bool Data_Gathering::DownloadWebFile(char* sTarget, char sDestination[FILENAME_MAX])
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

std::string Data_Gathering::GetWebFileAsString(const char* sTargetURL)
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

bool Data_Gathering::is64BitOS()
{	

#if _WIN64
	return true;
#elif _WIN32

	BOOL isWow64 = FALSE;

	//IsWow64Process is not available on all supported versions of Windows.
	//Use GetModuleHandle to get a handle to the DLL that contains the function
	//and GetProcAddress to get a pointer to the function if available.

	LPFN_ISWOW64PROCESS fnIsWow64Process = (LPFN_ISWOW64PROCESS)
		GetProcAddress(GetModuleHandle(TEXT("kernel32")), "IsWow64Process");

	if (fnIsWow64Process)
	{
		if (!fnIsWow64Process(GetCurrentProcess(), &isWow64))
			return false;

		if (isWow64)
			return true;
		else
			return false;
		return true;
	}
	else
		return false;
#else

	assert(0);
	return false;

#endif
}

uint16_t Data_Gathering::getVolumeHash()
{
	DWORD dwSerialNum = 0;

	// Determine if this volume uses an NTFS file system.      
	GetVolumeInformation(L"c:\\", NULL, 0, &dwSerialNum, NULL, NULL, NULL, 0);
	uint16_t hash = (uint32_t)((dwSerialNum + (dwSerialNum >> 16)) & 0xFFFF);

	return hash;
}

uint16_t Data_Gathering::getCpuHash()
{
	int iCpuInfo[4] = { 0, 0, 0, 0 };
	__cpuid(iCpuInfo, 0);
	uint16_t u16Hash = 0;
	uint16_t* u16Ptr = (uint16_t*)(&iCpuInfo[0]);
	for (uint32_t i = 0; i < 8; i++)
		u16Hash += u16Ptr[i];

	return u16Hash;
}

std::string Data_Gathering::getMachineName()
{
	char cAnsiBuffer[255];
	TCHAR  tcInfoBuffer[INFO_BUFFER_SIZE];
	DWORD  dwBufferCharCount;

	if (!GetComputerName(tcInfoBuffer, &dwBufferCharCount))
	{
		//error
		return "";
	}
	else
	{
		TCharToChar(tcInfoBuffer, cAnsiBuffer, sizeof(cAnsiBuffer));
		return std::string(cAnsiBuffer);
	}
}

//Private 

void Data_Gathering::TCharToChar(const wchar_t * Src, char * Dest, int Size)
{
	WideCharToMultiByte(CP_ACP, 0, Src, wcslen(Src) + 1, Dest, Size, NULL, NULL);
}


