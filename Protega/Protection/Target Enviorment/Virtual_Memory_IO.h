#pragma once


class Virtual_Memory_IO
{
private:
	
protected:
	//Vars
	HANDLE hProcessHandle;
	//Pointer Getter
	int GetIntViaLevel2Pointer(LPCVOID BaseAddress, LPCVOID Offset);
	float GetFloatViaLevel2Pointer(LPCVOID BaseAddress, LPCVOID Offset);
	const char* GetStringViaLevel2Pointer(LPCVOID BaseAddress, LPCVOID Offset);

	//Value Getter
	int ReadMemoryInt(HANDLE processHandle, LPCVOID address);
	float ReadMemoryFloat(HANDLE processHandle, LPCVOID address);

	//Value writer
	bool WriteIntToMemory(HANDLE processHandle, LPCVOID address, int iValue);

public:
	Virtual_Memory_IO();
	~Virtual_Memory_IO();

};

