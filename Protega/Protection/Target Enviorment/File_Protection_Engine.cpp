#include "../../stdafx.h"
#include "File_Protection_Engine.h"

File_Protection_Engine::File_Protection_Engine(int iTargetApplicationId, 
	std::function<void(std::string sFile, std::string sMd5, bool bInjection) > funcDetectCallbackHandler,
	std::pair<std::vector<std::string>, std::vector<std::string>> pFileAndMd5)
{
	this->pFileAndMd5 = pFileAndMd5;
	this->iTargetApplicationId = iTargetApplicationId;
	this->funcDetectCallbackHandler = funcDetectCallbackHandler;
}

//Public
int File_Protection_Engine::DetectLocalFileChange()
{
	for (int i = 0; i < pFileAndMd5.first.size(); i++)
	{
		std::string sActualMd5 = CryptoPP_Converter::GetMD5ofFile(pFileAndMd5.first[i].c_str());

		if (sActualMd5 == "") {
			return 1;
		}

		if (sActualMd5 != pFileAndMd5.second[i])
		{
			funcDetectCallbackHandler(pFileAndMd5.first[i], pFileAndMd5.second[i], false);
			return 2;
		}
	}
	return 0;
}

bool File_Protection_Engine::DetectInjection()
{
	return false;
}

bool File_Protection_Engine::DetectInjection(int iTargetApplicationId)
{
	return false;
}

File_Protection_Engine::~File_Protection_Engine()
{
}
