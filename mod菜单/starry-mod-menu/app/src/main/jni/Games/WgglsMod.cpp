/* ================================================================
 *  我嘎嘎乱杀 1.3.6 — 百倍 / 无敌 / 战斗加速 / 免广告
 *  RVA 来自 dump.cs 1.3.6（03_decompiled/我嘎嘎乱杀/il2cppdumper_1.3.6/）
 *  1.3.5→1.3.6 重编译整体搬迁；DamageData/BattleUnit/响应字段偏移逐一核对未变
 *  STARRY_TRACE=1 时打命中计数，装机前用 logcat -s StarryTrace 证实
 * ================================================================ */

#include "WgglsMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>

#include <android/log.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"
#include "Unity/Il2CppApi.h"

enum : uintptr_t {
    ELF_BIAS = 0x4000,  // RX LOAD vaddr - file_off（1.3.6 dump RVA-Offset 差已复核）
    RVA_CALCULATE_DAMAGE = 0x32FE9A0,  // BattleUtility.CalculateDamage
    RVA_APPLY_DAMAGE     = 0x32FEFA8,  // BattleUtility.ApplyDamage
    RVA_SET_BATTLE_SPEED = 0x32F25C8,  // BattleTimeMgr.SetBattleSpeed
    RVA_SET_TIMESCALE    = 0x32F9DA4,  // BattleTimeMgr.set_TimeScale
    // CodeHash/签名不过 → GateStatus=Restrict → SEC-GATE-01
    RVA_HANDLE_SESSION_SEC = 0x3487708,  // SecurityMgr.HandleSessionStartSecurityResult
    RVA_GATE_WB            = 0x3487910,  // SecurityMgr.$Wb(AppApiGateStatus, string)
    RVA_HANDLE_VER_GATE    = 0x3487C88,  // SecurityMgr.HandleVersionGateResult
    RVA_PROCESS_VER_CHECK  = 0x35DD1E4,  // LaunchWindow.ProcessVersionCheckResult
    RVA_SHOW_ENTER_ALERT   = 0x35DCE2C,  // LaunchWindow.ShowEnterGameAlert(string)
    RVA_SHOW_SEC_DIALOG    = 0x3481DA8,  // SecurityMgr.ShowSecurityDialog(...)
    RVA_SHOW_SEC_ASYNC     = 0x3488A70,  // SecurityMgr.$EB = ShowSecurityDialogAsync
    RVA_GET_CACHED_HASH    = 0x3486750,  // SecurityMgr.GetCachedAndroidCodeHash
    RVA_CODEHASH_ONSUCCESS = 0x30E6DB8,  // AndroidWorker.CodeHashGeneratorCallback.OnSuccess
    // 免广告：走官方跳过 / 纯洁模式发奖（非仅藏广告页）
    RVA_CAN_SKIP_AD        = 0x31DD350,  // ArchiveData.get_CanSkipAdWithoutWatching
    RVA_DAILY_SKIP_UNLOCK  = 0x31DD324,  // ArchiveData.get_IsDailyAdSkipUnlocked
    RVA_GET_PURITY         = 0x3401D8C,  // RoleSessionMgr.get_IsPurityMode
    RVA_APPLY_RUNTIME_SYNC = 0x3401FEC,  // RoleSessionMgr.ApplyRuntimeSyncState
};

// 官方 1.3.6 APK：dex+so+global-metadata.dat 共 28 项（ACTk Genuine 算法
// SHA1⊕0x90 大写 hex → ASCII 算术 XOR 累积，复现见 04_scripts/我嘎嘎乱杀/compute_codehash.py；
// 与运行时 paths_len=28 同过滤）
static const char* kOfficialCodeHash = "D0A3F9B20BAD9BE5473437269F4AD378E05A6311";

enum : size_t {
    kOffDdAttacker         = 0x10,
    kOffDdTarget           = 0x18,
    kOffDdDamageMultiplier = 0x48,  // double
    kOffDdDamageBeforeSh   = 0x60,  // double
    kOffDdDisplayDamage    = 0x78,  // double
    kOffDdAppliedDamage    = 0x80,  // double
    kOffUnitFaction        = 0x80,  // BattleFaction (int32)
    // AppSessionStartResponse
    kOffSessGateStatus     = 0x60,  // AppApiGateStatus
    kOffSessGateMessage    = 0x68,  // string
    kOffSessWhitelist      = 0x70,  // bool AntiCheatWhitelistPlayer
    kOffSessPurityMode     = 0x48,  // bool IsPurityMode
    kOffSessNonVipAdSkip   = 0x4C,  // int NonVipDailyRequiredWatchedAdCountForSkip
    kOffSecCachedCodeHash  = 0x48,  // SecurityMgr.$SA cached AndroidCodeHash
};

