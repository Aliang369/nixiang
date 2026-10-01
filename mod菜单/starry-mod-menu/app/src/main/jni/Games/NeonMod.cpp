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
 *    ⑥ 远程攻速 0x292B190 GunGraph.<AttackRoutineWithEvents>d__46::MoveNext
 *        [枪图＝真节奏] 逐帧 timer([sm+0x58]) -= deltaTime，归零后
 *          timer = graph.output.FireDelay；FireDelay(0x3A8F3F4) = 60.0f / RPM
 *          → 缩小新赋值的 timer（帧内判「突然变大」），只影响玩家（[sm+0x50]
 *            ＝<neonPlayerState>5__2 非空才算玩家路径，怪物走别的协程）
 *        [旧点保留] 0x292AB08 Gun.<shootingInterval>d__96::MoveNext
 *          2026-10-02 实测该协程在真机武器上未被启动（日志零命中）：
 *          玩家枪由枪械蓝图 GunGraph 驱动，shootingInterval 无直接调用者
 *          （protected virtual Slot:10），故仅作后备。
 *    ⑤' 近战攻速（主）＝ 同一支枪图协程 d__46：d__46 的反汇编里分支条件是
 *        GunGraph.firing(0x90)!=0 && GGEvents.meleeEvent(0x38)!=0 → 走近战，
 *        yield GunGraph.meleeDelegate::Invoke()(0x292B4E0)；否则走 GunOutput::Attack。
 *        两者共用同一个 <timer>5__3（初值 = GunOutput::get_FireDelay = 60/RPM）
 *        ⇒ 按 meleeEvent 是否为空选近战/远程倍率，机制与已验证的远程完全一致。
 *        Gun.<MeleeCheck>d__85 那条协程保留为后备（Gun::MeleeCheck() 0x3960168
 *        实测 0 个 BL 调用者，与 shootingInterval 同病，疑似不参与实战）。
 *
 *  对局内资源（2026-10-02 第二轮，dump.cs + 反汇编双向核对）：
 *    ⑦ 无限金币   NEONPlayerState.UseCoin(0x289295C) / HasCoin(0x28928EC)
 *                 / GetCoinNum(0x28952AC)；数值字段 Coin(PlayerStateInt 0x30)
 *    ⑧ 无限钥匙   NEONPlayerState.UseKey(0x2894094) / HasKey()(0x28921C4)
 *                 / HasKey(int)(0x2893FB0)；数值字段 Key(PlayerStateInt 0x380)
 *    ⑨ 无限彩色钥匙 NEONPlayerState.UseGameplayKey(0x2894EC8)
 *                 / HasGameplayKey(0x28951C4)；GameplayKey 不是 PlayerStateInt，
 *                 而是属性(attrid)：HasGameplayKey 走
 *                 Global::get_PlayerState→[+0x48]→EhiEntity::GetAttri；
 *                 拾取物 SpecialKeyPowerUp::PickItUp(0x33E49B8) 唯一调用
 *                 NEONPlayerState::AddGameplayKey ⇒ 彩色钥匙 = GameplayKey
 *    ⇒ 三项都只做「消耗变空操作 + 够不够恒真」，不写数值字段（零残留）
 *
 *  技能无CD（2026-10-02 实测失效后加强）：
 *    CanUseSkill 在 dump 里共 35 个实现（全部派生自 SecSkillPowerUp），
 *    旧版只挂了基类那一处 → 各角色改写的 override 直接 return false 时
 *    绕不过去。现在一次性全挂，并同时把该实例的
 *    CoolingTime(0x25C)/LastCoolingTime(0x260) 清零后强制返回 true。
 * ================================================================ */

#include "NeonMod.h"

