#include "stdafx.h"
#include "CppUnitTest.h"
#include <stdio.h>
#include <windows.h>
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