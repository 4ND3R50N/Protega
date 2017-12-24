#include "../../stdafx.h"
#include "Virtual_Memory_IO.h"


Virtual_Memory_IO::Virtual_Memory_IO()
{
}


Virtual_Memory_IO::~Virtual_Memory_IO()
{
}

//Public
int Virtual_Memory_IO::GetIntViaLevel2Pointer(LPCVOID BaseAddress, LPCVOID Offset)
{
	unsigned int iOffset = (int)Offset;
	unsigned int iBAValueAddress = ReadMemoryInt(hProcessHandle, BaseAddress);

	return ReadMemoryInt(hProcessHandle, (LPCVOID)(iBAValueAddress + iOffset));
}

float Virtual_Memory_IO::GetFloatViaLevel2Pointer(LPCVOID BaseAddress, LPCVOID Offset)
{
	unsigned int iOffset = (int)Offset;
	unsigned int iBAValueAddress = ReadMemoryInt(hProcessHandle, BaseAddress);

	return ReadMemoryFloat(hProcessHandle, (LPCVOID)(iBAValueAddress + iOffset));
}

const char * Virtual_Memory_IO::GetStringViaLevel2Pointer(LPCVOID BaseAddress, LPCVOID Offset)
{
	return nullptr;
}

//Address Getter
LPCVOID Virtual_Memory_IO::GetAddressOfLevel2Pointer(LPCVOID BaseAddress, LPCVOID Offset)
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

bool Virtual_Memory_IO::WriteIntToMemory(HANDLE processHandle, LPCVOID address, int iValue)
{	
	return WriteProcessMemory(processHandle, (LPVOID)address, &iValue, sizeof(iValue), 0);
}
