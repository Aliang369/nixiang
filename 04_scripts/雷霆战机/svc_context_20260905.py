#!/usr/bin/env python3
"""第二遍：tersafe2 间接 svc 聚类 + 上下文反汇编 + 已知坐标关系。
输出: tersafe2.svc_context.txt（每个 svc 站点回看 20 条指令窗口）
"""
import json, struct
from pathlib import Path
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN

BASE = Path("/Users/aliang/Documents/nixiang/03_decompiled/雷霆战机/reanalysis_20260905")
NR = {93:"exit",94:"exit_group",129:"kill",130:"tkill",131:"tgkill",134:"rt_sigaction",
      135:"rt_sigprocmask",117:"ptrace",270:"process_vm_readv",271:"process_vm_writev",
      56:"openat",57:"close",62:"lseek",63:"read",64:"write",101:"nanosleep",
      115:"clock_nanosleep",122:"sched_setaffinity",123:"sched_getaffinity",124:"sched_yield",
      169:"gettimeofday",222:"mmap",226:"mprotect",232:"epoll_wait",220:"clone",
      139:"rt_sigreturn",424:"pidfd_send_signal",434:"pidfd_open"}

b = (BASE / "libtersafe2.so").read_bytes()
segs = [(0, 0x55a32c, 0)]  # 单一 r-x 段（第一遍已确认）
md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)

def disasm_window(target_off, back=20, fwd=6):
    start = max(0, target_off - back * 4)
    code = b[start:target_off + fwd * 4 + 4]
    out, pos = [], 0
    while pos < len(code):
        got = False
        for ins in md.disasm(code[pos:], start + pos):
            mark = " <<<" if (start + pos) == target_off else ""
            out.append(f"  {start+pos:#08x}  {ins.mnemonic:<8} {ins.op_str}{mark}")
            pos += ins.size
            got = True
            if (start + pos) > target_off + fwd * 4:
                break
        if not got:
            pos += 4
        if (start + pos) > target_off + fwd * 4:
            break
    return "\n".join(out)

sites = json.loads((BASE / "libtersafe2.so.svc_enum.json").read_text())["sites"]
lines = []
print(f"tersafe2 svc 站点数: {len(sites)}")

# 聚类：按 offset 排序，gap<0x400 归簇
offs = sorted(int(s["off"], 16) for s in sites)
clusters, cur = [], [offs[0]]
for o in offs[1:]:
    if o - cur[-1] < 0x400:
        cur.append(o)
    else:
        clusters.append(cur); cur = [o]
clusters.append(cur)
print(f"聚类数（gap<0x400）: {len(clusters)}")
for c in clusters:
    lo, hi = c[0], c[-1]
    print(f"  簇 {lo:#x}–{hi:#x}  ({len(c)} 处)")

LANDMARKS = {"ptrace簇1": (0x124a5c, 0x1251a8), "filecrc调度": (0x37a730, 0x37b968),
             "线程入口3d7110": (0x3d7110, 0x3d7110), "openat直连": (0x329204, 0x329204)}
print("\nsvc 站点 ↔ 已知坐标 距离（<0x800 标注）:")
for s in sites:
    o = int(s["off"], 16)
    for name, (a, z) in LANDMARKS.items():
        d = min(abs(o - a), abs(o - z))
        if d < 0x800:
            print(f"  {o:#x} nr={s['nr']} ↔ {name} 距离 {d:#x}")

with open(BASE / "tersafe2.svc_context.txt", "w") as f:
    for s in sites:
        o = int(s["off"], 16)
        sem = NR.get(s["nr"], "间接/未定") if s["nr"] is not None else "间接/未定"
        f.write(f"\n{'='*60}\nsvc @{o:#x}  nr={s['nr']} ({sem})\n{'='*60}\n")
        f.write(disasm_window(o) + "\n")
print("\n上下文窗口 → tersafe2.svc_context.txt")
