#pragma once
#include "../../stdafx.h"
#include <thread>
#include "boost\lexical_cast.hpp"
#include "Virtual_Memory_IO.h"
#include <fstream>
#include <map>

class Virtual_Memory_Protection_Cabal_Online : Virtual_Memory_IO
{
private:
	
	//CABAL ADDRESSES
	LPCVOID lpcvCabalBaseAddress = (LPCVOID)0x00B93530;
	LPCVOID lpcvCabalWallBaseAddress = (LPCVOID)0x010838D8;
	LPCVOID lpcvCabalModuleAddress = (LPCVOID)0x400000;

	//GM range adresses
	LPCVOID lpcvCabalGmAddress = (LPCVOID)0x0107D588;
	LPCVOID lpcvCabalRangeAddress = (LPCVOID)0x010CDC40;
	LPCVOID lpcvCabalAoeAddress = (LPCVOID)0x010CDC44;

	//Channel address
	LPCVOID lpcvCabalChannelAddress = (LPCVOID)0x0107B1A4;

	// Skill cooldown
	LPCVOID lpcvCabalSkillCooldownAddress = (LPCVOID)0x876FB4;

	//Range Offsets
	LPCVOID lpcvCabalRangeOffset1 = (LPCVOID)0x6E70;
	LPCVOID lpcvCabalRangeOffset2 = (LPCVOID)0x4E8;

	//Map 
	LPCVOID lpcvCabalMapOffset = (LPCVOID)0x000072E4;
	
	//Class
	LPCVOID lpcvCabalClassOffset = (LPCVOID)0x4178;

	enum eCabalClasses
	{
		WA = 1,
		BL = 2,
		WI = 3,
		FA = 4,
		FS = 5,
		FB = 6,
	};

	//Speed
	LPCVOID lpcvCabalSpeedOffset = (LPCVOID)0x204;
	
	//Wall offsets
	LPCVOID lpcvCabalWallStartOffset = (LPCVOID)0x40814;
	LPCVOID lpcvCabalWallStopOffset = (LPCVOID)0x3ffff;
	
	//Zoom offsets
	LPCVOID lpcvCabalZoomOffset1 = (LPCVOID)0x78D01C;
	LPCVOID lpcvCabalZoomOffset2 = (LPCVOID)0x791DD0;

	//Animation offsets
	LPCVOID lpcvCabalSkillDelayOffset = (LPCVOID)0x72D4;
	LPCVOID lpcvCabalAnimationSkillOffset = (LPCVOID)0x74;
	LPCVOID lpcvCabalAnimationOffset = (LPCVOID)0x1F4;
	LPCVOID lpcvCabalNoCastTimeOffset = (LPCVOID)0x3578;

	//Nation
	LPCVOID lpcvCabalNationOffset = (LPCVOID)0x35C;

	//Combo offsets
	LPCVOID lpcvCabalComboOffset1 = (LPCVOID)0x73A1;
	LPCVOID lpcvCabalComboOffset2 = (LPCVOID)0x7399;
	LPCVOID lpcvCabalComboOffset3 = (LPCVOID)0x73A0;
	LPCVOID lpcvCabalComboOffset4 = (LPCVOID)0x738C;
	LPCVOID lpcvCabalComboOffset5 = (LPCVOID)0x73A8;
	LPCVOID lpcvCabalComboOffset6 = (LPCVOID)0x7397;

	//Battle mode state offsets
	LPCVOID lpcvCabalBattleModeStateOffset = (LPCVOID)0x41B0;
	LPCVOID lpcvCabalBattelModeStateBOffset = (LPCVOID)0x344;

	//BM Offsets
	//	BM1
	LPCVOID lpcvCabalBM1Offset1 = (LPCVOID)0x4A1C;
	LPCVOID lpcvCabalBM1Offset2 = (LPCVOID)0x120;
	LPCVOID lpcvCabalBM1Offset3 = (LPCVOID)0x20;

	//	BM2 (Offset 1 + 3 are the same as bm1
	LPCVOID lpcvCabalBM2Offset2 = (LPCVOID)0x124;

	//	Aura
	LPCVOID lpcvCabalAuraOffset2 = (LPCVOID)0x118;

	enum eNotShiftedBattleModeValues
	{
		Aura = 8,
		BM1 = 16,
		BM2 = 32,
		BM3 = 64,
		BM1_And_Aura = 24,
		BM2_And_Aura = 40,
		BM3_And_Aura = 72
	};


	//Cabal Values
	//	Central Value
	int iCurrentSelectedChannel = -1;
	//	Map
	int iCabalMapDefaultValue = 4294967295;
	//	Zoom
	int iCabalDefaultZoom1 = 2;
	int iCabalDefaultZoom2 = 1;

