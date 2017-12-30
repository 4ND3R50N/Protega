#include "../stdafx.h"
#include "CryptoPP_Converter.h"


std::string CryptoPP_Converter::AESEncrypt(const char * sKey, const char * sIV, std::string sData)
{
	std::string ciphertext;

	// Create Cipher Text
	CryptoPP::AES::Encryption aesEncryption((byte *)sKey, CryptoPP::AES::DEFAULT_KEYLENGTH);
	CryptoPP::CBC_Mode_ExternalCipher::Encryption cbcEncryption(aesEncryption, (byte *)sIV);

	CryptoPP::StreamTransformationFilter stfEncryptor(cbcEncryption, new CryptoPP::StringSink(ciphertext));
	stfEncryptor.Put(reinterpret_cast<const unsigned char*>(sData.c_str()), sData.length() + 1);
	stfEncryptor.MessageEnd();

	return ciphertext;
}

std::string CryptoPP_Converter::AESDecrypt(const char * sKey, const char * sIV, std::string sData)
{
	std::string sResult = "";
	CryptoPP::AES::Decryption aesDecryption((byte *)sKey, CryptoPP::AES::DEFAULT_KEYLENGTH);
	CryptoPP::CBC_Mode_ExternalCipher::Decryption cbcDecryption(aesDecryption, (byte *)sIV);

	CryptoPP::StreamTransformationFilter stfDecryptor(cbcDecryption, new CryptoPP::StringSink(sResult));
	stfDecryptor.Put(reinterpret_cast<const unsigned char*>(sData.c_str()), sData.size());
	stfDecryptor.MessageEnd();

	return sResult;
}

std::string CryptoPP_Converter::GetMD5ofFile(const char * sFilePath)
{
	std::string sMD5Value;
	CryptoPP::Weak1::MD5 hash;
	CryptoPP::FileSource(sFilePath, true, new CryptoPP::HashFilter(hash, new CryptoPP::HexEncoder(new CryptoPP::StringSink(sMD5Value))));

	return sMD5Value;
}
