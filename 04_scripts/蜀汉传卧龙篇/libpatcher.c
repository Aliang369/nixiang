// libpatcher.so —— 进程内自补丁（AB-5）：等待 libMyGame.so → 语义扫描 kill 分支 → 直写 NOP → 重臂
// 复用 flagpatcher2 扫描逻辑；无 syscall 依赖（同地址空间直写 + I-cache 维护）
#define _GNU_SOURCE
#include <jni.h>
#include <android/log.h>
#include <pthread.h>
#include <unistd.h>
#include <string.h>
#include <stdlib.h>
#include <errno.h>
#include <sys/syscall.h>
#include <link.h>
#include <dlfcn.h>
#include <stdio.h>
#include <sys/mman.h>

#define LOG(...) __android_log_print(ANDROID_LOG_INFO, "ShcPatcher", __VA_ARGS__)
#define WIN_LO 0x1e30000u
#define WIN_HI 0x1e5e000u
#define KILL_LO 0xCE2Bu
#define KILL_HI 0x79BAu

static const unsigned int NOP = 0xD503201Fu;

static unsigned long long g_base = 0;

static int phdr_cb(struct dl_phdr_info* info, size_t size, void* data) {
    (void)size;
    if (strstr(info->dlpi_name, "libMyGame.so")) {
        *(unsigned long long*)data = info->dlpi_addr;
        return 1;
    }
    return 0;
}

static void icache(void* p, size_t len) {
    for (size_t i = 0; i < len; i += 64) {
        void* a = (char*)p + i;
        __asm__ volatile("dc cvau, %0" :: "r"(a) : "memory");
        __asm__ volatile("ic ivau, %0" :: "r"(a) : "memory");
    }
    __asm__ volatile("dsb ish" ::: "memory");
    __asm__ volatile("isb" ::: "memory");
}

// 从 /proc/self/maps 取 libMyGame.so 的已映射段（防洞读取）
struct seg { unsigned long long lo, hi; };

static int get_segments(struct seg* out, int max) {
    FILE* f = fopen("/proc/self/maps", "r");
    if (!f) return 0;
    char line[512];
    int n = 0;
    while (fgets(line, sizeof line, f) && n < max) {
        if (!strstr(line, "libMyGame.so")) continue;
        unsigned long long lo = strtoull(line, NULL, 16);
        unsigned long long hi = strtoull(strchr(line, '-') + 1, NULL, 16);
        out[n].lo = lo; out[n].hi = hi; n++;
    }
    fclose(f);
    return n;
}

// ---------- SHC_APK_REDIRECT: GOT 值匹配 hook，base.apk 打开重定向到官方原包 ----------
#ifdef SHC_APK_REDIRECT

static const char* REDIR_PATH = "/data/local/tmp/shc_orig.apk";
static void log_open_caller(const char* path);
static const char* REDIR_ODEX = "/data/local/tmp/shc_orig.odex";
static const char* REDIR_VDEX = "/data/local/tmp/shc_orig.vdex";
static char g_install_apk[256] = {0};   // 抓到的真实安装路径

// 重定向 fd 表（fd → 安装路径），供 readlink 身份伪装
#define MAXFDS 64
static int g_rfd[MAXFDS];
static int g_nrfd = 0;

static void track_fd(int fd) {
    if (fd < 0 || g_nrfd >= MAXFDS) return;
    for (int i = 0; i < g_nrfd; i++) if (g_rfd[i] == fd) return;
    g_rfd[g_nrfd++] = fd;
}

static int (*real_openat)(int, const char*, int, mode_t);
static int (*real_open)(const char*, int, mode_t);
static FILE* (*real_fopen)(const char*, const char*);