	//	NSD + NCT
	std::map<int, unsigned int> NctMap;
	unsigned int iNctWaitAfterSkillChange = 250;
	unsigned int iNctQueueSize = 20;
	unsigned int iNctDetectionTolerance = 4;


	std::vector<unsigned int> NsdVector;
	unsigned int iNsdAnormalyWaitTime = 1000;
	unsigned int iNsdQueueSize = 10;
	unsigned int iNsdDetectionTolerance = 3;
	unsigned int iNsdDetectionToleranceForBm2 = 5;
	unsigned int iCabalLatestNSDValueForNSDAlgorithm = 0;

	unsigned int iCabalBm2Value1 = 32;
	unsigned int iCabalBm2Value2 = 40;

	int iCabalSkillAnimationDefaultValue = 4294967295;
	unsigned int iCabalSkillValueLowerLimit = 2000000;
	int iCabalAnimationSkill = 7;
	int iCabalAnimationDeath = 2;

	int iCabalLatestNSDValueForNCTAlgorithm = 0;
	int iCabalLatestSkillAnimationValueForNCTAlgorithm = 0;

	// Perfect Combo
	std::map<int, unsigned int> PerfectComboMap1;
	std::map<int, unsigned int> PerfectComboMap2;
	std::map<int, unsigned int> PerfectComboMap3;
	std::map<int, unsigned int> PerfectComboMap4;
	std::map<int, unsigned int> PerfectComboMap5;
	std::map<int, unsigned int> PerfectComboMap6;

	unsigned int iPerfectComboQueueSize = 5;
	unsigned int iPerfectComboDetectionTolerance = 5;

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
	int iCabalRangeUpperLimit = 1;
	int iCabalRangeLowerLimit = -2;

	//	No Cooldown
	int iCabalDefaultSkillCooldown = 69485707;

	//	Wallhack
	int iWallhackScanDelay = 10000; // <- Loading delay after player joins a map
	double iWallhackZeroTolerance = 80.0;
	bool bFirstChannelJoin = true;
	
	//	BM Cooldown reset
	int iCabalLatestBattleMode1CdValue = -1;
	int iCabalLatestBattleMode2CdValue = -1;
	int iCabalLatestAuraCdValue = -1;

	bool bBm1IsRunning = false;
	bool bBm2IsRunning = false;
	bool bAuraIsRunning = false;
	bool bBm1RecastException = true;
	bool bBm2RecastException = true;
	bool bAuraRecastException = true;

	clock_t ctCabalBM1Timer;
	clock_t ctCabalBM2Timer;
	clock_t ctCabalAuraTimer;

	unsigned int iLatestAnimationValueForBM1 = 0;
	unsigned int iLatestAnimationValueForBM2 = 0;
	unsigned int iLatestAnimationValueForAura = 0;

	//		Static vars, they get used by the abstract algorithm for all BMs
	unsigned int iBmRecastExceptionWaitTime = 1000;
	double dBmSkillCooldown = 27.0;
	
	//	KillGate
	int iCabalLatestMapValue = -1;

	//	Monitoring
	int iCabalGm = 2;
	
	//	FB Dame
	std::vector<unsigned int> FbDameVector;
	//Value has to be 5, because the FB Buffs stacks the BM State B.
	int iFbDameBattleModeStateBAnormaly = 5;
	int iFbDameBattleModeStateBAnormalyForAura = 7;
	int iFbDameDetectionTolerance = 3;
	int iFbDameAnormalyWaitTime = 1000;
	int iFbDameVectorQueueSize = 10;

	//Vars
	unsigned int iProcessID;	
	
	//Functions Vars
	std::function<void(std::string sDetectedBaseAddress, std::string sDetectedOffset, std::string sDetectedValue, std::string sStandartValue) > funcCallbackHandler;

	//Functions

	bool VMP_Private_Abstract_CheckBmCooldownReset(int* iCabalLatestBattleModeCdValue, LPCVOID CabalBaseAddress,
		LPCVOID CabalBMOffset1, LPCVOID CabalBMOffset2, LPCVOID CabalBMOffset3,
		clock_t* CabalBMTimer, bool* BmIsRunning, bool* BmRecastException, unsigned int* iLatestAnimationValueForBMCD);

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

	bool VMP_CheckNoCastTime_V2();

	bool VMP_CheckSkillRange();
	bool VMP_CheckSkillCooldown();
	bool VMP_CheckNation();
	bool VMP_CheckPerfectCombo();
	bool VMP_CheckFbBm1Freeze();
	bool VMP_CheckBmCooldownReset();
	bool VMP_CheckKillGate();

	bool VMP_CheckFbDame();

	//VMP Tests
	bool VMP_EnableWallHack();

	~Virtual_Memory_Protection_Cabal_Online();
};

