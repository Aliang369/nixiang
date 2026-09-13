#!/usr/bin/env python3
"""Decompile the concrete BinGe hook replacements identified by setup code."""
from __future__ import annotations

import sys

import ida_auto
import ida_funcs
import ida_hexrays
import idc


HOOKS = [
    ("method_resolver", 0x2CBA98),
    ("GgSet_replacement", 0x2CBC7C),
    ("CheckZuoBi_replacement", 0x2CBC94),
    ("ApplicationQuit_replacement", 0x2CBCC0),
    ("HurtShow_replacement", 0x2CBD54),
    ("ReduceCreatureSpNum_replacement", 0x2CBF5C),
    ("GetMoneyId_replacement", 0x2CC088),
    ("GetItemNum_replacement", 0x2CC138),
    ("fixed_rva_hook_replacement", 0x2CC290),
    ("setup_function", 0x2CC358),
]


def main() -> int:
    ida_auto.auto_wait()
    out_path = "/Users/aliang/Documents/nixiang/10_docs/我的修仙世界/ida_binge_hooks.txt"
    with open(out_path, "w", encoding="utf-8") as out:
        out.write("# BinGe hook replacement decompilation\n\n")
        for label, start in HOOKS:
            f = ida_funcs.get_func(start)
            if f is None:
                out.write(f"## {label} @ 0x{start:X}\n<no function>\n\n")
                continue
            out.write(f"## {label} @ 0x{start:X} ({idc.get_func_name(start)})\n")
            out.write(f"bounds: 0x{f.start_ea:X}-0x{f.end_ea:X}\n\n")
            try:
                cf = ida_hexrays.decompile(start)
                text = str(cf) if cf is not None else "<decompile returned None>"
            except Exception as exc:  # noqa: BLE001 - IDA-specific failure
                text = f"<decompile failed: {exc}>"
            out.write("```c\n")
            out.write(text)
            out.write("\n```\n\n")
    print(f"wrote {out_path}", flush=True)
    return 0


if __name__ == "__main__":
    sys.exit(main())
