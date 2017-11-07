#include "../stdafx.h"
#include "Data_Manager.h"


void WriteMemoryCallback(void *ptr, size_t size, size_t nmemb, void *stream) {
	const char* test = (const char*)ptr;
}


std::string Data_Manager::GetWebFileAsString()
{
	CURL *curl;
	CURLcode res;
	curl = curl_easy_init();
	if (curl) {
		curl_easy_setopt(curl, CURLOPT_URL, "http://62.138.6.50:13011/CabalOnline/Heuristic_Data.csv");
		curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteMemoryCallback);
		curl_easy_perform(curl);
		curl_easy_cleanup(curl);
	}

	return std::string();
}