enum AppApiGateStatus : int32_t {
    kGateNone     = 0,
    kGateAllow    = 1,
    kGateRestrict = 2,
};

enum BattleFaction : int32_t {
    kFactionNone  = 0,
    kFactionAlly  = 1,
    kFactionEnemy = 2,
};

static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_dmg{false};
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_noAd{false};
static std::atomic<int>  g_speedLvl{1};  // 1=off
static std::atomic<uint32_t> g_hitsCalc{0};
static std::atomic<uint32_t> g_hitsApply{0};
static std::atomic<uint32_t> g_hitsSpeed{0};
static std::atomic<uint32_t> g_patchDmg{0};
static std::atomic<uint32_t> g_patchInv{0};
static std::atomic<uint32_t> g_gateForce{0};
static std::atomic<uint32_t> g_hitsNoAd{0};

typedef void (*CalcDmg_t)(void* data, bool skipHit, bool skipBlock, bool skipCrit, void* method);
typedef void (*ApplyDmg_t)(void* data, bool skipLife, bool skipCombo, bool skipCounter, void* method);
typedef void (*SetSpeed_t)(void* thiz, float speed, void* method);
typedef void (*SetTs_t)(void* thiz, float value, void* method);
typedef bool (*HandleSessionSec_t)(void* thiz, void* sessionResp, void* method);
typedef bool (*GateWb_t)(void* thiz, int32_t gateStatus, void* gateMessage, void* method);
typedef bool (*HandleVerGate_t)(void* thiz, void* versionResp, void* method);
typedef bool (*ProcessVerCheck_t)(void* thiz, void* versionResult, uint32_t flag, void* method);
typedef void (*ShowEnterAlert_t)(void* thiz, void* message, void* method);
typedef void (*ShowSecDlg_t)(void* thiz, void* message, void* confirmText, void* onConfirm,
                             uint32_t closeable, uint32_t showCancel, void* cancelText, void* onCancel,
                             void* method);
// UniTaskVoid 以字节返回；$Wb 随后 Forget，吞弹时可跳过 orig
typedef uint8_t (*ShowSecAsync_t)(void* thiz, void* message, void* confirmText, void* onConfirm,
                                  uint32_t closeable, uint32_t showCancel, void* cancelText,
                                  void* onCancel, void* method);
typedef void* (*GetCachedHash_t)(void* thiz, void* method);
typedef void (*CodeHashOnSuccess_t)(void* thiz, void* buildPath, void* paths, void* hashes,
                                    void* summaryHash, void* method);
typedef void* (*Il2CppStringNew_t)(const char* utf8);
typedef uint8_t (*BoolGetter_t)(void* thiz, void* method);
typedef void (*ApplyRuntimeSync_t)(void* thiz, uint32_t isPurityMode, int32_t nonVipAdSkip,
                                   void* method);

static CalcDmg_t g_origCalc = nullptr;
static ApplyDmg_t g_origApply = nullptr;
static SetSpeed_t g_origSetSpeed = nullptr;
static SetTs_t g_origSetTs = nullptr;
static HandleSessionSec_t g_origHandleSession = nullptr;
static GateWb_t g_origGateWb = nullptr;
static HandleVerGate_t g_origHandleVerGate = nullptr;
static ProcessVerCheck_t g_origProcessVerCheck = nullptr;
static ShowEnterAlert_t g_origShowEnterAlert = nullptr;
static ShowSecDlg_t g_origShowSecDlg = nullptr;
static ShowSecAsync_t g_origShowSecAsync = nullptr;
static GetCachedHash_t g_origGetCachedHash = nullptr;
static CodeHashOnSuccess_t g_origCodeHashOnSuccess = nullptr;
static Il2CppStringNew_t g_il2cppStringNew = nullptr;
static BoolGetter_t g_origCanSkipAd = nullptr;
static BoolGetter_t g_origDailySkipUnlock = nullptr;
static BoolGetter_t g_origGetPurity = nullptr;
static ApplyRuntimeSync_t g_origApplyRuntimeSync = nullptr;

