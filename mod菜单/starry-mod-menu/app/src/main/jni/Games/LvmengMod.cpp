/* ================================================================
 *  绿梦：时空之声 — per-game 壳实现
 *  1) 菜单状态 → starry_lvmeng_state.txt 的落盘（原逻辑）
 *  2) FairGuard 输入侧伪装（Stage 6）：把 FairGuard 读到的 base.apk
 *     换成"原厂签名的那份"，让它读到原厂证书 —— 不触碰 FairGuard 任何代码/页。
 *
 *  状态文件用独立名 starry_lvmeng_state.txt，**避开框架 StateMirror**：
 *  StateMirror 在注入形态下也会往 {filesDir}/starry_state.txt 写 8 整数
 *  （那是给「独立助手 + 其它 HybridCLR 游戏」用的通道），与绿梦 native 壳
 *  的 9 整数 S1 格式同文件互覆（真机已复现：文件被覆成 8 整数，C# 读到
 *  n<9 直接 return，数值功能全失效）。绿梦 C# 只读本文件的 9 整数 S1 格式。
 */
#include "LvmengMod.h"

#include <android/log.h>
#include <cstdio>
#include <cstring>
#include <cstdlib>
#include <mutex>
#include <unistd.h>

/* ===== Stage 6：FairGuard 输入侧伪装 ===== */
#include <fcntl.h>
#include <pthread.h>
#include <cstdarg>
#include <cstdint>
#include <sys/stat.h>
#include <sys/system_properties.h>
#include <errno.h>
#include <dlfcn.h>
#include "Dobby/dobby.h"

/* ===== Stage 7（2026-10-20）：吞掉 FairGuard 对 UnityMain 发的 SIGILL =====
 * 实测：102–157 s 由别的线程 tgkill(UnityMain, SIGILL) 打死；handler 直接返回即吞掉。 */
#include <signal.h>
#include <sys/syscall.h>
/* 2026-10-20 05:5x（兜底轮）：CrashSight 在 ~12 s 装自己的 SIGILL 处理器，把我们的顶掉；
 * 于是 FairGuard 的 tgkill(UnityMain,SIGILL) 变成「上报 → uploadFinished → Java finish 回桌面」。
 * 本版：① **全吞**（含 ILL_ILLOPC —— 05:5x 实测把真故障交回原处理器会让进程在 ~38 s 被打死，
 *          而旧「全吞」版能活 318 s：滚动变换的非法字是瞬态，返回后重执行即可恢复）；
 *       ② 每 500 ms 抢回一次 SIGILL 处理器，让 CrashSight 再也接不到那一发（不再上报 → Java 不 finish）；
 *       ③ 只计数（SI_TKILL 记「伪造」、其余记「瞬态」），不改变行为。 */
static struct sigaction g_saSigillOur;
static struct sigaction g_saSigillOld;
static volatile int g_haveSigillOld = 0;
static volatile int g_sigillSwallowed = 0;
static volatile int g_sigillReal = 0;
static volatile int g_sigillReArmed = 0;

static void Starry_SigillSwallow(int sig, siginfo_t* si, void* uc) {
    (void)sig; (void)uc;
    if (si && si->si_code == SI_TKILL)
        __atomic_fetch_add(&g_sigillSwallowed, 1, __ATOMIC_RELAXED);   /* 伪造投递 */
    else
        __atomic_fetch_add(&g_sigillReal, 1, __ATOMIC_RELAXED);        /* 滚动变换瞬态非法字 */
    return;   /* 全吞：返回后内核重执行该指令，瞬态恢复即继续 */
}

/* L1 门禁要求：不出现裸 __android_log_print（用宏包装，与 Seccomp 的 SE_LOG* 同款） */
#define LM_LOGI(...) __android_log_print(ANDROID_LOG_INFO, "StarryLvmeng", __VA_ARGS__)
#define LM_LOGW(...) __android_log_print(ANDROID_LOG_WARN, "StarryLvmeng", __VA_ARGS__)

