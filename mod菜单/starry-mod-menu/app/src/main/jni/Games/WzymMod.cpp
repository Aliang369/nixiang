/* ================================================================
 *  物质与魔法 — StatusC IL2CPP Hook（对齐改包 6.03）
 *
 *  稳妥策略：
 *  - 无敌 / 倍伤：优先 OnDamage（子弹路径也覆盖），Update 仅补写 wudi
 *  - 全屏秒杀：保留 Update 每帧清敌军（效果不变），已死亡跳过、少日志
 *  - 移速：友军限频刷新，避免每帧狂写 agent
 *  - 字段：启动时用 il2cpp_field_get_offset 校准，失败回退文档常量
 * ================================================================ */

#include "WzymMod.h"

#include <atomic>
#include <cstdint>
#include <cstring>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;
using Il2Cpp::IsValidHeapPtr;
using Il2Cpp::ReadU8;
using Il2Cpp::ReadI32;
using Il2Cpp::WriteU8;
using Il2Cpp::WriteI32;
using Il2Cpp::WriteF32;
using Il2Cpp::ReadPtr;
using Il2Cpp::SafeProbe;

// 文档 fallback（dump.cs / 抓点笔记）；运行时优先 FieldOffset 覆盖
namespace DocOff {
enum : size_t {
    IS_HERO = 0x7D,
    IS_BUILD = 0x86,
    IS_ALLY = 0x91,
    MY_TEAM = 0xA4,
    IS_DEAD = 0xD9,
    BASE_H = 0x1B8,
    BUFF_H = 0x1C0,
    TOTAL_H = 0x1D0,
    CUR_HP = 0x1E0,
    IS_IN_CTRL = 0x200,
    ANIMATOR = 0x208,
    AGENT = 0x218,
    WUDI = 0x269,
    UP_ATK = 0x27F,
    UP_MOVE = 0x289,
    IN_WUDI = 0x310,
    IN_UP_ATK = 0x354,
    IN_UP_MOVE = 0x378,
};
}
enum : size_t {
    BH_MOVE = 0x28,
    BH_ATK_CD = 0x30,
    BH_ATKSPD_P = 0x3C,
    BH_MOVE_P = 0x44,
};

struct FieldOff {
    size_t isHero = DocOff::IS_HERO;
    size_t isBuild = DocOff::IS_BUILD;
    size_t isAlly = DocOff::IS_ALLY;
    size_t myTeam = DocOff::MY_TEAM;
    size_t isDead = DocOff::IS_DEAD;
    size_t baseH = DocOff::BASE_H;
    size_t buffH = DocOff::BUFF_H;
    size_t totalH = DocOff::TOTAL_H;
    size_t curHp = DocOff::CUR_HP;
    size_t isInCtrl = DocOff::IS_IN_CTRL;
    size_t animator = DocOff::ANIMATOR;
    size_t agent = DocOff::AGENT;
    size_t wudi = DocOff::WUDI;
    size_t upAtk = DocOff::UP_ATK;
    size_t upMove = DocOff::UP_MOVE;
    size_t inWudi = DocOff::IN_WUDI;
    size_t inUpAtk = DocOff::IN_UP_ATK;
    size_t inUpMove = DocOff::IN_UP_MOVE;
};

static FieldOff g_off;
static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_aoe{false};
static std::atomic<int> g_dmgMul{1};
static std::atomic<int> g_speedLvl{1};
static std::atomic<int> g_playerTeam{-1};
static std::atomic<uint32_t> g_speedTick{0};

static void* g_statusClass = nullptr;

typedef int32_t (*UpPhys_t)(void* thiz, int32_t tempDamage, void* atker, void* method);
typedef int32_t (*UpMagic_t)(void* thiz, int32_t tempDamage, int32_t magicSys, void* method);
typedef int32_t (*UpBoth_t)(void* thiz, int32_t tempDamage, void* atker, void* method);
typedef int32_t (*OnDamage_t)(void* thiz, void* atk, int32_t damage, int32_t aoePercent,
                              int32_t realDamage, void* method);
typedef void (*OnMagicDamage_t)(void* thiz, int32_t amount, int32_t magicSys, void* user,
                                bool isAoe, void* method);
typedef void (*Update_t)(void* thiz, void* method);
typedef void (*Death_t)(void* thiz, void* method);
typedef void (*AgentSetSpeed_t)(void* agent, float speed, void* method);
typedef void (*AnimSetSpeed_t)(void* anim, float speed, void* method);

