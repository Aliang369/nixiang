/* ================================================================
 *  迷雾大陆 MOD — 对齐斌哥 zip.apk get_ret 1..4 + 反作弊屏蔽
 *  路径：se::ScriptEngine::evalString 运行时拼接（方案 B）
 * ================================================================ */

#include "MwdlMod.h"
#include "Includes/obfuscate.h"

#include <atomic>
#include <cstdio>
#include <cstring>
#include <pthread.h>
#include <string>
#include <unistd.h>

#if defined(__linux__) || defined(__ANDROID__)
#include <string.h>
#endif

#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"
#include "Dobby/dobby.h"

static JavaVM* g_mwdlJvm = nullptr;
static bool g_ready = false;
static bool g_evalHooked = false;
static thread_local bool g_inEvalSplice = false;

static jclass g_hooksCls = nullptr;
static jmethodID g_hooksSetRet = nullptr;
static jmethodID g_hooksSetVal = nullptr;
static jmethodID g_hooksClear = nullptr;

static volatile bool g_dmg100 = false;
static volatile bool g_inv = false;
static volatile bool g_ultra = false;
static volatile float g_speedMul = 1.f;  // 1=关，>1 倍率
static std::atomic<int> g_spliceHits{0};

static const char* kAct = "org.cocos2dx.javascript.AppActivity";

static JNIEnv* GetEnv(bool* needDetach) {
    *needDetach = false;
    if (!g_mwdlJvm) return nullptr;
    JNIEnv* env = nullptr;
    int st = g_mwdlJvm->GetEnv((void**)&env, JNI_VERSION_1_6);
    if (st == JNI_EDETACHED) {
        if (g_mwdlJvm->AttachCurrentThread(&env, nullptr) != 0) return nullptr;
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
    jclass local = LoadAppClass(env, "com.starry.magic.cocos.MwdlHooks");
    if (!local) local = env->FindClass(OBFUSCATE("com/starry/magic/cocos/MwdlHooks"));
    if (local) {
        g_hooksCls = (jclass)env->NewGlobalRef(local);
        g_hooksSetRet = env->GetStaticMethodID(g_hooksCls, "setRet", "(IZ)V");
        g_hooksSetVal = env->GetStaticMethodID(g_hooksCls, "setVal", "(IF)V");
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
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 1, g_dmg100 ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 2, g_inv ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 4, g_ultra ? JNI_TRUE : JNI_FALSE);
    }
    if (g_hooksCls && g_hooksSetVal) {
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetVal, 3, (jfloat)g_speedMul);
    }
    if (detach) g_mwdlJvm->DetachCurrentThread();
}

