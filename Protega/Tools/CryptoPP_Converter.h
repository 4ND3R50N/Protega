#pragma once
#include <iostream>
#include "osrng.h"
#include "modes.h"
#include "md5.h"
#include "files.h"
#include "hex.h"

class CryptoPP_Converter
{
private:
	CryptoPP_Converter() {}
public:
	static std::string AESEncrypt(const char* sKey, const char* sIV, std::string sData);
	static std::string AESDecrypt(const char* sKey, const char* sIV, std::string sData);
	static std::string GetMD5ofFile(const char* sFilePath);
};