static UpPhys_t g_origUpPhys = nullptr;
static UpMagic_t g_origUpMagic = nullptr;
static UpBoth_t g_origUpBoth = nullptr;
static OnDamage_t g_origOnDamage = nullptr;
static OnMagicDamage_t g_origOnMagic = nullptr;
static Update_t g_origUpdate = nullptr;
static Death_t g_death = nullptr;
static AgentSetSpeed_t g_agentSetSpeed = nullptr;
static AnimSetSpeed_t g_animSetSpeed = nullptr;

static void* MethodPointer(void* methodInfo) {
    return methodInfo ? *reinterpret_cast<void**>(methodInfo) : nullptr;
}

static void ResolveField(Il2Cpp::Api& api, const char* name, size_t& out, size_t fallback) {
    size_t o = api.FieldOffset(g_statusClass, name, 0);
    // StatusC 实例字段应在合理范围内；0 表示未找到
    if (o > 0 && o < 0x800) {
        out = o;
        return;
    }
    out = fallback;
}

static void ResolveAllFields(Il2Cpp::Api& api) {
    ResolveField(api, "isHero", g_off.isHero, DocOff::IS_HERO);
    ResolveField(api, "isBuild", g_off.isBuild, DocOff::IS_BUILD);
    ResolveField(api, "isAlly", g_off.isAlly, DocOff::IS_ALLY);
    ResolveField(api, "myTeam", g_off.myTeam, DocOff::MY_TEAM);
    ResolveField(api, "isDead", g_off.isDead, DocOff::IS_DEAD);
    ResolveField(api, "baseHData", g_off.baseH, DocOff::BASE_H);
    ResolveField(api, "buffHData", g_off.buffH, DocOff::BUFF_H);
    ResolveField(api, "totalHData", g_off.totalH, DocOff::TOTAL_H);
    ResolveField(api, "curHp", g_off.curHp, DocOff::CUR_HP);
    ResolveField(api, "isInController", g_off.isInCtrl, DocOff::IS_IN_CTRL);
    ResolveField(api, "animator", g_off.animator, DocOff::ANIMATOR);
    ResolveField(api, "agent", g_off.agent, DocOff::AGENT);
    ResolveField(api, "wudi", g_off.wudi, DocOff::WUDI);
    ResolveField(api, "upAtk", g_off.upAtk, DocOff::UP_ATK);
    ResolveField(api, "upMove", g_off.upMove, DocOff::UP_MOVE);
    ResolveField(api, "inWudiTime", g_off.inWudi, DocOff::IN_WUDI);
    ResolveField(api, "inUpAtkTime", g_off.inUpAtk, DocOff::IN_UP_ATK);
    ResolveField(api, "inUpMoveTime", g_off.inUpMove, DocOff::IN_UP_MOVE);
    LOGI("[Wzym] fields wudi=0x%zx isHero=0x%zx isAlly=0x%zx isDead=0x%zx curHp=0x%zx",
         g_off.wudi, g_off.isHero, g_off.isAlly, g_off.isDead, g_off.curHp);
}

static void NoteController(void* status) {
    if (!IsValidHeapPtr(status)) return;
    if (ReadU8(status, g_off.isInCtrl) == 0 && ReadU8(status, g_off.isHero) == 0) return;
    int team = ReadI32(status, g_off.myTeam);
    if (team != 0 && team != 1) return;
    int prev = g_playerTeam.load(std::memory_order_relaxed);
    if (prev != team) {
        g_playerTeam.store(team, std::memory_order_relaxed);
        LOGI("[Wzym] playerTeam=%d", team);
    }
}

static bool IsFriendly(void* status) {
    if (!IsValidHeapPtr(status)) return false;
    if (ReadU8(status, g_off.isInCtrl)) return true;
    if (ReadU8(status, g_off.isHero)) return true;
    if (ReadU8(status, g_off.isAlly)) return true;
    int pt = g_playerTeam.load(std::memory_order_relaxed);
    if (pt >= 0 && ReadI32(status, g_off.myTeam) == pt) return true;
    return false;
}

