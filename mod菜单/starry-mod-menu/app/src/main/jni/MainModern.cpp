/* ================================================================
 *  星光魔法 MOD 菜单 — MainModern.cpp（注册表模式入口）
 *
 *  与 Main.cpp（legacy 双轨入口）并行存在：
 *    - 本文件不含任何具体游戏代码；游戏差异全部经 Games/ModEntry.h
 *      的 StarryGameMod 注册表声明（由 Games/{Game}Entry.cpp 提供）
 *    - 新游戏接手：只写 Mod + Entry + 私有 mk，本文件永不修改
 *    - 存量游戏：仍在 legacy Main.cpp，重新出包时顺手迁移（迁一个删一组 #if）
 *
 *  框架职责（从 legacy Main.cpp 原样继承，未改语义）：
 *    - 授权门控：grant HMAC 短票验签 / session_live / clear_runtime_session
 *    - JNI 桥：RegisterNatives 19 项（与 ModBridge.java 逐一对应）
 *    - 后台线程：等 waitLib → 参数化 tryInit 重试 → Tick 主循环（掉线清一次）
 *    - 通用 CaveBoot：cave/晚载注入场景的 Starry_CaveBoot 导出
 *
 *  作者：阿亮
 *  日期：2026-09-09（阶段 0：SELF 自测壳验收；旧路径 Main.cpp 零改动）
 * ================================================================
 */

#include <jni.h>
#include <string.h>
#include <stdlib.h>
#include <pthread.h>
#include <unistd.h>
#include <dlfcn.h>
#include <time.h>
#include <android/log.h>

// ===== 工具链（与 legacy Main.cpp 一致）=====
#include "Includes/Logger.h"
#include "CertPinned.h"        // 证书固定公钥（编译期烘焙，AuthClient 经桥接读取）
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"

#include "Includes/Macros.h"   // HOOK / PATCH / RESTORE 宏
#include "Includes/obfuscate.h"
#include "Includes/hmac_sha256.h"
#include "Dobby/dobby.h"

#include "xDL/xdl.h"

// ===== 游戏注册表（唯一游戏入口；本文件不再 include 任何 Games/*Mod.h）=====
#include "Games/ModEntry.h"

#define TAG "StarryMagic"

// ============================================================
//  全局状态（语义与 legacy Main.cpp 一致）
// ============================================================

static JavaVM* g_jvm = nullptr;
static jobject g_context = nullptr;
static bool g_initialized = false;
static bool g_gameLibLoaded = false;

// 注册表装配（静态初始化早于任何 constructor / JNI_OnLoad；
// Entry 侧的 featureList 数组是 .rodata 常量，无跨 TU 构造顺序问题）
static const StarryGameMod* g_mod = GameMod_Get();

// ===== 功能开关（通用） =====
static bool g_features[64] = {false};  // 最多64个开关
static int  g_values[64]   = {0};      // 滑块/下拉值
static char g_texts[64][256] = {{0}};  // 输入框文本
static bool g_authorized   = false;    // 诱饵开关（仅 JS/Java 可改，不单独放行功能）
static bool g_rt_ok        = false;    // 服务端短票验签通过

// ===== 按钮触发标志 =====
static bool g_btnTrigger[64] = {false};

// 须与 auth-server GRANT_HMAC_SECRET / grant_hmac_secret 默认一致
static const char* grant_pepper() {
    return OBFUSCATE("sg-grant-v1-7f3a9c2e");
}

static bool hex_eq_ci(const char* hex, const uint8_t* dig, size_t n) {
    static const char* lut = "0123456789abcdef";
    if (!hex || strlen(hex) < n * 2) return false;
    for (size_t i = 0; i < n; i++) {
        char a = hex[i * 2], b = hex[i * 2 + 1];
        if (a >= 'A' && a <= 'F') a = (char)(a - 'A' + 'a');
        if (b >= 'A' && b <= 'F') b = (char)(b - 'A' + 'a');
        if (a != lut[(dig[i] >> 4) & 0xf] || b != lut[dig[i] & 0xf]) return false;
    }
    return true;
}

