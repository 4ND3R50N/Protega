#include "../../stdafx.h"
#include "Virtual_Memory_Protection_Cabal_Online.h"


Virtual_Memory_Protection_Cabal_Online::Virtual_Memory_Protection_Cabal_Online(unsigned int iProcessID,
	std::function<void(std::string sDetectedBaseAddress, std::string sDetectedOffset, std::string sDetectedValue, std::string sStandartValue) > funcCallbackHandler)
{
	this->iProcessID = iProcessID;
	this->funcCallbackHandler = funcCallbackHandler;
	hProcessHandle = NULL;
}


Virtual_Memory_Protection_Cabal_Online::~Virtual_Memory_Protection_Cabal_Online()
{
	CloseProcessInstance();
}

bool Virtual_Memory_Protection_Cabal_Online::VMP_Private_Abstract_CheckBmCooldownReset(int* iCabalLatestBattleModeCdValue, LPCVOID CabalBaseAddress, 
	LPCVOID CabalBMOffset1, LPCVOID CabalBMOffset2, LPCVOID CabalBMOffset3, 
	clock_t* CabalBMTimer, bool* BmIsRunning, bool* BmRecastException, unsigned int* iLatestAnimationValueForBMCD)
{
	//This triggers if its the first run
	if (*iCabalLatestBattleModeCdValue == -1)
	{
		*iCabalLatestBattleModeCdValue = GetIntViaLevel3Pointer(CabalBaseAddress,
			CabalBMOffset1, CabalBMOffset2, CabalBMOffset3);
	}

	//Collect current BM + Aura value
	int iCurrentBattleModeCdValue = GetIntViaLevel3Pointer(lpcvCabalBaseAddress,
										CabalBMOffset1, CabalBMOffset2, CabalBMOffset3);

	//Check if there was a state change
	if (iCurrentBattleModeCdValue != *iCabalLatestBattleModeCdValue && !*BmIsRunning)
	{
		*BmIsRunning = true;
		//Trigger Bm1 ticker
		*CabalBMTimer = std::clock();
		//Safe the value change (Bm gets triggered first)
		*iCabalLatestBattleModeCdValue = iCurrentBattleModeCdValue;
	}

	//While counter is running -> While BM1 CD is running
	if (*BmIsRunning)
	{
		//Get the current passed time
		double dCurrentDuration = (std::clock() - *CabalBMTimer) / (double)CLOCKS_PER_SEC;

		//Check if the bm skill cooldown is nearly over || dBmSkillCooldown -> Global var
		if (dCurrentDuration < dBmSkillCooldown)
		{
			//If the var toggles to 0 after bm1 gets triggered again (which is normal), do an exception for that
			if (*BmRecastException)
			{
				//iBmRecastExceptionWaitTime -> Global var
				Sleep(iBmRecastExceptionWaitTime);
				//REFACTOR (Create new IO)
				*iCabalLatestBattleModeCdValue = GetIntViaLevel3Pointer(lpcvCabalBaseAddress,
					CabalBMOffset1, CabalBMOffset2, CabalBMOffset3);

				*BmRecastException = false;
				return false;
			}

			//Happens if u cancel BM
			if (iCurrentBattleModeCdValue > *iCabalLatestBattleModeCdValue)
			{
				*BmRecastException = true;
				return false;
			}

			//Check if Animation = 2 (Death)
			int iCurrentAnimationValue = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalAnimationOffset);

			//Check, if we are in death mode
			if (*iLatestAnimationValueForBMCD == 2)
			{
				//Check, if death mode ended in this iteration
				if (iCurrentAnimationValue != 2)
				{
					//Unset the death mode, update latest CD battle mode value to not cause a hack detect in the next run
					*iLatestAnimationValueForBMCD = iCurrentAnimationValue;
					*iCabalLatestBattleModeCdValue = 0;
				}
			}

			//Check, if we are in death mode
			if (iCurrentAnimationValue == 2)
			{
				//Set death mode for next iteration
				//This will also trigger each time we are in death mode
				*iLatestAnimationValueForBMCD = 2;
				return false;
			}

			if (iCurrentBattleModeCdValue < *iCabalLatestBattleModeCdValue)
			{
				return true;
			}
		}
		else
		{
			*BmIsRunning = false;
			*BmRecastException = true;
		}
	}

	return false;
}

//Private
void Virtual_Memory_Protection_Cabal_Online::SumUpIndividualKeysInMap(std::map<int, unsigned int>* Map, unsigned int iValue)
{
	if (Map->find(iValue) == Map->end())
	{
		Sleep(1);
		Map->insert(std::make_pair(iValue, (unsigned int)1));
	}
	else
	{
		//Win7 Bug ->
		Sleep(1);
		std::map<int, unsigned int>::iterator it = Map->find(iValue);
		it->second = it->second++;
	}
}

