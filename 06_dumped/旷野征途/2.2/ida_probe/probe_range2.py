# 限定范围分析：只对 HybridCLR 运行区 plan_range，避免 65MB 全工程 auto-analysis
import ida_auto, ida_funcs, idautils, idc, os

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "funcs.txt")
LO, HI = 0x1AF4000, 0x1AF8000

def main():
    print("[probe] plan_range", flush=True)
    ida_auto.plan_range(LO, HI)
    print("[probe] auto_wait start", flush=True)
    ida_auto.auto_wait()
    print("[probe] auto_wait done", flush=True)
    rows = []
    for ea in idautils.Functions(LO, HI):
        f = ida_funcs.get_func(ea)
        if not f:
            continue
        callers = [xr.frm for xr in idautils.XrefsTo(f.start_ea, 0)]
        rows.append((f.start_ea, f.end_ea, f.end_ea - f.start_ea, len(callers),
                     sorted(set(callers))[:6]))
    rows.sort()
    with open(OUT, "w") as fp:
        fp.write("# 0x1af4000-0x1af8000 funcs: start end size ncallers first_callers
")
        for r in rows:
            fp.write("0x%x 0x%x 0x%x n=%d %s
" % (r[0], r[1], r[2], r[3],
                     " ".join("0x%x" % c for c in r[4])))
    print("[probe] funcs=%d" % len(rows), flush=True)
    idc.qexit(0)

main()
