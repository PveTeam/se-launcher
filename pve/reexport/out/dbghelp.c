
int (__attribute__((ms_abi)) *__PVE_MiniDumpWriteDump)(void * hProcess, int processId, void * hFile, int dumpType, void * expParam, void * userStreamParam, void * callbackParam);
int MiniDumpWriteDump(void * hProcess, int processId, void * hFile, int dumpType, void * expParam, void * userStreamParam, void * callbackParam) {
	return __PVE_MiniDumpWriteDump(hProcess, processId, hFile, dumpType, expParam, userStreamParam, callbackParam);
}

char* __PVEExports[] = {
	"MiniDumpWriteDump",
	0
};

