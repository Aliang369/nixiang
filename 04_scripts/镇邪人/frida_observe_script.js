'use strict';

const libName = 'libcocos2djs.so';
const runScriptSym = '_ZN2se12ScriptEngine9runScriptERKNSt6__ndk112basic_stringIcNS_11char_traitsIcEENS_9allocatorIcEEEEPNS_5ValueE';
const evalSym = '_ZN2se12ScriptEngine10evalStringEPKclPNS_5ValueES2_';

function log(msg) {
  console.log('[zxr-observe] ' + msg);
}

function hasBytes(buf, needle) {
  const n = needle.length;
  const a = new Uint8Array(buf);
  const b = needle.split('').map(c => c.charCodeAt(0));
  outer:
  for (let i = 0; i + n <= a.length; i++) {
    for (let j = 0; j < n; j++) if (a[i + j] !== b[j]) continue outer;
    return true;
  }
  return false;
}

function readStdString(p) {
  try {
    const size = p.add(8).readU64().toNumber();
    if (size < 0 || size > 64 * 1024 * 1024) return null;
    const data = size < 256 ? p.add(1) : p.readPointer();
    return data.readByteArray(size);
  } catch (e) {
    return null;
  }
}

const run = Module.findExportByName(libName, runScriptSym);
if (run) {
  log('hook runScript ' + run);
  Interceptor.attach(run, {
    onEnter(args) {
      const buf = readStdString(args[1]);
      if (!buf) return;
      const len = buf.byteLength;
      if (len < 64 * 1024) return;
      const hit = hasBytes(buf, 'starry_zxr_safe_v2');
      const damage = hasBytes(buf, 'DamageCommonCalcFinal');
      const fight = hasBytes(buf, 'FightChangeEntityHp');
      const atk = hasBytes(buf, 'CalcFinalAtkSpeedValue');
      log('runScript len=' + len + ' marker=' + hit + ' damage=' + damage + ' fight=' + fight + ' atk=' + atk);
    }
  });
} else {
  log('runScript export missing');
}

const evalFn = Module.findExportByName(libName, evalSym);
if (evalFn) {
  log('hook evalString ' + evalFn);
  Interceptor.attach(evalFn, {
    onEnter(args) {
      const len = args[2].toInt32();
      if (len < 64 * 1024 || len > 64 * 1024 * 1024) return;
      try {
        const buf = args[1].readByteArray(len);
        const hit = hasBytes(buf, 'starry_zxr_safe_v2');
        const damage = hasBytes(buf, 'DamageCommonCalcFinal');
        const fight = hasBytes(buf, 'FightChangeEntityHp');
        const atk = hasBytes(buf, 'CalcFinalAtkSpeedValue');
        log('evalString len=' + len + ' marker=' + hit + ' damage=' + damage + ' fight=' + fight + ' atk=' + atk);
      } catch (e) {
        log('evalString len=' + len + ' read failed');
      }
    }
  });
} else {
  log('evalString export missing');
}
