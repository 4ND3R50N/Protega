#pragma once
#include <string>

class Virtual_Memory_IO
{
private:
	
protected:
	//Vars
	HANDLE hProcessHandle;

	//Pointer Getter
	int GetIntViaLevel1Pointer(LPCVOID BaseAddress, LPCVOID Offset);
	float GetFloatViaLevel1Pointer(LPCVOID BaseAddress, LPCVOID Offset);
	const char* GetStringViaLevel1Pointer(LPCVOID BaseAddress, LPCVOID Offset);

	int GetIntViaLevel3Pointer(LPCVOID BaseAddress, LPCVOID Offset1, LPCVOID Offset2, LPCVOID Offset3);


	//Address Getter
	LPCVOID GetAddressOfLevel1Pointer(LPCVOID BaseAddress, LPCVOID Offset);

	//Value Getter
	int ReadMemoryInt(HANDLE processHandle, LPCVOID address);
	float ReadMemoryFloat(HANDLE processHandle, LPCVOID address);
	std::string ReadMemoryString(HANDLE processHandle, LPCVOID address, short iLength);

	//Value writer
	bool WriteIntToMemory(HANDLE processHandle, LPCVOID address, int iValue);

public:
	Virtual_Memory_IO();
	Virtual_Memory_IO(int iTargetApplicationId);
	~Virtual_Memory_IO();

};