/** 运行时会话：登录后永不过期，除非进程重启或 nativeClearGrant。
 *
 * 设计目标：用户登录一次，功能就一直可用，直到 App 重启。
 * 心跳只用于服务端状态同步（卡密信息、菜单门控），不影响功能放行。
 * 后台禁卡/删卡不会断已登录用户的功能。
 *
 * g_rt_ok 在 apply_grant_utf8() 成功后设为 true，
 * 只在 clear_runtime_session()（进程重启 / nativeClearGrant）时清零。
 */
static bool session_live() {
    return g_rt_ok;
}

static void clear_runtime_session() {
    g_rt_ok = false;
    g_authorized = false;
    for (int i = 0; i < 64; i++) {
        g_features[i] = false;
        g_btnTrigger[i] = false;
    }
    // 游戏侧状态清理由 mod->forceClear 承担（掉线后 Tick 循环里执行一次）
}

/** grant 格式: 1|device|game|exp|menu|card|kid.hexsig */
static bool apply_grant_utf8(const char* grant, const char* device, const char* game) {
    if (!grant || !device || !game || !*grant) return false;
    const char* dot = strrchr(grant, '.');
    if (!dot || dot == grant || !*(dot + 1)) return false;
    size_t body_len = (size_t)(dot - grant);
    if (body_len < 8 || body_len > 512) return false;
    char body[520];
    memcpy(body, grant, body_len);
    body[body_len] = 0;
    const char* sig = dot + 1;

    uint8_t mac[32];
    const char* pepper = grant_pepper();
    hmac_sha256((const uint8_t*)pepper, strlen(pepper),
                (const uint8_t*)body, body_len, mac);
    if (!hex_eq_ci(sig, mac, 32)) return false;

    // parse fields
    char tmp[520];
    memcpy(tmp, body, body_len + 1);
    char* save = nullptr;
    char* ver = strtok_r(tmp, "|", &save);
    char* d = strtok_r(nullptr, "|", &save);
    char* g = strtok_r(nullptr, "|", &save);
    char* exp_s = strtok_r(nullptr, "|", &save);
    char* menu_s = strtok_r(nullptr, "|", &save);
    char* card_s = strtok_r(nullptr, "|", &save);
    char* kid = strtok_r(nullptr, "|", &save);
    if (!ver || !d || !g || !exp_s || !menu_s || !card_s || !kid) return false;
    if (strcmp(ver, "1") != 0) return false;
    if (strcmp(d, device) != 0 || strcmp(g, game) != 0) return false;
    int64_t exp = (int64_t)atoll(exp_s);
    int64_t now = (int64_t)time(nullptr);
    // 首次验签检查过期（登录后不过期，但首次不能接受已过期的票）
    if (now > exp + 45) return false;
    if (menu_s[0] == '0') return false; // 游戏停用票不放行

    LOGI("[验证] grant ok exp=%lld", (long long)exp);
    g_rt_ok = true;
    g_authorized = true; // 同步诱饵，避免只改诱饵造成错觉
    (void)card_s;
    (void)kid;
    return true;
}

// ============================================================
//  JNI 工具
// ============================================================

static JNIEnv* GetJNIEnv() {
    if (!g_jvm) return nullptr;
    JNIEnv* env = nullptr;
    if (g_jvm->GetEnv((void**)&env, JNI_VERSION_1_6) == JNI_EDETACHED) {
        g_jvm->AttachCurrentThread(&env, nullptr);
    }
    return env;
}

// ============================================================
//  Toast 回调（C → Java → JS）
// ============================================================

static void ShowToast(const char* text) {
    JNIEnv* env = GetJNIEnv();
    if (!env || !g_context) return;

    jclass cls = env->FindClass(OBFUSCATE("com/starry/magic/menu/MenuService"));
    if (!cls) return;

    jmethodID mid = env->GetStaticMethodID(cls, "getInstance",
        "()Lcom/starry/magic/menu/MenuService;");
    if (!mid) return;

    jobject service = env->CallStaticObjectMethod(cls, mid);
    if (!service) return;

    jmethodID toastMid = env->GetMethodID(cls, "showToast", "(Ljava/lang/String;)V");
    if (toastMid) {
        jstring jstr = env->NewStringUTF(text);
        env->CallVoidMethod(service, toastMid, jstr);
        env->DeleteLocalRef(jstr);
    }

    env->DeleteLocalRef(service);
    env->DeleteLocalRef(cls);
}