bool Virtual_Memory_Protection_Cabal_Online::ValuesInMapReachedUpperLimit(std::map<int, unsigned int>* Map, unsigned int iUpperLimit, unsigned int *iDetectedKey)
{

	std::map<int, unsigned int>::iterator it;
	for (it = Map->begin(); it != Map->end(); it++)
	{
		if (it->second >= iUpperLimit)
		{
			return true;
		}
	}
	return false;
}

void Virtual_Memory_Protection_Cabal_Online::CleanUpMapIfSizeIsReached(std::map<int, unsigned int> *Map, unsigned int iSize)
{
	if (Map->size() >= iSize)
	{
		//std::ofstream filestr;
		//std::map<int, unsigned int>::iterator it;
		//filestr.open(".\\nctmap.txt", std::fstream::in | std::fstream::out | std::fstream::app);
		//for (it = NctMap.begin(); it != NctMap.end(); it++)
		//{
		//	filestr << it->first << " || " << it->second << std::endl;
		//}
		//filestr << "-----------------------------" << std::endl;
		//filestr.close();
		Map->clear();
	}
}

//Public
bool Virtual_Memory_Protection_Cabal_Online::OpenProcessInstance()
{
	//Get Process Handle
	hProcessHandle = OpenProcess(PROCESS_ALL_ACCESS, false, iProcessID);
	if (!hProcessHandle) {
		return false;
	}
	return true;
}

bool Virtual_Memory_Protection_Cabal_Online::CloseProcessInstance()
{	
	return CloseHandle(hProcessHandle);
}

bool Virtual_Memory_Protection_Cabal_Online::NoIterativeFunctions_DetectManipulatedMemory()
{
	if ((VMP_CheckGameSpeed() || VMP_CheckWallBorders() || VMP_CheckZoomState() || 
		VMP_CheckSkillRange() || VMP_CheckSkillCooldown()  /*|| VMP_CheckFbDame()|| VMP_CheckNation()*/) == true)
	{
		return true;
	}
	return false;
}


//VMP Functions

//NOTE: There are different speed values! Check all of them!
bool Virtual_Memory_Protection_Cabal_Online::VMP_CheckGameSpeed()
{
	//Check if we are currently on a map
	if (GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalMapOffset) != iCabalMapDefaultValue)
	{
		//Check if the speed is normal (450)
		float fCurrentSpeed = GetFloatViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalSpeedOffset);
		if (fCurrentSpeed > fCabalMaxPossibleSpeed)
		{
			//Function callback triggern
			funcCallbackHandler("CABAL BASE ADDRESS", "SPEED OFFSET", boost::lexical_cast<std::string>(fCurrentSpeed), boost::lexical_cast<std::string>(fCabalMaxPossibleSpeed));
			return true;
		}
	}
	return false;
}

bool Virtual_Memory_Protection_Cabal_Online::VMP_CheckWallBorders()
{
	//Skip algorithm, if MapValue is on default
	if (GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalMapOffset) == iCabalMapDefaultValue)
	{
		return false;
	}
	//Use a delay after joining the game for the first time
	else if (bFirstChannelJoin)
	{
		Sleep(iWallhackScanDelay);
		bFirstChannelJoin = false;
	}

	//Iterate through the Wall adresses
	int iZeros = 0;
	int iNonZeros = 0;
	LPCVOID StartAdress = (LPCVOID)(ReadMemoryInt(hProcessHandle, lpcvCabalWallBaseAddress) + (unsigned int)lpcvCabalWallStartOffset);
	
	//std::ofstream file("Wallhack test.txt");
	
	for (unsigned int i = 0; i < (unsigned int)lpcvCabalWallStopOffset; i = i + 4)
	{
		//Get the current variable in wall address room
		int iCurrentVal = ReadMemoryInt(hProcessHandle, (LPCVOID)((unsigned int)StartAdress + i));
		
		//Count Zeros + Non Zeros
		(iCurrentVal != 0) ? iNonZeros++ : iZeros++;
		//Debug		
		//file << "Value on iteration " << i << " | Startaddress: " << (unsigned int)StartAdress << " | VALUE: " << iCurrentVal << "\n";
	}

	//Get procentual
	int iAmountOfAllAddresses = iZeros + iNonZeros;
	double dPercentOfZeros = (100.0 / iAmountOfAllAddresses)*iZeros;
	double dPercentOfNonZeros = 100.0 - dPercentOfZeros;

	//If the percent of zeros are higher than the setted tolerance -> detect
	if (dPercentOfZeros >= iWallhackZeroTolerance)
	{
		std::stringstream ss;
		ss << dPercentOfZeros << "% Zeros";
		std::string sDetectedValue = ss.str();
		ss.str("");
		ss << "NOT < " << iWallhackZeroTolerance << "%";
		std::string sDefaultValue = ss.str();
		ss.str("");
		funcCallbackHandler("CABAL WALL BASE ADDRESS", "WALL START OFFSET", sDetectedValue, sDefaultValue);
		return true;
	}

	//file << "Amount of all Addresses = " << iAmountOfAllAddresses << " | % of all Zeros = " << dPercentOfZeros << " | % of all non Zeros = " << dPercentOfNonZeros;
	//file.close();	
	return false;
}

