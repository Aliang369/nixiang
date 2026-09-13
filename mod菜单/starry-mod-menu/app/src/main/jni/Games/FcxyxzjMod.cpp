/* ================================================================
 *  FcxyxzjMod.cpp — 凡尘西游修真记 (com.qxjc.fcxyxzj / TapTap 1.1.0 / vc10)
 *
 *  锚点全部来自 1.1.0 静态反汇编 + 真机运行时实测（vaddr = 运行时地址 − libil2cpp 映射基址）：
 *    免广告    ToponVideo::isReady / showAd / PlayerData::getXianjing
 *    无限资源  PlayerData::addData / subData（按 slot 过滤 灵石@0x270 / 修为@0x280）
 *    物品不减  List<T>::RemoveAt（按返回地址白名单过滤 4 个使用入口）
 *    掉落翻倍  addData 内按返回地址识别 FightTimeMgr::GetSuccessWuPing 的两个掉落点
 *    掉落必掉  UnityEngine.Random::Range(int,int) 按返回地址白名单命中
 *              FightTimeMgr::GetSuccessWuPing 内 6 个概率判定点 → 返回下界(0)，判定必过；
 *              另钩 GetSuccessWuPing 本体：主掉落池是「按权重只选一个」，这里对表内其余
 *              正权重候选逐个 replay（权重临时掩码）→ 表内候选全部发放
 *
 *  ★ 保护面：libil2cpp.so 的 il2cpp_assembly_get_image / class_from_name /
 *    class_get_method_from_name / image_get_name / class_get_type 五个符号被
 *    libthemis-v8.so 劫持（dlsym 拿到的是 themis 的假实现）。
 *    因此：image 走裸读 *(void**)assembly；类/方法走 image 类枚举 + class_get_methods。
 *    仅额外自解析 il2cpp_class_get_methods（公共 Api 无此字段，不改公共文件）。
 *
 *  防闪退：懒装（开关全关 = 0 hook）/ RVA 优先 + 序言校验 / 失败转按名 /
 *          幂等（*orig 非空即已挂）/ orig 必调 / Tick 只翻开关不做重活。
 * ================================================================ */

#include "FcxyxzjMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <link.h>  // dl_iterate_phdr：取真实 load bias

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"
#include "xDL/xdl.h"

using Il2Cpp::GetApi;

#define TPL_LOG "[fcxyxzj]"
#define TPL_TARGET_LIB sec_lib_il2cpp()

// ============================================================
// ① 开关（Tick 只翻这些原子量，重活全在懒装/挂点里）
// ============================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_anyHooked{false};
static std::atomic<bool> g_ad{false};
static std::atomic<bool> g_cur{false};
static std::atomic<bool> g_item{false};
static std::atomic<bool> g_drop{false};
static std::atomic<bool> g_dropRate{false};
static std::atomic<bool> g_jade{false};

// ============================================================
// ② 锚点常量（1.1.0 实测；换包必须重新 dump 反汇编核对）
// ============================================================
enum : uintptr_t {
    // —— 资源读写 ——
    kRvaAddData         = 0x1A57A64,  // PlayerData::addData(slot, checkSlot, delta)
    kRvaSubData         = 0x1A57C2C,  // PlayerData::subData(slot, checkSlot, delta)
    // —— 掉落点（FightTimeMgr::GetSuccessWuPing 内 addLinShi/addXiuwei 的返回地址）——
    kRvaDropLinShiRet   = 0x1AA310C,
    kRvaDropXiuweiRet   = 0x1AA3174,
    // —— 掉落必掉（随机函数 + GetSuccessWuPing 内 6 个概率判定点的返回地址）——
    kRvaRandomRange     = 0x35ED058,  // UnityEngine.Random::Range(int,int) 存根（br x2 转真实实现）
    kRvaGetSuccessWuPing= 0x1AA0374,  // FightTimeMgr::GetSuccessWuPing（唯一调用者 fightProPlus @0x1A9BEC0）
    kRvaDropRateRet1    = 0x1AA07B8,  // 权重命中：Random(0,100000) 后 cfg > rand 才命中
    kRvaDropRateRet2    = 0x1AA1438,  // 千分比：cfg*1000 <= rand 跳过
    kRvaDropRateRet3    = 0x1AA1D7C,  // 固定 1%：rand > 99 跳过
    kRvaDropRateRet4    = 0x1AA2540,  // 难度相关 1~3%：rand >= 难度*10 跳过
    kRvaDropRateRet5    = 0x1AA2754,  // 百分比：cfg*2000 <= rand 跳过
    kRvaDropRateRet6    = 0x1AA2980,  // 百分比：cfg*2000 <= rand 跳过（另一分支）
    // —— 灵玉（加密存储于 PlayerData+0x2A0 字符串，显示值 = (解密值-100)/3，上限 30）——
    kRvaGetLingYu       = 0x1A58174,  // PlayerData::get_灵玉（唯一读取入口，含兑换按钮阈值判定）
    // —— 广告 ——
    kRvaShowAd          = 0x1ABA808,  // ToponVideo::showAd
    kRvaIsReady         = 0x1ABA8A0,  // ToponVideo::isReady
    kRvaOnReward        = 0x1ABACEC,  // ToponVideo::onReward（SDK 回调，静态调用图里 0 调用者）
    kRvaGetXianjing     = 0x1A58604,  // PlayerData::getXianjing（祈福免广告判定 >=1）
    // —— 物品消耗（List<T>::RemoveAt 的调用点返回地址白名单）——
    kRvaRemoveAt        = 0x24C3DC4,  // System.Collections.Generic.List<T>::RemoveAt(int)
    kRvaUseYaoPingRet   = 0x1B98D14,  // XxYaopBaoView::useYaoPing（背包药瓶）
    kRvaUseTaskWpRet    = 0x1B95A98,  // XxWpBaoView::BtnUseTaskWpBtn（背包物品）
    kRvaUseFuwenshuRet  = 0x1B554E8,  // XxFuShiBaoView::BtnUseFuwenshu（符文书使用）
    kRvaDrinkProRet     = 0x1B361BC,  // PlayerControl::DrinkPro（快捷栏喝药；此处 count 先 --）
};

