#!/usr/bin/env node
/* ============================================================================
 * offline_regression.js — 《城主别慌张》星空版补丁的离线行为回归
 *
 * 为什么需要它：本作是华为渠道包，重签名后平台登录被阻断（见 星空落地_1.0.10.md §5），
 * 所以 5 项功能**没法在真机上做行为验证**。这个闸门用 Node + vm 造一个
 * `window`/`window.xy` 仿真环境，把补丁源码原样丢进去跑，逐项断言补丁对游戏对象的
 * 实际写入 —— 把"逻辑层证据"补上（它证明不了游戏内的最终效果，但能证明补丁的
 * 判定、写入、幂等、自愈都对）。
 *
 * 覆盖：
 *   A. 装载与守卫（桥未就绪不占守卫 / 装载日志 / ack）
 *   B. 授权门控（live=0 全哑火 / live=1 生效 / 开关实时跟随）
 *   C. 免广告（P1 特权置真 / 被冲掉补装 / P2 兜底与超时）
 *   D. 战斗四项（PVE 单队 / PVP 双队**只动我方** / 幂等抗 12103 重入 / 滑块改值不复利）
 *   E. 鲁棒性（畸形 logicInput 不抛、不阻断开战 / 钩子被重建后自愈）
 *   F. 源码级地雷（`%`/`#` 会截断 loadUrl 退路 / featNum 与 CzbhzEntry.cpp 菜单对齐）
 *
 * 用法：node 04_scripts/城主别慌张/offline_regression.js
 * 退出码：0 = 全过；1 = 有失败
 * ==========================================================================*/
'use strict';

const fs = require('fs');
const path = require('path');
const vm = require('vm');

const ROOT = path.resolve(__dirname, '..', '..');
const PATCH = path.join(ROOT, 'mod菜单/starry-mod-menu/app/src/main/assets/starry/czbhz_patch.js');
const ENTRY = path.join(ROOT, 'mod菜单/starry-mod-menu/app/src/main/jni/Games/CzbhzEntry.cpp');

const POLL_MS = 500;
const AD_P1_TIMEOUT_MS = 15000;

/* ---------- 极简测试框架 ---------- */
let passed = 0;
const failures = [];
function check(name, fn) {
    try {
        fn();
        passed++;
        console.log('  ✓ ' + name);
    } catch (e) {
        failures.push(name + ' → ' + e.message);
        console.log('  ✗ ' + name + ' → ' + e.message);
    }
}
function eq(actual, expected, what) {
    if (actual !== expected) {
        throw new Error((what || '') + ' 期望 ' + JSON.stringify(expected) + '，实际 ' + JSON.stringify(actual));
    }
}
function ok(cond, what) {
    if (!cond) { throw new Error(what || '断言失败'); }
}
function section(t) { console.log('\n' + t); }

/* ---------- 仿真环境 ---------- */
const ATTR = {
    CUR_HP: 1000, HP: 1001, ATT: 1002, DEF: 1003, SPEED: 1005,
    ANGER_MAX: 1101, SKILL_ANGER_MIN: 1103, SHIELD: 1200, DAM_BONUS: 3001, COOL_REDUCE: 3046
};

/** 造一个单位（attr 用游戏里的真实数字键） */
function mkUnit(pid, att, spd, angerMax) {
    return {
        pid: pid,
        attr: Object.assign({
            [ATTR.HP]: 1000, [ATTR.CUR_HP]: 1000,
            [ATTR.ATT]: att, [ATTR.DEF]: 50, [ATTR.SPEED]: spd,
            [ATTR.ANGER_MAX]: angerMax, [ATTR.SKILL_ANGER_MIN]: 30,
            [ATTR.SHIELD]: 0, [ATTR.DAM_BONUS]: 0, [ATTR.COOL_REDUCE]: 0
        })
    };
}

function mkTeam(teamId, playerId, units, teamType) {
    return { team_id: teamId, player_id: playerId, team_type: teamType === undefined ? 1 : teamType, units: units };
}

/** PVE：teams 里只有我方一队（与 Battle.startLocalPveFight 一致） */
function mkPveInput(myAtt, mySpd) {
    return {
        uuid: '121001#1', fight_id: '121001', player_id: '10086', seed: 123456, drop_seed: 654321,
        teams: [mkTeam(1, '10086', [mkUnit(900001, myAtt, mySpd, 100)])]
    };
}

