#include "../stdafx.h"
#include "Exception_Manager.h"

const char* Exception_Manager::sCrashReporterName = "CrashReporter.exe";
const char* Exception_Manager::sErrorFileName = "latest_protega_error.err";
std::string Exception_Manager::sBaseFolder = ".\\";
//Private
void Exception_Manager::ShowErrorA(int iErrorNumber, const char * sMessage)
{

	std::stringstream ss;
		
	std::ofstream myfile;
	ss << sBaseFolder << sErrorFileName;
	myfile.open(ss.str());
	ss.str("");
	ss << "Error " << iErrorNumber << ": " << sMessage;
	myfile << ss.str();
	myfile.close();
	Sleep(1000);
	ss.str("");
	ss << "\"" << sBaseFolder << sCrashReporterName << "\"";

	//IMPORTANT: Globallizing!
	StartProgram(L".\\CrashReporter.exe");
}

void Exception_Manager::CloseOwnProcess()
{
	//Kill own handle
	exit(1);
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

void Exception_Manager::StartProgram(LPCTSTR lpApplicationName)
{
	// additional information
	STARTUPINFO si;
	PROCESS_INFORMATION pi;

	// set the size of the structures
	ZeroMemory(&si, sizeof(si));
	si.cb = sizeof(si);
	ZeroMemory(&pi, sizeof(pi));

	// start the program up
	CreateProcess(lpApplicationName,   // the path
		NULL,        // Command line
		NULL,           // Process handle not inheritable
		NULL,           // Thread handle not inheritable
		FALSE,          // Set handle inheritance to FALSE
		0,              // No creation flags
		NULL,           // Use parent's environment block
		NULL,           // Use parent's starting directory 
		&si,            // Pointer to STARTUPINFO structure
		&pi             // Pointer to PROCESS_INFORMATION structure (removed extra parentheses)
	);
	// Close process and thread handles. 
	CloseHandle(pi.hProcess);
	CloseHandle(pi.hThread);
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

void Exception_Manager::SetCrashReporterName(const char * _sCrashReporter)
{
	sCrashReporterName = _sCrashReporter;
}

void Exception_Manager::SetErrorFileName(const char * _sErrorFileName)
{
	sErrorFileName = _sErrorFileName;
}

void Exception_Manager::SetBaseFolder(std::string _sBaseFolder)
{
	sBaseFolder = _sBaseFolder;
}