// 序言校验（首 4 字节；0 = 不校验）
enum : uint32_t {
    kInsAddData     = 0xD101C3FF,  // sub sp, sp, #0x70
    kInsSubData     = 0xD10183FF,  // sub sp, sp, #0x60
    kInsOnReward    = 0xF81C0FFE,  // str x30, [sp, #-0x40]!
    kInsRemoveAt    = 0xF81E0FFE,  // str x30, [sp, #-0x20]!
    kInsShowAdLike  = 0xA9BE57FE,  // stp x30, x21, [sp, #-0x20]!（showAd/isReady/getXianjing 同序言）
    kInsGetSuccessWuPing = 0xD101C3FF,  // sub sp, sp, #0x70
    kInsGetLingYu   = 0xF81F0FFE,  // str x30, [sp, #-0x10]!
};

enum : size_t {
    kOffPdLingshiSlot  = 0x270,  // PlayerData::allLingshi
    kOffPdXiuweiSlot   = 0x280,  // PlayerData::allXiuwei
    kOffItemCount      = 0x5C,   // XxBaseItem::count
    kOffTvIdTx         = 0x28,   // ToponVideo::topon_video_id_TX
    kOffTvIdCsj        = 0x20,   // ToponVideo::topon_video_id_CSJ
    kOffFightCfg       = 0x130,  // FightTimeMgr::fightCfg
    kOffDropTable      = 0x78,   // fightCfg::dropList（List<掉落条目>）
    kOffEntryWeight    = 0x20,   // 掉落条目::weight（0x1AA07DC 按 32 位读取）
    kOffListItems      = 0x10,   // List<T>::_items（数组对象）
    kOffListSize       = 0x18,   // List<T>::_size
};

static const int32_t kResourceMultiplier = 1000;        // 无限资源：单次获取放大倍数
static const int32_t kResourceAddCeiling = 1000000000;  // 上限保护（防 int32 溢出；addData 自身夹到 1e12）
static const int32_t kItemCountRefill    = 99;          // 物品不减：count 归零时回填值
static const int64_t kJadeValue          = 9999;        // 灵玉不减：读值恒定大值（原值上限 30）
static const int32_t kMaxDropReplay      = 16;          // 掉落必掉：单局最多 replay 的候选数（防配置异常）
static const int32_t kDropGateSkip       = 0x7FFFFFFF;  // replay 期间让附加概率闸门走「跳过」分支

// ============================================================
// ③ 本地解析（绕开被劫持的 5 个 il2cpp 符号）
// ============================================================
static void* ResolveExtraSymbol(const char* name) {
    static void* handle = nullptr;
    static bool tried = false;
    if (!tried) {
        tried = true;
        handle = xdl_open(TPL_TARGET_LIB, XDL_DEFAULT);
        if (!handle) LOGW(TPL_LOG " xdl_open %s failed", TPL_TARGET_LIB);
    }
    if (!handle) return nullptr;
    return xdl_sym(handle, name, nullptr);
}

