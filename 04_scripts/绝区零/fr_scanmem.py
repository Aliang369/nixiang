#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""fr_scanmem.py — 在目标进程指定内存区间里按十六进制模式扫描（默认只读）。

用法:
  python fr_scanmem.py --pid 43628 --start 0x40003f9bf000 --size 0x9400000 \
      --pattern 75dd8699 --pattern 5acb6a16 [--max 40] [--prot rw-]
"""
import argparse
import json
import time

import frida

JS = """
var ranges = Process.enumerateRanges(PROT).filter(function (r) {
    return RSTART === 0 ? true :
        (r.base.compare(ptr(RSTART)) < 0 ? r.base.add(r.size).compare(ptr(RSTART)) >= 0
                                         : r.base.compare(ptr(REND)) < 0);
});
var pats = PATS.map(function (h) { return h.toUpperCase(); });
var res = [];
var scanned = 0;
ranges.forEach(function (r) { scanned += r.size; });
send({ranges: ranges.length, scanned: scanned});
ranges.forEach(function (r) {
    pats.forEach(function (p) {
        try {
            var hits = Memory.scanSync(r.base, r.size, p);
            hits.slice(0, MAX).forEach(function (h) { res.push(p + ' @' + h.address + ' (' + r.base + ')'); });
            if (hits.length) send({pat: p, range: r.base.toString(), n: hits.length,
                                   addrs: hits.slice(0, MAX).map(function (h) { return h.address.toString(); })});
        } catch (e) { send({err: '' + e, pat: p}); }
    });
});
send({done: true, total: res.length});
"""


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--device", default="127.0.0.1:16384")
    ap.add_argument("--pid", type=int, required=True)
    ap.add_argument("--start", type=lambda x: int(x, 0), default=0)
    ap.add_argument("--size", type=lambda x: int(x, 0), default=0)
    ap.add_argument("--prot", default="rw-")
    ap.add_argument("--pattern", action="append", required=True)
    ap.add_argument("--max", type=int, default=40)
    a = ap.parse_args()

    dev = frida.get_device(a.device)
    session = dev.attach(a.pid)
    js = (JS.replace("PROT", json.dumps(a.prot))
            .replace("RSTART", hex(a.start)).replace("REND", hex(a.start + a.size))
            .replace("PATS", json.dumps(a.pattern)).replace("MAX", str(a.max)))
    got = []
    script = session.create_script(js)
    script.on("message", lambda m, d: got.append(m))
    script.load()
    t0 = time.time()
    time.sleep(3)
    while time.time() - t0 < 120 and not any(
            isinstance(m.get("payload"), dict) and m["payload"].get("done") for m in got if m.get("payload")):
        time.sleep(1)
    print(json.dumps(got, ensure_ascii=False, indent=1)[:8000])
    session.detach()


if __name__ == "__main__":
    main()
