/* ================================================================
 *  伯吉的温馨厨房 1.0.1 — 金币 / 广告券
 *  硬规则：OnFeatureChanged 只改开关标志，禁止 native 调 IL2CPP / 碰 UI
 *  STARRY_TRACE=1 → adb logcat -s StarryTrace StarryMagic:V
 * ================================================================ */

#include "BjdwMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"

#if defined(STARRY_TRACE)
#include <android/log.h>
#define BJDW_TRACE(...) \
    __android_log_print(ANDROID_LOG_INFO, "StarryTrace", "[Bjdw] " __VA_ARGS__)
#else
#define BJDW_TRACE(...) ((void)0)
#endif

enum : uintptr_t {
    RVA_GET_CURRENT_COINS           = 0x16C05C8,
    RVA_SET_CURRENT_COINS           = 0x16C05D0,
    RVA_ADD_COINS                   = 0x16C087C,
    RVA_SUBTRACT_COINS              = 0x16C0A04,
    RVA_UNLOCKABLE_AWAKE            = 0x16C05FC,
    RVA_GET_AD_COUPONS              = 0x1632CBC,
    RVA_CAN_CLAIM_WITHOUT_AD        = 0x1632D2C,
    RVA_TRY_REDEEM_AD_COUPON_COINS  = 0x163316C,
    RVA_TRY_CONSUME_AD_COUPON       = 0x16334E4,
    RVA_TRY_CONSUME_AD_COUPONS      = 0x16334EC,
    RVA_SHOP_CAN_AFFORD             = 0x16BFD60,
    RVA_SHOP_CONSUME_COST           = 0x16C0020,
    RVA_UPDATE_COINS_TEXT           = 0x16B1C7C,
    RVA_UI_COIN_TEXT_TWEEN_GET      = 0x16B2724,
    RVA_REFRESH_AD_COUPON_COUNT     = 0x16B6918,
    RVA_GET_SAVED_INT               = 0x16A3270,
    RVA_SAVE_NEW_VALUE_INT          = 0x16A3058,
};

enum SaveableData : int32_t {
    SAVE_CURRENT_COINS = 2,
    SAVE_AD_COUPONS    = 13,
};

enum ShopCurrency : int32_t {
    SHOP_CURRENCY_COINS     = 0,
    SHOP_CURRENCY_AD_COUPON = 1,
};

static constexpr int32_t kFakeCoins        = 999999;
static constexpr int32_t kFakeCoupons      = 9999;
static constexpr int32_t kCoinsPerAdCoupon = 50;
static constexpr size_t  kCoinsFieldOff       = 0x40;
static constexpr size_t  kShopCurrencyOff     = 0x24;
static constexpr size_t  kSaveSteamStorageOff = 0x20;
static constexpr size_t  kPrefsAdCouponsOff   = 0x44;

static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_coins{false};
static std::atomic<bool> g_adCoupon{false};
static std::atomic<bool> g_pendingCoinBoost{false};
static std::atomic<bool> g_coinBoostDone{false};

static void* g_unlockSys = nullptr;
static void* g_saveMgr = nullptr;

typedef int32_t (*GetCurrentCoins_t)(void* self, void* method);
typedef void (*SetCurrentCoins_t)(void* self, int32_t value, void* method);
typedef void (*AddCoins_t)(void* self, int32_t amount, void* method);
typedef void (*SubtractCoins_t)(void* self, int32_t amount, void* method);
typedef void (*Awake_t)(void* self, void* method);
typedef int32_t (*GetAdCoupons_t)(void* method);
typedef bool (*CanClaimWithoutAd_t)(void* method);
typedef bool (*TryRedeemAdCouponForCoins_t)(void* method);
typedef bool (*TryConsumeAdCoupon_t)(void* method);
typedef bool (*TryConsumeAdCoupons_t)(int32_t amount, void* method);
typedef bool (*ShopCanAfford_t)(void* self, void* method);
typedef void (*ShopConsumeCost_t)(void* self, void* method);
typedef void (*UpdateCoinsText_t)(void* self, int32_t newCoinValue, void* method);
typedef int32_t (*UiCoinTweenGet_t)(void* self, void* method);
typedef void (*RefreshAdCouponCount_t)(void* self, void* method);
typedef int32_t (*GetSavedInt_t)(void* self, int32_t saveableData, void* method);
typedef void (*SaveNewValueInt_t)(void* self, int32_t saveableData, int32_t value, void* method);

