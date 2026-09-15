#!/usr/bin/env node
'use strict';

const fs = require('fs');
const path = require('path');
const vm = require('vm');
const assert = require('assert');

const ROOT = '/Users/aliang/Documents/nixiang';
const PATCH = path.join(ROOT, 'mod菜单/starry-mod-menu/app/src/main/jni/Games/HalyPatch.js');
const code = fs.readFileSync(PATCH, 'utf8');
assert(!code.includes('__starryHalyInstalled'), 'v1 one-shot guard must not return');

let intervalTick = null;
const context = {
  console: { log() {} },
  Promise,
  Date,
  Array,
  Object,
  Number,
  String,
  Math,
  setInterval(fn) {
    intervalTick = fn;
    return 1;
  },
  clearInterval() {},
  System: {
    values: new Map(),
    get(key) {
      return this.values.get(key) || null;
    }
  }
};
vm.createContext(context);
vm.runInContext(code, context, { filename: PATCH });

function register(name, exports) {
  context.System.values.set(name, exports);
  context.System.values.set(`chunks:///_virtual/${name}.ts`, exports);
}

function makeComponentCtor(name) {
  return class Component {
    constructor() {
      this.name = name;
    }
  };
}

const HealthComponent = makeComponentCtor('Health');
const SkillBookComponent = makeComponentCtor('SkillBook');
const StatusEffectType = { INVINCIBLE: 4 };
const StatusEffectComponent = makeComponentCtor('StatusEffect');
StatusEffectComponent.prototype.addEffect = function (id, type, duration, value, sourceId) {
  this.effects.push({ id, type, duration, timeLeft: duration, value, sourceId });
};

const events = [];
const playerStatus = new StatusEffectComponent();
playerStatus.effects = [];
const player = {
  id: 1,
  components: new Map(),
  getComponent(C) {
    return this.components.get(C) || null;
  },
  getOrCreateComponent(C) {
    if (!this.components.has(C)) this.components.set(C, new C());
    return this.components.get(C);
  }
};
player.components.set(HealthComponent, { isAlive: () => true });
player.components.set(SkillBookComponent, { basicSkillId: 77 });
player.components.set(StatusEffectComponent, playerStatus);
const target = {
  id: 2,
  getComponent(C) {
    return C === HealthComponent ? { isAlive: () => true } : null;
  }
};
const entitySystem = {
  scene: {
    playerEntity: player,
    findEntityById(id) {
      return id === 1 ? player : id === 2 ? target : null;
    },
    eventSystem: {
      emitSync(name, payload) {
        events.push({ name, payload });
      }
    }
  }
};

class OptionalTableCtor {}

register('PlayerPrefService', {
  PlayerPrefService: class {
    hasAdPrivilege() { return false; }
    canSkipAd() { return false; }
  }
});
register('AdSkipService', {
  AdSkipService: class {
    hasAdPrivilege() { return false; }
    canSkipAd() { return false; }
    tryShowSkipPopup() { return Promise.resolve(false); }
  }
});
register('SdkRewardAdGuard', {
  SdkRewardAdGuard: class {
    static showRewardAd() { return Promise.resolve(false); }
  }
});
register('SdkService', {
  SdkService: class {
    showRewardAd() { return Promise.resolve({ rewarded: false }); }
  }
});
register('SkillConfigManager', {
  SkillConfigManager: class {
    static instance = new this();
    constructor() { this.loaded = []; }
    parseSkillData(cfg) { return cfg; }
    getSkillDataSync(id) { return null; }
    preloadSkill(id) { this.loaded.push(id); }
    clearCache() {}
  }
});
register('SkillSummonSystem', {
  SkillSummonSystem: class {
    async handleSummonRequest(request) {
      this.lastRequest = request;
      return request;
    }
  }
});
register('PassiveSkillTriggerSystem', {
  PassiveSkillTriggerSystem: class {
    constructor() { this.scene = entitySystem.scene; }
    triggerAttackerPassiveSkills() { return 'orig'; }
    findAliveEntity(id) { return entitySystem.scene.findEntityById(id); }
  }
});
register('SkillCastSystem', {
  SkillCastSystem: class {
    executeSkillEffects() { return true; }
  }
});
register('ECSManager', {
  ECSManager: class {
    static instance = entitySystem;
  }
});
register('HealthComponent', { HealthComponent });
register('SkillBookComponent', { SkillBookComponent });
register('StatusEffectComponent', { StatusEffectComponent, StatusEffectType });

