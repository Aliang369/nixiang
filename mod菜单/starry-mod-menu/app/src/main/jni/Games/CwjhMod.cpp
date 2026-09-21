/* ================================================================
 *  超维进化 MOD (com.grinning.beta.yszl v2.2)
 *  战斗：game.js 明文（内置 2.1首发系 / 热更 0.2.89+ 重构系双形态），
 *        hook se::ScriptEngine::evalString 按锚点 splice 属性增益（同镇邪人模式）。
 *  20260909 功能重排（用户定稿）：移除秒杀/无敌/无限技能（封号风险），
 *        改为属性增益叠加——全部为现有值上的加法/乘法，任何情况不 nerf：
 *   0 超级回血   _timer 1s tick 己方多跳三次 recHp（回复速度 ×4，20260921 上调）
 *   1 超级回蓝   同 tick 己方多跳三次 recSp（×4，20260921 上调）
 *   2 高暴击率   暴击判定值 +60pp（原 +40pp，20260921 再 +20pp；玩家类+宠物类，己方攻击方）
 *   3 高暴击伤害 暴击倍率再 ×2（原倍率保留相乘，己方攻击方）
 *   4 高防御     护甲减伤 AMR +15pp（钳制后叠加，0.95 安全上限，己方受击方）
 *   5 高抗性     抗性净值 ×2（钳制后叠加，0.95 安全上限，己方受击方）
 *   6 高闪避     闪避值 +35pp（原 +15pp，20260921 再 +20pp；己方受击方）
 *   7 免广告     JS 侧强制走 skip 分支（C2S_WATCH_ADS_END 直领，需在线）
 *   8 秒杀（封号）doDmg 入口 己方攻击伤害=受击方 CUR_HP+1（避免天文数字进 Dps 统计）
 *   9 无敌（封号）doDmg 入口 己方受击伤害归零
 *  己方判定：sideType==1（MY_SIDE 字面量，内置/热更枚举一致，硬编码）。
 *  反篡改：libcocos2djs kill 包装器 + 2.2 炸弹函数（libc kill 路径）early 中和。
 *
 *  【20260921 热更重校（0.3.5，game.js 2,767,392 B / sha256 2fa45bc4…）】
 *   漂移面：① doDmg 签名再追尾加形参 C（0.3.3 加 u → 0.3.5 为 14 参），旧签名全 miss；
 *          ② 防御/抗性钳制行改写——变量 r/a→o/c，上限由字面量 .9/.5 改为函数
 *             StatusDef.getMinEdr(this)（形态 C 新锚点）；③ 暴击副本增至 3、暴伤副本 2。
 *          ①回血 ②暴击 ③暴伤 ⑥闪避 ⑦免广告 命中数与 0.3.x 同形（timer1/chcB3/chdB1+2+
 *          chdB2×2/ddgB1/ads1）。
 *   签名（0.3.5）：doDmg(r, a, o, c, h, l, _, d, u, m = !0, g = !1, p = i.Normal, f = !1, C = -1) {
 *           语义经函数体守卫实锤：r=伤害（首参）、o=攻击者（三参，o.sideType/o.isChar/o.trigger）、
 *           this=受击方；函数体内 t.SideType.MY_SIDE 仍在用，别名未漂移。
 *          已改为「前缀 + 下一个 ") {"」动态取签名，后续再追尾加形参不必改代码。
 *  实测 splice：热更档 2,767,392 → 2,769,815（+2423，离线模拟）；内置档 +2277（运行时实证）。
 * ================================================================ */

#include "CwjhMod.h"
#include "Includes/obfuscate.h"

#include <atomic>
#include <cstdio>
#include <cstring>
#include <pthread.h>
#include <string>
#include <unistd.h>
#include <cerrno>

#if defined(__linux__) || defined(__ANDROID__)
#include <string.h> // memmem
#endif

#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"
#include "Dobby/dobby.h"

static JavaVM* g_cwjhJvm = nullptr;
static bool g_ready = false;
static bool g_evalHooked = false;
static thread_local bool g_inEvalSplice = false;