static GetCurrentCoins_t g_origGetCoins = nullptr;
static SetCurrentCoins_t g_origSetCoins = nullptr;
static AddCoins_t g_origAddCoins = nullptr;
static SubtractCoins_t g_origSubtract = nullptr;
static Awake_t g_origAwake = nullptr;
static GetAdCoupons_t g_origGetCoupons = nullptr;
static CanClaimWithoutAd_t g_origCanClaimWithoutAd = nullptr;
static TryRedeemAdCouponForCoins_t g_origTryRedeem = nullptr;
static TryConsumeAdCoupon_t g_origTryConsumeOne = nullptr;
static TryConsumeAdCoupons_t g_origTryConsumeN = nullptr;
static ShopCanAfford_t g_origCanAfford = nullptr;
static ShopConsumeCost_t g_origConsumeCost = nullptr;
static UpdateCoinsText_t g_origUpdateCoinsText = nullptr;
static UiCoinTweenGet_t g_origUiCoinTweenGet = nullptr;
static RefreshAdCouponCount_t g_origRefreshCoupons = nullptr;
static GetSavedInt_t g_origGetSavedInt = nullptr;
static SaveNewValueInt_t g_origSaveInt = nullptr;

#if defined(STARRY_TRACE)
static std::atomic<uint32_t> g_hitGetCoins{0};
static std::atomic<uint32_t> g_hitSetCoins{0};
static std::atomic<uint32_t> g_hitGetCoupons{0};
static std::atomic<uint32_t> g_hitGetSaved{0};
static std::atomic<uint32_t> g_hitUpdateText{0};
static std::atomic<uint32_t> g_hitRefreshCoupon{0};
static std::atomic<uint32_t> g_hitCanAfford{0};
static std::atomic<uint32_t> g_hitConsume{0};
static std::atomic<uint32_t> g_tickReport{0};

static void TraceHit(const char* tag, uint32_t n, const char* detail) {
    if (n <= 12 || (n % 30) == 0)
        BJDW_TRACE("%s hits=%u %s", tag, n, detail ? detail : "");
}
#endif

static void* Abs(uintptr_t rva) {
    char buf[32];
    snprintf(buf, sizeof(buf), "0x%lX", (unsigned long)rva);
    return getAbsoluteAddress(GameUnity::TARGET_LIB, buf);
}

static bool HookOne(const char* tag, uintptr_t rva, void* hook, void** orig) {
    void* addr = Abs(rva);
    if (!addr) {
        LOGE("[Bjdw] %s resolve failed (0x%lX)", tag, (unsigned long)rva);
        return false;
    }
    if (*orig) return true;
    if (DobbyHook(addr, hook, orig) != 0) {
        LOGE("[Bjdw] DobbyHook %s failed @ %p", tag, addr);
        return false;
    }
    LOGI("[Bjdw] hooked %s @ %p (0x%lX)", tag, addr, (unsigned long)rva);
    return true;
}

static int32_t ShopCurrencyOf(void* shopItem) {
    if (!shopItem) return -1;
    return *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(shopItem) + kShopCurrencyOff);
}

static void CacheUnlockSys(void* self) {
    if (self) g_unlockSys = self;
}

static void PatchCoinField(void* self) {
    if (!self) return;
    *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(self) + kCoinsFieldOff) = kFakeCoins;
}

