#!/usr/bin/env python3
"""fg_xref.py — 运行期镜像(已解密载荷)线性反汇编索引与交叉引用（只读）

镜像按 raw 处理（LOAD#0 Offset==VirtAddr，故 image 偏移 = VA）。
用法:
  python3 fg_xref.py build
  python3 fg_xref.py around A [N]
  python3 fg_xref.py back A [SPAN]
  python3 fg_xref.py xrefs A
  python3 fg_xref.py calls A [END]
  python3 fg_xref.py thunks
  python3 fg_xref.py const V
  python3 fg_xref.py svc
"""
import sys, pickle, os, collections
import capstone

HERE = os.path.dirname(os.path.abspath(__file__))
IMG = os.path.abspath(os.path.join(HERE, '..', '..', '06_dumped', '绿梦_20261016',
                                   'fg_runtime', 'libFairGuard_rwxp.bin'))
CACHE = '/tmp/fg_idx.pkl'
LO, HI = 0x400, 0x1F0000


def load_img():
    with open(IMG, 'rb') as f:
        return f.read()


def build():
    img = load_img()
    md = capstone.Cs(capstone.CS_ARCH_ARM64, capstone.CS_MODE_ARM)
    ins = []
    addr = LO
    skips = 0
    while addr < HI:
        last = None
        for i in md.disasm(img[addr:HI], addr):
            last = i
            ins.append((i.address, i.size, i.mnemonic, i.op_str, bytes(i.bytes)))
        if last is None:
            addr += 4
            skips += 1
        else:
            nxt = last.address + last.size
            if nxt >= HI:
                break
            addr = nxt + 4
            skips += 1
    with open(CACHE, 'wb') as f:
        pickle.dump(ins, f)
    print('instructions=%d skipped_words=%d -> %s' % (len(ins), skips, CACHE))
    return ins


def idx():
    if not os.path.exists(CACHE):
        return build()
    with open(CACHE, 'rb') as f:
        return pickle.load(f)


def pos(ins, a):
    lo, hi = 0, len(ins) - 1
    while lo <= hi:
        m = (lo + hi) // 2
        if ins[m][0] == a:
            return m
        if ins[m][0] < a:
            lo = m + 1
        else:
            hi = m - 1
    return None


def tgt(op):
    op = op.strip()
    if op.startswith('#'):
        try:
            return int(op[1:], 0)
        except ValueError:
            return None
    return None


def show(ins, a, n=40):
    p = pos(ins, a)
    if p is None:
        print('no insn at %#x' % a)
        return
    for ins_ in ins[p:p + n]:
        print('%08x  %-8s %-32s %s' % (ins_[0], ins_[2], ins_[3],
                                       ins_[4].hex()))


def thunks(ins):
    out = []
    for k in range(len(ins) - 3):
        a, s, m, o, b = ins[k]
        if m != 'adrp' or not o.startswith('x16'):
            continue
        m1, o1 = ins[k + 1][2], ins[k + 1][3]
        m2, o2 = ins[k + 2][2], ins[k + 2][3]
        m3 = ins[k + 3][2]
        if m1 == 'ldr' and o1.startswith('x17, [x16') and m3 == 'br':
            slot = None
            try:
                page = int(o.split('#')[1], 0)
                disp = int(o1.split('#')[1].rstrip(']'), 0)
                slot = page + disp
            except Exception:
                pass
            out.append((a, slot, o2))
    return out


def main():
    if len(sys.argv) < 2:
        print(__doc__)
        return
    cmd = sys.argv[1]
    if cmd == 'build':
        build()
        return
    ins = idx()
    if cmd == 'around':
        a = int(sys.argv[2], 16)
        n = int(sys.argv[3]) if len(sys.argv) > 3 else 40
        show(ins, a, n)
    elif cmd == 'back':
        a = int(sys.argv[2], 16)
        span = int(sys.argv[3], 16) if len(sys.argv) > 3 else 0x4000
        p = pos(ins, a)
        cands = []
        for ins_ in ins[max(0, p - span // 4):p]:
            if ins_[2] == 'stp' and 'x29, x30, [sp, #-0x' in ins_[3]:
                cands.append(ins_[0])
            elif ins_[2] == 'sub' and ins_[3].startswith('sp, sp, #0x') and \
                    len(ins_) == 5:
                pass
        print('prologue candidates before %#x: %s' % (a, [hex(c) for c in cands[-12:]]))
        # xrefs to each candidate
        calls = collections.defaultdict(list)
        for a2, s2, m2, o2, b2 in ins:
            if m2 in ('bl', 'b'):
                t = tgt(o2)
                if t is not None:
                    calls[t].append((a2, m2))
        for c in cands[-12:]:
            print('  %#x called from %s' % (c, [(hex(x), y) for x, y in calls.get(c, [])][:6]))
    elif cmd == 'xrefs':
        a = int(sys.argv[2], 16)
        hits = []
        for a2, s2, m2, o2, b2 in ins:
            if m2 in ('bl', 'b', 'cbz', 'cbnz', 'tbz', 'tbnz'):
                t = tgt(o2.split(',')[-1])
                if t == a:
                    hits.append((a2, m2, o2))
            if m2 in ('adrp', 'add') and ('#%#x' % a) in o2:
                hits.append((a2, m2, o2))
        for h in hits[:80]:
            print('%08x  %-6s %s' % h)
        print('total=%d' % len(hits))
    elif cmd == 'calls':
        a = int(sys.argv[2], 16)
        end = int(sys.argv[3], 16) if len(sys.argv) > 3 else a + 0x1000
        for a2, s2, m2, o2, b2 in ins:
            if a2 < a:
                continue
            if a2 >= end:
                break
            if m2 in ('bl', 'blr', 'b', 'br', 'ret'):
                print('%08x  %-5s %s' % (a2, m2, o2))
    elif cmd == 'thunks':
        th = thunks(ins)
        print('thunks=%d' % len(th))
        for a, slot, o2 in th:
            print('  thunk %#x -> slot %s' % (a, hex(slot) if slot else '?'))
    elif cmd == 'const':
        v = int(sys.argv[2], 16)
        pat = v.to_bytes(4, 'little')
        img = load_img()
        hits = []
        st = 0
        while True:
            k = img.find(pat, st)
            if k < 0 or k >= HI:
                break
            hits.append(k)
            st = k + 1
        print('little-endian u32 %#x hits: %d %s' % (v, len(hits), [hex(h) for h in hits[:30]]))
    elif cmd == 'svc':
        for a2, s2, m2, o2, b2 in ins:
            if m2 == 'svc':
                print('%08x  svc %s' % (a2, o2))
    else:
        print(__doc__)


if __name__ == '__main__':
    main()
