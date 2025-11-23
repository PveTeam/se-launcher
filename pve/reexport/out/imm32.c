
void * (__attribute__((ms_abi)) *__PVE_ImmGetContext)(void * hWnd);
void * ImmGetContext(void * hWnd) {
	return __PVE_ImmGetContext(hWnd);
}

int (__attribute__((ms_abi)) *__PVE_ImmGetCandidateListW)(void * himc, int deIndex, void * lpCandidateList, int dwBufLen);
int ImmGetCandidateListW(void * himc, int deIndex, void * lpCandidateList, int dwBufLen) {
	return __PVE_ImmGetCandidateListW(himc, deIndex, lpCandidateList, dwBufLen);
}

int (__attribute__((ms_abi)) *__PVE_ImmReleaseContext)(void * hWnd, void * hIMC);
int ImmReleaseContext(void * hWnd, void * hIMC) {
	return __PVE_ImmReleaseContext(hWnd, hIMC);
}

int (__attribute__((ms_abi)) *__PVE_ImmGetCompositionStringW)(void * hIMC, int dwIndex, void * lpBuf, int dwBufLen);
int ImmGetCompositionStringW(void * hIMC, int dwIndex, void * lpBuf, int dwBufLen) {
	return __PVE_ImmGetCompositionStringW(hIMC, dwIndex, lpBuf, dwBufLen);
}

int (__attribute__((ms_abi)) *__PVE_ImmNotifyIME)(void * hIMC, int dwAction, int dwIndex, int dwValue);
int ImmNotifyIME(void * hIMC, int dwAction, int dwIndex, int dwValue) {
	return __PVE_ImmNotifyIME(hIMC, dwAction, dwIndex, dwValue);
}

int (__attribute__((ms_abi)) *__PVE_ImmGetCandidateWindow)(void * hIMC, int dwIndex, void * lpCandidate);
int ImmGetCandidateWindow(void * hIMC, int dwIndex, void * lpCandidate) {
	return __PVE_ImmGetCandidateWindow(hIMC, dwIndex, lpCandidate);
}

int (__attribute__((ms_abi)) *__PVE_ImmSetCompositionWindow)(void * hIMC, void * form);
int ImmSetCompositionWindow(void * hIMC, void * form) {
	return __PVE_ImmSetCompositionWindow(hIMC, form);
}

int (__attribute__((ms_abi)) *__PVE_ImmSetCandidateWindow)(void * hIMC, void * form);
int ImmSetCandidateWindow(void * hIMC, void * form) {
	return __PVE_ImmSetCandidateWindow(hIMC, form);
}

char* __PVEExports[] = {
	"ImmGetContext",
	"ImmGetCandidateList",
	"ImmReleaseContext",
	"ImmGetCompositionStringW",
	"ImmNotifyIME",
	"ImmGetCandidateWindow",
	"ImmSetCompositionWindow",
	"ImmSetCandidateWindow",
	0
};