// 战斗中缓存 BattleTimeMgr*，滑条变更时主动 SetBattleSpeed（否则要等过关再设速）
static std::atomic<void*> g_btm{nullptr};
static void* g_setSpeedMethod = nullptr;
static std::atomic<uint32_t> g_hashLogN{0};
static std::atomic<int> g_lastAppliedSpeed{-1};

/** maps：file_off → VA。libil2cpp RX 分段时禁止 ELF base+RVA（hook-rules）。 */
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

static bool HookOne(const char* tag, uintptr_t rva, void* hook, void** orig) {
    void* addr = Abs(rva);
    if (!addr) {
        LOGE("[Wggls] %s resolve failed (0x%lX)", tag, (unsigned long)rva);
        return false;
    }
    if (*orig) return true;
    if (DobbyHook(addr, hook, orig) != 0) {
        LOGE("[Wggls] DobbyHook %s failed @ %p", tag, addr);
        return false;
    }
    LOGI("[Wggls] hooked %s @ %p (0x%lX file=0x%lX)", tag, addr, (unsigned long)rva,
         (unsigned long)(rva - ELF_BIAS));
    return true;
}

static bool ValidObj(void* p) {
    return p && Il2Cpp::IsValidHeapPtr(p);
}

static int32_t ReadFaction(void* unit) {
    if (!ValidObj(unit)) return kFactionNone;
    return *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(unit) + kOffUnitFaction);
}

static float SpeedMul() {
    switch (g_speedLvl.load()) {
        case 2: return 1.5f;
        case 3: return 2.0f;
        case 4: return 3.0f;
        case 5: return 4.0f;
        default: return 1.0f;
    }
}

#if defined(STARRY_TRACE)
#define WGGLS_TRACE(...) \
    __android_log_print(ANDROID_LOG_INFO, "StarryTrace", "[Wggls] " __VA_ARGS__)
#else
#define WGGLS_TRACE(...) ((void)0)
#endif

static void TraceHit(const char* tag, uint32_t n, uint32_t patch) {
    if (n <= 8 || (n % 40) == 0) {
        WGGLS_TRACE("%s hits=%u patch=%u", tag, n, patch);
    }
}

static void Hooked_CalculateDamage(void* data, bool skipHit, bool skipBlock, bool skipCrit,
                                   void* method) {
    uint32_t n = g_hitsCalc.fetch_add(1) + 1;
    TraceHit("CalculateDamage", n, g_patchDmg.load());

    if (g_dmg.load() && ValidObj(data)) {
        void* atk = *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(data) + kOffDdAttacker);
        void* tgt = *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(data) + kOffDdTarget);
        if (ReadFaction(atk) == kFactionAlly && ReadFaction(tgt) == kFactionEnemy) {
            double* mul = reinterpret_cast<double*>(reinterpret_cast<uintptr_t>(data) +
                                                    kOffDdDamageMultiplier);
            *mul *= 100.0;
            g_patchDmg.fetch_add(1);
            TraceHit("CalculateDamage.patch", g_patchDmg.load(), g_patchDmg.load());
        }
    }
    if (g_origCalc) g_origCalc(data, skipHit, skipBlock, skipCrit, method);
}

static void ZeroDamageFields(void* data) {
    auto p = reinterpret_cast<uintptr_t>(data);
    *reinterpret_cast<double*>(p + kOffDdDamageBeforeSh) = 0.0;
    *reinterpret_cast<double*>(p + kOffDdDisplayDamage) = 0.0;
    *reinterpret_cast<double*>(p + kOffDdAppliedDamage) = 0.0;
    *reinterpret_cast<double*>(p + kOffDdDamageMultiplier) = 0.0;
}

static void Hooked_ApplyDamage(void* data, bool skipLife, bool skipCombo, bool skipCounter,
                               void* method) {
    uint32_t n = g_hitsApply.fetch_add(1) + 1;
    TraceHit("ApplyDamage", n, g_patchInv.load());

    if (g_inv.load() && ValidObj(data)) {
        void* tgt = *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(data) + kOffDdTarget);
        if (ReadFaction(tgt) == kFactionAlly) {
            ZeroDamageFields(data);
            g_patchInv.fetch_add(1);
            TraceHit("ApplyDamage.patch", g_patchInv.load(), g_patchInv.load());
        }
    }
    if (g_origApply) g_origApply(data, skipLife, skipCombo, skipCounter, method);
}

