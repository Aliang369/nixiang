/* ================================================================
 *  异世界勇者 MOD — se::ScriptEngine::evalString splice（JS 层补丁）
 *
 *  目标：com.mudamuda.idlewow 8.1.1 / Cocos Creator 2.4.2 / libcocos2djs.so
 *  主 bundle：assets/main/index.jsc（XXTEA+gzip）→ 明文 2,993,782 B，
 *             整包一次性进 evalString；明文末尾追加补丁块 = 全部模块执行完之后运行。
 *
 *  设计要点（与 skill/re-implement 分层一致）：
 *    - 全部改动落在 JS 层，native 只做一次 evalString hook；
 *    - 二档战斗类统一收在 BattleHero.AddHp 单点：属性面板/上报字段
 *      （Patk/Pdef/Php 由 GetMaxRate* 直接产出）零改写 → 不需要面板伪装；
 *    - 伤害上报由 StatDam 按放大系数还原成"原版数值"（S.sc），
 *      chapter/result 的 Dam 与未开挂时同量级；
 *    - 所有增益限定 grp===1（我方），避免怪物同步获益。
 *
 *  铁律：orig 必调（关闭态直接透传）/ 重入旗标 / 命中身份校验 / 只改明文不碰字节码。
 * ================================================================ */

#include "YsjYzMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <string>

#include <pthread.h>
#include <unistd.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/SecStrings.hpp"
#include "Includes/Utils.hpp"

#define YSJYZ_LOG "[YsjYz]"

// 目标库/符号：modern 注册表路线不依赖 GameConfig.h
static const char* kTargetLib() { return sec_lib_cocosjs(); }
static const char* kSymSeEvalString() {
    return "_ZN2se12ScriptEngine10evalStringEPKclPNS_5ValueES2_";
}
// 主 bundle 身份特征（Cocos 2.x bundle 头 + 本游戏专有模块名 + 幂等标记）
static const char* kBundleMark = "window.__require";
static const char* kGameMark = "\"BattleHero\"";
static const char* kIdemMark = "starry_ysjyz_v1";

// ============================================================
// ① 开关表（featNum 对齐 YsjYzEntry.cpp featureList）
// ============================================================
static std::atomic<bool> g_feat[YSJYZ_FEAT_COUNT];
static std::atomic<int> g_val[YSJYZ_FEAT_COUNT];
static std::atomic<bool> g_hooked{false};
static std::atomic<bool> g_ready{false};
static std::atomic<int> g_spliceHits{0};
static thread_local bool t_inSplice = false;

