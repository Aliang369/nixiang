/* ================================================================
 *  我在神界刷装备 (com.windforce.rg.offical) — Sjzz Mod
 *
 *  抓点：
 *    AdManager.CheckSkipAd                        免广告（3）
 *    BattleHealthState.BeHurt                     伤害倍率（1）/ 无敌（2）
 *    BattleMainPanel.GetPlaySpeed                 游戏加速（4）
 *    BattleMainPanel.UploadPlaySpeed              加速开启时跳过倍速上报
 *
 *  se::ScriptEngine::evalString 定位（libcocos.so）：
 *    file offset 0xC3D188，序言 D10503FF
 * ================================================================ */

#include "SjzzMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <dlfcn.h>
#include <pthread.h>
#include <string>
#include <unistd.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"

#define SJZZ_LOG "[Sjzz]"

static const char kTargetLib[] = "libcocos.so";
static const uintptr_t kSeEvalFileOff = 0xC3D188;
static const uint32_t kSeEvalProlog = 0xD10503FF;

static JavaVM* g_jvm = nullptr;
static std::atomic<bool> g_seHooked{false};
static std::atomic<bool> g_ready{false};
static bool g_inEvalSplice = false;

static jclass g_hooksCls = nullptr;
static jmethodID g_hooksSetRet = nullptr;
static jmethodID g_hooksClear = nullptr;
static jmethodID g_hooksSetAtkMul = nullptr;

static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_adfree{false};
static std::atomic<bool> g_speed{false};
static std::atomic<int> g_atkMul{1};
static std::atomic<int> g_spliceHits{0};

static JNIEnv* GetEnv(bool* needDetach) {
    *needDetach = false;
    if (!g_jvm) return nullptr;
    JNIEnv* env = nullptr;
    int st = g_jvm->GetEnv(reinterpret_cast<void**>(&env), JNI_VERSION_1_6);
    if (st == JNI_EDETACHED) {
        if (g_jvm->AttachCurrentThread(&env, nullptr) != 0) return nullptr;
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
        cls = static_cast<jclass>(env->CallObjectMethod(cl, loadClass, jname));
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
    jclass local = LoadAppClass(env, "com.starry.magic.cocos.SjzzHooks");
    if (!local) local = env->FindClass("com/starry/magic/cocos/SjzzHooks");
    if (local) {
        g_hooksCls = static_cast<jclass>(env->NewGlobalRef(local));
        g_hooksSetRet = env->GetStaticMethodID(g_hooksCls, "setRet", "(IZ)V");
        g_hooksClear = env->GetStaticMethodID(g_hooksCls, "clearAll", "()V");
        g_hooksSetAtkMul = env->GetStaticMethodID(g_hooksCls, "setAtkMul", "(I)V");
        env->DeleteLocalRef(local);
    } else {
        env->ExceptionClear();
    }
}

static void SyncJavaState() {
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    CacheJava(env);
    if (g_hooksCls && g_hooksSetRet) {
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 1,
                                  g_atkMul.load() > 1 ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 2,
                                  g_inv.load() ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 3,
                                  g_adfree.load() ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 4,
                                  g_speed.load() ? JNI_TRUE : JNI_FALSE);
    }
    if (g_hooksCls && g_hooksSetAtkMul) {
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetAtkMul, g_atkMul.load());
    }
    if (detach) g_jvm->DetachCurrentThread();
}

static bool ReplaceOnce(std::string& s, const char* from, const char* to, const char* tag) {
    const size_t p = s.find(from);
    if (p == std::string::npos) return false;
    s.replace(p, strlen(from), to);
    LOGI(SJZZ_LOG " splice %s", tag);
    return true;
}

