#pragma once
#include <utility>
#include <vector>
#include "../../Tools/CryptoPP_Converter.h"

class File_Protection_Engine
{
private:
	std::pair<std::vector<std::string>, std::vector<std::string>> pFileAndMd5;
	int iTargetApplicationId = 0;

	//functions
	std::function<void(std::string sFile, std::string sMd5, bool bInjection) > funcDetectCallbackHandler;

public:
	File_Protection_Engine(int iTargetApplicationId,
		std::function<void(std::string sFile, std::string sMd5, bool bInjection) > funcDetectCallbackHandler,
		std::pair<std::vector<std::string>, std::vector<std::string>> pFileAndMd5);

	int DetectLocalFileChange();
	bool DetectInjection();
	static bool DetectInjection(int iTargetApplicationId);
	~File_Protection_Engine();
};