// ============================================================
// ② JS 补丁块（追加到主 bundle 明文末尾）
// ============================================================
static const char* kPatchJs = R"YSJYZJS(
/*starry_ysjyz_v1*/
;(function () {
  if (window.__ysjyz) { return; }
  window.__ysjyz = 1;
  var ACT = "org/cocos2dx/javascript/AppActivity";
  var S = window.__ysjyz_s = {
    adfree: 0, skill: 0, inv: 0, dmg: 1, gspd: 1, sc: 1, n: 0,
    ax: 0, clip: 0
  };
  function REF() { return (typeof jsb !== "undefined") && jsb.reflection && jsb.reflection.callStaticMethod; }
  function RET(n) {
    try { return !!jsb.reflection.callStaticMethod(ACT, "get_ret", "(I)Z", n); } catch (e) { return false; }
  }
  function VAL(n) {
    try {
      var v = jsb.reflection.callStaticMethod(ACT, "get_val", "(I)I", n);
      return (typeof v === "number") ? (v | 0) : (parseInt(v, 10) || 0);
    } catch (e) { return 0; }
  }
  function clamp1(v) { return v > 1 ? v : 1; }
  // 上界夹取：滑块范围由菜单声明（伤害倍率 1-100 / 游戏倍速 1-5），
  // 这里再兜一层 —— 防止旧偏好/异常输入把倍率顶到病态值（v7 起 gspd 默认 1）。
  function clampv(v, hi) { v = clamp1(v); return v > hi ? hi : v; }
  function refresh() {
    if (!REF()) { return; }
    S.adfree = RET(1) ? 1 : 0;
    S.skill  = RET(2) ? 1 : 0;
    S.inv    = RET(3) ? 1 : 0;
    S.dmg    = clampv(VAL(4), 100);
    S.gspd   = clampv(VAL(5), 5);
  }
  function wrap(o, name, maker) {
    try {
      if (!o) { return 0; }
      var f = o[name];
      if (typeof f !== "function" || f.__ysjz) { return 0; }
      var w = maker(f);
      w.__ysjz = 1;
      o[name] = w;
      return 1;
    } catch (e) { return 0; }
  }
  // ---- 上报兜底限幅 -------------------------------------------------------
  // 本包已无任何"改面板"的功能（倍血防/必闪避/必中暴击已按需求删除），
  // 所有战斗增益都落在 AddHp 结算层，因此 Hp/Atk/Def/Free/Php/Patk/Pdef 天然与官包一致，
  // 不需要 CheckResult / Patk / Pdef 伪装。
  // 唯一还需要照看的是 Dam：整章累计（y32645 AddMonsterHurtDamage 跨波次累加），
  // 而 result.Mhp 只是"当前怪"的血上限（y32721），Dam/Mhp ≈ 本章怪数，可到几十倍 ——
  // 所以这里只做 Mhp×100 的**宽松兜底**，拦明显病态的数值，绝不压低正常战报。
  // Dam 的保真主要靠 StatDam 按放大系数 1:1 折回（见 applyBase）。
  function clampResult(R) {
    try {
      if (!R || typeof R !== "object") { return null; }
      if (!(R.Mhp > 0) || !(R.Dam > R.Mhp * 100)) { return null; }
      var cap = R.Dam;
      R.Dam = 0 | (R.Mhp * 100);
      return function () { try { R.Dam = cap; } catch (e) {} };
    } catch (e) { return null; }
  }
  // 混入模块成员（BattleHeroBase / BattleHeroSkCost）——按实例 applyBase
  function applyBase(o) {
    var n = 0;
    if (!o) { return 0; }
    n += wrap(o, "StatDam", function (orig) {
      return function (dmg, mid, hp, typ) {
        // 伤害上报还原：放大过的这一击按系数折回原版量级（对外与未开挂一致）
        if (S.sc > 1 && typeof dmg === "number") { dmg = dmg / S.sc; }
        return orig.call(this, dmg, mid, hp, typ);
      };
    });
    n += wrap(o, "GetSkillCDDuration", function (orig) {
      return function (st, sid) {
        if (S.skill && this && this.grp === 1) { return 0; }
        return orig.call(this, st, sid);
      };
    });
    n += wrap(o, "UpdateSkillCD", function (orig) {
      return function (dt) {
        if (S.skill && this && this.grp === 1) {
          try {
            for (var k in this.skState) {
              var t = this.skState[k];
              if (t && typeof t === "object") {
                t.Cd = 0;
                if (t.MaxCharged > t.Charged) { t.Charged = t.MaxCharged; }
              }
            }
          } catch (e) {}
          return;
        }
        return orig.call(this, dt);
      };
    });
    n += wrap(o, "GetSkillCost", function (orig) {
      return function (cfg, all) {
        if (S.skill && this && this.grp === 1) { return 0; }
        return orig.call(this, cfg, all);
      };
    });
    return n;
  }
  // BattleHero 原型：战斗单位通用入口
  function applyProto(P) {
    var n = 0;
    if (!P) { return 0; }
    n += wrap(P, "AddHp", function (orig) {
      return function (e, t, i, c, o, nn, s, r, l, a) {
        try {
          var d = e || 0;
          if (t && this.hp) { d += this.hp.max * t; }
          if (d < 0) {
            if (this.grp === 1) {
              if (S.inv) { return; }                       // 无敌：我方免伤，不结算
            } else if (S.dmg > 1) {                        // 对敌放大（v7 起不再含吸血）
              var pv = S.sc;
              var amp = S.dmg;
              var hp0 = this.hp ? this.hp.cur : 0;
              S.sc = amp;
              var res;
              try {
                res = orig.call(this, d * amp, 0, i, c, o, nn, s, r, l, a);
              } finally { S.sc = pv; }
              return res;
            }
          }
        } catch (x) {}
        return orig.apply(this, arguments);
      };
    });
    // ---- 修复「人物等级≥199 后打怪恒 1 滴血」（y29158 减护甲 + y29807 钳制）------
    // 病因链：官方 Damage 用 `d = GetAtk() - E`（E=目标护甲，y29158）先做减法，
    //         再在 y29807 用 `d < 1 && (d = 1)` 把 ≤0 的战前伤害钳成 1；
    //         本补丁的 ×amp 只把这个"1"放大 ⇒ 滑块拉到 N 就正好打 N（1×N 的本质）。
    // 修法：我方（grp===1）出刀期间，让 grp===2 目标的护甲上限 = 本次攻击的 80%
    //       ⇒ 反推 d ≥ 20%×攻击 > 0，y29807 永不触发，滑块乘的是真伤害。
    //       若目标护甲本来就低于 80% 攻击（正常高攻玩家），此上限不生效、完全走原公式。
    //       仅在滑块 >1（功能开启）时介入，滑块=1 保持官包原味。
    n += wrap(P, "Damage", function (orig) {
      return function (t, i, c, o, nn, s, r) {
        var pv = S.ax, av = 0;
        try {
          if (S.dmg > 1 && this && this.grp === 1 && typeof this.GetAtk === "function") {
            av = this.GetAtk(o);
            if (!(av > 0)) { av = 0; }
          }
        } catch (e) { av = 0; }
        S.ax = av;
        try { return orig.apply(this, arguments); } finally { S.ax = pv; }
      };
    });
    n += wrap(P, "GetDef", function (orig) {
      return function () {
        var v = orig.call(this);
        try {
          if (S.ax > 0 && this && this.grp === 2 && typeof v === "number" && v > S.ax * 0.8) {
            if (S.clip < 8) {
              S.clip++;
              cc.log("[ysjyz] armor-clip atk=" + S.ax + " def=" + v + " -> " + (S.ax * 0.8));
            }
            return S.ax * 0.8;
          }
        } catch (e) {}
        return v;
      };
    });
    n += wrap(P, "Extneds", function (orig) {
      return function () {
        var r = orig.apply(this, arguments);
        try { applyBase(this); } catch (e) {}
        return r;
      };
    });
    return n;
  }
  function findHero() {
    var c = null;
    try { c = cc.js.getClassByName("BattleHero"); } catch (e) {}
    if (!c) { try { c = cc.js._getClassById("930b8YtWDFL/7zRRe+NE3qD"); } catch (e) {} }
    if (!c) { try { c = window.__require("BattleHero"); } catch (e) {} }
    return c;
  }
  function applyAll() {
    var n = 0;
    try {
      if (cc.Service && cc.Service.Battle) {
        n += wrap(cc.Service.Battle, "Update", function (orig) {
          return function (dt, node) {
            if (S.gspd > 1 && typeof dt === "number" && dt > 0) { dt = dt * S.gspd; }
            return orig.call(this, dt, node);
          };
        });
      }
      // chapter/result 出口：Dam 兜底限幅
      if (cc.Service && cc.Service.Network) {
        n += wrap(cc.Service.Network, "Send", function (orig) {
          return function (api, data) {
            var undo = (api === "chapter/result") ? clampResult(data) : null;
            var r = orig.apply(this, arguments);
            if (undo) { try { undo(); } catch (e) {} }
            return r;
          };
        });
      }
      if (cc.UIHelper) {
        n += wrap(cc.UIHelper, "ShowAD", function (orig) {
          return function (cb) {
            if (S.adfree) {
              try { if (cb) { cb(); } } catch (e) { cc.log("[ysjyz] adfree cb err " + e); }
              return;
            }
            return orig.apply(this, arguments);
          };
        });
      }
      var H = findHero();
      if (H && H.prototype) {
        n += applyProto(H.prototype);
      }
      // 兜底：混入对象本体也补一次（此后新建实例直接拷贝到已补丁版本）
      try {
        if (typeof window.__require === "function") {
          applyBase(window.__require("BattleHeroBase"));
          applyBase(window.__require("BattleHeroSkCost"));
          applyBase(window.__require("BattleHeroBuffs"));
        }
      } catch (e) {}
    } catch (e) {}
    S.n = n;
    return n;
  }
  window.__ysjyz_apply = function () { try { refresh(); return applyAll(); } catch (e) { return -1; } };
  refresh();
  applyAll();
  // 菜单开关轮询（滑块/开关变更 0.5s 内进 JS，不依赖 native Tick 间隔）
  function beat() { try { refresh(); } catch (e) {} }
  var scheduled = 0;
  try {
    var sch = cc.director.getScheduler();
    sch.schedule(beat, cc.director, 0.5, cc.macro.REPEAT_FOREVER, 0, false);
    scheduled = 1;
  } catch (e) {}
  if (!scheduled) { try { setInterval(beat, 500); } catch (e) {} }
  try { cc.log("[ysjyz] patch applied n=" + S.n); } catch (e) {}
})();
)YSJYZJS";

