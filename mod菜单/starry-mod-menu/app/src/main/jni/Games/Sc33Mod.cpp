/* ================================================================
 *  生存33天 MOD
 *  战斗：对齐斌哥 — 在 Cocos evalString 明文中拼接 get_ret 钩子
 *  免广告：Java SdkManager.SdkShowAdReward（见 inject 脚本）
 * ================================================================ */

#include "Sc33Mod.h"
#include "Includes/obfuscate.h"

#include <atomic>
#include <cstdio>
#include <cstring>
#include <dlfcn.h>
#include <pthread.h>
#include <string>
#include <unistd.h>

#if defined(__linux__) || defined(__ANDROID__)
#include <string.h> // memmem
#endif

#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"
#include "Dobby/dobby.h"

#if defined(STARRY_TRACE)
#define SC33_TRACE_LOG(...) __android_log_print(ANDROID_LOG_INFO, "StarryTrace", "[Sc33] " __VA_ARGS__)
#else
#define SC33_TRACE_LOG(...) ((void)0)
#endif

/* 未验证锚点前禁止 splice（skill：数据先于行动） */
#if defined(SC33_SPLICE_ENABLED)
#define SC33_DO_SPLICE 1
#else
#define SC33_DO_SPLICE 0
#endif

static JavaVM* g_sc33Jvm = nullptr;
static bool g_ready = false;
static bool g_evalHooked = false;
static thread_local bool g_inEvalSplice = false;

static jclass g_bridgeCls = nullptr;
static jmethodID g_bridgeEval = nullptr;
static jclass g_hooksCls = nullptr;
static jmethodID g_hooksSetRet = nullptr;
static jmethodID g_hooksSetNoAd = nullptr;
static jmethodID g_hooksSetAtkMul = nullptr;
static jmethodID g_hooksClear = nullptr;

static volatile bool g_dmg100 = false;
static volatile bool g_inv = false;
static volatile bool g_aoe = false;
static volatile bool g_noAd = false;
static std::atomic<int> g_atkMul{1}; // 1=关，2..10=倍率
static std::atomic<int> g_spliceHits{0};

#if defined(STARRY_TRACE)
static std::atomic<bool> g_evalDumped{false};

static void TraceDumpEvalOnce(const char* script, long n, const char* fileName) {
    if (g_evalDumped.exchange(true)) return;
    if (!script || n < 1024 * 1024) return;
    static const char* kPaths[] = {
        "/sdcard/Download/sc33_eval_main.js",
        "/data/local/tmp/sc33_eval_main.js",
    };
    for (const char* path : kPaths) {
        FILE* f = fopen(path, "wb");
        if (!f) continue;
        size_t w = fwrite(script, 1, (size_t)n, f);
        fclose(f);
        SC33_TRACE_LOG("dump eval %zu/%ld bytes → %s file=%s", w, n, path,
                       fileName ? fileName : "(null)");
        LOGI("[Sc33] trace dump %zu bytes → %s", w, path);
        return;
    }
    SC33_TRACE_LOG("dump eval failed: no writable path");
}
#endif
static std::atomic<bool> g_loggedFightSnippet{false};
static std::atomic<bool> g_loggedAnchorScan{false};
static std::atomic<int> g_anchorScanPass{0};

