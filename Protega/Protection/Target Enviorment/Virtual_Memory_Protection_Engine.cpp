#include "../../stdafx.h"
#include "Virtual_Memory_Protection_Engine.h"

Virtual_Memory_Protection_Engine::Virtual_Memory_Protection_Engine(unsigned int iProcessID)
{
	this->iProcessID = iProcessID;
	hProcessHandle = NULL;
	ActualAddress = NULL;
	iProcessBaseAddress = 0;
}

Virtual_Memory_Protection_Engine::Virtual_Memory_Protection_Engine(unsigned int iProcessID, std::list<std::pair<unsigned int, unsigned int>> kvpAddressList,
	std::list<std::pair<const char*, unsigned int>> kvpValueInformation,
	std::function<void(std::list<std::pair<unsigned int, unsigned int>>::iterator kvpDetectedAddress, const char* sActualValue, const char* sWantedCondition)> funcCallbackHandler)
{
	this->iProcessID = iProcessID;
	this->kvpAddressList = kvpAddressList;
	this->kvpValueInformation = kvpValueInformation;
	hProcessHandle = NULL;
	ActualAddress = NULL;
	iProcessBaseAddress = 0;
}

Virtual_Memory_Protection_Engine::~Virtual_Memory_Protection_Engine()
{
	CloseProcessInstance();
}

//Public
bool Virtual_Memory_Protection_Engine::OpenProcessInstance()
{
	//Get Process Handle
	hProcessHandle = OpenProcess(PROCESS_ALL_ACCESS, false, iProcessID);
	if (!hProcessHandle) {
		return false;
	}
	return true;
}

bool Virtual_Memory_Protection_Engine::CloseProcessInstance()
{	
	return CloseHandle(hProcessHandle);
}

void Virtual_Memory_Protection_Engine::ScanAllAddresses()
{
	std::list<std::pair<unsigned int, unsigned int>>::iterator itAddress;
	std::list<std::pair<const char*, unsigned int>>::iterator itValueInformation = kvpValueInformation.begin();
	unsigned iIndex = 0;


	for (itAddress = kvpAddressList.begin(); itAddress != kvpAddressList.end(); ++itAddress)
	{
		//Collect data from lists
		unsigned int iCurrentAddress = itAddress->first;
		unsigned int iCurrentOffset = itAddress->second;

		std::string sValue = itValueInformation->first;
		unsigned int sType = itValueInformation->second;
		ActualAddress = (LPCVOID)(iCurrentAddress + iCurrentOffset);


		//Read memory value ( the right value. Check which read function is needed
		switch (sType)
		{
		case eMemoryDataTypes::Integer:
		{
			int iTargetValue = GetIntViaLevel2Pointer((LPCVOID)iCurrentAddress, (LPCVOID)iCurrentOffset);
			//Example Check token + definition (Adding the pair list for this later..)
			if (!isValid(iTargetValue, eValueCheckToken::EQL, ""))
			{
				funcCallbackHandler(itAddress, std::to_string(iTargetValue).c_str(), "");
				return;
			}
			break;
		}			
		case eMemoryDataTypes::Float:
		{
			float fTargetValue = GetFloatViaLevel2Pointer((LPCVOID)iCurrentAddress, (LPCVOID)iCurrentOffset);
			//Example Check token + definition (Adding the pair list for this later..)
			if (!isValid(fTargetValue, eValueCheckToken::EQL, "450.0f"))
			{
				funcCallbackHandler(itAddress, std::to_string(fTargetValue).c_str(), "450.0f");
				return;
			}
		}			
		default:
			break;
		}
		//Iterate also the other lists up to get the right information next run
		itValueInformation = std::next(kvpValueInformation.begin(), ++iIndex);
	}
}

