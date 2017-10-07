// dllmain.cpp : Definiert den Einstiegspunkt für die DLL-Anwendung.
#include "stdafx.h"
#include "dllmain.h"
#include <windows.h>
#include <stdio.h>
#include <stdlib.h>

void Test()
{
	MessageBoxA(0, "Hello from injected DLL!\n", "Hi", MB_ICONINFORMATION);
}


BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		Test();
		break;
	case DLL_THREAD_ATTACH:
		Test();
		break;
	case DLL_THREAD_DETACH:
		Test();
		break;
	case DLL_PROCESS_DETACH:
		Test();
		break;
	}
	return TRUE;
}
