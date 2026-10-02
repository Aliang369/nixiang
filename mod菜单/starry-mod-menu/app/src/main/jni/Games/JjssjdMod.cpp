/* ================================================================
 *  星光菜单游戏模块 — 境界死神激斗（com.cmge.ssjd.x7sy 1.38.2）
 *
 *  实现路线（第二阶段，2026-10-02 定稿）：
 *    本作玩法层全在 HybridCLR 热更 DLL（程序集名 Assembly-CSharp），
 *    AOT metadata（dump.cs）里没有任何玩法类 ⇒ 走「热更 DLL 替换」，
 *    与无限升级（Wxsj）同框架同开发者，配方照搬：
 *      1. hook libil2cpp.so 的 System.Reflection.Assembly.Load(byte[])
 *         @ RVA 0x11832AC（序言 16 字节校验；Frida spawn 实测唯一命中，
 *         收到的正是 11,800,576 B 的 Assembly-CSharp.dll）
 *      2. 把内嵌的补丁 DLL（Games/JjssjdPatchedDll.a，+4096 B）
 *         用 il2cpp_array_new 新开 byte[] 后交给原函数（长度变大 ⇒ 必须重开）
 *      3. 补丁 DLL 内注入 Starry.StarryConfig，逐项读文本开关；
 *         C++ 侧只负责「写配置」，功能全在 IL 里门控，配置缺失 = 原版行为
 *
 *      ⚠️ 装载 hook 无条件武装：Assembly.Load 发生在游戏启动 ~6.6s（实测），
 *      早于用户点菜单，所以 DLL 替换不能等开关；行为完全由配置门控
 *      （开关全关 = 补丁 DLL 但全部走原 IL）。
 *
 *  配置键（C++ 写 → DLL 读，2 秒缓存）：
 *    inv=1      基础无敌：血量不下降（ModifyHealth 负 delta 直接 return）
 *    inv_full=1 完全无敌：IsInvincible=true（Hurt 入口早退 + 免疫硬直 + 不被索敌）
 *    dmg_mul=N  伤害倍率（1=关，上限 1000；仅当攻击者 Camp==Player）
 *    nocd=1     无限技能（iSkillState 抹掉 Cooling/NotEnoughMp/NotEnoughFury + CD 归零）
 *  路径硬编码 /data/user/0/com.cmge.ssjd.x7sy/files/starry_config.ini
 *  （HybridCLR 解释器下 Application.persistentDataPath 返回空，见 Patch.cs 注释）
 *
 *  防闪退设施（保留自模板）：双路径挂点 / RVA 漂移拒绝 / 幂等安装 /
 *    orig 必调 / SafeWrite 校验写 / Tick 轻量
 *  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
 * ================================================================ */

#include "JjssjdMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <dlfcn.h>
#include <link.h>
#include <pthread.h>
#include <unistd.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

#define TPL_LOG "[Jjssjd]"  // 生成器会替换为本游戏标签

// 目标库：注册表模式不依赖 GameConfig.h（那是 legacy 配置中心）。
#define TPL_TARGET_LIB sec_lib_il2cpp()

// ============================================================
// ① 开关与 orig（功能位号见 .h 的 enum，与 Entry.cpp featureList 下标对齐）
// ============================================================
static std::atomic<bool> g_libReady{false};  // il2cpp 已映射（≠ 已 hook）
static std::atomic<bool> g_anyHooked{false};
static std::atomic<bool> g_hooked{false};     // Assembly.Load hook 已装上
static std::atomic<bool> g_threadStarted{false};
static std::atomic<bool> g_reenter{false};

static std::atomic<bool> g_invuln{false};     // JJSSJD_FEAT_INVULN
static std::atomic<int>  g_dmgMul{1};         // JJSSJD_FEAT_DMG_MUL（1 = 原版）
static std::atomic<bool> g_infSkill{false};   // JJSSJD_FEAT_INF_SKILL
static std::atomic<bool> g_kill{false};       // JJSSJD_FEAT_KILL（秒杀）

// ============================================================
// ② 目标常量（实测值，换包必须重测）
// ============================================================
// 内嵌补丁 DLL（llvm-objcopy -I binary 生成的 .a；符号名 = 文件相对名 jjssjd_dll）
extern "C" const uint8_t _binary_jjssjd_dll_start[];
extern "C" const uint8_t _binary_jjssjd_dll_end[];