// Il2CppAssembly::image 在结构体偏移 0；直接裸读（assembly_get_image 已被劫持）
static inline void* ImageOfAssemblyLocal(void* asmObj) {
    return asmObj ? *reinterpret_cast<void**>(asmObj) : nullptr;
}

// 枚举全部 image 的类表按短名找类（image_get_class_count / image_get_class / class_get_name 未被劫持）
static void* FindClassLocal(const char* shortName) {
    auto& api = GetApi();
    if (!api.ready || !api.image_class_count || !api.image_class || !api.class_name || !shortName) return nullptr;
    size_t n = 0;
    void** asms = api.Assemblies(&n);
    if (!asms || n == 0) return nullptr;
    for (size_t i = 0; i < n; i++) {
        void* img = ImageOfAssemblyLocal(asms[i]);
        if (!img) continue;
        uint32_t cnt = api.image_class_count(img);
        if (cnt == 0 || cnt > 200000) continue;
        for (uint32_t k = 0; k < cnt; k++) {
            void* klass = api.image_class(img, k);
            if (!klass) continue;
            const char* nm = api.class_name(klass);
            if (nm && strcmp(nm, shortName) == 0) return klass;
        }
    }
    return nullptr;
}

// 方法枚举按名找 MethodInfo（class_get_method_from_name 已被劫持）
static void* FindMethodLocal(void* klass, const char* methodName) {
    auto& api = GetApi();
    if (!klass || !methodName || !api.method_get_name) return nullptr;
    static void* pGetMethods = nullptr;
    if (!pGetMethods) pGetMethods = ResolveExtraSymbol("il2cpp_class_get_methods");
    if (!pGetMethods) return nullptr;
    typedef void* (*GetMethods_t)(void*, void**);
    auto getMethods = reinterpret_cast<GetMethods_t>(pGetMethods);
    void* iter = nullptr;
    for (int guard = 0; guard < 8192; guard++) {
        void* mi = getMethods(klass, &iter);
        if (!mi) break;
        const char* mn = api.method_get_name(mi);
        if (mn && strcmp(mn, methodName) == 0) return mi;
    }
    return nullptr;
}

// ============================================================
// ④ 地址助手（/proc/self/maps；不做全 RX 误扫，不做无校验 base+RVA）
// ============================================================
// dl_iterate_phdr 回调：libil2cpp 的 dlpi_addr 就是 load bias（对应 vaddr 0）
static int FcxyxzjPhdrCb(struct dl_phdr_info* info, size_t /*size*/, void* data) {
    if (info->dlpi_name && strstr(info->dlpi_name, "libil2cpp.so") && info->dlpi_addr) {
        *reinterpret_cast<uintptr_t*>(data) = static_cast<uintptr_t>(info->dlpi_addr);
        return 1;  // 找到即停
    }
    return 0;
}

struct FcxyxzjMapEntry {
    uintptr_t start;
    uintptr_t end;
    unsigned long off;
    unsigned long inode;
    char perm[8];
};

// maps 兜底：同名只读预加载副本没有对应的可执行段；用同 inode 的
// off==0 映射与 x 映射配对，才能拿到真正的 load bias。
static uintptr_t FcxyxzjBaseFromMaps() {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return 0;

    FcxyxzjMapEntry entries[256] = {};
    int count = 0;
    char line[768];
    while (count < 256 && fgets(line, sizeof(line), fp)) {
        if (!strstr(line, "libil2cpp.so")) continue;
        unsigned long start = 0, end = 0, off = 0, devMaj = 0, devMin = 0, inode = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx %lx:%lx %lu", &start, &end, perm, &off,
                   &devMaj, &devMin, &inode) != 7) {
            continue;
        }
        FcxyxzjMapEntry& e = entries[count++];
        e.start = static_cast<uintptr_t>(start);
        e.end = static_cast<uintptr_t>(end);
        e.off = off;
        e.inode = inode;
        memcpy(e.perm, perm, sizeof(e.perm));
    }
    fclose(fp);

    uintptr_t best = 0;
    for (int i = 0; i < count; i++) {
        const FcxyxzjMapEntry& exec = entries[i];
        if (!strchr(exec.perm, 'x')) continue;
        for (int j = 0; j < count; j++) {
            const FcxyxzjMapEntry& candidate = entries[j];
            if (candidate.off != 0 || candidate.inode != exec.inode) continue;
            if (candidate.start <= exec.start && exec.start - candidate.start < 0x10000000UL) {
                if (!best || candidate.start > best) best = candidate.start;
            }
        }
    }
    return best;
}

