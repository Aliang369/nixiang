#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
xhzm_repack_bundle.py —— 把补丁后的 .NET 程序集塞回 xasset/UnityFS bundle。

背景（邂逅在迷宫 cn.alltune.endless 2.6.53 实测）：
  * 热更 bundle 不是裸 UnityFS，前面有 **41 字节 xasset 自定义前缀**：
        [0]      ' ' (0x20)
        [1:33]   32 个 ASCII 十六进制字符 = bundle 内容哈希（与文件名后缀一致）
        [33:37]  uint32 LE = **整个文件总长度**
        [37:41]  uint32 LE = 0
    ⇒ 回包时必须重算 [33:37]，否则 xasset 侧长度校验会拒绝。
  * UnityFS 数据块里 TextAsset 的 m_Script 就是 .NET 程序集本体（m_Name = "EndlessStrike.dll"）。
    实测该 TextAsset 是 serialized file 中**最后一个对象**，所以变长不会移动其它对象的 byteStart；
    但 UnityPy 的 save() 会整体重建 blocksInfo / 数据块，长度变化可安全吸收。

用法：
    python3 xhzm_repack_bundle.py <原bundle> <补丁后的.dll> <输出bundle> [--packer lz4hc]
    python3 xhzm_repack_bundle.py --verify <bundle> [期望的.dll]

依赖：UnityPy（工作区用 venv：/tmp/maze_recon/venv/bin/python，或 export XHZM_PY=...）
"""
import io
import struct
import sys
import hashlib

try:
    import UnityPy
except ImportError:  # pragma: no cover
    sys.stderr.write(
        "缺少 UnityPy。用 venv 安装：\n"
        "  python3 -m venv /tmp/xhzm/venv && /tmp/xhzm/venv/bin/pip install UnityPy\n"
        "然后：/tmp/xhzm/venv/bin/python xhzm_repack_bundle.py ...\n"
    )
    raise

PREFIX_LEN = 41


def split_prefix(raw):
    i = raw.find(b"UnityFS")
    if i < 0:
        raise SystemExit("找不到 UnityFS 签名，不是 UnityFS/xasset bundle")
    prefix = raw[:i]
    if len(prefix) != PREFIX_LEN:
        sys.stderr.write(f"[warn] 前缀长度 {len(prefix)} != {PREFIX_LEN}，按实际长度处理\n")
    return prefix, raw[i:]


def fix_prefix(prefix, total_len):
    """把前缀里的总长度字段改写成 total_len。"""
    if len(prefix) < PREFIX_LEN:
        raise SystemExit(f"前缀太短（{len(prefix)}），无法写入长度字段")
    return prefix[:33] + struct.pack("<I", total_len) + prefix[37:PREFIX_LEN]


def load_bundle(body):
    env = UnityPy.load(io.BytesIO(body))
    bun = env.files[list(env.files.keys())[0]]
    return env, bun


def find_dll_textasset(env):
    """返回第一个 m_Script 以 MZ 开头的 TextAsset 的 (reader, typetree, bytes, name, codec)"""
    for o in env.objects:
        if o.type.name != "TextAsset":
            continue
        tt = o.read_typetree()
        s = tt.get("m_Script")
        if s is None:
            continue
        if isinstance(s, str):
            data = s.encode("utf-8", "surrogateescape")
            codec = "surrogateescape"
        else:
            data = bytes(s)
            codec = "bytes"
        if data[:2] == b"MZ":
            return o, tt, data, tt.get("m_Name"), codec
    raise SystemExit("bundle 里找不到 m_Script 以 MZ 开头的 TextAsset")


def verify(path, expect=None):
    raw = open(path, "rb").read()
    prefix, body = split_prefix(raw)
    env, bun = load_bundle(body)
    o, tt, data, name, codec = find_dll_textasset(env)
    size_field = struct.unpack("<I", prefix[33:37])[0]
    print(f"[verify] {path}")
    print(f"         文件长度      = {len(raw)}")
    print(f"         前缀声明长度  = {size_field}  {'OK' if size_field == len(raw) else 'MISMATCH'}")
    print(f"         前缀哈希      = {prefix[1:33].decode('ascii', 'replace')}")
    print(f"         TextAsset     = {name!r}  {len(data)} B  md5={hashlib.md5(data).hexdigest()}")
    ok = size_field == len(raw)
    if expect:
        exp = open(expect, "rb").read()
        same = exp == data
        print(f"         期望 DLL md5  = {hashlib.md5(exp).hexdigest()}  {'OK' if same else 'MISMATCH'}")
        ok = ok and same
    return ok


def main():
    a = sys.argv[1:]
    packer = "lz4hc"
    if "--packer" in a:
        k = a.index("--packer")
        packer = a[k + 1]
        del a[k:k + 2]

    if a and a[0] == "--verify":
        if len(a) < 2:
            raise SystemExit("用法: --verify <bundle> [期望的.dll]")
        ok = verify(a[1], a[2] if len(a) > 2 else None)
        raise SystemExit(0 if ok else 1)

    if len(a) < 3:
        raise SystemExit(__doc__)
    src, dll_path, out_path = a[0], a[1], a[2]

    raw = open(src, "rb").read()
    prefix, body = split_prefix(raw)
    target = open(dll_path, "rb").read()
    print(f"[in ] bundle={src}  ({len(raw)} B, 前缀 {len(prefix)} B, hash={prefix[1:33].decode()})")
    print(f"[in ] dll   ={dll_path}  ({len(target)} B, md5={hashlib.md5(target).hexdigest()})")

    env, bun = load_bundle(body)
    o, tt, old, name, codec = find_dll_textasset(env)
    print(f"[map] TextAsset {name!r}  原 {len(old)} B → 新 {len(target)} B  (Δ{len(target) - len(old):+d})")

    tt["m_Script"] = target.decode("utf-8", "surrogateescape") if codec == "surrogateescape" else target
    o.save_typetree(tt)

    new_body = bun.save(packer=packer)
    # 前缀定长（41 B），长度字段即最终文件长度
    full = fix_prefix(prefix, len(prefix) + len(new_body)) + new_body
    open(out_path, "wb").write(full)
    print(f"[out] {out_path}  ({len(full)} B, packer={packer})")

    if not verify(out_path, dll_path):
        raise SystemExit("[gate] 回包自检 FAIL")
    print("[gate] 回包自检 PASS")


if __name__ == "__main__":
    main()
