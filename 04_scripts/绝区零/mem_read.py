#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""mem_read.py — 通过 root + /proc/<pid>/mem 读取任意地址（只读，PC 侧展示）。

用法:
  python mem_read.py --serial S --pid N 0xADDR [--len 64] [--out file] [--hex]
  python mem_read.py --serial S --pid N --batch addrs.txt --outdir D   # 每行一个 0xADDR

批量模式为一次性设备侧脚本（dd+base64），适合几十~几百个地址。
"""
import argparse
import base64
import os
import subprocess
import sys


def sh(serial, script, timeout=300):
    return subprocess.run(["adb", "-s", serial, "shell", "sh"], input=script.encode(),
                          capture_output=True, timeout=timeout)


def read_one(serial, pid, addr, length):
    blocks = (length + 15) // 16
    script = ('dd if=/proc/%d/mem bs=16 skip=%d count=%d 2>/dev/null | base64 -w0\n'
              % (pid, addr // 16, blocks))
    r = sh(serial, script)
    try:
        return base64.b64decode(r.stdout.strip())
    except Exception:
        return b""


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--serial", required=True)
    ap.add_argument("--pid", type=int, required=True)
    ap.add_argument("addr", nargs="?")
    ap.add_argument("--len", type=int, default=64)
    ap.add_argument("--out")
    ap.add_argument("--outdir")
    ap.add_argument("--batch")
    a = ap.parse_args()

    if a.batch:
        os.makedirs(a.outdir or ".", exist_ok=True)
        addrs = [int(x, 0) for x in open(a.batch) if x.strip()]
        B = 200
        for i in range(0, len(addrs), B):
            chunk = addrs[i:i + B]
            script = ["#!/system/bin/sh"]
            for ad in chunk:
                script.append('echo %d $(dd if=/proc/%d/mem bs=16 skip=%d count=1 2>/dev/null | base64 -w0)'
                              % (ad, a.pid, ad // 16))
            r = sh(a.serial, "\n".join(script) + "\n")
            for line in r.stdout.splitlines():
                p = line.split()
                if len(p) != 2:
                    continue
                ad = int(p[0])
                data = base64.b64decode(p[1])
                with open(os.path.join(a.outdir, "0x%x.bin" % ad), "wb") as f:
                    f.write(data)
            sys.stderr.write("batch %d/%d\n" % (min(i + B, len(addrs)), len(addrs)))
        return

    addr = int(a.addr, 0)
    data = read_one(a.serial, a.pid, addr, a.len)
    if a.out:
        open(a.out, "wb").write(data)
        print("wrote %d bytes -> %s" % (len(data), a.out))
    else:
        for off in range(0, len(data), 16):
            row = data[off:off + 16]
            hexs = " ".join("%02x" % b for b in row)
            txt = "".join(chr(b) if 32 <= b < 127 else "." for b in row)
            print("%012x  %-47s  %s" % (addr + off, hexs, txt))


if __name__ == "__main__":
    main()