//NOTE: reset vars to 3 after a detect
bool Virtual_Memory_Protection_Cabal_Online::VMP_CheckZoomState()
{
	//Maybe we need here a map check later...
	//Get Zoom Values
	int iZoom1 = ReadMemoryInt(hProcessHandle, (LPCVOID)((unsigned int)lpcvCabalModuleAddress + (unsigned int)lpcvCabalZoomOffset1));
	int iZoom2 = ReadMemoryInt(hProcessHandle, (LPCVOID)((unsigned int)lpcvCabalModuleAddress + (unsigned int)lpcvCabalZoomOffset2));
	
	//Check if they are showing the default value
	if ((iZoom1 != iCabalDefaultZoom1 && iZoom1 != iCabalDefaultZoom2)  ||
		iZoom2 != iCabalDefaultZoom1 && iZoom2 != iCabalDefaultZoom2)
	{
		std::stringstream ss;
		ss << "Zoom1: " << iZoom1 << " | Zoom2: " << iZoom2;
		std::string sDetectedValue = ss.str();
		ss.str("");
		ss << "Default Zoom 1/2: " << iCabalDefaultZoom1 << " and " << iCabalDefaultZoom2;
		std::string sDefaultValue = ss.str();
		ss.str("");

		funcCallbackHandler("CABAL MODULE ADDRESS", "ZOOM OFFSET 1 OR 2", sDetectedValue, sDefaultValue);
		return true;
	}
	return false;
}

//NOTE: There has to be some other checks. E.G: No stun = 7 -> cast > 30000000, No stun = 3 -> 1XXXXXXXX ....
bool Virtual_Memory_Protection_Cabal_Online::VMP_CheckNoSkillDelay()
{
	//Skip algorithm if skill animation is default
	if (GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalAnimationSkillOffset) == iCabalSkillAnimationDefaultValue)
	{
		return false;
	}

	//Get Skill Cast address to write them later
	LPCVOID SkillCastAddress = GetAddressOfLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalSkillDelayOffset);

	//CreateThread(NULL, NULL, LPTHREAD_START_ROUTINE(WriteMemoryValueAsync), (void*)this, 0, 0);
	
	//Get the actual animation value
	int iCurrentAnimationValue = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalAnimationOffset);

	//Check of the animation value is currently showing a skill
	if (iCurrentAnimationValue == iCabalAnimationSkill)
	{
		//Get the current skill cast value
		int iCurrentSkillCastValue = ReadMemoryInt(hProcessHandle, SkillCastAddress);
		//Check if the value is NOT a skill animation cast
		if (iCurrentSkillCastValue < iCabalSkillValueLowerLimit)
		{
			std::stringstream ss;
			ss << iCurrentSkillCastValue;
			//Note: Information is unclear...
			funcCallbackHandler("CABAL MODULE ADDRESS", "SKILL CAST OFFSET", ss.str(), ">2000000");
			return true;
		}
	}
	return false;
}

