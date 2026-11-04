/* ================================================================
 *  无限升级 11.266018 / vc1637 — 星空版 native 模块
 *
 *  与 12.x 的 WxsjMod 完全隔离：
 *    - RuntimeAssemblyLoader::ExecuteRuntimeAssembly RVA = 0x2b3f3c4
 *    - 内嵌**明文**补丁 DLL（hf2/hf4/hf3 三变体），装载时强制覆写
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

// 无加固形态（2026-11-04 用户定：加固与载荷 sha 全去）：
// so 内嵌**明文**补丁 DLL，没有密文段、没有票/密钥依赖、后台不需要登记载荷 sha。
#include "Includes/sg_plain_wxsj_vc1637.h"

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
// 每段补丁：上游 DLL 身份 + 它在 so 里的**明文**位置（objcopy 内嵌 .bin）。
struct DllVariant {
    int32_t officialLen;
    uint64_t officialHash;
    const char* name;
    const uint8_t* data;    // 明文补丁 DLL 起始（objcopy 内嵌的 .bin 段）
    uint32_t len;           // 明文长度（gen_plain_header.py 按文件实测写入）
};
extern "C" const uint8_t _binary_targetfuntional_dll_vc1637_hf2_bin_start[];
extern "C" const uint8_t _binary_targetfuntional_dll_vc1637_hf4_bin_start[];
extern "C" const uint8_t _binary_targetfuntional_dll_vc1637_hf3_bin_start[];

/* 原生 toast（实现在 MainModern.cpp）——热更版本未识别时给用户一句可读提示 */
extern "C" void StarryShowToast(const char* text);

/* ---- 2026-11-04 签名轨（运行时签名替换）----
 * Android 的 sun.misc.Unsafe 没有 staticFieldOffset/staticFieldBase，Java 侧改不了
 * PackageInfo.CREATOR 这个 static final；这里用 JNI 直接写（SetStaticObjectField 绕 final）。
 * 调用方：Java com.starry.magic.sign.SigReplace.nativeSetStaticField（等 so 加载后调用）。 */
extern "C" JNIEXPORT jboolean JNICALL
Java_com_starry_magic_sign_SigReplace_nativeSetStaticField(JNIEnv* env, jclass, jclass declaring,
                                                          jobject reflectedField, jobject value) {
    if (!env || !declaring || !reflectedField) return JNI_FALSE;
    jfieldID fid = env->FromReflectedField(reflectedField);
    if (!fid) return JNI_FALSE;
    env->SetStaticObjectField(declaring, fid, value);
    if (env->ExceptionCheck()) { env->ExceptionClear(); return JNI_FALSE; }
    return JNI_TRUE;
}