context.__starryHalyV2SetFlags(true, true, true, true);

const pref = new (context.System.get('PlayerPrefService').PlayerPrefService)();
const ad = new (context.System.get('AdSkipService').AdSkipService)();
const guard = context.System.get('SdkRewardAdGuard').SdkRewardAdGuard;
const sdk = new (context.System.get('SdkService').SdkService)();

Promise.resolve().then(async () => {
  assert.strictEqual(pref.hasAdPrivilege(), true, 'no-ad privilege query');
  assert.strictEqual(pref.canSkipAd(), true, 'no-ad skip query');
  assert.strictEqual(ad.tryShowSkipPopup().constructor, Promise, 'skip popup remains thenable');
  assert.strictEqual(await ad.tryShowSkipPopup(), true, 'skip popup returns true');
  assert.strictEqual(await guard.showRewardAd(), true, 'reward guard returns boolean true');
  const sdkResult = await sdk.showRewardAd();
  assert.strictEqual(sdkResult.rewarded, true, 'sdk reward result shape');

  assert(playerStatus.effects.some(e => e.id === 1002 && e.type === StatusEffectType.INVINCIBLE), 'invincible applied');

  const summonEvents = events.filter(e => e.name === 'skill_summon_request');
  assert(summonEvents.length >= 1, 'auto summon event emitted');
  const summon = summonEvents[summonEvents.length - 1].payload;
  assert.strictEqual(summon.effect.summon_count, 3, 'summon count');
  assert.strictEqual(summon.effect.duration, 20000, 'summon duration');
  assert.strictEqual(summon.effect.inherit_attack, 100000000, 'summon attack ratio');
  assert.strictEqual(summon.__starryHalyV2, true, 'summon marker');

  const SummonSystem = context.System.get('SkillSummonSystem').SkillSummonSystem;
  const summonSystem = new SummonSystem();
  const forwarded = await summonSystem.handleSummonRequest(summon);
  assert.strictEqual(forwarded.skillId, 0, 'boosted summon bypasses runtime override');
  assert.strictEqual(forwarded.effect.summon_count, 3, 'boosted summon effect preserved');

  const PassiveSystem = context.System.get('PassiveSkillTriggerSystem').PassiveSkillTriggerSystem;
  const passiveSystem = new PassiveSystem();
  const result = passiveSystem.triggerAttackerPassiveSkills({ sourceId: 1, targetId: 2, skillId: 77 });
  assert.strictEqual(result, 'orig', 'original passive result preserved');
  const thunder = events.filter(e => e.name === 'skill_passive_trigger');
  assert.strictEqual(thunder.length, 1, 'one thunder trigger per main-player basic hit');
  assert.strictEqual(thunder[0].payload.skillId, 100903, 'thunder skill id');

  const CastSystem = context.System.get('SkillCastSystem').SkillCastSystem;
  CastSystem.prototype.executeSkillEffects = function () { throw new Error('synthetic battle error'); };
  context.__starryHalyV2Install();
  const cast = new CastSystem();
  assert.strictEqual(cast.executeSkillEffects({}, { id: 100903, skillId: 1009 }), false, 'battle error isolated');

  context.__starryHalyV2SetFlags(false, false, false, false);
  assert.strictEqual(playerStatus.effects.some(e => e.id === 1002), false, 'invincible removed on OFF');
  assert.strictEqual(pref.hasAdPrivilege(), false, 'original ad query restored on OFF');
  assert.strictEqual(await guard.showRewardAd(), false, 'original ad flow restored on OFF');

  assert(typeof intervalTick === 'function', 'retry timer installed');
  console.log('PASS haly patch behavior');
}).catch(error => {
  console.error(error && error.stack ? error.stack : error);
  process.exit(1);
});
