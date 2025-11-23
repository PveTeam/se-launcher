
long int (__attribute__((ms_abi)) *__PVE_GetTotalAllocations)();
long int GetTotalAllocations() {
	return __PVE_GetTotalAllocations();
}

void * (__attribute__((ms_abi)) *__PVE_GetThreadAllocationPtr)();
void * GetThreadAllocationPtr() {
	return __PVE_GetThreadAllocationPtr();
}

char* __PVEExports[] = {
	"GetTotalAllocations",
	"GetThreadAllocationPtr",
	0
};