bool Virtual_Memory_Protection_Cabal_Online::VMP_CheckNoSkillDelay_V2()
{
	//Skip algorithm if skill animation is default
	if (GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalAnimationSkillOffset) == iCabalSkillAnimationDefaultValue)
	{
		return false;
	}	

	if (iCabalLatestNSDValueForNSDAlgorithm == 0)
	{
		iCabalLatestNSDValueForNSDAlgorithm = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalSkillDelayOffset);
	}

	//Get the actual animation value
	int iCurrentAnimationValue = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalAnimationOffset);
	//Get the current skill cast value
	int iCurrentSkillDelayValue = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalSkillDelayOffset);
	//Get battle mode
	int iCurrentBattleModeState = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalBattleModeStateOffset);

	//Check if there is currently an attack animation, AND
	//1. an NSD that starts with 1 -> This value causes an Anomaly
	//2. an NSD that start with > 2 but is not the same as the run before -> this causes just a fill into the vector
	if ((iCurrentAnimationValue == iCabalAnimationSkill && iCurrentSkillDelayValue < iCabalSkillValueLowerLimit) || 
		(iCurrentAnimationValue == iCabalAnimationSkill && iCabalLatestNSDValueForNSDAlgorithm != iCurrentSkillDelayValue))
	{
		//Push NSD var in the list
		NsdVector.push_back(iCurrentSkillDelayValue);

		//Check if the NSD list contains > tolerance values
		std::vector<unsigned int>::iterator iIt;
		unsigned int iAnomalyApperances = 0;

		for (iIt = NsdVector.begin(); iIt != NsdVector.end(); iIt++)
		{
			unsigned int& iItData(*iIt);
			if (iItData < iCabalSkillValueLowerLimit)
			{
				iAnomalyApperances++;
				//Slow down for fail detection (BM2 WI Mana refill)
				Sleep(iNsdAnormalyWaitTime);
			}
		}

		//Check, if user is in BM2 (then the apperances needs to be X or higer
		//		 if user is not in BM2 (then the apperances needs to be Y or higher
		if (((iCurrentBattleModeState == iCabalBm2Value1 || iCurrentBattleModeState == iCabalBm2Value2) && iAnomalyApperances >= iNsdDetectionToleranceForBm2) ||
			((iCurrentBattleModeState != iCabalBm2Value1 && iCurrentBattleModeState != iCabalBm2Value2) && iAnomalyApperances >= iNsdDetectionTolerance))
		{
	/*		std::ofstream filestr;
			filestr.open(".\\nsddetect.txt", std::fstream::in | std::fstream::out | std::fstream::app);
			std::vector<unsigned int>::iterator iIt;
			for (iIt = NsdVector.begin(); iIt != NsdVector.end(); iIt++)
			{
				unsigned int& iItData(*iIt);
				filestr << iItData << std::endl;
			}
			filestr << "-----------------------------1 " << iCurrentBattleModeState << std::endl;
			filestr.close();*/

			std::stringstream ss;
			ss << ">= " << iNctDetectionTolerance;
			funcCallbackHandler("CABAL MODULE ADDRESS", "NSD", "-", ss.str());
		}

		//Cleanup vector if too large
		if (NsdVector.size() >= iNsdQueueSize)
		{
			/*std::ofstream filestr;
			filestr.open(".\\nsdmap.txt", std::fstream::in | std::fstream::out | std::fstream::app);
			std::vector<unsigned int>::iterator iIt;
			for (iIt = NsdVector.begin(); iIt != NsdVector.end(); iIt++)
			{
				unsigned int& iItData(*iIt);
				filestr << iItData << std::endl;
			}
			filestr << "-----------------------------" << iCurrentBattleModeState << std::endl;
			filestr.close();*/
			NsdVector.clear();
		}

		//rewrite iCabalLatestNSDValueForNSDAlgorithm only if it was an attack
		if (iCurrentSkillDelayValue > iCabalSkillValueLowerLimit)
		{
			iCabalLatestNSDValueForNSDAlgorithm = iCurrentSkillDelayValue;
		}

	}
	return false;
}

