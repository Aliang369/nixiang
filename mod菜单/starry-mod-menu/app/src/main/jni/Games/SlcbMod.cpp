/* ================================================================
 *  星光菜单游戏模块 — 标准骨架 v3（注册表版，复制后按 TODO 填空）
 *
 *  ⚠️ 防闪退设施已内置为代码，删任何一节都可能复现历史闪退：
 *    默认 0 hook（懒装）/ 双路径挂点 / RVA 漂移拒绝 / 幂等安装
 *    orig 必调 / reenter 防递归 / SafeWrite 校验写 / 类名缓存 / Tick 轻量
 *  生成：bash mod菜单/starry-mod-menu/scripts/new_game_mod.sh {Game}
 *  （产出四件套：Mod.h/.cpp + Entry.cpp + Android_{tag}.mk，走 MainModern.cpp）
 *  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
 *
 *  功能实现分层（定稿 2026-09-09，详见 GAME_MOD_GUIDE.md）：
 *    数据型（无敌=HP回写/属性增益/资源锁定）→ 偏移定位 + SafeWrite 直写，
 *      Tick 轮询维持（零 hook 痕迹，防封首选；开关全关=零改写）
 *    行为型（改判定/伤害结算/拦截上报）   → Dobby hook：RVA 优先（序言
 *      校验），RVA 失效自动转按名（本骨架 HookSlot 双路径）
 *    字节 patch 改函数 → 默认禁用（E15-E18 死路清单）
 * ================================================================ */

#include "SlcbMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <ctime>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/StarPtr.h"   // 设备无关指针判据（清 TBI/MTE 顶字节）
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

#define TPL_LOG "[Slcb]"  // 生成器会替换为本游戏标签

// 目标库：注册表模式不依赖 GameConfig.h（那是 legacy 配置中心）。
// Cocos 系生成后把下面 4 处 sec_lib_il2cpp() 换成目标库名即可。
#define TPL_TARGET_LIB sec_lib_il2cpp()

// ============================================================
// ① 开关与 orig（TODO：按游戏增减；功能位号见 .h 的 enum）
// ============================================================
static std::atomic<bool> g_libReady{false};  // il2cpp 已映射（≠ 已 hook）
static std::atomic<bool> g_anyHooked{false};
static std::atomic<bool> g_reenter{false};

// 功能组开关（环境阶段全部 0 hook；功能阶段按需唤醒）
static std::atomic<bool> g_inv{false};    // SLCB_FEAT_INV
static std::atomic<int>  g_dmg{1};        // SLCB_FEAT_DMG（1 = 原版倍率）
static std::atomic<bool> g_skill{false};  // SLCB_FEAT_SKILL
static std::atomic<bool> g_gold{false};   // SLCB_FEAT_GOLD
static std::atomic<bool> g_soul{false};   // SLCB_FEAT_SOUL

// ============================================================
// ② 字段偏移与 RVA（TODO：dump.cs / IDA / Trace 实测，禁止猜；换包重 dump）
// ============================================================
enum : size_t {
    kOffHurtValue = 0x30,  // 示例：伤害字段偏移（功能阶段回填）
};

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

