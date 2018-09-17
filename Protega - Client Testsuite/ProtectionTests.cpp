#pragma once
#include "stdafx.h"
#include "CppUnitTest.h"
#include "../Protega/Core/ProtegaCore.h"
#include <TlHelp32.h>

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace ProtegaClientTestsuite
{
	TEST_CLASS(ProtectionTests)
	{
	private:
		//Global
		std::wstring sProcessName = L"CabalMain.exe";
		bool bDetect = false;
		
		//HEP
		std::vector<std::wstring> vBlackListProcessNames;
		std::vector<std::string> vBlackListWindowNames;
		std::vector<std::string> vBlackListClassNames;
		std::vector<std::string> vBlackListMd5Values;


		DWORD FindProcessId(const std::wstring& processName)
		{
			PROCESSENTRY32 processInfo;
			processInfo.dwSize = sizeof(processInfo);

			HANDLE processesSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, NULL);
			if (processesSnapshot == INVALID_HANDLE_VALUE)
				return 0;

			Process32First(processesSnapshot, &processInfo);
			if (!processName.compare(processInfo.szExeFile))
			{
				CloseHandle(processesSnapshot);
				return processInfo.th32ProcessID;
			}

			while (Process32Next(processesSnapshot, &processInfo))
			{
				if (!processName.compare(processInfo.szExeFile))
				{
					CloseHandle(processesSnapshot);
					return processInfo.th32ProcessID;
				}
			}

			CloseHandle(processesSnapshot);
			return 0;
		}

	public:
		//This test emulates the usage of Virtual_Memory_Protection_Engine class.

		TEST_METHOD(Protection_VMP_Algorithm_Template)
		{
			//Get process id
			unsigned int processId = (int)FindProcessId(sProcessName);
			//init class
			Virtual_Memory_Protection_Cabal_Online *VMP_S = new Virtual_Memory_Protection_Cabal_Online(processId,
				std::bind(&ProtectionTests::VMP_S_CallBack, this, std::placeholders::_1, std::placeholders::_2, std::placeholders::_3, std::placeholders::_4));

			VMP_S->OpenProcessInstance();

			do
			{
				VMP_S->VMP_CheckZoomState();
				Sleep(10);
			} while (!bDetect);
		}	

		TEST_METHOD(Service_Algorithm_Template)
		{
			//Get process id
			unsigned int processId = (int)FindProcessId(sProcessName);
			//init class
			Service_Manager* ServiceManager = new Service_Manager(std::bind(&ProtectionTests::Service_Callback, this, std::placeholders::_1, std::placeholders::_2), processId);
			ServiceManager->JobCallbackCurrentCabalAccount();
		}

		void Service_Callback(unsigned int iType, std::string sDetection)
		{
			MessageBoxA(NULL, sDetection.c_str(),"",0);
		}

		void VMP_S_CallBack(std::string sDetectedBaseAddress, std::string sDetectedOffset, std::string sDetectedValue, std::string sStandartValue)
		{
			bDetect = true;
			MessageBoxA(NULL, "DETECT!", "Protega antihack engine", NULL);
			Assert::AreEqual((float)atof(sDetectedValue.c_str()), 600.0f);
		}
		
		TEST_METHOD(Protection_HEM_Test)
		{
			vBlackListProcessNames.push_back(L"Notepad.exe");
			vBlackListWindowNames.push_back("ddjbneidb");
			vBlackListMd5Values.push_back("AF79F5A331C50CC87F0A5F921AD93B0F");
			vBlackListClassNames.push_back("miaudfhh");

			Heuristic_Scan_Engine* HEM = new Heuristic_Scan_Engine(vBlackListProcessNames,
				vBlackListWindowNames,
				vBlackListClassNames,
				vBlackListMd5Values,
				std::bind(&ProtectionTests::Heuristic_Callback, this, std::placeholders::_1));
			do
			{
				HEM->DetectBlacklistedProcessMd5Hash();
			} while (true);
			
		}


		void Heuristic_Callback(std::string sDetection)
		{

		}


		//This tests emulates the usage of File_Protection_Engine
		TEST_METHOD(Protection_FP_LocalFileChange)
		{
			unsigned int processId = (int)FindProcessId(sProcessName);
			vBlackListWindowNames.push_back("C:\\Users\\lpickeli\\Documents\\CABAL CODEZERO\\Data\\Map\\world_01.mcl");
			vBlackListWindowNames.push_back("C:\\Users\\lpickeli\\Documents\\CABAL CODEZERO\\Data\\Map\\world_02.mcl");
			vBlackListMd5Values.push_back("250F0794AA6723271C53C28AD8E5B821");
			vBlackListMd5Values.push_back("8A32A8F17FF1A44204286B6C7515340C");

			File_Protection_Engine* FP = new File_Protection_Engine(processId, "", std::bind(&ProtectionTests::FP_Callback, this, std::placeholders::_1, std::placeholders::_2),
				std::make_pair(vBlackListWindowNames, vBlackListMd5Values), 100);
			
			do
			{
				FP->DetectLocalFileChange();
			} while (true);
			
		}

		TEST_METHOD(Protection_FP_DLL_Injection)
		{
			unsigned int processId = 8008;
			vBlackListWindowNames.push_back("D:\\Games\\CABAL CODEZERO NEW\\Data\\Map\\world_01.mcl");
			vBlackListMd5Values.push_back("B5865AAAFB570F68DCA4C6326587E939");

			File_Protection_Engine* FP = new File_Protection_Engine(processId, "", std::bind(&ProtectionTests::FP_Callback, this, std::placeholders::_1, std::placeholders::_2),
				std::make_pair(vBlackListWindowNames, vBlackListMd5Values), 100);

			FP->DetectInjection();
		}

		void FP_Callback(std::string sSection, std::string sContent)
		{

		}

	};
}