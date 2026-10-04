/* Auto-generated from 10_docs/勇士之心/斌哥注入脚本_提取_999999_20260915.js. */
#pragma once

static const char* kYsxzPatchJs = R"YSXZJS(

/*starry_ysxz_v1*/
;(function () {
  var H = "com.starry.magic.cocos.YsxzHooks";
  function R(n) {
    try {
      if (typeof jsb === "undefined" || !jsb.reflection) return false;
      return !!jsb.reflection.callStaticMethod(H, "getRet", "(I)Z", n);
    } catch (e) { return false; }
  }
  function V(n) {
    try {
      if (typeof jsb === "undefined" || !jsb.reflection) return 1;
      var v = jsb.reflection.callStaticMethod(H, "getVal", "(I)I", n) | 0;
      return v > 0 ? v : 1;
    } catch (e) { return 1; }
  }
  function J(s) {
    try {
      if (typeof jsb === "undefined" || !jsb.reflection) return false;
      jsb.reflection.callStaticMethod(H, "logStatus", "(Ljava/lang/String;)V", s);
      return true;
    } catch (e) { return false; }
  }
  var M = {};
  /* 序号 = 菜单里的位置（0=分类标题），必须与 Games/YsxzEntry.cpp::kYsxz_Features 严格一致。
     2026-10-04 移植：把 1.0.4 的菜单序号搬到 1.0.3 包 —— 1 号位新增伤害倍率滑块，
     秒杀/无敌/无限技能/自动战斗 顺延为 2/3/4/6，移动速度滑块在 5 号位、上限 1..10。 */
  Object.defineProperty(M, "feature1", { get: function () { return R(2); } });   /* 秒杀 */
  Object.defineProperty(M, "feature2", { get: function () { return R(3); } });   /* 无敌 */
  Object.defineProperty(M, "feature3", { get: function () { return R(4); } });   /* 无限技能 */
  Object.defineProperty(M, "feature4", { get: function () { return R(0); } });   /* 占位：移动速度是滑块，仅状态日志用 */
  Object.defineProperty(M, "feature5", { get: function () { return R(6); } });   /* 自动战斗 */
  Object.defineProperty(M, "speed", { get: function () { return V(5); } });      /* 移动速度 1..10 */
  Object.defineProperty(M, "damage", { get: function () { return V(1); } });     /* 伤害倍率 1..50 */
  globalThis.__starry_nlog = J;
  globalThis.MOD = M;
})();

/* ================== MOD 自愈安装器 (勇士之心) ==================
 * 挂载点 = Cocos 自己生成的 window.__require(永不混淆),由它拿到模块注册表。
 * 全部混淆名在运行时发现,不写死任何一个:
 *   1) 用【明文锚点】正则从模块源码里把混淆名捕获出来
 *      —— 字符串字面量("isPlayer")和明文字段名(lockMinHP / maxCdTime /
 *         maxApDefRate / worldTimeScale ...)混淆器不会动
 *   2) 用 Function.prototype.toString() 按指纹认出目标函数,再包装
 *
 * ★ 本 bundle 的入口列表包含【全部模块】,即所有模块在 bundle 加载瞬间就执行完了,
 *   那时菜单还没注入 MOD。所以:
 *     发现 + 挂钩 一律无条件执行;
 *     if (MOD.featureN) 一律放在钩子【内部】(调用时才读),这样菜单随时开关都生效。
 *
 * 日志: 统一 [MODMOD] 前缀, 走 cc.log(原生)→console.log(WebView) 双通道,
 *       同时存一份到 window.MOD_LOG。抓法: adb logcat | grep MODMOD
 * ============================================================== */