static uintptr_t LibBaseAddr() {
    static uintptr_t base = 0;
    if (base) return base;
    // 主路径：linker 维护的 load bias。不能用 maps 里的第一个 off==0 映射：
    // 本包实测存在同名的不可执行只读副本（OPPO 预加载），按 maps 顺序取会拿到它。
    uintptr_t found = 0;
    dl_iterate_phdr(FcxyxzjPhdrCb, &found);
    if (found) {
        base = found;
        LOGI(TPL_LOG " libil2cpp base=0x%lX (dl_iterate_phdr)", (unsigned long)base);
        return base;
    }
    base = FcxyxzjBaseFromMaps();
    if (base) LOGI(TPL_LOG " libil2cpp base=0x%lX (maps fallback)", (unsigned long)base);
    return base;
}

static bool VaInLib(uintptr_t va) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return false;
    char line[768];
    bool ok = false;
    while (fgets(line, sizeof(line), fp)) {
        if (!strstr(line, "libil2cpp.so")) continue;
        unsigned long start = 0, end = 0, off = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (!strchr(perm, 'x')) continue;  // 排除同名只读预加载副本
        if (va >= start && va < end) { ok = true; break; }
    }
    fclose(fp);
    return ok;
}

static void* VaFromRva(const char* tag, uintptr_t rva, uint32_t expectInsn) {
    uintptr_t base = LibBaseAddr();
    if (!base) { LOGW(TPL_LOG " %s libil2cpp base not found", tag); return nullptr; }
    uintptr_t va = base + rva;
    if (!VaInLib(va)) { LOGW(TPL_LOG " %s rva 0x%lX outside libil2cpp maps", tag, (unsigned long)rva); return nullptr; }
    if (expectInsn) {
        uint32_t got = 0;
        memcpy(&got, reinterpret_cast<void*>(va), sizeof(got));
        if (got != expectInsn) {
            LOGW(TPL_LOG " %s rva drift got=0x%08X want=0x%08X — try by-name", tag, got, expectInsn);
            return nullptr;
        }
    }
    return reinterpret_cast<void*>(va);
}

// ============================================================
// ⑤ 挂点安装（RVA 优先 + 序言校验；失败转按名；幂等）
// ============================================================
static bool HookAtRva(const char* tag, uintptr_t rva, uint32_t expectInsn, void* hook, void** orig) {
    void* va = VaFromRva(tag, rva, expectInsn);
    if (!va) return false;
    if (DobbyHook(va, hook, orig) != 0) { LOGW(TPL_LOG " %s dobby fail @rva 0x%lX", tag, (unsigned long)rva); return false; }
    LOGI(TPL_LOG " %s hooked @rva 0x%lX", tag, (unsigned long)rva);
    return true;
}

static bool HookByNameLocal(const char* tag, const char* cls, const char* method, void* hook, void** orig) {
    auto& api = GetApi();
    if (!api.ready) { LOGW(TPL_LOG " %s api not ready — %s degrade", tag, method); return false; }
    void* klass = FindClassLocal(cls);
    if (!klass) { LOGW(TPL_LOG " %s class miss %s — degrade", tag, cls); return false; }
    void* mi = FindMethodLocal(klass, method);
    if (!mi) { LOGW(TPL_LOG " %s method miss %s.%s — degrade", tag, cls, method); return false; }
    void* fn = *reinterpret_cast<void**>(mi);
    if (!fn) { LOGW(TPL_LOG " %s bad method ptr %s.%s", tag, cls, method); return false; }
    if (DobbyHook(fn, hook, orig) != 0) { LOGW(TPL_LOG " %s dobby fail @name %s", tag, method); return false; }
    LOGI(TPL_LOG " %s hooked %s.%s @name %p", tag, cls, method, fn);
    return true;
}

struct HookSlot {
    const char* tag;
    uintptr_t   rva;
    uint32_t    expectInsn;
    const char* cls;
    const char* method;
    void*       hookFn;
    void**      origPtr;
};

static bool HookSlotInstall(const HookSlot& s) {
    if (s.origPtr && *s.origPtr) return true;  // 幂等：防双 Dobby 叠钩
    if (s.rva && HookAtRva(s.tag, s.rva, s.expectInsn, s.hookFn, s.origPtr)) return true;
    return HookByNameLocal(s.tag, s.cls, s.method, s.hookFn, s.origPtr);
}