static jclass g_bridgeCls = nullptr;
static jmethodID g_bridgeEval = nullptr;
static jclass g_hooksCls = nullptr;
static jmethodID g_hooksSetRet = nullptr;
static jmethodID g_hooksSetNoAd = nullptr;
static jmethodID g_hooksClear = nullptr;

static volatile bool g_feat[CWJH_FEAT_COUNT] = {};  // 0回血 1回蓝 2暴率 3暴伤 4防御 5抗性 6闪避 7免广告
static std::atomic<int> g_spliceHits{0};

static JNIEnv* GetEnv(bool* needDetach) {
    *needDetach = false;
    if (!g_cwjhJvm) return nullptr;
    JNIEnv* env = nullptr;
    int st = g_cwjhJvm->GetEnv((void**)&env, JNI_VERSION_1_6);
    if (st == JNI_EDETACHED) {
        if (g_cwjhJvm->AttachCurrentThread(&env, nullptr) != 0) return nullptr;
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
            LOGI("[Cwjh] CocosJsBridge cached ok=%d", g_bridgeEval ? 1 : 0);
        } else {
            env->ExceptionClear();
            LOGW("[Cwjh] CocosJsBridge not found");
        }
    }
    if (!g_hooksCls) {
        jclass local = LoadAppClass(env, "com.starry.magic.cocos.CwjhHooks");
        if (!local) local = env->FindClass(OBFUSCATE("com/starry/magic/cocos/CwjhHooks"));
        if (local) {
            g_hooksCls = (jclass)env->NewGlobalRef(local);
            g_hooksSetRet = env->GetStaticMethodID(g_hooksCls, "setRet", "(IZ)V");
            g_hooksSetNoAd = env->GetStaticMethodID(g_hooksCls, "setNoAd", "(Z)V");
            g_hooksClear = env->GetStaticMethodID(g_hooksCls, "clearAll", "()V");
            env->DeleteLocalRef(local);
        } else {
            env->ExceptionClear();
        }
    }
}

static void EvalJs(const char* js) {
    if (!g_cwjhJvm || !js) return;
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
    if (detach) g_cwjhJvm->DetachCurrentThread();
}

static void SyncJavaRets() {
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    CacheJava(env);
    if (g_hooksCls && g_hooksSetRet) {
        // 0 基对齐菜单下标：0回血 1回蓝 2暴率 3暴伤 4防御 5抗性 6闪避 7免广告
        for (int i = 0; i < CWJH_FEAT_COUNT; i++) {
            env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, i,
                                      g_feat[i] ? JNI_TRUE : JNI_FALSE);
        }
    }
    if (g_hooksCls && g_hooksSetNoAd) {
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetNoAd,
                                  g_feat[CWJH_FEAT_NO_AD] ? JNI_TRUE : JNI_FALSE);
    }
    if (detach) g_cwjhJvm->DetachCurrentThread();
}

/* ---------------- game.js splice ----------------
 * get_ret 索引（0 基，与菜单下标一致）：
 *   0=超级回血 1=超级回蓝 2=高暴击率 3=高暴击伤害
 *   4=高防御 5=高抗性 6=高闪避 7=免广告（走 adsPlay）
 * Activity = org.cocos2dx.javascript.AppActivity（get_ret 由首包脚本注入 smali）
 * 双形态锚点：内置 2.2（2.1首发系）与热更 0.2.89+（重构暴击/钳 0.5）各一套，
 * 均经 20260909 双文件逐一验证；多副本锚点（暴率/暴伤 玩家+宠物类）全替换。
 */
#define CWJH_RET "\"org.cocos2dx.javascript.AppActivity\",\"get_ret\",\"(I)Z\","
#define CWJH_GATE(i) "jsb.reflection.callStaticMethod(" CWJH_RET #i ")"

/* 功能数值（20260921 用户定稿，改这里即可，勿散落字面量）
 *   回血/回蓝：每 tick 己方 recHp/recSp 调用总次数（原始 1 次，倍率 = 该值）
 *   暴击率/闪避：在「当前值」上的加算 pp（原 +.4 / +.15，本次各再 +20pp） */