static std::string SpliceCombatJs(const char* src) {
    std::string s(src);

    static char kKhHook[1100];
    static char kSpdRepl[1400];
    static char kTickHook[900];
    static bool once = false;
    if (!once) {
        // get_ret(1)+get_ret(2)：百倍伤害 / 无敌 — 对齐斌哥 KH 头
        snprintf(
            kKhHook, sizeof(kKhHook),
            "/*starry_mwdl_kh*/ var isChecked_RET=jsb.reflection.callStaticMethod("
            "\"%s\",\"get_ret\",\"(I)Z\",1);"
            "if(isChecked_RET){if(e.unitType===qA.Hero||e.unitType===qA.Summon||"
            "e.unitType===qA.Guardian||e.unitType===qA.Player||e.unitType===qA.ParasyteGuardian)"
            "o.target.damage*=100,o.realDamage&&(o.target.realDamage*=100);}"
            "var isChecked_RET2=jsb.reflection.callStaticMethod("
            "\"%s\",\"get_ret\",\"(I)Z\",2);"
            "if(isChecked_RET2){if(e.unitType===qA.Monster)"
            "o.target.damage=0,o.target.realDamage=0;} ",
            kAct, kAct);
        // get_val(3)：移速倍率 — 原版压缩写法无 var n，整段替换 return
        snprintf(
            kSpdRepl, sizeof(kSpdRepl),
            "/*starry_mwdl_spd*/return t=Math.max(0,t),function(n){try{var "
            "m=jsb.reflection.callStaticMethod(\"%s\",\"get_val\",\"(I)F\",3);"
            "m>1&&(this.unitType===qA.Hero||this.unitType===qA.Summon||"
            "this.unitType===qA.Guardian||this.unitType===qA.Player||"
            "this.unitType===qA.ParasyteGuardian)&&(n*=m)}catch(_e){}return "
            "n}.call(this,this.AttributeComponent.getAttr(mS.MOV_SPEED)*t)}",
            kAct);
        // get_ret(4)：无限技能 — 插在 SkillComponent.tick 前
        snprintf(
            kTickHook, sizeof(kTickHook),
            "/*starry_mwdl_uls*/ jsb.reflection.callStaticMethod("
            "\"%s\",\"get_ret\",\"(I)Z\",4)&&(this.unitType===qA.Hero||"
            "this.unitType===qA.Summon||this.unitType===qA.Guardian||"
            "this.unitType===qA.Player||this.unitType===qA.ParasyteGuardian)&&"
            "(this.SkillComponent.reduceAllSkillColdDown(1e9),"
            "this.ActiveSkillComponent&&this.ActiveSkillComponent.reduceAllSkillColdDown(1e9)), ",
            kAct);
        once = true;
    }

    auto already = [&](const char* tag) -> bool {
        return s.find(tag) != std::string::npos;
    };

    // --- KH ---
    if (!already("starry_mwdl_kh") && s.find("isChecked_RET") == std::string::npos) {
        static const char* kKhOpens[] = {
            "function KH(t, e, i, o) {",
            "function KH(t,e,i,o){",
            nullptr,
        };
        for (int i = 0; kKhOpens[i]; i++) {
            size_t p = s.find(kKhOpens[i]);
            if (p == std::string::npos) continue;
            size_t end = p + strlen(kKhOpens[i]);
            s.insert(end, kKhHook);
            LOGI("[Mwdl] spliced KH");
            break;
        }
    }

    // --- MoveSpeed（原版压缩：return t=Math.max(0,t),getAttr(MOV_SPEED)*t）---
    if (!already("starry_mwdl_spd")) {
        static const char* kSpdFroms[] = {
            "return t=Math.max(0,t),this.AttributeComponent.getAttr(mS.MOV_SPEED)*t}",
            "return t = Math.max(0, t), this.AttributeComponent.getAttr(mS.MOV_SPEED) * t}",
            nullptr,
        };
        for (int i = 0; kSpdFroms[i]; i++) {
            size_t p = s.find(kSpdFroms[i]);
            if (p == std::string::npos) continue;
            s.replace(p, strlen(kSpdFroms[i]), kSpdRepl);
            LOGI("[Mwdl] spliced MoveSpeed (get_val)");
            break;
        }
    }

    // --- tick 无限技能 ---
    if (!already("starry_mwdl_uls") &&
        s.find("reduceAllSkillColdDown(1e9)") == std::string::npos) {
        static const char* kTickAnchors[] = {
            "this.isDormant || (this.SkillComponent.tick(t)",
            "this.isDormant||(this.SkillComponent.tick(t)",
            nullptr,
        };
        for (int i = 0; kTickAnchors[i]; i++) {
            size_t p = s.find(kTickAnchors[i]);
            if (p == std::string::npos) continue;
            // 插在 `(this.SkillComponent.tick` 的 `(` 之后，使 get_ret 与 tick 成逗号表达式前项
            size_t paren = s.find('(', p);
            if (paren == std::string::npos) continue;
            s.insert(paren + 1, kTickHook);
            LOGI("[Mwdl] spliced tick ultra skill");
            break;
        }
    }

    // --- 反作弊：onStageCheat / onDevilClear ---
    if (!already("starry_mwdl_nocheat_stage")) {
        static const char* kStageCheat[] = {
            "t.prototype.onStageCheat=function(t){C(v.ShowErrorTips,p.EErrorTips.FightCheat)}",
            "t.prototype.onStageCheat = function(t) {C(v.ShowErrorTips, p.EErrorTips.FightCheat)}",
            "t.prototype.onStageCheat = function(t) {\n          C(v.ShowErrorTips, p.EErrorTips.FightCheat)\n        }",
            nullptr,
        };
        for (int i = 0; kStageCheat[i]; i++) {
            size_t p = s.find(kStageCheat[i]);
            if (p == std::string::npos) continue;
            s.replace(p, strlen(kStageCheat[i]),
                      "/*starry_mwdl_nocheat_stage*/t.prototype.onStageCheat=function(t){}");
            LOGI("[Mwdl] neutralized onStageCheat");
            break;
        }
    }

    // --- addCheat 上报 ---
    if (!already("starry_mwdl_nocheat_add") &&
        s.find("scene.playerHandler.addCheat") != std::string::npos) {
        size_t p = s.find("scene.playerHandler.addCheat");
        // 向前找最近的 function( 或 =function
        size_t brace = s.rfind('{', p);
        if (brace != std::string::npos && brace + 1 < p &&
            s.find("starry_mwdl_nocheat_add", brace) == std::string::npos) {
            s.insert(brace + 1, "/*starry_mwdl_nocheat_add*/return;");
            LOGI("[Mwdl] neutralized addCheat");
        }
    }

    // --- 聊天 md5 拦截（对齐斌哥，提示换星光文案）---
    if (!already("starry_mwdl_nochatter") &&
        s.find("hex_md5(MUtil.Utf8Encode(a + t + e))") != std::string::npos) {
        size_t p = s.find("hex_md5(MUtil.Utf8Encode(a + t + e))");
        s.insert(p,
                 "/*starry_mwdl_nochatter*/MUtil.createFloatTips(\"星光：禁止聊天\");return;");
        LOGI("[Mwdl] blocked chat md5");
    } else if (!already("starry_mwdl_nochatter") &&
               s.find("hex_md5(MUtil.Utf8Encode(a+t+e))") != std::string::npos) {
        size_t p = s.find("hex_md5(MUtil.Utf8Encode(a+t+e))");
        s.insert(p,
                 "/*starry_mwdl_nochatter*/MUtil.createFloatTips(\"星光：禁止聊天\");return;");
        LOGI("[Mwdl] blocked chat md5 (min)");
    }

    return s;
}

