/* ================================================================
 *  星光菜单游戏模块 — 我的勇者（wdyz，build 357 热更版）
 *
 *  路线（星光自注入，仅挂 Battle 结算层安全热路径）：
 *    - 五倍攻击 / 伤害倍率：BattleObjectCore.SettleDamage 基类 + MonsterCore 覆盖入口
 *    - 无敌：HeroCore.SettleDamage 承伤归零
 *    - 无限技能：免蓝 = HeroCore.settleMp 扣蓝归零；无CD = CheckSkill 返回值归零
 *
 *  2026-10-18 热更 build 357 重 dump 重定位（so sha256 aeb62966…）：
 *    - 运行时加载 files/update/current/lib/arm64-v8a/libil2cpp.so（非 base.apk 内旧 so）
 *    - 类/方法名零漂移（Battle 全家桶未混淆）；RVA 全漂移，按新 dump 回填：
 *        HeroSettle   0x4794970 -> 0x4795FAC（序言 6DBC2BEB 不变）
 *        BaseSettle   0x47748A4 -> 0x4775EE0（序言 D10503FF 不变）
 *        MonsterSettle 0x47BF274 -> 0x47C08B0（序言 6DBB2BEB 不变）
 *        HeroSettleMp 新增 0x47960F0（序言 FE0F1EF8）
 *    - 铁律：不挂 TryEnterCd/任何技能/CD 簇函数（§10.1-1 SIGILL 自杀）；
 *            不直调任何 IL2CPP 方法（GoDeath 等必崩）；ObscuredFloat 字段不直写。
 * ================================================================ */

#include "WdyzMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <sys/mman.h>

#include "Dobby/dobby.h"

// Dobby 自带的指令缓存刷新（读 CTR_EL0 拿真实行大小 + 查 IDC/DIC 位）。
// 本机自写的 dc cvau/ic ivau 写死 64 字节行、不查 IDC/DIC → 无效，故直接用 Dobby 的实现。
extern "C" void ClearCache(void* start, void* end);
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

#define WDYZ_LOG "[Wdyz]"

// ============================================================
// ① 开关与 orig
// ============================================================
static std::atomic<bool> g_libReady{false};  // il2cpp 已映射（≠ 已 hook）
static std::atomic<bool> g_anyHooked{false};
static std::atomic<bool> g_dmg{false};    // 五倍攻击
static std::atomic<bool> g_inv{false};    // 无敌
static std::atomic<bool> g_skill{false};  // 无限技能（无CD+免蓝）
static std::atomic<int>  g_dmgx{1};       // 伤害倍率 Slider 1-50（1=关闭）

// BattleObjectCore.SettleDamage(value, shield, toughness, energy, fromgroup)
// 参数寄存器：s0=value s1=shield s2=toughness s3=energy w1=fromgroup（GP 与 FP 寄存器独立分配）
typedef void (*SettleDamage_t)(void* self, float value, float shield, float toughness,
                               float energy, int fromgroup);
static SettleDamage_t g_origMonsterSettle = nullptr;
static SettleDamage_t g_origHeroSettle = nullptr;
static SettleDamage_t g_origBaseSettle = nullptr;

// HeroCore.settleMp(int value)：value<0 为扣蓝
typedef void (*SettleMp_t)(void* self, int value);
static SettleMp_t g_origHeroSettleMp = nullptr;

// ============================================================
// ② 偏移与 RVA（build 357 重 dump：06_dumped/我的勇者/7.6.3_hu357_il2cpp/dump.cs）
//   地址口径 = dump.cs 的 Offset（文件偏移），供 FileOffToVa 在 /proc/self/maps 换算 VA。
//   RVA 序言首 4 字节（小端）：
//     MonsterCore.SettleDamage   Offset 0x47C08B0  "eb 2b bb 6d" → 0x6DBB2BEB
//     HeroCore.SettleDamage      Offset 0x4795FAC  "eb 2b bc 6d" → 0x6DBC2BEB
//     BattleObjectCore.SettleDamage Offset 0x4775EE0 "ff 03 05 d1" → 0xD10503FF
//     HeroCore.settleMp          Offset 0x47960F0  "fe 0f 1e f8" → 0xF81E0FFE
//   任一序言不符（热更/漂移）→ 自动转按名，不挂错地址。
// ============================================================
enum : size_t {
    kRvaMonsterSettle = 0x47C08B0,
    kRvaHeroSettle    = 0x4795FAC,
    kRvaBaseSettle    = 0x4775EE0,
    kRvaHeroSettleMp  = 0x47960F0,
};

