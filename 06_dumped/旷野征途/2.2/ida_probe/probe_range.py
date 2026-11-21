# 限定范围导出：HybridCLR 运行时区 0x1af4000-0x1af8000 的函数边界与调用者
# 目的：拿真实函数边界，替代 capstone 邻近猜测
import ida_auto, ida_funcs, idautils, idc, idaapi, ida_bytes, os, sys

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "funcs.txt")

def main():
    print("[probe] auto_wait start", flush=True)
    ida_auto.auto_wait()
    print("[probe] auto_wait done", flush=True)
    lo, hi = 0x1AF4000, 0x1AF8000
    rows = []
    for ea in idautils.Functions(lo, hi):
        f = ida_funcs.get_func(ea)
        if not f:
            continue
        size = f.end_ea - f.start_ea
        callers = []
        for xr in idautils.XrefsTo(f.start_ea, 0):
            if xr.type in (16, 17, 18, 19, 20, 21):  # call/jump kinds
                callers.append(xr.frm)
        rows.append((f.start_ea, f.end_ea, size, len(callers), sorted(set(callers))[:6]))
    rows.sort()
    with open(OUT, "w") as fp:
        fp.write("# funcs in 0x1af4000-0x1af8000 (start,end,size,ncallers,first_callers)\n")
        for r in rows:
            fp.write("0x%x 0x%x 0x%x  n=%d  %s\n" % (r[0], r[1], r[2], r[3],
                     " ".join("0x%x" % c for c in r[4])))
    print("[probe] funcs=%d -> %s" % (len(rows), OUT), flush=True)
    idc.qexit(0)

main()