/** 仅在 Tick / 游戏线程 hook 内调用，禁止 OnFeatureChanged 调用 */
static void BoostCoinsOnce() {
    if (!g_coins.load() || !g_pendingCoinBoost.load() || g_coinBoostDone.load()) return;
    if (!g_unlockSys) return;
    g_coinBoostDone.store(true);
    g_pendingCoinBoost.store(false);
    PatchCoinField(g_unlockSys);
    LOGI("[Bjdw] coin boost field -> %d (inst=%p)", kFakeCoins, g_unlockSys);
}

static void PatchPrefsAdCouponsMem() {
    if (!g_adCoupon.load() || !g_saveMgr) return;
    auto* storage = reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(g_saveMgr) + kSaveSteamStorageOff);
    void* prefs = storage ? *storage : nullptr;
    if (!prefs) return;
    auto* p = reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(prefs) + kPrefsAdCouponsOff);
    if (*p != kFakeCoupons) {
        *p = kFakeCoupons;
        LOGI("[Bjdw] prefs.adCoupons mem -> %d", kFakeCoupons);
    }
}

static int32_t Hooked_GetCurrentCoins(void* self, void* method) {
    CacheUnlockSys(self);
    const int32_t orig = g_origGetCoins ? g_origGetCoins(self, method) : 0;
#if defined(STARRY_TRACE)
    const uint32_t n = g_hitGetCoins.fetch_add(1) + 1;
    char buf[96];
    snprintf(buf, sizeof(buf), "orig=%d feat=%d ret=%d", orig, g_coins.load() ? 1 : 0,
             g_coins.load() ? kFakeCoins : orig);
    TraceHit("get_currentCoins", n, buf);
#endif
    if (g_coins.load()) {
        if (g_pendingCoinBoost.load() && !g_coinBoostDone.load()) BoostCoinsOnce();
        return kFakeCoins;
    }
    return orig;
}

static void Hooked_SetCurrentCoins(void* self, int32_t value, void* method) {
    CacheUnlockSys(self);
#if defined(STARRY_TRACE)
    const uint32_t n = g_hitSetCoins.fetch_add(1) + 1;
    char buf[64];
    snprintf(buf, sizeof(buf), "in=%d feat=%d", value, g_coins.load() ? 1 : 0);
    TraceHit("set_currentCoins", n, buf);
#endif
    if (g_coins.load()) value = kFakeCoins;
    if (g_origSetCoins) g_origSetCoins(self, value, method);
}

static void Hooked_AddCoins(void* self, int32_t amount, void* method) {
    CacheUnlockSys(self);
    if (g_coins.load()) {
        PatchCoinField(self);
        return;
    }
    if (g_origAddCoins) g_origAddCoins(self, amount, method);
}

static void Hooked_SubtractCoins(void* self, int32_t amount, void* method) {
    CacheUnlockSys(self);
    if (g_coins.load()) return;
    if (g_origSubtract) g_origSubtract(self, amount, method);
}

static void Hooked_UnlockableAwake(void* self, void* method) {
    CacheUnlockSys(self);
    if (g_origAwake) g_origAwake(self, method);
    if (g_coins.load()) {
        g_pendingCoinBoost.store(true);
        g_coinBoostDone.store(false);
        BoostCoinsOnce();
    }
}

static int32_t Hooked_GetAdCoupons(void* method) {
    const int32_t orig = g_origGetCoupons ? g_origGetCoupons(method) : 0;
#if defined(STARRY_TRACE)
    const uint32_t n = g_hitGetCoupons.fetch_add(1) + 1;
    char buf[80];
    snprintf(buf, sizeof(buf), "orig=%d feat=%d ret=%d", orig, g_adCoupon.load() ? 1 : 0,
             g_adCoupon.load() ? kFakeCoupons : orig);
    TraceHit("get_AdCoupons", n, buf);
#endif
    if (g_adCoupon.load()) {
        PatchPrefsAdCouponsMem();
        return kFakeCoupons;
    }
    return orig;
}

