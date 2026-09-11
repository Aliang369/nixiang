// ================================================================
//  ysjyz_js_regression.js — 异世界勇者 JS 补丁块行为回归
//
//  用法: node 04_scripts/异世界勇者/ysjyz_js_regression.js [YsjYzMod.cpp 路径]
//
//  做的是"纯离线行为回归"：mock 掉 cc / jsb / BattleHero，把补丁块 eval 进来，
//  逐项断言 6 个功能的开/关行为、上报折回、Dam 限幅、怪物侧不受影响，
//  以及**已删除功能确实不再生效**（负向断言）。
//  覆盖的 y 行号对应 06_dumped/异世界勇者/jsc_decrypted/main.index.js。
//
//  ⚠️ 这只证明"补丁逻辑正确"，不证明"游戏里生效"——后者只能由真机实测确认。
//
//  菜单（数组下标 = featNum）：
//    0 Category_基础功能（唯一分类，用户要求不做多分类）
//    1 Toggle_免广告   2 Toggle_无限技能(零CD+不耗蓝)   3 Toggle_无敌
//    4 Toggle_吸血拉满 5 SeekBar_伤害倍率秒杀          6 SeekBar_游戏倍速
// ================================================================

global.window = global;   // 补丁块按 Cocos/JSB 环境写，这里把 window 指回 global

const RET = {}, VAL = {};
const calls = { statDam: [], sent: [], realShowAD: 0 };
const uiCalls = [];

// ---- mock: java 桥 ----
global.jsb = { reflection: { callStaticMethod(cls, m, sig, a) {
  if (m === 'get_ret') return !!RET[a];
  if (m === 'get_val') return VAL[a] | 0;
  throw new Error('unknown ' + m);
} } };

// ---- mock: 混入模块（真实游戏里由 Init 的 Extneds 逐成员拷贝到实例）----
const BattleHeroBase = {
  StatDam: function (dmg) { calls.statDam.push(dmg); return dmg; },
  syncAtkSpeedValue: function (v) {
    this.atk.speed += v || 0;
    var t = Math.max(this.atk.speed, .2);
    this.atk.duration.cur = this.atk.duration.origin / t;
    this.atk.hitAt.cur = this.atk.hitAt.origin / t;
    this.atk.cd.cur = this.atk.cd.origin / t;
  },
  GetSkillCost: function (cfg) { return (cfg && cfg.mp) || 10; },
  GetSkillCDDuration: function () { return 8; },
  UpdateSkillCD: function (dt) {
    for (var k in this.skState) { var t = this.skState[k]; if (t && t.Charged < t.MaxCharged) { t.Cd -= dt; } }
  },
};
const BattleHeroSkCost = {};
const BattleHeroBuffs = {
  IsVertigo: function () { return true; },
  IsIce: function () { return true; },
  IsFreeze: function () { return true; },
};
const MODS = { BattleHeroBase, BattleHeroSkCost, BattleHeroBuffs };

const CURRENT = { hero: null };
function BattleHeroCls() {}
BattleHeroCls.prototype.grp = 1;
BattleHeroCls.prototype.Init = function () { this.Extneds(['BattleHeroBase', 'BattleHeroSkCost', 'BattleHeroBuffs']); };
BattleHeroCls.prototype.Extneds = function (list) {
  const self = this;
  list.forEach(function (n) { const m = MODS[n]; for (const k in m) { if (k !== 'Init') self[k] = m[k]; } });
};
// LoadHero = 我方面板重算入口（真实游戏里内部还重挂 node.CheckResult，y13581）
BattleHeroCls.prototype.LoadHero = function () {
  this.cri = 0.1; this.criValue = 2; this.def = 50; this.defOrigin = 50;
  this.free = 0.03; this.withstand = 0.5;
  this.hp = { max: 100, cur: 100 };
  this.atk = this.atk || {}; this.atk.value = 80; this.atk.origin = 80;
  const self = this;
  this.node.CheckResult = function (e) {
    e.Hp = Math.max(self.hp.max, e.Hp);
    e.Atk = Math.max(self.atk.value, e.Atk);
    e.Def = Math.max(self.def, e.Def);
    e.Free = Math.max(self.free, e.Free);
  };
};
// AddHp(e,t,i,c,o) — this = 受击方，末参 o = 攻击方（y30529 判据）
BattleHeroCls.prototype.AddHp = function (e, t, i, c, o) {
  let d = e || 0;
  if (t && this.hp) d += this.hp.max * t;
  if (d < 0 && o && this.grp === 2) { o.StatDam(-d, this.Mid, 0, 0); }
  this.hp.cur += d;
  return d;
};
BattleHeroCls.prototype.Move = function (dt) { this.__moved = (this.__moved || 0) + dt; return dt; };