static void* Starry_SigillRearmWorker(void*) {
    for (int i = 0; i < 3600; i++) {               /* 30 min 上限 */
        usleep(500 * 1000);
        struct sigaction cur;
        if (sigaction(SIGILL, nullptr, &cur) != 0) continue;
        if (cur.sa_sigaction != Starry_SigillSwallow) {
            __atomic_fetch_add(&g_sigillReArmed, 1, __ATOMIC_RELAXED);
            sigaction(SIGILL, &g_saSigillOur, nullptr);
            LM_LOGW("SIGILL 处理器被覆盖(现=%p) -> 已抢回(第 %d 次)",
                    (void*)cur.sa_sigaction, g_sigillReArmed);
        }
        if ((i % 20) == 0 && (g_sigillSwallowed || g_sigillReal))
            LM_LOGI("SIGILL 统计：吞伪造=%d 转交真故障=%d 抢回=%d",
                    g_sigillSwallowed, g_sigillReal, g_sigillReArmed);
    }
    return nullptr;
}

__attribute__((constructor)) static void Starry_InstallSigillHandler() {
    memset(&g_saSigillOur, 0, sizeof(g_saSigillOur));
    g_saSigillOur.sa_sigaction = Starry_SigillSwallow;
    g_saSigillOur.sa_flags = SA_SIGINFO | SA_RESTART | SA_NODEFER;
    sigemptyset(&g_saSigillOur.sa_mask);
    if (sigaction(SIGILL, &g_saSigillOur, &g_saSigillOld) == 0 && g_saSigillOld.sa_sigaction)
        g_haveSigillOld = 1;
    pthread_t th;
    if (pthread_create(&th, nullptr, Starry_SigillRearmWorker, nullptr) == 0) pthread_detach(th);
}

