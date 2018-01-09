#pragma once
#include <exception>
#include <string>
#include <sstream>
#include <tchar.h>
#include <TlHelp32.h>
#include <memory>
class Exception_Manager
{
private:
	Exception_Manager() {}
	~Exception_Manager() {}
	//Data
	static const char* sExceptionCaption;
	static const char* sTargetName;

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
	static void SetExeptionCaption(const char* sExceptionCaption);
	static void SetTargetName(const char* sTargetName);

	
};

//Exception classes
//class LocalObjectMissing : public std::exception {
//	virtual const char* what() const throw(){ return "301"; }
//} LocalObjectMissingException;
