'use strict';

/*
 * Chongya container probe: watch for libnesec.so, report the dispatch-table
 * slot at +0x105ff0, and optionally neutralise it.
 *
 * Runtime facts established 2026-09-28:
 *   - libnesec.so on disk is encrypted (code shows up as udf #0xc3 filler);
 *     only the runtime image is meaningful.
 *   - vaddr 0x105ff0 lives inside the first R-X LOAD segment, so writing it
 *     needs a page-permission change.
 *   - +0xc0f68 loads a function pointer from +0x105ff0; when the slot is 0
 *     the code falls through to the fallback at +0xc0fa0, otherwise it
 *     branches to the pointer (blr x16).
 *   - In the container that slot points at an anonymous r-x trampoline whose
 *     +0x4cc is a trap, producing SIGILL with LR = libnesec+0xc0f94.
 *
 * Config is injected by the driver as a JS literal in PROBE_CONFIG.
 */

var PROBE_CONFIG = (typeof PROBE_CONFIG !== 'undefined') ? PROBE_CONFIG : {};
var NESEC = 'libnesec.so';
var SLOT_RVA = 0x105ff0;
var FN_RVA = 0xc0f68;
var FALLBACK_RVA = 0xc0fa0;
var LR_RVA = 0xc0f94;
var POLL_MS = 2;

var reported = false;
var patched = false;
var lastValue = null;
var repatchCount = 0;
var MAX_REPATCH = 16;

function hex(p, n) {
  try {
    return Array.from(new Uint8Array(p.readByteArray(n)))
      .map(function (b) { return b.toString(16).padStart(2, '0'); })
      .join('');
  } catch (e) {
    return 'ERR:' + e;
  }
}

function rangeInfo(p) {
  try {
    var r = Process.findRangeByAddress(p);
    if (r === null) {
      return null;
    }
    return { base: r.base.toString(), size: r.size, prot: r.protection };
  } catch (e) {
    return 'ERR:' + e;
  }
}

function anonExecRanges() {
  var out = [];
  try {
    Process.enumerateRanges('r-x').forEach(function (r) {
      if (r.file === null || r.file === undefined) {
        out.push({ base: r.base.toString(), size: r.size, prot: r.protection });
      }
    });
  } catch (e) {
    out.push({ error: String(e) });
  }
  return out;
}

function regsFor(context) {
  var out = {};
  var names = ['x0', 'x1', 'x2', 'x3', 'x8', 'x9', 'x10', 'x16', 'sp', 'pc', 'lr'];
  for (var i = 0; i < names.length; i++) {
    try {
      var v = context[names[i]];
      out[names[i]] = v !== undefined ? ptr(v).toString() : null;
    } catch (e) {
      out[names[i]] = 'ERR:' + e;
    }
  }
  return out;
}

function tryPatchSlot(base) {
  var slot = base.add(SLOT_RVA);
  var before = null;
  try { before = slot.readU64().toString(); } catch (e) { before = 'ERR:' + e; }
  var attempts = [];
  try {
    slot.writeU64(0);
    attempts.push({ mode: 'direct', ok: true });
  } catch (e) {
    attempts.push({ mode: 'direct', ok: false, error: String(e) });
    try {
      var r = Process.findRangeByAddress(slot);
      Memory.protect(r.base, r.size, 'rw-');
      slot.writeU64(0);
      attempts.push({ mode: 'mprotect-rw', ok: true });
      try {
        Memory.protect(r.base, r.size, 'r-x');
        attempts.push({ mode: 'restore-rx', ok: true });
      } catch (e3) {
        attempts.push({ mode: 'restore-rx', ok: false, error: String(e3) });
      }
    } catch (e2) {
      attempts.push({ mode: 'mprotect', ok: false, error: String(e2) });
    }
  }
  var after = null;
  try { after = slot.readU64().toString(); } catch (e) { after = 'ERR:' + e; }
  patched = after === '0';
  send({
    kind: 'slot-patch',
    pid: Process.id,
    before: before,
    after: after,
    attempts: attempts
  });
  return patched;
}

