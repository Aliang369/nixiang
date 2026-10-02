# -*- coding: utf-8 -*-
"""Generate interp_entry_disasm.txt by driving llvm-objdump on the fixed RVA ranges.
Read-only w.r.t. the .so (only reads; writes only the report file).
"""
import subprocess, os, sys

OD  = r"C:\Users\Administrator\AppData\Local\Android\Sdk\ndk\27.0.12077973\toolchains\llvm\prebuilt\windows-x86_64\bin\llvm-objdump.exe"
SO  = r"E:\nixiang\06_dumped\冒险与征途\apk_extract\lib\arm64-v8a\libil2cpp.so"
OUT = r"E:\nixiang\06_dumped\冒险与征途\interp_entry_disasm.txt"
BASE = 0x400023200000

RANGES = [
    ("① HybridCLR 解释器入口 —— 函数头（前 16 条指令）",
     [(0x207F798, 0x207F820, "入口函数前 16 条指令（含 prologue）")]),
    ("② HybridCLR 解释器入口 —— 第二个帧初始化调用点",
     [(0x207F814, 0x207F830, "bl 0x207DE04 处")]),
    ("③ 帧初始化 A：0x207DCCC（调用 AllocStackSlot + AllocFrame）", []),
    ("④ 帧初始化 B：0x207DE04（调用 AllocStackSlot + AllocFrame）", []),
    ("⑤ AllocStackSlot 本体 0x207DD40（含引用 \"AllocStackSlot\" 串的冷路径）", []),
    ("⑥ AllocFrame 本体 0x207DDAC（含引用 \"AllocFrame\" 串的冷路径）", []),
    ("⑦ 异常构造链（两处冷路径共同调用）", []),
    ("⑧ 入口的调用者之一：全部入口调用的 2 个非 AOT 调用点", []),
    ("⑨ 入口的调用者之二：AOT 解释器桩（样例 2 个，共 5221 个调用点）", []),
    ("⑩ 入口内部首次调用 0x205FF04（每线程帧池取用）", []),
]
# real ranges
R = {
 "③": [(0x207DCCC, 0x207DD44, "帧初始化 A 全函数（ret 于 0x207DD3C）")],
 "④": [(0x207DE04, 0x207DE6C, "帧初始化 B 全函数")],
 "⑤": [(0x207DD40, 0x207DDB0, "AllocStackSlot 全函数 + 冷路径")],
 "⑥": [(0x207DDAC, 0x207DE04, "AllocFrame 全函数 + 冷路径")],
 "⑦": [(0x20C9E60, 0x20C9E88, "0x20C9E60 —— 用 (namespace, name, msg) 构造异常对象"),
        (0x20C881C, 0x20C8844, "0x20C881C —— __cxa_allocate_exception(8) + __cxa_throw，noreturn")],
 "⑧": [(0x207F630, 0x207F648, "调用点 0x207F640（属于函数 0x207F5D4 的 fast path）"),
        (0x208DFF0, 0x208E030, "调用点 0x208E018")],
 "⑨": [(0x21C7950, 0x21C7998, "AOT 桩样例 1（0x21C7950）与样例 2（0x21C7974）"),
        (0x220B9F0, 0x220BA08, "AOT 桩样例 3：最后一个调用点 0x220BA00")],
 "⑩": [(0x205FF04, 0x205FF60, "0x205FF04 —— pthread_getspecific 取每线程帧池（0x68 字节对象）")],
}

lines = []
lines.append("# 候选解释器入口 / 帧管理函数的反汇编 —— libil2cpp.so")
lines.append("#")
lines.append("# 【llvm-objdump 正确调用姿势（实测确认）】")
lines.append("#   llvm-objdump 对本 so 吃的是 **ELF 虚拟地址（= RVA = 节 sh_addr）**，")
lines.append("#   直接 --start-address=<RVA> 即可；**不要**再减 0x1000 的段 delta。")
lines.append("#   反例：传 file offset（减 0x1000）会得到整体错位一帧的伪反汇编。")
lines.append("#   --adjust-vma 不需要；不加也正确。")
lines.append("#")
lines.append("#   命令原文：")
lines.append('#   "%s" -d \\' % OD)
lines.append("#       --start-address=0x207F798 --stop-address=0x207F820 \\")
lines.append('#       "%s"' % SO)
lines.append("#")
lines.append("# 校验：0x207DD98 行必须显示 `adrp x0, 0xcf2000` 且下一行 `add x0, x0, #0x929`；")
lines.append("#       0x207DDF0 行必须显示 `adrp x0, 0xd1c000` 且下一行 `add x0, x0, #0xa18`。")
lines.append("#       这两行证明 objdump 的寻址模型与本文件的 RVA 一致。")
lines.append("#")
lines.append("# 运行时换算：vaddr = 0x400023200000 + RVA")
lines.append("#")
lines.append("=" * 84)
lines.append("")

def run(a, b):
    cmd = [OD, "-d", "--start-address=%#x" % a, "--stop-address=%#x" % b, SO]
    lines.append("### 命令原文（可直接复制执行）")
    lines.append("")
    lines.append('"%s" -d --start-address=%#x --stop-address=%#x "%s"' % (OD, a, b, SO))
    lines.append("")
    p = subprocess.run(cmd, capture_output=True, text=True, errors="replace")
    txt = (p.stdout or "") + (p.stderr or "")
    keep = []
    for ln in txt.splitlines():
        if ln.startswith("Disassembly of section") or ln.startswith("\n"):
            continue
        if ln.strip() == "":
            continue
        keep.append(ln)
    for ln in keep:
        lines.append("    " + ln)
    lines.append("")

for title, _ in RANGES:
    lines.append("=" * 84)
    lines.append("## " + title)
    lines.append("")
    key = title[0]
    rr = R.get(key)
    if rr is None:
        rr = [(0x207F798, 0x207F820, "")]
    # ①/② use their own lists
    if key == "①":
        rr = [(0x207F798, 0x207F820, "")]
    if key == "②":
        rr = [(0x207F814, 0x207F830, "")]
    for (a, b, note) in rr:
        if note:
            lines.append("<!-- %s -->" % note)
        lines.append("RVA %#x .. %#x   (运行时 0x%012X .. 0x%012X)" % (a, b, BASE + a, BASE + b))
        lines.append("")
        run(a, b)

open(OUT, "w", encoding="utf-8").write("\n".join(lines) + "\n")
print("wrote", OUT, len(lines), "lines")
