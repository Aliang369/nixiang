/* ================================================================
 *  迷雾大陆 MOD — 对齐斌哥 zip.apk get_ret 1..4 + 反作弊屏蔽
 *  路径：se::ScriptEngine::evalString 运行时拼接（方案 B）
 *  15.0.0/bffa2：单位枚举 ow→gA、属性枚举 RS.MOV_SPEED→PS.MOV_SPEED，
 *                 伤害尾锚 i.target.damage=E} → i.target.damage=P}（局部变量名随混淆改名）
 *  15.0.0/d8f3b（2026-10-31）：单位枚举 gA→CA、属性枚举 PS.MOV_SPEED→NS.MOV_SPEED，
 *                 伤害尾锚回到 i.target.damage=E}（局部变量名又变回 E）；
 *                 无限技能 / onStageCheat 两锚点文本未变。
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
#include "Includes/SecStrings.hpp"
#include "Dobby/dobby.h"

static JavaVM* g_mwdlJvm = nullptr;
static bool g_ready = false;
static bool g_evalHooked = false;
static thread_local bool g_inEvalSplice = false;

static jclass g_hooksCls = nullptr;
static jmethodID g_hooksSetRet = nullptr;
static jmethodID g_hooksSetVal = nullptr;
static jmethodID g_hooksClear = nullptr;

static volatile float g_dmgRate = 1.f;   // 伤害倍率（1=原伤害，最高 1000）
static volatile bool g_inv = false;
static volatile bool g_ultra = false;
static volatile bool g_autoFight = false;
static volatile bool g_pickAll = false;
static volatile float g_speedMul = 1.f;  // 1=关，>1 倍率
static std::atomic<int> g_spliceHits{0};

static const char* kAct = "org.cocos2dx.javascript.AppActivity";

#define MWDL_TARGET_LIB sec_lib_cocosjs()
static constexpr const char* kSeEvalString =
    "_ZN2se12ScriptEngine10evalStringEPKclPNS_5ValueES2_";
static constexpr const char* kJniEvalString =
    "Java_org_cocos2dx_lib_Cocos2dxJavascriptJavaBridge_evalString";

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
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 2, g_inv ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 4, g_ultra ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 5, g_autoFight ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 6, g_pickAll ? JNI_TRUE : JNI_FALSE);
    }
    if (g_hooksCls && g_hooksSetVal) {
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetVal, 1, (jfloat)g_dmgRate);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetVal, 3, (jfloat)g_speedMul);
    }
    if (detach) g_mwdlJvm->DetachCurrentThread();
}

static std::string SpliceCombatJs(const char* src) {
    std::string s(src);

    static char kKhHook[1400];
    static char kSpdRepl[1400];
    static char kTickHook[900];
    static char kAutoFightRepl[400];
    static char kAfGuardRepl[400];
    static char kPickAllRepl[700];
    static bool once = false;
    if (!once) {
        // get_val(1) 伤害倍率滑块 + get_ret(2) 无敌 — 15.0.0/d8f3b 尾锚 i.target.damage=E}
        snprintf(
            kKhHook, sizeof(kKhHook),
            "/*starry_mwdl_kh*/var _su=i.sourceUnit;if(_su&&(_su.unitType===CA.Hero||"
            "_su.unitType===CA.Summon||_su.unitType===CA.Guardian||"
            "_su.unitType===CA.Player||_su.unitType===CA.ParasyteGuardian)){"
            "var _dm=jsb.reflection.callStaticMethod(\"%s\",\"get_val\",\"(I)F\",1);"
            "_dm>1&&(E*=Math.min(_dm,1000))}"
            "if(_su&&_su.unitType===CA.Monster&&"
            "jsb.reflection.callStaticMethod(\"%s\",\"get_ret\",\"(I)Z\",2)){E=0}"
            "i.target.damage=E}",
            kAct, kAct);
        // get_val(3)：移速倍率 — MoveSpeed getter（15.0.0/d8f3b 为 NS.MOV_SPEED）
        snprintf(
            kSpdRepl, sizeof(kSpdRepl),
            "/*starry_mwdl_spd*/function(n){try{var "
            "m=jsb.reflection.callStaticMethod(\"%s\",\"get_val\",\"(I)F\",3);"
            "m>1&&(this.unitType===CA.Hero||this.unitType===CA.Summon||"
            "this.unitType===CA.Guardian||this.unitType===CA.Player||"
            "this.unitType===CA.ParasyteGuardian)&&(n*=m)}catch(_e){}return "
            "n}.call(this,this.AttributeComponent.getAttr(NS.MOV_SPEED)*t)}",
            kAct);
        // get_ret(4)：无限技能 — 插在 SkillComponent.tick 前
        snprintf(
            kTickHook, sizeof(kTickHook),
            "/*starry_mwdl_uls*/ jsb.reflection.callStaticMethod("
            "\"%s\",\"get_ret\",\"(I)Z\",4)&&(this.unitType===CA.Hero||"
            "this.unitType===CA.Summon||this.unitType===CA.Guardian||"
            "this.unitType===CA.Player||this.unitType===CA.ParasyteGuardian)&&"
            "(this.SkillComponent.reduceAllSkillColdDown(1e9),"
            "this.ActiveSkillComponent&&this.ActiveSkillComponent.reduceAllSkillColdDown(1e9)), ",
            kAct);
        // 自动挂机：强开 autoFight 服务分支（AI 控制器负责自动走位/放技能），
        // 但**必须**同时掐掉该分支里的战后结算 `tryFightNext()` —— 服务未真正开通时它会读
        // 到 undefined.mode（tryFightBonus）并每帧抛 TypeError，把「挑战成功」卡死（2026-10-31 实测）。
        snprintf(
            kAutoFightRepl, sizeof(kAutoFightRepl),
            "/*starry_mwdl_af*/checkAutoFightSetting=function(){var t=ix.getService(sT);if("
            "jsb.reflection.callStaticMethod(\"%s\",\"get_ret\",\"(I)Z\",5)||t.enabled){",
            kAct);
        snprintf(
            kAfGuardRepl, sizeof(kAfGuardRepl),
            "/*starry_mwdl_afg*/if(this.aiController.finished()&&!"
            "jsb.reflection.callStaticMethod(\"%s\",\"get_ret\",\"(I)Z\",5))return "
            "this._autoFightFinished=!0,void t.tryFightNext();",
            kAct);
        // 全图拾取：checkInRangeDropLoot 的「有宠物才全图」分支加开关（两处实现都改）
        snprintf(
            kPickAllRepl, sizeof(kPickAllRepl),
            "/*starry_mwdl_pall*/if(jsb.reflection.callStaticMethod(\"%s\",\"get_ret\",\"(I)Z\",6)"
            "||this._fight.getHasPet())this._inRangeDropLoots=this._loots;",
            kAct);
        once = true;
    }

    auto already = [&](const char* tag) -> bool {
        return s.find(tag) != std::string::npos;
    };

    // --- 伤害结算（百倍伤害 + 无敌）：15.0.0/d8f3b 尾锚 i.target.damage=E} ---
    // 注：该锚点尾部是混淆后的伤害局部变量名（a1e47=E / bffa2=P / d8f3b=E），
    //     与 hook 文本强耦合，不做旧锚回退（回退会拼出引用未声明变量的 JS）。
    if (!already("starry_mwdl_kh") && s.find("i.target.damage=E}") != std::string::npos) {
        size_t p = s.find("i.target.damage=E}");
        s.replace(p, strlen("i.target.damage=E}"), kKhHook);
        LOGI("[Mwdl] spliced damage (15.0.0/d8f3b)");
    }

    // --- MoveSpeed（移动速度：15.0.0/d8f3b 为 NS.MOV_SPEED；旧枚举名为等价回退）---
    if (!already("starry_mwdl_spd")) {
        static const char* kSpdFroms[] = {
            "this.AttributeComponent.getAttr(NS.MOV_SPEED)*t}",
            "this.AttributeComponent.getAttr(RS.MOV_SPEED)*t}",
            "this.AttributeComponent.getAttr(vS.MOV_SPEED)*t}",
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

    // --- 自动挂机：强开服务分支（AI 自动走位）+ 掐掉战后 tryFightNext（防 undefined.mode 卡死）---
    if (!already("starry_mwdl_af")) {
        const char* kAf =
            "checkAutoFightSetting=function(){var t=ix.getService(sT);if(t.enabled){";
        size_t p = s.find(kAf);
        if (p != std::string::npos) {
            s.replace(p, strlen(kAf), kAutoFightRepl);
            LOGI("[Mwdl] spliced auto-fight (AI)");
        }
    }
    if (!already("starry_mwdl_afg")) {
        const char* kGuard =
            "if(this.aiController.finished())return this._autoFightFinished=!0,void t.tryFightNext();";
        size_t p = s.find(kGuard);
        if (p != std::string::npos) {
            s.replace(p, strlen(kGuard), kAfGuardRepl);
            LOGI("[Mwdl] spliced auto-fight guard (no auto-next)");
        }
    }

    // --- 全图拾取：两处 checkInRangeDropLoot 实现都改（get_ret 6）---
    if (!already("starry_mwdl_pall")) {
        const char* kPick =
            "if(this._fight.getHasPet())this._inRangeDropLoots=this._loots;";
        size_t p = 0;
        int n = 0;
        while ((p = s.find(kPick, p)) != std::string::npos) {
            s.replace(p, strlen(kPick), kPickAllRepl);
            p += strlen(kPickAllRepl);
            n++;
        }
        if (n > 0) LOGI("[Mwdl] spliced pick-all x%d", n);
    }

    // --- 反作弊：按方法边界中和，避免依赖 15.0.0 内部变量名 ---
    if (!already("starry_mwdl_nocheat_stage")) {
        static const char* kStageCheat = "t.prototype.onStageCheat=function(t){";
        size_t p = s.find(kStageCheat);
        if (p != std::string::npos) {
            size_t body = p + strlen(kStageCheat);
            size_t end = s.find('}', body);
            if (end != std::string::npos) {
                s.replace(p, end - p + 1,
                          "/*starry_mwdl_nocheat_stage*/t.prototype.onStageCheat=function(t){}");
                LOGI("[Mwdl] neutralized onStageCheat (15.0.0)");
            }
        }
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

    // 预筛：只按「与混淆符号无关」的稳定片段判定，符号改名后仍能进入拼接流程，
    //       拼接失败由 SpliceCombatJs 的告警日志暴露（避免静默不拼接）。
    bool interesting = has("i.target.damage=") || has("MOV_SPEED") ||
                       has("SkillComponent.tick") || has("onStageCheat");
    if (!interesting) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    std::string src(script, (size_t)n);
    std::string patched = SpliceCombatJs(src.c_str());
    if (patched == src) {
        static std::atomic<bool> warned{false};
        if (!warned.exchange(true)) {
            LOGI("[Mwdl] WARN combat bundle matched but no anchor spliced (bundle drift?)");
        }
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
    const char* lib = MWDL_TARGET_LIB;
    if (!isLibraryLoaded(lib)) return;
    if (!g_seHooked) {
        void* sym = DobbySymbolResolver(lib, kSeEvalString);
        if (sym &&
            DobbyHook(sym, (dobby_dummy_func_t)hooked_seEval,
                      (dobby_dummy_func_t*)&orig_seEval) == 0) {
            g_seHooked = true;
            g_evalHooked = true;
            LOGI("[Mwdl] hooked se::evalString");
        }
    }
    if (!g_jniHooked) {
        void* jni = DobbySymbolResolver(lib, kJniEvalString);
        if (jni &&
            DobbyHook(jni, (dobby_dummy_func_t)hooked_jniEval,
                      (dobby_dummy_func_t*)&orig_jniEval) == 0) {
            g_jniHooked = true;
        }
    }
}

bool Mwdl_TryInit(JavaVM* jvm) {
    g_mwdlJvm = jvm;
    if (!isLibraryLoaded(MWDL_TARGET_LIB)) return false;
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
        if (isLibraryLoaded(MWDL_TARGET_LIB)) {
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
    if (featNum == MWDL_FEAT_INVINCIBLE) g_inv = enabled;
    if (featNum == MWDL_FEAT_ULTRA_SKILL) g_ultra = enabled;
    if (featNum == MWDL_FEAT_AUTO_FIGHT) g_autoFight = enabled;
    if (featNum == MWDL_FEAT_PICK_ALL) g_pickAll = enabled;
    SyncJavaRets();
    LOGI("[Mwdl] feature #%d=%s inv=%d spd=%.1f uls=%d af=%d pickall=%d", featNum,
         enabled ? "ON" : "OFF", g_inv ? 1 : 0, (double)g_speedMul, g_ultra ? 1 : 0,
         g_autoFight ? 1 : 0, g_pickAll ? 1 : 0);
}

void Mwdl_OnValueChanged(int featNum, int value) {
    if (featNum == MWDL_FEAT_DMG_RATE) {
        int v = value < 1 ? 1 : (value > 1000 ? 1000 : value);
        g_dmgRate = (float)v;
        SyncJavaRets();
        LOGI("[Mwdl] damage rate x%d", v);
    }
    if (featNum == MWDL_FEAT_MOVESPEED) {
        g_speedMul = (value <= 1) ? 1.f : (float)value;
        SyncJavaRets();
        LOGI("[Mwdl] speed mul=%.1f (slider=%d)", (double)g_speedMul, value);
    }
}

void Mwdl_ForceClear() {
    g_inv = g_ultra = false;
    g_autoFight = g_pickAll = false;
    g_dmgRate = 1.f;
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
    g_inv = features[MWDL_FEAT_INVINCIBLE];
    g_ultra = features[MWDL_FEAT_ULTRA_SKILL];
    g_autoFight = features[MWDL_FEAT_AUTO_FIGHT];
    g_pickAll = features[MWDL_FEAT_PICK_ALL];
    if (!g_ready) return;
    if (!g_evalHooked) TryHookEvalString();
    SyncJavaRets();
}
