/* In-process ACE soft patch for official LTZJ 1.25.31 — call ASAP from starry JNI_OnLoad.
 * Offsets: 10_docs/雷霆战机/新ACE_4点重定位_20260817.md
 */
#include "LtzjAceSoft.h"

#include <android/log.h>
#include <dlfcn.h>
#include <errno.h>
#include <link.h>
#include <pthread.h>
#include <signal.h>
#include <stdint.h>
#include <string.h>
#include <sys/mman.h>
#include <sys/ucontext.h>
#include <unistd.h>

#define LOGI(...) __android_log_print(ANDROID_LOG_INFO, "LtzjAceSoft", __VA_ARGS__)
#define LOGE(...) __android_log_print(ANDROID_LOG_ERROR, "LtzjAceSoft", __VA_ARGS__)

static void* g_tersafe2_base = NULL;
static void* g_tprt_base = NULL;

/* tprt executable PT_LOAD ranges (vaddr relative to dlpi_addr). */
static uintptr_t g_tprt_rx_off[4];
static size_t g_tprt_rx_len[4];
static int g_tprt_rx_n = 0;

static int phdr_cb(struct dl_phdr_info* info, size_t size, void* data) {
    (void)size;
    (void)data;
    if (!info || !info->dlpi_name) return 0;
    if (strstr(info->dlpi_name, "libtersafe2.so")) g_tersafe2_base = (void*)info->dlpi_addr;
    if (strstr(info->dlpi_name, "libtprt.so")) {
        g_tprt_base = (void*)info->dlpi_addr;
        g_tprt_rx_n = 0;
        for (int i = 0; i < info->dlpi_phnum && g_tprt_rx_n < 4; i++) {
            const ElfW(Phdr)* ph = &info->dlpi_phdr[i];
            if (ph->p_type != PT_LOAD) continue;
            if ((ph->p_flags & PF_X) == 0) continue;
            g_tprt_rx_off[g_tprt_rx_n] = (uintptr_t)ph->p_vaddr;
            g_tprt_rx_len[g_tprt_rx_n] = (size_t)ph->p_memsz;
            g_tprt_rx_n++;
        }
    }
    return 0;
}

static int patch32(void* base, uintptr_t vma, uint32_t expected, uint32_t val) {
    if (!base) {
        LOGE("patch: base null vma=0x%lx", (unsigned long)vma);
        return 0;
    }
    uintptr_t addr = (uintptr_t)base + vma;
    uintptr_t pagesz = (uintptr_t)sysconf(_SC_PAGESIZE);
    uintptr_t page = addr & ~(pagesz - 1);
    if (mprotect((void*)page, pagesz, PROT_READ | PROT_WRITE | PROT_EXEC) != 0) {
        LOGE("patch: mprotect RWX fail vma=0x%lx", (unsigned long)vma);
        return 0;
    }
    uint32_t* p = (uint32_t*)addr;
    if (*p == val) {
        LOGI("patch: already vma=0x%lx", (unsigned long)vma);
        mprotect((void*)page, pagesz, PROT_READ | PROT_EXEC);
        return 1;
    }
    if (*p != expected) {
        LOGE("patch: MISMATCH vma=0x%lx expect=0x%08x got=0x%08x", (unsigned long)vma, expected, *p);
        mprotect((void*)page, pagesz, PROT_READ | PROT_EXEC);
        return 0;
    }
    *p = val;
    __builtin___clear_cache((char*)addr, (char*)addr + 4);
    if (mprotect((void*)page, pagesz, PROT_READ | PROT_EXEC) != 0) {
        LOGE("patch: mprotect RX restore fail vma=0x%lx", (unsigned long)vma);
    }
    LOGI("patch: ok vma=0x%lx 0x%08x → 0x%08x", (unsigned long)vma, expected, val);
    return 1;
}

