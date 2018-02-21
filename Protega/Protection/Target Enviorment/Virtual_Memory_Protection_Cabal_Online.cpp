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

bool Virtual_Memory_Protection_Cabal_Online::DetectManipulatedMemory()
{
	if ((VMP_CheckGameSpeed() || VMP_CheckWallBorders() || VMP_CheckZoomState() || VMP_CheckNoSkillDelay()  || 
		VMP_CheckNoCastTime() || VMP_CheckSkillRange() || VMP_CheckSkillCooldown() /*|| VMP_CheckNation()*/) == true)
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
	if (GetIntViaLevel2Pointer(lpcvCabalBaseAddress, lpcvCabalMapOffset) != iCabalMapDefaultValue)
	{
		//Check if the speed is normal (450)
		float fCurrentSpeed = GetFloatViaLevel2Pointer(lpcvCabalBaseAddress, lpcvCabalSpeedOffset);
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
	if (GetIntViaLevel2Pointer(lpcvCabalBaseAddress, lpcvCabalMapOffset) == iCabalMapDefaultValue)
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
		ss << "NOT > " << iWallhackZeroTolerance << "%";
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
//HAS TO BE TESTED
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
	if (GetIntViaLevel2Pointer(lpcvCabalBaseAddress, lpcvCabalAnimationSkillOffset) == iCabalSkillAnimationDefaultValue)
	{
		return false;
	}

	//Get Skill Cast address to write them later
	LPCVOID SkillCastAddress = GetAddressOfLevel2Pointer(lpcvCabalBaseAddress, lpcvCabalSkillCastOffset);

	//CreateThread(NULL, NULL, LPTHREAD_START_ROUTINE(WriteMemoryValueAsync), (void*)this, 0, 0);
	
	//Get the actual animation value
	int iCurrentAnimationValue = GetIntViaLevel2Pointer(lpcvCabalBaseAddress, lpcvCabalAnimationOffset);

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
			funcCallbackHandler("CABAL MODULE ADDRESS", "SKILL CAST OFFSET", ss.str(), ">3000000");
			return true;
		}
	}
	return false;
}

//NOTE: Set sleep vars global! Algorithm speed: 100ms! Gamestart + freeze NCT = 0 -> Makes some problems. Check it later!
//NOTE: Map or animation check is necessary.... maybe...
bool Virtual_Memory_Protection_Cabal_Online::VMP_CheckNoCastTime()
{
	//Skip algorithm if animation var is default
	if (GetIntViaLevel2Pointer(lpcvCabalBaseAddress, lpcvCabalAnimationSkillOffset) == iCabalSkillAnimationDefaultValue)
	{
		return false;
	}

	//Check if its the first run, collect NSD + NCT Value
	if (iCabalLatestNoCastTimeValue == 0 && iCabalLatestCastValue == 0)
	{
		iCabalLatestNoCastTimeValue = GetIntViaLevel2Pointer(lpcvCabalBaseAddress, lpcvCabalNoCastTimeOffset);
		iCabalLatestCastValue = GetIntViaLevel2Pointer(lpcvCabalBaseAddress, lpcvCabalSkillCastOffset);
		//BM Fix
		iCabalLatestBattleModeStateValue = GetIntViaLevel2Pointer(lpcvCabalBaseAddress, lpcvCabalBattleModeStateOffset);
	}

	//Get actual NSD value
	int iCurrentSkillCastValue = GetIntViaLevel2Pointer(lpcvCabalBaseAddress, lpcvCabalSkillCastOffset);

	//Check, if there was a skill change after the last run of this algorithm
	if (iCurrentSkillCastValue != iCabalLatestCastValue)
	{
		//Collect the animation value
		int iCurrentAnimationValue = GetIntViaLevel2Pointer(lpcvCabalBaseAddress, lpcvCabalAnimationOffset);
		//Collect battle mode value
		int iCurrentBattleModeValue = GetIntViaLevel2Pointer(lpcvCabalBaseAddress, lpcvCabalBattleModeStateOffset);

		//Check, if we are currently performing an attack + check if the animation is also an attack + check if we havent changed the battle mode 
		if (iCurrentAnimationValue == iCabalAnimationSkill && iCurrentSkillCastValue >= iCabalSkillValueLowerLimit && iCurrentBattleModeValue == iCabalLatestBattleModeStateValue)
		{
			//This sleep exist for the following reason:
			//If you freeze the NCT var via hack, it rewrites the variable permanently. That means, the NCT is toggling between a random value + the value that is freezig.
			//There must be a smart delay, after the algorithm detected a skill change + recognized, that there is a attack performing (IF1 and IF2) to catch the var that the hack is freezing.
			//Since we store the latest NCT value, it will recognize -> a change, if there is no hack
			//														 -> no change, if there is a hack
			Sleep(200);

			//Get the actual NCT value
			int iCurrentNoCastValue = GetIntViaLevel2Pointer(lpcvCabalBaseAddress, lpcvCabalNoCastTimeOffset);
			
			//Check if the var did not change after a second attack skill
			if (iCabalLatestNoCastTimeValue == iCurrentNoCastValue)
			{
				funcCallbackHandler("CABAL MODULE ADDRESS", "NO CAST TIME OFFSET", ">3000000", "");
				return true;
			}
			else
			{
				iCabalLatestNoCastTimeValue = iCurrentNoCastValue;				
			}
		}
		iCabalLatestCastValue = iCurrentSkillCastValue;
		iCabalLatestBattleModeStateValue = iCabalLatestBattleModeStateValue;
	}

	return false;
}

bool Virtual_Memory_Protection_Cabal_Online::VMP_CheckSkillRange()
{
	//Check GM, GM range, GM AOE <> 0
	int iCabalCurrentGmValue = ReadMemoryInt(hProcessHandle, lpcvCabalGmAddress);
	int iCabalCurrentRangeValue = ReadMemoryInt(hProcessHandle, lpcvCabalRangeAddress);
	int iCabalCurrentAoeValue = ReadMemoryInt(hProcessHandle, lpcvCabalAoeAddress);

	if (iCabalCurrentGmValue != iCabalDefaultGM || iCabalCurrentRangeValue != iCabalDefaultRange || iCabalCurrentAoeValue != iCabalDefaultAOE)
	{
		std::stringstream ss;
		ss << "GM: " << iCabalCurrentGmValue << " | Range: " << iCabalCurrentRangeValue << " | Aoe: " << iCabalCurrentAoeValue;
		std::string sDetectedValues = ss.str();
		ss.str("");
		ss << "GM: " << iCabalDefaultGM << " | Range: " << iCabalDefaultRange << " | Aoe: " << iCabalDefaultAOE;
		std::string sDefaultValues = ss.str();
		ss.str("");
		funcCallbackHandler("GM | RANGE | AOE", "X", sDetectedValues, sDefaultValues);
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
	int iCurrentNationValue = GetIntViaLevel2Pointer(lpcvCabalBaseAddress, lpcvCabalNationOffset);

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