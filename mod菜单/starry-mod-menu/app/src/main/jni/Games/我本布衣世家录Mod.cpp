/* ================================================================
 *  我本布衣世家录Mod.cpp — 我本布衣：世家录 (com.JAGames.TalesoftheClan / TapTap 1.1.66 / vc94)
 *
 *  v5 版本更新重定位（2026-10-31，1.1.56/vc71 → 1.1.66/vc94）:
 *  1. 新 so（sha256 ce2d6551…）与 1.1.56 so（8d27a4ff…）不同源 ⇒ 23 条 RVA 全部重取
 *  2. 类/方法名逐条不变（ItemProtocol / ItemManager / RoleProtocol / RoleManager / MapProtocol /
 *     Family / ScienceProtocol / AdEngine / GameLaunch），仅广告 SDK 类换名 ToBidAdSDK → TapAdnAdSDK
 *  3. 序言交叉验证：21/23 与 1.1.56 逐字节相同；InitAd（0xF81D0FFE→0xA9BD5FFE）与
 *     TapAdnAdSDK::ShowRewardAd（0xF81D0FFE→0xF81A0FFE）代码有变，按实测回填
 *  4. ItemManager 新增第三重载 TryRemoveItem(int,int)（1.1.56 仅有 ItemPrice / (long,int)）
 *     ⇒ 同功能补挂 1 条，防绕过扣费；功能集仍为 6 项，不新增菜单项
 *  功能集与 hook 机制与 v4 完全一致（1:1 复刻，见 10_docs/我本布衣世家录/路线锁定.md）
 *
 *  v4 版本更新重定位（2026-10-08，1.1.46/vc55 → 1.1.56/vc71）:
 *  1. 旧 so（md5 6e6024a9…）与 1.1.56 so（sha256 8d27a4ff…）不同源，旧表 23 条 RVA 序言 23/23 DRIFT
 *  2. 按「类短名+方法名+参数个数」重新出 Il2CppDumper dump 取新 RVA，逐条序言实测回填
 *  3. 命名空间与类名逐条不变 ⇒ 按名兜底路径（@by-name）仍可用
 *  4. TryRemoveItem 两个重载序言互换（旧表口径疑误），新表以实测为准
 *  功能集与 hook 机制与 v3 完全一致（1:1 复刻，见 10_docs/我本布衣世家录/路线锁定.md）
 *
 *  v2 修复记录（2026-09-28）:
 *  1. delegate 调用改 runtime_invoke（旧版把 Action<int> 对象指针当函数指针 → SIGSEGV）
 *  2. AddItem 系列改为 GetCount + 扣费拦截（旧版 void → int 返回值 ABI 不匹配）
 *  3. CanAffordBuyLand Vector3Int 按 AArch64 ABI 传值（HPR 拆参）
 *  4. InitAd 修正 this 参数
 *  5. GetMaxAP RVA 修正 0x12F82F4 → 0x12F82F8
 *  6. 全部序言用 ELF 文件偏移提取（不再占位）
 *  7. 集成 CrashTrap 信号处理器
 *
 *  地址坐标系：Il2CppDumper RVA = ELF VA = load_bias + RVA。
 * ================================================================ */

#include "我本布衣世家录Mod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <link.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"

extern "C" void CrashTrap_Install(void);

using Il2Cpp::GetApi;

#define WBJY_LOG "[wbjy]"
#define WBJY_TARGET_LIB sec_lib_il2cpp()

// ============================================================
// ① 开关与状态
// ============================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_ad{false};
static std::atomic<bool> g_copper{false};
static std::atomic<bool> g_gold{false};
static std::atomic<bool> g_ap{false};
static std::atomic<bool> g_research{false};
static std::atomic<bool> g_land{false};

static std::atomic<bool> g_inAdHook{false};

// delegate invoke 缓存（class → MethodInfo*）
static std::atomic<void*> g_actionIntClass{nullptr};
static std::atomic<void*> g_actionIntInvoke{nullptr};