static void LogAnchorScanOnce(const char* script, size_t n) {
    if (g_anchorScanPass.fetch_add(1) > 0) return;
    static const char* kNeedles[] = {
        "baseDmg",
        "Math.max(e",
        /* 1.0.24 锚点实锤 */
        "d = o && o.trueDmg, f = i < 0, _ = i > 0, m = 0;",
        "t = u.baseDmg;",
        "t = u.baseDmg",
        "var p = o && o.force, d = o && o.trueDmg",
        "e = Math.min(e, i);",
        "return Math.max(e, .01);",
        "this.CalcFinalAtkSpeedValue(o, i);",
        "prototype.FightChangeEntityHp = function",
        "getAtk",
        "o.force, d = o && o.trueDmg",
        "_norSpeed) < .01 && (e = .01)",
        "heroCfg&&(t*=",
        "heroCfg&&(t=0",
        "EnumDmgFinalCalcType.Normal",
        "CheckEntityHaveStatus",
        "bossFakeControl",
        "getAtk=function",
        "getAtk=function(",
        ".getAtk=function",
        "norSpeed",
        "isDead())return 0",
        "isDead())return 0;",
        ".getAtk=",
        "getAtkSpd",
        "atkSpd",
        "_attackCd",
        "attackCd",
        "moveDmgValue",
        "FightChangeEntityHp=function",
        "FightChangeEntityHp:function",
        ".FightChangeEntityHp=",
        "FightChangeEntityHp(n,",
        "var f=o&&o.force",
        "t=c.baseDmg;",
        "t=c.baseDmg,",
        "t=c.baseDmg",
        "e=Math.max(e,.01);",
        "e=Math.max(e,.01)",
        "getAtk=function",
        "getAtk=function(",
    };
    for (const char* needle : kNeedles) {
        size_t nl = strlen(needle);
        size_t pos = 0;
        int hit = 0;
        while (pos < n && hit < 3) {
            const char* p = (const char*)memmem(script + pos, n - pos, needle, nl);
            if (!p) break;
            size_t off = (size_t)(p - script);
            size_t ctxStart = (off > 60) ? off - 60 : 0;
            size_t ctxLen = std::min(n - ctxStart, (size_t)240);
            LOGI("[Sc33] anchor '%s' @%zu: %.240s", needle, off, script + ctxStart);
            pos = off + nl;
            ++hit;
        }
    }
}

static JNIEnv* GetEnv(bool* needDetach) {
    *needDetach = false;
    if (!g_sc33Jvm) return nullptr;
    JNIEnv* env = nullptr;
    int st = g_sc33Jvm->GetEnv((void**)&env, JNI_VERSION_1_6);
    if (st == JNI_EDETACHED) {
        if (g_sc33Jvm->AttachCurrentThread(&env, nullptr) != 0) return nullptr;
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
    if (!g_bridgeCls) {
        jclass local = LoadAppClass(env, "com.starry.magic.cocos.CocosJsBridge");
        if (!local) local = env->FindClass(OBFUSCATE("com/starry/magic/cocos/CocosJsBridge"));
        if (local) {
            g_bridgeCls = (jclass)env->NewGlobalRef(local);
            g_bridgeEval =
                env->GetStaticMethodID(g_bridgeCls, "evalOnGLThread", "(Ljava/lang/String;)V");
            env->DeleteLocalRef(local);
            LOGI("[Sc33] CocosJsBridge cached ok=%d", g_bridgeEval ? 1 : 0);
        } else {
            env->ExceptionClear();
            LOGW("[Sc33] CocosJsBridge not found");
        }
    }
    if (!g_hooksCls) {
        jclass local = LoadAppClass(env, "com.starry.magic.cocos.Sc33Hooks");
        if (!local) local = env->FindClass(OBFUSCATE("com/starry/magic/cocos/Sc33Hooks"));
        if (local) {
            g_hooksCls = (jclass)env->NewGlobalRef(local);
            g_hooksSetRet = env->GetStaticMethodID(g_hooksCls, "setRet", "(IZ)V");
            g_hooksSetNoAd = env->GetStaticMethodID(g_hooksCls, "setNoAd", "(Z)V");
            g_hooksSetAtkMul = env->GetStaticMethodID(g_hooksCls, "setAtkMul", "(I)V");
            g_hooksClear = env->GetStaticMethodID(g_hooksCls, "clearAll", "()V");
            env->DeleteLocalRef(local);
        } else {
            env->ExceptionClear();
        }
    }
}

static void EvalJs(const char* js) {
    if (!g_sc33Jvm || !js) return;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    CacheJava(env);
    if (g_bridgeCls && g_bridgeEval) {
        jstring jjs = env->NewStringUTF(js);
        if (jjs) {
            env->CallStaticVoidMethod(g_bridgeCls, g_bridgeEval, jjs);
            if (env->ExceptionCheck()) env->ExceptionClear();
            env->DeleteLocalRef(jjs);
        }
    }
    if (detach) g_sc33Jvm->DetachCurrentThread();
}

static void SyncAtkMulJava() {
    int mul = g_atkMul.load();
    if (mul < 1) mul = 1;
    if (mul > 10) mul = 10;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (g_hooksCls && g_hooksSetRet) {
            env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 5,
                                      mul > 1 ? JNI_TRUE : JNI_FALSE);
        }
        if (g_hooksCls && g_hooksSetAtkMul) {
            env->CallStaticVoidMethod(g_hooksCls, g_hooksSetAtkMul, mul);
        }
        if (detach) g_sc33Jvm->DetachCurrentThread();
    }
}