// ============================================================
// ③ 安全助手（防闪退核心，勿删）
// ============================================================
static bool OkPtr(void* p) { return p && Il2Cpp::IsValidHeapPtr(p); }

// ============================================================
// ④ 双路径挂点机器（禁 base+RVA、禁全 RX 误扫）
// ============================================================
static bool IsLibMapLine(const char* line, const char* lib) {
    return line && lib && strstr(line, lib) && strstr(line, ".so");
}

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

static bool HookAtRva(const char* tag, uintptr_t rva, uint32_t expectInsn, void* hook, void** orig) {
    void* va = FileOffToVa(GameUnity::TARGET_LIB, rva);
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
    return GetApi().Load(GameUnity::TARGET_LIB);
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
    uint32_t    expectInsn;  // 0 = 不校验序言
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
    return HookByName(s.tag, s.ns, s.cls, s.method, s.argc, s.hookFn, s.origPtr);
}

// ============================================================
// ⑤ 敌我/类别判定（复用 7.6.3 已验证实现）
// ============================================================
static std::atomic<int> g_monSamp{0};
static std::atomic<int> g_heroSamp{0};
static std::atomic<int> g_rawBaseSamp{0};
static std::atomic<int> g_rawHeroSamp{0};
static std::atomic<int> g_rawMonSamp{0};

static void* g_monsterCoreClass = nullptr;
static void* g_heroCoreClass = nullptr;
static std::atomic<bool> g_classCacheReady{false};

static bool IsSameOrSubclass(void* klass, void* base) {
    if (!klass || !base) return false;
    if (klass == base) return true;
    auto& api = GetApi();
    if (!api.class_parent) return false;
    for (int i = 0; i < 32 && klass; i++) {
        klass = api.class_parent(klass);
        if (klass == base) return true;
    }
    return false;
}

static void ResolveBattleClasses() {
    if (g_classCacheReady.load() || !EnsureApi()) return;
    void* monster = FindClassNs("Battle", "MonsterCore");
    void* hero = FindClassNs("Battle", "HeroCore");
    if (monster) g_monsterCoreClass = monster;
    if (hero) g_heroCoreClass = hero;
    if (monster && hero) {
        g_classCacheReady.store(true);
        LOGI(WDYZ_LOG " class cache MonsterCore=%p HeroCore=%p", monster, hero);
    }
}

static const char* ClassNameOf(void* self) {
    auto& api = GetApi();
    if (!self || !api.object_class || !api.class_name) return "?";
    void* k = api.object_class(self);
    const char* n = k ? api.class_name(k) : nullptr;
    return n ? n : "?";
}

static int ClassTagOf(void* self) {
    auto& api = GetApi();
    if (!api.object_class || !self) return 0;
    void* k = api.object_class(self);
    if (!k) return 0;
    if (IsSameOrSubclass(k, g_monsterCoreClass)) return 1;
    if (IsSameOrSubclass(k, g_heroCoreClass)) return 2;
    if (!api.class_name) return 0;
    const char* n = api.class_name(k);
    if (!n) return 0;
    if (strstr(n, "Monster") || strstr(n, "Boss") || strstr(n, "mob") || strstr(n, "Mob")) return 1;
    if (strstr(n, "Hero")) return 2;
    return 0;
}

static int MonsterTypeOf(void* self) {
    if (!self) return -1;
    auto* core = static_cast<uint8_t*>(self);
    void* monster = *reinterpret_cast<void**>(core + 0xD8);
    if (!monster) return -1;
    auto* obj = static_cast<uint8_t*>(monster);
    return *reinterpret_cast<int*>(obj + 0x60);
}