int Virtual_Memory_Protection_Engine::GetIntViaLevel2Pointer(LPCVOID BaseAddress, LPCVOID Offset)
{
	unsigned int iOffset = (int)Offset;
	unsigned int iBAValueAddress = ReadMemoryInt(hProcessHandle, BaseAddress);

	return ReadMemoryInt(hProcessHandle, (LPCVOID)(iBAValueAddress + iOffset));
}

float Virtual_Memory_Protection_Engine::GetFloatViaLevel2Pointer(LPCVOID BaseAddress, LPCVOID Offset)
{
	unsigned int iOffset = (int)Offset;
	unsigned int iBAValueAddress = ReadMemoryInt(hProcessHandle, BaseAddress);

	return ReadMemoryFloat(hProcessHandle, (LPCVOID)(iBAValueAddress + iOffset));
}

const char * Virtual_Memory_Protection_Engine::GetStringViaLevel2Pointer(LPCVOID BaseAddress, LPCVOID Offset)
{
	return nullptr;
}

//Private

bool Virtual_Memory_Protection_Engine::isValid(int iTargetValue, unsigned int iCheckToken, const char * sCheckDefinition)
{
	switch (iCheckToken)
	{
	case eValueCheckToken::EQL:
		return (iTargetValue == atoi(sCheckDefinition)) ? true : false;
	default:
		break;
	}
	return false;
}

bool Virtual_Memory_Protection_Engine::isValid(float fTargetValue, unsigned int iCheckToken, const char * sCheckDefinition)
{
	switch (iCheckToken)
	{
	case eValueCheckToken::EQL:
		return (fTargetValue == atof(sCheckDefinition)) ? true : false;
	default:
		break;
	}
	return false;
}

bool Virtual_Memory_Protection_Engine::isValid(const char * sTargetValue, unsigned int iCheckToken, const char * sCheckDefinition)
{
	return false;
}



DWORD_PTR Virtual_Memory_Protection_Engine::dwGetModuleBaseAddress(DWORD dwProcID, TCHAR *szModuleName)
{
	DWORD_PTR dwModuleBaseAddress = 0;
	HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE | TH32CS_SNAPMODULE32, dwProcID);
	if (hSnapshot != INVALID_HANDLE_VALUE)
	{
		MODULEENTRY32 ModuleEntry32;
		ModuleEntry32.dwSize = sizeof(MODULEENTRY32);
		if (Module32First(hSnapshot, &ModuleEntry32))
		{
			do
			{
				if (_tcsicmp(ModuleEntry32.szModule, szModuleName) == 0)
				{
					dwModuleBaseAddress = (DWORD_PTR)ModuleEntry32.modBaseAddr;
					break;
				}
			} while (Module32Next(hSnapshot, &ModuleEntry32));
		}
		CloseHandle(hSnapshot);
	}
	return dwModuleBaseAddress;
}

//	ReadMemory functions
int Virtual_Memory_Protection_Engine::ReadMemoryInt(HANDLE processHandle, LPCVOID address)
{
	int buffer = 0;
	SIZE_T NumberOfBytesToRead = sizeof(buffer); //this is equal to 4
	SIZE_T NumberOfBytesActuallyRead;
	BOOL err = ReadProcessMemory(processHandle, address, &buffer, NumberOfBytesToRead, &NumberOfBytesActuallyRead);
	if (err || NumberOfBytesActuallyRead != NumberOfBytesToRead)
		/*an error occured*/;
	return buffer;
}

float Virtual_Memory_Protection_Engine::ReadMemoryFloat(HANDLE processHandle, LPCVOID address) {
	float buffer = 0;
	SIZE_T NumberOfBytesToRead = sizeof(buffer); //this is equal to 4
	SIZE_T NumberOfBytesActuallyRead;
	BOOL err = ReadProcessMemory(processHandle, address, &buffer, NumberOfBytesToRead, &NumberOfBytesActuallyRead);
	if (err || NumberOfBytesActuallyRead != NumberOfBytesToRead)
		/*an error occured*/;
	return buffer;
}