static const char* swap_path_ra(const char* p, void* ra) {
    if (!p) return p;
    if (strstr(p, "base.apk") || strstr(p, "base.odex") || strstr(p, "base.vdex")) {
        if (strstr(p, "base.apk") && !g_install_apk[0] && p[0] == '/') {
            strncpy(g_install_apk, p, sizeof g_install_apk - 1);
            LOG("install path captured: %s", g_install_apk);
        }
        Dl_info di; memset(&di, 0, sizeof di);
        if (dladdr(ra, &di) && di.dli_fname) {
            const char* sn = strrchr(di.dli_fname, '/');
            sn = sn ? sn + 1 : di.dli_fname;
            LOG("OPEN-CALLER %s -> %s+%#lx", p, sn, (unsigned long)((char*)ra - (char*)di.dli_fbase));
        } else {
            LOG("OPEN-CALLER %s -> ra=%p (anon)", p, ra);
        }
        LOG("REDIRECT open(%s)", p);
        if (strstr(p, "base.odex")) return REDIR_ODEX;
        if (strstr(p, "base.vdex")) return REDIR_VDEX;
        return REDIR_PATH;
    }
    return p;
}

static int hook_openat(int dirfd, const char* path, int flags, mode_t mode) {
    int fd = real_openat(dirfd, swap_path_ra(path, __builtin_return_address(0)), flags, mode);
    if (fd >= 0 && path && (strstr(path, "base.apk") || strstr(path, "base.odex") || strstr(path, "base.vdex")))
        track_fd(fd);
    return fd;
}
static int hook_open(const char* path, int flags, mode_t mode) {
    int fd = real_open(swap_path_ra(path, __builtin_return_address(0)), flags, mode);
    if (fd >= 0 && path && (strstr(path, "base.apk") || strstr(path, "base.odex") || strstr(path, "base.vdex")))
        track_fd(fd);
    return fd;
}
static FILE* hook_fopen(const char* path, const char* m) {
    FILE* f = real_fopen(swap_path_ra(path, __builtin_return_address(0)), m);
    if (f && path && (strstr(path, "base.apk") || strstr(path, "base.odex") || strstr(path, "base.vdex")))
        track_fd(fileno(f));
    return f;
}

// ---------- readlink 身份伪装：重定向 fd 的 /proc/self/fd/N 指回安装路径 ----------
static ssize_t (*real_readlinkat)(int, const char*, char*, size_t);
static ssize_t hook_readlinkat(int dirfd, const char* path, char* buf, size_t size) {
    ssize_t r = real_readlinkat(dirfd, path, buf, size);
    if (r > 0 && path && (strncmp(path, "/proc/self/fd/", 14) == 0 || strncmp(path, "/proc/thread-self/fd/", 21) == 0)) {
        const char* fp = strrchr(path, '/');
        int fd = fp ? atoi(fp + 1) : -1;
        for (int i = 0; i < g_nrfd; i++) {
            if (g_rfd[i] == fd && g_install_apk[0]) {
                strncpy(buf, g_install_apk, size);
                buf[size - 1] = 0;
                LOG("READLINK spoofed fd=%d", fd);
                return (ssize_t)strlen(buf);
            }
        }
    }
    return r;
}

// ---------- kill/tgkill 拦截（归因 + 阻断） ----------
static int (*real_kill)(int, int);
static int (*real_tgkill)(int, int, int);
static int hook_kill(int pid, int sig) {
    LOG("KILL-blocked pid=%d sig=%d", pid, sig);
    return 0;
}
static int hook_tgkill(int tgid, int tid, int sig) {
    LOG("TGKILL-blocked tgid=%d tid=%d sig=%d", tgid, tid, sig);
    return 0;
}

// ---------- exit/abort 拦截（点名调用者 + 拒绝退出） ----------
static void (*real_exit)(int);
static void (*real__exit)(int);
static void (*real_abort)(void);

static void log_caller(const char* what, int code) {
    void* ra = __builtin_return_address(0);
    unsigned long long mybase = 0, fgbase = 0;
    dl_iterate_phdr(phdr_cb, &mybase);
    Dl_info di; memset(&di, 0, sizeof di);
    dladdr(ra, &di);
    LOG("%s code=%d ra=%p (%s:%p) mygame_base=0x%llx",
        what, code, ra, di.dli_fname ? di.dli_fname : "?", di.dli_fbase, mybase);
}

