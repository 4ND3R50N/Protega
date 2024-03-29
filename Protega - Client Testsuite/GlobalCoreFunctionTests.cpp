#pragma once
#include "stdafx.h"
//#include <Windows.h>
#include "CppUnitTest.h"
#include "../Protega/Core/ProtegaCore.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace ProtegaClientTestsuite
{		

	TEST_CLASS(GlobalCoreFunctionTests)
	{
	private:
		//Protega
		typedef int(*MainCallFunction)();
		typedef int(*getStopTrigger)();

		//Shell
		typedef void(*MainCallFunctionShell)();

		//AES
		const std::string sMessage = "Katze";

		//Data Gathering
		const std::string sHttpRequest = "Test!";
		bool bCurrentOS64 = true;
		uint16_t u16VolumeHash = 23901;
		uint16_t u16CpuHash = 29548;
		std::string sComputerName = "DESKTOP-MTTQVRN";
		std::wstring wsOsLanguage = L"de-DE";
		std::string sComputerID = "29548-23901";

	public:
	    //Triggers the main entry point of protega.dll -> maybe also get return
		TEST_METHOD(Core_CompleteStartUpProtega)
		{		
			HINSTANCE hInstLibrary = LoadLibrary(L"Protega.dll");
			MainCallFunction PEntryMain;

			PEntryMain = (MainCallFunction)GetProcAddress(hInstLibrary, "ProcMainEntry");
			//stopTrigger = (getStopTrigger)GetProcAddress(hInstLibrary, "stopTrigger");

			PEntryMain();
			//stopTrigger();
			Sleep(10000);
			
		}

		TEST_METHOD(Core_AntihackRunTest)
		{
			ProtegaCore CoreTest;
			CoreTest.StartAntihack();
			
		}

		TEST_METHOD(Data_WebDataGathering)
		{			
			Assert::AreEqual(Data_Gathering::GetWebFileAsString("http://62.138.6.50:13011/index.html"), sHttpRequest);
		}

		TEST_METHOD(Data_HardwareDataGathering)
		{
			//Get OS
			WCHAR wcBuffer[16];
			if (GetSystemDefaultLocaleName(wcBuffer, 16)) {
				printf("%S\n", wcBuffer);
			}

			std::wstring wsOsLanguageC(wcBuffer);

			//Get rest + compare
			Assert::IsTrue((bCurrentOS64 == Data_Gathering::Is64BitOS() &&
				u16VolumeHash == Data_Gathering::GetVolumeHash() &&
				u16CpuHash == Data_Gathering::GetCpuHash() &&
				sComputerName == Data_Gathering::GetMachineName() &&
				wsOsLanguage == wsOsLanguageC));
		}

		TEST_METHOD(Data_GetComputerID)
		{
			Assert::AreEqual(sComputerID, Data_Manager::GenerateComputerID());
		}

		TEST_METHOD(Data_DynamicDataCollecting)
		{
			Data_Manager::CollectDynamicProtesData();
		}

		TEST_METHOD(Task_ConvertFileToEnc)
		{
			std::string sFileToConvert = ".\\..\\docs\\client\\Heuristic_Process_Names.csv";
			const char* sAESKey = "1234567890123456";
			const char* sIV = "bbbbbbbbbbbbbbbb";
			std::ifstream isReader;
			std::string sTmp;
			std::string sInput;

			isReader.open(sFileToConvert);
			while (getline(isReader, sTmp))
			{
				sInput.append(sTmp.c_str());
			}
			isReader.close();
			sTmp.clear();

			std::string sDecryptedValue = "";
			std::string sEncryptedString = CryptoPP_Converter::AESEncrypt(sAESKey, sIV, sInput);
			std::ofstream file(sFileToConvert.append(".enc"));

			for (int i = 0; i < sEncryptedString.size(); i++)
			{
				file << std::to_string(int(sEncryptedString.at(i))) << endl;
			}

			//Safe encode to file			
			file.close();
			isReader.clear();
		}
		
		TEST_METHOD(Task_ConvertEncToFile)
		{
			//Read enc file
			std::string sEncodedString = "";
			std::string sCurrentLine;
			std::string sPathToEnc = "C:\\Users\\lpickeli\\Documents\\GitHub\\Protega\\docs\\Files_To_Check.csv_TEST.enc";
			std::ifstream ifEncFile(sPathToEnc);

			while (getline(ifEncFile, sCurrentLine))  // same as: while (getline( myfile, line ).good())
			{
				sEncodedString += ((char)atoi(sCurrentLine.c_str()));
			}
			ifEncFile.close();

			MessageBoxA(NULL, CryptoPP_Converter::AESDecrypt(Data_Manager::GetNetworkAesKey(), Data_Manager::GetNetworkAesIV(), sEncodedString).c_str(),"Debug",0);
		}


		TEST_METHOD(Tools_DisplayBitmap)
		{
			SplashDisplayer Splashtest(TEXT(".\\Protega_Logo.bmp"), RGB(128, 128, 128));
			Splashtest.ShowSplash();
			Sleep(5000);
			Splashtest.CloseSplash();
			Splashtest.~SplashDisplayer();
			Assert::IsTrue(true);
		}

		TEST_METHOD(Tools_CheckAESEncryption)
		{
			std::string sEncryptedData = CryptoPP_Converter::AESEncrypt("0123456789abcdef", "aaaaaaaaaaaaaaaa", sMessage);
			std::string sDecryptedData = CryptoPP_Converter::AESDecrypt("0123456789abcdef", "aaaaaaaaaaaaaaaa", sEncryptedData);

			Assert::AreEqual(sMessage.c_str(), sDecryptedData.c_str());
		}

	

	};
}