/** PVP/GVE：两支队伍在同一数组（与 buildPvpLogicInput / onResGve 一致） */
function mkPvpInput(myAtt, mySpd, rivalAtt, rivalSpd) {
    return {
        uuid: '201001#1', fight_id: '201001', player_id: '10086', seed: 111, drop_seed: 222,
        teams: [
            mkTeam(1, '10086', [mkUnit(900001, myAtt, mySpd, 100)], 1),
            mkTeam(2, 'rival42', [mkUnit(900002, rivalAtt, rivalSpd, 100)], 2)
        ]
    };
}

/**
 * 造一个沙箱并把补丁跑起来。
 * @param {object} opt  { live, bools, vals, noBridge, noAdModel, specs }
 * @returns 句柄：{ win, logs, ticks, advance, load, reloadPatch, adCallCount }
 */
function newEnv(opt) {
    opt = opt || {};
    const clock = { now: 1000000 };
    const timers = [];
    const logs = [];
    const acks = [];

    const state = {
        live: opt.live === undefined ? 1 : opt.live,
        specs: opt.specs || [
            'Category_基础功能', 'Toggle_免广告', 'Category_战斗', 'Toggle_无敌',
            'SeekBar_伤害倍率秒杀_1_1000_50', 'SeekBar_移速_1_10_1', 'Toggle_无限技能'
        ],
        bools: opt.bools ? opt.bools.slice() : [0, 0, 0, 0, 0, 0, 0],
        vals: opt.vals ? opt.vals.slice() : [0, 0, 0, 0, 0, 0, 0]
    };

    const win = {};
    if (!opt.noBridge) {
        win.StarryH5 = {
            log: function (s) { logs.push(String(s)); },
            ack: function (s) { acks.push(String(s)); },
            authorized: function () { return !!state.live; },
            getState: function () {
                return JSON.stringify({
                    live: state.live, specs: state.specs,
                    bools: state.bools, vals: state.vals
                });
            }
        };
    }

    const counters = { adOrig: 0, startCalls: 0, adSdkOrig: 0 };
    const xy = {};
    if (!opt.noAdModel) {
        // AdModel 的实例属性（构造里赋的），默认不享免广告特权
        xy.D = { ad: { canSkipAdWatch: function () { counters.adOrig++; return false; } } };
    }
    xy.U = {
        sdk: {
            watchAd: function (handler) {
                counters.adSdkOrig++;
                if (handler && typeof handler.run === 'function') { handler.run(); }
                return 'played-real-ad';
            }
        }
    };
    xy.battle = {
        startFightFromLogicInput: function (li) { counters.startCalls++; return 'battle-started:' + (li && li.fight_id); }
    };
    win.xy = xy;

    const sandbox = {
        window: win,
        setInterval: function (fn, ms) { const t = { fn: fn, ms: ms || POLL_MS, alive: true }; timers.push(t); return t; },
        clearInterval: function (t) { if (t) { t.alive = false; } },
        setTimeout: function (fn, ms) { const t = { fn: fn, ms: ms || 0, alive: true, once: true }; timers.push(t); return t; },
        clearTimeout: function (t) { if (t) { t.alive = false; } },
        Date: { now: function () { return clock.now; } },
        JSON: JSON, Math: Math, Number: Number, String: String, Object: Object,
        Array: Array, Boolean: Boolean, isNaN: isNaN, parseInt: parseInt, parseFloat: parseFloat,
        console: { log: function () {}, error: function () {}, warn: function () {} },
        Error: Error
    };
    sandbox.globalThis = sandbox;
    const ctx = vm.createContext(sandbox);

    const src = fs.readFileSync(PATCH, 'utf8');
    function load() { vm.runInContext(src, ctx, { filename: 'czbhz_patch.js' }); }

    function fireTimers() {
        for (const t of timers.slice()) {
            if (t.alive) { t.fn(); if (t.once) { t.alive = false; } }
        }
    }

    return {
        win: win, xy: xy, state: state, logs: logs, acks: acks, timers: timers, counters: counters, src: src,
        load: load,
        /** 推进虚拟时间并按周期触发定时器 */
        advance: function (ms) {
            const steps = Math.ceil(ms / POLL_MS);
            for (let i = 0; i < steps; i++) { clock.now += POLL_MS; fireTimers(); }
        },
        tick: function (n) { this.advance((n || 1) * POLL_MS); },
        waitUntil: function (fn, maxMs) {
            const deadline = (maxMs || 30000);
            let spent = 0;
            while (spent < deadline) {
                if (fn()) { return true; }
                this.advance(POLL_MS); spent += POLL_MS;
            }
            return fn();
        },
        logsMatching: function (re) { return logs.filter(function (l) { return re.test(l); }); }
    };
}