void Ltzj_AceSoftPatch12531(void) {
    g_tersafe2_base = NULL;
    g_tprt_base = NULL;
    dl_iterate_phdr(phdr_cb, NULL);
    LOGI("base: tersafe2=%p tprt=%p (official-12531)", g_tersafe2_base, g_tprt_base);
    if (!g_tersafe2_base || !g_tprt_base) {
        LOGE("base not found, skip");
        return;
    }
    patch32(g_tersafe2_base, 0x3ad0b4, 0x97f3f10f, 0xd503201f);
    patch32(g_tprt_base, 0x37fac, 0xa9be7bfd, 0xd65f03c0);
    patch32(g_tprt_base, 0xd8bb8, 0x94003b99, 0x2a1f03e0);
    patch32(g_tprt_base, 0x137b44, 0xa9bc7bfd, 0xd65f03c0);
}

void Ltzj_AceIntegritySoft12531(void) {
    g_tersafe2_base = NULL;
    g_tprt_base = NULL;
    dl_iterate_phdr(phdr_cb, NULL);
    LOGI("integ: tersafe2=%p (sectionhash/filecrc/HashBySyscall)", g_tersafe2_base);
    if (!g_tersafe2_base) {
        LOGE("integ: tersafe2 not mapped");
        return;
    }
    /* blr x8 → mov x0, xzr  (skip vtable integrity call, pretend ok) */
    const uint32_t BLR_X8 = 0xd63f0100;
    const uint32_t MOV_X0_XZR = 0xaa1f03e0;
    int n = 0;
    n += patch32(g_tersafe2_base, 0x37a750, BLR_X8, MOV_X0_XZR);
    n += patch32(g_tersafe2_base, 0x37adb0, BLR_X8, MOV_X0_XZR);
    n += patch32(g_tersafe2_base, 0x37b968, BLR_X8, MOV_X0_XZR);
    LOGI("integ: patched %d/3 dispatch BLRs", n);
}

/* Official 1.25.31 JUMP_SLOT (llvm-readelf --relocs). */
static const uintptr_t kTersafeKillGot = 0x564ea8;
static const uintptr_t kTersafeSigactionGot = 0x564f78;
static const uintptr_t kTersafeSignalGot = 0x565470;
static const uintptr_t kTersafeExitGot = 0x565158;   /* exit@LIBC */
static const uintptr_t kTersafeUnderscoreExitGot = 0x564f30; /* _exit@LIBC */
static const uintptr_t kTersafeAbortGot = 0x565308;
static const uintptr_t kTprtKillGot = 0x165970;
static const uintptr_t kTprtSigactionGot = 0x165820;
static const uintptr_t kTprtUnderscoreExitGot = 0x165a68;

static int (*g_real_kill)(int, int) = nullptr;
static int (*g_real_sigaction)(int, const struct sigaction*, struct sigaction*) = nullptr;
typedef void (*starry_sighandler_t)(int);
static starry_sighandler_t (*g_real_signal)(int, starry_sighandler_t) = nullptr;
static volatile int g_suicide_ready = 0;

static volatile sig_atomic_t g_swallowed_sig = 0;
static volatile sig_atomic_t g_skip_count = 0;

/* Async-signal-safe: no logging here. */
static void starry_sig_skip(int sig, siginfo_t* info, void* uctx) {
    (void)info;
    g_swallowed_sig = (sig_atomic_t)sig;
    g_skip_count++;
#if defined(__aarch64__)
    ucontext_t* uc = (ucontext_t*)uctx;
    if (uc) {
        /* Skip faulting instruction so deliberate ACE bus-fault suicide can continue. */
        uc->uc_mcontext.pc += 4;
    }
#else
    (void)uctx;
#endif
}

static void install_bus_handler(void) {
    struct sigaction sa;
    memset(&sa, 0, sizeof(sa));
    sa.sa_sigaction = starry_sig_skip;
    sigemptyset(&sa.sa_mask);
    sa.sa_flags = SA_SIGINFO | SA_NODEFER | SA_RESTART;
    if (g_real_sigaction) {
        g_real_sigaction(SIGBUS, &sa, nullptr);
        g_real_sigaction(SIGSEGV, &sa, nullptr);
    } else {
        sigaction(SIGBUS, &sa, nullptr);
        sigaction(SIGSEGV, &sa, nullptr);
    }
}