// open 调用者定位（相对模块基址偏移，用于 dump 反汇编定位）
static void log_open_caller(const char* path) {
    void* ra = __builtin_return_address(0);
    Dl_info di; memset(&di, 0, sizeof di);
    if (dladdr(ra, &di) && di.dli_fname) {
        const char* shortn = strrchr(di.dli_fname, '/');
        shortn = shortn ? shortn + 1 : di.dli_fname;
        LOG("OPEN-CALLER %s -> %s+%#lx (fbase=%p)", path, shortn,
            (unsigned long)((char*)ra - (char*)di.dli_fbase), di.dli_fbase);
    } else {
        LOG("OPEN-CALLER %s -> ra=%p (anon)", path, ra);
    }
}

static void hook_exit(int code) {
    log_caller("EXIT-blocked", code);
    return;  // 拒绝退出
}
static void hook_exit_(int code) {
    log_caller("_EXIT-blocked", code);
    return;
}
static void hook_abort(void) {
    log_caller("ABORT-blocked", 0);
    return;
}

// 扫描模块可写段 GOT：值等于 libc 函数地址的槽 → 替换为 hook（不需符号名）
static void hook_module_got(const char* mod, void* fn, void* hk, void** saved) {
    FILE* f = fopen("/proc/self/maps", "r");
    if (!f) return;
    char line[512];
    struct seg wl[64];
    int nw = 0;
    while (fgets(line, sizeof line, f) && nw < 64) {
        if (!strstr(line, mod)) continue;
        char* perms = strchr(line, ' ');
        if (!perms) continue;
        if (perms[1] != 'r') continue;                 // 必须可读
        if (perms[3] == 'x') continue;                 // 跳过可执行段（代码区）
        unsigned long long lo = strtoull(line, NULL, 16);
        unsigned long long hi = strtoull(strchr(line, '-') + 1, NULL, 16);
        if (hi - lo > (64ULL << 20)) continue;         // 跳过超大匿名段
        wl[nw].lo = lo; wl[nw].hi = hi; nw++;
    }
    fclose(f);
    if (!nw) { LOG("hook %s: no data segs", mod); return; }
    int n = 0;
    for (int si = 0; si < nw; si++) {
        for (unsigned long long p = wl[si].lo; p + 8 <= wl[si].hi; p += 8) {
            unsigned long long v = *(unsigned long long*)p;
            if (v != (unsigned long long)fn) continue;
            unsigned long long pg = p & ~0xFFFULL;
            if (mprotect((void*)pg, 0x1000, PROT_READ | PROT_WRITE) != 0) {
                LOG("hook %s: mprotect fail @0x%llx", mod, p);
                continue;
            }
            if (saved && !*saved) *saved = (void*)v;
            *(unsigned long long*)p = (unsigned long long)hk;
            n++;
        }
    }
    LOG("hook %s fn=%p -> %p slots=%d", mod, fn, hk, n);
}

