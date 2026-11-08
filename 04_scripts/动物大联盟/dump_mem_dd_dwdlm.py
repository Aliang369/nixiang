#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""dwdlm — 用 su+dd 只读通道 dump 目标进程 rw 段，并在本机做多编码关键词扫描。

为什么不用 frida（本轮实测）：新装包后只要 frida-server 在跑，游戏启动 5s 内自杀
（停掉 frida-server 立刻能稳定存活）；因此改用 adb exec-out + su -c dd
（iflag=skip_bytes,count_bytes），纯只读、目标进程无感，实测 1MB 约 0.18s（约 6MB/s）。

输出（默认 09_test/动物大联盟/memscan_20261106）：
  raw/partNN.bin           原始拼接流，段之间夹 MARK 标记行
  manifest.jsonl           每条 {addr,size,file,off,path,perm}
  hits/NNNN_<tag>_<addr>.bin  命中点前后 ctx 字节
  scan_counts.json / scan_index.jsonl
"""
import argparse
import json
import os
import re
import subprocess
import sys
import time

SERIAL = "f9a116a2"
PKG = "com.fightingstars.games.animal"
HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", ".."))
OUT = os.path.join(ROOT, "09_test", "动物大联盟", "memscan_20261106")

KW = ["数据检查", "检查数据", "资源校验", "完整性", "下载", "更新中", "加载中", "校验",
      "checkData", "verifyRes", "hotupdate", "hotUpdate", "resVersion", "AssetsManager",
      "assetManager", "loadBundle", "downloadFile", "jsb.fileUtils", "project.manifest",
      "version.manifest", "yltext", "dwdlm", "assets/", "main/index", ".jsc", "http"]


def sh(cmd, timeout=300):
    return subprocess.run(["adb", "-s", SERIAL, "shell", cmd], capture_output=True,
                          text=True, timeout=timeout, errors="replace")


def get_maps(pid):
    r = sh("su -c 'cat /proc/%d/maps'" % pid)
    return r.stdout or ""


def parse_maps(txt):
    segs = []
    for ln in txt.splitlines():
        m = re.match(r"^([0-9a-f]+)-([0-9a-f]+)\s+(\S+)\s+(\S+)\s+\S+\s+\d+\s*(.*)$", ln.strip())
        if not m:
            continue
        segs.append({"start": int(m.group(1), 16), "end": int(m.group(2), 16),
                     "perm": m.group(3), "path": m.group(5).strip()})
    return segs


def pick(segs, max_mb=64, skip_kw=("dalvik", "/system/framework", "/apex/", "kgsl", "stack_and_tls",
                                   "/dev/", "[vdso]", "[vvar]")):
    sel, skipped = [], []
    for s in segs:
        if "w" not in s["perm"]:
            continue
        sz = s["end"] - s["start"]
        if sz <= 0:
            continue
        p = s["path"]
        if sz > max_mb * 1048576 or any(k in p for k in skip_kw):
            skipped.append((s, sz))
            continue
        sel.append((s, sz))
    return sel, skipped


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--pid", type=int, default=0)
    ap.add_argument("--max-mb", type=int, default=64)
    ap.add_argument("--group-mb", type=int, default=32)
    ap.add_argument("--ctx", type=int, default=3000)
    ap.add_argument("--max-hits", type=int, default=300)
    args = ap.parse_args()

    pid = args.pid
    if not pid:
        o = (sh("pidof %s" % PKG).stdout or "").split()
        pid = int(o[0]) if o else 0
    if not pid:
        print("!! 目标进程不存在")
        return 2
    print("target pid=%d" % pid)

    os.makedirs(os.path.join(OUT, "raw"), exist_ok=True)
    segs = parse_maps(get_maps(pid))
    sel, skipped = pick(segs, args.max_mb)
    total = sum(s for _, s in sel)
    print("segs=%d selected=%d (%.1f MB) skipped=%d (%.1f MB)" % (
        len(segs), len(sel), total / 1048576.0, len(sel) and len(skipped),
        sum(s for _, s in skipped) / 1048576.0))

    token = "SEGMARK7f3c9a1d"
    groups, cur, cursz = [], [], 0
    for s, sz in sel:
        if cur and cursz + sz > args.group_mb * 1048576:
            groups.append(cur)
            cur, cursz = [], 0
        cur.append((s, sz))
        cursz += sz
    if cur:
        groups.append(cur)

    for gi, grp in enumerate(groups):
        parts = []
        for s, sz in grp:
            parts.append("echo; echo '%s 0x%x 0x%x %s'; dd if=/proc/%d/mem iflag=skip_bytes,count_bytes "
                         "skip=%d bs=1048576 count=%d 2>/dev/null" % (token, s["start"], sz, s["perm"],
                                                                     pid, s["start"], sz))
        script = "; ".join(parts)
        path = os.path.join(OUT, "raw", "part%02d.bin" % gi)
        t0 = time.time()
        with open(path, "wb") as fh:
            subprocess.run(["adb", "-s", SERIAL, "exec-out", "su -c", script], stdout=fh, timeout=600)
        print("part%02d %.1f MB in %.1fs" % (gi, os.path.getsize(path) / 1048576.0, time.time() - t0))

    print("splitting ...")
    man = []
    for gi, grp in enumerate(groups):
        path = os.path.join(OUT, "raw", "part%02d.bin" % gi)
        data = open(path, "rb").read()
        pos = 0
        for s, sz in grp:
            pat = ("%s 0x%x 0x%x %s" % (token, s["start"], sz, s["perm"])).encode()
            i = data.find(pat, pos)
            if i < 0:
                print("  !! missing mark 0x%x" % s["start"])
                continue
            vs = i + len(pat)
            while vs < len(data) and data[vs] in (0x0a, 0x0d):
                vs += 1
            seg = data[vs:vs + sz]
            man.append({"addr": s["start"], "size": len(seg), "want": sz, "path": s["path"],
                        "perm": s["perm"], "file": os.path.basename(path), "off": vs})
            pos = vs + len(seg)
    with open(os.path.join(OUT, "manifest.jsonl"), "w", encoding="utf-8") as fh:
        for m in man:
            fh.write(json.dumps(m, ensure_ascii=False) + "\n")
    print("manifest rows=%d bytes=%d" % (len(man), sum(m["size"] for m in man)))

    print("scanning ...")
    pats = []
    for kw in KW:
        pats.append((kw, kw.encode("utf-8")))
        pats.append((kw + "@u16", kw.encode("utf-16-le")))
    hits, rows, idx = {}, [], 0
    ctx = args.ctx
    hints = {}
    for m in man:
        with open(os.path.join(OUT, "raw", m["file"]), "rb") as fh:
            fh.seek(m["off"])
            data = fh.read(m["size"])
        for tag, pat in pats:
            st = data.find(pat)
            while st >= 0:
                key = (m["addr"] + st, tag)
                if key not in hits:
                    hits[key] = 1
                    hints[m["addr"] + st] = (tag, m, st, data)
                    if idx < args.max_hits:
                        idx += 1
                        cs = max(0, st - ctx)
                        ce = min(len(data), st + len(pat) + ctx)
                        sub = data[cs:ce]
                        d = os.path.join(OUT, "hits")
                        os.makedirs(d, exist_ok=True)
                        fn = "%04d_%s_%x.bin" % (idx, re.sub(r"[^0-9A-Za-z_]", "_", tag), m["addr"] + st)
                        open(os.path.join(d, fn), "wb").write(sub)
                        txt = re.sub(rb"[^\x20-\x7e\n]", b".", sub).decode("latin1")
                        rows.append({"idx": idx, "addr": hex(m["addr"] + st), "tag": tag,
                                     "seg": hex(m["addr"]), "path": m["path"], "bin": fn, "text": txt})
                st = data.find(pat, st + 1)
    cnt = {}
    for (a, t) in hits:
        cnt[t] = cnt.get(t, 0) + 1
    summary = {"total_hits": len(hits), "per_tag": cnt, "segments": len(man),
               "bytes_scanned": sum(m["size"] for m in man)}
    with open(os.path.join(OUT, "scan_counts.json"), "w", encoding="utf-8") as fh:
        json.dump(summary, fh, ensure_ascii=False, indent=2)
    with open(os.path.join(OUT, "scan_index.jsonl"), "w", encoding="utf-8") as fh:
        for r in rows:
            fh.write(json.dumps({k: r[k] for k in ("idx", "addr", "tag", "seg", "path", "bin")},
                                ensure_ascii=False) + "\n")
    print(json.dumps(summary, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    sys.exit(main())