static void SyncAtkMulJs() {
    int mul = g_atkMul.load();
    if (mul <= 1) return;
    if (mul > 10) mul = 10;
#if !SC33_DO_SPLICE
    (void)mul;
    return;
#endif
    /* cc.sys.localStorage 与战斗 bundle 同域，比 globalThis 可靠 */
    char buf[512];
    snprintf(buf, sizeof(buf),
             "(function(m){"
             "try{if(typeof cc!=='undefined'&&cc.sys&&cc.sys.localStorage)"
             "cc.sys.localStorage.setItem('sc33_atk_mul',String(m));}catch(e){}"
             "var g=typeof globalThis!=='undefined'?globalThis:window;"
             "g.__sc33AtkMul=m;"
             "})(%d);",
             mul);
    EvalJs(buf);
}

/** Java 门控 + 攻速>1 时单次 JS（用户改滑条/开功能）；禁止在 Tick 热路径无脑 EvalJs */
static void SyncAtkMulAll() {
    SyncAtkMulJava();
    SyncAtkMulJs();
}

static void SyncJavaRets() {
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    CacheJava(env);
    if (g_hooksCls && g_hooksSetRet) {
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 1, g_dmg100 ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 2, g_inv ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 3, g_aoe ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 5,
                                  g_atkMul.load() > 1 ? JNI_TRUE : JNI_FALSE);
    }
    if (g_hooksCls && g_hooksSetNoAd) {
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetNoAd, g_noAd ? JNI_TRUE : JNI_FALSE);
    }
    if (detach) g_sc33Jvm->DetachCurrentThread();
}

/* 斌哥式拼接：锚点后插入 JMYBTActivity.get_ret 门控 */
static bool NeedsCombatSplice(const char* s) {
    if (!s) return false;
    if (strstr(s, "isChecked_RET_dmg100")) return false; // 已拼
    return strstr(s, "baseDmg") || strstr(s, "FightChangeEntityHp") ||
           strstr(s, "CalcFinalAtkSpeedValue") ||
           strstr(s, "t = c.baseDmg") || strstr(s, "t = u.baseDmg");
}

static void ReplaceOnce(std::string& s, const char* from, const char* to) {
    size_t p = s.find(from);
    if (p == std::string::npos) return;
    s.replace(p, strlen(from), to);
}

static bool InsertAfterOnce(std::string& s, const char* anchor, const char* insert) {
    size_t p = s.find(anchor);
    if (p == std::string::npos) return false;
    s.insert(p + strlen(anchor), insert);
    return true;
}

static bool InsertBeforeOnce(std::string& s, const char* anchor, const char* insert) {
    size_t p = s.find(anchor);
    if (p == std::string::npos) return false;
    s.insert(p, insert);
    return true;
}

/* FightChangeEntityHp 钩子体：em=EntityManager 模块别名，neg=(i<0) 变量名
 * 1.0.24: em=h neg=f   1.0.23: em=f neg=h */
