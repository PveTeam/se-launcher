
void * (__attribute__((ms_abi)) *__PVE_FindWindow)(void * lpClassName, void * lpWindowName);
void * FindWindow(void * lpClassName, void * lpWindowName) {
	return __PVE_FindWindow(lpClassName, lpWindowName);
}

void * (__attribute__((ms_abi)) *__PVE_FindWindowEx)(void * hwndParent, void * hwndChildAfter, void * lpClassName, void * lpWindowName);
void * FindWindowEx(void * hwndParent, void * hwndChildAfter, void * lpClassName, void * lpWindowName) {
	return __PVE_FindWindowEx(hwndParent, hwndChildAfter, lpClassName, lpWindowName);
}

void * (__attribute__((ms_abi)) *__PVE_SendMessage)(void * hWnd, int Msg, void * wParam, void * lParam);
void * SendMessage(void * hWnd, int Msg, void * wParam, void * lParam) {
	return __PVE_SendMessage(hWnd, Msg, wParam, lParam);
}

void * (__attribute__((ms_abi)) *__PVE_PostMessage)(void * hWnd, int Msg, void * wParam, void * lParam);
void * PostMessage(void * hWnd, int Msg, void * wParam, void * lParam) {
	return __PVE_PostMessage(hWnd, Msg, wParam, lParam);
}

void * (__attribute__((ms_abi)) *__PVE_GetWindow)(void * hWnd, int uCmd);
void * GetWindow(void * hWnd, int uCmd) {
	return __PVE_GetWindow(hWnd, uCmd);
}

int (__attribute__((ms_abi)) *__PVE_IsWindow)(void * hWnd);
int IsWindow(void * hWnd) {
	return __PVE_IsWindow(hWnd);
}

int (__attribute__((ms_abi)) *__PVE_ShowCursor)(int bVisible);
int ShowCursor(int bVisible) {
	return __PVE_ShowCursor(bVisible);
}

int (__attribute__((ms_abi)) *__PVE_PeekMessage)(void * lpMsg, void * hWnd, int wMsgFilterMin, int wMsgFilterMax, int wRemoveMsg);
int PeekMessage(void * lpMsg, void * hWnd, int wMsgFilterMin, int wMsgFilterMax, int wRemoveMsg) {
	return __PVE_PeekMessage(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
}

int (__attribute__((ms_abi)) *__PVE_TranslateMessage)(void * lpMsg);
int TranslateMessage(void * lpMsg) {
	return __PVE_TranslateMessage(lpMsg);
}

void * (__attribute__((ms_abi)) *__PVE_DispatchMessage)(void * lpmsg);
void * DispatchMessage(void * lpmsg) {
	return __PVE_DispatchMessage(lpmsg);
}

void * (__attribute__((ms_abi)) *__PVE_GetForegroundWindow)();
void * GetForegroundWindow() {
	return __PVE_GetForegroundWindow();
}

void * (__attribute__((ms_abi)) *__PVE_LoadImage)(void * hinst, void * lpszName, int uType, int cxDesired, int cyDesired, int fuLoad);
void * LoadImage(void * hinst, void * lpszName, int uType, int cxDesired, int cyDesired, int fuLoad) {
	return __PVE_LoadImage(hinst, lpszName, uType, cxDesired, cyDesired, fuLoad);
}

int (__attribute__((ms_abi)) *__PVE_MessageBox)(void * hWndle, void * text, void * caption, int buttons);
int MessageBox(void * hWndle, void * text, void * caption, int buttons) {
	return __PVE_MessageBox(hWndle, text, caption, buttons);
}

void * (__attribute__((ms_abi)) *__PVE_DefWindowProc)(void * hWnd, int uMsg, void * wParam, void * lParam);
void * DefWindowProc(void * hWnd, int uMsg, void * wParam, void * lParam) {
	return __PVE_DefWindowProc(hWnd, uMsg, wParam, lParam);
}

void * (__attribute__((ms_abi)) *__PVE_LoadKeyboardLayout)(void * keyboardLayoutID, int flags);
void * LoadKeyboardLayout(void * keyboardLayoutID, int flags) {
	return __PVE_LoadKeyboardLayout(keyboardLayoutID, flags);
}

int (__attribute__((ms_abi)) *__PVE_UnloadKeyboardLayout)(void * handle);
int UnloadKeyboardLayout(void * handle) {
	return __PVE_UnloadKeyboardLayout(handle);
}

void * (__attribute__((ms_abi)) *__PVE_GetKeyboardLayout)(void * threadId);
void * GetKeyboardLayout(void * threadId) {
	return __PVE_GetKeyboardLayout(threadId);
}

short (__attribute__((ms_abi)) *__PVE_GetKeyState)(int keyCode);
short GetKeyState(int keyCode) {
	return __PVE_GetKeyState(keyCode);
}

short (__attribute__((ms_abi)) *__PVE_GetAsyncKeyState)(int keyCode);
short GetAsyncKeyState(int keyCode) {
	return __PVE_GetAsyncKeyState(keyCode);
}

void * (__attribute__((ms_abi)) *__PVE_CallNextHookEx)(void * hhk, int nCode, void * wParam, void * lParam);
void * CallNextHookEx(void * hhk, int nCode, void * wParam, void * lParam) {
	return __PVE_CallNextHookEx(hhk, nCode, wParam, lParam);
}

void * (__attribute__((ms_abi)) *__PVE_SetWindowsHookEx)(int idHook, void * lpfn, void * hMod, int dwThreadId);
void * SetWindowsHookEx(int idHook, void * lpfn, void * hMod, int dwThreadId) {
	return __PVE_SetWindowsHookEx(idHook, lpfn, hMod, dwThreadId);
}

int (__attribute__((ms_abi)) *__PVE_UnhookWindowsHookEx)(void * hhk);
int UnhookWindowsHookEx(void * hhk) {
	return __PVE_UnhookWindowsHookEx(hhk);
}

int (__attribute__((ms_abi)) *__PVE_MessageBox)(void * hWndle, void * text, void * caption, int buttons);
int MessageBox(void * hWndle, void * text, void * caption, int buttons) {
	return __PVE_MessageBox(hWndle, text, caption, buttons);
}

short (__attribute__((ms_abi)) *__PVE_GetAsyncKeyState)(int vKey);
short GetAsyncKeyState(int vKey) {
	return __PVE_GetAsyncKeyState(vKey);
}

char* __PVEExports[] = {
	"FindWindow",
	"FindWindowEx",
	"SendMessage",
	"PostMessage",
	"GetWindow",
	"IsWindow",
	"ShowCursor",
	"PeekMessage",
	"TranslateMessage",
	"DispatchMessage",
	"GetForegroundWindow",
	"LoadImage",
	"MessageBox",
	"DefWindowProc",
	"LoadKeyboardLayout",
	"UnloadKeyboardLayout",
	"GetKeyboardLayout",
	"GetKeyState",
	"GetAsyncKeyState",
	"CallNextHookEx",
	"SetWindowsHookEx",
	"UnhookWindowsHookEx",
	"MessageBox",
	"GetAsyncKeyState",
	0
};