// ============================================================
// ⑥ Hook 实现
// ============================================================
typedef void    (*AddData_t)(void* self, void* slot, void* checkSlot, int32_t delta);
typedef bool    (*IsReady_t)(void* self, void* method);
typedef void    (*ShowAd_t)(void* self, void* method);
typedef int32_t (*GetInt_t)(void* self, void* method);
typedef void    (*RemoveAt_t)(void* list, int32_t index, void* method);
typedef int32_t (*RandomRange_t)(int32_t lo, int32_t hi, void* method);
typedef void*   (*GetSuccessWuPing_t)(void* self);  // 返回托管字符串（奖励提示文本）
typedef int64_t (*GetLingYu_t)(void* self);         // PlayerData::get_灵玉（加密字段解密后的显示值）

static AddData_t g_origAddData = nullptr;
static AddData_t g_origSubData = nullptr;
static IsReady_t g_origIsReady = nullptr;
static ShowAd_t  g_origShowAd = nullptr;
static GetInt_t  g_origGetXianjing = nullptr;
static RemoveAt_t g_origRemoveAt = nullptr;
static RandomRange_t g_origRandomRange = nullptr;
static GetSuccessWuPing_t g_origGetSuccessWuPing = nullptr;
static GetLingYu_t g_origGetLingYu = nullptr;
static thread_local bool t_allDropReplay = false;  // 主池候选重放中（同线程同步调用）

static inline int32_t ScaleUp(int32_t v, int32_t mul) {
    if (v <= 0 || mul <= 1) return v;
    int64_t n = static_cast<int64_t>(v) * mul;
    if (n > kResourceAddCeiling) n = kResourceAddCeiling;
    return static_cast<int32_t>(n);
}

// 无限资源：获取放大；掉落翻倍：结算来源额外 ×2（同一挂点内按返回地址区分）
static void Hooked_AddData(void* self, void* slot, void* checkSlot, int32_t delta) {
    if (self && delta > 0 && slot) {
        const uintptr_t off = reinterpret_cast<uintptr_t>(slot) - reinterpret_cast<uintptr_t>(self);
        if (off == kOffPdLingshiSlot || off == kOffPdXiuweiSlot) {
            const uintptr_t base = LibBaseAddr();
            const uintptr_t ret = reinterpret_cast<uintptr_t>(__builtin_return_address(0));
            const bool dropPoint = base && (ret == base + kRvaDropLinShiRet ||
                                             ret == base + kRvaDropXiuweiRet);
            if (dropPoint && t_allDropReplay) {
                // 主池 replay 只补发候选物品；尾部 addLinShi/addXiuwei 不重复结算
                delta = 0;
            } else if (dropPoint && g_drop.load()) {
                delta = ScaleUp(delta, 2);
            }
            if (g_cur.load()) delta = ScaleUp(delta, kResourceMultiplier);
        }
    }
    if (g_origAddData) g_origAddData(self, slot, checkSlot, delta);
}

// 无限资源：修为/灵石 消耗不减（其余资源木材/食物/石头/星币照常扣）
static void Hooked_SubData(void* self, void* slot, void* checkSlot, int32_t delta) {
    if (self && slot && g_cur.load()) {
        const uintptr_t off = reinterpret_cast<uintptr_t>(slot) - reinterpret_cast<uintptr_t>(self);
        if (off == kOffPdLingshiSlot || off == kOffPdXiuweiSlot) return;
    }
    if (g_origSubData) g_origSubData(self, slot, checkSlot, delta);
}

// 免广告：广告位一律“就绪”，点广告不播片直接走游戏自己的发奖回调
static void GrantAdReward(void* tv) {
    typedef void (*OnReward_t)(void* self, void* a1, void* a2);
    static OnReward_t onReward = nullptr;
    if (!onReward) {
        void* va = VaFromRva("onReward", kRvaOnReward, kInsOnReward);
        if (!va) { LOGW(TPL_LOG " onReward anchor missing — 免广告仅保留 isReady/祈福分支"); return; }
        onReward = reinterpret_cast<OnReward_t>(va);
    }
    void* id = *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(tv) + kOffTvIdTx);
    if (!id) id = *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(tv) + kOffTvIdCsj);
    if (!id) { LOGW(TPL_LOG " ad placement id null — skip reward"); return; }
    // onReward 只读 arg2+0x10（广告位 id 字符串）；假对象放在 .bss（非 GC 堆，不会被保守扫描当对象）
    static struct { void* klass; void* monitor; void* id; void* pad; } fake;
    fake.klass = GetApi().object_class ? GetApi().object_class(id) : nullptr;
    fake.monitor = nullptr;
    fake.id = id;
    fake.pad = nullptr;
    onReward(tv, nullptr, &fake);
}