#include <atomic>
#include <cstddef>
#include <cstdint>
#include <cstdio>
#include <utility>

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
    RVA_SEC_COOL_TIME    = 0x2FBCA54,
    RVA_SEC_CUR_COOL     = 0x2FBCAAC,
    RVA_SEC_SKILL_BEGIN  = 0x2FB6654,  // SecSkillPowerUp::SkillBegin(bool)（探针）
    RVA_SEC_SKILL_END    = 0x2FB70A4,  // SecSkillPowerUp::SkillEnd()（探针）

    RVA_DICT_MOVE_SPEED  = 0x5D7511C,
    RVA_PLAYER_SPEED_MF  = 0x2669740,
    RVA_COMP_MOVE_SPEED  = 0x2645A20,
    RVA_GET_MAX_SPEED    = 0x264590C,

    // 武器攻速（2026-10-02 dump.cs + 反汇编核对）
    RVA_SHOOT_INTERVAL_MN = 0x292AB08,  // Gun.<shootingInterval>d__96::MoveNext（后备）
    RVA_MELEE_CHECK_MN    = 0x2929E78,  // Gun.<MeleeCheck>d__85::MoveNext
    RVA_GRAPH_ATTACK_MN   = 0x292B190,  // GunGraph.<AttackRoutineWithEvents>d__46::MoveNext

    // 对局内资源（NEONPlayerState，2026-10-02 dump.cs 行号→RVA 核对）
    RVA_PS_USE_COIN       = 0x289295C,  // void UseCoin(int amount)
    RVA_PS_HAS_COIN       = 0x28928EC,  // bool HasCoin(int amount)
    RVA_PS_GET_COIN       = 0x28952AC,  // int  GetCoinNum()
    RVA_PS_USE_KEY        = 0x2894094,  // void UseKey()
    RVA_PS_HAS_KEY        = 0x28921C4,  // bool HasKey()
    RVA_PS_HAS_KEY_N      = 0x2893FB0,  // bool HasKey(int needCount)
    RVA_PS_USE_GKEY       = 0x2894EC8,  // void UseGameplayKey()
    RVA_PS_HAS_GKEY       = 0x28951C4,  // bool HasGameplayKey()
};

// 技能可用性门：dump.cs 里 CanUseSkill 共 35 个实现，全部派生自 SecSkillPowerUp
// （2026-10-02 由 dump.cs 生成；只挂基类会被各角色 override 的 return false 绕过）
struct NeonSkillGate { uintptr_t rva; const char* name; };
static const NeonSkillGate kSkillGates[] = {
    { 0x2CBC0CC, "ManaSecSkillPowerUp" },
    { 0x2CE6BA4, "Nezha_Active_001" },
    { 0x2E91ACC, "Buro_Active_001" },
    { 0x2ED95D8, "MinionRevivePowerUp" },
    { 0x2FB6418, "SecSkillBlink" },
    { 0x2FB6498, "SecSkillPowerUp" },
    { 0x2FB68C0, "SecSkillBuildLauncherPlatform" },
    { 0x2FB6B04, "SecSkillBuyEgg" },
    { 0x2FB77B0, "SecSkillDropKey" },
    { 0x2FB7C40, "SecSkillFishing" },
    { 0x2FB8264, "SecSkillGamble" },
    { 0x2FB8D4C, "SecSkillGoldenEgg" },
    { 0x2FB978C, "SecSkillGunReroll" },
    { 0x2FB9AB8, "SecSkillHeartToCoin" },
    { 0x2FBA940, "SecSkillHoldObject" },
    { 0x2FBB820, "SecSkillPlaceMinePowerUp" },
    { 0x2FBD450, "SecSkillPowerupReloot" },
    { 0x2FBDB3C, "SecSkillProtectiveShellEnergy" },
    { 0x2FBE78C, "SecSkillRoll" },
    { 0x2FBEBA4, "SecSkillSelfExplode" },
    { 0x2FC094C, "SecSkillSmashdown" },
    { 0x2FC2BB0, "SecSkillTeleportToTarget" },
    { 0x3137450, "Lin_Active_001" },
    { 0x31652C4, "LuKaSi_Active_001" },
    { 0x32CA110, "IncreaseGunGraphStatPower" },
    { 0x34D3E58, "SecSkillAUnlock" },
    { 0x34D46F8, "SecSkillBackOff" },
    { 0x34D4820, "SecSkillBalloon" },
    { 0x34D4C1C, "SecSkillBlankBomb" },
    { 0x353EBC0, "Hacker_Active_001" },
    { 0x35BA378, "SiShe_Active_001" },
    { 0x3606CBC, "Shell_Active_001" },
    { 0x3C4BE60, "Neon_Active_001" },
    { 0x3D55FC8, "Icarus_Active_001" },
    { 0x3DCEAD4, "ZhaKe_SP_Active_001" },
};
enum : size_t { NEON_SKILL_GATE_COUNT = sizeof(kSkillGates) / sizeof(kSkillGates[0]) };

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

    // GunGraph.<AttackRoutineWithEvents>d__46 状态机布局
    OFF_SM_NEON_PS     = 0x50,  // private NEONPlayerState <neonPlayerState>5__2
    OFF_SM_GRAPH_TIMER = 0x58,  // private float <timer>5__3（射击间隔倒计时）
    OFF_SM_MELEE_EVENT = 0x38,  // GunGraphEvents.meleeEvent（0x38 处 struct 的首字段；非空=近战图）

    // SecSkillPowerUp 冷却字段（35 个门共用同一基类布局）
    OFF_SEC_COOLING_TIME = 0x25C,
    OFF_SEC_LAST_COOL    = 0x260,
};

