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

索引文件（--index-out，2026-10-02 P1-3 定）：
  从 dump.cs 生成 methods.txt / classes.txt，**首行固定写列序注释头**（消费侧必须跳过
  以 '#' 开头的行，用 iter_index_rows()）：

    methods.txt 首行： # RVA|file_off|cid|class decl|method sig
      第 1 列 RVA      —— 运行时虚拟地址，hook / IDA / Ghidra 里用的就是它
      第 2 列 file_off —— ELF 文件偏移，= RVA − 段加载差（该 so 实测 0x4000）
      第 3 列 cid      —— 该 class 声明在 dump.cs 里的行号（1-based）
      第 4/5 列        —— class 声明、方法签名
      ⚠️ 第 1 列才是 RVA。把第 2 列 file_off 当 RVA 用会整体错位 0x4000，反汇编必错
         （2026-10-02 疾风骑士会话踩过 2 次）。

    classes.txt 首行： # cid:class decl

用法（生成索引）：
  python3 extract_il2cpp_preambles.py \\
      --dump 06_dumped/{游戏}/il2cppdump/out/dump.cs \\
      --index-out /tmp/{游戏}/              # 产出 methods.txt + classes.txt（带列序头）
  python3 extract_il2cpp_preambles.py --index-verify /tmp/{游戏}/methods.txt
"""

import argparse
import hashlib
import json
import re
import struct
import sys
from collections import defaultdict
from pathlib import Path

# ---------------------------------------------------------------------------
# 索引文件列序（P1-3，2026-10-02）：生成时首行写死，消费侧必须跳过 '#' 行。
# 历史事故：疾风骑士会话 2 次把 file_off 列当 RVA 用，反汇编整体错位 0x4000。
# ---------------------------------------------------------------------------
METHODS_HEADER = '# RVA|file_off|cid|class decl|method sig'
CLASSES_HEADER = '# cid:class decl'

# dump.cs 里的类/枚举/结构/接口声明行，以及方法前的 "// RVA: 0x… Offset: 0x… VA: 0x…"
_CLASS_DECL_RE = re.compile(
    r'^(?:public|internal|private|protected|static|sealed|abstract|partial|\s)*'
    r'(?:class|struct|enum|interface)\b')
_RVA_RE = re.compile(r'//\s*RVA:\s*0x([0-9A-Fa-f]+)\s+Offset:\s*0x([0-9A-Fa-f]+)')
_TYPEDEFINDEX_RE = re.compile(r'\s*//\s*TypeDefIndex:.*$')


def write_index(path, header, rows):
    """写索引文件：首行固定为列序注释头，其后逐行数据（P1-3）。"""
    with open(path, 'w', encoding='utf-8') as f:
        f.write(header + '\n')
        for r in rows:
            f.write(r + '\n')


def iter_index_rows(path):
    """读 methods.txt / classes.txt，自动跳过 '#' 注释头与空行。

    这两个文件首行是列序注释头，消费侧一律走本函数（或等效的 grep -v '^#'），
    否则首行会被当成数据行解析。
    """
    with open(path, encoding='utf-8', errors='replace') as f:
        for line in f:
            line = line.rstrip('\n')
            if not line or line.startswith('#'):
                continue
            yield line


def build_index_rows(dump_path):
    """从 dump.cs 抽 methods.txt / classes.txt 的数据行（不含列序注释头）。

    列序见 METHODS_HEADER：RVA|file_off|cid|class decl|method sig（cid = dump.cs 行号）。
    """
    methods = []
    classes = []
    cur_cid = None
    cur_decl = None
    pending = None  # 上一行是 "// RVA: …" 时暂存 (rva, file_off)
    with open(dump_path, encoding='utf-8', errors='replace') as f:
        for lineno, raw in enumerate(f, 1):
            line = raw.rstrip('\n')
            m = _RVA_RE.search(line)
            if m:
                pending = (int(m.group(1), 16), int(m.group(2), 16))
                continue
            stripped = line.strip()
            if not stripped:
                continue
            if _CLASS_DECL_RE.match(stripped):
                cur_cid = lineno
                cur_decl = _TYPEDEFINDEX_RE.sub('', stripped)
                classes.append('%d:%s' % (lineno, stripped))
                pending = None
                continue
            if pending is not None:
                rva, foff = pending
                pending = None
                if cur_cid is not None:
                    methods.append('0x%X|0x%X|%d|%s|%s'
                                   % (rva, foff, cur_cid, cur_decl, stripped))
                continue
    return methods, classes


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
    ap.add_argument('--script', help='script.json 路径（序言模式必填）')
    ap.add_argument('--so', help='libil2cpp.so 路径（序言模式必填）')
    ap.add_argument('--method', action='append',
                    help='完整方法名（可多次出现；重载用 FullName:N）（序言模式必填）')
    ap.add_argument('--format', choices=['c-enum', 'json', 'shell'], default='c-enum')
    ap.add_argument('--short', action='store_true',
                    help='标识符只保留方法名（同一方法多个重载会冲突，一般不加）')
    ap.add_argument('--dump', help='dump.cs 路径（索引模式必填）')
    ap.add_argument('--index-out', metavar='DIR',
                    help='索引模式：在 DIR 下生成 methods.txt + classes.txt（首行为列序注释头）')
    ap.add_argument('--index-verify', metavar='FILE',
                    help='索引模式：按 iter_index_rows() 复读索引文件，报告行数并打印首行数据行')
    args = ap.parse_args()

    # ---------- 索引模式（P1-3）：生成 / 复读 methods.txt、classes.txt ----------
    if args.index_out or args.index_verify:
        if args.index_out:
            if not args.dump:
                print('ERROR: --index-out 需要 --dump <dump.cs>', file=sys.stderr)
                sys.exit(1)
            dump_path = Path(args.dump)
            if not dump_path.is_file():
                print(f'ERROR: dump.cs 不存在: {dump_path}', file=sys.stderr)
                sys.exit(1)
            methods, classes = build_index_rows(dump_path)
            out_dir = Path(args.index_out)
            out_dir.mkdir(parents=True, exist_ok=True)
            m_path = out_dir / 'methods.txt'
            c_path = out_dir / 'classes.txt'
            write_index(m_path, METHODS_HEADER, methods)
            write_index(c_path, CLASSES_HEADER, classes)
            print(f'✓ methods.txt {len(methods)} 行 → {m_path}')
            print(f'  首行（列序头，第 1 列才是 RVA）: {METHODS_HEADER}')
            print(f'✓ classes.txt {len(classes)} 行 → {c_path}')
            print(f'  首行（列序头）: {CLASSES_HEADER}')
        for f in ([args.index_verify] if args.index_verify else
                  [str(Path(args.index_out) / 'methods.txt'),
                   str(Path(args.index_out) / 'classes.txt')]):
            p = Path(f)
            if not p.is_file():
                print(f'ERROR: 索引文件不存在: {p}', file=sys.stderr)
                sys.exit(1)
            n = 0
            first = None
            with open(p, encoding='utf-8', errors='replace') as fh:
                head = fh.readline().rstrip('\n')
            for row in iter_index_rows(p):
                n += 1
                if first is None:
                    first = row
            ok = head.startswith('#')
            print(f'{"✓" if ok else "✗"} {p}: 列序头={"有" if ok else "缺失"}（{head}），'
                  f'数据行={n}，首行数据行={first}')
            if not ok:
                sys.exit(1)
        return

    if not (args.script and args.so and args.method):
        print('ERROR: 序言模式需要 --script + --so + --method（索引模式见 --help）',
              file=sys.stderr)
        sys.exit(1)

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
