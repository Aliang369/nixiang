/* ================================================================
 *  冒险战争 1.1.036 / vc7 (com.mxzz.zbml) — 星空版 native 模块
 *
 *  做法（加载边界换程序集字节；机制移植自 Games/HkldMod.cpp，已真机验证的同构实现）：
 *    1) 按名解析 AOT 方法 System.Reflection.Assembly.Load(byte[])（不被 HybridCLR 解释，
 *       methodPointer 是真代码地址；禁止写死 RVA —— 热更/换版全部作废）
 *    2) Dobby inline hook；收到的 byte[] 若是 PE（MZ）⇒ 算 sha256，
 *          · 等于补丁体 sha    → 幂等放行
 *          · 在**官方白名单**里 → 用 il2cpp_array_new 按补丁体真实长度重新分配容器，
 *                                拷入内嵌补丁字节，再交给原函数
 *          · 都不命中          → PASSTHROUGH 官方版（不崩、不闪退，logcat 一行 + 一次 toast）
 *    3) 补丁版 IL 从 starry_config.ini 读菜单键；本文件的 OnFeature* 负责写该文件
 *
 *  ★ 白名单是 sha256（不是长度）：热更版本频繁，长度相同内容不同的换版长度比不出来。
 *  ★ 白名单为空 ⇒ 一律 PASSTHROUGH。白名单与补丁体长度/sha 由
 *    04_scripts/冒险战争/gen_mxzz_payload_header.py 生成 Games/MxzzPayload.h
 *    （取值来自 10_docs/冒险战争/分析报告_复刻_*.md 的静态结论，禁手填、禁猜）。
 * ================================================================ */
#include "MxzzMod.h"
#include "MxzzPayload.h"
#include "Includes/SecStrings.hpp"

#include <cinttypes>
#include <fcntl.h>
#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <mutex>
#include <string>
#include <vector>
#include <pthread.h>
#include <unistd.h>
#include <sys/stat.h>

#include <android/log.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/StarPtr.h"
#include "Includes/hmac_sha256.h"
#include "Unity/Il2CppApi.h"

#define MXZZ_LOG "[Mxzz]"

/* objcopy 内嵌的明文补丁程序集（jni/libs_mxzz/MxzzPatchedDll.a；符号与对象名绑定，
 * 见 04_scripts/冒险战争/mxzz_build_so.sh：mxzz_dev.bin / mxzz_base.bin） */
extern "C" const uint8_t _binary_mxzz_dev_bin_start[];
extern "C" const uint8_t _binary_mxzz_base_bin_start[];

/* 原生 toast（实现在 MainModern.cpp）；未知版本时给用户一句可读提示 */
extern "C" void StarryShowToast(const char* text);

/* ---- Il2CppArray 布局（Unity 2022 / IL2CPP，与 汉克旅店 vc337 同一实测口径）----
 *   Il2CppObject{klass*,monitor*} = 16B；bounds* @0x10；max_length @0x18；元素数据 @0x20 */
static constexpr size_t kArrLenOff  = 0x18;
static constexpr size_t kArrDataOff = 0x20;

typedef void* (*ArrayNew_t)(void* elementClass, uintptr_t length);
typedef void* (*GchandleNew_t)(void* obj, bool pinned);
typedef void  (*GchandleFree_t)(void* handle);
typedef void* (*AssemblyLoadBytes_t)(void* rawAssembly, const void* method);
typedef int   (*TypeGetType_t)(void* type);   // Il2CppType* -> Il2CppTypeEnum

static AssemblyLoadBytes_t g_origLoad = nullptr;
static ArrayNew_t  g_arrayNew   = nullptr;
static GchandleNew_t g_gcNew    = nullptr;
static GchandleFree_t g_gcFree  = nullptr;
static void* g_byteClass = nullptr;
static TypeGetType_t g_typeGetType = nullptr;

// ---- il2cpp_init 事件钩（方案 E：用事件不用轮询）----
typedef void* (*Il2CppInit_t)(const char* domain_name);
static Il2CppInit_t g_origInit = nullptr;
static std::atomic<bool> g_initHookInstalled{false};
static std::atomic<bool> g_runtimeReady{false};
static void Mxzz_TryHookFromReady();     // 前置声明
static bool TryHookOnce();               // 前置声明（供事件回调与轮询路径共用）

static std::atomic<bool> g_hooked{false};
static std::atomic<int>  g_hits{0};
static std::atomic<int>  g_unknownNotice{0};
static std::atomic<bool> g_retryStarted{false};
static std::mutex g_mtx;

// Il2CppTypeEnum：SZARRAY = 0x1d（byte[]），STRING = 0x0e（string）
static constexpr int kTypeSzArray = 0x1d;
static const char* kAsmLoadClassNs = "System.Reflection";
static const char* kAsmLoadClass   = "Assembly";

// ============================================================
//  starry_config.ini 通道（与 ilpatch 的 Starry.StarryConfig 同路径）
// ============================================================
static std::string PkgName() {
    char b[160] = {0};
    FILE* f = fopen("/proc/self/cmdline", "rb");
    if (!f) return std::string();
    size_t n = fread(b, 1, sizeof(b) - 1, f);
    fclose(f);
    if (!n) return std::string();
    b[n] = 0;
    for (size_t i = 0; i < n; i++) if (b[i] == ':') { b[i] = 0; break; }   // 子进程名取冒号前
    return std::string(b);
}

