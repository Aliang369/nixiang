'use strict';

const MODULE = 'libil2cpp.so';
const RVA = {
  attackDamage: 0x1A733C8,
  attackDamageOnce: 0x1A756B8,
  skillDamage: 0x1A7695C,
  getSuccessWuPing: 0x1A7BF00,
  getSuccessWuPingEnd: 0x1A7EAB0,
  randomRangeInt: 0x35BD284,
  toponShowAd: 0x1A91F08,
  toponIsReady: 0x1A91FA0,
  toponOnReward: 0x1A923EC,
};

function log(message) {
  console.log('[hxwj-probe] ' + message);
}

function hex(value) {
  return value === null || value === undefined ? 'null' : value.toString();
}

function safe(description, fn, fallback) {
  try {
    return fn();
  } catch (e) {
    log(description + ' failed: ' + e);
    return fallback;
  }
}

function readIl2CppString(value) {
  if (value.isNull()) return null;
  const length = value.add(0x10).readS32();
  if (length < 0 || length > 4096) return null;
  return value.add(0x14).readUtf16String(length);
}

function moduleBase() {
  const module = Process.getModuleByName(MODULE);
  return module.base;
}

function inFunction(address, startRva, endRva, base) {
  const rva = address.sub(base).toUInt32();
  return rva >= startRva && rva < endRva;
}

const base = moduleBase();
log('module=' + MODULE + ' base=' + base);
log('prologues attack=' + base.add(RVA.attackDamage).readU32().toString(16)
    + ' attackOnce=' + base.add(RVA.attackDamageOnce).readU32().toString(16)
    + ' skill=' + base.add(RVA.skillDamage).readU32().toString(16)
    + ' drop=' + base.add(RVA.getSuccessWuPing).readU32().toString(16)
    + ' random=' + base.add(RVA.randomRangeInt).readU32().toString(16)
    + ' showAd=' + base.add(RVA.toponShowAd).readU32().toString(16)
    + ' onReward=' + base.add(RVA.toponOnReward).readU32().toString(16));

const counters = {
  attackPlayerToGw: 0,
  attackGwToPlayer: 0,
  skillPlayerToGw: 0,
  skillGwToPlayer: 0,
  dropCalls: 0,
  adShow: 0,
  adReady: 0,
  adReward: 0,
};

function dumpCombatCall(tag, self, target, countersKey) {
  if (self.isNull() || target.isNull()) return;
  const selfType = safe(tag + ' self type', () => self.add(0x64).readS32(), -1);
  const targetType = safe(tag + ' target type', () => target.add(0x64).readS32(), -1);
  if (selfType === 1 && targetType === 2) counters[countersKey + 'PlayerToGw']++;
  if (selfType === 2 && targetType === 1) counters[countersKey + 'GwToPlayer']++;
  if ((selfType === 1 && targetType === 2) || (selfType === 2 && targetType === 1)) {
    const targetInfo = safe(tag + ' target info', () => target.add(0x30).readPointer(), ptr(0));
    const curHp = targetInfo.isNull() ? -1 : safe(tag + ' curHp', () => targetInfo.add(0x80).readS32(), -1);
    log(tag + ' self=' + hex(self) + ' target=' + hex(target)
        + ' selfType=' + selfType + ' targetType=' + targetType + ' targetHp=' + curHp);
  }
}

Interceptor.attach(base.add(RVA.attackDamage), {
  onEnter(args) {
    dumpCombatCall('AttackDamage', args[0], args[1], 'attack');
  },
});

Interceptor.attach(base.add(RVA.attackDamageOnce), {
  onEnter(args) {
    dumpCombatCall('AttackDamageOnce', args[0], args[1], 'attack');
  },
});

Interceptor.attach(base.add(RVA.skillDamage), {
  onEnter(args) {
    dumpCombatCall('SkillDamage', args[0], args[1], 'skill');
  },
});