// 导出给 Games/*Mod（如失配保护）崩溃前提示复用
extern "C" void StarryShowToast(const char* text) { ShowToast(text); }

// ============================================================
//  后台作弊线程（参数化版；替代 legacy 每游戏一份的 #elif 轮询）
// ============================================================

static void* mod_thread(void* arg) {
    const StarryGameMod* mod = g_mod;
    if (!mod) return nullptr;
    LOGI("MOD 后台线程已启动 tag=%s", mod->tag);

    // ---- 1. 等待目标库（waitLib；nullptr = 纯壳直接放行）----
    if (mod->waitLib) {
        int waitCount = 0;
        while (!isLibraryLoaded(mod->waitLib)) {
            sleep(1);
            waitCount++;
            if ((waitCount % 5) == 0) {
                LOGI("等待游戏库 %s… %ds", mod->waitLib, waitCount);
            }
            if (waitCount >= 120) {
                LOGW("等待游戏库超时（120s），放行交由游戏侧 Tick 继续试");
                break;
            }
        }
    }
    g_gameLibLoaded = true;

    // ---- 2. 启动期初始化（initRetryMax=0 → 跳过，Tick 懒加载模式，如 gjc2 死锁案）----
    if (mod->initDelayMs > 0) usleep(mod->initDelayMs);
    if (mod->tryInit && mod->initRetryMax > 0) {
        int interval = mod->initRetryIntervalMs > 0 ? mod->initRetryIntervalMs : 2000;
        bool ok = false;
        for (int i = 0; i < mod->initRetryMax; i++) {
            // 兼容两类游戏：tryInit 返回 true 即成功；或 tryInit false 但 isReady true（fast-retry 线程装好）
            if (mod->tryInit(g_jvm) || (mod->isReady && mod->isReady())) { ok = true; break; }
            LOGI("[%s] 等待初始化，%dms 后重试 (%d/%d)", mod->tag, interval, i + 1, mod->initRetryMax);
            usleep(interval);
        }
        if (!ok) LOGE("[%s] 初始化超时（Tick 会继续尝试）", mod->tag);
        else LOGI("[%s] 初始化成功", mod->tag);
    }

    LOGI("MOD 已就绪 tag=%s ready=%d", mod->tag,
         mod->isReady ? (mod->isReady() ? 1 : 0) : -1);

    // ---- 3. 主循环：掉线清一次；在役则 tick ----
    int tickInterval = mod->tickIntervalMs > 0 ? mod->tickIntervalMs : 200000;
    static bool s_clearedOnDeadSession = false;
    while (true) {
        if (!session_live()) {
            if (!s_clearedOnDeadSession && mod->forceClear &&
                (!mod->isReady || mod->isReady())) {
                mod->forceClear();
            }
            s_clearedOnDeadSession = true;
            usleep(500000);
            continue;
        }
        s_clearedOnDeadSession = false;
        if (mod->tick) mod->tick(g_features, g_values);
        usleep(tickInterval);
    }

    return nullptr;
}

static void start_mod_thread() {
    pthread_t tid;
    pthread_create(&tid, nullptr, mod_thread, nullptr);
    pthread_detach(tid);
}

// ============================================================
//  JNI 方法实现（桥接面与 legacy Main.cpp 完全一致）
// ============================================================

