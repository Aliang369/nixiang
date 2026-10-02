# -*- coding: utf-8 -*-
"""通用「short[] 池明文恢复器」。
解密算法： out[i] = (char)(S[off+i] ^ key)， key 为 16 位整数。
恢复法：对每个起点 i，用「明文首字符必为可打印 ASCII 0x20..0x7E」反推候选
key = S[i] ^ c，再沿数组贪婪延伸，凡解出字符落在允许集合内就继续；
长度 >= minlen 的片段即为一个明文串。两种模式分别尝试：
  mode='ascii'  只允许可打印 ASCII
  mode='cjk'    只允许 CJK 区码元（中文串）
输出写 UTF-8 文件（避免控制台编码问题）。
用法: python _pools.py <dex> <out.txt> [minlen]
"""
import sys, io, struct
sys.stdout.reconfigure(encoding='utf-8', errors='replace')
import _dex_disasm as D

ASCII_OK = set(range(0x20, 0x7f)) | {0x09, 0x0a, 0x0d}
CJK_OK = set(range(0x4e00, 0x9fff)) | set(range(0x3000, 0x303f)) | set(range(0xff00, 0xffef))


def okch(c, mode):
    return c in (ASCII_OK if mode == 'ascii' else CJK_OK)


def walk_payloads(dex, blob):
    """返回 list of dict(owner, meth, co, po, ew, size, entries)"""
    dis = D.Disasm(dex)
    res, seen = [], set()
    for cd in dex.class_defs():
        cname = dex.type_desc(cd['class_idx'])
        for (mi, acc, co, kind) in dex.all_methods(cd):
            if co == 0:
                continue
            regs, ins, outs, tries, dbg, n = struct.unpack_from('<HHHHII', blob, co)
            cs = co + 16
            insns = list(struct.unpack_from('<%dH' % n, blob, cs))
            pc = 0
            while pc < n:
                text, L, ok = dis.insn_at(insns, pc, cs)
                if L <= 0:
                    L = 1
                if (insns[pc] & 0xFF) == 0x26:     # fill-array-data
                    off = struct.unpack_from('<i', blob, cs + pc * 2 + 2)[0]
                    po = cs + (pc + off) * 2
                    if po not in seen and 0 <= po < len(blob) - 8:
                        ident, ew, size = struct.unpack_from('<HHI', blob, po)
                        if ident == 0x0300 and size < 200000 and 8 + ew * size <= len(blob):
                            seen.add(po)
                            ents = []
                            for k in range(size):
                                if ew == 2:
                                    ents.append(struct.unpack_from('<h', blob, po + 8 + 2 * k)[0] & 0xFFFF)
                                elif ew == 1:
                                    ents.append(blob[po + 8 + k])
                                elif ew == 4:
                                    ents.append(struct.unpack_from('<i', blob, po + 8 + 4 * k)[0] & 0xFFFFFFFF)
                            res.append({'owner': cname, 'meth': dex.method_full(mi), 'co': co,
                                        'po': po, 'ew': ew, 'size': size, 'entries': ents})
                pc += L
    return res


def recover(entries, minlen, mode):
    out = []
    n = len(entries)
    for i in range(n):
        found = set()
        for c in range(0x20, 0x7f):
            key = entries[i] ^ c
            j, chars = i, []
            while j < n and okch(entries[j] ^ key, mode):
                chars.append(chr(entries[j] ^ key))
                j += 1
            if len(chars) >= minlen:
                txt = ''.join(chars)
                letters = sum(1 for ch in txt if ch.isalnum() or ord(ch) > 0x2000)
                if letters >= max(2, len(txt) // 2) and key not in found:
                    found.add(key)
                    out.append((i, key, txt))
    out.sort(key=lambda x: (-len(x[2]), x[0]))
    kept = []
    for (i, k, t) in out:
        if any(i2 <= i and i + len(t) <= i2 + len(t2) and k == k2 for (i2, k2, t2) in kept):
            continue
        kept.append((i, k, t))
    kept.sort(key=lambda x: x[0])
    return kept


def main():
    dexpath, outpath = sys.argv[1], sys.argv[2]
    minlen = int(sys.argv[3]) if len(sys.argv) > 3 else 5
    blob = open(dexpath, 'rb').read()
    dex = D.Dex(blob, dexpath)
    pools = walk_payloads(dex, blob)
    allstr = {}
    with io.open(outpath, 'w', encoding='utf-8', newline='\n') as f:
        f.write('file=%s  fill-array-data 数组 %d 个  minlen=%d\n\n' % (dexpath, len(pools), minlen))
        for mode, ml in (('ascii', minlen), ('cjk', 3)):
            f.write('\n############ mode=%s  minlen=%d ############\n' % (mode, ml))
            for p in pools:
                if p['ew'] != 2:
                    continue
                rec = recover(p['entries'], ml, mode)
                if not rec:
                    continue
                f.write('=== pool po=0x%X size=%d owner=%s :: %s -> %d 段 ===\n' %
                        (p['po'], p['size'], p['owner'], p['meth'], len(rec)))
                for (i, k, t) in rec:
                    f.write('   off=%4d key=0x%04X(%d)  %r\n' % (i, k, k, t))
                    allstr.setdefault(t, set()).add(k)
                f.write('\n')
        f.write('\n======== 全部去重明文（%d 条） ========\n' % len(allstr))
        for t in sorted(allstr, key=lambda s: (-len(s), s)):
            f.write('%s\n' % t)
    print('WROTE', outpath, 'pools=', len(pools), 'uniq_strings=', len(allstr))


if __name__ == '__main__':
    main()