// ============================================================
// ② 锚点常量（1.1.66 / vc94）
//    §23 绑定：libil2cpp.so SHA-256 ce2d6551a1cabb46b274d0805c2c15d48252a6d4ae48607cb6575b7d18a0e57c
//             global-metadata.dat SHA-256 b735fc1c8785b569aa57b528a2e756fd731bc32c5a2b67169795bf1f9db5fde0
//    坐标系：dump.cs 的 RVA/VA 字段（= ELF VA），file offset = RVA − 0x4000
//            （可执行段 p_vaddr−p_offset，实测程序头；非通用常数）
//    重定位依据：1.1.56 旧表 23 条在本 so 上 RVA 全变；按「类短名+方法名+参数个数」重新出
//    Il2CppDumper dump 取新值；序言为文件偏移处小端 uint32_t（21/23 与旧表一致）。
// ============================================================
enum : uintptr_t {
    kRvaModifyResearch = 0x1100148,
    kRvaForceRemoveMoney = 0x14D6F0C,
    kRvaForceRemoveCopper = 0x14D6F70,
    kRvaCheckItemEnough = 0x14CEBF8,
    kRvaItemManagerGetCount = 0x14CAC30,
    kRvaItemMgrInstCheckEnough = 0x14D4E54,
    kRvaItemMgrInstForceRemMoney = 0x14D61C8,
    kRvaItemMgrInstForceRemCopper = 0x14D62C0,
    kRvaItemMgrInstTryRemPrice = 0x14CED5C,
    kRvaItemMgrInstTryRemInt = 0x14D5EA8,
    kRvaItemMgrInstTryRemTpl = 0x14D6050,
    kRvaItemMoneyGetCount = 0x14D3CE4,
    kRvaItemMoneyForceRemove = 0x14D3B6C,
    kRvaGetMaxAP = 0x13317EC,
    kRvaIsApEnough = 0x1331C54,
    kRvaModifyAP = 0x1331CA8,
    kRvaRoleModifyAP = 0x1336300,
    kRvaRoleCheckMyApEnough = 0x1335FBC,
    kRvaGetOwnPlotLimit = 0x11214B0,
    kRvaCanAffordBuyLand = 0x14C0E94,
    kRvaInitAd = 0x1411470,
    kRvaShowRewardAd = 0x1424D50,
    kRvaTapAdnShowRewardAd = 0x1425AB0,
    kRvaOnLoginSuccess = 0x10EDDAC,
};

// 序言期望值：从 ELF 文件偏移（RVA - ΔPT_LOAD）读出的真实指令（小端 uint32_t）
enum : uint32_t {
    kInsModifyResearch = 0xA9BE57FE,
    kInsForceRemoveMoney = 0xF81D0FFE,
    kInsForceRemoveCopper = 0xA9BE57FE,
    kInsCheckItemEnough = 0xA9BE57FE,
    kInsItemManagerGetCount = 0xD100C3FF,
    kInsItemMgrInstCheckEnough = 0xA9BF4FFE,
    kInsItemMgrInstForceRemMoney = 0xF81D0FFE,
    kInsItemMgrInstForceRemCopper = 0x2A0103E2,
    kInsItemMgrInstTryRemPrice = 0xF81F0FFE,
    kInsItemMgrInstTryRemInt = 0xD10183FF,
    kInsItemMgrInstTryRemTpl = 0xD10103FF,
    kInsItemMoneyGetCount = 0xD10103FF,
    kInsItemMoneyForceRemove = 0xF81C0FFE,
    kInsGetMaxAP = 0xF81E0FFE,
    kInsIsApEnough = 0xB9407408,
    kInsModifyAP = 0xA9BE57FE,
    kInsRoleModifyAP = 0xA9BD5FFE,
    kInsRoleCheckMyApEnough = 0xF81D0FFE,
    kInsGetOwnPlotLimit = 0xF81D0FFE,
    kInsCanAffordBuyLand = 0xA9BD5FFE,
    kInsInitAd = 0xA9BD5FFE,
    kInsShowRewardAd = 0xF81D0FFE,
    kInsTapAdnShowRewardAd = 0xF81A0FFE,
    kInsOnLoginSuccess = 0xF81D0FFE,
};

// ============================================================
// ③ 安全助手
// ============================================================
static bool OkPtr(void* p) { return p && Il2Cpp::IsValidHeapPtr(p); }

static bool HookAtRva(const char* tag, uintptr_t rva, uint32_t expectInsn,
                      void* hook, void** orig);

// ============================================================
// ④ 地址助手
// ============================================================
static int WbjyPhdrCb(struct dl_phdr_info* info, size_t /*size*/, void* data) {
    if (info->dlpi_name && strstr(info->dlpi_name, WBJY_TARGET_LIB) && info->dlpi_addr) {
        *reinterpret_cast<uintptr_t*>(data) = static_cast<uintptr_t>(info->dlpi_addr);
        return 1;
    }
    return 0;
}

static bool VaInExecLib(uintptr_t va) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return false;
    char line[768];
    bool ok = false;
    while (fgets(line, sizeof(line), fp)) {
        if (!strstr(line, WBJY_TARGET_LIB)) continue;
        unsigned long start = 0, end = 0, off = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (!strchr(perm, 'x')) continue;
        if (va >= start && va < end) { ok = true; break; }
    }
    fclose(fp);
    return ok;
}

