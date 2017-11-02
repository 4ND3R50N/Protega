#pragma once
#include "stdafx.h"
#include <Windows.h>
#include "CppUnitTest.h"
#include "../Protega/Tools/SplashDisplayer.h"

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

		//tests the 
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

		TEST_METHOD(DisplayBitmap)
		{
			SplashDisplayer Splashtest(TEXT(".\\Protega_Logo.bmp"), RGB(128, 128, 128));
			Splashtest.ShowSplash();
			Sleep(5000);
			Splashtest.CloseSplash();
			Splashtest.~SplashDisplayer();
			Assert::IsTrue(true);
		}


	};
}