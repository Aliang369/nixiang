/* ================================================================
 *  霓虹深渊 3.2.860 — 仅战斗（便利功能已移除）
 *  无敌 / 秒杀 / 技能无CD / 移速 / 近战攻速 / 远程攻速
 *  RVA 一律 dump.cs；禁止 Il2Cpp::Api::Load；禁止非主线程调 timeScale
 *
 *  2026-10-02 迁移 modern 注册表：库名改走 sec_lib_il2cpp()（不再依赖
 *  GameConfig.h，那是 legacy 配置中心）。地址解析机制不变（file_off）。
 *
 *  新增两项攻速（逆向证据，ARM64 反汇编 + dump.cs 双向核对）：
 *    ⑤ 近战攻速 0x2929E78 Gun.<MeleeCheck>d__85::MoveNext
 *        挥砍后 timer = [Gun+0x370]BaseMeleeCD + [Gun+0x374]ExtraMeleeCD
 *        或 [Gun+0x30]kanonBaseMeleeCd + [Gun+0x374]，写回 [sm+0x28]<timer>5__2
 *        → 直接缩小该 timer（两路都覆盖，不必动 Gun 字段）
 *    ⑥ 远程攻速 0x292AB08 Gun.<shootingInterval>d__96::MoveNext
 *        射击间隔 = 60.0f / Gun.RPM(Gun+0x1A8)，产出 WaitForSeconds
 *        → 该协程唯一 yield 对象，缩 m_Seconds(obj+0x10) 即等倍提速
 *    两者都先用 GunBase.InstigatorIsPlayer(Gun+0xC0) 过滤玩家武器，
 *    避免连带加速怪物/召唤物的同类协程。
 * ================================================================ */

#include "NeonMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/SecStrings.hpp"
#include "Includes/Utils.hpp"

// 目标库名（注册表模式不依赖 GameConfig.h）
#define NEON_LIB sec_lib_il2cpp()

enum : uintptr_t {
    RVA_HITPOINTS_DEDUCT = 0x346D8C4,
    RVA_HITPOINTS_FIX    = 0x346B738,
    RVA_HITPOINTS_DO     = 0x346DE04,
    RVA_HEARTS_DEDUCT    = 0x3545ED0,
    RVA_BUILD_PASS       = 0x3676B3C,
    RVA_BUILD_PASS_BASE  = 0x3676C64,

    RVA_IS_SKILL_CD      = 0x35C01A8,
    RVA_SET_SKILL_CD     = 0x35BFF90,
    RVA_GET_REMAIN_CD    = 0x35C0270,
    RVA_SEC_CAN_USE      = 0x2FB6498,
    RVA_SEC_COOL_TIME    = 0x2FBCA54,
    RVA_SEC_CUR_COOL     = 0x2FBCAAC,

    RVA_DICT_MOVE_SPEED  = 0x5D7511C,
    RVA_PLAYER_SPEED_MF  = 0x2669740,
    RVA_COMP_MOVE_SPEED  = 0x2645A20,
    RVA_GET_MAX_SPEED    = 0x264590C,

    // 武器攻速（2026-10-02 dump.cs 反汇编核对：两处都是 MoveNext 状态机）
    RVA_SHOOT_INTERVAL_MN = 0x292AB08,  // Gun.<shootingInterval>d__96::MoveNext
    RVA_MELEE_CHECK_MN    = 0x2929E78,  // Gun.<MeleeCheck>d__85::MoveNext
};

enum : size_t {
    OFF_HP_CUR        = 0x13C,
    OFF_HEARTS_INV    = 0x148,
    OFF_HEARTS_UNKILL = 0x160,
    OFF_DMG_DAMAGE    = 0x10,
    OFF_DMG_FINAL     = 0x18,
    OFF_DMG_BASE      = 0x1C,