static bool Hooked_IsReady(void* self, void* method) {
    if (g_ad.load()) return true;
    return g_origIsReady ? g_origIsReady(self, method) : false;
}

static void Hooked_ShowAd(void* self, void* method) {
    if (g_ad.load() && self) {
        LOGI(TPL_LOG " showAd intercepted — grant reward without video");
        GrantAdReward(self);
        return;  // 不调 orig：否则会拉起真实广告 SDK 播放
    }
    if (g_origShowAd) g_origShowAd(self, method);
}

// 祈福：getXianjing() >= 1 时游戏自身跳过广告直接发奖（免广告原生分支）
static int32_t Hooked_GetXianjing(void* self, void* method) {
    if (g_ad.load()) return 1;
    return g_origGetXianjing ? g_origGetXianjing(self, method) : 0;
}

// 灵玉不减：灵玉加密存在 PlayerData+0x2A0（Base64 密文），显示值 = (解密值-100)/3，上限 30。
// 全库只有 getter 0x1A58174 一处读值出口（兑换按钮 cmp x0,#9 阈值、面板刷新、充值塔均走它），
// 开启时直接返回大值：阈值永远通过、UI 显示大数字、消耗不再受影响（不碰密文与解密链）。
static int64_t Hooked_GetLingYu(void* self) {
    if (g_jade.load()) return kJadeValue;
    return g_origGetLingYu ? g_origGetLingYu(self) : 0;
}

// 物品不减：只在 4 个“使用物品”入口拦 List<T>::RemoveAt，其余 150+ 调用点原样放行
static void* ListElementAt(void* list, int32_t index) {
    if (!list || index < 0) return nullptr;
    void* items = *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(list) + 0x10);
    if (!items) return nullptr;
    const int32_t size = *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(list) + 0x18);
    if (index >= size) return nullptr;
    return *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(items) + 0x20 +
                                     static_cast<size_t>(index) * sizeof(void*));
}

static void Hooked_RemoveAt(void* list, int32_t index, void* method) {
    if (g_item.load() && list) {
        const uintptr_t base = LibBaseAddr();
        const uintptr_t ret = reinterpret_cast<uintptr_t>(__builtin_return_address(0));
        if (base) {
            const uintptr_t off = ret - base;
            if (off == kRvaUseYaoPingRet || off == kRvaUseTaskWpRet || off == kRvaUseFuwenshuRet) {
                LOGI(TPL_LOG " item use blocked @0x%lX idx=%d", (unsigned long)off, index);
                return;  // 不消耗：直接跳过移除
            }
            if (off == kRvaDrinkProRet) {
                // DrinkPro 先 count-- 再在归零时移除 → 回填 count 使物品继续可用
                void* item = ListElementAt(list, index);
                if (item) {
                    *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(item) + kOffItemCount) = kItemCountRefill;
                    LOGI(TPL_LOG " quick-slot item refilled to %d idx=%d", kItemCountRefill, index);
                    return;
                }
            }
        }
    }
    if (g_origRemoveAt) g_origRemoveAt(list, index, method);
}

// 掉落必掉：Random.Range 全库 258 个调用点，只拦 GetSuccessWuPing 内 6 个概率判定点。
// 返回下界(均为 0) 使 "随机值 < 配置概率" 恒成立 → 配置里非零概率的掉落全部必掉；
// 配置为 0 的项仍然不掉（不伪造内容，只取消运气成分）。
static int32_t Hooked_RandomRange(int32_t lo, int32_t hi, void* method) {
    if (g_dropRate.load()) {
        const uintptr_t base = LibBaseAddr();
        if (base) {
            const uintptr_t off = reinterpret_cast<uintptr_t>(__builtin_return_address(0)) - base;
            switch (off) {
                case kRvaDropRateRet1: {  // 主池：返回下界 → 命中权重掩码钦定的候选
                    static std::atomic<int> s_hits{0};
                    const int n = s_hits.fetch_add(1);
                    if (n < 6) LOGI(TPL_LOG " drop-rate forced @0x%lX range=[%d,%d)", (unsigned long)off, lo, hi);
                    return lo;
                }
                case kRvaDropRateRet2:
                case kRvaDropRateRet3:
                case kRvaDropRateRet4:
                case kRvaDropRateRet5:
                case kRvaDropRateRet6: {
                    // replay 时这些附加奖励已在首次调用发过 → 走「跳过」分支，不重复发
                    if (t_allDropReplay) return kDropGateSkip;
                    static std::atomic<int> s_hits{0};
                    const int n = s_hits.fetch_add(1);
                    if (n < 6) LOGI(TPL_LOG " drop-rate forced @0x%lX range=[%d,%d)", (unsigned long)off, lo, hi);
                    return lo;
                }
                default: break;
            }
        }
    }
    return g_origRandomRange ? g_origRandomRange(lo, hi, method) : lo;
}