/* ---------- 源码级断言 ---------- */
const src = fs.readFileSync(PATCH, 'utf8');
const entrySrc = fs.readFileSync(ENTRY, 'utf8');

console.log('离线行为回归 —— 《城主别慌张》星空版补丁');
console.log('补丁: ' + path.relative(ROOT, PATCH) + ' (' + Buffer.byteLength(src, 'utf8') + ' B)');

/* ===================== A. 装载与守卫 ===================== */
section('A. 装载与守卫');
check('桥未就绪时不占守卫，且不抛异常', function () {
    const env = newEnv({ noBridge: true });
    env.load();
    eq(env.win.__czbhz_v1, undefined, 'window.__czbhz_v1');
    eq(env.logs.length, 0, '日志条数');
});
check('桥就绪后装载：写守卫 + 装载日志 + ack', function () {
    const env = newEnv({ live: 1 });
    env.load();
    eq(env.win.__czbhz_v1, 1, 'window.__czbhz_v1');
    ok(env.logsMatching(/补丁 v1 已装载（live=1，功能表 7 项）/).length === 1, '装载日志: ' + env.logs.join(' | '));
    eq(env.acks[0], 'patch v1 loaded', 'ack');
});
check('重复注入被守卫拦住（幂等，不重复打日志）', function () {
    const env = newEnv({ live: 1 });
    env.load();
    const n = env.logs.length;
    env.load(); env.load();
    eq(env.logs.length, n, '日志条数不变');
    eq(env.timers.length, 1, '定时器只有一个');
});

/* ===================== B. 授权门控 ===================== */
section('B. 授权门控');
check('live=0：免广告与四项战斗全部哑火', function () {
    const env = newEnv({ live: 0, bools: [0, 1, 0, 1, 1, 1, 1], vals: [0, 0, 0, 0, 50, 3, 0] });
    env.load();
    env.tick(4);
    eq(env.xy.D.ad.canSkipAdWatch(), false, '免广告（live=0 应走原逻辑）');
    const li = mkPveInput(1000, 100);
    env.xy.battle.startFightFromLogicInput(li);
    const a = li.teams[0].units[0].attr;
    eq(a[ATTR.SHIELD], 0, '护盾');
    eq(a[ATTR.ATT], 1000, '攻击');
    eq(a[ATTR.SPEED], 100, '移速');
    eq(a[ATTR.COOL_REDUCE], 0, '冷却缩减');
});
check('live=1：免广告生效；开关关掉后立刻回落', function () {
    const env = newEnv({ live: 1, bools: [0, 1, 0, 0, 0, 0, 0] });
    env.load();
    env.tick(2);
    eq(env.xy.D.ad.canSkipAdWatch(), true, '免广告开');
    env.state.bools[1] = 0;
    env.tick(2);
    eq(env.xy.D.ad.canSkipAdWatch(), false, '免广告关');
    ok(env.counters.adOrig > 0, '关掉后必须回到原函数');
});
check('live 由 1 变 0 时功能立即停止', function () {
    const env = newEnv({ live: 1, bools: [0, 1, 0, 0, 0, 0, 0] });
    env.load();
    env.tick(2);
    eq(env.xy.D.ad.canSkipAdWatch(), true, '开');
    env.state.live = 0;
    env.tick(2);
    eq(env.xy.D.ad.canSkipAdWatch(), false, '授权失效后必须哑火');
});

