// ConsoleApplication1.cpp : 此檔案包含 'main' 函式。程式會於該處開始執行及結束執行。
//
#include <Windows.h>
#include <tchar.h>
#include <iostream>


void main(int argc, TCHAR *argv[])
{
	DWORD  dwFilter = REG_NOTIFY_CHANGE_NAME |
		REG_NOTIFY_CHANGE_ATTRIBUTES |
		REG_NOTIFY_CHANGE_LAST_SET |
		REG_NOTIFY_CHANGE_SECURITY;

	HANDLE hEvent;
	HKEY   hMainKey;
	HKEY   hKey;
	LONG   lErrorCode;

	// Display the usage error message.
	//if (argc != 3)
	//{
	//	_tprintf(TEXT("Usage: notify [HKLM|HKU|HKCU|HKCR|HCC] [<subkey>]\n"));
	//	return;
	//}

	// Convert parameters to appropriate handles.
	//if (_tcscmp(TEXT("HKLM"), argv[1]) == 0) hMainKey = HKEY_LOCAL_MACHINE;
	//else if (_tcscmp(TEXT("HKU"), argv[1]) == 0) hMainKey = HKEY_USERS;
	//else if (_tcscmp(TEXT("HKCU"), argv[1]) == 0) hMainKey = HKEY_CURRENT_USER;
	//else if (_tcscmp(TEXT("HKCR"), argv[1]) == 0) hMainKey = HKEY_CLASSES_ROOT;
	//else if (_tcscmp(TEXT("HCC"), argv[1]) == 0) hMainKey = HKEY_CURRENT_CONFIG;
	//else
	//{
	//	_tprintf(TEXT("Usage: notify [HKLM|HKU|HKCU|HKCR|HCC] [<subkey>]\n"));
	//	return;
	//}
	hMainKey = HKEY_CURRENT_CONFIG;
	const wchar_t* str = _T("UnitTest\\Company\\");
	// Open a key.
	lErrorCode = RegOpenKeyEx(hMainKey, str, 0, KEY_NOTIFY, &hKey);
	if (lErrorCode != ERROR_SUCCESS)
	{
		_tprintf(TEXT("Error in RegOpenKeyEx (%d).\n"), lErrorCode);
		return;
	}

	// Create an event.
	hEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
	if (hEvent == NULL)
	{
		_tprintf(TEXT("Error in CreateEvent (%d).\n"), GetLastError());
		return;
	}

	// Watch the registry key for a change of value.
	lErrorCode = RegNotifyChangeKeyValue(hKey,
		TRUE,
		dwFilter,
		hEvent,
		TRUE);
	if (lErrorCode != ERROR_SUCCESS)
	{
		_tprintf(TEXT("Error in RegNotifyChangeKeyValue (%d).\n"), lErrorCode);
		return;
	}

	// Wait for an event to occur.
	_tprintf(TEXT("Waiting for a change in the specified key...\n"));
	if (WaitForSingleObject(hEvent, INFINITE) == WAIT_FAILED)
	{
		_tprintf(TEXT("Error in WaitForSingleObject (%d).\n"), GetLastError());
		return;
	}
	else _tprintf(TEXT("\nChange has occurred.\n"));

	// Close the key.
	lErrorCode = RegCloseKey(hKey);
	if (lErrorCode != ERROR_SUCCESS)
	{
		_tprintf(TEXT("Error in RegCloseKey (%d).\n"), GetLastError());
		return;
	}

	// Close the handle.
	if (!CloseHandle(hEvent))
	{
		_tprintf(TEXT("Error in CloseHandle.\n"));
		return;
	}
    std::cout << "Hello World!\n";
}

// 執行程式: Ctrl + F5 或 [偵錯] > [啟動但不偵錯] 功能表
// 偵錯程式: F5 或 [偵錯] > [啟動偵錯] 功能表

// 開始使用的提示: 
//   1. 使用 [方案總管] 視窗，新增/管理檔案
//   2. 使用 [Team Explorer] 視窗，連線到原始檔控制
//   3. 使用 [輸出] 視窗，參閱組建輸出與其他訊息
//   4. 使用 [錯誤清單] 視窗，檢視錯誤
//   5. 前往 [專案] > [新增項目]，建立新的程式碼檔案，或是前往 [專案] > [新增現有項目]，將現有程式碼檔案新增至專案
//   6. 之後要再次開啟此專案時，請前往 [檔案] > [開啟] > [專案]，然後選取 .sln 檔案
