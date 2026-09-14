/* ================================================================
 *  追迹迷城 1.0.20260912 / vc98503
 *  RVA 来自官方包 Il2CppDumper script.json（2026-09-14 静态重定位）。
 *  原四功能 13 个 HookOne 语义/签名不变：BattleManager +0x11520、
 *  Attack/Partner +0x11564、LocalPlayer +0x10EB8、Monster +0x110DC。
 *  新增免广告：ATGameManager.GetFreeSkipAdsRemainingCount 开启时恒返回 3，
 *  复用官方免费跳过广告分支，不伪造广告 SDK 回调。
 *  ELF bias +0x4000（RX LOAD vaddr-file = 0x4000，新版实测不变），运行时用 maps file_off。
 * ================================================================ */

#include "ZjmcMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"

enum : uintptr_t {
    ELF_BIAS = 0x4000,
    RVA_BM_UPDATE            = 0x3040214,  // FixedUpdate（AOE 用；Update=0x3040718）
    RVA_BM_GET_ALIVE         = 0x3044788,
    RVA_ATK_CAN_USE          = 0x306C1C8,
    RVA_ATK_CUR_SP           = 0x306C1E4,
    RVA_PARTNER_CAN_USE      = 0x306C61C,
    RVA_PARTNER_CUR_SP       = 0x306C650,
    RVA_LP_CHECK_1           = 0x3111E48,
    RVA_LP_CHECK_2           = 0x3111EF0,
    RVA_LP_CHECK_DODGE       = 0x3111F1C,
    RVA_LP_PROCESS_DMG       = 0x3112354,  // LocalPlayer$$ProcessDamageOrHeal
    RVA_LP_CHANGE_HP         = 0x3113F68,  // LocalPlayer$$ChangeHP（非 DisplayClass 闭包）
    RVA_LP_CHECK_ACTION      = 0x31156AC,
    RVA_MONSTER_PROCESS_DMG  = 0x311B360,  // Monster$$ProcessDamageOrHeal
    RVA_MONSTER_CHANGE_HP    = 0x311BE7C,
    RVA_GET_FREE_SKIP_ADS    = 0x2DCFD24,  // ATGameManager$$GetFreeSkipAdsRemainingCount
};

static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_seckill{false};
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_aoe{false};
static std::atomic<bool> g_nocd{false};
static std::atomic<bool> g_noad{false};
static std::atomic<bool> g_aoeReenter{false};

typedef float (*ChangeHP_t)(void* thiz, float hp, bool showText, void* method);
typedef bool (*BoolFn_t)(void* thiz, void* method);
typedef void (*Update_t)(void* thiz, void* method);
typedef void* (*GetList_t)(void* thiz, void* method);
typedef bool (*ProcessDmg_t)(void* thiz, void* attacker, void* resultData, void* method);
typedef int (*IntReturnFn_t)(void* thiz, void* method);

static ChangeHP_t g_origMonsterHp = nullptr;
static ChangeHP_t g_origPlayerHp = nullptr;
static ChangeHP_t g_monsterHpAbs = nullptr;
static Update_t g_origBmUpdate = nullptr;
static GetList_t g_getAlive = nullptr;
static ProcessDmg_t g_origMonsterPdh = nullptr;
static ProcessDmg_t g_origPlayerPdh = nullptr;

static BoolFn_t g_origAtkCan = nullptr;
static BoolFn_t g_origAtkSp = nullptr;
static BoolFn_t g_origPartnerCan = nullptr;
static BoolFn_t g_origPartnerSp = nullptr;
static BoolFn_t g_origLp1 = nullptr;
static BoolFn_t g_origLp2 = nullptr;
static BoolFn_t g_origLpDodge = nullptr;
static BoolFn_t g_origLpAction = nullptr;
static IntReturnFn_t g_origFreeSkipAds = nullptr;

/** maps：file_off → VA。libil2cpp RX 可能分段，禁止 ELF base+RVA。 */
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

static void* Abs(uintptr_t rva) {
    void* va = FileOffToVa(GameUnity::TARGET_LIB, rva - ELF_BIAS);
    if (va) return va;
    char buf[32];
    snprintf(buf, sizeof(buf), "0x%lX", (unsigned long)rva);
    return getAbsoluteAddress(GameUnity::TARGET_LIB, buf);
}