static constexpr size_t kOfficialLen = 11800576;   // 官方 Assembly-CSharp.dll（设备热更版去 8B 头）
static constexpr size_t kPatchedLen  = 11804672;   // 补丁版（+4096 B ⇒ 超原数组容量）
// System.Reflection.Assembly.Load(byte[]) 在 libil2cpp.so 里的 RVA（Il2CppDumper dump.cs）
static constexpr uintptr_t kLoadRva = 0x11832AC;
// 序言 16 字节：str x19,[sp,#-32]! ; stp x29,x30,[sp,#16] ; add x29,sp,#16 ; mov x19,x0
static const uint8_t kLoadPrologue[16] = {
    0xf3, 0x0f, 0x1e, 0xf8, 0xfd, 0x7b, 0x01, 0xa9,
    0xfd, 0x43, 0x00, 0x91, 0xf3, 0x03, 0x00, 0xaa,
};
static const char* const kCfgPath = "/data/user/0/com.cmge.ssjd.x7sy/files/starry_config.ini";

// ============================================================
// ③ 安全助手（防闪退核心，勿删；新代码禁止绕开它们裸读写内存）
// ============================================================
static bool OkPtr(void* p) { return p && Il2Cpp::IsValidHeapPtr(p); }

static void SafeWriteI32(void* obj, size_t off, int32_t v) {
    if (OkPtr(obj)) Il2Cpp::WriteI32(obj, off, v);
}

static void SafeWriteF32(void* obj, size_t off, float v) {
    if (OkPtr(obj)) memcpy(reinterpret_cast<uint8_t*>(obj) + off, &v, sizeof(v));
}

static int32_t SafeReadI32(void* obj, size_t off, int32_t defv) {
    if (!OkPtr(obj)) return defv;
    int32_t v = defv;
    memcpy(&v, reinterpret_cast<uint8_t*>(obj) + off, sizeof(v));
    return v;
}

// 类名缓存：热路径禁止每帧 object_class + class_name 解析
struct ClassNameCache {
    void* klass = nullptr;
    char  name[40] = {};
    bool  valid = false;
};

static const char* CacheClassName(ClassNameCache& cache, void* obj) {
    if (!OkPtr(obj)) return nullptr;
    auto& api = GetApi();
    void* k = api.object_class ? api.object_class(obj) : nullptr;
    if (!k || !api.class_name) return nullptr;
    if (cache.klass != k) {
        const char* n = api.class_name(k);
        cache.klass = k;
        cache.valid = false;
        if (n) {
            size_t i = 0;
            for (; i + 1 < sizeof(cache.name) && n[i]; i++) cache.name[i] = n[i];
            cache.name[i] = 0;
            cache.valid = i > 0;
        }
    }
    return cache.valid ? cache.name : nullptr;
}

static bool NameHas(const char* got, const char* needle) {
    if (!got || !needle) return false;
    for (size_t i = 0; got[i]; i++) {
        size_t j = 0;
        while (needle[j] && got[i + j] == needle[j]) j++;
        if (!needle[j]) return true;
    }
    return false;
}

// ============================================================
// ④ 双路径挂点机器（提炼自 WhrhxMod 实测；禁 base+RVA、禁全 RX 误扫）
// ============================================================
static bool IsLibMapLine(const char* line, const char* lib) {
    return line && lib && strstr(line, lib) && strstr(line, ".so");
}

// file_off → 运行时 VA：只在 RX 段找（libil2cpp 分段映射，base+RVA 会挂错）
static void* FileOffToVa(const char* lib, uintptr_t fileOff) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return nullptr;
    char line[768];
    void* found = nullptr;
    size_t bestSpan = 0;
    while (fgets(line, sizeof(line), fp)) {
        if (!IsLibMapLine(line, lib)) continue;
        unsigned long start = 0, end = 0, off = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (!strchr(perm, 'x') || end <= start) continue;
        if (fileOff < off || fileOff - off >= static_cast<uintptr_t>(end - start)) continue;
        const size_t span = static_cast<size_t>(end - start);
        if (span > bestSpan) {
            bestSpan = span;
            found = reinterpret_cast<void*>(start + (fileOff - off));
        }
    }
    fclose(fp);
    return found;
}

// 目标库在内存里的加载基址（dl_iterate_phdr；RVA = 虚拟地址 ⇒ VA = base + RVA）
struct LibBaseCtx { const char* lib; uintptr_t base; };
static int LibBaseCb(struct dl_phdr_info* info, size_t, void* data) {
    auto* ctx = static_cast<LibBaseCtx*>(data);
    if (!info->dlpi_name || !ctx->lib) return 0;
    if (!strstr(info->dlpi_name, ctx->lib)) return 0;
    ctx->base = static_cast<uintptr_t>(info->dlpi_addr);
    return 1;
}
static uintptr_t LibBase(const char* lib) {
    LibBaseCtx ctx{lib, 0};
    dl_iterate_phdr(LibBaseCb, &ctx);
    return ctx.base;
}

