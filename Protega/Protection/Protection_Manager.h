#pragma once
#include "Target Enviorment\Virtual_Memory_Protection_Cabal_Online.h"
#include "Target Enviorment\Heuristic_Scan_Engine.h"
#include "Target Enviorment\File_Protection_Engine.h"
#include "../Core/Exception_Manager.h"

class Protection_Manager
{
private:
	//Var
	bool bProtectionIsRunning = false;
	int iTargetProcessId = 0;
	int iVMErrorCode = 0;
	int iFPErrorCode = 0;
	int iThreadErrorCode = 0;

	std::thread* tHeThread;
	std::thread* tVmpThread;
	std::thread* tFpThread;

	double dThreadResponseDelta;
	std::clock_t ctMainThreadResponse = 0;
	std::clock_t ctHeResponse = 0;
	std::clock_t ctVmpResponse = 0;
	std::clock_t ctFpResponse = 0;

	std::function<void(unsigned int iType, std::vector<std::string> lDetectionInformation)> funcCallbackHandler;

	//Classes
	Heuristic_Scan_Engine* HE;
	Virtual_Memory_Protection_Cabal_Online* VMP;
	File_Protection_Engine* FP;

	//Functions
	//	Threads
	void VMP_Thread();
	void HE_Thread();
	void FP_Thread();
	//	Callbacks
	void HE_Callback(std::string sSection, std::string sDetectionValue);
	void VMP_Callback(std::string sDetectedBaseAddress, std::string sDetectedOffset, std::string sDetectedValue, std::string sDefaultValue);
	void FP_Callback(std::string sSection, std::string sDetectionValue);
	// Test

	// Normal functions
	int GetProcessIdByName(char* ProcName);
	void StringToWString(std::string sStringToConvert, std::wstring* wsOutput);
public:
	Protection_Manager(std::function<void(unsigned int iType, std::vector<std::string> lDetectionInformation)> funcCallbackHandler,
		int iTargetApplicationId,
		double dThreadResponseDelta,
		int iVMErrorCode,
		int iFPErrorCode,
		int iThreadErrorCode,
		int iFPMaxDlls,
		std::string sBaseFolder,
		std::vector<std::wstring> vBlackListProcessNames,
		std::vector<std::string> vBlackListWindowNames,
		std::vector<std::string> vBlackListClassNames,
		std::vector<std::string> vBlackListMd5Values, 
		std::pair<std::vector<std::string>, std::vector<std::string>> pFilesAndMd5);
	//Functions
	//	User
	bool StartProtectionThreads();
	//	System
	bool ProtectionIsRunning();
	std::clock_t* GetMainThreadClock();
	bool CheckClocks(std::clock_t* ctOwnClock);


	~Protection_Manager();

	
	
};

