#ifndef CRINGEBOOTSTRAP_NATIVE_CALLBACK_H
#define CRINGEBOOTSTRAP_NATIVE_CALLBACK_H

#ifdef __cplusplus
extern "C" {
#endif
    void *callback_make_trampoline(void *handler, void *userdata);

    extern __thread void *cb_userdata_tls;
#ifdef __cplusplus
}
#endif

#endif //CRINGEBOOTSTRAP_NATIVE_CALLBACK_H