# -*- coding: utf-8 -*-
"""
_dex_strdec.py  —  classes5.dex 字符串解密器（纯标准库）

反汇编得到解密函数：
  Lcom/android/support/۟ۥۣ۟۟;->۟ۥۡ۟۟([SIII)Ljava/lang/String;
  code_off=0x66EF0, regs=7 ins=4  -> 参数映射 v3=arg0=short[] S, v4=arg1=off, v5=arg2=len, v6=arg3=key
  语义：  char[] c = new char[len]; for(i=0;i<len;i++) c[i] = (char)(S[off+i] ^ key);
          return new String(c);

用法：
  python _dex_strdec.py <dex> --arrays                 列出所有 fill-array-data 的 short[] 数组
  python _dex_strdec.py <dex> --dump <cls> <meth> <key> 解密某方法内嵌数组
  python _dex_strdec.py <dex> --sites                  列出所有 [SIII 解密调用点
"""
import io
import sys
import struct
import re

sys.path.insert(0, r'E:\nixiang\06_dumped\冒险与征途')
import _dex_disasm as DD

if hasattr(sys.stdout, 'reconfigure'):
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')

DEC_SIG = '\u06df\u06e5\u06e1\u06df\u06df;->\u06df\u06e5\u06e1\u06df\u06df'  # 占位，下面用真名替代


def code_of(dex, code_off):
    buf = dex.buf
    regs, ins, outs, tries, dbg, n = struct.unpack_from('<HHHHII', buf, code_off)
    insns = list(struct.unpack_from('<%dH' % n, buf, code_off + 16))
    return regs, ins, outs, n, insns


def all_methods(dex):
    """yield (mfull, midx, acc, code_off, kind)"""
    for cd in dex.class_defs():
        data = dex.class_data(cd)
        if not data:
            continue
        for kind in ('direct', 'virtual'):
            for mi, acc, code_off in data[kind]:
                yield dex.method_full(mi), mi, acc, code_off, kind