#define CWJH_HP_TICK_MULT   4       /* 超级回血 ×4 */
#define CWJH_SP_TICK_MULT   4       /* 超级回蓝 ×4 */
#define CWJH_CHC_BONUS      ".6"    /* 高暴击率 +60pp（含原 +40pp） */
#define CWJH_DODGE_BONUS    ".35"   /* 高闪避 +35pp（含原 +15pp） */

static bool NeedsCombatSplice(const char* s, size_t n) {
    if (!s) return false;
    auto has = [&](const char* needle) -> bool {
        size_t nl = strlen(needle);
        if (n < nl) return false;
        return memmem(s, n, needle, nl) != nullptr;
    };
    if (has("\"org.cocos2dx.javascript.AppActivity\",\"get_ret\"")) return false; // 已拼
    // 战斗文件识别：_timer 回复 tick / adsPlay / doDmg 三签名（doDmg 供 ⑧ 秒杀/无敌）
    return has("_timer() {\nthis.recHp();") ||
           has("async adsPlay(t, e, i = \"\")") ||
           has("doDmg(r, a, o, c, h, l, _, d, m, u = !0") ||  // 0.2.9x+ 热更
           has("doDmg(e, r, a, o, c, h, l, _, d, m = !0") ||   // 0.2.83
           has("doDmg(e, n, r, a, o, c, h, l, _, d = !0");     // 2.1 首发/2.2 内置
}

static void ReplaceAll(std::string& s, const char* from, const char* to) {
    size_t p = s.find(from);
    while (p != std::string::npos) {
        s.replace(p, strlen(from), to);
        p = s.find(from, p + strlen(to));
    }
}

