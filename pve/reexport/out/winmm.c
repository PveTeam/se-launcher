
int (__attribute__((ms_abi)) *__PVE_timeBeginPeriod)(int uMilliseconds);
int timeBeginPeriod(int uMilliseconds) {
	return __PVE_timeBeginPeriod(uMilliseconds);
}

int (__attribute__((ms_abi)) *__PVE_timeEndPeriod)(int uMilliseconds);
int timeEndPeriod(int uMilliseconds) {
	return __PVE_timeEndPeriod(uMilliseconds);
}

char* __PVEExports[] = {
	"TimeBeginPeriod",
	"TimeEndPeriod",
	0
};

