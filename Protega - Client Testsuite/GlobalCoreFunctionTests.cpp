#pragma once
#include "stdafx.h"
#include <Windows.h>
#include "CppUnitTest.h"
#include "../Protega/Tools/SplashDisplayer.h"
#include "../Protega/Tools/CryptoPP_AES_Converter.h"
#include "../Protega/Core/Data_Manager.h"

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

	public:
	    //Triggers the main entry point of protega.dll -> maybe also get return
		TEST_METHOD(Core_CompleteStartUpProtega)
		{		
			HINSTANCE hInstLibrary = LoadLibrary(L"Protega.dll");
			MainCallFunction PEntryMain;
			getStopTrigger stopTrigger;

			PEntryMain = (MainCallFunction)GetProcAddress(hInstLibrary, "ProcMainEntry");
			//stopTrigger = (getStopTrigger)GetProcAddress(hInstLibrary, "stopTrigger");

			PEntryMain();
			//stopTrigger();
			Sleep(10000);
			
		}

		//tests the 
		TEST_METHOD(Core_CompleteStartUpShell)
		{
			HINSTANCE hInstLibrary = LoadLibrary(L"ShellX64.dll");
			MainCallFunctionShell MainEntry;

			MainEntry = (MainCallFunctionShell)GetProcAddress(hInstLibrary, "Test");
			//stopTrigger = (getStopTrigger)GetProcAddress(hInstLibrary, "stopTrigger");

			MainEntry();
			//stopTrigger();
			Sleep(10000);

		}

		TEST_METHOD(Core_WebDataGathering)
		{			
			Assert::AreEqual(Data_Manager::GetWebFileAsString("http://62.138.6.50:13011/index.html"), sHttpRequest);
		}
		TEST_METHOD(Core_DynamicDataCollecting)
		{
			Data_Manager::CollectDynamicProtesData();
		}

		TEST_METHOD(Task_ConvertFileToEnc)
		{
			std::ifstream infile;
			std::string sTmp;
			std::string input;
			std::string Dateidecryption;
			infile.open("VMP_Addresses.csv");
			while (getline(infile, sTmp))
			{
				input.append(sTmp.c_str());
			}
			infile.close();
			sTmp.clear();

			std::string test = CryptoPP_AES_Converter::Encrypt("1234567890123456", "bbbbbbbbbbbbbbbb", input);
			//Safe encode to file
			std::ofstream file("VMP_Addresses.csv.enc");
			file << test;
			file.close();
			input.clear();

			//Normal call
			std::string DirekteDecryption = CryptoPP_AES_Converter::Decrypt("1234567890123456", "bbbbbbbbbbbbbbbb", test);
			//Read in call
			infile.open("VMP_Addresses.csv.enc");
			while (getline(infile, sTmp))
			{
				Dateidecryption.append(sTmp.c_str());
			}
			infile.close();		
			std::string test2 = CryptoPP_AES_Converter::Decrypt("1234567890123456", "bbbbbbbbbbbbbbbb", Dateidecryption);
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
			std::string sEncryptedData = CryptoPP_AES_Converter::Encrypt("0123456789abcdef", "aaaaaaaaaaaaaaaa", sMessage);
			std::string sDecryptedData = CryptoPP_AES_Converter::Decrypt("0123456789abcdef", "aaaaaaaaaaaaaaaa", sEncryptedData);

			Assert::AreEqual(sMessage.c_str(), sDecryptedData.c_str());
		}



	};
}