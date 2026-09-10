/* ================================================================
 *  无尽探险队 MOD — 对齐斌哥注入 JS（get_ret 1..2）
 *  Cocos Creator 3.8.75 / libUnityV114514.so（com.cocos.lib 新栈）
 *
 *  斌哥抓点（09_test/无尽探险队/sdk_bss.bin）：
 *    1 超级无敌：get invulnerable(){...get_ret(1) ON → return true}
 *    2 超级移速：getFrameSpeed(){...get_ret(2) ON → return i*=delta*5}
 *  类型限定 [1,4,5,7]（玩家角色 _type）—— 与斌哥一致。
 *
 *  注入通道：hook JNI 导出 Java_com_cocos_lib_CocosJavascriptJavaBridge_evalString
 *    （Creator 3 唯一稳定导出；se::ScriptEngine::evalString 被 strip，尝试解析失败降级）
 *  在 JNI 拿到的明文脚本上做文本 splice（锚点插入 get_ret 检测段）。
 * ================================================================ */

#include "WjtxdMod.h"
#include "Includes/obfuscate.h"

#include <algorithm>
#include <atomic>
#include <cstdio>
#include <cstring>
#include <dlfcn.h>
#include <pthread.h>
#include <string>
#include <unistd.h>

#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Dobby/dobby.h"

static JavaVM* g_wjtxdJvm = nullptr;
static bool g_ready = false;
static bool g_seHooked = false;
static bool g_jniHooked = false;
static thread_local bool g_inEvalSplice = false;

static jclass g_hooksCls = nullptr;
static jmethodID g_hooksSetRet = nullptr;
static jmethodID g_hooksClear = nullptr;

static volatile bool g_inv = false;
static volatile bool g_spd = false;
static volatile bool g_adfree = false;
static std::atomic<int> g_spliceHits{0};

// 本游戏引擎库：官方与斌哥包同名 libUnityV114514.so（Creator 3.8.75 / com.cocos.lib）
static const char* kTargetLib = OBFUSCATE("libUnityV114514.so");
// JNI 桥导出符号（Creator 3 新栈；se::ScriptEngine::evalString 被 strip，改按 RVA）
static const char* kJniEvalSym = OBFUSCATE("Java_com_cocos_lib_CocosJavascriptJavaBridge_evalString");

static JNIEnv* GetEnv(bool* needDetach) {
    *needDetach = false;
    if (!g_wjtxdJvm) return nullptr;
    JNIEnv* env = nullptr;
    int st = g_wjtxdJvm->GetEnv((void**)&env, JNI_VERSION_1_6);
    if (st == JNI_EDETACHED) {
        if (g_wjtxdJvm->AttachCurrentThread(&env, nullptr) != 0) return nullptr;
        *needDetach = true;
    } else if (st != JNI_OK) {
        return nullptr;
    }
    return env;
}

static jclass LoadAppClass(JNIEnv* env, const char* name) {
    jclass atCls = env->FindClass("android/app/ActivityThread");
    if (!atCls) {
        env->ExceptionClear();
        return nullptr;
    }
    jmethodID curApp = env->GetStaticMethodID(
        atCls, "currentApplication", "()Landroid/app/Application;");
    if (!curApp) {
        env->ExceptionClear();
        env->DeleteLocalRef(atCls);
        return nullptr;
    }
    jobject app = env->CallStaticObjectMethod(atCls, curApp);
    env->DeleteLocalRef(atCls);
    if (!app) {
        env->ExceptionClear();
        return nullptr;
    }
    jclass appCls = env->GetObjectClass(app);
    jmethodID getCl = env->GetMethodID(appCls, "getClassLoader", "()Ljava/lang/ClassLoader;");
    jobject cl = getCl ? env->CallObjectMethod(app, getCl) : nullptr;
    env->DeleteLocalRef(appCls);
    env->DeleteLocalRef(app);
    if (!cl) {
        env->ExceptionClear();
        return nullptr;
    }
    jclass clCls = env->GetObjectClass(cl);
    jmethodID loadClass =
        env->GetMethodID(clCls, "loadClass", "(Ljava/lang/String;)Ljava/lang/Class;");
    jstring jname = env->NewStringUTF(name);
    jclass cls = nullptr;
    if (loadClass && jname) {
        cls = (jclass)env->CallObjectMethod(cl, loadClass, jname);
        if (env->ExceptionCheck()) {
            env->ExceptionClear();
            cls = nullptr;
        }
    }
    if (jname) env->DeleteLocalRef(jname);
    env->DeleteLocalRef(clCls);
    env->DeleteLocalRef(cl);
    return cls;
}