// 掉落必掉（主池全候选）：主掉落池是「按权重只选一个」——
// 0x1AA0790 循环遇到首个 weight > rand 的条目就 break，后面的类型分发只发这一件。
// 所以「提示里两个候选都要掉」必须让原函数按不同候选重复执行：
//   ① 先正常调用一次（保留既有 6 个概率闸门行为）；
//   ② 表内其余正权重候选逐个 replay：临时把别人 weight 掩成 0（只写低 32 位，
//      与 0x1AA07DC 的 ldr w8 读法一致），命中目标候选后立即还原；
//   ③ replay 期间 Random.Range 钩子让 5 个附加闸门走「跳过」分支 → 附加奖励不重复发；
//   ④ replay 期间尾部 addLinShi/addXiuwei 被 addData 钩子置 0 → 灵石/修为不重复结算。
static void* Hooked_GetSuccessWuPing(void* self) {
    if (!g_origGetSuccessWuPing) return nullptr;
    // replay 自身重入 / 关闭 / 无 self：直通原函数
    if (!g_dropRate.load() || !self || t_allDropReplay) return g_origGetSuccessWuPing(self);

    void*   entries[kMaxDropReplay] = {};
    int32_t weights[kMaxDropReplay] = {};
    int32_t count = 0;

    // 快照主掉落池（cfg@self+0x130 → dropList@cfg+0x78）
    void* cfg = *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(self) + kOffFightCfg);
    void* list = cfg ? *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(cfg) + kOffDropTable) : nullptr;
    if (list) {
        void* items = *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(list) + kOffListItems);
        int32_t size = *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(list) + kOffListSize);
        if (items && size > 0) {
            if (size > kMaxDropReplay) size = kMaxDropReplay;
            for (int32_t i = 0; i < size; i++) {
                void* e = *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(items) + 0x20 +
                                                  static_cast<size_t>(i) * sizeof(void*));
                if (!e) continue;
                entries[i] = e;
                weights[i] = *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(e) + kOffEntryWeight);
                count = i + 1;
            }
        }
    }

    void* first = g_origGetSuccessWuPing(self);  // 首次：命中首个正权重候选 + 附加闸门
    if (count < 2) return first;

    int32_t firstIdx = -1;
    for (int32_t i = 0; i < count; i++) {
        if (entries[i] && weights[i] > 0) { firstIdx = i; break; }
    }
    if (firstIdx < 0) return first;

    for (int32_t i = 0; i < count; i++) {
        if (i == firstIdx || !entries[i] || weights[i] <= 0) continue;
        for (int32_t j = 0; j < count; j++) {
            if (j != i && entries[j]) {
                *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(entries[j]) + kOffEntryWeight) = 0;
            }
        }
        t_allDropReplay = true;
        g_origGetSuccessWuPing(self);
        t_allDropReplay = false;
        for (int32_t j = 0; j < count; j++) {  // 无论跳转路径都还原权重
            if (entries[j]) {
                *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(entries[j]) + kOffEntryWeight) = weights[j];
            }
        }
        static std::atomic<int> s_replay{0};
        const int n = s_replay.fetch_add(1);
        if (n < 8) LOGI(TPL_LOG " drop-all replay idx=%d/%d (first=%d)", i, count, firstIdx);
    }
    return first;
}

