/* ================================================================
 *  GyzwMod.cpp — 光阴之外（com.shbq.gyzw.ppyx 1.0.3 TapTap）星光菜单模块
 *
 *  引擎：Cocos Creator 3.x / libcocos2djs.so
 *  注入：Dobby hook se::ScriptEngine::evalString，对解密后的 boot JS
 *        做「尾部追加前缀 + 4 处锚点原地替换」，再交回原函数。
 *
 *  与 ZxrMod（镇邪人）同构，但走 modern 注册表路径：
 *    - 不 include GameConfig.h（那是 legacy 配置中心，会触发
 *      #error "Android.mk 须启用且仅启用一个 STARRY_GAME_*"）
 *    - 目标库名 / 符号名在本文件内自持
 *
 *  功能位号（= GyzwEntry.cpp featureList 下标）：
 *    1 百倍伤害  2 超级无敌  3 战斗加速(滑块1..10)  4 免广告
 * ================================================================ */

#include "GyzwMod.h"
#include "GyzwPatchJs.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <mutex>
#include <string>

#include <pthread.h>
#include <unistd.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/obfuscate.h"

#define GYZW_LOG "[Gyzw]"

static const char* kGyzwLib = "libcocos2djs.so";
static const char* kGyzwSeEvalSym = "_ZN2se12ScriptEngine10evalStringEPKclPNS_5ValueES2_";

// ============================================================
// ① 全局状态
// ============================================================
static JavaVM* g_gyzwJvm = nullptr;

static std::mutex g_hookMutex;
static std::mutex g_javaMutex;

static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_seHooked{false};
static std::atomic<bool> g_spliceDisabled{false};
static std::atomic<bool> g_dumped{false};

static std::atomic<int> g_spliceHits{0};
static std::atomic<int> g_seCalls{0};

// 战斗加速滑块 1..10（1 = 关）
static std::atomic<int> g_speedLvl{1};

static std::atomic<bool> g_dmg{false};
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_noAd{false};

static thread_local bool t_inEvalSplice = false;

// Java 侧缓存
static jclass g_bridgeCls = nullptr;
static jmethodID g_bridgeEval = nullptr;
static jclass g_hooksCls = nullptr;
static jmethodID g_hooksSetRet = nullptr;
static jmethodID g_hooksSetVal = nullptr;
static jmethodID g_hooksSetNoAd = nullptr;
static jmethodID g_hooksClear = nullptr;

// ============================================================
// ② JNI 辅助（绕开 native 线程 FindClass 失效）
// ============================================================
static JNIEnv* GetEnv(bool* needDetach) {
    if (!g_gyzwJvm) return nullptr;
    JNIEnv* env = nullptr;
    const jint st = g_gyzwJvm->GetEnv(reinterpret_cast<void**>(&env), JNI_VERSION_1_6);
    if (st == JNI_OK) {
        if (needDetach) *needDetach = false;
        return env;
    }
    if (st == JNI_EDETACHED) {
        if (g_gyzwJvm->AttachCurrentThread(&env, nullptr) != JNI_OK) return nullptr;
        if (needDetach) *needDetach = true;
        return env;
    }
    return nullptr;
}

static jclass LoadAppClass(JNIEnv* env, const char* name) {
    jclass at = env->FindClass("android/app/ActivityThread");
    if (!at) { env->ExceptionClear(); return nullptr; }

    jmethodID cam = env->GetStaticMethodID(at, "currentApplication", "()Landroid/app/Application;");
    if (!cam) { env->ExceptionClear(); return nullptr; }

    jobject app = env->CallStaticObjectMethod(at, cam);
    if (!app) { env->ExceptionClear(); return nullptr; }

    jclass ctxCls = env->FindClass("android/content/Context");
    if (!ctxCls) { env->ExceptionClear(); return nullptr; }
    jmethodID gcl = env->GetMethodID(ctxCls, "getClassLoader", "()Ljava/lang/ClassLoader;");
    if (!gcl) { env->ExceptionClear(); return nullptr; }

    jobject cl = env->CallObjectMethod(app, gcl);
    if (!cl) { env->ExceptionClear(); return nullptr; }

    jclass clCls = env->FindClass("java/lang/ClassLoader");
    jmethodID loadClass = env->GetMethodID(clCls, "loadClass", "(Ljava/lang/String;)Ljava/lang/Class;");
    if (!loadClass) { env->ExceptionClear(); return nullptr; }

    jstring jn = env->NewStringUTF(name);
    jobject k = env->CallObjectMethod(cl, loadClass, jn);
    if (env->ExceptionCheck()) { env->ExceptionClear(); return nullptr; }
    return reinterpret_cast<jclass>(k);
}