static std::string SpliceSjzzJs(const char* src, size_t len) {
    std::string s(src, len);

    // 免广告：走 AdManager 原有的跳过广告分支。
    ReplaceOnce(
        s,
        "CheckSkipAd(){return y.GetPrivilegeValue(C.SkipAd)}",
        "CheckSkipAd(){/*starry_sjzz_ad*/if(jsb.reflection.callStaticMethod("
        "\"com.starry.magic.cocos.SjzzHooks\",\"getRet\",\"(I)Z\",3))return!0;"
        "return y.GetPrivilegeValue(C.SkipAd)}",
        "AdManager.CheckSkipAd");

    // 服务端按 ad_start -> ad_stop 的间隔做 6 秒观看时长校验。
    // 免广告不拉起视频，但延迟完成上报；缩短等待会被服务端拒绝发奖。
    ReplaceOnce(
        s,
        "if(this.CheckSkipAd())e(!0);else",
        "if(this.CheckSkipAd())this.SetTimeOut((()=>{e(!0)}),6.6);else",
        "AdManager.PlayAD delayed skip");

    // 免广告的完成回包会晚到，延长客户端重复点击窗口，保持原版手动 skip 行为。
    ReplaceOnce(
        s,
        "if(!this.CheckSkipAd()&&this.lastVideoAdTime&&Date.now()-this.lastVideoAdTime<2e3)",
        "if((!this.CheckSkipAd()||jsb.reflection.callStaticMethod("
        "\"com.starry.magic.cocos.SjzzHooks\",\"getRet\",\"(I)Z\",3))"
        "&&this.lastVideoAdTime&&Date.now()-this.lastVideoAdTime<7e3)",
        "AdManager.PlayAD cooldown");
    // 无敌 + 伤害倍率：Wall=0 / Monster=1，与 BattleOwnerType 对齐。
    ReplaceOnce(
        s,
        "BeHurt(e,t=n()){",
        "BeHurt(e,t=n()){/*starry_sjzz_battle*/"
        "var __sjzz_owner_type=this.owner&&this.owner.GetOwnerType?this.owner.GetOwnerType():-1;"
        "if(__sjzz_owner_type===0&&jsb.reflection.callStaticMethod("
        "\"com.starry.magic.cocos.SjzzHooks\",\"getRet\",\"(I)Z\",2))e.dmg=0;"
        "if(__sjzz_owner_type===1){"
        "var __sjzz_dmg_mul=jsb.reflection.callStaticMethod("
        "\"com.starry.magic.cocos.SjzzHooks\",\"getAtkMul\",\"()I\");"
        "__sjzz_dmg_mul>1&&(e.dmg*=__sjzz_dmg_mul)}",
        "BattleHealthState.BeHurt");

    // 游戏加速：在现有倍速上再乘 2，x2 -> x4。
    ReplaceOnce(
        s,
        "GetPlaySpeed(e){return $e.GetMonthSpeedData()[e]}",
        "GetPlaySpeed(e){/*starry_sjzz_speed*/"
        "var __sjzz_speed=$e.GetMonthSpeedData()[e];"
        "if(typeof __sjzz_speed===\"number\"&&jsb.reflection.callStaticMethod("
        "\"com.starry.magic.cocos.SjzzHooks\",\"getRet\",\"(I)Z\",4))__sjzz_speed*=2;"
        "return __sjzz_speed}",
        "BattleMainPanel.GetPlaySpeed");

    // 加速开启时跳过倍速异常上报；关闭时保留原逻辑。
    ReplaceOnce(
        s,
        "UploadPlaySpeed(){const e=m.type;",
        "UploadPlaySpeed(){/*starry_sjzz_speed_report*/"
        "if(jsb.reflection.callStaticMethod("
        "\"com.starry.magic.cocos.SjzzHooks\",\"getRet\",\"(I)Z\",4))return;"
        "const e=m.type;",
        "BattleMainPanel.UploadPlaySpeed");

    return s;
}

typedef bool (*SeEvalString_t)(void*, const char*, long, void*, const char*);
static SeEvalString_t g_orig_seEval = nullptr;

