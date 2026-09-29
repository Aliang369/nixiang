#!/usr/bin/env python3
"""运行时导出 Cocos Creator 传给 se::ScriptEngine::evalString 的解密后 JS。

仅用于分析：spawn 目标进程后在 libcocos.so 内按 /proc/self/maps 计算
evalString(const char*, int, Value*, Value*) 真实地址，校验序言后 hook，
把长度达到阈值的脚本写到应用自身 files 目录，再由外部 adb 拉取。
"""
from __future__ import annotations

import argparse
import sys
import time

import frida

DEFAULT_PACKAGE = "com.windforce.rg.offical"
DEFAULT_EVAL_OFF = 0xC3D188
DEFAULT_PROLOGUE = "ff0305d1fd7b0ea9"

JS_TEMPLATE = r"""
const PACKAGE = %(package_json)s;
const EVAL_OFF = ptr('0x%(off)x');
const EXPECTED = '%(prologue)s';
const MIN_LEN = %(min_len)d;
const OUT_DIR = '/data/data/' + PACKAGE + '/files';

function emit(m) { send({ t: 'log', m: m }); }

function resolveEntry() {
  let lines;
  try {
    lines = File.readAllText('/proc/self/maps').split('\n');
  } catch (e) {
    emit('read maps failed: ' + e);
    return null;
  }
  const candidates = [];
  for (const line of lines) {
    if (line.indexOf('libcocos.so') < 0) continue;
    const m = line.match(/^([0-9a-f]+)-([0-9a-f]+)\s+\S{4}\s+([0-9a-f]+)/);
    if (!m) continue;
    const start = ptr('0x' + m[1]);
    const fileOff = parseInt(m[3], 16);
    candidates.push(start.sub(fileOff).add(EVAL_OFF));
  }
  for (const cand of candidates) {
    try {
      const bytes = cand.readByteArray(8);
      const got = Array.from(new Uint8Array(bytes))
        .map(function (b) { return ('0' + b.toString(16)).slice(-2); })
        .join('');
      if (got === EXPECTED) {
        emit('entry resolved at ' + cand + ' (base candidate count=' + candidates.length + ')');
        return cand;
      }
    } catch (e) {
      // 该候选不可读，继续
    }
  }
  return null;
}

let dumped = 0;
let busy = false;

function install(entry) {
  Interceptor.attach(entry, {
    onEnter: function (args) {
      if (busy) return;
      let len = 0;
      try { len = args[2].toInt32(); } catch (e) { return; }
      if (len <= 0 || len > 64 * 1024 * 1024) return;
      let text = null;
      try { text = args[1].readUtf8String(len); } catch (e) {
        try { text = Memory.readUtf8String(args[1], len); } catch (e2) {
          emit('read script failed len=' + len + ': ' + e2);
          return;
        }
      }
      if (text === null) return;
      if (len >= MIN_LEN) {
        busy = true;
        try {
          dumped += 1;
          const name = OUT_DIR + '/sjzz_eval_' + dumped + '_' + len + '.js';
          const f = new File(name, 'w');
          f.write(text);
          f.flush();
          f.close();
          send({ t: 'dumped', n: dumped, len: len, path: name, head: text.slice(0, 120) });
        } catch (e) {
          emit('write failed len=' + len + ': ' + e);
        } finally {
          busy = false;
        }
      } else {
        emit('eval len=' + len + ' head=' + text.slice(0, 80).replace(/\r?\n/g, ' '));
      }
    }
  });
  emit('hook installed');
}

let installed = false;

function tryInstall(tag) {
  if (installed) return;
  const entry = resolveEntry();
  if (entry !== null) {
    installed = true;
    install(entry);
  }
}

// spawn 暂停点在入口，libcocos.so 尚未映射：等模块出现，再装 hook。
try {
  Process.attachModuleObserver({
    onAdded: function (m) {
      if (!installed && m.name === 'libcocos.so') {
        emit('module added at ' + m.base);
        tryInstall('observer');
      }
    }
  });
} catch (e) {
  emit('module observer unavailable: ' + e);
}

const timer = setInterval(function () {
  tryInstall('poll');
  if (installed) clearInterval(timer);
}, 50);
tryInstall('initial');
"""


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--package", default=DEFAULT_PACKAGE)
    ap.add_argument("--off", type=lambda v: int(v, 0), default=DEFAULT_EVAL_OFF)
    ap.add_argument("--prologue", default=DEFAULT_PROLOGUE)
    ap.add_argument("--min-len", type=int, default=65536)
    ap.add_argument("--wait", type=float, default=60.0)
    args = ap.parse_args()

    dev = frida.get_usb_device(timeout=5)
    try:
        dev.kill(args.package)
        time.sleep(1)
    except Exception:
        pass

    pid = dev.spawn([args.package])
    session = dev.attach(pid)
    source = JS_TEMPLATE % {
        "package_json": repr(args.package).replace("'", '"'),
        "off": args.off,
        "prologue": args.prologue,
        "min_len": args.min_len,
    }

    seen = {"dumped": 0, "error": 0}

    def on_message(message, data):
        if message.get("type") == "error":
            seen["error"] += 1
            print("[frida-error]", message.get("description"), file=sys.stderr)
            return
        payload = message.get("payload") or {}
        kind = payload.get("t")
        if kind == "dumped":
            seen["dumped"] += 1
            print(
                "[dumped] n={n} len={len} path={path} head={head!r}".format(**payload),
                flush=True,
            )
        else:
            print("[log]", payload.get("m"), flush=True)

    script = session.create_script(source)
    script.on("message", on_message)
    script.load()
    dev.resume(pid)
    print(f"[info] spawned {args.package} pid={pid}; waiting up to {args.wait}s", flush=True)

    deadline = time.time() + args.wait
    while time.time() < deadline:
        if seen["dumped"] > 0:
            time.sleep(3)
            break
        time.sleep(0.5)

    try:
        session.detach()
    except Exception:
        pass
    print(f"[info] done dumped={seen['dumped']} errors={seen['error']}", flush=True)
    return 0 if seen["dumped"] > 0 else 1


if __name__ == "__main__":
    raise SystemExit(main())
