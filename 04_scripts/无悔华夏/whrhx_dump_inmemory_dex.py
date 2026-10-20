#!/usr/bin/env python3
"""精确 dump 无悔华夏（B 站渠道）进程里的 InMemoryDexFile。

只读：通过 Frida 枚举全部 ClassLoader，读取每个 InMemoryDexFile 的真实
DEX 起始指针与 file_size，精确导出真实 DEX（供无壳重建使用）。

与 4399 版差异：不按 `com.tpf.sdk.TPFApplication` 过滤 loader（B 站渠道真实
Application 类名未知），改为全 loader 扫，输出里带 loader/element 标签，
由人工按大小与内容认领。
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

  function sendDex(loaderIndex, elementIndex, cookiePtr, loaderLabel) {
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

    var h = new Uint8Array(dexPtr.readByteArray(0x40));
    var view = new DataView(h.buffer);
    var size = view.getUint32(0x20, true);
    if (!size || size < 0x70 || size > 64 * 1024 * 1024) return;

    var key = dexPtr.toString();
    if (seen[key]) return;
    seen[key] = true;
    pending++;
    var data = dexPtr.readByteArray(size);
    send({
      type: "dex",
      loader: loaderLabel,
      element: elementIndex,
      cookie: cookiePtr,
      base: dexPtr.toString(),
      file_size: size
    }, data);
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
        var baseLoader;
        try {
          baseLoader = Java.cast(loader, Java.use("dalvik.system.BaseDexClassLoader"));
        } catch (e) { continue; }

        var elements;
        try { elements = baseLoader.pathList.value.dexElements.value; } catch (e) { continue; }
        for (var ei = 0; ei < elements.length; ei++) {
          var dexFile;
          try { dexFile = elements[ei].dexFile.value; } catch (e) { continue; }
          var label = String(dexFile);
          if (label.indexOf("InMemoryDexFile") !== 0) continue;
          var cookie = f.get(dexFile);
          var text = Arrays.toString.overload("[J").call(Arrays, cookie);
          var values = text.replace(/\[|\]/g, "").split(",");
          if (values.length !== 2) continue;
          sendDex(li, ei, hex64(values[1].trim()), "L" + li);
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
    parser.add_argument("package", nargs="?", default="com.yhhy.whrhx.bilibili")
    parser.add_argument("output", type=Path)
    parser.add_argument("--pid", type=int, default=0)
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
    session = device.attach(pid)
    done = threading.Event()
    written: list[dict] = []

    def on_message(message, data):
        if message.get("type") == "send":
            payload = message.get("payload") or {}
            if payload.get("type") == "dex" and data:
                name = f"inmemory_{payload['loader']}_e{payload['element']}_{payload['file_size']}.dex"
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
    if not done.wait(90):
        print("timeout waiting for dex dump", file=sys.stderr)
        session.detach()
        return 1
    session.detach()
    (args.output / "inmemory_dex.json").write_text(
        json.dumps(written, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    print(f"total={len(written)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