// file_off -> runtime VA：只在 RX 段找，禁止 base+RVA。
static void* FileOffToVa(const char* lib, uintptr_t fileOff) {
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

static bool hooked_seEval(void* thiz, const char* script, long length, void* ret,
                          const char* fileName) {
    if (!g_orig_seEval) return false;
    if (!script || g_inEvalSplice) {
        return g_orig_seEval(thiz, script, length, ret, fileName);
    }

    long n = length;
    if (n < 0) n = static_cast<long>(strlen(script));
    if (n < 64 || n > 48L * 1024L * 1024L) {
        return g_orig_seEval(thiz, script, length, ret, fileName);
    }

    auto has = [&](const char* needle) -> bool {
        const size_t nl = strlen(needle);
        if (static_cast<size_t>(n) < nl) return false;
        return memmem(script, static_cast<size_t>(n), needle, nl) != nullptr;
    };

    if (has("starry_sjzz_")) {
        return g_orig_seEval(thiz, script, length, ret, fileName);
    }

    const bool relevant =
        has("CheckSkipAd(){return y.GetPrivilegeValue(C.SkipAd)}") ||
        has("BeHurt(e,t=n()){") ||
        has("GetPlaySpeed(e){return $e.GetMonthSpeedData()[e]}") ||
        has("UploadPlaySpeed(){const e=m.type;");
    if (!relevant) {
        return g_orig_seEval(thiz, script, length, ret, fileName);
    }

    std::string patched = SpliceSjzzJs(script, static_cast<size_t>(n));
    if (patched.size() == static_cast<size_t>(n) &&
        memcmp(patched.data(), script, static_cast<size_t>(n)) == 0) {
        LOGW(SJZZ_LOG " relevant script but no splice hit len=%ld file=%s", n,
             fileName ? fileName : "(null)");
        return g_orig_seEval(thiz, script, length, ret, fileName);
    }

    const int hit = ++g_spliceHits;
    LOGI(SJZZ_LOG " js splice #%d len %ld->%zu file=%s", hit, n, patched.size(),
         fileName ? fileName : "(null)");

    g_inEvalSplice = true;
    const bool ok = g_orig_seEval(thiz, patched.c_str(), static_cast<long>(patched.size()),
                                  ret, fileName);
    g_inEvalSplice = false;
    return ok;
}

static bool TryHookSeEval() {
    if (g_seHooked.load()) return true;
    void* va = FileOffToVa(kTargetLib, kSeEvalFileOff);
    if (!va) {
        LOGW(SJZZ_LOG " se eval rva 0x%lX not in RX", (unsigned long)kSeEvalFileOff);
        return false;
    }
    uint32_t got = 0;
    memcpy(&got, va, sizeof(got));
    if (got != kSeEvalProlog) {
        LOGW(SJZZ_LOG " se eval prolog drift got=0x%08X want=0x%08X", got, kSeEvalProlog);
        return false;
    }
    if (DobbyHook(va, reinterpret_cast<dobby_dummy_func_t>(hooked_seEval),
                  reinterpret_cast<dobby_dummy_func_t*>(&g_orig_seEval)) != 0) {
        LOGW(SJZZ_LOG " dobby fail @se eval");
        return false;
    }
    g_seHooked.store(true);
    LOGI(SJZZ_LOG " hooked se::evalString @%p (file 0x%lX)", va,
         (unsigned long)kSeEvalFileOff);
    return true;
}

static void* EarlyHookThread(void* arg) {
    g_jvm = static_cast<JavaVM*>(arg);
    for (int i = 0; i < 2000; i++) {
        if (isLibraryLoaded(kTargetLib) && TryHookSeEval()) {
            bool detach = false;
            JNIEnv* env = GetEnv(&detach);
            if (env) {
                CacheJava(env);
                SyncJavaState();
                if (detach) g_jvm->DetachCurrentThread();
            }
            g_ready.store(true);
            LOGI(SJZZ_LOG " early hook ok iter=%d", i);
            return nullptr;
        }
        usleep(20 * 1000);
    }
    LOGW(SJZZ_LOG " early hook timeout");
    return nullptr;
}

void Sjzz_StartEarlyHookThread(JavaVM* vm) {
    static std::atomic_bool started{false};
    bool expected = false;
    if (!started.compare_exchange_strong(expected, true)) return;
    pthread_t th;
    if (pthread_create(&th, nullptr, EarlyHookThread, vm) == 0) {
        pthread_detach(th);
    } else {
        started.store(false);
    }
}

bool Sjzz_TryInit(JavaVM* vm) {
    g_jvm = vm;
    if (!isLibraryLoaded(kTargetLib)) return false;

    JNIEnv* env = nullptr;
    bool detach = false;
    env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (detach) g_jvm->DetachCurrentThread();
    }

    if (!TryHookSeEval()) return false;
    g_ready.store(true);
    SyncJavaState();
    return true;
}

bool Sjzz_IsReady() { return g_ready.load(); }

void Sjzz_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case SJZZ_FEAT_INV:
            g_inv.store(enabled);
            break;
        case SJZZ_FEAT_ADFREE:
            g_adfree.store(enabled);
            break;
        case SJZZ_FEAT_SPEED:
            g_speed.store(enabled);
            break;
        default:
            break;
    }
    SyncJavaState();
    LOGI(SJZZ_LOG " feature #%d=%s inv=%d adfree=%d speed=%d atkMul=%d", featNum,
         enabled ? "ON" : "OFF", g_inv.load() ? 1 : 0, g_adfree.load() ? 1 : 0,
         g_speed.load() ? 1 : 0, g_atkMul.load());
}

void Sjzz_OnValueChanged(int featNum, int value) {
    if (featNum != SJZZ_FEAT_DMG) return;
    if (value < 1) value = 1;
    if (value > 100) value = 100;
    g_atkMul.store(value);
    SyncJavaState();
    LOGI(SJZZ_LOG " atkMul=%d", value);
}

void Sjzz_Tick(const bool* features, const int* values) {
    if (features) {
        g_inv.store(features[SJZZ_FEAT_INV]);
        g_adfree.store(features[SJZZ_FEAT_ADFREE]);
        g_speed.store(features[SJZZ_FEAT_SPEED]);
    }
    if (values && values[SJZZ_FEAT_DMG] > 0) {
        int mul = values[SJZZ_FEAT_DMG];
        if (mul > 100) mul = 100;
        g_atkMul.store(mul);
    }
    if (!g_seHooked.load() && isLibraryLoaded(kTargetLib)) {
        if (TryHookSeEval()) g_ready.store(true);
    }
    SyncJavaState();
}

void Sjzz_ForceClear() {
    g_inv.store(false);
    g_adfree.store(false);
    g_speed.store(false);
    g_atkMul.store(1);

    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (g_hooksCls && g_hooksClear) {
            env->CallStaticVoidMethod(g_hooksCls, g_hooksClear);
        }
        if (detach) g_jvm->DetachCurrentThread();
    }
}
