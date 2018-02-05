#pragma once

//#include <sstream>

#include <utility>
#include <vector>
#include <psapi.h>
#include "../../Tools/CryptoPP_Converter.h"

class File_Protection_Engine
{
private:
	std::pair<std::vector<std::string>, std::vector<std::string>> pFileAndMd5;
	int iTargetApplicationId = 0;
	int iMaxPossibleDlls = 0;
	//functions
	std::function<void(std::string sSection, std::string sDetectionValue) > funcDetectCallbackHandler;

public:
	File_Protection_Engine(int iTargetApplicationId,
		std::function<void(std::string sSection, std::string sDetectionValue)> funcDetectCallbackHandler,
		std::pair<std::vector<std::string>, std::vector<std::string>> pFileAndMd5, int iMaxPossibleDlls);

	int DetectLocalFileChange();
	int DetectInjection();
	static bool DetectInjection(int iTargetApplicationId);
	~File_Protection_Engine();
};

