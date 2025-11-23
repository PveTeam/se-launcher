#include <cstdint>
#include <cstdlib>
#include <dlfcn.h>
#include <cstdio>
#include <unistd.h>
#include <iostream>
#include <string>
#include <vector>
#include <algorithm>
#undef _WIN32
#include <nethost.h>
#include <hostfxr.h>
#include <coreclr_delegates.h>

#include "wine_defs.h"
#define WIN32_LEAN_AND_MEAN
#include <windows.h>

#define DLLEXPORT extern "C" __attribute__((sysv_abi))

hostfxr_initialize_for_runtime_config_fn init_fptr;
hostfxr_initialize_for_dotnet_command_line_fn init_cmd_fptr;
hostfxr_run_app_fn run_app;
hostfxr_get_runtime_delegate_fn get_delegate_fptr;
hostfxr_close_fn close_fptr;

load_assembly_and_get_function_pointer_fn get_dotnet_load_assembly(const char_t *config_path);

int run_app_cmd(int argc, const char_t **argv);

int load_hostfxr(std::string & hostfxr_path);

void load_reexport_shared_object(const char *dll_name);

typedef int (*pve_fn)(int nIndex);

typedef void (*PlatformThreadStartRoutine)() __attribute__((sysv_abi));

struct thread_parameter {
    PlatformThreadStartRoutine start;
    PCWSTR threadName;
};

DWORD WINAPI PlatformThreadProc(LPVOID lpParameter) {
    const auto param = static_cast<thread_parameter*>(lpParameter);
    // SetThreadDescription(GetCurrentThread(), param->threadName);
    printf("PlatformThreadProc %04x %S\n", GetCurrentThreadId(), param->threadName);
    param->start();
    delete param;
    return 0;
}

DLLEXPORT HRESULT CringeBoostrap_PlatformCreateThread(const SIZE_T stackSize, PlatformThreadStartRoutine start, PCWSTR threadName, HANDLE *threadHandle) {
    const auto startParameter = new thread_parameter;
    startParameter->start = start;
    // if (threadName != nullptr) {
    //     const auto size = lstrlenW(threadName) * sizeof(wchar_t);
    //     auto str = static_cast<PWSTR>(__builtin_malloc(size));
    //     __builtin_memcpy(str, threadName, size);
    //     startParameter->threadName = str;
    // } else {
    //     startParameter->threadName = L"PlatformThread";
    // }
    *threadHandle = CreateThread(nullptr, stackSize, &PlatformThreadProc, startParameter, CREATE_SUSPENDED, nullptr);
    if (*threadHandle == nullptr) {
        return HRESULT_FROM_WIN32(GetLastError());
    }
    return S_OK;
}

DLLEXPORT HRESULT CringeBoostrap_PlatformStartThread(HANDLE threadHandle) {
    if (ResumeThread(threadHandle) == -1) {
        return HRESULT_FROM_WIN32(GetLastError());
    }
    CloseHandle(threadHandle);
    return S_OK;
}

// typedef struct lib_args
// {
//     const char_t *message;
//     int number;
// };