extern "C" {

// ===== nativeInit =====
static void mb_nativeInit(JNIEnv* env, jclass clazz, jobject context) {
    if (g_initialized) return;

    LOGI("===== 星光魔法 MOD 菜单初始化（modern/%s）=====", g_mod ? g_mod->tag : "?");
    g_context = env->NewGlobalRef(context);
    g_initialized = true;

    start_mod_thread();
}

// ===== nativeSetFeature (开关) =====
static void mb_nativeSetFeature(JNIEnv* env, jclass clazz,
        jint featNum, jstring featName, jboolean enabled) {

    const char* name = env->GetStringUTFChars(featName, nullptr);
    // 未授权时禁止开启；关闭始终允许（用于 lock / OnFeatureChanged 回落）
    if (enabled && !session_live()) {
        LOGW("[开关] 拒绝开启 #%d %s（未授权）", featNum, name ? name : "");
        env->ReleaseStringUTFChars(featName, name);
        return;
    }
    if (featNum >= 0 && featNum < 64) {
        g_features[featNum] = enabled;
    }
    if (g_mod && g_mod->onFeatureChanged) g_mod->onFeatureChanged(featNum, enabled);
    LOGI("[开关] #%d %s = %s", featNum, name, enabled ? "ON" : "OFF");
    env->ReleaseStringUTFChars(featName, name);
}

// ===== nativeSetAuthorized（诱饵：true alone 不能放行功能） =====
static void mb_nativeSetAuthorized(JNIEnv* env, jclass clazz,
        jboolean authorized) {
    g_authorized = authorized;
    if (!authorized) {
        clear_runtime_session();
    }
    LOGI("[验证] flag=%s live=%s", authorized ? "true" : "false", session_live() ? "1" : "0");
}

// ===== 短票验签并激活运行时会话 =====
static jboolean mb_nativeApplyGrant(JNIEnv* env, jclass clazz,
        jstring jGrant, jstring jDevice, jstring jGame) {
    if (!jGrant || !jDevice || !jGame) return JNI_FALSE;
    const char* grant = env->GetStringUTFChars(jGrant, nullptr);
    const char* device = env->GetStringUTFChars(jDevice, nullptr);
    const char* game = env->GetStringUTFChars(jGame, nullptr);
    // 续票失败不得清活会话：登录后 session 永不过期（见 session_live 注释），
    // 一张坏票/过期票只代表本次续票失败，等下次续票重试，不把已开功能全清
    bool wasLive = session_live();
    bool ok = apply_grant_utf8(grant, device, game);
    env->ReleaseStringUTFChars(jGrant, grant);
    env->ReleaseStringUTFChars(jDevice, device);
    env->ReleaseStringUTFChars(jGame, game);
    if (!ok) {
        if (wasLive) {
            LOGW("[验证] grant rejected (session kept)");
        } else {
            clear_runtime_session();
            LOGW("[验证] grant rejected");
        }
        return JNI_FALSE;
    }
    return JNI_TRUE;
}

static jboolean mb_nativeHasLiveSession(JNIEnv* env, jclass clazz) {
    return session_live() ? JNI_TRUE : JNI_FALSE;
}

static void mb_nativeClearGrant(JNIEnv* env, jclass clazz) {
    clear_runtime_session();
}

// ===== OTA manifest 验签：payload = game_id|ver|json_sha|so_sha|note =====
// 与服务端 ota_store.sign_payload 一致（hmac_sha256(grant_hmac_secret, payload)）。
static jboolean mb_nativeVerifyOtaSig(JNIEnv* env, jclass clazz,
        jstring jPayload, jstring jSig) {
    if (!jPayload || !jSig) return JNI_FALSE;
    const char* payload = env->GetStringUTFChars(jPayload, nullptr);
    const char* sig = env->GetStringUTFChars(jSig, nullptr);
    if (!payload || !sig) {
        if (payload) env->ReleaseStringUTFChars(jPayload, payload);
        if (sig) env->ReleaseStringUTFChars(jSig, sig);
        return JNI_FALSE;
    }
    uint8_t mac[32];
    const char* pepper = grant_pepper();
    hmac_sha256((const uint8_t*)pepper, strlen(pepper),
                (const uint8_t*)payload, strlen(payload), mac);
    bool ok = hex_eq_ci(sig, mac, 32);
    env->ReleaseStringUTFChars(jPayload, payload);
    env->ReleaseStringUTFChars(jSig, sig);
    if (!ok) LOGW("[OTA] 补丁签名校验失败");
    return ok ? JNI_TRUE : JNI_FALSE;
}

// ===== OTA patch.json 应用：分发给 mod->onApplyPlayPatch =====
static void mb_nativeApplyPlayPatch(JNIEnv* env, jclass clazz,
        jstring jJson) {
    if (!jJson) return;
    const char* json = env->GetStringUTFChars(jJson, nullptr);
    if (!json || !*json) {
        if (json) env->ReleaseStringUTFChars(jJson, json);
        return;
    }
    if (g_mod && g_mod->onApplyPlayPatch) {
        g_mod->onApplyPlayPatch(json);
        LOGI("[OTA] 玩法 patch.json 已应用（%s）", g_mod->tag);
    } else {
        // 游戏未提供偏移热更入口：patch.json 下载后不改变 native 偏移。
        LOGW("[OTA] 当前游戏未实现偏移热更，patch.json 已忽略");
    }
    env->ReleaseStringUTFChars(jJson, json);
}

// ===== nativeSetValue (滑块/下拉) =====
static void mb_nativeSetValue(JNIEnv* env, jclass clazz,
        jint featNum, jstring featName, jint value) {

    const char* name = env->GetStringUTFChars(featName, nullptr);
    if (!session_live()) {
        LOGW("[数值] 拒绝 #%d（未授权）", featNum);
        env->ReleaseStringUTFChars(featName, name);
        return;
    }
    if (featNum >= 0 && featNum < 64) {
        g_values[featNum] = value;
    }
    if (g_mod && g_mod->onValueChanged) g_mod->onValueChanged(featNum, value);
    LOGI("[数值] #%d %s = %d", featNum, name, value);
    env->ReleaseStringUTFChars(featName, name);
}

// ===== nativeSetLongValue =====
static void mb_nativeSetLongValue(JNIEnv* env, jclass clazz,
        jint featNum, jstring featName, jlong value) {
    LOGI("[长整] #%d = %lld", featNum, (long long)value);
}

// ===== nativeSetText (输入框) =====
static void mb_nativeSetText(JNIEnv* env, jclass clazz,
        jint featNum, jstring featName, jstring text) {

    const char* name = env->GetStringUTFChars(featName, nullptr);
    const char* txt  = env->GetStringUTFChars(text, nullptr);

    if (!session_live()) {
        LOGW("[文本] 拒绝 #%d（未授权）", featNum);
        env->ReleaseStringUTFChars(featName, name);
        env->ReleaseStringUTFChars(text, txt);
        return;
    }
    if (featNum >= 0 && featNum < 64) {
        strncpy(g_texts[featNum], txt, sizeof(g_texts[featNum]) - 1);
    }
    if (g_mod && g_mod->onInputChanged) g_mod->onInputChanged(featNum, txt);
    LOGI("[文本] #%d %s = %s", featNum, name, txt);

    env->ReleaseStringUTFChars(featName, name);
    env->ReleaseStringUTFChars(text, txt);
}

// ===== nativeExecButton (执行按钮) =====
static void mb_nativeExecButton(JNIEnv* env, jclass clazz,
        jint featNum, jstring featName) {

    const char* name = env->GetStringUTFChars(featName, nullptr);
    if (!session_live()) {
        LOGW("[按钮] 拒绝 #%d %s（未授权）", featNum, name ? name : "");
        env->ReleaseStringUTFChars(featName, name);
        return;
    }
    if (featNum >= 0 && featNum < 64) {
        g_btnTrigger[featNum] = true;
    }
    // 按钮不走 nativeSetFeature，经 onFeatureChanged(featNum, true) 通知游戏模块
    if (g_mod && g_mod->onFeatureChanged) g_mod->onFeatureChanged(featNum, true);
    LOGI("[按钮] #%d %s", featNum, name);
    env->ReleaseStringUTFChars(featName, name);
}

// ===== nativeGetAuthGameId（mod->authGameId，替代 GameConfig.h 映射） =====
static jstring mb_nativeGetAuthGameId(JNIEnv* env, jclass clazz) {
    (void)clazz;
    if (!g_mod || !g_mod->authGameId) {
        LOGE("[桥接] authGameId 未配置（Entry 表缺失？）");
        return env->NewStringUTF("");
    }
    return env->NewStringUTF(g_mod->authGameId);
}

// ===== AceGate：legacy 雷霆专用倒计时已通用化为恒 0 =====
static jint mb_nativeGetAceGateRemainSec(JNIEnv* env, jclass clazz) {
    (void)env;
    (void)clazz;
    return 0;
}

// ===== nativeGetFeatureList（读 mod->featureList） =====
static jobjectArray mb_nativeGetFeatureList(JNIEnv* env, jclass clazz) {
    int count = g_mod ? g_mod->featureCount : 0;
    jclass stringClass = env->FindClass("java/lang/String");
    jobjectArray result = env->NewObjectArray(count > 0 ? count : 1, stringClass,
                                               env->NewStringUTF(""));
    if (g_mod && g_mod->featureList) {
        for (int i = 0; i < count; i++) {
            env->SetObjectArrayElement(result, i, env->NewStringUTF(g_mod->featureList[i]));
        }
    }
    return result;
}

// ===== nativeGetSettingsList =====
static jobjectArray mb_nativeGetSettingsList(JNIEnv* env, jclass clazz) {

    const char* settings[] = {
        "Category_界面设置",
        "Button_关闭菜单",
    };

    int count = sizeof(settings) / sizeof(settings[0]);
    jclass stringClass = env->FindClass("java/lang/String");
    jobjectArray result = env->NewObjectArray(count, stringClass,
                                               env->NewStringUTF(""));

    for (int i = 0; i < count; i++) {
        env->SetObjectArrayElement(result, i, env->NewStringUTF(settings[i]));
    }

    return result;
}

// ===== nativeIsGameLibLoaded =====
static jboolean mb_nativeIsGameLibLoaded(JNIEnv* env, jclass clazz) {
    return g_gameLibLoaded ? JNI_TRUE : JNI_FALSE;
}

// ===== nativeGetPinnedCert =====
// 返回编译期烘焙进 so 的证书固定公钥（PEM 字节）；AuthClient 不再读 assets。
static jbyteArray mb_nativeGetPinnedCert(JNIEnv* env, jclass clazz) {
    (void)clazz;
    if (!starry_pinned_crt || starry_pinned_crt_len == 0) {
        LOGE("[桥接] 证书烘焙数据为空");
        return nullptr;
    }
    jbyteArray arr = env->NewByteArray((jsize)starry_pinned_crt_len);
    if (!arr) return nullptr;
    env->SetByteArrayRegion(arr, 0, (jsize)starry_pinned_crt_len,
                            (const jbyte*)starry_pinned_crt);
    return arr;
}

// 2026-09-19 移除已废弃的 Preferences.Changes 静态 JNI 导出（Java 侧早已无声明、无调用点，
// 保留只会让 pre_install_gate 的静态 JNI 配套检查误报；将来需兼容旧包再按需恢复）。

// ============================================================
//  桥接注册（RegisterNatives，签名与 ModBridge.java 逐一对应）
// ============================================================
static bool g_bridge_registered_ok = false;

static bool regOne(JNIEnv* env, jclass clazz, const char* name, const char* sig, void* fn) {
    JNINativeMethod m{name, sig, fn};
    if (env->RegisterNatives(clazz, &m, 1) != 0) {
        LOGE("[桥接] 注册失败: %s %s", name, sig);
        return false;
    }
    return true;
}

static jboolean mb_bridgeRegistered() { return g_bridge_registered_ok ? JNI_TRUE : JNI_FALSE; }
static bool registerBridgeNatives(JNIEnv* env) {
    static bool done = false;
    if (done) return true;   // 可重入：dlopen 构造函数与 System.loadLibrary 都会进 JNI_OnLoad
    jclass bridge = env->FindClass(OBFUSCATE("com/starry/magic/bridge/ModBridge"));
    if (!bridge) { LOGE("[桥接] FindClass ModBridge 失败"); return false; }
    bool ok = true;
    ok = regOne(env, bridge, OBFUSCATE("nativeInit"),              OBFUSCATE("(Landroid/content/Context;)V"),                         (void*)mb_nativeInit) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeSetFeature"),        OBFUSCATE("(ILjava/lang/String;Z)V"),                              (void*)mb_nativeSetFeature) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeSetValue"),          OBFUSCATE("(ILjava/lang/String;I)V"),                              (void*)mb_nativeSetValue) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeSetLongValue"),      OBFUSCATE("(ILjava/lang/String;J)V"),                              (void*)mb_nativeSetLongValue) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeSetText"),           OBFUSCATE("(ILjava/lang/String;Ljava/lang/String;)V"),             (void*)mb_nativeSetText) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeExecButton"),        OBFUSCATE("(ILjava/lang/String;)V"),                               (void*)mb_nativeExecButton) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeGetFeatureList"),    OBFUSCATE("()[Ljava/lang/String;"),                                (void*)mb_nativeGetFeatureList) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeGetSettingsList"),   OBFUSCATE("()[Ljava/lang/String;"),                                (void*)mb_nativeGetSettingsList) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeIsGameLibLoaded"),   OBFUSCATE("()Z"),                                                  (void*)mb_nativeIsGameLibLoaded) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeSetAuthorized"),     OBFUSCATE("(Z)V"),                                                 (void*)mb_nativeSetAuthorized) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeApplyGrant"),        OBFUSCATE("(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)Z"), (void*)mb_nativeApplyGrant) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeHasLiveSession"),    OBFUSCATE("()Z"),                                                  (void*)mb_nativeHasLiveSession) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeClearGrant"),        OBFUSCATE("()V"),                                                  (void*)mb_nativeClearGrant) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeGetAuthGameId"),     OBFUSCATE("()Ljava/lang/String;"),                                 (void*)mb_nativeGetAuthGameId) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeGetAceGateRemainSec"), OBFUSCATE("()I"),                                                (void*)mb_nativeGetAceGateRemainSec) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeVerifyOtaSig"),      OBFUSCATE("(Ljava/lang/String;Ljava/lang/String;)Z"),              (void*)mb_nativeVerifyOtaSig) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeApplyPlayPatch"),    OBFUSCATE("(Ljava/lang/String;)V"),                                (void*)mb_nativeApplyPlayPatch) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeGetPinnedCert"),    OBFUSCATE("()[B"),                                                 (void*)mb_nativeGetPinnedCert) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeBridgeRegistered"),  OBFUSCATE("()Z"),                                                  (void*)mb_bridgeRegistered) && ok;
    if (ok) {
        done = true;
        g_bridge_registered_ok = true;
        LOGI("[桥接] RegisterNatives 19/19 全部成功");
    }
    return ok;
}