static void redirect_install(void) {
    void* h = dlopen("libc.so", RTLD_NOW);
    if (!h) { LOG("dlopen libc fail"); return; }
    real_openat = (int (*)(int, const char*, int, mode_t))dlsym(h, "openat");
    real_open = (int (*)(const char*, int, mode_t))dlsym(h, "open");
    real_fopen = (FILE* (*)(const char*, const char*))dlsym(h, "fopen");
    LOG("libc openat=%p open=%p fopen=%p", (void*)real_openat, (void*)real_open, (void*)real_fopen);
    if (real_openat) {
        static void* s1; s1 = NULL;
        hook_module_got("libMyGame.so", (void*)real_openat, (void*)hook_openat, &s1);
        hook_module_got("libFairGuard.so", (void*)real_openat, (void*)hook_openat, &s1);
    }
    if (real_open) {
        static void* s2; s2 = NULL;
        hook_module_got("libMyGame.so", (void*)real_open, (void*)hook_open, &s2);
        hook_module_got("libFairGuard.so", (void*)real_open, (void*)hook_open, &s2);
    }
    if (real_fopen) {
        static void* s3; s3 = NULL;
        hook_module_got("libMyGame.so", (void*)real_fopen, (void*)hook_fopen, &s3);
        hook_module_got("libFairGuard.so", (void*)real_fopen, (void*)hook_fopen, &s3);
    }
    // exit/abort 拦截（同库 GOT，值匹配）
    real_exit = (void (*)(int))dlsym(h, "exit");
    real__exit = (void (*)(int))dlsym(h, "_exit");
    real_abort = (void (*)(void))dlsym(h, "abort");
    if (real_exit) {
        static void* s4; s4 = NULL;
        hook_module_got("libMyGame.so", (void*)real_exit, (void*)hook_exit, &s4);
        hook_module_got("libFairGuard.so", (void*)real_exit, (void*)hook_exit, &s4);
        hook_module_got("libart.so", (void*)real_exit, (void*)hook_exit, &s4);
    }
    if (real__exit) {
        static void* s5; s5 = NULL;
        hook_module_got("libMyGame.so", (void*)real__exit, (void*)hook_exit_, &s5);
        hook_module_got("libFairGuard.so", (void*)real__exit, (void*)hook_exit_, &s5);
        hook_module_got("libart.so", (void*)real__exit, (void*)hook_exit_, &s5);
    }
    if (real_abort) {
        static void* s6; s6 = NULL;
        hook_module_got("libMyGame.so", (void*)real_abort, (void*)hook_abort, &s6);
        hook_module_got("libFairGuard.so", (void*)real_abort, (void*)hook_abort, &s6);
        hook_module_got("libart.so", (void*)real_abort, (void*)hook_abort, &s6);
    }
    real_readlinkat = (ssize_t (*)(int, const char*, char*, size_t))dlsym(h, "readlinkat");
    if (real_readlinkat) {
        static void* s7; s7 = NULL;
        hook_module_got("libMyGame.so", (void*)real_readlinkat, (void*)hook_readlinkat, &s7);
        hook_module_got("libFairGuard.so", (void*)real_readlinkat, (void*)hook_readlinkat, &s7);
    }
    real_kill = (int (*)(int, int))dlsym(h, "kill");
    real_tgkill = (int (*)(int, int, int))dlsym(h, "tgkill");
    if (real_kill) {
        static void* s8; s8 = NULL;
        hook_module_got("libMyGame.so", (void*)real_kill, (void*)hook_kill, &s8);
        hook_module_got("libFairGuard.so", (void*)real_kill, (void*)hook_kill, &s8);
    }
    if (real_tgkill) {
        static void* s9; s9 = NULL;
        hook_module_got("libMyGame.so", (void*)real_tgkill, (void*)hook_tgkill, &s9);
        hook_module_got("libFairGuard.so", (void*)real_tgkill, (void*)hook_tgkill, &s9);
    }
}

static void* redirect_thread(void* arg) {
    (void)arg;
    // 等 libc/两库加载完（onCreate 时游戏库已加载，稍等兜底）
    for (int i = 0; i < 50; i++) {
        FILE* f = fopen("/proc/self/maps", "r");
        int ok = 0;
        if (f) { char line[512]; while (fgets(line, sizeof line, f)) {
            if (strstr(line, "libFairGuard.so") && strstr(line, "r--")) ok = 1; } fclose(f); }
        if (ok) break;
        usleep(100000);
    }
    redirect_install();
    return NULL;
}
#endif

