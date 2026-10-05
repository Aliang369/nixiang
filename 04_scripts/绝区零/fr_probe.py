#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""fr_probe.py — MuMu 上 com.miHoYo.Nap 只读探针：架构 / libil2cpp 基址 / 地址换算校验。

用法:
  python fr_probe.py [--device 127.0.0.1:16384] [--proc com.miHoYo.Nap] [--rva 0x11521138] [--len 16]
输出: arch、pageSize、模块表、按 base+RVA / base+RVA-0x4000 两种公式读到的字节。
"""
import argparse
import json
import time

import frida

JS = """
var mod = Process.findModuleByName('libil2cpp.so');
var mods = Process.enumerateModules().filter(function (m) {
    return /libil2cpp|libcymod|libChiy|libmain|libunity|libmono/.test(m.name);
}).map(function (m) { return m.name + ' base=' + m.base + ' size=' + m.size + ' path=' + m.path; });
var out = {
    arch: Process.arch, pointerSize: Process.pointerSize, pageSize: Process.pageSize,
    modCount: Process.enumerateModules().length, hits: mods,
    rva: RVA, base: mod ? mod.base.toString() : null
};
function hx(base, len) {
    try { return hxBytes(base, len); } catch (e) { return 'ERR ' + e; }
}
function hxBytes(base, len) {
    var b = new Uint8Array(Memory.readByteArray(base, len));
    var s = '';
    for (var i = 0; i < b.length; i++) s += ('0' + b[i].toString(16)).slice(-2);
    return s;
}
if (mod) {
    out.at_plus = hx(mod.base.add(RVA), LEN);
    if (RVA >= 0x4000) out.at_minus = hx(mod.base.add(RVA - 0x4000), LEN);
}
send(out);
"""


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--device", default="127.0.0.1:16384")
    ap.add_argument("--proc", default="com.miHoYo.Nap")
    ap.add_argument("--rva", type=lambda x: int(x, 0), default=0x11521138)
    ap.add_argument("--len", type=int, default=16)
    a = ap.parse_args()

    dev = frida.get_device(a.device)
    pid = int(a.proc) if a.proc.isdigit() else dev.get_process(a.proc).pid
    print("pid=%s" % pid)
    session = dev.attach(pid)
    got = []
    script = session.create_script("var RVA = %#x; var LEN = %d;\n%s" % (a.rva, a.len, JS))
    script.on("message", lambda m, d: got.append(m))
    script.load()
    time.sleep(1.5)
    print(json.dumps(got, ensure_ascii=False, indent=1))
    session.detach()


if __name__ == "__main__":
    main()
