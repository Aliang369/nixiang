/* ================================================================
 *  农场守护者 2.1.21 / vc21 / com.qld.ncsh.xiyou
 *
 *  挂点（2026-11-10 真机取证，详见 10_docs/农场守护者/路线锁定.md）：
 *    HybridCLR InterpFrameGroup::EnterFrameFromInterpreter
 *      VA 0x17E7888 / file 0x17E3888 / 序言 0xA9BC67FE (stp x30,x25,[sp,#-0x40]!)
 *      签名 (x0=frameGroup, x1=MethodInfo, x2=argsPkt)
 *    判据：ldr x24,[x22,#0x58] 取 interpData / ldr w1,[x24,#0x2c] 读栈大小 /
 *          bl 0x17e77c4 分配栈槽 / 尾部 br x10 跳进 Execute(0x17E9220)。
 *    调用环：桩(0x18C7A08) → Execute(0x17E9220) ──call 指令──→ 本函数 ──br x10──→ 回 Execute。
 *          ⇒ C#→C# 与 native→C# 都必经本函数（长生天机降世「只承接 native→C#」不适用本作）。
 *
 *  目标方法（QLD.Fight.Runtime.dll，运行时枚举）：
 *    MonsterEntity.DoRemoveHp/6(bool, Int64, TrueSync.FP&, bool, Int32, Int32)  怪物扣血
 *    TowerEntity / ChildTowerEntity / HeroEntity / SummonEntity .DoRemoveHp/6   我方扣血
 *    SkillComponent.SetSkillCd/3(SkillUnit, Boolean, TrueSync.FP)               技能冷却
 *
 *  argsPkt 槽位（每槽 8 字节）：
 *    DoRemoveHp: +0=this  +8=bool  +16=Int64 dmg  +24=FP&  +32=bool  +40=int  +48=int
 *    SetSkillCd: +0=this  +8=SkillUnit  +16=bool  +24=FP cd
 *
 *  敌我判据：无阵营字段，按类类型（MonsterEntity 是怪；其余是我方）。
 *  禁止：ELF base+RVA（本 so va−file=0x4000，frida/native 侧都用 VA）。
 * ================================================================ */

#include "NcshMod.h"
#include "Includes/SecStrings.hpp"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <dlfcn.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
// #include "GameConfig.h"  // 本模块不依赖 GameConfig（TARGET_LIB 自解析）
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

#define TPL_TARGET_LIB sec_lib_il2cpp()

enum : uintptr_t {
    FILE_ENTER_INTERP = 0x17E3888,   // EnterFrameFromInterpreter（VA 0x17E7888）
};

// argsPkt 槽位
enum : size_t {
    PKT_THIS  = 0,
    PKT_DMG   = 16,   // DoRemoveHp arg1 : Int64
    PKT_CD_FP = 24,   // SetSkillCd arg2 : TrueSync.FP
};

// EntityBase 字段（1.8/2.1.21 实测）
enum : size_t {
    OFF_CUR_HP  = 88,
    OFF_MAX_HP  = 92,
    OFF_IS_DEAD = 236,
};

enum : uint8_t {
    KIND_NONE = 0,
    KIND_MONSTER_HP = 1,   // MonsterEntity.DoRemoveHp
    KIND_ALLY_HP    = 2,   // Tower/ChildTower/Hero/Summon .DoRemoveHp
    KIND_SET_CD     = 3,   // SkillComponent.SetSkillCd
};

static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_dmg100{false};
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_skill{false};
static std::atomic<uint32_t> g_tickN{0};
static std::atomic<uint32_t> g_hitDmg{0};
static std::atomic<uint32_t> g_hitInv{0};
static std::atomic<uint32_t> g_hitCd{0};
static std::atomic<uint32_t> g_see{0};

typedef const char* (*method_get_name_t)(void* method);
typedef void* (*method_get_class_t)(void* method);
typedef const char* (*class_get_name_t)(void* klass);
static method_get_name_t g_methodGetName = nullptr;
static method_get_class_t g_methodGetClass = nullptr;
static class_get_name_t g_classGetName = nullptr;

typedef void* (*EnterInterp_t)(void* self, void* methodInfo, void* argsPkt);
static EnterInterp_t g_origEnterInterp = nullptr;

