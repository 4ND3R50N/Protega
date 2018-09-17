#include "../../stdafx.h"
#include "Service_Cabal_Online_Collector.h"

Service_Cabal_Online::Service_Cabal_Online(int iTargetApplicationId):Virtual_Memory_IO(iTargetApplicationId)
{

}

std::string Service_Cabal_Online::GetCabalAccountName()
{
	return ReadMemoryString(hProcessHandle, lpcvAccountNameAddress, 128);
}
