#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
so_inspect.py — ELF(AArch64) / APK 取证工具箱（只读）

为什么要有这个脚本（2026-10-12 霓虹深渊2 会话效率复盘 P1-11）
-------------------------------------------------------------
那一轮在 bash 里现场写了 **200 次** `python3 - <<PY` 一次性分析（合计 472 KB 输出），
加上 md5/shasum 104 次、readelf/objdump 53 次、unzip/zip 23 次 —— 同样的取证反复手写，
结论却散在对话里。这里把最常用的六件事收敛成子命令（都只读、都打印可复制的结论行）。

子命令
------
  sha      <文件>...                      大小 + md5 + sha256（一行一个文件）
  strings  <so/文件> [--grep RE] [--min N] [--utf8/--no-utf8] [--max N]
                                          UTF-8 感知的字符串抽取
                                          ★ 默认 --utf8：`strings` 会把 >=0x80 的字节当不可打印，
                                            中文菜单串全部漏掉（会话里的实证结论）
  zip      <apk> [--list] [--grep RE] [--entry 名] [--extract 目标目录]
                                          # 条目名/大小/压缩后大小/sha256；--entry 打印条目内容摘要
  hex      <文件> --off 0xNN [--n 64]     文件偏移处的字节
  disasm   <文件> --off 0xNN [--n 16] [--va 0xNN] [--arch aarch64]
                                          capstone 反汇编（装了 capstone 才有；未装只报提示）
  ptr      <文件> --off 0xNN [--n 32] [--va-base 0xNN]
                                          按 8 字节读指针表（.rela.dyn / 指针数组场景）