static std::string ConfigPath() {
    std::string pkg = PkgName();
    if (pkg.empty()) return std::string();
    return "/data/user/0/" + pkg + "/files/starry_config.ini";
}

static bool ReadWholeFile(const std::string& path, std::vector<std::string>* out) {
    FILE* f = fopen(path.c_str(), "rb");
    if (!f) return false;
    std::string all;
    char buf[4096];
    size_t n;
    while ((n = fread(buf, 1, sizeof(buf), f)) > 0) all.append(buf, n);
    fclose(f);
    size_t pos = 0;
    while (pos <= all.size()) {
        size_t e = all.find('\n', pos);
        if (e == std::string::npos) {
            if (pos < all.size()) out->push_back(all.substr(pos));
            break;
        }
        std::string line = all.substr(pos, e - pos);
        while (!line.empty() && (line.back() == '\r' || line.back() == ' ')) line.pop_back();
        if (!line.empty()) out->push_back(line);
        pos = e + 1;
    }
    return true;
}

static void WriteConfigKey(const char* key, const std::string& val) {
    std::string path = ConfigPath();
    if (path.empty()) { LOGE(MXZZ_LOG " pkg name empty, skip %s", key); return; }
    std::string dir = path.substr(0, path.rfind('/'));
    if (!dir.empty()) mkdir(dir.c_str(), 0755);

    std::vector<std::string> lines;
    ReadWholeFile(path, &lines);
    std::string tmp = path + ".tmp";
    FILE* f = fopen(tmp.c_str(), "wb");
    if (!f) { LOGE(MXZZ_LOG " cannot write %s", tmp.c_str()); return; }
    const size_t klen = strlen(key);
    bool replaced = false;
    for (auto& l : lines) {
        if (l.compare(0, klen, key) == 0 && l.size() > klen && l[klen] == '=') {
            fprintf(f, "%s=%s\n", key, val.c_str());
            replaced = true;
        } else {
            fprintf(f, "%s\n", l.c_str());
        }
    }
    if (!replaced) fprintf(f, "%s=%s\n", key, val.c_str());
    fclose(f);
    if (rename(tmp.c_str(), path.c_str()) != 0) LOGE(MXZZ_LOG " rename failed %s", path.c_str());
    else LOGI(MXZZ_LOG " config %s=%s", key, val.c_str());
}

// sha256 -> 64 位小写 hex
static void Sha256Hex(const uint8_t* data, size_t len, char out[65]) {
    uint8_t d[32];
    sha256(data, len, d);
    static const char* kHex = "0123456789abcdef";
    for (int i = 0; i < 32; i++) {
        out[i * 2]     = kHex[(d[i] >> 4) & 0xF];
        out[i * 2 + 1] = kHex[d[i] & 0xF];
    }
    out[64] = 0;
}

// 一次性把「运行时实际装载的程序集」落到私有目录（版本辨识证据用；只诊断，不参与补丁决策）
static void DumpSeenOnce(const uint8_t* data, size_t len, const char* sha) {
    std::string pkg = PkgName();
    if (pkg.empty()) return;
    std::string dir = "/data/user/0/" + pkg + "/files";
    mkdir(dir.c_str(), 0755);
    char name[128];
    snprintf(name, sizeof(name), "%s/mxzz_seen_%.8s_%zu.dll", dir.c_str(), sha, len);
    if (access(name, F_OK) == 0) return;
    FILE* f = fopen(name, "wb");
    if (!f) { LOGE(MXZZ_LOG " cannot dump seen dll %s", name); return; }
    size_t n = fwrite(data, 1, len, f);
    fclose(f);
    LOGI(MXZZ_LOG " 运行时程序集已落盘诊断: %s (%zu/%zu B)", name, n, len);
}