static std::string SpliceCombatJs(const char* src) {
    std::string s(src);

    // ① 超级回血/回蓝：_timer 1s tick，己方单位各多跳 N-1 次（回复速度 ×N，N=CWJH_*_TICK_MULT）
    //    锚点内置/热更同形，各 1 处。不做任何公式改动，纯 tick 次数放大。
    {
        static const char* kAnchor = "_timer() {\nthis.recHp();\nthis.recSp();\n}";
        if (s.find(kAnchor) != std::string::npos) {
            // 注意：CWJH_GATE 用 #i 字符串化，只能喂字面量（喂变量会展开成变量名，
            // 20260921 实测：gated(0,...) 传 int 参数 → JS 里变成未定义标识符 idx，tick 抛异常）
            auto gated = [](const char* gate, const char* fn) {
                return std::string("if(") + gate + "&&this.sideType==1)this." + fn + "();\n";
            };
            std::string rep = "_timer() {\n";
            for (int i = 0; i < CWJH_HP_TICK_MULT - 1; i++) rep += gated(CWJH_GATE(0), "recHp");
            rep += "this.recHp();\n";
            for (int i = 0; i < CWJH_SP_TICK_MULT - 1; i++) rep += gated(CWJH_GATE(1), "recSp");
            rep += "this.recSp();\n}";
            ReplaceAll(s, kAnchor, rep.c_str());
        }
    }

    // ② 高暴击率：暴击判定值 +CWJH_CHC_BONUS（己方攻击方；玩家类+宠物类全替换）
    {
        // 形态A（2.2 内置/2.1首发系）
        static const char* kChcA =
            "u = t ? this.getPropVal(\"CHC\") + (this.getPropVal(\"SK_CHC\", t.skId.toString()) || 0) + "
            "(this.getPropVal(\"SK_CHC\", \"0\") || 0) : this.getPropVal(\"CHC\");";
        std::string repA = std::string(kChcA) + "\nif(" CWJH_GATE(2) "&&this.sideType==1)u+=" CWJH_CHC_BONUS ";";
        ReplaceAll(s, kChcA, repA.c_str());
        // 形态B（0.2.89+ 热更重构）
        static const char* kChcB = "let i = e ? e._dmgChc || 0 : this.getPropVal(\"CHC\") || 0;";
        std::string repB = std::string(kChcB) + "\nif(" CWJH_GATE(2) "&&this.sideType==1)i+=" CWJH_CHC_BONUS ";";
        ReplaceAll(s, kChcB, repB.c_str());
    }

    // ③ 高暴击伤害：原暴伤倍率再 ×2（乘法叠加，任何情况不低于原值，己方攻击方）
    {
        // 形态A：m *= t ? Math.max(1, CHD+SK_CHD+触发) : Math.max(1, CHD+触发);
        static const char* kChdA =
            "m *= t ? Math.max(1, this.getPropVal(\"CHD\") + (this.getPropVal(\"SK_CHD\", t.skId.toString()) || 0) + "
            "(this.getPropVal(\"SK_CHD\", \"0\") || 0) + e) : Math.max(1, this.getPropVal(\"CHD\") + e);";
        std::string repA = std::string(
            "m *= (jsb.reflection.callStaticMethod(" CWJH_RET
            "3)&&this.sideType==1?2:1) * (t ? Math.max(1, this.getPropVal(\"CHD\") + "
            "(this.getPropVal(\"SK_CHD\", t.skId.toString()) || 0) + (this.getPropVal(\"SK_CHD\", \"0\") || 0) + e) : "
            "Math.max(1, this.getPropVal(\"CHD\") + e));");
        ReplaceAll(s, kChdA, repA.c_str());
        // 形态B：c *= Math.max(1, i + t);（玩家）/ o *= …（宠物）
        static const char* kChdB1 = "c *= Math.max(1, i + t);";
        std::string repB1 = std::string(
            "c *= (jsb.reflection.callStaticMethod(" CWJH_RET
            "3)&&this.sideType==1?2:1) * Math.max(1, i + t);");
        ReplaceAll(s, kChdB1, repB1.c_str());
        static const char* kChdB2 = "o *= Math.max(1, i + t);";
        std::string repB2 = std::string(
            "o *= (jsb.reflection.callStaticMethod(" CWJH_RET
            "3)&&this.sideType==1?2:1) * Math.max(1, i + t);");
        ReplaceAll(s, kChdB2, repB2.c_str());
    }

    // ④ 高防御：护甲减伤 AMR +15pp（己方受击方；沿用引擎自己的钳制作为上限，不另设 0.95）
    {
        // 形态C（0.3.5+，20260921 二次热更）：变量改名 o，上限改成函数 StatusDef.getMinEdr(this)
        static const char* kAmrC = "o = Math.min(Math.max(o, -1), t.StatusDef.getMinEdr(this));";
        std::string repC = std::string(kAmrC) + "\nif(" CWJH_GATE(
            4) "&&i.sideType==1){o+=.15;o=Math.min(o,t.StatusDef.getMinEdr(this));}";
        ReplaceAll(s, kAmrC, repC.c_str());
        // 形态A（钳 0.9）：r = Math.min(Math.max(r, -1), .9);（e=受击方）
        static const char* kAmrA = "r = Math.min(Math.max(r, -1), .9);";
        std::string repA = std::string(kAmrA) + "\nif(" CWJH_GATE(
            4) "&&e.sideType==1){r+=.15;r=Math.min(r,.95);}";
        ReplaceAll(s, kAmrA, repA.c_str());
        // 形态B（钳 0.5）：i=受击方
        static const char* kAmrB = "r = Math.min(Math.max(r, -1), .5);";
        std::string repB = std::string(kAmrB) + "\nif(" CWJH_GATE(
            4) "&&i.sideType==1){r+=.15;r=Math.min(r,.95);}";
        ReplaceAll(s, kAmrB, repB.c_str());
    }

    // ⑤ 高抗性：抗性净值 ×2（己方受击方；钳制后叠加，沿用引擎钳制作上限）
    {
        // 形态C（0.3.5+）：变量改名 c，上限 StatusDef.getMinEdr(this)
        static const char* kResC = "c = Math.min(Math.max(c, -1), t.StatusDef.getMinEdr(this));";
        std::string repC = std::string(kResC) + "\nif(" CWJH_GATE(
            5) "&&i.sideType==1){c*=2;c=Math.min(c,t.StatusDef.getMinEdr(this));}";
        ReplaceAll(s, kResC, repC.c_str());
        // 形态A（钳 0.9）
        static const char* kResA = "a = Math.min(Math.max(a, -1), .9);";
        std::string repA = std::string(kResA) + "\nif(" CWJH_GATE(
            5) "&&e.sideType==1){a*=2;a=Math.min(a,.95);}";
        ReplaceAll(s, kResA, repA.c_str());
        // 形态B（钳 0.5）
        static const char* kResB = "a = Math.min(Math.max(a, -1), .5);";
        std::string repB = std::string(kResB) + "\nif(" CWJH_GATE(
            5) "&&i.sideType==1){a*=2;a=Math.min(a,.95);}";
        ReplaceAll(s, kResB, repB.c_str());
    }

    // ⑥ 高闪避：闪避值 +CWJH_DODGE_BONUS（己方受击方；对面命中仍会抵消，引擎 roll 封顶保留为安全网）
    {
        // 形态A：s = e.getPropVal("DGC") 行后
        static const char* kDdgA = "s += e.trigger(skill.TriggerTiming.PreDodge, i, e, s);";
        std::string repA = std::string(kDdgA) + "\nif(" CWJH_GATE(6) "&&e.sideType==1)s+=" CWJH_DODGE_BONUS ";";
        ReplaceAll(s, kDdgA, repA.c_str());
        // 形态B
        static const char* kDdgB = "n += i.trigger(skill.TriggerTiming.PreDodge, s, i, n);";
        std::string repB = std::string(kDdgB) + "\nif(" CWJH_GATE(6) "&&i.sideType==1)n+=" CWJH_DODGE_BONUS ";";
        ReplaceAll(s, kDdgB, repB.c_str());
    }

    // ⑦ 免广告：adsPlay 开头直接走 skip 分支（C2S_WATCH_ADS_END 服务器照常发奖，需在线）
    {
        static const char* kAnchor = "async adsPlay(t, e, i = \"\") {";
        if (s.find(kAnchor) != std::string::npos) {
            std::string rep = std::string(kAnchor);
            rep += "\nif(jsb.reflection.callStaticMethod(" CWJH_RET "7)){this._isPlaying=!0;"
                   "try{let __r=await network.rpcCall(pb.MessageID.C2S_WATCH_ADS_END,"
                   "pb.WatchAdsArg.encode({Type:t,Data:e}));"
                   "if(0==__r.errcode){let __w=pb.WatchAdsReply.decode(__r.payload);"
                   "return {succ:!0,rdata:__w};}}finally{this._isPlaying=!1;}}";
            ReplaceAll(s, kAnchor, rep.c_str());
        }
    }

    // ⑧ 秒杀/无敌（20260909 用户要求加回，菜单尾部署，高风险自担）：
    //    doDmg 入口（this=受击方）。签名随热更被压缩器重命名（语义经函数体守卫实锤：
    //    首参=伤害，三参=攻击者）。无敌=己方受击归零；秒杀=己方攻击伤害=当前HP+1（避免天文数字进统计）。
    {
        // 0.3.5+ 起签名会继续追尾加形参（0.3.3 加 u、0.3.5 加 C = -1），
        // 故先按「前缀 + 下一个 ") {"」动态取整条签名，避免每次热更重编一端。
        static const char* kPrefix = "doDmg(r, a, o, c, h, l, _, d, u, m = !0";
        struct DmgSig { const char* anchor; const char* dmg; const char* atk; };
        static const DmgSig kSigs[] = {
            {"doDmg(r, a, o, c, h, l, _, d, m, u = !0, g = !1, p = i.Normal, f = !1) {",
             "r", "o"},                                                       // 0.2.9x 热更
            {"doDmg(e, r, a, o, c, h, l, _, d, m = !0, u = !1, g = i.Normal, p = !1) {",
             "e", "a"},                                                       // 0.2.83
            {"doDmg(e, n, r, a, o, c, h, l, _, d = !0, m = !1, u = i.Normal) {",
             "e", "r"},                                                       // 2.1 首发/2.2 内置
        };
        auto inject = [&](const std::string& anchor, const char* dmg, const char* atk) -> bool {
            if (s.find(anchor) == std::string::npos) return false;
            std::string rep = anchor;
            // 无敌（get_ret 9）：己方单位受伤归零
            rep += "\nif(jsb.reflection.callStaticMethod(" CWJH_RET "9)"
                   "&&this.sideType==t.SideType.MY_SIDE){return 0;}";
            // 秒杀（get_ret 8）：己方攻击时伤害=当前HP+1（致死量，避免天文数字进 Dps 统计）
            rep += std::string("\nif(jsb.reflection.callStaticMethod(" CWJH_RET "8)")
                   + "&&" + atk + "&&this.sideType!=" + atk + ".sideType&&" + atk +
                   ".sideType==t.SideType.MY_SIDE)"
                   "{" + dmg + "=this.getPropVal(\"CUR_HP\")+1;}";
            ReplaceAll(s, anchor.c_str(), rep.c_str());
            return true;
        };
        size_t p = s.find(kPrefix);
        bool done = false;
        if (p != std::string::npos) {
            size_t e = s.find(") {", p);
            if (e != std::string::npos && e - p < 220) {
                // 语义经函数体守卫实锤：首参 r=伤害、三参 o=攻击者、this=受击方（0.3.x/0.3.5 一致）
                done = inject(s.substr(p, e + 3 - p), "r", "o");
            }
        }
        if (!done) {
            for (const auto& sig : kSigs) {
                if (inject(sig.anchor, sig.dmg, sig.atk)) break;
            }
        }
    }

    return s;
}

