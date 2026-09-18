/* ================= MOD 自动定位安装器 (燃烧纪元) =================
 * 游戏更新后代码变了也能自己找到位置, 原代码一字不改。
 *
 * 开关(外部注入 window.MOD, 判定一律带 typeof 守卫):
 *   MOD.feature1 百倍伤害   MOD.feature2 玩家无敌   MOD.feature3 战斗速度×10
 *   公告固定为"暂无公告" = 无条件生效, 不挂开关
 *
 * ★ 打包结构: Cocos browserify, 整个包就是一个立即调用的函数:
 *       window . __require = function t( 参数表 ) { ... } ( 模块表 , {} , [入口] );
 *   这行是引擎生成的样板代码, **永不混淆** -> 天然挂载点。
 *   工厂是 e[a][0].call(...) 用到时才取, 且 MOD_boot 在原函数跑之前先执行,
 *   所以此刻【所有工厂都还没执行】, 改写模块表一定生效。
 *
 * ★★ 本作补丁是【插在函数体中间】的(不是包方法), 所以做法是:
 *   工厂 toString() -> 结构正则插入 -> new Function 重建 -> 写回模块表。
 *   工厂只闭包到全局(模块表是顶层字面量参数), 所以重建是安全的。
 *
 * ★★★ 定位【不写死任何名字】, 全靠明文字段名和纯形状:
 *   ① 伤害/无敌: 认 changeHp 的函数头 —— 靠 delta / crit / retShield /
 *      isDmg / beKilled 这几个明文字段, 顺带把形参名抄出来用。
 *   ② 速度: 认倍速三元表 `1 == X ? 1 : 2 == X ? 1.5 : 3 == X ? 2 : 1`,
 *      纯形状 + 反向引用, 一个标识符都不含。
 *   ③ 敌我判据: 从 GameConst 源码里【读出 UnitGroup.A 的数值】写成字面量,
 *      运行时零依赖 —— 比生成 `x.default.UnitGroup.A` 少一个抛异常的机会
 *      (实测 Unit 模块里 UnitGroup 出现 0 次, 别名认错就会当场崩战斗)。
 *
 * ☠ 绝不能改属性(attack/defense): Unit.updateOriginAttr 里有【战力压制】
 *   getPowerRepress 会按队伍重算属性把你的修改盖掉; 改最终 delta 不受影响。
 * ================================================================= */

/* ☆ 排障开关: 改成 true 就【不看 MOD】, 3 个功能全部硬开。
   用来分清"钩子没装上"和"MOD 没注入"。apply.py -nosw 会自动改成 true。 */
var MOD_FORCE = false;

/* 战斗速度倍率 */
var MOD_SPEED = 10;

/* 公告固定文案(无条件生效, 不挂开关) */
var MOD_NOTICE = "暂无公告";

/* 日志: 一条消息同时往所有可能的出口写, 保证至少有一个能进 logcat。
   cc.error 在 logcat 里是 error 级, 最不容易被日志等级过滤掉。 */
var MOD_LOG = [];
var MOD_log = function (s) {
    var m = "[MOD] " + s;
    try { MOD_LOG.push(m); } catch (e) {}
    try { if (typeof cc !== "undefined" && cc.error) cc.error(m); } catch (e) {}
    try { if (typeof cc !== "undefined" && cc.log) cc.log(m); } catch (e) {}
    try { if (typeof console !== "undefined" && console.log) console.log(m); } catch (e) {}
    try { if (typeof jsb !== "undefined" && jsb.log) jsb.log(m); } catch (e) {}
    try { if (typeof window !== "undefined") window.__MOD_LOG = MOD_LOG; } catch (e) {}
};

/* 跨模块学到的东西 */
var MOD_mem = {};

/* 本包"看起来该有哪些功能"(明文指纹标记, 每个包一份)。
   ☠ "没匹配上"型告警【绝不能】写在模块级 —— 含某明文串的模块可能有好几个,
   只有一个是目标, 其余每个都会喊一遍, 真信号被淹。交给 MOD_report 包级汇总。 */
var MOD_want = {};

/* 开关判定文本。MOD_FORCE 打开时直接写 1, 功能恒开。 */
var MOD_cond = function (n) {
    return MOD_FORCE ? "1" : ('"undefined" != typeof MOD && MOD.feature' + n);
};