typedef jint (*JniEvalString_t)(JNIEnv*, jclass, jstring);
static JniEvalString_t orig_jniEval = nullptr;

typedef bool (*SeEvalString_t)(void*, const char*, long, void*, const char*);
static SeEvalString_t orig_seEval = nullptr;
static bool g_seHooked = false;
static bool g_jniHooked = false;
static std::atomic<int> g_seCalls{0};

static jint hooked_jniEval(JNIEnv* env, jclass cls, jstring js) {
    if (!orig_jniEval) return 0;
    return orig_jniEval(env, cls, js);
}

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
        LOGI("[Mwdl] se::eval #%d len=%ld file=%s", calls, n,
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

    if (has("starry_mwdl_")) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    bool interesting = has("function KH") || has("mS.MOV_SPEED") ||
                       has("SkillComponent.tick") || has("onStageCheat") ||
                       has("playerHandler.addCheat") || has("hex_md5(MUtil");
    if (!interesting) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    std::string src(script, (size_t)n);
    std::string patched = SpliceCombatJs(src.c_str());
    if (patched == src) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    int hits = ++g_spliceHits;
    LOGI("[Mwdl] combat splice #%d len %zu→%zu file=%s", hits, src.size(), patched.size(),
         fileName ? fileName : "(null)");

    g_inEvalSplice = true;
    bool ok = orig_seEval(thiz, patched.c_str(), (long)patched.size(), ret, fileName);
    g_inEvalSplice = false;
    return ok;
}

