'use strict';

/*
 * Read-mostly probe for the NetEase nesec SIGILL seen inside the Chongya
 * container.  The first matching illegal instruction is logged, then the
 * current PC is redirected to LR for a single live bypass experiment.
 */

const NESEC_RVA = 0xc0f94;
const NESEC_FALLBACK_RVA = 0xc0fa0;
const MAX_HITS = 4;

let hits = 0;

function toHex(buffer) {
  if (buffer === null) {
    return null;
  }
  const bytes = new Uint8Array(buffer);
  let out = '';
  for (let i = 0; i < bytes.length; i++) {
    out += bytes[i].toString(16).padStart(2, '0');
  }
  return out;
}

function readHex(address, length) {
  try {
    return toHex(ptr(address).readByteArray(length));
  } catch (e) {
    return 'ERR:' + e;
  }
}

function moduleFor(address) {
  try {
    return Process.findModuleByAddress(address);
  } catch (e) {
    return null;
  }
}

function rangeFor(address) {
  try {
    const range = Process.findRangeByAddress(address);
    if (range === null) {
      return null;
    }
    let file = null;
    let offset = null;
    if (range.file !== null && range.file !== undefined) {
      if (typeof range.file === 'string') {
        file = range.file;
      } else {
        file = range.file.path !== undefined ? range.file.path : String(range.file);
        offset = range.file.offset !== undefined ? range.file.offset : null;
      }
    }
    return {
      base: range.base.toString(),
      size: range.size,
      protection: range.protection,
      file,
      offset
    };
  } catch (e) {
    return { error: String(e) };
  }
}

function regsFor(context) {
  const out = {};
  const names = [];
  for (let i = 0; i <= 30; i++) {
    names.push('x' + i);
  }
  names.push('sp', 'pc');
  for (const name of names) {
    try {
      const value = context[name];
      out[name] = value !== undefined ? ptr(value).toString() : null;
    } catch (e) {
      out[name] = 'ERR:' + e;
    }
  }
  return out;
}

function emitReady() {
  const modules = Process.enumerateModules()
    .filter(m => /nesec|protectClass|starry|il2cpp|unity/i.test(m.name))
    .map(m => ({ name: m.name, base: m.base.toString(), size: m.size, path: m.path }));
  send({
    kind: 'probe-ready',
    pid: Process.id,
    arch: Process.arch,
    pointerSize: Process.pointerSize,
    modules
  });
}

emitReady();

Process.setExceptionHandler(function (details) {
  if (details.type !== 'illegal-instruction') {
    send({
      kind: 'fatal-signal',
      pid: Process.id,
      type: details.type,
      address: details.address !== undefined ? details.address.toString() : null,
      regs: regsFor(details.context),
      pcBytes: readHex(ptr(details.context.pc).sub(32), 96),
      stackBytes: readHex(ptr(details.context.sp), 256)
    });
    return false;
  }

  const context = details.context;
  const pc = ptr(context.pc);
  const lr = ptr(context.lr);
  const sp = ptr(context.sp);
  const lrModule = moduleFor(lr);
  const lrOffset = lrModule !== null ? lr.sub(lrModule.base) : null;
  const isNesecReturn =
    lrModule !== null &&
    /libnesec\.so$/i.test(lrModule.name) &&
    lrOffset !== null &&
    Math.abs(lrOffset.toInt32() - NESEC_RVA) <= 0x100;

  if (!isNesecReturn) {
    return false;
  }

  hits++;
  const pcRange = rangeFor(pc);
  const record = {
    kind: 'nesec-sigill',
    hit: hits,
    pid: Process.id,
    type: details.type,
    address: details.address.toString(),
    pc: pc.toString(),
    lr: lr.toString(),
    lrModule: lrModule !== null ? lrModule.name : null,
    lrOffset: lrOffset !== null ? lrOffset.toString() : null,
    pcRange,
    lrRange: rangeFor(lr),
    regs: regsFor(context),
    pcBytes: readHex(pc.sub(32), 96),
    lrBytes: readHex(lr.sub(64), 192),
    pcPageBytes: pcRange !== null && pcRange.base !== undefined ? readHex(ptr(pcRange.base), 4096) : null,
    stackBytes: readHex(sp, 256)
  };

  send(record);

  if (hits <= MAX_HITS) {
    try {
      const fallback = lrModule.base.add(NESEC_FALLBACK_RVA);
      context.pc = fallback;
      send({ kind: 'nesec-skip', hit: hits, newPc: fallback.toString(), mode: 'fallback' });
      return true;
    } catch (e) {
      send({ kind: 'nesec-skip-error', hit: hits, error: String(e) });
    }
  }

  return false;
});