static bool EnsureApi() {
    if (GetApi().ready) return true;
    return GetApi().Load(TPL_TARGET_LIB);
}

static void* FindClassNs(const char* ns, const char* name) {
    auto& api = GetApi();
    size_t n = 0;
    void** asms = api.Assemblies(&n);
    if (!asms || n == 0) return nullptr;
    for (size_t i = 0; i < n; i++) {
        void* img = api.ImageOf(asms[i]);
        if (!img) continue;
        void* k = api.ClassFromName(img, ns, name);
        if (k) return k;
    }
    return nullptr;
}

// 按名路径：类/方法缺失只 log 降级，不整包失败（hook-rules §17）
static bool HookByName(const char* tag, const char* ns, const char* cls, const char* method,
                       int argc, void* hook, void** orig) {
    if (!EnsureApi()) {
        LOGW("%s api not ready — %s will retry via Tick", tag, method);
        return false;
    }
    auto& api = GetApi();
    void* k = FindClassNs(ns, cls);
    if (!k) k = api.FindClassByShortName(cls);
    if (!k) {
        LOGW("%s class miss %s — degrade", tag, cls);
        return false;
    }
    void* mi = api.GetMethod(k, method, argc);
    if (!mi) {
        LOGW("%s method miss %s.%s — degrade, install others", tag, cls, method);
        return false;
    }
    void* fn = *reinterpret_cast<void**>(mi);
    if (!OkPtr(fn)) {
        LOGW("%s bad method ptr %s.%s", tag, cls, method);
        return false;
    }
    if (DobbyHook(fn, hook, orig) != 0) {
        LOGW("%s dobby fail @name %s", tag, method);
        return false;
    }
    LOGI("%s hooked %s.%s", tag, cls, method);
    return true;
}

// RVA 路径：序言字节不符 = 版本漂移/换包，拒绝挂（防挂错地址闪退）
static bool HookAtRvaStrict(const char* tag, uintptr_t rva, const uint8_t* prologue, size_t plen,
                            void* hook, void** orig) {
    // 先按 dl_iterate_phdr 基址，再按 /proc/self/maps 文件偏移（.so 首段 vaddr==offset）
    void* cands[2] = {nullptr, nullptr};
    uintptr_t base = LibBase(TPL_TARGET_LIB);
    if (base) cands[0] = reinterpret_cast<void*>(base + rva);
    cands[1] = FileOffToVa(TPL_TARGET_LIB, rva);
    for (int i = 0; i < 2; i++) {
        void* va = cands[i];
        if (!va) continue;
        if (i == 1 && cands[0] == cands[1]) break;
        uint8_t got[16] = {0};
        memcpy(got, va, plen);
        if (memcmp(got, prologue, plen) != 0) {
            LOGW("%s prologue mismatch @%p (%02X %02X %02X %02X) — skip", tag, va,
                 got[0], got[1], got[2], got[3]);
            continue;
        }
        if (DobbyHook(va, hook, orig) != 0) {
            LOGW("%s dobby fail @%p", tag, va);
            continue;
        }
        LOGI("%s hooked @%p (base=%p rva=0x%lX)", tag, va, (void*)base, (unsigned long)rva);
        return true;
    }
    return false;
}

// ============================================================
// ⑤ 热更 DLL 替换 hook
// ============================================================
static void* (*g_origAssemblyLoad)(void* a0, void* a1, void* a2, void* a3, void* a4, void* a5) = nullptr;

