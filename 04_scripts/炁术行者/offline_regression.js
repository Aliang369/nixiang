/*
 * qsxz_patch.js 离线行为回归（Node + vm，无设备、无网络）
 *
 * 目的：补丁是「页面内投值器」，写错会出两类真机才发现的问题：
 *   ① 未授权也生效（越权）② 授权了却没生效（本次真实踩到：值投不进去）
 *   v2 起还包含「战斗加速」：滑块 1 = 原版，值必须原样透传且越界必钳（否则城里/结算也被提速）
 * 本闸门把补丁源码放进沙盒跑，断言门控、取值口径与**投值通路**。
 *
 * 用法：node 04_scripts/炁术行者/offline_regression.js
 * 退出码：0 全过；1 有断言失败
 */
'use strict';

const fs = require('fs');
const path = require('path');
const vm = require('vm');

const ROOT = path.resolve(__dirname, '../..');
const PATCH = path.join(ROOT, 'mod菜单/starry-mod-menu/app/src/main/assets/starry/qsxz_patch.js');

const SRC = fs.readFileSync(PATCH, 'utf8');

let pass = 0, fail = 0;
function ok(cond, name) {
    if (cond) { pass++; console.log('  PASS ' + name); }
    else { fail++; console.log('  FAIL ' + name); }
}
function eq(a, b, name) { ok(a === b, name + '（实测 ' + JSON.stringify(a) + '，期望 ' + JSON.stringify(b) + '）'); }
function j(v) { return JSON.stringify(v); }

/**
 * 造一个页面沙盒：StarryH5 桥 + 主 JS 暴露的 __QSW_APPLY。
 * box.state 可直接改，用来模拟「菜单里改了值 / 登录后授权状态变化」。
 */
function makeCtx(initial) {
    const box = { state: initial };
    const logs = [];
    const applied = [];
    const ctx = {
        window: {},
        console: { log() {} },
        JSON, Date, parseInt,
        setInterval: () => 0,   // 手动驱动，不跑真实定时器
        clearInterval: () => {},
        setTimeout: () => 0,
    };
    ctx.window.StarryH5 = {
        log(m) { logs.push(String(m)); },
        ack(d) { logs.push('ack:' + d); },
        getState() { return box.state; },
    };
    ctx.window.__QSW_APPLY = (k, dr, spd) => { applied.push([k, dr, spd]); };
    ctx.globalThis = ctx;
    vm.createContext(ctx);
    return { ctx, logs, applied, box };
}

function load(ctx) { vm.runInContext(SRC, ctx, { filename: 'qsxz_patch.js' }); }
function st(live, k, d, spd) {
    // bools/vals 按 featureList 数组下标：specs[0]=Category 占 0，1=倍率，2=减伤，3=战斗加速
    // spd 缺省 1 = 原版不干预（旧调用方语义）
    return JSON.stringify({ live: live, specs: [], bools: [0, 0, 0], vals: [0, k, d, spd === undefined ? 1 : spd] });
}
function applyLogs(logs) { return logs.filter(l => l.indexOf('apply ') === 0); }

console.log('== qsxz_patch.js 离线回归 ==');

// 0. 补丁资产不得含百分号/井号（历史坑：loadUrl 报 javascript: 时被 URL 解析截断）
ok(SRC.indexOf('%') < 0, '源码不含百分号');
ok(SRC.indexOf('#') < 0, '源码不含井号');

// 1. 桥未就绪 → 不占守卫，直接返回
{
    const r = makeCtx(null);
    delete r.ctx.window.StarryH5;
    load(r.ctx);
    ok(r.ctx.window.__qsxz_v1 === undefined, '无 JS 桥时不占守卫（等下一轮注入）');
}

// 2. 主 JS 未就绪（__QSW_APPLY 还没挂）→ 不占守卫
{
    const r = makeCtx(null);
    delete r.ctx.window.__QSW_APPLY;
    load(r.ctx);
    ok(r.ctx.window.__qsxz_v1 === undefined, '主 JS 未就绪时不占守卫');
}

// 3. 未授权（live=0）→ 必须回落原版（k=1, dr=0），且忽略菜单里的值
{
    const r = makeCtx(st(0, 5, 50));
    load(r.ctx);
    eq(r.ctx.window.__qsxz_v1, 1, '桥+主 JS 就绪后装守卫');
    eq(j(r.applied[0]), '[1,0,1]', 'live=0 回落原版，忽略菜单值');
}

// 4. 已授权 → 透传菜单值
{
    const r = makeCtx(st(1, 5, 50));
    load(r.ctx);
    eq(j(r.applied[0]), '[5,50,1]', 'live=1 透传 (5,50)');
    ok(applyLogs(r.logs).length === 1, '首次投值写一条 apply 日志（可对账）');
    ok(r.logs.some(l => l.indexOf('ack:') === 0), '装好后 ack（守护降频）');
}