static void CacheJava(JNIEnv* env) {
    std::lock_guard<std::mutex> lk(g_javaMutex);
    if (g_bridgeCls && g_hooksCls) return;

    if (!g_bridgeCls) {
        jclass c = LoadAppClass(env, "com.starry.magic.cocos.CocosJsBridge");
        if (!c) c = env->FindClass(OBFUSCATE("com/starry/magic/cocos/CocosJsBridge"));
        if (c) {
            jmethodID m = env->GetStaticMethodID(c, "evalOnGLThread", "(Ljava/lang/String;)V");
            if (m) {
                g_bridgeEval = m;
                g_bridgeCls = reinterpret_cast<jclass>(env->NewGlobalRef(c));
            }
            env->DeleteLocalRef(c);
        }
        if (env->ExceptionCheck()) env->ExceptionClear();
    }

    if (!g_hooksCls) {
        jclass c = LoadAppClass(env, "com.starry.magic.cocos.GyzwHooks");
        if (!c) c = env->FindClass(OBFUSCATE("com/starry/magic/cocos/GyzwHooks"));
        if (c) {
            g_hooksSetRet  = env->GetStaticMethodID(c, "setRet", "(IZ)V");
            g_hooksSetVal  = env->GetStaticMethodID(c, "setVal", "(II)V");
            g_hooksSetNoAd = env->GetStaticMethodID(c, "setNoAd", "(Z)V");
            g_hooksClear   = env->GetStaticMethodID(c, "clearAll", "()V");
            if (g_hooksSetRet && g_hooksSetVal) {
                g_hooksCls = reinterpret_cast<jclass>(env->NewGlobalRef(c));
            }
            env->DeleteLocalRef(c);
        }
        if (env->ExceptionCheck()) env->ExceptionClear();
    }
}

static void EvalJs(const char* js) {
    if (!js || !g_gyzwJvm) return;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    CacheJava(env);
    if (g_bridgeCls && g_bridgeEval) {
        jstring jjs = env->NewStringUTF(js);
        env->CallStaticVoidMethod(g_bridgeCls, g_bridgeEval, jjs);
        env->DeleteLocalRef(jjs);
    }
    if (env->ExceptionCheck()) env->ExceptionClear();
    if (detach) g_gyzwJvm->DetachCurrentThread();
}

static void SyncJavaRets() {
    if (!g_gyzwJvm) return;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    CacheJava(env);
    if (g_hooksCls && g_hooksSetRet) {
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, (jint)GYZW_FEAT_DMG,
                                  g_dmg.load() ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, (jint)GYZW_FEAT_INV,
                                  g_inv.load() ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, (jint)GYZW_FEAT_SPEED,
                                  g_speedLvl.load() > 1 ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, (jint)GYZW_FEAT_NOAD,
                                  g_noAd.load() ? JNI_TRUE : JNI_FALSE);
        if (g_hooksSetVal) {
            env->CallStaticVoidMethod(g_hooksCls, g_hooksSetVal, (jint)GYZW_FEAT_SPEED,
                                      (jint)g_speedLvl.load());
        }
        if (g_hooksSetNoAd) {
            env->CallStaticVoidMethod(g_hooksCls, g_hooksSetNoAd, g_noAd.load() ? JNI_TRUE : JNI_FALSE);
        }
    }
    if (env->ExceptionCheck()) env->ExceptionClear();
    if (detach) g_gyzwJvm->DetachCurrentThread();
}

