#pragma once
#include "stdafx.h"
#include "CppUnitTest.h"
#include <Windows.h>
#include <tlhelp32.h>
#include <list>
#include <comdef.h>
#include "../Protega/Protection/Protection_Manager.h"



using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace ProtegaClientTestsuite
{
	TEST_CLASS(ProtectionTests)
	{
	private:
		//Global
		char* sProcessName = "CabalMain22.exe";
		bool bDetect = false;
		
		//HEP
		std::list<std::wstring> lBlackListProcessNames;
		std::list<std::string> lBlackListWindowNames;
		std::list<std::string> lBlackListClassNames;
		std::list<std::string> lBlackListMd5Values;

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
		//Protection manager
		TEST_METHOD(Protection_Threads_Test)
		{

			lBlackListProcessNames.push_back(L"Notepad.exe");
			lBlackListWindowNames.push_back("ddjbneidb");
			lBlackListMd5Values.push_back("1c32647a706fbef6faeac45a75201489");
			lBlackListClassNames.push_back("miaudfhh");

			Protection_Manager* PM = new Protection_Manager(std::bind(&ProtectionTests::PM_Callback, this, std::placeholders::_1),
				"CabalMain22.exe", 20,
				lBlackListProcessNames, lBlackListWindowNames, lBlackListClassNames, lBlackListMd5Values);
			PM->StartProtectionThreads();
			//Loop "scan all addresses" function
			do
			{
				Sleep(1000);
			} while (!bDetect);
		}

		void PM_Callback(std::list<std::wstring> lDetectionInformation)
		{
			MessageBoxA(0, "PM_Callback", "PM_Callback", MB_OK);
		}

		//This test emulates the usage of Virtual_Memory_Protection_Engine class.
		
		TEST_METHOD(Protection_VMP_SpeedHackPrevention_Test)
		{
			//Get process id
			unsigned int processId = GetProcessId(sProcessName);
			//init class
			Virtual_Memory_Protection_Cabal_Online *VMP_S = new Virtual_Memory_Protection_Cabal_Online(processId,
				std::bind(&ProtectionTests::VMP_S_CallBack, this, std::placeholders::_1, std::placeholders::_2, std::placeholders::_3, std::placeholders::_4));

			VMP_S->OpenProcessInstance();
			
			//Loop "scan all addresses" function
			do
			{
				VMP_S->VMP_CheckGameSpeed();
				Sleep(1000);
			} while (!bDetect);
		}

		TEST_METHOD(Protection_VMP_WallHackPrevention_Test)
		{
			//Get process id
			unsigned int processId = GetProcessId(sProcessName);
			//init class
			Virtual_Memory_Protection_Cabal_Online *VMP_S = new Virtual_Memory_Protection_Cabal_Online(processId,
				std::bind(&ProtectionTests::VMP_S_CallBack, this, std::placeholders::_1, std::placeholders::_2, std::placeholders::_3, std::placeholders::_4));

			VMP_S->OpenProcessInstance();
			VMP_S->VMP_CheckWallBorders();
		}	

		TEST_METHOD(Protection_VMP_ZoomHackPrevention_Test) 
		{
			//Get process id
			unsigned int processId = GetProcessId(sProcessName);
			//init class
			Virtual_Memory_Protection_Cabal_Online *VMP_S = new Virtual_Memory_Protection_Cabal_Online(processId,
				std::bind(&ProtectionTests::VMP_S_CallBack, this, std::placeholders::_1, std::placeholders::_2, std::placeholders::_3, std::placeholders::_4));

			VMP_S->OpenProcessInstance();

			//Loop "scan all addresses" function
			do
			{
				VMP_S->VMP_CheckZoomState();
				Sleep(1000);
			} while (!bDetect);
		}

		TEST_METHOD(Protection_VMP_NoSkillDelayAndNoCastTimePrevention_Test)
		{
			//Get process id
			unsigned int processId = GetProcessId(sProcessName);
			//init class
			Virtual_Memory_Protection_Cabal_Online *VMP_S = new Virtual_Memory_Protection_Cabal_Online(processId,
				std::bind(&ProtectionTests::VMP_S_CallBack, this, std::placeholders::_1, std::placeholders::_2, std::placeholders::_3, std::placeholders::_4));

			VMP_S->OpenProcessInstance();

			//Loop "scan all addresses" function
			do
			{
				VMP_S->VMP_CheckNoSkillDelay();
				VMP_S->VMP_CheckNoCastTime();
				Sleep(1000);
			} while (!bDetect);
		}	

		TEST_METHOD(Protection_VMP_RangeHackPrevention_Test)
		{
			//Get process id
			unsigned int processId = GetProcessId(sProcessName);
			//init class
			Virtual_Memory_Protection_Cabal_Online *VMP_S = new Virtual_Memory_Protection_Cabal_Online(processId,
				std::bind(&ProtectionTests::VMP_S_CallBack, this, std::placeholders::_1, std::placeholders::_2, std::placeholders::_3, std::placeholders::_4));

			VMP_S->OpenProcessInstance();

			//Loop "scan all addresses" function
			do
			{
				VMP_S -> VMP_CheckSkillRange();
				Sleep(1000);
			} while (!bDetect);
		}

		TEST_METHOD(Protection_VMP_NoSkillCooldownPrevention_Test)
		{
			//Get process id
			unsigned int processId = GetProcessId(sProcessName);
			//init class
			Virtual_Memory_Protection_Cabal_Online *VMP_S = new Virtual_Memory_Protection_Cabal_Online(processId,
				std::bind(&ProtectionTests::VMP_S_CallBack, this, std::placeholders::_1, std::placeholders::_2, std::placeholders::_3, std::placeholders::_4));

			VMP_S->OpenProcessInstance();

			//Loop "scan all addresses" function
			do
			{
				VMP_S->VMP_CheckSkillCooldown();
				Sleep(1000);
			} while (!bDetect);
		}

		void VMP_S_CallBack(std::string sDetectedBaseAddress, std::string sDetectedOffset, std::string sDetectedValue, std::string sStandartValue)
		{
			bDetect = true;
			MessageBoxA(NULL, "DETECT!", "Protega antihack engine", NULL);
			Assert::AreEqual((float)atof(sDetectedValue.c_str()), 600.0f);
		}


		//This tests emulates the usage of Heuristic_Scan_Engine class
		TEST_METHOD(Protection_HEP_Test)
		{
			lBlackListProcessNames.push_back(L"Notepad.exe");
			lBlackListWindowNames.push_back("ddjbneidb");
			lBlackListMd5Values.push_back("12343333");
			lBlackListClassNames.push_back("miaudfhh");

			Heuristic_Scan_Engine* HEP = new Heuristic_Scan_Engine(lBlackListProcessNames,
				lBlackListWindowNames, 
				lBlackListClassNames, 
				lBlackListMd5Values,
				std::bind(&ProtectionTests::Heuristic_Callback, this, std::placeholders::_1));
			
			HEP->DoScanProcessNames();
		}
		
		TEST_METHOD(Protection_HEM_Test)
		{
			lBlackListProcessNames.push_back(L"Notepad.exe");
			lBlackListWindowNames.push_back("ddjbneidb");
			lBlackListMd5Values.push_back("1c32647a706fbef6faeac45a75201489");
			lBlackListClassNames.push_back("miaudfhh");

			Heuristic_Scan_Engine* HEM = new Heuristic_Scan_Engine(lBlackListProcessNames,
				lBlackListWindowNames,
				lBlackListClassNames,
				lBlackListMd5Values,
				std::bind(&ProtectionTests::Heuristic_Callback, this, std::placeholders::_1));

			HEM->ScanProcessMd5Hash();
		}

		void Heuristic_Callback(std::wstring sDetection)
		{

		}

	};
}