    // 攻速协程状态机 / 玩家武器过滤
    OFF_SM_GUN        = 0x20,  // <>4__this（Gun*）
    OFF_SM_CURRENT    = 0x18,  // <>2__current（yield 对象）
    OFF_SM_TIMER      = 0x28,  // <MeleeCheck>d__85 的 <timer>5__2（挥砍冷却）
    OFF_WFS_SECONDS   = 0x10,  // UnityEngine.WaitForSeconds.m_Seconds
    OFF_INSTIGATOR_IS_PLAYER = 0xC0,  // GunBase.InstigatorIsPlayer
};

static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_invincible{false};
static std::atomic<bool> g_seckill{false};
static std::atomic<bool> g_noCd{false};
static std::atomic<int>  g_moveLvl{1};
static std::atomic<int>  g_meleeLvl{1};   // 近战攻速档位 1..5（1 = 关）
static std::atomic<int>  g_rangedLvl{1};  // 远程攻速档位 1..5（1 = 关）

typedef bool (*DeductHP_t)(void* thiz, void* damage, int32_t count, void* method);
typedef void (*DoDeduct_t)(void* thiz, void* damage, void* method);
typedef void* (*BuildPass_t)(void* thiz, void* method);
typedef bool (*IsSkillCD_t)(void* thiz, void* skillKey, void* method);
typedef void (*SetSkillCD_t)(void* thiz, void* skillKey, float countTime, void* method);
typedef float (*GetRemainCD_t)(void* thiz, void* skillKey, void* method);
typedef bool (*CanUseSkill_t)(void* thiz, void* method);
typedef float (*CoolFn_t)(void* thiz, void* method);
typedef double (*DictMove_t)(void* inst, void* method);
typedef float (*SpeedFn_t)(void* thiz, void* method);
typedef bool (*MoveNext_t)(void* self, void* method);  // IEnumerator.MoveNext

static DeductHP_t g_origHitDeduct = nullptr;
static DeductHP_t g_origHitFix = nullptr;
static DoDeduct_t g_origDoDeduct = nullptr;
static DeductHP_t g_origHeartDeduct = nullptr;
static BuildPass_t g_origBuildPass = nullptr;
static BuildPass_t g_buildPassBase = nullptr;
static IsSkillCD_t g_origIsSkillCD = nullptr;
static SetSkillCD_t g_origSetSkillCD = nullptr;
static GetRemainCD_t g_origGetRemainCD = nullptr;
static CanUseSkill_t g_origSecCanUse = nullptr;
static CoolFn_t g_origSecCool = nullptr;
static CoolFn_t g_origSecCurCool = nullptr;
static DictMove_t g_origDictMove = nullptr;
static SpeedFn_t g_origPlayerSpeedMf = nullptr;
static SpeedFn_t g_origCompMove = nullptr;
static SpeedFn_t g_origMaxSpeed = nullptr;
static MoveNext_t g_origShootIntervalMn = nullptr;
static MoveNext_t g_origMeleeCheckMn = nullptr;

static void* Abs(uintptr_t rva) {
    char buf[32];
    snprintf(buf, sizeof(buf), "0x%lX", (unsigned long)rva);
    return getAbsoluteAddress(NEON_LIB, buf);
}

static bool HookOne(const char* tag, uintptr_t rva, void* hook, void** orig) {
    void* addr = Abs(rva);
    if (!addr) {
        LOGE("[Neon] %s resolve failed (0x%lX)", tag, (unsigned long)rva);
        return false;
    }
    if (*orig) return true;
    if (DobbyHook(addr, hook, orig) != 0) {
        LOGE("[Neon] DobbyHook %s failed @ %p", tag, addr);
        return false;
    }
    LOGI("[Neon] hooked %s @ %p (0x%lX)", tag, addr, (unsigned long)rva);
    return true;
}

// 档位 → 倍率（与移速同表；1 = 关）
static float LvlMul(int lvl) {
    switch (lvl) {
        case 2: return 1.5f;
        case 3: return 2.0f;
        case 4: return 3.0f;
        case 5: return 4.0f;
        default: return 1.0f;
    }
}