static void Hooked_SetBattleSpeed(void* thiz, float speed, void* method) {
    g_btm.store(thiz);
    g_setSpeedMethod = method;
    uint32_t n = g_hitsSpeed.fetch_add(1) + 1;
    float out = speed;
    if (g_speedLvl.load() > 1) out = SpeedMul();
    TraceHit("SetBattleSpeed", n, g_speedLvl.load() > 1 ? 1u : 0u);
    if (g_origSetSpeed) g_origSetSpeed(thiz, out, method);
}

static void Hooked_SetTimeScale(void* thiz, float value, void* method) {
    g_btm.store(thiz);
    float out = value;
    if (g_speedLvl.load() > 1) out = SpeedMul();
    if (g_origSetTs) g_origSetTs(thiz, out, method);
}

/** 菜单改档后立刻写入；lvl=1 时写回 1.0 */
static void ApplySpeedNow(const char* reason) {
    void* thiz = g_btm.load();
    if (!thiz || !g_origSetSpeed) {
        LOGI("[Wggls] ApplySpeedNow skip (no BattleTimeMgr yet) reason=%s", reason);
        return;
    }
    int lvl = g_speedLvl.load();
    float mul = (lvl > 1) ? SpeedMul() : 1.0f;
    g_origSetSpeed(thiz, mul, g_setSpeedMethod);
    g_lastAppliedSpeed.store(lvl);
    LOGI("[Wggls] ApplySpeedNow mul=%.1f lvl=%d reason=%s", mul, lvl, reason);
    WGGLS_TRACE("ApplySpeedNow mul=%.1f lvl=%d %s", mul, lvl, reason);
}

// 兜底：服端仍 Restrict 时本地改 Allow；正经路径靠 CodeHash 伪造让服端直接 Allow。
static void ForceSessionAllow(void* sessionResp, const char* tag) {
    if (!sessionResp) {
        LOGI("[Wggls] %s sessionResp=null", tag);
        return;
    }
    auto base = reinterpret_cast<uintptr_t>(sessionResp);
    int32_t st = *reinterpret_cast<int32_t*>(base + kOffSessGateStatus);
    if (st != kGateAllow) {
        *reinterpret_cast<int32_t*>(base + kOffSessGateStatus) = kGateAllow;
        *reinterpret_cast<void**>(base + kOffSessGateMessage) = nullptr;
        *reinterpret_cast<uint8_t*>(base + kOffSessWhitelist) = 1;
        uint32_t n = g_gateForce.fetch_add(1) + 1;
        WGGLS_TRACE("%s force Allow was=%d n=%u", tag, st, n);
        LOGI("[Wggls] %s force Allow was=%d n=%u", tag, st, n);
    }
    if (g_noAd.load()) {
        *reinterpret_cast<uint8_t*>(base + kOffSessPurityMode) = 1;
        *reinterpret_cast<int32_t*>(base + kOffSessNonVipAdSkip) = 1;
    }
}

static bool Hooked_HandleSessionSec(void* thiz, void* sessionResp, void* method) {
    ForceSessionAllow(sessionResp, "HandleSessionSec");
    if (g_origHandleSession) return g_origHandleSession(thiz, sessionResp, method);
    return true;
}

static bool Hooked_GateWb(void* thiz, int32_t gateStatus, void* gateMessage, void* method) {
    if (gateStatus != kGateAllow) {
        LOGI("[Wggls] $Wb status=%d -> Allow (fallback)", gateStatus);
        WGGLS_TRACE("$Wb status=%d -> Allow", gateStatus);
        gateStatus = kGateAllow;
        gateMessage = nullptr;
    }
    if (g_origGateWb) return g_origGateWb(thiz, gateStatus, gateMessage, method);
    return true;
}

