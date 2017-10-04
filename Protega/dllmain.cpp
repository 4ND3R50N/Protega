#include "stdafx.h"
#include "Core/ProtegaCore.h"

bool bStopper = false;
void initAntihack();

extern "C"
{
	int __declspec(dllexport) __cdecl PEntryMain()
	{
		
		CreateThread(NULL, NULL, LPTHREAD_START_ROUTINE(initAntihack), NULL, 0, 0);
		return 1;
	}

	//Just for testing reasons
	int __declspec(dllexport) stopTrigger()
	{
		while (bStopper == false)
		{
			if (bStopper == true)
			{
				break;
			}
		}
		return true;
	}
}
void initAntihack()
{
	ProtegaCore *Antihack = new ProtegaCore();
	Antihack->StartAntihack();
}