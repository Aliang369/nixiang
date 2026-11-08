#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""dwdlm 1.0.25 — 只读 mem 跟随探针（v3）

背景（本轮实测）：
 - frida spawn + 注入 app 后，0x55a78 在 264ms 被调用一次即再无消息，进程约 1s 内结束
   （detached=connection-terminated）。对比上一轮只 attach helper、不碰 app 的 memread
   运行活满 30s，可判定注入本身触发目标自身保护（壳的代码段/线程校验）。
 - 因此改用父代理方案 5：su 循环读 /proc/<pid>/mem，完全不改目标进程代码。

静态依据（libsecret.so e0a3169...）：
 - 0x56000..0x56014: x21=0x786a4, x20=x21+0x1b4=0x78858, x0=x20, x1=&sp+0x130(局部 hex 串)
   bl 0x4cfd0  => *(0x78858) = 64 字符 hex 串（std::string）
 - 0x5601c/0x56024/0x56028: *(0x786a8)=1, *(0x786a4)=1（flag）
 - 0x55fbc..0x55ffc: 32 字节 -> 0x12114 的 "0123456789abcdef" 表 -> 64 字符
 - 0x78858 对象布局实测为 {u64 0x51, u64 0x40(=64), char* data}，data 在 +16

采样：每 --interval ms 一次，共 --rounds 次；窗口 0x786b0..0x788c0（含 flag/KEY/IV/hex 对象），
      flag==1 时跟随 0x78858+16 的指针读 128 字节（真正的 64 字符 hex 串）。
