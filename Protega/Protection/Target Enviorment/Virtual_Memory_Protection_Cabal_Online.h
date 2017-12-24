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
	LPCVOID lpcvCabalModuleAddress = (LPCVOID)0x400000;


	//Cabal Offsets
	LPCVOID lpcvCabalMapOffset =   (LPCVOID)0x000072E4;
	LPCVOID lpcvCabalSpeedOffset = (LPCVOID)0x204;

	LPCVOID lpcvCabalWallStartOffset = (LPCVOID)0x40814;
	LPCVOID lpcvCabalWallStopOffset = (LPCVOID)0x3ffff;

	LPCVOID lpcvCabalZoomOffset1 = (LPCVOID)0x78D01C;
	LPCVOID lpcvCabalZoomOffset2 = (LPCVOID)0x791DD0;

	LPCVOID lpcvCabalSkillCastOffset = (LPCVOID)0x72D4;
	LPCVOID lpcvCabalAnimationOffset = (LPCVOID)0x1F4;

	//Cabal Values
	int iCabalMapDefaultValue = 4294967295;
	int iCabalDefaultZoom = 3;
	int iCabalSkillValueLowerLimit = 3000000;
	int iCabalAnimationRun = 5;
	int iCabalAnimationSkill = 7;
	float fCabalNormalSpeed = 450.f;


	//Vars
	unsigned int iProcessID;
	
	//Functions Vars
	std::function<void(std::string sDetectedBaseAddress, std::string sDetectedOffset, std::string sDetectedValue, std::string sStandartValue) > funcCallbackHandler;

	//Functions
	//static void WriteMemoryValueAsync(void* Param);

public:
	Virtual_Memory_Protection_Cabal_Online(unsigned int iProcessID,
		std::function<void(std::string sDetectedBaseAddress, std::string sDetectedOffset, std::string sDetectedValue, std::string sStandartValue) > funcCallbackHandler);
	bool OpenProcessInstance();
	bool CloseProcessInstance();
	void CheckAllVmpFunctions();

	//VMP Functions
	bool VMP_CheckGameSpeed();
	bool VMP_CheckWallBorders();
	bool VMP_CheckZoomState();
	bool VMP_CheckNoSkillAnimation();
	//VMP Tests
	bool VMP_EnableWallHack();

	~Virtual_Memory_Protection_Cabal_Online();
};

