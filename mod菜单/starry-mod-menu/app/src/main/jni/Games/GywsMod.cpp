/* ================================================================
 *  孤勇无双 1.0.3 — 伤害三件套（运行时按名解析，RVA 随版本浮动）
 *
 *  钉死见 10_docs/孤勇无双/抓点定稿_一次收齐_20260814.md（1.0.2 定稿；1.0.3 同名钩）
 *  - ReceiveDamage：己方免伤（无敌）
 *  - HpComponent.DeductHp：打怪实际扣血口（秒杀×100）——实机 ReceiveDamage
 *    几乎只见到 camp=Player 挨打，打怪不走该钩
 *  - OnDamageEffect：MonsterMgr 列表扩散（敌方 camp=Npc/Unknown）
 *  - 客户端上报降敏（按「服端只核客户端字段」模型）：
 *    BattleStatistics.OnMonterDamageAbnormal 吞掉计数；
 *    get_monsterDamageAbnormalTimes / GameInfo.GetMonsterDamageAbnormalTimes 恒 0
 * ================================================================ */

#include "GywsMod.h"

#include <atomic>
#include <cstdint>
#include <cstring>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

enum EntityCamp : int32_t {
    kCampUnknown = 0,
    kCampPlayer  = 1,
    kCampNpc     = 2,
    kCampPet     = 3,
    kCampPhantom = 4,
};

enum EntityTypeBattle : int32_t {
    kTypeMonster = 5,  // Battle.EntityType.Monster
};

enum : size_t {
    kOffEntityType     = 0x28,  // BattleEntity.m_EntityType
    kOffCamp           = 0x2C,  // BattleEntity.m_Camp
    kOffCompOwner      = 0x18,  // Component.m_Owner
    kOffMgrEntityList  = 0x18,  // BattleEntityMgr.m_EntityList
    kOffListItems      = 0x10,  // List<>._items
    kOffListSize       = 0x18,  // List<>._size
    kOffArrayData      = 0x20,  // Il2CppArray first element
};

static constexpr int64_t kSeckillMul = 100;
static constexpr int64_t kDamageCap  = 0x0147AE147AE147AELL;  // INT64_MAX/100
static constexpr int kAoeMaxExtra    = 40;                    // 0x28

static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_hooked{false};
static std::atomic<bool> g_seckill{false};
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_aoe{false};
static std::atomic<bool> g_aoeReenter{false};
/** 客户端异常伤害计数字段不上报脏值（与战斗开关无关，hook 成功即生效） */
static std::atomic<bool> g_sanitizeReport{true};
static std::atomic<uint32_t> g_hitsRecv{0};
static std::atomic<uint32_t> g_hitsDeduct{0};
static std::atomic<uint32_t> g_hitsAoe{0};
static std::atomic<uint32_t> g_hitsAbnSwallow{0};
static std::atomic<uint32_t> g_hitsAbnGet{0};

typedef int64_t (*ReceiveDamage_t)(void* self, void* target, int64_t damage, void* method);
typedef int64_t (*OnDamageEffect_t)(void* self, void* launcher, void* target, void* a,
                                    void* b, void* method);
typedef int64_t (*DeductHp_t)(void* self, int64_t value, void* method);
typedef void* (*GetInstance_t)(void* method);
typedef void (*OnAbnormal_t)(void* self, void* method);
typedef int32_t (*GetAbnTimes_t)(void* self, void* method);

static ReceiveDamage_t g_origRecv = nullptr;
static OnDamageEffect_t g_origOnDmg = nullptr;
static DeductHp_t g_origDeduct = nullptr;
static GetInstance_t g_getMonsterMgr = nullptr;
static void* g_miGetInst = nullptr;
static OnAbnormal_t g_origOnAbn = nullptr;
static GetAbnTimes_t g_origGetAbnStat = nullptr;
static GetAbnTimes_t g_origGetAbnInfo = nullptr;
static size_t g_offAbnTimes = 0;

static void* MethodPointer(void* mi) {
    return mi ? *reinterpret_cast<void**>(mi) : nullptr;
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

static int32_t ReadCamp(void* entity) {
    if (!entity || !Il2Cpp::IsValidHeapPtr(entity)) return kCampUnknown;
    return *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(entity) + kOffCamp);
}

static int32_t ReadEntityType(void* entity) {
    if (!entity || !Il2Cpp::IsValidHeapPtr(entity)) return 0;
    return *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(entity) + kOffEntityType);
}

static bool IsInvCamp(int32_t camp) {
    return camp == kCampPlayer || camp == kCampPet || camp == kCampPhantom;
}

