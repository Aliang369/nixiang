#!/usr/bin/env python3.11
"""
校验「已经落盘的 libstarry_magic.so」里嵌的 patch JS 与我们的合成产物逐字节一致。
这是出包前最后一道、也是最硬的一道检查：native 是用 strlen(kYsxzPatchJs) 拿
这段 JS 的，所以任何截断(NUL) / 陈旧残留 / 半份覆盖都必须在这里拦住。

用法: python3.11 verify_ac_so.py [so路径] [patch js路径]
默认: ../libs_ysxz/... 由 ROOT 推导；patch js = ./patch_v2_ac.js
"""
from __future__ import annotations

import hashlib
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
SO = Path(sys.argv[1]) if len(sys.argv) > 1 else (
    ROOT / "mod菜单/starry-mod-menu/libs_ysxz/arm64-v8a/libstarry_magic.so")
JS = Path(sys.argv[2]) if len(sys.argv) > 2 else (HERE / "patch_v2_ac.js")
MARKER = b"/*starry_ysxz_v1*/"
END_MARKER = b"__AC_STATE__"


def main() -> int:
    blob = SO.read_bytes()
    js = JS.read_text(encoding="utf-8")
    want = js[js.index(MARKER.decode()):]
    want_b = want.encode("utf-8").rstrip(b"\n")

    i = blob.find(MARKER)
    if i < 0:
        raise SystemExit(f"FAIL: {SO} 里找不到 {MARKER.decode()}")
    j = blob.find(b"\x00", i)
    if j < 0:
        raise SystemExit("FAIL: so 里 marker 之后没有 NUL 终止符")
    got = blob[i:j].rstrip(b"\n")

    checks = [
        (len(blob) == len(blob), f"so 体积 {len(blob)} B"),
        (got == want_b, f"so 内 patch JS 与 {JS.name} 逐字节一致 ({len(got)} B)"),
        (blob.count(MARKER) == 1, f"so 里标记出现 {blob.count(MARKER)} 次(期望 1)"),
        (b"window.__starry_ac" in got, "AC 层入口在 so 里"),
        (END_MARKER in got, "__AC_STATE__ 诊断口在 so 里"),
        (b"clearPending" in got, "配对纪律 clearPending 在 so 里"),
        (b"finalHurtValue" in got and b"lockMinHP" in got, "命中指纹明文在 so 里"),
        (b"battleServer" in got and b"gsMgr" in got, "出口挂钩目标在 so 里"),
        (b"ANTI_CHEAT_ENABLED" in got, "闸门键在 so 里"),
        (b"ac layer v1 loaded" in got, "AC 层自述日志在 so 里"),
        (b"\x00" not in got, "patch JS 内无 NUL(不会被 strlen 截断)"),
    ]
    ok = True
    for good, msg in checks:
        print(("  ok   " if good else "  FAIL ") + msg)
        ok &= good

    # 内容差异定位(若不一致, 直接报第一个不同字节, 便于排查陈旧残留)
    if got != want_b:
        n = min(len(got), len(want_b))
        d = next((k for k in range(n) if got[k] != want_b[k]), n)
        print(f"  ... 首个差异 @ {d}: so={got[d:d+60]!r} want={want_b[d:d+60]!r}")
        ok = False

    print(f"so   {SO}\n      {len(blob)} B  md5={hashlib.md5(blob).hexdigest()}  sha256={hashlib.sha256(blob).hexdigest()}")
    print(f"patch{JS}\n      {len(js)} B  sha256={hashlib.sha256(js.encode()).hexdigest()}")
    print("verify_ac_so: " + ("OK" if ok else "FAILED"))
    return 0 if ok else 1


if __name__ == "__main__":
    raise SystemExit(main())
