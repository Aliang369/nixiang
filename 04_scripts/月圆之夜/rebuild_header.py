#!/usr/bin/env python3.11
# 重建 ver24.0 标准头部 (按已知/猜测填充) -> 输出给 Il2CppDumper
import struct, sys

SRC = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_runtime.dat"
OUT = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_rebuilt.dat"

# ver24.0 头部字段顺序 (Il2CppDumper Metadata.cs)
FIELDS = [
    "stringLiteral", "stringLiteralData", "stringData",
    "events", "properties", "methods",
    "parameterDefaultValues", "fieldDefaultValues",
    "fieldAndParameterDefaultValueData", "fieldMarshaledSizes",
    "parameters", "fields",
    "genericParameters", "genericParameterConstraints",
    "genericParameterContainers", "genericContainers",
    "nestedTypes", "interfaces", "vtableMethods", "interfaceOffsets",
    "typeDefinitions", "images", "assemblies",
    "fieldRefs", "referencedAssemblies",
    "attributeData", "attributeDataRange",
    "unresolvedIndirectCallParameterTypes", "unresolvedIndirectCallParameterRows",
    "windowsRuntimeTypeNames", "windowsRuntimeStrings",
    "exportedTypeDefinitions",
]

# 已实锤 (off, size)
KNOWN = {
    "stringLiteral": (0x100, 17658 * 8),
    "stringLiteralData": (0x23000, 0x7C000),
    "stringData": (0xA0000, 0x183000),
    "events": (0x223934, 33 * 24),
    "properties": (0x223C4C, 2673 * 20),
    "methods": (0x278A30, 16145 * 32),
    "parameters": (0x5F39B8, 15588 * 12),
    "fields": (0x740AE8, 7471 * 12),
    "typeDefinitions": (0x95C9A4, 35 * 88),  # 待扩
}

d = bytearray(open(SRC, "rb").read())
# sanity=0xFAB11BAF version=24
struct.pack_into("<II", d, 0, 0xFAB11BAF, 24)
for i, name in enumerate(FIELDS):
    off, size = KNOWN.get(name, (0, 0))
    struct.pack_into("<II", d, 8 + i * 8, off, size)
open(OUT, "wb").write(bytes(d))
print(f"→ {OUT}")
print("已填:", [k for k in KNOWN])
print("待定:", [f for f in FIELDS if f not in KNOWN])
