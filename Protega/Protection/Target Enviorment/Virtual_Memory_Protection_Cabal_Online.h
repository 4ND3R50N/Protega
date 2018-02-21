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

	LPCVOID lpcvCabalGmAddress = (LPCVOID)0x0107D588;
	LPCVOID lpcvCabalRangeAddress = (LPCVOID)0x010CDC40;
	LPCVOID lpcvCabalAoeAddress = (LPCVOID)0x010CDC44;

	LPCVOID lpcvCabalSkillCooldownAddress = (LPCVOID)0x876FB4;

	//Cabal Offsets
	LPCVOID lpcvCabalMapOffset =   (LPCVOID)0x000072E4;
	LPCVOID lpcvCabalSpeedOffset = (LPCVOID)0x204;

	LPCVOID lpcvCabalWallStartOffset = (LPCVOID)0x40814;
	LPCVOID lpcvCabalWallStopOffset = (LPCVOID)0x3ffff;

	LPCVOID lpcvCabalZoomOffset1 = (LPCVOID)0x78D01C;
	LPCVOID lpcvCabalZoomOffset2 = (LPCVOID)0x791DD0;

	LPCVOID lpcvCabalSkillCastOffset = (LPCVOID)0x72D4;
	LPCVOID lpcvCabalAnimationSkillOffset = (LPCVOID)0x74;
	LPCVOID lpcvCabalAnimationOffset = (LPCVOID)0x1F4;
	LPCVOID lpcvCabalNoCastTimeOffset = (LPCVOID)0x3578;

	LPCVOID lpcvCabalNationOffset = (LPCVOID)0x35C;

	LPCVOID lpcvCabalBattleModeStateOffset = (LPCVOID)0x41B0;

	//Cabal Values
	//	Map
	int iCabalMapDefaultValue = 4294967295;
	//	Zoom
	int iCabalDefaultZoom1 = 2;
	int iCabalDefaultZoom2 = 1;
	//	NSD + NCT
	int iCabalSkillAnimationDefaultValue = 4294967295;
	int iCabalSkillValueLowerLimit = 3000000;
	int iCabalAnimationRun = 5;
	int iCabalAnimationSkill = 7;

	int iCabalLatestNoCastTimeValue = 0;
	int iCabalLatestCastValue = 0;
	int iCabalLatestBattleModeStateValue = 0;
	//	Speedcheck
	float fCabalMaxPossibleSpeed = 1200.f;
	//	Rangecheck
	int iCabalDefaultGM = 0;
	int iCabalDefaultRange = 0;
	int iCabalDefaultAOE = 0;
	//	No Cooldown
	int iCabalDefaultSkillCooldown = 69485707;
	//	Wallhack
	int iWallhackScanDelay = 5000; // <- Loading delay after player joins a map
	double iWallhackZeroTolerance = 80.0;
	bool bFirstChannelJoin = true;
	//	Monitoring
	int iCabalGm = 2;
	
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
	bool DetectManipulatedMemory();

	//VMP Functions
	bool VMP_CheckGameSpeed();
	bool VMP_CheckWallBorders();
	bool VMP_CheckZoomState();
	bool VMP_CheckNoSkillDelay();
	bool VMP_CheckNoCastTime();
	bool VMP_CheckSkillRange();
	bool VMP_CheckSkillCooldown();
	bool VMP_CheckNation();

	//VMP Tests
	bool VMP_EnableWallHack();

	~Virtual_Memory_Protection_Cabal_Online();
};

