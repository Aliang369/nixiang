# -*- coding: utf-8 -*-
"""为了吾王 R2e：在 R2d 基础上新增「专注点不消耗」，只加这一个变量。

新点位（本轮挖出来的）：
  CharacterStats::UpdateFocusPoints  = metadata 全局方法索引 1040
  → runtime vaddr 0x1F5DA0C / 文件偏移 0x1F5CA0C
  定位链路：明文 metadata(mz.bin) 名字表 → methods/typeDefs 表 → 方法索引
            → 运行时 codeGenModules[8]=Assembly-CSharp.dll 的 methodPointers（索引即全局方法索引，
              用 SkillRoll::Roll=1148 作锚点校验，命中索引正好 1148）。

原逻辑（capstone 反汇编 0x1f5da74..0x1f5da98）：
  ldr  w22, [x19, #0x14c]      ; 当前专注点
  mov  x0, x19 ; bl get_MaxFocus(=m981 @0x1f60a44)
  adds w8, w22, w21            ; ← 本点位：focus + delta
  b.mi 0x1f5daa0               ; 负 → 夹 0
  cmp  w8, w0 ; csel w1, w0, w8, gt   ; min(新值, 上限)
  str  w1, [x19, #0x14c]
  ...
改动：把 `adds w8, w22, w21` 换成跳到 cave，cave 里把 delta 夹成 max(delta,0) 再相加，
      **扣点变成无操作，加点照旧**。不改 getter（避开 A 那种打转风险）。

用法: python patch_libil2cpp_r2e.py <in.so> <out.so>
"""
import struct
import sys

TEXT_DELTA = 0x1000
CAVE_FILE = 0x44E6000
CAVE_VA = 0x4720000

SITES = {
    "B1": (0x1F62074, 0x5290D3E9, 0x528847E9),   # mov w9,#0x869f  → #0x423F
    "B2": (0x1F62078, 0x72A00029, 0x72A001E9),   # movk w9,#1,lsl16 → #0xF
    "C": (0x1F6903C, 0x1A9FB7E0, 0x52800020),    # cset w0,ge → mov w0,#1
    "D": (0x1DBD5B4, 0x1A9FD7E0, 0x52800020),    # cset w0,gt → mov w0,#1
    "E": (0x2059C14, 0x1A9F87E0, 0x52800020),    # cset w0,ls → mov w0,#1
}
FOCUS = (0x1F5CA80, 0x2B1502C8, 0x1F5DA80, 0x1F5DA84)  # file, 原指令, vaddr, 返回点


def b_insn(pc, target):
    off = (target - pc) >> 2
    assert -(1 << 25) <= off < (1 << 25)
    return 0x14000000 | (off & 0x03FFFFFF)


def main():
    src, dst = sys.argv[1], sys.argv[2]
    data = bytearray(open(src, "rb").read())
    n = len(data)

    for k, (off, exp, _) in SITES.items():
        got = struct.unpack_from("<I", data, off)[0]
        assert got == exp, f"{k}@{off:#x} 期望 {exp:#010x} 实得 {got:#010x}"
        struct.pack_into("<I", data, off, SITES[k][2])
        print(f"  ✓ {k}@{off:#x} → {SITES[k][2]:#010x}")

    f_off, f_exp, f_va, f_ret = FOCUS
    got = struct.unpack_from("<I", data, f_off)[0]
    assert got == f_exp, f"focus@{f_off:#x} 期望 {f_exp:#010x} 实得 {got:#010x}"
    print(f"  ✓ focus@{f_off:#x} 原指令 {got:#010x} = adds w8, w22, w21")

    assert set(data[CAVE_FILE:CAVE_FILE + 0x40]) == {0}, "cave 非全零"
    stub = [
        0x710002BF,                              # cmp  w21, #0
        0x1A9FC2A9,                              # csel w9, w21, wzr, gt   → w9 = max(delta,0)
        0x2B0902C8,                              # adds w8, w22, w9
        b_insn(CAVE_VA + 12, f_ret),             # b    0x1f5da84（原下一条）
    ]
    data[CAVE_FILE:CAVE_FILE + 16] = b"".join(struct.pack("<I", x) for x in stub)
    struct.pack_into("<I", data, f_off, b_insn(f_va, CAVE_VA))
    print(f"  ✓ focus → b {CAVE_VA:#x}；cave: cmp/csel/adds/b")

    assert len(data) == n
    open(dst, "wb").write(bytes(data))
    print(f"输出 {dst}，{len(data)} B")


if __name__ == "__main__":
    main()
