#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""dwdlm — 只读内存字符串扫描 v2（frida root helper + /proc/<pid>/mem）

v1 结论（实测）：只用 UTF-8 关键词扫了 912 个 rw 段 / 1.76 GB，中文零命中
（只有 resVersion/assets//yltext/.jsc 共 11 条 ASCII 命中，都落在 Java 堆的 dex 字符串区）。
=> Cocos JSB(V8) 里的中文是 UTF-16LE 双字节串，必须按 UTF-16LE 再搜一遍。
=> 另加 runtime=v8（v1 用 QuickJS 扫 1.76GB 要 100s，逐字节循环太慢）。

通道（实测可用）：frida-server 由 su 启动（root/ksu 域），spawn /system/bin/sleep 做 helper，
在 helper 里用 File API 只读 /proc/<target>/mem。Frida 17 的 File 只有 readBytes()（无 readAll /
readByteArray），seek(off,0) 可用；4MB 读取约 16ms。

产物：
  09_test/动物大联盟/memscan_20261106/hits/NNNN_<tag>_<addr>.bin/.txt
  09_test/动物大联盟/memscan_20261106/hits/index.jsonl
  09_test/动物大联盟/memscan_20261106/summary.txt
"""
import argparse
import json
import os
import re
import sys
import time

import frida

SERIAL = "f9a116a2"
PKG = "com.fightingstars.games.animal"
HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", ".."))
OUT = os.path.join(ROOT, "09_test", "动物大联盟", "memscan_20261106")

# (关键词, 编码)  编码: u8 = UTF-8, u16 = UTF-16LE, both = 两种都搜
KW = [
    ("数据检查", "both"), ("检查数据", "both"), ("资源校验", "both"), ("完整性", "both"),
    ("下载", "both"), ("更新中", "both"), ("加载中", "both"), ("校验", "both"),
    ("checkData", "both"), ("verifyRes", "both"), ("hotupdate", "both"), ("hotUpdate", "both"),
    ("resVersion", "both"), ("AssetsManager", "both"), ("assetManager", "both"),
    ("loadBundle", "both"), ("downloadFile", "both"), ("jsb.fileUtils", "both"),
    ("project.manifest", "both"), ("version.manifest", "both"),
    ("yltext", "both"), ("dwdlm", "both"), ("assets/", "u8"), ("main/index", "u8"),
    (".jsc", "u8"), ("http://", "u8"), ("https://", "u8"),
]

AGENT = r"""
var KWS = __KWS__;
var CHUNK = 1048576;
var CTX = __CTX__;

function u8bytes(s) {
  var out = [];
  for (var i = 0; i < s.length; i++) {
    var c = s.charCodeAt(i);
    if (c < 0x80) out.push(c);
    else if (c < 0x800) out.push(0xc0 | (c >> 6), 0x80 | (c & 0x3f));
    else out.push(0xe0 | (c >> 12), 0x80 | ((c >> 6) & 0x3f), 0x80 | (c & 0x3f));
  }
  return out;
}

function u16bytes(s) {
  var out = [];
  for (var i = 0; i < s.length; i++) { var c = s.charCodeAt(i); out.push(c & 0xff, (c >> 8) & 0xff); }
  return out;
}

var PATS = [];
(function () {
  for (var i = 0; i < KWS.length; i++) {
    var kw = KWS[i][0], enc = KWS[i][1];
    if (enc === "u8" || enc === "both") PATS.push({ kw: kw, tag: kw, b: u8bytes(kw) });
    if (enc === "u16" || enc === "both") PATS.push({ kw: kw, tag: kw + "@u16", b: u16bytes(kw) });
  }
})();
var FIRST = new Uint8Array(256);
for (var i = 0; i < PATS.length; i++) FIRST[PATS[i].b[0]] = 1;

function scanBuf(u8, base, seen, out) {
  var n = u8.length;
  for (var i = 0; i < n; i++) {
    if (FIRST[u8[i]] === 0) continue;
    for (var p = 0; p < PATS.length; p++) {
      var pat = PATS[p].b, m = pat.length;
      if (u8[i] !== pat[0] || i + m > n) continue;
      var ok = true;
      for (var j = 1; j < m; j++) { if (u8[i + j] !== pat[j]) { ok = false; break; } }
      if (!ok) continue;
      var addr = base + i, key = addr + ":" + p;
      if (seen[key]) continue;
      seen[key] = 1;
      out.push({ addr: addr, pat: p, kw: PATS[p].kw, tag: PATS[p].tag, plen: m });
    }
  }
  return out;
}