// 把开关镜像进 JS 全局（免 JNI 抖动 + 热更兜底）
static void SyncJsState() {
    char js[512];
    snprintf(js, sizeof(js),
             "(function(){try{var g=typeof globalThis!=='undefined'?globalThis:window;"
             "var s=g.__starryGyzwState=g.__starryGyzwState||{};"
             "s.ret=s.ret||{};s.val=s.val||{};"
             "s.ret[1]=%d;s.ret[2]=%d;s.ret[3]=%d;s.ret[4]=%d;s.val[3]=%d;"
             "g.__starryGyzwOn=g.__starryGyzwOn||function(n){return !!s.ret[n];};"
             "g.__starryGyzwVal=g.__starryGyzwVal||function(n){var v=s.val[n]|0;return v>0?v:1;};"
             "}catch(e){}})();",
             g_dmg.load() ? 1 : 0, g_inv.load() ? 1 : 0,
             g_speedLvl.load() > 1 ? 1 : 0, g_noAd.load() ? 1 : 0,
             g_speedLvl.load());
    EvalJs(js);
}

// ============================================================
// ③ 脚本工具
// ============================================================
static bool MemHas(const char* data, size_t size, const char* needle) {
    if (!data || !needle || !*needle) return false;
    return memmem(data, size, needle, strlen(needle)) != nullptr;
}

static bool ReplaceRequired(std::string& script, const char* anchor,
                            const char* replacement, const char* name) {
    const size_t pos = script.find(anchor);
    if (pos == std::string::npos) {
        LOGW(GYZW_LOG " anchor missing: %s", name);
        return false;
    }
    if (script.find(anchor, pos + 1) != std::string::npos) {
        // 锚点出现多次：无法确定唯一替换点，跳过（宁缺勿错）
        LOGW(GYZW_LOG " anchor ambiguous (%s) x%d", name, 2);
        return false;
    }
    script.replace(pos, strlen(anchor), replacement);
    return true;
}

// 捕获脚本内 `X.BtConst.EUnitType.` 的模块别名 X。
// 先在锚点附近（前后各 64KB 窗口）找；找不到再全脚本按最高频次兜底。
static bool CaptureAlias(const std::string& script, size_t anchorPos, std::string* out) {
    static const char* kNeedle = ".BtConst.EUnitType.";
    const size_t kNeedleLen = strlen(kNeedle);
    const size_t kWin = 64 * 1024;

    auto identBefore = [&](size_t dotPos, std::string* id) -> bool {
        size_t i = dotPos;
        while (i > 0) {
            const char c = script[i - 1];
            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
                (c >= '0' && c <= '9') || c == '_' || c == '$') {
                i--;
            } else {
                break;
            }
        }
        if (i == dotPos) return false;
        *id = script.substr(i, dotPos - i);
        return !id->empty();
    };

    // 前向（锚点之后最近）
    std::string fwd;
    const size_t from = anchorPos;
    const size_t to = (anchorPos + kWin < script.size()) ? anchorPos + kWin : script.size();
    size_t p = script.find(kNeedle, from);
    if (p != std::string::npos && p < to) identBefore(p, &fwd);

    // 后向（锚点之前最近）
    std::string bwd;
    const size_t lo = (anchorPos > kWin) ? anchorPos - kWin : 0;
    size_t q = script.rfind(kNeedle, anchorPos);
    if (q != std::string::npos && q >= lo) identBefore(q, &bwd);

    if (!fwd.empty() && !bwd.empty()) {
        if (fwd == bwd) { *out = fwd; return true; }
        // 不一致时取距离更近的一条
        *out = (p - anchorPos <= anchorPos - q) ? fwd : bwd;
        LOGW(GYZW_LOG " alias ambiguous fwd=%s bwd=%s, pick %s",
             fwd.c_str(), bwd.c_str(), out->c_str());
        return true;
    }
    if (!fwd.empty()) { *out = fwd; return true; }
    if (!bwd.empty()) { *out = bwd; return true; }

    // 全脚本兜底：按出现频次取最高的别名
    std::string best;
    int bestHits = 0;
    size_t i = 0;
    while ((i = script.find(kNeedle, i)) != std::string::npos) {
        std::string id;
        if (identBefore(i, &id)) {
            if (id == best) {
                bestHits++;
            } else if (best.empty()) {
                best = id;
                bestHits = 1;
            }
        }
        i += kNeedleLen;
    }
    if (!best.empty()) {
        LOGW(GYZW_LOG " alias by-frequency %s (%d)", best.c_str(), bestHits);
        *out = best;
        return true;
    }
    return false;
}

