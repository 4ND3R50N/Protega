#pragma once
#include <iostream>
#include "D:\Programme\Visual Studio Libs\cryptopp565\osrng.h"
#include "D:\Programme\Visual Studio Libs\cryptopp565\modes.h"
class CryptoPP_AES_Converter
{
private:
	CryptoPP_AES_Converter() {}
public:
	static std::string Encrypt(const char* sKey, const char* sIV, std::string sData);
	static std::string Decrypt(const char* sKey, const char* sIV, std::string sData);
};

