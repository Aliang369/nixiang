#!/usr/bin/env python3
"""
从 IL2CPP Dumper 输出提取目标方法的 RVA 和真实序言（uint32 小端）。

用途：
  写 Dobby hook 时填 HookSlot 的 rva 和 expectInsn，不再手抄 dump.cs、不再用占位值。

用法：
  python3 extract_il2cpp_preambles.py \\
      --script 03_decompiled/{游戏}/il2cpp_1.1.46/script.json \\
      --so      03_decompiled/{游戏}/apktool_1.1.46/lib/arm64-v8a/libil2cpp.so \\
      --method  'Gameplay.Item.ItemProtocol$$AddItem:0' \\
      --method  'Gameplay.Item.ItemProtocol$$ForceRemoveMoney' \\
      --format  c-enum

method 语法：
  FullName              → 取第一个同名方法
  FullName:2            → 取第 3 个（0-based）同名方法（处理重载）

format:
  c-enum   → 输出 C++ enum 块（直接贴进 Mod.cpp）
  json     → 输出 JSON dict {name: {"rva": ..., "insn": ...}}
  shell    → 输出 kV_XXX=0x... kI_XXX=0x... 行
"""

import argparse
import hashlib
import json
import struct
import sys
from collections import defaultdict
from pathlib import Path


def parse_program_headers(data: bytes):
    """返回 [(p_vaddr, p_offset, p_filesz, p_flags), ...] 只含 PT_LOAD"""
    e_phoff = struct.unpack_from('<Q', data, 0x20)[0]
    e_phentsize = struct.unpack_from('<H', data, 0x36)[0]
    e_phnum = struct.unpack_from('<H', data, 0x38)[0]
    segments = []
    for i in range(e_phnum):
        off = e_phoff + i * e_phentsize
        p_type, p_flags = struct.unpack_from('<II', data, off)
        p_offset, p_vaddr, _, p_filesz, _ = struct.unpack_from('<QQQQQ', data, off + 8)
        if p_type == 1:  # PT_LOAD
            segments.append((p_vaddr, p_offset, p_filesz, p_flags))
    return segments


def va_to_file_off(va: int, segments):
    for vaddr, offset, filesz, _ in segments:
        if vaddr <= va < vaddr + filesz:
            return va - vaddr + offset
    return None


def read_preamble(data: bytes, va: int, segments, size: int = 4):
    fo = va_to_file_off(va, segments)
    if fo is None or fo + size > len(data):
        return None
    return struct.unpack_from('<I', data, fo)[0]


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument('--script', required=True, help='script.json 路径')
    ap.add_argument('--so', required=True, help='libil2cpp.so 路径')
    ap.add_argument('--method', required=True, action='append',
                    help='完整方法名（可多次出现；重载用 FullName:N）')
    ap.add_argument('--format', choices=['c-enum', 'json', 'shell'], default='c-enum')
    ap.add_argument('--short', action='store_true',
                    help='标识符只保留方法名（同一方法多个重载会冲突，一般不加）')
    args = ap.parse_args()

    so_path = Path(args.so)
    data = so_path.read_bytes()
    segments = parse_program_headers(data)

    with open(args.script) as f:
        sd = json.load(f)

    byname = defaultdict(list)
    for s in sd['ScriptMethod']:
        byname[s['Name']].append(s)

    results = []
    for m in args.method:
        if ':' in m and not m.endswith(':'):
            full, idx_s = m.rsplit(':', 1)
            try:
                idx = int(idx_s)
            except ValueError:
                print(f'ERROR: method 索引格式错误 {m}', file=sys.stderr)
                sys.exit(1)
        else:
            full, idx = m, 0
        entries = byname.get(full, [])
        if not entries:
            results.append({'name': full, 'error': 'not found in script.json'})
            continue
        if idx >= len(entries):
            results.append({'name': full, 'error': f'overload index {idx} out of range (have {len(entries)})'})
            continue
        s = entries[idx]
        rva = s['Address']
        insn = read_preamble(data, rva, segments, 4)
        if insn is None:
            results.append({'name': full, 'rva': rva, 'error': 'VA outside PT_LOAD filesz'})
            continue
        results.append({'name': full, 'rva': rva, 'insn': insn})

    # SHA256 so 内容（用于 RVA↔so 绑定记录）
    so_sha = hashlib.sha256(data).hexdigest()
    so_md5 = hashlib.md5(data).hexdigest()

    if args.format == 'json':
        print(json.dumps({
            'so_md5': so_md5,
            'so_sha256': so_sha,
            'so_size': len(data),
            'methods': results,
        }, indent=2, ensure_ascii=False))
        return

    # 生成短标识符（去命名空间、$$ → _）
    def short_name(full):
        if args.short:
            base = full.rsplit('$$', 1)[-1]
        else:
            base = full.replace('$$', '_').replace('.', '_').replace('<', '_').replace('>', '_')
        base = ''.join(c if c.isalnum() or c == '_' else '_' for c in base)
        if base[0].isdigit():
            base = '_' + base
        return base

    if args.format == 'c-enum':
        print(f'// 序言与 RVA 由 extract_il2cpp_preambles.py 自动提取')
        print(f'// libil2cpp.so md5={so_md5} sha256={so_sha[:16]}… size={len(data)}')
        print(f'// 生成时间：{__import__("datetime").datetime.now().strftime("%Y-%m-%d %H:%M")}')
        print()
        print('// RVA')
        print('enum : uintptr_t {')
        for r in results:
            if 'error' in r:
                print(f'    // MISS {r["name"]}: {r["error"]}')
            else:
                print(f'    kRva_{short_name(r["name"])} = 0x{r["rva"]:X},')
        print('};')
        print()
        print('// 真实序言（ELF 文件偏移处 uint32 小端）')
        print('enum : uint32_t {')
        for r in results:
            if 'error' in r:
                continue
            print(f'    kIns_{short_name(r["name"])} = 0x{r["insn"]:08X},')
        print('};')
        return

    # shell
    for r in results:
        if 'error' in r:
            print(f'# MISS {r["name"]}: {r["error"]}')
        else:
            print(f'kV_{short_name(r["name"])}=0x{r["rva"]:X}')
            print(f'kI_{short_name(r["name"])}=0x{r["insn"]:08X}')


if __name__ == '__main__':
    main()
