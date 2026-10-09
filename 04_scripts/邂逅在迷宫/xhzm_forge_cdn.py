#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
xhzm_forge_cdn.py —— 生成「伪造热更 CDN 应答集」，用于免 root / 免 PC 投递补丁 bundle。

原理（邂逅在迷宫 cn.alltune.endless 2.6.53 实测，2026-10-10）：
  客户端**每次冷启动**都会明文 HTTP 拉取
      GET /bundle3/2.6.53.13/Android/versions.json   Host: install2.hotune.com
  然后按 data[] 里的 file/hash 与本地文件比对，缺什么就去
      http://install2.hotune.com/bundle3/            （扁平基址）
  拉什么。因此只要把 install2.hotune.com 解析到自己可控的 HTTP 服务器，
  返回篡改过的 versions.json / buildpatch 清单，客户端就会**自己**把补丁
  bundle 写进它的 Android/data —— 全程不需要 root、不需要电脑。

哈希算法（已实测确认）：
  * JSON 清单：文件名尾部 32-hex == **md5(文件内容)**（buildpatch_v63_3e1d… 验证通过）
  * .bundle ：文件名尾部 32-hex == bundle 内 41 字节 xasset 前缀 [1:33] 里自带的构建哈希
              （官方 endlessstrikedll 文件 md5=7c85c8f3… ≠ 8fa83621…，故不是内容 md5）

产物（输出目录 cdn/）：
  1. endlessstrikedll_<H3>.bundle      H3 = 补丁 DLL 的 md5（改名 + 改前缀哈希，强制客户端"没这个文件"）
  2. buildpatch_v63_<H2>.json          H2 = md5(清单内容)；bundle0 指向 H3 与新 size
  3. versions.json                     BuildPatch 项指向 H2 清单；BuildNormal 保持不变

用法：
    python3 xhzm_forge_cdn.py --official <官方bundle> --patched <补丁bundle> \
        --buildpatch <官方buildpatch.json> --versions <官方versions.json> --out cdn/
"""
import argparse
import hashlib
import json
import os
import struct
import sys

PREFIX_LEN = 41


def sha(s):
    return hashlib.md5(s).hexdigest()


def forge(official_bundle, patched_bundle, buildpatch_src, versions_src, outdir):
    os.makedirs(outdir, exist_ok=True)

    off_raw = open(official_bundle, "rb").read()
    pat_raw = open(patched_bundle, "rb").read()

    # ---- 1. 补丁 bundle：重命名 + 前缀哈希改成 H3 ----
    if pat_raw.find(b"UnityFS") != PREFIX_LEN:
        sys.exit("补丁 bundle 前缀异常（找不到 41 字节 xasset 前缀）")
    # 从 bundle 前缀里取出官方哈希用于日志
    old_hash = pat_raw[1:33].decode("ascii", "replace")

    # 新构建哈希 H3：默认 = 补丁 bundle 全文件 md5（唯一、可追溯），可用 --hash 覆盖
    H3 = HASH_OVERRIDE or hashlib.md5(pat_raw).hexdigest()

    new_prefix = b" " + H3.encode("ascii") + struct.pack("<I", len(pat_raw)) + b"\x00\x00\x00\x00"
    patched_out = new_prefix + pat_raw[PREFIX_LEN:]
    assert len(patched_out) == len(pat_raw), "重写前缀后长度不应变化"

    bundle_name = f"endlessstrikedll_{H3}.bundle"
    open(os.path.join(outdir, bundle_name), "wb").write(patched_out)

    # ---- 2. 伪造 buildpatch 清单：bundle0 换成新 hash/size ----
    bp = json.loads(open(buildpatch_src, "rb").read().decode("utf-8"))
    hit = 0
    for b in bp["bundles"]:
        if b["name"] == "endlessstrikedll.bundle":
            b["size"] = len(patched_out)
            b["hash"] = H3
            hit += 1
    if hit != 1:
        sys.exit(f"官方 buildpatch 里 endlessstrikedll.bundle 命中 {hit} 次，预期 1")

    bp_body = json.dumps(bp, ensure_ascii=False, separators=(",", ":")).encode("utf-8")
    H2 = sha(bp_body)
    bp_name = f"buildpatch_v{bp['version']}_{H2}.json"
    open(os.path.join(outdir, bp_name), "wb").write(bp_body)

    # ---- 3. 伪造 versions.json：BuildPatch 指向新清单 ----
    vj = json.loads(open(versions_src, "rb").read().decode("utf-8"))
    hit = 0
    for d in vj["data"]:
        if d["name"] == "BuildPatch":
            d["file"] = bp_name
            d["size"] = len(bp_body)
            d["hash"] = H2
            hit += 1
    if hit != 1:
        sys.exit(f"官方 versions.json 里 BuildPatch 命中 {hit} 次，预期 1")
    vj_body = json.dumps(vj, ensure_ascii=False, separators=(",", ":")).encode("utf-8")
    open(os.path.join(outdir, "versions.json"), "wb").write(vj_body)

    print(f"[forge] 输出目录 : {outdir}")
    print(f"[forge] 官方哈希 : {old_hash}  全文件 md5={hashlib.md5(pat_raw).hexdigest()}  size={len(pat_raw)}")
    print(f"[forge] 新构建哈希 H3 = {H3}   -> {bundle_name}  size={len(patched_out)}")
    print(f"[forge] 清单哈希   H2 = {H2}   -> {bp_name}  size={len(bp_body)}")
    print(f"[forge] versions.json size={len(vj_body)}  BuildPatch={bp_name}")
    print(f"[forge] 官方 bundle size={len(off_raw)} (仅供参考)")


HASH_OVERRIDE = None

if __name__ == "__main__":
    ap = argparse.ArgumentParser()
    ap.add_argument("--official", required=True)
    ap.add_argument("--patched", required=True)
    ap.add_argument("--buildpatch", required=True)
    ap.add_argument("--versions", required=True)
    ap.add_argument("--out", default="cdn")
    ap.add_argument("--hash", help="指定新的 bundle 构建哈希（默认 = 补丁 bundle 全文件 md5）")
    a = ap.parse_args()
    HASH_OVERRIDE = a.hash.lower() if a.hash else None
    if HASH_OVERRIDE is not None and (len(HASH_OVERRIDE) != 32 or any(c not in "0123456789abcdef" for c in HASH_OVERRIDE)):
        sys.exit("--hash 必须是 32 位小写十六进制")
    forge(a.official, a.patched, a.buildpatch, a.versions, a.out)