static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_invincible{false};
static std::atomic<bool> g_seckill{false};
static std::atomic<bool> g_noCd{false};
static std::atomic<int>  g_moveLvl{1};
static std::atomic<int>  g_meleeLvl{1};   // 近战攻速档位 1..5（1 = 关）
static std::atomic<int>  g_rangedLvl{1};  // 远程攻速档位 1..5（1 = 关）
static std::atomic<bool> g_infCoin{false};  // 无限金币
static std::atomic<bool> g_infKey{false};   // 无限钥匙
static std::atomic<bool> g_infGkey{false};  // 无限彩色钥匙（GameplayKey）

typedef bool (*DeductHP_t)(void* thiz, void* damage, int32_t count, void* method);
typedef void (*DoDeduct_t)(void* thiz, void* damage, void* method);
typedef void* (*BuildPass_t)(void* thiz, void* method);
typedef bool (*IsSkillCD_t)(void* thiz, void* skillKey, void* method);
typedef void (*SetSkillCD_t)(void* thiz, void* skillKey, float countTime, void* method);
typedef float (*GetRemainCD_t)(void* thiz, void* skillKey, void* method);
typedef bool (*CanUseSkill_t)(void* thiz, void* method);
typedef void (*SkillBegin_t)(void* thiz, bool isSecSkill, void* method);
typedef void (*SkillEnd_t)(void* thiz, void* method);
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
static CanUseSkill_t g_origSkillGate[NEON_SKILL_GATE_COUNT] = {};
static SkillBegin_t g_origSkillBegin = nullptr;
static SkillEnd_t g_origSkillEnd = nullptr;
static CoolFn_t g_origSecCool = nullptr;
static CoolFn_t g_origSecCurCool = nullptr;
static DictMove_t g_origDictMove = nullptr;
static SpeedFn_t g_origPlayerSpeedMf = nullptr;
static SpeedFn_t g_origCompMove = nullptr;
static SpeedFn_t g_origMaxSpeed = nullptr;
static MoveNext_t g_origShootIntervalMn = nullptr;
static MoveNext_t g_origMeleeCheckMn = nullptr;
static MoveNext_t g_origGraphAttackMn = nullptr;

// 对局内资源（NEONPlayerState 成员方法；x0=this，尾参 = MethodInfo*）
typedef void    (*UseCoin_t)(void* thiz, int32_t amount, void* method);
typedef bool    (*HasCoin_t)(void* thiz, int32_t amount, void* method);
typedef int32_t (*GetCoinNum_t)(void* thiz, void* method);
typedef void    (*UseKey_t)(void* thiz, void* method);
typedef bool    (*HasKey0_t)(void* thiz, void* method);
typedef bool    (*HasKeyN_t)(void* thiz, int32_t needCount, void* method);
typedef void    (*UseGKey_t)(void* thiz, void* method);
typedef bool    (*HasGKey_t)(void* thiz, void* method);

static UseCoin_t   g_origUseCoin = nullptr;
static HasCoin_t   g_origHasCoin = nullptr;
static GetCoinNum_t g_origGetCoin = nullptr;
static UseKey_t    g_origUseKey = nullptr;
static HasKey0_t   g_origHasKey0 = nullptr;
static HasKeyN_t   g_origHasKeyN = nullptr;
static UseGKey_t   g_origUseGKey = nullptr;
static HasGKey_t   g_origHasGKey = nullptr;

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

