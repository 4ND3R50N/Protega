#pragma once
#include "../../stdafx.h"
#include <thread>
#include "boost\lexical_cast.hpp"
#include "Virtual_Memory_IO.h"
#include <fstream>
class Virtual_Memory_Protection_Cabal_Online: Virtual_Memory_IO
{
private:
	
	//CABAL ADDRESSES
	LPCVOID lpcvCabalBaseAddress = (LPCVOID)0x00B93530;
	LPCVOID lpcvCabalWallBaseAddress = (LPCVOID)0x010838D8;

	//Cabal Offsets
	LPCVOID lpcvCabalMapOffset =   (LPCVOID)0x000072E4;
	LPCVOID lpcvCabalSpeedOffset = (LPCVOID)0x204;

	LPCVOID lpcvCabalWallStartOffset = (LPCVOID)0x40814;
	LPCVOID lpcvCabalWallStopOffset = (LPCVOID)0x3ffff;

	//Cabal Values
	int iCabalMapDefaultValue = 4294967295;
	float fCabalNormalSpeed = 450.f;

	//Vars
	unsigned int iProcessID;
	
	//Functions Vars
	std::function<void(std::string sDetectedBaseAddress, std::string sDetectedOffset, std::string sDetectedValue, std::string sStandartValue) > funcCallbackHandler;

	//Functions


public:
	Virtual_Memory_Protection_Cabal_Online(unsigned int iProcessID,
		std::function<void(std::string sDetectedBaseAddress, std::string sDetectedOffset, std::string sDetectedValue, std::string sStandartValue) > funcCallbackHandler);
	bool OpenProcessInstance();
	bool CloseProcessInstance();
	void CheckAllVmpFunctions();

	//VMP Functions
	bool VMP_CheckGameSpeed();
	bool VMP_CheckWallBorders();
	//VMP Tests
	bool VMP_EnableWallHack();

	~Virtual_Memory_Protection_Cabal_Online();
};

