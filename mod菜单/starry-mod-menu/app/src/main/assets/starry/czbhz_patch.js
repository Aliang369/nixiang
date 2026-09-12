/* ============================================================================
 * czbhz_patch.js — 星空版《城主别慌张》页面内 JS 补丁（v1）
 *
 * 载体：assets/starry/czbhz_patch.js（外科注入塞进游戏 APK）
 * 注入：com.starry.magic.h5box.StarryH5 —— 每 1s（ack 前）/ 5s（ack 后）
 *       用 evaluateJavascript 灌一次；页面重载后自动重装。
 *       （**不要**改回 loadUrl 拼 javascript: URL：那条路径会把脚本当 URL 解析 ——
 *        百分号会被解码，而"百分号编码的井号"在 URL 里是片段起点，于是脚本被当场
 *        截断、WebView 静默丢弃（本作曾因此整段补丁一行日志都没有）。
 *        本文件因此**禁止出现百分号与井号**，连注释里也不行；离线回归有对应断言。
 *        详见 10_docs/城主别慌张/分析_总览_20260911.md §4.4。）
 *
 * 设计铁律
 *   1) 幂等：window.__czbhz_v1 守卫；重载即重装，重复注入零副作用。
 *   2) 自延迟：不假设任何加载时序，轮询等 window.xy 就绪再装钩（本作业务代码是
 *      System.import 异步装入的，window.xy 由 Core.ts 在模块执行时才挂上）。
 *   3) 授权门控：state.live=0（未授权/过期/后台停用）时一律不生效。
 *   4) 不抛异常：每个钩子独立 try/catch，坏一个不影响游戏与其他钩子。
 *   5) 偏离最小：优先复用游戏自身的官方捷径，而不是绕开它的数据流
 *      （见下方「免广告」的选型理由）。
 *
 * 日志：window.StarryH5.log(...) → logcat tag "StarryCzbhzJs"
 * 分析依据：10_docs/城主别慌张/{分析_总览,上报与检测面,战斗钩子分析}_20260911.md
 * ==========================================================================*/