static bool HookOne(const char* tag, uintptr_t rva, void* hook, void** orig, bool required) {
    void* addr = Abs(rva);
    if (!addr) {
        LOGE("[Zjmc] %s resolve failed (0x%lX)", tag, (unsigned long)rva);
        return !required;
    }
    if (*orig) return true;
    if (DobbyHook(addr, hook, orig) != 0) {
        LOGE("[Zjmc] DobbyHook %s failed @ %p", tag, addr);
        return !required;
    }
    LOGI("[Zjmc] hooked %s @ %p (0x%lX)", tag, addr, (unsigned long)rva);
    return true;
}

static float Hooked_MonsterChangeHP(void* thiz, float hp, bool showText, void* method) {
    if (g_seckill.load() && hp < 0.f) {
        hp = -999999.f;
    }
    return g_origMonsterHp ? g_origMonsterHp(thiz, hp, showText, method) : hp;
}

static float Hooked_PlayerChangeHP(void* thiz, float hp, bool showText, void* method) {
    if (g_inv.load() && hp < 0.f) {
        hp = 0.f;
    }
    return g_origPlayerHp ? g_origPlayerHp(thiz, hp, showText, method) : hp;
}

/** 普攻常走 ProcessDamageOrHeal，不一定先到 ChangeHP；秒杀在此补一刀。 */
static bool Hooked_MonsterPdh(void* thiz, void* attacker, void* resultData, void* method) {
    if (g_seckill.load() && g_origMonsterHp) {
        g_origMonsterHp(thiz, -999999.f, false, nullptr);
        return true;
    }
    return g_origMonsterPdh ? g_origMonsterPdh(thiz, attacker, resultData, method) : true;
}

/** 无敌：拒收伤害处理（ChangeHP 钩仍作兜底）。 */
static bool Hooked_PlayerPdh(void* thiz, void* attacker, void* resultData, void* method) {
    if (g_inv.load()) {
        return false;
    }
    return g_origPlayerPdh ? g_origPlayerPdh(thiz, attacker, resultData, method) : true;
}

/* 无限技能：闸门开时强制 true，不调 orig（语义就是放行，0.9 已验）。 */
static bool PassTrue(BoolFn_t orig, void* thiz, void* method) {
    if (g_nocd.load()) return true;
    return orig ? orig(thiz, method) : true;
}

static bool Hooked_AtkCan(void* thiz, void* method) { return PassTrue(g_origAtkCan, thiz, method); }
static bool Hooked_AtkSp(void* thiz, void* method) { return PassTrue(g_origAtkSp, thiz, method); }
static bool Hooked_PartnerCan(void* thiz, void* method) { return PassTrue(g_origPartnerCan, thiz, method); }
static bool Hooked_PartnerSp(void* thiz, void* method) { return PassTrue(g_origPartnerSp, thiz, method); }
static bool Hooked_Lp1(void* thiz, void* method) { return PassTrue(g_origLp1, thiz, method); }
static bool Hooked_Lp2(void* thiz, void* method) { return PassTrue(g_origLp2, thiz, method); }
static bool Hooked_LpDodge(void* thiz, void* method) { return PassTrue(g_origLpDodge, thiz, method); }
static bool Hooked_LpAction(void* thiz, void* method) { return PassTrue(g_origLpAction, thiz, method); }

/** 免广告：走游戏官方 FREE_SKIP_ADS_DAILY_MAX=3 分支，开关开启时不消耗实际额度。 */
static int Hooked_FreeSkipAds(void* thiz, void* method) {
    if (g_noad.load()) return 3;
    return g_origFreeSkipAds ? g_origFreeSkipAds(thiz, method) : 0;
}

/** Il2Cpp List`1：_items@0x10, _size@0x18；Array max_length@0x18, data@0x20 */
static void AoeKillFromList(void* list) {
    if (!list || !g_origMonsterHp) return;
    void** items = *reinterpret_cast<void***>(reinterpret_cast<uintptr_t>(list) + 0x10);
    int32_t size = *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(list) + 0x18);
    if (!items || size <= 0 || size > 512) return;
    void** vec = reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(items) + 0x20);
    for (int i = 0; i < size; i++) {
        void* mon = vec[i];
        if (!mon) continue;
        /* 走 orig，避免再进 Hooked_MonsterChangeHP / Update 重入 */
        g_origMonsterHp(mon, -999999.f, false, nullptr);
    }
}

static void Hooked_BmUpdate(void* thiz, void* method) {
    if (g_aoe.load() && g_getAlive && g_origMonsterHp && !g_aoeReenter.load()) {
        g_aoeReenter.store(true);
        void* list = g_getAlive(thiz, nullptr);
        AoeKillFromList(list);
        g_aoeReenter.store(false);
    }
    if (g_origBmUpdate) g_origBmUpdate(thiz, method);
}

