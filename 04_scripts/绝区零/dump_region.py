#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""dump_region.py — 从 root 设备进程内存 dump 任意区域到本地文件（只读）。

用法:
  python dump_region.py --serial 127.0.0.1:16384 --pid 43628 \
      --addr 0x5f80001000 --size 0x14c90000 --out region.bin

实现：PC 侧按 4MB 分块，逐块 adb exec-out dd 落盘（对不可读页返回短读，遇短读即停并报告）。
"""
import argparse
import os
import subprocess
import sys

BLOCK = 4 << 20


def dump_block(serial, pid, addr, size, out_fh):
    skip = addr // 4096
    count = (size + 4095) // 4096
    cmd = ["adb", "-s", serial, "exec-out",
           "dd if=/proc/%d/mem bs=4096 skip=%d count=%d 2>/dev/null" % (pid, skip, count)]
    r = subprocess.run(cmd, capture_output=True, timeout=600)
    out_fh.write(r.stdout)
    return len(r.stdout)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--serial", required=True)
    ap.add_argument("--pid", type=int, required=True)
    ap.add_argument("--addr", required=True, type=lambda x: int(x, 0))
    ap.add_argument("--size", required=True, type=lambda x: int(x, 0))
    ap.add_argument("--out", required=True)
    a = ap.parse_args()

    done = 0
    with open(a.out, "wb") as f:
        while done < a.size:
            n = min(BLOCK, a.size - done)
            got = dump_block(a.serial, a.pid, a.addr + done, n, f)
            f.flush()
            done += got
            sys.stderr.write("\r%.1f%% (%d/%d MB)" % (100.0 * done / a.size,
                                                      done >> 20, a.size >> 20))
            sys.stderr.flush()
            if got < n:
                sys.stderr.write("\nshort read at +0x%x, stop\n" % done)
                break
    sys.stderr.write("\n")


if __name__ == "__main__":
    main()
