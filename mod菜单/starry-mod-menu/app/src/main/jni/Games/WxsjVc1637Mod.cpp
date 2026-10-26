/* ================================================================
 *  无限升级 11.266018 / vc1637 — 星空版 native 模块
 *
 *  与 12.x 的 WxsjMod 完全隔离：
 *    - RuntimeAssemblyLoader::ExecuteRuntimeAssembly RVA = 0x2b3f3c4
 *    - 内嵌 targetfuntional_dll_vc1637.bin，装载时强制覆写
 *    - 不匹配官方长度时也覆写（只要缓冲区容得下），实现版本钉死
 *
 *  功能开关由 DLL 内 StarryConfig 读取 starry_config.ini（2s 缓存）。
 * ================================================================ */
#include "WxsjVc1637Mod.h"
#include "Includes/SecStrings.hpp"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <mutex>
#include <string>
#include <vector>
#include <pthread.h>
#include <unistd.h>
#include <sys/stat.h>

#include <android/log.h>
#include <dlfcn.h>
#include <link.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/StarPtr.h"   // 设备无关指针判据（清 TBI/MTE 顶字节）

// P2-2 载荷加密层（跨游戏共享件 SgPayload.h + 本游戏密文头）：
// 内嵌的不再是补丁 DLL 明文，而是密文段；运行时用票里的 kx 还原 K_build 再解封。
#include "Includes/SgPayload.h"
#include "Includes/sg_payload_enc_wxsj_vc1637.h"

#define WXSJVC_LOG "[WxsjVC1637]"

// vc1637 / 11.266018 真机运行时映射实锤。
static constexpr uintptr_t kExecRva = 0x2b3f3c4;
static const uint8_t kExecPrologue[16] = {
    0xfe, 0x0f, 0x1c, 0xf8, 0xf8, 0x5f, 0x01, 0xa9,
    0xf6, 0x57, 0x02, 0xa9, 0xf4, 0x4f, 0x03, 0xa9,
};

static const char kDllNameKey[] = "targetfuntional";
static constexpr int32_t kOfficialDllLen = 1960960;  // 仅日志兜底用

// ---- 四版本变体：按运行时下发的官方 DLL 长度/哈希选对应补丁 ----
// 2026-09-29 真机实测：服务端给 vc1637 下发的是 hotfix2 2,030,080 B，不是分析期假设的
// 1,960,960 B。ildiff 实测两版差 52 类型 / 512 方法（整个 MissionTree 系统），
// 把 1960960 系补丁注入 2030080 官方包 = A 线当年「MissionTreeData 序列化异常 / 卡等待
// 响应」的复现。无匹配变体时一律 PASSTHROUGH 官方版（宁可不生效，不能错版）。
//
// hf4 = 2026-10-26 现网热更（2,028,544 B / fnv 0x1bf6c08f1ba38809）。它与 hf3 同长度，
// 靠 fnv 精确区分；同长度回退时必须优先 hf4（最近一次现网基线），故排在 hf3 之前。
//
// 2026-10-26 起不再内置 base（1,960,960 = 2026-09-22 之前的官方 DLL）：那份基线还没有
// 2026-09-23 二次热更引入的 AddItem(string,long,bool,string) 形态，物品倍数/转生无限两项
// 物理上装不进去，留着只会产出「菜单有开关但无效」的错版。未知长度/哈希一律 PASSTHROUGH。
// 每段补丁：上游 DLL 身份 + 它在 so 里的**密文**位置。明文只在运行时解封，绝不进包。
struct DllVariant {
    int32_t officialLen;
    uint64_t officialHash;
    const char* name;
    const uint8_t* ct;      // 密文起始（objcopy 内嵌的 .enc.bin 段）
    uint32_t ctLen;         // 密文长度（= 明文长度，生成器写死在密文头里）
    const uint8_t* mac;     // Encrypt-then-MAC 的 32B MAC
    // **防封基线**（无票路径用）：官方 + 上报/执法屏蔽，**不含任何作弊功能代码**。
    // 密钥由包内 payload sha 派生（非秘密）⇒ 与票无关，so 加载即生效。
    const uint8_t* abCt;
    uint32_t abLen;
    const uint8_t* abMac;
};
extern "C" const uint8_t _binary_targetfuntional_dll_vc1637_hf2_enc_bin_start[];
extern "C" const uint8_t _binary_targetfuntional_dll_vc1637_hf4_enc_bin_start[];
extern "C" const uint8_t _binary_targetfuntional_dll_vc1637_hf3_enc_bin_start[];
extern "C" const uint8_t _binary_targetfuntional_dll_vc1637_antiban_enc_bin_start[];