static void TryHookEvalString() {
    const char* lib = GameCocos::TARGET_LIB;
    if (!isLibraryLoaded(lib)) return;
    if (!g_seHooked) {
        void* sym = DobbySymbolResolver(lib, GameCocos::Symbols::SE_EVAL_STRING);
        if (sym &&
            DobbyHook(sym, (dobby_dummy_func_t)hooked_seEval,
                      (dobby_dummy_func_t*)&orig_seEval) == 0) {
            g_seHooked = true;
            g_evalHooked = true;
            LOGI("[Mwdl] hooked se::evalString");
        }
    }
    if (!g_jniHooked) {
        void* jni = DobbySymbolResolver(lib, GameCocos::Symbols::EVAL_STRING);
        if (jni &&
            DobbyHook(jni, (dobby_dummy_func_t)hooked_jniEval,
                      (dobby_dummy_func_t*)&orig_jniEval) == 0) {
            g_jniHooked = true;
        }
    }
}

bool Mwdl_TryInit(JavaVM* jvm) {
    g_mwdlJvm = jvm;
    if (!isLibraryLoaded(GameCocos::TARGET_LIB)) return false;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (detach) g_mwdlJvm->DetachCurrentThread();
    }
    TryHookEvalString();
    if (!g_seHooked) return false;
    g_ready = true;
    SyncJavaRets();
    return true;
}

static void* EarlyHookThread(void* arg) {
    g_mwdlJvm = (JavaVM*)arg;
    for (int i = 0; i < 2000; i++) {
        if (isLibraryLoaded(GameCocos::TARGET_LIB)) {
            TryHookEvalString();
            if (g_seHooked) {
                g_ready = true;
                LOGI("[Mwdl] early se hook ok iter=%d", i);
                bool detach = false;
                JNIEnv* env = GetEnv(&detach);
                if (env) {
                    CacheJava(env);
                    SyncJavaRets();
                    if (detach) g_mwdlJvm->DetachCurrentThread();
                }
                return nullptr;
            }
        }
        usleep(20 * 1000);
    }
    LOGW("[Mwdl] early hook timeout");
    return nullptr;
}

void Mwdl_StartEarlyHookThread(JavaVM* jvm) {
    static std::atomic_bool started{false};
    bool expected = false;
    if (!started.compare_exchange_strong(expected, true)) return;
    pthread_t th;
    if (pthread_create(&th, nullptr, EarlyHookThread, jvm) == 0) pthread_detach(th);
    else started.store(false);
}

bool Mwdl_IsReady() { return g_ready; }

void Mwdl_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == MWDL_FEAT_DMG100) g_dmg100 = enabled;
    if (featNum == MWDL_FEAT_INVINCIBLE) g_inv = enabled;
    if (featNum == MWDL_FEAT_ULTRA_SKILL) g_ultra = enabled;
    SyncJavaRets();
    LOGI("[Mwdl] feature #%d=%s dmg=%d inv=%d spd=%.1f uls=%d", featNum,
         enabled ? "ON" : "OFF", g_dmg100 ? 1 : 0, g_inv ? 1 : 0, (double)g_speedMul,
         g_ultra ? 1 : 0);
}

void Mwdl_OnValueChanged(int featNum, int value) {
    if (featNum == MWDL_FEAT_MOVESPEED) {
        g_speedMul = (value <= 1) ? 1.f : (float)value;
        SyncJavaRets();
        LOGI("[Mwdl] speed mul=%.1f (slider=%d)", (double)g_speedMul, value);
    }
}

void Mwdl_ForceClear() {
    g_dmg100 = g_inv = g_ultra = false;
    g_speedMul = 1.f;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (g_hooksCls && g_hooksClear) env->CallStaticVoidMethod(g_hooksCls, g_hooksClear);
        if (detach) g_mwdlJvm->DetachCurrentThread();
    }
}

void Mwdl_Tick(const bool* features) {
    if (!features) return;
    g_dmg100 = features[MWDL_FEAT_DMG100];
    g_inv = features[MWDL_FEAT_INVINCIBLE];
    g_ultra = features[MWDL_FEAT_ULTRA_SKILL];
    if (!g_ready) return;
    if (!g_evalHooked) TryHookEvalString();
    SyncJavaRets();
}