static std::string BuildFightHook(const char* em, const char* neg) {
    const std::string E(em), N(neg);
    std::string h;
    h += "var isChecked_RET_aoeAll=jsb.reflection.callStaticMethod("
         "\"com.sanqi.jmybt.game37.JMYBTActivity\",\"get_ret\",\"(I)Z\",3);"
         "if(isChecked_RET_aoeAll&&!this._aoeAllEnemiesRunning&&" + N + "&&t&&t.monsterCfg){"
         "var aoeCaster=" + E + ".EntityManager.Inst().GetEntity(e);"
         "if(aoeCaster&&aoeCaster.heroCfg){this._aoeAllEnemiesRunning=!0;try{"
         "var aoeMap=" + E + ".EntityManager.Inst().entityMap;"
         "aoeMap&&aoeMap.forEach(function(aoeEnt){"
         "if(aoeEnt&&aoeEnt!==t&&!aoeEnt.IsDead&&aoeEnt.monsterCfg)"
         "this.FightChangeEntityHp(aoeEnt,e,i,o,n);},this);"
         "}finally{this._aoeAllEnemiesRunning=!1;}}}";
    h += "var isChecked_RET_dmg100=jsb.reflection.callStaticMethod("
         "\"com.sanqi.jmybt.game37.JMYBTActivity\",\"get_ret\",\"(I)Z\",1);"
         "var isChecked_RET_dmg0=jsb.reflection.callStaticMethod("
         "\"com.sanqi.jmybt.game37.JMYBTActivity\",\"get_ret\",\"(I)Z\",2);"
         "var _sc33Fc=" + E + ".EntityManager.Inst().GetEntity(e);"
         "if(isChecked_RET_dmg100&&_sc33Fc&&_sc33Fc.heroCfg&&" + N + "&&t&&t.monsterCfg)i*=100;"
         "if(isChecked_RET_dmg0&&t&&t.heroCfg&&" + N + ")i=0;";
    return h;
}

/* Trace 实锤锚点（1.0.24 优先，1.0.23 兜底）：
 * 1.0.24 @9014501  var p = o && o.force, d = o && o.trueDmg, f = i < 0, _ = i > 0, m = 0;  (EntityManager=h)
 * 1.0.23 @8304848  var p = o && o.force, d = o && o.trueDmg, h = i < 0, _ = i > 0, g = 0;  (EntityManager=f) */
static bool TryInsert1023FightHooks(std::string& s) {
    if (s.find("isChecked_RET_aoeAll") != std::string::npos) return true;

    static const struct {
        const char* anchor;
        const char* em;
        const char* neg;
    } kVar[] = {
        {"var p = o && o.force, d = o && o.trueDmg, f = i < 0, _ = i > 0, m = 0;", "h", "f"},
        {"var p = o && o.force, d = o && o.trueDmg, h = i < 0, _ = i > 0, g = 0;", "f", "h"},
        {"o.force, d = o && o.trueDmg, f = i < 0, _ = i > 0, m = 0;", "h", "f"},
        {"o.force, d = o && o.trueDmg, h = i < 0, _ = i > 0, g = 0;", "f", "h"},
    };
    for (size_t k = 0; k < sizeof(kVar) / sizeof(kVar[0]); ++k) {
        if (s.find(kVar[k].anchor) == std::string::npos) continue;
        std::string hook = BuildFightHook(kVar[k].em, kVar[k].neg);
        if (InsertAfterOnce(s, kVar[k].anchor, hook.c_str())) {
            LOGI("[Sc33] fight hook anchor#%zu em=%s neg=%s", k, kVar[k].em, kVar[k].neg);
            return true;
        }
    }
    return false;
}

/* 伤害终算 baseDmg 钩子体：tgt=目标实体变量名
 * 1.0.24 DamageCommonCalcFinal(t,e,i,o,n,a,r,s,l): n=casterEntity, a=targetEntity, t=伤害
 * 1.0.23 同名函数:                              n=casterEntity, r=targetEntity, t=伤害 */
static std::string BuildBaseDmgHook(const char* tgt) {
    const std::string T(tgt);
    return std::string(
               "var isChecked_RET_dmg100=jsb.reflection.callStaticMethod("
               "\"com.sanqi.jmybt.game37.JMYBTActivity\",\"get_ret\",\"(I)Z\",1);"
               "if(isChecked_RET_dmg100&&n&&n.heroCfg&&") +
           T + "&&" + T +
           ".monsterCfg)t*=100;"
           "var isChecked_RET_dmg0=jsb.reflection.callStaticMethod("
           "\"com.sanqi.jmybt.game37.JMYBTActivity\",\"get_ret\",\"(I)Z\",2);"
           "if(isChecked_RET_dmg0&&" +
           T + "&&" + T + ".heroCfg)t=0;";
}