/* 票派生密钥（实现在 MainModern.cpp：SgAuth 的 static 状态每 TU 一份，必须跨 TU 取） */
extern "C" int sgp_payload_key(uint8_t out[32]);
/* 原生 toast（实现在 MainModern.cpp）——防封基线路径要给用户一句可读提示 */
extern "C" void StarryShowToast(const char* text);

// 防封基线只备现网这一版（2028544 / fnv 0x1bf6c08f1ba38809）；旧热更变体没有基线 ⇒ 无票时仍 PASSTHROUGH。
static const DllVariant kDllVariants[] = {
    {2030080, 0x91f9acbd064fa875ULL, "hf2", _binary_targetfuntional_dll_vc1637_hf2_enc_bin_start, kWxsjvcHf2PayloadLen, kWxsjvcHf2PayloadMac, nullptr, 0, nullptr},
    {2028544, 0x1bf6c08f1ba38809ULL, "hf4", _binary_targetfuntional_dll_vc1637_hf4_enc_bin_start, kWxsjvcHf4PayloadLen, kWxsjvcHf4PayloadMac,
     _binary_targetfuntional_dll_vc1637_antiban_enc_bin_start, kWxsjvcAntibanLen, kWxsjvcAntibanMac},
    {2028544, 0x43df8d0d87bf08b2ULL, "hf3", _binary_targetfuntional_dll_vc1637_hf3_enc_bin_start, kWxsjvcHf3PayloadLen, kWxsjvcHf3PayloadMac, nullptr, 0, nullptr},
};
static constexpr int kDllVariantCount = (int)(sizeof(kDllVariants) / sizeof(kDllVariants[0]));

static const DllVariant* PickDllVariant(int32_t len, uint64_t h) {
    for (const auto& v : kDllVariants) {
        if (v.officialLen == len && v.officialHash == h) return &v;
    }
    for (const auto& v : kDllVariants) {
        if (v.officialLen == len) return &v;
    }
    return nullptr;
}

// ============================================================
//  P2-2 解封：拿到本变体的补丁明文。
//    成功 ⇒ 缓存（后续装载复用，避免每轮 2 MB 解封）
//    失败 ⇒ 返回 nullptr，调用方 PASSTHROUGH 官方 DLL = **诱饵**（静默无效、不崩不闪退）
//    诱饵结果**不缓存**：v3 票可能比这次程序集装载晚到，下一个装载点还能重试。
// ============================================================
static std::mutex g_plainMtx;
static std::string g_plain;                 // 最近一次成功解封的补丁明文（缓存）
static const DllVariant* g_plainFor = nullptr;
static std::atomic<int> g_decoyHits{0};

/* 防封基线密钥：sha256("starry-antiban-v1|" || payload_sha_hex)。
   ★ 这份密文**不是秘密**（攻击者能自己推出密钥）——它的作用是让基线 DLL 不以明文出现在包里
     （C8「无明文载荷泄漏」照旧 0 命中）；安全面靠它**不含任何作弊功能代码**，
     作弊功能仍然只在票控载荷里。 */
static void AntibanKey(uint8_t out[32]) {
    char buf[sizeof("starry-antiban-v1|") - 1 + 64];
    const size_t n = sizeof("starry-antiban-v1|") - 1;
    memcpy(buf, "starry-antiban-v1|", n);
    memcpy(buf + n, kWxsjvcPayloadSha, 64);
    sha256((const uint8_t*)buf, n + 64, out);
    memset(buf, 0, sizeof(buf));
}

