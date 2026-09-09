/* ================================================================
 *  霓虹深渊 3.2.860 — 仅战斗（便利功能已移除）
 *  无敌 / 秒杀 / 技能无CD / 移速
 *  RVA 一律 dump.cs；禁止 Il2Cpp::Api::Load；禁止非主线程调 timeScale
 * ================================================================ */

#include "NeonMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"

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
};

enum : size_t {
    OFF_HP_CUR        = 0x13C,
    OFF_HEARTS_INV    = 0x148,
    OFF_HEARTS_UNKILL = 0x160,
    OFF_DMG_DAMAGE    = 0x10,
    OFF_DMG_FINAL     = 0x18,
    OFF_DMG_BASE      = 0x1C,
};

static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_invincible{false};
static std::atomic<bool> g_seckill{false};
static std::atomic<bool> g_noCd{false};
static std::atomic<int>  g_moveLvl{1};

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

static void* Abs(uintptr_t rva) {
    char buf[32];
    snprintf(buf, sizeof(buf), "0x%lX", (unsigned long)rva);
    return getAbsoluteAddress(GameUnity::TARGET_LIB, buf);
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

static float MoveMul() {
    switch (g_moveLvl.load()) {
        case 2: return 1.5f;
        case 3: return 2.0f;
        case 4: return 3.0f;
        case 5: return 4.0f;
        default: return 1.0f;
    }
}

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

bool Neon_TryInit() {
    if (g_ready.load()) return true;
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) return false;

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

    g_ready.store(true);
    LOGI("[Neon] init ok combat cd=%d move=%d",
         g_origIsSkillCD ? 1 : 0, g_origDictMove ? 1 : 0);
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
        default:
            break;
    }
}

void Neon_OnValueChanged(int featNum, int value) {
    if (featNum != NEON_FEAT_MOVE_SPEED) return;
    if (value < 1) value = 1;
    if (value > 5) value = 5;
    g_moveLvl.store(value);
    LOGI("[Neon] 移速 lvl=%d mul=%.1f", value, MoveMul());
}

void Neon_ForceClear() {
    g_invincible.store(false);
    g_seckill.store(false);
    g_noCd.store(false);
    g_moveLvl.store(1);
}

void Neon_Tick(const bool* features) {
    if (!features) return;
    g_invincible.store(features[NEON_FEAT_INVINCIBLE]);
    g_seckill.store(features[NEON_FEAT_SECKILL]);
    g_noCd.store(features[NEON_FEAT_NO_CD]);
    if (!g_ready.load()) Neon_TryInit();
}