static float MoveMul() { return LvlMul(g_moveLvl.load()); }

static void AmplifyDamage(void* damage) {
    if (!damage) return;
    auto p = reinterpret_cast<uintptr_t>(damage);
    *reinterpret_cast<float*>(p + OFF_DMG_DAMAGE) = 999999.f;
    *reinterpret_cast<float*>(p + OFF_DMG_FINAL) = 999999.f;
    *reinterpret_cast<float*>(p + OFF_DMG_BASE) = 999999.f;
}

static void ForceKillHp(void* thiz) {
    if (!thiz) return;
    *reinterpret_cast<float*>(reinterpret_cast<uintptr_t>(thiz) + OFF_HP_CUR) = 0.f;
}

static bool Hooked_HitPoints_DeductHP(void* thiz, void* damage, int32_t count, void* method) {
    if (g_seckill.load()) {
        AmplifyDamage(damage);
        ForceKillHp(thiz);
    }
    return g_origHitDeduct ? g_origHitDeduct(thiz, damage, count, method) : false;
}

static bool Hooked_HitPoints_FixDeduct(void* thiz, void* damage, int32_t count, void* method) {
    if (g_seckill.load()) AmplifyDamage(damage);
    return g_origHitFix ? g_origHitFix(thiz, damage, count, method) : false;
}

static void Hooked_HitPoints_DoDeduct(void* thiz, void* damage, void* method) {
    if (g_seckill.load()) {
        AmplifyDamage(damage);
        ForceKillHp(thiz);
    }
    if (g_origDoDeduct) g_origDoDeduct(thiz, damage, method);
}

static bool Hooked_Hearts_DeductHP(void* thiz, void* damage, int32_t count, void* method) {
    if (g_invincible.load()) {
        if (thiz) {
            *reinterpret_cast<uint8_t*>(reinterpret_cast<uintptr_t>(thiz) + OFF_HEARTS_INV) = 1;
            *reinterpret_cast<uint8_t*>(reinterpret_cast<uintptr_t>(thiz) + OFF_HEARTS_UNKILL) = 1;
        }
        return false;
    }
    return g_origHeartDeduct ? g_origHeartDeduct(thiz, damage, count, method) : false;
}

static void* Hooked_BuildPassCheckInfo(void* thiz, void* method) {
    if ((g_invincible.load() || g_seckill.load()) && g_buildPassBase)
        return g_buildPassBase(thiz, method);
    return g_origBuildPass ? g_origBuildPass(thiz, method) : nullptr;
}

static bool Hooked_IsSkillCD(void* thiz, void* key, void* method) {
    if (g_noCd.load()) return false;
    return g_origIsSkillCD ? g_origIsSkillCD(thiz, key, method) : false;
}

static void Hooked_SetSkillCD(void* thiz, void* key, float countTime, void* method) {
    if (g_noCd.load()) {
        if (g_origSetSkillCD) g_origSetSkillCD(thiz, key, 0.f, method);
        return;
    }
    if (g_origSetSkillCD) g_origSetSkillCD(thiz, key, countTime, method);
}

static float Hooked_GetRemainCD(void* thiz, void* key, void* method) {
    if (g_noCd.load()) return 0.f;
    return g_origGetRemainCD ? g_origGetRemainCD(thiz, key, method) : 0.f;
}

static bool Hooked_SecCanUse(void* thiz, void* method) {
    if (g_noCd.load()) return true;
    return g_origSecCanUse ? g_origSecCanUse(thiz, method) : false;
}

static float Hooked_SecCoolTime(void* thiz, void* method) {
    if (g_noCd.load()) return 0.01f;
    return g_origSecCool ? g_origSecCool(thiz, method) : 0.f;
}

static float Hooked_SecCurCool(void* thiz, void* method) {
    if (g_noCd.load()) return 0.f;
    return g_origSecCurCool ? g_origSecCurCool(thiz, method) : 0.f;
}