bool Virtual_Memory_Protection_Cabal_Online::VMP_CheckNoCastTime_V2()
{
	//Skip algorithm if animation var is default
	if (GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalAnimationSkillOffset) == iCabalSkillAnimationDefaultValue)
	{
		return false;
	}
	
	//Check if its the first run, collect NSD + NCT Value
	if (iCabalLatestNSDValueForNCTAlgorithm == 0)
	{
		iCabalLatestSkillAnimationValueForNCTAlgorithm = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalAnimationSkillOffset);
		iCabalLatestNSDValueForNCTAlgorithm = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalSkillDelayOffset);
		iCurrentSelectedChannel = ReadMemoryInt(hProcessHandle, lpcvCabalChannelAddress);
	}

	//Get current NSD value
	int iCurrentNSDValue = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalSkillDelayOffset);
	
	//Get current animation ´value
	int iCurrentAnimationValue1 = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalAnimationSkillOffset);

	//Animation
	int iCurrentAnimationValue2 = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalAnimationOffset);

	//Battle mode state
	//int iCurrentBattleModeState = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalBattleModeStateOffset);

	//Channel
	int iCurrentChannelInMemory = ReadMemoryInt(hProcessHandle, lpcvCabalChannelAddress);

	//If character dies, or map switched
	if (iCurrentAnimationValue2 == iCabalAnimationDeath && iCurrentChannelInMemory != iCurrentSelectedChannel)
	{
		iCurrentSelectedChannel = iCurrentChannelInMemory;
		CleanUpMapIfSizeIsReached(&NctMap, 0);
		return false;
	}

	//On attack - skill change
	if( //Normal Mode. Waits for animation change + requires to be in an attack 
		iCurrentAnimationValue1 != iCabalLatestSkillAnimationValueForNCTAlgorithm && /*iCurrentAnimationValue2 == iCabalAnimationSkill ||*/
		//Wizzard exception: Check if in Bm + Skill change + Skill = Attack + requires to be in an attack
		/*iCurrentBattleModeState > 2 &&*/ iCurrentNSDValue != iCabalLatestNSDValueForNCTAlgorithm && iCurrentNSDValue > iCabalSkillValueLowerLimit && iCurrentAnimationValue2 == iCabalAnimationSkill ||
		//Normal Attack spammer BMs: Check if normal attack spam
		//	GL + WA
		iCurrentAnimationValue1 == 96 ||
		//  BL
		iCurrentAnimationValue1 == 100 ||
		//	FA
		iCurrentAnimationValue1 == 104 ||
		iCurrentAnimationValue1 == 106 ||
		//  FS
		iCurrentAnimationValue1 == 108 ||
		//  FB
		iCurrentAnimationValue1 == 111)
	{
		Sleep(iNctWaitAfterSkillChange);

		//If triggered because of BM2 spam: wait X second to slow down iteration (Can be added to canon)
		if (iCurrentAnimationValue1 == 96 ||
			iCurrentAnimationValue1 == 100 ||
			iCurrentAnimationValue1 == 104 ||
			iCurrentAnimationValue1 == 106 ||
			iCurrentAnimationValue1 == 108 ||
			iCurrentAnimationValue1 == 111)
		{
			Sleep(600);
		}

		//Get the actual NCT value
		int iCurrentNoCastValue = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalNoCastTimeOffset);

		//Add value to map
		SumUpIndividualKeysInMap(&NctMap, iCurrentNoCastValue);

		iCabalLatestNSDValueForNCTAlgorithm = iCurrentNSDValue;
		iCabalLatestSkillAnimationValueForNCTAlgorithm = iCurrentAnimationValue1;

		//Check map
		unsigned int iDetectedKey = 0;
		if (ValuesInMapReachedUpperLimit(&NctMap, iNctDetectionTolerance, &iDetectedKey))
		{
			/*std::ofstream filestr;

			std::map<int, unsigned int>::iterator it;
			filestr.open(".\\nctdetect.txt", std::fstream::in | std::fstream::out | std::fstream::app);
			for (it = NctMap.begin(); it != NctMap.end(); it++)
			{
				filestr << it->first << " || " << it->second << std::endl;
			}
			filestr << "-----------------------------" << " B: " << iCurrentBattleModeState << " A1: " << iCurrentAnimationValue1 << " A2: " << iCurrentAnimationValue2 << " ND: " << iCurrentNSDValue << "LND: " << iCabalLatestNSDValueForNCTAlgorithm << std::endl;
			filestr.close();*/

			std::stringstream ss;
			ss << ">= " << iNctDetectionTolerance;
			funcCallbackHandler("CABAL MODULE ADDRESS", "NCT", std::to_string(iDetectedKey), ss.str());
			return true;
		}

		//Cleanup
		CleanUpMapIfSizeIsReached(&NctMap, iNctQueueSize);
	
	}
	return false;
}

bool Virtual_Memory_Protection_Cabal_Online::VMP_CheckSkillRange()
{
	//Check GM, GM range, GM AOE <> 0
	int iCabalCurrentGmValue = ReadMemoryInt(hProcessHandle, lpcvCabalGmAddress);
	int iCabalCurrentRangeValue = ReadMemoryInt(hProcessHandle, lpcvCabalRangeAddress);
	int iCabalCurrentAoeValue = ReadMemoryInt(hProcessHandle, lpcvCabalAoeAddress);

	int iCabalCurrentRangeValue1 = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalRangeOffset1);
	int iCabalCurrentRangeValue2 = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalRangeOffset2);

	if (iCabalCurrentGmValue != iCabalDefaultGM || iCabalCurrentRangeValue != iCabalDefaultRange || iCabalCurrentAoeValue != iCabalDefaultAOE
		|| (iCabalCurrentRangeValue1 < iCabalRangeLowerLimit || iCabalCurrentRangeValue1 > iCabalRangeUpperLimit)
		|| (iCabalCurrentRangeValue2 < iCabalRangeLowerLimit || iCabalCurrentRangeValue2 > iCabalRangeUpperLimit))
	{
		std::stringstream ss;
		ss << "GM: " << iCabalCurrentGmValue << " | Range: " << iCabalCurrentRangeValue << " | Aoe: " << iCabalCurrentAoeValue << " | RangeA: "
			<< iCabalCurrentRangeValue1 << " | RangeB: " << iCabalCurrentRangeValue2;
		std::string sDetectedValues = ss.str();
		ss.str("");
		ss << "GM: " << iCabalDefaultGM << " | Range: " << iCabalDefaultRange << " | Aoe: " << iCabalDefaultAOE << "RangeA: " << iCabalDefaultRange
			<< " | RangeB: " << iCabalDefaultRange;
		std::string sDefaultValues = ss.str();
		ss.str("");
		funcCallbackHandler("GM | RANGE | AOE | RangeA | RangeB", "RNG", sDetectedValues, sDefaultValues);
	}

	return false;
}