/* ===================== C. 免广告 ===================== */
section('C. 免广告');
check('P1：包装 canSkipAdWatch 并打标记（幂等）', function () {
    const env = newEnv({ live: 1, bools: [0, 1, 0, 0, 0, 0, 0] });
    env.load();
    env.tick(1);
    eq(env.xy.D.ad.canSkipAdWatch.__czbhz, 1, '标记');
    ok(env.logsMatching(/免广告已装\[P1\]/).length === 1, 'P1 日志');
    env.tick(6);
    ok(env.logsMatching(/免广告已装\[P1\]/).length === 1, '不得重复安装');
});
check('P1 被游戏重建冲掉后自愈补装', function () {
    const env = newEnv({ live: 1, bools: [0, 1, 0, 0, 0, 0, 0] });
    env.load();
    env.tick(1);
    env.xy.D.ad.canSkipAdWatch = function () { return false; };   // 游戏重建
    env.tick(2);
    eq(env.xy.D.ad.canSkipAdWatch.__czbhz, 1, '重新装上');
    ok(env.logsMatching(/补装/).length >= 1, '补装日志');
});
check('P2 兜底：无 AdModel 时超时后包装 sdk.watchAd，跳过真播', function () {
    const env = newEnv({ live: 1, bools: [0, 1, 0, 0, 0, 0, 0], noAdModel: true });
    env.load();
    env.tick(2);
    eq(env.xy.U.sdk.watchAd.__czbhz, undefined, '未超时不得装 P2');
    env.advance(AD_P1_TIMEOUT_MS + POLL_MS);
    eq(env.xy.U.sdk.watchAd.__czbhz, 1, '超时后装上 P2');
    let ran = 0;
    env.xy.U.sdk.watchAd({ run: function () { ran++; } });
    eq(ran, 1, '奖励回调被直接续接');
    eq(env.counters.adSdkOrig, 0, '真播路径未被调用');
});
check('P2 在 live=0 时不得跳过（回落真播）', function () {
    const env = newEnv({ live: 0, bools: [0, 1, 0, 0, 0, 0, 0], noAdModel: true });
    env.load();
    env.advance(AD_P1_TIMEOUT_MS + 2 * POLL_MS);
    let ran = 0;
    env.xy.U.sdk.watchAd({ run: function () { ran++; } });
    eq(ran, 1, '回调由真播路径触发');
    eq(env.counters.adSdkOrig, 1, '必须走真播');
});