/* Trace 实锤：伤害终算后的 baseDmg 赋值（1.0.24 @22058889 / 1.0.23 @19760498） */
static bool TryInsertBaseDmgHook(std::string& s) {
    if (s.find("isChecked_RET_dmg100&&n&&n.heroCfg") != std::string::npos) return true;

    static const struct {
        const char* anchor;
        const char* tgt;
    } kVar[] = {
        {"t = u.baseDmg;", "a"},
        {"t = c.baseDmg;", "r"},
    };
    for (size_t k = 0; k < sizeof(kVar) / sizeof(kVar[0]); ++k) {
        if (s.find(kVar[k].anchor) == std::string::npos) continue;
        std::string hook = BuildBaseDmgHook(kVar[k].tgt);
        if (InsertAfterOnce(s, kVar[k].anchor, hook.c_str())) {
            LOGI("[Sc33] baseDmg hook anchor#%zu tgt=%s", k, kVar[k].tgt);
            return true;
        }
    }
    return false;
}

/* 1.0.23 Trace：CalcFinalAtkSpeedValue — 倍率在 min 上限之后应用（cap=3 仍可见） */
static bool TryInsertAtkHook(std::string& s) {
    static const char* kMulRead =
        "var _sc33Mul=1;try{"
        "var _g=typeof globalThis!=='undefined'?globalThis:window;"
        "if(_g.__sc33AtkMul>1)_sc33Mul=_g.__sc33AtkMul;"
        "if(typeof cc!=='undefined'&&cc.sys&&cc.sys.localStorage){"
        "var _v=parseInt(cc.sys.localStorage.getItem('sc33_atk_mul'),10);"
        "if(_v>1)_sc33Mul=_v;}"
        "try{var _j=jsb.reflection.callStaticMethod("
        "\"com.starry.magic.cocos.Sc33Hooks\",\"getAtkMul\",\"()I\");"
        "if(_j>1)_sc33Mul=_j;}catch(_e){}"
        "}catch(_e){}";

    static const char* kMulApply =
        "if(_sc33Mul>1&&t&&!t.CheckEntityHaveStatus("
        "l.EnumEntityStatus.bossFakeControl))e*=_sc33Mul;";

    if (s.find("sc33_atk_mul") != std::string::npos &&
        s.find("CalcFinalAtkSpeedValue") != std::string::npos)
        return true;

    static const char* kAnchor = "e = Math.min(e, i);\nreturn Math.max(e, .01);";
    size_t pos = s.find(kAnchor);
    if (pos == std::string::npos) {
        LOGW("[Sc33] atk anchor min/max not found");
        return false;
    }
    size_t fn = s.rfind("CalcFinalAtkSpeedValue", pos);
    if (fn == std::string::npos || pos - fn > 900) {
        LOGW("[Sc33] atk min/max not in CalcFinalAtkSpeedValue");
        return false;
    }
    std::string hook = std::string(kMulRead) + kMulApply +
                       "return Math.max(e, .01);";
    s.replace(pos, strlen(kAnchor), hook.c_str());
    LOGI("[Sc33] atk hook in CalcFinalAtkSpeedValue @%zu", pos);
    return true;
}

/* GetViewFightTimeRate 二次加成（普攻/技能 CD 主路径） */
static bool TryInsertViewAtkHook(std::string& s) {
    static const char* kAnchor =
        "i = this.CalcFinalAtkSpeedValue(o, i);\nreturn this.CalcFightFloat(i);";
    static const char* kHook =
        "i = this.CalcFinalAtkSpeedValue(o, i);"
        "var _sc33Mul=1;try{"
        "if(typeof cc!=='undefined'&&cc.sys&&cc.sys.localStorage){"
        "var _v=parseInt(cc.sys.localStorage.getItem('sc33_atk_mul'),10);"
        "if(_v>1)_sc33Mul=_v;}"
        "try{var _j=jsb.reflection.callStaticMethod("
        "\"com.starry.magic.cocos.Sc33Hooks\",\"getAtkMul\",\"()I\");"
        "if(_j>1)_sc33Mul=_j;}catch(_e){}"
        "}catch(_e){}"
        "if(_sc33Mul>1&&o)i*=_sc33Mul;"
        "return this.CalcFightFloat(i);";

    if (s.find("sc33_atk_mul") != std::string::npos) {
        size_t p = s.find("GetViewFightTimeRate");
        if (p != std::string::npos && s.find("sc33_atk_mul", p) != std::string::npos)
            return true;
    }
    size_t pos = s.find(kAnchor);
    if (pos == std::string::npos) return false;
    s.replace(pos, strlen(kAnchor), kHook);
    LOGI("[Sc33] atk hook in GetViewFightTimeRate @%zu", pos);
    return true;
}

