#!/usr/bin/env python3
"""Resolve writers/readers of the BinGe feature flags and JNI registration."""
from __future__ import annotations

import sys
from collections import defaultdict

import ida_auto
import ida_funcs
import ida_hexrays
import idautils
import idc


TARGETS = [
    ("feature_0x16A04C0", 0x16A04C0),
    ("feature_0x16A04C4", 0x16A04C4),
    ("feature_0x16A04C8", 0x16A04C8),
    ("diag_or_safety_0x16A04CC", 0x16A04CC),
    ("hooks_ready_0x16A0514", 0x16A0514),
    ("il2cpp_base_0x16A0500", 0x16A0500),
]

EXTRA_FUNCS = [
    ("jni_setup_a", 0x2CDB80),
    ("jni_setup_b", 0x2CEF30),
    ("jni_register_or_init", 0x2CF240),
]


def decompile(start: int) -> str:
    try:
        cf = ida_hexrays.decompile(start)
    except Exception as exc:  # noqa: BLE001 - IDA-specific failure
        return f"<decompile failed: {exc}>\n"
    return str(cf) if cf is not None else "<decompile returned None>\n"


def main() -> int:
    ida_auto.auto_wait()
    out_path = "/Users/aliang/Documents/nixiang/10_docs/我的修仙世界/ida_binge_flags.txt"
    by_func: dict[int, list[str]] = defaultdict(list)
    with open(out_path, "w", encoding="utf-8") as out:
        out.write("# BinGe flag writers/readers + JNI setup\n\n")
        for label, ea in TARGETS:
            out.write(f"## {label} @ 0x{ea:X}\n")
            refs = list(idautils.XrefsTo(ea))
            if not refs:
                out.write("- no direct xrefs\n\n")
                continue
            for x in refs:
                f = ida_funcs.get_func(x.frm)
                fdesc = (
                    f"func={idc.get_func_name(f.start_ea)}@{f.start_ea:#x}"
                    if f
                    else "data"
                )
                out.write(f"- ref 0x{x.frm:X} type={x.type} {fdesc}\n")
                if f is not None:
                    by_func[f.start_ea].append(label)
            out.write("\n")

        for label, start in EXTRA_FUNCS:
            by_func[start].append(label)

        out.write("# Functions\n\n")
        for start, labels in sorted(by_func.items()):
            out.write(
                f"## {idc.get_func_name(start)} @ 0x{start:X}\n"
                f"labels: {', '.join(sorted(set(labels)))}\n\n"
            )
            out.write("```c\n")
            out.write(decompile(start))
            out.write("\n```\n\n")

    print(f"wrote {out_path}", flush=True)
    return 0


if __name__ == "__main__":
    sys.exit(main())