namespace fgspoof {

const char* kSpoofPath = "/data/data/com.zhuziplay.lvmengnew.op/files/fg_orig.apk";
const char* kLogPath   = "/data/data/com.zhuziplay.lvmengnew.op/files/starry_loader.log";
const char* kAssetName = "assets/lvmeng/fg_orig.apk";   /* 包内 STORED 的完整原厂 base.apk */

static int  (*o_open)(const char*, int, ...)        = nullptr;
static int  (*o_openat)(int, const char*, int, ...) = nullptr;
/* 诊断用（只观测 FairGuard 源的读取，不改变行为） */
static ssize_t (*o_read)(int, void*, size_t)              = nullptr;
static ssize_t (*o_pread64)(int, void*, size_t, off64_t)  = nullptr;
static void*   (*o_mmap)(void*, size_t, int, int, int, off_t) = nullptr;
static int     (*o_fstat)(int, struct stat*)              = nullptr;
static ssize_t (*o_readlink)(const char*, char*, size_t)  = nullptr;
static bool g_ready = false;
static uintptr_t g_fg_lo = 0, g_fg_hi = 0;
static int  g_hits = 0;
/* --- 伪装轮（2026-10-20 05:2x）：把 FairGuard 的 open(base.apk) 换成原厂包 ---
 * 依据：cert_reader_mode7_20261020.md —— FairGuard 只读「EOCD 窗口 278B + 签名块 footer 24B
 * + 整个 APK Sig Block 4096B」三段（自有 svc 桩，files 里的 3 条 lseek），然后从 v2 块里取出
 * 证书 DER 送去比对；真比对在混淆解释器里（4 路表派发），不适合直接改码。
 * 换成原厂包后，这三段读到的都是「原厂签名块 + 原厂证书」⇒ 比对自然为真，且不碰 FairGuard 任何页
 * （避开 ≤0.3 s 的完整性反制）。prop debug.starry.lvmeng_fgspoof=0 可退回「只记录不替换」。 */
static int  g_spoofFd = -1;
static int  g_spoofOn = 1;
static char g_apkPath[512] = { 0 };
static int  (*o_stat)(const char*, struct stat*) = nullptr;

void logger(const char* fmt, ...) {
    char b[512];
    va_list ap; va_start(ap, fmt); vsnprintf(b, sizeof(b), fmt, ap); va_end(ap);
    LM_LOGI("%s", b);
    FILE* f = fopen(kLogPath, "ab");
    if (f) { fprintf(f, "[fgspoof] %s\n", b); fclose(f); }
}

static uint32_t rd32(const unsigned char* p) { return (uint32_t)p[0] | ((uint32_t)p[1] << 8) | ((uint32_t)p[2] << 16) | ((uint32_t)p[3] << 24); }
static uint16_t rd16(const unsigned char* p) { return (uint16_t)(p[0] | (p[1] << 8)); }

static bool findFgRange() {
    FILE* f = fopen("/proc/self/maps", "r");
    if (!f) return false;
    char line[1024];
    uintptr_t lo = 0, hi = 0;
    while (fgets(line, sizeof(line), f)) {
        if (!strstr(line, "libFairGuard.so")) continue;
        unsigned long long s = 0, e = 0;
        if (sscanf(line, "%llx-%llx", &s, &e) != 2) continue;
        if (!lo || (uintptr_t)s < lo) lo = (uintptr_t)s;
        if ((uintptr_t)e > hi) hi = (uintptr_t)e;
    }
    fclose(f);
    g_fg_lo = lo; g_fg_hi = hi;
    return lo != 0;
}

static bool findApkPath(char* out, size_t n) {
    FILE* f = fopen("/proc/self/maps", "r");
    if (!f) return false;
    char line[1024];
    bool ok = false;
    while (fgets(line, sizeof(line), f)) {
        char* p = strstr(line, "/data/app/");
        if (!p) continue;
        char* q = strstr(p, "/base.apk");
        if (!q) continue;
        char* e = p; while (*e && *e != '\n') e++;
        size_t len = (size_t)(e - p);
        if (len >= n) len = n - 1;
        memcpy(out, p, len); out[len] = 0;
        ok = true; break;
    }
    fclose(f);
    return ok;
}

/* 从"包内 STORED 资产"里抽出原厂 base.apk（最小 zip 解析：EOCD → 中央目录 → 本地头） */
static bool extractAsset(const char* apk, const char* out, long long* outSize) {
    int fd = ::open(apk, O_RDONLY | O_CLOEXEC);
    if (fd < 0) { logger("extract: open apk failed %d", errno); return false; }
    struct stat st;
    if (fstat(fd, &st) != 0 || st.st_size < 1024) { logger("extract: fstat failed"); close(fd); return false; }
    long long fsz = (long long)st.st_size;
    size_t tl = 66000; if ((long long)tl > fsz) tl = (size_t)fsz;
    unsigned char* tail = (unsigned char*)malloc(tl);
    if (!tail) { close(fd); return false; }
    if (pread(fd, tail, tl, fsz - (long long)tl) != (ssize_t)tl) { logger("extract: tail read failed"); free(tail); close(fd); return false; }
    long long eocd = -1;
    for (long long i = (long long)tl - 22; i >= 0; i--) if (rd32(tail + i) == 0x06054b50) { eocd = i; break; }
    if (eocd < 0) { logger("extract: no EOCD"); free(tail); close(fd); return false; }
    unsigned cnt = rd16(tail + eocd + 10);
    uint32_t cdOff = rd32(tail + eocd + 16);
    free(tail);
    unsigned char hdr[46];
    bool found = false;
    long long dataOff = 0; long long usize = 0;
    for (unsigned k = 0; k < cnt; k++) {
        if (pread(fd, hdr, 46, (long long)cdOff) != 46) break;
        if (rd32(hdr) != 0x02014b50) break;
        unsigned nl = rd16(hdr + 28), el = rd16(hdr + 30), cl = rd16(hdr + 32);
        uint32_t lo = rd32(hdr + 42);
        long long us = (long long)rd32(hdr + 24);
        unsigned method = rd16(hdr + 10);
        char name[300];
        memset(name, 0, sizeof(name));
        unsigned want = nl < sizeof(name) - 1 ? nl : (unsigned)(sizeof(name) - 1);
        pread(fd, name, want, (long long)cdOff + 46);
        if (strcmp(name, kAssetName) == 0) {
            if (method != 0) { logger("extract: asset not STORED (method=%u)", method); break; }
            unsigned char lh[30];
            if (pread(fd, lh, 30, (long long)lo) != 30 || rd32(lh) != 0x04034b50) { logger("extract: bad local header"); break; }
            dataOff = (long long)lo + 30 + rd16(lh + 26) + rd16(lh + 28);
            usize = us; found = true; break;
        }
        cdOff += 46 + nl + el + cl;
    }
    if (!found) { logger("extract: asset entry not found"); close(fd); return false; }
    int ofd = ::open(out, O_WRONLY | O_CREAT | O_TRUNC | O_CLOEXEC, 0600);
    if (ofd < 0) { logger("extract: out open failed %d", errno); close(fd); return false; }
    char* buf = (char*)malloc(1 << 20);
    if (!buf) { close(ofd); close(fd); return false; }
    long long off = dataOff, left = usize;
    while (left > 0) {
        size_t want = left > (long long)(1 << 20) ? (size_t)(1 << 20) : (size_t)left;
        ssize_t r = pread(fd, buf, want, off);
        if (r <= 0) break;
        if (write(ofd, buf, (size_t)r) != r) break;
        off += r; left -= r;
    }
    free(buf); close(ofd); close(fd);
    *outSize = usize;
    if (left != 0) { logger("extract: short copy left=%lld", left); unlink(out); return false; }
    return true;
}

static bool ra_in_fg(void* ra) {
    uintptr_t a = (uintptr_t)ra;
    return g_fg_lo != 0 && a >= g_fg_lo && a < g_fg_hi;
}

static bool is_target(const char* path) { return path && strstr(path, "base.apk") != nullptr; }

static int my_open(const char* path, int flags, ...) {
    void* ra = __builtin_return_address(0);
    if (g_ready && is_target(path) && ra_in_fg(ra)) {
        if (g_spoofOn) {
            /* 伪装：fd 指向 /data/data/<pkg>/files/fg_orig.apk（完整原厂 base.apk） */
            int sfd = o_open(kSpoofPath, flags & ~(O_CREAT | O_EXCL | O_TRUNC));
            if (sfd >= 0) {
                g_spoofFd = sfd;
                int n = ++g_hits;
                if (n <= 12) logger("FG open(SPOOF) #%d %s -> fd=%d ra=%p", n, path, sfd, ra);
                return sfd;
            }
            logger("FG open(SPOOF) 失败 errno=%d ⇒ 回退真实 base.apk", errno);
        }
        int n = ++g_hits;
        if (n <= 12) logger("FG open(real) #%d %s ra=%p", n, path, ra);
    }
    if (flags & (O_CREAT | O_TMPFILE)) {
        va_list ap; va_start(ap, flags); int m = va_arg(ap, int); va_end(ap);
        return o_open(path, flags, m);
    }
    return o_open(path, flags);
}

static int my_openat(int dirfd, const char* path, int flags, ...) {
    void* ra = __builtin_return_address(0);
    if (g_ready && is_target(path) && ra_in_fg(ra)) {
        int n = ++g_hits;
        if (n <= 12) logger("FG openat(real) #%d %s ra=%p", n, path, ra);
    }
    if (flags & (O_CREAT | O_TMPFILE)) {
        va_list ap; va_start(ap, flags); int m = va_arg(ap, int); va_end(ap);
        return o_openat(dirfd, path, flags, m);
    }
    return o_openat(dirfd, path, flags);
}

/* ---- 诊断钩子：只记 FairGuard 源的读取/探测，转发原函数 ---- */
static ssize_t my_read(int fd, void* b, size_t n) {
    void* ra = __builtin_return_address(0);
    ssize_t r = o_read(fd, b, n);
    if (ra_in_fg(ra) && r > 0) logger("FG read fd=%d n=%zd ra=%p", fd, r, ra);
    return r;
}
static ssize_t my_pread64(int fd, void* b, size_t n, off64_t off) {
    void* ra = __builtin_return_address(0);
    ssize_t r = o_pread64(fd, b, n, off);
    if (ra_in_fg(ra) && r > 0) logger("FG pread fd=%d off=%lld n=%zd ra=%p", fd, (long long)off, r, ra);
    return r;
}
static void* my_mmap(void* a, size_t l, int p, int fl, int fd, off_t off) {
    void* ra = __builtin_return_address(0);
    void* r = o_mmap(a, l, p, fl, fd, off);
    if (ra_in_fg(ra)) logger("FG mmap fd=%d len=%zu prot=0x%x off=%lld => %p ra=%p", fd, l, p, (long long)off, r, ra);
    return r;
}
static int my_fstat(int fd, struct stat* st) {
    void* ra = __builtin_return_address(0);
    int r = o_fstat(fd, st);
    if (ra_in_fg(ra)) logger("FG fstat fd=%d rc=%d size=%lld ra=%p", fd, r, st ? (long long)st->st_size : -1, ra);
    return r;
}
static ssize_t my_readlink(const char* p, char* b, size_t n) {
    void* ra = __builtin_return_address(0);
    ssize_t r = o_readlink(p, b, n);
    /* 伪装一致性：fd 反查路径时把 fg_orig.apk 报回成真实 base.apk */
    if (r > 0 && g_spoofOn && p && strstr(p, "/proc/self/fd/") && strstr(b, "fg_orig.apk") &&
        g_apkPath[0]) {
        snprintf(b, n, "%s", g_apkPath);
        r = (ssize_t)strlen(b);
        logger("FG readlink fd 伪装 -> base.apk ra=%p", ra);
        return r;
    }
    if (ra_in_fg(ra)) logger("FG readlink %s => %s ra=%p", p ? p : "(null)", r > 0 ? b : "-", ra);
    return r;
}
static int my_stat(const char* path, struct stat* st) {
    void* ra = __builtin_return_address(0);
    if (g_spoofOn && g_ready && is_target(path) && ra_in_fg(ra) && o_stat) {
        int r = o_stat(kSpoofPath, st);
        logger("FG stat(SPOOF) %s rc=%d size=%lld ra=%p", path, r,
               st ? (long long)st->st_size : -1, ra);
        return r;
    }
    return o_stat ? o_stat(path, st) : -1;
}

static void* worker(void*) {
    char apk[512] = { 0 };
    for (int i = 0; i < 200; i++) { if (findApkPath(apk, sizeof(apk))) break; usleep(50 * 1000); }
    if (!apk[0]) { logger("apk path not found in maps; spoof disabled"); return nullptr; }
    snprintf(g_apkPath, sizeof(g_apkPath), "%s", apk);
    logger("apk=%s", apk);
    {   /* 伪装开关：prop debug.starry.lvmeng_fgspoof=0 退回「只记录不替换」 */
        char pb[PROP_VALUE_MAX] = { 0 };
        if (__system_property_get("debug.starry.lvmeng_fgspoof", pb) > 0 && pb[0] == '0')
            g_spoofOn = 0;
        logger("fgspoof=%s", g_spoofOn ? "ON(替换为原厂包)" : "OFF(只记录)");
    }

    struct stat st;
    long long want = -1;
    bool have = (stat(kSpoofPath, &st) == 0 && st.st_size > 1024 * 1024);
    if (!have) {
        long long got = 0;
        if (!extractAsset(apk, kSpoofPath, &got)) { logger("extract failed; spoof disabled"); return nullptr; }
        want = got;
        logger("extracted asset -> %s (%lld B)", kSpoofPath, got);
    } else {
        logger("spoof file already present (%lld B)", (long long)st.st_size);
    }

    // 注意：libFairGuard 比本壳晚加载，此处通常还不在 maps 里 —— 先装 hook，再轮询补范围。
    // ra_in_fg() 在 g_fg_lo==0 时恒 false，所以范围未知期间 hook 等价于直通，安全。

    void* sym_open = (void*)dlsym(RTLD_DEFAULT, "open");
    void* sym_openat = (void*)dlsym(RTLD_DEFAULT, "openat");
    int rc = 0;
    if (sym_open) rc |= DobbyHook(sym_open, (void*)my_open, (void**)&o_open);
    if (sym_openat) rc |= DobbyHook(sym_openat, (void*)my_openat, (void**)&o_openat);
    if (!o_open) { logger("hook open failed rc=%d; spoof disabled", rc); return nullptr; }
    // 诊断钩子（不改变行为）：看清 FairGuard 拿到 fd 之后到底读什么、有没有探测 fd 身份
    DobbyHook((void*)dlsym(RTLD_DEFAULT, "read"),      (void*)my_read,      (void**)&o_read);
    DobbyHook((void*)dlsym(RTLD_DEFAULT, "pread64"),   (void*)my_pread64,   (void**)&o_pread64);
    DobbyHook((void*)dlsym(RTLD_DEFAULT, "mmap"),      (void*)my_mmap,      (void**)&o_mmap);
    DobbyHook((void*)dlsym(RTLD_DEFAULT, "fstat"),     (void*)my_fstat,     (void**)&o_fstat);
    DobbyHook((void*)dlsym(RTLD_DEFAULT, "readlink"),  (void*)my_readlink,  (void**)&o_readlink);
    {   /* stat 伪装：FairGuard 若用 stat(path) 探尺寸，也要看到原厂包 */
        void* sym_stat = (void*)dlsym(RTLD_DEFAULT, "stat");
        if (sym_stat) DobbyHook(sym_stat, (void*)my_stat, (void**)&o_stat);
    }
    logger("hooks installed (open=%p openat=%p)", (void*)o_open, (void*)o_openat);
    g_ready = true;
    // 轮询等 libFairGuard 出现在 maps（最多 60 s），拿到范围后伪装立即生效
    for (int i = 0; i < 600; i++) {
        if (findFgRange()) {
            logger("fg range 0x%llx..0x%llx (t=%d ms)", (unsigned long long)g_fg_lo,
                   (unsigned long long)g_fg_hi, i * 100);
            break;
        }
        usleep(100 * 1000);
    }
    if (g_fg_lo == 0) logger("fg range still unknown after 60s; spoof inactive");
    return nullptr;
}

pthread_once_t g_once = PTHREAD_ONCE_INIT;
void startOnce() {
    pthread_t th;
    if (pthread_create(&th, nullptr, worker, nullptr) == 0) pthread_detach(th);
    else logger("pthread_create failed");
}
void start() { pthread_once(&g_once, startOnce); }

}  // namespace fgspoof

