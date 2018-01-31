#include "stdafx.h"
#include "Core/ProtegaCore.h"

bool bStopper = false;

void DebugThreadStart();
static DWORD WINAPI HiddenThreadStart(void* Param);

extern "C"
{
	int __declspec(dllexport) __cdecl ProcMainEntry()
	{		
		CreateThread(NULL, NULL, HiddenThreadStart, NULL, 0, 0);
		return 1;
	}
}
void DebugThreadStart()
{
	ProtegaCore *Antihack = new ProtegaCore();
	Antihack->StartAntihack();
}

static DWORD WINAPI HiddenThreadStart(void* Param)
{
	ProtegaCore *Antihack = new ProtegaCore();
	Antihack->StartAntihack();
	return 1;
}