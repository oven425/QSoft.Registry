// ConsoleApplication1.cpp : 此檔案包含 'main' 函式。程式會於該處開始執行及結束執行。
//
#include <Windows.h>
#include <tchar.h>
#include <iostream>
#include <ktmw32.h>

//CLS 

void DetectChanged()
{
	//DWORD  dwFilter = REG_NOTIFY_CHANGE_NAME |
	//	REG_NOTIFY_CHANGE_ATTRIBUTES |
	//	REG_NOTIFY_CHANGE_LAST_SET |
	//	REG_NOTIFY_CHANGE_SECURITY;

	DWORD  dwFilter = REG_NOTIFY_CHANGE_NAME |
		REG_NOTIFY_CHANGE_ATTRIBUTES |
		REG_NOTIFY_CHANGE_LAST_SET;

	HANDLE hEvent;
	HKEY   hMainKey;
	HKEY   hKey;
	LONG   lErrorCode;


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
}



LPTSTR ErrMsg(LSTATUS data)
{
	LPTSTR errorText = NULL;

	FormatMessage(
		// use system message tables to retrieve error text
		FORMAT_MESSAGE_FROM_SYSTEM
		// allocate buffer on local heap for error text
		| FORMAT_MESSAGE_ALLOCATE_BUFFER
		// Important! will fail otherwise, since we're not 
		// (and CANNOT) pass insertion parameters
		| FORMAT_MESSAGE_IGNORE_INSERTS,
		NULL,    // unused with FORMAT_MESSAGE_FROM_SYSTEM
		data,
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
		(LPTSTR)&errorText,  // output 
		0, // minimum size for output buffer
		NULL);   // arguments - see note 
	return errorText;
}

void Transacted()
{
	HANDLE hTransaction = ::CreateTransaction(nullptr, nullptr, TRANSACTION_DO_NOT_PROMOTE, 0, 0, INFINITE, nullptr);
	HKEY hKey;
	auto error = ::RegCreateKeyTransacted(HKEY_CURRENT_CONFIG, L"UnitTest\\Company\\1",
		0, nullptr, REG_OPTION_NON_VOLATILE, KEY_WRITE,
		nullptr, &hKey, nullptr, hTransaction, nullptr);
	if (error != ERROR_SUCCESS)
	{
		auto msg = ErrMsg(error);
	}

	::RegDeleteKeyTransacted(HKEY_CURRENT_CONFIG, L"UnitTest\\Company\\dd", 0, 0, hTransaction, nullptr);

	WCHAR value[] = L"Name1";
	error = ::RegSetValueEx(hKey, L"Name1", 0, REG_SZ, (const BYTE*)value, sizeof(value));
	if (error != ERROR_SUCCESS)
	{
		auto msg = ErrMsg(error);
	}
	RegCloseKey(hKey);

	//::RollbackTransaction(hTransaction);
	::CommitTransaction(hTransaction);
	
	::CloseHandle(hTransaction);
}

void Admin(const wchar_t* se)
{
	HANDLE token;
	::OpenProcessToken(::GetCurrentProcess(), TOKEN_ALL_ACCESS, &token);

	LUID PrivilegeRequired;
	BOOL bRes = FALSE;
	bRes = LookupPrivilegeValue(NULL, se, &PrivilegeRequired);

	TOKEN_PRIVILEGES tp;
	tp.PrivilegeCount = 1;
	tp.Privileges[0].Luid = PrivilegeRequired;
	tp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
	bRes = AdjustTokenPrivileges(token, FALSE, &tp, sizeof(TOKEN_PRIVILEGES), NULL, NULL);

	::CloseHandle(token);
}

void Backup(HKEY root, const wchar_t* subkey, const wchar_t* filename)
{
	Admin(SE_BACKUP_NAME);
	BOOL is_success = FALSE;
	DWORD dwDisposition = 0;
	HKEY hKey;
	LONG ret;
	//const wchar_t* hiveName = L"UnitTest\\Company";
	//const wchar_t* filename = L"test1";
	ret = RegCreateKeyEx(root, subkey, 0, NULL, REG_OPTION_BACKUP_RESTORE, 0, NULL, &hKey, &dwDisposition);
	
	if (ret != ERROR_SUCCESS)
	{
		LPTSTR errorText = ErrMsg(ret);
		errorText = NULL;
	}
	else if (dwDisposition != REG_OPENED_EXISTING_KEY) 
	{
		RegCloseKey(hKey);
	}
	else 
	{
		is_success = (RegSaveKeyEx(hKey, filename, NULL, REG_STANDARD_FORMAT) == ERROR_SUCCESS);
		RegCloseKey(hKey);
	}

}

void Restore(HKEY root, const wchar_t* subkey, const wchar_t* filename)
{
	Admin(SE_RESTORE_NAME);
	BOOL is_success = FALSE;
	DWORD dwDisposition = 0;
	HKEY hKey;
	LONG ret;
	//const wchar_t* hiveName = L"Citys1";
	//const wchar_t* filename = L"test";
	ret = RegCreateKeyEx(root, subkey, 0, NULL, REG_OPTION_BACKUP_RESTORE, 0, NULL, &hKey, &dwDisposition);

	if (ret != ERROR_SUCCESS)
	{
		LPTSTR errorText = NULL;

		FormatMessage(
			// use system message tables to retrieve error text
			FORMAT_MESSAGE_FROM_SYSTEM
			// allocate buffer on local heap for error text
			| FORMAT_MESSAGE_ALLOCATE_BUFFER
			// Important! will fail otherwise, since we're not 
			// (and CANNOT) pass insertion parameters
			| FORMAT_MESSAGE_IGNORE_INSERTS,
			NULL,    // unused with FORMAT_MESSAGE_FROM_SYSTEM
			ret,
			MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
			(LPTSTR)&errorText,  // output 
			0, // minimum size for output buffer
			NULL);   // arguments - see note 
		errorText = NULL;
	}
	//else if (dwDisposition != REG_OPENED_EXISTING_KEY)
	//{
	//	RegCloseKey(hKey);
	//}
	else
	{
		auto err = RegRestoreKey(hKey, filename, REG_FORCE_RESTORE);
		auto msg = ErrMsg(err);
		RegCloseKey(hKey);
	}
}

void main(int argc, TCHAR *argv[])
{
	//Transacted();
	Backup(HKEY_LOCAL_MACHINE, L"SYSTEM\\CurrentControlSet", L"Controlset");
	//Restore(HKEY_CURRENT_CONFIG, L"Test", L"Controlset");
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