static void* patch_thread(void* arg) {
    (void)arg;
#ifdef SHC_APK_REDIRECT
    redirect_thread(NULL);
#endif
    for (int i = 0; i < 300 && !g_base; i++) {
        dl_iterate_phdr(phdr_cb, &g_base);
        if (!g_base) usleep(100000);
    }
    if (!g_base) { LOG("NO_BASE"); return NULL; }
    LOG("BASE=0x%llx", g_base);

    struct seg segs[8];
    static unsigned int win[(WIN_HI - WIN_LO) / 4];
    int patches = 0;
    for (long t = 0; t < 60000; t += 200) {
        int ns = get_segments(segs, 8);
        for (int si = 0; si < ns; si++) {
            unsigned long long lo = segs[si].lo, hi = segs[si].hi;
            if (lo < g_base + WIN_LO) lo = g_base + WIN_LO;
            if (hi > g_base + WIN_HI) hi = g_base + WIN_HI;
            if (lo >= hi) continue;
            size_t bytes = (size_t)(hi - lo);
            memcpy(win, (void*)lo, bytes);
            int n = (int)(bytes / 4);
            for (int i = 0; i + 1 < n && i + 18 < n; i++) {
                unsigned w1 = win[i], w2 = win[i + 1];
                unsigned d1 = w1 & 0x1F, d2 = w2 & 0x1F;
                if (d1 != d2) continue;
                if ((w1 & 0xFFFFFFE0u) != (0x52800000u | (KILL_LO << 5))) continue;
                if ((w2 & 0xFFFFFFE0u) != (0x72A00000u | (KILL_HI << 5))) continue;
                for (int j = i + 2; j < i + 18; j++) {
                    unsigned b = win[j];
                    if ((b & 0xFF000010u) == 0x54000000u) {   // b.cond
                        unsigned int* site = (unsigned int*)(lo + (unsigned long long)j * 4);
                        if (*site != NOP) {
                            unsigned long long pg = (unsigned long long)site & ~0xFFFULL;
                            long plen = 0x1000;
                            mprotect((void*)pg, plen, PROT_READ | PROT_WRITE | PROT_EXEC);
                            *site = NOP;
                            icache(site, 4);
                            LOG("PATCH rel=0x%llx insn=%08x t=%ldms",
                                (unsigned long long)((char*)site - (char*)g_base), b, t);
                            patches++;
                        }
                        break;
                    }
                }
            }
            // exit_group 内联 wrapper：MOVZ w8/x8,#94 → (≤4 条内) SVC #0 → 改为 mov w0,#0
            for (int i = 0; i + 4 < n; i++) {
                unsigned m = win[i];
                if (m != 0x52800BC0u && m != 0xD2800BC0u) continue;  // movz w8/x8,#94
                for (int j = i + 1; j <= i + 4 && j < n; j++) {
                    if (win[j] == 0xD4000001u) {                     // svc #0
                        unsigned int* site = (unsigned int*)(lo + (unsigned long long)j * 4);
                        const unsigned int MOVW0_0 = 0x52800000u;
                        if (*site != MOVW0_0) {
                            unsigned long long pg = (unsigned long long)site & ~0xFFFULL;
                            mprotect((void*)pg, 0x1000, PROT_READ | PROT_WRITE | PROT_EXEC);
                            *site = MOVW0_0;
                            icache(site, 4);
                            LOG("PATCH-EXITGRP rel=0x%llx t=%ldms",
                                (unsigned long long)((char*)site - (char*)g_base), t);
                            patches++;
                        }
                        break;
                    }
                }
            }
        }
        usleep(200000);
    }
    LOG("DONE patches=%d", patches);
    return NULL;
}

__attribute__((constructor))
static void on_load(void) {
#ifdef SHC_SECC_TRAP
    // 取证：exit/exit_group syscall → SIGSYS（tombstone backtrace 点名调用者）
    {
#include <linux/filter.h>
#include <linux/seccomp.h>
#include <linux/audit.h>
#include <sys/prctl.h>
#include <sys/syscall.h>
        struct sock_filter code[] = {
            BPF_STMT(BPF_LD | BPF_W | BPF_ABS, 0),                    // nr
            BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, __NR_exit_group, 0, 1),
            BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, __NR_exit, 0, 1),
            BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_TRAP),
            BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW),
        };
        struct sock_fprog prog = { .len = (unsigned short)5, .filter = code };
        if (prctl(PR_SET_NO_NEW_PRIVS, 1, 0, 0, 0) == 0 &&
            syscall(__NR_seccomp, SECCOMP_SET_MODE_FILTER, 1, &prog) == 0) {
            LOG("seccomp TRAP armed");
        } else {
            LOG("seccomp arm FAILED errno=%d", errno);
        }
    }
#endif
    pthread_t th;
    pthread_create(&th, NULL, patch_thread, NULL);
    pthread_detach(th);
    LOG("loaded");
}

// ---------- SHC_JNI_SIGSPOOF: JNI 函数表层伪装 PackageInfo.signatures ----------
#ifdef SHC_JNI_SIGSPOOF
#include <stddef.h>

static const unsigned char OFF_CERT[] = {
#include "shc_cert_bytes.h"
};

