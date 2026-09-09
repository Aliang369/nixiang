#!/usr/bin/env python3
# 无限升级 · 从运行时 dump 定位 AOT 方法原生 VA（免 Il2CppDumper）
# 原理（爱加密壳把 CodeRegistration/MetadataRegistration 搬进 [anon:.bss]，
#       磁盘 .dynsym 无 il2cpp_* 导出、Il2CppDumper auto 失效）：
#   1. bss 里搜「指向 codeGenModules 模块名字符串的绝对指针」→ 模块结构体
#      （Il2CppCodeGenModule: name, methodPointerCount, methodPointers...）
#   2. metadata 解析出目标类的方法 token rid
#   3. methodPointers[rid-1] 即原生 VA（运行时绝对指针 - 运行时基址）
# 依赖：06_dumped/无限升级/bss/（真机 /proc/pid/mem dump）、global-metadata.memdump_fixed.dat
import struct, sys, os

WS = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
BSS_DIR = f"{WS}/06_dumped/无限升级/bss"
META = f"{WS}/03_decompiled/无限升级/global-metadata.memdump_fixed.dat"
# bss 各段 VA 起点（与 bss{1..6}.bin 对应；换进程需按新 maps 重算，见进度文档）
BSS_VA = {
    'bss1': 0x774c4d8000, 'bss2': 0x774c4dc000, 'bss3': 0x774c4df000,
    'bss4': 0x774c963000, 'bss5': 0x774c966000, 'bss6': 0x774cebf000,
}

def load_bss():
    out = {}
    for n, va in BSS_VA.items():
        p = f"{BSS_DIR}/{n}.bin"
        if os.path.exists(p):
            out[n] = (va, open(p, 'rb').read())
    return out

def find_ptr_to(bss, val):
    """返回 [(region, offset, abs_addr)] 指向 val 的 8 字节指针"""
    t = struct.pack('<Q', val)
    hits = []
    for n, (va, d) in bss.items():
        i = d.find(t)
        while i >= 0:
            hits.append((n, i, va + i)); i = d.find(t, i + 1)
    return hits

def parse_metadata():
    m = open(META, 'rb').read()
    names = ["stringLiteral", "stringLiteralData", "string", "events", "properties", "methods",
             "parameterDefaultValues", "fieldDefaultValues", "fieldAndParameterDefaultValueData",
             "fieldMarshaledSizes", "parameters", "fields", "genericParameters",
             "genericParameterConstraints", "genericContainers", "nestedTypes", "interfaces",
             "vtableMethods", "interfaceOffsets", "typeDefinitions", "images", "assemblies",
             "fieldRefs", "referencedAssemblies", "attributeData", "attributeDataRange",
             "unresolvedVirtualCallParameterTypes", "unresolvedVirtualCallParameterRanges",
             "windowsRuntimeTypeNames", "windowsRuntimeStrings", "exportedTypeDefinitions"]
    hdr = {nm: struct.unpack_from('<ii', m, 8 + i * 8) for i, nm in enumerate(names)}
    SOff = hdr['string'][0]
    def S(idx):
        e = m.index(b'\0', SOff + idx); return m[SOff + idx:e].decode(errors='replace')
    return m, hdr, S

def resolve(image_name, type_name, namespace_filter=None):
    bss = load_bss()
    m, hdr, S = parse_metadata()
    # 1) metadata: image -> type -> methods
    iOff, iSz = hdr['images']
    img = None
    for k in range(iSz // 40):
        o = iOff + k * 40
        if S(struct.unpack_from('<i', m, o)[0]) == image_name:
            _, _, typeStart, typeCount = struct.unpack_from('<iiii', m, o); img = (typeStart, typeCount); break
    assert img, f"image {image_name} not found"
    tOff = hdr['typeDefinitions'][0]
    for i in range(img[0], img[0] + img[1]):
        o = tOff + i * 88
        nmI, nsI = struct.unpack_from('<ii', m, o)
        if S(nmI) == type_name and (namespace_filter is None or S(nsI) == namespace_filter):
            mstart, = struct.unpack_from('<i', m, o + 36)
            mcount = struct.unpack_from('<H', m, o + 64)[0]
            break
    else:
        raise SystemExit(f"type {type_name} not found")
    mOff = hdr['methods'][0]
    methods = []
    for j in range(mcount):
        o = mOff + (mstart + j) * 36
        nmI, _, _, _, _, _, token = struct.unpack_from('<7i', m, o)
        methods.append((S(nmI), token & 0xFFFFFF))
    # 2) bss: 模块结构体（第一个 name 指针命中的即是）
    strVa = None
    # 模块名字符串 VA：在运行时镜像的 rodata，用「绝对指针=基址+VA」搜索失败时改搜 raw VA。
    # 本游戏实测：bss 里存的是运行时绝对指针，字符串 VA 需先从任一已知模块结构体反推。
    # 这里直接用已实锤的 YooAssetAesCrypto 所在 Assembly-CSharp 模块（dump 时实测值）。
    STR_VA = {  # image_name -> 模块名字符串 VA（第一次跑时用 bss3 搜 'abs' 实测写入）
        'Assembly-CSharp.dll': 0x113fea8, 'mscorlib.dll': 0x1116888,
    }
    base = 0x7746a40000  # 运行时基址（dump 时 maps 第一行）
    strVa = STR_VA.get(image_name)
    if strVa is None:
        raise SystemExit(f"先把 {image_name} 的模块名字符串 VA 加入 STR_VA（搜 bss 里指向 rodata 字符串的绝对指针）")
    hits = find_ptr_to(bss, base + strVa)
    assert hits, "module struct not found in bss"
    n, off, _ = hits[0]
    d = bss[n][1]
    mpc = struct.unpack_from('<I', d, off + 8)[0]
    mpAbs = struct.unpack_from('<Q', d, off + 16)[0]
    mpVa = mpAbs - base
    # 3) methodPointers 数组本体也在 bss（或堆页），先在 bss 里找
    tgt = (n, mpVa - bss[n][0])
    print(f"module {image_name}: count={mpc} methodPointers VA=0x{mpVa:x} (bss:{tgt[0]} off=0x{tgt[1]:x})")
    if 0 <= tgt[1] <= len(d) - 8 * mpc:
        arr = d[tgt[1]:tgt[1] + 8 * mpc]
        for name, rid in methods:
            va = struct.unpack_from('<Q', arr, 8 * (rid - 1))[0] - base
            print(f"  {name:40s} rid={rid} VA=0x{va:x}")
    else:
        print("  methodPointers 数组不在该 bss 段——需按 VA 读堆页（见进度文档 §dump）")

if __name__ == "__main__":
    if len(sys.argv) < 3:
        print("usage: resolve_il2cpp_method_va.py <Assembly.dll> <TypeName> [namespace]")
        sys.exit(1)
    resolve(sys.argv[1], sys.argv[2], sys.argv[3] if len(sys.argv) > 3 else None)
