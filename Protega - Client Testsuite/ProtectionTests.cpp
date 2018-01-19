#pragma once
#include "stdafx.h"
#include "CppUnitTest.h"
#include "../Protega/Core/ProtegaCore.h"


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
		std::vector<std::wstring> vBlackListProcessNames;
		std::vector<std::string> vBlackListWindowNames;
		std::vector<std::string> vBlackListClassNames;
		std::vector<std::string> vBlackListMd5Values;

	public:
		//Protection manager
		TEST_METHOD(Protection_Threads_Test)
		{

			vBlackListProcessNames.push_back(L"Notepad.exe");
			vBlackListWindowNames.push_back("ddjbneidb");
			vBlackListMd5Values.push_back("1c32647a706fbef6faeac45a75201489");
			vBlackListClassNames.push_back("miaudfhh");

			Protection_Manager* PM = new Protection_Manager(std::bind(&ProtectionTests::PM_Callback, this, std::placeholders::_1),
				9999, 20, 1, 2, 2,
				vBlackListProcessNames, vBlackListWindowNames, vBlackListClassNames, vBlackListMd5Values,
				std::pair<std::vector<std::string>,std::vector<std::string>>());
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
			vBlackListProcessNames.push_back(L"Notepad.exe");
			vBlackListWindowNames.push_back("ddjbneidb");
			vBlackListMd5Values.push_back("12343333");
			vBlackListClassNames.push_back("miaudfhh");

			Heuristic_Scan_Engine* HEP = new Heuristic_Scan_Engine(vBlackListProcessNames,
				vBlackListWindowNames, 
				vBlackListClassNames, 
				vBlackListMd5Values,
				std::bind(&ProtectionTests::Heuristic_Callback, this, std::placeholders::_1));
			
			HEP->DetectBlacklistedProcessNames();
		}
		
		TEST_METHOD(Protection_HEM_Test)
		{
			vBlackListProcessNames.push_back(L"Notepad.exe");
			vBlackListWindowNames.push_back("ddjbneidb");
			vBlackListMd5Values.push_back("1c32647a706fbef6faeac45a75201489");
			vBlackListClassNames.push_back("miaudfhh");

			Heuristic_Scan_Engine* HEM = new Heuristic_Scan_Engine(vBlackListProcessNames,
				vBlackListWindowNames,
				vBlackListClassNames,
				vBlackListMd5Values,
				std::bind(&ProtectionTests::Heuristic_Callback, this, std::placeholders::_1));

			HEM->DetectBlacklistedProcessMd5Hash();
		}


		void Heuristic_Callback(std::wstring sDetection)
		{

		}


		//This tests emulates the usage of File_Protection_Engine
		TEST_METHOD(Protection_FP_Test)
		{
			unsigned int processId = GetProcessId(sProcessName);
			vBlackListWindowNames.push_back("D:\\Games\\CABAL CODEZERO NEW\\Data\\Map\\world_01.mcl");
			vBlackListMd5Values.push_back("B5865AAAFB570F68DCA4C6326587E939");

			File_Protection_Engine* FP = new File_Protection_Engine(processId, std::bind(&ProtectionTests::FP_Callback, this, std::placeholders::_1, std::placeholders::_2, std::placeholders::_3),
				std::make_pair(vBlackListWindowNames, vBlackListMd5Values));
			
			FP->DetectLocalFileChange();

		}
		void FP_Callback(std::string sFile, std::string sMd5, bool bInjection)
		{

		}

	};
}