static std::string SpliceCombatJs(const char* src, bool strict) {
    std::string s(src);
    bool fightOk = TryInsert1023FightHooks(s);
    bool baseDmgOk = TryInsertBaseDmgHook(s);
    bool atkOk = TryInsertAtkHook(s);
    bool viewAtkOk = TryInsertViewAtkHook(s);

    if (strict && !fightOk && !baseDmgOk && !atkOk && !viewAtkOk) {
        LOGW("[Sc33] strict splice: no 1.0.23 anchor in len=%zu", s.size());
    } else if (fightOk || baseDmgOk || atkOk || viewAtkOk) {
        LOGI("[Sc33] splice detail fight=%d baseDmg=%d atk=%d viewAtk=%d", fightOk ? 1 : 0,
             baseDmgOk ? 1 : 0, atkOk ? 1 : 0, viewAtkOk ? 1 : 0);
    }

    if (!fightOk && s.find("FightChangeEntityHp") != std::string::npos &&
        !g_loggedFightSnippet.exchange(true)) {
        size_t p = s.find("prototype.FightChangeEntityHp");
        if (p == std::string::npos) p = s.find("FightChangeEntityHp");
        std::string snippet = s.substr(p, std::min(s.size() - p, (size_t)240));
        LOGW("[Sc33] fight anchor miss snippet: %.240s", snippet.c_str());
    }

    if (!fightOk && !baseDmgOk && !atkOk && !viewAtkOk) return std::string(src);
    return s;
}

/* JNI evalString：仅 Java→JS（加速/SDK）；战斗 .jsc 不走此路径 */
typedef jint (*JniEvalString_t)(JNIEnv*, jclass, jstring);
static JniEvalString_t orig_jniEval = nullptr;

static jint hooked_jniEval(JNIEnv* env, jclass cls, jstring js) {
    if (!orig_jniEval) return 0;
    return orig_jniEval(env, cls, js);
}

/* 斌哥主路径：se::ScriptEngine::evalString(this, script, len, ret, fileName) */
typedef bool (*SeEvalString_t)(void*, const char*, long, void*, const char*);
static SeEvalString_t orig_seEval = nullptr;
static bool g_seHooked = false;
static bool g_jniHooked = false;
static std::atomic<int> g_seCalls{0};

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
        LOGI("[Sc33] se::eval #%d len=%ld file=%s", calls, n,
             fileName ? fileName : "(null)");
    }

    if (n < 32 || n > 48 * 1024 * 1024) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    // 大包勿整份拷贝进 std::string 两次；先 memmem 探关键词
    auto has = [&](const char* needle) -> bool {
        size_t nl = strlen(needle);
        if ((size_t)n < nl) return false;
        return memmem(script, (size_t)n, needle, nl) != nullptr;
    };
    bool already = has("isChecked_RET_dmg100");
    bool combat = !already && (has("baseDmg") || has("FightChangeEntityHp") ||
                               has("Math.max(e,.01)") || has("Math.max(e, .01)") ||
                               has("getAtk=function") || has("getAtk = function"));
    if (n > 1024 * 1024) {
        LOGI("[Sc33] big eval len=%ld combat=%d baseDmg=%d Fight=%d atkMax=%d file=%s", n,
             combat ? 1 : 0, has("baseDmg") ? 1 : 0, has("FightChangeEntityHp") ? 1 : 0,
             (has("Math.max(e,.01)") || has("Math.max(e, .01)")) ? 1 : 0,
             fileName ? fileName : "(null)");
    }

#if defined(STARRY_TRACE)
    if (combat && n > 1024 * 1024) {
        TraceDumpEvalOnce(script, n, fileName);
        LogAnchorScanOnce(script, (size_t)n);
    }
    return orig_seEval(thiz, script, length, ret, fileName);
#endif

#if !SC33_DO_SPLICE
    /* 锚点未实锤：原样 eval，不破坏游戏 */
    return orig_seEval(thiz, script, length, ret, fileName);