/* ===================== D. 战斗四项 ===================== */
section('D. 战斗四项');
check('PVE：四项全部落到我方单位', function () {
    const env = newEnv({ live: 1, bools: [0, 0, 0, 1, 1, 1, 1], vals: [0, 0, 0, 0, 50, 3, 0] });
    env.load();
    env.tick(2);
    const li = mkPveInput(1000, 100);
    env.xy.battle.startFightFromLogicInput(li);
    const a = li.teams[0].units[0].attr;
    eq(a[ATTR.SHIELD], 1000000000, '无敌护盾');
    eq(a[ATTR.ATT], 1000 * 50, '攻击×50');
    eq(a[ATTR.DAM_BONUS], 10000, '伤害加成');
    eq(a[ATTR.SPEED], 100 * 3, '移速×3');
    eq(a[ATTR.COOL_REDUCE], 10000, '冷却缩减满');
    eq(a[ATTR.SKILL_ANGER_MIN], a[ATTR.ANGER_MAX], '怒气门槛=上限');
    eq(env.counters.startCalls, 1, '原函数必须被调用');
});
check('★ PVP/GVE：对手的属性一个字都不能动', function () {
    const env = newEnv({ live: 1, bools: [0, 0, 0, 1, 1, 1, 1], vals: [0, 0, 0, 0, 50, 3, 0] });
    env.load();
    env.tick(2);
    const li = mkPvpInput(1000, 100, 800, 90);
    env.xy.battle.startFightFromLogicInput(li);
    const mine = li.teams[0].units[0].attr;
    const foe = li.teams[1].units[0].attr;
    eq(mine[ATTR.ATT], 50000, '我方攻击×50');
    eq(mine[ATTR.SPEED], 300, '我方移速×3');
    eq(mine[ATTR.SHIELD], 1000000000, '我方无敌');
    eq(foe[ATTR.ATT], 800, '对手攻击');
    eq(foe[ATTR.SPEED], 90, '对手移速（不得被一起加速）');
    eq(foe[ATTR.SHIELD], 0, '对手护盾');
    eq(foe[ATTR.DAM_BONUS], 0, '对手伤害加成');
    eq(foe[ATTR.COOL_REDUCE], 0, '对手冷却');
    eq(foe[ATTR.SKILL_ANGER_MIN], 30, '对手怒气门槛');
});
check('幂等：同一 logicInput 二次入口（模拟 12103 重入）不复利', function () {
    const env = newEnv({ live: 1, bools: [0, 0, 0, 1, 1, 1, 1], vals: [0, 0, 0, 0, 50, 3, 0] });
    env.load();
    env.tick(2);
    const li = mkPveInput(1000, 100);
    env.xy.battle.startFightFromLogicInput(li);
    env.xy.battle.startFightFromLogicInput(li);
    env.xy.battle.startFightFromLogicInput(li);
    const a = li.teams[0].units[0].attr;
    eq(a[ATTR.ATT], 50000, '攻击不得复利');
    eq(a[ATTR.SPEED], 300, '移速不得复利');
    eq(a[ATTR.SHIELD], 1000000000, '护盾仍是绝对值');
});
check('中途改滑块：按首见原值重算，不复利', function () {
    const env = newEnv({ live: 1, bools: [0, 0, 0, 0, 1, 1, 0], vals: [0, 0, 0, 0, 50, 3, 0] });
    env.load();
    env.tick(2);
    const li = mkPveInput(1000, 100);
    env.xy.battle.startFightFromLogicInput(li);
    eq(li.teams[0].units[0].attr[ATTR.ATT], 50000, '第一次 ×50');
    env.state.vals[4] = 10;             // 伤害倍率改成 10
    env.state.vals[5] = 5;              // 移速改成 5
    env.tick(2);
    env.xy.battle.startFightFromLogicInput(li);
    eq(li.teams[0].units[0].attr[ATTR.ATT], 10000, '改成 ×10 应基于原值 1000');
    eq(li.teams[0].units[0].attr[ATTR.SPEED], 500, '改成 ×5 应基于原值 100');
});
check('player_id 数字/字符串混用也能认出我方', function () {
    const env = newEnv({ live: 1, bools: [0, 0, 0, 1, 1, 0, 0], vals: [0, 0, 0, 0, 2, 0, 0] });
    env.load();
    env.tick(2);
    const li = mkPveInput(1000, 100);
    li.player_id = 10086;                    // 顶层是数字
    li.teams[0].player_id = '10086';         // 队里是字符串
    env.xy.battle.startFightFromLogicInput(li);
    eq(li.teams[0].units[0].attr[ATTR.ATT], 2000, '仍应命中我方');
});
check('倍率为 1 时数值保持不变（滑块最小档不制造差异）', function () {
    const env = newEnv({ live: 1, bools: [0, 0, 0, 0, 1, 1, 0], vals: [0, 0, 0, 0, 1, 1, 0] });
    env.load();
    env.tick(2);
    const li = mkPveInput(1234, 77);
    env.xy.battle.startFightFromLogicInput(li);
    eq(li.teams[0].units[0].attr[ATTR.ATT], 1234, '攻击不变');
    eq(li.teams[0].units[0].attr[ATTR.SPEED], 77, '移速不变');
});