extern "C" int WINAPI WinMain(HINSTANCE hInstance,
                              HINSTANCE hPrevInstance,
                              LPSTR lpCmdLine,
                              int nCmdShow) {
    // load_reexport_shared_object("psapi", "/home/user/nethost/linux-x64/psapi.dll.so");

    std::string hostfxr_path;
    if (load_hostfxr(hostfxr_path) == 0) {
        std::cout << "PVE tragedy 1" << std::endl;
        return 1;
    }

    const std::string cwd(getcwd(nullptr, 0));
    std::cout << "CWD: " << cwd << std::endl;

    load_reexport_shared_object("/home/zznty/.steam/steam/steamapps/common/SpaceEngineers/Bin64/Havok.dll");
    load_reexport_shared_object("/home/zznty/.steam/steam/steamapps/common/SpaceEngineers/Bin64/VRage.Native.dll");
    load_reexport_shared_object("/home/zznty/.steam/steam/steamapps/common/SpaceEngineers/Bin64/RecastDetour.dll");

    hostfxr_handle handle = nullptr;

    std::vector<const char *> argv;
    argv.push_back("../../CringeBootstrap/bin/Debug/net9.0/CringeBootstrap.dll");
    argv.push_back("/home/zznty/.steam/steam/steamapps/common/SpaceEngineers/Bin64/SpaceEngineers.exe");
    argv.push_back("--skip-crossgen");

    auto r = init_cmd_fptr(argv.size(), argv.data(), nullptr, &handle);
    if (r != 0) {
        std::cout << "PVE tragedy 3" << std::endl;
        std::cout << "code: 0x" << std::hex << r << std::endl;
        return 1;
    }

    const auto dotnet_root = hostfxr_path.substr(0, hostfxr_path.find("/host/fxr/"));

    std::cout << "Dotnet root: " << dotnet_root << std::endl;

    setenv("DOTNET_ROOT", dotnet_root.c_str(), 1);

    std::cout << "Running..." << std::endl;

    const auto exit_code = run_app(handle);

    std::cout << "Exit code: 0x" << std::hex << exit_code << std::endl;

    close_fptr(handle);

    /*char buffer[1024];
    // sprintf(buffer, "%s/SE/SpaceEngineersDedicatedServer/DedicatedServer64/DS.runtimeconfig.json", cwd);
    sprintf(buffer, "%s/linux-x64/PVETragedyStarter.runtimeconfig.json", cwd);
    // sprintf(buffer, "%s/linux-x64/PVETragedyStarter.dll", cwd);

    const char* bufferp = buffer;

    load_assembly_and_get_function_pointer_fn load_assembly_and_get_function_pointer = get_dotnet_load_assembly(buffer);
    // run_app_cmd(1, &bufferp);
    // load_assembly_and_get_function_pointer_fn load_assembly_and_get_function_pointer = NULL;


    if (load_assembly_and_get_function_pointer == nullptr) {
        printf("PVE tragedy 2\n");
        return 1;
    }*/

    // sprintf(buffer, "%s/SE/SpaceEngineersDedicatedServer/DedicatedServer64/SpaceEngineersDedicated.exe", cwd);
    /*sprintf(buffer, "%s/linux-x64/PVETragedyStarter.dll", cwd);*/

    /*void (*hello)() = nullptr;
    load_assembly_and_get_function_pointer(
        buffer,
        "PVETragedyStarter.Class1, PVETragedyStarter",
        "StartTragedy",
        "PVETragedyStarter.Class1+StartTragedyDelegate, PVETragedyStarter",
        NULL,
        (void**)&hello
    );
    if (hello == nullptr) {
        printf("PVE tragedy 3\n");
        return 1;
    }*/

    // struct lib_args
    // {
    //     void* tc;
    //     void* c;
    //     const char_t *message;
    //     int number;
    // };

    // struct lib_args args = {
    //     GetTickCount,
    //     GetSysColor_r,
    //     "Native PVE",
    //     228,
    // };

    // hello();
    // hello();

    return 0;
}

/*DLLEXPORT DWORD WinLastError() {
    return HRESULT_FROM_WIN32(GetLastError());
}

DLLEXPORT HINSTANCE WinLoadLibrary(LPCSTR lpLibFileName) {
    std::cout << "Load win library: " << lpLibFileName << std::endl;
    return LoadLibrary(lpLibFileName);
}

DLLEXPORT FARPROC WinGetProcAddress(HMODULE hModule, LPCSTR lpProcName) {
    std::cout << "GetProcAddress: " << lpProcName << std::endl;
    return GetProcAddress(hModule, lpProcName);
}*/

template<typename FnPtr = void *>
FnPtr load_fptr(void *lib, const char *name) {
    const auto fptr = dlsym(lib, name);

    if (const auto err = dlerror(); err != nullptr) {
        std::cout << "Failed to load " << name << ": " << err << std::endl;
    }

    return FnPtr(fptr);
}