// 静默版（技能门有 35 处，逐个打日志会淹没 logcat）
static bool HookOneQ(uintptr_t rva, void* hook, void** orig) {
    void* addr = Abs(rva);
    if (!addr) return false;
    if (*orig) return true;
    return DobbyHook(addr, hook, orig) == 0;
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

// ---- 技能门：35 个 CanUseSkill 实现，逐实例清零冷却字段后强制可用 ----
template <int N>
static bool Hooked_SkillGate(void* thiz, void* method) {
    if (g_noCd.load()) {
        if (thiz) {
            auto p = reinterpret_cast<uintptr_t>(thiz);
            *reinterpret_cast<float*>(p + OFF_SEC_COOLING_TIME) = 0.f;
            *reinterpret_cast<float*>(p + OFF_SEC_LAST_COOL) = 0.f;
        }
        static std::atomic<unsigned> n{0};
        if ((n.fetch_add(1) % 40u) == 0u)
            LOGI("[Neon] 技能无CD: %s.CanUseSkill -> true", kSkillGates[N].name);
        return true;
    }
    return g_origSkillGate[N] ? g_origSkillGate[N](thiz, method) : false;
}

template <std::size_t... I>
static int InstallSkillGates(std::index_sequence<I...>) {
    int ok = 0;
    ((HookOneQ(kSkillGates[I].rva, (void*)&Hooked_SkillGate<(int)I>,
               (void**)&g_origSkillGate[I]) ? ++ok : 0), ...);
    return ok;
}

// 探针：技能是否真的开始/结束（排障用，只打日志不改行为）
static void Hooked_SkillBegin(void* thiz, bool isSecSkill, void* method) {
    static std::atomic<unsigned> n{0};
    if ((n.fetch_add(1) % 4u) == 0u)
        LOGI("[Neon] probe SkillBegin thiz=%p isSec=%d noCd=%d",
             thiz, isSecSkill ? 1 : 0, g_noCd.load() ? 1 : 0);
    if (g_origSkillBegin) g_origSkillBegin(thiz, isSecSkill, method);
}

static void Hooked_SkillEnd(void* thiz, void* method) {
    if (g_origSkillEnd) g_origSkillEnd(thiz, method);
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
//  仅动「协程本地等待量」，不写 Gun/GunOutput 的持久字段
//  ⇒ 失效即恢复原值，随时可零残留关掉
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

// 通用槽位表：按「状态机对象指针」记账，判「数值突然变大 = 新赋值」后缩放一次
template <int MAX>
struct NeonSlots {
    struct Slot { void* sm; float prev; };
    Slot tab[MAX]{};
    int Find(void* sm) {
        int freeIdx = -1;
        for (int i = 0; i < MAX; ++i) {
            if (tab[i].sm == sm) return i;
            if (!tab[i].sm && freeIdx < 0) freeIdx = i;
        }
        if (freeIdx >= 0) { tab[freeIdx].sm = sm; tab[freeIdx].prev = 0.f; }
        return freeIdx;   // -1 = 槽满：宁可本次不生效，也不做会连乘的抢占
    }
};

static unsigned g_speedLogN = 0;

// ⑤ 近战攻速：MeleeCheck 每次挥砍后把「冷却」写进 <timer>5__2，逐帧减 deltaTime。
static NeonSlots<16> g_meleeSlots;

static bool Hooked_MeleeCheckMoveNext(void* self, void* method) {
    bool r = g_origMeleeCheckMn ? g_origMeleeCheckMn(self, method) : false;
    float mul = LvlMul(g_meleeLvl.load());
    if (!self || mul <= 1.01f) return r;
    if (!PlayerGunOf(self, nullptr)) return r;

    int idx = g_meleeSlots.Find(self);
    if (idx < 0) return r;

    float* timer = reinterpret_cast<float*>(reinterpret_cast<uintptr_t>(self) + OFF_SM_TIMER);
    float t = *timer;
    if (t > g_meleeSlots.tab[idx].prev + 0.0001f && t > 0.0001f && t < 60.f) {
        float nt = t / mul;
        *timer = nt;
        g_meleeSlots.tab[idx].prev = nt;
        if ((++g_speedLogN % 20u) == 1u)
            LOGI("[Neon] 近战攻速 x%.2f CD %.3f->%.3f", mul, t, nt);
    } else {
        g_meleeSlots.tab[idx].prev = t;
    }
    return r;
}

// ⑤⑥ 武器攻速（主）：枪械蓝图协程 <timer>5__3，初值 = graph.output.FireDelay(60/RPM)，
//     逐帧减 deltaTime，归零即出刀/开枪。缩放新赋值的 timer ＝ 整条节奏 ×mul。
//     玩家门控：[sm+0x50]<neonPlayerState>5__2 非空才算玩家路径。
//     近战/远程分流：[sm+0x38]＝GunGraphEvents.meleeEvent；反汇编里 d__46 正是用
//     `if (firing!=0 && meleeEvent!=null) → yield meleeDelegate::Invoke()` 走近战，
//     否则走 GunOutput::Attack ⇒ 该指针是否为空就是官方自己的分流条件。
static NeonSlots<24> g_graphSlots;

static bool Hooked_GraphAttackMoveNext(void* self, void* method) {
    bool r = g_origGraphAttackMn ? g_origGraphAttackMn(self, method) : false;
    if (!self) return r;
    if (!*reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(self) + OFF_SM_NEON_PS)) return r;

    const bool isMelee = *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(self) +
                                                   OFF_SM_MELEE_EVENT) != nullptr;

    const float mul = LvlMul(isMelee ? g_meleeLvl.load() : g_rangedLvl.load());
    if (mul <= 1.01f) {
        // 诊断：滑块没开时也记一笔，用来确认近战武器是否真走枪图
        static std::atomic<unsigned> diag{0};
        if (isMelee && (diag.fetch_add(1) % 200u) == 0u)
            LOGI("[Neon] 枪图近战分支命中(未加速) meleeLvl=%d rangedLvl=%d",
                 g_meleeLvl.load(), g_rangedLvl.load());
        return r;
    }

    int idx = g_graphSlots.Find(self);
    if (idx < 0) return r;

    float* timer = reinterpret_cast<float*>(reinterpret_cast<uintptr_t>(self) + OFF_SM_GRAPH_TIMER);
    float t = *timer;
    if (t > g_graphSlots.tab[idx].prev + 0.0001f && t > 0.0001f && t < 60.f) {
        float nt = t / mul;
        *timer = nt;
        g_graphSlots.tab[idx].prev = nt;
        if ((++g_speedLogN % 20u) == 1u) {
            if (isMelee)
                LOGI("[Neon] 近战攻速(枪图) x%.2f 挥砍间隔 %.3f->%.3f", mul, t, nt);
            else
                LOGI("[Neon] 远程攻速(枪图) x%.2f 间隔 %.3f->%.3f", mul, t, nt);
        }
    } else {
        g_graphSlots.tab[idx].prev = t;
    }
    return r;
}

