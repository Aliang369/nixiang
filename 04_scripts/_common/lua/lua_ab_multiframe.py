#!/usr/bin/env python3
# -*- coding: utf-8 -*-
# ============================================================
#  lua_ab_multiframe.py — 从「多帧 LZ4 自定义容器」的 lua AssetBundle 全量抽 Lua 5.3 chunk
#
#  容器结构（2026-11-19 鹦鹉团 1.0.7 实测）:
#    每个 chunk 前是 LZ4 帧（魔数 04 22 4d 18），帧与帧之间可能有填充字节；
#    帧头里的 4 字节长度字段是**压缩后帧长**（≠ 解出长度），不要拿它切文件。
#    正确做法：用 LZ4FrameDecompressor 解第一帧，靠 unused_data 反推真实帧长。
#    实测 lua_csys_dance.unity3d: 837,241 B → 127 帧 → 127 个 Lua chunk（解出 1,806,006 B）
#
#  用法:
#    python3 04_scripts/_common/lua/lua_ab_multiframe.py <bundle|目录> <输出目录>
#  输出:
#    <out>/<bundle>__<序号>.luac   (以 \x1bL 开头 = Lua 5.3 字节码，含 27 字节变体头)
#    <out>/<bundle>__<序号>.bin    (非 Lua 载荷，仅存前 4KB 供判型)
#  下一步: python3 04_scripts/_common/lua/lua53_dis.py <x>.luac > <x>.txt
#  注意: lua53_dis.py 直接吃 27 字节变体头（format=0x01），不要手工改成 32 字节头。
#  ============================================================
import lz4.frame
import os
import sys

MAGIC = b"\x04\x22\x4d\x18"


def split_frames(data):
    """返回 [(start, frame_len, payload_bytes), ...]，按帧序。"""
    out = []
    i, n = 0, len(data)
    while i < n:
        k = data.find(MAGIC, i)
        if k < 0:
            break
        try:
            dec = lz4.frame.LZ4FrameDecompressor()
            payload = dec.decompress(data[k:])
            if dec.eof:
                used = n - k - len(dec.unused_data)
                if used > 0:
                    out.append((k, used, payload))
                    i = k + used
                    continue
        except Exception:
            pass
        i = k + 1
    return out


def main():
    if len(sys.argv) != 3:
        print(__doc__ or "usage: lua_ab_multiframe.py <bundle|dir> <outdir>")
        return 2
    src, dst = sys.argv[1], sys.argv[2]
    os.makedirs(dst, exist_ok=True)
    targets = []
    if os.path.isdir(src):
        for name in sorted(os.listdir(src)):
            p = os.path.join(src, name)
            if os.path.isfile(p) and ".unity3d" in name:
                targets.append(p)
    else:
        targets.append(src)

    total = lua = 0
    for p in targets:
        base = os.path.basename(p).split(".unity3d")[0]
        frames = split_frames(open(p, "rb").read())
        for idx, (_start, _len, chunk) in enumerate(frames):
            total += 1
            if chunk[:2] == b"\x1bL":
                lua += 1
                ext, payload = ".luac", chunk
            else:
                ext, payload = ".bin", chunk[:4096]
            with open(os.path.join(dst, "%s__%03d%s" % (base, idx, ext)), "wb") as f:
                f.write(payload)
        print("%-30s frames=%d" % (base, len(frames)))
    print("TOTAL frames=%d lua_chunks=%d out=%s" % (total, lua, dst))
    return 0


if __name__ == "__main__":
    sys.exit(main())