/* JNI evalString：仅 Java→JS 桥（passthrough） */
typedef jint (*JniEvalString_t)(JNIEnv*, jclass, jstring);
static JniEvalString_t orig_jniEval = nullptr;

static jint hooked_jniEval(JNIEnv* env, jclass cls, jstring js) {
    if (!orig_jniEval) return 0;
    return orig_jniEval(env, cls, js);
}

/* 主路径：se::ScriptEngine::evalString(this, script, len, ret, fileName) */
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
#if defined(STARRY_TRACE)
    if (calls <= 8 || calls % 200 == 0) {
        LOGI("[Cwjh] se::eval #%d len=%ld file=%s", calls, n,
             fileName ? fileName : "(null)");
    }
#else
    (void)calls;
#endif

    if (n < 32 || n > 48 * 1024 * 1024) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    if (!NeedsCombatSplice(script, (size_t)n)) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    std::string src(script, (size_t)n);
    std::string patched = SpliceCombatJs(src.c_str());
    if (patched == src) {
        LOGW("[Cwjh] combat keywords hit but splice anchors missed len=%ld", n);
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    int hits = ++g_spliceHits;
    LOGI("[Cwjh] combat splice #%d len %zu→%zu file=%s", hits, src.size(), patched.size(),
         fileName ? fileName : "(null)");

    g_inEvalSplice = true;
    bool ok = orig_seEval(thiz, patched.c_str(), (long)patched.size(), ret, fileName);
    g_inEvalSplice = false;
    return ok;
}

