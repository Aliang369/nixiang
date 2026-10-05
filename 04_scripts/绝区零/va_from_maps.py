#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""va_from_maps.py — 用 /proc/<pid>/maps 把「文件偏移」换算成运行时 VA（只读）。

为什么单独写：MuMu(Houdini) 下 arm64 libil2cpp.so 不是按 ELF 段一股脑映射，
而是被拆成 4 段 + 尾部匿名 BSS（段间还有 0x3000 空洞），
`base + 文件偏移` 会算错（实测偏 0x10000）。正确口径：
    VA(fo) = seg.start + (fo - seg.off)        # fo 落在文件段内
    VA(fo) = anon.start + (fo - tail_end)      # fo 落在尾部匿名 BSS

用法：
  python va_from_maps.py --serial S --pid N --lib libil2cpp.so \
      --file <本地同名 so 或省略> 0x1151d138 0xfb2700c ...

带 --file 时读回 16B 并与本地文件同偏移字节对比，用于自证换算口径。
"""
import argparse
import re
import subprocess
import sys


def sh(serial, script, timeout=180):
    return subprocess.run(["adb", "-s", serial, "shell", "sh"],
                          input=script.encode(), capture_output=True, timeout=timeout)


def sh_raw(serial, script, timeout=180):
    return subprocess.run(["adb", "-s", serial, "shell", script],
                          capture_output=True, timeout=timeout)


def read_exact(serial, pid, addr, length):
    aligned = addr & ~0xF
    pad = addr - aligned
    blocks = (pad + length + 15) // 16
    script = ('dd if=/proc/%d/mem bs=16 skip=%d count=%d 2>/dev/null | base64 -w0\n'
              % (pid, aligned // 16, blocks))
    r = sh(serial, script)
    import base64
    try:
        raw = base64.b64decode(r.stdout.strip())
    except Exception:
        return b""
    if len(raw) < pad + length:
        return b""
    return raw[pad:pad + length]


def load_segments(serial, pid, lib):
    r = sh_raw(serial, "cat /proc/%d/maps" % pid)
    lines = r.stdout.decode("utf-8", "replace").splitlines()
    segs, anon = [], []
    for i, line in enumerate(lines):
        m = re.match(r"([0-9a-f]+)-([0-9a-f]+)\s+(\S+)\s+([0-9a-f]+)\s+(\S+)\s+(\S+)\s*(.*)$", line)
        if not m:
            continue
        start, end = int(m.group(1), 16), int(m.group(2), 16)
        perm, off, path = m.group(3), int(m.group(4), 16), m.group(7).strip()
        if lib in path:
            segs.append({"start": start, "end": end, "perm": perm, "off": off, "path": path,
                         "idx": i})
    if not segs:
        return None, None, lines
    segs.sort(key=lambda s: s["off"])
    tail = segs[-1]
    tail_end = tail["off"] + (tail["end"] - tail["start"])
    # 尾部匿名 BSS：紧跟最后一段、同权限、offset=0、无路径
    for line in lines[tail["idx"] + 1: tail["idx"] + 4]:
        m = re.match(r"([0-9a-f]+)-([0-9a-f]+)\s+rw-p\s+00000000\s+\S+\s+\S+\s*$", line)
        if m and int(m.group(1), 16) == tail["end"]:
            anon = {"start": int(m.group(1), 16), "end": int(m.group(2), 16)}
            break
    return {"segs": segs, "tail_end": tail_end, "anon": anon}, segs, lines


def fo_to_va(tbl, fo):
    for s in tbl["segs"]:
        size = s["end"] - s["start"]
        if s["off"] <= fo < s["off"] + size:
            return s["start"] + (fo - s["off"]), "seg@%x" % s["off"]
    if tbl["anon"] and fo >= tbl["tail_end"]:
        va = tbl["anon"]["start"] + (fo - tbl["tail_end"])
        if va < tbl["anon"]["end"]:
            return va, "bss"
        return None, "bss-out-of-range"
    return None, "unmapped"


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--serial", required=True)
    ap.add_argument("--pid", type=int, required=True)
    ap.add_argument("--lib", default="libil2cpp.so")
    ap.add_argument("--file", default=None)
    ap.add_argument("offsets", nargs="+")
    a = ap.parse_args()

    tbl, segs, _lines = load_segments(a.serial, a.pid, a.lib)
    if not tbl:
        print("ERROR: %s not mapped" % a.lib)
        return 2
    for s in segs:
        print("seg VA %016x-%016x perm=%s off=%09x size=%08x" %
              (s["start"], s["end"], s["perm"], s["off"], s["end"] - s["start"]))
    if tbl["anon"]:
        print("bss VA %016x-%016x (tail_end=0x%x)" %
              (tbl["anon"]["start"], tbl["anon"]["end"], tbl["tail_end"]))
    local = open(a.file, "rb") if a.file else None
    rc = 0
    for spec in a.offsets:
        fo = int(spec, 0)
        va, how = fo_to_va(tbl, fo)
        if va is None:
            print("fo=0x%08x -> UNMAPPED (%s)" % (fo, how))
            rc = 3
            continue
        blob = read_exact(a.serial, a.pid, va, 16)
        note = ""
        if local is not None and (fo + 16) <= _fsize(local):
            local.seek(fo)
            want = local.read(16)
            note = " MATCH" if blob == want else " MISMATCH(want %s)" % want.hex(" ")
        print("fo=0x%08x -> VA=0x%016x [%s] %s%s" % (fo, va, how, blob.hex(" "), note))
    return rc


def _fsize(f):
    cur = f.tell()
    f.seek(0, 2)
    n = f.tell()
    f.seek(cur)
    return n


if __name__ == "__main__":
    sys.exit(main())