bool Virtual_Memory_Protection_Cabal_Online::VMP_CheckSkillCooldown()
{	
	//Get skill cooldown value
	int iCurrentSkillCooldownValue = ReadMemoryInt(hProcessHandle, lpcvCabalSkillCooldownAddress);

	//If the variable gets changed, detect.
	if (iCurrentSkillCooldownValue != iCabalDefaultSkillCooldown)
	{
		std::stringstream ss;
		ss << "Cooldown: " << iCurrentSkillCooldownValue;
		std::string sDetectedValue = ss.str();
		ss.str("");
		ss << "Cooldown: " << iCabalDefaultSkillCooldown;
		std::string sDefaultValue = ss.str();
		ss.str("");
		funcCallbackHandler("CABAL SKILL COOLDOWN", "X", sDetectedValue, sDefaultValue);
		return true;
	}
	return false;
}

bool Virtual_Memory_Protection_Cabal_Online::VMP_CheckNation()
{
	//Get nation value
	int iCurrentNationValue = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalNationOffset);

	//Check if its the GM nation
	if (iCurrentNationValue == iCabalGm)
	{
		std::stringstream ss;
		ss << "Nation: " << iCurrentNationValue;
		std::string sDetectedValue = ss.str();
		ss.str("");
		ss << "Nation: " << iCabalGm;
		std::string sDefaultValue = ss.str();
		funcCallbackHandler("CABAL MODULE ADDRESS", "NATION OFFSET", sDetectedValue, sDefaultValue);
	}
	return false;
}

//UNDER CONSTRUCTION
bool Virtual_Memory_Protection_Cabal_Online::VMP_CheckPerfectCombo()
{
	//Skip algorithm if animation var is default
	if (GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalAnimationSkillOffset) == iCabalSkillAnimationDefaultValue)
	{
		return false;
	}

	//Check if its the first run, collect combo values
	if (iCabalLatestComboValue1 == 0 && iCabalLatestComboValue2 == 0 && iCabalLatestComboValue3 == 0 &&
		iCabalLatestComboValue4 == 0 && iCabalLatestComboValue5 == 0)
	{
		iCabalLatestComboValue1 = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalComboOffset1);
		iCabalLatestComboValue2 = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalComboOffset2);
		iCabalLatestComboValue3 = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalComboOffset3);
		iCabalLatestComboValue4 = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalComboOffset4);
		iCabalLatestComboValue5 = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalComboOffset5);
		iCabalLatestComboValue6 = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalComboOffset6);
	}

	//Collect current combo values to check, if there is a combo started
	int iCurrentComboValue1 = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalComboOffset1);
	int iCurrentComboValue2 = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalComboOffset2);
	int iCurrentComboValue3 = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalComboOffset3);
	int iCurrentComboValue4 = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalComboOffset4);
	int iCurrentComboValue5 = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalComboOffset5);
	int iCurrentComboValue6 = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalComboOffset6);

	if (iCurrentComboValue1 != iCabalLatestComboValue1 && iCurrentComboValue2 != iCabalLatestComboValue2 && iCurrentComboValue3 != iCabalLatestComboValue3 &&
		iCurrentComboValue4 != iCabalLatestComboValue4 && iCurrentComboValue5 != iCabalLatestComboValue5 && iCurrentComboValue6 != iCabalLatestComboValue6)
	{
		//Wait for hack to reset data, if applied
		Sleep(iNctWaitAfterSkillChange);

		//Collect data again
		int iCurrentComboValue1 = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalComboOffset1);
		int iCurrentComboValue2 = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalComboOffset2);
		int iCurrentComboValue3 = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalComboOffset3);
		int iCurrentComboValue4 = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalComboOffset4);
		int iCurrentComboValue5 = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalComboOffset5);
		int iCurrentComboValue6 = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalComboOffset6);

		//Write vars to map
		SumUpIndividualKeysInMap(&PerfectComboMap1, iCurrentComboValue1);
		SumUpIndividualKeysInMap(&PerfectComboMap2, iCurrentComboValue2);
		SumUpIndividualKeysInMap(&PerfectComboMap3, iCurrentComboValue3);
		SumUpIndividualKeysInMap(&PerfectComboMap4, iCurrentComboValue4);
		SumUpIndividualKeysInMap(&PerfectComboMap5, iCurrentComboValue5);
		SumUpIndividualKeysInMap(&PerfectComboMap6, iCurrentComboValue6);

		//Patch latest vars
		iCabalLatestComboValue1 = iCurrentComboValue1;
		iCabalLatestComboValue2 = iCurrentComboValue2;
		iCabalLatestComboValue3 = iCurrentComboValue3;
		iCabalLatestComboValue4 = iCurrentComboValue4;
		iCabalLatestComboValue5 = iCurrentComboValue5;
		iCabalLatestComboValue6 = iCurrentComboValue6;

		//Check map
		unsigned int iDetectedKey = 0;
		if (ValuesInMapReachedUpperLimit(&PerfectComboMap1, iPerfectComboDetectionTolerance, &iDetectedKey) ||
			ValuesInMapReachedUpperLimit(&PerfectComboMap2, iPerfectComboDetectionTolerance, &iDetectedKey) ||
			ValuesInMapReachedUpperLimit(&PerfectComboMap3, iPerfectComboDetectionTolerance, &iDetectedKey) ||
			ValuesInMapReachedUpperLimit(&PerfectComboMap4, iPerfectComboDetectionTolerance, &iDetectedKey) ||
			ValuesInMapReachedUpperLimit(&PerfectComboMap5, iPerfectComboDetectionTolerance, &iDetectedKey) ||
			ValuesInMapReachedUpperLimit(&PerfectComboMap6, iPerfectComboDetectionTolerance, &iDetectedKey))
		{
			std::stringstream ss;
			ss << ">= " << iPerfectComboDetectionTolerance;
			funcCallbackHandler("CABAL MODULE ADDRESS", "PCO", std::to_string(iDetectedKey), ss.str());
		}

		//Cleanup
		CleanUpMapIfSizeIsReached(&PerfectComboMap1, iPerfectComboQueueSize);
		CleanUpMapIfSizeIsReached(&PerfectComboMap2, iPerfectComboQueueSize);
		CleanUpMapIfSizeIsReached(&PerfectComboMap3, iPerfectComboQueueSize);
		CleanUpMapIfSizeIsReached(&PerfectComboMap4, iPerfectComboQueueSize);
		CleanUpMapIfSizeIsReached(&PerfectComboMap5, iPerfectComboQueueSize);
		CleanUpMapIfSizeIsReached(&PerfectComboMap6, iPerfectComboQueueSize);
	}
	return false;
}