// ============================================================
//  JNI_OnLoad（游戏早期动作统一经 mod->onLoad）
// ============================================================

// 隐藏 API 豁免（JNI 直调 VMRuntime.setHiddenApiExemptions）。
// 部分 ROM（如 MuMu 硬化）会把 setHiddenApiExemptions 列入黑名单，
// Java 反射（含元反射）找不到 → SignSpoof 清 PackageInfo 缓存失败。
// JNI GetStaticMethodID 走同一条 hiddenapi 策略，失败则记录并继续
// （SignSpoof 的 PackageInfo.CREATOR 伪装不受影响，多数游戏仍可过签）。
static void TryExemptHiddenApis(JNIEnv* env) {
    if (!env) return;
    jclass vm = env->FindClass("dalvik/system/VMRuntime");
    if (!vm) {
        env->ExceptionClear();
        return;
    }
    jmethodID getRuntime = env->GetStaticMethodID(
        vm, "getRuntime", "()Ldalvik/system/VMRuntime;");
    jmethodID setExempt = env->GetStaticMethodID(
        vm, "setHiddenApiExemptions", "([Ljava/lang/String;)V");
    if (!getRuntime || !setExempt) {
        env->ExceptionClear();
        LOGW("[桥接] exempt hidden apis via JNI 失败（隐藏方法）");
        env->DeleteLocalRef(vm);
        return;
    }
    jobject runtime = env->CallStaticObjectMethod(vm, getRuntime);
    if (!runtime) {
        env->ExceptionClear();
        env->DeleteLocalRef(vm);
        return;
    }
    jclass strCls = env->FindClass("java/lang/String");
    jobjectArray arr = env->NewObjectArray(1, strCls, nullptr);
    jstring all = env->NewStringUTF("L");
    env->SetObjectArrayElement(arr, 0, all);
    env->CallVoidMethod(runtime, setExempt, arr);
    if (env->ExceptionCheck()) {
        env->ExceptionClear();
        LOGW("[桥接] exempt hidden apis via JNI 调用异常");
    } else {
        LOGI("[桥接] exempt hidden apis via JNI OK");
    }
    env->DeleteLocalRef(all);
    env->DeleteLocalRef(arr);
    env->DeleteLocalRef(strCls);
    env->DeleteLocalRef(runtime);
    env->DeleteLocalRef(vm);
}

