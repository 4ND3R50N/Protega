#pragma once
#include "stdafx.h"
#include "CppUnitTest.h"
#include <Windows.h>
#include <tlhelp32.h>
#include <list>
#include <comdef.h>
#include "../Protega/Protection/Target Enviorment/Virtual_Memory_Protection_Engine.h"
#include "../Protega/Protection/Target Enviorment/Heuristic_Scan_Engine.h"


using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace ProtegaClientTestsuite
{
	TEST_CLASS(ProtectionTests)
	{
	private:
		//Global
		char* sProcessName = "CabalMain22.exe";
		bool bDetect = false;
		//VMP_S
		typedef std::pair<unsigned int, unsigned int> ADDRESSPAIR;
		typedef std::pair<const char*, unsigned int> INFORMATIONPAIR;
		
		//HEP



		//Temporary here...
		int GetProcessId(char* ProcName) {
			PROCESSENTRY32 pe32;
			HANDLE hSnapshot = NULL;
			pe32.dwSize = sizeof(PROCESSENTRY32);
			hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);

			if (Process32First(hSnapshot, &pe32)) {
				do {
					_bstr_t sPe32ExeFile(pe32.szExeFile);
					if (strcmp(sPe32ExeFile, ProcName) == 0)
						break;
				} while (Process32Next(hSnapshot, &pe32));
			}

			if (hSnapshot != INVALID_HANDLE_VALUE)
				CloseHandle(hSnapshot);

			return pe32.th32ProcessID;
		}

	public:

		//This test emulates the usage of Virtual_Memory_Protection_Engine class.
		//The speed value of cabal is the value that gets checked here
		TEST_METHOD(Protection_VMP_Test)
		{
			//Declare lists with address entries
			bDetect = false;
			std::list<std::pair<unsigned int, unsigned int>> kvpAddressList;
			std::list<std::pair<const char*, unsigned int>> kvpValueInformation;
			kvpAddressList.push_back(ADDRESSPAIR(0x00B93530, 0x00000204));		
			kvpValueInformation.push_back(INFORMATIONPAIR("450.0f", 2));
			//Get process id
			unsigned int processId = GetProcessId(sProcessName);
			//init class
			Virtual_Memory_Protection_Engine *VMP_S = new Virtual_Memory_Protection_Engine(processId, kvpAddressList, kvpValueInformation,
				std::bind(&ProtectionTests::VMP_S_CallBack, this, std::placeholders::_1, std::placeholders::_2, std::placeholders::_3));

			VMP_S->OpenProcessInstance();
			
			//Loop "scan all addresses" function
			do
			{
				VMP_S->ScanAllAddresses();
				Sleep(1000);
			} while (!bDetect);
		}

		void VMP_S_CallBack(std::list<std::pair<unsigned int, unsigned int>>::iterator kvpDetectedAddress,
			const char* sActualValue, const char* sWantedCondition)
		{
			bDetect = true;
			MessageBoxA(NULL, "Detect!", "Protega antihack engine", NULL);
			Assert::AreEqual((float)atof(sActualValue), 600.0f);
		}


		//This tests emulates the usage of Heuristic_Scan_Engine class
		TEST_METHOD(Protection_HEP_Test)
		{
			std::list<std::wstring> lBlackListProcessNames;
			std::list<std::string> lBlackListWindowNames;
			std::list<std::string> lBlackListClassNames;
			std::list<std::string> lBlackListMd5Values;

			lBlackListProcessNames.push_back(L"Notepad.exe");
			lBlackListWindowNames.push_back("ddjbneidb");
			lBlackListMd5Values.push_back("12343333");
			lBlackListClassNames.push_back("miaudfhh");

			Heuristic_Scan_Engine* HEP = new Heuristic_Scan_Engine(lBlackListProcessNames,
				lBlackListWindowNames, 
				lBlackListClassNames, 
				lBlackListMd5Values,
				std::bind(&ProtectionTests::Heuristic_Callback, this, std::placeholders::_1, std::placeholders::_2));
			
			HEP->DoScanProcessNames();
		}
		void Heuristic_Callback(std::wstring sDetection, std::list<std::string> lOtherInformation)
		{

		}

	};
}