global.cc = {
  director: { getScheduler: () => ({ schedule: () => {} }) },
  macro: { REPEAT_FOREVER: -1 },
  log: () => {},
  js: { getClassByName: (n) => (n === 'BattleHero' ? BattleHeroCls : null), _getClassById: () => null },
  Service: {
    Battle: { Update(dt) { return dt; } },
    // 以下三个网关已按用户要求"不挂钩"——留作负向断言，证明删除干净
    Store: { GetAdCD() { return 30; }, IsNoNeedAd() { return false; } },
    Skill: { IsAutoDpen() { return false; } },
    GetHero: () => CURRENT.hero,
    Network: { Send(api, data) { calls.sent.push([api, JSON.parse(JSON.stringify(data))]); return true; } },
  },
  UIHelper: {
    ShowAD(cb) { calls.realShowAD++; if (cb) cb(); },
    Call(t, name) { uiCalls.push(name); return 'called'; },
  },
};
global.__require = function (n) { if (!MODS[n]) throw new Error('no module ' + n); return MODS[n]; };

// ---- 自包含：直接从 YsjYzMod.cpp 抽 JS 补丁块（不依赖中间文件）----
const fs = require('fs'), path = require('path');
const CPP = process.argv[2] || path.join(__dirname, '../../mod菜单/starry-mod-menu/app/src/main/jni/Games/YsjYzMod.cpp');
const _m = fs.readFileSync(CPP, 'utf8').match(/R"YSJYZJS\(([\s\S]*?)\)YSJYZJS"/);
if (!_m) { console.error('未能在 ' + CPP + ' 中找到 R"YSJYZJS(...)" 补丁块'); process.exit(2); }
eval(_m[1]);

function hero(grp) {
  const h = new BattleHeroCls();
  h.grp = grp; h.Mid = 7;
  h.node = {};
  h.atk = { speed: 2, value: 80, origin: 80,
            duration: { cur: 1, origin: 1 }, hitAt: { cur: 1, origin: 1 }, cd: { cur: 1, origin: 1 } };
  h.skState = {};
  h.Init();
  h.LoadHero();
  if (grp === 1) CURRENT.hero = h;
  return h;
}
const out = [];
const r4 = (x) => Math.round(x * 10000) / 10000;
function check(name, got, want) { out.push(`${got === want ? 'PASS' : 'FAIL'} ${name}: got=${JSON.stringify(got)} want=${JSON.stringify(want)}`); }
const ap = () => global.__ysjyz_apply();
const clear = () => { for (const k in RET) delete RET[k]; for (const k in VAL) delete VAL[k]; };
function res(extra) { return Object.assign({ Mhp: 1000, Dam: 500, Hp: 0, Atk: 0, Def: 0, Free: 0, Php: 0, Patk: 0, Pdef: 0 }, extra || {}); }