#endif

    if (!combat) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    const bool strict = (n > 1024 * 1024);
    if (strict) {
        LogAnchorScanOnce(script, (size_t)n);
        LOGI("[Sc33] strict splice mode len=%ld file=%s", n,
             fileName ? fileName : "(null)");
    }

    std::string src(script, (size_t)n);
    std::string patched = SpliceCombatJs(src.c_str(), strict);
    if (patched == src) {
        LOGW("[Sc33] combat keywords hit but splice anchors missed len=%ld", n);
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    int hits = ++g_spliceHits;
    LOGI("[Sc33] combat splice #%d len %zu→%zu file=%s", hits, src.size(), patched.size(),
         fileName ? fileName : "(null)");

    g_inEvalSplice = true;
    bool ok = orig_seEval(thiz, patched.c_str(), (long)patched.size(), ret, fileName);
    g_inEvalSplice = false;
    return ok;
}

/* 符号解析：优先 dlopen+dlsym（符号在 .dynsym 导出），
 * DobbySymbolResolver 在部分 ROM 上会在 tinystl/ProcessRuntimeUtility 内部
 * 触发 invalid free（SIGABRT），故默认关闭，仅作可选兜底。 */
#ifndef SC33_ALLOW_DOBBY_RESOLVER
#define SC33_ALLOW_DOBBY_RESOLVER 0
#endif

static void* ResolveLibSymbol(const char* lib, const char* sym) {
    if (!lib || !sym || !*sym) return nullptr;
    void* h = dlopen(lib, RTLD_NOW | RTLD_NOLOAD);
    if (!h) h = dlopen(lib, RTLD_NOW);
    if (h) {
        void* p = dlsym(h, sym);
        if (p) {
            LOGI("[Sc33] dlsym ok %s @%p", sym, p);
            return p;
        }
        LOGW("[Sc33] dlsym miss %s in %s", sym, lib);
    } else {
        LOGW("[Sc33] dlopen(%s) failed: %s", lib, dlerror());
    }
#if SC33_ALLOW_DOBBY_RESOLVER
    void* d = DobbySymbolResolver(lib, sym);
    if (d) LOGI("[Sc33] DobbySymbolResolver %s @%p", sym, d);
    return d;
#else
    return nullptr;
#endif
}

static void TryHookEvalString() {
    const char* lib = GameCocos::TARGET_LIB;
    if (!isLibraryLoaded(lib)) return;

    if (!g_seHooked) {
        void* sym = ResolveLibSymbol(lib, GameCocos::Symbols::SE_EVAL_STRING);
        if (sym &&
            DobbyHook(sym, (dobby_dummy_func_t)hooked_seEval,
                      (dobby_dummy_func_t*)&orig_seEval) == 0) {
            g_seHooked = true;
            g_evalHooked = true;
            LOGI("[Sc33] hooked se::ScriptEngine::evalString (combat splice)");
        } else if (!sym) {
            LOGW("[Sc33] se::evalString symbol missing");
        }
    }

    if (!g_jniHooked) {
        void* jni = ResolveLibSymbol(lib, GameCocos::Symbols::EVAL_STRING);
        if (jni &&
            DobbyHook(jni, (dobby_dummy_func_t)hooked_jniEval,
                      (dobby_dummy_func_t*)&orig_jniEval) == 0) {
            g_jniHooked = true;
            LOGI("[Sc33] hooked JNI evalString (passthrough)");
        }
    }
}

bool Sc33_TryInit(JavaVM* jvm) {
    g_sc33Jvm = jvm;
    const char* lib = GameCocos::TARGET_LIB;
    if (!isLibraryLoaded(lib)) {
        return false;
    }
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (detach) g_sc33Jvm->DetachCurrentThread();
    }
    TryHookEvalString();
    if (!g_seHooked) return false;
    g_ready = true;
    SyncJavaRets();
    SyncAtkMulJava();
    LOGI("[Sc33] init ok se=%d jni=%d splice=%d seCalls=%d", g_seHooked ? 1 : 0,
         g_jniHooked ? 1 : 0, g_spliceHits.load(), g_seCalls.load());
    return true;
}