static bool Hooked_CanClaimWithoutAd(void* method) {
    if (g_adCoupon.load()) return true;
    return g_origCanClaimWithoutAd ? g_origCanClaimWithoutAd(method) : false;
}

static bool Hooked_TryRedeemAdCouponForCoins(void* method) {
    if (g_coins.load()) return true;
    if (g_adCoupon.load()) {
        if (g_unlockSys && g_origAddCoins)
            g_origAddCoins(g_unlockSys, kCoinsPerAdCoupon, method);
        return true;
    }
    return g_origTryRedeem ? g_origTryRedeem(method) : false;
}

static bool Hooked_TryConsumeAdCoupon(void* method) {
    if (g_adCoupon.load()) return true;
    return g_origTryConsumeOne ? g_origTryConsumeOne(method) : false;
}

static bool Hooked_TryConsumeAdCoupons(int32_t amount, void* method) {
    (void)amount;
    if (g_adCoupon.load()) return true;
    return g_origTryConsumeN ? g_origTryConsumeN(amount, method) : false;
}

static bool Hooked_ShopCanAfford(void* self, void* method) {
#if defined(STARRY_TRACE)
    const uint32_t n = g_hitCanAfford.fetch_add(1) + 1;
    char buf[48];
    snprintf(buf, sizeof(buf), "cur=%d", self ? ShopCurrencyOf(self) : -1);
    TraceHit("CanAfford", n, buf);
#endif
    if (self) {
        const int32_t cur = ShopCurrencyOf(self);
        if (g_coins.load() && cur == SHOP_CURRENCY_COINS) return true;
        if (g_adCoupon.load() && cur == SHOP_CURRENCY_AD_COUPON) return true;
    }
    return g_origCanAfford ? g_origCanAfford(self, method) : false;
}

static void Hooked_ShopConsumeCost(void* self, void* method) {
#if defined(STARRY_TRACE)
    const uint32_t n = g_hitConsume.fetch_add(1) + 1;
    char buf[48];
    snprintf(buf, sizeof(buf), "cur=%d skip=%d", self ? ShopCurrencyOf(self) : -1,
             (self && ((g_adCoupon.load() && ShopCurrencyOf(self) == SHOP_CURRENCY_AD_COUPON) ||
                       (g_coins.load() && ShopCurrencyOf(self) == SHOP_CURRENCY_COINS)))
                 ? 1
                 : 0);
    TraceHit("ConsumeCost", n, buf);
#endif
    if (self) {
        const int32_t cur = ShopCurrencyOf(self);
        if (g_adCoupon.load() && cur == SHOP_CURRENCY_AD_COUPON) return;
        if (g_coins.load() && cur == SHOP_CURRENCY_COINS) return;
    }
    if (g_origConsumeCost) g_origConsumeCost(self, method);
}

/** 被动显示：仅游戏线程回调内改参，禁止 OnFeatureChanged native 调 UI */
static void Hooked_UpdateCurrentCoinsText(void* self, int32_t newCoinValue, void* method) {
    if (g_coins.load()) newCoinValue = kFakeCoins;
#if defined(STARRY_TRACE)
    const uint32_t n = g_hitUpdateText.fetch_add(1) + 1;
    char buf[64];
    snprintf(buf, sizeof(buf), "show=%d ui=%p", newCoinValue, self);
    TraceHit("UpdateCurrentCoinsText", n, buf);
#endif
    if (g_origUpdateCoinsText) g_origUpdateCoinsText(self, newCoinValue, method);
}

/** DOTween 起始值 closure；Trace 证实 HUD 走 UpdateCurrentCoinsText 链 */
static int32_t Hooked_UiCoinTweenGet(void* self, void* method) {
    if (g_coins.load()) return kFakeCoins;
    return g_origUiCoinTweenGet ? g_origUiCoinTweenGet(self, method) : 0;
}