/** 可秒杀 / 可倍伤的敌军目标（跳过建筑与已死亡） */
static bool IsEnemyTarget(void* status) {
    if (!IsValidHeapPtr(status)) return false;
    if (ReadU8(status, g_off.isDead)) return false;
    if (ReadU8(status, g_off.isBuild)) return false;
    if (IsFriendly(status)) return false;
    int pt = g_playerTeam.load(std::memory_order_relaxed);
    if (pt >= 0) return ReadI32(status, g_off.myTeam) != pt;
    return true;
}

static int32_t ApplyMul(int32_t dmg) {
    int mul = g_dmgMul.load(std::memory_order_relaxed);
    if (mul <= 1 || dmg <= 0) return dmg;
    int64_t x = (int64_t)dmg * (int64_t)mul;
    if (x > 0x7fffffff) x = 0x7fffffff;
    return (int32_t)x;
}

static bool ShouldAmplifyHit(void* victim, void* attacker) {
    if (g_dmgMul.load(std::memory_order_relaxed) <= 1) return false;
    // 子弹路径 atk 常不是玩家 StatusC：受害方是敌军即放大（对齐实战）
    if (IsEnemyTarget(victim)) return true;
    // 文档条件：友军打非友军
    if (IsFriendly(attacker) && victim && IsValidHeapPtr(victim) && !IsFriendly(victim) &&
        ReadU8(victim, g_off.isBuild) == 0)
        return true;
    return false;
}

static int32_t Hooked_UpPhysicalDamage(void* thiz, int32_t tempDamage, void* atker, void* method) {
    if (tempDamage > 0 && ShouldAmplifyHit(thiz, atker))
        tempDamage = ApplyMul(tempDamage);
    return g_origUpPhys ? g_origUpPhys(thiz, tempDamage, atker, method) : tempDamage;
}

static int32_t Hooked_UpMagicDamage(void* thiz, int32_t tempDamage, int32_t magicSys, void* method) {
    if (tempDamage > 0 && IsEnemyTarget(thiz))
        tempDamage = ApplyMul(tempDamage);
    return g_origUpMagic ? g_origUpMagic(thiz, tempDamage, magicSys, method) : tempDamage;
}

static int32_t Hooked_UpBothDamage(void* thiz, int32_t tempDamage, void* atker, void* method) {
    if (tempDamage > 0 && ShouldAmplifyHit(thiz, atker))
        tempDamage = ApplyMul(tempDamage);
    return g_origUpBoth ? g_origUpBoth(thiz, tempDamage, atker, method) : tempDamage;
}

static int32_t Hooked_OnDamage(void* thiz, void* atk, int32_t damage, int32_t aoePercent,
                               int32_t realDamage, void* method) {
    // 无敌：友军受害归零（不依赖 Update 写 wudi）
    if (g_inv.load(std::memory_order_relaxed) && IsFriendly(thiz)) {
        damage = 0;
        realDamage = 0;
    } else if (damage > 0 && ShouldAmplifyHit(thiz, atk)) {
        damage = ApplyMul(damage);
        if (realDamage > 0) realDamage = ApplyMul(realDamage);
    }
    return g_origOnDamage ? g_origOnDamage(thiz, atk, damage, aoePercent, realDamage, method)
                          : damage;
}

static void Hooked_OnMagicDamage(void* thiz, int32_t amount, int32_t magicSys, void* user,
                                 bool isAoe, void* method) {
    if (g_inv.load(std::memory_order_relaxed) && IsFriendly(thiz))
        amount = 0;
    else if (amount > 0 && ShouldAmplifyHit(thiz, user))
        amount = ApplyMul(amount);
    if (g_origOnMagic) g_origOnMagic(thiz, amount, magicSys, user, isAoe, method);
}

/** 全屏秒杀：已死跳过；优先调 Death（效果与原先一致） */
static void KillEnemy(void* thiz) {
    if (ReadU8(thiz, g_off.isDead)) return;
    if (g_death) {
        SafeProbe([&]() -> bool {
            g_death(thiz, nullptr);
            return true;
        });
        return;
    }
    WriteI32(thiz, g_off.curHp, 0);
}

static float SpeedMulFromLvl(int lvl) {
    if (lvl <= 1) return 1.0f;
    if (lvl > 5) lvl = 5;
    return 1.0f + 0.25f * (float)(lvl - 1);
}