static void CacheJava(JNIEnv* env) {
    if (g_hooksCls) return;
    jclass local = LoadAppClass(env, "com.starry.magic.cocos.WjtxdHooks");
    if (!local) local = env->FindClass(OBFUSCATE("com/starry/magic/cocos/WjtxdHooks"));
    if (local) {
        g_hooksCls = (jclass)env->NewGlobalRef(local);
        g_hooksSetRet = env->GetStaticMethodID(g_hooksCls, "setRet", "(IZ)V");
        g_hooksClear = env->GetStaticMethodID(g_hooksCls, "clearAll", "()V");
        env->DeleteLocalRef(local);
    } else {
        env->ExceptionClear();
    }
}

static void SyncJavaRets() {
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    CacheJava(env);
    if (g_hooksCls && g_hooksSetRet) {
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 1, g_inv ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 2, g_spd ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 3, g_adfree ? JNI_TRUE : JNI_FALSE);
    }
    if (detach) g_wjtxdJvm->DetachCurrentThread();
}

// 对齐斌哥 BSS 注入 JS（get_ret 改读星光 Java 桥 WjtxdHooks）
static std::string SpliceCombatJs(const char* src) {
    std::string s(src);

    static char kInvHook[600];
    static char kSpdHook[700];
    static char kAdHook[500];
    static bool once = false;
    if (!once) {
        // get_ret(1) 超级无敌 — 锚 get invulnerable(){ 之后（斌哥同款）
        snprintf(
            kInvHook, sizeof(kInvHook),
            "/*starry_wjtxd_inv*/var isChecked_RET_sw=jsb.reflection.callStaticMethod("
            "\"com.starry.magic.cocos.WjtxdHooks\",\"getRet\",\"(I)Z\",1);"
            "if(isChecked_RET_sw){if([1,4,5,7].includes(this._type)){return true;}}");
        // get_ret(2) 超级移速 — 锚 getFrameSpeed 内 let i=deltaTime 行后（斌哥同款 ×5）
        snprintf(
            kSpdHook, sizeof(kSpdHook),
            "/*starry_wjtxd_spd*/var isChecked_RET_sw=jsb.reflection.callStaticMethod("
            "\"com.starry.magic.cocos.WjtxdHooks\",\"getRet\",\"(I)Z\",2);"
            "if(isChecked_RET_sw){if([1,4,5,7].includes(this._type)){"
            "return i*=d.battleManager.golbalTimeScale,e*i*5;}}");
        // get_ret(3) 免广告 — 广告调用点换星光 fake（不触发真广告，直接回发 success）
        // ⚠ 注入点位于逗号表达式内（console.log(...), sendToNative(...)），禁止以
        // var 开头（`a,var b=1` 语法错误导致 openadsdk 注册失败卡加载，2026-09-11 实测）。
        // 必须 IIFE：console.log(...),(function(){...})() 才是合法逗号表达式。
        snprintf(
            kAdHook, sizeof(kAdHook),
            "/*starry_wjtxd_ad*/(function(){var sw_adf=jsb.reflection.callStaticMethod("
            "\"com.starry.magic.cocos.WjtxdHooks\",\"getRet\",\"(I)Z\",3);"
            "if(sw_adf){jsb.reflection.callStaticMethod("
            "\"com.starry.magic.cocos.WjtxdHooks\",\"fakeAd\",\"()V\");}else{"
            "b.bridge.sendToNative(\"adVideo\")}})()");
        once = true;
    }

    bool hit = false;

    // 免广告：sendToNative("adVideo") → 包里已注入 JS 段（getRet(3) 运行时决定 fake/真广告）
    // 必须无条件 splice：index.jsc 只在启动 eval 一次，若 gate 在 g_adfree 则开关开启后
    // 不会再来 eval（已驻内存），广告点永远不被替换（2026-09-11 实测 adVideo 仍被拉起）。
    // 注入段内部用 getRet(3) 判断，开关 ON→fakeAd，OFF→原样 sendToNative。
    {
        const char* adAnchor = "b.bridge.sendToNative(\"adVideo\")";
        size_t p = s.find(adAnchor);
        if (p != std::string::npos) {
            size_t end = p + strlen(adAnchor);
            if (s.find("starry_wjtxd_ad", end) == std::string::npos ||
                s.find("starry_wjtxd_ad", end) >= end + 300) {
                s.replace(p, strlen(adAnchor), kAdHook);
                hit = true;
                LOGI("[Wjtxd] spliced adVideo -> starry ad gate");
            }
        }
    }

    // 无敌：get invulnerable(){ → 后插检测（幂等：已带 starry_wjtxd_inv 跳过）
    {
        const char* anchor = "get invulnerable(){";
        size_t p = s.find(anchor);
        if (p != std::string::npos) {
            size_t end = p + strlen(anchor);
            if (s.find("starry_wjtxd_inv", end) == std::string::npos ||
                s.find("starry_wjtxd_inv", end) >= end + 300) {
                s.insert(end, kInvHook);
                hit = true;
                LOGI("[Wjtxd] spliced inv after 'get invulnerable(){'");
            }
        }
    }

    // 移速：let i=l.deltaTime>.04?.04:l.deltaTime; 后插检测（幂等）
    {
        const char* anchors[] = {
            "let i=l.deltaTime>.04?.04:l.deltaTime;",
            "let i=l.deltaTime>.04?.04:l.deltaTime;",
            nullptr,
        };
        for (int i = 0; anchors[i]; i++) {
            size_t p = s.find(anchors[i]);
            if (p == std::string::npos) continue;
            size_t end = p + strlen(anchors[i]);
            if (s.find("starry_wjtxd_spd", end) != std::string::npos &&
                s.find("starry_wjtxd_spd", end) < end + 300)
                break;
            s.insert(end, kSpdHook);
            hit = true;
            LOGI("[Wjtxd] spliced spd after 'let i=l.deltaTime...'");
            break;
        }
    }

    if (!hit && s.find("getFrameSpeed") == std::string::npos &&
        s.find("get invulnerable") == std::string::npos) {
        // 非战斗脚本，原样返回（避免无谓日志）
        return s;
    }
    return hit ? s : s;
}

