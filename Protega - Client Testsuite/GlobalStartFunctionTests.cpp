#pragma once
#include "stdafx.h"
#include <Windows.h>
#include "CppUnitTest.h"




using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace ProtegaClientTestsuite
{		

	TEST_CLASS(GlobalStartFunctionTests)
	{
	private:
		typedef int(*MainCallFunction)();

	public:
		
		//Triggers the main entry point of protega.dll
		TEST_METHOD(CompleteStartUp)
		{		
			HINSTANCE hInstLibrary = LoadLibrary(L"Protega.dll");
			MainCallFunction PEntryMain;


			PEntryMain = (MainCallFunction)GetProcAddress(hInstLibrary, "PEntryMain");

			Assert::AreEqual(1, PEntryMain());
		}
	};
}