static bool BuildHotPatch(const char* src, size_t size, std::string* out) {
    std::string script;
    script.reserve(size + 4096);
    script.append(src, size);

    // 1) 捕获 BtConst 模块别名
    const size_t a1 = script.find(kGyzwAnchor1);
    std::string alias;
    if (a1 == std::string::npos || !CaptureAlias(script, a1, &alias)) {
        LOGW(GYZW_LOG " BtConst alias capture failed — abort splice");
        return false;
    }

    char repl1[1024];
    snprintf(repl1, sizeof(repl1), kGyzwRepl1Fmt, alias.c_str());
    char repl2[1024];
    snprintf(repl2, sizeof(repl2), kGyzwRepl2Fmt, alias.c_str());
    char prefix[4096];
    snprintf(prefix, sizeof(prefix), kGyzwPatchPrefixFmt, alias.c_str());

    // 2) 四处锚点全部成功才生效（任一失败 = 版本漂移，整体放弃）
    if (!ReplaceRequired(script, kGyzwAnchor1, repl1, "dmg")) return false;
    if (!ReplaceRequired(script, kGyzwAnchor2, repl2, "inv")) return false;
    if (!ReplaceRequired(script, kGyzwAnchor3, kGyzwRepl3, "speed")) return false;
    if (!ReplaceRequired(script, kGyzwAnchor4, kGyzwRepl4, "logicstep")) return false;

    // 3) 追加前缀
    script.append("\n");
    script.append(prefix);

    out->swap(script);
    return true;
}

// ============================================================
// ④ boot JS 落盘（一次性，便于离线核对锚点）
// ============================================================
static bool AppFilesDir(char* out, size_t cap) {
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return false;
    bool ok = false;
    do {
        jclass at = env->FindClass("android/app/ActivityThread");
        if (!at) { env->ExceptionClear(); break; }
        jmethodID cam = env->GetStaticMethodID(at, "currentApplication", "()Landroid/app/Application;");
        if (!cam) { env->ExceptionClear(); break; }
        jobject app = env->CallStaticObjectMethod(at, cam);
        if (!app) { env->ExceptionClear(); break; }
        jclass ctx = env->FindClass("android/content/Context");
        if (!ctx) { env->ExceptionClear(); break; }
        jmethodID gfd = env->GetMethodID(ctx, "getFilesDir", "()Ljava/io/File;");
        if (!gfd) { env->ExceptionClear(); break; }
        jobject f = env->CallObjectMethod(app, gfd);
        if (!f) { env->ExceptionClear(); break; }
        jclass fc = env->FindClass("java/io/File");
        jmethodID gap = env->GetMethodID(fc, "getAbsolutePath", "()Ljava/lang/String;");
        if (!gap) { env->ExceptionClear(); break; }
        jstring s = reinterpret_cast<jstring>(env->CallObjectMethod(f, gap));
        if (!s) { env->ExceptionClear(); break; }
        const char* c = env->GetStringUTFChars(s, nullptr);
        if (c) {
            snprintf(out, cap, "%s", c);
            env->ReleaseStringUTFChars(s, c);
            ok = true;
        }
        if (env->ExceptionCheck()) env->ExceptionClear();
    } while (false);
    if (detach) g_gyzwJvm->DetachCurrentThread();
    return ok;
}