static int starry_kill_stub(int pid, int sig) {
    if (sig == 7 || sig == 11 || sig == 6) {
        LOGI("suicide: blocked kill(pid=%d,sig=%d)", pid, sig);
        return 0;
    }
    if (g_real_kill) return g_real_kill(pid, sig);
    return 0;
}

static int starry_sigaction_stub(int sig, const struct sigaction* act, struct sigaction* oldact) {
    if (sig == SIGBUS || sig == SIGSEGV) {
        LOGI("suicide: blocked sigaction(%d) from ACE, keeping skip-handler", sig);
        if (oldact) {
            memset(oldact, 0, sizeof(*oldact));
            oldact->sa_sigaction = starry_sig_skip;
            oldact->sa_flags = SA_SIGINFO;
        }
        install_bus_handler();
        return 0;
    }
    if (g_real_sigaction) return g_real_sigaction(sig, act, oldact);
    return -1;
}

static starry_sighandler_t starry_signal_stub(int sig, starry_sighandler_t handler) {
    if (sig == SIGBUS || sig == SIGSEGV) {
        LOGI("suicide: blocked signal(%d) from ACE", sig);
        install_bus_handler();
        return SIG_DFL; /* report DFL; actual handler is SA_SIGINFO skip */
    }
    if (g_real_signal) return g_real_signal(sig, handler);
    return SIG_ERR;
}

static int patch_got_ptr(void* base, uintptr_t got_vma, void* replacement, void** saved_out,
                         const char* tag) {
    if (!base) return 0;
    void** slot = (void**)((uintptr_t)base + got_vma);
    uintptr_t pagesz = (uintptr_t)sysconf(_SC_PAGESIZE);
    uintptr_t page = ((uintptr_t)slot) & ~(pagesz - 1);
    if (mprotect((void*)page, pagesz, PROT_READ | PROT_WRITE) != 0) {
        LOGE("suicide: mprotect GOT fail %s errno=%d", tag, errno);
        return 0;
    }
    void* old = *slot;
    if (old == replacement) {
        LOGI("suicide: %s already stub", tag);
        return 1;
    }
    if (saved_out && !*saved_out && old) *saved_out = old;
    *slot = replacement;
    LOGI("suicide: %s GOT %p → stub (was %p)", tag, (void*)slot, old);
    return 1;
}

static void* suicide_watchdog(void* arg) {
    (void)arg;
    sig_atomic_t last = 0;
    for (int i = 0; i < 120 && g_suicide_ready; i++) {
        install_bus_handler();
        sig_atomic_t n = g_skip_count;
        if (n != last) {
            LOGI("suicide: watchdog skips=%d last_sig=%d", (int)n, (int)g_swallowed_sig);
            last = n;
        }
        usleep(500000);
    }
    return nullptr;
}

static void (*g_real_exit)(int) = nullptr;
static void (*g_real__exit)(int) = nullptr;
static void (*g_real_abort)(void) = nullptr;

static void starry_exit_stub(int status) {
    LOGI("suicide: blocked exit(%d)", status);
}

static void starry__exit_stub(int status) {
    LOGI("suicide: blocked _exit(%d)", status);
}

static void starry_abort_stub(void) {
    LOGI("suicide: blocked abort()");
}

/** tprt: mov w0,#7; mov x8,#94 (exit_group); svc #0  → mov w0,#0; ret; nop
 * Only scan PF_X PT_LOAD — blind 0x1A0000 scan hits the LOAD gap @0x15f000 (SEGV_ACCERR). */