function printable(u8) {
  var out = [];
  for (var i = 0; i < u8.length; i++) {
    var c = u8[i];
    if (c === 0x0a) out.push("\n");
    else if (c >= 0x20 && c < 0x7f) out.push(String.fromCharCode(c));
    else out.push(".");
  }
  return out.join("");
}

function parseMaps(txt) {
  var segs = [], lines = txt.split("\n");
  for (var i = 0; i < lines.length; i++) {
    var m = /^([0-9a-f]+)-([0-9a-f]+)\s+(\S+)\s+(\S+)\s+\S+\s+\d+\s*(.*)$/.exec(lines[i]);
    if (!m) continue;
    segs.push({ start: parseInt(m[1], 16), end: parseInt(m[2], 16), perm: m[3], off: m[4], path: (m[5] || "").trim() });
  }
  return segs;
}

function doScan(targetPid, opts) {
  var res = { segsTotal: 0, segsScanned: 0, segsSkippedBig: 0, bytesScanned: 0,
              hits: 0, perKw: {}, errors: 0, bigSegs: [] };
  var mf = new File("/proc/" + targetPid + "/maps", "r");
  var segs = parseMaps(mf.readText(8 * 1024 * 1024));
  mf.close();
  res.segsTotal = segs.length;

  var mem = new File("/proc/" + targetPid + "/mem", "r");
  var ctxF = new File("/proc/" + targetPid + "/mem", "r");
  var seen = {}, idx = 0;

  for (var s = 0; s < segs.length; s++) {
    var seg = segs[s], size = seg.end - seg.start;
    if (seg.perm.indexOf("w") < 0 || size <= 0) continue;
    if (size > opts.maxSegMb * 1048576) {
      res.segsSkippedBig++;
      res.bigSegs.push({ addr: "0x" + seg.start.toString(16), mb: Math.round(size / 1048576), path: seg.path });
      if (opts.skipBig) continue;
    }
    try { mem.seek(seg.start, 0); } catch (e) { res.errors++; continue; }
    res.segsScanned++;
    var pos = seg.start, segHits = [];
    while (pos < seg.end) {
      var want = Math.min(CHUNK, seg.end - pos), buf = null;
      try { buf = mem.readBytes(want); }
      catch (e) {
        res.errors++;
        var small = Math.min(65536, want);
        try { mem.seek(pos + small, 0); } catch (e2) { break; }
        pos += small;
        continue;
      }
      if (!buf || buf.byteLength === 0) break;
      var u8 = new Uint8Array(buf);
      scanBuf(u8, pos, seen, segHits);
      res.bytesScanned += buf.byteLength;
      pos += buf.byteLength;
      if (buf.byteLength < want) break;
    }
    if (segHits.length) {
      send({ type: "seghits", seg: "0x" + seg.start.toString(16), path: seg.path,
             mb: Math.round(size / 1048576), n: segHits.length });
    }
    for (var h = 0; h < segHits.length; h++) {
      var hit = segHits[h];
      res.hits++;
      res.perKw[hit.tag] = (res.perKw[hit.tag] || 0) + 1;
      if (opts.maxHits > 0 && idx >= opts.maxHits) continue;
      var cs = Math.max(0, hit.addr - CTX), ce = hit.addr + hit.plen + CTX, got = null;
      try { ctxF.seek(cs, 0); got = ctxF.readBytes(ce - cs); } catch (e) { res.errors++; }
      if (!got) continue;
      var cu = new Uint8Array(got);
      idx++;
      send({ type: "hit", idx: idx, addr: "0x" + hit.addr.toString(16), kw: hit.kw,
             tag: hit.tag, seg: "0x" + seg.start.toString(16), path: seg.path,
             text: printable(cu).substring(0, 6000) }, cu.buffer);
    }
  }
  try { mem.close(); } catch (e) {}
  try { ctxF.close(); } catch (e) {}
  return res;
}

