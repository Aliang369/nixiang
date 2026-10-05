#!/usr/bin/env python3
"""fr_attach_test.py — 验证 frida 能否注入 MuMu 上的目标游戏进程（只读）。

用法: python fr_attach_test.py [进程名或PID]   (默认 com.miHoYo.Nap)
输出: arch / pointerSize / module 数 / libil2cpp 基址与大小 / pageSize
"""
import sys
import time
import json
import frida

TARGET = sys.argv[1] if len(sys.argv) > 1 else "com.miHoYo.Nap"

JS = """
var mods = Process.enumerateModules();
var out = [];
for (var i = 0; i < mods.length; i++) {
  var n = mods[i].name;
  if (n === 'libil2cpp.so' || n === 'libcymod.so' || n === 'libChiy.so') {
    out.push(n + ' base=' + mods[i].base + ' size=' + mods[i].size + ' path=' + mods[i].path);
  }
}
send({arch: Process.arch, ptr: Process.pointerSize, page: Process.pageSize,
      nm: mods.length, hits: out});
"""

def main():
    dev = frida.get_usb_device(timeout=20)
    try:
        p = dev.get_process(TARGET)
        pid = p.pid
    except Exception:
        pid = int(TARGET)
    print("target pid=%s" % pid)
    session = dev.attach(pid)
    got = []
    script = session.create_script(JS)
    script.on("message", lambda m, d: got.append(m))
    script.load()
    time.sleep(1.5)
    print(json.dumps(got, ensure_ascii=False, indent=1))
    session.detach()
    print("ATTACH_OK")

if __name__ == "__main__":
    try:
        main()
    except Exception as e:
        print("ATTACH_FAIL %s: %s" % (type(e).__name__, e))
        sys.exit(1)
