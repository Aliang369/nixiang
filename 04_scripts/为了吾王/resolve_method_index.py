# -*- coding: utf-8 -*-
"""为了吾王：metadata 全局方法索引 → 运行时 vaddr（离线解析内存 dump）。

用法:
  python resolve_method_index.py <so_mem.bin> <cgm.bin> [目标方法索引] [锚点方法索引] [锚点vaddr]

默认值即 3.0.4 实测：目标=1040(CharacterStats::UpdateFocusPoints)、锚点=1148(SkillRoll::Roll)、
锚点 vaddr=0x205AA9C。

输入准备（设备侧，root）：
  su -c "toybox dd if=/proc/<pid>/mem of=/data/local/tmp/so_mem.bin bs=4096 \
         skip=$((0x400023000000/4096)) count=$((0x44E9000/4096))"   # libil2cpp 四段，连续 72 MB
  su -c "toybox dd if=/proc/<pid>/mem of=/data/local/tmp/cgm.bin bs=8 \
         skip=$((0x4000270F7F30/8)) count=230"                      # codeGenModules 指针数组
  注意：本机 /system/bin/dd 是 toolbox 空壳（报 no such tool），必须用 toybox dd。

原理：codeGenModules[i] 指向 Il2CppCodeGenModule{moduleName@+0, methodPointerCount@+8,
      methodPointers@+0x10}；Assembly-CSharp.dll 的 methodPointers 索引**就等于** metadata
      全局方法索引（用锚点校验，命中即偏移 0）。
"""
import struct
import sys

BASE = 0x400023000000


def main():
    if len(sys.argv) < 3:
        print(__doc__)
        raise SystemExit(1)
    mem = open(sys.argv[1], "rb").read()
    cgm = open(sys.argv[2], "rb").read()
    target = int(sys.argv[3]) if len(sys.argv) > 3 else 1040
    anchor = int(sys.argv[4]) if len(sys.argv) > 4 else 1148
    anchor_va = int(sys.argv[5], 0) if len(sys.argv) > 5 else 0x205AA9C

    def rd(addr, n):
        o = addr - BASE
        return mem[o:o + n] if 0 <= o and o + n <= len(mem) else None

    def u32(a):
        b = rd(a, 4)
        return struct.unpack("<I", b)[0] if b else None

    def u64(a):
        b = rd(a, 8)
        return struct.unpack("<Q", b)[0] if b else None

    def cstr(a, n=96):
        b = rd(a, n)
        if not b:
            return None
        e = b.find(b"\x00")
        return b[:e].decode("utf-8", "replace") if e >= 0 else None

    ptrs = struct.unpack_from("<%dQ" % (len(cgm) // 8), cgm, 0)
    mod = None
    for m in ptrs:
        if not m:
            continue
        p = u64(m)
        name = cstr(p) if p else None
        if name == "Assembly-CSharp.dll":
            mod = (name, u32(m + 8), u64(m + 0x10))
            break
    if not mod:
        raise SystemExit("没找到 Assembly-CSharp.dll 这个 codeGenModule")

    name, cnt, arr_ptr = mod
    print(f"module={name} count={cnt} methodPointers={arr_ptr:#x}")
    vals = struct.unpack("<%dQ" % cnt, rd(arr_ptr, cnt * 8))

    hits = [k for k, v in enumerate(vals) if v == BASE + anchor_va]
    if not hits:
        raise SystemExit(f"锚点未命中（索引 {anchor} / vaddr {anchor_va:#x}）")
    i_anchor = hits[0]
    print(f"锚点 {anchor} 落在 methodPointers[{i_anchor}] ⇒ 索引偏移 {i_anchor - anchor}")

    i_t = target + (i_anchor - anchor)
    if not (0 <= i_t < cnt):
        raise SystemExit(f"目标索引 {i_t} 越界")
    a = vals[i_t]
    print(f"目标 {target} → methodPointers[{i_t}] = {a:#x}  vaddr={a - BASE:#x}  文件偏移={a - BASE - 0x1000:#x}")


if __name__ == "__main__":
    main()