/* 解封进调用方自己的 buffer（不用返回内部指针：别的线程换变体时会把它换掉）。
   成功返回 true 且 out 已填；失败返回 false ⇒ 调用方落诱饵。 */
/* 防封基线（官方 DLL + 上报/执法屏蔽，无任何功能代码）。
 * 密钥只由载荷 sha 派生（sha256("starry-antiban-v1|"+sha)），**与票/卡密无关** ⇒ 无票、旧票、票解不开时都能生效。
 * 2026-10-27 用户指正：防封不允许被授权链牵连；只要全量载荷解不开就必须先落到这里，再谈诱饵。 */
static bool TryAntibanBaseline(const DllVariant* v, std::string* out) {
    if (!v->abCt || !v->abLen) return false;
    uint8_t ak[32];
    AntibanKey(ak);
    std::string ab;
    const bool abOk = sgpayload::unseal(v->abCt, (size_t)v->abLen, v->abMac, ak, &ab);
    memset(ak, 0, sizeof(ak));
    if (!abOk || ab.empty()) {
        LOGW(WXSJVC_LOG " antiban baseline unseal failed (%s)", v->name);
        return false;
    }
    LOGI(WXSJVC_LOG " antiban baseline unsealed %zu B (%s): 防封 on / 功能 off", ab.size(), v->name);
    static std::atomic<int> s_abNotice{0};
    if (s_abNotice.fetch_add(1) == 0)
        StarryShowToast("[无限升级] 防封已生效；功能需登录卡密后重启游戏一次");
    g_plain = ab;
    g_plainFor = v;
    out->swap(ab);
    return true;
}

static bool GetPatchedPlain(const DllVariant* v, std::string* out) {
    std::lock_guard<std::mutex> lk(g_plainMtx);
    if (g_plainFor == v && !g_plain.empty()) {
        out->assign(g_plain);
        return true;
    }

    uint8_t pk[32];
    bool haveKey = sgp_payload_key(pk) != 0;
    if (!haveKey) {
        // 等票（最多 3s）：Java 侧 bootstrap 把 v3 票交到 native 往往比程序集装载晚几百 ms~几秒。
        // 不等就落基线，会出现「票其实马上就绪、整局却只有防封没功能」的假故障
        // （2026-10-27 用户实测：登录后首次启动 ⇒ 功能全失效，根因即此）。
        for (int i = 0; i < 60 && !haveKey; i++) {
            usleep(50 * 1000);
            haveKey = sgp_payload_key(pk) != 0;
            if (haveKey) LOGI(WXSJVC_LOG " payload key arrived after %d ms wait (%s)", (i + 1) * 50, v->name);
        }
    }
    if (!haveKey) {
        // 无票：**先走防封基线**（2026-10-26 用户定「防封随菜单 so 加载、不要卡密」）。
        // 基线 = 官方 DLL + 上报/执法屏蔽（无任何功能代码）⇒ 防封照常生效、功能仍锁在票控载荷里。
        if (TryAntibanBaseline(v, out)) return true;
        const int n = g_decoyHits.fetch_add(1) + 1;
        LOGW(WXSJVC_LOG " payload key absent -> decoy (%d)", n);
        return false;
    }
    std::string real;
    const bool ok = sgpayload::unseal(v->ct, (size_t)v->ctLen, v->mac, pk, &real);
    memset(pk, 0, sizeof(pk));
    if (!ok) {
        // 票在但解不开（票是旧 payload sha 铸的 / 已吊销 / 损坏）⇒ 先落防封基线，防封不许被授权链牵连。
        LOGW(WXSJVC_LOG " payload unseal failed with ticket (%s) -> try antiban baseline", v->name);
        if (TryAntibanBaseline(v, out)) return true;
        const int n = g_decoyHits.fetch_add(1) + 1;
        LOGW(WXSJVC_LOG " payload unseal failed -> decoy (%d)", n);
        return false;
    }
    LOGI(WXSJVC_LOG " payload decrypted %zu B (%s)", real.size(), v->name);
    g_plain = real;
    g_plainFor = v;
    out->swap(real);
    return true;
}

typedef uint8_t (*Exec_t)(void* self, void* data, void* outP, void* mi);

