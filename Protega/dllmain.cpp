#include "stdafx.h"

extern "C" int __declspec(dllexport) __cdecl PEntryMain()
{
		
	MessageBoxA(NULL, "Protega is running!", "Protega antihack engine", NULL);
	return 1;
}
