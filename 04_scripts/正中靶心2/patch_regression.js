const fs = require('fs');
const vm = require('vm');
const assert = require('assert');
const path = require('path');

const patch = fs.readFileSync(path.join(__dirname, 'patch_main.js'), 'utf8');
const flags = [false, true, true, false, true];
const values = [0, 0, 0, 2, 0];
let scheduledTick = null;
let adOriginalCalls = 0;
let finishCalls = 0;

function TypeNumber(value) {
  this.value = value;
}
TypeNumber.zero = new TypeNumber(0);
TypeNumber.prototype.mulInt = function (n) { return new TypeNumber(this.value * n); };
TypeNumber.prototype.GTInt = function (n) { return this.value > n; };
TypeNumber.prototype.LEInt = function (n) { return this.value <= n; };
TypeNumber.prototype.add = function (other) { return new TypeNumber(this.value + other.value); };
TypeNumber.prototype.sub = function (other) { return new TypeNumber(this.value - other.value); };

function BaseObjectComponent() {}
BaseObjectComponent.prototype.getBuffCanBeAttack = function (dmg) { return dmg; };
BaseObjectComponent.prototype.changeHp = function (amount, damageType) {
  const applied = amount.value > this.hp.value ? this.hp.value : amount.value;
  this.hp = this.hp.sub(new TypeNumber(applied));
  return { realityNum: new TypeNumber(applied), showNum: amount };
};

function AdvertMgr() {}
AdvertMgr.prototype.adsPlay = function () {
  adOriginalCalls += 1;
  return Promise.resolve({ succ: true, original: true });
};
AdvertMgr.prototype.startAdverView = function () {};
AdvertMgr.prototype.finishAdverViewed = function () {
  finishCalls += 1;
  return Promise.resolve();
};

const context = {
  console,
  Promise,
  setInterval,
  clearInterval,
  jsb: {
    reflection: {
      callStaticMethod: (_cls, method, _sig, n) => {
        if (method === 'getRet') return !!flags[n];
        if (method === 'getVal') return values[n] || 0;
        throw new Error('unexpected bridge method');
      }
    }
  },
  cc: {
    director: {
      getScheduler: () => ({
        schedule: (fn) => { scheduledTick = fn; }
      })
    },
    macro: { REPEAT_FOREVER: -1 },
    log: () => {}
  },
  Battle: {
    BaseObjectComponent,
    EntitySide: { Own: 0, Enemy: 1 },
    DamageType: { damage: 0, bulletDamage: 3 },
    GameWorld: { inst: { gameSpeed: 1 } }
  },
  Reward: { AdvertMgr }
};
context.window = context;
vm.createContext(context);
vm.runInContext(patch, context);
assert(scheduledTick, 'patch should schedule its tick');

const enemy = new BaseObjectComponent();
enemy.entity = { side: 1 };
enemy.hp = new TypeNumber(100);
enemy.changeHp(new TypeNumber(1), 0, true);
assert.strictEqual(enemy.hp.value, 0, '秒杀应把敌方一击伤害提升到当前 HP');

const own = new BaseObjectComponent();
own.entity = { side: 0 };
own.hp = new TypeNumber(100);
own.changeHp(new TypeNumber(60), 0, true);
assert.strictEqual(own.hp.value, 100, '无敌应保持本方 HP 不变');

assert.strictEqual(context.Battle.GameWorld.inst.gameSpeed, 2, '战斗加速应为 2x');
values[3] = 5;
scheduledTick();
assert.strictEqual(context.Battle.GameWorld.inst.gameSpeed, 5, '滑块应为 5x');
values[3] = 1;
scheduledTick();
assert.strictEqual(context.Battle.GameWorld.inst.gameSpeed, 1, '滑块为 1 时应恢复原速度');

const goodInst = context.Battle.GameWorld.inst;
context.Battle.GameWorld.inst = {
  get gameSpeed() { throw new Error('gameSpeed getter test'); },
  set gameSpeed(_v) { throw new Error('gameSpeed setter test'); }
};
assert.doesNotThrow(() => scheduledTick(), '战斗代码抛错时 tick 应兜底不向外抛');
context.Battle.GameWorld.inst = goodInst;

const ad = new AdvertMgr();
(async () => {
  const result = await ad.adsPlay(7, null, 9);
  assert.strictEqual(result.succ, true);
  assert.strictEqual(adOriginalCalls, 0, '免广告开启时不应进入原生广告路径');
  assert.strictEqual(finishCalls, 1, '免广告应调用官方完成链');

  const origFinish = AdvertMgr.prototype.finishAdverViewed;
  AdvertMgr.prototype.finishAdverViewed = () => Promise.reject(new Error('finish reject test'));
  const failResult = await new AdvertMgr().adsPlay(7, null, 9);
  AdvertMgr.prototype.finishAdverViewed = origFinish;
  assert.strictEqual(failResult.succ, false, '广告完成链拒绝时不应向游戏抛异常');
  console.log('patch_regression: PASS');
})().catch((e) => { console.error(e); process.exit(1); });