// ============================================================
// ③ evalString hook
// ============================================================
typedef bool (*SeEvalString_t)(void*, const char*, long, void*, const char*);
static SeEvalString_t g_origSeEval = nullptr;

static bool MemHas(const char* buf, size_t len, const char* needle) {
    size_t nl = strlen(needle);
    if (!buf || nl == 0 || len < nl) return false;
    return memmem(buf, len, needle, nl) != nullptr;
}

static bool hooked_seEval(void* thiz, const char* script, long length, void* ret,
                          const char* fileName) {
    if (!g_origSeEval) return false;
    if (!script || t_inSplice) return g_origSeEval(thiz, script, length, ret, fileName);

    long n = length;
    if (n < 0) n = (long)strlen(script);
    if (n < 200000 || n > 64L * 1024 * 1024) {
        return g_origSeEval(thiz, script, length, ret, fileName);
    }
    // 命中主 bundle：Cocos 2.x 头 + 本游戏专有模块名 + 幂等标记
    if (!MemHas(script, (size_t)n, kBundleMark) || !MemHas(script, (size_t)n, kGameMark) ||
        MemHas(script, (size_t)n, kIdemMark)) {
        return g_origSeEval(thiz, script, length, ret, fileName);
    }

    std::string patched;
    patched.reserve((size_t)n + 20480);
    patched.assign(script, (size_t)n);
    patched += "\n";
    patched += kPatchJs;

    int hits = ++g_spliceHits;
    LOGI("%s bundle splice #%d len %ld->%zu file=%s", YSJYZ_LOG, hits, n, patched.size(),
         fileName ? fileName : "(null)");

    t_inSplice = true;
    bool ok = g_origSeEval(thiz, patched.c_str(), (long)patched.size(), ret, fileName);
    t_inSplice = false;
    return ok;
}