// 三变体：hf2=2030080 / hf4=2028544(现网 fnv 1bf6c08f) / hf3=2028544(旧 hf3 fnv 43df8d0d)。
// 2026-11-04：三份都已叠上「投掷物倍数 / 技能伤害倍数 / 转生无条件升级」三项。
static const DllVariant kDllVariants[] = {
    {2030080, 0x91f9acbd064fa875ULL, "hf2", _binary_targetfuntional_dll_vc1637_hf2_bin_start, kWxsjvcHf2PlainLen},
    {2028544, 0x1bf6c08f1ba38809ULL, "hf4", _binary_targetfuntional_dll_vc1637_hf4_bin_start, kWxsjvcHf4PlainLen},
    {2028544, 0x43df8d0d87bf08b2ULL, "hf3", _binary_targetfuntional_dll_vc1637_hf3_bin_start, kWxsjvcHf3PlainLen},
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
//  取本变体的补丁明文（无加固：直接就是内嵌明文，不加密、不依赖票）
//    拿到 ⇒ 缓存（同一变体后续装载复用，省一次 2 MB 拷贝）
//    拿不到 ⇒ 调用方 PASSTHROUGH 官方 DLL（不崩不闪退）
// ============================================================
static std::mutex g_plainMtx;
static std::string g_plain;
static const DllVariant* g_plainFor = nullptr;

static bool GetPatchedPlain(const DllVariant* v, std::string* out) {
    if (!v->data || !v->len) {
        LOGE(WXSJVC_LOG " variant %s has no plaintext payload", v->name);
        return false;
    }
    std::lock_guard<std::mutex> lk(g_plainMtx);
    if (g_plainFor == v && !g_plain.empty()) {
        out->assign(g_plain);
        return true;
    }
    std::string real((const char*)v->data, (size_t)v->len);
    g_plain = real;
    g_plainFor = v;
    LOGI(WXSJVC_LOG " payload ready %zu B (%s, plaintext inline)", real.size(), v->name);
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
            // 补丁 DLL 是明文内嵌（无加固、无票）⇒ 只有「变体没有载荷」这一种拿不到的情况，
            // 那就 PASSTHROUGH 官方 DLL 原样执行（不崩、不闪退、不弹错）。
            std::string plain;
            if (!GetPatchedPlain(v, &plain) || plain.empty()) {
                LOGW(WXSJVC_LOG " variant %s payload unavailable -> PASSTHROUGH official (no inject)",
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

    LOGI(WXSJVC_LOG " hook installed @base=%p +0x%zx orig=%p variants=%d(plain hf2:%uB hf4:%uB hf3:%uB)",
         (void*)base, kExecRva, orig, kDllVariantCount,
         (unsigned)kWxsjvcHf2PlainLen, (unsigned)kWxsjvcHf4PlainLen, (unsigned)kWxsjvcHf3PlainLen);
    LOGI(WXSJVC_LOG " payload policy=%s -> patch injected on next assembly load", kWxsjvcPlainPolicy);

    // 防封常开（2026-10-26 用户定：撤出菜单、随 so 加载即生效、不需要卡密）：
    // 补丁 DLL 里的上报/执法屏蔽按 block_report 门控生效，这里落 1 保证开着。
    // 菜单已无「防封」开关，玩家无法关闭（ini 手改下轮冷启动也会被这行覆盖回 1）。
#ifdef WXSJ_ANTIBAN_MENU
    // 正常版：防封由菜单开关控制（缺键=false=关，与原定制版一致），这里不强制写。
#else
    WriteConfigKey("block_report", "1");
#endif
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
#ifdef WXSJ_ANTIBAN_MENU
    case WXSJVC_FEAT_ANTIBAN:    WriteConfigKey("block_report", on); break;
#endif
    case WXSJVC_FEAT_ZHUANSHENG: WriteConfigKey("zhuansheng_free", on); break;
    // feat 转生无条件升级（默认关）：开=「轮回属性升级」不扣轮回点（转生动作/其他资源不动）
    case WXSJVC_FEAT_LEGACYFREE: WriteConfigKey("zhuan_sheng_free_point", on); break;
    // 三个倍数开关（2026-11-04 用户定）：输入框只存倍数，开关决定是否生效；关=按 1 倍
    case WXSJVC_FEAT_ADSREWARD_ON:  WriteConfigKey("ads_reward_on", on); break;
    case WXSJVC_FEAT_ITEMDOUBLE_ON: WriteConfigKey("item_double_on", on); break;
    case WXSJVC_FEAT_EXPMULTI_ON:   WriteConfigKey("exp_multi_on", on); break;
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

// 滑块回调（2026-11-04 新增两项）：
//   技能投掷物倍数 -> skill_projectile_x（1..100，默认 1=官方原样）
//   技能伤害倍数   -> skill_damage_x  （1..1000，默认 1=官方原样）
// 夹取范围与菜单控件声明严格一致；缺键时 DLL 侧 GetInt 缺省也是 1。
void WxsjVc1637_OnValueChanged(int featNum, int value) {
    const char* key = nullptr;
    int lo = 1, hi = 100;
    switch (featNum) {
    case WXSJVC_FEAT_PROJECTILE: key = "skill_projectile_x"; lo = 1; hi = 100; break;
    case WXSJVC_FEAT_SKILLDMG:   key = "skill_damage_x";     lo = 1; hi = 1000; break;
    default: return;
    }
    if (value < lo) value = lo;
    if (value > hi) value = hi;
    char buf[16];
    snprintf(buf, sizeof(buf), "%d", value);
    WriteConfigKey(key, buf);
}