// AppVersionResponse.GateStatus@0x38 / GateMessage@0x40 → 改 Allow 后走 orig（正经过检流程）
static bool Hooked_HandleVerGate(void* thiz, void* versionResp, void* method) {
    if (versionResp) {
        auto base = reinterpret_cast<uintptr_t>(versionResp);
        int32_t st = *reinterpret_cast<int32_t*>(base + 0x38);
        if (st != kGateAllow) {
            *reinterpret_cast<int32_t*>(base + 0x38) = kGateAllow;
            *reinterpret_cast<void**>(base + 0x40) = nullptr;
            LOGI("[Wggls] HandleVersionGateResult patch Allow was=%d", st);
            WGGLS_TRACE("HandleVersionGateResult patch Allow was=%d", st);
        }
    }
    if (g_origHandleVerGate) return g_origHandleVerGate(thiz, versionResp, method);
    return false;  // 无 orig 时：caller eor#1 → false=放行
}

// AppApiCallResult<AppVersionResponse>：Success@0x10 Data@0x38；Data.GateStatus@0x38 GateMessage@0x40
static void ForceVersionResultAllow(void* versionResult, const char* tag) {
    if (!versionResult) return;
    auto base = reinterpret_cast<uintptr_t>(versionResult);
    *reinterpret_cast<uint8_t*>(base + 0x10) = 1;  // Success
    void* data = *reinterpret_cast<void**>(base + 0x38);
    if (!data) {
        LOGI("[Wggls] %s Data=null", tag);
        return;
    }
    auto d = reinterpret_cast<uintptr_t>(data);
    int32_t st = *reinterpret_cast<int32_t*>(d + 0x38);
    *reinterpret_cast<int32_t*>(d + 0x38) = kGateAllow;
    *reinterpret_cast<void**>(d + 0x40) = nullptr;
    if (st != kGateAllow) {
        LOGI("[Wggls] %s version GateStatus was=%d -> Allow", tag, st);
        WGGLS_TRACE("%s version Gate was=%d -> Allow", tag, st);
    }
}

// 走 orig，让 LaunchWindow 写正式通过旗标；先改写响应再进 orig（服端仍 Restrict 时也能进）
static bool Hooked_ProcessVerCheck(void* thiz, void* versionResult, uint32_t flag, void* method) {
    ForceVersionResultAllow(versionResult, "ProcessVersionCheckResult");
    if (g_origProcessVerCheck) return g_origProcessVerCheck(thiz, versionResult, flag, method);
    return true;
}

// Il2CppString: +0x10 length, +0x14 utf16（本包 Metadata 31）
static bool Il2CppStrHasAscii(void* str, const char* needle) {
    if (!str || !needle) return false;
    int32_t len = *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(str) + 0x10);
    if (len <= 0 || len > 8192) return false;
    auto* chars = reinterpret_cast<char16_t*>(reinterpret_cast<uintptr_t>(str) + 0x14);
    size_t needleLen = 0;
    while (needle[needleLen]) ++needleLen;
    if ((size_t)len < needleLen) return false;
    for (int32_t i = 0; i <= len - (int32_t)needleLen; ++i) {
        size_t j = 0;
        for (; j < needleLen; ++j) {
            if (chars[i + j] != (char16_t)needle[j]) break;
        }
        if (j == needleLen) return true;
    }
    return false;
}

static void LogIl2CppStrPreview(const char* tag, void* str) {
    if (!str) {
        LOGI("[Wggls] %s msg=<null>", tag);
        return;
    }
    int32_t len = *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(str) + 0x10);
    if (len <= 0 || len > 8192) {
        LOGI("[Wggls] %s msg len=%d", tag, len);
        return;
    }
    auto* chars = reinterpret_cast<char16_t*>(reinterpret_cast<uintptr_t>(str) + 0x14);
    char buf[96];
    int n = len < 80 ? len : 80;
    int o = 0;
    for (int i = 0; i < n && o < (int)sizeof(buf) - 2; ++i) {
        char16_t c = chars[i];
        buf[o++] = (c >= 32 && c < 127) ? (char)c : '?';
    }
    buf[o] = 0;
    LOGI("[Wggls] %s len=%d ascii='%s'", tag, len, buf);
    WGGLS_TRACE("%s len=%d ascii='%s'", tag, len, buf);
}

static bool IsSecGateMessage(void* msg) {
    return Il2CppStrHasAscii(msg, "SEC-GATE") || Il2CppStrHasAscii(msg, "GATE-01") ||
           Il2CppStrHasAscii(msg, "SEC-ROOT") || Il2CppStrHasAscii(msg, "SEC-PROT");
}