static JavaVM* g_vm = NULL;
static void* orig_GetFieldID = NULL;
static void* orig_GetObjectField = NULL;
static void* orig_GetMethodID = NULL;
static void* orig_FindClass = NULL;
static jclass g_proxy_class = NULL;
static jmethodID g_proxy_ctor = NULL;
static jobject g_sig_array = NULL;      // 官方 Signature[] 缓存（global ref）
static void** g_sig_fieldids = NULL;    // 被记录的 signatures fieldID 池
static int g_nsigfids = 0;
static void* g_mid_content_signers = NULL;   // SigningInfo.getApkContentsSigners
static void* g_mid_history = NULL;           // getSigningCertificateHistory
static void* g_mid_has_multi = NULL;         // hasMultipleSigners
static void* g_fid_signing_info = NULL;      // PackageInfo.signingInfo fieldID

JNIEXPORT jint JNI_OnLoad(JavaVM* vm, void* reserved);

static jobject make_sig_array(JNIEnv* env) {
    if (g_sig_array) return g_sig_array;
    jclass cls = (*env)->FindClass(env, "android/content/pm/Signature");
    if (!cls) { LOG("sigspoof: no Signature class"); return NULL; }
    jmethodID ctor = (*env)->GetMethodID(env, cls, "<init>", "([B)V");
    if (!ctor) { LOG("sigspoof: no ctor"); return NULL; }
    jbyteArray arr = (*env)->NewByteArray(env, (jsize)sizeof OFF_CERT);
    if (!arr) return NULL;
    (*env)->SetByteArrayRegion(env, arr, 0, (jsize)sizeof OFF_CERT, (const jbyte*)OFF_CERT);
    jobject sig = (*env)->NewObject(env, cls, ctor, arr);
    if (!sig) { LOG("sigspoof: NewObject fail"); return NULL; }
    jobjectArray out = (*env)->NewObjectArray(env, 1, cls, sig);
    if (!out) return NULL;
    g_sig_array = (*env)->NewGlobalRef(env, out);
    LOG("sigspoof: official Signature[] built (%zu bytes)", sizeof OFF_CERT);
    return g_sig_array;
}

static jfieldID hook_GetFieldID(JNIEnv* env, jclass clazz, const char* name, const char* sig) {
    jfieldID fid = ((jfieldID (*)(JNIEnv*, jclass, const char*, const char*))orig_GetFieldID)(env, clazz, name, sig);
    if (name && fid) {
        if (strcmp(name, "signatures") == 0 && sig && strcmp(sig, "[Landroid/content/pm/Signature;") == 0) {
            if (g_nsigfids < 16) {
                int seen = 0;
                for (int i = 0; i < g_nsigfids; i++) if (g_sig_fieldids[i] == (void*)fid) seen = 1;
                if (!seen) { g_sig_fieldids[g_nsigfids++] = (void*)fid;
                    LOG("sigspoof: signatures fieldID #%d (%p)", g_nsigfids, fid); }
            }
        } else if (strcmp(name, "signingInfo") == 0) {
            g_fid_signing_info = (void*)fid;
            LOG("sigspoof: signingInfo fieldID tracked (%p)", fid);
        } else if (strstr(name, "sign") || strstr(name, "Sign")) {
            LOG("sigspoof: GetFieldID(%s,%s) -> %p", name, sig ? sig : "?", fid);
        }
    }
    return fid;
}

static int caller_is_ra(const char* mod, void* ra) {
    Dl_info di; memset(&di, 0, sizeof di);
    if (dladdr(ra, &di) && di.dli_fname && strstr(di.dli_fname, mod)) return 1;
    return 0;
}

static jobject make_proxy(JNIEnv* env) {
    if (!g_proxy_class) return NULL;
    jobject o = (*env)->NewObject(env, g_proxy_class, g_proxy_ctor);
    LOG("sigspoof: SigningInfo -> proxy instance %p", o);
    return o;
}

static jobject hook_GetObjectField(JNIEnv* env, jobject obj, jfieldID fid) {
    void* ra = __builtin_return_address(0);
    for (int i = 0; i < g_nsigfids; i++) {
        if (g_sig_fieldids[i] == (void*)fid) {
            jobject a = make_sig_array(env);
            if (a) return a;
        }
    }
    if (g_fid_signing_info && (void*)fid == g_fid_signing_info && caller_is_ra("libFairGuard", ra)) {
        jobject p = make_proxy(env);
        if (p) return p;
    }
    return ((jobject (*)(JNIEnv*, jobject, jfieldID))orig_GetObjectField)(env, obj, fid);
}

