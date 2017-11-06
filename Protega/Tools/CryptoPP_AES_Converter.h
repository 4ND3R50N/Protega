#pragma once
#include <iostream>
#include "osrng.h"
#include "modes.h"
class CryptoPP_AES_Converter
{
private:
	CryptoPP_AES_Converter() {}
public:
	static std::string Encrypt(const char* sKey, const char* sIV, std::string sData);
	static std::string Decrypt(const char* sKey, const char* sIV, std::string sData);
};