typedef jint (*JniEvalString_t)(JNIEnv*, jclass, jstring);
static JniEvalString_t orig_jniEval = nullptr;
static std::atomic<int> g_jniCalls{0};

typedef bool (*SeEvalString_t)(void*, const char*, long, void*, const char*);
static SeEvalString_t orig_seEval = nullptr;
static std::atomic<int> g_seCalls{0};

// Creator 3 se::ScriptEngine::evalString（libUnityV114514.so RVA，斌哥同款注入点）
// 特征：函数体引用 rodata "ScriptEngine::evalString catch exception:"（0x1bc3ce）
// 序言 d10503ff（sub sp,#0x140）。2026-09-10 官方 26.1.5 dump 定位。
static const uintptr_t kSeEvalRva = 0xC086D8;
static const uint32_t kSeEvalProlog = 0xD10503FF;

// 前向声明：hooked_seEval 定义在 WjtxdHookSeEval 之后
static bool hooked_seEval(void* thiz, const char* script, long length, void* ret,
                          const char* fileName);

// file_off → 运行时 VA：只在 RX 段找（多段映射，base+RVA 会挂错）
static void* WjtxdFileOffToVa(const char* lib, uintptr_t fileOff) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return nullptr;
    char line[768];
    void* found = nullptr;
    size_t bestSpan = 0;
    while (fgets(line, sizeof(line), fp)) {
        if (!strstr(line, lib) || !strstr(line, ".so")) continue;
        unsigned long start = 0, end = 0, off = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (!strchr(perm, 'x') || end <= start) continue;
        if (fileOff < off || fileOff - off >= static_cast<uintptr_t>(end - start)) continue;
        const size_t span = static_cast<size_t>(end - start);
        if (span > bestSpan) {
            bestSpan = span;
            found = reinterpret_cast<void*>(start + (fileOff - off));
        }
    }
    fclose(fp);
    return found;
}

