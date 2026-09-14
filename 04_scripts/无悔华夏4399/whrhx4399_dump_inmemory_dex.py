#!/usr/bin/env python3
"""精确 dump 4399 无悔华夏进程里的 InMemoryDexFile。

只读：通过 Frida 枚举 ClassLoader，找到能加载 com.tpf.sdk.TPFApplication
的 loader，再读取其中 InMemoryDexFile 的真实 DEX 起始指针和 file_size。
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
  var pending = 0;
  var sent = 0;

  function hex64(v) {
    return "0x" + BigInt.asUintN(64, BigInt(v)).toString(16);
  }

  function sendDex(loaderIndex, elementIndex, cookiePtr) {
    var cookieBase = ptr(cookiePtr);
    var dexPtr = null;
    for (var off = 8; off < 0x80; off += 8) {
      var p;
      try { p = cookieBase.add(off).readPointer(); } catch (e) { continue; }
      try {
        var h = new Uint8Array(p.readByteArray(0x40));
        if (h[0] === 0x64 && h[1] === 0x65 && h[2] === 0x78 && h[3] === 0x0a &&
            h[4] === 0x30 && h[5] === 0x33 && h[6] === 0x35 && h[7] === 0x00) {
          dexPtr = p;
          break;
        }
      } catch (e) {}
    }
    if (dexPtr === null) return;

    var size = dexPtr.readU32();
    var h = new Uint8Array(dexPtr.readByteArray(0x40));
    var view = new DataView(h.buffer);
    size = view.getUint32(0x20, true);
    if (!size || size < 0x70 || size > 64 * 1024 * 1024) return;

    var key = dexPtr.toString();
    if (seen[key]) return;
    seen[key] = true;
    pending++;
    var data = dexPtr.readByteArray(size);
    send({
      type: "dex",
      loader: "L" + loaderIndex,
      element: elementIndex,
      cookie: cookiePtr,
      base: dexPtr.toString(),
      file_size: size
    }, data);
    pending--;
  }

  var loaders = [];
  Java.enumerateClassLoaders({
    onMatch: function (loader) { loaders.push(loader); },
    onComplete: function () {
      var dl = Java.use("dalvik.system.DexFile");
      var f = dl.class.getDeclaredField("mCookie");
      f.setAccessible(true);
      var Arrays = Java.use("java.util.Arrays");

      for (var li = 0; li < loaders.length; li++) {
        var loader = loaders[li];
        try { loader.loadClass("com.tpf.sdk.TPFApplication"); } catch (e) { continue; }
        var baseLoader;
        try {
          baseLoader = Java.cast(loader, Java.use("dalvik.system.BaseDexClassLoader"));
        } catch (e) { continue; }

        var elements = baseLoader.pathList.value.dexElements.value;
        for (var ei = 0; ei < elements.length; ei++) {
          var dexFile = elements[ei].dexFile.value;
          var label = String(dexFile);
          if (label.indexOf("InMemoryDexFile") !== 0) continue;
          var cookie = f.get(dexFile);
          var text = Arrays.toString.overload("[J").call(Arrays, cookie);
          var values = text.replace(/\[|\]/g, "").split(",");
          if (values.length !== 2) continue;
          sendDex(li, ei, hex64(values[1].trim()));
        }
      }
      send({ type: "done", sent: sent });
    }
  });
});
"""


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("serial", nargs="?", default="f9a116a2")
    parser.add_argument("package", nargs="?", default="com.yhhy.whrhx.m4399")
    parser.add_argument("output", type=Path)
    parser.add_argument("--pid", type=int, default=0)
    args = parser.parse_args()

    args.output.mkdir(parents=True, exist_ok=True)
    device = frida.get_device(args.serial)
    if args.pid:
        pid = args.pid
    else:
        wanted = {args.package, "无悔华夏"}
        proc = next((p for p in device.enumerate_processes() if p.name in wanted), None)
        if proc is None:
            raise SystemExit(f"process not found: {args.package}")
        pid = proc.pid
    session = device.attach(pid)
    done = threading.Event()
    written: list[dict] = []

    def on_message(message, data):
        if message.get("type") == "send":
            payload = message.get("payload") or {}
            if payload.get("type") == "dex" and data:
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
                print(f"dumped {name} ({len(data)} bytes) base={payload.get('base')}")
            elif payload.get("type") == "done":
                done.set()
        elif message.get("type") == "error":
            print(json.dumps(message, ensure_ascii=False), file=sys.stderr)

    script = session.create_script(JS)
    script.on("message", on_message)
    script.load()
    if not done.wait(60):
        print("timeout waiting for dex dump", file=sys.stderr)
        session.detach()
        return 1
    session.detach()
    (args.output / "inmemory_dex.json").write_text(
        json.dumps(written, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
