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
		//Protega
		typedef int(*MainCallFunction)();
		typedef int(*getStopTrigger)();

		//Shell
		typedef void(*MainCallFunctionShell)();

	public:
	    //Triggers the main entry point of protega.dll -> maybe also get return
		TEST_METHOD(CompleteStartUpProtega)
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

		TEST_METHOD(CompleteStartUpShell)
		{
			HINSTANCE hInstLibrary = LoadLibrary(L"ShellX64.dll");
			MainCallFunctionShell MainEntry;

			MainEntry = (MainCallFunctionShell)GetProcAddress(hInstLibrary, "Test");
			//stopTrigger = (getStopTrigger)GetProcAddress(hInstLibrary, "stopTrigger");

			MainEntry();
			//stopTrigger();
			Sleep(10000);

		}

	};
}