static void* MakeIl2CppString(const char* utf8) {
    if (!utf8) return nullptr;
    if (!g_il2cppStringNew) {
        void* h = xdl_open(GameUnity::TARGET_LIB, XDL_DEFAULT);
        if (h) {
            g_il2cppStringNew = (Il2CppStringNew_t)xdl_sym(h, "il2cpp_string_new", nullptr);
            if (!g_il2cppStringNew)
                g_il2cppStringNew = (Il2CppStringNew_t)xdl_dsym(h, "il2cpp_string_new", nullptr);
            xdl_close(h);
        }
    }
    if (!g_il2cppStringNew) {
        LOGE("[Wggls] il2cpp_string_new unresolved");
        return nullptr;
    }
    return g_il2cppStringNew(utf8);
}

static void* SpoofOfficialCodeHash(const char* tag, void* realHash, void* secMgr) {
    LogIl2CppStrPreview(tag, realHash);
    void* fake = MakeIl2CppString(kOfficialCodeHash);
    if (!fake) return realHash;
    if (secMgr) {
        *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(secMgr) + kOffSecCachedCodeHash) = fake;
    }
    LOGI("[Wggls] %s spoof CodeHash -> %s", tag, kOfficialCodeHash);
    WGGLS_TRACE("%s spoof CodeHash", tag);
    return fake;
}

// 正经：上报官方 CodeHash，让服务端下发 Allow（不是藏弹窗）。
static void* Hooked_GetCachedHash(void* thiz, void* method) {
    void* real = g_origGetCachedHash ? g_origGetCachedHash(thiz, method) : nullptr;
    // 热路径可能反复取 hash：限流日志，避免点按/UI 卡顿
    uint32_t n = g_hashLogN.fetch_add(1) + 1;
    if (n <= 3 || (n % 40) == 0)
        return SpoofOfficialCodeHash("GetCachedAndroidCodeHash", real, thiz);
    void* fake = MakeIl2CppString(kOfficialCodeHash);
    if (fake && thiz) {
        *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(thiz) + kOffSecCachedCodeHash) = fake;
    }
    return fake ? fake : real;
}

static void Hooked_CodeHashOnSuccess(void* thiz, void* buildPath, void* paths, void* hashes,
                                     void* summaryHash, void* method) {
    LogIl2CppStrPreview("CodeHash.OnSuccess.real", summaryHash);
    // paths/hashes: Il2CppString[] → length@+0x18, items@+0x20（常见 Array 布局）
    if (paths) {
        int32_t n = *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(paths) + 0x18);
        LOGI("[Wggls] CodeHash.OnSuccess paths_len=%d", n);
        WGGLS_TRACE("CodeHash.OnSuccess paths_len=%d", n);
        if (n > 0 && n < 512) {
            auto** items = reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(paths) + 0x20);
            auto** hitems = hashes ? reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(hashes) + 0x20)
                                   : nullptr;
            int show = n < 40 ? n : 40;  // Trace 期全量打印，用于本地 CodeHash 公式核对
            for (int i = 0; i < show; ++i) {
                LogIl2CppStrPreview("CodeHash.path", items[i]);
                if (hitems) LogIl2CppStrPreview("CodeHash.hash", hitems[i]);
            }
        }
    }
    void* fake = MakeIl2CppString(kOfficialCodeHash);
    if (fake) {
        LOGI("[Wggls] CodeHash.OnSuccess spoof -> %s", kOfficialCodeHash);
        WGGLS_TRACE("CodeHash.OnSuccess spoof");
        summaryHash = fake;
    }
    if (g_origCodeHashOnSuccess)
        g_origCodeHashOnSuccess(thiz, buildPath, paths, hashes, summaryHash, method);
}

// SEC 弹窗仅作兜底吞掉；主路径应已 Allow，不应再走到这里。
static void Hooked_ShowEnterAlert(void* thiz, void* message, void* method) {
    LogIl2CppStrPreview("ShowEnterGameAlert", message);
    if (IsSecGateMessage(message)) {
        LOGI("[Wggls] swallowed SEC-* ShowEnterGameAlert (fallback)");
        WGGLS_TRACE("swallowed ShowEnterGameAlert");
        return;
    }
    if (g_origShowEnterAlert) g_origShowEnterAlert(thiz, message, method);
}