static jclass hook_FindClass(JNIEnv* env, const char* name) {
    if (name && strcmp(name, "android/content/pm/SigningInfo") == 0 && caller_is_ra("libFairGuard", __builtin_return_address(0)) && g_proxy_class) {
        LOG("sigspoof: FindClass(SigningInfo) -> proxy class");
        return g_proxy_class;
    }
    return ((jclass (*)(JNIEnv*, const char*))orig_FindClass)(env, name);
}

static jmethodID hook_GetMethodID(JNIEnv* env, jclass clazz, const char* name, const char* sig) {
    jmethodID mid = ((jmethodID (*)(JNIEnv*, jclass, const char*, const char*))orig_GetMethodID)(env, clazz, name, sig);
    if (name && mid) {
        if (strcmp(name, "getApkContentsSigners") == 0) {
            g_mid_content_signers = (void*)mid;
            LOG("sigspoof: getApkContentsSigners tracked (%p)", mid);
        } else if (strcmp(name, "getSigningCertificateHistory") == 0) {
            g_mid_history = (void*)mid;
            LOG("sigspoof: getSigningCertificateHistory tracked (%p)", mid);
        } else if (strcmp(name, "hasMultipleSigners") == 0) {
            g_mid_has_multi = (void*)mid;
        } else if ((strstr(name, "sign") || strstr(name, "Sign")) && !strstr(name, "design")) {
            LOG("sigspoof: GetMethodID(%s%s%s) -> %p", name, sig ? "," : "", sig ? sig : "", mid);
        }
    }
    return mid;
}


static void jni_sigspoof_install(JNIEnv* env) {
    struct JNINativeInterface* tbl = *env;
    void** gf = (void**)&tbl->GetFieldID;
    void** gof = (void**)&tbl->GetObjectField;
    void** gmi = (void**)&tbl->GetMethodID;
    void** fc = (void**)&tbl->FindClass;
    unsigned long long pages[4];
    void** slots[4] = { gf, gof, gmi, fc };
    for (int i = 0; i < 4; i++) pages[i] = (unsigned long long)slots[i] & ~0xFFFULL;
    for (int i = 0; i < 4; i++)
        for (int j = 0; j < i; j++)
            if (pages[i] == pages[j]) pages[j] = 0;
    for (int i = 0; i < 4; i++)
        if (pages[i]) mprotect((void*)pages[i], 0x1000, PROT_READ | PROT_WRITE);
    orig_GetFieldID = *gf; orig_GetObjectField = *gof;
    orig_GetMethodID = *gmi; orig_FindClass = *fc;
    *gf = (void*)hook_GetFieldID;
    *gof = (void*)hook_GetObjectField;
    *gmi = (void*)hook_GetMethodID;
    *fc = (void*)hook_FindClass;
    // 代理类
    jclass pc = ((jclass (*)(JNIEnv*, const char*))orig_FindClass)(env, "com/bantu/trgame/ShcSignProxy");
    if (pc) {
        g_proxy_class = (*env)->NewGlobalRef(env, pc);
        g_proxy_ctor = (*env)->GetMethodID(env, pc, "<init>", "()V");
        LOG("sigspoof: proxy class ready (%p)", g_proxy_class);
    } else {
        LOG("sigspoof: proxy class NOT found");
    }
    LOG("sigspoof: JNI table patched gf=%p gof=%p gmi=%p fc=%p",
        (void*)tbl->GetFieldID, (void*)tbl->GetObjectField,
        (void*)tbl->GetMethodID, (void*)tbl->FindClass);
}
#endif
// ---------- sigspoof end ----------

JNIEXPORT jint JNI_OnLoad(JavaVM* vm, void* reserved) {
    (void)reserved;
#ifdef SHC_JNI_SIGSPOOF
    g_vm = vm;
    JNIEnv* env = NULL;
    if ((*vm)->GetEnv(vm, (void**)&env, JNI_VERSION_1_6) == JNI_OK && env) {
        g_sig_fieldids = (void**)calloc(16, sizeof(void*));
        jni_sigspoof_install(env);
        LOG("sigspoof: installed");
    }
#endif
    return JNI_VERSION_1_6;
}