// ⑥b 远程攻速（后备）：shootingInterval 唯一 yield = WaitForSeconds(60/RPM)。
//     实测该协程在玩家武器上未被启动，留着以防个别武器走这条老路。
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
            LOGI("[Neon] 远程攻速(协程) x%.2f 间隔 %.3f->%.3f", mul, v, v / mul);
    }
    return r;
}

// ================================================================
//  ⑦⑧⑨ 对局内资源：金币 / 钥匙 / 彩色钥匙（2026-10-02）
//  只拦「消耗」+ 让「够不够」恒真 ⇒ 用不完；不改数值字段，关掉即恢复
// ================================================================
static void Hooked_UseCoin(void* thiz, int32_t amount, void* method) {
    if (g_infCoin.load()) {
        static std::atomic<unsigned> n{0};
        if ((n.fetch_add(1) % 40u) == 0u) LOGI("[Neon] 无限金币: UseCoin(%d) 已拦截", amount);
        return;
    }
    if (g_origUseCoin) g_origUseCoin(thiz, amount, method);
}

static bool Hooked_HasCoin(void* thiz, int32_t amount, void* method) {
    if (g_infCoin.load()) {
        static std::atomic<unsigned> n{0};
        if ((n.fetch_add(1) % 40u) == 0u) LOGI("[Neon] 无限金币: HasCoin(%d) -> true", amount);
        return true;
    }
    return g_origHasCoin ? g_origHasCoin(thiz, amount, method) : false;
}