// ============================================================
//  就绪探测（方案 D：只读内存，不碰任何 il2cpp 导出）
//
//  崩溃反推（b3，CRASH #03 = libstarry_magic.so:0x14EA8）：
//    il2cpp_domain_get(0x1A4266C) → b 0x1A05A98（verifier 另证：mono_domain_get/mono_get_root_domain
//    三条导出共用同一 4 字节跳板目标 ⇒ 就是 domain 访问器）
//    0x1A05A98: adrp x20,0x3B1B000 ; ldr x0,[x20,#0xF18]     ; domain 槽
//    0x1A05AB8: adrp x8,0x398A000  ; ldr x8,[x8,#0xE50]      ; ★slot（.got，R_AARCH64_RELATIVE 填）
//    0x1A05AC8: ldr x8,[x8,#0x10]                            ; field = G+0x10 ← 未初始化时 NULL
//    0x1A05AD0: bl 0x19B3554       ; f(NULL)
//    0x18CC45C: ldrb w8,[x0,#0x135]                          ; 空解引用（SEGV_MAPERR addr≈0x135）
//
//  ★数值（本版本 libil2cpp，BuildId 33287dfe…，61,846,032 B；由反汇编 + 段表核算，禁自己估）：
//      B      = maps 里第一个 r--p 映射 start（LOAD#0 vaddr=0 ⇒ base+vaddr 成立）
//      slot   = B + 0x398AE50   在 LOAD#1(.got) [0x394C430,0x398B558) 内，RW
//      G      = *(u64)slot      正常恒等 B + 0x3B18528（RELATIVE 重定位写入）
//      field  = G + 0x10 = B + 0x3B18538，在 LOAD#2(.bss) [0x3990818,0x3D32BA0) 内
//      ready  = (G == B + 0x3B18528) && (read64(G + 0x10) != 0)
//  ★最容易犯的错：只查 slot 非空 —— slot 在重定位完成时就恒非空，会 100% 误判就绪并立刻复现崩溃。
//  ★版本绑定：偏移与 G 期望值都是**本版本**常量，APK/引擎换版即失效 ⇒ 任何一步不符一律判「未就绪」，
//    并退化为「固定延迟」路径（见 Mxzz_TryInit 的 mapsOk=false 分支）。
//  ★读取方式：**禁止裸解引用**（MTE/竞态/坏指针都会直接崩），统一 pread(/proc/self/mem)。
// ============================================================
static constexpr uintptr_t kMxzzSlotOff  = 0x398AE50;   // .got 槽
static constexpr uintptr_t kMxzzGExpect  = 0x3B18528;   // G 的期望偏移（版本绑定）
static constexpr uintptr_t kMxzzBuildIdLen = 20;
static const uint8_t kMxzzBuildId[20] = {
    0x33,0x28,0x7d,0xfe,0x1b,0x92,0x40,0x7f,0xc9,0x9c,
    0x64,0xcc,0xb9,0x82,0x44,0x5e,0x7a,0xd6,0xdc,0x97
};   // BuildId 33287dfe1b92407fc99c64ccb982445e7ad6dc97

struct ModuleSpan { uintptr_t base; size_t size; char path[256]; };

static ModuleSpan FindModuleSpan(const char* soname) {
    ModuleSpan r{}; r.base = 0; r.size = 0; r.path[0] = 0;
    FILE* f = fopen("/proc/self/maps", "r");
    if (!f) return r;
    char line[512];
    const size_t nlen = strlen(soname);
    uintptr_t lo = 0, hi = 0;
    while (fgets(line, sizeof(line), f)) {
        uintptr_t a = 0, b = 0;
        if (sscanf(line, "%" SCNxPTR "-%" SCNxPTR, &a, &b) != 2) continue;
        char* path = strchr(line, '/');
        if (!path) continue;
        size_t plen = strlen(path);
        while (plen && (path[plen - 1] == '\n' || path[plen - 1] == ' ')) path[--plen] = 0;
        if (plen < nlen || strcmp(path + plen - nlen, soname) != 0) continue;
        if (!lo || a < lo) { lo = a; snprintf(r.path, sizeof(r.path), "%s", path); }
        if (b > hi) hi = b;
    }
    fclose(f);
    r.base = lo; r.size = hi > lo ? hi - lo : 0;
    return r;
}

// 从 /proc/self/mem 读（不裸解引用）；返回 false = 不可读
static bool MemRead(uintptr_t addr, void* out, size_t len) {
    static int fd = -1;
    if (fd < 0) fd = open("/proc/self/mem", O_RDONLY | O_CLOEXEC);
    if (fd < 0) return false;
    ssize_t n = pread(fd, out, len, (off_t)addr);
    return n == (ssize_t)len;
}

// 校验 BuildId（PT_NOTE）；读不到/不符 ⇒ false
static bool CheckModuleBuildId(const ModuleSpan& mod) {
    if (!mod.base || !mod.size) return false;
    uint8_t eh[64];
    if (!MemRead(mod.base, eh, sizeof(eh))) return false;
    if (eh[0] != 0x7f || eh[1] != 'E' || eh[2] != 'L' || eh[3] != 'F') return false;
    const uint64_t phoff = *(const uint64_t*)(eh + 0x20);
    const uint16_t phentsize = *(const uint16_t*)(eh + 0x36);
    const uint16_t phnum = *(const uint16_t*)(eh + 0x38);
    for (uint16_t i = 0; i < phnum && i < 64; i++) {
        uint8_t ph[56];
        if (!MemRead(mod.base + (uintptr_t)phoff + (uintptr_t)i * phentsize, ph, sizeof(ph))) return false;
        const uint32_t type = *(const uint32_t*)ph;
        if (type != 4 /*PT_NOTE*/) continue;
        const uint64_t off = *(const uint64_t*)(ph + 8);
        const uint64_t fsz = *(const uint64_t*)(ph + 32);
        uint8_t note[64];
        if (fsz < 36 || fsz > sizeof(note)) continue;
        if (!MemRead(mod.base + (uintptr_t)off, note, (size_t)fsz)) continue;
        const uint32_t namesz = *(const uint32_t*)note;
        const uint32_t descsz = *(const uint32_t*)(note + 4);
        const uint32_t ntype  = *(const uint32_t*)(note + 8);
        if (ntype != 3 /*NT_GNU_BUILD_ID*/) continue;
        const uint8_t* desc = note + 12 + ((namesz + 3) & ~3u);
        if (descsz < kMxzzBuildIdLen) continue;
        return memcmp(desc, kMxzzBuildId, kMxzzBuildIdLen) == 0;
    }
    return false;
}