// ============================================================
// ⑦ 分组安装（按功能位懒装）
// ============================================================
static const HookSlot kSlots[] = {
    // 无限资源 / 掉落翻倍
    {"addData",     kRvaAddData,     kInsAddData,    "PlayerData", "addData", (void*)Hooked_AddData, (void**)&g_origAddData},
    {"subData",     kRvaSubData,     kInsSubData,    "PlayerData", "subData", (void*)Hooked_SubData, (void**)&g_origSubData},
    // 免广告
    {"showAd",      kRvaShowAd,      kInsShowAdLike, "ToponVideo", "showAd",  (void*)Hooked_ShowAd,  (void**)&g_origShowAd},
    {"isReady",     kRvaIsReady,     kInsShowAdLike, "ToponVideo", "isReady", (void*)Hooked_IsReady, (void**)&g_origIsReady},
    {"getXianjing", kRvaGetXianjing, kInsShowAdLike, "PlayerData", "getXianjing", (void*)Hooked_GetXianjing, (void**)&g_origGetXianjing},
    // 物品不减
    {"RemoveAt",    kRvaRemoveAt,    kInsRemoveAt,   nullptr, nullptr, (void*)Hooked_RemoveAt, (void**)&g_origRemoveAt},
    // 掉落必掉（Random.Range 有 float/int 双重载，按名会拿错签名 → 只走 RVA + 序言校验）
    {"RandomRange", kRvaRandomRange, kInsShowAdLike, nullptr, nullptr, (void*)Hooked_RandomRange, (void**)&g_origRandomRange},
    // 掉落必掉（主池全候选：replay 原函数逐个发放；同名单方法无重载，可按名兼底）
    {"GetSuccessWuPing", kRvaGetSuccessWuPing, kInsGetSuccessWuPing, "FightTimeMgr", "GetSuccessWuPing", (void*)Hooked_GetSuccessWuPing, (void**)&g_origGetSuccessWuPing},
    // 灵玉不减（加密字段，方法名无 LingYu 字样 → 只走 RVA + 序言校验）
    {"getLingYu", kRvaGetLingYu, kInsGetLingYu, nullptr, nullptr, (void*)Hooked_GetLingYu, (void**)&g_origGetLingYu},
};

static void InstallSlotByTag(const char* tag) {
    for (const auto& s : kSlots) {
        if (strcmp(s.tag, tag) != 0) continue;
        if (HookSlotInstall(s)) g_anyHooked.store(true);
        return;
    }
}

static void SyncHooks() {
    if (!g_libReady.load()) return;
    if (!GetApi().ready) GetApi().Load(TPL_TARGET_LIB);  // 只为按名兜底；失败不影响 RVA 路径
    if (g_ad.load()) {
        InstallSlotByTag("isReady");
        InstallSlotByTag("showAd");
        InstallSlotByTag("getXianjing");
    }
    if (g_cur.load()) {
        InstallSlotByTag("addData");
        InstallSlotByTag("subData");
    }
    if (g_drop.load()) InstallSlotByTag("addData");
    if (g_jade.load()) InstallSlotByTag("getLingYu");
    if (g_item.load()) InstallSlotByTag("RemoveAt");
    if (g_dropRate.load()) {
        // 顺序固定：addData（replay 期间抑住尾部灵石/修为）→ RandomRange（概率归零）
        // → GetSuccessWuPing（主池全候选 replay）。首次调用必须已处于概率归零状态。
        InstallSlotByTag("addData");
        InstallSlotByTag("RandomRange");
        InstallSlotByTag("GetSuccessWuPing");
    }
}

// ============================================================
// ⑧ 对外接口（ModEntry.h 注册表契约）
// ============================================================
bool Fcxyxzj_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;
    g_libReady.store(true);
    LOGI(TPL_LOG " libil2cpp ready (local resolver, lazy hooks)");
    SyncHooks();
    return true;
}

bool Fcxyxzj_IsReady() { return g_libReady.load(); }

void Fcxyxzj_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case FCXYXZJ_FEAT_AD:   g_ad.store(enabled);   break;
        case FCXYXZJ_FEAT_CUR:  g_cur.store(enabled);  break;
        case FCXYXZJ_FEAT_ITEM: g_item.store(enabled); break;
        case FCXYXZJ_FEAT_DROP: g_drop.store(enabled); break;
        case FCXYXZJ_FEAT_RATE: g_dropRate.store(enabled); break;
        case FCXYXZJ_FEAT_JADE: g_jade.store(enabled); break;
        default: break;
    }
    SyncHooks();
}

void Fcxyxzj_OnValueChanged(int /*featNum*/, int /*value*/) {}

void Fcxyxzj_Tick(const bool* features, const int* /*values*/) {
    if (features) {
        g_ad.store(features[FCXYXZJ_FEAT_AD]);
        g_cur.store(features[FCXYXZJ_FEAT_CUR]);
        g_item.store(features[FCXYXZJ_FEAT_ITEM]);
        g_drop.store(features[FCXYXZJ_FEAT_DROP]);
        g_dropRate.store(features[FCXYXZJ_FEAT_RATE]);
        g_jade.store(features[FCXYXZJ_FEAT_JADE]);
    }
    if (!g_libReady.load() && isLibraryLoaded(TPL_TARGET_LIB)) g_libReady.store(true);
    SyncHooks();
}

void Fcxyxzj_ForceClear() {
    g_ad.store(false);
    g_cur.store(false);
    g_item.store(false);
    g_drop.store(false);
    g_dropRate.store(false);
    g_jade.store(false);
}