/** 实机怪物常为 camp=Unknown(0)；Npc(2) 仍保留。Monster 类型一律当敌。 */
static bool IsEnemyEntity(void* entity) {
    if (!entity) return false;
    int32_t camp = ReadCamp(entity);
    if (IsInvCamp(camp)) return false;
    if (camp == kCampNpc || camp == kCampUnknown) return true;
    return ReadEntityType(entity) == kTypeMonster;
}

static int64_t ApplySeckill(int64_t damage) {
    if (damage < 1) return damage;
    int64_t d = damage < kDamageCap ? damage : kDamageCap;
    return d * kSeckillMul;
}

static void ZeroAbnField(void* self) {
    if (!self || !g_offAbnTimes || !Il2Cpp::IsValidHeapPtr(self)) return;
    Il2Cpp::WriteI32(self, g_offAbnTimes, 0);
}

/** 吞掉异常伤害事件：不调 orig，避免计数 + 可能的连带上报。 */
static void Hooked_OnMonterDamageAbnormal(void* self, void* method) {
    if (g_sanitizeReport.load()) {
        ZeroAbnField(self);
        uint32_t n = g_hitsAbnSwallow.fetch_add(1) + 1;
        if (n <= 8 || (n % 40) == 0)
            LOGI("[Gyws] swallow OnMonterDamageAbnormal n=%u", n);
        return;
    }
    if (g_origOnAbn) g_origOnAbn(self, method);
}

static int32_t Hooked_get_monsterDamageAbnormalTimes(void* self, void* method) {
    if (g_sanitizeReport.load()) {
        ZeroAbnField(self);
        uint32_t n = g_hitsAbnGet.fetch_add(1) + 1;
        if (n <= 8 || (n % 50) == 0)
            LOGI("[Gyws] get_monsterDamageAbnormalTimes -> 0 (stat) n=%u", n);
        return 0;
    }
    return g_origGetAbnStat ? g_origGetAbnStat(self, method) : 0;
}

static int32_t Hooked_GetMonsterDamageAbnormalTimes(void* self, void* method) {
    if (g_sanitizeReport.load()) {
        uint32_t n = g_hitsAbnGet.fetch_add(1) + 1;
        if (n <= 8 || (n % 50) == 0)
            LOGI("[Gyws] GetMonsterDamageAbnormalTimes -> 0 (GameInfo) n=%u", n);
        return 0;
    }
    return g_origGetAbnInfo ? g_origGetAbnInfo(self, method) : 0;
}

static int64_t Hooked_ReceiveDamage(void* self, void* target, int64_t damage, void* method) {
    const int64_t raw = damage;
    int32_t camp = target ? ReadCamp(target) : kCampUnknown;
    if (target && damage >= 1) {
        if (IsInvCamp(camp)) {
            if (g_inv.load()) damage = 0;
        } else if (IsEnemyEntity(target)) {
            if (g_seckill.load()) damage = ApplySeckill(damage);
        }
    }
    uint32_t n = g_hitsRecv.fetch_add(1) + 1;
    if (n <= 16 || (n % 80) == 0)
        LOGI("[Gyws] ReceiveDamage hits=%u camp=%d type=%d raw=%lld out=%lld sk=%d inv=%d",
             n, camp, target ? ReadEntityType(target) : -1, (long long)raw, (long long)damage,
             g_seckill.load() ? 1 : 0, g_inv.load() ? 1 : 0);
    return g_origRecv ? g_origRecv(self, target, damage, method) : damage;
}

static int64_t Hooked_DeductHp(void* self, int64_t value, void* method) {
    // 秒杀只在 ReceiveDamage 做；这里再乘会变成 ×10000。
    // 仅保留己方 Deduct 归零，防止个别不走 ReceiveDamage 的扣血。
    if (self && value >= 1 && g_inv.load()) {
        void* owner = Il2Cpp::ReadPtr(self, kOffCompOwner);
        if (owner && Il2Cpp::IsValidHeapPtr(owner) && IsInvCamp(ReadCamp(owner))) {
            value = 0;
        }
    }
    return g_origDeduct ? g_origDeduct(self, value, method) : value;
}