static bool WjtxdHookSeEval() {
    if (g_seHooked) return true;
    void* va = WjtxdFileOffToVa(kTargetLib, kSeEvalRva);
    if (!va) {
        LOGW("[Wjtxd] se eval rva 0x%lX not in RX", (unsigned long)kSeEvalRva);
        return false;
    }
    uint32_t got = 0;
    memcpy(&got, va, sizeof(got));
    if (got != kSeEvalProlog) {
        LOGW("[Wjtxd] se eval prolog drift got=0x%08X want=0x%08X", got, kSeEvalProlog);
        return false;
    }
    if (DobbyHook(va, (dobby_dummy_func_t)hooked_seEval,
                  (dobby_dummy_func_t*)&orig_seEval) != 0) {
        LOGW("[Wjtxd] dobby fail @se eval");
        return false;
    }
    g_seHooked = true;
    LOGI("[Wjtxd] hooked se::evalString @%p (file 0x%lX)", va, (unsigned long)kSeEvalRva);
    return true;
}

// 主路径：C++ se::ScriptEngine::evalString — 游戏战斗脚本明文都从这里过
static bool hooked_seEval(void* thiz, const char* script, long length, void* ret,
                          const char* fileName) {
    if (!orig_seEval) return false;
    if (!script || g_inEvalSplice) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    long n = length;
    if (n < 0) n = (long)strlen(script);
    int calls = ++g_seCalls;
    if (calls <= 8 || calls % 200 == 0) {
        LOGI("[Wjtxd] se::eval #%d len=%ld file=%s", calls, n,
             fileName ? fileName : "(null)");
    }

    if (n < 32 || n > 48 * 1024 * 1024) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    auto has = [&](const char* needle) -> bool {
        size_t nl = strlen(needle);
        if ((size_t)n < nl) return false;
        return memmem(script, (size_t)n, needle, nl) != nullptr;
    };

    if (has("starry_wjtxd_")) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

#ifdef STARRY_WJTXD_PROBE
    // 探针：dump 含广告/战斗关键词的明文脚本（锚点重定位诊断用，正式构建不编译）
    if (has("adVideo") || has("onAdResult") || has("Play800Bridge") ||
        has("getFrameSpeed") || has("get invulnerable")) {
        static std::atomic<int> probeIdx{0};
        int idx = ++probeIdx;
        if (idx <= 12) {
            char path[256];
            snprintf(path, sizeof(path),
                     "/sdcard/Android/data/com.wjtxdtt.zy/files/starry_eval_%d.js", idx);
            FILE* f = fopen(path, "wb");
            if (f) {
                fwrite(script, 1, (size_t)n, f);
                fclose(f);
            }
            LOGI("[Wjtxd] probe dump #%d len=%ld file=%s", idx, n,
                 fileName ? fileName : "(null)");
        }
    }
#endif

    bool combat = has("getFrameSpeed") || has("get invulnerable") ||
                      has("sendToNative(\"adVideo\")");
    if (!combat) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    std::string src(script, (size_t)n);
    std::string patched = SpliceCombatJs(src.c_str());
    if (patched == src) {
        if (n < 8192) LOGW("[Wjtxd] se splice miss len=%ld", n);
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    int h = ++g_spliceHits;
    LOGI("[Wjtxd] se combat splice #%d len %zu->%zu file=%s", h, src.size(),
         patched.size(), fileName ? fileName : "(null)");

    g_inEvalSplice = true;
    bool ok = orig_seEval(thiz, patched.c_str(), (long)patched.size(), ret, fileName);
    g_inEvalSplice = false;
    return ok;
}

static jint hooked_jniEval(JNIEnv* env, jclass cls, jstring js) {
    if (!orig_jniEval) return 0;
    if (!js || g_inEvalSplice) return orig_jniEval(env, cls, js);

    const char* c = env->GetStringUTFChars(js, nullptr);
    if (!c) return orig_jniEval(env, cls, js);
    size_t n = strlen(c);
    int calls = ++g_jniCalls;
    if (calls <= 8 || calls % 200 == 0) {
        LOGI("[Wjtxd] jni eval #%d len=%zu head=%.64s", calls, n, c);
    }

    bool combat = strstr(c, "getFrameSpeed") != nullptr ||
                  strstr(c, "get invulnerable") != nullptr;
    if (!combat) {
        env->ReleaseStringUTFChars(js, c);
        return orig_jniEval(env, cls, js);
    }

    std::string patched = SpliceCombatJs(c);
    env->ReleaseStringUTFChars(js, c);
    if (patched.empty()) return orig_jniEval(env, cls, js);

    jstring jjs = env->NewStringUTF(patched.c_str());
    if (!jjs) return orig_jniEval(env, cls, js);
    jint ret = orig_jniEval(env, cls, jjs);
    int h = ++g_spliceHits;
    LOGI("[Wjtxd] jni splice #%d", h);
    env->DeleteLocalRef(jjs);
    return ret;
}

static void TryHookEvalString() {
    const char* lib = kTargetLib;
    if (!isLibraryLoaded(lib)) return;
    // 主路径：C++ se::ScriptEngine::evalString（游戏战斗脚本明文；JNI 桥仅 Java→JS 旁路）
    if (!g_seHooked) WjtxdHookSeEval();
    // JNI 导出符号用 dlsym(RTLD_DEFAULT) 解析：DobbySymbolResolver 在
    // libUnityV114514.so（≥28MB 多段映射）上会遍历 /proc/self/maps 且可能
    // 踩特殊段 BUS_ADRERR（MuMu 实测，24-09-10 tombstone#17）。
    if (!g_jniHooked) {
        void* jni = dlsym(RTLD_DEFAULT, kJniEvalSym);
        if (!jni) jni = DobbySymbolResolver(lib, kJniEvalSym);
        if (jni && DobbyHook(jni, (dobby_dummy_func_t)hooked_jniEval,
                             (dobby_dummy_func_t*)&orig_jniEval) == 0) {
            g_jniHooked = true;
            LOGI("[Wjtxd] hooked JNI evalString @%p", jni);
        } else {
            LOGW("[Wjtxd] jni evalString hook fail (lib=%s)", lib);
        }
    }
}

bool Wjtxd_TryInit(JavaVM* jvm) {
    g_wjtxdJvm = jvm;
    if (!isLibraryLoaded(kTargetLib)) return false;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (detach) g_wjtxdJvm->DetachCurrentThread();
    }
    TryHookEvalString();
    if (!g_jniHooked && !g_seHooked) return false;
    g_ready = true;
    SyncJavaRets();
    return true;
}