static double Hooked_DictMoveSpeed(void* inst, void* method) {
    double v = g_origDictMove ? g_origDictMove(inst, method) : 1.0;
    float mul = MoveMul();
    if (mul > 1.01f) return v * (double)mul;
    return v;
}

static float Hooked_PlayerSpeedMf(void* thiz, void* method) {
    float v = g_origPlayerSpeedMf ? g_origPlayerSpeedMf(thiz, method) : 1.f;
    float mul = MoveMul();
    if (mul > 1.01f) return v * mul;
    return v;
}

static float Hooked_CompMoveSpeed(void* thiz, void* method) {
    float v = g_origCompMove ? g_origCompMove(thiz, method) : 0.f;
    float mul = MoveMul();
    if (mul > 1.01f) return v * mul;
    return v;
}

static float Hooked_GetMaxSpeed(void* thiz, void* method) {
    float v = g_origMaxSpeed ? g_origMaxSpeed(thiz, method) : 0.f;
    float mul = MoveMul();
    if (mul > 1.01f) return v * mul;
    return v;
}

// ================================================================
//  武器攻速（2026-10-02）
//  仅动「协程本地等待量」，不写 Gun 的持久字段 ⇒ 随时可零残留关掉
// ================================================================

// 取状态机的 <>4__this（Gun）并校验是玩家武器（InstigatorIsPlayer）
static bool PlayerGunOf(void* sm, void** outGun) {
    void* gun = nullptr;
    if (sm) gun = *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(sm) + OFF_SM_GUN);
    if (outGun) *outGun = gun;
    if (!gun) return false;
    return *reinterpret_cast<uint8_t*>(reinterpret_cast<uintptr_t>(gun) +
                                      OFF_INSTIGATOR_IS_PLAYER) != 0;
}

static unsigned g_speedLogN = 0;

// ⑤ 近战攻速：MeleeCheck 每次挥砍后把「冷却」写进 <timer>5__2，逐帧减 deltaTime。
//    这里用「timer 突然变大(=新赋值)」判定，缩放一次；多把武器(主/副)各自记账。
struct NeonMeleeSlot { void* sm; float prev; };
static NeonMeleeSlot g_meleeSlots[16];

static bool Hooked_MeleeCheckMoveNext(void* self, void* method) {
    bool r = g_origMeleeCheckMn ? g_origMeleeCheckMn(self, method) : false;
    float mul = LvlMul(g_meleeLvl.load());
    if (!self || mul <= 1.01f) return r;
    if (!PlayerGunOf(self, nullptr)) return r;

    int idx = -1, freeIdx = -1;
    for (int i = 0; i < 16; ++i) {
        if (g_meleeSlots[i].sm == self) { idx = i; break; }
        if (!g_meleeSlots[i].sm && freeIdx < 0) freeIdx = i;
    }
    if (idx < 0) {
        if (freeIdx < 0) return r;   // 槽满：宁可本次不生效，也不做会连乘的抢占
        idx = freeIdx;
        g_meleeSlots[idx].sm = self;
        g_meleeSlots[idx].prev = 0.f;
    }

    float* timer = reinterpret_cast<float*>(reinterpret_cast<uintptr_t>(self) + OFF_SM_TIMER);
    float t = *timer;
    if (t > g_meleeSlots[idx].prev + 0.0001f && t > 0.0001f && t < 60.f) {
        float nt = t / mul;
        *timer = nt;
        g_meleeSlots[idx].prev = nt;
        if ((++g_speedLogN % 20u) == 1u)
            LOGI("[Neon] 近战攻速 x%.2f CD %.3f->%.3f", mul, t, nt);
    } else {
        g_meleeSlots[idx].prev = t;
    }
    return r;
}

