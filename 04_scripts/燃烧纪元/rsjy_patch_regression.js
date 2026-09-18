#!/usr/bin/env node
/* 燃烧纪元核心四功能离线回归。 */
const fs = require('fs');
const vm = require('vm');
const path = require('path');

const patch = fs.readFileSync(path.join(__dirname, 'patch_rsjy.js'), 'utf8');
let pass = 0, fail = 0;
function ok(name, cond) {
  if (cond) { pass++; console.log('PASS', name); }
  else { fail++; console.log('FAIL', name); }
}

function build(state) {
  const store = { ...state };
  function Unit() {}
  Unit.prototype.changeHp = function (info) { this.lastDelta = info.delta; return info.delta; };
  function Panel() {}
  Panel.prototype.getBattleSpeed = function () { return 1; };
  function AdHelper() {}
  AdHelper.prototype.showVideoAd = function (cb) { this.originalCalled = true; if (cb) cb(1); };

  const modules = {
    Unit: { default: Unit },
    ChapterFbBattlePanel: { default: Panel },
    OnHookBossBattlePanel: { default: Panel },
    OnHookSettingModel: { default: Panel },
    OutlandBattlePanel: { default: Panel },
    PveBattlePanel: { default: Panel },
    AdHelper: { AdHelper, AdStatus: { Reward: 2 } },
  };

  const ctx = {
    console,
    setTimeout: (fn) => { fn(); return 1; },
    setInterval: () => 1,
    cc: {
      log: () => {},
      macro: { REPEAT_FOREVER: -1 },
      director: { getScheduler: () => ({ schedule: () => 1 }) },
    },
    jsb: { reflection: { callStaticMethod: (_cls, method, _sig, n) => {
      if (method === 'getRet') return !!store.ret[n];
      if (method === 'getVal') return store.val[n] || 0;
      return 0;
    } } },
  };
  ctx.window = ctx;
  ctx.globalThis = ctx;
  ctx.__require = (name) => modules[name] || null;
  vm.createContext(ctx);
  vm.runInContext(patch, ctx, { timeout: 5000 });
  return { ctx, Unit, Panel, AdHelper };
}

{
  const { Unit } = build({ ret: {}, val: { 1: 5 } });
  const u = new Unit(); u.group = 1;
  Unit.prototype.changeHp.call(u, { delta: -10 }, { group: 0 }, 1, 1);
  ok('攻击倍率: 敌方受伤 ×5', u.lastDelta === -50);
}
{
  const { Unit } = build({ ret: { 2: true }, val: { 1: 5 } });
  const u = new Unit(); u.group = 0;
  Unit.prototype.changeHp.call(u, { delta: -10 }, { group: 1 }, 1, 1);
  ok('上帝模式: 我方负 delta 清零', u.lastDelta === 0);
}
{
  const { Panel } = build({ ret: {}, val: { 3: 7 } });
  const p = new Panel();
  ok('战斗速度: 返回 7', p.getBattleSpeed() === 7);
}
{
  const { AdHelper } = build({ ret: { 4: true }, val: {} });
  const a = new AdHelper();
  let reward = null;
  a.showVideoAd((v) => { reward = v; });
  ok('免广告: 直接回调 Reward=2', reward === 2 && !a.originalCalled);
}
{
  const { AdHelper } = build({ ret: {}, val: {} });
  const a = new AdHelper();
  a.showVideoAd(() => {});
  ok('免广告关闭: 保留原广告路径', a.originalCalled === true);
}

console.log(`RESULT pass=${pass} fail=${fail}`);
process.exit(fail ? 1 : 0);
