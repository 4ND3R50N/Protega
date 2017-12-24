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

void Virtual_Memory_Protection_Cabal_Online::CheckAllVmpFunctions()
{

}

//VMP Functions
bool Virtual_Memory_Protection_Cabal_Online::VMP_CheckGameSpeed()
{
	//Check if we are currently on a map
	if (GetIntViaLevel2Pointer(lpcvCabalBaseAddress, lpcvCabalMapOffset) != iCabalMapDefaultValue)
	{
		//Check if the speed is normal (450)
		float fCurrentSpeed = GetFloatViaLevel2Pointer(lpcvCabalBaseAddress, lpcvCabalSpeedOffset);
		if (fCurrentSpeed != fCabalNormalSpeed)
		{
			//Function callback triggern
			funcCallbackHandler("CABAL BASE ADDRESS", "SPEED OFFSET", boost::lexical_cast<std::string>(fCurrentSpeed), boost::lexical_cast<std::string>(fCabalNormalSpeed));
			return true;
		}
	}
	return false;
}

bool Virtual_Memory_Protection_Cabal_Online::VMP_CheckWallBorders()
{
	//Iterate through the Wall adresses
	int iZeros = 0;
	int iNonZeros = 0;
	LPCVOID StartAdress = (LPCVOID)(ReadMemoryInt(hProcessHandle, lpcvCabalWallBaseAddress) + (unsigned int)lpcvCabalWallStartOffset);
	
	//std::ofstream file("Wallhack test.txt");
	
	for (unsigned int i = 0; i < (unsigned int)lpcvCabalWallStopOffset; i = i + 4)
	{
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

	//This decision must be adjusted later!!!
	if (dPercentOfZeros == 100.0)
	{
		funcCallbackHandler("CABAL WALL BASE ADDRESS", "WALL START OFFSET", "100 % ZEROS", "NOT 100 %");
		return true;
	}

	//file << "Amount of all Addresses = " << iAmountOfAllAddresses << " | % of all Zeros = " << dPercentOfZeros << " | % of all non Zeros = " << dPercentOfNonZeros;
	//file.close();	
	return false;
}

//HAS TO BE TESTED
bool Virtual_Memory_Protection_Cabal_Online::VMP_CheckZoomState()
{
	//Maybe we need here a map check later...

	int iZoom1 = ReadMemoryInt(hProcessHandle, (LPCVOID)((unsigned int)lpcvCabalModuleAddress + (unsigned int)lpcvCabalZoomOffset1));
	int iZoom2 = ReadMemoryInt(hProcessHandle, (LPCVOID)((unsigned int)lpcvCabalModuleAddress + (unsigned int)lpcvCabalZoomOffset2));

	if (iZoom1 != iCabalDefaultZoom && iZoom2 != iCabalDefaultZoom)
	{
		std::stringstream ss;
		ss << "Zoom1: " << iZoom1 << " | Zoom2: " << iZoom2;
		std::string sDetectedValue = ss.str();
		ss.clear();
		ss << "Default Zoom 1/2: " << iCabalDefaultZoom;
		std::string sDefaultValue = ss.str();
		ss.clear();

		funcCallbackHandler("CABAL MODULE ADDRESS", "ZOOM OFFSET 1 AND 2", sDetectedValue, sDefaultValue);
		return true;
	}

	if (iZoom1 != iCabalDefaultZoom)
	{
		std::stringstream ss;
		ss << "Zoom1: " << iZoom1;
		std::string sDetectedValue = ss.str();
		ss.clear();
		ss << "Default Zoom 1/2: " << iCabalDefaultZoom;
		std::string sDefaultValue = ss.str();
		ss.clear();
		funcCallbackHandler("CABAL MODULE ADDRESS", "ZOOM OFFSET 1", sDetectedValue, sDefaultValue);
		return true;
	}

	if (iZoom2 != iCabalDefaultZoom)
	{
		std::stringstream ss;
		ss << "Zoom2: " << iZoom2;
		std::string sDetectedValue = ss.str();
		ss.clear();
		ss << "Default Zoom 1/2: " << iCabalDefaultZoom;
		std::string sDefaultValue = ss.str();
		ss.clear();
		funcCallbackHandler("CABAL MODULE ADDRESS", "ZOOM OFFSET 2", sDetectedValue, sDefaultValue);
		return true;
	}

	return false;
}

bool Virtual_Memory_Protection_Cabal_Online::VMP_CheckNoSkillAnimation()
{
	LPCVOID SkillCastAddress = GetAddressOfLevel2Pointer(lpcvCabalBaseAddress, lpcvCabalSkillCastOffset);
	//CreateThread(NULL, NULL, LPTHREAD_START_ROUTINE(WriteMemoryValueAsync), (void*)this, 0, 0);

	int iCurrentAnimationValue = GetIntViaLevel2Pointer(lpcvCabalBaseAddress, lpcvCabalAnimationOffset);

	if (iCurrentAnimationValue == iCabalAnimationSkill)
	{
		int iCurrentSkillCastValue = ReadMemoryInt(hProcessHandle, SkillCastAddress);
		if (iCurrentSkillCastValue < iCabalSkillValueLowerLimit)
		{
			std::stringstream ss;
			ss << iCurrentSkillCastValue;
			funcCallbackHandler("CABAL MODULE ADDRESS", "SKILL CAST OFFSET", ">3000000", ss.str());
			return true;
		}
	}
	return false;
}

//VMP Tests
bool Virtual_Memory_Protection_Cabal_Online::VMP_EnableWallHack()
{
	//Iterate through the Wall adresses
	LPCVOID StartAdress = (LPCVOID)(ReadMemoryInt(hProcessHandle, lpcvCabalWallBaseAddress) + (unsigned int)lpcvCabalWallStartOffset);

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


