#include "../stdafx.h"
#include "CryptoPP_AES_Converter.h"


std::string CryptoPP_AES_Converter::Encrypt(const char * sKey, const char * sIV, std::string sData)
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

std::string CryptoPP_AES_Converter::Decrypt(const char * sKey, const char * sIV, std::string sData)
{
	std::string sResult = "";
	CryptoPP::AES::Decryption aesDecryption((byte *)sKey, CryptoPP::AES::DEFAULT_KEYLENGTH);
	CryptoPP::CBC_Mode_ExternalCipher::Decryption cbcDecryption(aesDecryption, (byte *)sIV);

	CryptoPP::StreamTransformationFilter stfDecryptor(cbcDecryption, new CryptoPP::StringSink(sResult));
	stfDecryptor.Put(reinterpret_cast<const unsigned char*>(sData.c_str()), sData.size());
	stfDecryptor.MessageEnd();

	return sResult;
}