static uintptr_t LibBaseAddr() {
    static uintptr_t base = 0;
    if (base) return base;
    uintptr_t found = 0;
    dl_iterate_phdr(WbjyPhdrCb, &found);
    if (found) {
        base = found;
        LOGI(WBJY_LOG " lib base=0x%lX", (unsigned long)base);
    }
    return base;
}

static void* VaFromRva(const char* tag, uintptr_t rva, uint32_t expectInsn) {
    uintptr_t base = LibBaseAddr();
    if (!base) return nullptr;
    uintptr_t va = base + rva;
    if (!VaInExecLib(va)) {
        LOGW(WBJY_LOG " %s rva 0x%lX outside exec maps", tag, (unsigned long)rva);
        return nullptr;
    }
    if (expectInsn) {
        uint32_t got = 0;
        memcpy(&got, reinterpret_cast<void*>(va), sizeof(got));
        if (got != expectInsn) {
            LOGW(WBJY_LOG " %s rva drift got=0x%08X want=0x%08X", tag, got, expectInsn);
            return nullptr;
        }
    }
    return reinterpret_cast<void*>(va);
}

static bool HookAtRva(const char* tag, uintptr_t rva, uint32_t expectInsn,
                      void* hook, void** orig) {
    void* va = VaFromRva(tag, rva, expectInsn);
    if (!va) return false;
    if (DobbyHook(va, hook, orig) != 0) {
        LOGW(WBJY_LOG " %s dobby fail @rva 0x%lX", tag, (unsigned long)rva);
        return false;
    }
    LOGI(WBJY_LOG " %s hooked @rva 0x%lX", tag, (unsigned long)rva);
    return true;
}

static bool HookByNameLocal(const char* tag, const char* ns, const char* cls, const char* method,
                            int argc, void* hook, void** orig) {
    if (!cls || !method) return false;
    auto& api = GetApi();
    if (!api.ready && !api.Load(WBJY_TARGET_LIB)) return false;
    void* klass = api.FindClassByShortName(cls);
    if (!klass && ns) {
        size_t n = 0;
        void** asms = api.Assemblies(&n);
        if (asms) {
            for (size_t i = 0; i < n; ++i) {
                void* img = api.ImageOf(asms[i]);
                if (!img) continue;
                klass = api.ClassFromName(img, ns, cls);
                if (klass) break;
            }
        }
    }
    if (!klass) return false;
    void* mi = api.GetMethod(klass, method, argc);
    if (!mi) return false;
    void* fn = *reinterpret_cast<void**>(mi);
    if (!OkPtr(fn)) return false;
    if (DobbyHook(fn, hook, orig) != 0) return false;
    LOGI(WBJY_LOG " %s hooked %s.%s @name", tag, cls, method);
    return true;
}

struct HookSlot {
    const char* tag;
    uintptr_t   rva;
    uint32_t    expectInsn;
    const char* ns;
    const char* cls;
    const char* method;
    int         argc;
    void*       hookFn;
    void**      origPtr;
};

static bool HookSlotInstall(const HookSlot& s) {
    if (s.origPtr && *s.origPtr) return true;
    if (s.rva && HookAtRva(s.tag, s.rva, s.expectInsn, s.hookFn, s.origPtr)) return true;
    if (s.cls && s.method)
        return HookByNameLocal(s.tag, s.ns, s.cls, s.method, s.argc, s.hookFn, s.origPtr);
    return false;
}

// ============================================================
// ⑤ Delegate invoke（DkzjMod 已验证安全模式）
// ============================================================
static bool InvokeActionInt(void* action, int32_t value) {
    if (!OkPtr(action)) return false;
    auto& api = GetApi();
    if (!api.ready) return false;

    void* klass = api.object_class ? api.object_class(action) : nullptr;
    if (!klass) return false;

    if (g_actionIntClass.load(std::memory_order_relaxed) != klass) {
        void* mi = api.GetMethod(klass, "Invoke", 1);
        if (!mi) {
            LOGW(WBJY_LOG " Action<int>.Invoke resolve miss");
            return false;
        }
        g_actionIntInvoke.store(mi, std::memory_order_relaxed);
        g_actionIntClass.store(klass, std::memory_order_release);
    }
    void* mi = g_actionIntInvoke.load(std::memory_order_acquire);
    if (!mi) return false;

    void* args[1] = {&value};
    void* exception = nullptr;
    api.runtime_invoke(mi, action, args, &exception);
    if (exception) {
        LOGW(WBJY_LOG " ad callback exception");
        return false;
    }
    return true;
}