// 模块闸（maps + BuildId）：只回答「这是不是我们认得的那个 libil2cpp 映射」，
// **不代表 il2cpp 就绪**（verifier C10：probe 通过 ≠ 就绪，thread_attach/class_from_name 各有自己的全局）。
// 取某地址在 /proc/self/maps 中的权限串（诊断用；读不到给 "?"）
static void MapPermOf(uintptr_t addr, char* out, size_t outLen) {
    if (outLen < 8) return;
    snprintf(out, outLen, "?");
    FILE* f = fopen("/proc/self/maps", "r");
    if (!f) return;
    char line[512];
    while (fgets(line, sizeof(line), f)) {
        uintptr_t a = 0, b = 0;
        char perm[8] = {0};
        if (sscanf(line, "%" SCNxPTR "-%" SCNxPTR " %7s", &a, &b, perm) != 3) continue;
        if (addr >= a && addr < b) { snprintf(out, outLen, "%s", perm); break; }
    }
    fclose(f);
}

// 缓存一份 libil2cpp 的 [min_start, max_end)；不看权限位
static ModuleSpan GetIl2CppSpan() {
    static ModuleSpan cached{};
    static bool done = false;
    if (!done) { cached = FindModuleSpan("libil2cpp.so"); done = true; }
    return cached;
}

static bool ModuleValid(bool* mapsOk) {
    static ModuleSpan mod{};
    static int state = 0;          // 0=未查 1=有效 2=不可用
    if (state == 0) {
        mod = FindModuleSpan("libil2cpp.so");
        if (mod.base && mod.size > kMxzzSlotOff + 8) {
            const bool bidOk = CheckModuleBuildId(mod);
            state = bidOk ? 1 : 2;
            LOGI(MXZZ_LOG " 模块闸: base=%p size=%zu buildid=%s → %s",
                 (void*)mod.base, mod.size, bidOk ? "match" : "mismatch/unreadable",
                 bidOk ? "有效" : "不可用");
        } else {
            state = 2;
            LOGI(MXZZ_LOG " 模块闸: libil2cpp 未在 maps 中 → 不可用");
        }
    }
    if (mapsOk) *mapsOk = (state != 2 || mod.base != 0);
    return state == 1;
}

// 字段快照：**只写日志，不参与放行**（避免把「一个字段非空」当就绪充分条件）
static void LogFieldSnapshot() {
    static ModuleSpan mod{};
    if (!mod.base) mod = FindModuleSpan("libil2cpp.so");
    if (!mod.base || mod.size <= kMxzzSlotOff + 8) return;
    uint64_t g = 0, field = 0;
    if (!MemRead(mod.base + kMxzzSlotOff, &g, sizeof(g))) return;
    if (g && MemRead((uintptr_t)g + 0x10, &field, sizeof(field))) {
        LOGI(MXZZ_LOG " 字段快照: G=%p（期望 %p, %s） [G+0x10]=%p",
             (void*)g, (void*)(mod.base + kMxzzGExpect),
             (g == (uint64_t)(mod.base + kMxzzGExpect)) ? "一致" : "不一致", (void*)field);
    }
}

// ============================================================
//  il2cpp_init 事件钩（b4 主路径）
//
//  依据（Lead 实测 llvm-nm -D 06_dumped/冒险战争/1.1.036_il2cpp/input/libil2cpp.so）：
//    il2cpp_init            0x1A42010  ← 有导出
//    il2cpp_init_utf16      0x1A4203C
//    il2cpp_runtime_class_init / il2cpp_thread_attach 亦在导出表
//  ⇒ 「等到 il2cpp_init 被调用并返回」是**事件**，不是猜时间：
//      解析符号 + DobbyHook 全程只碰 native 符号，不调用任何 il2cpp API ⇒ 早期执行天然安全；
//      在钩子里先调原函数（运行时此刻完成初始化），返回后再装 Assembly.Load 钩。
// ============================================================
static void* HookIl2cppInit(const char* domainName) {
    Il2CppInit_t orig = g_origInit;
    void* ret = orig ? orig(domainName) : nullptr;
    if (!g_runtimeReady.exchange(true)) {
        LOGI(MXZZ_LOG " il2cpp_init 已返回（domain=%s）⇒ runtime ready，立即装 Assembly.Load 钩",
             domainName ? domainName : "(null)");
        Mxzz_TryHookFromReady();
    }
    return ret;
}

static bool InstallInitHook() {
    if (g_initHookInstalled.load()) return true;
    void* p = Il2Cpp::ResolveExport("il2cpp_init");
    if (!p) {
        LOGW(MXZZ_LOG " il2cpp_init 未解析到（换发行版？）⇒ 退化路径");
        return false;
    }
    void* orig = nullptr;
    if (DobbyHook(p, (void*)HookIl2cppInit, &orig) != 0 || !orig) {
        LOGE(MXZZ_LOG " DobbyHook(il2cpp_init) 失败 @%p ⇒ 退化路径", p);
        return false;
    }
    g_origInit = (Il2CppInit_t)orig;
    g_initHookInstalled.store(true);
    LOGI(MXZZ_LOG " il2cpp_init hook 已安装 target=%p orig=%p（不调用任何 il2cpp API）", p, orig);
    return true;
}

