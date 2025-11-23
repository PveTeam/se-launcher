
int (__attribute__((ms_abi)) *__PVE_GetProcessMemoryInfo)(void * hProcess, void * counters, int size);
int GetProcessMemoryInfo(void * hProcess, void * counters, int size) {
	return __PVE_GetProcessMemoryInfo(hProcess, counters, size);
}

char* __PVEExports[] = {
	"GetProcessMemoryInfo",
	0
};