// ============================================================
// ⑥ Hook 实现
// ============================================================
typedef void (*ModifyResearch_t)(int32_t delta, void* method);
typedef bool (*ForceRemoveMoney_t)(int32_t templateId, int32_t count, void* method);
typedef bool (*ForceRemoveCopper_t)(int32_t count, void* method);
typedef bool (*CheckItemEnough_t)(void* price, void* method);
typedef int32_t (*ItemManagerGetCount_t)(void* self, int32_t templateId, void* method);
typedef int32_t (*GetMaxAP_t)(void* self, void* method);
typedef bool (*IsApEnough_t)(void* self, int32_t need, void* method);
typedef void (*ModifyAP_t)(void* self, int32_t delta, void* method);
typedef void (*RoleModifyAP_t)(int32_t delta, void* method);
typedef bool (*RoleCheckMyApEnough_t)(int32_t need, void* method);
typedef int32_t (*GetOwnPlotLimit_t)(void* self, void* method);
typedef bool (*CanAffordBuyLand_t)(int64_t familyId, int32_t posX, int32_t posY, int32_t posZ, void* method);
typedef void (*InitAd_t)(void* self, void* method);
typedef void (*ShowRewardAd_t)(void* self, void* onSuccess, void* onFailed, void* method);
typedef void (*TapAdnShowRewardAd_t)(void* self, void* onSuccess, void* onFailed, void* method);
typedef void (*OnLoginSuccess_t)(void* method);

static ModifyResearch_t g_origModifyResearch = nullptr;
static ForceRemoveMoney_t g_origForceRemoveMoney = nullptr;
static ForceRemoveCopper_t g_origForceRemoveCopper = nullptr;
static CheckItemEnough_t g_origCheckItemEnough = nullptr;
typedef bool (*InstCheckEnough_t)(void* self, void* price, void* method);
typedef bool (*InstForceRemMoney_t)(void* self, int32_t templateId, int32_t count, void* method);
typedef bool (*InstForceRemCopper_t)(void* self, int32_t count, void* method);
typedef bool (*InstTryRemovePrice_t)(void* self, void* price, void* method);
typedef bool (*InstTryRemoveInt_t)(void* self, int32_t templateId, int32_t count, void* method);
typedef int32_t (*InstGetCount_t)(void* self, int32_t templateId, void* method);

static InstCheckEnough_t g_origItemMgrInstCheckEnough = nullptr;
static InstForceRemMoney_t g_origItemMgrInstForceRemMoney = nullptr;
static InstForceRemCopper_t g_origItemMgrInstForceRemCopper = nullptr;
static InstTryRemovePrice_t g_origItemMgrInstTryRemPrice = nullptr;
static InstTryRemoveInt_t g_origItemMgrInstTryRemInt = nullptr;
static InstTryRemoveInt_t g_origItemMgrInstTryRemTpl = nullptr;
static InstGetCount_t g_origItemMoneyGetCount = nullptr;
static InstForceRemMoney_t g_origItemMoneyForceRemove = nullptr;
static ItemManagerGetCount_t g_origItemManagerGetCount = nullptr;
static GetMaxAP_t g_origGetMaxAP = nullptr;
static IsApEnough_t g_origIsApEnough = nullptr;
static ModifyAP_t g_origModifyAP = nullptr;
static RoleModifyAP_t g_origRoleModifyAP = nullptr;
static RoleCheckMyApEnough_t g_origRoleCheckMyApEnough = nullptr;
static GetOwnPlotLimit_t g_origGetOwnPlotLimit = nullptr;
static CanAffordBuyLand_t g_origCanAffordBuyLand = nullptr;
static InitAd_t g_origInitAd = nullptr;
static ShowRewardAd_t g_origShowRewardAd = nullptr;
static TapAdnShowRewardAd_t g_origTapAdnShowRewardAd = nullptr;
static OnLoginSuccess_t g_origOnLoginSuccess = nullptr;

// --- 科研点 ---
static void Hooked_ModifyResearch(int32_t delta, void* method) {
    if (g_research.load() && delta > 0) {
        delta *= 10;
    }
    if (g_origModifyResearch) g_origModifyResearch(delta, method);
}

// --- 货币（GetCount 钉大值 + 扣费拦截）---
static int32_t Hooked_ItemManagerGetCount(void* self, int32_t templateId, void* method) {
    int32_t real = g_origItemManagerGetCount ? g_origItemManagerGetCount(self, templateId, method) : 0;
    if (g_copper.load() && templateId == 1) return real > 100000 ? real : 100000;
    if (g_gold.load() && templateId == 2) return real > 100000 ? real : 100000;
    return real;
}