static std::mutex g_mtx;
static Exec_t g_origExec = nullptr;
static std::atomic<bool> g_hooked{false};
static std::atomic<bool> g_replaceArmed{false};
static std::atomic<int> g_hits{0};
static std::atomic<int> g_pinNotice{0};
static std::atomic<bool> g_retryStarted{false};
static const char* g_lastVariant = "none";

static bool ReadFileLines(const std::string& path, std::vector<std::string>* out);
static std::string PkgName();
static std::string ConfigPath();
static void WriteConfigKey(const char* key, const std::string& val);

// 数值输入框（feat 7 广告奖励倍数 / 8 物品倍数 / 12 经验倍数 / 15 物品反增倍率）
// 统一夹取 1..1000000，与 DLL 内 kItemReverseMinX/MaxX 口径一致（两边都夹，防手改配置溢出）。
// 缺省一律 1 = 官方原样（不放大）。
static const long long kWxsjItemReverseMinX = 1;
static const long long kWxsjItemReverseMaxX = 1000000;
static const long long kWxsjValueMin = 1;
static const long long kWxsjValueMax = 1000000;

// ★设备无关修正：真机（arm64 + Android 12+）堆指针带 TBI 标签（形如 0xb4000076…），
//   原先对原始指针判 <0x800000000000 会把合法对象判否 ⇒ 真机功能静默失效。
static bool OkPtr(void* p) { return starptr::LiveFast(p); }

static int PhdrCb(struct dl_phdr_info* info, size_t, void* data) {
    uintptr_t* out = (uintptr_t*)data;
    const char* n = info->dlpi_name;
    if (n && strlen(n) >= 12 && strcmp(n + strlen(n) - 12, sec_lib_il2cpp()) == 0) {
        *out = info->dlpi_addr;
        return 1;
    }
    return 0;
}

static uintptr_t Il2CppBase() {
    uintptr_t base = 0;
    dl_iterate_phdr(PhdrCb, &base);
    return base;
}

extern "C" void StarryShowToast(const char* text);

static bool ContainsDllNameKey(const uint8_t* bytes, size_t len) {
    const size_t klen = sizeof(kDllNameKey) - 1;
    for (size_t o = 0; o + klen <= len; o++) {
        if (bytes[o] == 't' && memcmp(bytes + o, kDllNameKey, klen) == 0) return true;
    }
    return false;
}

static uint64_t Fnv1a64(const uint8_t* p, size_t n) {
    uint64_t h = 0xcbf29ce484222325ULL;
    for (size_t i = 0; i < n; i++) { h ^= (uint64_t)p[i]; h *= 0x100000001b3ULL; }
    return h;
}

static void WritePinMarker(int32_t loadedLen, size_t patchedLen) {
    std::string pkg = PkgName();
    if (pkg.empty()) return;
    std::string path = "/data/user/0/" + pkg + "/files/starry_hotfix_pinned";
    FILE* f = fopen(path.c_str(), "wb");
    if (!f) return;
    fprintf(f, "vc1637 loaded=%d patched=%zu variant=%s action=forced-patch\n",
            loadedLen, patchedLen, g_lastVariant);
    fclose(f);
}