JNIEXPORT jint JNICALL JNI_OnLoad(JavaVM* vm, void* reserved) {
    g_jvm = vm;
    {
        JNIEnv* env = nullptr;
        if (vm->GetEnv((void**)&env, JNI_VERSION_1_6) == JNI_OK && env) {
            TryExemptHiddenApis(env);                 // 先豁免，SignSpoof.install 后续才有用
            registerBridgeNatives(env);   // 失败可重试：下次 JNI_OnLoad（System.loadLibrary）再注册
        } else {
            LOGW("[桥接] JNI_OnLoad 无 JNIEnv，注册延后");
        }
    }
    if (g_mod && g_mod->onLoad) g_mod->onLoad(vm);
    LOGI("星光魔法 MOD 菜单已加载 (MainModern tag=%s)", g_mod ? g_mod->tag : "?");
    return JNI_VERSION_1_6;
}

// ============================================================
//  通用 CaveBoot（cave/晚载注入场景；合并 legacy LTZJ/WGDZZ 两份重复实现）
//  说明：app namespace 里 dlopen(libart) 会失败，用 xDL 从已映射库解析。
//  需要 dlopen constructor 自动兜底的游戏（如 wgdzz KwProtect K1 场景），
//  在自己的 Mod 文件里写 constructor 调本函数，框架不默认挂。
// ============================================================

