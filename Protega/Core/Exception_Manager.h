#pragma once
#include <exception>
#include <string>
#include <sstream>
#include <tchar.h>
#include <TlHelp32.h>
#include <memory>
#include <iostream>
#include <fstream>
#include <codecvt>

class Exception_Manager
{
private:
	Exception_Manager() {}
	~Exception_Manager() {}
	//Data
	static const char* sCrashReporterName;
	static const char* sErrorFileName;

	//Functions
	static void ShowErrorA(int iErrorNumber, const char* sMessage);
	static void ShowErrorW(int iErrorNumber, std::wstring wsMessage);
	static void CloseOwnProcess();
	static DWORD GetMainThreadId();
	static bool FreezeMainThread();
	static bool KillMainThread();
public:
	//Functions
	static void HandleProtegaStandardError(int iErrorNumber, const char* sMessage);
	static void HandleProtegaStandardError(int iErrorNumber, std::wstring wsMessage);

	//Setter
	static void SetCrashReporterName(const char* _sTargetName);
	static void SetErrorFileName(const char* _sErrorFileName);

	
};

//Exception classes
//class LocalObjectMissing : public std::exception {
//	virtual const char* what() const throw(){ return "301"; }
//} LocalObjectMissingException;