// il2cpp 数组布局（64-bit）：klass@0 monitor@8 bounds@0x10 max_length@0x18 data@0x20
static void* AllocByteArray(size_t len) {
    static void* (*pArrayNew)(void*, size_t) = nullptr;
    static void* (*pClassFromName)(void*, const char*, const char*) = nullptr;
    static void* (*pGetCorlib)() = nullptr;
    if (!pArrayNew) {
        void* h = dlopen(TPL_TARGET_LIB, RTLD_NOW);
        if (!h) h = dlopen(TPL_TARGET_LIB, RTLD_NOLOAD | RTLD_NOW);
        if (!h) {
            LOGW(TPL_LOG " dlopen il2cpp fail — 放行官方 DLL");
            return nullptr;
        }
        pArrayNew = reinterpret_cast<void* (*)(void*, size_t)>(dlsym(h, "il2cpp_array_new"));
        pClassFromName = reinterpret_cast<void* (*)(void*, const char*, const char*)>(
            dlsym(h, "il2cpp_class_from_name"));
        pGetCorlib = reinterpret_cast<void* (*)()>(dlsym(h, "il2cpp_get_corlib"));
        if (!pArrayNew || !pClassFromName || !pGetCorlib) {
            LOGW(TPL_LOG " il2cpp symbol miss (array_new/class_from_name/get_corlib)");
            pArrayNew = nullptr;
            return nullptr;
        }
    }
    void* img = pGetCorlib();
    if (!img) return nullptr;
    void* klass = pClassFromName(img, "System", "Byte");
    if (!klass) return nullptr;
    return pArrayNew(klass, len);
}

// 自变量只用 a0（byte[]）；其余原样透传（Assembly.Load(byte[]) 是静态方法）
static void* Hooked_AssemblyLoad(void* a0, void* a1, void* a2, void* a3, void* a4, void* a5) {
    if (!g_origAssemblyLoad) return nullptr;
    if (g_reenter.exchange(true)) {
        g_reenter.store(false);
        return g_origAssemblyLoad(a0, a1, a2, a3, a4, a5);
    }
    if (!a0) {
        void* r = g_origAssemblyLoad(a0, a1, a2, a3, a4, a5);
        g_reenter.store(false);
        return r;
    }

    // 校验：长度必须是官方 Assembly-CSharp.dll，且前两字节是 PE 魔数
    int32_t len = 0;
    memcpy(&len, reinterpret_cast<const uint8_t*>(a0) + 0x18, 4);
    bool hit = ((size_t)len == kOfficialLen);
    if (!hit) {
        static std::atomic<int> s_odd{0};
        if (len > 0 && s_odd.fetch_add(1) < 4)
            LOGI(TPL_LOG " Assembly.Load 其它程序集 len=%d — 放行", len);
    } else {
        const uint8_t* b = reinterpret_cast<const uint8_t*>(a0) + 0x20;
        hit = (b[0] == 'M' && b[1] == 'Z');
    }
    if (!hit) {
        void* r = g_origAssemblyLoad(a0, a1, a2, a3, a4, a5);
        g_reenter.store(false);
        return r;
    }

    // 长度变大（+4096B）⇒ 原数组装不下，用 il2cpp_array_new 新开 byte[] 交给原函数
    void* arr = AllocByteArray(kPatchedLen);
    if (!arr) {
        LOGE(TPL_LOG " 新 byte[] 分配失败 — 放行官方 DLL（功能不生效）");
        void* r = g_origAssemblyLoad(a0, a1, a2, a3, a4, a5);
        g_reenter.store(false);
        return r;
    }
    memcpy(reinterpret_cast<uint8_t*>(arr) + 0x20, _binary_jjssjd_dll_start, kPatchedLen);
    void* ret = g_origAssemblyLoad(arr, a1, a2, a3, a4, a5);
    g_reenter.store(false);
    LOGI(TPL_LOG " patched dll injected (%u → %u B) ret=%p",
         (unsigned)kOfficialLen, (unsigned)kPatchedLen, ret);
    return ret;
}

// 幂等安装；RVA 序言不符则拒绝（绝不盲打）
static bool TryHookOnce() {
    if (g_hooked.load()) return true;
    if (!g_origAssemblyLoad && HookAtRvaStrict(TPL_LOG " Assembly.Load(byte[])",
                                               kLoadRva, kLoadPrologue, sizeof(kLoadPrologue),
                                               reinterpret_cast<void*>(Hooked_AssemblyLoad),
                                               reinterpret_cast<void**>(&g_origAssemblyLoad))) {
        g_hooked.store(true);
        g_anyHooked.store(true);
        return true;
    }
    if (g_origAssemblyLoad) {
        g_hooked.store(true);
        g_anyHooked.store(true);
        return true;
    }
    return false;
}

// 早装线程：框架等库是 sleep(1) 粒度，太粗（Assembly.Load 实测在 ~6.6s），自己 20ms 轮询
static void* FastRetryThread(void*) {
    for (int i = 0; i < 6000; i++) {  // 6000 × 20ms = 120s
        if (g_hooked.load()) return nullptr;
        if (isLibraryLoaded(TPL_TARGET_LIB)) {
            g_libReady.store(true);
            if (TryHookOnce()) {
                LOGI(TPL_LOG " loader hook ready (i=%d, ~%dms)", i, i * 20);
                return nullptr;
            }
        }
        usleep(20000);
    }
    LOGW(TPL_LOG " loader hook 120s 未装上 — 功能不会生效");
    return nullptr;
}

