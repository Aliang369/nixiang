#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""chain_probe.py — 指针链探针（只读设备内存，验证「玩家 actorId」链路可复现）。

背景：池鸳 libcymod 的 actorId 取值链（libcymod file 0x4ed30 反汇编）：
    x8 = [libbase + 0x1d749418]     # libil2cpp 全局
    x8 = [x8 + 0x08]
    x8 = [x8 + 0x18]
    x8 = [x8 + 0x10]
    x8 = [x8 + 0x78]
    x8 = [x8 + 0x00]
    x8 = [x8 + 0xB8]
    x8 = [x8 + 0x00]
    actorId = u32 [x8 + 0x38]

本脚本在真机/模拟器上按同一条链取值，用来确认：
  ① 映射基址口径（maps 中 file offset 0 段的 start == 池鸳理解的 base？）
  ② 链路在官方包上是否稳定给出非 0 actorId。

用法：
  python chain_probe.py --serial 127.0.0.1:16384 --pid 2454 [--addr 0x1d749418]
                        [--chain 8,18,10,78,0,b8,0,38] [--base-delta 0]
"""
import argparse
import base64
import re
import subprocess
import sys


def sh(serial, script, timeout=120):
    return subprocess.run(["adb", "-s", serial, "shell", "sh"],
                          input=script.encode(), capture_output=True, timeout=timeout)


def read_mem(serial, pid, addr, length):
    blocks = (length + 15) // 16
    script = ('dd if=/proc/%d/mem bs=16 skip=%d count=%d 2>/dev/null | base64 -w0\n'
              % (pid, addr // 16, blocks))
    r = sh(serial, script)
    try:
        return base64.b64decode(r.stdout.strip())
    except Exception:
       return b""

def read_exact(serial, pid, addr, length):
    """按 16B 对齐读取再切片（/proc/pid/mem 的 dd 只能按 16B 块跳）。"""
    aligned = addr & ~0xF
    pad = addr - aligned
    raw = read_mem(serial, pid, aligned, pad + length)
    if len(raw) < pad + length:
        return b""
    return raw[pad:pad + length]


def lib_base_from_maps(serial, pid, name):
    """返回 maps 中该 lib 里 file offset 最小（通常=0）段的 start。"""
    r = sh(serial, "cat /proc/%d/maps\n" % pid)
    best = None
    for line in r.stdout.decode("utf-8", "replace").splitlines():
        if name not in line:
            continue
        m = re.match(r"([0-9a-f]+)-([0-9a-f]+)\s+(\S+)\s+([0-9a-f]+)", line)
        if not m:
            continue
        start, _end, _perm, off = int(m.group(1), 16), int(m.group(2), 16), m.group(3), int(m.group(4), 16)
        if best is None or off < best[0]:
            best = (off, start, line)
    if best is None:
        return None, None
    return best[1], best[2]


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--serial", required=True)
    ap.add_argument("--pid", type=int, required=True)
    ap.add_argument("--lib", default="libil2cpp.so")
    ap.add_argument("--addr", default="0x1d749418", help="全局地址（file offset 空间）")
    ap.add_argument("--chain", default="8,18,10,78,0,b8,0",
                    help="逐级解引用的偏移（十六进制，逗号分隔），最后一级后读 u32")
    ap.add_argument("--base-delta", default="0", help="基址修正（十六进制，例如 -4000）")
    ap.add_argument("--final-off", default="38", help="末级结构里的 u32 偏移")
    ap.add_argument("--final-len", type=int, default=0x60, help="末级 dump 长度（>=0 则 dump）")
    a = ap.parse_args()

    base, line = lib_base_from_maps(a.serial, a.pid, a.lib)
    if base is None:
        print("ERROR: %s not mapped in pid %d" % (a.lib, a.pid))
        return 2
    delta = int(a.base_delta, 16)
    print("maps line : %s" % line.strip())
    print("base      : 0x%016x (delta %+x)" % (base, delta))

    addr = base + delta + int(a.addr, 16)
    print("root      : 0x%016x" % addr)
    for i, off in enumerate(a.chain.split(",")):
        raw = read_exact(a.serial, a.pid, addr, 8)
        if len(raw) < 8:
            print("  [%d] read fail @0x%016x (%d bytes)" % (i, addr, len(raw)))
            return 3
        val = int.from_bytes(raw, "little")
        print("  [%d] [0x%016x] = 0x%016x" % (i, addr, val))
        if val == 0:
            print("  -> NULL，链路中断")
            return 4
        addr = val + int(off, 16)
    fin = addr + int(a.final_off, 16)
    raw = read_exact(a.serial, a.pid, fin, 4)
    if len(raw) < 4:
        print("  final read fail @0x%016x" % fin)
        return 5
    print("  final u32 @0x%016x = %d (0x%08x)" % (fin, int.from_bytes(raw, "little"),
                                                  int.from_bytes(raw, "little")))
    if a.final_len > 0:
        blob = read_exact(a.serial, a.pid, addr, a.final_len)
        print("  struct dump @0x%016x (%d bytes):" % (addr, len(blob)))
        for i in range(0, len(blob), 16):
            chunk = blob[i:i + 16]
            print("   +%03x  %s" % (i, " ".join("%02x" % b for b in chunk)))
    return 0


if __name__ == "__main__":
    sys.exit(main())