static void Hooked_ShowSecDlg(void* thiz, void* message, void* confirmText, void* onConfirm,
                              uint32_t closeable, uint32_t showCancel, void* cancelText, void* onCancel,
                              void* method) {
    LogIl2CppStrPreview("ShowSecurityDialog", message);
    if (IsSecGateMessage(message)) {
        LOGI("[Wggls] swallowed SEC-* ShowSecurityDialog");
        WGGLS_TRACE("swallowed ShowSecurityDialog");
        return;
    }
    if (g_origShowSecDlg)
        g_origShowSecDlg(thiz, message, confirmText, onConfirm, closeable, showCancel, cancelText,
                         onCancel, method);
}

static uint8_t Hooked_ShowSecAsync(void* thiz, void* message, void* confirmText, void* onConfirm,
                                   uint32_t closeable, uint32_t showCancel, void* cancelText,
                                   void* onCancel, void* method) {
    LogIl2CppStrPreview("ShowSecurityDialogAsync/$EB", message);
    if (IsSecGateMessage(message)) {
        LOGI("[Wggls] swallowed SEC-* $EB");
        WGGLS_TRACE("swallowed $EB");
        return 0;
    }
    if (g_origShowSecAsync)
        return g_origShowSecAsync(thiz, message, confirmText, onConfirm, closeable, showCancel,
                                  cancelText, onCancel, method);
    return 0;
}

static uint8_t Hooked_CanSkipAd(void* thiz, void* method) {
    if (g_noAd.load()) {
        uint32_t n = g_hitsNoAd.fetch_add(1) + 1;
        if (n <= 8 || (n % 40) == 0) {
            LOGI("[Wggls] CanSkipAdWithoutWatching force true n=%u", n);
            WGGLS_TRACE("CanSkipAd force true n=%u", n);
        }
        return 1;
    }
    return g_origCanSkipAd ? g_origCanSkipAd(thiz, method) : 0;
}

static uint8_t Hooked_DailySkipUnlock(void* thiz, void* method) {
    if (g_noAd.load()) return 1;
    return g_origDailySkipUnlock ? g_origDailySkipUnlock(thiz, method) : 0;
}

static uint8_t Hooked_GetPurity(void* thiz, void* method) {
    if (g_noAd.load()) return 1;
    return g_origGetPurity ? g_origGetPurity(thiz, method) : 0;
}

static void Hooked_ApplyRuntimeSync(void* thiz, uint32_t isPurityMode, int32_t nonVipAdSkip,
                                    void* method) {
    if (g_noAd.load()) {
        isPurityMode = 1;
        if (nonVipAdSkip > 1) nonVipAdSkip = 1;
        LOGI("[Wggls] ApplyRuntimeSync force purity=1 nonVip=%d", nonVipAdSkip);
        WGGLS_TRACE("ApplyRuntimeSync purity force");
    }
    if (g_origApplyRuntimeSync) g_origApplyRuntimeSync(thiz, isPurityMode, nonVipAdSkip, method);
}

