/* ================================================================
 *  ZzzProbeMod — 绝区零 3.3.0 骨架探针 v0.2（计数探针）
 *
 *  在 v0.1（只读扫描）基础上增加**透传计数钩子**：
 *    对「事件分发」候选（ldrh w?,[x?,#0x16] 后 8 条内 cmp #0x85/#0x86）
 *    逐点装 Dobby 钩子：只做计数 + 打印现场字段，**立即调原函数、不改任何行为**。
 *    目的：一场战斗内判定哪个候选是真正的伤害事件分发点（无敌/伤害倍率挂点）。
 *
 *  安全底线：
 *    - 读现场指针前先用 /proc/self/maps 解析出的**可读区间**过滤（IsReadable），不裸读野指针
 *    - 装钩前逐点打日志；若崩溃，最后一条 installed 日志即崩点
 *    - 日志节流：每站点前 3 次 + 之后每 500 次
 *
 *  诊断宏按框架规范进 STARRY_DIAG 守卫块（探针包用 STARRY_DIAG=1 构建）。
 *  站点口径：RVA = 运行时VA - libil2cpp基址；file_off = RVA - 0x4000
 * ================================================================ */
#include "ZzzMod.h"

#include <android/log.h>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <cstdlib>
#include <fcntl.h>
#include <link.h>
#include <elf.h>
#include <unistd.h>

#include "Dobby/dobby.h"

#define TAG "StarryMagic"
#ifdef STARRY_DIAG
#define PLOG(fmt, ...) __android_log_print(ANDROID_LOG_INFO, TAG, "[jql][probe] " fmt, ##__VA_ARGS__)
#else
#define PLOG(fmt, ...) ((void)0)
#endif

namespace {

constexpr uint64_t kRvaToFileDelta = 0x4000ull;
constexpr int kMaxSites = 20;

uintptr_t g_base = 0;
uintptr_t g_code_lo = 0, g_code_hi = 0;
const uint32_t* g_w = nullptr;
size_t g_nw = 0;
bool g_ready = false, g_scanned = false;
int g_ticks = 0;

struct Range { uintptr_t lo, hi; };
Range g_rd[512];
int g_nrd = 0;

struct Site {
    uintptr_t rva;
    void* orig;
    volatile int calls;
    volatile int logleft;
};
Site g_site[kMaxSites];
int g_nsite = 0;

// ---------- /proc/self/maps：可读区间（用于安全读） ----------
void load_maps() {
    g_nrd = 0;
    FILE* f = fopen("/proc/self/maps", "r");
    if (!f) return;
    char line[512];
    while (fgets(line, sizeof(line), f) && g_nrd < 512) {
        unsigned long lo = 0, hi = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s", &lo, &hi, perm) != 3) continue;
        if (perm[0] != 'r') continue;
        g_rd[g_nrd].lo = (uintptr_t)lo;
        g_rd[g_nrd].hi = (uintptr_t)hi;
        ++g_nrd;
    }
    fclose(f);
    PLOG("maps readable ranges=%d", g_nrd);
}

bool IsReadable(uintptr_t p, size_t len) {
    if (p < 0x1000) return false;
    for (int i = 0; i < g_nrd; ++i)
        if (p >= g_rd[i].lo && p + len <= g_rd[i].hi) return true;
    return false;
}
inline bool rd_u16(uintptr_t p, uint16_t* out) {
    if (!IsReadable(p, 2)) return false;
    *out = *reinterpret_cast<const uint16_t*>(p);
    return true;
}
inline bool rd_u32(uintptr_t p, uint32_t* out) {
    if (!IsReadable(p, 4)) return false;
    *out = *reinterpret_cast<const uint32_t*>(p);
    return true;
}

void log_arg(const char* tag, uint64_t v) {
    if (!v) return;
    uintptr_t p = (uintptr_t)v;
    uint16_t u16 = 0; uint32_t u32 = 0;
    bool ok16 = rd_u16(p + 0x16, &u16);
    bool ok32 = rd_u32(p + 0x38, &u32);
    if (ok16 || ok32)
        PLOG("    %s=%#llx  +0x16=%s%#x  +0x38=%s%#x", tag, (unsigned long long)v,
             ok16 ? "" : "?", ok16 ? u16 : 0u, ok32 ? "" : "?", ok32 ? u32 : 0u);
}

inline uint64_t call_site(int idx, uint64_t* a) {
    Site& s = g_site[idx];
    int c = ++s.calls;
    if (s.logleft > 0) {
        --s.logleft;
        PLOG("HIT site#%d rva=%#llx call=%d args=%#llx %#llx %#llx %#llx",
             idx, (unsigned long long)s.rva, c,
             (unsigned long long)a[0], (unsigned long long)a[1],
             (unsigned long long)a[2], (unsigned long long)a[3]);
        log_arg("a0", a[0]);
        log_arg("a1", a[1]);
        log_arg("a2", a[2]);
        log_arg("a3", a[3]);
    } else if (c % 500 == 0) {
        PLOG("HIT site#%d rva=%#llx call=%d", idx, (unsigned long long)s.rva, c);
    }
    typedef uint64_t (*fn_t)(uint64_t, uint64_t, uint64_t, uint64_t,
                             uint64_t, uint64_t, uint64_t, uint64_t);
    fn_t orig = reinterpret_cast<fn_t>(s.orig);
    return orig ? orig(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7]) : 0;
}