// 事件触发后的装钩入口（与轮询路径互斥）
static void Mxzz_TryHookFromReady() {
    std::lock_guard<std::mutex> lk(g_mtx);
    if (g_hooked.load()) return;
    TryHookOnce();
}

// ============================================================
//  按名解析 System.Reflection.Assembly.Load(byte[])
// ============================================================
/* 在 klass 的方法表里找「名字 == name、参数个数 == argc、第 p0 个参数类型 == wantTypeEnum」的方法。
 * 为什么不能直接用 GetMethod(name, argc)：Load 有两个 argc==1 的重载
 * （Load(string) / Load(byte[])），先命中的可能是 string 版 ⇒ 只解析不报错、运行时全哑。 */
static void* FindMethodByParamType(void* klass, const char* name, int argc, int wantTypeEnum) {
    auto& api = Il2Cpp::GetApi();
    if (!klass || !api.class_methods || !api.method_get_name) return nullptr;
    if (!g_typeGetType || !api.method_param) return nullptr;
    void* iter = nullptr;
    while (true) {
        void* m = api.class_methods(klass, &iter);
        if (!m) break;
        const char* mn = api.method_get_name(m);
        if (!mn || strcmp(mn, name) != 0) continue;
        if (api.method_param_count && (int)api.method_param_count(m) != argc) continue;
        if (argc == 0) return m;
        void* pt = api.method_param(m, 0);
        if (!pt) continue;
        // 用 Il2CppTypeEnum 判别参数类型（byte[] = SZARRAY 0x1d / string = 0x0e），
        // 不用 il2cpp_type_get_name：其返回值需 free，而该指针在真机上带 tag，
        // 传给 free 会 SIGABRT「Pointer tag ... was truncated」（2026-11-13 汉克旅店实测）。
        if (g_typeGetType(pt) == wantTypeEnum) return m;
    }
    return nullptr;
}

static bool ResolveByteClass() {
    auto& api = Il2Cpp::GetApi();
    if (!api.domain_get || !api.thread_attach || !api.class_from_name || !api.get_corlib) {
        LOGW(MXZZ_LOG " 槽位不全（domain_get/thread_attach/class_from_name/get_corlib）⇒ 视为未就绪");
        return false;
    }
    void* corlib = api.GetCorlib();
    if (!corlib) return false;
    g_byteClass = api.ClassFromName(corlib, "System", "Byte");
    return g_byteClass != nullptr;
}

static void* HookAssemblyLoad(void* rawAssembly, const void* method);