static void BoostSpeed(void* thiz) {
    int lvl = g_speedLvl.load(std::memory_order_relaxed);
    if (lvl <= 1) return;
    float mul = SpeedMulFromLvl(lvl);

    void* base = ReadPtr(thiz, g_off.baseH);
    void* total = ReadPtr(thiz, g_off.totalH);
    if (!IsValidHeapPtr(base) || !IsValidHeapPtr(total)) return;

    float baseMove = *(float*)((uintptr_t)base + BH_MOVE);
    float baseCd = *(float*)((uintptr_t)base + BH_ATK_CD);
    if (!(baseMove > 0.05f && baseMove < 100.f)) return;
    if (!(baseCd > 0.05f && baseCd < 30.f)) baseCd = 1.0f;

    float newMove = baseMove * mul;
    float newCd = baseCd / mul;
    if (newCd < 0.12f) newCd = 0.12f;

    WriteF32(total, BH_MOVE, newMove);
    WriteF32(total, BH_ATK_CD, newCd);
    WriteI32(total, BH_MOVE_P, (int32_t)((mul - 1.0f) * 100.0f));
    WriteI32(total, BH_ATKSPD_P, (int32_t)((mul - 1.0f) * 100.0f));

    void* buff = ReadPtr(thiz, g_off.buffH);
    if (IsValidHeapPtr(buff)) {
        float buffMove = newMove - baseMove;
        if (buffMove < 0.f) buffMove = 0.f;
        WriteF32(buff, BH_MOVE, buffMove);
    }

    WriteU8(thiz, g_off.upMove, 1);
    WriteU8(thiz, g_off.upAtk, 1);
    WriteF32(thiz, g_off.inUpMove, 2.0f);
    WriteF32(thiz, g_off.inUpAtk, 2.0f);

    // agent/anim 调用较重：每 4 拍刷一次即可，数值层每帧已写
    if ((g_speedTick.load(std::memory_order_relaxed) & 3u) != 0) return;

    void* agent = ReadPtr(thiz, g_off.agent);
    if (g_agentSetSpeed && IsValidHeapPtr(agent)) {
        SafeProbe([&]() -> bool {
            g_agentSetSpeed(agent, newMove, nullptr);
            return true;
        });
    }
    void* anim = ReadPtr(thiz, g_off.animator);
    if (g_animSetSpeed && IsValidHeapPtr(anim)) {
        float animSpd = 1.0f + (mul - 1.0f) * 0.35f;
        SafeProbe([&]() -> bool {
            g_animSetSpeed(anim, animSpd, nullptr);
            return true;
        });
    }
}

static void Hooked_Update(void* thiz, void* method) {
    if (g_origUpdate) g_origUpdate(thiz, method);
    if (!g_ready.load(std::memory_order_relaxed) || !IsValidHeapPtr(thiz)) return;

    const bool inv = g_inv.load(std::memory_order_relaxed);
    const bool aoe = g_aoe.load(std::memory_order_relaxed);
    const int spd = g_speedLvl.load(std::memory_order_relaxed);
    if (!inv && !aoe && spd <= 1) return;

    // 不用整段 SafeProbe（信号处理器进出太贵）；字段读写靠 IsValidHeapPtr
    NoteController(thiz);

    if (IsFriendly(thiz)) {
        if (inv) {
            // 已无敌则少写
            if (ReadU8(thiz, g_off.wudi) == 0)
                WriteU8(thiz, g_off.wudi, 1);
            WriteF32(thiz, g_off.inWudi, 999.0f);
        }
        if (spd > 1) BoostSpeed(thiz);
        return;
    }

    // 全屏秒杀：效果保留 —— 每个敌军 Update 都清（已死跳过）
    if (aoe && IsEnemyTarget(thiz))
        KillEnemy(thiz);
}

static bool HookOne(const char* tag, void* addr, void* hook, void** orig) {
    if (!addr) {
        LOGW("[Wzym] %s addr null", tag);
        return false;
    }
    if (*orig) return true;
    if (DobbyHook(addr, hook, orig) != 0) {
        LOGE("[Wzym] DobbyHook %s failed @ %p", tag, addr);
        return false;
    }
    LOGI("[Wzym] hooked %s @ %p", tag, addr);
    return true;
}

