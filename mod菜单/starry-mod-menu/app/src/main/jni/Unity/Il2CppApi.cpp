/* Unity IL2CPP 公共实现 */
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"

#include <elf.h>
#include <link.h>

#include <cstdio>
#include <cstring>
#include <dlfcn.h>
#include <mutex>
#include <string>
#include <unordered_map>
#include <unordered_set>

#include "Includes/Logger.h"
#include "xDL/xdl.h"

namespace Il2Cpp {
namespace detail {

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
                 * 终审以序言特征（STP a9/MOV aa/ADRP b0 开头）人眼复核。 */
                uint8_t sb[16] = {0}, rb[16] = {0};
                bool sOk = SafeProbe([&] { memcpy(sb, sa, 16); return true; });
                char shex[20] = "unmapped", rhex[20] = "absent";
                if (sOk)
                    for (int i = 0; i < 8; i++) snprintf(shex + i * 2, 3, "%02x", sb[i]);
                if (rt) {
                    if (SafeProbe([&] { memcpy(rb, rt, 16); return true; }))
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

static void* ResolveIl2CppSym(void* handle, const char* name) {
    if (!handle || !name) return nullptr;
    void* p = xdl_sym(handle, name, nullptr);
    if (!p) p = xdl_dsym(handle, name, nullptr);
    /* TP 重排 dynsym 但未重建 GNU hash 表 → xdl_sym 可能「静默返回错误的非空
     * 地址」（不是 null，所以旧的 miss 兜底永不触发）。恒取自解析表结果做对照，
     * 首次解析时打一行 diag（xdl/self 双方地址 + 前 8 字节）。 */
    void* self = DynsymLookup(name);  // TP 符号隐藏对策
    if (!p) p = self;
    {
        static std::unordered_set<std::string> loggedDiag;
        if (loggedDiag.insert(name).second) {
            char h1[20] = "none", h2[20] = "none";
            uint8_t b1[8] = {0}, b2[8] = {0};
            if (p && SafeProbe([&] { memcpy(b1, p, 8); return true; }))
                for (int i = 0; i < 8; i++) snprintf(h1 + i * 2, 3, "%02x", b1[i]);
            if (self && SafeProbe([&] { memcpy(b2, self, 8); return true; }))
                for (int i = 0; i < 8; i++) snprintf(h2 + i * 2, 3, "%02x", b2[i]);
            LOGI("[Il2Cpp] diag %s xdl=%p[%s] self=%p[%s] %s", name, p, h1, self, h2,
                 (self && self != p) ? "MISMATCH" : "same");
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

static void ScanSegvHandler(int sig) {
    if (g_inScan && g_jmpDepth > 0 && g_scanThreadSet &&
        pthread_equal(pthread_self(), g_scanThread)) {
        siglongjmp(*g_jmpStack[g_jmpDepth - 1], 1);
    }
    signal(sig, SIG_DFL);
    raise(sig);
}

bool PushJmp(sigjmp_buf* buf) {
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
    sa.sa_handler = ScanSegvHandler;
    sigemptyset(&sa.sa_mask);
    sa.sa_flags = SA_NODEFER;
    sigaction(SIGSEGV, &sa, oldSegv);
    sigaction(SIGBUS, &sa, oldBus);
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
    // 清 top-byte / PAC 无关时的用户态掩码（部分机型）
    uintptr_t u = v & 0x00FFFFFFFFFFFFFFULL;
    // 旧假设 0x70–0x80 在部分机（物质与魔法实测 so/堆在 0x6c–0x6e）会把合法
    // Il2Cpp 对象全判否 → 功能全哑。放宽到常见 ARM64 用户态区间。
    if (u < 0x100000000ULL) return false;          // <4GB：排除空/低地址
    if (u >= 0x800000000000ULL) return false;     // 非用户态
    return true;
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

    if (!class_from_name || !class_get_field || !field_offset ||
        !domain_get || (!domain_assemblies && !domain_assembly_open) ||
        !object_class || !class_name) {
        LOGI("[Il2Cpp] core api missing (xdl+dynsym) asm_enum=%d asm_open=%d",
             domain_assemblies != nullptr, domain_assembly_open != nullptr);
        return false;
    }

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
    };
    int changed = 0, missing = 0;
    for (auto& e : k) {
        void* self = detail::DynsymLookup(e.name);
        if (!self) { missing++; continue; }
        if (self != *e.slot) {
            LOGI("[Il2Cpp] reprobe %s: %p -> %p", e.name, *e.slot, self);
            *e.slot = self;
            changed++;
        }
    }
    LOGI("[Il2Cpp] reprobe done changed=%d missing=%d", changed, missing);
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
    if (!SafeProbe([&] { img = *(void**)assembly; return true; })) return nullptr;
    static int s_log = 0;
    if (s_log < 12) {
        s_log++;
        LOGI("[Il2Cpp] ImageOf asm=%p -> img=%p", assembly, img);
        if (img && IsValidHeapPtr(img)) {
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

void* Api::ClassFromName(void* image, const char* ns, const char* name) const {
    if (!class_from_name || !image) return nullptr;
    return class_from_name(image, ns, name);
}

void* Api::GetMethod(void* klass, const char* name, int argc) const {
    if (!class_method || !klass || !name) return nullptr;
    return class_method(klass, name, argc);
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
        uintptr_t size = end - start;
        if (size == 0 || size > ((uintptr_t)maxScanMB << 20)) continue;

        for (uintptr_t a = start; a + 8 <= end; a += 8) {
            const uint8_t* p = (const uint8_t*)a;
            if (memcmp(p, patBytes, 8) == 0) {
                if (a >= start + 8) out.push_back((void*)(a - 8));
            }
        }
    }
    fclose(f);
}

} // namespace Il2Cpp