static int64_t Hooked_OnDamageEffect(void* self, void* launcher, void* target, void* a, void* b,
                                     void* method) {
    int64_t ret = g_origOnDmg ? g_origOnDmg(self, launcher, target, a, b, method) : 0;

    if (!g_aoe.load() || !target || g_aoeReenter.load()) return ret;
    if (!IsEnemyEntity(target)) {
        static std::atomic<uint32_t> skipCamp{0};
        uint32_t s = skipCamp.fetch_add(1) + 1;
        if (s <= 12 || (s % 40) == 0)
            LOGI("[Gyws] AOE skip camp=%d type=%d (skips=%u)", ReadCamp(target),
                 ReadEntityType(target), s);
        return ret;
    }
    if (!g_getMonsterMgr || !g_origOnDmg) return ret;

    g_aoeReenter.store(true);
    void* mgr = nullptr;
    if (!Il2Cpp::SafeProbe([&]() -> bool {
            mgr = g_getMonsterMgr(g_miGetInst);
            return true;
        })) {
        g_aoeReenter.store(false);
        return ret;
    }
    if (!mgr || !Il2Cpp::IsValidHeapPtr(mgr)) {
        g_aoeReenter.store(false);
        return ret;
    }

    void* list = Il2Cpp::ReadPtr(mgr, kOffMgrEntityList);
    if (!list || !Il2Cpp::IsValidHeapPtr(list)) {
        g_aoeReenter.store(false);
        return ret;
    }

    int32_t size = *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(list) + kOffListSize);
    if (size < 2 || size > 500) {
        g_aoeReenter.store(false);
        return ret;
    }

    void* items = Il2Cpp::ReadPtr(list, kOffListItems);
    if (!items || !Il2Cpp::IsValidHeapPtr(items)) {
        g_aoeReenter.store(false);
        return ret;
    }

    int spread = 0;
    uintptr_t base = reinterpret_cast<uintptr_t>(items) + kOffArrayData;
    for (int i = 0; i < size && spread < kAoeMaxExtra; i++) {
        void* ent = *reinterpret_cast<void**>(base + static_cast<size_t>(i) * 8u);
        if (!ent || ent == target) continue;
        if (!Il2Cpp::IsValidHeapPtr(ent)) continue;
        if (!IsEnemyEntity(ent)) continue;
        g_origOnDmg(self, launcher, ent, a, b, method);
        spread++;
    }

    g_aoeReenter.store(false);
    if (spread > 0) {
        uint32_t n = g_hitsAoe.fetch_add(1) + 1;
        if (n <= 8 || (n % 50) == 0)
            LOGI("[Gyws] AOE spread=%d bursts=%u", spread, n);
    }
    return ret;
}

