'use strict';

/*
 * Dump the nesec runtime dispatch table entry used by the container crash.
 * The relevant caller is libnesec.so+0xc0f68 and it loads the target from
 * libnesec.so+0x105ff0.
 */

const TABLE_OFFSET = 0x105000;
const TABLE_LENGTH = 0x120;
const CALLER_OFFSET = 0xc0f40;
const CALLER_LENGTH = 0x80;

let reported = false;
let foundAt = 0;
let lastEntry = null;
let samples = 0;

function toHex(buffer) {
  const bytes = new Uint8Array(buffer);
  let out = '';
  for (let i = 0; i < bytes.length; i++) {
    out += bytes[i].toString(16).padStart(2, '0');
  }
  return out;
}

function readHex(base, offset, length) {
  try {
    return toHex(base.add(offset).readByteArray(length));
  } catch (e) {
    return 'ERR:' + e;
  }
}

function poll() {
  const module = Process.findModuleByName('libnesec.so');
  if (module === null) {
    return;
  }
  if (!reported) {
    reported = true;
    foundAt = Date.now();
    send({ kind: 'nesec-found', pid: Process.id, base: module.base.toString() });
  }

  const elapsed = Date.now() - foundAt;
  let entry = null;
  try {
    entry = module.base.add(TABLE_OFFSET + 0xff0).readPointer().toString();
  } catch (e) {
    entry = 'ERR:' + e;
  }
  if (entry !== lastEntry) {
    lastEntry = entry;
    send({ kind: 'table-entry', pid: Process.id, elapsed, entry });
  }

  if (samples === 0 || (elapsed >= 100 && samples === 1) || (elapsed >= 500 && samples === 2)) {
    samples++;
    send({
      kind: 'nesec-table',
      pid: Process.id,
      elapsed,
      base: module.base.toString(),
      table: readHex(module.base, TABLE_OFFSET, TABLE_LENGTH),
      caller: readHex(module.base, CALLER_OFFSET, CALLER_LENGTH)
    });
  }

  if (elapsed > 2000) {
    clearInterval(timer);
  }
}

send({ kind: 'probe-ready', pid: Process.id });
const timer = setInterval(poll, 5);