#define MK_HANDLER(N)                                                                     \
    extern "C" __attribute__((noinline)) uint64_t ZzzH##N(                                \
        uint64_t a0, uint64_t a1, uint64_t a2, uint64_t a3,                               \
        uint64_t a4, uint64_t a5, uint64_t a6, uint64_t a7) {                             \
        uint64_t a[8] = {a0, a1, a2, a3, a4, a5, a6, a7};                                 \
        return call_site(N, a);                                                           \
    }
MK_HANDLER(0)  MK_HANDLER(1)  MK_HANDLER(2)  MK_HANDLER(3)  MK_HANDLER(4)
MK_HANDLER(5)  MK_HANDLER(6)  MK_HANDLER(7)  MK_HANDLER(8)  MK_HANDLER(9)
MK_HANDLER(10) MK_HANDLER(11) MK_HANDLER(12) MK_HANDLER(13) MK_HANDLER(14)
MK_HANDLER(15) MK_HANDLER(16) MK_HANDLER(17) MK_HANDLER(18) MK_HANDLER(19)

void* handler_for(int i) {
    static void* tab[20] = {
        (void*)ZzzH0, (void*)ZzzH1, (void*)ZzzH2, (void*)ZzzH3, (void*)ZzzH4,
        (void*)ZzzH5, (void*)ZzzH6, (void*)ZzzH7, (void*)ZzzH8, (void*)ZzzH9,
        (void*)ZzzH10, (void*)ZzzH11, (void*)ZzzH12, (void*)ZzzH13, (void*)ZzzH14,
        (void*)ZzzH15, (void*)ZzzH16, (void*)ZzzH17, (void*)ZzzH18, (void*)ZzzH19,
    };
    return (i >= 0 && i < 20) ? tab[i] : nullptr;
}

int phdr_cb(struct dl_phdr_info* info, size_t, void*) {
    if (!info->dlpi_name || !strstr(info->dlpi_name, "libil2cpp.so")) return 0;
    uintptr_t lo = 0, hi = 0;
    for (int i = 0; i < info->dlpi_phnum; ++i) {
        const ElfW(Phdr)& ph = info->dlpi_phdr[i];
        if (ph.p_type != PT_LOAD || (ph.p_flags & PF_X) == 0) continue;
        uintptr_t l = info->dlpi_addr + ph.p_vaddr;
        uintptr_t h = l + (uintptr_t)ph.p_filesz;
        if (h - l > hi - lo) { lo = l; hi = h; }
    }
    if (hi <= lo) return 0;
    g_base = (uintptr_t)info->dlpi_addr;
    g_code_lo = lo; g_code_hi = hi;
    return 1;
}

int scan_evt_sites(uintptr_t* out, int maxn) {
    int n = 0;
    const uint32_t* w = g_w;
    const size_t N = g_nw;
    for (size_t i = 0; i < N && n < maxn; ++i) {
        const uint32_t v = w[i];
        if ((v & 0xFFC00000u) != 0x79400000u) continue;
        if (((v >> 10) & 0xFFFu) != 11u) continue;
        for (size_t j = i + 1; j < N && j <= i + 8; ++j) {
            const uint32_t x = w[j];
            if ((x & 0xFFC00000u) != 0x71000000u) continue;
            const unsigned imm = (x >> 10) & 0xFFFu;
            if (imm == 0x85u || imm == 0x86u) { out[n++] = (uintptr_t)&w[i] - g_base; break; }
        }
    }
    return n;
}

}  // namespace