static JavaVM* Starry_ResolveJvm() {
    typedef jint (*GetCreatedJavaVMs_t)(JavaVM**, jsize, jsize*);
    GetCreatedJavaVMs_t get_vms = nullptr;
    const char* candidates[] = {"libart.so", "libnativehelper.so", "libandroid_runtime.so", nullptr};
    for (int i = 0; candidates[i]; i++) {
        void* h = xdl_open(candidates[i], XDL_DEFAULT);
        if (!h) continue;
        get_vms = (GetCreatedJavaVMs_t)xdl_sym(h, "JNI_GetCreatedJavaVMs", nullptr);
        xdl_close(h);
        if (get_vms) break;
    }
    if (!get_vms) return nullptr;
    JavaVM* vm = nullptr;
    jsize n = 0;
    if (get_vms(&vm, 1, &n) != JNI_OK || !vm) return nullptr;
    return vm;
}

__attribute__((visibility("default"))) void Starry_CaveBoot(void) {
    LOGI("Starry_CaveBoot enter (modern)");
    JavaVM* vm = Starry_ResolveJvm();
    if (!vm) {
        LOGE("Starry_CaveBoot: no JavaVM");
        return;
    }
    g_jvm = vm;
    JNIEnv* env = nullptr;
    if (vm->GetEnv((void**)&env, JNI_VERSION_1_6) == JNI_OK && env) {
        registerBridgeNatives(env);
    }
    if (g_mod && g_mod->onLoad) g_mod->onLoad(vm);
    LOGI("星光魔法 MOD 菜单已加载 (CaveBoot modern tag=%s)", g_mod ? g_mod->tag : "?");
    start_mod_thread();
}

} // extern "C"
