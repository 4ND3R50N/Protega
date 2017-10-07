#pragma once
#include "stdafx.h"
#include <Windows.h>
#include "CppUnitTest.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace ProtegaClientTestsuite
{		

	TEST_CLASS(GlobalCoreFunctionTests)
	{
	private:
		typedef int(*MainCallFunction)();
		typedef int(*getStopTrigger)();

	public:
		

#pragma region DLL INJECTION
		HMODULE TryInjectDll(DWORD adw_ProcessId, const std::wstring& as_DllFile)
		{
			//Find the address of the LoadLibrary api, luckily for us, it is loaded in the same address for every process 
			HMODULE hLocKernel32 = GetModuleHandleW(L"KERNEL32");
			FARPROC hLocLoadLibrary = GetProcAddress(hLocKernel32, "LoadLibraryW");

			//Adjust token privileges to open system processes 
			HANDLE hToken;
			TOKEN_PRIVILEGES tkp;
			if (OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hToken))
			{
				LookupPrivilegeValue(NULL, SE_DEBUG_NAME, &tkp.Privileges[0].Luid);
				tkp.PrivilegeCount = 1;
				tkp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
				AdjustTokenPrivileges(hToken, 0, &tkp, sizeof(tkp), NULL, NULL);
				CloseHandle(hToken);
			}

			//Open the process with all access 
			HANDLE hProc = OpenProcess(PROCESS_ALL_ACCESS, FALSE, adw_ProcessId);
			if (hProc == NULL)
				return NULL;

			//Allocate memory to hold the path to the Dll File in the process's memory 
			LPVOID hRemoteMem = VirtualAllocEx(hProc, NULL, as_DllFile.size() * sizeof(wchar_t), MEM_COMMIT, PAGE_READWRITE);

			//Write the path to the Dll File in the location just created 
			DWORD numBytesWritten;
			WriteProcessMemory(hProc, hRemoteMem, as_DllFile.c_str(), as_DllFile.size() * sizeof(wchar_t), &numBytesWritten);

			//Create a remote thread that starts begins at the LoadLibrary function and is passed are memory pointer 
			HANDLE hRemoteThread = CreateRemoteThread(hProc, NULL, 0, (LPTHREAD_START_ROUTINE)hLocLoadLibrary, hRemoteMem, 0, NULL);

			//Wait for the thread to finish 
			::WaitForSingleObject(hRemoteThread, INFINITE);
			DWORD  hLibModule = 0;
			::GetExitCodeThread(hRemoteThread, &hLibModule);

			//Free the memory created on the other process 
			::VirtualFreeEx(hProc, hRemoteMem, as_DllFile.size() * sizeof(wchar_t), MEM_RELEASE);

			//Release the handle to the other process 
			::CloseHandle(hProc);

			return (HMODULE)hLibModule;
		}



#pragma endregion

		//Triggers the main entry point of protega.dll -> maybe also get return
		TEST_METHOD(CompleteStartUp)
		{		
			HINSTANCE hInstLibrary = LoadLibrary(L"Protega.dll");
			MainCallFunction PEntryMain;
			getStopTrigger stopTrigger;

			PEntryMain = (MainCallFunction)GetProcAddress(hInstLibrary, "PEntryMain");
			stopTrigger = (getStopTrigger)GetProcAddress(hInstLibrary, "stopTrigger");

			PEntryMain();
			//stopTrigger();
			Sleep(10000);
			
		}

		TEST_METHOD(DLLINJECTING)
		{
			TryInjectDll(10748, L"D:\\GitHub SW-Projekte\\Protega\\Client - Debug\\ShellX64.dll");
		}
	};
}