Interceptor.attach(base.add(RVA.getSuccessWuPing), {
  onEnter(args) {
    counters.dropCalls++;
    const self = args[0];
    const cur = safe('drop cur', () => self.add(0x130).readPointer(), ptr(0));
    const list = cur.isNull() ? ptr(0) : safe('drop list', () => cur.add(0x78).readPointer(), ptr(0));
    const size = list.isNull() ? -1 : safe('drop size', () => list.add(0x18).readS32(), -1);
    const items = list.isNull() ? ptr(0) : safe('drop items', () => list.add(0x10).readPointer(), ptr(0));
    log('GetSuccessWuPing call#' + counters.dropCalls + ' self=' + hex(self)
        + ' cur=' + hex(cur) + ' list=' + hex(list) + ' size=' + size);
    if (!items.isNull() && size > 0 && size < 128) {
      for (let i = 0; i < size; i++) {
        const entry = safe('drop entry ' + i, () => items.add(0x20 + i * 8).readPointer(), ptr(0));
        if (entry.isNull()) continue;
        const type = safe('drop entry type ' + i, () => entry.add(0x10).readS32(), -1);
        const name = safe('drop entry name ' + i, () => readIl2CppString(entry.add(0x18).readPointer()), null);
        const getNum = safe('drop entry getNum ' + i, () => entry.add(0x20).readS32(), -1);
        const count = safe('drop entry count ' + i, () => entry.add(0x24).readS32(), -1);
        log('  drop[' + i + '] entry=' + hex(entry) + ' type=' + type
            + ' name=' + name + ' getNum=' + getNum + ' count=' + count);
      }
    }
  },
});

Interceptor.attach(base.add(RVA.randomRangeInt), {
  onEnter(args) {
    const caller = this.returnAddress;
    this.hxwjDropCall = inFunction(caller, RVA.getSuccessWuPing, RVA.getSuccessWuPingEnd, base);
    if (this.hxwjDropCall) {
      this.hxwjMin = args[0].toInt32();
      this.hxwjMax = args[1].toInt32();
      this.hxwjCallerRva = caller.sub(base);
    }
  },
  onLeave(retval) {
    if (this.hxwjDropCall) {
      log('Random.Range drop caller=' + this.hxwjCallerRva + ' range=[' + this.hxwjMin + ','
          + this.hxwjMax + ') result=' + retval.toInt32());
    }
  },
});

Interceptor.attach(base.add(RVA.toponShowAd), {
  onEnter(args) {
    counters.adShow++;
    const idCsj = safe('showAd csj id', () => readIl2CppString(args[0].add(0x20).readPointer()), null);
    const idTx = safe('showAd tx id', () => readIl2CppString(args[0].add(0x28).readPointer()), null);
    log('ToponVideo.showAd call#' + counters.adShow + ' self=' + hex(args[0])
        + ' csj=' + idCsj + ' tx=' + idTx);
  },
});

Interceptor.attach(base.add(RVA.toponIsReady), {
  onEnter(args) {
    counters.adReady++;
  },
  onLeave(retval) {
    if (counters.adReady <= 20 || counters.adReady % 20 === 0) {
      log('ToponVideo.isReady call#' + counters.adReady + ' result=' + retval.toInt32());
    }
  },
});

Interceptor.attach(base.add(RVA.toponOnReward), {
  onEnter(args) {
    counters.adReward++;
    const placement = safe('onReward placement', () => readIl2CppString(args[2].add(0x10).readPointer()), null);
    log('ToponVideo.onReward call#' + counters.adReward + ' self=' + hex(args[0])
        + ' sender=' + hex(args[1]) + ' placement=' + placement);
  },
});

setInterval(() => {
  log('counters attack[P>G=' + counters.attackPlayerToGw + ',G>P=' + counters.attackGwToPlayer
      + '] skill[P>G=' + counters.skillPlayerToGw + ',G>P=' + counters.skillGwToPlayer
      + '] drop=' + counters.dropCalls + ' ad[show=' + counters.adShow
      + ',ready=' + counters.adReady + ',reward=' + counters.adReward + ']');
}, 5000);
