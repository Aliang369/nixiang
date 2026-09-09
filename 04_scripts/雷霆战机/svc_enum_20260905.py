#!/usr/bin/env python3
"""雷霆战机 1.25.31 ACE so — svc 指令全枚举（P0-2 收口点排查第一步）
方法：capstone 线性扫 PT_LOAD 可执行段，找 `svc #0`，回看 8 条内 `mov x8/w8, #imm` 定系统调用号。
锚点校验：tprt 应有 ~108 处 nr=94(exit_group) 且附近 mov w0,#7（官方原包崩溃文档 §10.3）。
输出：<so>.svc_enum.json + 控制台汇总表。
"""
import json, sys
from pathlib import Path
import struct
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN

INTERESTING = {
    93: "exit", 94: "exit_group",
    129: "kill", 130: "tkill", 131: "tgkill",
    134: "rt_sigaction", 135: "rt_sigprocmask", 139: "rt_sigreturn",
    117: "ptrace", 270: "process_vm_readv", 271: "process_vm_writev",
    424: "pidfd_send_signal", 434: "pidfd_open",
    56: "openat", 437: "openat2",
    222: "mmap", 226: "mprotect",
    101: "nanosleep", 115: "clock_nanosleep",
    220: "clone", 435: "clone3", 57: "fork", 58: "vfork",
}

def exec_segments(path):
    b = Path(path).read_bytes()
    assert b[:4] == b"\x7fELF", "not ELF"
    phoff = struct.unpack_from("<Q", b, 0x20)[0]
    phentsize, phnum = struct.unpack_from("<HH", b, 0x36)
    segs = []
    for i in range(phnum):
        o = phoff + i * phentsize
        p_type, p_flags = struct.unpack_from("<II", b, o)
        p_offset, p_vaddr, _pa, p_filesz = struct.unpack_from("<QQQQ", b, o + 8)
        if p_type == 1 and (p_flags & 1):  # PT_LOAD + PF_X
            segs.append((p_offset, p_filesz, p_vaddr))
    return b, segs

def enum_so(path):
    b, segs = exec_segments(path)
    md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)
    md.detail = False
    sites = []  # (file_off, va, nr or None, w0_hint or None)
    for off, sz, va in segs:
        code = b[off:off + sz]
        insns = []
        pos = 0
        while pos < len(code):
            chunk = md.disasm(code[pos:], va + pos)
            got = False
            for ins in chunk:
                insns.append(ins)
                got = True
            pos = insns[-1].address - va + insns[-1].size if got else pos + 4
        print(f"  [seg off={hex(off)} size={hex(sz)} va={hex(va)}] 指令数 {len(insns)}")
        for idx, ins in enumerate(insns):
            if ins.mnemonic != "svc":
                continue
            nr = None
            for j in range(idx - 1, max(-1, idx - 9), -1):
                p = insns[j]
                if p.mnemonic == "mov" and p.op_str.startswith(("x8,", "w8,")):
                    part = p.op_str.split(",")[1].strip()
                    if part.startswith("#"):
                        nr = int(part[1:].replace("0x", ""), 16) if part[1:].startswith("0x") else int(part[1:])
                    break  # x8 被别的指令改写/来自寄存器 → 间接
            # w0 提示（如 exit_group(7) 的 mov w0,#7）
            w0 = None
            for j in range(idx - 1, max(-1, idx - 9), -1):
                p = insns[j]
                if p.mnemonic == "mov" and p.op_str.startswith(("x0,", "w0,")) and "#" in p.op_str:
                    try:
                        w0 = int(p.op_str.split("#")[1].split(",")[0], 0)
                    except ValueError:
                        pass
                    break
            sites.append({"off": hex(off + ins.address - va), "va": hex(ins.address),
                          "nr": nr, "w0": w0})
    return sites

def summarize(name, sites):
    by_nr = {}
    for s in sites:
        by_nr.setdefault(s["nr"], []).append(s)
    print(f"\n===== {name}: svc 总数 {len(sites)} =====")
    print(f"{'nr':>6} {'语义':<20} {'count':>6}  位置样例")
    for nr, lst in sorted(by_nr.items(), key=lambda kv: (kv[0] is None, kv[0] or 0)):
        sem = INTERESTING.get(nr, "?") if nr is not None else "(间接/未定 x8)"
        sample = ", ".join(x["off"] for x in lst[:5])
        more = f" …(+{len(lst)-5})" if len(lst) > 5 else ""
        w0s = sorted({x['w0'] for x in lst if x['w0'] is not None})
        extra = f"  w0∈{w0s[:6]}" if w0s else ""
        print(f"{str(nr):>6} {sem:<20} {len(lst):>6}  {sample}{more}{extra}")
    return by_nr

def main():
    base = Path("/Users/aliang/Documents/nixiang/03_decompiled/雷霆战机/reanalysis_20260905")
    out = {}
    for so in ("libtersafe2.so", "libtprt.so"):
        sites = enum_so(base / so)
        by_nr = summarize(so, sites)
        out[so] = {"total": len(sites), "sites": sites}
        Path(base / (so + ".svc_enum.json")).write_text(json.dumps(out[so], ensure_ascii=False, indent=1))
        # 锚点：tprt exit_group 附近 mov w0,#7
        if so == "libtprt.so":
            eg7 = [s for s in by_nr.get(94, []) if s["w0"] == 7]
            print(f"[锚点] tprt exit_group(7) 形态: {len(eg7)} 处（文档记录 ~100，量级吻合即可）")

if __name__ == "__main__":
    main()