static void TryHookEvalString() {
    if (g_hooked.load()) return;
    const char* lib = kTargetLib();
    if (!isLibraryLoaded(lib)) return;
    void* sym = DobbySymbolResolver(lib, kSymSeEvalString());
    if (!sym) {
        LOGW("%s se::evalString symbol not found in %s", YSJYZ_LOG, lib);
        return;
    }
    if (DobbyHook(sym, (dobby_dummy_func_t)hooked_seEval,
                  (dobby_dummy_func_t*)&g_origSeEval) == 0) {
        g_hooked.store(true);
        LOGI("%s hooked se::evalString @%p", YSJYZ_LOG, sym);
    } else {
        LOGW("%s DobbyHook failed", YSJYZ_LOG);
    }
}

// ============================================================
// ④ Java 桥（YsjYzHooks；JS 经 AppActivity.get_ret/get_val 读回本表）
// ============================================================
static JavaVM* g_jvm = nullptr;
static jclass g_hooksCls = nullptr;
static jmethodID g_setRet = nullptr;
static jmethodID g_setVal = nullptr;
static jmethodID g_clearAll = nullptr;

static JNIEnv* GetEnv(bool* needDetach) {
    *needDetach = false;
    if (!g_jvm) return nullptr;
    JNIEnv* env = nullptr;
    int st = g_jvm->GetEnv((void**)&env, JNI_VERSION_1_6);
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
    jclass local = LoadAppClass(env, "com.starry.magic.cocos.YsjYzHooks");
    if (!local) local = env->FindClass("com/starry/magic/cocos/YsjYzHooks");
    if (local) {
        g_hooksCls = (jclass)env->NewGlobalRef(local);
        g_setRet = env->GetStaticMethodID(g_hooksCls, "setRet", "(IZ)V");
        g_setVal = env->GetStaticMethodID(g_hooksCls, "setVal", "(II)V");
        g_clearAll = env->GetStaticMethodID(g_hooksCls, "clearAll", "()V");
        env->DeleteLocalRef(local);
    } else {
        env->ExceptionClear();
    }
}