bool Wzym_TryInit() {
    if (g_ready.load()) return true;
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) return false;

    auto& api = GetApi();
    if (!api.Load(GameUnity::TARGET_LIB)) return false;

    g_statusClass = api.FindClassByShortName("StatusC");
    if (!g_statusClass) {
        LOGW("[Wzym] StatusC not found yet");
        return false;
    }

    ResolveAllFields(api);

    void* pPhys = MethodPointer(api.GetMethod(g_statusClass, "UpPhysicalDamage", 2));
    void* pMagic = MethodPointer(api.GetMethod(g_statusClass, "UpMagicDamage", 2));
    void* pBoth = MethodPointer(api.GetMethod(g_statusClass, "UpBothDamage", 2));
    void* pOnDmg = MethodPointer(api.GetMethod(g_statusClass, "OnDamage", 4));
    void* pOnMag = MethodPointer(api.GetMethod(g_statusClass, "OnMagicDamage", 4));
    void* pUpdate = MethodPointer(api.GetMethod(g_statusClass, "Update", 0));
    g_death = (Death_t)MethodPointer(api.GetMethod(g_statusClass, "Death", 0));

    if (!pPhys) pPhys = getAbsoluteAddress(GameUnity::TARGET_LIB, "0x162E6AC");
    if (!pMagic) pMagic = getAbsoluteAddress(GameUnity::TARGET_LIB, "0x16309B0");
    if (!pBoth) pBoth = getAbsoluteAddress(GameUnity::TARGET_LIB, "0x162E48C");
    if (!pOnDmg) pOnDmg = getAbsoluteAddress(GameUnity::TARGET_LIB, "0x162E950");
    if (!pOnMag) pOnMag = getAbsoluteAddress(GameUnity::TARGET_LIB, "0x162F9C0");
    if (!pUpdate) pUpdate = getAbsoluteAddress(GameUnity::TARGET_LIB, "0x163456C");
    if (!g_death) g_death = (Death_t)getAbsoluteAddress(GameUnity::TARGET_LIB, "0x1630D4C");

    g_agentSetSpeed = (AgentSetSpeed_t)getAbsoluteAddress(GameUnity::TARGET_LIB, "0x2B7F8A8");
    g_animSetSpeed = (AnimSetSpeed_t)getAbsoluteAddress(GameUnity::TARGET_LIB, "0x2B98CCC");

    if (!pPhys || !pUpdate) {
        LOGE("[Wzym] missing core ptrs phys=%p update=%p", pPhys, pUpdate);
        return false;
    }

    if (!HookOne("UpPhysicalDamage", pPhys, (void*)Hooked_UpPhysicalDamage, (void**)&g_origUpPhys))
        return false;
    HookOne("UpMagicDamage", pMagic, (void*)Hooked_UpMagicDamage, (void**)&g_origUpMagic);
    HookOne("UpBothDamage", pBoth, (void*)Hooked_UpBothDamage, (void**)&g_origUpBoth);
    HookOne("OnDamage", pOnDmg, (void*)Hooked_OnDamage, (void**)&g_origOnDamage);
    HookOne("OnMagicDamage", pOnMag, (void*)Hooked_OnMagicDamage, (void**)&g_origOnMagic);
    if (!HookOne("Update", pUpdate, (void*)Hooked_Update, (void**)&g_origUpdate))
        return false;

    g_ready.store(true);
    LOGI("[Wzym] init ok Death=%p", (void*)g_death);
    return true;
}

bool Wzym_IsReady() { return g_ready.load(); }

void Wzym_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == WZYM_FEAT_INVINCIBLE) g_inv.store(enabled);
    else if (featNum == WZYM_FEAT_AOE_KILL) g_aoe.store(enabled);
    (void)enabled;
}

void Wzym_OnValueChanged(int featNum, int value) {
    if (featNum == WZYM_FEAT_DMG_MUL) {
        if (value < 1) value = 1;
        if (value > 999) value = 999;
        g_dmgMul.store(value);
        return;
    }
    if (featNum == WZYM_FEAT_SPEED) {
        if (value < 1) value = 1;
        if (value > 5) value = 5;
        g_speedLvl.store(value);
    }
}

void Wzym_ForceClear() {
    g_inv.store(false);
    g_aoe.store(false);
    g_dmgMul.store(1);
    g_speedLvl.store(1);
}

void Wzym_Tick(const bool* features) {
    if (!features || !g_ready.load()) return;
    g_inv.store(features[WZYM_FEAT_INVINCIBLE]);
    g_aoe.store(features[WZYM_FEAT_AOE_KILL]);
    g_speedTick.fetch_add(1, std::memory_order_relaxed);
}