static bool TryHookOnce() {
    auto& api = Il2Cpp::GetApi();
    if (!api.ready) {
        if (!api.Load(sec_lib_il2cpp())) return false;
    }
    if (api.thread_attach) {
        void* dom = api.Domain();
        if (dom) api.thread_attach(dom);   // 后台线程调 il2cpp API 前必须先 attach
    }
    void* corlib = api.GetCorlib();
    if (!corlib) return false;
    // ⚠ 必须在 FindMethodByParamType 之前解析（汉克旅店首版把这段放在 mi 解析之后 ⇒
    //   首轮 g_typeGetType 为空、按签名匹配恒失败，静默退化成 GetMethod 首个 argc=1）。
    if (!g_typeGetType) g_typeGetType = (TypeGetType_t)Il2Cpp::ResolveExport("il2cpp_type_get_type");
    if (!g_arrayNew) g_arrayNew = (ArrayNew_t)Il2Cpp::ResolveExport("il2cpp_array_new");
    if (!g_gcNew)    g_gcNew    = (GchandleNew_t)Il2Cpp::ResolveExport("il2cpp_gchandle_new");
    if (!g_gcFree)   g_gcFree   = (GchandleFree_t)Il2Cpp::ResolveExport("il2cpp_gchandle_free");
    if (!g_arrayNew || !g_gcNew || !g_gcFree) {
        LOGE(MXZZ_LOG " il2cpp_array_new/gchandle 未解析到，禁用替换");
        return false;
    }

    void* klass = api.ClassFromName(corlib, kAsmLoadClassNs, kAsmLoadClass);
    if (!klass) { LOGE(MXZZ_LOG " System.Reflection.Assembly 未解析到"); return false; }

    void* mi = FindMethodByParamType(klass, "Load", 1, kTypeSzArray);
    if (!mi) {
        mi = api.GetMethod(klass, "Load", 1);   // 兜底（并用下面的可执行段校验兜住挂错）
        if (mi) LOGW(MXZZ_LOG " Load(byte[]) 未按签名命中，退化为 GetMethod 首个 argc=1（需日志复核）");
    }
    if (!mi) { LOGE(MXZZ_LOG " Assembly.Load 未解析到"); return false; }
    if (g_typeGetType && api.method_param && api.method_param_count && api.method_param_count(mi) == 1) {
        void* pt = api.method_param(mi, 0);
        LOGI(MXZZ_LOG " Assembly.Load 命中 mi=%p paramType=%d（期望 %d=SZARRAY；%d 即 string）",
             mi, pt ? g_typeGetType(pt) : -1, kTypeSzArray, 0x0e);
    }

    void* target = *(void**)mi;   // MethodInfo.methodPointer @ 0x0

    // ★b5：把「可执行」判据换成「属于 libil2cpp 模块映射」。
    //   依据（b4 实机日志）：methodPointer=0x400028EBD2E4 反推 delta=0x2ABD2E4，
    //   落在 libil2cpp PT_LOAD#1 (R E, vaddr 0x16E814C, filesz 0x2046194 ⇒ 0x37322E0) 内，
    //   是 .text 真函数；而 Il2Cpp::IsExecutableRange 走 maps 'x' 位，在 **houdini 转译环境**下
    //   arm64 代码段的权限位不可信（实测判成不可执行）⇒ 假阴性把装钩路径永久锁死。
    //   故：主判据 = 模块归属；IsExecutableRange 降级为**只打日志**。
    {
        const ModuleSpan mod = GetIl2CppSpan();
        const uintptr_t tv = (uintptr_t)target;
        char perm[8] = "?";
        if (target) MapPermOf(tv, perm, sizeof(perm));
        const bool inModule = target && mod.base && mod.size &&
                              tv >= mod.base && tv < mod.base + mod.size;
        const bool xOk = target && Il2Cpp::IsExecutableRange(tv, 16);   // 仅日志：houdini 下恒 false
        const uintptr_t delta = (target && mod.base && tv >= mod.base) ? (tv - mod.base) : 0;
        LOGI(MXZZ_LOG " methodPointer=0x%" PRIXPTR " base=0x%" PRIXPTR " delta=0x%" PRIXPTR
                      " in_module=%d perm=%s exec_perm=%d（本机 libil2cpp 全段 r--，exec 判据不可用）",
             tv, mod.base, delta, (int)inModule, perm, (int)xOk);
        bool ok = inModule;
        if (!ok) {
            static constexpr uintptr_t kFileSize = 61846032;   // 本版本 libil2cpp 文件大小
            ok = target && delta && delta < kFileSize;
            if (ok) LOGW(MXZZ_LOG " 模块归属失败，按「delta<文件大小」放行（houdini 权限位不可信的兜底）");
        }
        if (!ok) { LOGE(MXZZ_LOG " methodPointer 判定失败 (%p) —— 稍后重试", target); return false; }
    }

    if (!ResolveByteClass()) { LOGE(MXZZ_LOG " System.Byte 未解析到"); return false; }

    void* orig = nullptr;
    if (DobbyHook(target, (void*)HookAssemblyLoad, &orig) != 0 || !orig) {
        LOGE(MXZZ_LOG " DobbyHook 失败 @%p", target);
        return false;
    }
    g_origLoad = (AssemblyLoadBytes_t)orig;
    g_hooked.store(true);
    LOGI(MXZZ_LOG " hook 已安装: Assembly.Load(byte[]) mi=%p target=%p orig=%p", mi, target, orig);
    LOGI(MXZZ_LOG " 载荷策略=%s 载荷表=%d 条", MXZZ_PAYLOAD_POLICY, (int)MXZZ_PAYLOAD_COUNT);
    for (int i = 0; i < MXZZ_PAYLOAD_COUNT; i++) {
        LOGI(MXZZ_LOG "   载荷[%d] 官方sha=%s 补丁=%uB", i, kMxzzPayloads[i].sha, kMxzzPayloads[i].len);
    }
    if (MXZZ_PAYLOAD_COUNT == 0) {
        LOGW(MXZZ_LOG " 载荷表为空 ⇒ 仅 PASSTHROUGH（等静态结论落地后由生成器写入）");
    }
    return true;
}

// ============================================================
//  Hook 本体：官方程序集 -> 补丁版（sha256 白名单）
// ============================================================
// 缓冲区是否已经是补丁体（幂等：同级重入/二次 Load 不应误报未知版本、不应弹 toast）
static bool IsPatchedBody(const char* sha) {
    for (int i = 0; i < MXZZ_SELF_SHA_COUNT; i++) {
        if (strcmp(kMxzzSelfShas[i], sha) == 0) return true;
    }
    return false;
}

// 命中则返回对应补丁体（官方版本 → 同版本补丁体），未命中返回 nullptr
static const MxzzPayloadEntry* FindPayload(const char* sha) {
    for (int i = 0; i < MXZZ_PAYLOAD_COUNT; i++) {
        if (kMxzzPayloads[i].sha && strcmp(kMxzzPayloads[i].sha, sha) == 0) return &kMxzzPayloads[i];
    }
    return nullptr;
}

