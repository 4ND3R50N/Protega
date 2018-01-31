#include "../stdafx.h"
#include "Exception_Manager.h"

const char* Exception_Manager::sExceptionCaption = "";
const char* Exception_Manager::sTargetName = "";


//Private
void Exception_Manager::ShowErrorA(int iErrorNumber, const char * sMessage)
{

	std::stringstream ss;
	ss << "Error " << iErrorNumber << ": " << sMessage;
	system(ss.str().c_str());

	std::ofstream myfile;
	myfile.open("latest_protega_error.err");
	myfile << ss.str();
	myfile.close();
	Sleep(1000);
	system("\".\\CrashReporter.exe\"");
	//std::terminate();
}

void Exception_Manager::ShowErrorW(int iErrorNumber, std::wstring wsMessage)
{
	std::wstringstream ss;
	ss << "Error " << iErrorNumber << ": " << wsMessage;

	std::wstring wsError = ss.str();
	std::string sError = "";
	using convert_type = std::codecvt_utf8<wchar_t>;
	std::wstring_convert<convert_type, wchar_t> converter;

	//use converter (.to_bytes: wstr->str, .from_bytes: str->wstr)
	sError = converter.to_bytes(wsError);
	
	std::ofstream myfile;
	myfile.open("latest_protega_error.err");
	myfile << sError;
	myfile.close();
	Sleep(1000);
	system("\".\\CrashReporter.exe\"");
	//std::terminate();
}

void Exception_Manager::CloseOwnProcess()
{
	//Kill own handle
	int retval = ::_tsystem(_T("taskkill /F /T /IM CabalMain.exe"));
}

DWORD Exception_Manager::GetMainThreadId() {
	const std::tr1::shared_ptr<void> hThreadSnapshot(
		CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, 0), CloseHandle);
	if (hThreadSnapshot.get() == INVALID_HANDLE_VALUE) {
		throw std::runtime_error("GetMainThreadId failed");
	}
	THREADENTRY32 tEntry;
	tEntry.dwSize = sizeof(THREADENTRY32);
	DWORD result = 0;
	DWORD currentPID = GetCurrentProcessId();
	for (BOOL success = Thread32First(hThreadSnapshot.get(), &tEntry);
		!result && success && GetLastError() != ERROR_NO_MORE_FILES;
		success = Thread32Next(hThreadSnapshot.get(), &tEntry))
	{
		if (tEntry.th32OwnerProcessID == currentPID) {
			result = tEntry.th32ThreadID;
		}
	}
	return result;
}

bool Exception_Manager::FreezeMainThread()
{
	HANDLE hMainThread = OpenThread(THREAD_ALL_ACCESS,
		FALSE,
		GetMainThreadId());
	if (!hMainThread) { return false; }

	SuspendThread(hMainThread);
	return true;
}

bool Exception_Manager::KillMainThread()
{
	HANDLE hMainThread = OpenThread(THREAD_ALL_ACCESS,
		FALSE,
		GetMainThreadId());
	if (!hMainThread) { return false; }

	TerminateThread(hMainThread, 1);
	return true;
}

//Public
void Exception_Manager::HandleProtegaStandardError(int iErrorNumber, const char * sMessage)
{
	//Freeze Process
	if (!FreezeMainThread())
	{
		//What shall i do here?
	}
	ShowErrorA(iErrorNumber, sMessage);	
	CloseOwnProcess();
}

void Exception_Manager::HandleProtegaStandardError(int iErrorNumber, std::wstring wsMessage)
{
	//Freeze Process
	if (!FreezeMainThread())
	{
		//What shall i do here?
	}
	ShowErrorW(iErrorNumber, wsMessage);
	CloseOwnProcess();
}


void Exception_Manager::SetExeptionCaption(const char * _sExceptionCaption)
{
	sExceptionCaption = _sExceptionCaption;
}

void Exception_Manager::SetTargetName(const char * _sTargetName)
{
	sTargetName = _sTargetName;
}

