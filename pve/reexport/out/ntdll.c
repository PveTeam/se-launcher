
int (__attribute__((ms_abi)) *__PVE_NtQueryTimerResolution)(void * MinimumResolution, void * MaximumResolution, void * CurrentResolution);
int NtQueryTimerResolution(void * MinimumResolution, void * MaximumResolution, void * CurrentResolution) {
	return __PVE_NtQueryTimerResolution(MinimumResolution, MaximumResolution, CurrentResolution);
}

int (__attribute__((ms_abi)) *__PVE_NtSetTimerResolution)(int DesiredResolution, int SetResolution, void * CurrentResolution);
int NtSetTimerResolution(int DesiredResolution, int SetResolution, void * CurrentResolution) {
	return __PVE_NtSetTimerResolution(DesiredResolution, SetResolution, CurrentResolution);
}

int (__attribute__((ms_abi)) *__PVE_NtQueryObject)(void * ObjectHandle, int ObjectInformationClass, void * ObjectInformation, int ObjectInformationLength, void * returnLength);
int NtQueryObject(void * ObjectHandle, int ObjectInformationClass, void * ObjectInformation, int ObjectInformationLength, void * returnLength) {
	return __PVE_NtQueryObject(ObjectHandle, ObjectInformationClass, ObjectInformation, ObjectInformationLength, returnLength);
}

int (__attribute__((ms_abi)) *__PVE_NtQuerySystemInformation)(int SystemInformationClass, void * SystemInformation, int SystemInformationLength, void * returnLength);
int NtQuerySystemInformation(int SystemInformationClass, void * SystemInformation, int SystemInformationLength, void * returnLength) {
	return __PVE_NtQuerySystemInformation(SystemInformationClass, SystemInformation, SystemInformationLength, returnLength);
}

char* __PVEExports[] = {
	"NtQueryTimerResolution",
	"NtSetTimerResolution",
	"NtQueryObject",
	"NtQuerySystemInformation",
	0
};

