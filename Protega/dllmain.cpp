#include "stdafx.h"
#include "Core/ProtegaCore.h"

bool bStopper = false;
void initAntihack();

extern "C"
{
	int __declspec(dllexport) __cdecl ProcMainEntry()
	{		
		CreateThread(NULL, NULL, LPTHREAD_START_ROUTINE(initAntihack), NULL, 0, 0);
		return 1;
	}
}
void initAntihack()
{
	ProtegaCore *Antihack = new ProtegaCore();
	Antihack->StartAntihack();
}