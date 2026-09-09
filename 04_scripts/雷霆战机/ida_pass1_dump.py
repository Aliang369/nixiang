import os
import idaapi
import idautils
import idc
import ida_auto
import ida_funcs
import ida_hexrays
import ida_xref
import ida_bytes

OUT = "/Users/aliang/Documents/nixiang/05_ida/雷霆战机/pass1"
os.makedirs(OUT, exist_ok=True)

TARGETS = {
    "thread_entry_3d7110": 0x3D7110,
    "master_35aa60": 0x35AA60,
    "twinA_36cdac": 0x36CDAC,
    "twinB_36f1d8": 0x36F1D8,
    "up_345f64": 0x345F64,
    "dispatch_37a4c4": 0x37A4C4,
    "fileread_3141e8": 0x3141E8,
    "prim_3441b8": 0x3441B8,
    "prim_344340": 0x344340,
    "prim_3446f4": 0x3446F4,
    "prim_344afc": 0x344AFC,
    "prim_345110": 0x345110,
    "prim_34566c": 0x34566C,
    "callers_34d29c": 0x34D29C,
    "callers_35449c": 0x35449C,
    "callers_34e244": 0x34E244,
    "callers_356fb0": 0x356FB0,
}
TABLES = [0x38C10, 0x570258, 0x38E20, 0x570328]

ida_auto.auto_wait()

with open(os.path.join(OUT, "func_stats.txt"), "w") as f:
    f.write(f"total_funcs={len(list(idautils.Functions()))}\n")
    for ea in sorted(idautils.Functions()):
        fn = ida_funcs.get_func(ea)
        f.write(f"{ea:X} size={fn.size()}\n")

def dump_target(tag, ea):
    fn = ida_funcs.get_func(ea)
    start = fn.start_ea if fn else ea
    with open(os.path.join(OUT, f"disasm_{tag}_{start:X}.asm"), "w") as f:
        a = start
        end = fn.end_ea if fn else ea + 0x200
        while a < end:
            f.write(f"{a:X}  {idc.GetDisasm(a)}\n")
            a = idc.next_head(a, end)
    if ida_hexrays.init_hexrays_plugin():
        try:
            cf = ida_hexrays.decompile(start)
            with open(os.path.join(OUT, f"pcode_{tag}_{start:X}.c"), "w") as f:
                f.write(str(cf))
        except Exception as e:
            with open(os.path.join(OUT, f"pcode_{tag}_FAIL.txt"), "w") as f:
                f.write(f"{start:X}: {e}\n")

for tag, ea in TARGETS.items():
    dump_target(tag, ea)

# 数据表 xref
with open(os.path.join(OUT, "tables_xref.txt"), "w") as f:
    for t in TABLES:
        f.write(f"\n==== table {t:X} ====\n")
        for i in range(24):
            a = t + i * 8
            v = ida_bytes.get_qword(a)
            xr = ""
            for x in idautils.XrefsTo(t + i * 8):
                xr += f" xref<{x.frm:X}>"
            nm = idc.get_name(v) if v else ""
            f.write(f"  [{i:02d}] +{i*8:X}: {v:016X} {nm}{xr}\n")

idc.qexit(0)