function MOD_boot(REG) {
    var N = {};        /* 发现到的混淆名 */
    var CTX = null;    /* 战斗上下文(从系统 update 里捎带缓存) */

    function log(s) {
        var msg = "[MODMOD] " + s;
        try { if (!window.MOD_LOG) window.MOD_LOG = []; window.MOD_LOG.push(msg); } catch (e) {}
        try { if (window.__starry_nlog) window.__starry_nlog(msg); } catch (e) {}
        try { if (typeof cc !== "undefined" && cc && typeof cc.log === "function") { cc.log(msg); return; } } catch (e) {}
        try { if (typeof console !== "undefined" && console && console.log) console.log(msg); } catch (e) {}
    }
    function srcOf(f) { try { return Function.prototype.toString.call(f); } catch (e) { return ""; } }

    /* ---------- 1. 明文锚点 → 捕获混淆名 ---------- */
    function harvestA(s) {
        var m;
        if (!N.hx) {
            m = /case\s*["']isPlayer["']\s*:\s*return\s+\w+\.(\w+)\.(\w+)\s*[;,}]/.exec(s);
            if (m) { N.hx = m[1]; N.isPlayer = m[2]; }
        }
        if (!N.hD) {
            m = /\w+\.(\w+)\.(\w+)\s*===\s*\w+\s*!==\s*\w+\.isPlayerCamp/.exec(s);
            if (m) { N.hD = m[1]; N.camp = m[2]; }
        }
        if (!N.ctxKey) {
            m = /this\.context\.(\w+)\.(\w+)\[0\]\.(\w+)/.exec(s);
            if (m) { N.ctxKey = m[1]; N.teams = m[2]; N.teamCamp = m[3]; }
        }
        if (!N.hp) {
            m = /(\w+)\.(\w+)\s*=\s*Math\.max\(\1\.\2\s*-\s*\w+,\s*\1\.lockMinHP\)/.exec(s);
            if (m) { N.hp = m[2]; }
        }
    }
    /* 需要先有 ctxKey, 故第二遍再扫: 实体 → 世界 的属性名 */
    function harvestB(s) {
        if (N.world || !N.ctxKey) return;
        if (s.indexOf("lockMinHP") < 0) return;           /* 只认实体/属性模块 */
        /* 必须遍历所有匹配: 同一模块里可能先出现 this.context.<ctx>(系统的写法) */
        var re = new RegExp("this\\.([A-Za-z_$][\\w$]*)\\." + N.ctxKey + "\\.", "g"), m;
        while ((m = re.exec(s)) !== null) {
            if (m[1] !== "context") { N.world = m[1]; return; }
        }
    }

    /* ---------- 2. 敌我判定(全部基于发现到的名字, 带降级) ---------- */
    function ctxOf(ent) {
        var w = N.world && ent ? ent[N.world] : null;
        var c = w ? w[N.ctxKey] : null;
        return c || CTX;
    }
    function myCamp(ent) {
        var c = ctxOf(ent);
        var t0 = c && N.teams && c[N.teams] ? c[N.teams][0] : null;
        var v = t0 && N.teamCamp ? t0[N.teamCamp] : null;
        if (typeof v === "number" && v >= 0) return v;
        return 1;                 /* 降级: 本作阵营 1 = 我方(gF() 取 camp!=1 当敌人) */
    }
    function isMine(ent) {
        if (!ent || !N.hD || !N.camp) return false;
        var h = ent[N.hD];
        if (!h) return false;
        var c = h[N.camp];
        if (typeof c !== "number" || c < 0) return false;
        return c === myCamp(ent);
    }
    function isMyHero(ent) {
        if (!isMine(ent) || !N.hx) return false;
        var x = ent[N.hx];
        return !!(x && x[N.isPlayer]);
    }
    /* 时间缩放组件: 靠明文字段 hTimeScale 认, 认到后缓存槽名 */
    function tsComp(ent) {
        if (N.hy) return ent[N.hy];
        for (var k in ent) {
            var v;
            try { v = ent[k]; } catch (e) { continue; }
            if (v && typeof v === "object" && typeof v.hTimeScale === "number") {
                N.hy = k; log("timeScale comp = ent." + k); return v;
            }
        }
        return null;
    }

    /* ---------- 3. 按指纹找函数 ---------- */
    function findFn(obj, test) {
        if (!obj) return null;
        for (var k in obj) {
            var v;
            try { v = obj[k]; } catch (e) { continue; }
            if (typeof v !== "function") continue;
            if (test(srcOf(v))) return k;
        }
        return null;
    }

    /* ---------- 4. 战斗静态逻辑类: 伤害公式 / 技能门 / 技能消耗 / 死亡 ---------- */
    function tryLogic(K) {
        if (N.logicDone) return;
        var fv = findFn(K, function (s) {
            return s.indexOf("maxApDefRate") >= 0 && s.indexOf("baseCritDamage") >= 0;
        });
        if (!fv) return;
        N.logicDone = 1;
        N.FV = fv;
        log("battle-logic class hit: damage formula = ." + fv);

        /* ---- 挂钩(无条件安装, 开关在钩子内部读) ---- */
        var oFV = K[fv];
        K[fv] = function (ctx, hit, ratio, canCrit) {
            var d = oFV.apply(this, arguments);
            try {
                if (hit && d > 0 && isMine(hit.atkEntity) && !isMine(hit.target)) {
                // 咽喉 = 唯一伤害数值公式(指纹: 明文 maxApDefRate + baseCritDamage)。
                // 在公式里放大而不是在扣血口放大, 飘字/结算数值才会同步。
                // 命中上下文的 atkEntity / target 本身是明文字段, 不需要发现。
                // 伤害倍率(滑块 1..50, 1=原倍率) 与 秒杀(x100) 共用这一个咽喉。
                var _f1 = false, _dm = 1;
                try { _f1 = !!MOD.feature1; _dm = MOD.damage; } catch (readErr) {}
                if (_dm > 1) d = d * _dm;            /* 1 伤害倍率滑块 1..50, 1=原倍率 */
                if (_f1) d = Math.ceil(d * 100);     /* 2 秒杀: 玩家 x100 伤害 */
                if (d > 1e9) d = 1e9;                /* 上限, 防数值溢出 */
                }
            } catch (e) {}
            return d;
        };
        log("feature1 hooked");

        /* 技能消耗函数(指纹: 明文 maxCdTime + "充能 -= 1" 结构) */
        var fg = findFn(K, function (s) {
            return s.indexOf("maxCdTime") >= 0 && /\w+\.\w+\s*>\s*0\s*\)\s*\w+\.\w+\s*-=\s*1/.test(s);
        });
        if (fg) {
            N.Fg = fg;
            var fgs = srcOf(K[fg]), m;
            m = /\w+\.(\w+)\s*>\s*0\s*\)\s*\w+\.(\w+)\s*-=\s*1/.exec(fgs);
            if (m) { N.CZ = m[1]; N.CY = m[2]; }
            m = /\w+\.(\w+)\s*=\s*\w+\.maxCdTime/.exec(fgs);
            if (m) { N.cd = m[1]; }
            m = /if\s*\(\s*\w+\.(\w+)\s*\)\s*\{\s*\w+\.\w+\s*=\s*Math\.max\(0,/.exec(fgs);
            if (m) { N.Da = m[1]; }
            if (N.hp) {
                m = new RegExp("if\\s*\\(\\s*\\w+\\.(\\w+)\\s*\\)\\s*\\{\\s*var\\s+\\w+\\s*=\\s*\\w+\\." + N.hp + "\\s*,").exec(fgs);
                if (m) { N.Db = m[1]; }
            }
            log("skill cost = ." + fg + "  fields: maxCharge=" + N.CZ + " charge=" + N.CY +
                " cd=" + N.cd + " mpCost=" + N.Da + " hpCost=" + N.Db);
        } else log("!! skill cost fn NOT FOUND");

        /* 技能门(指纹: 用上面拿到的 CZ/CY/cd 拼出它特有的三元式) */
        var ff = null;
        if (N.CZ && N.CY && N.cd) {
            var re = new RegExp("\\w+\\." + N.CZ + "\\s*>\\s*0\\s*\\?\\s*\\w+\\." + N.CY + "\\s*<=\\s*0\\s*:\\s*\\w+\\." + N.cd);
            ff = findFn(K, function (s) { return re.test(s); });
            if (ff) {
                N.Ff = ff;
                var m2 = /null\s*==\s*\w+\s*\|\|\s*!\w+\.(\w+)/.exec(srcOf(K[ff]));
                if (m2) { N.Dc = m2[1]; }
                log("skill gate = ." + ff + "  unlockedField=" + N.Dc);
            } else log("!! skill gate NOT FOUND");
        }

        //// 无限技能
        // 咽喉1 = 技能门(战斗逻辑与 UI 技能按钮共用同一个函数, 改这里按钮同步亮起)
        // 咽喉2 = 释放消耗: 临时清零蓝耗/血耗, 事后把 cd 归零、充能补满
        if (ff) {
            var oFf = K[ff];
            K[ff] = function (ent, sk) {
                try {
                    if (MOD.feature3) {
                    if (sk && (!N.Dc || sk[N.Dc]) && isMine(ent)) return true;
                    }
                } catch (e) {}
                return oFf.apply(this, arguments);
            };
        }
        if (fg && N.cd) {
            var oFg = K[fg];
            K[fg] = function (ent, sk) {
                var on = false;
                try { on = !!(MOD.feature3 && sk && isMine(ent)); } catch (e) {}
                if (!on) return oFg.apply(this, arguments);
                var da = 0, db = 0, cz = 0, hasDa = false, hasDb = false, called = false, r;
                function restoreCosts() {
                    try { if (hasDa) sk[N.Da] = da; } catch (restoreDaErr) {}
                    try { if (hasDb) sk[N.Db] = db; } catch (restoreDbErr) {}
                }
                try {
                    if (N.Da) { da = sk[N.Da]; sk[N.Da] = 0; hasDa = true; }
                    if (N.Db) { db = sk[N.Db]; sk[N.Db] = 0; hasDb = true; }
                    if (N.CZ) cz = sk[N.CZ];
                    called = true;
                    r = oFg.apply(this, arguments);
                } catch (costErr) {
                    restoreCosts();
                    if (!called) {
                        try { return oFg.apply(this, arguments); } catch (fallbackErr) { return; }
                    }
                    return r;
                }
                restoreCosts();
                try { sk[N.cd] = 0; } catch (cdErr) {}
                try { if (N.CY && cz > 0) sk[N.CY] = cz; } catch (chargeErr) {}
                return r;
            };
        }
        log("feature3 hooked (gate=" + !!ff + " cost=" + !!fg + ")");

        //// 敌人0伤害的代码逻辑(第二处: 死亡例程)
        // 死亡例程内部直接把当前HP写 0, 绕过扣血口。
        // 指纹 = 明文 hitDirType + "把HP字段置0"
        var gj = null;
        if (N.hp) {
            var reHp = new RegExp("\\.\\s*" + N.hp + "\\s*=\\s*0\\s*;");
            gj = findFn(K, function (s) { return s.indexOf("hitDirType") >= 0 && reHp.test(s); });
        }
        if (gj) {
            N.GJ = gj;
            var oGJ = K[gj];
            K[gj] = function (ent) {
                try {
                    if (MOD.feature2) {
                    if (isMine(ent)) return;
                    }
                } catch (e) {}
                return oGJ.apply(this, arguments);
            };
            log("feature2 death-route hooked = ." + gj);
        } else log("!! death route NOT FOUND (hp=" + N.hp + ")");
    }

    /* ---------- 5. 实体类: 唯一扣血口 ---------- */
    /* ☠ 只用 lockMinHP + finalHurtValue 会撞车: 属性组件有个"清零"方法
       (this.finalDamage = 0; this.finalHurtValue = 0; this.lockMinHP = 0;)
       三个词全含, 而且它所在模块【比实体模块先加载】→ 会把 entDone 占掉,
       真正的扣血口就再也挂不上(实机表现: 只有无敌不生效)。
       判据必须是【当前HP 减 伤害 再 clamp 到 lockMinHP】这个减法。 */
    var RE_APPLY = /(\w+)\.(\w+)\s*=\s*Math\.max\(\1\.\2\s*-\s*\w+,\s*\1\.lockMinHP\)/;
    function tryEntity(Cls) {
        if (N.entDone || !Cls.prototype) return;
        var hx = findFn(Cls.prototype, function (s) {
            return s.indexOf("finalHurtValue") >= 0 && RE_APPLY.test(s);
        });
        if (!hx) {
            /* 撞车时留一条线索, 便于以后排查 */
            var decoy = findFn(Cls.prototype, function (s) {
                return s.indexOf("lockMinHP") >= 0 && s.indexOf("finalHurtValue") >= 0;
            });
            if (decoy) log("skip decoy .prototype." + decoy + " (no hp-subtraction)");
            return;
        }
        N.entDone = 1;
        N.hX = hx;
        log("entity class hit: damage-apply = .prototype." + hx);

        var oHX = Cls.prototype[hx];
        Cls.prototype[hx] = function (atk, dmg, hit) {
            try {
                if (MOD.feature2) {
                //// 敌人0伤害的代码逻辑
                // 咽喉 = 全局唯一扣血口(指纹: 明文 lockMinHP + finalHurtValue)。
                //        普攻 / DOT / 反伤 / 技能自扣血 / GM 全部走这里。
                if (isMine(this)) {
                    if (hit) hit.finalDamage = 0;      /* 飘字显示 0(明文字段) */
                    return;
                }
                }
            } catch (e) {}
            return oHX.apply(this, arguments);
        };
        log("feature2 hooked");
    }

    /* ---------- 6. 移动系统: 移速 ---------- */
    function trySystem(Cls) {
        if (N.sysDone || !Cls.prototype) return;
        var up = Cls.prototype.update;                 /* update 是明文 ECS API 名 */
        if (typeof up !== "function") return;
        var s = srcOf(up);
        if (s.indexOf("worldTimeScale") < 0 || s.indexOf("hitPauseTimeScale") < 0 || s.indexOf("hTimeScale") < 0) return;
        N.sysDone = 1;
        log("movement system hit");

        var oUp = up;
        Cls.prototype.update = function (list) {
            try {
                if (this.context && N.ctxKey && this.context[N.ctxKey]) CTX = this.context[N.ctxKey];
            } catch (e) {}
            var touched = null;
            try {
                if (MOD.speed > 1) {
                //// 移速
                // 水平位移 = 速度 * (dt * worldTimeScale * hitPauseTimeScale * hTimeScale)。
                // 进 update 前把我方英雄的 hTimeScale 乘 3、出来还原, 等价于把水平
                // 步长放大 3 倍;垂直/跳跃走 vTimeScale 分支, 不受影响。
                if (list && list.length) for (var i = 0; i < list.length; i++) {
                    var ent = list[i];
                    if (!isMyHero(ent)) continue;
                    var hy = tsComp(ent);
                    if (!hy) continue;
                    var _sp = (MOD.speed > 0 ? MOD.speed : 1); hy.hTimeScale = hy.hTimeScale * _sp;
                    if (!touched) touched = [];
                    touched.push([ hy, _sp ]);
                }
                }
            } catch (e) {}
            try {
                return oUp.apply(this, arguments);
            } finally {
                if (touched) for (var j = 0; j < touched.length; j++) {
                    try { touched[j][0].hTimeScale = touched[j][0].hTimeScale / (touched[j][1] || 1); } catch (e) {}
                }
            }
        };
        log("feature4 hooked");
    }


    /* ---------- 6b. 自动战斗特权: 仅在开关开启时放行 AUTO_BATTLE 判定 ---------- */
    function tryAuto(Cls) {
        if (N.autoDone || !Cls || !Cls.prototype) return;
        var f = findFn(Cls.prototype, function (s) {
            return s.indexOf("AUTO_BATTLE") >= 0 && s.indexOf("bVs(") >= 0 && s.length < 600;
        });
        if (!f || !Object.prototype.hasOwnProperty.call(Cls.prototype, f)) return;
        var orig = Cls.prototype[f];
        if (typeof orig !== "function") return;
        N.autoDone = 1;
        N.autoFn = f;
        Cls.prototype[f] = function () {
            try {
                if (MOD.feature5) {
                    if (!N.autoGranted) { N.autoGranted = 1; log("auto-battle privilege granted"); }
                    return true;
                }
            } catch (e) {}
            return orig.apply(this, arguments);
        };
        log("auto-battle privilege hooked = .prototype." + f);
    }


    /* ---------- 7. 启动: 先扫全部模块源码取名, 再懒包装目标模块 ---------- */
    var srcs = {}, id, nmod = 0;
    for (id in REG) {
        if (!REG[id] || typeof REG[id][0] !== "function") continue;
        var s0 = srcOf(REG[id][0]);
        if (s0) { srcs[id] = s0; nmod++; }
    }
    log("boot: " + nmod + " modules, toString " + (nmod > 0 ? "OK" : "UNAVAILABLE — 自愈无法工作"));
    for (id in srcs) harvestA(srcs[id]);
    for (id in srcs) harvestB(srcs[id]);
    MOD_boot.names = N;                /* 排查用: 控制台看 MOD_boot.names */
    log("names: hx=" + N.hx + "." + N.isPlayer + "  camp=" + N.hD + "." + N.camp +
        "  ctx=" + N.ctxKey + "/" + N.teams + "/" + N.teamCamp + "  hp=" + N.hp + "  world=" + N.world);

    function scan(exp) {
        if (!exp) return;
        for (var k in exp) {
            var V;
            try { V = exp[k]; } catch (e) { continue; }
            if (typeof V !== "function") continue;
            try { tryLogic(V); } catch (e) { log("tryLogic ERR: " + e); }
            try { tryEntity(V); } catch (e) { log("tryEntity ERR: " + e); }
            try { trySystem(V); } catch (e) { log("trySystem ERR: " + e); }
            try { tryAuto(V); } catch (e) { log("tryAuto ERR: " + e); }
        }
    }
    var targets = [];
    for (id in srcs) {
        var s = srcs[id];
        if (s.indexOf("maxApDefRate") < 0 && s.indexOf("lockMinHP") < 0 && s.indexOf("hitPauseTimeScale") < 0 && s.indexOf("AUTO_BATTLE") < 0) continue;
        targets.push(id);
        (function (mid, entry) {
            var orig = entry[0];
            entry[0] = function (req, mod, exp) {
                orig.apply(this, arguments);
                try { scan(mod && mod.exports ? mod.exports : exp); } catch (e) { log("scan ERR: " + e); }
            };
        })(id, REG[id]);
    }
    log("wrapped " + targets.length + " target modules: " + targets.join(","));

    /* 全部模块跑完后报一次总账(入口列表会立刻同步执行完所有模块) */
    try {
        setTimeout(function () {
            log("REPORT  formula=" + N.FV + " apply=" + N.hX + " gate=" + N.Ff +
                " cost=" + N.Fg + " death=" + N.GJ + " movesys=" + (N.sysDone ? "yes" : "NO") + " auto=" + (N.autoDone ? "yes" : "NO"));
            log("REPORT  MOD=" + (typeof MOD === "undefined" ? "undefined" :
                ("f1=" + MOD.feature1 + " f2=" + MOD.feature2 + " f3=" + MOD.feature3 + " f4=" + MOD.feature4 + " f5=" + MOD.feature5 + " dmg=" + MOD.damage)));
        }, 3000);
    } catch (e) {}
}

window.__starry_capture = function (reg) {
  try { MOD_boot(reg); } catch (e) {
    try { if (window.__starry_nlog) window.__starry_nlog("[MODMOD] boot ERR: " + e); } catch (ignored) {}
  }
  return reg;
};

try {
  if (window.__starry_nlog) window.__starry_nlog("[MODMOD] capture installed");
} catch (e) {}

)YSXZJS";