退出码：0=成功；1=无命中；2=用法/文件错
"""
import sys, re, os, hashlib, zipfile, argparse, struct

def _sha(path):
    h1, h2 = hashlib.md5(), hashlib.sha256()
    with open(path, 'rb') as f:
        for chunk in iter(lambda: f.read(1 << 20), b''):
            h1.update(chunk); h2.update(chunk)
    return h1.hexdigest(), h2.hexdigest()

def cmd_sha(a):
    for p in a.files:
        if not os.path.isfile(p):
            print(f"✗ 不存在: {p}"); return 2
        m, s = _sha(p)
        print(f"{os.path.getsize(p):>14,} B  md5={m}  sha256={s}  {p}")
    return 0

def cmd_strings(a):
    data = open(a.file, 'rb').read()
    out, cur = [], bytearray()
    def flush():
        if len(cur) >= a.min:
            out.append(cur.decode('utf-8', 'replace'))
        cur.clear()
    for b in data:
        printable = (32 <= b < 127) or (b >= 0x80 if a.utf8 else False)
        if printable:
            cur.append(b)
        else:
            flush()
    flush()
    if a.grep:
        rx = re.compile(a.grep)
        out = [s for s in out if rx.search(s)]
    if not out:
        print(f"✗ 无命中（共扫描 {len(data)} B；--utf8={a.utf8} --min={a.min}）")
        return 1
    for s in out[:a.max]:
        print(s)
    if len(out) > a.max:
        print(f"…（共 {len(out)} 条，已截到 {a.max}）")
    return 0

def cmd_zip(a):
    if not zipfile.is_zipfile(a.apk):
        print(f"✗ 不是 zip/apk: {a.apk}"); return 2
    z = zipfile.ZipFile(a.apk)
    if a.extract:
        os.makedirs(a.extract, exist_ok=True)
        names = a.entries or ([a.entry] if a.entry else [])
        if not names:
            print("✗ --extract 需要 --entry 或 --entries"); return 2
        for n in names:
            if n not in z.namelist():
                print(f"✗ 条目不存在: {n}"); return 2
            dst = os.path.join(a.extract, os.path.basename(n))
            with z.open(n) as src, open(dst, 'wb') as f:
                f.write(src.read())
            m, s = _sha(dst)
            print(f"✓ 已取出 {n} → {dst}（{os.path.getsize(dst):,} B md5={m} sha256={s}）")
        return 0
    if a.entry:
        if a.entry not in z.namelist():
            print(f"✗ 条目不存在: {a.entry}"); return 2
        i = z.getinfo(a.entry)
        blob = z.read(a.entry)
        m = hashlib.md5(blob).hexdigest(); s = hashlib.sha256(blob).hexdigest()
        print(f"{a.entry}\n  原始={i.file_size:,} B 压缩后={i.compress_size:,} B "
              f"CRC={i.CRC:08x} md5={m} sha256={s}")
        return 0
    rx = re.compile(a.grep) if a.grep else None
    n = 0
    total = 0
    for i in z.infolist():
        total += i.file_size
        if rx and not rx.search(i.filename):
            continue
        n += 1
        print(f"{i.file_size:>13,} → {i.compress_size:>12,}  {i.filename}")
    print(f"-- 共 {len(z.infolist())} 条 / 解压后合计 {total:,} B；本次列出 {n} 条")
    return 0

def cmd_hex(a):
    data = open(a.file, 'rb').read()
    off = int(a.off, 0)
    if off >= len(data):
        print(f"✗ --off 0x{off:x} 越界（文件 {len(data):,} B）"); return 2
    chunk = data[off:off + a.n]
    print(f"{a.file} @ file-off 0x{off:x}（{len(chunk)} B）")
    for i in range(0, len(chunk), 16):
        row = chunk[i:i+16]
        print(f"  0x{off+i:08x}  {' '.join(f'{b:02x}' for b in row):<47}  "
              f"{''.join(chr(b) if 32 <= b < 127 else '.' for b in row)}")
    print(f"  hex: {' '.join(f'{b:02x}' for b in chunk)}")
    return 0

def cmd_disasm(a):
    try:
        from capstone import Cs, CS_ARCH_ARM64, CS_ARCH_X86, CS_MODE_LITTLE_ENDIAN
    except Exception:
        print("✗ 未安装 capstone（pip3 install capstone 后可反汇编）；已装的话请确认 python3 用的是同一解释器")
        return 2
    data = open(a.file, 'rb').read()
    off = int(a.off, 0)
    blk = data[off:off + a.n * 4]
    va = int(a.va, 0) if a.va else off
    arch = CS_ARCH_ARM64 if a.arch in ('aarch64', 'arm64') else CS_ARCH_X86
    md = Cs(arch, CS_MODE_LITTLE_ENDIAN)
    print(f"{a.file} @ file-off 0x{off:x}（va={va:#x}）")
    n = 0
    for ins in md.disasm(blk, va):
        n += 1
        print(f"  {ins.address:#014x}  {' '.join(f'{b:02x}' for b in ins.bytes):<11} {ins.mnemonic}\t{ins.op_str}")
    if n == 0:
        print("  ⚠ 该偏移没解出指令 —— 多半落在数据段/未对齐，或 --off 不是代码偏移；"
              "先 `readelf -S <文件>` 找 .text 的 file-off。")
        return 1
    return 0

def cmd_ptr(a):
    data = open(a.file, 'rb').read()
    off = int(a.off, 0); base = int(a.va_base, 0) if a.va_base else 0
    n = 0
    for i in range(a.n):
        p = off + i * 8
        if p + 8 > len(data):
            break
        v = struct.unpack_from('<Q', data, p)[0]
        print(f"  [{i:>3}] file-off 0x{p:x}  {v:#018x}" + (f"  va={base + v:#x}" if base else ""))
        n += 1
    print(f"-- 共读 {n} 个指针")
    return 0

def main():
    ap = argparse.ArgumentParser(add_help=False)
    sub = ap.add_subparsers(dest='cmd')
    p = sub.add_parser('sha'); p.add_argument('files', nargs='+'); p.set_defaults(f=cmd_sha)
    p = sub.add_parser('strings'); p.add_argument('file'); p.add_argument('--grep'); p.add_argument('--min', type=int, default=4)
    p.add_argument('--utf8', dest='utf8', action='store_true'); p.add_argument('--no-utf8', dest='utf8', action='store_false')
    p.add_argument('--max', type=int, default=400); p.set_defaults(f=cmd_strings, utf8=True)
    p = sub.add_parser('zip'); p.add_argument('apk'); p.add_argument('--list', action='store_true'); p.add_argument('--grep')
    p.add_argument('--entry'); p.add_argument('--entries', nargs='+'); p.add_argument('--extract'); p.set_defaults(f=cmd_zip)
    p = sub.add_parser('hex'); p.add_argument('file'); p.add_argument('--off', required=True); p.add_argument('--n', type=int, default=64); p.set_defaults(f=cmd_hex)
    p = sub.add_parser('disasm'); p.add_argument('file'); p.add_argument('--off', required=True); p.add_argument('--n', type=int, default=16)
    p.add_argument('--va'); p.add_argument('--arch', default='aarch64'); p.set_defaults(f=cmd_disasm)
    p = sub.add_parser('ptr'); p.add_argument('file'); p.add_argument('--off', required=True); p.add_argument('--n', type=int, default=32)
    p.add_argument('--va-base'); p.set_defaults(f=cmd_ptr)
    if len(sys.argv) < 2 or sys.argv[1] in ('-h', '--help'):
        print(__doc__); return 0 if len(sys.argv) > 1 else 2
    a = ap.parse_args()
    if not getattr(a, 'f', None):
        print(__doc__); return 2
    return a.f(a)

if __name__ == '__main__':
    sys.exit(main())