// RVA 路径：序言不匹配 = 版本漂移/空页，拒绝挂（防挂错地址闪退），转按名
static bool HookAtRva(const char* tag, uintptr_t rva, uint32_t expectInsn, void* hook, void** orig) {
    void* va = FileOffToVa(TPL_TARGET_LIB, rva);
    if (!va) {
        LOGW("%s rva 0x%lX not in RX — try by-name", tag, (unsigned long)rva);
        return false;
    }
    if (expectInsn) {
        uint32_t got = 0;
        memcpy(&got, va, sizeof(got));
        if (got == 0 || got == 0xFFFFFFFFu || got != expectInsn) {
            LOGW("%s rva drift got=0x%08X want=0x%08X — try by-name", tag, got, expectInsn);
            return false;
        }
    }
    if (DobbyHook(va, hook, orig) != 0) {
        LOGW("%s dobby fail @rva", tag);
        return false;
    }
    LOGI("%s hooked @rva 0x%lX", tag, (unsigned long)rva);
    return true;
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

struct HookSlot {
    const char* tag;
    uintptr_t   rva;         // 0 = 只走按名
    uint32_t    expectInsn;  // 0 = 不校验序言；建议实测后回填防漂移
    const char* ns;
    const char* cls;
    const char* method;
    int         argc;
    void*       hookFn;
    void**      origPtr;
};

// 幂等 + 双路径：*orig 非空 = 已挂过（防双 Dobby 叠钩 SIGSEGV）；RVA 失败转按名
static bool HookSlotInstall(const HookSlot& s) {
    if (s.origPtr && *s.origPtr) return true;
    if (s.rva && HookAtRva(s.tag, s.rva, s.expectInsn, s.hookFn, s.origPtr)) return true;
    return HookByName(s.tag, s.ns, s.cls, s.method, s.argc, s.hookFn, s.origPtr);
}

// ============================================================
// ⑤ hook 函数与挂点表
//   易盾（NIS）把 .so 的节表/符号名/RVA 全毁 ⇒ rva 一律填 0，全走 HookByName。
//   每个 hook 都是「排除法」：勾选才动参数/返回值，未勾选原样透传 orig。
//   按名解析失败的槽只 LOGW 降级，不影响其它槽（HookByName 已保证）。
//
//   类名/签名来源：解密后的 global-metadata.dat（Il2CppDumper 静态路线因
//   易盾毁符号而失败，改用自建 01_tools/MetadataSearch 按 metadata 读名+签名）。
// ============================================================

// 热路径对象门闩：对齐 + 堆校验 + 高位区间（挡掉打包 int、tombstone 脏值）
// ★设备无关修正：真机（arm64 + Android 12+）堆指针带 TBI 标签（形如 0xb4000076…），
//   必须先清顶字节再判区间，否则合法对象被这行提前判否 ⇒ 真机功能静默全灭。
static bool SafeObjPtr(void* p) {
    if (!p) return false;
    if ((reinterpret_cast<uintptr_t>(p) & 7u) != 0) return false;
    if (!starptr::InUserVa(p)) return false;
    return Il2Cpp::IsValidHeapPtr(p);
}

// self 是不是玩家：类名含 "Hero"（Tera_OB.Hero / HeroBase / Teran.Hero_Phatom）
static bool SelfIsHero(void* self) {
    if (!SafeObjPtr(self)) return false;
    const char* n = GetApi().ClassName(self);
    if (!n) return false;
    return strstr(n, "Hero") != nullptr;
}

// ---- 无敌（g_inv）--------------------------------------------------
// ① Tera_OB.HeroBase.SetIsCanBeAttacked(bool)：游戏自带的「可被攻击」旗标，
//    置 false 与受击无敌帧走同一条路径（下游伤害/硬直/击退全被跳过）。
typedef void (*TplSetIsCanBeAttacked_t)(void* self, bool isCan, void* method);
static TplSetIsCanBeAttacked_t g_origSetIsCanBeAttacked = nullptr;
static void Hooked_SetIsCanBeAttacked(void* self, bool isCan, void* method) {
    if (g_inv.load()) isCan = false;
    if (g_origSetIsCanBeAttacked) g_origSetIsCanBeAttacked(self, isCan, method);
}

// ② Tera_OB.HeroBase.SetCanBeAttacked_Force()：游戏用它强制恢复可攻击；
//    无敌开时整个吞掉（不调 orig），否则会把 ① 刚置的旗标翻回来。
typedef void (*TplSetCanBeAttackedForce_t)(void* self, void* method);
static TplSetCanBeAttackedForce_t g_origSetCanBeAttackedForce = nullptr;
static void Hooked_SetCanBeAttackedForce(void* self, void* method) {
    if (g_inv.load()) return;
    if (g_origSetCanBeAttackedForce) g_origSetCanBeAttackedForce(self, method);
}

// ③ Tera_OB.HeroBase.BeAttacked(AttackInfo,float,bool) -> int32：玩家受击总入口，
//    返回值＝本次实际掉血。无敌开时直接返回 0（不调 orig），保证一次都不掉血。
typedef int32_t (*TplHeroBeAttacked_t)(void* self, void* atkInfo, float backDis,
                                       bool isCalculDamage, void* method);
static TplHeroBeAttacked_t g_origHeroBeAttacked = nullptr;
static int32_t Hooked_HeroBeAttacked(void* self, void* atkInfo, float backDis,
                                     bool isCalculDamage, void* method) {
    if (g_inv.load()) return 0;
    if (!g_origHeroBeAttacked) return 0;
    return g_origHeroBeAttacked(self, atkInfo, backDis, isCalculDamage, method);
}

// ④ Teran.Creature.calPureDamange(int,int,float,bool,float) -> int32
//    self = 挨打方（"my" 参数是 self 的防御/减伤），返回结算后的纯伤害。
//    ⇒ 一个钩子同时喂两个功能：self 是玩家 → 无敌归零；self 是怪物 → 乘伤害倍率。
typedef int32_t (*TplCalPureDamange_t)(void* self, int32_t attackerDamage, int32_t myDefence,
                                       float myDamageFactor, bool isCritical,
                                       float criticalFactor, void* method);
static TplCalPureDamange_t g_origCalPureDamange = nullptr;
static int32_t Hooked_CalPureDamange(void* self, int32_t attackerDamage, int32_t myDefence,
                                     float myDamageFactor, bool isCritical,
                                     float criticalFactor, void* method) {
    if (!g_origCalPureDamange) return attackerDamage;  // 兜底：绝不让伤害凭空消失
    const bool hero = SelfIsHero(self);
    if (hero && g_inv.load()) return 0;
    int32_t d = g_origCalPureDamange(self, attackerDamage, myDefence, myDamageFactor,
                                     isCritical, criticalFactor, method);
    const int mult = g_dmg.load();
    if (!hero && mult > 1 && d > 0) {
        int64_t scaled = static_cast<int64_t>(d) * mult;
        d = scaled > 0x7FFFFFFF ? 0x7FFFFFFF : static_cast<int32_t>(scaled);
    }
    return d;
}

// ---- 无限技能（g_skill）--------------------------------------------
// 关键：Teran.ActionInfo 是英雄与怪物共用的「动作/技能描述对象」，直接对整个类
// 改 CD 会连怪物一起加速 ⇒ 先捕获「玩家那一个 ActionInfo 实例」，只对它放行。
static void* volatile g_heroSkill = nullptr;

// 诊断限流：每个调用点最多打 SLCB_DIAG_LIMIT 行，避免热路径刷爆 logcat。
#define SLCB_DIAG_LIMIT 24
#define SLCB_DIAG(...)                       \
    do {                                     \
        static int _slcb_n = 0;              \
        if (_slcb_n < SLCB_DIAG_LIMIT) {      \
            ++_slcb_n;                       \
            LOGI(TPL_LOG " " __VA_ARGS__);   \
        }                                    \
    } while (0)

// ⑤ Tera_OB.HeroBase.get_Skill() -> Teran.ActionInfo：捕获玩家技能实例（主路径）
static void* g_heroSkillMI = nullptr;
typedef void* (*TplHeroGetSkill_t)(void* self, void* method);
static TplHeroGetSkill_t g_origHeroGetSkill = nullptr;
static void* Hooked_HeroGetSkill(void* self, void* method) {
    if (method) g_heroSkillMI = method;
    void* r = g_origHeroGetSkill ? g_origHeroGetSkill(self, method) : nullptr;
    if (r && r != g_heroSkill) {
        g_heroSkill = r;
        LOGI(TPL_LOG " skl.capture @get_Skill self=%p skill=%p", self, r);
    }
    return r;
}

// 兜底捕获：直接从 hero 实例上取 get_Skill()（用于钩子装得比英雄生成晚的情况）
static void EnsureHeroSkillFrom(void* hero) {
    if (!hero || g_heroSkill) return;
    if (!EnsureApi()) return;
    auto& api = GetApi();
    void* k = FindClassNs("Tera_OB", "HeroBase");
    if (!k) return;
    void* mi = g_heroSkillMI;
    if (!mi) mi = api.GetMethod(k, "get_Skill", 0);
    if (!mi) return;
    g_heroSkillMI = mi;
    void* fn = *reinterpret_cast<void**>(mi);
    if (!OkPtr(fn)) return;
    void* r = reinterpret_cast<void* (*)(void*, void*)>(fn)(hero, mi);
    if (r) {
        g_heroSkill = r;
        LOGI(TPL_LOG " skl.capture @backfill self=%p skill=%p", hero, r);
    }
}

// ⑥ Tera_OB.HeroBase.DoAction_Skill_GetInfo() -> Teran.ActionInfo：备用捕获点
typedef void* (*TplDoActionSkillGetInfo_t)(void* self, void* method);
static TplDoActionSkillGetInfo_t g_origDoActionSkillGetInfo = nullptr;
static void* Hooked_DoActionSkillGetInfo(void* self, void* method) {
    void* r = g_origDoActionSkillGetInfo ? g_origDoActionSkillGetInfo(self, method) : nullptr;
    if (r && r != g_heroSkill) {
        g_heroSkill = r;
        LOGI(TPL_LOG " skl.capture @DoAction_Skill_GetInfo self=%p skill=%p", self, r);
    }
    return r;
}

// ⑫ Tera_OB.HeroBase.UseSkill() -> bool：用技能时一定被调 ⇒ 最可靠的捕获时机
typedef bool (*TplHeroUseSkill_t)(void* self, void* method);
static TplHeroUseSkill_t g_origHeroUseSkill = nullptr;
static bool Hooked_HeroUseSkill(void* self, void* method) {
    EnsureHeroSkillFrom(self);
    bool r = g_origHeroUseSkill ? g_origHeroUseSkill(self, method) : false;
    SLCB_DIAG("skl.use self=%p hero=%p eq=%d ret=%d skill_on=%d", self, (void*)g_heroSkill,
              (g_heroSkill && self == g_heroSkill) ? 1 : 0, r ? 1 : 0, g_skill.load() ? 1 : 0);
    return r;
}

static inline bool IsHeroSkill(void* self) {
    void* hs = g_heroSkill;
    return g_skill.load() && hs && self == hs;
}

// ⑦ Teran.ActionInfo.get_IsCanUsed() -> bool：技能可用门禁
typedef bool (*TplActionIsCanUsed_t)(void* self, void* method);
static TplActionIsCanUsed_t g_origActionIsCanUsed = nullptr;
// ⚠️ 铁律：**绝不要在这里强制 return true**。
// 实测（01:31 日志）：游戏的 CD 驱动是 `if (!IsCanUsed) UpdateCoolingTime(dt)`，
// 一旦 getter 恒真，游戏就再也不调 UpdateCoolingTime ⇒ CD 冻死、图标永远灰、
// 且关掉开关后才恢复（正是用户 m01110/m01125 报的现象）。
// 这里只做原样透传 + 诊断；真正的快进放在 UpdateCoolingTime 里。
static bool Hooked_ActionIsCanUsed(void* self, void* method) {
    bool r = g_origActionIsCanUsed ? g_origActionIsCanUsed(self, method) : false;
    SLCB_DIAG("skl.can self=%p hero=%p eq=%d orig=%d skill_on=%d", self, (void*)g_heroSkill,
              (g_heroSkill && self == g_heroSkill) ? 1 : 0, r ? 1 : 0, g_skill.load() ? 1 : 0);
    return r;
}

// ⑬ Teran.ActionInfo.UsedSkill()：观察「技能被消耗」这一刻
typedef void (*TplActionUsedSkill_t)(void* self, void* method);
static TplActionUsedSkill_t g_origActionUsedSkill = nullptr;
static void Hooked_ActionUsedSkill(void* self, void* method) {
    SLCB_DIAG("skl.used self=%p hero=%p eq=%d", self, (void*)g_heroSkill,
              (g_heroSkill && self == g_heroSkill) ? 1 : 0);
    if (g_origActionUsedSkill) g_origActionUsedSkill(self, method);
}

// ⑭ Teran.ActionInfo.UpdateCoolingTime(float t, ?)：**技能 CD 的真正计时器**
// 诊断实证（01:26 实测）：它每帧被调一次、t≈0.018（帧 dt）；get_IsCanUsed() 已被我们
// 强制 true，但 HeroBase.UseSkill() 依旧返回 false ⇒ 真门禁是 ActionInfo 自己的
// CD/充能状态。于是：① 把每帧的 dt 换成 999（时间快进）；② 再把充能值钉到 MaxChargeValue。
typedef void (*TplActionUpdateCool_t)(void* self, float t, int64_t p2, void* method);
static TplActionUpdateCool_t g_origActionUpdateCool = nullptr;
static void Hooked_ActionUpdateCool(void* self, float t, int64_t p2, void* method) {
    SLCB_DIAG("skl.cool hit self=%p hero=%p eq=%d t=%f", self, (void*)g_heroSkill,
              (g_heroSkill && self == g_heroSkill) ? 1 : 0, t);   // 无条件：先证明它到底有没有被调
    if (IsHeroSkill(self)) {
        if (g_origActionUpdateCool) g_origActionUpdateCool(self, 999.0f, p2, method);  // 时间快进
        SLCB_DIAG("skl.cool FIX self=%p t=999", self);
        return;
    }
    if (g_origActionUpdateCool) g_origActionUpdateCool(self, t, p2, method);
}

// ---- 无限技能 · 第二条通路：Teran.SkillInfo（另一套 技能定义：CoolDowmTime/MPConsumption）----
// 它与 ActionInfo 有同名三件套（IsCanUsed / UsedSkill / UpdateCoolingTime），
// 是「技能不可用」的另一个可能门禁。这里只改只读侧（IsCanUsed/CoolingTime），
// 绝不碰 UpdateCoolingTime —— 其 t 若是每帧 dt，置 0 会把 CD 永久冻住（越改越坏）。
typedef bool (*TplSkillInfoIsCanUsed_t)(void* self, int32_t curMP, void* method);
static TplSkillInfoIsCanUsed_t g_origSkillInfoIsCanUsed = nullptr;
static bool Hooked_SkillInfoIsCanUsed(void* self, int32_t curMP, void* method) {
    const bool on = g_skill.load();
    bool r = g_origSkillInfoIsCanUsed ? g_origSkillInfoIsCanUsed(self, curMP, method) : false;
    SLCB_DIAG("skinf.can self=%p mp=%d orig=%d skill_on=%d", self, curMP, r ? 1 : 0, on ? 1 : 0);
    if (on) return true;
    return r;
}

typedef float (*TplSkillInfoGetCool_t)(void* self, void* method);
static TplSkillInfoGetCool_t g_origSkillInfoGetCool = nullptr;
static float Hooked_SkillInfoGetCool(void* self, void* method) {
    if (g_skill.load()) return 0.0f;
    return g_origSkillInfoGetCool ? g_origSkillInfoGetCool(self, method) : 0.0f;
}

// ---- 无限金币 / 无限魂石（g_gold / g_soul）--------------------------
// Tera_OB.BagSystem 是局内两种货币的持有者：m_money = 金币、m_soul = 魂石
// （＝界面上紫色旋涡图标那个，用户点名要的「旁边那个货币」）。
// 真实存储在混淆包装字段 m_money_secret / m_soul_secret 里，所以只从
// 属性 getter/setter + Get*/Update* 方法层下手，不碰裸字段偏移。
// 四路一起钉：setter 保证内部值锁死，getter 保证 UI 与购买判定都看到大数。
static const int32_t kMoneyLock = 999;
static const int32_t kSoulLock  = 999;

// ⑪ Tera_OB.BagSystem.set_m_money(int32)
typedef void (*TplSetIntMoney_t)(void* self, int32_t v, void* method);
static TplSetIntMoney_t g_origSetMoney = nullptr;
static void Hooked_SetMoney(void* self, int32_t v, void* method) {
    if (g_gold.load()) v = kMoneyLock;
    if (g_origSetMoney) g_origSetMoney(self, v, method);
}

// ⑫ get_m_money() -> int32 / ⑬ GetMoney() -> int32（两个同名不同 argc 的读口）
typedef int32_t (*TplGetIntMoney_t)(void* self, void* method);
static TplGetIntMoney_t g_origGetMoneyProp = nullptr;
static TplGetIntMoney_t g_origGetMoneyFn   = nullptr;
static int32_t Hooked_GetMoneyProp(void* self, void* method) {
    if (g_gold.load()) return kMoneyLock;
    return g_origGetMoneyProp ? g_origGetMoneyProp(self, method) : 0;
}
static int32_t Hooked_GetMoneyFn(void* self, void* method) {
    if (g_gold.load()) return kMoneyLock;
    return g_origGetMoneyFn ? g_origGetMoneyFn(self, method) : 0;
}

// ⑭ UpdateMoney(int32)：升级/SDK 回填走这里，一并钉死
typedef void (*TplUpdateIntMoney_t)(void* self, int32_t v, void* method);
static TplUpdateIntMoney_t g_origUpdateMoney = nullptr;
static void Hooked_UpdateMoney(void* self, int32_t v, void* method) {
    if (g_gold.load()) v = kMoneyLock;
    if (g_origUpdateMoney) g_origUpdateMoney(self, v, method);
}

// ⑮~⑱ 魂石：与金币完全同构
static TplSetIntMoney_t g_origSetSoul = nullptr;
static void Hooked_SetSoul(void* self, int32_t v, void* method) {
    if (g_soul.load()) v = kSoulLock;
    if (g_origSetSoul) g_origSetSoul(self, v, method);
}
static TplGetIntMoney_t g_origGetSoulProp = nullptr;
static TplGetIntMoney_t g_origGetSoulFn   = nullptr;
static int32_t Hooked_GetSoulProp(void* self, void* method) {
    if (g_soul.load()) return kSoulLock;
    return g_origGetSoulProp ? g_origGetSoulProp(self, method) : 0;
}
static int32_t Hooked_GetSoulFn(void* self, void* method) {
    if (g_soul.load()) return kSoulLock;
    return g_origGetSoulFn ? g_origGetSoulFn(self, method) : 0;
}
static TplUpdateIntMoney_t g_origUpdateSoul = nullptr;
static void Hooked_UpdateSoul(void* self, int32_t v, void* method) {
    if (g_soul.load()) v = kSoulLock;
    if (g_origUpdateSoul) g_origUpdateSoul(self, v, method);
}

/* ================================================================
 *  ⑧ 免广告（SLCB_FEAT_NOAD）
 *    挂 `Teran.OpenAdSDK.ShowAd(AdType, action, soulCount, soulAction)`：
 *    开关 ON 时不播广告，直接按 adType 调游戏自己的 `OnXxxAdFinished()`，
 *    让「广告完成 → 发奖」那条路径照常走完。
 *    adType 数值 → 回调名 的映射**运行时从 Teran.AdType 的静态枚举值读出**，
 *    不靠猜枚举声明顺序。
 *    注：只在 ShowAd 这一层拦截，下层 LeitingSdkAndroid 的原生广告不会发起。
 *  ================================================================ */
static std::atomic<bool> g_noad{false};  // SLCB_FEAT_NOAD

static const char* s_adFinish[32] = {nullptr};

// 通用：按名取 MethodInfo（NS 优先，失败退短名）
static void* SlcbMI(const char* ns, const char* cls, const char* method, int argc) {
    auto& api = GetApi();
    if (!api.ready) return nullptr;
    void* k = FindClassNs(ns, cls);
    if (!k) k = api.FindClassByShortName(cls);
    if (!k) return nullptr;
    return api.GetMethod(k, method, argc);
}

// 通用：runtime_invoke（我们的 Tick 线程不是 il2cpp 线程，必须先 attach）
static void* SlcbInvoke(void* mi, void* obj, void** args) {
    auto& api = GetApi();
    if (!mi || !api.runtime_invoke) return nullptr;
    if (api.thread_attach) {
        void* d = api.Domain();
        if (d) api.thread_attach(d);
    }
    void* exc = nullptr;
    return api.runtime_invoke(mi, obj, args, &exc);
}

static void BuildAdFinishMap() {
    auto& api = GetApi();
    if (!api.ready || !api.class_get_field || !api.field_static_get) {
        LOGW(TPL_LOG " noad: 缺 class_get/field_static_get — 只能用回退回调");
        return;
    }
    void* k = FindClassNs("Teran", "AdType");
    if (!k) k = api.FindClassByShortName("AdType");
    if (!k) { LOGW(TPL_LOG " noad: AdType class miss"); return; }
    static const char* kNames[] = {
        "todayAd", "todayCreateRoomAd", "singlePlayerStatisticsAd", "jailNPCAd", "rebornAd",
        "reforgeAd", "bossRushAd", "multiBossRushAd", "bossRushAdOnline", "multiBossRushAdOnline",
    };
    static const char* kHandlers[] = {
        "OnTodayAdFinished", "OnTodayCreateRoomAdFinished", "OnSinglePlayerStatisticsAdFinished",
        "OnJailNPCAdFinished", "OnRebornAdFinished", "OnReforgeAdFinished",
        "OnBossRushAdFinished", "OnMultiBossRushAdFinished",
        "OnBossRushAdOnlineFinished", "OnMultiBossRushOnlineAdFinished",
    };
    for (int i = 0; i < 10; ++i) {
        void* f = api.class_get_field(k, kNames[i]);
        if (!f) { LOGW(TPL_LOG " noad: enum miss %s", kNames[i]); continue; }
        int32_t v = 0;
        api.field_static_get(f, &v);
        if (v < 0 || v >= 32) { LOGW(TPL_LOG " noad: %s=%d 越界", kNames[i], v); continue; }
        s_adFinish[v] = kHandlers[i];
        LOGI(TPL_LOG " noad: map %s=%d -> %s", kNames[i], v, kHandlers[i]);
    }
}

static void CallAdFinished(void* self, int32_t adType) {
    const char* h = (adType >= 0 && adType < 32) ? s_adFinish[adType] : nullptr;
    if (!h) {
        SLCB_DIAG("noad: 无对应完成回调 type=%d — 退 FinishedAdForAndroid", adType);
        void* fb = SlcbMI("Teran", "OpenAdSDK", "FinishedAdForAndroid", 0);
        if (fb) SlcbInvoke(fb, self, nullptr);
        return;
    }
    void* mi = SlcbMI("Teran", "OpenAdSDK", h, 0);
    if (!mi) { LOGW(TPL_LOG " noad: 完成回调 miss %s", h); return; }
    SlcbInvoke(mi, self, nullptr);
    SLCB_DIAG("noad: type=%d -> %s", adType, h);
}

typedef void (*TplOpenAdShowAd_t)(void* self, int32_t adType, void* a2, void* a3, void* a4, void* method);
static TplOpenAdShowAd_t g_origOpenAdShowAd = nullptr;
static void Hooked_OpenAdShowAd(void* self, int32_t adType, void* a2, void* a3, void* a4, void* method) {
    if (g_noad.load() && SafeObjPtr(self)) {
        SLCB_DIAG("noad.show self=%p type=%d a2=%p a4=%p", self, adType, a2, a4);
        CallAdFinished(self, adType);
        return;
    }
    if (g_origOpenAdShowAd) g_origOpenAdShowAd(self, adType, a2, a3, a4, method);
}

typedef bool (*TplOpenAdIsReady_t)(void* self, void* method);
static TplOpenAdIsReady_t g_origOpenAdIsReady = nullptr;
static bool Hooked_OpenAdIsReady(void* self, void* method) {
    if (g_noad.load()) return true;  // 广告按钮始终可点（点了由上面拦掉）
    return g_origOpenAdIsReady ? g_origOpenAdIsReady(self, method) : false;
}

// 表尾必须是 tag == nullptr 的空槽（InstallBattleHooks 靠它收尾）。
static const HookSlot kSlots[] = {
    // tag,        rva, expectInsn, ns,        cls,          method,                     argc, hookFn,                              origPtr
    {"inv.can",    0, 0, "Tera_OB", "HeroBase",   "SetIsCanBeAttacked",     1, (void*)Hooked_SetIsCanBeAttacked,      (void**)&g_origSetIsCanBeAttacked},
    {"inv.force",  0, 0, "Tera_OB", "HeroBase",   "SetCanBeAttacked_Force", 0, (void*)Hooked_SetCanBeAttackedForce,   (void**)&g_origSetCanBeAttackedForce},
    {"inv.beat",   0, 0, "Tera_OB", "HeroBase",   "BeAttacked",             3, (void*)Hooked_HeroBeAttacked,          (void**)&g_origHeroBeAttacked},
    {"dmg.pure",   0, 0, "Teran",   "Creature",   "calPureDamange",         5, (void*)Hooked_CalPureDamange,          (void**)&g_origCalPureDamange},
    {"skl.get",    0, 0, "Tera_OB", "HeroBase",   "get_Skill",              0, (void*)Hooked_HeroGetSkill,            (void**)&g_origHeroGetSkill},
    {"skl.act",    0, 0, "Tera_OB", "HeroBase",   "DoAction_Skill_GetInfo", 0, (void*)Hooked_DoActionSkillGetInfo,    (void**)&g_origDoActionSkillGetInfo},
    {"skl.can",    0, 0, "Teran",   "ActionInfo", "get_IsCanUsed",          0, (void*)Hooked_ActionIsCanUsed,         (void**)&g_origActionIsCanUsed},
    {"skl.use",    0, 0, "Tera_OB", "HeroBase",   "UseSkill",               0, (void*)Hooked_HeroUseSkill,            (void**)&g_origHeroUseSkill},
    {"skl.used",   0, 0, "Teran",   "ActionInfo", "UsedSkill",              0, (void*)Hooked_ActionUsedSkill,         (void**)&g_origActionUsedSkill},
    {"skl.cool",   0, 0, "Teran",   "ActionInfo", "UpdateCoolingTime",      2, (void*)Hooked_ActionUpdateCool,        (void**)&g_origActionUpdateCool},
    {"skinf.can",  0, 0, "Teran",   "SkillInfo",  "IsCanUsed",              1, (void*)Hooked_SkillInfoIsCanUsed,      (void**)&g_origSkillInfoIsCanUsed},
    {"skinf.cool", 0, 0, "Teran",   "SkillInfo",  "get_CoolingTime",        0, (void*)Hooked_SkillInfoGetCool,        (void**)&g_origSkillInfoGetCool},
    {"gold.set",   0, 0, "Tera_OB", "BagSystem",  "set_m_money",            1, (void*)Hooked_SetMoney,                (void**)&g_origSetMoney},
    {"gold.get",   0, 0, "Tera_OB", "BagSystem",  "get_m_money",            0, (void*)Hooked_GetMoneyProp,            (void**)&g_origGetMoneyProp},
    {"gold.fn",    0, 0, "Tera_OB", "BagSystem",  "GetMoney",               0, (void*)Hooked_GetMoneyFn,              (void**)&g_origGetMoneyFn},
    {"gold.upd",   0, 0, "Tera_OB", "BagSystem",  "UpdateMoney",            1, (void*)Hooked_UpdateMoney,             (void**)&g_origUpdateMoney},
    {"soul.set",   0, 0, "Tera_OB", "BagSystem",  "set_m_soul",             1, (void*)Hooked_SetSoul,                 (void**)&g_origSetSoul},
    {"soul.get",   0, 0, "Tera_OB", "BagSystem",  "get_m_soul",             0, (void*)Hooked_GetSoulProp,             (void**)&g_origGetSoulProp},
    {"soul.fn",    0, 0, "Tera_OB", "BagSystem",  "GetSoul",                0, (void*)Hooked_GetSoulFn,               (void**)&g_origGetSoulFn},
    {"soul.upd",   0, 0, "Tera_OB", "BagSystem",  "UpdateSoul",             1, (void*)Hooked_UpdateSoul,              (void**)&g_origUpdateSoul},
    {"noad.show",  0, 0, "Teran",   "OpenAdSDK",  "ShowAd",                 4, (void*)Hooked_OpenAdShowAd,            (void**)&g_origOpenAdShowAd},
    {"noad.ready", 0, 0, "Teran",   "OpenAdSDK",  "IsAdReady",              0, (void*)Hooked_OpenAdIsReady,           (void**)&g_origOpenAdIsReady},
    {nullptr,      0, 0, nullptr,   nullptr,      nullptr,                  0, nullptr,                               nullptr},  // 表尾
};

// ============================================================
// ⑥ 分组安装 + 懒装（开关全关 = 0 hook）
// ============================================================
static void InstallBattleHooks() {
    for (const auto& s : kSlots) {
        if (!s.tag) continue;  // 空占位槽 = 表尾
        if (HookSlotInstall(s)) g_anyHooked.store(true);
    }
}

static void SyncHooks() {
    if (!g_libReady.load()) return;
    // 开关全关 ⇒ 0 hook（懒装，不留 hook 痕迹）
    if (g_inv.load() || g_dmg.load() > 1 || g_skill.load() || g_gold.load() || g_soul.load()
        || g_noad.load()) {
        InstallBattleHooks();
    }
    // 免广告：枚举值→回调名 的映射只需建一次
    static bool s_adMapBuilt = false;
    if (g_noad.load() && !s_adMapBuilt) {
        s_adMapBuilt = true;
        BuildAdFinishMap();
    }
}

// ============================================================
// ⑦ 对外接口（ModEntry.h 注册表契约，勿改签名）
// ============================================================
bool Slcb_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;
    g_libReady.store(true);
    SyncHooks();  // 已 ON 的开关立即装；全关则保持 0 hook
    LOGI(TPL_LOG " lib ready (feature stage, kSlots=22)");
    return true;
}

