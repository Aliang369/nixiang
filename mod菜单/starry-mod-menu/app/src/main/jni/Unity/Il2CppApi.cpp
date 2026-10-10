/* Unity IL2CPP 公共实现 */
#include "Includes/SecStrings.hpp"
#include "Includes/StarPtr.h"   // 设备无关指针判据（清 TBI/MTE 顶字节 + 用户态区间）
#include "Unity/Il2CppApi.h"

#include <elf.h>
#include <link.h>

#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <ctime>
#include <dlfcn.h>
#include <mutex>
#include <pthread.h>
#include <string>
#include <sys/syscall.h>
#include <unistd.h>
#include <unordered_map>
#include <unordered_set>

#include "Includes/Logger.h"
#include "xDL/xdl.h"

namespace Il2Cpp {
namespace detail {

/* ---- /proc/self/maps 权限判定（模拟器安全读的前提，见 probe 块 v4 注释）----
 * 要求 [addr, addr+len) 完整落在一个映射内，且该映射 perms 含 want（'r'/'x'）。
 * 每次调用 fopen /proc/self/maps（微秒级）；仅诊断/装钩/低频路径用，禁热路径。 */
static bool MappingHasPerm(uintptr_t addr, size_t len, char want) {
    if (!addr || !len) return false;
    uintptr_t wantEnd = addr + len;
    if (wantEnd < addr) return false; /* 地址回绕 */
    FILE* f = fopen("/proc/self/maps", "r");
    if (!f) return false;
    char line[512];
    bool ok = false;
    while (fgets(line, sizeof(line), f)) {
        unsigned long s = 0, e = 0;
        char perms[8] = {0};
        if (sscanf(line, "%lx-%lx %7s", &s, &e, perms) < 3) continue;
        if ((uintptr_t)s > addr) break; /* maps 按地址升序 */
        if (addr >= (uintptr_t)s && wantEnd <= (uintptr_t)e) {
            ok = (strchr(perms, want) != nullptr);
            break;
        }
    }
    fclose(f);
    return ok;
}
static bool IsReadableRange(uintptr_t addr, size_t len) { return MappingHasPerm(addr, len, 'r'); }

/* ---- 缓存版权限判定（热路径用）：maps 快照 + 未命中时按 1s 节流刷新 ----
 * IsValidHeapPtr 会被伤害/命中热路径反复调用，fopen maps 太贵 ⇒ 建一次表线性扫。
 * 未命中时最多每秒重读一次 maps（JIT/anon 新映射是常态），避免把合法新页判否。 */
struct MapRange {
    uintptr_t s, e;
    unsigned char perm; /* bit0=r bit1=w bit2=x */
};
// v26u：1024 → 8192。本机映射数已超 1024（scan done regions=541 skipped=517 ⇒ 1058+），
//   表溢出会让缓存判定对高地址对象一律 false（见 SafeObjType v25 注释），故扩表。
static MapRange g_mapRanges[8192];
static int g_mapRangeN = 0;
static uint64_t g_mapRangeMs = 0;
static pthread_mutex_t g_mapRangeMu = PTHREAD_MUTEX_INITIALIZER;

static uint64_t MapNowMs() {
    struct timespec ts;
    clock_gettime(CLOCK_MONOTONIC, &ts);
    return (uint64_t)ts.tv_sec * 1000ull + (uint64_t)(ts.tv_nsec / 1000000);
}

static void RefreshMapRanges() {
    FILE* f = fopen("/proc/self/maps", "r");
    if (!f) return;
    int n = 0;
    const int cap = (int)(sizeof(g_mapRanges) / sizeof(g_mapRanges[0]));
    char line[512];
    while (fgets(line, sizeof(line), f) && n < cap) {
        unsigned long s = 0, e = 0;
        char perms[8] = {0};
        if (sscanf(line, "%lx-%lx %7s", &s, &e, perms) < 3) continue;
        unsigned char p = 0;
        if (perms[0] == 'r') p |= 1u;
        if (perms[1] == 'w') p |= 2u;
        if (perms[2] == 'x') p |= 4u;
        g_mapRanges[n].s = (uintptr_t)s;
        g_mapRanges[n].e = (uintptr_t)e;
        g_mapRanges[n].perm = p;
        n++;
    }
    fclose(f);
    g_mapRangeN = n;
    g_mapRangeMs = MapNowMs();
}

/* 缓存权限判定；want 为位掩码（1=r 2=w 4=x）。 */
static bool PermCached(uintptr_t addr, size_t len, unsigned char want) {
    if (!addr || !len) return false;
    uintptr_t wantEnd = addr + len;
    if (wantEnd < addr) return false;
    pthread_mutex_lock(&g_mapRangeMu);
    if (g_mapRangeN == 0) RefreshMapRanges();
    bool ok = false;
    for (int pass = 0; pass < 2; pass++) {
        bool found = false;
        for (int i = 0; i < g_mapRangeN; i++) {
            const MapRange& r = g_mapRanges[i];
            if (addr >= r.s && wantEnd <= r.e) {
                ok = ((r.perm & want) == want);
                found = true;
                break;
            }
        }
        if (found) break;
        if (pass == 0 && MapNowMs() - g_mapRangeMs >= 1000) RefreshMapRanges();
        else break;
    }
    pthread_mutex_unlock(&g_mapRangeMu);
    return ok;
}

/* v26u：热路径专用的「软」缓存判定。
 * 与 PermCached 的区别：**未命中时不重读 maps**，而是把 found=false 交给调用方
 * 去走精确的 fopen 路径。理由：SafeObjType 每次调用要判 3 个地址，v26t 的 SecAttrAll
 * 每个演员每轮最多 44 次 SafeObjType ⇒ 单拍上千次 fopen("/proc/self/maps")，
 * 这是用户反馈「游戏变的好掉帧」的头号来源。命中即零 I/O；表最多每秒刷新一次。 */
static bool PermCachedSoft(uintptr_t addr, size_t len, unsigned char want, bool* found) {
    *found = false;
    if (!addr || !len) return false;
    uintptr_t wantEnd = addr + len;
    if (wantEnd < addr) return false;
    pthread_mutex_lock(&g_mapRangeMu);
    if (g_mapRangeN == 0) RefreshMapRanges();
    else if (MapNowMs() - g_mapRangeMs >= 1000) RefreshMapRanges();
    bool ok = false;
    for (int i = 0; i < g_mapRangeN; i++) {
        const MapRange& r = g_mapRanges[i];
        if (addr >= r.s && wantEnd <= r.e) {
            ok = ((r.perm & want) == want);
            *found = true;
            break;
        }
    }
    pthread_mutex_unlock(&g_mapRangeMu);
    return ok;
}

/* ================================================================
 * TP 符号隐藏对策：进程内 dynsym 自解析
 *
 * 终末地 1.5.3 的 TP 把 dynsym 条目改写为（静态验证 4344/4345 全命中）：
 *   st_name  → 指向字符串“终止符”（真名在其前面，需回扫到上一个 NUL）
 *   st_value → 最高字节 = 0x80 标志，真址（库内偏移）以大端存于 b3~b6，
 *              即 bswap32((st_value >> 24) & 0xFFFFFFFF)
 *              （磁盘模拟交叉验证：5 个核心 API bswap 后全落加密岛内）
 * 并重排了符号顺序但未重建 GNU hash 表 → 任何按名查找（dlsym/xdl_sym）必死。
 * 对策：dl_iterate_phdr 找到基址 → PT_DYNAMIC → DT_SYMTAB/STRTAB/STRSZ/GNU_HASH
 *       → 线性扫描自建 name→addr 表。零外部依赖。
 * ================================================================ */

struct DynsymInfo {
    const Elf64_Sym* sym = nullptr;
    const char* str = nullptr;
    uint32_t strsz = 0;
    uint32_t nsyms = 0;  // 由 GNU hash 推出；0 = 推算失败
    uintptr_t base = 0;
};

struct PhdrCtx {
    const char* lib;
    uintptr_t base;
    const Elf64_Phdr* phdr;
    int phnum;
    bool found;
};

static int PhdrCallback(struct dl_phdr_info* info, size_t, void* data) {
    PhdrCtx* ctx = (PhdrCtx*)data;
    const char* n = info->dlpi_name;
    if (!n || !ctx->lib) return 0;
    size_t ln = strlen(ctx->lib), l = strlen(n);
    if (l < ln || strcmp(n + (l - ln), ctx->lib) != 0) return 0;
    LOGI("[Il2Cpp] phdr match: %s base=%p", n, (void*)info->dlpi_addr);  // 诊断：排查同名假库
    ctx->base = (uintptr_t)info->dlpi_addr;
    ctx->phdr = info->dlpi_phdr;
    ctx->phnum = info->dlpi_phnum;
    ctx->found = true;
    return 1;
}

static bool LocateDynsym(const char* lib, DynsymInfo* out) {
    PhdrCtx ctx = {};
    ctx.lib = lib;
    if (dl_iterate_phdr(PhdrCallback, &ctx) <= 0 || !ctx.found) return false;

    /* 可信映射范围 [base, mapend)，用于校验 d_ptr 是否已被 linker 补上 bias */
    uintptr_t mapend = ctx.base;
    for (int i = 0; i < ctx.phnum; i++) {
        if (ctx.phdr[i].p_type == PT_LOAD) {
            uintptr_t e = ctx.base + ctx.phdr[i].p_vaddr + ctx.phdr[i].p_memsz;
            if (e > mapend) mapend = e;
        }
    }

    const Elf64_Dyn* dyn = nullptr;
    for (int i = 0; i < ctx.phnum; i++) {
        if (ctx.phdr[i].p_type == PT_DYNAMIC) {
            dyn = (const Elf64_Dyn*)(ctx.base + ctx.phdr[i].p_vaddr);
            break;
        }
    }
    if (!dyn) return false;

    uintptr_t symtab = 0, strtab = 0, gnuhash = 0;
    uint64_t strsz = 0;
    for (const Elf64_Dyn* d = dyn; d->d_tag != DT_NULL; d++) {
        switch (d->d_tag) {
            case DT_SYMTAB:   symtab  = (uintptr_t)d->d_un.d_ptr; break;
            case DT_STRTAB:   strtab  = (uintptr_t)d->d_un.d_ptr; break;
            case DT_STRSZ:    strsz   = d->d_un.d_val; break;
            case DT_GNU_HASH: gnuhash = (uintptr_t)d->d_un.d_ptr; break;
            default: break;
        }
    }
    if (!symtab || !strtab || !strsz) return false;

    /* d_ptr 兜底：bionic linker 通常已把 d_ptr 写回为绝对地址，
     * 但 TP 自加载器可能不写回（保持库内偏移）→ 解引用直接 SIGSEGV。
     * 若落在映射范围外而「+base 后落在范围内」，则按裸偏移补 bias。 */
    auto fixptr = [&](uintptr_t v) -> uintptr_t {
        if (v >= ctx.base && v < mapend) return v;
        uintptr_t biased = v + ctx.base;
        if (biased >= ctx.base && biased < mapend) return biased;
        return v;
    };
    symtab = fixptr(symtab);
    strtab = fixptr(strtab);
    gnuhash = fixptr(gnuhash);

    out->sym = (const Elf64_Sym*)symtab;
    out->str = (const char*)strtab;
    out->strsz = (uint32_t)strsz;
    out->base = ctx.base;

    /* 符号总数：GNU hash 布局 [nbucket|symoffset|bloom_size|bloom_shift|bloom|buckets|chains]，
     * 取最大 bucket 沿 chain 走到 bit0=1 的末位。TP 重排符号但未动 hash 表，
     * 静态验证 chain 覆盖范围完整，可给出正确上界（实测 ≈4719）。 */
    if (gnuhash) {
        const uint32_t* gh = (const uint32_t*)gnuhash;
        uint32_t nbucket = gh[0], symoffset = gh[1], bloom_size = gh[2];
        const uint32_t* buckets = gh + 4 + bloom_size * (sizeof(Elf64_Addr) / 4);
        const uint32_t* chains = buckets + nbucket;
        uint32_t maxIdx = 0;
        for (uint32_t i = 0; i < nbucket; i++)
            if (buckets[i] > maxIdx) maxIdx = buckets[i];
        if (maxIdx >= symoffset) {
            uint32_t guard = 0;
            while ((chains[maxIdx - symoffset] & 1u) == 0u && guard++ < 1000000u) maxIdx++;
            out->nsyms = maxIdx + 1;
        }
    }
    return true;
}

static bool NameIsPrintable(const char* s, size_t len) {
    for (size_t i = 0; i < len; i++) {
        unsigned char c = (unsigned char)s[i];
        if (c < 0x20 || c > 0x7e) return false;
    }
    return true;
}

/* 单条符号取名：TP 模式（st_name 指向终止符，回扫）优先，失败退回标准直读 */
static bool SymName(const DynsymInfo& t, const Elf64_Sym& s, std::string* out) {
    if (s.st_name == 0 || s.st_name >= t.strsz) return false;
    const char* endp = t.str + s.st_name;
    if (*endp == 0) {
        /* TP 模式：真名 = [q, endp)，q 从 endp 回扫到上一个 NUL（或 strtab 起点） */
        const char* q = endp;
        const char* lo = t.str;
        int back = 0;
        while (q > lo && q[-1] != 0 && back < 1024) { q--; back++; }
        size_t len = (size_t)(endp - q);
        if (len == 0 || len > 512 || !NameIsPrintable(q, len)) return false;
        out->assign(q, len);
        return true;
    }
    /* 标准模式：直读到 NUL */
    const char* end = (const char*)memchr(endp, 0, t.strsz - s.st_name);
    if (!end) return false;
    size_t len = (size_t)(end - endp);
    if (len == 0 || len > 512 || !NameIsPrintable(endp, len)) return false;
    out->assign(endp, len);
    return true;
}

static void BuildDynsymMap(const char* lib, std::unordered_map<std::string, void*>* m) {
    DynsymInfo t;
    if (!LocateDynsym(lib, &t) || t.nsyms == 0 || t.nsyms > 200000) {
        LOGI("[Il2Cpp] dynsym locate failed or nsyms bad");
        return;
    }
    LOGI("[Il2Cpp] dynsym ctx: base=%p sym=%p str=%p strsz=%u nsyms=%u",
         (void*)t.base, (const void*)t.sym, (const void*)t.str, t.strsz, t.nsyms);
    m->reserve(t.nsyms);
    std::string name;
    for (uint32_t i = 0; i < t.nsyms; i++) {
        const Elf64_Sym& s = t.sym[i];
        if (s.st_shndx == SHN_UNDEF) continue;
        if (!SymName(t, s, &name)) continue;
        uintptr_t addr;
        if ((s.st_value >> 56) == 0x80) {
            /* TP 模式：真址 = 库内偏移（b3~b6 大端，须 bswap32） */
            uint32_t packed = (uint32_t)((s.st_value >> 24) & 0xFFFFFFFFULL);
            addr = t.base + __builtin_bswap32(packed);
        } else {
            if (s.st_value == 0) continue;
            addr = t.base + s.st_value;
        }
        if (addr == t.base) continue;
        m->emplace(name, (void*)addr);  // 首次出现优先
    }

    /* ---- 兜底：TP 运行时改写 dynsym 名字层的符号，用 strtab 明文反查 ----
     * 磁盘基线解析 0 失败；运行时若某符号「strtab 明文仍在、map 却缺失」，
     * 其 dynsym 条目名字层必被 TP 改写（实测：il2cpp_domain_get_assemblies）。
     * 三条策略：
     *   A. st_name == 终止符偏移(TP)/名字起始偏移(std) 精确反查（不看主循环
     *      跳过原因，UNDEF 但带 TP 打包地址的条目也能救回）；
     *   B. 仅对 SymName 失败条目试 st_name ±8 邻域，取回名字恰为目标才接受；
     *   C. 排除法：失败条目恰 1 条且缺失必需名恰 1 个 → 唯一配对
     *      （名字集 = 磁盘集 − 运行时集，strtab 一致时差集恰为失败条目宿主名）。 */
    {
        static const char* kNeed[] = {
            "il2cpp_class_from_name", "il2cpp_class_get_field_from_name",
            "il2cpp_field_get_offset", "il2cpp_field_static_get_value",
            "il2cpp_domain_get", "il2cpp_domain_get_assemblies",
            "il2cpp_assembly_get_image", "il2cpp_object_get_class",
            "il2cpp_class_get_name", "il2cpp_class_get_method_from_name",
            "il2cpp_runtime_invoke", "il2cpp_string_new",
            "il2cpp_string_new_wrapper", "il2cpp_image_get_class_count",
            "il2cpp_image_get_class", "il2cpp_method_get_name",
            "il2cpp_value_box", "il2cpp_get_corlib"};
        /* ---- 策略 D v3（实测终审）：磁盘偏移是 TP 密文，运行时值才是真代码。
         * 实测（1.5.3 probe[D] 日志）：18 名 runtime 端 16 字节全部为标准
         * AArch64 函数序言（STP/MOV/ADRP+LDR），而磁盘推导地址处无序言特征
         * （磁盘 .text 整体加密，偏移≠运行时真址）。v1/v2 强覆磁盘值 = hook
         * 进密文 → 开功能即崩。故恒保留运行时值，kDiskOffsets 仅留作探针
         * 证据日志；init 失败问题已由 strtab 明文反查修复（core api PLAINTEXT）。 */
        static const struct { const char* name; uint32_t off; } kDiskOffsets[] = {
            { "il2cpp_class_from_name", 0x9a0a98c },
            { "il2cpp_class_get_field_from_name", 0x9a0aabc },
            { "il2cpp_field_get_offset", 0x9a0b5a8 },
            { "il2cpp_field_static_get_value", 0x9a0b698 },
            { "il2cpp_domain_get", 0x9a0b3e0 },
            { "il2cpp_domain_get_assemblies", 0x9a0b420 },
            { "il2cpp_assembly_get_image", 0x9a0a8bc },
            { "il2cpp_object_get_class", 0x9a0b984 },
            { "il2cpp_class_get_name", 0x9a0ac34 },
            { "il2cpp_class_get_method_from_name", 0x9a0ac2c },
            { "il2cpp_runtime_invoke", 0x9a0bba0 },
            { "il2cpp_string_new", 0x9a0bc04 },
            { "il2cpp_string_new_wrapper", 0x9a0bc00 },
            { "il2cpp_image_get_class_count", 0x9a0be1c },
            { "il2cpp_image_get_class", 0x9a0beb0 },
            { "il2cpp_method_get_name", 0x9a0b8ac },
            { "il2cpp_value_box", 0x9a0bac0 },
            { "il2cpp_get_corlib", 0x9a0a868 },
        };
        {
            int dfix = 0, dkeep = 0;
            for (const auto& e : kDiskOffsets) {
                if (e.off == 0) continue;
                void* rt = nullptr;
                auto it = m->find(e.name);
                if (it != m->end()) rt = it->second;
                void* sa = (void*)(t.base + e.off);
                /* 探针（仅证据，不再覆盖）：hexdump static/runtime 双方内存。
                 * v2 教训：16 字节=4 指令只有 4 个高字节，hb≤5 恒真，判别失真；
                 * 终审以序言特征（STP a9/MOV aa/ADRP b0 开头）人眼复核。
                 * v4（2026-10-02 冒险与征途 / MuMu）：**读前先查 /proc/self/maps**。
                 * 模拟器（libhoudini 翻译 + libsigchain 接管 sigaction）下，对未映射
                 * 地址 memcpy 引发的 SIGSEGV 一旦落进「非扫描线程」分支，旧实现
                 * signal(SIG_DFL)+raise 在 chain 下不会终止 → 退化成「故障→返回→
                 * 重执」死循环：线程 state=R 空转、无任何日志、mod 初始化永久卡死、
                 * 全部功能失效。故未映射一律不读（证据降级为 nomap 标记）。 */
                uint8_t sb[16] = {0}, rb[16] = {0};
                bool sMap = IsReadableRange((uintptr_t)sa, 16);
                LOGI("[Il2Cpp] probe[>] %s sa=%p map=%d rt=%p", e.name, (void*)sa,
                     (int)sMap, rt); /* 前置日志：卡死时最后一行即故障点 */
                bool sOk = sMap && SafeProbe([&] { memcpy(sb, sa, 16); return true; });
                char shex[20] = "unmapped", rhex[20] = "absent";
                if (!sMap) snprintf(shex, sizeof shex, "nomap");
                else if (sOk)
                    for (int i = 0; i < 8; i++) snprintf(shex + i * 2, 3, "%02x", sb[i]);
                if (rt) {
                    if (!IsReadableRange((uintptr_t)rt, 16))
                        snprintf(rhex, sizeof rhex, "nomap");
                    else if (SafeProbe([&] { memcpy(rb, rt, 16); return true; }))
                        for (int i = 0; i < 8; i++) snprintf(rhex + i * 2, 3, "%02x", rb[i]);
                    else snprintf(rhex, sizeof rhex, "unmapped");
                }
                dkeep++; /* 恒保留运行时值（v3） */
                LOGI("[Il2Cpp] probe[D] %s disk=+%#x[%s] runtime=%p[%s] -> KEEP-RUNTIME",
                     e.name, e.off, shex, rt, rhex);
            }
            LOGI("[Il2Cpp] probe[D] applied=%d keep=%d", dfix, dkeep);
        }
        auto decode = [&](const Elf64_Sym& s, uintptr_t* addr) -> bool {
            if ((s.st_value >> 56) == 0x80) {
                uint32_t packed = (uint32_t)((s.st_value >> 24) & 0xFFFFFFFFULL);
                *addr = t.base + __builtin_bswap32(packed);
                return *addr != t.base;
            }
            if (s.st_value == 0 || s.st_shndx == SHN_UNDEF) return false;
            *addr = t.base + s.st_value;
            return *addr != t.base;
        };
        /* 失败条目登记（非 UNDEF、st_name 合法但 SymName 失败）+ 原始字段留证 */
        std::vector<uint32_t> failIdx;
        for (uint32_t i = 0; i < t.nsyms; i++) {
            const Elf64_Sym& s = t.sym[i];
            if (s.st_shndx == SHN_UNDEF) continue;
            if (s.st_name == 0 || s.st_name >= t.strsz) continue;
            if (!SymName(t, s, &name)) failIdx.push_back(i);
        }
        for (uint32_t i : failIdx) {
            const Elf64_Sym& s = t.sym[i];
            uintptr_t a = 0;
            LOGI("[Il2Cpp] failentry[%u] st_name=%#x st_value=%#llx shndx=%u info=%u addr=%s",
                 i, (unsigned)s.st_name, (unsigned long long)s.st_value,
                 (unsigned)s.st_shndx, (unsigned)s.st_info,
                 decode(s, &a) ? "decodable" : "no");
        }
        int fixed = 0;
        for (const char* need : kNeed) {
            if (m->count(need)) continue;
            size_t ln = strlen(need);
            const char* hit = nullptr;
            for (uint32_t o = 0; o + ln + 1 <= t.strsz; o++)
                if (memcmp(t.str + o, need, ln) == 0 && t.str[o + ln] == 0) { hit = t.str + o; break; }
            if (!hit) continue;  // strtab 无完整明文，无从反查
            uintptr_t no = (uintptr_t)(hit - t.str), to = no + ln;
            bool done = false;
            /* A：精确反查 st_name == to（TP 模式）/ == no（标准模式） */
            for (uint32_t i = 0; i < t.nsyms && !done; i++) {
                const Elf64_Sym& s = t.sym[i];
                if (s.st_name != to && s.st_name != no) continue;
                uintptr_t a;
                if (!decode(s, &a)) continue;
                m->emplace(need, (void*)a); done = true; fixed++;
                LOGI("[Il2Cpp] backfill[A] %s @sym[%u] +%#x", need, i, (unsigned)(a - t.base));
            }
            /* B：仅失败条目，st_name ±8 邻域修复后名字完全一致才接受 */
            for (uint32_t i : failIdx) {
                if (done) break;
                const Elf64_Sym& s0 = t.sym[i];
                for (int d = -8; d <= 8 && !done; d++) {
                    if (d == 0) continue;
                    int64_t nn = (int64_t)s0.st_name + d;
                    if (nn <= 0 || nn >= (int64_t)t.strsz) continue;
                    Elf64_Sym s = s0; s.st_name = (uint32_t)nn;
                    std::string tryName;
                    if (!SymName(t, s, &tryName) || tryName != need) continue;
                    uintptr_t a;
                    if (!decode(s0, &a)) continue;
                    m->emplace(need, (void*)a); done = true; fixed++;
                    LOGI("[Il2Cpp] backfill[B] %s @sym[%u] st_name%+d +%#x",
                         need, i, d, (unsigned)(a - t.base));
                }
            }
            if (!done) LOGI("[Il2Cpp] backfill: %s unresolved", need);
        }
        /* C：排除法（失败条目唯一 + 缺失必需名唯一） */
        if (failIdx.size() == 1) {
            int missing = 0; const char* missName = nullptr;
            for (const char* need : kNeed)
                if (!m->count(need)) { missing++; missName = need; }
            if (missing == 1 && missName) {
                const Elf64_Sym& s = t.sym[failIdx[0]];
                uintptr_t a;
                if (decode(s, &a)) {
                    m->emplace(missName, (void*)a); fixed++;
                    LOGI("[Il2Cpp] backfill[C] %s @sym[%u] +%#x (排除法唯一候选)",
                         missName, failIdx[0], (unsigned)(a - t.base));
                }
            }
        }
        if (fixed) LOGI("[Il2Cpp] backfill fixed=%d", fixed);
    }
    /* 策略 E（已废弃）：δ 内插候选 + 盲调用验证定位 assemblies 真址——
     * 实测 14 候选全败（调用 SEGV/返回空），且邻域 0x9a0b418~0x9a0b42c
     * 已被异常类函数占据，该地址区间无 assemblies 函数头。
     * 终解：改用 il2cpp_domain_assembly_open 按名开程序集，见 Api::Assemblies。 */

    /* ---- 诊断（trace 包）：strtab 明文扫描 + 必需名检测 + map 样本 ---- */
    {
        int cnt = 0;
        for (uint32_t o = 0; o + 7 <= t.strsz; o++) {
            if (memcmp(t.str + o, "il2cpp_", 7) != 0) continue;
            cnt++;
            if (cnt <= 10) {
                char buf[128];
                size_t k = 0;
                while (k < 120 && t.str[o + k] >= 0x20 && t.str[o + k] <= 0x7e) k++;
                memcpy(buf, t.str + o, k);
                buf[k] = 0;
                LOGI("[Il2Cpp] strtab il2cpp_[%d] @+%u %s", cnt - 1, o, buf);
            }
        }
        LOGI("[Il2Cpp] strtab plaintext il2cpp_ count=%d", cnt);
        static const char* kReq[] = {
            "il2cpp_class_from_name", "il2cpp_class_get_field_from_name",
            "il2cpp_field_get_offset", "il2cpp_domain_get",
            "il2cpp_domain_get_assemblies", "il2cpp_object_get_class",
            "il2cpp_class_get_name"};
        for (const char* n : kReq) {
            size_t ln = strlen(n);
            const char* hit = nullptr;
            for (uint32_t o = 0; o + ln <= t.strsz; o++)
                if (memcmp(t.str + o, n, ln) == 0) { hit = t.str + o; break; }
            LOGI("[Il2Cpp] strtab %s: %s", n, hit ? "PLAINTEXT" : "ABSENT");
        }
        int dumped = 0;
        for (const auto& kv : *m) {
            if (kv.first.compare(0, 7, "il2cpp_") != 0) continue;
            LOGI("[Il2Cpp] map[%02d] %s = +%#x", dumped,
                 kv.first.c_str(), (unsigned)((uintptr_t)kv.second - t.base));
            if (++dumped >= 30) break;
        }
        LOGI("[Il2Cpp] map il2cpp_ sample: %d", dumped);
    }
}

/* xdl_sym 失败后的兜底：进程内自解析 dynsym（懒构建，仅一次） */
static void* DynsymLookup(const char* name) {
    static std::unordered_map<std::string, void*>* map = nullptr;
    static std::once_flag once;
    std::call_once(once, [] {
        map = new std::unordered_map<std::string, void*>();
        SetScanThread(pthread_self());  // SEGV handler 按线程匹配，必须先登记当前线程
        bool ok = SafeProbe([&]() -> bool {
            BuildDynsymMap(sec_lib_il2cpp(), map);
            return true;
        });
        LOGI("[Il2Cpp] dynsym self-parse: %zu syms (probe=%s)",
             map->size(), ok ? "ok" : "SEGV-caught");
    });
    if (!map) return nullptr;
    auto it = map->find(name);
    return it != map->end() ? it->second : nullptr;
}

/* ★2026-10-05 新增：libil2cpp 模块范围校验（themis 诱饵符号过滤）
 *
 * 现象（霓虹深渊2 / com.veewo.neonabyss2，纯真机实测）：30 个 il2cpp_* 符号里
 * 27 个正确落在 libil2cpp.so，3 个被 libthemis-v8.so 抢走：
 *   il2cpp_class_from_name / il2cpp_assembly_get_image / il2cpp_class_get_method_from_name
 * 它们被解析出来的地址前 8 字节是**正常函数序言**（`ffc301d1fd7b01a9` =
 * `sub sp,sp,#0x70; stp x29,x30,[sp,#0x10]`），完全不像诱饵 —— 但拿游戏的
 * Il2CppImage* 去调它必崩：`signal 11 SIGSEGV fault addr 0xb8, null pointer dereference`。
 *
 * 离线定位（解析 /tmp/nhsy2/libil2cpp.live.so 的 phdr + PT_DYNAMIC + DT_HASH；该库
 * 只有 SysV DT_HASH、无 DT_GNU_HASH，nbucket=2455 / nchain=2454 ⇒ 2454 个符号）：
 * 三个名字都在、st_shndx/st_info 正常，但 **st_value 被改成负值**：
 *   il2cpp_assembly_get_image          sym idx 11   st_value 0xfffffffffefd864c
 *   il2cpp_class_from_name             sym idx 1801 st_value 0xfffffffffefd89f4
 *   il2cpp_class_get_method_from_name  sym idx 1429 st_value 0xfffffffffefd8d1c
 * 即故意让 `base + st_value` 以 64 位回绕到 base 之下约 16 MB（-0x10279b4），恰好落进
 * 紧贴 libil2cpp 下方的 libthemis-v8.so（themis 自带一份真 il2cpp 代码副本，并用同名
 * 导出抢先注册）。其余 27 个 st_value 正常，全落在可执行段内的 0x2fe59xx~0x2fe65xx 导出桩簇。
 *
 * ⇒ **dlsym 也救不了**：bionic 同样算 `load_bias + sym->st_value`，会得到同一个假地址。
 *   唯一可靠判据 = 地址必须落在 libil2cpp.so 自己的映射区间内。
 * 其它游戏完全不受影响：正常包所有符号都在本模块内 ⇒ 校验恒通过，行为与改造前一致。
 */
static int PhdrCallbackQuiet(struct dl_phdr_info* info, size_t, void* data) {
    PhdrCtx* ctx = (PhdrCtx*)data;
    const char* n = info->dlpi_name;
    if (!n || !ctx->lib) return 0;
    size_t ln = strlen(ctx->lib), l = strlen(n);
    if (l < ln || strcmp(n + (l - ln), ctx->lib) != 0) return 0;
    ctx->base = (uintptr_t)info->dlpi_addr;
    ctx->phdr = info->dlpi_phdr;
    ctx->phnum = info->dlpi_phnum;
    ctx->found = true;
    return 1;
}

/* libil2cpp.so 的可信映射范围 [base, end)。取不到时返回 false（调用方放行，退回旧行为）。 */
static bool LibIl2CppRange(uintptr_t* outBase, uintptr_t* outEnd) {
    PhdrCtx ctx = {};
    ctx.lib = sec_lib_il2cpp();
    if (!ctx.lib || dl_iterate_phdr(PhdrCallbackQuiet, &ctx) <= 0 || !ctx.found) return false;
    uintptr_t end = ctx.base;
    for (int i = 0; i < ctx.phnum; i++) {
        if (ctx.phdr[i].p_type == PT_LOAD) {
            uintptr_t e = ctx.base + ctx.phdr[i].p_vaddr + ctx.phdr[i].p_memsz;
            if (e > end) end = e;
        }
    }
    if (!ctx.base || end <= ctx.base) return false;
    *outBase = ctx.base;
    *outEnd = end;
    return true;
}

/* 地址是否落在 libil2cpp 自己的映射内。判不了（取不到 phdr）时返回 true = 不拦。 */
static bool InIl2CppRange(const void* p) {
    if (!p) return false;
    uintptr_t b = 0, e = 0;
    if (!LibIl2CppRange(&b, &e)) return true;
    uintptr_t v = (uintptr_t)p;
    return v >= b && v < e;
}

static void* ResolveIl2CppSym(void* handle, const char* name) {
    if (!handle || !name) return nullptr;
    void* p = xdl_sym(handle, name, nullptr);
    if (!p) p = xdl_dsym(handle, name, nullptr);
    /* ★保护壳诱饵过滤（见上方 PhdrCallbackQuiet 前的长注释）：非空 ≠ 正确。 */
    const bool poison = (p != nullptr) && !InIl2CppRange(p);
    if (poison) p = nullptr;

    /* TP 重排 dynsym 但未重建 GNU hash 表 → xdl_sym 可能「静默返回错误的非空
     * 地址」（不是 null，所以旧的 miss 兜底永不触发）。恒取自解析表结果做对照，
     * 首次解析时打一行 diag（xdl/self 双方地址 + 前 8 字节）。 */
    void* self = DynsymLookup(name);  // TP 符号隐藏对策
    if (self && !InIl2CppRange(self)) self = nullptr;
    if (!p) p = self;
    {
        static std::unordered_set<std::string> loggedDiag;
        if (loggedDiag.insert(name).second) {
            char h1[20] = "none", h2[20] = "none";
            uint8_t b1[8] = {0}, b2[8] = {0};
            /* v4：xdl_sym 在 TP 环境下可能返回「非空但非法」的地址（见上注释），
             * 直接 memcpy 会造故障。模拟器上这类自造故障会卡死初始化 ⇒ 先查 maps。 */
            if (p && IsReadableRange((uintptr_t)p, 8) &&
                SafeProbe([&] { memcpy(b1, p, 8); return true; }))
                for (int i = 0; i < 8; i++) snprintf(h1 + i * 2, 3, "%02x", b1[i]);
            if (self && IsReadableRange((uintptr_t)self, 8) &&
                SafeProbe([&] { memcpy(b2, self, 8); return true; }))
                for (int i = 0; i < 8; i++) snprintf(h2 + i * 2, 3, "%02x", b2[i]);
            LOGI("[Il2Cpp] diag %s xdl=%p[%s] self=%p[%s] %s", name, p, h1, self, h2,
                 poison ? "POISONED(dropped)"
                        : ((self && self != p) ? "MISMATCH" : "same"));
        }
    }
    if (!p) {
        static std::unordered_set<std::string> loggedMiss;
        if (loggedMiss.insert(name).second) LOGI("[Il2Cpp] miss %s", name);
    }
    return p;
}

static pthread_t g_scanThread;
static bool g_scanThreadSet = false;
static sigjmp_buf* g_jmpStack[32];
static volatile sig_atomic_t g_jmpDepth = 0;
static volatile sig_atomic_t g_inScan = 0;

/* 前序信号处理器（链式回退用）：首次安装时保存。
 * 绝不把「非我造成」的故障吞掉或原样重发——模拟器实测两者都会变成
 * 「故障指令无限重执」的 R 态空转（mod 初始化卡死）；必须交回前序处理器。 */
static struct sigaction g_prevSegv;
static struct sigaction g_prevBus;
static volatile sig_atomic_t g_prevSaved = 0;

static void ChainSignal(const struct sigaction& p, int sig, siginfo_t* si, void* uc) {
    if (p.sa_flags & SA_SIGINFO) {
        if (p.sa_sigaction) p.sa_sigaction(sig, si, uc);
        return;
    }
    if (p.sa_handler == SIG_IGN) return;
    if (p.sa_handler && p.sa_handler != SIG_DFL) {
        p.sa_handler(sig);
        return;
    }
    /* 真的没有前序处理器：恢复默认并重发（老行为，此时进程会正常终止） */
    signal(sig, SIG_DFL);
    raise(sig);
}

/* 重复故障熔断（v5，v26w 加强）：链式回退后「从处理器返回 = 重执故障指令」⇒ 模拟器上实测变成
 * 无限 SIGSEGV 风暴（houdini 狂刷 "Forwarding signal 11"）→ 一个核 100% 跑满 + logcat 洪水 → ANR。
 * v5 原版按 **tid + 同一地址** 计数（阈值 2），但 v26v2 实测的故障风暴里 `si_addr` **每次都不同**
 * （CRASH 头里 `fault addr --------` 为空）⇒ 熔断从未触发，风暴跑了 **131233 次**、
 * 刷出 375387 行 houdini 寄存器转储，并让 mod tick 线程彻底停摆（四项功能全灭）。
 * v26w 改成 **与地址无关**：同一线程 + 同一信号连续第 3 次即硬终止（SIGKILL），
 * 把「卡死/ANR」降级成一次干净崩溃（用户重启即恢复）。 */
static volatile sig_atomic_t g_faultSig = 0;
static volatile sig_atomic_t g_faultTid = 0;
static volatile sig_atomic_t g_faultHit = 0;
static volatile uintptr_t g_faultAddr = 0;

static void HardTerminate(int sig) {
    (void)sig;
    static const char kMsg[] =
        "[Il2Cpp] repeated fatal fault (same tid+sig, addr-independent) -> self SIGKILL to avoid signal loop\n";
    ssize_t wr = write(2, kMsg, sizeof(kMsg) - 1);
    (void)wr;
    syscall(SYS_tgkill, getpid(), (pid_t)gettid(), SIGKILL);
    _exit(1);
}

static void ScanSegvHandler(int sig, siginfo_t* si, void* uc) {
    if (g_inScan && g_jmpDepth > 0 && g_scanThreadSet &&
        pthread_equal(pthread_self(), g_scanThread)) {
        siglongjmp(*g_jmpStack[g_jmpDepth - 1], 1);
    }
    if (sig == SIGSEGV || sig == SIGBUS) {
        const int tid = (int)gettid();
        const uintptr_t a = si ? (uintptr_t)si->si_addr : (uintptr_t)1;
        /* v26w：只比 tid + 信号，**不比地址**（地址每次不同，见上方注释） */
        if (g_faultHit && g_faultSig == sig && g_faultTid == tid) g_faultHit++;
        else {
            g_faultSig = sig;
            g_faultTid = tid;
            g_faultHit = 1;
        }
        g_faultAddr = a;
        if (g_faultHit >= 3) HardTerminate(sig);
        /* 首次交回前序处理器（ART/libsigchain 的隐式空指针检查会 longjmp 走掉） */
        ChainSignal(sig == SIGSEGV ? g_prevSegv : g_prevBus, sig, si, uc);
        return; /* 前序也能没处理 → 返回即重执 → 下一次进来熔断 */
    }
    signal(sig, SIG_DFL);
    raise(sig);
}

bool PushJmp(sigjmp_buf* buf) {
    /* 保险（模拟器/多线程）：推入时把**当前线程**登记为扫描线程。
     * 否则 ScanSegvHandler 落进非本线程分支 → 本线程自己造成的故障也不会被
     * longjmp 收回，表现为 SafeProbe 恒 false 甚至卡死（该设计本就只支持
     * 单线程探测，全局 depth/stack 非线程局部）。 */
    if (!g_scanThreadSet || !pthread_equal(pthread_self(), g_scanThread)) {
        g_scanThread = pthread_self();
        g_scanThreadSet = true;
    }
    if (g_jmpDepth >= 32 || !buf) return false;
    g_jmpStack[g_jmpDepth++] = buf;
    return true;
}

void PopJmp() {
    if (g_jmpDepth > 0) g_jmpDepth--;
}

void InstallHandlers(struct sigaction* oldSegv, struct sigaction* oldBus) {
    struct sigaction sa;
    memset(&sa, 0, sizeof(sa));
    sa.sa_sigaction = ScanSegvHandler;
    sigemptyset(&sa.sa_mask);
    sa.sa_flags = SA_NODEFER | SA_SIGINFO;
    sigaction(SIGSEGV, &sa, oldSegv);
    sigaction(SIGBUS, &sa, oldBus);
    /* 只在首次安装时记录前序处理器（嵌套安装时 old 即我们自己的处理器） */
    if (!g_prevSaved) {
        if (oldSegv) g_prevSegv = *oldSegv;
        if (oldBus) g_prevBus = *oldBus;
        g_prevSaved = 1;
    }
}

void RestoreHandlers(const struct sigaction* oldSegv, const struct sigaction* oldBus) {
    sigaction(SIGSEGV, oldSegv, nullptr);
    sigaction(SIGBUS, oldBus, nullptr);
}

void SetInScan(int v) { g_inScan = v; }

} // namespace detail

void SetScanThread(pthread_t t) {
    detail::g_scanThread = t;
    detail::g_scanThreadSet = true;
}

Api& GetApi() {
    static Api api;
    return api;
}

bool IsValidHeapPtr(void* p) {
    if (!p) return false;
    uintptr_t v = (uintptr_t)p;
    // 清 top-byte / PAC 无关时的用户态掩码（部分机型）——掩码与区间只由 StarPtr.h 定义
    uintptr_t u = starptr::Untag(v);
    // 旧假设 0x70–0x80 在部分机（物质与魔法实测 so/堆在 0x6c–0x6e）会把合法
    // Il2Cpp 对象全判否 → 功能全哑。放宽到常见 ARM64 用户态区间。
    if (!starptr::InUserVa(u)) return false;      // 4GiB ~ 128TiB，排除空/低地址与非用户态
    // v5（MuMu 实测）：纯区间判定会把 0x400000000000 这类「解释器标记值」判成合法，
    // 随后解引用 → SIGSEGV → 处理链回退即重执 → 无限风暴 → ANR。必须再确认落在可读映射内。
    if (detail::PermCached(u, 1, 1u)) return true;
    // 带 top-byte 标签（MTE/PAC）的指针：maps 里是带标签地址，掩码后再判一次（只放宽，不收紧）
    if (u != v && detail::PermCached(v, 1, 1u)) return true;
    return false;
}

/* maps 级可读/可执行判定（[addr, addr+len) 必须完整落在单个映射内）。
 * 供外部（Games/*.cpp）在做「读目标地址 / 装 inline hook」前做前置守卫。 */
bool IsReadableRange(uintptr_t addr, size_t len) {
    // v26u 热路径：先查 8192 条 maps 快照（零 I/O），未命中才走精确 fopen 判定。
    bool found = false;
    const bool ok = detail::PermCachedSoft(addr, len, 1u, &found);
    if (found) return ok;
    return detail::IsReadableRange(addr, len);
}
bool IsExecutableRange(uintptr_t addr, size_t len) {
    bool found = false;
    const bool ok = detail::PermCachedSoft(addr, len, 4u, &found);
    if (found) return ok;
    return detail::MappingHasPerm(addr, len, 'x');
}

bool Api::Load(const char* libName) {
    if (ready) return true;
    if (!libName) libName = sec_lib_il2cpp();

    void* handle = xdl_open(libName, XDL_DEFAULT);
    if (!handle) {
        LOGI("[Il2Cpp] xdl_open %s failed", libName);
        return false;
    }

    class_from_name   = (class_from_name_t)detail::ResolveIl2CppSym(handle, "il2cpp_class_from_name");
    class_get_field   = (class_get_field_from_name_t)detail::ResolveIl2CppSym(handle, "il2cpp_class_get_field_from_name");
    field_offset      = (field_get_offset_t)detail::ResolveIl2CppSym(handle, "il2cpp_field_get_offset");
    field_static_get  = (field_static_get_value_t)detail::ResolveIl2CppSym(handle, "il2cpp_field_static_get_value");
    domain_get        = (domain_get_t)detail::ResolveIl2CppSym(handle, "il2cpp_domain_get");
    domain_assemblies = (domain_get_assemblies_t)detail::ResolveIl2CppSym(handle, "il2cpp_domain_get_assemblies");
    domain_assembly_open = (domain_assembly_open_t)detail::ResolveIl2CppSym(handle, "il2cpp_domain_assembly_open");
    assembly_image    = (assembly_get_image_t)detail::ResolveIl2CppSym(handle, "il2cpp_assembly_get_image");
    object_class      = (object_get_class_t)detail::ResolveIl2CppSym(handle, "il2cpp_object_get_class");
    class_fields      = (class_get_fields_t)detail::ResolveIl2CppSym(handle, "il2cpp_class_get_fields");
    field_name        = (field_get_name_t)detail::ResolveIl2CppSym(handle, "il2cpp_field_get_name");
    class_parent      = (class_get_parent_t)detail::ResolveIl2CppSym(handle, "il2cpp_class_get_parent");
    field_type        = (field_get_type_t)detail::ResolveIl2CppSym(handle, "il2cpp_field_get_type");
    type_name         = (type_get_name_t)detail::ResolveIl2CppSym(handle, "il2cpp_type_get_name");
    thread_attach     = (thread_attach_t)detail::ResolveIl2CppSym(handle, "il2cpp_thread_attach");
    method_ret_type   = (method_get_return_type_t)detail::ResolveIl2CppSym(handle, "il2cpp_method_get_return_type");
    method_param      = (method_get_param_t)detail::ResolveIl2CppSym(handle, "il2cpp_method_get_param");
    method_flags      = (method_get_flags_t)detail::ResolveIl2CppSym(handle, "il2cpp_method_get_flags");
    class_name        = (class_get_name_t)detail::ResolveIl2CppSym(handle, "il2cpp_class_get_name");
    class_method      = (class_get_method_from_name_t)detail::ResolveIl2CppSym(handle, "il2cpp_class_get_method_from_name");
    runtime_invoke    = (runtime_invoke_t)detail::ResolveIl2CppSym(handle, "il2cpp_runtime_invoke");
    /* 1.5.3 dynsym 中无 il2cpp_string_new，用签名相同的 il2cpp_string_new_wrapper 替代 */
    string_new        = (string_new_t)detail::ResolveIl2CppSym(handle, "il2cpp_string_new");
    if (!string_new)
        string_new    = (string_new_t)detail::ResolveIl2CppSym(handle, "il2cpp_string_new_wrapper");
    image_class_count = (image_get_class_count_t)detail::ResolveIl2CppSym(handle, "il2cpp_image_get_class_count");
    image_class       = (image_get_class_t)detail::ResolveIl2CppSym(handle, "il2cpp_image_get_class");
    method_get_name   = (method_get_name_t)detail::ResolveIl2CppSym(handle, "il2cpp_method_get_name");
    value_box         = (value_box_t)detail::ResolveIl2CppSym(handle, "il2cpp_value_box");
    get_corlib        = (get_corlib_t)detail::ResolveIl2CppSym(handle, "il2cpp_get_corlib");
    /* 可空：仅 GetMethodBySig 的重载消歧用，缺了也不影响 ready */
    class_methods     = (class_get_methods_t)detail::ResolveIl2CppSym(handle, "il2cpp_class_get_methods");
    /* ★2026-10-05：ClassFromName / GetMethod 的降级扫描用（两个符号在 nhsy2 上都未被
     *  themis 污染，离线实测 class_get_namespace st_value=0x2fe59f0 / idx717、
     *  method_get_param_count st_value=0x2fe61d8 / idx928）。缺了只是降级能力弱一点。 */
    class_namespace   = (class_get_namespace_t)detail::ResolveIl2CppSym(handle, "il2cpp_class_get_namespace");
    method_param_count = (method_get_param_count_t)detail::ResolveIl2CppSym(handle, "il2cpp_method_get_param_count");

    /* ★class_from_name 允许为空：保护壳（如 nhsy2 的 themis-v8）会把它伪造成指向
     * 自己的假地址，被上面的模块范围校验丢弃 ⇒ 此时走 ClassFromNameScan 降级扫描
     * （image_get_class_count + image_class + class_name + class_namespace）。
     * 能给出 ClassFromNameScan 能力时，就不算 core 缺失。 */
    const bool class_lookup_ok = class_from_name != nullptr ||
        (image_class_count != nullptr && image_class != nullptr && class_name != nullptr);
    if (!class_lookup_ok || !class_get_field || !field_offset ||
        !domain_get || (!domain_assemblies && !domain_assembly_open) ||
        !object_class || !class_name) {
        LOGI("[Il2Cpp] core api missing (xdl+dynsym) asm_enum=%d asm_open=%d cfname=%d scan=%d",
             domain_assemblies != nullptr, domain_assembly_open != nullptr,
             class_from_name != nullptr, class_lookup_ok && !class_from_name);
        return false;
    }
    if (!class_from_name)
        LOGI("[Il2Cpp] NOTE: il2cpp_class_from_name poisoned/absent -> ClassFromNameScan fallback");
    if (!class_method)
        LOGI("[Il2Cpp] NOTE: il2cpp_class_get_method_from_name poisoned/absent -> GetMethodScan fallback (methods=%d pcount=%d)",
             class_methods != nullptr, method_param_count != nullptr);

    ready = true;
    LOGI("[Il2Cpp] api loaded from %s (xdl+dynsym)", libName);
    return true;
}

/* TP 场景自愈：用自解析 dynsym 表（strtab 明文反查，见 BuildDynsymMap）覆盖
 * 核心符号。仅当 xdl_sym 静默给错地址时才会改变结果，逐条打日志。 */
bool Api::ReprobeCore() {
    struct { const char* name; void** slot; } k[] = {
        { "il2cpp_class_from_name",           (void**)&class_from_name },
        { "il2cpp_class_get_field_from_name", (void**)&class_get_field },
        { "il2cpp_field_get_offset",          (void**)&field_offset },
        { "il2cpp_field_static_get_value",    (void**)&field_static_get },
        { "il2cpp_domain_get",                (void**)&domain_get },
        { "il2cpp_domain_get_assemblies",     (void**)&domain_assemblies },
        { "il2cpp_domain_assembly_open",      (void**)&domain_assembly_open },
        { "il2cpp_assembly_get_image",        (void**)&assembly_image },
        { "il2cpp_object_get_class",          (void**)&object_class },
        { "il2cpp_class_get_name",            (void**)&class_name },
        { "il2cpp_class_get_method_from_name",(void**)&class_method },
        { "il2cpp_runtime_invoke",            (void**)&runtime_invoke },
        { "il2cpp_string_new",                (void**)&string_new },
        { "il2cpp_image_get_class_count",     (void**)&image_class_count },
        { "il2cpp_image_get_class",           (void**)&image_class },
        { "il2cpp_method_get_name",           (void**)&method_get_name },
        { "il2cpp_value_box",                 (void**)&value_box },
        { "il2cpp_get_corlib",                (void**)&get_corlib },
        { "il2cpp_class_get_methods",         (void**)&class_methods },
        { "il2cpp_class_get_namespace",       (void**)&class_namespace },
        { "il2cpp_method_get_param_count",    (void**)&method_param_count },
    };
    int changed = 0, missing = 0, poisoned = 0;
    for (auto& e : k) {
        void* self = detail::DynsymLookup(e.name);
        if (!self) { missing++; continue; }
        /* ★自解析表和 xdl 一样会读到被壳篡改的负 st_value（同一个文件 dynsym），
         * 必须过同一道模块范围校验，否则 reprobe 会把刚丢掉的诱饵地址又装回去。 */
        if (!detail::InIl2CppRange(self)) {
            LOGI("[Il2Cpp] reprobe %s: POISONED %p (out of libil2cpp range) -> keep nullptr",
                 e.name, self);
            *e.slot = nullptr;
            poisoned++;
            continue;
        }
        if (self != *e.slot) {
            LOGI("[Il2Cpp] reprobe %s: %p -> %p", e.name, *e.slot, self);
            *e.slot = self;
            changed++;
        }
    }
    LOGI("[Il2Cpp] reprobe done changed=%d missing=%d poisoned=%d", changed, missing, poisoned);
    return changed > 0;
}

void* Api::Domain() const {
    return domain_get ? domain_get() : nullptr;
}

void** Api::Assemblies(size_t* count) const {
    if (!count) return nullptr;
    void* domain = Domain();
    if (!domain) return nullptr;
    if (domain_assemblies) {
        size_t n = 0;
        void** a = domain_assemblies(domain, &n);
        if (a && n > 0) { *count = n; return a; }
        /* 境界刀鸣实测（1.39.211507 / libil2cpp.so 100MB）：该枚举接口返回
         * 「非空指针 + count=0」——既不是 null 也没有数据。旧代码 `if (a) return a`
         * 会静默把 count=0 交回调用方，TryInit 因此永久失败且无任何日志。
         * 这里改判为「枚举失效」，落到按名 open 的降级路径。 */
        static bool s_enumDead = false;
        if (!s_enumDead) {
            s_enumDead = true;
            LOGI("[Il2Cpp] domain_assemblies -> %p count=%zu 枚举失效 → 转按名 open",
                 (void*)a, n);
        }
    }
    if (!domain_assembly_open) return nullptr;
    if (!asm_fallback_done_) {
        /* 名单覆盖本仓库两款 TP 包：境界刀鸣（Assembly-CSharp 内含
         * GameFramework.LuaManager，已由 runtime global-metadata 反查确认）
         * 与终末地（*.Beyond）。open 失败只打 miss，不影响其它名字。 */
        static const char* kNames[] = {
            "Assembly-CSharp", "GameCore.Runtime", "PX.API",
            "PXUtils.Runtime.AppCtrlr", "UtilTool", "XLog.Runtime", "mscorlib",
            "Gameplay.Beyond", "AbilityUnsafe.Gameplay.Beyond",
            "DynamicSceneUnsafe.Gameplay.Beyond", "FactoryUnsafe.Gameplay.Beyond",
            "Entry.Beyond", "Base.Beyond", "Audio.Beyond", "Input.Beyond",
            "Lua.Beyond"};
        for (const char* n : kNames) {
            void* a = nullptr;
            bool ok = SafeProbe([&] { a = domain_assembly_open(domain, n); return true; });
            if (ok && a) {
                LOGI("[Il2Cpp] asm open ok: %s", n);
                asm_fallback_.push_back(a);
            } else {
                LOGI("[Il2Cpp] asm open miss: %s", n);
            }
        }
        asm_fallback_done_ = true;
        LOGI("[Il2Cpp] Assemblies fallback: %zu assemblies", asm_fallback_.size());
    }
    *count = asm_fallback_.size();
    return asm_fallback_.empty() ? nullptr : asm_fallback_.data();
}

void* Api::ImageOf(void* assembly) const {
    if (!assembly || !IsValidHeapPtr(assembly)) return nullptr;
    /* 境界刀鸣 1.39.211507 实测：调 il2cpp_assembly_get_image 必崩
     * （SIGSEGV/SEGV_ACCERR，fault-pc=libil2cpp.so+0x25a1c48，lr=libstarry_magic.so+0x1cfa4）。
     * 而 domain_assembly_open 的返回值本身有效（7 个真实程序集 open ok、9 个不存在的 miss）。
     * 故直接读 Il2CppAssembly 首字段（Il2CppImage*）绕过该导出；用 SafeProbe 包住解引用，
     * 万一 asm 是坏指针也只是返回 nullptr 并打日志，不会把进程带崩。 */
    void* img = nullptr;
    /* v4：读前先查 maps（不可读 = 等价于故障，直接给 nullptr），
     * 避免在模拟器上靠信号兜底——那会把初始化拖进死循环。 */
    if (!detail::IsReadableRange((uintptr_t)assembly, 8)) return nullptr;
    if (!SafeProbe([&] { img = *(void**)assembly; return true; })) return nullptr;
    static int s_log = 0;
    if (s_log < 12) {
        s_log++;
        LOGI("[Il2Cpp] ImageOf asm=%p -> img=%p", assembly, img);
        if (img && IsValidHeapPtr(img) && detail::IsReadableRange((uintptr_t)img, 8)) {
            const char* nm = nullptr;
            SafeProbe([&] { nm = *(const char**)img; return true; });
            if (nm && IsValidHeapPtr((void*)nm))
                LOGI("[Il2Cpp] ImageOf img=%p name=%.24s", img, nm);
        }
    }
    if (!img || !IsValidHeapPtr(img)) return nullptr;
    return img;
}

void* Api::FindClassInImage(void* image, const char* shortName) const {
    if (!image || !image_class_count || !image_class || !class_name || !shortName) return nullptr;
    uint32_t n = image_class_count(image);
    static int s_dbg = 0;
    bool dbg = (s_dbg < 4);
    if (dbg) { s_dbg++; LOGI("[Il2Cpp] FindClassInImage image=%p n=%u want=%s", image, n, shortName); }
    for (uint32_t i = 0; i < n; i++) {
        void* k = image_class(image, i);
        if (!k) continue;
        const char* cn = class_name(k);
        if (dbg && i < 4) LOGI("[Il2Cpp] class[%u]=%p name=%.40s", i, k, cn ? cn : "(null)");
        if (cn && strcmp(cn, shortName) == 0) return k;
    }
    return nullptr;
}

void* Api::FindClassByShortName(const char* shortName) const {
    size_t asmCount = 0;
    void** assemblies = Assemblies(&asmCount);
    if (!assemblies || asmCount == 0) return nullptr;
    for (size_t i = 0; i < asmCount; i++) {
        void* img = ImageOf(assemblies[i]);
        if (!img) continue;
        void* c = FindClassInImage(img, shortName);
        if (c) return c;
    }
    return nullptr;
}

/* ============================================================================
 * ★2026-10-05 ClassFromNameScan：`il2cpp_class_from_name` 被保护壳污染时的降级查找
 *
 * 背景见 ResolveIl2CppSym 上方的长注释：nhsy2 的 themis-v8 把
 * `il2cpp_class_from_name` 的 dynsym st_value 改成负值，令 `base + st_value`
 * 回绕到 themis 自己的真代码副本 ⇒ 模块范围校验把它丢掉（指针 = nullptr）。
 * 但**另有一整套未被污染的符号**能完成同样的事：
 *     il2cpp_image_get_class_count / il2cpp_image_get_class / il2cpp_class_get_name
 *     / il2cpp_class_get_namespace    （离线逐条核对过 st_value 都落在本模块可执行段内）
 *
 * 做法：按 image 懒建一次「ns \x1f name → klass」全量哈希表，之后 O(1)。
 * 只有真正被查询过的 image 才会建表（FindClassNs 命中即返回，通常只建 1~2 张）。
 * ========================================================================== */
void* Api::ClassFromNameScan(void* image, const char* ns, const char* name) const {
    if (!image || !name) return nullptr;
    if (!image_class_count || !image_class || !class_name) return nullptr;

    static std::mutex s_mtx;
    static std::unordered_map<void*, std::unordered_map<std::string, void*>*> s_cache;

    std::string key;
    if (ns && *ns) { key.assign(ns); key.push_back('\x1f'); }
    key.append(name);

    std::lock_guard<std::mutex> lk(s_mtx);
    auto it = s_cache.find(image);
    std::unordered_map<std::string, void*>* m = nullptr;
    if (it != s_cache.end()) {
        m = it->second;
    } else {
        m = new std::unordered_map<std::string, void*>();
        uint32_t n = image_class_count(image);
        if (n > 300000) {   // 防御：异常 image 就给个空表，不要 OOM/长扫
            LOGI("[Il2Cpp] ClassFromNameScan: image=%p bogus class count %u", image, n);
            n = 0;
        }
        m->reserve(n * 2);
        for (uint32_t i = 0; i < n; i++) {
            void* k = image_class(image, i);
            if (!k) continue;
            const char* cn = class_name(k);
            if (!cn) continue;
            std::string kk;
            if (class_namespace) {
                const char* kns = class_namespace(k);
                if (kns && *kns) { kk.assign(kns); kk.push_back('\x1f'); }
            }
            kk.append(cn);
            /* 同名同命名空间重复时保留第一个，与 il2cpp 自己的线性查找语义一致 */
            m->emplace(std::move(kk), k);
        }
        s_cache[image] = m;
        LOGI("[Il2Cpp] ClassFromNameScan: built map for image=%p classes=%zu", image, m->size());
    }
    auto f = m->find(key);
    return f != m->end() ? f->second : nullptr;
}

void* Api::ClassFromName(void* image, const char* ns, const char* name) const {
    if (!image) return nullptr;
    if (class_from_name) {
        void* k = class_from_name(image, ns, name);
        if (k) return k;
    }
    /* 指针为空（被污染丢弃）或原 API 未命中 → 降级扫描。
     * 注意：只有 class_namespace 在场时才能区分同名类；否则退化为「按短名匹配」。 */
    return ClassFromNameScan(image, ns, name);
}

/* ★2026-10-05 GetMethodScan：`il2cpp_class_get_method_from_name` 被污染时的降级查找。
 * 语义刻意与 il2cpp 原生实现对齐：「按方法表顺序返回第一个 name 匹配且参数个数匹配的
 * 方法」（原生实现也是返回第一个，不是唯一命中），argc < 0 表示不校验参数个数。
 * 只用未被污染的 class_methods 迭代器 + method_get_name + method_param_count。 */
void* Api::GetMethodScan(void* klass, const char* name, int argc) const {
    if (!klass || !name || !class_methods || !method_get_name) return nullptr;
    void* iter = nullptr;
    void* hit = nullptr;
    int guard = 0;
    while (void* mi = class_methods(klass, &iter)) {
        if (++guard > 20000) break;   // 防御：迭代器异常时不无限转
        const char* mn = method_get_name(mi);
        if (!mn || strcmp(mn, name) != 0) continue;
        if (argc >= 0) {
            if (method_param_count) {
                if ((int)method_param_count(mi) != argc) continue;
            } else {
                static std::mutex s_warnMtx;
                static bool s_warned = false;
                std::lock_guard<std::mutex> lk(s_warnMtx);
                if (!s_warned) {
                    s_warned = true;
                    LOGI("[Il2Cpp] GetMethodScan: no method_param_count -> argc unchecked for %s", name);
                }
            }
        }
        hit = mi;
        break;
    }
    return hit;
}

void* Api::GetMethod(void* klass, const char* name, int argc) const {
    if (!klass || !name) return nullptr;
    if (class_method) {
        void* mi = class_method(klass, name, argc);
        if (mi) return mi;
    }
    return GetMethodScan(klass, name, argc);
}

/* 重载消歧：见头文件注释。核心约束是「宁可退回旧行为，也不猜」——
 * 多个候选都满足后缀条件时不挑第一个，而是退回 GetMethod，让调用方自己复核。 */
void* Api::GetMethodBySig(void* klass, const char* name, int argc,
                          const char* retSuffix, const char* p0Suffix) const {
    if (!klass || !name) return nullptr;

    const bool needRet = (retSuffix && *retSuffix);
    const bool needP0  = (p0Suffix  && *p0Suffix);
    if (!needRet && !needP0) return GetMethod(klass, name, argc);

    if (!class_methods || !method_get_name || !method_ret_type ||
        !method_param  || !type_name)
        return GetMethod(klass, name, argc);

    /* idx < 0 → 返回类型；idx >= 0 → 第 idx 个参数。后缀子串比较。 */
    auto TypeEndsWith = [this](void* mi, int idx, const char* suffix) -> bool {
        void* ty = (idx < 0) ? method_ret_type(mi) : method_param(mi, (uint32_t)idx);
        if (!ty) return false;
        char* full = type_name(ty);
        if (!full) return false;
        const size_t lf = strlen(full), ls = strlen(suffix);
        const bool ok = (lf >= ls) && (memcmp(full + (lf - ls), suffix, ls) == 0);
        /* 禁止 free：热更（HybridCLR）类型上 type_name 返回的是运行时管理的字符串，
         * 直接 free 会 SIGABRT（实测 libstarry_magic 装钩路径崩在 libc free）。 */
        return ok;
    };

    void* iter = nullptr;
    void* hit  = nullptr;
    int   hits = 0;
    int   guard = 0;
    while (void* mi = class_methods(klass, &iter)) {
        if (++guard > 4096) break;  /* 热更类的迭代器不终止，必须有上限 */
        const char* mn = method_get_name(mi);
        if (!mn || strcmp(mn, name) != 0) continue;
        if (needRet && !TypeEndsWith(mi, -1, retSuffix)) continue;
        if (needP0  && !TypeEndsWith(mi,  0, p0Suffix))  continue;
        hit = mi;
        if (++hits > 1) break;                /* 仍有歧义 → 不猜 */
    }
    if (hits == 1) return hit;
    return GetMethod(klass, name, argc);
}

size_t Api::FieldOffset(void* klass, const char* name, size_t bias) const {
    if (!klass || !name || !class_get_field || !field_offset) return 0;
    void* f = class_get_field(klass, name);
    if (!f) return 0;
    return field_offset(f) + bias;
}

void* Api::StaticRefField(void* klass, const char* name) const {
    if (!klass || !name || !class_get_field || !field_static_get) return nullptr;
    void* f = class_get_field(klass, name);
    if (!f) return nullptr;
    void* value = nullptr;
    field_static_get(f, &value);
    return value;
}

const char* Api::ClassName(void* obj) const {
    if (!IsValidHeapPtr(obj) || !object_class || !class_name) return "";
    void* k = object_class(obj);
    if (!k) return "";
    const char* n = class_name(k);
    return n ? n : "";
}

void* Api::GetCorlib() const {
    return get_corlib ? get_corlib() : nullptr;
}

void* Api::BoxInt32(int32_t v) const {
    if (!value_box || !class_from_name) return nullptr;
    void* corlib = GetCorlib();
    if (!corlib) return nullptr;
    void* klass = class_from_name(corlib, "System", "Int32");
    if (!klass) return nullptr;
    return value_box(klass, &v);
}

void* Api::BoxBool(bool v) const {
    if (!value_box || !class_from_name) return nullptr;
    void* corlib = GetCorlib();
    if (!corlib) return nullptr;
    void* klass = class_from_name(corlib, "System", "Boolean");
    if (!klass) return nullptr;
    uint8_t b = v ? 1 : 0;
    return value_box(klass, &b);
}

void Api::ScanInstances(void* klass, std::vector<void*>& out, size_t maxScanMB) const {
    if (!klass) return;
    uintptr_t pat = (uintptr_t)klass;
    const uint8_t* patBytes = (const uint8_t*)&pat;

    /* v22 加固：本函数是对 rw anon/[heap] 的**裸字节遍历**，在模拟器上必须限界，
     *   否则一次扫描就是数 GB（实测：开「秒杀小怪」后扫描 1.16s 仍未结束，随后进程以
     *   signal 11 风暴收场）。三道闸：
     *   ① 跳过 ART/houdini 的巨型空间（不是 il2cpp 的 GC 堆，且含大块未提交页）；
     *   ② 单段 >64MB 不扫；③ 总量按字节预算封顶（默认 256MB，可被 maxScanMB 抬高）。 */
    const size_t perRegionCap = 64u << 20;
    size_t budget = maxScanMB ? (maxScanMB << 20) : (256u << 20);
    if (budget > (size_t)(1024u << 20)) budget = (size_t)(1024u << 20);
    size_t used = 0;
    int nreg = 0, nskip = 0;
    size_t hits0 = out.size();

    FILE* f = fopen("/proc/self/maps", "r");
    if (!f) return;
    char line[512];
    while (fgets(line, sizeof(line), f)) {
        uintptr_t start = 0, end = 0;
        char perms[8] = {0};
        char path[256] = {0};
        sscanf(line, "%lx-%lx %s %*s %*s %*s %255s", &start, &end, perms, path);
        if (perms[0] != 'r' || perms[1] != 'w') continue;
        // 空 path、[anon*]、[heap] 都是 Unity/il2cpp 堆常见映射；旧逻辑 path[0]!=0 直接跳过导致扫不到
        if (path[0] != 0 && strncmp(path, "[anon", 5) != 0 && strcmp(path, "[heap]") != 0) continue;
        if (strstr(path, "dalvik") || strstr(path, "jit-cache") || strstr(path, "scudo") ||
            strstr(path, "houdini") || strstr(path, "memfd") || strstr(path, "jit-zygote"))
            continue;
        uintptr_t size = end - start;
        if (size == 0) continue;
        if (size > perRegionCap) {
            nskip++;
            if (nskip <= 8)
                LOGI("[Il2Cpp] scan skip big %p-%p sz=%zu %s", (void*)start, (void*)end, (size_t)size,
                     path[0] ? path : "(anon)");
            continue;
        }
        if (used >= budget) {
            nskip++;
            continue;
        }
        if (used + size > budget) size = budget - used;
        LOGI("[Il2Cpp] scan[%d] %p-%p sz=%zu %s", nreg, (void*)start, (void*)end, (size_t)size,
             path[0] ? path : "(anon)");
        nreg++;
        used += size;
        const uintptr_t scanEnd = start + size;
        for (uintptr_t a = start; a + 8 <= scanEnd; a += 8) {
            const uint8_t* p = (const uint8_t*)a;
            if (memcmp(p, patBytes, 8) == 0) {
                if (a >= start + 8) out.push_back((void*)(a - 8));
            }
        }
    }
    fclose(f);
    LOGI("[Il2Cpp] scan done regions=%d skipped=%d bytes=%zuMB hits=%zu", nreg, nskip,
         used >> 20, out.size() - hits0);
}

} // namespace Il2Cpp