static uint8_t HookExec(void* self, void* data, void* outP, void* mi) {
    Exec_t orig = g_origExec;
    if (orig && OkPtr(data)) {
        int32_t len = 0;
        memcpy(&len, (const uint8_t*)data + 0x18, sizeof(len));

        char head[16] = {0};
        const uint8_t* bytes = (const uint8_t*)data + 0x20;
        if (len >= 4) {
            snprintf(head, sizeof(head), "%02x%02x%02x%02x", bytes[0], bytes[1], bytes[2], bytes[3]);
        }
        LOGI(WXSJVC_LOG " Exec call len=%d head=%s armed=%d", len, head,
             g_replaceArmed.load(std::memory_order_relaxed) ? 1 : 0);

        const bool isDll = len >= 2 && bytes[0] == 'M' && bytes[1] == 'Z';
        if (g_replaceArmed.load(std::memory_order_relaxed) && isDll
            && ContainsDllNameKey(bytes, (size_t)len)) {
            const uint64_t h = Fnv1a64(bytes, (size_t)len);
            const DllVariant* v = PickDllVariant(len, h);
            if (!v) {
                if (g_pinNotice.fetch_add(1) == 0) {
                    StarryShowToast("[无限升级] 游戏热更版本未识别，本次以官方版本运行（存档安全）。"
                                    "新功能请等人工更新安装包");
                }
                LOGE(WXSJVC_LOG " no variant for len=%d fnv=%016llx : PASSTHROUGH official",
                     len, (unsigned long long)h);
                return orig(self, data, outP, mi);
            }
            // 载荷必须先解封：拿不到（没有 v3 票 / 无钥 / 密文被改）⇒ 落诱饵
            // = 官方 DLL 原样执行（功能静默无效，不崩、不闪退、不弹错）。
            std::string plain;
            if (!GetPatchedPlain(v, &plain) || plain.empty()) {
                LOGW(WXSJVC_LOG " variant %s payload unavailable -> PASSTHROUGH official (decoy, no inject)",
                     v->name);
                return orig(self, data, outP, mi);
            }
            const size_t pLen = plain.size();
            if (pLen > (size_t)len) {
                LOGE(WXSJVC_LOG " variant %s patched %zuB > loaded %dB: PASSTHROUGH official",
                     v->name, pLen, len);
                return orig(self, data, outP, mi);
            }
            // 幂等：同一装载缓冲区被二次执行（换号 / 重登录 / 程序集重装载）时，
            // 缓冲区里已经是我们这一版补丁，直接放行，不重复覆写。
            if (memcmp(bytes, plain.data(), pLen) == 0) {
                LOGI(WXSJVC_LOG " buffer already carries %s patch (hit#%d fnv=%016llx) -> passthrough",
                     v->name, g_hits.load(), (unsigned long long)h);
                return orig(self, data, outP, mi);
            }
            memcpy((void*)bytes, plain.data(), pLen);
            memset((void*)(bytes + pLen), 0, (size_t)len - pLen);
            g_lastVariant = v->name;
            int n = g_hits.fetch_add(1) + 1;
            LOGI(WXSJVC_LOG " patched dll injected (vc1637 %s, hit#%d loaded=%d fnv=%016llx patched=%zu pad=%d)",
                 v->name, n, len, (unsigned long long)h, pLen, (int)((size_t)len - pLen));
            WritePinMarker(len, pLen);
            return orig(self, data, outP, mi);
        }
    }
    return orig(self, data, outP, mi);
}

static bool TryHookOnce() {
    uintptr_t base = Il2CppBase();
    if (!base) return false;

    uint8_t* target = (uint8_t*)(base + kExecRva);
    uint8_t cur[16];
    memcpy(cur, target, sizeof(cur));
    if (memcmp(cur, kExecPrologue, sizeof(cur)) != 0) return false;

    void* orig = nullptr;
    if (DobbyHook(target, (void*)HookExec, &orig) != 0 || !orig) {
        LOGE(WXSJVC_LOG " DobbyHook failed");
        return false;
    }

    g_origExec = (Exec_t)orig;
    g_hooked.store(true);
    g_replaceArmed.store(true);

    LOGI(WXSJVC_LOG " hook installed @base=%p +0x%zx orig=%p variants=%d(enc hf2:%uB hf4:%uB hf3:%uB)",
         (void*)base, kExecRva, orig, kDllVariantCount,
         (unsigned)kWxsjvcHf2PayloadLen, (unsigned)kWxsjvcHf4PayloadLen, (unsigned)kWxsjvcHf3PayloadLen);

    // arm 时先探一次票里的载荷密钥：没票也能装 hook（诱饵 = 官方 DLL 原样跑），
    // 有票才注入补丁。这条日志是「授权墙生效」与「诱饵层生效」的分界证据。
    uint8_t pkProbe[32];
    if (sgp_payload_key(pkProbe)) {
        memset(pkProbe, 0, sizeof(pkProbe));
        LOGI(WXSJVC_LOG " payload key ready (v3 ticket) -> real patch on next assembly load");
    } else {
        LOGW(WXSJVC_LOG " payload key absent at arm time -> uses antiban baseline (防封 on, 功能等 v3 ticket)%s",
             kWxsjvcAntibanLen ? "" : " / no baseline for this variant");
    }

    // 防封常开（2026-10-26 用户定：撤出菜单、随 so 加载即生效、不需要卡密）：
    // ① 有票 ⇒ 全量补丁 DLL 里的上报/执法屏蔽按 block_report 门控生效，这里落 1 保证开着；
    // ② 无票 ⇒ 走 GetPatchedPlain 里的「防封基线」（不含任何功能代码，同样按此键生效）。
    // 菜单已无「防封」开关，玩家无法关闭（ini 手改下轮冷启动也会被这行覆盖回 1）。
    WriteConfigKey("block_report", "1");
    return true;
}