bool Slcb_IsReady() { return g_libReady.load(); }

void Slcb_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case SLCB_FEAT_INV:   g_inv.store(enabled);  break;
        case SLCB_FEAT_DMG:   g_dmg.store(enabled ? 100 : 1); break;  // Toggle 语义兜底，正常走 OnValueChanged
        case SLCB_FEAT_SKILL: g_skill.store(enabled); break;
        case SLCB_FEAT_GOLD:  g_gold.store(enabled);  break;
        case SLCB_FEAT_SOUL:  g_soul.store(enabled);  break;
        case SLCB_FEAT_NOAD:  g_noad.store(enabled);  break;
        default: break;
    }
    SyncHooks();
}

void Slcb_OnValueChanged(int featNum, int value) {
    if (featNum == SLCB_FEAT_DMG) {
        if (value < 1) value = 1;
        if (value > 100) value = 100;
        g_dmg.store(value);
    }
    SyncHooks();
}

void Slcb_Tick(const bool* features, const int* values) {
    // 只做：同步开关 + 兜底安装。禁止在这里 SafeProbe/扫堆/遍历对象/逐对象写
    if (features) {
        g_inv.store(features[SLCB_FEAT_INV]);
        g_skill.store(features[SLCB_FEAT_SKILL]);
        g_gold.store(features[SLCB_FEAT_GOLD]);
        g_soul.store(features[SLCB_FEAT_SOUL]);
        g_noad.store(features[SLCB_FEAT_NOAD]);
    }
    if (values) {
        int v = values[SLCB_FEAT_DMG];
        if (v < 1) v = 1;
        if (v > 100) v = 100;
        g_dmg.store(v);
    }
    if (!g_libReady.load() && isLibraryLoaded(TPL_TARGET_LIB)) g_libReady.store(true);
    SyncHooks();
}

void Slcb_ForceClear() {
    g_inv.store(false);
    g_dmg.store(1);
    g_skill.store(false);
    g_gold.store(false);
    g_soul.store(false);
    g_noad.store(false);
}
