#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""find_live_metadata.py — 在 root 设备上从运行中的进程内存里定位已解密的 il2cpp metadata（只读）。

背景：miHoYo 的 libil2cpp 在运行期把加密的 global-metadata.dat（磁盘 magic=`MHY`）解密到堆里，
      磁盘文件不可直接喂 Il2CppDumper；本脚本扫描每个可读映射的首 16 字节找标准 magic
      `AF 1B B1 FA`（也可用 --magic 指定），命中后可整段 dump 出来。

用法:
  python find_live_metadata.py scan --serial 127.0.0.1:16384 --pkg com.miHoYo.Nap [--pid N]
  python find_live_metadata.py dump --serial ... --pid N --start 0x1234 --size 0x68CA000 --out meta.bin

实现：设备侧只做 dd + base64（都在 /system/bin），扫描逻辑与解析在 PC 侧。
"""
import argparse
import base64
import re
import struct
import subprocess
import sys

MAGIC = b"\xaf\x1b\xb1\xfa"          # Il2Cpp global-metadata.dat 标准 magic (0xFAB11BAF)
ENC_MAGIC = b"MHY\x00"               # miHoYo 加密后的磁盘 magic


def adb(serial, args, timeout=120, text=True, capture=True):
    cmd = ["adb", "-s", serial] + args
    kw = dict(capture_output=capture, timeout=timeout)
    if text:
        kw["text"] = True
    return subprocess.run(cmd, **kw)


def sh(serial, script, timeout=180):
    """把脚本喂给设备的 sh 执行（避免超长命令行与引号问题）。"""
    cmd = ["adb", "-s", serial, "shell", "sh"]
    if isinstance(script, str):
        script = script.encode()
    return subprocess.run(cmd, input=script, capture_output=True, timeout=timeout)


def get_pid(serial, pkg):
    r = adb(serial, ["shell", "pidof", pkg])
    pid = r.stdout.strip().split()
    if not pid:
        raise SystemExit("pidof %s 无输出: %s" % (pkg, r.stderr))
    return int(pid[0])


def get_maps(serial, pid):
    r = adb(serial, ["shell", "cat", "/proc/%d/maps" % pid], timeout=60)
    return r.stdout


def parse_maps(text):
    out = []
    for line in text.splitlines():
        m = re.match(
            r"([0-9a-f]+)-([0-9a-f]+) (\S{4}) ([0-9a-f]+) (\S+) (\d+)\s*(.*)$", line)
        if not m:
            continue
        s, e, perm, off, dev, ino, path = m.groups()
        out.append(dict(start=int(s, 16), end=int(e, 16), perm=perm,
                        off=int(off, 16), dev=dev, ino=int(ino), path=path.strip()))
    return out


def scan(serial, pid, min_mb=1.0, magic=MAGIC):
    maps = parse_maps(get_maps(serial, pid))
    regions = [r for r in maps if "r" in r["perm"] and (r["end"] - r["start"]) >= int(min_mb * 1048576)]
    sys.stderr.write("regions(>=%.1fMB, readable)=%d / total=%d\n" % (min_mb, len(regions), len(maps)))
    hits = []
    BATCH = 150
    for i in range(0, len(regions), BATCH):
        chunk = regions[i:i + BATCH]
        script = ["#!/system/bin/sh"]
        for r in chunk:
            # 读该映射首 32 字节（bs=16，skip=addr/16；只看头部）
            script.append('dd if=/proc/%d/mem bs=16 skip=%d count=2 2>/dev/null | base64 | tr -d "\\n" | sed \'s/^/%d /\''
                          % (pid, r["start"] // 16, r["start"]))
            script.append("echo")
        r = sh(serial, "\n".join(script) + "\n", timeout=300)
        for line in r.stdout.splitlines():
            parts = line.split()
            if len(parts) != 2:
                continue
            addr = int(parts[0])
            try:
                data = base64.b64decode(parts[1])
            except Exception:
                continue
            off = data.find(magic)
            if off >= 0:
                reg = [x for x in chunk if x["start"] == addr]
                info = reg[0] if reg else {}
                hits.append((addr + off, off, info))
                print("[HIT] magic at 0x%x (reg 0x%x size=%.1fMB perm=%s path=%s) off_in_reg=%d"
                      % (addr + off, addr, (info.get("end", 0) - addr) / 1048576.0,
                         info.get("perm"), (info.get("path") or "anon")[:60], off))
        sys.stderr.write("  scanned %d/%d\n" % (min(i + BATCH, len(regions)), len(regions)))
    if not hits:
        print("[MISS] 未在映射首部找到 magic %s" % magic.hex())
    return hits


def scan_full(serial, pid, min_mb=32.0, max_mb=4096.0, magic=MAGIC, keep_dir=None):
    """逐段 dump 后直接在本地搜 magic（比只看头部可靠；用完即删）。"""
    import os
    import tempfile
    maps = parse_maps(get_maps(serial, pid))
    regions = [r for r in maps if "r" in r["perm"]
               and min_mb * 1048576 <= (r["end"] - r["start"]) <= max_mb * 1048576]
    regions.sort(key=lambda r: r["start"])
    total = sum(r["end"] - r["start"] for r in regions)
    sys.stderr.write("full-scan regions=%d total=%.0fMB\n" % (len(regions), total / 1048576.0))
    for r in regions:
        size = (r["end"] - r["start"]) & ~0xFFF
        fd, tmp = tempfile.mkstemp(prefix="nxscan_", suffix=".bin")
        os.close(fd)
        cmd = ["adb", "-s", serial, "exec-out", "dd", "if=/proc/%d/mem" % pid,
               "bs=4096", "skip=%d" % (r["start"] // 4096), "count=%d" % (size // 4096)]
        with open(tmp, "wb") as f:
            subprocess.run(cmd, stdout=f, stderr=subprocess.DEVNULL, timeout=1800)
        d = open(tmp, "rb").read()
        i = d.find(magic)
        print("[%s] 0x%x size=%.1fMB perm=%s path=%s%s"
              % ("HIT" if i >= 0 else "--", r["start"], size / 1048576.0, r["perm"],
                 (r["path"] or "anon")[:40], (" magic@0x%x" % i) if i >= 0 else ""), flush=True)
        if i >= 0 and keep_dir:
            keep = os.path.join(keep_dir, "region_%x.bin" % r["start"])
            os.replace(tmp, keep)
            print("  kept -> %s" % keep)
        else:
            os.unlink(tmp)
    return None


def dump(serial, pid, start, size, out, tmp="/data/local/tmp/nx_meta_dump.bin"):
    blocks = size // 4096
    assert start % 4096 == 0, "start 必须页对齐"
    cmd = ["adb", "-s", serial, "exec-out", "dd", "if=/proc/%d/mem" % pid,
           "bs=4096", "skip=%d" % (start // 4096), "count=%d" % blocks]
    with open(out, "wb") as f:
        r = subprocess.run(cmd, stdout=f, stderr=subprocess.PIPE, timeout=1800)
    if r.returncode != 0:
        print("dd rc=%d stderr=%s" % (r.returncode, r.stderr[:400]))
    import os
    print("local %s bytes -> %s" % (os.path.getsize(out), out))


def main():
    ap = argparse.ArgumentParser()
    sub = ap.add_subparsers(dest="mode", required=True)
    for name in ("scan", "dump", "scanfull"):
        p = sub.add_parser(name)
        p.add_argument("--serial", required=True)
        p.add_argument("--pkg", default="com.miHoYo.Nap")
        p.add_argument("--pid", type=int)
    ap_scan = sub.choices["scan"]
    ap_scan.add_argument("--min-mb", type=float, default=1.0)
    ap_sf = sub.choices["scanfull"]
    ap_sf.add_argument("--min-mb", type=float, default=32.0)
    ap_sf.add_argument("--max-mb", type=float, default=4096.0)
    ap_sf.add_argument("--keep-dir", default=None)
    ap_dump = sub.choices["dump"]
    ap_dump.add_argument("--start", required=True)
    ap_dump.add_argument("--size", required=True)
    ap_dump.add_argument("--out", required=True)
    a = ap.parse_args()
    pid = a.pid or get_pid(a.serial, a.pkg)
    print("pid=%d" % pid)
    if a.mode == "scan":
        scan(a.serial, pid, a.min_mb)
    elif a.mode == "scanfull":
        scan_full(a.serial, pid, a.min_mb, a.max_mb, keep_dir=a.keep_dir)
    else:
        dump(a.serial, pid, int(a.start, 0), int(a.size, 0), a.out)


if __name__ == "__main__":
    main()
