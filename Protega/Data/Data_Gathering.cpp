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

