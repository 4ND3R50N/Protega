#pragma once
#include <iostream>
#include <stdio.h>
#include <stdlib.h>
#include <curl\curl.h>

class Data_Manager
{
private:
	//Data
	static const char* TARGET_ENVIORMENT_DATA_URL;

public:
	//Construktor
	
	//Getter
	static const char* GetTargetEnviormentDataUrl();

	//Support functions
	static std::string GetWebFileAsString(const char* sTargetURL);
};