static void* FastRetryThread(void*) {
    for (int i = 0; i < 600 && !g_hooked.load(); i++) {
        if (TryHookOnce()) return nullptr;
        usleep(100 * 1000);
    }
    if (!g_hooked.load()) {
        LOGE(WXSJVC_LOG " fast-retry 60s 超时放弃（允许下一轮 tryInit 重启线程）");
        g_retryStarted.store(false, std::memory_order_release);
    }
    return nullptr;
}

bool WxsjVc1637_TryInit() {
    if (g_hooked.load()) return true;
    std::lock_guard<std::mutex> lk(g_mtx);
    if (g_hooked.load()) return true;
    // MainModern 每 initRetryIntervalMs 回调一次；只允许存在一个 fast-retry 线程。
    if (!g_retryStarted.exchange(true, std::memory_order_acq_rel)) {
        pthread_t tid;
        if (pthread_create(&tid, nullptr, FastRetryThread, nullptr) == 0) {
            pthread_detach(tid);
            LOGI(WXSJVC_LOG " fast-retry thread started");
        } else {
            g_retryStarted.store(false, std::memory_order_release);
            LOGE(WXSJVC_LOG " pthread_create failed");
        }
    }
    return false;
}

bool WxsjVc1637_IsReady() { return g_hooked.load(); }

void WxsjVc1637_ForceClear() {
    LOGI(WXSJVC_LOG " ForceClear (hook kept, config-driven)");
}

static std::string PkgName() {
    char b[160] = {0};
    FILE* f = fopen("/proc/self/cmdline", "rb");
    if (!f) return "";
    size_t n = fread(b, 1, sizeof(b) - 1, f);
    fclose(f);
    if (!n) return "";
    b[n] = 0;
    return std::string(b);
}

static bool ReadFileLines(const std::string& path, std::vector<std::string>* out) {
    FILE* f = fopen(path.c_str(), "rb");
    if (!f) return false;
    std::string all;
    char buf[4096];
    size_t n;
    while ((n = fread(buf, 1, sizeof(buf), f)) > 0) all.append(buf, n);
    fclose(f);
    size_t pos = 0;
    while (pos <= all.size()) {
        size_t e = all.find('\n', pos);
        if (e == std::string::npos) {
            if (pos < all.size()) out->push_back(all.substr(pos));
            break;
        }
        out->push_back(all.substr(pos, e - pos));
        pos = e + 1;
    }
    return true;
}

static std::string ConfigPath() {
    std::string pkg = PkgName();
    if (pkg.empty()) return std::string();
    return "/data/user/0/" + pkg + "/files/starry_config.ini";
}

