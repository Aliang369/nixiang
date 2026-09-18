import ida_auto
import ida_hexrays
import ida_funcs
import ida_name
import ida_segment
import idc

ida_auto.auto_wait()

TARGETS = [
    0x6B72A4,  # AppDelegate::applicationDidFinishLaunching
    0x6D1B14,  # FileUtils constructor
    0x758FC8,  # file/data decrypt callback
    0x759684,  # string decrypt callback
    0x7738D4,  # xxtea_decrypt
    0x74CA78,  # jsb_set_xxtea_key
]

for ea in TARGETS:
    name = ida_name.get_name(ea)
    func = ida_funcs.get_func(ea)
    print("=" * 80)
    print(f"FUNCTION {ea:#x} {name}")
    if func is None:
        print("NO FUNCTION")
        continue
    print(f"RANGE {func.start_ea:#x}-{func.end_ea:#x}")
    try:
        cfunc = ida_hexrays.decompile(ea)
    except Exception as exc:
        print(f"DECOMPILE FAILED: {exc}")
        continue
    if cfunc is None:
        print("DECOMPILE RETURNED NONE")
        continue
    print(str(cfunc))

idc.qexit(0)