// ============================================================
// ⑥ 无限技能：无CD 直写链（见 10_docs/我的勇者/挂点分析_无限技能.md）
//   HeroCore → skillEntity(0x30, ISkillable) → skillContainer(0x18) →
//   skillDic(0x18, Dictionary<int,MSkill>) → 枚举 value → MSkill →
//   skillBase(0x18, SKillBase) → CDTime(0x38)=0 / cding(0x3C)=false
//   Dictionary<int,MSkill> 布局（.NET 标准，Metadata v31）：0x18=_entries 0x20=_count；
//   Entry=24B：hashCode(0) next(4) key(8) pad(12) value(16)。防御性判空，布局误读时静默不写。
// ============================================================
static void ClearHeroSkillCd(void* heroCore) {
    if (!heroCore || !OkPtr(heroCore)) return;
    static std::atomic<int> chainSamp{0};
    auto* core = static_cast<uint8_t*>(heroCore);
    void* skillEntity = *reinterpret_cast<void**>(core + 0x30);
    if (!OkPtr(skillEntity)) return;
    void* container = *reinterpret_cast<void**>(static_cast<uint8_t*>(skillEntity) + 0x18);
    if (!OkPtr(container)) return;
    void* dic = *reinterpret_cast<void**>(static_cast<uint8_t*>(container) + 0x18);
    if (!OkPtr(dic)) return;
    void* entries = *reinterpret_cast<void**>(static_cast<uint8_t*>(dic) + 0x18);
    int count = *reinterpret_cast<int*>(static_cast<uint8_t*>(dic) + 0x20);
    if (!entries || count <= 0 || count > 64) {
        if (chainSamp.fetch_add(1) < 6) {
            LOGI(WDYZ_LOG "CD chain stop hero=%p entity=%p cont=%p dic=%p entries=%p count=%d",
                 heroCore, skillEntity, container, dic, entries, count);
        }
        return;
    }
    // Il2CppArray 头 = 0x20（klass0x0+monitor0x8+bounds0x10+max_length0x18）；
    // Entry<int,MSkill*> = 0x18，value 指针在 entry+0x10。
    int cleared = 0;
    for (int i = 0; i < count && cleared < 16; i++) {
        void* mskill = *reinterpret_cast<void**>(static_cast<uint8_t*>(entries) + 0x20 + i * 0x18 + 0x10);
        if (!OkPtr(mskill)) continue;
        void* skillBase = *reinterpret_cast<void**>(static_cast<uint8_t*>(mskill) + 0x18);
        if (!OkPtr(skillBase)) continue;
        *reinterpret_cast<float*>(static_cast<uint8_t*>(skillBase) + 0x38) = 0.0f;  // CDTime
        *reinterpret_cast<bool*>(static_cast<uint8_t*>(skillBase) + 0x3C) = false;   // cding
        cleared++;
    }
    if (chainSamp.fetch_add(1) < 6) {
        LOGI(WDYZ_LOG "CD clear hero=%p dic=%p count=%d cleared=%d",
             heroCore, dic, count, cleared);
    }
}

