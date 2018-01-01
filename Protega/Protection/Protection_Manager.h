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

	std::thread* tTestThread;

	double dThreadResponseDelta;
	std::clock_t ctHeResponse;
	std::clock_t ctVmpResponse;
	std::clock_t ctFpResponse;

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
	// Normal functions
	bool CheckClocks(std::clock_t* ctOwnClock);
	int GetProcessIdByName(char* ProcName);
public:
	Protection_Manager();
	Protection_Manager(std::string sTargetApplication, 
		double dThreadResponseDelta,
		std::list<std::wstring> lBlackListProcessNames,
		std::list<std::string> lBlackListWindowNames,
		std::list<std::string> lBlackListClassNames,
		std::list<std::string> lBlackListMd5Values);
	bool StartProtectionThreads();
	~Protection_Manager();
};