static bool Hooked_ForceRemoveMoney(int32_t templateId, int32_t count, void* method) {
    if ((g_copper.load() && templateId == 1) || (g_gold.load() && templateId == 2)) {
        return true;
    }
    return g_origForceRemoveMoney ? g_origForceRemoveMoney(templateId, count, method) : false;
}

static bool Hooked_ForceRemoveCopper(int32_t count, void* method) {
    if (g_copper.load()) return true;
    return g_origForceRemoveCopper ? g_origForceRemoveCopper(count, method) : false;
}

static bool Hooked_CheckItemEnough(void* price, void* method) {
    if (g_copper.load() || g_gold.load()) return true;
    return g_origCheckItemEnough ? g_origCheckItemEnough(price, method) : false;
}

// --- ItemManager instance 方法（消费判定真正入口）---
static bool Hooked_ItemMgrInstCheckEnough(void* self, void* price, void* method) {
    if (g_copper.load() || g_gold.load()) return true;
    return g_origItemMgrInstCheckEnough ? g_origItemMgrInstCheckEnough(self, price, method) : false;
}

static bool Hooked_ItemMgrInstForceRemMoney(void* self, int32_t templateId, int32_t count, void* method) {
    if ((g_copper.load() && templateId == 1) || (g_gold.load() && templateId == 2)) return true;
    return g_origItemMgrInstForceRemMoney ? g_origItemMgrInstForceRemMoney(self, templateId, count, method) : false;
}

static bool Hooked_ItemMgrInstForceRemCopper(void* self, int32_t count, void* method) {
    if (g_copper.load()) return true;
    return g_origItemMgrInstForceRemCopper ? g_origItemMgrInstForceRemCopper(self, count, method) : false;
}

static bool Hooked_ItemMgrInstTryRemPrice(void* self, void* price, void* method) {
    if (g_copper.load() || g_gold.load()) return true;
    return g_origItemMgrInstTryRemPrice ? g_origItemMgrInstTryRemPrice(self, price, method) : false;
}

static bool Hooked_ItemMgrInstTryRemInt(void* self, int32_t templateId, int32_t count, void* method) {
    if ((g_copper.load() && templateId == 1) || (g_gold.load() && templateId == 2)) return true;
    return g_origItemMgrInstTryRemInt ? g_origItemMgrInstTryRemInt(self, templateId, count, method) : false;
}

// 1.1.66 新增重载 TryRemoveItem(int templateId, int count)
static bool Hooked_ItemMgrInstTryRemTpl(void* self, int32_t templateId, int32_t count, void* method) {
    if ((g_copper.load() && templateId == 1) || (g_gold.load() && templateId == 2)) return true;
    return g_origItemMgrInstTryRemTpl ? g_origItemMgrInstTryRemTpl(self, templateId, count, method) : false;
}

static int32_t Hooked_ItemMoneyGetCount(void* self, int32_t templateId, void* method) {
    int32_t real = g_origItemMoneyGetCount ? g_origItemMoneyGetCount(self, templateId, method) : 0;
    if (g_copper.load() && templateId == 1) return real > 100000 ? real : 100000;
    if (g_gold.load() && templateId == 2) return real > 100000 ? real : 100000;
    return real;
}

static bool Hooked_ItemMoneyForceRemove(void* self, int32_t templateId, int32_t count, void* method) {
    if ((g_copper.load() && templateId == 1) || (g_gold.load() && templateId == 2)) return true;
    return g_origItemMoneyForceRemove ? g_origItemMoneyForceRemove(self, templateId, count, method) : false;
}

// --- 行动力 ---
static int32_t Hooked_GetMaxAP(void* self, void* method) {
    if (g_ap.load()) return 9999;
    return g_origGetMaxAP ? g_origGetMaxAP(self, method) : 0;
}

static bool Hooked_IsApEnough(void* self, int32_t need, void* method) {
    if (g_ap.load()) return true;
    return g_origIsApEnough ? g_origIsApEnough(self, need, method) : false;
}

static void Hooked_ModifyAP(void* self, int32_t delta, void* method) {
    if (g_ap.load() && delta < 0) return;
    if (g_origModifyAP) g_origModifyAP(self, delta, method);
}

static void Hooked_RoleModifyAP(int32_t delta, void* method) {
    if (g_ap.load() && delta < 0) return;
    if (g_origRoleModifyAP) g_origRoleModifyAP(delta, method);
}

static bool Hooked_RoleCheckMyApEnough(int32_t need, void* method) {
    if (g_ap.load()) return true;
    return g_origRoleCheckMyApEnough ? g_origRoleCheckMyApEnough(need, method) : false;
}