// ============================================================
// ⑦ hook 函数与挂点表
// ============================================================
// 基类 BattleObjectCore.SettleDamage — 真正 HP 落地点。
// 普通怪的五倍攻击/伤害倍率必须在这里改：MonsterCore 覆盖入口会重算 value。
static void Wdyz_HookedBaseSettle(void* self, float value, float shield, float toughness,
                                  float energy, int fromgroup) {
    if (g_rawBaseSamp.fetch_add(1) < 12) {
        LOGI(WDYZ_LOG "BASE raw v=%+.3f sh=%+.3f tg=%+.3f g=%d self=%p class=%p tag=%d name=%s",
             value, shield, toughness, fromgroup, self,
             GetApi().object_class ? GetApi().object_class(self) : nullptr,
             ClassTagOf(self), ClassNameOf(self));
    }
    const int tag = ClassTagOf(self);
    if (tag == 1) {
        if (g_dmg.load()) {
            const float inValue = value;
            if (value != 0.0f) value *= 5.0f;
            if (g_monSamp.fetch_add(1) < 12) {
                LOGI(WDYZ_LOG "BASE enemy x5 v=%+.3f->%+.3f sh=%+.3f g=%d self=%p",
                     inValue, value, shield, fromgroup, self);
            }
        }
        const float N = static_cast<float>(g_dmgx.load());
        if (N > 1.0f) {
            const float inValue = value, inShield = shield;
            if (value > 0.0f) value *= N;     // 正=伤害，只放大伤害（负=治疗不动）
            if (shield > 0.0f) shield *= N;   // 正=扣盾，独立 if（全吸收时 value==0 也要放大盾伤）
            if (g_monSamp.fetch_add(1) < 12) {
                LOGI(WDYZ_LOG "BASE enemy x%.1f v=%+.3f->%+.3f sh=%+.3f->%+.3f g=%d self=%p",
                     N, inValue, value, inShield, shield, fromgroup, self);
            }
        }
    } else if (tag == 2 && g_inv.load()) {
        if (g_heroSamp.fetch_add(1) < 12) {
            LOGI(WDYZ_LOG "BASE hero inv v=%+.3f->0 sh=%+.3f->0 g=%d self=%p",
                 value, shield, fromgroup, self);
        }
        value = 0.0f;
        shield = 0.0f;
    }
    if (g_origBaseSettle) g_origBaseSettle(self, value, shield, toughness, energy, fromgroup);
}

// 五倍攻击/伤害倍率：仅处理 DamageCollectionBoss=7（覆盖入口内提前返回，不走基类）。
static void Wdyz_HookedMonsterSettle(void* self, float value, float shield, float toughness,
                                     float energy, int fromgroup) {
    const int monsterType = MonsterTypeOf(self);
    if (g_rawMonSamp.fetch_add(1) < 12) {
        LOGI(WDYZ_LOG "MON raw v=%+.3f sh=%+.3f tg=%+.3f g=%d type=%d self=%p class=%p name=%s",
             value, shield, toughness, fromgroup, monsterType, self,
             GetApi().object_class ? GetApi().object_class(self) : nullptr,
             ClassNameOf(self));
    }
    if (monsterType == 7) {
        if (g_dmg.load()) {
            const float inValue = value;
            if (value != 0.0f) value *= 5.0f;
            if (g_monSamp.fetch_add(1) < 12) {
                LOGI(WDYZ_LOG "MON type7 x5 v=%+.3f->%+.3f sh=%+.3f tg=%+.3f g=%d self=%p",
                     inValue, value, shield, toughness, fromgroup, self);
            }
        }
        const float N = static_cast<float>(g_dmgx.load());
        if (N > 1.0f) {
            if (value > 0.0f) value *= N;     // 正=伤害
            if (shield > 0.0f) shield *= N;   // 正=扣盾，独立 if
        }
    }
    if (g_origMonsterSettle) g_origMonsterSettle(self, value, shield, toughness, energy, fromgroup);
}

// 无敌：HeroCore.SettleDamage — value 与 shield 都是承伤分量，必须同时归零。
// 无限技能(无CD)：self=HeroCore 时走链清 CD。
static void Wdyz_HookedHeroSettle(void* self, float value, float shield, float toughness,
                                  float energy, int fromgroup) {
    if (g_rawHeroSamp.fetch_add(1) < 12) {
        LOGI(WDYZ_LOG "HERO raw v=%+.3f sh=%+.3f tg=%+.3f g=%d self=%p class=%p name=%s",
             value, shield, toughness, fromgroup, self,
             GetApi().object_class ? GetApi().object_class(self) : nullptr,
             ClassNameOf(self));
    }
    if (g_inv.load()) {
        if (value != 0.0f || shield != 0.0f) {
            if (g_heroSamp.fetch_add(1) < 12) {
                LOGI(WDYZ_LOG "HERO inv v=%+.3f->0 sh=%+.3f->0 tg=%+.3f g=%d self=%p",
                     value, shield, toughness, fromgroup, self);
            }
        }
        value = 0.0f;
        shield = 0.0f;
    }
    if (g_skill.load()) {
        // ⚠️ 无CD（直写 CDTime/cding）已停用：与游戏 CD 状态机冲突，会把倒计时冻结、技能无法点击。
        // 目前「无限技能」只保留免蓝（settleMp）。无CD 需换方案（见 挂点分析_无限技能_重查.md 未闭环）。
        // ClearHeroSkillCd(self);
    }
    if (g_origHeroSettle) g_origHeroSettle(self, value, shield, toughness, energy, fromgroup);
}