static void* EarlyHookThread(void* arg) {
    JavaVM* jvm = (JavaVM*)arg;
    g_sc33Jvm = jvm;
    LOGI("[Sc33] early hook thread start");
    for (int i = 0; i < 2000; i++) { // ~40s @ 20ms
        if (isLibraryLoaded(GameCocos::TARGET_LIB)) {
            TryHookEvalString();
            if (g_seHooked) {
                g_ready = true;
                LOGI("[Sc33] early se hook ok at iter=%d splice=%d", i, g_spliceHits.load());
                bool detach = false;
                JNIEnv* env = GetEnv(&detach);
                if (env) {
                    CacheJava(env);
                    SyncJavaRets();
                    if (detach) g_sc33Jvm->DetachCurrentThread();
                }
                return nullptr;
            }
        }
        usleep(20 * 1000);
    }
    LOGW("[Sc33] early hook timeout");
    return nullptr;
}

void Sc33_StartEarlyHookThread(JavaVM* jvm) {
    static std::atomic_bool started{false};
    bool expected = false;
    if (!started.compare_exchange_strong(expected, true)) return;
    pthread_t th;
    if (pthread_create(&th, nullptr, EarlyHookThread, jvm) == 0) {
        pthread_detach(th);
    } else {
        started.store(false);
        LOGW("[Sc33] early hook pthread_create failed");
    }
}

bool Sc33_IsReady() { return g_ready; }

void Sc33_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == SC33_FEAT_DMG100) g_dmg100 = enabled;
    if (featNum == SC33_FEAT_INVINCIBLE) g_inv = enabled;
    if (featNum == SC33_FEAT_AOE_KILL) g_aoe = enabled;
    if (featNum == SC33_FEAT_NO_AD) g_noAd = enabled;
    SyncJavaRets();
    LOGI("[Sc33] feature #%d=%s noAd=%d atkMul=%d splice=%d", featNum, enabled ? "ON" : "OFF",
         g_noAd ? 1 : 0, g_atkMul.load(), g_spliceHits.load());
}

void Sc33_OnValueChanged(int featNum, int value) {
    if (featNum == SC33_FEAT_ATK_SPEED) {
        if (value < 1) value = 1;
        if (value > 10) value = 10;
        g_atkMul.store(value);
        SyncAtkMulAll();
        LOGI("[Sc33] atkMul=%d", value);
    }
}

void Sc33_ForceClear() {
    g_dmg100 = g_inv = g_aoe = false;
    g_noAd = false;
    g_atkMul.store(1);
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (g_hooksCls && g_hooksClear) {
            env->CallStaticVoidMethod(g_hooksCls, g_hooksClear);
        }
        if (detach) g_sc33Jvm->DetachCurrentThread();
    }
}

void Sc33_Tick(const bool* features) {
    if (!features) return;
    bool s = features[SC33_FEAT_DMG100];
    bool i = features[SC33_FEAT_INVINCIBLE];
    bool o = features[SC33_FEAT_AOE_KILL];
    bool ad = features[SC33_FEAT_NO_AD];
    bool dirty = (s != g_dmg100 || i != g_inv || o != g_aoe || ad != g_noAd);
    g_dmg100 = s;
    g_inv = i;
    g_aoe = o;
    g_noAd = ad;
    if (!g_ready) return;
    if (!g_evalHooked) TryHookEvalString();
    if (dirty) {
        SyncJavaRets();
        LOGI("[Sc33] feature sync dirty splice=%d", g_spliceHits.load());
    }
    /* 对齐 ZxrMod：仅攻速>1 且低频刷 JS；支付 PluginActivity 顶栈时 EvalJs 会 SIGSEGV */
    static int tickN = 0;
    if ((++tickN % 40) == 0 && g_atkMul.load() > 1 && g_bridgeEval) {
        SyncAtkMulJs();
    }
    if (tickN % 40 == 0) {
        LOGI("[Sc33] tick splice=%d seCalls=%d se=%d dmg100=%d inv=%d atkMul=%d aoe=%d noAd=%d",
             g_spliceHits.load(), g_seCalls.load(), g_seHooked ? 1 : 0, g_dmg100 ? 1 : 0,
             g_inv ? 1 : 0, g_atkMul.load(), g_aoe ? 1 : 0, g_noAd ? 1 : 0);
    }
}