static int32_t Hooked_GetCoinNum(void* thiz, void* method) {
    if (g_infCoin.load()) return 99999;
    return g_origGetCoin ? g_origGetCoin(thiz, method) : 0;
}

static void Hooked_UseKey(void* thiz, void* method) {
    if (g_infKey.load()) {
        static std::atomic<unsigned> n{0};
        if ((n.fetch_add(1) % 20u) == 0u) LOGI("[Neon] 无限钥匙: UseKey() 已拦截");
        return;
    }
    if (g_origUseKey) g_origUseKey(thiz, method);
}

static bool Hooked_HasKey0(void* thiz, void* method) {
    if (g_infKey.load()) return true;
    return g_origHasKey0 ? g_origHasKey0(thiz, method) : false;
}

static bool Hooked_HasKeyN(void* thiz, int32_t needCount, void* method) {
    if (g_infKey.load()) {
        static std::atomic<unsigned> n{0};
        if ((n.fetch_add(1) % 40u) == 0u)
            LOGI("[Neon] 无限钥匙: HasKey(%d) -> true", needCount);
        return true;
    }
    return g_origHasKeyN ? g_origHasKeyN(thiz, needCount, method) : false;
}

static void Hooked_UseGKey(void* thiz, void* method) {
    if (g_infGkey.load()) {
        static std::atomic<unsigned> n{0};
        if ((n.fetch_add(1) % 20u) == 0u) LOGI("[Neon] 无限彩色钥匙: UseGameplayKey() 已拦截");
        return;
    }
    if (g_origUseGKey) g_origUseGKey(thiz, method);
}

