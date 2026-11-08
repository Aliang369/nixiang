#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""dwdlm — 目标进程内存分块流式搜索（不落盘整段，可扫 384MB 级 dalvik 大段）

编码：UTF-8 + UTF-16LE 双份；另支持 hex: 前缀的原始字节模式（如 hex:6465780a303335 = dex\n035）。
命中可保存上下文到 --out 目录，便于离线判读。

用法：
  python3 search_mem_dwdlm.py --pid 1234 --kw 数据检查 --seg dalvik --out /tmp/hits
  python3 search_mem_dwdlm.py --pid 1234 --kw hex:6465780a303335 --seg libc_malloc
"""
import argparse
import json
import os
import re
import subprocess
import sys

SERIAL = "f9a116a2"
PKG = "com.fightingstars.games.animal"
CHUNK = 32 * 1048576


def sh(cmd, timeout=300):
    return subprocess.run(["adb", "-s", SERIAL, "shell", cmd], capture_output=True,
                          text=True, timeout=timeout, errors="replace")


def parse_maps(txt):
    out = []
    for ln in txt.splitlines():
        m = re.match(r"^([0-9a-f]+)-([0-9a-f]+)\s+(\S+)\s+(\S+)\s+\S+\s+\d+\s*(.*)$", ln.strip())
        if m:
            out.append((int(m.group(1), 16), int(m.group(2), 16), m.group(3), m.group(5).strip()))
    return out


def read_chunk(pid, addr, size):
    cmd = ("dd if=/proc/%d/mem iflag=skip_bytes,count_bytes skip=%d bs=1048576 count=%d 2>/dev/null"
           % (pid, addr, size))
    r = subprocess.run(["adb", "-s", SERIAL, "exec-out", "su -c", cmd], capture_output=True, timeout=300)
    return r.stdout or b""


def show(d, st, plen, addr, tag, path, ctx, outdir, idx):
    cs, ce = max(0, st - ctx), min(len(d), st + plen + ctx)
    sub = d[cs:ce]
    print("-" * 70)
    print("[%s] 0x%x  %s" % (tag, addr, path[:40]))
    v8 = re.sub(rb"[^\x20-\x7e]", b".", sub).decode("latin1")
    s16 = sub.decode("utf-16-le", errors="replace")
    s16 = "".join(ch if ch.isprintable() else "." for ch in s16)
    print("  (u8 ) %s" % v8[:300])
    print("  (u16) %s" % s16[:300])
    if outdir:
        os.makedirs(outdir, exist_ok=True)
        p = os.path.join(outdir, "%s_%x.bin" % (re.sub(r"[^0-9A-Za-z_]", "_", tag), addr))
        open(p, "wb").write(sub)
        open(p + ".txt", "w", encoding="utf-8").write(
            "tag=%s addr=0x%x path=%s\n[u8]\n%s\n[u16]\n%s\n" % (tag, addr, path, v8, s16))
        print("  saved %s" % p)
    sys.stdout.flush()


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--pid", type=int, default=0)
    ap.add_argument("--kw", default="数据检查")
    ap.add_argument("--seg", default="")
    ap.add_argument("--ctx", type=int, default=4000)
    ap.add_argument("--out", default="")
    ap.add_argument("--min-mb", type=int, default=0)
    ap.add_argument("--max-hits", type=int, default=10)
    args = ap.parse_args()

    pid = args.pid
    if not pid:
        o = (sh("pidof %s" % PKG).stdout or "").split()
        pid = int(o[0]) if o else 0
    if not pid:
        print("!! 进程不存在")
        return 2

    pats = []
    for k in [x for x in args.kw.split(",") if x]:
        if k.startswith("hex:"):
            pats.append((k, bytes.fromhex(k[4:])))
        else:
            pats.append((k + "|u8", k.encode("utf-8")))
            pats.append((k + "|u16", k.encode("utf-16-le")))

    segs = parse_maps(sh("su -c 'cat /proc/%d/maps'" % pid).stdout or "")
    sel = [s for s in segs if "w" in s[2] and args.seg in s[3] and (s[1] - s[0]) >= args.min_mb * 1048576]
    total = sum(s[1] - s[0] for s in sel)
    print("pid=%d 段=%d %.0fMB kw=%s" % (pid, len(sel), total / 1048576.0, ",".join(args.kw.split(","))))
    hits, scanned = 0, 0
    for st0, en, perm, path in sel:
        pos = st0
        while pos < en:
            want = min(CHUNK, en - pos)
            d = read_chunk(pid, pos, want)
            if not d:
                pos += want
                continue
            scanned += len(d)
            for tag, p in pats:
                i = d.find(p)
                while i >= 0:
                    hits += 1
                    if hits <= args.max_hits:
                        show(d, i, len(p), pos + i, tag, path, args.ctx, args.out, hits)
                    i = d.find(p, i + 1)
            if len(d) < want:
                break
            pos += len(d)
        print("  段 0x%x %dMB 完成，累计 %.0fMB 命中 %d" % (st0, (en - st0) // 1048576, scanned / 1048576.0, hits))
        sys.stdout.flush()
    print("== 扫 %.0fMB 命中 %d ==" % (scanned / 1048576.0, hits))
    return 0


if __name__ == "__main__":
    sys.exit(main())
