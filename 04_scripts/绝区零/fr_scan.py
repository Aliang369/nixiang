#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""fr_scan.py — 只读列出目标进程的模块与高地址映射范围（排查 MuMu 上 frida 看到的进程内容）。

用法:
  python fr_scan.py [--device 127.0.0.1:16384] [--pid 43628] [--proc com.miHoYo.Nap]
                    [--filter il2cpp] [--max 60]
"""
import argparse
import json
import time

import frida

JS = """
var flt = FILTER;
var mods = Process.enumerateModules().filter(function (m) { return !flt || m.name.indexOf(flt) >= 0; })
    .slice(0, MAX).map(function (m) { return m.name + ' @' + m.base + ' sz=' + m.size; });
var hi = [];
if (HIGH) {
    hi = Process.enumerateRanges('r-x').filter(function (r) {
        return r.base.compare(ptr('0x400000000000')) >= 0 && r.base.compare(ptr('0x400200000000')) < 0;
    }).slice(0, 20).map(function (r) { return r.base + '-' + r.base.add(r.size) + ' ' + (r.file ? r.file.path : ''); });
}
send({id: Process.id, arch: Process.arch, ptr: Process.pointerSize, page: Process.pageSize,
      modTotal: Process.enumerateModules().length, mods: mods, hi: hi,
      probe: Probe});
"""


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--device", default="127.0.0.1:16384")
    ap.add_argument("--pid", type=int, default=0)
    ap.add_argument("--proc", default="com.miHoYo.Nap")
    ap.add_argument("--filter", default="")
    ap.add_argument("--max", type=int, default=60)
    ap.add_argument("--high", action="store_true")
    ap.add_argument("--probe", default="", help="要读的绝对地址（hex），读 16 字节")
    a = ap.parse_args()

    dev = frida.get_device(a.device)
    pid = a.pid or dev.get_process(a.proc).pid
    print("pid=%s" % pid)
    session = dev.attach(pid)
    probe = "null"
    if a.probe:
        probe = "hexdump(ptr('%s'), {length: 16})" % a.probe
    js = JS.replace("FILTER", json.dumps(a.filter)).replace("MAX", str(a.max)) \
           .replace("HIGH", "true" if a.high else "false").replace("Probe", probe)
    got = []
    script = session.create_script(js)
    script.on("message", lambda m, d: got.append(m))
    script.load()
    time.sleep(1.2)
    print(json.dumps(got, ensure_ascii=False, indent=1)[:6000])
    session.detach()


if __name__ == "__main__":
    main()