bool Zzz_TryInit(JavaVM*) {
    if (g_scanned) return g_ready;

    dl_iterate_phdr(phdr_cb, nullptr);
    if (!g_base || g_code_hi <= g_code_lo) { PLOG("libil2cpp NOT found"); return false; }

    g_w = reinterpret_cast<const uint32_t*>(g_code_lo);
    g_nw = (size_t)((g_code_hi - g_code_lo) / 4u);
    PLOG("base=%#llx code=[%#llx,%#llx) %llu MB", (unsigned long long)g_base,
         (unsigned long long)g_code_lo, (unsigned long long)g_code_hi,
         (unsigned long long)((g_code_hi - g_code_lo) >> 20));

    load_maps();

    uintptr_t rvas[kMaxSites];
    int n = scan_evt_sites(rvas, kMaxSites);
    PLOG("evt candidates=%d (cap %d)", n, kMaxSites);

#ifndef ZZZ_PROBE_HOOKS
    // 2026-10-22 停用：核对发现 3.2.0 真·无敌函数（0x1152104c，418 条指令）内 **没有** cmp #0x85/#0x86
    // 也没有 ldrh #0x16 —— 类型判定在池鸳自己的 handler 里，不在这 17 个事件分发函数中。
    // 因此对它们装钩缺乏依据（且候选是函数中部指令，DobbyHook 的 orig 模型不成立）。
    // 需要装钩实验时显式 -DZZZ_PROBE_HOOKS 构建，勿默认开启。
    (void)rvas; (void)n;
    PLOG("hooks DISABLED (build without -DZZZ_PROBE_HOOKS) -> scan-only");
#else
    for (int i = 0; i < n; ++i) {
        g_site[i].rva = rvas[i];
        g_site[i].orig = nullptr;
        g_site[i].calls = 0;
        g_site[i].logleft = 3;
        uintptr_t va = g_base + rvas[i];
        PLOG("install hook#%d rva=%#llx file_off=%#llx word=%08x", i,
             (unsigned long long)rvas[i],
             (unsigned long long)(rvas[i] - kRvaToFileDelta),
             *reinterpret_cast<const uint32_t*>(va));
        int rc = DobbyHook((void*)va, (dobby_dummy_func_t)handler_for(i),
                           (dobby_dummy_func_t*)&g_site[i].orig);
        PLOG("  hook#%d rc=%d orig=%p", i, rc, g_site[i].orig);
        if (rc == 0) ++g_nsite;
    }
    PLOG("hooks installed=%d/%d", g_nsite, n);
#endif

    g_scanned = true;
    g_ready = true;
    PLOG("init done (counting probe: passthrough hooks, behavior unchanged)");
    return true;
}

bool Zzz_IsReady() { return g_ready; }

void Zzz_OnFeatureChanged(int featNum, bool enabled) {
    PLOG("feature[%d] -> %s (probe: not implemented)", featNum, enabled ? "ON" : "OFF");
}
void Zzz_OnValueChanged(int featNum, int value) {
    PLOG("value[%d] = %d (probe: not implemented)", featNum, value);
}
void Zzz_Tick(const bool* features, const int* values) {
    ++g_ticks;
    if (g_ticks == 1) {
        PLOG("menu tick alive; feats=%d%d%d%d val2=%d", features ? features[0] : 0,
             features ? features[1] : 0, features ? features[2] : 0,
             features ? features[3] : 0, values ? values[2] : -1);
    } else if (g_ticks % 150 == 0) {
        char buf[600]; int off = 0;
        for (int i = 0; i < g_nsite && i < kMaxSites && off < (int)sizeof(buf) - 24; ++i)
            off += snprintf(buf + off, sizeof(buf) - off, " #%d=%d", i, g_site[i].calls);
        PLOG("tick=%d counts:%s", g_ticks, buf);
    }
}
void Zzz_ForceClear() { PLOG("forceClear (probe: nothing)"); }