def extract_arrays(dex, code_off):
    """返回 [(pc, elem_width, [values...])]"""
    res = []
    regs, ins, outs, n, insns = code_of(dex, code_off)
    pc = 0
    while pc < n:
        w = insns[pc]
        if (w & 0xFF) == 0 and ((w >> 8) & 0xFF) in (1, 2, 3):
            size = insns[pc + 1]
            if (w >> 8) == 1:      # packed-switch
                pc += 4 + size * 2
            elif (w >> 8) == 2:    # sparse-switch
                pc += 2 + size * 4
            else:                  # array-data-payload
                ew = insns[pc + 1]
                cnt = insns[pc + 2] | (insns[pc + 3] << 16)
                base = pc + 4
                vals = []
                if ew == 2:
                    for i in range(cnt):
                        vals.append(insns[base + i])
                elif ew == 1:
                    for i in range(cnt):
                        v = insns[base + i // 2]
                        vals.append((v >> (8 * (i % 2))) & 0xFF)
                elif ew == 4:
                    for i in range(cnt):
                        vals.append(insns[base + 2 * i] | (insns[base + 2 * i + 1] << 16))
                elif ew == 8:
                    for i in range(cnt):
                        vals.append(insns[base + 4 * i] | (insns[base + 4 * i + 1] << 16))
                res.append((pc, ew, vals))
                pc = base + ((cnt * ew + 1) // 2)
                continue
            continue
        # 用长度表前进
        text, L, ok = DIS.insn_at(insns, pc, 0)
        if L <= 0:
            L = 1
        pc += L
    return res


def dec(arr, off, ln, key):
    return ''.join(chr((arr[off + i] ^ key) & 0xFFFF) for i in range(ln)
                   if off + i < len(arr))


def printable(s):
    return ''.join(ch if 32 <= ord(ch) < 127 else '.' for ch in s)


def main():
    args = sys.argv[1:]
    if not args:
        print(__doc__)
        return
    path = args[0]
    blob = open(path, 'rb').read()
    dex = DD.Dex(blob, path)
    global DIS
    DIS = DD.Disasm(dex)

    if '--arrays' in args:
        for mfull, mi, acc, co, kind in all_methods(dex):
            if not co:
                continue
            for pc, ew, vals in extract_arrays(dex, co):
                print('%s  pc=%d ew=%d n=%d  head=%s' %
                      (mfull, pc, ew, len(vals), ' '.join('%04x' % v for v in vals[:12])))
        return

    if '--sites' in args:
        pat = re.compile(r'\[SIII\)Ljava/lang/String;$')
        for mfull, mi, acc, co, kind in all_methods(dex):
            if not co:
                continue
            regs, ins, outs, n, insns = code_of(dex, co)
            pc = 0
            while pc < n:
                w = insns[pc]
                if (w & 0xFF) == 0 and ((w >> 8) & 0xFF) in (1, 2, 3):
                    text, L, ok = DIS.insn_at(insns, pc, 0)
                    if L <= 0:
                        L = 1
                    # 跳过 payload 更稳妥：直接用长度表
                    if (w >> 8) == 1:
                        pc += 4 + insns[pc + 1] * 2
                    elif (w >> 8) == 2:
                        pc += 2 + insns[pc + 1] * 4
                    else:
                        pc += 4 + ((insns[pc + 2] | (insns[pc + 3] << 16)) *
                                   insns[pc + 1] + 1) // 2
                    continue
                if (w & 0xFF) == 0x71:  # invoke-static
                    idx = insns[pc + 1]
                    if idx < dex.method_ids_size:
                        m = dex.method_full(idx)
                        if pat.search(m):
                            print('%s @pc=%d  %s' % (mfull, pc, m))
                text, L, ok = DIS.insn_at(insns, pc, 0)
                if L <= 0:
                    L = 1
                pc += L
        return

    if '--dumpoff' in args:
        i = args.index('--dumpoff')
        want = int(args[i + 1], 0)
        key = int(args[i + 2], 0)
        for mfull, mi, acc, co, kind in all_methods(dex):
            if co != want:
                continue
            print('=== %s code_off=0x%X key=0x%X' % (mfull, co, key))
            for pc, ew, vals in extract_arrays(dex, co):
                if ew != 2:
                    continue
                s = dec(vals, 0, len(vals), key)
                print('  [pc=%d] n=%d' % (pc, len(vals)))
                print('  ' + printable(s))
        return

    if '--dump' in args:
        i = args.index('--dump')
        cls, meth, key = args[i + 1], args[i + 2], int(args[i + 3], 0)
        for mfull, mi, acc, co, kind in all_methods(dex):
            if cls in mfull and meth in mfull and co:
                print('=== %s code_off=0x%X key=0x%X' % (mfull, co, key))
                for pc, ew, vals in extract_arrays(dex, co):
                    if ew != 2:
                        continue
                    s = dec(vals, 0, len(vals), key)
                    print('  [pc=%d] n=%d' % (pc, len(vals)))
                    print('  ' + printable(s))
        return

    if '--try' in args:
        # 对某方法数组，暴力枚举可能 key，找可打印率最高的
        i = args.index('--try')
        cls, meth = args[i + 1], args[i + 2]
        for mfull, mi, acc, co, kind in all_methods(dex):
            if cls in mfull and meth in mfull and co:
                for pc, ew, vals in extract_arrays(dex, co):
                    if ew != 2:
                        continue
                    best = []
                    for k in range(0x10000):
                        s = dec(vals, 0, min(len(vals), 64), k)
                        good = sum(1 for ch in s if 32 <= ord(ch) < 127)
                        if good >= 60:
                            best.append((k, good, s[:48]))
                    for k, g, s in best[:40]:
                        print('key=0x%04X good=%d  %s' % (k, g, printable(s)))
                    print('--- 数组 %s n=%d 枚举完毕，命中 %d 个 key' % (mfull, len(vals), len(best)))
        return

    print('未知参数')


DIS = None
main()
