import ida_auto, ida_funcs, idautils, idc, os
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "funcs_wide.txt")
LO, HI = 0x1900000, 0x1B40000
def main():
    print("[wide] plan_range", flush=True)
    ida_auto.plan_range(LO, HI)
    ida_auto.auto_wait()
    print("[wide] auto_wait done", flush=True)
    rows=[]
    for ea in idautils.Functions(LO, HI):
        f=ida_funcs.get_func(ea)
        if not f: continue
        rows.append((f.start_ea, f.end_ea, len(list(idautils.XrefsTo(f.start_ea,0)))))
    rows.sort()
    with open(OUT,"w") as fp:
        fp.write("# wide boundaries 0x1900000-0x1b40000"+chr(10))
        for r in rows: fp.write("0x%x 0x%x 0x%x n=%d"%r+chr(10))
    print("[wide] funcs=%d"%len(rows), flush=True)
    idc.qexit(0)
main()
