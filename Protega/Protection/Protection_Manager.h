#pragma once
#include "Target Enviorment\Virtual_Memory_Protection_Cabal_Online.h"
#include "Target Enviorment\Heuristic_Scan_Engine.h"
#include "Target Enviorment\File_Protection_Engine.h"

class Protection_Manager
{
private:
	//Var
	bool iProtectionIsRunning = false;
	int iTargetProcessId;

	std::thread* tHeThread;
	std::thread* tVmpThread;
	std::thread* tFpThread;

	double dThreadResponseDelta;
	std::clock_t ctMainThreadResponse = 0;
	std::clock_t ctHeResponse = 0;
	std::clock_t ctVmpResponse = 0;
	std::clock_t ctFpResponse = 0;

	std::function<void(std::list<std::wstring> lDetectionInformation)> funcCallbackHandler;

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
	void HE_Callback(std::wstring sDetectionValue);
	void VMP_Callback(std::string sDetectedBaseAddress, std::string sDetectedOffset, std::string sDetectedValue, std::string sDefaultValue);
	// Test

	// Normal functions
	int GetProcessIdByName(char* ProcName);
	void StringToWString(std::string sStringToConvert, std::wstring* wsOutput);
public:
	Protection_Manager(std::string sTargetApplication, double dThreadResponseDelta);
	Protection_Manager(std::function<void(std::list<std::wstring> lDetectionInformation)> funcCallbackHandler,
		std::string sTargetApplication,
		double dThreadResponseDelta,
		std::list<std::wstring> lBlackListProcessNames,
		std::list<std::string> lBlackListWindowNames,
		std::list<std::string> lBlackListClassNames,
		std::list<std::string> lBlackListMd5Values);
	//Functions
	//	User
	bool StartProtectionThreads();
	//	System
	std::clock_t* GetMainThreadClock();
	bool CheckClocks(std::clock_t* ctOwnClock);


	~Protection_Manager();

	
	
};