static void* Cwjh_ResolveLibSym(const char* lib, const char* symName) {
    // 本游戏 extractNativeLibs=false：so 从 base.apk 内映射，maps/dlopen 无独立路径，
    // DobbySymbolResolver 内部的 dlopen/名字查找会拿到垃圾指针（实测 SIGSEGV，
    // fault addr 高位即字符串 ASCII）。改用 xdl（dl_iterate_phdr 支持内嵌库）解析。
    void* addr = getSymAddress(lib, symName, false);
    if (!addr) return nullptr;
    uintptr_t base = getLibraryAddress(lib);
    if (!base) return nullptr;
    uintptr_t a = (uintptr_t)addr;
    // 映射范围粗校验：库基址之后 256MB 内才算有效，挡住垃圾指针进 DobbyHook
    if (a < base || a >= base + 256u * 1024u * 1024u) {
        LOGW("[Cwjh] symbol %s resolved out of range %p (base %lx)", symName, addr,
             (unsigned long)base);
        return nullptr;
    }
    return addr;
}

static void TryHookEvalString() {
    const char* lib = GameCocos::TARGET_LIB;
    if (!isLibraryLoaded(lib)) return;

    if (!g_seHooked) {
        void* sym = Cwjh_ResolveLibSym(lib, GameCocos::Symbols::SE_EVAL_STRING);
        if (sym &&
            DobbyHook(sym, (dobby_dummy_func_t)hooked_seEval,
                      (dobby_dummy_func_t*)&orig_seEval) == 0) {
            g_seHooked = true;
            g_evalHooked = true;
            LOGI("[Cwjh] hooked se::ScriptEngine::evalString (combat splice)");
        } else if (!sym) {
            LOGW("[Cwjh] se::evalString symbol missing");
        }
    }

    if (!g_jniHooked) {
        void* jni = Cwjh_ResolveLibSym(lib, GameCocos::Symbols::EVAL_STRING);
        if (jni &&
            DobbyHook(jni, (dobby_dummy_func_t)hooked_jniEval,
                      (dobby_dummy_func_t*)&orig_jniEval) == 0) {
            g_jniHooked = true;
            LOGI("[Cwjh] hooked JNI evalString (passthrough)");
        }
    }
}