(function () {
    'use strict';

    /* ---- 防御：注入可能早于页面初始化（此时还没有 JS 桥），别占守卫 ---- */
    if (window.__czbhz_v1) { return; }
    if (!window.StarryH5) { return; }           // 桥未就绪，等下一次注入
    window.__czbhz_v1 = 1;

    /* ---- 功能位号：必须与 Games/CzbhzEntry.cpp 的 featureList 下标一致 ---- */
    var F_AD = 1,   // Toggle_免广告
        F_INV = 3,  // Toggle_无敌
        F_DMG = 4,  // SeekBar_伤害倍率秒杀_1_1000_50
        F_SPD = 5,  // SeekBar_移速_1_10_1
        F_SKILL = 6; // Toggle_无限技能

    var POLL_MS = 500;
    var MAX_WAIT_MS = 10 * 60 * 1000;   // 10 分钟后放弃（足够进游戏）
    var AD_P1_TIMEOUT_MS = 15000;       // P1 装不上就退化到 P2
    var waitStart = Date.now();

    function log() {
        try {
            var s = '';
            for (var i = 0; i < arguments.length; i++) {
                var a = arguments[i];
                s += (i ? ' ' : '') + (typeof a === 'object' ? safeJson(a) : String(a));
            }
            window.StarryH5.log(s);
        } catch (e) { /* 日志失败绝不影响游戏 */ }
    }

    function safeJson(o) {
        try { return JSON.stringify(o); } catch (e) { return String(o); }
    }

    /* ======================================================================
     * 菜单状态（由 Java 侧给出；live=0 → 全功能哑火）
     * ==================================================================== */
    var state = { live: 0, specs: [], bools: [], vals: [] };

    function b(i) { return state.live && state.bools && state.bools[i] ? true : false; }
    function v(i) { return state.live && state.vals ? (state.vals[i] | 0) : 0; }

    function refreshState() {
        try {
            var raw = window.StarryH5.getState();
            if (!raw) { return; }
            var o = JSON.parse(raw);
            if (o && typeof o.live !== 'undefined') { state = o; }
        } catch (e) {
            log('getState 失败: ' + e);
        }
    }

    /* ======================================================================
     * ① 免广告 —— 走游戏官方的「免广告特权」捷径
     *
     * 已核实的实现（dumped/mod_SdkUtils.pretty.js:282）：
     *   SdkUtils.watchAd = function (e) {
     *       o.D.ad.canSkipAdWatch() ? e.run() : <isAlipayPcWeb> : <xyMini.watchVideoAd(...)>
     *   }
     * 而 AdModel 构造函数里（AdModel.ts.pretty.js:63）：
     *   o.canSkipAdWatch = function () { return i.D.privilege.getPrivilegeValue(r.SkipAd) > 0 }
     *
     * 全库只有这 1 个 canSkipAdWatch 调用点 ⇒ 把它置真，等价于
     * 「一个拥有免广告特权的正常玩家」，后续 onWatchAdDone → 11801 服务端请求
     * 的 {type,params} 与真播路径逐字节相同，服务端 AfkReward.AdTimes 记账照常。
     *
     * 为什么不用别的口子：
     *   ✗ 改 getAdInfo().leftTimes/canWatch → 绕过 watchAd→onWatchAdDone，
     *     11801 根本不发 → 服务端不记账、UI 不刷新，反而比官包更可疑；
     *   ✗ 给 SdkUtils.uploadAdAction 补真上报 → 它本来就是空函数
     *     （uploadAdAction = function () {}），补上报等于凭空制造差异。
     *
     * P2 兜底：万一本版 xy.D.ad 始终拿不到（实例被延迟创建/命名变化），
     * 退化为包装 xy.U.sdk.watchAd —— 效果相同，只是偏离稍大（跨过了官方的
     * 特权判定那一步）。二者互斥，P1 装上了就不再装 P2。
     * ==================================================================== */
    var adHook = 0;         // 0=未装 1=P1(特权置真) 2=P2(包装 watchAd)
    var adSdkOrig = null;

    function installAdP1() {
        var model = window.xy && window.xy.D && window.xy.D.ad;
        if (!model || typeof model.canSkipAdWatch !== 'function') { return false; }
        if (model.canSkipAdWatch.__czbhz === 1) { adHook = 1; return true; }
        var orig = model.canSkipAdWatch;
        var wrap = function () {
            try {
                if (b(F_AD)) { return true; }
            } catch (e) { /* 状态异常时走原逻辑 */ }
            return orig.apply(this, arguments);
        };
        wrap.__czbhz = 1;
        model.canSkipAdWatch = wrap;
        adHook = 1;
        log('免广告已装[P1]：AdModel.canSkipAdWatch 恒真（官方特权捷径，11801 照发）');
        return true;
    }

    function installAdP2() {
        var sdk = window.xy && window.xy.U && window.xy.U.sdk;
        if (!sdk || typeof sdk.watchAd !== 'function') { return false; }
        if (sdk.watchAd.__czbhz === 1) { adHook = 2; return true; }
        var orig = sdk.watchAd;
        adSdkOrig = orig;
        var wrap = function (handler) {
            try {
                if (b(F_AD) && handler && typeof handler.run === 'function') {
                    log('免广告[P2]：直接续接奖励回调');
                    handler.run();
                    return;
                }
            } catch (e) {
                log('免广告[P2] 异常，回退真实广告: ' + e);
            }
            return orig.apply(this, arguments);
        };
        wrap.__czbhz = 1;
        sdk.watchAd = wrap;
        adHook = 2;
        log('免广告已装[P2]：包装 xy.U.sdk.watchAd');
        return true;
    }

    /** 钩子存活检查：xy.D.ad 极少被重建，但账户切换/重登有可能，被冲掉就补装 */
    function adHookAlive() {
        try {
            if (adHook === 1) {
                var m = window.xy && window.xy.D && window.xy.D.ad;
                return !!(m && m.canSkipAdWatch && m.canSkipAdWatch.__czbhz === 1);
            }
            if (adHook === 2) {
                var s = window.xy && window.xy.U && window.xy.U.sdk;
                return !!(s && s.watchAd && s.watchAd.__czbhz === 1);
            }
        } catch (e) { /* ignore */ }
        return false;
    }

    function maintainAdHook() {
        if (adHook !== 0 && !adHookAlive()) {
            log('免广告钩子被游戏重建冲掉，正在补装');
            adHook = 0;
        }
        if (adHook !== 0) { return; }
        if (installAdP1()) { return; }
        if (Date.now() - waitStart > AD_P1_TIMEOUT_MS) {
            installAdP2();
        }
    }

    /* ======================================================================
     * ② 战斗类钩子（无敌 / 伤害倍率秒杀 / 移速 / 无限技能）
     *
     * 锚点（已本人复核，非子代理结论）：`xy.battle.startFightFromLogicInput(li, opts)`
     *   —— Battle.ts 实例方法，每场真实战斗的唯一入口：
     *        Battle.startFightFromLogicInput = function (li, e) {
     *            if (!D.fight.getFightCfg(li.fight_id)) { 请求 12103 后**重入本函数** }
     *            else BattleInputConverter.startFightFromLogicInput(li, e)
     *        }
     *      Converter 内部：`createBattleParamFromLogicInput(li)` 直接返回
     *        { ..., logic_input: li }  ← **同一对象引用**，仿真读的就是它
     *        teams: li.teams.map(BattleTeamData.fromBattleTeam)  ← 也从被改后的 unit 派生
     *      所以在包装器里就地改 `li.teams[].units[].attr`，
     *      仿真与显示**同时**吃到改动。
     *
     * 为什么不是子代理推荐的 `FightUtil.applyDebugLogicInput`：复核其真实实现为
     *     applyDebugLogicInput = function (li) { li && this.debugFixedFight && (li.is_fixed = !0) }
     *   —— debugFixedFight 默认 0，只置 is_fixed，且仅被 super_jump 分支与
     *   startToolFight 调用；普通战斗根本不经过它。**该结论已作废**。
     *
     * 「我方」判据：`team.player_id === li.player_id`
     *   —— 与游戏自身构造 logic_input 的方式同源（debugPveFight/debugPvpFight 里
     *      顶层 player_id 与我方 team.player_id 取同一个变量）。
     *   首场战斗会打印每队 team_type/player_id，便于一眼确认识别是否正确。
     *
     * ⚠️ 风险（务必知情）：四类改动都会改变仿真的 end_seed
     *   （`end_seed = randomer.getSeed()`，LCG 迭代式 seed = (seed×9301 + 49297) mod 233280），
     *   而 12102 reqFightVerify 会回传 data.last_seed 与 options.damage/damage_list。
     *   服务端是否按 begin_seed/last_seed 重放复算无法从客户端确认（标「待验证」）。
     *   PVE 的 logic_input 与 seed 本身由客户端构造，服务端手上没有可直接比对的输入，
     *   但这只是"降低了可校验性"，不等于"不会被判定为异常"。
     *   详见 10_docs/城主别慌张/战斗钩子分析_20260911.md §④上报一致性。
     * ==================================================================== */
    var ATTR = {
        ATT: 1002,             // 攻击
        SPEED: 1005,           // 移速（定点真值 speed10=10050 由 UnitComp.setInit 自动派生）
        SHIELD: 1200,          // 护盾（免伤走此路径，不进 df.wrapBeDead 死亡分支）
        ANGER_MAX: 1101,       // 怒气上限
        SKILL_ANGER_MIN: 1103, // 施法怒气门槛
        COOL_REDUCE: 3046,     // 冷却缩减（1e4 满值）
        DAM_BONUS: 3001        // 伤害加成
    };

    var battleHooked = false;
    var battleDiagLogged = false;

    function battleTarget() {
        var xy = window.xy;
        if (!xy || !xy.battle) { return null; }
        if (typeof xy.battle.startFightFromLogicInput !== 'function') { return null; }
        return xy.battle;
    }

    /** 首次开战打印一次战场结构，用于确认「我方」识别与 attr 形状 */
    function dumpBattleInputOnce(li) {
        if (battleDiagLogged || !li || !li.teams) { return; }
        battleDiagLogged = true;
        try {
            var desc = [];
            for (var i = 0; i < li.teams.length; i++) {
                var t = li.teams[i] || {};
                var u0 = (t.units && t.units[0]) || {};
                desc.push('[i=' + i + ' team_id=' + t.team_id + ' team_type=' + t.team_type +
                    ' player_id=' + t.player_id + ' units=' + (t.units ? t.units.length : 0) +
                    ' 首单位pid=' + u0.pid + ' attrKeys=' +
                    (u0.attr ? Object.keys(u0.attr).slice(0, 12).join('/') : 'none') + ']');
            }
            log('战场结构 li.player_id=' + li.player_id + ' fight_id=' + li.fight_id +
                ' ' + desc.join(' '));
        } catch (e) {
            log('战场结构 dump 异常: ' + e);
        }
    }

    /**
     * 就地注入属性。幂等：攻击/移速以**首见原值**为基数换算（`__czbhz_*`
     * 记录原始值），因此 12103 取配置后的重入、或用户中途拖滑块，都不会复利叠加；
     * 护盾/冷却/怒气是绝对值赋值，天然幂等。
     */
    function patchLogicInput(li) {
        if (!li || typeof li !== 'object' || !li.teams) { return; }
        var inv = b(F_INV), dmg = v(F_DMG), spd = v(F_SPD), skill = b(F_SKILL);
        dumpBattleInputOnce(li);
        if (!inv && !dmg && !spd && !skill) { return; }

        var myPid = (li.player_id === null || li.player_id === undefined)
            ? null : String(li.player_id);
        var touched = 0;
        for (var t = 0; t < li.teams.length; t++) {
            var team = li.teams[t];
            if (!team || !team.units) { continue; }
            var isMine = (myPid !== null && team.player_id !== undefined
                && String(team.player_id) === myPid);
            for (var u = 0; u < team.units.length; u++) {
                var unit = team.units[u];
                if (!unit || !unit.attr) { continue; }
                var a = unit.attr;
                if (isMine) {
                    if (inv) { a[ATTR.SHIELD] = 1000000000; touched++; }
                    if (dmg > 0) {
                        if (unit.__czbhz_att === undefined) { unit.__czbhz_att = Number(a[ATTR.ATT]) || 1; }
                        a[ATTR.ATT] = Math.floor(unit.__czbhz_att * dmg);
                        if (!a[ATTR.DAM_BONUS] || a[ATTR.DAM_BONUS] < 10000) { a[ATTR.DAM_BONUS] = 10000; }
                        touched++;
                    }
                    // ⚠️ 移速必须留在 isMine 内：PVP/GVE 的 logic_input.teams 里**两队同处一个数组**
                    //    （buildPvpLogicInput: [Challenge, Rival]；onResGve/startLocalGveFight 同），
                    //    写在 isMine 外会把对手一起加速。PVE 只有我方一队（startLocalPveFight），
                    //    所以这个 bug 只在 PVP/GVE 暴露 —— 2026-09-12 由离线回归用例抓出。
                    if (spd > 0) {
                        if (unit.__czbhz_spd === undefined) { unit.__czbhz_spd = Number(a[ATTR.SPEED]) || 0; }
                        a[ATTR.SPEED] = Math.floor(unit.__czbhz_spd * spd);
                        touched++;
                    }
                    if (skill) {
                        a[ATTR.COOL_REDUCE] = 10000;                 // 冷却缩减拉满
                        a[ATTR.SKILL_ANGER_MIN] = a[ATTR.ANGER_MAX]; // 怒气门槛 = 上限
                        touched++;
                    }
                }
            }
        }
        if (touched) {
            log('战斗属性注入：fight_id=' + li.fight_id + ' 无敌=' + (inv ? 1 : 0) +
                ' 伤害×' + dmg + ' 移速×' + spd + ' 无限技能=' + (skill ? 1 : 0));
        }
    }

    function installBattleHooks() {
        var battle = battleTarget();
        if (!battle) { return false; }
        if (battle.startFightFromLogicInput.__czbhz === 1) { battleHooked = true; return true; }
        var orig = battle.startFightFromLogicInput;
        var wrap = function (logicInput) {
            try {
                patchLogicInput(logicInput);
            } catch (e) {
                log('战斗属性注入异常（已忽略，按原样开战）: ' + e);
            }
            return orig.apply(this, arguments);
        };
        wrap.__czbhz = 1;
        battle.startFightFromLogicInput = wrap;
        battleHooked = true;
        log('战斗钩子已装：xy.battle.startFightFromLogicInput（每次开战前注入 attr）');
        return true;
    }

    function battleHookAlive() {
        try {
            var b2 = window.xy && window.xy.battle;
            return !!(b2 && b2.startFightFromLogicInput
                && b2.startFightFromLogicInput.__czbhz === 1);
        } catch (e) { return false; }
    }

    /* ======================================================================
     * 主循环
     * ==================================================================== */
    setInterval(function () {
        refreshState();
        if (Date.now() - waitStart > MAX_WAIT_MS) { return; }
        maintainAdHook();
        if (battleHooked && !battleHookAlive()) {
            log('战斗钩子被游戏重建冲掉，正在补装');
            battleHooked = false;
        }
        if (!battleHooked) { installBattleHooks(); }
    }, POLL_MS);

    refreshState();
    log('补丁 v1 已装载（live=' + state.live + '，功能表 ' +
        (state.specs ? state.specs.length : 0) + ' 项）');
    try { window.StarryH5.ack('patch v1 loaded'); } catch (e) { /* ignore */ }
})();
