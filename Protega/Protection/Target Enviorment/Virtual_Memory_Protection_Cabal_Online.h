#pragma once
#include "../../stdafx.h"
#include <thread>
#include "boost\lexical_cast.hpp"
#include "Virtual_Memory_IO.h"
#include <fstream>
#include <map>

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

	LPCVOID lpcvCabalSkillDelayOffset = (LPCVOID)0x72D4;
	LPCVOID lpcvCabalAnimationSkillOffset = (LPCVOID)0x74;
	LPCVOID lpcvCabalAnimationOffset = (LPCVOID)0x1F4;
	LPCVOID lpcvCabalNoCastTimeOffset = (LPCVOID)0x3578;

	LPCVOID lpcvCabalNationOffset = (LPCVOID)0x35C;

	LPCVOID lpcvCabalBattleModeStateOffset = (LPCVOID)0x41B0;

	LPCVOID lpcvCabalComboOffset1 = (LPCVOID)0x73A1;
	LPCVOID lpcvCabalComboOffset2 = (LPCVOID)0x7399;
	LPCVOID lpcvCabalComboOffset3 = (LPCVOID)0x73A0;
	LPCVOID lpcvCabalComboOffset4 = (LPCVOID)0x738C;
	LPCVOID lpcvCabalComboOffset5 = (LPCVOID)0x73A8;
	LPCVOID lpcvCabalComboOffset6 = (LPCVOID)0x7397;



	//Cabal Values
	//	Map
	int iCabalMapDefaultValue = 4294967295;
	//	Zoom
	int iCabalDefaultZoom1 = 2;
	int iCabalDefaultZoom2 = 1;

	//	NSD + NCT
	std::map<int, unsigned int> NctMap;
	unsigned int iNctWaitAfterSkillChange = 200;
	unsigned int iNctQueueSize = 30;
	unsigned int iNctDetectionTolerance = 3;

	std::vector<unsigned int> NsdVector;
	unsigned int iNsdAnormalyWaitTime = 1000;
	unsigned int iNsdQueueSize = 10;
	unsigned int iNsdDetectionTolerance = 3;
	unsigned int iCabalLatestNSDValueForNSDAlgorithm = 0;

	int iCabalSkillAnimationDefaultValue = 4294967295;
	int iCabalSkillValueLowerLimit = 2000000;
	int iCabalAnimationSkill = 7;

	int iCabalLatestNoCastTimeValue = 0;
	int iCabalLatestNSDValueForNCTAlgorithm = 0;
	int iCabalLatestBattleModeStateValue = 0;

	// Perfect Combo
	std::map<int, unsigned int> PerfectComboMap;
	unsigned int iPerfectComboQueueSize = 30;
	unsigned int iPerfectComboDetectionTolerance = 13;

	int iCabalLatestComboValue1 = 0;
	int iCabalLatestComboValue2 = 0;
	int iCabalLatestComboValue3 = 0;
	int iCabalLatestComboValue4 = 0;
	int iCabalLatestComboValue5 = 0;
	int iCabalLatestComboValue6 = 0;

	//	Speedcheck
	float fCabalMaxPossibleSpeed = 1200.f;

	//	Rangecheck
	int iCabalDefaultGM = 0;
	int iCabalDefaultRange = 0;
	int iCabalDefaultAOE = 0;

	//	No Cooldown
	int iCabalDefaultSkillCooldown = 69485707;

	//	Wallhack
	int iWallhackScanDelay = 10000; // <- Loading delay after player joins a map
	double iWallhackZeroTolerance = 80.0;
	bool bFirstChannelJoin = true;
	 
	//	Monitoring
	int iCabalGm = 2;
	
	//Vars
	unsigned int iProcessID;	
	
	//Functions Vars
	std::function<void(std::string sDetectedBaseAddress, std::string sDetectedOffset, std::string sDetectedValue, std::string sStandartValue) > funcCallbackHandler;

	//Functions
	void SumUpIndividualKeysInMap(std::map<int, unsigned int> *Map, unsigned int iValue);
	bool ValuesInMapReachedUpperLimit(std::map<int, unsigned int> *Map, unsigned int iUpperLimit, unsigned int *iDetectedKey);
	void CleanUpMapIfSizeIsReached(std::map<int, unsigned int> *Map, unsigned int iSize);

public:
	Virtual_Memory_Protection_Cabal_Online(unsigned int iProcessID,
		std::function<void(std::string sDetectedBaseAddress, std::string sDetectedOffset, std::string sDetectedValue, std::string sStandartValue) > funcCallbackHandler);
	
	bool OpenProcessInstance();
	bool CloseProcessInstance();
	bool NoIterativeFunctions_DetectManipulatedMemory();

	//VMP Functions
	bool VMP_CheckGameSpeed();
	bool VMP_CheckWallBorders();
	bool VMP_CheckZoomState();

	bool VMP_CheckNoSkillDelay();
	bool VMP_CheckNoSkillDelay_V2();

	bool VMP_CheckNoCastTime();
	bool VMP_CheckNoCastTime_V2();

	bool VMP_CheckSkillRange();
	bool VMP_CheckSkillCooldown();
	bool VMP_CheckNation();
	bool VMP_CheckPerfectCombo();

	//VMP Tests
	bool VMP_EnableWallHack();

	~Virtual_Memory_Protection_Cabal_Online();
};

