struct MEMORYSTATUSEX {
	int dwLength;
	int dwMemoryLoad;
	long int ullTotalPhys;
	long int ullAvailPhys;
	long int ullTotalPageFile;
	long int ullAvailPageFile;
	long int ullTotalVirtual;
	long int ullAvailVirtual;
	long int ullAvailExtendedVirtual;
};


int (__attribute__((ms_abi)) *__PVE_GetCurrentThreadId)();
int GetCurrentThreadId() {
	return __PVE_GetCurrentThreadId();
}

void * (__attribute__((ms_abi)) *__PVE_GetModuleHandle)(void * lpModuleName);
void * GetModuleHandle(void * lpModuleName) {
	return __PVE_GetModuleHandle(lpModuleName);
}

void * (__attribute__((ms_abi)) *__PVE_GetProcAddress)(void * hModule, void * procname);
void * GetProcAddress(void * hModule, void * procname) {
	return __PVE_GetProcAddress(hModule, procname);
}

int (__attribute__((ms_abi)) *__PVE_AllocConsole)();
int AllocConsole() {
	return __PVE_AllocConsole();
}

int (__attribute__((ms_abi)) *__PVE_SetConsoleCtrlHandler)(void * handler, int add);
int SetConsoleCtrlHandler(void * handler, int add) {
	return __PVE_SetConsoleCtrlHandler(handler, add);
}

int (__attribute__((ms_abi)) *__PVE_GlobalMemoryStatusEx)(struct MEMORYSTATUSEX lpBuffer);
int GlobalMemoryStatusEx(struct MEMORYSTATUSEX lpBuffer) {
	return __PVE_GlobalMemoryStatusEx(lpBuffer);
}

int (__attribute__((ms_abi)) *__PVE_SetProcessWorkingSetSize)(void * handle, int minSize, int maxSize);
int SetProcessWorkingSetSize(void * handle, int minSize, int maxSize) {
	return __PVE_SetProcessWorkingSetSize(handle, minSize, maxSize);
}

void * (__attribute__((ms_abi)) *__PVE_CreateMutex)(void * lpMutexAttributes, int bInitialOwner, void * pName);
void * CreateMutex(void * lpMutexAttributes, int bInitialOwner, void * pName) {
	return __PVE_CreateMutex(lpMutexAttributes, bInitialOwner, pName);
}

int (__attribute__((ms_abi)) *__PVE_GetUserGeoID)(int c);
int GetUserGeoID(int c) {
	return __PVE_GetUserGeoID(c);
}

int (__attribute__((ms_abi)) *__PVE_GetGeoInfoW)(int location, int geoType, void * str, int strSize, short langId);
int GetGeoInfoW(int location, int geoType, void * str, int strSize, short langId) {
	return __PVE_GetGeoInfoW(location, geoType, str, strSize, langId);
}

void * (__attribute__((ms_abi)) *__PVE_GetModuleHandle)(void * lpModuleName);
void * GetModuleHandle(void * lpModuleName) {
	return __PVE_GetModuleHandle(lpModuleName);
}

int (__attribute__((ms_abi)) *__PVE_QueryDosDevice)(void * lpDeviceName, void * lpTargetPath, int ucchMax);
int QueryDosDevice(void * lpDeviceName, void * lpTargetPath, int ucchMax) {
	return __PVE_QueryDosDevice(lpDeviceName, lpTargetPath, ucchMax);
}

void * (__attribute__((ms_abi)) *__PVE_OpenProcess)(int dwDesiredAccess, int bInheritHandle, int dwProcessId);
void * OpenProcess(int dwDesiredAccess, int bInheritHandle, int dwProcessId) {
	return __PVE_OpenProcess(dwDesiredAccess, bInheritHandle, dwProcessId);
}

int (__attribute__((ms_abi)) *__PVE_CloseHandle)(void * hObject);
int CloseHandle(void * hObject) {
	return __PVE_CloseHandle(hObject);
}

int (__attribute__((ms_abi)) *__PVE_DuplicateHandle)(void * hSourceProcessHandle, short hSourceHandle, void * hTargetProcessHandle, void * lpTargetHandle, int dwDesiredAccess, int bInheritHandle, int dwOptions);
int DuplicateHandle(void * hSourceProcessHandle, short hSourceHandle, void * hTargetProcessHandle, void * lpTargetHandle, int dwDesiredAccess, int bInheritHandle, int dwOptions) {
	return __PVE_DuplicateHandle(hSourceProcessHandle, hSourceHandle, hTargetProcessHandle, lpTargetHandle, dwDesiredAccess, bInheritHandle, dwOptions);
}

void * (__attribute__((ms_abi)) *__PVE_GetCurrentProcess)();
void * GetCurrentProcess() {
	return __PVE_GetCurrentProcess();
}

char* __PVEExports[] = {
	"GetCurrentThreadId",
	"GetModuleHandle",
	"GetProcAddress",
	"AllocConsole",
	"SetConsoleCtrlHandler",
	"GlobalMemoryStatusEx",
	"SetProcessWorkingSetSize",
	"CreateMutex",
	"GetUserGeoID",
	"GetGeoInfoW",
	"GetModuleHandle",
	"QueryDosDevice",
	"OpenProcess",
	"CloseHandle",
	"DuplicateHandle",
	"GetCurrentProcess",
	0
};