bool Cwjh_TryInit(JavaVM* jvm) {
    g_cwjhJvm = jvm;
    const char* lib = GameCocos::TARGET_LIB;
    if (!isLibraryLoaded(lib)) {
        return false;
    }
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (detach) g_cwjhJvm->DetachCurrentThread();
    }
    TryHookEvalString();
    if (!g_seHooked) return false;
    g_ready = true;
    SyncJavaRets();
    LOGI("[Cwjh] init ok se=%d jni=%d splice=%d seCalls=%d", g_seHooked ? 1 : 0,
         g_jniHooked ? 1 : 0, g_spliceHits.load(), g_seCalls.load());
    return true;
}

/* ---------------- 反篡改 kill 原语中和 ----------------
 * 静态实锤（antitamper_thread_evidence.txt，2.1；2.2 复扫 20260909）：
 * libcocos2djs.so 可执行段内自定义 raw-svc 包装器仅两处（同一模块，同构前导）：
 *   - 0x16ebdf8：kill(0, SIGKILL) 封装（x8=129），7 条自杀路径（2.1）/6 个 BL 调用者（2.2）
 *   - 0x16ebe24：openat(AT_FDCWD,…) 封装（x8=56，文件存在性检查用，无自杀用途，不动）
 * 另 .rodata 0x19ab91c/0x19ab920 有两条 d4000001 字节常量（非执行段，不处理）。
 *
 * 【2.2 新投递路径实锤（20260909 模拟器 SIGKILL 复盘）】
 *   0x16d5a08：sleep(x0) → getpid() → kill(pid, 9)，直调 libc kill@plt——
 *   绕过被中和的自定义包装器。watchdog Adler32 检出 inline hook（Dobby）后
 *   派线程 sleep(6) 进它；实测 hook→死 6.03s，与已知「sleep(6) 后 kill」吻合。
 *   处理：入口两指令补 mov x0,xzr; ret（原函数末尾就返回 0，语义等价空转），
 *   前导版本守卫 {str x30,[sp,#-0x10]! ; bl sleep@plt}，失配即跳过。
 * 处理（包装器）：early 阶段把函数头改为 mov w0,#0; ret —— 所有检测即便命中也无法自杀。
 * 原始前两条指令做版本守卫：0x2a1f03e0 (mov w0,wzr) / 0xd10203ff (sub sp,sp,#0x80)。
 */
#include <sys/mman.h>

static bool g_killNeutered = false;

static bool Cwjh_PatchHead(uintptr_t base, uintptr_t off, const uint32_t expect[2],
                           const uint32_t patch[2], const char* tag) {
    void* fn = (void*)(base + off);
    uint32_t cur[2] = {0, 0};
    memcpy(cur, fn, sizeof(cur));
    if (cur[0] != expect[0] || cur[1] != expect[1]) {
        LOGW("[Cwjh] %s prologue mismatch %08x %08x, skip", tag, cur[0], cur[1]);
        return false;
    }
    uintptr_t pageStart = base + off;
    pageStart &= ~(uintptr_t)(getpagesize() - 1);
    size_t pageLen = getpagesize() * 2;
    if (mprotect((void*)pageStart, pageLen, PROT_READ | PROT_WRITE | PROT_EXEC) != 0) {
        LOGW("[Cwjh] %s mprotect rwx failed errno=%d", tag, errno);
        return false;
    }
    memcpy(fn, patch, 2 * sizeof(uint32_t));
    __builtin___clear_cache((char*)fn, (char*)fn + 2 * sizeof(uint32_t));
    mprotect((void*)pageStart, pageLen, PROT_READ | PROT_EXEC);
    LOGI("[Cwjh] %s neutralized @%lx", tag, (unsigned long)(base + off));
    return true;
}