/* 从整包里学 UnitGroup.A 的【数值】。
   GameConst 里长这样:  t.UnitGroup = {  A: 0,  B: 1, ... }
   读成字面量写进补丁 -> 运行时不需要再去访问任何对象, 不可能抛异常。 */
var MOD_learn = function (src) {
    if (MOD_mem.GA != null) return;
    var m = /UnitGroup = \{\s*A: (\d+),/.exec(src);
    if (m) MOD_mem.GA = m[1];
};

/* 对一份工厂源码做全部可用的插入; 没改动就返回 null */
var MOD_patchSource = function (src, log) {
    var out = src, hit = [];
    var say = function (s) { if (log) log(s); };

    // ===== 开关1/2 百倍伤害 / 玩家无敌 =====
    // 锚点: 血量唯一出入口 changeHp 的函数头。
    // 全靠明文字段名认, 函数名和形参名都是抄出来的, 不写死。
    var rHp = /\.prototype\.(\w+) = function\((\w+), (\w+), (\w+), (\w+)\) \{\n/.source
        + /var \w+, (\w+) = \2\.delta, \w+ = \2\.crit, \w+ = \{\n/.source
        + /retShield: 0,\ndelta: \6,\nisDmg: !0,\nbeKilled: !1\n\}/.source;
    var mHp = new RegExp(rHp).exec(out);
    if (mHp) MOD_want.dmg = 1;
    if (mHp) {
        if (MOD_mem.GA == null) {
            say("☠ 伤害/无敌: 认出了 " + mHp[1] + " 但没学到 UnitGroup.A 的值, 宁可不装");
        } else {
            var DMG = mHp[2];          // 伤害信息对象(带 delta/crit) —— 方法第1个形参
            var SRC = mHp[3];          // 来源单位          —— 方法第2个形参
            var GA = MOD_mem.GA;       // 我方队伍号(字面量)
            // 定位到函数头 '{' 之后那一行的开头, 整块插在函数第一句
            var head = out.indexOf("{", mHp.index) + 2;
            // ☠☠☠ 必须先判"这场战斗是不是服务器算好的回放", 否则会出现
            //   【画面打赢了、结算还是输】—— 实测踩过。
            //   回放战斗的胜负是服务器下发的 BR.Ret, 客户端只是拿服务器给的种子重演播动画;
            //   而且战斗结束时客户端会算 verify = randomCtr.random(1,1e6) 跟服务器对,
            //   我们改了伤害 -> 随机数消耗变了 -> verify 必然对不上。
            // ★判据: 回放数据是 replay.BI, 而 ReplayDataDef.createFromReplay 里有
            //     n.BI.Args.seed = o.BR.Vars.seed
            //   客户端权威的那些(挂机/章节副本/外域…)是本地现造 { ..., Args:{module} }, 没有 seed。
            //   组队副本是多人共享种子(handler.data.seed), 改了会让【别人也不同步】, 一并挡掉。
            // ☆ 取不到数据时按"不是回放"处理(放行) —— 门控失败要放行, 不能静默拦掉功能。
            // ☆ 诊断行: 【无条件】打前 3 次, 整局就 3 行, 不刷屏。
            //   ☠ 踩过两次, 记住教训:
            //   ① 别挂在 MOD.debug 上 —— 真机日志一行都没有时, 分不清是
            //      "补丁没跑到" 还是 "用户没注入那个标志", 白跑一轮。
            //   ② 别只打【改之前】的值 —— 看不出补丁到底生没生效, 又白跑一轮。
            //      要打 "改前 -> 改后", 而且要把开关的【实际取值】和
            //      MOD 对象里【到底有哪些字段】一起打出来:
            //      字段名对不对、菜单勾没勾上, 一眼就能看见。
            //   ☆ 这里读开关也一律用【和功能块同一套守卫写法】, 不用三元简写 ——
            //     好处是"每处 MOD.feature 同行都带 typeof MOD &&"这条不变量保持可校验,
            //     自测和 verify 能一直盯着它, 不用为了诊断去放松安全检查。
            var dbg = "var _mf1 = " + MOD_cond(1) + " ? 1 : 0;\n"
                + "var _mf2 = " + MOD_cond(2) + " ? 1 : 0;\n"
                + "var _mf3 = " + MOD_cond(3) + " ? 1 : 0;\n"
                + 'var _mk = "MOD未注入"; try { if ("undefined" != typeof MOD) _mk = JSON.stringify(MOD).slice(0, 300); } catch (_e) { _mk = "无法序列化:" + Object.keys(MOD).join("|"); }\n'
                + 'if ((window.__mdbgD = (window.__mdbgD || 0) + 1) <= 3) '
                + 'MOD_log("扣血#" + window.__mdbgD + ": f1=" + _mf1 + " f2=" + _mf2 + " f3=" + _mf3'
                + ' + " keys=" + _mk'
                + ' + " 回放=" + _mSvr + " 挨打方group=" + this.group'
                + ' + " 出手方group=" + (' + SRC + ' ? ' + SRC + '.group : "无")'
                + ' + " delta " + _m0 + " -> " + ' + DMG + '.delta);\n';
            var ins = "var _mD = this.battleCtr && this.battleCtr.handler && this.battleCtr.handler.data;\n"
                + "var _mSvr = _mD && ((_mD.Args && null != _mD.Args.seed) || null != _mD.seed) ? 1 : 0;\n"
                + "var _m0 = " + DMG + ".delta;\n"
                + "if (" + MOD_cond(1) + ") {\n"
                + "if (!_mSvr && " + DMG + ".delta < 0 && " + SRC + " && " + SRC + ".group == " + GA
                + " && this.group != " + GA + ") " + DMG + ".delta = " + DMG + ".delta * 100;\n"
                + "}\n"
                + "if (" + MOD_cond(2) + ") {\n"
                + "if (!_mSvr && " + DMG + ".delta < 0 && this.group == " + GA + ") " + DMG + ".delta = 0;\n"
                + "}\n"
                + dbg;      // ★放在最后: 要打的是"改后"的值
            out = out.slice(0, head) + ins + out.slice(head);
            hit.push("feature1/2");
            say("百倍伤害+无敌 -> 方法=" + mHp[1] + " 伤害对象=" + DMG
                + " 来源单位=" + SRC + " 我方队伍号=" + GA);
        }
    }

    // ===== 开关3 战斗速度 =====
    // 锚点: 游戏自带的倍速三元表, 纯形状不含任何标识符。
    //   return 1 == X ? 1 : 2 == X ? 1.5 : 3 == X ? 2 : 1;
    // 接管它而不是自己加循环: BattleWin.update 本来就是固定步长累加器
    //   (dt += t * 倍速; while (dt >= FIX_DT) onUpdate(FIX_DT)),
    // 而且同一个倍速还驱动 spine 动画/子弹/倒计时 —— 逻辑和表现天然同步。
    var rSp = /return 1 == ([\w.$]+) \? 1 : 2 == \1 \? 1\.5 : 3 == \1 \? 2 : 1;/g;
    var nSp = 0, srcList = [];
    out = out.replace(rSp, function (whole, expr, offset, all) {
        // ☠ 幂等: 伤害那块插完之后正则自然就不匹配了, 但倍速表插在它【前面】,
        //   表本身还在, 再跑一遍就会再插一行。同一个模块被处理两次是不该发生,
        //   但"跑两遍就坏"的性质不能留 —— 看一眼前面有没有插过。
        if (all.slice(Math.max(0, offset - 60), offset).indexOf("return " + MOD_SPEED + ";") >= 0) {
            return whole;
        }
        nSp++;
        srcList.push(expr);
        // 诊断行放在开关【前面】: 开关没注入时也要能看见, 否则分不清
        // "没接管上" 和 "接管了但开关是关的"。
        return 'if ((window.__mdbgS = (window.__mdbgS || 0) + 1) <= 5) '
            + 'MOD_log("取倍速#" + window.__mdbgS + ": MOD=" + (typeof MOD)'
            + ' + " 开=" + (' + MOD_cond(3) + ' ? 1 : 0));\n'
            + "if (" + MOD_cond(3) + ") return " + MOD_SPEED + ";\n" + whole;
    });
    if (nSp > 0) {
        MOD_want.speed = 1;
        hit.push("feature3x" + nSp);
        say("战斗速度×" + MOD_SPEED + " -> 接管 " + nSp + " 处倍速表 (档位来源: "
            + srcList.join(", ") + ")");
    }

    // ===== 公告固定成"暂无公告"(无条件, 不挂开关) =====
    // 公告是 HTTP 拉的: httpGet(serverConfig.noticeUrl + "?sdk=...") 回来一个
    //   [{ name, title, content, d_start }, ...] 数组, 再 openWin("GameNoticeWin", 数组)。
    // 打开它的地方有三处(登录自动弹 / 登录页手点 / 玩家信息页), 但【显示只有一个出口】:
    //   start() 里的 this._allNotices = this.data
    // 所以在这里把 this.data 换掉, 三条路径全覆盖, 服务器回什么都不影响。
    // ☆ 换的是"要显示的数据", 不是掐 HTTP —— 请求照发(响应里可能还带别的东西),
    //   只是内容一律不采用。要连请求一起掐得动 LoginWin 那边的自动弹窗判定, 风险更大。
    var rNotice = /\.prototype\.(\w+) = function\(\) \{\nthis\.(\w+) = this\.data;\nthis\.\2\.sort\(function\((\w+), (\w+)\) \{\nreturn \4\.d_start - \3\.d_start;\n\}\);/;
    var mN = rNotice.exec(out);
    if (mN) {
        MOD_want.notice = 1;
        var nHead = out.indexOf("{", mN.index) + 2;
        var insN = "this.data = [ {\n"
            + 'name: "' + MOD_NOTICE + '",\n'
            + 'title: "' + MOD_NOTICE + '",\n'
            + 'content: "' + MOD_NOTICE + '",\n'
            + "d_start: Math.floor(Date.now() / 1000)\n"
            + "} ];\n";
        out = out.slice(0, nHead) + insN + out.slice(nHead);
        hit.push("notice");
        say('公告固定为"' + MOD_NOTICE + '" -> 方法=' + mN[1] + " 列表字段=" + mN[2]);
    }

    return hit.length ? { src: out, hit: hit } : null;
};

/* 包级汇总: 拿"本包该有什么"比对"实际装上了什么", 只在真缺的时候喊一次。
   模块级那些"找到一半"的告警不走这里 —— 那些有正向匹配打底, 不会误报。 */
var MOD_report = function (got, log) {
    var say = function (s) { if (log) log(s); };
    if (MOD_mem.GA == null) {
        say("☠ 没学到 UnitGroup.A 的值(GameConst 里 UnitGroup 枚举结构变了) -> 百倍伤害和无敌整个跳过");
    }
    if (!got.dmg) {
        say("☠ 伤害/无敌: 全包没认出 changeHp -> 查 rHp(delta/crit/retShield/isDmg/beKilled 这几个字段名变了?)");
    }
    if (!got.notice) {
        say("☠ 公告: 全包没认出 GameNoticeWin.start 那个出口 -> 查 rNotice(_allNotices/d_start 结构变了?)");
    }
    if (!got.speed) {
        say("☠ 战斗速度: 全包没认出倍速三元表 -> 查 rSp(游戏改了倍速档位或数值?)");
    }
    if (got.speed && got.speedCnt != 5) {
        say("☆ 战斗速度: 认出 " + got.speedCnt + " 处(上个版本是 5 处), 游戏可能增删了战斗面板, 留意有没有玩法没提速");
    }
};

/* 把改好的源码重建成函数。工厂只闭包到全局, 所以 new Function 是安全的。 */
var MOD_rebuild = function (src) {
    var m = /^function\s*[\w$]*\s*\(([^)]*)\)\s*\{/.exec(src);
    if (!m) return null;
    var args = [], raw = m[1].split(",");
    for (var i = 0; i < raw.length; i++) {
        var a = raw[i].replace(/^\s+|\s+$/g, "");
        if (a) args.push(a);
    }
    var body = src.slice(m[0].length, src.lastIndexOf("}"));
    return Function.apply(null, args.concat([ body ]));
};

/* 把整个 __require 构造函数包一层: 被调用时先跑 MOD_boot(模块表), 再原样走原逻辑。
   用 arguments[0] 取模块表 -> 不依赖参数叫什么名字。返回值原样透传, 行为完全不变。 */
var MOD_wrap = function (fn) {
    return function () {
        try { MOD_boot(arguments[0]); } catch (x) {
            try { MOD_log("hook 异常 " + x); } catch (y) {}
        }
        return fn.apply(this, arguments);
    };
};

/* 入口: 参数就是模块表 */
var MOD_boot = function (reg) {
    try {
        if (!reg) { MOD_log("boot: 没拿到模块表, 放弃"); return; }
        if (MOD_boot.__done) { MOD_log("boot: 本文件已装过, 跳过"); return; }
        MOD_boot.__done = 1;
        MOD_want = {};

        var keys = Object.keys(reg);
        MOD_log("boot 进入: 模块数=" + keys.length + (MOD_FORCE ? "  ★硬开模式(不看MOD)" : "  开关模式"));

        // ---- 硬前提自检: toString 必须能拿到真实源码 ----
        // 若引擎预编译成字节码并丢弃源码, 整套做法直接失效 —— 第一时间喊出来
        var probe = null;
        for (var p = 0; p < keys.length; p++) {
            var pf = reg[keys[p]] && reg[keys[p]][0];
            if (typeof pf === "function") { probe = Function.prototype.toString.call(pf); break; }
        }
        if (probe === null) { MOD_log("☠ 模块表里一个函数都没有, 结构不对"); return; }
        if (probe.indexOf("[native code]") >= 0) {
            MOD_log("☠☠☠ toString 返回 [native code] —— 这个引擎拿不到源码, 整套自愈做法失效!");
            return;
        }
        MOD_log("自检: toString 可用, 首个工厂源码 " + probe.length + " 字节");

        // ---- 第一遍: 只学 UnitGroup.A 的值, 不改任何东西 ----
        var scanned = 0;
        for (var i = 0; i < keys.length; i++) {
            try {
                var f0 = reg[keys[i]] && reg[keys[i]][0];
                if (typeof f0 !== "function") continue;
                scanned++;
                MOD_learn(Function.prototype.toString.call(f0));
            } catch (e) {}
            if (MOD_mem.GA != null) break;
        }
        MOD_log("学习: 扫了 " + scanned + " 个模块, 我方队伍号 UnitGroup.A = "
            + (MOD_mem.GA != null ? MOD_mem.GA : "未学到"));

        // ---- 第二遍: 逐个工厂尝试插入, 有改动才重建 ----
        var total = 0, cand = 0, got = { speedCnt: 0 };
        for (var j = 0; j < keys.length; j++) {
            var k = keys[j];
            try {
                var ent = reg[k];
                if (!ent || typeof ent[0] !== "function") continue;
                var src = Function.prototype.toString.call(ent[0]);
                // 便宜预筛: 挑【只有目标模块才有】的串, 省掉大量正则
                //   实测全包出现次数: retShield 4 处 / "? 1.5 : 3 ==" 5 处
                // ☆ d_start 是公告模块的指纹(全包才 3 处), 不加它公告模块会被预筛挡掉
                if (src.indexOf("retShield") < 0 && src.indexOf("? 1.5 : 3 ==") < 0
                    && src.indexOf("d_start") < 0) continue;
                cand++;
                var r = MOD_patchSource(src, MOD_log);
                if (!r) { MOD_log("模块 " + k + " 含锚点但没认出任何功能"); continue; }
                var nf = MOD_rebuild(r.src);
                if (!nf) { MOD_log("☠ 模块 " + k + " 重建失败(函数头没匹配上), 已跳过, 该模块功能不生效"); continue; }
                ent[0] = nf;
                for (var h = 0; h < r.hit.length; h++) {
                    var tag = r.hit[h];
                    if (tag === "feature1/2") got.dmg = 1;
                    if (tag === "notice") got.notice = 1;
                    if (tag.indexOf("feature3") === 0) {
                        got.speed = 1;
                        got.speedCnt += parseInt(tag.slice("feature3x".length), 10) || 0;
                    }
                }
                total += r.hit.length;
                MOD_log("模块 " + k + " 已重写: " + r.hit.join(", "));
            } catch (e) {
                MOD_log("☠ 模块 " + k + " 处理异常, 该模块功能不生效: " + e);
            }
        }
        MOD_log("安装完成: 候选模块 " + cand + " 个, 伤害/无敌=" + (got.dmg ? "已装" : "☠没装")
            + ", 倍速=" + got.speedCnt + " 处, 公告=" + (got.notice ? "已固定" : "☠没装"));
        MOD_report(got, MOD_log);
        if (total === 0) MOD_log("☠☠ 一处都没装上 —— 锚点全部失效, 需要重新定位");
    } catch (e) {
        MOD_log("☠☠☠ boot 整体异常: " + e + (e && e.stack ? (" | " + e.stack) : ""));
    }
};