static bool InstallHooks() {
    if (g_hooked.load()) return true;
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) return false;
    auto& apiLoad = GetApi();
    if (!apiLoad.Load(GameUnity::TARGET_LIB)) {
        LOGE("[Gyws] Il2Cpp Api::Load failed");
        return false;
    }

    void* ses = FindClassNs("Battle.Logic", "SkillEffectSystem");
    void* mm = FindClassNs("Battle.Logic", "MonsterMgr");
    void* hp = FindClassNs("Battle.Logic", "HpComponent");
    void* stats = FindClassNs("Battle.Logic", "BattleStatistics");
    void* ginfo = FindClassNs("", "GameInfo");
    if (!ginfo) ginfo = FindClassNs("Game", "GameInfo");
    if (!ses || !mm || !hp) {
        LOGE("[Gyws] class missing ses=%p mm=%p hp=%p", ses, mm, hp);
        return false;
    }

    auto& api = GetApi();
    void* miRecv = api.GetMethod(ses, "ReceiveDamage", 2);
    void* miOn = api.GetMethod(ses, "OnDamageEffect", 4);
    void* miDeduct = api.GetMethod(hp, "DeductHp", 1);
    g_miGetInst = api.GetMethod(mm, "get_Instance", 0);
    void* fnRecv = MethodPointer(miRecv);
    void* fnOn = MethodPointer(miOn);
    void* fnDeduct = MethodPointer(miDeduct);
    void* fnInst = MethodPointer(g_miGetInst);
    if (!fnRecv || !fnOn || !fnDeduct || !fnInst) {
        LOGE("[Gyws] method ptr missing recv=%p on=%p deduct=%p inst=%p", fnRecv, fnOn, fnDeduct,
             fnInst);
        return false;
    }

    g_getMonsterMgr = reinterpret_cast<GetInstance_t>(fnInst);
    if (DobbyHook(fnRecv, (void*)Hooked_ReceiveDamage, (void**)&g_origRecv) != 0) {
        LOGE("[Gyws] Dobby ReceiveDamage failed");
        return false;
    }
    if (DobbyHook(fnOn, (void*)Hooked_OnDamageEffect, (void**)&g_origOnDmg) != 0) {
        LOGE("[Gyws] Dobby OnDamageEffect failed");
        return false;
    }
    if (DobbyHook(fnDeduct, (void*)Hooked_DeductHp, (void**)&g_origDeduct) != 0) {
        LOGE("[Gyws] Dobby DeductHp failed");
        return false;
    }

    // 上报降敏：失败不阻断战斗 hook
    int reportOk = 0;
    if (stats) {
        g_offAbnTimes = api.FieldOffset(stats, "m_MonsterDamageAbnormalTimes", 0);
        void* miAbn = api.GetMethod(stats, "OnMonterDamageAbnormal", 0);
        if (!miAbn) miAbn = api.GetMethod(stats, "OnMonterDamageAbnormal", 1);
        void* miGetStat = api.GetMethod(stats, "get_monsterDamageAbnormalTimes", 0);
        void* fnAbn = MethodPointer(miAbn);
        void* fnGetStat = MethodPointer(miGetStat);
        if (fnAbn && DobbyHook(fnAbn, (void*)Hooked_OnMonterDamageAbnormal,
                               (void**)&g_origOnAbn) == 0) {
            reportOk++;
            LOGI("[Gyws] hooked OnMonterDamageAbnormal=%p offAbn=%zu", fnAbn, g_offAbnTimes);
        } else {
            LOGE("[Gyws] OnMonterDamageAbnormal hook fail fn=%p", fnAbn);
        }
        if (fnGetStat && DobbyHook(fnGetStat, (void*)Hooked_get_monsterDamageAbnormalTimes,
                                   (void**)&g_origGetAbnStat) == 0) {
            reportOk++;
            LOGI("[Gyws] hooked get_monsterDamageAbnormalTimes=%p", fnGetStat);
        } else {
            LOGE("[Gyws] get_monsterDamageAbnormalTimes hook fail fn=%p", fnGetStat);
        }
    } else {
        LOGE("[Gyws] BattleStatistics class missing");
    }

    // GameInfo 读口：空 ns 找到的类可能不是业务 GameInfo，按短名再找并扫 argc
    if (!ginfo) ginfo = api.FindClassByShortName("GameInfo");
    void* miGetInfo = nullptr;
    void* fnGetInfo = nullptr;
    void* ginfoHit = ginfo;
    auto tryGameInfoMethod = [&](void* klass) -> bool {
        if (!klass) return false;
        for (int argc = 0; argc <= 3; argc++) {
            void* mi = api.GetMethod(klass, "GetMonsterDamageAbnormalTimes", argc);
            void* fn = MethodPointer(mi);
            if (fn) {
                miGetInfo = mi;
                fnGetInfo = fn;
                ginfoHit = klass;
                return true;
            }
        }
        return false;
    };
    if (!tryGameInfoMethod(ginfo) && api.class_name && api.image_class_count && api.image_class) {
        // 业务读口不一定叫 GameInfo；凡带 StartStage 的类再试 GetMonsterDamageAbnormalTimes
        size_t nAsm = 0;
        void** asms = api.Assemblies(&nAsm);
        for (size_t i = 0; !fnGetInfo && asms && i < nAsm; i++) {
            void* img = api.ImageOf(asms[i]);
            if (!img) continue;
            uint32_t nc = api.image_class_count(img);
            for (uint32_t ci = 0; ci < nc && !fnGetInfo; ci++) {
                void* k = api.image_class(img, ci);
                if (!k) continue;
                if (!api.GetMethod(k, "StartStage", 0) && !api.GetMethod(k, "StartStage", 1) &&
                    !api.GetMethod(k, "StartStage", 2))
                    continue;
                tryGameInfoMethod(k);
            }
        }
    }
    if (fnGetInfo && DobbyHook(fnGetInfo, (void*)Hooked_GetMonsterDamageAbnormalTimes,
                               (void**)&g_origGetAbnInfo) == 0) {
        reportOk++;
        LOGI("[Gyws] hooked GetMonsterDamageAbnormalTimes=%p klass=%p", fnGetInfo, ginfoHit);
    } else {
        LOGI("[Gyws] GetMonsterDamageAbnormalTimes 未挂上（结算多半走 BattleStatistics getter）");
    }

    g_hooked.store(true);
    g_ready.store(true);
    LOGI("[Gyws] hooked ReceiveDamage=%p OnDamageEffect=%p DeductHp=%p get_Instance=%p "
         "reportSanitize=%d/3",
         fnRecv, fnOn, fnDeduct, fnInst, reportOk);
    return true;
}

bool Gyws_TryInit() { return InstallHooks(); }

bool Gyws_IsReady() { return g_ready.load(); }

void Gyws_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == GYWS_FEAT_SECKILL) g_seckill.store(enabled);
    if (featNum == GYWS_FEAT_INVINCIBLE) g_inv.store(enabled);
    if (featNum == GYWS_FEAT_AOE) g_aoe.store(enabled);
    LOGI("[Gyws] feat %d -> %d", featNum, enabled ? 1 : 0);
}

void Gyws_Tick(const bool* features) {
    if (!features) return;
    g_seckill.store(features[GYWS_FEAT_SECKILL]);
    g_inv.store(features[GYWS_FEAT_INVINCIBLE]);
    g_aoe.store(features[GYWS_FEAT_AOE]);
    if (!g_ready.load()) InstallHooks();
}

void Gyws_ForceClear() {
    g_seckill.store(false);
    g_inv.store(false);
    g_aoe.store(false);
}