"""
import argparse
import base64
import binascii
import json
import os
import re
import shutil
import subprocess
import sys
import time

PKG = "com.fightingstars.games.animal"
SERIAL = "f9a116a2"
HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", ".."))
OUT = os.path.join(ROOT, "09_test", "动物大联盟", "sigprobe_20261105")
ADB = shutil.which("adb") or "/opt/homebrew/bin/adb"

WIN_START = 0x786a0
WIN_LEN = 0x240
OFF_FLAG = 0x786a4
OFF_FLAG2 = 0x786a8
OFF_HEXOBJ = 0x78858
OFF_KEY = 0x788a0
OFF_IV = 0x787a0


def adb(args, timeout=30):
    return subprocess.run([ADB, "-s", SERIAL] + args, capture_output=True, text=True,
                          timeout=timeout, errors="replace")


def su(cmd, timeout=30):
    r = adb(["shell", "su -c " + json.dumps(cmd)], timeout=timeout)
    return r.stdout or ""


def read_mem(pid, addr, n):
    out = su("dd if=/proc/%d/mem bs=1 skip=%d count=%d 2>/dev/null | base64" % (pid, addr, n))
    b64 = "".join(out.split())
    b64 = b64.strip()
    if not b64:
        return None
    try:
        data = base64.b64decode(b64 + "=" * (-len(b64) % 4))
        return data if data else None
    except Exception:
        return None


def find_pid():
    r = adb(["shell", "pidof %s" % PKG])
    s = (r.stdout or "").strip()
    return int(s.split()[0]) if s and s.split()[0].isdigit() else None


def find_base(pid):
    out = su("cat /proc/%d/maps 2>/dev/null | grep libsecret.so" % pid)
    hits, base = [], None
    for ln in out.splitlines():
        m = re.match(r"^([0-9a-f]+)-([0-9a-f]+)\s+(\S+)\s+([0-9a-f]+)\s+\S+\s+\d+\s+(.*)$", ln.strip())
        if not m:
            continue
        hits.append(ln.strip())
        st, en, perm, off, path = int(m.group(1), 16), int(m.group(2), 16), m.group(3), m.group(4), m.group(5)
        if base is None and "x" in perm and off == "00000000":
            base = st
    return base, hits


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--rounds", type=int, default=40)
    ap.add_argument("--interval", type=float, default=0.1)
    ap.add_argument("--launch", action="store_true", help="自己用 monkey 启动游戏")
    ap.add_argument("--pid", type=int, default=0)
    args = ap.parse_args()

    os.makedirs(OUT, exist_ok=True)
    logp = os.path.join(OUT, "probe_v3.log")
    rawp = os.path.join(OUT, "probe_v3.jsonl")
    t0 = time.time()

    with open(logp, "w", encoding="utf-8") as fh, open(rawp, "w", encoding="utf-8") as raw:
        def log(s):
            line = "[%6.2fs] %s" % (time.time() - t0, s)
            print(line, flush=True)
            fh.write(line + "\n")
            fh.flush()

        log("# dwdlm 只读 mem 跟随探针 v3  %s" % time.strftime("%Y-%m-%d %H:%M:%S"))
        log("# pkg=%s rounds=%d interval=%.0fms win=0x%x+0x%x" % (PKG, args.rounds, args.interval * 1000, WIN_START, WIN_LEN))
        log("# SELinux=%s" % (adb(["shell", "getenforce"]).stdout or "").strip())

        pid = args.pid or find_pid()
        if args.launch or not pid:
            r = adb(["shell", "monkey -p %s -c android.intent.category.LAUNCHER 1" % PKG], timeout=40)
            log("# 启动游戏(monkey): rc=%d %s" % (r.returncode, (r.stdout or "").strip().splitlines()[-1:] or ""))
            for _ in range(60):
                time.sleep(0.1)
                pid = find_pid()
                if pid:
                    break
        if not pid:
            log("[FAIL] 拿不到 pid")
            return 2
        log("# 目标 pid=%d" % pid)

        base = None
        for _ in range(150):
            base, hits = find_base(pid)
            if base:
                break
            time.sleep(0.1)
        if not base:
            log("[FAIL] maps 里没有 libsecret.so")
            return 3
        log("# base=0x%x  maps:" % base)
        for h in hits:
            log("   %s" % h)

        seen_keys, seen_hex, samples = set(), set(), 0
        win_addr = base + WIN_START
        ok_samples = 0
        for i in range(args.rounds):
            t_round = time.time()
            data = read_mem(pid, win_addr, WIN_LEN)
            cur_pid = find_pid()
            if cur_pid and cur_pid != pid:
                log("# pid 变化 %d -> %d，重新解析 base" % (pid, cur_pid))
                pid = cur_pid
                nb, hits = find_base(pid)
                if nb:
                    base = nb
                    win_addr = base + WIN_START
                    log("# 新 base=0x%x" % base)
            if not data or len(data) < WIN_LEN:
                log("[miss] round=%d 读到 %s 字节（进程可能已退）" % (i, len(data) if data else 0))
                if not cur_pid:
                    log("# 目标进程已不在，结束采样")
                    break
                time.sleep(args.interval)
                continue
            ok_samples += 1
            u32 = lambda off: int.from_bytes(data[off - WIN_START:off - WIN_START + 4], "little")
            get = lambda off, n: data[off - WIN_START:off - WIN_START + n]
            flag = u32(OFF_FLAG)
            flag2 = u32(OFF_FLAG2)
            key = get(OFF_KEY, 32).hex()
            iv = get(OFF_IV, 16).hex()
            obj = get(OFF_HEXOBJ, 24)
            f0 = int.from_bytes(obj[0:8], "little")
            f8 = int.from_bytes(obj[8:16], "little")
            f16 = int.from_bytes(obj[16:24], "little")
            rec = {"t": "sample", "round": i, "pid": pid, "base": "0x%x" % base,
                   "at_ms": round((time.time() - t0) * 1000, 1),
                   "flag": flag, "flag2": flag2, "key": key, "iv": iv,
                   "hexobj_q0": "0x%x" % f0, "hexobj_q1": "0x%x" % f8, "hexobj_ptr": "0x%x" % f16,
                   "hexobj_raw": obj.hex()}
            # 跟随 data 指针
            if f16 > 0x10000:
                # Android ARM64 指针常带 top-byte 标签(TBI/MTE)：0xb4.. 先去掉 top byte 再试原样
                cands = []
                low = f16 & 0x00FFFFFFFFFFFFFF
                if low != f16 and low > 0x10000:
                    cands.append(low)
                cands.append(f16)
                d = None
                used = None
                for a in cands:
                    d = read_mem(pid, a, 96)
                    if d and len(d) >= 32:
                        used = a
                        break
                    d = None
                rec["follow_addr_try"] = ["0x%x" % a for a in cands]
                if d:
                    asc = d.split(b"\x00")[0]
                    rec["follow_addr_used"] = "0x%x" % used
                    rec["follow_len"] = len(d)
                    rec["follow_hex"] = d.hex()
                    rec["follow_ascii"] = asc.decode("latin1")
                    if re.fullmatch(rb"[0-9a-f]{64}", asc):
                        rec["follow_is_hex64"] = True
                        if asc.decode() not in seen_hex:
                            seen_hex.add(asc.decode())
                            log("# >>> 64 字符 hex 串: %s" % asc.decode())
                    elif asc[:2] == b"0\x82":
                        rec["follow_is_der"] = True
                        log("# >>> DER(3082) 数据 len=%d head=%s" % (len(asc), asc[:16].hex()))
            if key.strip("0") and key not in seen_keys:
                seen_keys.add(key)
                log("# KEY(非零) %s  iv=%s  at_round=%d" % (key, iv, i))
            if rec.get("follow_is_hex64") or flag == 1:
                samples += 1
                log("[hit] r=%d flag=1 key=%s.. hexobj={0x%x,0x%x,0x%x} follow=%s"
                    % (i, key[:16], f0, f8, f16, rec.get("follow_ascii", "-")[:80]))
            raw.write(json.dumps(rec, ensure_ascii=False) + "\n")
            raw.flush()
            dt = time.time() - t_round
            if dt < args.interval:
                time.sleep(args.interval - dt)

        log("# 汇总: ok_samples=%d flag1_samples=%d distinct_keys=%d distinct_hex=%d"
            % (ok_samples, samples, len(seen_keys), len(seen_hex)))
        if seen_hex:
            for h in seen_hex:
                log("# hex64 = %s" % h)
        if seen_keys:
            for k in seen_keys:
                log("# KEY   = %s" % k)
    return 0


if __name__ == "__main__":
    sys.exit(main())