// 5. 越界钳制（后台配错值也不能放出超范围口径）
{
    const a = makeCtx(st(1, 999, 999)); load(a.ctx);
    eq(j(a.applied[0]), '[5,50,1]', '超上限钳到 (5,50)');
    const b = makeCtx(st(1, 0, -5)); load(b.ctx);
    eq(j(b.applied[0]), '[1,0,1]', '低于下限钳到 (1,0)');
}

// 6. 同一状态重复注入：值不抖、日志不刷屏（一次性装点也不重复）
{
    const r = makeCtx(st(1, 5, 30));
    load(r.ctx);
    const n1 = applyLogs(r.logs).length;
    load(r.ctx); load(r.ctx); load(r.ctx);
    eq(j(r.applied[r.applied.length - 1]), '[5,30,1]', '重复注入投的仍是同一组值（幂等）');
    eq(applyLogs(r.logs).length, n1, '重复注入不重复刷 apply 日志');
}

// 7. ★ 本次修复回归：授权/取值变化后，Java 守护再次注入必须把新值投进去
//    （旧实现只靠页面内 setInterval 轮询，页面被菜单浮层盖住时定时器被节流 → 投不进去）
{
    const r = makeCtx(st(0, 5, 50));        // 未授权：原版
    load(r.ctx);
    eq(j(r.applied[0]), '[1,0,1]', '授权前 = 原版');
    r.box.state = st(1, 5, 50);             // 模拟：登录成功 + 滑块拉到上限 5 / 50
    load(r.ctx);                            // 模拟：守护下一次注入
    eq(j(r.applied[r.applied.length - 1]), '[5,50,1]', '状态变化后再次注入即生效（不依赖页面定时器）');
    ok(r.logs.some(l => l.indexOf('apply k=5 dr=50 spd=1 live=1') === 0), '变化后有 apply 日志可对账');
}

// 8. 授权失效（过期/后台停用）→ 下一次注入必须回落原版
{
    const r = makeCtx(st(1, 5, 50));
    load(r.ctx);
    r.box.state = st(0, 5, 50);
    load(r.ctx);
    eq(j(r.applied[r.applied.length - 1]), '[1,0,1]', '授权失效后回落原版');
}

// 9. 页面重载（新 window，全局被清）→ 重新投值
{
    const r = makeCtx(st(1, 5, 40));
    load(r.ctx);
    // 模拟重载：只换 window，沙盒里补丁的 window 引用随之更新
    const r2 = makeCtx(st(1, 5, 40));
    load(r2.ctx);
    eq(j(r2.applied[0]), '[5,40,1]', '页面重载后重新投值');
}

// 10. getState 抛异常 → 不崩、不生效
{
    const r = makeCtx(null);
    r.ctx.window.StarryH5.getState = () => { throw new Error('boom'); };
    load(r.ctx);
    ok(r.ctx.window.__qsxz_v1 === 1, 'getState 异常时补丁仍活着（不抛到游戏）');
}

// 11. ★ 战斗加速（v2 新增）：透传滑块值 / 钳制 / 未授权回落 / 缺槽不炸
{
    const r = makeCtx(st(1, 5, 50, 3));
    load(r.ctx);
    eq(j(r.applied[0]), '[5,50,3]', 'live=1 透传战斗加速 3（上限）');
    r.box.state = st(1, 5, 50, 2);
    load(r.ctx);
    eq(j(r.applied[r.applied.length - 1]), '[5,50,2]', '战斗中改滑块（3→2）再次注入即生效');
    ok(r.logs.some(l => l.indexOf('apply k=5 dr=50 spd=2 live=1') === 0), '速度变化有 apply 日志可对账');
    r.box.state = st(1, 5, 50, 5);   // 2026-10-10：上限 10 → 3（实测 5、10 服务端 9998 拒判）
    load(r.ctx);
    eq(j(r.applied[r.applied.length - 1]), '[5,50,3]', '滑块 5 → 钳到上限 3');
}
{
    const a = makeCtx(st(1, 5, 50, 99)); load(a.ctx);
    eq(j(a.applied[0]), '[5,50,3]', '战斗加速超上限钳到 3');
    const b = makeCtx(st(1, 5, 50, -3)); load(b.ctx);
    eq(j(b.applied[0]), '[5,50,1]', '战斗加速低于下限钳到 1（原版）');
}
{
    const r = makeCtx(st(0, 5, 50, 8)); load(r.ctx);
    eq(j(r.applied[0]), '[1,0,1]', '未授权时战斗加速一并回落原版（1 = 不干预）');
}
{
    const r = makeCtx(st(1, 5, 50)); load(r.ctx);
    eq(j(r.applied[0]), '[5,50,1]', '状态里没有第 4 槽（旧菜单/半升级态）→ 速度保持原版，不炸');
}

console.log('\n结果: PASS=' + pass + ' FAIL=' + fail);
process.exit(fail === 0 ? 0 : 1);