// ⑥ 远程攻速：shootingInterval 唯一 yield = WaitForSeconds(60/RPM)。
//    缩该对象 m_Seconds ⇒ 等效 RPM × mul，且只影响这一发/这条协程。
static bool Hooked_ShootIntervalMoveNext(void* self, void* method) {
    bool r = g_origShootIntervalMn ? g_origShootIntervalMn(self, method) : false;
    float mul = LvlMul(g_rangedLvl.load());
    if (!r || !self || mul <= 1.01f) return r;
    if (!PlayerGunOf(self, nullptr)) return r;

    void* cur = *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(self) + OFF_SM_CURRENT);
    if (!cur) return r;
    float* sec = reinterpret_cast<float*>(reinterpret_cast<uintptr_t>(cur) + OFF_WFS_SECONDS);
    float v = *sec;
    if (v > 0.0001f && v < 60.f) {
        *sec = v / mul;
        if ((++g_speedLogN % 20u) == 1u)
            LOGI("[Neon] 远程攻速 x%.2f 间隔 %.3f->%.3f", mul, v, v / mul);
    }
    return r;
}

bool Neon_TryInit(JavaVM* vm) {
    (void)vm;
    if (g_ready.load()) return true;
    if (!isLibraryLoaded(NEON_LIB)) return false;

    LOGI("[Neon] init combat-only (3.2.860)");
    g_buildPassBase = reinterpret_cast<BuildPass_t>(Abs(RVA_BUILD_PASS_BASE));

    if (!HookOne("HitPoints.DeductHP", RVA_HITPOINTS_DEDUCT,
                 (void*)Hooked_HitPoints_DeductHP, (void**)&g_origHitDeduct))
        return false;
    HookOne("HitPoints.FixDeduct", RVA_HITPOINTS_FIX,
            (void*)Hooked_HitPoints_FixDeduct, (void**)&g_origHitFix);
    HookOne("HitPoints.DoDeduct", RVA_HITPOINTS_DO,
            (void*)Hooked_HitPoints_DoDeduct, (void**)&g_origDoDeduct);
    if (!HookOne("Hearts.DeductHP", RVA_HEARTS_DEDUCT,
                 (void*)Hooked_Hearts_DeductHP, (void**)&g_origHeartDeduct))
        return false;
    if (!HookOne("BuildPassCheckInfo", RVA_BUILD_PASS,
                 (void*)Hooked_BuildPassCheckInfo, (void**)&g_origBuildPass))
        return false;

    HookOne("IsSkillCD", RVA_IS_SKILL_CD, (void*)Hooked_IsSkillCD, (void**)&g_origIsSkillCD);
    HookOne("SetSkillCD", RVA_SET_SKILL_CD, (void*)Hooked_SetSkillCD, (void**)&g_origSetSkillCD);
    HookOne("GetRemainCD", RVA_GET_REMAIN_CD, (void*)Hooked_GetRemainCD, (void**)&g_origGetRemainCD);
    HookOne("SecCanUse", RVA_SEC_CAN_USE, (void*)Hooked_SecCanUse, (void**)&g_origSecCanUse);
    HookOne("SecCoolTime", RVA_SEC_COOL_TIME, (void*)Hooked_SecCoolTime, (void**)&g_origSecCool);
    HookOne("SecCurCool", RVA_SEC_CUR_COOL, (void*)Hooked_SecCurCool, (void**)&g_origSecCurCool);

    HookOne("Dict_MoveSpeed", RVA_DICT_MOVE_SPEED, (void*)Hooked_DictMoveSpeed, (void**)&g_origDictMove);
    HookOne("PlayerSpeedMf", RVA_PLAYER_SPEED_MF, (void*)Hooked_PlayerSpeedMf, (void**)&g_origPlayerSpeedMf);
    HookOne("CompMoveSpeed", RVA_COMP_MOVE_SPEED, (void*)Hooked_CompMoveSpeed, (void**)&g_origCompMove);
    HookOne("GetMaxSpeed", RVA_GET_MAX_SPEED, (void*)Hooked_GetMaxSpeed, (void**)&g_origMaxSpeed);

    // 新增：武器攻速（挂不上不影响上面 4 项，只丢新功能）
    HookOne("Gun.shootingInterval.MoveNext", RVA_SHOOT_INTERVAL_MN,
            (void*)Hooked_ShootIntervalMoveNext, (void**)&g_origShootIntervalMn);
    HookOne("Gun.MeleeCheck.MoveNext", RVA_MELEE_CHECK_MN,
            (void*)Hooked_MeleeCheckMoveNext, (void**)&g_origMeleeCheckMn);

    g_ready.store(true);
    LOGI("[Neon] init ok combat cd=%d move=%d ranged=%d melee=%d",
         g_origIsSkillCD ? 1 : 0, g_origDictMove ? 1 : 0,
         g_origShootIntervalMn ? 1 : 0, g_origMeleeCheckMn ? 1 : 0);
    return true;
}