static void DumpBootJsOnce(const char* src, size_t size) {
    if (g_dumped.exchange(true)) return;
    char dir[512];
    if (!AppFilesDir(dir, sizeof(dir))) {
        LOGW(GYZW_LOG " dump: filesDir unavailable");
        return;
    }
    char path[640];
    snprintf(path, sizeof(path), "%s/gyzw_boot.js", dir);
    if (access(path, F_OK) == 0) {
        LOGI(GYZW_LOG " dump: already exists %s", path);
        return;
    }
    FILE* fp = fopen(path, "wb");
    if (!fp) {
        LOGW(GYZW_LOG " dump: open fail %s", path);
        return;
    }
    const size_t w = fwrite(src, 1, size, fp);
    fclose(fp);
    LOGI(GYZW_LOG " dump: wrote %s (%zu/%zu)", path, w, size);
}

// ============================================================
// ⑤ evalString hook（5 参，末参 fileName）
// ============================================================
typedef bool (*SeEvalString_t)(void* thiz, const char* script, long length,
                               void* ret, const char* fileName);
static SeEvalString_t orig_seEval = nullptr;

static bool hooked_seEval(void* thiz, const char* script, long length,
                          void* ret, const char* fileName) {
    if (!orig_seEval) return false;
    if (!script || t_inEvalSplice || g_spliceDisabled.load()) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    long n = length;
    if (n < 0) n = static_cast<long>(strlen(script));
    g_seCalls.fetch_add(1);

    // 窗口过滤：boot JS 约 1.87MB；1MB..48MB 之外的脚本一律直通
    if (n < 1024 * 1024 || n > 48 * 1024 * 1024) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }
    // 已打过补丁则直通（幂等）
    if (MemHas(script, static_cast<size_t>(n), kGyzwPatchMarker)) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }
    // 目标脚本指纹：三条锚点串须齐全
    if (!MemHas(script, static_cast<size_t>(n), kGyzwSign1) ||
        !MemHas(script, static_cast<size_t>(n), kGyzwSign2) ||
        !MemHas(script, static_cast<size_t>(n), kGyzwSign3)) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    DumpBootJsOnce(script, static_cast<size_t>(n));

    std::string patched;
    if (!BuildHotPatch(script, static_cast<size_t>(n), &patched)) {
        g_spliceDisabled.store(true);
        LOGW(GYZW_LOG " splice disabled: anchor set incomplete");
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    const int hit = g_spliceHits.fetch_add(1) + 1;
    LOGI(GYZW_LOG " splice #%d len=%ld->%zu file=%s", hit, n, patched.size(),
         fileName ? fileName : "(null)");

    t_inEvalSplice = true;
    const bool ok = orig_seEval(thiz, patched.c_str(), (long)patched.size(), ret, fileName);
    t_inEvalSplice = false;

    if (!ok) {
        g_spliceDisabled.store(true);
        LOGW(GYZW_LOG " splice eval failed — disabled");
    }
    return ok;
}

static bool TryHookEvalString() {
    std::lock_guard<std::mutex> lk(g_hookMutex);
    if (g_seHooked.load()) return true;
    if (!isLibraryLoaded(kGyzwLib)) return false;

    void* sym = DobbySymbolResolver(kGyzwLib, kGyzwSeEvalSym);
    if (!sym) {
        LOGW(GYZW_LOG " DobbySymbolResolver miss %s", kGyzwSeEvalSym);
        return false;
    }
    if (DobbyHook(sym, reinterpret_cast<dobby_dummy_func_t>(hooked_seEval),
                  reinterpret_cast<dobby_dummy_func_t*>(&orig_seEval)) != 0) {
        LOGW(GYZW_LOG " DobbyHook fail @%p", sym);
        return false;
    }
    g_seHooked.store(true);
    LOGI(GYZW_LOG " hooked se::evalString @%p (hot-update splice)", sym);
    return true;
}

// ============================================================
// ⑥ 生命周期
// ============================================================
bool Gyzw_TryInit(JavaVM* vm) {
    if (vm) g_gyzwJvm = vm;
    if (g_ready.load()) return true;
    if (!isLibraryLoaded(kGyzwLib)) return false;

    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (detach) g_gyzwJvm->DetachCurrentThread();
    }

    if (!TryHookEvalString()) return false;

    g_ready.store(true);
    SyncJavaRets();
    SyncJsState();
    LOGI(GYZW_LOG " ready (lib=%s)", kGyzwLib);
    return true;
}