/* ===================== E. 鲁棒性 ===================== */
section('E. 鲁棒性');
check('畸形 logicInput 不抛异常，且不阻断开战', function () {
    const env = newEnv({ live: 1, bools: [0, 0, 0, 1, 1, 1, 1], vals: [0, 0, 0, 0, 50, 3, 0] });
    env.load();
    env.tick(2);
    env.xy.battle.startFightFromLogicInput({ fight_id: '121001' });          // 没有 teams
    env.xy.battle.startFightFromLogicInput(null);
    env.xy.battle.startFightFromLogicInput({ fight_id: 'x', teams: [null, { units: null }], player_id: '10086' });
    eq(env.counters.startCalls, 3, '三次都必须照常开战');
    ok(env.logsMatching(/异常/).length >= 0, '允许记日志但不许中断');
});
check('单位缺 attr 时跳过而不抛', function () {
    const env = newEnv({ live: 1, bools: [0, 0, 0, 1, 0, 0, 0] });
    env.load();
    env.tick(2);
    const li = { uuid: 'u', fight_id: '121001', player_id: '10086',
        teams: [mkTeam(1, '10086', [{ pid: 1 }, { pid: 2, attr: null }])] };
    env.xy.battle.startFightFromLogicInput(li);
    eq(env.counters.startCalls, 1, '照常开战');
});
check('battle 钩子被游戏重建后自愈补装', function () {
    const env = newEnv({ live: 1 });
    env.load();
    env.tick(2);
    ok(env.xy.battle.startFightFromLogicInput.__czbhz === 1, '首次装上');
    env.xy.battle.startFightFromLogicInput = function () { env.counters.startCalls++; };  // 游戏重建
    env.tick(2);
    eq(env.xy.battle.startFightFromLogicInput.__czbhz, 1, '重新装上');
    ok(env.logsMatching(/战斗钩子被游戏重建冲掉/).length >= 1, '补装日志');
});
check('xy 尚未就绪时不报错，就绪后自动装钩', function () {
    const env = newEnv({ live: 1, bools: [0, 1, 0, 1, 0, 0, 0] });
    delete env.win.xy;                    // 业务代码还没执行
    env.load();
    env.tick(4);
    env.win.xy = env.xy;                  // 现在 xy 出现了
    env.tick(2);
    eq(env.xy.D.ad.canSkipAdWatch.__czbhz, 1, '延迟就绪后装上免广告');
    ok(env.xy.battle.startFightFromLogicInput.__czbhz === 1, '延迟就绪后装上战斗钩子');
});
check('战场结构诊断只打一次', function () {
    const env = newEnv({ live: 1, bools: [0, 0, 0, 1, 0, 0, 0] });
    env.load();
    env.tick(2);
    env.xy.battle.startFightFromLogicInput(mkPveInput(1000, 100));
    env.xy.battle.startFightFromLogicInput(mkPveInput(1000, 100));
    ok(env.logsMatching(/战场结构 li\.player_id=/).length === 1, '诊断日志条数');
});

/* ===================== F. 源码级地雷 ===================== */
section('F. 源码级地雷');
check('补丁源码不含 % 与 #（loadUrl 退路会被解码截断）', function () {
    const bad = [];
    src.split('\n').forEach(function (line, i) {
        if (line.indexOf('%') >= 0) { bad.push('第 ' + (i + 1) + ' 行含 %: ' + line.trim().slice(0, 60)); }
        if (line.indexOf('#') >= 0) { bad.push('第 ' + (i + 1) + ' 行含 #: ' + line.trim().slice(0, 60)); }
    });
    eq(bad.length, 0, bad.join(' ; '));
});
check('featNum 与 CzbhzEntry.cpp 的菜单顺序对齐', function () {
    const m = entrySrc.match(/kCzbhz_Features\[\]\s*=\s*\{([\s\S]*?)\};/);
    ok(m, '未找到 kCzbhz_Features[]');
    const items = m[1].match(/"[^"]*"/g).map(function (s) { return s.slice(1, -1); });
    eq(items.length, 7, '菜单项数');
    eq(items[1], 'Toggle_免广告', 'index 1');
    eq(items[3], 'Toggle_无敌', 'index 3');
    ok(items[4].indexOf('SeekBar_伤害倍率秒杀') === 0, 'index 4 = ' + items[4]);
    eq(items[5], 'SeekBar_移速_1_10_1', 'index 5');
    eq(items[6], 'Toggle_无限技能', 'index 6');
});
check('补丁里的功能位号常量与菜单对齐', function () {
    const expect = { F_AD: 1, F_INV: 3, F_DMG: 4, F_SPD: 5, F_SKILL: 6 };
    for (const k of Object.keys(expect)) {
        const re = new RegExp(k + '\\s*=\\s*(\\d+)');
        const m = src.match(re);
        ok(m, '未找到 ' + k);
        eq(Number(m[1]), expect[k], k);
    }
});
check('补丁代码内不自行拼 javascript: URL（不得出现 loadUrl 调用）', function () {
    const code = src.replace(/\/\*[\s\S]*?\*\//g, '').replace(/^[ \t]*\/\/.*$/gm, '');
    const hit = code.split('\n').map(function (l, i) { return l.indexOf('loadUrl') >= 0 ? (i + 1) + ': ' + l.trim() : null; }).filter(Boolean);
    eq(hit.length, 0, hit.join(' ; '));
});

/* ---------- 汇总 ---------- */
console.log('\n' + '─'.repeat(64));
if (failures.length === 0) {
    console.log('✅ 离线行为回归全部通过：' + passed + ' 项');
    process.exit(0);
} else {
    console.log('❌ 失败 ' + failures.length + ' 项 / 通过 ' + passed + ' 项');
    failures.forEach(function (f) { console.log('   - ' + f); });
    process.exit(1);
}