void Jjssjd_OnLoad(JavaVM* /*vm*/) {
    if (g_threadStarted.exchange(true)) return;
    pthread_t t;
    if (pthread_create(&t, nullptr, FastRetryThread, nullptr) == 0) {
        pthread_detach(t);
        LOGI(TPL_LOG " onLoad: 早装线程已启动（20ms 轮询 libil2cpp）");
    } else {
        g_threadStarted.store(false);
        LOGW(TPL_LOG " onLoad: 早装线程创建失败，回退框架 tryInit");
    }
}

// ============================================================
// ⑥ 配置写入（菜单开关 → starry_config.ini，patched DLL 2 秒内生效）
// ============================================================
static void WriteConfig(bool force) {
    static char s_last[160] = {0};
    char buf[160];
    const int inv = g_invuln.load() ? 1 : 0;
    const int n = snprintf(buf, sizeof(buf),
                           "inv=%d\ninv_full=%d\ndmg_mul=%d\nnocd=%d\nkill=%d\n",
                           inv, inv, g_dmgMul.load(), g_infSkill.load() ? 1 : 0,
                           g_kill.load() ? 1 : 0);
    if (n <= 0 || (size_t)n >= sizeof(buf)) return;
    if (!force && strcmp(buf, s_last) == 0) return;
    FILE* fp = fopen(kCfgPath, "wt");
    if (!fp) {
        LOGW(TPL_LOG " 配置写入失败 %s", kCfgPath);
        return;
    }
    fwrite(buf, 1, (size_t)n, fp);
    fclose(fp);
    memcpy(s_last, buf, sizeof(s_last));
    s_last[sizeof(s_last) - 1] = 0;
    LOGI(TPL_LOG " 配置已更新\n%s", buf);
}

// ============================================================
// ⑦ 组装（装载 hook 无条件武装；行为全在 DLL 侧按配置门控）
// ============================================================
static void SyncHooks() {
    if (!g_libReady.load() && isLibraryLoaded(TPL_TARGET_LIB)) g_libReady.store(true);
    if (g_libReady.load() && !g_hooked.load()) TryHookOnce();
    WriteConfig(false);
}

// ============================================================
// ⑧ 对外接口（ModEntry.h 注册表契约，勿改签名）
// ============================================================
bool Jjssjd_TryInit(JavaVM* /*vm*/) {
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;
    g_libReady.store(true);
    TryHookOnce();
    SyncHooks();
    return g_hooked.load();
}

bool Jjssjd_IsReady() { return g_hooked.load() || g_libReady.load(); }

void Jjssjd_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case JJSSJD_FEAT_INVULN:    g_invuln.store(enabled); break;
        case JJSSJD_FEAT_INF_SKILL: g_infSkill.store(enabled); break;
        case JJSSJD_FEAT_KILL:      g_kill.store(enabled); break;
        case JJSSJD_FEAT_DMG_MUL:   g_dmgMul.store(enabled ? 100 : 1); break;  // 开关语义兜底
        default: break;
    }
    SyncHooks();
}

void Jjssjd_OnValueChanged(int featNum, int value) {
    if (featNum == JJSSJD_FEAT_DMG_MUL) {
        if (value < 1) value = 1;          // 1 = 原版；禁止 0/负数
        else if (value > 100) value = 100;  // 菜单上限（DLL 侧硬上限 1000）
        g_dmgMul.store(value);
    }
    SyncHooks();
}

void Jjssjd_Tick(const bool* features, const int* values) {
    // 只做：同步开关 + 写配置 + 兜底安装。禁止在这里扫堆/遍历对象/逐对象写
    if (features) {
        g_invuln.store(features[JJSSJD_FEAT_INVULN]);
        g_infSkill.store(features[JJSSJD_FEAT_INF_SKILL]);
        g_kill.store(features[JJSSJD_FEAT_KILL]);
    }
    if (values) {
        int v = values[JJSSJD_FEAT_DMG_MUL];
        if (v < 1 || v > 100) v = 1;
        g_dmgMul.store(v);
    }
    SyncHooks();
}

void Jjssjd_ForceClear() {
    g_invuln.store(false);
    g_dmgMul.store(1);
    g_infSkill.store(false);
    g_kill.store(false);
    WriteConfig(true);  // 授权失效 → 立刻回原版行为
}