static void SyncJava() {
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    CacheJava(env);
    if (g_hooksCls && g_setRet && g_setVal) {
        for (int i = 0; i < YSJYZ_FEAT_COUNT; i++) {
            env->CallStaticVoidMethod(g_hooksCls, g_setRet, i,
                                      g_feat[i].load() ? JNI_TRUE : JNI_FALSE);
            env->CallStaticVoidMethod(g_hooksCls, g_setVal, i, g_val[i].load());
        }
    }
    if (detach) g_jvm->DetachCurrentThread();
}

// ============================================================
// ⑤ 生命周期（MainModern 注册表回调）
// ============================================================
bool YsjYz_TryInit(JavaVM* jvm) {
    g_jvm = jvm;
    if (!isLibraryLoaded(kTargetLib())) return false;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (detach) g_jvm->DetachCurrentThread();
    }
    TryHookEvalString();
    if (!g_hooked.load()) return false;
    g_ready.store(true);
    SyncJava();
    return true;
}

static void* EarlyHookThread(void* arg) {
    g_jvm = (JavaVM*)arg;
    for (int i = 0; i < 3000; i++) {
        if (isLibraryLoaded(kTargetLib())) {
            TryHookEvalString();
            if (g_hooked.load()) {
                g_ready.store(true);
                SyncJava();
                LOGI("%s early hook ok iter=%d", YSJYZ_LOG, i);
                return nullptr;
            }
        }
        usleep(20 * 1000);
    }
    LOGW("%s early hook timeout", YSJYZ_LOG);
    return nullptr;
}

void YsjYz_StartEarlyHookThread(JavaVM* jvm) {
    static std::atomic_bool started{false};
    bool expected = false;
    if (!started.compare_exchange_strong(expected, true)) return;
    pthread_t th;
    if (pthread_create(&th, nullptr, EarlyHookThread, jvm) == 0) {
        pthread_detach(th);
    } else {
        started.store(false);
    }
}

bool YsjYz_IsReady() { return g_ready.load(); }

void YsjYz_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum >= 0 && featNum < YSJYZ_FEAT_COUNT) g_feat[featNum].store(enabled);
    SyncJava();
    LOGI("%s feature #%d=%s", YSJYZ_LOG, featNum, enabled ? "ON" : "OFF");
}

void YsjYz_OnValueChanged(int featNum, int value) {
    if (featNum >= 0 && featNum < YSJYZ_FEAT_COUNT) g_val[featNum].store(value);
    SyncJava();
    LOGI("%s value #%d=%d", YSJYZ_LOG, featNum, value);
}

void YsjYz_Tick(const bool* features, const int* values) {
    if (features) {
        for (int i = 0; i < YSJYZ_FEAT_COUNT; i++) g_feat[i].store(features[i]);
    }
    if (values) {
        for (int i = 0; i < YSJYZ_FEAT_COUNT; i++) g_val[i].store(values[i]);
    }
    if (!g_hooked.load()) TryHookEvalString();
    SyncJava();
}

void YsjYz_ForceClear() {
    for (int i = 0; i < YSJYZ_FEAT_COUNT; i++) {
        g_feat[i].store(false);
        g_val[i].store(0);
    }
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (g_hooksCls && g_clearAll) env->CallStaticVoidMethod(g_hooksCls, g_clearAll);
        if (detach) g_jvm->DetachCurrentThread();
    }
}
