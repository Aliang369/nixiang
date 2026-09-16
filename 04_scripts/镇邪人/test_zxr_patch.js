#!/usr/bin/env node
"use strict";

const fs = require("fs");
const path = require("path");

const specPath = process.argv[2] || path.join(__dirname, "zxr_patch_spec.json");
const spec = JSON.parse(fs.readFileSync(specPath, "utf8"));
const byName = Object.fromEntries(spec.replacements.map((r) => [r.name, r]));
const state = { ret: {} };
const window = { __starryZxrState: state, __starryZxrOn: (i) => !!state.ret[i] };

function assert(cond, msg) {
  if (!cond) throw new Error(msg);
}

const damage = byName.damage_and_invincible;
const damageFn = new Function("window", "n", "r", "t", "c",
  "t = c.baseDmg;" + damage.insert + ";return t;");
assert(damageFn(window, { heroCfg: { id: 1 } }, { monsterCfg: { id: 2 } }, 10, { baseDmg: 10 }) === 10,
  "damage must be unchanged when feature 1 is off");
state.ret[1] = true;
assert(damageFn(window, { heroCfg: { id: 1 } }, { monsterCfg: { id: 2 } }, 10, { baseDmg: 10 }) === 1000,
  "feature 1 must multiply hero damage by 100");
state.ret[1] = false;
state.ret[2] = true;
assert(damageFn(window, { monsterCfg: { id: 2 } }, { heroCfg: { id: 1 } }, 10, { baseDmg: 10 }) === 0,
  "feature 2 must zero damage received by hero");

const speed = byName.attack_speed;
const speedFn = new Function("window", "l", "t", "e", "i",
  speed.insert + ";return Math.max(e, .01);");
state.ret[2] = false;
state.ret[3] = true;
assert(speedFn(window, { EnumEntityStatus: { bossFakeControl: 1 } }, { heroCfg: { id: 1 }, CheckEntityHaveStatus: () => false }, 1, 3) === 3,
  "feature 3 must raise hero attack speed to the game cap");
assert(speedFn(window, { EnumEntityStatus: { bossFakeControl: 1 } }, { monsterCfg: { id: 2 } }, 1, 3) === 1,
  "feature 3 must not affect monsters");

const aoe = byName.full_screen_kill;
const enemies = [
  { EntityID: 1, monsterCfg: { id: 1 }, IsDead: false, IsLogicDead: false },
  { EntityID: 2, monsterCfg: { id: 2 }, IsDead: false, IsLogicDead: false },
  { EntityID: 3, heroCfg: { id: 1 }, IsDead: false, IsLogicDead: false },
];
const killed = [];
const host = {
  FightForceKillEntity(entity, caster) {
    killed.push([entity.EntityID, caster]);
  },
};
const manager = {
  GetEntity(id) {
    return id === 7 ? { heroCfg: { id: 1 } } : null;
  },
  entityMap: new Map(enemies.map((e) => [e.EntityID, e])),
};
const d = { EntityManager: { Inst: () => manager } };
const s = { EnumEntityType: { Player: 99 } };
const aoeFn = new Function("window", "d", "s", "t", "e", "i", aoe.insert);
aoeFn.call(host, window, d, s, enemies[0], 7, -10);
assert(killed.length === 0, "feature 4 must be disabled by default");
state.ret[4] = true;
aoeFn.call(host, window, d, s, enemies[0], 7, -10);
assert(JSON.stringify(killed) === JSON.stringify([[2, 7]]),
  "feature 4 must force-kill only other live monsters");
assert(host.__starryZxrAoeBusy === false, "feature 4 guard must always reset");

console.log("zxr_patch_regression=pass cases=7");