static int patch_tprt_exit_group7(void* base) {
    if (!base || g_tprt_rx_n <= 0) {
        LOGE("suicide: exit_group7 skip (no RX phdr)");
        return 0;
    }
    uint8_t* p = (uint8_t*)base;
    const uint32_t a = 0x528000e0; /* mov w0,#7 */
    const uint32_t b = 0xd2800bc8; /* mov x8,#94 */
    const uint32_t c = 0xd4000001; /* svc #0 */
    const uint32_t ra = 0x52800000; /* mov w0,#0 */
    const uint32_t rb = 0xd65f03c0; /* ret */
    const uint32_t rc = 0xd503201f; /* nop */
    int n = 0;
    uintptr_t pagesz = (uintptr_t)sysconf(_SC_PAGESIZE);
    for (int si = 0; si < g_tprt_rx_n; si++) {
        size_t start = g_tprt_rx_off[si];
        size_t end = start + g_tprt_rx_len[si];
        if (end < start + 12) continue;
        for (size_t off = start; off + 12 <= end; off += 4) {
            uint32_t* w = (uint32_t*)(p + off);
            if (w[0] != a || w[1] != b || w[2] != c) continue;
            uintptr_t page = ((uintptr_t)w) & ~(pagesz - 1);
            if (mprotect((void*)page, pagesz * 2, PROT_READ | PROT_WRITE | PROT_EXEC) != 0) {
                LOGE("suicide: mprotect exit_group7 fail off=0x%zx", off);
                continue;
            }
            w[0] = ra;
            w[1] = rb;
            w[2] = rc;
            __builtin___clear_cache((char*)w, (char*)w + 12);
            mprotect((void*)page, pagesz * 2, PROT_READ | PROT_EXEC);
            n++;
        }
    }
    LOGI("suicide: tprt exit_group(7) stubs patched %d (rx_segs=%d)", n, g_tprt_rx_n);
    return n;
}

void Ltzj_AceSuicideSoft12531(void) {
    g_tersafe2_base = NULL;
    g_tprt_base = NULL;
    dl_iterate_phdr(phdr_cb, NULL);
    LOGI("suicide: tersafe2=%p tprt=%p", g_tersafe2_base, g_tprt_base);

    if (g_tersafe2_base && !g_real_sigaction) {
        void** slot = (void**)((uintptr_t)g_tersafe2_base + kTersafeSigactionGot);
        g_real_sigaction = (int (*)(int, const struct sigaction*, struct sigaction*))(*slot);
    }

    int n = 0;
    n += patch_got_ptr(g_tersafe2_base, kTersafeKillGot, (void*)&starry_kill_stub,
                       (void**)&g_real_kill, "tersafe2.kill");
    n += patch_got_ptr(g_tprt_base, kTprtKillGot, (void*)&starry_kill_stub,
                       (void**)&g_real_kill, "tprt.kill");
    n += patch_got_ptr(g_tersafe2_base, kTersafeSigactionGot, (void*)&starry_sigaction_stub,
                       (void**)&g_real_sigaction, "tersafe2.sigaction");
    n += patch_got_ptr(g_tprt_base, kTprtSigactionGot, (void*)&starry_sigaction_stub,
                       (void**)&g_real_sigaction, "tprt.sigaction");
    n += patch_got_ptr(g_tersafe2_base, kTersafeSignalGot, (void*)&starry_signal_stub,
                       (void**)&g_real_signal, "tersafe2.signal");
    n += patch_got_ptr(g_tersafe2_base, kTersafeExitGot, (void*)&starry_exit_stub,
                       (void**)&g_real_exit, "tersafe2.exit");
    n += patch_got_ptr(g_tersafe2_base, kTersafeUnderscoreExitGot, (void*)&starry__exit_stub,
                       (void**)&g_real__exit, "tersafe2._exit");
    n += patch_got_ptr(g_tprt_base, kTprtUnderscoreExitGot, (void*)&starry__exit_stub,
                       (void**)&g_real__exit, "tprt._exit");
    n += patch_got_ptr(g_tersafe2_base, kTersafeAbortGot, (void*)&starry_abort_stub,
                       (void**)&g_real_abort, "tersafe2.abort");
    LOGI("suicide: patched %d GOT slots", n);

    int eg = patch_tprt_exit_group7(g_tprt_base);
    LOGI("suicide: exit_group7=%d", eg);

    install_bus_handler();
    LOGI("suicide: SIGBUS/SEGV pc-skip + watchdog + exit stubs");
    g_suicide_ready = 1;
    pthread_t th;
    if (pthread_create(&th, nullptr, suicide_watchdog, nullptr) == 0) {
        pthread_detach(th);
    }
}
