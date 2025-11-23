#ifndef __WINE__
#define __WINE__

// -DWIN64 -D_WIN64 -D__WIN64 -D__WIN64__ -DWIN32 -D_WIN32 -D__WIN32 -D__WIN32__ -D__WINNT -D__WINNT__

#define WIN64
#define _WIN64
#define __WIN64
#define __WIN64__
#define WIN32
#define _WIN32
#define __WIN32
#define __WIN32__
#define __WINNT
#define __WINNT__

// -D__stdcall=__attribute__((ms_abi)) -D__cdecl=__stdcall -D__fastcall=__stdcall -D_stdcall=__stdcall -D_cdecl=__cdecl -D_fastcall=__fastcall

#define __stdcall __attribute__((ms_abi))
#define __cdecl __stdcall
#define __fastcall __stdcall
#define _stdcall __stdcall
#define _cdecl __cdecl
#define _fastcall __fastcall

// -D__declspec(x)=__declspec_##x -D__declspec_align(x)=__attribute__((aligned(x))) -D__declspec_allocate(x)=__attribute__((section(x)))

#define __declspec(x) __declspec_##x
#define __declspec_align(x) __attribute__((aligned(x)))
#define __declspec_allocate(x) __attribute__((section(x)))

//-D__declspec_deprecated=__attribute__((deprecated)) -D__declspec_dllimport=__attribute__((dllimport)) -D__declspec_dllexport=__attribute__((dllexport)) -D__declspec_naked=__attribute__((naked)) -D__declspec_noinline=__attribute__((noinline)) -D__declspec_noreturn=__attribute__((noreturn)) -D__declspec_nothrow=__attribute__((nothrow)) -D__declspec_novtable=__attribute__(()) -D__declspec_selectany=__attribute__((weak)) -D__declspec_thread=__thread

#define __declspec_deprecated __attribute__((deprecated))
#define __declspec_dllimport __attribute__((dllimport))
#define __declspec_dllexport __attribute__((dllexport))
#define __declspec_naked __attribute__((naked))
#define __declspec_noinline __attribute__((noinline))
#define __declspec_noreturn __attribute__((noreturn))
#define __declspec_nothrow __attribute__((nothrow))
#define __declspec_novtable __attribute__((()))
#define __declspec_selectany __attribute__((weak))
#define __declspec_thread __thread

// -D__int8=char -D__int16=short -D__int32=int -D__int64=long

#define __int8 char
#define __int16 short
#define __int32 int
#define __int64 long

#endif