static void* HookAssemblyLoad(void* rawAssembly, const void* method) {
    AssemblyLoadBytes_t orig = g_origLoad;
    if (!orig) return nullptr;
    if (!rawAssembly || !starptr::LiveFast(rawAssembly)) return orig(rawAssembly, method);
    if (MXZZ_PAYLOAD_COUNT == 0) return orig(rawAssembly, method);   // 无补丁体：纯放行

    const int64_t len = *(const int64_t*)((const char*)rawAssembly + kArrLenOff);
    const uint8_t* data = (const uint8_t*)rawAssembly + kArrDataOff;
    if (len < 2 || data[0] != 'M' || data[1] != 'Z') return orig(rawAssembly, method);   // 非 PE

    char sha[65];
    Sha256Hex(data, (size_t)len, sha);

    // 版本辨识：**无条件**落一次运行时原始程序集（自带 access() 守卫，同 sha 只写一次）。
    // 仅诊断用，不参与补丁决策；缺它则「热更基线 sha256 复核」在真机上无证据可取。
    DumpSeenOnce(data, (size_t)len, sha);

    if (IsPatchedBody(sha)) {
        LOGI(MXZZ_LOG " 缓冲区已是补丁版（幂等放行，sha=%s）", sha);
        return orig(rawAssembly, method);
    }
    const MxzzPayloadEntry* pl = FindPayload(sha);
    if (!pl) {
        if (g_unknownNotice.fetch_add(1) == 0) {
            LOGW(MXZZ_LOG " 未知程序集 sha256=%s len=%d（不在载荷表）→ PASSTHROUGH 官方版",
                 sha, (int)len);
            StarryShowToast("[冒险战争] 游戏热更版本与补丁不符，本次按官方版本运行");
        }
        return orig(rawAssembly, method);
    }

    void* newArr = g_arrayNew(g_byteClass, (uintptr_t)pl->len);
    if (!newArr) {
        LOGE(MXZZ_LOG " il2cpp_array_new 失败 → PASSTHROUGH");
        return orig(rawAssembly, method);
    }
    memcpy((char*)newArr + kArrDataOff, pl->data, pl->len);

    // 分配后到调用前万一有 GC，本地 C 指针不构成根 ⇒ 用 gchandle 钉住
    void* h = g_gcNew(newArr, false);
    int n = g_hits.fetch_add(1) + 1;
    LOGI(MXZZ_LOG " 装载边界替换成功 (#%d) 官方=%d(%s) 补丁=%u B", n, (int)len, sha, pl->len);
    void* ret = orig(newArr, method);
    if (h) g_gcFree(h);
    return ret;
}

static void* RetryThread(void*) {
    // 【主路径·事件】il2cpp_init 钩：装钩只碰 native 符号，安全；
    //   等它的回调置 g_runtimeReady（= 原函数已返回、运行时已初始化），再从回调里装 Assembly.Load 钩。
    // 【退化路径】解析不到 / hook 失败：>=6s 固定延迟 + 每 2s 重试；
    //   verifier 的第二跳探测若可用（maps+BuildId+RELATIVE 校验通过）就作为**必要**条件叠加，
    //   不可用则纯时间闸——两条路都在首行日志写明走的是哪条。
    const bool initHooked = InstallInitHook();
    if (initHooked) {
        LOGI(MXZZ_LOG " 就绪判定=事件(il2cpp_init hook)：等 runtime 初始化完成，最多 60s");
        for (int i = 0; i < 600 && !g_hooked.load(); i++) {
            if (g_runtimeReady.load()) {
                Mxzz_TryHookFromReady();          // 内部持 g_mtx + 再判 g_hooked
                if (g_hooked.load()) return nullptr;
            }
            usleep(100 * 1000);
        }
        // 事件路径超时：退到探测路径再兜一轮（可能 il2cpp_init 已在本 so 加载前跑完）
        if (!g_hooked.load()) LOGW(MXZZ_LOG " il2cpp_init 钩 60s 未触发 ⇒ 转探测/延时路径");
    }

    // 退化路径：**纯时间闸**（≥6s，崩溃点 0.75s 的 8× 余量）+ 模块闸（maps+BuildId）；
    // verifier C10：probe 通过 ≠ il2cpp 就绪 ⇒ 字段快照只打日志，**不参与放行**。
    const int kIter = 30;                     // 30 × 2s = 60s
    for (int i = 0; i < kIter && !g_hooked.load(); i++) {
        const int elapsed_ms = i * 2000;
        bool mapsOk = false;
        const bool valid = ModuleValid(&mapsOk);
        if (i == 0) {
            LOGI(MXZZ_LOG " 就绪判定=时间闸(退化)：≥6s + 模块闸=%s", valid ? "有效" : "不可用");
            LogFieldSnapshot();
        }
        const bool timeOk = elapsed_ms >= 6000;
        if (timeOk && (valid || !mapsOk)) {     // 模块闸可用时必须过；不可用则纯时间
            std::lock_guard<std::mutex> lk(g_mtx);
            if (g_hooked.load()) return nullptr;
            if (TryHookOnce()) return nullptr;
        }
        if (i % 5 == 0) {
            LOGI(MXZZ_LOG " 等待(退化) %dms timeOk=%d module=%d maps=%d", elapsed_ms, (int)timeOk, (int)valid, (int)mapsOk);
        }
        usleep(2000 * 1000);
    }
    if (!g_hooked.load()) {
        LOGE(MXZZ_LOG " 60s 内未就绪/未装钩，放弃本轮（下一轮 tryInit 会再起线程）");
        g_retryStarted.store(false);
    }
    return nullptr;
}

