cmake_minimum_required(VERSION 4.1)

project(CringeBootstrap.Native)

set(CXX_STANDARD 20)

set(CMAKE_VERBOSE_MAKEFILE ON)

set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fshort-wchar -fPIC -mstackrealign -maccumulate-outgoing-args")
set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -fshort-wchar -fPIC -mstackrealign -maccumulate-outgoing-args")

add_library(callback SHARED
        callback/callback.cpp
        callback/callback.h
)

target_include_directories(callback PUBLIC "callback/")

add_library(havok SHARED reexport/out/havok.c)

target_link_libraries(havok PRIVATE callback)

add_library(vrage_native SHARED reexport/out/vrage_native.c)

target_link_libraries(vrage_native PRIVATE callback)

add_library(recastdetour SHARED reexport/out/recastdetour.c)

target_link_libraries(recastdetour PRIVATE callback)

add_library(nethost STATIC IMPORTED)
set_property(TARGET nethost PROPERTY IMPORTED_LOCATION "/usr/share/dotnet/packs/Microsoft.NETCore.App.Host.arch-x64/9.0.11/runtimes/arch-x64/native/libnethost.a")

add_library(libadvapi32 STATIC IMPORTED)
set_property(TARGET libadvapi32 PROPERTY IMPORTED_LOCATION "/usr/bin/../lib/wine/x86_64-unix/libadvapi32.a")

add_library(libkernel32 STATIC IMPORTED)
set_property(TARGET libkernel32 PROPERTY IMPORTED_LOCATION "/usr/bin/../lib/wine/x86_64-unix/libkernel32.a")

add_library(libuser32 STATIC IMPORTED)
set_property(TARGET libuser32 PROPERTY IMPORTED_LOCATION "/usr/bin/../lib/wine/x86_64-unix/libuser32.a")

add_library(libwinecrt0 STATIC IMPORTED)
set_property(TARGET libwinecrt0 PROPERTY IMPORTED_LOCATION "/usr/bin/../lib/wine/x86_64-unix/libwinecrt0.a")

target_include_directories(nethost INTERFACE "/usr/share/dotnet/packs/Microsoft.NETCore.App.Host.arch-x64/9.0.11/runtimes/arch-x64/native/")

add_executable(CringeBootstrap.Native winload.cpp)
set(OUTPUT_NAME "CringeBootstrap.Native.exe")

target_include_directories(CringeBootstrap.Native PRIVATE "/usr/bin/../include/wine/windows")
target_include_directories(CringeBootstrap.Native PRIVATE "/usr/include/wine/windows")
target_include_directories(CringeBootstrap.Native PRIVATE "/usr/local/include/wine/windows")

target_compile_definitions(CringeBootstrap.Native PRIVATE -DWINE_UNICODE_NATIVE -D_REENTRANT)

target_link_directories(CringeBootstrap.Native PRIVATE "/usr/bin/../lib/wine/x86_64-unix/")

target_link_libraries(CringeBootstrap.Native PRIVATE libadvapi32 libkernel32 libuser32 libwinecrt0)

target_link_libraries(CringeBootstrap.Native PRIVATE nethost)

target_link_libraries(CringeBootstrap.Native PUBLIC havok vrage_native recastdetour)

#target_link_libraries(CringeBootstrap.Native /usr/share/dotnet/packs/Microsoft.NETCore.App.Host.arch-x64/9.0.11/runtimes/arch-x64/native/libnethost.so)