// ---------------------------------------------------------------- 落盘日志
// logcat main 环形缓冲只有 256 KiB，Unity 日志几秒冲光 ⇒ 关键状态必须落盘。
static void DbgLog(const char* fmt, ...) {
    static std::atomic<uint32_t> cnt{0};
    if (cnt.fetch_add(1) > 4000) return;      // 熔断，防写爆
    char path[256];
    snprintf(path, sizeof(path), "/data/data/com.qld.ncsh.xiyou/files/ncsh_dbg.log");
    FILE* f = fopen(path, "a");
    if (!f) return;
    va_list ap;
    va_start(ap, fmt);
    vfprintf(f, fmt, ap);
    va_end(ap);
    std::fputc(10, f);
    fclose(f);
}

static bool EnsureSyms() {
    if (g_methodGetName) return true;
    void* h = dlopen(TPL_TARGET_LIB, RTLD_NOW);
    if (!h) return false;
    g_methodGetName = (method_get_name_t)dlsym(h, "il2cpp_method_get_name");
    g_methodGetClass = (method_get_class_t)dlsym(h, "il2cpp_method_get_class");
    g_classGetName = (class_get_name_t)dlsym(h, "il2cpp_class_get_name");
    return g_methodGetName != nullptr;
}

/** file_off → 运行时 VA。认 r-xp 及 HybridCLR 的 rwxp。 */
static void* FileOffToVa(const char* lib, uintptr_t fileOff) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return nullptr;
    char line[768];
    void* found = nullptr;
    while (fgets(line, sizeof(line), fp)) {
        if (!strstr(line, lib)) continue;
        unsigned long start = 0, end = 0, off = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (!strchr(perm, 'x')) continue;
        if (end <= start) continue;
        if (fileOff >= off && fileOff < off + (end - start)) {
            found = (void*)(start + (fileOff - off));
            break;
        }
    }
    fclose(fp);
    return found;
}

static const char* MiName(void* mi) {
    if (!mi || !g_methodGetName) return "";
    const char* n = g_methodGetName(mi);
    return n ? n : "";
}

static const char* MiClass(void* mi) {
    if (!mi || !g_methodGetClass || !g_classGetName) return "";
    void* k = g_methodGetClass(mi);
    if (!k) return "";
    const char* n = g_classGetName(k);
    return n ? n : "";
}

static uint8_t ClassifyMi(void* mi) {
    const char* cn = MiClass(mi);
    const char* mn = MiName(mi);
    if (!cn || !mn || !cn[0] || !mn[0]) return KIND_NONE;
    if (strcmp(mn, "DoRemoveHp") == 0) {
        if (strcmp(cn, "MonsterEntity") == 0) return KIND_MONSTER_HP;
        if (strcmp(cn, "TowerEntity") == 0 || strcmp(cn, "ChildTowerEntity") == 0 ||
            strcmp(cn, "HeroEntity") == 0 || strcmp(cn, "SummonEntity") == 0)
            return KIND_ALLY_HP;
        return KIND_NONE;
    }
    if (strcmp(mn, "SetSkillCd") == 0 && strcmp(cn, "SkillComponent") == 0) return KIND_SET_CD;
    return KIND_NONE;
}

// ---------------------------------------------------------------- mi 分类缓存
struct MiCacheEnt {
    void* mi;
    uint8_t kind;
};
static MiCacheEnt g_miCache[4096];

static uint8_t KindOf(void* mi) {
    if (!mi) return KIND_NONE;
    uintptr_t h = (reinterpret_cast<uintptr_t>(mi) >> 4) & 4095u;
    for (int p = 0; p < 12; p++) {
        MiCacheEnt& e = g_miCache[(h + static_cast<uintptr_t>(p)) & 4095u];
        if (e.mi == mi) return e.kind;
        if (!e.mi) {
            uint8_t k = ClassifyMi(mi);
            e.mi = mi;
            e.kind = k;
            if (k != KIND_NONE) {
                uint32_t s = g_see.fetch_add(1);
                if (s < 12) {
                    DbgLog("see %s.%s kind=%u", MiClass(mi), MiName(mi), k);
                    LOGI("[NCSH] "" see %s.%s kind=%u", MiClass(mi), MiName(mi), k);
                }
            }
            return k;
        }
    }
    return ClassifyMi(mi);
}