bool Gyzw_IsReady() { return g_ready.load(); }

static void* EarlyHookThread(void* arg) {
    JavaVM* vm = reinterpret_cast<JavaVM*>(arg);
    if (vm) g_gyzwJvm = vm;
    for (int i = 0; i < 3000; i++) {
        if (isLibraryLoaded(kGyzwLib) && TryHookEvalString()) {
            g_ready.store(true);
            SyncJavaRets();
            SyncJsState();
            LOGI(GYZW_LOG " early hook ready iter=%d", i);
            return nullptr;
        }
        usleep(20 * 1000);
    }
    LOGW(GYZW_LOG " early hook timeout");
    return nullptr;
}

void Gyzw_StartEarlyHookThread(JavaVM* vm) {
    static std::atomic<bool> started{false};
    bool expect = false;
    if (!started.compare_exchange_strong(expect, true)) return;
    if (vm) g_gyzwJvm = vm;
    pthread_t th;
    if (pthread_create(&th, nullptr, EarlyHookThread, vm) == 0) {
        pthread_detach(th);
        LOGI(GYZW_LOG " early hook thread started");
    }
}

void Gyzw_OnLoad(JavaVM* vm) { Gyzw_StartEarlyHookThread(vm); }

void Gyzw_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case GYZW_FEAT_DMG:
            g_dmg.store(enabled);
            break;
        case GYZW_FEAT_INV:
            g_inv.store(enabled);
            break;
        case GYZW_FEAT_SPEED:
            if (!enabled) g_speedLvl.store(1);
            break;
        case GYZW_FEAT_NOAD:
            g_noAd.store(enabled);
            break;
        default:
            break;
    }
    if (g_ready.load()) {
        SyncJavaRets();
        SyncJsState();
    }
    LOGI(GYZW_LOG " feature %d -> %d (spd=%d)", featNum, enabled ? 1 : 0, g_speedLvl.load());
}

void Gyzw_OnValueChanged(int featNum, int value) {
    if (featNum != GYZW_FEAT_SPEED) return;
    int v = value;
    if (v < 1) v = 1;
    if (v > 10) v = 10;
    const int prev = g_speedLvl.exchange(v);
    if (prev == v) return;
    if (g_ready.load()) {
        SyncJavaRets();
        SyncJsState();
    }
    LOGI(GYZW_LOG " speed level -> %d", v);
}

void Gyzw_ForceClear() {
    g_dmg.store(false);
    g_inv.store(false);
    g_noAd.store(false);
    g_speedLvl.store(1);
    if (g_ready.load() && g_gyzwJvm) {
        bool detach = false;
        JNIEnv* env = GetEnv(&detach);
        if (env) {
            CacheJava(env);
            if (g_hooksCls && g_hooksClear) {
                env->CallStaticVoidMethod(g_hooksCls, g_hooksClear);
            }
            if (env->ExceptionCheck()) env->ExceptionClear();
            if (detach) g_gyzwJvm->DetachCurrentThread();
        }
        SyncJavaRets();
        SyncJsState();
    }
}

void Gyzw_Tick(const bool* features, const int* values) {
    if (features) {
        g_dmg.store(features[GYZW_FEAT_DMG]);
        g_inv.store(features[GYZW_FEAT_INV]);
        g_noAd.store(features[GYZW_FEAT_NOAD]);
        if (!features[GYZW_FEAT_SPEED]) g_speedLvl.store(1);
    }
    if (values) {
        int v = values[GYZW_FEAT_SPEED];
        if (v < 1) v = 1;
        if (v > 10) v = 10;
        g_speedLvl.store(v);
    }
    // noAd 每次 Tick 重同步（热更 / 重登自愈）
    static bool lastNoAd = false;
    const bool na = g_noAd.load();
    if (na != lastNoAd) {
        lastNoAd = na;
        SyncJavaRets();
    }
    if (!g_ready.load() && isLibraryLoaded(kGyzwLib)) {
        Gyzw_TryInit(g_gyzwJvm);
    }
}