static void WriteConfigKey(const char* key, const std::string& val) {
    std::string path = ConfigPath();
    if (path.empty()) {
        LOGE(WXSJVC_LOG " pkg name empty, config write skipped (%s)", key);
        return;
    }
    std::string dir = path.substr(0, path.rfind('/'));
    if (dir.empty()) return;
    mkdir(dir.c_str(), 0755);
    std::vector<std::string> lines;
    bool exists = ReadFileLines(path, &lines);
    std::string tmp = path + ".tmp";
    FILE* f = fopen(tmp.c_str(), "wb");
    if (!f) {
        LOGE(WXSJVC_LOG " cannot write %s", tmp.c_str());
        return;
    }
    bool replaced = false;
    for (auto& l : lines) {
        if (l.compare(0, strlen(key), key) == 0 && l.size() > strlen(key) && l[strlen(key)] == '=') {
            fprintf(f, "%s=%s\n", key, val.c_str());
            replaced = true;
        } else if (!l.empty()) {
            fprintf(f, "%s\n", l.c_str());
        }
    }
    if (!replaced) fprintf(f, "%s=%s\n", key, val.c_str());
    fclose(f);
    rename(tmp.c_str(), path.c_str());
    (void)exists;
    LOGI(WXSJVC_LOG " config %s=%s (exists=%d)", key, val.c_str(), exists ? 1 : 0);
}

void WxsjVc1637_OnFeatureChanged(int featNum, bool enabled) {
    const char* on = enabled ? "1" : "0";
    switch (featNum) {
    case WXSJVC_FEAT_GOD:        WriteConfigKey("god", on); break;
    case WXSJVC_FEAT_ONESHOT:    WriteConfigKey("oneshot", on); break;
    case WXSJVC_FEAT_FREEADS:    WriteConfigKey("free_ads", on); break;
    case WXSJVC_FEAT_ITEMKEEP:   WriteConfigKey("item_keep", on); break;
    case WXSJVC_FEAT_SKILLFREE:  WriteConfigKey("skill_free", on); break;
    case WXSJVC_FEAT_MIJINGFREE: WriteConfigKey("mijing_free", on); break;
    case WXSJVC_FEAT_SKINUNLOCK: WriteConfigKey("skin_unlock", on); break;
    case WXSJVC_FEAT_FUNCUNLOCK: WriteConfigKey("func_unlock", on); break;
    case WXSJVC_FEAT_SHOPADS:    WriteConfigKey("shop_ads_unlimited", on); break;
    case WXSJVC_FEAT_ITEMREVERSE: WriteConfigKey("item_reverse", on); break;
    // 转生时间无限制（默认关）：开=取消转生 1 小时客户端冷却（按钮常亮、文案常「轮回」）。
    case WXSJVC_FEAT_ZHUANSHENG: WriteConfigKey("zhuansheng_free", on); break;
    default: break;
    }
}

// 输入框回调：feat 7 广告奖励倍数 -> ads_reward_x
//               feat 8 物品倍数     -> item_double_x
//               feat 12 经验倍数    -> exp_multi
//               feat 15 物品反增倍率 -> item_reverse_x
// InputValue_ 控件不走 nativeSetValue，冷启动只推空串（native 收到空串直接 return）⇒ 用户填的值能留住。
// 缺省一律 1 = 官方原样（DLL 侧 GetInt 缺键亦为 1）；用户填 N 才放大 N 倍。
void WxsjVc1637_OnInputChanged(int featNum, const char* text) {
    const char* key = nullptr;
    switch (featNum) {
    case WXSJVC_FEAT_ADSREWARD:   key = "ads_reward_x"; break;
    case WXSJVC_FEAT_ITEMDOUBLE:  key = "item_double_x"; break;
    case WXSJVC_FEAT_EXPMULTI:    key = "exp_multi"; break;
    case WXSJVC_FEAT_ITEMREVERSEX: key = "item_reverse_x"; break;
    default:
        return;
    }
    if (text == nullptr || *text == '\0') return;
    // 只收纯数字：输入途中/非法串一律不落盘，避免把半成品写进配置
    for (const char* p = text; *p != '\0'; ++p) {
        if (*p < '0' || *p > '9') return;
    }
    long long v = strtoll(text, nullptr, 10);
    if (v < kWxsjValueMin) v = kWxsjValueMin;
    if (v > kWxsjValueMax) v = kWxsjValueMax;
    char buf[24];
    snprintf(buf, sizeof(buf), "%lld", v);
    WriteConfigKey(key, buf);
}

// 滑块回调：本轮菜单已无滑块（feat 12 经验倍率改为输入框），保留空实现给注册表用。
void WxsjVc1637_OnValueChanged(int featNum, int value) {
    (void)featNum;
    (void)value;
}