// 免蓝：HeroCore.settleMp(int value)。
// 符号（反汇编 0x477A3C4 scvtf s1,w19 + 0x477A3C8 fsub s1,s8,s1 实证）：
//   newMP = 当前MP − value ⇒ value>0 才是扣蓝、value<0 是加蓝。故归零 value>0。
static void Wdyz_HookedHeroSettleMp(void* self, int value) {
    if (g_skill.load() && value > 0) {
        static std::atomic<int> samp{0};
        if (samp.fetch_add(1) < 12) {
            LOGI(WDYZ_LOG "MP free v=%d->0 self=%p class=%p name=%s",
                 value, self,
                 GetApi().object_class ? GetApi().object_class(self) : nullptr,
                 ClassNameOf(self));
        }
        value = 0;
    }
    if (g_origHeroSettleMp) g_origHeroSettleMp(self, value);
}

static const HookSlot kSlots[] = {
    {"HeroSettle",    kRvaHeroSettle,    0x6DBC2BEB, "Battle", "HeroCore",     "SettleDamage", 5,
     (void*)Wdyz_HookedHeroSettle,    (void**)&g_origHeroSettle},
    {"MonsterSettle", kRvaMonsterSettle, 0x6DBB2BEB, "Battle", "MonsterCore",  "SettleDamage", 5,
     (void*)Wdyz_HookedMonsterSettle, (void**)&g_origMonsterSettle},
    {"BaseSettle",    kRvaBaseSettle,    0xD10503FF, "Battle", "BattleObjectCore", "SettleDamage", 5,
     (void*)Wdyz_HookedBaseSettle,    (void**)&g_origBaseSettle},
    {"HeroSettleMp",  kRvaHeroSettleMp,  0xF81E0FFE, "Battle", "HeroCore",     "settleMp", 1,
     (void*)Wdyz_HookedHeroSettleMp,  (void**)&g_origHeroSettleMp},
};

// ============================================================
// ⑧ 受保护函数补丁助手（无CD 用）：thunk 跟随 + 4B 补丁
// ============================================================
// 跟随 thunk 解析真实函数体基址。游戏把受保护函数搬到匿名内存，原 .text 位置只留
//   ldr x17, [pc,#imm]; br x17; <8字节字面量=wrapper地址>
// wrapper 固定 0x34 字节后才是真实函数体（已实测 CheckSkill / SettleDamage 两处一致）。
static void* ResolveFn(uintptr_t entryFileOff) {
    void* va = FileOffToVa(GameUnity::TARGET_LIB, entryFileOff);
    if (!va) return nullptr;
    uint32_t insn = 0;
    memcpy(&insn, va, 4);
    if (insn == 0x58000051u) {  // ldr x17, [pc, #imm]
        uint64_t t = 0;
        memcpy(&t, (char*)va + 8, 8);
        if (t > 0x10000 && t < 0x7FFFFFFFFFFFull) {
            return (char*)t + 0x34;  // wrapper + 0x34 = 真实函数体
        }
    }
    return va;  // 未 thunk，返回原入口
}

