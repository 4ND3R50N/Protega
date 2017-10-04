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
	};
}