#!/usr/bin/env python3.11
"""
星空包(勇士之心 1.0.3)「过检测复刻」出包后的外科完整性校验。
把新包与底包逐条目对比，证明**只有 2 个条目被替换**，其余条目逐字节未动；
并复核新包内 so 携带的 patch JS 与本地合成产物逐字节一致。

用法:
  python3.11 verify_ac_pack.py <新包.apk> [底包.apk]
默认底包 = ./BACKUP_星空包_1.0.3_原样.apk，patch js = ./patch_v2_ac.js
"""
from __future__ import annotations

import hashlib
import sys
import zipfile
from pathlib import Path

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
NEW = Path(sys.argv[1]) if len(sys.argv) > 1 else None
OLD = Path(sys.argv[2]) if len(sys.argv) > 2 else HERE / "BACKUP_星空包_1.0.3_原样.apk"
JS = HERE / "patch_v2_ac.js"

REPLACED = {"classes4.dex", "lib/arm64-v8a/libstarry_magic.so"}
MARKER = b"/*starry_ysxz_v1*/"
KEEP_DEX_DEFAULT = True


def sig_name(n: str) -> bool:
    u = n.upper()
    return u.startswith("META-INF/") and (u.endswith(".SF") or u.endswith(".RSA") or u.endswith(".DSA")
                                          or u.endswith(".EC") or u.endswith("MANIFEST.MF"))


def main() -> int:
    if NEW is None:
        raise SystemExit("用法: verify_ac_pack.py <新包.apk> [底包.apk]")
    if not NEW.exists():
        raise SystemExit(f"找不到新包 {NEW}")
    if not OLD.exists():
        raise SystemExit(f"找不到底包 {OLD}")
    js = JS.read_text(encoding="utf-8")
    want = js[js.index(MARKER.decode()):].encode("utf-8").rstrip(b"\n")

    znew, zold = zipfile.ZipFile(NEW), zipfile.ZipFile(OLD)
    ninfo = {i.filename: i for i in znew.infolist()}
    oinfo = {i.filename: i for i in zold.infolist()}
    ok = True

    def chk(good: bool, msg: str) -> None:
        nonlocal ok
        ok &= good
        print(("  ok   " if good else "  FAIL ") + msg)

    print(f"新包 {NEW}\n      {NEW.stat().st_size} B  md5={hashlib.md5(NEW.read_bytes()).hexdigest()}")
    print(f"底包 {OLD}\n      {OLD.stat().st_size} B  md5={hashlib.md5(OLD.read_bytes()).hexdigest()}")

    added = sorted(set(ninfo) - set(oinfo))
    removed = sorted(set(oinfo) - set(ninfo))
    chk(not added or all(sig_name(n) for n in added), f"新增条目 {len(added)} 条 {added[:6]}（应只有签名文件）")
    chk(not removed or all(sig_name(n) for n in removed), f"消失条目 {len(removed)} 条 {removed[:6]}（应只有旧签名文件）")

    diff, same = [], 0
    for n in sorted(set(ninfo) & set(oinfo)):
        a, b = ninfo[n], oinfo[n]
        if a.CRC == b.CRC and a.file_size == b.file_size:
            same += 1
        else:
            diff.append(n)
    unexpected = [n for n in diff if n not in REPLACED and not sig_name(n)]
    chk(not unexpected, f"被替换条目 = {diff}（预期只有 {sorted(REPLACED)} + 签名文件）")
    chk(same >= 1000, f"逐字节未动条目数 = {same}")
    chk(all(n in ninfo for n in REPLACED), "两个目标条目都存在于新包")

    # so 内嵌 patch JS 与被替换 so 的 md5
    so = znew.read("lib/arm64-v8a/libstarry_magic.so")
    i = so.find(MARKER)
    j = so.find(b"\x00", i) if i >= 0 else -1
    got = so[i:j].rstrip(b"\n") if i >= 0 and j > i else b""
    chk(i >= 0 and j > i, "新包 so 里能找到 patch JS 标记")
    chk(got == want, f"新包 so 内 patch JS 与 {JS.name} 逐字节一致 ({len(got)} B)")
    if got != want:
        n = min(len(got), len(want))
        d = next((k for k in range(n) if got[k] != want[k]), n)
        print(f"       首个差异 @ {d}: so={got[d:d+60]!r} want={want[d:d+60]!r}")
    chk(so.count(MARKER) == 1, "标记在 so 内恰好 1 次")
    chk(b"\x00" not in got, "so 内 patch JS 无 NUL")

    dex = znew.read("classes4.dex")
    for tok, label in ((b"MagicRes$PanelBgDrawable", "菜单外观标记"), (b"[auth] store verdict=", "授权修法标记"),
                       (b"cold-start self-heal", "冷启动自愈标记")):
        chk(tok in dex, f"classes4.dex 含 {label} {tok.decode()}")
    chk(b"YsxzAppComponentFactory" not in dex, "classes4.dex 未混入 1.0.4 过检测类")
    old_dex = zold.read("classes4.dex")
    if dex == old_dex:
        print("  ok   classes4.dex 与底包逐字节一致（本次未改菜单 dex）")
    else:
        # 2026-10-07 起 dex 会因新增开关而变（YsxzHooks 的 RET/VAL 容量 8 -> 16）。
        # 一旦变了就必须带 CAP_MARK —— 否则等于装了「新菜单 + 旧容量」的包，
        # 10..13 号开关永远读 false，不报错、只是静默失效。
        chk(b"ysxz_hooks_cap16_20261007" in dex,
            "classes4.dex 已更新且带 CAP_MARK（YsxzHooks 容量 = 16）")
        print(f"  --   classes4.dex 已更新（{len(old_dex)} -> {len(dex)} B）")

    # so 的菜单【项目清单】必须含 2026-10-07 新增的 5 项（与 dex 容量配成一组）
    for tok in ("Toggle_解锁金卡", "Toggle_自动重复挑战", "Toggle_自动出售",
                "Toggle_自动分解", "Toggle_自动穿戴"):
        chk(tok.encode() in so, f"so 菜单表含 [{tok}]")

    print("verify_ac_pack: " + ("OK" if ok else "FAILED"))
    return 0 if ok else 1


if __name__ == "__main__":
    raise SystemExit(main())