function onNesec(mod) {
  if (reported) {
    return;
  }
  reported = true;
  var base = mod.base;
  var slot = base.add(SLOT_RVA);
  var record = {
    kind: 'nesec-loaded',
    pid: Process.id,
    module: { base: base.toString(), size: mod.size, path: mod.path },
    slot: {
      addr: slot.toString(),
      range: rangeInfo(slot),
      bytes: hex(slot.sub(0x40), 0x90)
    },
    fn: { addr: base.add(FN_RVA).toString(), range: rangeInfo(base.add(FN_RVA)) },
    fnBytes: hex(base.add(FN_RVA - 0x40), 0x100),
    anonExec: anonExecRanges()
  };
  try { record.slot.value = slot.readU64().toString(); } catch (e) { record.slot.error = String(e); }
  send(record);
  if (PROBE_CONFIG.patchSlot) {
    tryPatchSlot(base);
  }
}

function poll() {
  try {
    var mod = Process.findModuleByName(NESEC);
    if (mod !== null) {
      onNesec(mod);
    }
  } catch (e) {
    send({ kind: 'poll-error', pid: Process.id, error: String(e) });
  }
}

function pollSlot() {
  if (!reported || !PROBE_CONFIG.patchSlot || repatchCount >= MAX_REPATCH) {
    return;
  }
  try {
    var mod = Process.findModuleByName(NESEC);
    if (mod === null) {
      return;
    }
    var slot = mod.base.add(SLOT_RVA);
    var value = slot.readU64().toString();
    if (value !== lastValue) {
      lastValue = value;
      if (value !== '0') {
        repatchCount++;
        send({ kind: 'slot-rechanged', pid: Process.id, value: value, repatch: repatchCount });
        tryPatchSlot(mod.base);
      }
    }
  } catch (e) {
    /* ignore transient read failures */
  }
}

send({ kind: 'probe-ready', pid: Process.id, arch: Process.arch, patchSlot: !!PROBE_CONFIG.patchSlot });
try {
  Process.attachModuleObserver({
    onAdded: function (m) {
      try {
        if (m.name === NESEC) {
          onNesec(m);
        }
      } catch (e) {
        send({ kind: 'observer-error', pid: Process.id, error: String(e) });
      }
    }
  });
} catch (e) {
  send({ kind: 'observer-unavailable', pid: Process.id, error: String(e) });
}
setInterval(poll, POLL_MS);
poll();
setInterval(pollSlot, POLL_MS);

Process.setExceptionHandler(function (details) {
  if (details.type !== 'illegal-instruction') {
    return false;
  }
  var context = details.context;
  var pc = ptr(context.pc);
  var lr = ptr(context.lr);
  var lrMod = null;
  var lrOff = null;
  try {
    lrMod = Process.findModuleByAddress(lr);
    if (lrMod !== null) {
      lrOff = lr.sub(lrMod.base);
    }
  } catch (e) { /* ignore */ }

  var isNesec =
    lrMod !== null &&
    /libnesec\.so$/i.test(lrMod.name) &&
    lrOff !== null &&
    Math.abs(lrOff.toInt32() - LR_RVA) <= 0x200;

  var record = {
    kind: 'sigill',
    pid: Process.id,
    pc: pc.toString(),
    lr: lr.toString(),
    lrModule: lrMod !== null ? lrMod.name : null,
    lrOffset: lrOff !== null ? lrOff.toString() : null,
    isNesec: isNesec,
    regs: regsFor(context),
    pcRange: rangeInfo(pc),
    pcPage: hex(ptr(rangeInfo(pc) && rangeInfo(pc).base ? rangeInfo(pc).base : pc), 0x600)
  };
  send(record);

  if (isNesec && PROBE_CONFIG.skipSigill) {
    try {
      var fallback = lrMod.base.add(FALLBACK_RVA);
      context.pc = fallback;
      send({ kind: 'sigill-skip', pid: Process.id, newPc: fallback.toString() });
      return true;
    } catch (e) {
      send({ kind: 'sigill-skip-error', pid: Process.id, error: String(e) });
    }
  }
  return false;
});