// --- 土地上限 ---
static int32_t Hooked_GetOwnPlotLimit(void* self, void* method) {
    if (g_land.load()) {
        int32_t base = g_origGetOwnPlotLimit ? g_origGetOwnPlotLimit(self, method) : 0;
        return base > 0 ? base + 50 : 50;
    }
    return g_origGetOwnPlotLimit ? g_origGetOwnPlotLimit(self, method) : 0;
}

// Vector3Int 是 12-byte struct，AArch64 ABI 用 HPR 传参：X1=int_x, X2=int_y, X3=int_z
static bool Hooked_CanAffordBuyLand(int64_t familyId, int32_t posX, int32_t posY, int32_t posZ, void* method) {
    if (g_land.load()) return true;
    return g_origCanAffordBuyLand ? g_origCanAffordBuyLand(familyId, posX, posY, posZ, method) : false;
}

// --- 免广告 ---
static void Hooked_InitAd(void* self, void* method) {
    if (g_ad.load()) return;
    if (g_origInitAd) g_origInitAd(self, method);
}

static void Hooked_ShowRewardAd(void* self, void* onSuccess, void* onFailed, void* method) {
    if (g_ad.load() && !g_inAdHook.load()) {
        g_inAdHook.store(true);
        LOGI(WBJY_LOG " ShowRewardAd intercepted");
        bool ok = InvokeActionInt(onSuccess, 1);
        g_inAdHook.store(false);
        if (ok) return;
        LOGW(WBJY_LOG " ad invoke failed, fallback orig");
    }
    if (g_origShowRewardAd) g_origShowRewardAd(self, onSuccess, onFailed, method);
}

static void Hooked_TapAdnShowRewardAd(void* self, void* onSuccess, void* onFailed, void* method) {
    if (g_ad.load() && !g_inAdHook.load()) {
        g_inAdHook.store(true);
        LOGI(WBJY_LOG " TapAdnShowRewardAd intercepted");
        bool ok = InvokeActionInt(onSuccess, 1);
        g_inAdHook.store(false);
        if (ok) return;
        LOGW(WBJY_LOG " TapAdn ad invoke failed, fallback orig");
    }
    if (g_origTapAdnShowRewardAd) g_origTapAdnShowRewardAd(self, onSuccess, onFailed, method);
}

// --- 登录探针 ---
static void Hooked_OnLoginSuccess(void* method) {
    LOGI(WBJY_LOG " OnLoginSuccess");
    if (g_origOnLoginSuccess) g_origOnLoginSuccess(method);
}

