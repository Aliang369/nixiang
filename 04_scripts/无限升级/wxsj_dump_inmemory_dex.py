#!/usr/bin/env python3.11
"""精确 dump 无限升级进程里的 InMemoryDexFile。

只读：枚举所有 ClassLoader，读取每个 InMemoryDexFile 的真实 DEX 起始指针
与 file_size，并把字节通过 Frida send 送回本地保存。同时读取运行时
com.netease.nis.wrapper.MyApplication.strAppName，拿到壳解密后设置的真实
Application 类名。

用法：
    python3.11 wxsj_dump_inmemory_dex.py /tmp/wxsj_unshell/dex --pid <pid>
    python3.11 wxsj_dump_inmemory_dex.py /tmp/wxsj_unshell/dex
"""

from __future__ import annotations

import argparse
import json
import sys
import threading
from pathlib import Path

import frida


JS = r"""
Java.perform(function () {
  var seen = {};
  var sent = 0;

  try {
    var MA = Java.use("com.netease.nis.wrapper.MyApplication");
    send({ type: "appname", value: String(MA.strAppName.value) });
  } catch (e) {
    send({ type: "appname-error", error: String(e) });
  }

  function hex64(v) {
    return "0x" + BigInt.asUintN(64, BigInt(v)).toString(16);
  }

  function dumpElement(loaderIndex, elementIndex, cookiePtr) {
    var cookieBase = ptr(cookiePtr);
    var dexPtr = null;
    for (var off = 8; off < 0x80; off += 8) {
      var p;
      try { p = cookieBase.add(off).readPointer(); } catch (e) { continue; }
      try {
        var h = new Uint8Array(p.readByteArray(8));
        if (h[0] === 0x64 && h[1] === 0x65 && h[2] === 0x78 && h[3] === 0x0a &&
            h[4] === 0x30 && h[5] === 0x33 && h[6] === 0x35 && h[7] === 0x00) {
          dexPtr = p;
          break;
        }
      } catch (e) {}
    }
    if (dexPtr === null) {
      send({ type: "element-miss", loader: loaderIndex, element: elementIndex, cookie: cookiePtr });
      return;
    }

    var hdr = new Uint8Array(dexPtr.readByteArray(0x40));
    var size = new DataView(hdr.buffer).getUint32(0x20, true);
    if (!size || size < 0x70 || size > 256 * 1024 * 1024) {
      send({ type: "element-badsize", loader: loaderIndex, element: elementIndex, size: size });
      return;
    }

    var key = dexPtr.toString();
    if (seen[key]) return;
    seen[key] = true;
    sent++;
    send({
      type: "dex",
      loader: "L" + loaderIndex,
      element: elementIndex,
      cookie: cookiePtr,
      base: dexPtr.toString(),
      file_size: size
    }, dexPtr.readByteArray(size));
  }

  var loaders = [];
  Java.enumerateClassLoaders({
    onMatch: function (loader) { loaders.push(loader); },
    onComplete: function () {
      var dl = Java.use("dalvik.system.DexFile");
      var f = dl.class.getDeclaredField("mCookie");
      f.setAccessible(true);
      var Arrays = Java.use("java.util.Arrays");
      var BaseDex = Java.use("dalvik.system.BaseDexClassLoader");

      for (var li = 0; li < loaders.length; li++) {
        var baseLoader;
        try { baseLoader = Java.cast(loaders[li], BaseDex); } catch (e) { continue; }

        var labels = [];
        var elements;
        try { elements = baseLoader.pathList.value.dexElements.value; } catch (e) { continue; }
        for (var ei = 0; ei < elements.length; ei++) {
          var dexFile;
          try { dexFile = elements[ei].dexFile.value; } catch (e) { continue; }
          if (dexFile === null) { labels.push("null"); continue; }
          var label = String(dexFile);
          labels.push(label);
          if (label.indexOf("InMemoryDexFile") !== 0) continue;
          var cookie;
          try { cookie = f.get(dexFile); } catch (e) { continue; }
          var text = Arrays.toString.overload("[J").call(Arrays, cookie);
          var values = text.replace(/\[|\]/g, "").split(",");
          if (values.length !== 2) continue;
          dumpElement(li, ei, hex64(values[1].trim()));
        }
        send({ type: "loader", index: li, name: String(loaders[li]), elements: labels });
      }
      send({ type: "done", sent: sent });
    }
  });
});
"""


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("output", type=Path)
    parser.add_argument("--serial", default="f9a116a2")
    parser.add_argument("--package", default="com.xingzuodaoshi.infinityUpgrade")
    parser.add_argument("--pid", type=int, default=0)
    parser.add_argument("--timeout", type=float, default=120.0)
    args = parser.parse_args()

    args.output.mkdir(parents=True, exist_ok=True)
    device = frida.get_device(args.serial)
    if args.pid:
        pid = args.pid
    else:
        proc = next((p for p in device.enumerate_processes() if p.name == args.package), None)
        if proc is None:
            raise SystemExit(f"process not found: {args.package}")
        pid = proc.pid
    print(f"[dump] attach pid={pid}")
    session = device.attach(pid)
    done = threading.Event()
    written: list[dict] = []
    loaders: list[dict] = []
    appname = {"value": None}

    def on_message(message, data):
        if message.get("type") == "send":
            payload = message.get("payload") or {}
            kind = payload.get("type")
            if kind == "dex" and data:
                name = f"inmemory_L{payload['loader'][1:]}_e{payload['element']}_{payload['file_size']}.dex"
                path = args.output / name
                path.write_bytes(data)
                record = {
                    "path": str(path),
                    "bytes": len(data),
                    "base": payload.get("base"),
                    "cookie": payload.get("cookie"),
                    "loader": payload.get("loader"),
                    "element": payload.get("element"),
                }
                written.append(record)
                print(f"[dump] wrote {name} ({len(data)} bytes) base={payload.get('base')}")
            elif kind == "appname":
                appname["value"] = payload.get("value")
                print(f"[dump] runtime Application = {payload.get('value')}")
            elif kind == "appname-error":
                print(f"[dump] appname error: {payload.get('error')}", file=sys.stderr)
            elif kind == "loader":
                loaders.append(payload)
                print(f"[dump] loader[{payload.get('index')}] {payload.get('name')} elements={payload.get('elements')}")
            elif kind in ("element-miss", "element-badsize"):
                print(f"[dump] {kind} {payload}", file=sys.stderr)
            elif kind == "done":
                print(f"[dump] done sent={payload.get('sent')}")
                done.set()
        elif message.get("type") == "error":
            print(json.dumps(message, ensure_ascii=False), file=sys.stderr)

    # Frida 17 defaults to QJS, where the Java bridge is not injected.
    script = session.create_script(JS, runtime="v8")
    script.on("message", on_message)
    script.load()
    ok = done.wait(args.timeout)
    session.detach()
    if not ok:
        print("[dump] timeout waiting for dex dump", file=sys.stderr)
    (args.output / "inmemory_dex.json").write_text(
        json.dumps({"application": appname["value"], "loaders": loaders, "dex": written},
                   ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )
    print(f"[dump] app={appname['value']} dex_files={len(written)}")
    return 0 if written else 1


if __name__ == "__main__":
    raise SystemExit(main())
