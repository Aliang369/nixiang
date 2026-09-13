#!/usr/bin/env python3
"""Targeted IDA export for the BinGe thin-shell libSDKloading.so.

The shell is stripped.  We only resolve cross references to the small set of
runtime strings that describe the three menu features and its anti-ban logic,
then decompile the owning functions.  This keeps the report focused instead of
dumping the whole 23 MB binary.
"""
from __future__ import annotations

import sys
from collections import defaultdict

import ida_auto
import ida_bytes
import ida_funcs
import ida_hexrays
import idautils
import idc


TARGETS = [
    ("百倍伤害", 0x41AAE0),
    ("玩家无敌", 0x41AAEF),
    ("货币无限", 0x41AAFE),
    ("三个功能钩子已挂上", 0x41AAB8),
    ("功能钩子故意不挂", 0x41AB3E),
    ("featNum=0", 0x41ABC8),
    ("featNum=1", 0x41ABF6),
    ("存档已被标记", 0x41A41D),
    ("吞掉玩家扣血", 0x41A50C),
    ("吞掉这次退出", 0x41A492),
    ("游戏继续跑", 0x41A4A9),
    ("不调原函数", 0x41A362),
    ("反作弊", 0x41A2A5),
    ("安全网", 0x41A2D4),
    ("永久封", 0x41A313),
    ("GgSet", 0x41A2E9),
    ("CheckZuoBi", 0x41A345),
    ("HurtShow", 0x41A4C3),
    ("GetItemNum", 0x41A538),
    ("ReduceCreatureSpNum", 0x41A726),
    ("LongRand", 0x41A782),
    ("SetValue", 0x41A78B),
    ("GetMoneyId", 0x41A794),
    ("Application.Quit", 0x41A79F),
    ("CheckConsume(List)", 0x41A86E),
    ("UpdatePower", 0x41A3F7),
    ("DailyDataMgr.GG", 0x41A40D),
]


def emit(msg: str) -> None:
    print(msg, flush=True)


def collect_refs(seed: int, max_depth: int = 2):
    """Follow direct data refs a little, then collect code owners."""
    refs = []
    funcs = set()
    queue = [(seed, 0)]
    seen = set()
    while queue:
        ea, depth = queue.pop(0)
        if (ea, depth) in seen:
            continue
        seen.add((ea, depth))
        for x in idautils.XrefsTo(ea):
            refs.append((x.frm, x.type))
            f = ida_funcs.get_func(x.frm)
            if f is not None:
                funcs.add(f.start_ea)
            elif depth < max_depth:
                queue.append((x.frm, depth + 1))
    return refs, funcs


def decompile_func(start: int) -> str:
    try:
        cf = ida_hexrays.decompile(start)
    except Exception as exc:  # noqa: BLE001 - report IDA-specific failure
        return f"<decompile failed: {exc}>\n"
    if cf is None:
        return "<decompile returned None>\n"
    return str(cf)


def main() -> int:
    ida_auto.auto_wait()
    out_path = "/Users/aliang/Documents/nixiang/10_docs/我的修仙世界/ida_binge_functions.txt"
    by_func: dict[int, list[str]] = defaultdict(list)
    with open(out_path, "w", encoding="utf-8") as out:
        out.write("# BinGe libSDKloading targeted IDA export\n\n")
        for label, ea in TARGETS:
            refs, funcs = collect_refs(ea)
            out.write(f"## {label} @ 0x{ea:X}\n")
            if not refs:
                out.write("- no direct xrefs\n")
                continue
            for frm, xtype in refs:
                f = ida_funcs.get_func(frm)
                fdesc = f"func={idc.get_func_name(f.start_ea)}@{f.start_ea:#x}" if f else "data"
                out.write(f"- ref 0x{frm:X} type={xtype} {fdesc}\n")
            for start in sorted(funcs):
                by_func[start].append(label)
            out.write("\n")

        out.write("# Referencing functions\n\n")
        for start, labels in sorted(by_func.items()):
            out.write(
                f"## {idc.get_func_name(start)} @ 0x{start:X}\n"
                f"labels: {', '.join(sorted(set(labels)))}\n\n"
            )
            out.write("```c\n")
            out.write(decompile_func(start))
            out.write("\n```\n\n")

    emit(f"wrote {out_path}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
