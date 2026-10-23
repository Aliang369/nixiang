#!/usr/bin/env node
/**
 * 离线单测：战斗加速（真·全局变速）层。2026-10-24 第二版
 *
 * 背景（用户实测反馈）：「战斗加速没效果，不是全局加速；我要的是类似变速器那样对全局都加速」。
 * 所以主路线改成 **挂引擎的 calculateDeltaTime，把每帧 _deltaTime ×k**
 * （cocos2d-jsb 1.1.8.1 src/cocos2d-jsb.js@6406：calculateDeltaTime → _deltaTime →
 *  compScheduler.updatePhase / _scheduler.update / ActionManager / 物理 / 组件 update），
 * 一处放大即全局生效。worldTimeScale 与 scheduler.setTimeScale 降为兜底，且与 dt 路线互斥。
 *
 * 本测试证明：
 *   1) dt 路线：k=3 时同一墙钟间隔产出 3 倍 _deltaTime；k=1 时退回原值；
 *   2) 互斥：dt 路线生效时不再写 worldTimeScale，也不把调度器抬到 k；
 *   3) 结算账：clampArmed 随 speedMul>1 自动武装，useTime 按自己倍率折回；
 *   4) 无 cc 时兜底不炸（返回 false，不影响其它功能）。
 *
 * 用法：node 04_scripts/勇士之心/ysxz_anticheat/test_speed_layer.js
 */
"use strict";
const fs = require("fs");
const path = require("path");
const vm = require("vm");

const src = fs.readFileSync(path.join(__dirname, "ac_layer.js"), "utf8");

let pass = 0;
const fails = [];
function ok(cond, name, extra) {
  if (cond) { pass++; return; }
  fails.push(name + (extra ? "  <" + extra + ">" : ""));
}

/** 造沙箱：可选 cc.director（带 calculateDeltaTime 的假引擎）+ $eZ + MOD */
function boot(opts) {
  const win = {};
  win.window = win;
  win.console = { log() {}, warn() {}, error() {} };
  win.MOD = { bspeed: opts.bspeed };

  let nowMs = 1000;
  const sched = { clock: 1, setTimeScale(v) { this.clock = v; } };
  if (opts.withEngine !== false) {
    win.cc = {
      director: {
        _deltaTime: 0,
        _lastUpdate: nowMs,
        getScheduler: () => sched,
        calculateDeltaTime: function (t) {
          const now = (t === undefined) ? nowMs : t;
          this._deltaTime = now > this._lastUpdate ? (now - this._lastUpdate) / 1e3 : 0;
          this._lastUpdate = now;
        },
      },
    };
  }
  win.$eZ = opts.ez || null;
  const ctx = vm.createContext({
    window: win, cc: win.cc, console: win.console, performance: { now: () => nowMs },
    setTimeout: () => 0, JSON, Math, Date, Object, Function,
  });
  vm.runInContext(src, ctx, { filename: "ac_layer.js" });
  return {
    win, sched, AC: win.__starry_ac,
    tickEngine(advanceMs) { nowMs += advanceMs; win.cc.director.calculateDeltaTime(); return win.cc.director._deltaTime; },
  };
}

// ---- 1. dt 路线：k=3 时一帧 16ms 变成 48ms -------------------------------------
{
  const h = boot({ bspeed: 3, ez: { battleMgr: null } });
  h.AC.applySpeed();
  ok(h.AC.dtScaled === true, "1a calculateDeltaTime 已被包装(dtScaled)", "dtScaled=" + h.AC.dtScaled);
  const dt = h.tickEngine(16);
  ok(Math.abs(dt - 0.048) < 1e-6, "1b 16ms 墙钟 → 48ms 逻辑 dt", "got=" + dt);
  ok(h.sched.clock === 1, "1c dt 路线下调度器保持 1（不叠乘）", "sched=" + h.sched.clock);
}

// ---- 2. k=1：dt 恢复原值 -------------------------------------------------------
{
  const h = boot({ bspeed: 1, ez: { battleMgr: null } });
  h.AC.applySpeed();
  const dt = h.tickEngine(16);
  ok(Math.abs(dt - 0.016) < 1e-6, "2a 1x 时 dt 不放大", "got=" + dt);
  ok(h.AC.clampArmed === false, "2b 1x 时不武装 useTime 补偿");
}

// ---- 3. 互斥：dt 路线 + 战斗 world 同时存在时，worldTimeScale 不动 -----------------
{
  const world = { worldTimeScale: 1 };
  const h = boot({ bspeed: 4, ez: { battleMgr: { ctrlX: { world: world } } } });
  h.AC.applySpeed();
  ok(h.AC.dtScaled === true, "3a dt 路线优先");
  ok(world.worldTimeScale === 1, "3b dt 路线生效时不再写 worldTimeScale（防 k²）", "got=" + world.worldTimeScale);
}

// ---- 4. 兜底：引擎没有 calculateDeltaTime 时退回 worldTimeScale -------------------
{
  const world = { worldTimeScale: 1 };
  const win = {};
  win.window = win;
  win.console = { log() {}, warn() {}, error() {} };
  win.MOD = { bspeed: 5 };
  win.cc = { director: { getScheduler: () => ({ clock: 1, setTimeScale(v) { this.clock = v; } }) } };
  win.$eZ = { battleMgr: { ctrlY: { world: world } } };
  const ctx = vm.createContext({ window: win, cc: win.cc, console: win.console, setTimeout: () => 0, JSON, Math, Date, Object, Function });
  vm.runInContext(src, ctx, { filename: "ac_layer.js" });
  const AC = win.__starry_ac;
  AC.applySpeed();
  ok(AC.dtScaled === false, "4a 没有 calculateDeltaTime 时不误报已装");
  ok(world.worldTimeScale === 5, "4b 兜底走 worldTimeScale", "got=" + world.worldTimeScale + " path=" + AC.speedPath);
}

// ---- 5. 结算账：加速 5x 时 useTime 折回墙钟量级 ----------------------------------
{
  const h = boot({ bspeed: 5, ez: { battleMgr: null } });
  h.AC.applySpeed();
  h.AC.wallMs = 10000;
  const payload = { completeResult: { useTime: 50000 } };
  h.AC.onReport(payload, "test");
  ok(Math.abs(payload.completeResult.useTime - 10000) <= 1500, "5a useTime 50000 -> ~10000", "got=" + payload.completeResult.useTime);
  ok(h.AC.clamped === 1, "5b 记一次补偿");
}

// ---- 6. 1x 时结算原样保留（不制造新异常） ---------------------------------------
{
  const h = boot({ bspeed: 1, ez: { battleMgr: null } });
  h.AC.applySpeed();
  h.AC.wallMs = 10000;
  const payload = { completeResult: { useTime: 12345 } };
  h.AC.onReport(payload, "test");
  ok(payload.completeResult.useTime === 12345, "6a 1x 时 useTime 原样", "got=" + payload.completeResult.useTime);
}

console.log("战斗加速(dt 全局变速) 离线单测：pass=" + pass + " fail=" + fails.length);
for (const f of fails) console.log("  FAIL  " + f);
process.exit(fails.length ? 1 : 0);