__attribute__((constructor)) static void Lvmeng_FgSpoofCtor() { fgspoof::start(); }

#define LTAG "StarryLvmeng"
#define LLOGI(...) __android_log_print(ANDROID_LOG_INFO,  LTAG, __VA_ARGS__)
#define LLOGE(...) __android_log_print(ANDROID_LOG_ERROR, LTAG, __VA_ARGS__)

namespace {

const char* kStatePath = "/data/data/com.zhuziplay.lvmengnew.op/files/starry_lvmeng_state.txt";
const char* kTmpPath   = "/data/data/com.zhuziplay.lvmengnew.op/files/starry_lvmeng_state.txt.tmp";

std::mutex g_lock;
bool g_feat[LVMENG_FEAT_COUNT];
// g_val 存「菜单 UI 原始值」：hp = 用户输入值（0=关）；spd = 1x..10x；dmg = 1x..1000x。
// 写状态文件时 SPD/DMG 才 ×100（C# 契约 _spdX100/_dmgX100 ÷100 还原）。
int  g_val[LVMENG_FEAT_COUNT] = { 0, 0, 1, 1, 0, 0 };
char g_last[160] = { 0 };

void writeStateLocked() {
    char body[160];
    int n = snprintf(body, sizeof(body),
                     "S1 1 %d %d %d %d %d %d %d %d",
                     g_feat[LVMENG_FEAT_HP] ? 1 : 0, g_val[LVMENG_FEAT_HP],
                     g_feat[LVMENG_FEAT_SPD] ? 1 : 0, g_val[LVMENG_FEAT_SPD] * 100,
                     g_feat[LVMENG_FEAT_DMG] ? 1 : 0, g_val[LVMENG_FEAT_DMG] * 100,
                     g_feat[LVMENG_FEAT_SKILL] ? 1 : 0,
                     g_feat[LVMENG_FEAT_JUMP] ? 1 : 0);
    if (n <= 0 || n >= (int)sizeof(body)) return;
    if (strcmp(body, g_last) == 0) return;

    FILE* f = fopen(kTmpPath, "wb");
    if (!f) { LLOGE("state open failed: %d", errno); return; }
    fwrite(body, 1, (size_t)n, f);
    fflush(f);
    fclose(f);
    if (rename(kTmpPath, kStatePath) != 0) {
        f = fopen(kStatePath, "wb");
        if (f) { fwrite(body, 1, (size_t)n, f); fclose(f); }
    }
    strncpy(g_last, body, sizeof(g_last) - 1);
    LLOGI("state -> %s", body);
}

}  // namespace