bool Zjmc_TryInit() {
    if (g_ready.load()) return true;
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) return false;

    g_monsterHpAbs = reinterpret_cast<ChangeHP_t>(Abs(RVA_MONSTER_CHANGE_HP));
    g_getAlive = reinterpret_cast<GetList_t>(Abs(RVA_BM_GET_ALIVE));

    bool ok = true;
    ok &= HookOne("Monster.ChangeHP", RVA_MONSTER_CHANGE_HP, (void*)Hooked_MonsterChangeHP,
                  (void**)&g_origMonsterHp, true);
    ok &= HookOne("LocalPlayer.ChangeHP", RVA_LP_CHANGE_HP, (void*)Hooked_PlayerChangeHP,
                  (void**)&g_origPlayerHp, true);
    ok &= HookOne("Monster.PDH", RVA_MONSTER_PROCESS_DMG, (void*)Hooked_MonsterPdh,
                  (void**)&g_origMonsterPdh, true);
    ok &= HookOne("LocalPlayer.PDH", RVA_LP_PROCESS_DMG, (void*)Hooked_PlayerPdh,
                  (void**)&g_origPlayerPdh, true);
    ok &= HookOne("BattleManager.Update", RVA_BM_UPDATE, (void*)Hooked_BmUpdate,
                  (void**)&g_origBmUpdate, true);
    ok &= HookOne("Atk.CanUse", RVA_ATK_CAN_USE, (void*)Hooked_AtkCan, (void**)&g_origAtkCan, true);
    ok &= HookOne("Atk.CurSp", RVA_ATK_CUR_SP, (void*)Hooked_AtkSp, (void**)&g_origAtkSp, true);
    ok &= HookOne("Partner.CanUse", RVA_PARTNER_CAN_USE, (void*)Hooked_PartnerCan,
                  (void**)&g_origPartnerCan, true);
    ok &= HookOne("Partner.CurSp", RVA_PARTNER_CUR_SP, (void*)Hooked_PartnerSp,
                  (void**)&g_origPartnerSp, true);
    ok &= HookOne("LP.Check1", RVA_LP_CHECK_1, (void*)Hooked_Lp1, (void**)&g_origLp1, true);
    ok &= HookOne("LP.Check2", RVA_LP_CHECK_2, (void*)Hooked_Lp2, (void**)&g_origLp2, true);
    /* Dodge 是 B 到 ActionState 的 4 字节跳板，Dobby 可能失败；Action 已覆盖 */
    HookOne("LP.Dodge", RVA_LP_CHECK_DODGE, (void*)Hooked_LpDodge, (void**)&g_origLpDodge, false);
    ok &= HookOne("LP.Action", RVA_LP_CHECK_ACTION, (void*)Hooked_LpAction, (void**)&g_origLpAction,
                  true);
    HookOne("AT.GetFreeSkipAds", RVA_GET_FREE_SKIP_ADS, (void*)Hooked_FreeSkipAds,
            (void**)&g_origFreeSkipAds, false);
 
    if (ok) {
        g_ready.store(true);
        LOGI("[Zjmc] 初始化成功");
    }
    return ok;
}

bool Zjmc_IsReady() { return g_ready.load(); }

void Zjmc_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case ZJMC_FEAT_SECKILL: g_seckill.store(enabled); break;
        case ZJMC_FEAT_INVINCIBLE: g_inv.store(enabled); break;
        case ZJMC_FEAT_AOE_KILL: g_aoe.store(enabled); break;
        case ZJMC_FEAT_NO_CD: g_nocd.store(enabled); break;
        case ZJMC_FEAT_NO_AD: g_noad.store(enabled); break;
        default: break;
    }
    LOGI("[Zjmc] feat %d -> %d", featNum, enabled ? 1 : 0);
}

void Zjmc_OnValueChanged(int featNum, int value) {
    (void)featNum;
    (void)value;
}

void Zjmc_Tick(const bool* features) {
    if (!features) return;
    g_seckill.store(features[ZJMC_FEAT_SECKILL]);
    g_inv.store(features[ZJMC_FEAT_INVINCIBLE]);
    g_aoe.store(features[ZJMC_FEAT_AOE_KILL]);
    g_nocd.store(features[ZJMC_FEAT_NO_CD]);
    g_noad.store(features[ZJMC_FEAT_NO_AD]);
}

void Zjmc_ForceClear() {
    g_seckill.store(false);
    g_inv.store(false);
    g_aoe.store(false);
    g_nocd.store(false);
    g_noad.store(false);
    LOGI("[Zjmc] ForceClear");
}
