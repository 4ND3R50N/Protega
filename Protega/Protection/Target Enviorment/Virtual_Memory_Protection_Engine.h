#pragma once
#include "../../stdafx.h"
#include <Windows.h>
#include <list>
#include <thread>
#include <tlhelp32.h>
#include <tchar.h>
#include <string>
#include <iostream>

class Virtual_Memory_Protection_Engine
{
private:
	//Data container
	std::list<std::pair<unsigned int, unsigned int>> kvpAddressList;
	std::list<std::pair<const char*, unsigned int>> kvpValueInformation;
	enum eMemoryDataTypes
	{
		Integer = 1,
		Float = 2
	};
	enum eValueCheckToken
	{
		EQL = 1,
		HIGHER = 2,
		LOWER = 3,
		BETWEEN = 4
	};
	
	//Vars
	HANDLE hProcessHandle;
	unsigned int iProcessID;
	LPCVOID ActualAddress;
	
	//Functions Vars
	std::function<void(std::list<std::pair<unsigned int, unsigned int>>::iterator kvpDetectedAddress,
		const char* sActualValue, const char* sWantedCondition)> funcCallbackHandler;

    //This will be used later
	unsigned int iProcessBaseAddress;

	//Functions
	//	Read Memory functions
	int ReadMemoryInt(HANDLE processHandle, LPCVOID address);
	float ReadMemoryFloat(HANDLE processHandle, LPCVOID address);
	//	Other functions
	bool isValid(int iTargetValue, unsigned int iCheckToken, const char* sCheckDefinition);
	bool isValid(float fTargetValue, unsigned int iCheckToken, const char* sCheckDefinition);
	bool isValid(const char* sTargetValue, unsigned int iCheckToken, const char* sCheckDefinition);
	DWORD_PTR dwGetModuleBaseAddress(DWORD dwProcID, TCHAR *szModuleName);

public:
	//There is a third kvp list missing: the definitions of how the values has to be tested
	Virtual_Memory_Protection_Engine(unsigned int iProcessID);
	Virtual_Memory_Protection_Engine(unsigned int iProcessID, std::list<std::pair<unsigned int, unsigned int>> kvpAddressList, 
		std::list<std::pair<const char*, unsigned int>> kvpValueInformation,
		std::function<void(std::list<std::pair<unsigned int, unsigned int>>::iterator kvpDetectedAddress, const char* sActualValue, const char* sWantedCondition)> funcCallbackHandler);
	bool OpenProcessInstance();
	bool CloseProcessInstance();
	void ScanAllAddresses();
	int GetIntViaLevel2Pointer(LPCVOID BaseAddress, LPCVOID Offset);
    float GetFloatViaLevel2Pointer(LPCVOID BaseAddress, LPCVOID Offset);
	const char* GetStringViaLevel2Pointer(LPCVOID BaseAddress, LPCVOID Offset);

	~Virtual_Memory_Protection_Engine();
};

