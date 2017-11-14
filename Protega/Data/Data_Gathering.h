#pragma once
#include <iostream>
#include <curl\curl.h>
#include <boost\algorithm/string.hpp>

class Data_Gathering
{
private:
	Data_Gathering() {}
public:
	//Http
	static bool DownloadWebFile(char* sTarget, char sDestination[FILENAME_MAX]);
	static std::string GetWebFileAsString(const char* sTargetURL);

	//Hardware
};

