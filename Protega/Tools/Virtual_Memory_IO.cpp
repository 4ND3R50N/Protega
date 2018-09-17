#include "../stdafx.h"
#include "Virtual_Memory_IO.h"


Virtual_Memory_IO::Virtual_Memory_IO()
{
}

Virtual_Memory_IO::Virtual_Memory_IO(int iTargetApplicationId)
{
	this->hProcessHandle = OpenProcess(PROCESS_ALL_ACCESS, false, iTargetApplicationId);
}


Virtual_Memory_IO::~Virtual_Memory_IO()
{
}

//Public
int Virtual_Memory_IO::GetIntViaLevel1Pointer(LPCVOID BaseAddress, LPCVOID Offset)
{
	unsigned int iOffset = (int)Offset;
	unsigned int iBAValueAddress = ReadMemoryInt(hProcessHandle, BaseAddress);

	return ReadMemoryInt(hProcessHandle, (LPCVOID)(iBAValueAddress + iOffset));
}

float Virtual_Memory_IO::GetFloatViaLevel1Pointer(LPCVOID BaseAddress, LPCVOID Offset)
{
	unsigned int iOffset = (int)Offset;
	unsigned int iBAValueAddress = ReadMemoryInt(hProcessHandle, BaseAddress);

	return ReadMemoryFloat(hProcessHandle, (LPCVOID)(iBAValueAddress + iOffset));
}

const char * Virtual_Memory_IO::GetStringViaLevel1Pointer(LPCVOID BaseAddress, LPCVOID Offset)
{
	return nullptr;
}

int Virtual_Memory_IO::GetIntViaLevel3Pointer(LPCVOID BaseAddress, LPCVOID Offset1, LPCVOID Offset2, LPCVOID Offset3)
{
	return GetIntViaLevel1Pointer(GetAddressOfLevel1Pointer(GetAddressOfLevel1Pointer(BaseAddress, Offset1), Offset2), Offset3);
}

//Address Getter
LPCVOID Virtual_Memory_IO::GetAddressOfLevel1Pointer(LPCVOID BaseAddress, LPCVOID Offset)
{
	unsigned int iOffset = (int)Offset;
	unsigned int iBAValueAddress = ReadMemoryInt(hProcessHandle, BaseAddress);
	return (LPCVOID)(iBAValueAddress + iOffset);
}

//	ReadMemory functions
int Virtual_Memory_IO::ReadMemoryInt(HANDLE processHandle, LPCVOID address)
{
	int buffer = 0;
	SIZE_T NumberOfBytesToRead = sizeof(buffer); //this is equal to 4
	SIZE_T NumberOfBytesActuallyRead;
	BOOL err = ReadProcessMemory(processHandle, address, &buffer, NumberOfBytesToRead, &NumberOfBytesActuallyRead);
	//if (err || NumberOfBytesActuallyRead != NumberOfBytesToRead)
	/*an error occured*/;
	return buffer;
}

float Virtual_Memory_IO::ReadMemoryFloat(HANDLE processHandle, LPCVOID address) 
{
	float buffer = 0;
	SIZE_T NumberOfBytesToRead = sizeof(buffer); //this is equal to 4
	SIZE_T NumberOfBytesActuallyRead;
	BOOL err = ReadProcessMemory(processHandle, address, &buffer, NumberOfBytesToRead, &NumberOfBytesActuallyRead);
	//if (err || NumberOfBytesActuallyRead != NumberOfBytesToRead)
	/*an error occured*/;
	return buffer;
}

std::string Virtual_Memory_IO::ReadMemoryString(HANDLE processHandle, LPCVOID address, short iLength)
{
	char value[128];
	BOOL err = ReadProcessMemory(processHandle, address, &value, 128, 0);
	std::string sValue(value);
	return sValue;
}

bool Virtual_Memory_IO::WriteIntToMemory(HANDLE processHandle, LPCVOID address, int iValue)
{	
	return WriteProcessMemory(processHandle, (LPVOID)address, &iValue, sizeof(iValue), 0);
}