// ================= 功能 1：免广告（featNum 1）=================
RET[1] = false; ap(); calls.realShowAD = 0;
cc.UIHelper.ShowAD(() => {});
check('免广告 OFF → 走原生 ShowAD', calls.realShowAD, 1);
RET[1] = true; ap(); calls.realShowAD = 0;
let adCb = 0;
cc.UIHelper.ShowAD(() => { adCb++; });
check('免广告 ON → 不调用原生 ShowAD', calls.realShowAD, 0);
check('免广告 ON → 直接执行回调(发奖)', adCb, 1);
cc.UIHelper.ShowAD(null);   // 无回调也不能抛
check('免广告 ON → cb 为空不抛异常', calls.realShowAD, 0);
clear(); ap();

// ================= 功能 2：无限技能（零CD + 不耗蓝，合并项，featNum 2）=================
{ const h = hero(1);
  RET[2] = false; ap();
  check('无限技能 OFF → 原 CD', h.GetSkillCDDuration(), 8);
  h.skState = { s1: { Cd: 5, Charged: 0, MaxCharged: 3 } }; h.UpdateSkillCD(0.1);
  check('无限技能 OFF → Cd 正常递减', h.skState.s1.Cd < 5, true);
  check('无限技能 OFF → 原蓝耗', h.GetSkillCost({ mp: 10 }), 10);

  RET[2] = true; ap();
  check('无限技能 ON → CD=0', h.GetSkillCDDuration(), 0);
  h.skState = { s1: { Cd: 5, Charged: 0, MaxCharged: 3 } }; h.UpdateSkillCD(0.1);
  check('无限技能 ON → Cd 清零', h.skState.s1.Cd, 0);
  check('无限技能 ON → Charged 充满', h.skState.s1.Charged, 3);
  check('无限技能 ON → 蓝耗=0', h.GetSkillCost({ mp: 10 }), 0);
  clear(); ap();
}
// 怪物侧不受影响（同一个混入模块，必须按 grp 过滤）
{ const m = hero(2);
  RET[2] = true; ap();
  check('无限技能 → 怪物 CD 不受影响', m.GetSkillCDDuration(), 8);
  check('无限技能 → 怪物蓝耗不受影响', m.GetSkillCost({ mp: 10 }), 10);
  m.skState = { s1: { Cd: 5, Charged: 0, MaxCharged: 3 } }; m.UpdateSkillCD(0.1);
  check('无限技能 → 怪物 Cd 正常递减', m.skState.s1.Cd < 5, true);
  clear(); ap();
}

// ================= 功能 4：无敌 =================
{ const h = hero(1), m = hero(2);
  RET[3] = false; ap();
  let h0 = h.hp.cur; h.AddHp(-30, 0, 0, 0, m);
  check('无敌 OFF → 正常掉血', h0 - h.hp.cur, 30);
  RET[3] = true; ap();
  h0 = h.hp.cur; h.AddHp(-30, 0, 0, 0, m);
  check('无敌 ON → 完全不掉血', h0 - h.hp.cur, 0);
  // 无敌不能顺带保护怪物
  m.hp.cur = 100;
  m.AddHp(-30, 0, 0, 0, h);
  check('无敌 → 不影响怪物受击', m.hp.cur, 70);
  clear(); ap();
}

// ================= 功能 5：吸血拉满 =================
// AddHp 的 this = 受击方、o = 攻击方；我方打怪 → m.AddHp(..., h)
{ const h = hero(1), m = hero(2);
  m.hp = { max: 100000, cur: 100000 };
  RET[4] = false; ap(); h.hp.cur = 50; m.AddHp(-10, 0, 0, 0, h);
  check('吸血 OFF → 不回血', r4(h.hp.cur), 50);
  RET[4] = true; ap(); h.hp.cur = 50; m.AddHp(-10, 0, 0, 0, h);
  check('吸血 ON → 按实伤 100% 回血', r4(h.hp.cur), 60);
  // 怪物打我方：不该给怪物回血
  m.hp.cur = 100000; h.hp.cur = 100;
  h.AddHp(-10, 0, 0, 0, m);
  check('吸血 → 怪物打我时怪物不回血', r4(m.hp.cur), 100000);
  clear(); ap();
}