static void Cwjh_NeutralizeKillPrimitive() {
    if (g_killNeutered) return;
    uintptr_t base = getLibraryAddress(GameCocos::TARGET_LIB);
    if (!base) return;
    // ① kill(0,9) 包装器：mov w0,#0 → 调用即空转
    const uint32_t kKillExp[2] = {0x2a1f03e0u, 0xd10203ffu};
    const uint32_t kKillPatch[2] = {0x52800000u /* mov w0,#0 */, 0xd65f03c0u /* ret */};
    // ② 2.2 炸弹函数 sleep→getpid→kill(libc)：入口直接空转返回 0
    const uint32_t kBombExp[2] = {0xf81f0ffeu /* str x30,[sp,#-0x10]! */,
                                  0x97c06eb9u /* bl sleep@plt */};
    const uint32_t kBombPatch[2] = {0xaa1f03e0u /* mov x0,xzr */, 0xd65f03c0u /* ret */};
    Cwjh_PatchHead(base, 0x16d5a08, kBombExp, kBombPatch, "bomb fn (libc kill)");
    if (Cwjh_PatchHead(base, 0x16ebdf8, kKillExp, kKillPatch, "kill wrapper")) {
        g_killNeutered = true;
    }
}

static void* EarlyHookThread(void* arg) {
    JavaVM* jvm = (JavaVM*)arg;
    g_cwjhJvm = jvm;
    LOGI("[Cwjh] early hook thread start");
    for (int i = 0; i < 2000; i++) { // ~40s @ 20ms
        if (isLibraryLoaded(GameCocos::TARGET_LIB)) {
            Cwjh_NeutralizeKillPrimitive();  // 先拔牙，再挂钩
            TryHookEvalString();
            if (g_seHooked) {
                g_ready = true;
                LOGI("[Cwjh] early se hook ok at iter=%d splice=%d", i, g_spliceHits.load());
                bool detach = false;
                JNIEnv* env = GetEnv(&detach);
                if (env) {
                    CacheJava(env);
                    SyncJavaRets();
                    if (detach) g_cwjhJvm->DetachCurrentThread();
                }
                return nullptr;
            }
        }
        usleep(20 * 1000);
    }
    LOGW("[Cwjh] early hook timeout");
    return nullptr;
}

void Cwjh_StartEarlyHookThread(JavaVM* jvm) {
    static std::atomic_bool started{false};
    bool expected = false;
    if (!started.compare_exchange_strong(expected, true)) return;
    pthread_t th;
    if (pthread_create(&th, nullptr, EarlyHookThread, jvm) == 0) {
        pthread_detach(th);
    } else {
        started.store(false);
        LOGW("[Cwjh] early hook pthread_create failed");
    }
}

bool Cwjh_IsReady() { return g_ready; }

void Cwjh_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum >= 0 && featNum < CWJH_FEAT_COUNT) g_feat[featNum] = enabled;
    SyncJavaRets();
    LOGI("[Cwjh] feature #%d=%s splice=%d", featNum, enabled ? "ON" : "OFF",
         g_spliceHits.load());
}

void Cwjh_ForceClear() {
    for (int i = 0; i < CWJH_FEAT_COUNT; i++) g_feat[i] = false;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (g_hooksCls && g_hooksClear) {
            env->CallStaticVoidMethod(g_hooksCls, g_hooksClear);
        }
        if (detach) g_cwjhJvm->DetachCurrentThread();
    }
}

void Cwjh_Tick(const bool* features) {
    if (!features) return;
    bool dirty = false;
    for (int i = 0; i < CWJH_FEAT_COUNT; i++) {
        if (features[i] != g_feat[i]) {
            g_feat[i] = features[i];
            dirty = true;
        }
    }
    if (!g_ready) return;
    if (!g_evalHooked) TryHookEvalString();
    if (dirty) {
        SyncJavaRets();
        LOGI("[Cwjh] feature sync dirty splice=%d", g_spliceHits.load());
    }
}