static void* EarlyHookThread(void* arg) {
    g_wjtxdJvm = (JavaVM*)arg;
    for (int i = 0; i < 2000; i++) {
        if (isLibraryLoaded(kTargetLib)) {
            TryHookEvalString();
            if (g_jniHooked || g_seHooked) {
                g_ready = true;
                LOGI("[Wjtxd] early hook ok iter=%d", i);
                bool detach = false;
                JNIEnv* env = GetEnv(&detach);
                if (env) {
                    CacheJava(env);
                    SyncJavaRets();
                    if (detach) g_wjtxdJvm->DetachCurrentThread();
                }
                return nullptr;
            }
        }
        usleep(20 * 1000);
    }
    LOGW("[Wjtxd] early hook timeout");
    return nullptr;
}

void Wjtxd_StartEarlyHookThread(JavaVM* jvm) {
    static std::atomic_bool started{false};
    bool expected = false;
    if (!started.compare_exchange_strong(expected, true)) return;
    pthread_t th;
    if (pthread_create(&th, nullptr, EarlyHookThread, jvm) == 0) pthread_detach(th);
    else started.store(false);
}

bool Wjtxd_IsReady() { return g_ready; }

void Wjtxd_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == WJTXD_FEAT_INV) g_inv = enabled;
    if (featNum == WJTXD_FEAT_SPD) g_spd = enabled;
    if (featNum == WJTXD_FEAT_ADFREE) g_adfree = enabled;
    SyncJavaRets();
    LOGI("[Wjtxd] feature #%d=%s inv=%d spd=%d adfree=%d", featNum, enabled ? "ON" : "OFF",
         g_inv ? 1 : 0, g_spd ? 1 : 0, g_adfree ? 1 : 0);
}

void Wjtxd_ForceClear() {
    g_inv = g_spd = g_adfree = false;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (g_hooksCls && g_hooksClear) env->CallStaticVoidMethod(g_hooksCls, g_hooksClear);
        if (detach) g_wjtxdJvm->DetachCurrentThread();
    }
}

void Wjtxd_Tick(const bool* features, const int* /*values*/) {
    if (!features) return;
    g_inv = features[WJTXD_FEAT_INV];
    g_spd = features[WJTXD_FEAT_SPD];
    g_adfree = features[WJTXD_FEAT_ADFREE];
    if (!g_ready) return;
    if (!g_jniHooked && !g_seHooked) TryHookEvalString();
    SyncJavaRets();
}