// ================= 功能 6：伤害倍率秒杀（含 StatDam 上报 1:1 折回）=================
{ const h = hero(1), m = hero(2);
  h.hp = { max: 100000, cur: 100000 };
  m.hp = { max: 100000, cur: 100000 };
  VAL[5] = 50; RET[5] = true; ap();
  m.hp.cur = 100000; calls.statDam.length = 0;
  m.AddHp(-30, 0, 0, 0, h);
  check('伤害倍率 → 实伤 ×50', r4(m.hp.cur), 98500);
  check('伤害倍率 → 上报 Dam 折回原版', r4(calls.statDam[0]), 30);
  // 滑块=1（最小值即"关"）必须透传
  VAL[5] = 1; ap(); m.hp.cur = 100000; calls.statDam.length = 0;
  m.AddHp(-30, 0, 0, 0, h);
  check('倍率=1 → 实伤透传', r4(m.hp.cur), 99970);
  check('倍率=1 → 上报原样', r4(calls.statDam[0]), 30);
  // 越界夹取
  VAL[5] = 0; ap(); m.hp.cur = 100000; m.AddHp(-30, 0, 0, 0, h);
  check('倍率=0 → clamp 到 1（透传）', r4(m.hp.cur), 99970);
  // 怪物打我方：不放大我方受到的伤害
  VAL[5] = 50; ap(); h.hp.cur = 100000; h.AddHp(-30, 0, 0, 0, m);
  check('伤害倍率 → 不放大我方受到的伤害', r4(h.hp.cur), 99970);
  clear(); ap();
}

// ================= 功能 8：游戏倍速 =================
RET[6] = false; ap(); check('游戏倍速 OFF', cc.Service.Battle.Update(1), 1);
VAL[6] = 5; ap(); check('游戏倍速 ×5', cc.Service.Battle.Update(1), 5);
VAL[6] = 0; ap(); check('游戏倍速=0 → clamp 到 1', cc.Service.Battle.Update(1), 1);
clear(); ap();

// ================= chapter/result 出口：Dam 兜底限幅 =================
{ const R = res({ Dam: 500, Mhp: 1000 });
  calls.sent.length = 0;
  cc.Service.Network.Send('chapter/result', R);
  check('Dam 正常值(500 < Mhp×100) → 不动', calls.sent[0][1].Dam, 500);
  check('发完还原对象', R.Dam, 500);

  const R2 = res({ Dam: 9999999, Mhp: 1000 });
  cc.Service.Network.Send('chapter/result', R2);
  check('Dam 病态值 → 封顶 Mhp×100', calls.sent[1][1].Dam, 100000);
  check('限幅后对象已还原(不污染内存态)', R2.Dam, 9999999);
  cc.Service.Network.Send('chapter/result', R2);
  check('二次发送结果一致(不累积)', calls.sent[2][1].Dam, 100000);

  // Dam 是"整章累计"、Mhp 只是"当前怪" → 长章节的合法高倍率不能被压低
  const R3 = res({ Dam: 80000, Mhp: 1000 });
  cc.Service.Network.Send('chapter/result', R3);
  check('长章节 80× 战报 → 不误伤(宽松兜底)', calls.sent[3][1].Dam, 80000);

  const R4 = res({ Dam: 9999999, Mhp: 0 });
  cc.Service.Network.Send('chapter/result', R4);
  check('Mhp=0 → 不猜，不限幅', calls.sent[4][1].Dam, 9999999);

  const R5 = res({ Php: 1000, Patk: 80, Pdef: 500 });
  cc.Service.Network.Send('chapter/battle', R5);
  check('其它接口 → 完全不碰', calls.sent[5][1].Php, 1000);
}

