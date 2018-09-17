#pragma once
#include <functional>
#include <vector>
#include "../../Tools/Virtual_Memory_IO.h"

class Service_Cabal_Online : Virtual_Memory_IO
{
private:
	//Addresses
	LPCVOID lpcvAccountNameAddress = (LPCVOID)((unsigned int)0x00400000 + (unsigned int)0x78D020);

public:
	Service_Cabal_Online(int iTargetApplicationId);
	std::string GetCabalAccountName();
};