// ============================================================
// ⑦ 挂点表
// ============================================================
static const HookSlot kSlots[] = {
    // 科研点
    {"ModifyResearch",   kRvaModifyResearch,     kInsModifyResearch,       "Gameplay.Science", "ScienceProtocol",  "ModifyResearchPoints", 1,
     (void*)Hooked_ModifyResearch,       (void**)&g_origModifyResearch},
    // 货币
    {"ItemMgrGetCount",  kRvaItemManagerGetCount, kInsItemManagerGetCount, "Gameplay.Item",    "ItemManager",      "GetCount",             1,
     (void*)Hooked_ItemManagerGetCount,  (void**)&g_origItemManagerGetCount},
    {"ForceRemoveMoney", kRvaForceRemoveMoney,   kInsForceRemoveMoney,     "Gameplay.Item",    "ItemProtocol",     "ForceRemoveMoney",     2,
     (void*)Hooked_ForceRemoveMoney,     (void**)&g_origForceRemoveMoney},
    {"ForceRemoveCopper", kRvaForceRemoveCopper, kInsForceRemoveCopper,    "Gameplay.Item",    "ItemProtocol",     "ForceRemoveCopper",    1,
     (void*)Hooked_ForceRemoveCopper,    (void**)&g_origForceRemoveCopper},
    {"CheckItemEnough",  kRvaCheckItemEnough,    kInsCheckItemEnough,      "Gameplay.Item",    "ItemProtocol",     "CheckItemEnough",      1,
     (void*)Hooked_CheckItemEnough,      (void**)&g_origCheckItemEnough},
    {"ItemMgrInstCheckEnough", kRvaItemMgrInstCheckEnough, kInsItemMgrInstCheckEnough, "Gameplay.Item", "ItemManager",   "CheckItemEnough",     1,
     (void*)Hooked_ItemMgrInstCheckEnough, (void**)&g_origItemMgrInstCheckEnough},
    {"ItemMgrInstForceRemMoney", kRvaItemMgrInstForceRemMoney, kInsItemMgrInstForceRemMoney, "Gameplay.Item", "ItemManager", "ForceRemoveMoney",   2,
     (void*)Hooked_ItemMgrInstForceRemMoney, (void**)&g_origItemMgrInstForceRemMoney},
    {"ItemMgrInstForceRemCopper", kRvaItemMgrInstForceRemCopper, kInsItemMgrInstForceRemCopper, "Gameplay.Item", "ItemManager", "ForceRemoveCopper", 1,
     (void*)Hooked_ItemMgrInstForceRemCopper, (void**)&g_origItemMgrInstForceRemCopper},
    {"ItemMgrInstTryRemPrice", kRvaItemMgrInstTryRemPrice, kInsItemMgrInstTryRemPrice, "Gameplay.Item", "ItemManager",  "TryRemoveItem",       1,
     (void*)Hooked_ItemMgrInstTryRemPrice, (void**)&g_origItemMgrInstTryRemPrice},
    {"ItemMgrInstTryRemInt", kRvaItemMgrInstTryRemInt, kInsItemMgrInstTryRemInt,  "Gameplay.Item",   "ItemManager",   "TryRemoveItem",       2,
     (void*)Hooked_ItemMgrInstTryRemInt, (void**)&g_origItemMgrInstTryRemInt},
    {"ItemMgrInstTryRemTpl", kRvaItemMgrInstTryRemTpl, kInsItemMgrInstTryRemTpl,  "Gameplay.Item",   "ItemManager",   "TryRemoveItem",       2,
     (void*)Hooked_ItemMgrInstTryRemTpl, (void**)&g_origItemMgrInstTryRemTpl},
    {"ItemMoneyGetCount", kRvaItemMoneyGetCount, kInsItemMoneyGetCount,   "Gameplay.Item",   "ItemMoneyComponent", "GetCount",       1,
     (void*)Hooked_ItemMoneyGetCount,   (void**)&g_origItemMoneyGetCount},
    {"ItemMoneyForceRemove", kRvaItemMoneyForceRemove, kInsItemMoneyForceRemove, "Gameplay.Item", "ItemMoneyComponent", "ForceRemove",  2,
     (void*)Hooked_ItemMoneyForceRemove, (void**)&g_origItemMoneyForceRemove},
    // 行动力
    {"GetMaxAP",         kRvaGetMaxAP,           kInsGetMaxAP,             "Gameplay.Role",    "RoleManager",      "GetMaxAP",             0,
     (void*)Hooked_GetMaxAP,             (void**)&g_origGetMaxAP},
    {"IsApEnough",       kRvaIsApEnough,         kInsIsApEnough,           "Gameplay.Role",    "RoleManager",      "IsApEnough",           1,
     (void*)Hooked_IsApEnough,           (void**)&g_origIsApEnough},
    {"ModifyAP",         kRvaModifyAP,           kInsModifyAP,             "Gameplay.Role",    "RoleManager",      "ModifyAP",             1,
     (void*)Hooked_ModifyAP,             (void**)&g_origModifyAP},
    {"RoleModifyAP",     kRvaRoleModifyAP,       kInsRoleModifyAP,         "Gameplay.Role",    "RoleProtocol",     "ModifyAP",             1,
     (void*)Hooked_RoleModifyAP,         (void**)&g_origRoleModifyAP},
    {"RoleCheckMyApEnough", kRvaRoleCheckMyApEnough, kInsRoleCheckMyApEnough, "Gameplay.Role",  "RoleProtocol",     "CheckMyApEnough",      1,
     (void*)Hooked_RoleCheckMyApEnough,  (void**)&g_origRoleCheckMyApEnough},
    // 土地
    {"GetOwnPlotLimit",  kRvaGetOwnPlotLimit,    kInsGetOwnPlotLimit,      "Gameplay.Family",  "Family",           "GetOwnPlotLimit",      0,
     (void*)Hooked_GetOwnPlotLimit,      (void**)&g_origGetOwnPlotLimit},
    {"CanAffordBuyLand", kRvaCanAffordBuyLand,   kInsCanAffordBuyLand,     "Gameplay.Map",     "MapProtocol",      "CanAffordBuyLand",     2,
     (void*)Hooked_CanAffordBuyLand,     (void**)&g_origCanAffordBuyLand},
    // 免广告
    {"InitAd",           kRvaInitAd,             kInsInitAd,               "Core.Ad",          "AdEngine",         "InitAd",               0,
     (void*)Hooked_InitAd,               (void**)&g_origInitAd},
    {"ShowRewardAd",     kRvaShowRewardAd,       kInsShowRewardAd,         "Core.Ad",          "AdEngine",         "ShowRewardAd",         2,
     (void*)Hooked_ShowRewardAd,         (void**)&g_origShowRewardAd},
    {"TapAdnShowRewardAd", kRvaTapAdnShowRewardAd, kInsTapAdnShowRewardAd,    "Core.Ad",          "TapAdnAdSDK",      "ShowRewardAd",         2,
     (void*)Hooked_TapAdnShowRewardAd,    (void**)&g_origTapAdnShowRewardAd},
    // 登录探针
    {"OnLoginSuccess",   kRvaOnLoginSuccess,     kInsOnLoginSuccess,       nullptr,            "GameLaunch",       "OnLoginSuccess",       0,
     (void*)Hooked_OnLoginSuccess,       (void**)&g_origOnLoginSuccess},
};