static void Patch4At(void* va, const uint8_t* bytes, uint32_t want, uint32_t revert, const char* tag) {
    if (!va) {
        LOGW(WDYZ_LOG " %s patch va null", tag);
        return;
    }
    uint32_t cur = 0;
    memcpy(&cur, va, 4);
    if (cur == want) return;  // 已是目标态，幂等跳过
    if (cur != revert) {
        LOGW(WDYZ_LOG " %s patch expect drift cur=0x%08X (skip)", tag, cur);
        return;
    }
    uintptr_t page = (uintptr_t)va & ~(uintptr_t)0xFFFu;
    if (mprotect((void*)page, 0x1000, PROT_READ | PROT_WRITE | PROT_EXEC) != 0) {
        LOGW(WDYZ_LOG " %s patch mprotect fail @%p", tag, va);
        return;
    }
    memcpy(va, bytes, 4);
    ClearCache(va, (char*)va + 4);
    mprotect((void*)page, 0x1000, PROT_READ | PROT_EXEC);
    LOGI(WDYZ_LOG " %s patch applied @%p", tag, va);
}

// 无CD：SKillBase.CheckSkill（被 thunk 保护）最终 `orr w0,w0,w19`(0x4832D18) → mov w0,#0。
// 直接把返回值强制为 0=无错误，一并清掉 cding(bit4)/次数(bit7)/连续CD(bit8)/虚方法结果，
// 不碰 CDTime/cding 字段、不动事件链 → 不会出现上版「倒计时卡死」。
static void ApplyNoCdPatch(bool on) {
    void* base = ResolveFn(0x482ECB8);  // SKillBase.CheckSkill 入口（文件偏移）
    if (!base) return;
    static const uint8_t kOn[]  = {0x00, 0x00, 0x80, 0x52};  // mov w0, #0
    static const uint8_t kOff[] = {0x00, 0x00, 0x13, 0x2a};  // orr w0, w0, w19（原指令）
    Patch4At((char*)base + 0x60, on ? kOn : kOff, on ? 0x52800000u : 0x2A130000u,
             on ? 0x2A130000u : 0x52800000u, "nocd");
}

// ============================================================
// ⑨ 分组安装 + 懒装
// ============================================================
static void InstallBattleHooks() {
    ResolveBattleClasses();
    for (const auto& s : kSlots) {
        if (HookSlotInstall(s)) g_anyHooked.store(true);
    }
}

static void SyncHooks() {
    if (!g_libReady.load()) return;
    if (g_dmg.load() || g_inv.load() || g_skill.load()) InstallBattleHooks();
    ApplyNoCdPatch(g_skill.load());
}

// ============================================================
// ⑩ 对外接口（MainModern.cpp 注册表契约）
// ============================================================
bool Wdyz_TryInit() {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) return false;
    g_libReady.store(true);
    SyncHooks();
    LOGI(WDYZ_LOG " lib ready (dual-path, lazy hooks)");
    return true;
}

bool Wdyz_IsReady() { return g_libReady.load(); }

void Wdyz_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case WDYZ_FEAT_DMG:  g_dmg.store(enabled); break;
        case WDYZ_FEAT_INV:  g_inv.store(enabled); break;
        case WDYZ_FEAT_SKILL:g_skill.store(enabled); break;
        default: break;
    }
    SyncHooks();
}

void Wdyz_OnValueChanged(int featNum, int value) {
    if (featNum == WDYZ_FEAT_DMGX) {
        if (value < 1) value = 1;
        if (value > 50) value = 50;
        g_dmgx.store(value);
        LOGI(WDYZ_LOG " dmgx=%d", value);
    }
}

void Wdyz_Tick(const bool* features, const int* values) {
    if (features) {
        g_dmg.store(features[WDYZ_FEAT_DMG]);
        g_inv.store(features[WDYZ_FEAT_INV]);
        g_skill.store(features[WDYZ_FEAT_SKILL]);
    }
    if (values) {
        int v = values[WDYZ_FEAT_DMGX];
        if (v < 1) v = 1;
        if (v > 50) v = 50;
        g_dmgx.store(v);
    }
    if (!g_libReady.load() && isLibraryLoaded(GameUnity::TARGET_LIB)) g_libReady.store(true);
    SyncHooks();
}

void Wdyz_ForceClear() {
    g_dmg.store(false);
    g_inv.store(false);
    g_skill.store(false);
    g_dmgx.store(1);
    ApplyNoCdPatch(false);
}
