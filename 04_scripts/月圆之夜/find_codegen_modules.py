#!/usr/bin/env python3.11
# 在内存镜像中定位 Il2CppCodeGenModule 表 → CodeRegistration / MetadataRegistration
# 锚点: CodeGenModule.moduleName 指向 "*.dll" 字符串, +8 methodPointerCount, +0x10 methodPointers[]
import struct, re, sys

DUMP = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/il2cpp_mem_full.bin"
d = open(DUMP, "rb").read()
BASE = 0x774a046000
MSIZE = 0x56d4000
# 段范围(内存布局): r-x text/rodata: 0..0x4e6e664-ish; rw data: 0x4e7e668..0x56d4000
TEXT_END = 0x4e6e664
DATA_START = 0x4e7e668

def u32(o): return struct.unpack_from("<I", d, o)[0]
def u64(o): return struct.unpack_from("<Q", d, o)[0]

def mod_off(addr):
    if BASE <= addr < BASE + MSIZE:
        return addr - BASE
    return None

def read_cstr(o, maxlen=128):
    end = d.find(b"\x00", o, o + maxlen)
    if end < 0: return None
    try:
        s = d[o:end].decode("ascii")
    except Exception:
        return None
    return s

dll_re = re.compile(r"^[A-Za-z0-9_\-.]+\.dll$")

# 1) 全镜像找指针槽: 指向模块内且目标处为 *.dll 字符串
print("[*] 扫描 moduleName 候选...", flush=True)
modules = []  # (struct_off, name, methodCount, mp_off)
# 只扫 data 段指针槽 + codeGenModules 数组通常在 data
cand = []
step = 8
o = 0
while o < MSIZE - 8:
    v = u64(o)
    tgt = mod_off(v)
    if tgt is not None:
        s = read_cstr(tgt)
        if s and dll_re.match(s):
            cand.append((o, tgt, s))
    o += step
print(f"    dll 字符串指针槽: {len(cand)}", flush=True)

# 2) 验证 CodeGenModule 结构: moduleName 槽即结构起点; +8 count u32, +0x10 methodPointers 指针
for slot, tgt, name in cand:
    try:
        cnt = u32(slot + 8)
        mp_ptr = u64(slot + 0x10)
        mp_off = mod_off(mp_ptr)
        if cnt == 0 or cnt > 300000 or mp_off is None:
            continue
        # 抽样 methodPointers[0] 和中部
        p0 = mod_off(u64(mp_off)) if mp_off + 8 < MSIZE else None
        ok = p0 is not None and p0 < TEXT_END and p0 > 0x1000
        pm = mod_off(u64(mp_off + min(cnt-1, 100)*8))
        okm = pm is not None and pm < TEXT_END and pm > 0x1000
        if ok and okm:
            modules.append((slot, name, cnt, mp_off))
    except Exception:
        continue

print(f"[+] 确认 CodeGenModule 结构: {len(modules)}", flush=True)
for tgt, name, cnt, mp in modules[:40]:
    print(f"    va=0x{tgt:x} {name} methods={cnt} mp=0x{mp:x}", flush=True)

# 3) 找指向这些 module 结构的指针数组 (CodeReg.codeGenModules)
mod_offs = set(t for t, _, _, _ in modules)
print("[*] 扫描 codeGenModules 数组...", flush=True)
best = None
o = DATA_START
while o < MSIZE - 8:
    v = u64(o)
    t = mod_off(v)
    if t in mod_offs:
        # 向后数连续 module 指针
        n = 0
        oo = o
        while oo < MSIZE - 8:
            tt = mod_off(u64(oo))
            if tt in mod_offs:
                n += 1; oo += 8
            else:
                break
        if n >= 3:
            print(f"    数组 @va0x{o:x} count={n}", flush=True)
            if best is None or n > best[1]:
                best = (o, n)
        o = oo
        continue
    o += step

if best:
    arr_off, n = best
    print(f"[+] codeGenModules 数组 va=0x{arr_off:x} count={n}", flush=True)
    # CodeReg v24 末尾: ..., uint32 codeGenModulesCount, pad, ptr codeGenModules
    # count 应在数组指针槽前 8 字节
    count_slot = arr_off - 8
    cnt_val = u32(count_slot)
    print(f"    前 8 字节 u32 = {cnt_val} (期望 {n})", flush=True)
    # 向回找 CodeReg 起点: v24 结构 (reversePInvokeWrapperCount u32+pad, ptr) 起
    # 直接打印 arr_off 前 0x60 字节供定位
    for k in range(-0x60, 0x10, 8):
        qv = u64(arr_off + k)
        note = ""
        t2 = mod_off(qv)
        if t2 is not None:
            ss = read_cstr(t2, 48)
            note = f" -> mod+0x{t2:x} " + (f"str={ss!r}" if ss and ss.isprintable() else "")
        print(f"    {k:+04x}: 0x{qv:016x}{note}", flush=True)