bool Neon_IsReady() { return g_ready.load(); }

void Neon_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case NEON_FEAT_INVINCIBLE:
            g_invincible.store(enabled);
            LOGI("[Neon] 无敌=%s", enabled ? "ON" : "OFF");
            break;
        case NEON_FEAT_SECKILL:
            g_seckill.store(enabled);
            LOGI("[Neon] 秒杀=%s", enabled ? "ON" : "OFF");
            break;
        case NEON_FEAT_NO_CD:
            g_noCd.store(enabled);
            LOGI("[Neon] 技能无CD=%s", enabled ? "ON" : "OFF");
            break;
        case NEON_FEAT_MOVE_SPEED:
            if (!enabled) g_moveLvl.store(1);
            break;
        case NEON_FEAT_MELEE_SPEED:
            if (!enabled) g_meleeLvl.store(1);
            break;
        case NEON_FEAT_RANGED_SPEED:
            if (!enabled) g_rangedLvl.store(1);
            break;
        default:
            break;
    }
}

static int ClampLvl(int v) {
    if (v < 1) return 1;
    if (v > 5) return 5;
    return v;
}

void Neon_OnValueChanged(int featNum, int value) {
    if (featNum == NEON_FEAT_MOVE_SPEED) {
        g_moveLvl.store(ClampLvl(value));
        LOGI("[Neon] 移速 lvl=%d mul=%.1f", ClampLvl(value), MoveMul());
        return;
    }
    if (featNum == NEON_FEAT_MELEE_SPEED) {
        int lvl = ClampLvl(value);
        g_meleeLvl.store(lvl);
        LOGI("[Neon] 近战攻速 lvl=%d mul=%.1f", lvl, LvlMul(lvl));
        return;
    }
    if (featNum == NEON_FEAT_RANGED_SPEED) {
        int lvl = ClampLvl(value);
        g_rangedLvl.store(lvl);
        LOGI("[Neon] 远程攻速 lvl=%d mul=%.1f", lvl, LvlMul(lvl));
        return;
    }
}

void Neon_ForceClear() {
    g_invincible.store(false);
    g_seckill.store(false);
    g_noCd.store(false);
    g_moveLvl.store(1);
    g_meleeLvl.store(1);
    g_rangedLvl.store(1);
}

void Neon_Tick(const bool* features, const int* values) {
    if (!features) return;
    g_invincible.store(features[NEON_FEAT_INVINCIBLE]);
    g_seckill.store(features[NEON_FEAT_SECKILL]);
    g_noCd.store(features[NEON_FEAT_NO_CD]);
    if (values) {
        // 滑块值每轮由框架同步（未动过时 = 下限 1 = 关）
        g_moveLvl.store(ClampLvl(values[NEON_FEAT_MOVE_SPEED]));
        g_meleeLvl.store(ClampLvl(values[NEON_FEAT_MELEE_SPEED]));
        g_rangedLvl.store(ClampLvl(values[NEON_FEAT_RANGED_SPEED]));
    }
    if (!g_ready.load()) Neon_TryInit();
}