// Using the nethost library, discover the location of hostfxr and get exports
int load_hostfxr(std::string &hostfxr_path) {
    size_t buffer_size;
    get_hostfxr_path(nullptr, &buffer_size, nullptr);
    hostfxr_path.resize(buffer_size);
    int rc = get_hostfxr_path(hostfxr_path.data(), &buffer_size, nullptr);
    if (rc != 0)
        return rc;

    hostfxr_path.resize(buffer_size);

    std::cout << "Hostfxr path: " << hostfxr_path << std::endl;

    // Load hostfxr and get desired exports
    void *lib = dlopen(hostfxr_path.c_str(), RTLD_NOW);
    if (lib == nullptr) {
        const auto err = dlerror();
        std::cout << "Failed to load hostfxr: " << err << std::endl;
        return 0;
    }

    dlerror(); // clear error

    init_fptr = load_fptr<hostfxr_initialize_for_runtime_config_fn>(lib, "hostfxr_initialize_for_runtime_config");
    init_cmd_fptr = load_fptr<hostfxr_initialize_for_dotnet_command_line_fn>(
        lib, "hostfxr_initialize_for_dotnet_command_line");
    run_app = load_fptr<hostfxr_run_app_fn>(lib, "hostfxr_run_app");
    get_delegate_fptr = load_fptr<hostfxr_get_runtime_delegate_fn>(lib, "hostfxr_get_runtime_delegate");
    close_fptr = load_fptr<hostfxr_close_fn>(lib, "hostfxr_close");

    return init_fptr && init_cmd_fptr && run_app && get_delegate_fptr && close_fptr;
}

// Load and initialize .NET Core and get desired function pointer for scenario
load_assembly_and_get_function_pointer_fn get_dotnet_load_assembly(const char_t *config_path) {
    // Load .NET Core
    void *load_assembly_and_get_function_pointer = nullptr;
    hostfxr_handle cxt = nullptr;

    int rc = init_fptr(config_path, nullptr, &cxt);
    if (rc != 0 || cxt == nullptr) {
        printf("PVE failed %x\n", rc);
        close_fptr(cxt);
        return nullptr;
    }

    // Get the load assembly function pointer
    rc = get_delegate_fptr(
        cxt,
        hdt_load_assembly_and_get_function_pointer,
        &load_assembly_and_get_function_pointer);
    if (rc != 0 || load_assembly_and_get_function_pointer == nullptr)
        printf("PVE delegate failed %x\n", rc);

    close_fptr(cxt);
    return load_assembly_and_get_function_pointer_fn(load_assembly_and_get_function_pointer);
}

int run_app_cmd(int argc, const char_t **argv) {
    // Load .NET Core
    hostfxr_handle cxt = nullptr;

    int rc = init_cmd_fptr(argc, argv, nullptr, &cxt);
    if (rc != 0 || cxt == nullptr) {
        printf("PVE failed %x\n", rc);
        close_fptr(cxt);
        return 1;
    }

    // Get the load assembly function pointer
    rc = run_app(cxt);

    close_fptr(cxt);
    return rc;
}

void load_reexport_shared_object(const char *dll_name) {
    void *dll_handle = LoadLibrary(dll_name);
    if (dll_handle == nullptr) {
        const auto hr = HRESULT_FROM_WIN32(GetLastError());
        std::cout << "error loading " << dll_name << ": 0x" << std::hex << hr << std::endl;
        return;
    }

    std::string so_name(dll_name);

    so_name.erase(0, so_name.find_last_of("/\\") + 1);
    so_name.erase(so_name.find_last_of("."), so_name.length());

    std::transform(so_name.begin(), so_name.end(), so_name.begin(),
    [](unsigned char c){ return std::tolower(c); });
    std::replace(so_name.begin(), so_name.end(), '.', '_');

    std::string so_path = "lib" + so_name + ".so";

    void *so_handle = dlopen(so_path.c_str(), RTLD_NOW);
    if (const auto error = dlerror(); error != nullptr) {
        std::cout << "error loading " << so_name << ": " << error << std::endl;
        return;
    }

    so_name = "__" + so_name + "_PVEExports";

    const auto exports = static_cast<char **>(dlsym(so_handle, so_name.c_str()));

    if (const auto exports_error = dlerror(); exports_error != nullptr) {
        std::cout << "exports symbol" << " error: " << exports_error << std::endl;
    }

    int index = 0;
    while (exports[index]) {
        const auto dll_symbol = (void *) GetProcAddress(HMODULE(dll_handle), exports[index]);
        if (dll_symbol == nullptr) {
            const auto hr = HRESULT_FROM_WIN32(GetLastError());
            std::cout << "symbol " << exports[index] << " error: 0x" << std::hex << hr << std::endl;
        }

        std::string s("__PVE_");
        s.append(exports[index]);
        auto so_symbol = static_cast<void **>(dlsym(so_handle, s.c_str()));
        if (const auto error = dlerror(); error != nullptr)
            std::cout << "symbol " << exports[index] << " error: " << error << std::endl;

        *so_symbol = dll_symbol;
        index++;
    }
}