static void InstallSlotByTag(const char* tag) {
    for (const auto& s : kSlots) {
        if (strcmp(s.tag, tag) != 0) continue;
        HookSlotInstall(s);
        return;
    }
}

static void SyncHooks() {
    if (!g_libReady.load()) return;
    if (g_research.load()) InstallSlotByTag("ModifyResearch");
    if (g_copper.load() || g_gold.load()) {
        InstallSlotByTag("ItemMgrGetCount");
        InstallSlotByTag("ForceRemoveMoney");
        InstallSlotByTag("ForceRemoveCopper");
        InstallSlotByTag("CheckItemEnough");
        InstallSlotByTag("ItemMgrInstCheckEnough");
        InstallSlotByTag("ItemMgrInstForceRemMoney");
        InstallSlotByTag("ItemMgrInstForceRemCopper");
        InstallSlotByTag("ItemMgrInstTryRemPrice");
        InstallSlotByTag("ItemMgrInstTryRemInt");
        InstallSlotByTag("ItemMgrInstTryRemTpl");
        InstallSlotByTag("ItemMoneyGetCount");
        InstallSlotByTag("ItemMoneyForceRemove");
    }
    if (g_ap.load()) {
        InstallSlotByTag("GetMaxAP");
        InstallSlotByTag("IsApEnough");
        InstallSlotByTag("ModifyAP");
        InstallSlotByTag("RoleModifyAP");
        InstallSlotByTag("RoleCheckMyApEnough");
    }
    if (g_land.load()) {
        InstallSlotByTag("GetOwnPlotLimit");
        InstallSlotByTag("CanAffordBuyLand");
    }
    if (g_ad.load()) {
        InstallSlotByTag("InitAd");
        InstallSlotByTag("ShowRewardAd");
        InstallSlotByTag("TapAdnShowRewardAd");
    }
    // 登录探针始终安装
    InstallSlotByTag("OnLoginSuccess");
}

// ============================================================
// ⑧ 对外接口
// ============================================================
bool Wbjy_TryInit(JavaVM* /*vm*/) {
    CrashTrap_Install();
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(WBJY_TARGET_LIB)) return false;
    g_libReady.store(true);

    // 立刻初始化 IL2CPP API（免广告 delegate invoke 需要，不等开关触发）
    {
        auto& api = GetApi();
        if (!api.ready) {
            api.Load(WBJY_TARGET_LIB);
            LOGI(WBJY_LOG " IL2CPP API proactively loaded ready=%d", (int)api.ready);
        }
    }

    SyncHooks();
    LOGI(WBJY_LOG " lib ready (load-bias RVA, lazy hooks)");
    return true;
}

bool Wbjy_IsReady() { return g_libReady.load(); }

void Wbjy_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case WBJY_FEAT_AD:       g_ad.store(enabled);       break;
        case WBJY_FEAT_COPPER:   g_copper.store(enabled);   break;
        case WBJY_FEAT_GOLD:     g_gold.store(enabled);     break;
        case WBJY_FEAT_AP:       g_ap.store(enabled);       break;
        case WBJY_FEAT_RESEARCH: g_research.store(enabled); break;
        case WBJY_FEAT_LAND:     g_land.store(enabled);     break;
        default: break;
    }
    SyncHooks();
}

void Wbjy_OnValueChanged(int /*featNum*/, int /*value*/) {}

void Wbjy_Tick(const bool* features, const int* /*values*/) {
    if (features) {
        g_ad.store(features[WBJY_FEAT_AD]);
        g_copper.store(features[WBJY_FEAT_COPPER]);
        g_gold.store(features[WBJY_FEAT_GOLD]);
        g_ap.store(features[WBJY_FEAT_AP]);
        g_research.store(features[WBJY_FEAT_RESEARCH]);
        g_land.store(features[WBJY_FEAT_LAND]);
    }
    if (!g_libReady.load() && isLibraryLoaded(WBJY_TARGET_LIB)) g_libReady.store(true);
    SyncHooks();
}

void Wbjy_ForceClear() {
    g_ad.store(false);
    g_copper.store(false);
    g_gold.store(false);
    g_ap.store(false);
    g_research.store(false);
    g_land.store(false);
}