bool Mxzz_TryInit(JavaVM* /*vm*/) {
    if (g_hooked.load()) return true;
    std::lock_guard<std::mutex> lk(g_mtx);
    if (g_hooked.load()) return true;
    if (!g_retryStarted.exchange(true)) {
        pthread_t tid;
        if (pthread_create(&tid, nullptr, RetryThread, nullptr) == 0) {
            pthread_detach(tid);
            LOGI(MXZZ_LOG " 初始化线程已启动（主：il2cpp_init 事件；退：6s+2s 探测）");
        } else {
            g_retryStarted.store(false);
            LOGE(MXZZ_LOG " pthread_create 失败");
        }
    }
    return false;
}

bool Mxzz_IsReady() { return g_hooked.load(); }

// ============================================================
//  b10 探针：读取补丁体内的零依赖计数器（纯 int 自增，不含任何托管调用）
//  ---- 为什么需要 ----
//   b7 起注入的 File 通道（mxzz_patch.log）与 b8 起的 UnityEngine.Debug.Log 通道，
//   在本机运行期**全部零输出**，无法区分「AddHP/SetHP/ReceiveDamage 没被调用」与
//   「日志调用本身失败」。计数器不依赖任何外部方法，由 native 用
//   il2cpp_field_static_get_value 直接读，能给出确定答案。
//   若输出里没有 Starry.MxzzPatch ⇒ 内存里加载的 mygame 不是补丁体；
//   若有但三个计数恒 0 ⇒ 这三个方法在本场战斗里确实没被调用（伤害走了别的路）。
// ============================================================
static void ProbePatchCounters() {
    auto& api = Il2Cpp::GetApi();
    if (!api.ready) return;
    if (!api.domain_assembly_open || !api.assembly_image) return;
    void* dom = api.Domain();
    if (!dom) { LOGE(MXZZ_LOG " probe: domain 为空"); return; }
    void* asmMy = api.domain_assembly_open(dom, "mygame");
    if (!asmMy) { LOGE(MXZZ_LOG " probe: 未找到程序集 mygame"); return; }
    void* img = const_cast<void*>(api.assembly_image(asmMy));  // api 返回 const void*（Il2CppImage*），本模块只读用
    if (!img) { LOGE(MXZZ_LOG " probe: mygame 无 image"); return; }
    void* klass = api.ClassFromName(img, "Starry", "MxzzPatch");
    if (!klass) { LOGE(MXZZ_LOG " probe: mygame 内无 Starry.MxzzPatch ⇒ 已加载的不是补丁体"); return; }
    int hit = 0, gh = 0, kh = 0, lc = -9, lcur = 0, lval = 0, god = 0, kill = 0;
    if (void* f = api.class_get_field(klass, "s_hit"))      api.field_static_get(f, &hit);
    int cb = 0, cf = 0, cadd = 0;
    if (void* f = api.class_get_field(klass, "s_cBtl"))     api.field_static_get(f, &cb);
    if (void* f = api.class_get_field(klass, "s_cFrame"))   api.field_static_get(f, &cf);
    if (void* f = api.class_get_field(klass, "s_cAdd"))     api.field_static_get(f, &cadd);
    if (void* f = api.class_get_field(klass, "s_godHit"))   api.field_static_get(f, &gh);
    if (void* f = api.class_get_field(klass, "s_killHit"))  api.field_static_get(f, &kh);
    if (void* f = api.class_get_field(klass, "s_lastCamp")) api.field_static_get(f, &lc);
    if (void* f = api.class_get_field(klass, "s_lastCur"))  api.field_static_get(f, &lcur);
    if (void* f = api.class_get_field(klass, "s_lastVal"))  api.field_static_get(f, &lval);
    if (void* f = api.class_get_field(klass, "s_god"))      api.field_static_get(f, &god);
    if (void* f = api.class_get_field(klass, "s_kill"))     api.field_static_get(f, &kill);
    // hit=set_HP 命中次数；gh/kh=无敌/秒杀在记录层实际改写次数；lastCamp/Cur/Val=最近一次记录的三元组
    LOGE(MXZZ_LOG " probe: MxzzPatch 在内存中 | 逻辑层 cBtl=%d cFrame=%d cAdd=%d | 表现层 setHpHit=%d godHit=%d killHit=%d | last camp=%d cur=%d val=%d | god=%d kill=%d",
         cb, cf, cadd, hit, gh, kh, lc, lcur, lval, god, kill);
}

void Mxzz_Tick(const bool* /*features*/, const int* /*values*/) {
    // 开关全走 ini 文件通道；这里只做 b10 探针（≈5s 一次，tickInterval=200ms）
    static int n = 0;
    if ((++n % 25) != 0) return;
    ProbePatchCounters();
}

void Mxzz_OnFeatureChanged(int featNum, bool enabled) {
    const char* on = enabled ? "1" : "0";
    switch (featNum) {
    case MXZZ_FEAT_KILL: WriteConfigKey("kill", on); break;
    case MXZZ_FEAT_GOD:  WriteConfigKey("god", on);  break;
    default: break;
    }
}

void Mxzz_OnValueChanged(int /*featNum*/, int /*value*/) {
    // 本作只有两个 Toggle，无滑块 ⇒ 无操作
}

void Mxzz_ForceClear() {
    LOGI(MXZZ_LOG " ForceClear: 关闭所有生效开关");
    WriteConfigKey("kill", "0");
    WriteConfigKey("god", "0");
}