rpc.exports = { scan: function (pid, opts) {
  try { return doScan(pid, opts); } catch (e) { return { fatal: String(e) }; }
} };
"""


def on_message(msg, data):
    if msg.get("type") == "error":
        print("[frida-error] %s" % (msg.get("stack", msg)), flush=True)
        return
    if msg.get("type") != "send":
        return
    p = msg.get("payload") or {}
    if p.get("type") == "seghits":
        print("[seg] %s %sMB %s -> %d" % (p["seg"], p["mb"], p["path"][:44], p["n"]), flush=True)
    elif p.get("type") == "hit":
        d = os.path.join(OUT, "hits")
        os.makedirs(d, exist_ok=True)
        safe = re.sub(r"[^0-9A-Za-z_]", "_", p["tag"])
        fn = "%04d_%s_%s" % (p["idx"], safe, p["addr"].replace("0x", ""))
        raw = data or b""
        with open(os.path.join(d, fn + ".bin"), "wb") as fh:
            fh.write(raw)
        with open(os.path.join(d, fn + ".txt"), "w", encoding="utf-8") as fh:
            fh.write("kw=%s tag=%s addr=%s seg=%s path=%s\n%s\n" % (
                p["kw"], p["tag"], p["addr"], p["seg"], p["path"], p["text"]))
        with open(os.path.join(d, "index.jsonl"), "a", encoding="utf-8") as fh:
            fh.write(json.dumps({k: p[k] for k in ("idx", "addr", "kw", "tag", "seg", "path")},
                                ensure_ascii=False) + "\n")
        print("\n##### HIT #%d tag=%s addr=%s seg=%s %s" % (p["idx"], p["tag"], p["addr"], p["seg"], p["path"][:50]))
        print(p["text"][:700])
        print("##### (%s)\n" % fn, flush=True)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--pid", type=int, default=0)
    ap.add_argument("--host", default="127.0.0.1:27042")
    ap.add_argument("--runtime", default="v8")
    ap.add_argument("--max-seg-mb", type=int, default=64)
    ap.add_argument("--skip-big", type=int, default=1)
    ap.add_argument("--ctx", type=int, default=3000)
    ap.add_argument("--max-hits", type=int, default=400)
    ap.add_argument("--helper-sec", type=int, default=600)
    args = ap.parse_args()

    pid = args.pid
    if not pid:
        r = os.popen("adb -s %s shell pidof %s" % (SERIAL, PKG)).read().strip()
        pid = int(r.split()[0]) if r.split() else 0
    if not pid:
        print("!! 目标进程不存在")
        return 2

    os.makedirs(OUT, exist_ok=True)
    idxp = os.path.join(OUT, "hits", "index.jsonl")
    if os.path.exists(idxp):
        os.remove(idxp)

    dev = frida.get_device_manager().add_remote_device(args.host)
    helper = dev.spawn(["/system/bin/sleep", str(args.helper_sec)])
    print("helper pid=%d target pid=%d runtime=%s" % (helper, pid, args.runtime), flush=True)
    sess = dev.attach(helper)
    src = AGENT.replace("__KWS__", json.dumps(KW, ensure_ascii=False)).replace("__CTX__", str(args.ctx))
    try:
        script = sess.create_script(src, runtime=args.runtime)
    except Exception as e:
        print("!! runtime=%s 不可用(%s)，回退默认" % (args.runtime, e), flush=True)
        script = sess.create_script(src)
    script.on("message", on_message)
    script.load()
    dev.resume(helper)

    t0 = time.time()
    res = script.exports_sync.scan(pid, {"maxSegMb": args.max_seg_mb, "skipBig": bool(args.skip_big),
                                        "maxHits": args.max_hits})
    dt = time.time() - t0
    res["wall_s"] = round(dt, 1)
    with open(os.path.join(OUT, "summary.txt"), "w", encoding="utf-8") as fh:
        fh.write("# dwdlm 内存扫描 v2  %s\n" % time.strftime("%Y-%m-%d %H:%M:%S"))
        fh.write("# pid=%d helper=%d runtime=%s skipBig=%d maxSegMb=%d\n" % (
            pid, helper, args.runtime, args.skip_big, args.max_seg_mb))
        fh.write("# keywords=%s\n" % json.dumps(KW, ensure_ascii=False))
        fh.write(json.dumps(res, ensure_ascii=False, indent=2) + "\n")
    print("\n=== SUMMARY ===")
    print(json.dumps({k: v for k, v in res.items() if k != "bigSegs"}, ensure_ascii=False, indent=2))
    try:
        sess.detach()
    except Exception:
        pass
    return 0


if __name__ == "__main__":
    sys.exit(main())