static void Hooked_RefreshAdCouponCount(void* self, void* method) {
#if defined(STARRY_TRACE)
    const uint32_t n = g_hitRefreshCoupon.fetch_add(1) + 1;
    const int32_t viaGet = g_origGetCoupons ? g_origGetCoupons(method) : -1;
    char buf[64];
    snprintf(buf, sizeof(buf), "ui=%p get_AdCoupons=%d", self, viaGet);
    TraceHit("RefreshAdCouponCount", n, buf);
#endif
    if (g_adCoupon.load()) PatchPrefsAdCouponsMem();
    if (g_origRefreshCoupons) g_origRefreshCoupons(self, method);
}

static int32_t Hooked_GetSavedInt(void* self, int32_t saveableData, void* method) {
    if (self) g_saveMgr = self;
    const int32_t orig = g_origGetSavedInt ? g_origGetSavedInt(self, saveableData, method) : 0;
#if defined(STARRY_TRACE)
    if (saveableData == SAVE_CURRENT_COINS || saveableData == SAVE_AD_COUPONS) {
        const uint32_t n = g_hitGetSaved.fetch_add(1) + 1;
        char buf[80];
        snprintf(buf, sizeof(buf), "key=%d orig=%d featC=%d featA=%d", saveableData, orig,
                 g_coins.load() ? 1 : 0, g_adCoupon.load() ? 1 : 0);
        TraceHit("GetSavedInt", n, buf);
    }
#endif
    if (saveableData == SAVE_CURRENT_COINS && g_coins.load()) return kFakeCoins;
    if (saveableData == SAVE_AD_COUPONS && g_adCoupon.load()) {
        PatchPrefsAdCouponsMem();
        return kFakeCoupons;
    }
    return orig;
}

static void Hooked_SaveNewValueInt(void* self, int32_t saveableData, int32_t value, void* method) {
    if (saveableData == SAVE_CURRENT_COINS && g_coins.load()) value = kFakeCoins;
    if (g_origSaveInt) g_origSaveInt(self, saveableData, value, method);
}

bool Bjdw_TryInit() {
    if (g_ready.load()) return true;
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) return false;

#if defined(STARRY_TRACE)
    LOGI("[Bjdw] init TRACE build");
#else
    LOGI("[Bjdw] init economy hooks (v12 passive-hud)");