bool Virtual_Memory_Protection_Cabal_Online::VMP_CheckFbBm1Freeze()
{

	int iCurrentBattleModeState = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalBattleModeStateOffset);
	Sleep(20);
	int iCurrentBattlemodeStateB = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalBattelModeStateBOffset);;

	if (iCurrentBattleModeState != 16 && iCurrentBattlemodeStateB == 3)
	{
		std::stringstream ss;
		ss << "State B: " << iCurrentBattlemodeStateB;
		funcCallbackHandler("CABAL MODULE ADDRESS", "FBBM1F", "<> 3", ss.str());
	}

	return false;
}

bool Virtual_Memory_Protection_Cabal_Online::VMP_CheckBmCooldownReset()
{
	//BM1
	if (VMP_Private_Abstract_CheckBmCooldownReset(&iCabalLatestBattleMode1CdValue,
		lpcvCabalBaseAddress, lpcvCabalBM1Offset1, lpcvCabalBM1Offset2, lpcvCabalBM1Offset3,
		&ctCabalBM1Timer, &bBm1IsRunning, &bBm1RecastException, &iLatestAnimationValueForBM1))
	{

		funcCallbackHandler("CABAL BASE ADDRESS", "BMCD1", "", "");
		return true;
	}
	//BM2
	if (VMP_Private_Abstract_CheckBmCooldownReset(&iCabalLatestBattleMode2CdValue,
		lpcvCabalBaseAddress, lpcvCabalBM1Offset1, lpcvCabalBM2Offset2, lpcvCabalBM1Offset3,
		&ctCabalBM2Timer, &bBm2IsRunning, &bBm2RecastException, &iLatestAnimationValueForBM2))
	{

		funcCallbackHandler("CABAL BASE ADDRESS", "BMCD2", "", "");
		return true;
	}
	//AURA
	if (VMP_Private_Abstract_CheckBmCooldownReset(&iCabalLatestAuraCdValue,
		lpcvCabalBaseAddress, lpcvCabalBM1Offset1, lpcvCabalAuraOffset2, lpcvCabalBM1Offset3,
		&ctCabalAuraTimer, &bAuraIsRunning, &bAuraRecastException, &iLatestAnimationValueForAura))
	{

		funcCallbackHandler("CABAL BASE ADDRESS", "ACD", "", "");
		return true;
	}
	
	return false;
}