bool Lvmeng_TryInit(JavaVM* vm) {
    (void)vm;
    fgspoof::start();
    std::lock_guard<std::mutex> lk(g_lock);
    writeStateLocked();
    LLOGI("shell init ok");
    return true;
}

bool Lvmeng_IsReady() { return true; }

void Lvmeng_Tick(const bool* features, const int* values) {
    if (!features || !values) return;
    std::lock_guard<std::mutex> lk(g_lock);
    for (int i = 0; i < LVMENG_FEAT_COUNT; i++) {
        g_feat[i] = features[i];
        g_val[i] = values[i];
    }
    g_feat[LVMENG_FEAT_SPD] = (g_val[LVMENG_FEAT_SPD] > 1);
    g_feat[LVMENG_FEAT_DMG] = (g_val[LVMENG_FEAT_DMG] > 1);
    writeStateLocked();
}

void Lvmeng_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum < 0 || featNum >= LVMENG_FEAT_COUNT) return;
    std::lock_guard<std::mutex> lk(g_lock);
    g_feat[featNum] = enabled;
    writeStateLocked();
}

void Lvmeng_OnValueChanged(int featNum, int value) {
    if (featNum < 0 || featNum >= LVMENG_FEAT_COUNT) return;
    std::lock_guard<std::mutex> lk(g_lock);
    g_val[featNum] = value;
    if (featNum == LVMENG_FEAT_SPD || featNum == LVMENG_FEAT_DMG) g_feat[featNum] = (value > 1);
    writeStateLocked();
}

void Lvmeng_OnInputChanged(int featNum, const char* text) {
    if (featNum < 0 || featNum >= LVMENG_FEAT_COUNT) return;
    int v = text ? atoi(text) : 0;
    std::lock_guard<std::mutex> lk(g_lock);
    g_val[featNum] = v;
    g_feat[featNum] = (v > 0);
    writeStateLocked();
}

void Lvmeng_ForceClear() {
    std::lock_guard<std::mutex> lk(g_lock);
    for (int i = 0; i < LVMENG_FEAT_COUNT; i++) g_feat[i] = false;
    writeStateLocked();
}