bool Wggls_TryInit() {
    if (g_ready.load()) return true;
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) return false;

    if (!HookOne("CalculateDamage", RVA_CALCULATE_DAMAGE, (void*)Hooked_CalculateDamage,
                 (void**)&g_origCalc))
        return false;
    if (!HookOne("ApplyDamage", RVA_APPLY_DAMAGE, (void*)Hooked_ApplyDamage, (void**)&g_origApply))
        return false;
    HookOne("SetBattleSpeed", RVA_SET_BATTLE_SPEED, (void*)Hooked_SetBattleSpeed,
            (void**)&g_origSetSpeed);
    HookOne("set_TimeScale", RVA_SET_TIMESCALE, (void*)Hooked_SetTimeScale, (void**)&g_origSetTs);
    HookOne("HandleSessionSec", RVA_HANDLE_SESSION_SEC, (void*)Hooked_HandleSessionSec,
            (void**)&g_origHandleSession);
    HookOne("GateWb", RVA_GATE_WB, (void*)Hooked_GateWb, (void**)&g_origGateWb);
    HookOne("HandleVerGate", RVA_HANDLE_VER_GATE, (void*)Hooked_HandleVerGate,
            (void**)&g_origHandleVerGate);
    HookOne("ProcessVerCheck", RVA_PROCESS_VER_CHECK, (void*)Hooked_ProcessVerCheck,
            (void**)&g_origProcessVerCheck);
    HookOne("ShowEnterAlert", RVA_SHOW_ENTER_ALERT, (void*)Hooked_ShowEnterAlert,
            (void**)&g_origShowEnterAlert);
    HookOne("ShowSecDlg", RVA_SHOW_SEC_DIALOG, (void*)Hooked_ShowSecDlg, (void**)&g_origShowSecDlg);
    HookOne("ShowSecAsync", RVA_SHOW_SEC_ASYNC, (void*)Hooked_ShowSecAsync,
            (void**)&g_origShowSecAsync);
    HookOne("GetCachedHash", RVA_GET_CACHED_HASH, (void*)Hooked_GetCachedHash,
            (void**)&g_origGetCachedHash);
    HookOne("CodeHashOnSuccess", RVA_CODEHASH_ONSUCCESS, (void*)Hooked_CodeHashOnSuccess,
            (void**)&g_origCodeHashOnSuccess);
    HookOne("CanSkipAd", RVA_CAN_SKIP_AD, (void*)Hooked_CanSkipAd, (void**)&g_origCanSkipAd);
    HookOne("DailySkipUnlock", RVA_DAILY_SKIP_UNLOCK, (void*)Hooked_DailySkipUnlock,
            (void**)&g_origDailySkipUnlock);
    HookOne("GetPurity", RVA_GET_PURITY, (void*)Hooked_GetPurity, (void**)&g_origGetPurity);
    HookOne("ApplyRuntimeSync", RVA_APPLY_RUNTIME_SYNC, (void*)Hooked_ApplyRuntimeSync,
            (void**)&g_origApplyRuntimeSync);

    g_ready.store(true);
    LOGI("[Wggls] init ok calc=%d apply=%d speed=%d hash=%d canSkip=%d purity=%d sync=%d",
         g_origCalc ? 1 : 0, g_origApply ? 1 : 0, g_origSetSpeed ? 1 : 0,
         g_origGetCachedHash ? 1 : 0, g_origCanSkipAd ? 1 : 0, g_origGetPurity ? 1 : 0,
         g_origApplyRuntimeSync ? 1 : 0);
    return true;
}

bool Wggls_IsReady() { return g_ready.load(); }

void Wggls_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case WGGLS_FEAT_DMG100:
            g_dmg.store(enabled);
            LOGI("[Wggls] 百倍伤害=%s", enabled ? "ON" : "OFF");
            break;
        case WGGLS_FEAT_INV:
            g_inv.store(enabled);
            LOGI("[Wggls] 超级无敌=%s", enabled ? "ON" : "OFF");
            break;
        case WGGLS_FEAT_SPEED:
            if (!enabled) g_speedLvl.store(1);
            LOGI("[Wggls] 战斗加速 toggle=%s lvl=%d", enabled ? "ON" : "OFF", g_speedLvl.load());
            ApplySpeedNow("feature");
            break;
        case WGGLS_FEAT_NOAD:
            g_noAd.store(enabled);
            LOGI("[Wggls] 免广告=%s", enabled ? "ON" : "OFF");
            WGGLS_TRACE("免广告=%s", enabled ? "ON" : "OFF");
            break;
        default:
            break;
    }
}

void Wggls_OnValueChanged(int featNum, int value) {
    if (featNum != WGGLS_FEAT_SPEED) return;
    if (value < 1) value = 1;
    if (value > 5) value = 5;
    int prev = g_speedLvl.exchange(value);
    if (prev == value && g_lastAppliedSpeed.load() == value) return;
    LOGI("[Wggls] 战斗加速 lvl=%d mul=%.1f", value, SpeedMul());
    ApplySpeedNow("slider");
}

void Wggls_ForceClear() {
    g_dmg.store(false);
    g_inv.store(false);
    g_noAd.store(false);
    g_speedLvl.store(1);
    ApplySpeedNow("clear");
    g_btm.store(nullptr);
}

void Wggls_Tick(const bool* features) {
    if (!features) return;
    g_dmg.store(features[WGGLS_FEAT_DMG100]);
    g_inv.store(features[WGGLS_FEAT_INV]);
    g_noAd.store(features[WGGLS_FEAT_NOAD]);
    if (!g_ready.load()) Wggls_TryInit();
    // 战斗内偶发把倍速写回 1：低频再刷一次
    static uint32_t tick = 0;
    if (g_speedLvl.load() > 1 && (++tick % 20) == 0) ApplySpeedNow("tick");
}