bool Virtual_Memory_Protection_Cabal_Online::VMP_CheckKillGate()
{
	//Skip algorithm, if MapValue is on default
	if (GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalMapOffset) == iCabalMapDefaultValue)
	{
		return false;
	}

	if (iCabalLatestMapValue == -1)
	{
		iCabalLatestMapValue = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalMapOffset);
	}

	int iCurrentMapValue = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalMapOffset);
	int iCurrentNoSkillDelayValue = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalSkillDelayOffset);

	LPCVOID TestAdress = (LPCVOID)((unsigned int)0x400000 + (unsigned int)0x793530);

	const char* test = ReadMemoryString(hProcessHandle, lpcvCabalBaseAddress);
	if (iCurrentMapValue != iCabalLatestMapValue)
	{
		if (iCurrentNoSkillDelayValue < iCabalSkillValueLowerLimit)
		{
			funcCallbackHandler("CABAL BASE ADDRESS", "KG", std::to_string(iCurrentMapValue), std::to_string(iCabalLatestMapValue));
			return true;
		}
		else
		{
			iCabalLatestMapValue = iCurrentMapValue;
		}
	}

	return false;
}

bool Virtual_Memory_Protection_Cabal_Online::VMP_CheckFbDame()
{
	int iCurrentBattleModeState = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalBattleModeStateOffset);
	int iCurrentSkillDelayValue = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalSkillDelayOffset);

	if (iCurrentBattleModeState != 16 && iCurrentBattleModeState != 32 && iCurrentBattleModeState != 8 && iCurrentBattleModeState != 24 && iCurrentBattleModeState != 40 && iCurrentSkillDelayValue < iCabalSkillValueLowerLimit)
	{
		int iCurrentBattleModeStateB = GetIntViaLevel1Pointer(lpcvCabalBaseAddress, lpcvCabalBattelModeStateBOffset);
		
		if (iCurrentBattleModeStateB >= iFbDameBattleModeStateBAnormaly)
		{
			
			FbDameVector.push_back(iCurrentBattleModeStateB);

			//Collect anormalies in map
			std::vector<unsigned int>::iterator iIt;
			unsigned int iAnomalyApperances = 0;

			for (iIt = FbDameVector.begin(); iIt != FbDameVector.end(); iIt++)
			{
				unsigned int& iItData(*iIt);
				if (iItData > iFbDameBattleModeStateBAnormaly)
				{
					iAnomalyApperances++;
					Sleep(iFbDameAnormalyWaitTime);
				}
			}

			//Check if anormalies are too high
			if (iAnomalyApperances > iFbDameDetectionTolerance)
			{
				std::stringstream ss;
				ss << ">= " << iFbDameDetectionTolerance;
				std::string sDefault = ss.str();
				ss.str("");
				ss << FbDameVector.back();
				std::string sDetected = ss.str();
				funcCallbackHandler("CABAL MODULE ADDRESS", "FBDAME", sDetected, sDefault);
			}

			//Cleanup vector if too large
			if (FbDameVector.size() >= iFbDameVectorQueueSize)
			{
				/*std::ofstream filestr;
				filestr.open(".\\FbDameMap.txt", std::fstream::in | std::fstream::out | std::fstream::app);
				std::vector<unsigned int>::iterator iIt;
				for (iIt = NsdVector.begin(); iIt != NsdVector.end(); iIt++)
				{
					unsigned int& iItData(*iIt);
					filestr << iItData << std::endl;
				}
				filestr << "-----------------------------" << iCurrentBattleModeState << std::endl;
				filestr.close();*/
				FbDameVector.clear();
			}

		}
	}



	return false;
}

//VMP Tests

bool Virtual_Memory_Protection_Cabal_Online::VMP_EnableWallHack()
{
	//Iterate through the Wall adresses
	//Get Start address of wall based address room
	LPCVOID StartAdress = (LPCVOID)(ReadMemoryInt(hProcessHandle, lpcvCabalWallBaseAddress) + (unsigned int)lpcvCabalWallStartOffset);

	//Iterate throu the address room. Do i + 4 steps to get the right addresses. Set them to 0, to set the walls down
	for (unsigned int i = 0; i < (unsigned int)lpcvCabalWallStopOffset; i = i + 4)
	{
		WriteIntToMemory(hProcessHandle, (LPCVOID)((unsigned int)StartAdress + i), 0);
	}
	return false;
}


//Private
//TEST
//void Virtual_Memory_Protection_Cabal_Online::WriteMemoryValueAsync(void* Param)
//{
//
//	Virtual_Memory_Protection_Cabal_Online* This = (Virtual_Memory_Protection_Cabal_Online*)Param;
//	
//	std::clock_t start;
//	double dDuration = 0;
//	start = std::clock();
//	do
//	{
//		This->WriteIntToMemory(This->hProcessHandle, (LPCVOID)0x0f12f2ec, 1);
//		dDuration = (std::clock() - start) / (double)CLOCKS_PER_SEC;
//	} while (dDuration < 0.1);
//	
//}