// ---------------------------------------------------------------- 改参
static void PatchArgs(uint8_t kind, void* argsPkt) {
    if (!argsPkt) return;
    if (kind == KIND_MONSTER_HP && g_dmg100.load()) {
        int64_t d = *reinterpret_cast<int64_t*>(reinterpret_cast<uintptr_t>(argsPkt) + PKT_DMG);
        if (d > 0 && d < 10000000LL) {
            int64_t nd = d * 100;
            *reinterpret_cast<int64_t*>(reinterpret_cast<uintptr_t>(argsPkt) + PKT_DMG) = nd;
            uint32_t n = g_hitDmg.fetch_add(1);
            if (n < 20) DbgLog("dmg100 #%u %lld -> %lld", n + 1, (long long)d, (long long)nd);
        }
    } else if (kind == KIND_ALLY_HP && g_inv.load()) {
        int64_t d = *reinterpret_cast<int64_t*>(reinterpret_cast<uintptr_t>(argsPkt) + PKT_DMG);
        if (d != 0) {
            *reinterpret_cast<int64_t*>(reinterpret_cast<uintptr_t>(argsPkt) + PKT_DMG) = 0;
            uint32_t n = g_hitInv.fetch_add(1);
            if (n < 20) DbgLog("inv #%u dropped %lld", n + 1, (long long)d);
        }
    } else if (kind == KIND_SET_CD && g_skill.load()) {
        int64_t cd = *reinterpret_cast<int64_t*>(reinterpret_cast<uintptr_t>(argsPkt) + PKT_CD_FP);
        if (cd != 0) {
            *reinterpret_cast<int64_t*>(reinterpret_cast<uintptr_t>(argsPkt) + PKT_CD_FP) = 0;
            uint32_t n = g_hitCd.fetch_add(1);
            if (n < 20) DbgLog("cd0 #%u raw=%lld", n + 1, (long long)cd);
        }
    }
}

static void* Hooked_EnterInterp(void* self, void* methodInfo, void* argsPkt) {
    if (g_dmg100.load() || g_inv.load() || g_skill.load()) {
        uint8_t k = KindOf(methodInfo);
        if (k != KIND_NONE) PatchArgs(k, argsPkt);
    }
    return g_origEnterInterp ? g_origEnterInterp(self, methodInfo, argsPkt) : nullptr;
}

static bool HookPtr(void* addr, void* repl, void** orig, const char* tag) {
    if (!addr) {
        LOGW("[NCSH] "" %s addr null", tag);
        return false;
    }
    if (*orig) return true;
    if (DobbyHook(addr, repl, orig) != 0) {
        LOGE("[NCSH] "" DobbyHook %s failed @ %p", tag, addr);
        return false;
    }
    LOGI("[NCSH] "" hooked %s @ %p", tag, addr);
    DbgLog("hooked %s @ %p", tag, addr);
    return true;
}

bool Ncsh_TryInit(JavaVM* vm) {
    (void)vm;
    if (g_ready.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;
    EnsureSyms();

    void* enter = FileOffToVa(sec_lib_il2cpp(), FILE_ENTER_INTERP);
    LOGI("[NCSH] "" enter-interp VA=%p file=0x%lX", enter, (unsigned long)FILE_ENTER_INTERP);
    if (!HookPtr(enter, (void*)Hooked_EnterInterp, (void**)&g_origEnterInterp, "enter-interp"))
        return false;

    g_ready.store(true);
    DbgLog("init ok enter=%p", enter);
    LOGI("[NCSH] "" 初始化成功");
    return true;
}

bool Ncsh_IsReady() { return g_ready.load(); }

void Ncsh_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case NCSH_FEAT_DMG100: g_dmg100.store(enabled); break;
        case NCSH_FEAT_INVULN: g_inv.store(enabled); break;
        case NCSH_FEAT_SKILL:  g_skill.store(enabled); break;
        default: break;
    }
    LOGI("[NCSH] "" feat %d -> %d", featNum, enabled ? 1 : 0);
}

void Ncsh_OnValueChanged(int featNum, int value) {
    (void)featNum;
    (void)value;
}

void Ncsh_Tick(const bool* features, const int* values) {
    (void)values;
    g_tickN.fetch_add(1);
    if (!features) return;
    g_dmg100.store(features[NCSH_FEAT_DMG100]);
    g_inv.store(features[NCSH_FEAT_INVULN]);
    g_skill.store(features[NCSH_FEAT_SKILL]);
}

void Ncsh_ForceClear() {
    g_dmg100.store(false);
    g_inv.store(false);
    g_skill.store(false);
    LOGI("[NCSH] "" ForceClear");
}