// ================= 上报字段与官包一致（本包无"改面板"功能）=================
{ const h = hero(1), m = hero(2);
  RET[3] = true; RET[4] = true; VAL[5] = 50; VAL[6] = 3; ap();
  h.LoadHero(); m.LoadHero();
  const e = res(); h.node.CheckResult(e);
  check('我方 Hp 上报 = 面板原值', e.Hp, 100);
  check('我方 Def 上报 = 面板原值', e.Def, 50);
  check('我方 Atk 上报 = 面板原值', e.Atk, 80);
  check('我方 Free 上报 = 面板原值', e.Free, 0.03);
  check('面板 hp.max 未被任何功能改动', h.hp.max, 100);
  check('面板 def 未被任何功能改动', h.def, 50);
  check('面板 cri 未被改动', h.cri, 0.1);
  check('面板 criValue 未被改动', h.criValue, 2);
  check('面板 free 未被改动', h.free, 0.03);
  check('面板 withstand 未被改动', h.withstand, 0.5);
  const e2 = res(); m.node.CheckResult(e2);
  check('怪物侧 CheckResult 不被包装', e2.Hp, 100);
  clear(); ap();
}

// ================= 负向断言：已删除的功能确实不再生效 =================
{ const h = hero(1), m = hero(2);
  // 只打开"保留下来的"功能，其余网关应全部保持原样
  RET[1] = true; RET[2] = true; RET[3] = true; RET[4] = true; VAL[5] = 50; VAL[6] = 3; ap();

  check('已删除 · 广告CD清零 → GetAdCD 仍为 30', cc.Service.Store.GetAdCD(), 30);
  check('已删除 · 广告特权一键领取 → IsNoNeedAd 仍 false', cc.Service.Store.IsNoNeedAd(), false);
  check('已删除 · 自动战斗自动技能 → IsAutoDpen 仍 false', cc.Service.Skill.IsAutoDpen(), false);

  uiCalls.length = 0;
  cc.UIHelper.Call(null, 'ShowSystemNotice', 1);
  cc.UIHelper.Call(null, 'ShowMessage', 2);
  check('已删除 · 去公告弹窗 → 两个 Call 都放行', uiCalls.join(','), 'ShowSystemNotice,ShowMessage');

  check('已删除 · 免疫控制 → IsVertigo 仍 true', h.IsVertigo(), true);
  check('已删除 · 免疫控制 → IsIce 仍 true', h.IsIce(), true);
  check('已删除 · 免疫控制 → IsFreeze 仍 true', h.IsFreeze(), true);

  h.__moved = 0; h.Move(1);
  check('已删除 · 移速倍率 → Move 不被加速', h.__moved, 1);

  h.atk.speed = 2; h.syncAtkSpeedValue(0);
  check('已删除 · 攻速倍率 → duration.cur 按原逻辑', r4(h.atk.duration.cur), 0.5);
  check('已删除 · 攻速倍率 → cd.origin 未被改', h.atk.cd.origin, 1);

  // 减伤百分比：我方受击按原值
  h.hp.cur = 1000; h.AddHp(-40, 0, 0, 0, m);
  check('已删除 · 减伤百分比 → 不减免（但无敌仍生效）', r4(h.hp.cur), 1000);
  RET[3] = false; ap(); h.hp.cur = 1000; h.AddHp(-40, 0, 0, 0, m);
  check('无敌关掉后 → 受击按原值（无减伤残留）', r4(h.hp.cur), 960);

  // 倍血防 / 必闪避 / 必中暴击：面板三件套全不该被动
  h.LoadHero();
  check('已删除 · 倍血防 → hp.max 原版', h.hp.max, 100);
  check('已删除 · 倍血防 → hp.cur 原版', h.hp.cur, 100);
  check('已删除 · 倍血防 → def 原版', h.def, 50);
  check('已删除 · 必闪避 → free 原版', h.free, 0.03);
  check('已删除 · 必中暴击 → cri 原版', h.cri, 0.1);
  check('已删除 · 必中暴击 → criValue 原版', h.criValue, 2);
  clear(); ap();
}

const fails = out.filter((l) => l.startsWith('FAIL'));
console.log(out.join('\n'));
console.log(`\n== ${out.length - fails.length}/${out.length} PASS ==`);
if (fails.length) process.exit(1);
