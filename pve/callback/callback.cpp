#include "callback.h"
#include <stdio.h>
#include <stdint.h>
#include <string.h>
#include <sys/mman.h>
#include <unistd.h>
#include <stdlib.h>
#include <unordered_map>
#include <mutex>

// vibing

// std::unordered_map<void*, void*> trampolines;
// std::mutex trampolines_mutex;

__thread void *cb_userdata_tls = nullptr;

/* Make the setter use MS x64 ABI (userdata in RCX). */
void __attribute__((ms_abi)) set_cb_userdata_c(void *u) {
    printf("callback invoke - 0x%016lx\n", (unsigned long)u);
    cb_userdata_tls = u;
}

/* mmap a writable page (we will flip to RX after writing code) */
static void *alloc_writable_page(size_t len) {
    size_t pagesz = sysconf(_SC_PAGESIZE);
    size_t s = (len + pagesz - 1) & ~(pagesz - 1);
    void *p = mmap(NULL, s, PROT_READ | PROT_WRITE,
                   MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
    if (p == MAP_FAILED) { perror("mmap"); return NULL; }
    return p;
}

/* Build the trampoline for MS x64 incoming ABI; it:
   - stores handler addr in r12
   - pushes caller-saved registers (rax, rcx, rdx, r8, r9, r10, r11)
   - movabs rcx, userdata
   - sub rsp, 0x20 (shadow space)
   - call set_cb_userdata_c (expects RCX)
   - add rsp, 0x20
   - pop saved registers
   - jmp r12 (tail-call into handler; handler uses MS ABI)
*/
void *callback_alloc_trampoline(void *handler, void *userdata) {
    uint8_t code[512];
    uint8_t *p = code;

    // movabs r12, handler  -> 49 BC imm64
    *p++ = 0x49; *p++ = 0xBC;
    memcpy(p, &handler, 8); p += 8;

    // push rax
    *p++ = 0x50;
    // push rcx
    *p++ = 0x51;
    // push rdx
    *p++ = 0x52;
    // push r8
    *p++ = 0x41; *p++ = 0x50;
    // push r9
    *p++ = 0x41; *p++ = 0x51;
    // push r10
    *p++ = 0x41; *p++ = 0x52;
    // push r11
    *p++ = 0x41; *p++ = 0x53;

    // movabs rcx, userdata   -> 48 B9 imm64
    *p++ = 0x48; *p++ = 0xB9;
    memcpy(p, &userdata, 8); p += 8;

    // sub rsp, 0x20
    *p++ = 0x48; *p++ = 0x83; *p++ = 0xEC; *p++ = 0x20;

    // movabs rax, set_cb_userdata_c -> 48 B8 imm64
    void *setter = (void*)set_cb_userdata_c;
    *p++ = 0x48; *p++ = 0xB8;
    memcpy(p, &setter, 8); p += 8;

    // call rax -> FF D0
    *p++ = 0xFF; *p++ = 0xD0;

    // add rsp, 0x20
    *p++ = 0x48; *p++ = 0x83; *p++ = 0xC4; *p++ = 0x20;

    // pop r11
    *p++ = 0x41; *p++ = 0x5B;
    // pop r10
    *p++ = 0x41; *p++ = 0x5A;
    // pop r9
    *p++ = 0x41; *p++ = 0x59;
    // pop r8
    *p++ = 0x41; *p++ = 0x58;
    // pop rdx
    *p++ = 0x5A;
    // pop rcx
    *p++ = 0x59;
    // pop rax
    *p++ = 0x58;

    // jmp r12 -> 41 FF E4
    *p++ = 0x41; *p++ = 0xFF; *p++ = 0xE4;

    size_t len = p - code;
    void *mem = alloc_writable_page(len);
    if (!mem) return NULL;
    memcpy(mem, code, len);

    size_t pagesz = sysconf(_SC_PAGESIZE);
    size_t alloc_size = (len + pagesz - 1) & ~(pagesz - 1);
    if (mprotect(mem, alloc_size, PROT_READ | PROT_EXEC) != 0) {
        perror("mprotect");
        munmap(mem, alloc_size);
        return NULL;
    }
    __builtin___clear_cache((char*)mem, (char*)mem + len);
    return mem;
}

void *callback_make_trampoline(void *handler, void *userdata) {
    // return nullptr;
    return callback_alloc_trampoline(handler, userdata);
    /*std::lock_guard<std::mutex> guard(trampolines_mutex);
    printf("pve 1 0x%016lx\n", userdata);
    if (auto entry = trampolines.find(userdata); entry != trampolines.end())
        return entry->second;

    printf("pve 2\n");
    const auto trampoline = callback_alloc_trampoline(handler, userdata);
    if (!trampoline)
        return NULL;
    printf("pve 3\n");

    trampolines[userdata] = trampoline;
    printf("pve 4\n");
    return trampoline;*/
}