#endif
    if (!HookOne("UnlockableSystem.get_currentCoins", RVA_GET_CURRENT_COINS,
                 (void*)Hooked_GetCurrentCoins, (void**)&g_origGetCoins))
        return false;
    HookOne("UnlockableSystem.set_currentCoins", RVA_SET_CURRENT_COINS,
            (void*)Hooked_SetCurrentCoins, (void**)&g_origSetCoins);
    HookOne("UnlockableSystem.AddCoins", RVA_ADD_COINS,
            (void*)Hooked_AddCoins, (void**)&g_origAddCoins);
    HookOne("UnlockableSystem.SustractCoins", RVA_SUBTRACT_COINS,
            (void*)Hooked_SubtractCoins, (void**)&g_origSubtract);
    HookOne("UnlockableSystem.Awake", RVA_UNLOCKABLE_AWAKE,
            (void*)Hooked_UnlockableAwake, (void**)&g_origAwake);
    HookOne("AdRewardSystem.get_AdCoupons", RVA_GET_AD_COUPONS,
            (void*)Hooked_GetAdCoupons, (void**)&g_origGetCoupons);
    HookOne("AdRewardSystem.get_CanClaimRewardWithoutAd", RVA_CAN_CLAIM_WITHOUT_AD,
            (void*)Hooked_CanClaimWithoutAd, (void**)&g_origCanClaimWithoutAd);
    HookOne("AdRewardSystem.TryRedeemAdCouponForCoins", RVA_TRY_REDEEM_AD_COUPON_COINS,
            (void*)Hooked_TryRedeemAdCouponForCoins, (void**)&g_origTryRedeem);
    HookOne("AdRewardSystem.TryConsumeAdCoupon", RVA_TRY_CONSUME_AD_COUPON,
            (void*)Hooked_TryConsumeAdCoupon, (void**)&g_origTryConsumeOne);
    HookOne("AdRewardSystem.TryConsumeAdCoupons", RVA_TRY_CONSUME_AD_COUPONS,
            (void*)Hooked_TryConsumeAdCoupons, (void**)&g_origTryConsumeN);
    HookOne("UIShopItem.CanAfford", RVA_SHOP_CAN_AFFORD,
            (void*)Hooked_ShopCanAfford, (void**)&g_origCanAfford);
    HookOne("UIShopItem.ConsumeCost", RVA_SHOP_CONSUME_COST,
            (void*)Hooked_ShopConsumeCost, (void**)&g_origConsumeCost);
    HookOne("UICoinManager.UpdateCurrentCoinsText", RVA_UPDATE_COINS_TEXT,
            (void*)Hooked_UpdateCurrentCoinsText, (void**)&g_origUpdateCoinsText);
    HookOne("UICoinManager.UpdateCurrentCoinsText.b__0", RVA_UI_COIN_TEXT_TWEEN_GET,
            (void*)Hooked_UiCoinTweenGet, (void**)&g_origUiCoinTweenGet);
    HookOne("UIOptionsInGame.RefreshAdCouponCount", RVA_REFRESH_AD_COUPON_COUNT,
            (void*)Hooked_RefreshAdCouponCount, (void**)&g_origRefreshCoupons);
    HookOne("SaveManager.GetSavedInt", RVA_GET_SAVED_INT,
            (void*)Hooked_GetSavedInt, (void**)&g_origGetSavedInt);
    HookOne("SaveManager.SaveNewValue(int)", RVA_SAVE_NEW_VALUE_INT,
            (void*)Hooked_SaveNewValueInt, (void**)&g_origSaveInt);

    g_ready.store(true);
    LOGI("[Bjdw] init ok");
    return true;
}

bool Bjdw_IsReady() { return g_ready.load(); }

void Bjdw_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case BJDW_FEAT_COINS:
            g_coins.store(enabled);
            if (enabled) {
                g_coinBoostDone.store(false);
                g_pendingCoinBoost.store(true);
            } else {
                g_pendingCoinBoost.store(false);
            }
            LOGI("[Bjdw] 无限金币=%s", enabled ? "ON" : "OFF");
            break;
        case BJDW_FEAT_AD_COUPON:
            g_adCoupon.store(enabled);
            LOGI("[Bjdw] 无限广告券=%s", enabled ? "ON" : "OFF");
            break;
        default:
            break;
    }
}

void Bjdw_ForceClear() {
    g_coins.store(false);
    g_adCoupon.store(false);
    g_pendingCoinBoost.store(false);
}

void Bjdw_Tick(const bool* features) {
    if (!features) return;
    g_coins.store(features[BJDW_FEAT_COINS]);
    g_adCoupon.store(features[BJDW_FEAT_AD_COUPON]);
    if (!g_ready.load()) {
        Bjdw_TryInit();
        return;
    }
    if (g_pendingCoinBoost.load() && !g_coinBoostDone.load()) BoostCoinsOnce();
    if (g_adCoupon.load() && g_saveMgr) PatchPrefsAdCouponsMem();
#if defined(STARRY_TRACE)
    const uint32_t t = g_tickReport.fetch_add(1) + 1;
    if ((t % 600) == 0) {
        BJDW_TRACE("tick summary getCoins=%u setCoins=%u getCoupons=%u getSaved=%u "
                   "updateText=%u refreshCoupon=%u canAfford=%u consume=%u",
                   g_hitGetCoins.load(), g_hitSetCoins.load(), g_hitGetCoupons.load(),
                   g_hitGetSaved.load(), g_hitUpdateText.load(), g_hitRefreshCoupon.load(),
                   g_hitCanAfford.load(), g_hitConsume.load());
    }
#endif
}