static bool Hooked_HasGKey(void* thiz, void* method) {
    if (g_infGkey.load()) {
        static std::atomic<unsigned> n{0};
        if ((n.fetch_add(1) % 40u) == 0u) LOGI("[Neon] 无限彩色钥匙: HasGameplayKey() -> true");
        return true;
    }
    return g_origHasGKey ? g_origHasGKey(thiz, method) : false;
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
    HookOne("SecCoolTime", RVA_SEC_COOL_TIME, (void*)Hooked_SecCoolTime, (void**)&g_origSecCool);
    HookOne("SecCurCool", RVA_SEC_CUR_COOL, (void*)Hooked_SecCurCool, (void**)&g_origSecCurCool);
    HookOne("SecSkillBegin(probe)", RVA_SEC_SKILL_BEGIN, (void*)Hooked_SkillBegin, (void**)&g_origSkillBegin);
    HookOne("SecSkillEnd(probe)", RVA_SEC_SKILL_END, (void*)Hooked_SkillEnd, (void**)&g_origSkillEnd);

    // 技能门：35 处 CanUseSkill 全挂（旧版只挂基类那一处，被 override 绕过）
    int gateOk = InstallSkillGates(std::make_index_sequence<NEON_SKILL_GATE_COUNT>{});
    LOGI("[Neon] skill gates hooked %d/%d", gateOk, (int)NEON_SKILL_GATE_COUNT);

    HookOne("Dict_MoveSpeed", RVA_DICT_MOVE_SPEED, (void*)Hooked_DictMoveSpeed, (void**)&g_origDictMove);
    HookOne("PlayerSpeedMf", RVA_PLAYER_SPEED_MF, (void*)Hooked_PlayerSpeedMf, (void**)&g_origPlayerSpeedMf);
    HookOne("CompMoveSpeed", RVA_COMP_MOVE_SPEED, (void*)Hooked_CompMoveSpeed, (void**)&g_origCompMove);
    HookOne("GetMaxSpeed", RVA_GET_MAX_SPEED, (void*)Hooked_GetMaxSpeed, (void**)&g_origMaxSpeed);

    // 新增：武器攻速（挂不上不影响上面 4 项，只丢新功能）
    HookOne("GunGraph.AttackRoutineWithEvents.MoveNext", RVA_GRAPH_ATTACK_MN,
            (void*)Hooked_GraphAttackMoveNext, (void**)&g_origGraphAttackMn);
    HookOne("Gun.shootingInterval.MoveNext", RVA_SHOOT_INTERVAL_MN,
            (void*)Hooked_ShootIntervalMoveNext, (void**)&g_origShootIntervalMn);
    HookOne("Gun.MeleeCheck.MoveNext", RVA_MELEE_CHECK_MN,
            (void*)Hooked_MeleeCheckMoveNext, (void**)&g_origMeleeCheckMn);

    // 对局内资源（挂不上不影响战斗项）
    HookOne("NEONPlayerState.UseCoin", RVA_PS_USE_COIN, (void*)Hooked_UseCoin, (void**)&g_origUseCoin);
    HookOne("NEONPlayerState.HasCoin", RVA_PS_HAS_COIN, (void*)Hooked_HasCoin, (void**)&g_origHasCoin);
    HookOne("NEONPlayerState.GetCoinNum", RVA_PS_GET_COIN, (void*)Hooked_GetCoinNum, (void**)&g_origGetCoin);
    HookOne("NEONPlayerState.UseKey", RVA_PS_USE_KEY, (void*)Hooked_UseKey, (void**)&g_origUseKey);
    HookOne("NEONPlayerState.HasKey", RVA_PS_HAS_KEY, (void*)Hooked_HasKey0, (void**)&g_origHasKey0);
    HookOne("NEONPlayerState.HasKey(n)", RVA_PS_HAS_KEY_N, (void*)Hooked_HasKeyN, (void**)&g_origHasKeyN);
    HookOne("NEONPlayerState.UseGameplayKey", RVA_PS_USE_GKEY, (void*)Hooked_UseGKey, (void**)&g_origUseGKey);
    HookOne("NEONPlayerState.HasGameplayKey", RVA_PS_HAS_GKEY, (void*)Hooked_HasGKey, (void**)&g_origHasGKey);

    g_ready.store(true);
    LOGI("[Neon] init ok combat cd=%d move=%d ranged=%d rangedgraph=%d melee=%d coin=%d key=%d gkey=%d",
         g_origIsSkillCD ? 1 : 0, g_origDictMove ? 1 : 0,
         g_origShootIntervalMn ? 1 : 0, g_origGraphAttackMn ? 1 : 0,
         g_origMeleeCheckMn ? 1 : 0,
         g_origUseCoin ? 1 : 0, g_origUseKey ? 1 : 0, g_origUseGKey ? 1 : 0);
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
        case NEON_FEAT_INF_COIN:
            g_infCoin.store(enabled);
            LOGI("[Neon] 无限金币=%s", enabled ? "ON" : "OFF");
            break;
        case NEON_FEAT_INF_KEY:
            g_infKey.store(enabled);
            LOGI("[Neon] 无限钥匙=%s", enabled ? "ON" : "OFF");
            break;
        case NEON_FEAT_INF_GKEY:
            g_infGkey.store(enabled);
            LOGI("[Neon] 无限彩色钥匙=%s", enabled ? "ON" : "OFF");
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
    g_infCoin.store(false);
    g_infKey.store(false);
    g_infGkey.store(false);
}

void Neon_Tick(const bool* features, const int* values) {
    if (!features) return;
    g_invincible.store(features[NEON_FEAT_INVINCIBLE]);
    g_seckill.store(features[NEON_FEAT_SECKILL]);
    g_noCd.store(features[NEON_FEAT_NO_CD]);
    g_infCoin.store(features[NEON_FEAT_INF_COIN]);
    g_infKey.store(features[NEON_FEAT_INF_KEY]);
    g_infGkey.store(features[NEON_FEAT_INF_GKEY]);
    if (values) {
        // 滑块值每轮由框架同步（未动过时 = 下限 1 = 关）
        g_moveLvl.store(ClampLvl(values[NEON_FEAT_MOVE_SPEED]));
        g_meleeLvl.store(ClampLvl(values[NEON_FEAT_MELEE_SPEED]));
        g_rangedLvl.store(ClampLvl(values[NEON_FEAT_RANGED_SPEED]));
    }
    if (!g_ready.load()) Neon_TryInit();
}
