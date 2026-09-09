#!/usr/bin/env python3
"""第三遍：找通用 syscall stub 农场（33 处）的调用点，提取调用时 w0 立即数（系统调用号）。
判据：bl 目标落在任一 svc 站点前 0x30 字节内；调用点回看 10 条找 mov w0/x0, #imm。
输出: 控制台直方图 + syscall_farm_callsites.json
"""
import json
import struct
from pathlib import Path
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN

BASE = Path("/Users/aliang/Documents/nixiang/03_decompiled/雷霆战机/reanalysis_20260905")
NR_NAME = {93:"exit",94:"exit_group",129:"kill",130:"tkill",131:"tgkill",134:"rt_sigaction",
           135:"rt_sigprocmask",136:"rt_sigtimedwait",117:"ptrace",270:"process_vm_readv",
           271:"process_vm_writev",56:"openat",57:"close",62:"lseek",63:"read",64:"write",
           101:"nanosleep",115:"clock_nanosleep",169:"gettimeofday",222:"mmap",226:"mprotect",
           232:"epoll_wait",220:"clone",124:"sched_yield",66:"writev",65:"readv",
           29:"ioctl",98:"futex",96:"set_tid_address",160:"uname",78:"getdents64",
           48:"faccessat",79:"fchown",5:"fstat",43:"sendto",44:"recvfrom",198:"socket",
           203:"connect",42:"pipe2"}

b = (BASE / "libtersafe2.so").read_bytes()
TEXT_OFF, TEXT_SZ = 0, 0x55a32c
md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)

# 1) svc 站点（从第一遍 JSON）
sites = json.loads((BASE / "libtersafe2.svc_enum.json".replace("libtersafe2.", "libtersafe2.so.")).read_text())["sites"] if False else json.loads((BASE / "libtersafe2.so.svc_enum.json").read_text())["sites"]
farm = sorted(int(s["off"], 16) for s in sites if 0x3f8300 <= int(s["off"], 16) <= 0x3f9200)
print(f"农场 stub svc 数: {len(farm)}")

# 2) 全段反汇编（分段续扫），记录 bl + mov 立即数窗口
insns = []
pos = 0
code = b[TEXT_OFF:TEXT_OFF + TEXT_SZ]
while pos < len(code):
    got = False
    for ins in md.disasm(code[pos:], pos):
        insns.append((ins.address, ins.mnemonic, ins.op_str))
        pos += ins.size
        got = True
    if not got:
        pos += 4

# 3) 扫 bl → 农场，回看 w0 立即数
calls = []
idx_of = {}
for i, (a, m, o) in enumerate(insns):
    idx_of[a] = i
for i, (a, m, o) in enumerate(insns):
    if m != "bl":
        continue
    try:
        tgt = int(o.replace("#", ""), 16)
    except ValueError:
        continue
    for svc_a in farm:
        if 0 < svc_a - tgt <= 0x30:
            nr = None
            for j in range(i - 1, max(-1, i - 11), -1):
                am, mo, mops = insns[j]
                if mo == "mov" and mops.split(",")[0].strip() in ("w0", "x0") and "#" in mops:
                    try:
                        nr = int(mops.split("#")[1].split(",")[0], 0)
                    except ValueError:
                        nr = -1  # 非纯立即数
                    break
                if mo in ("bl", "b.", "b") and j != i:
                    break
            calls.append({"call_site": hex(a), "stub_svc": hex(svc_a), "nr_imm": nr})
            break

print(f"农场调用点总数: {len(calls)}")
hist = {}
for c in calls:
    hist.setdefault(c["nr_imm"], []).append(c)
print(f"\n{'nr':>6} {'语义':<18} {'count':>6}  调用点样例")
for nr, lst in sorted(hist.items(), key=lambda kv: (kv[0] is None, kv[0] if kv[0] is not None else 0)):
    sem = NR_NAME.get(nr, "?") if isinstance(nr, int) and nr >= 0 else ("(变量)" if nr == -1 else "(未取到)")
    sample = ", ".join(x["call_site"] for x in lst[:6])
    more = f" …(+{len(lst)-6})" if len(lst) > 6 else ""
    print(f"{str(nr):>6} {sem:<18} {len(lst):>6}  {sample}{more}")

Path(BASE / "syscall_farm_callsites.json").write_text(json.dumps(calls, indent=1))
print("\n→ syscall_farm_callsites.json")
