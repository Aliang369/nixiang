#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""xref_veneer.py — 绝区零 libil2cpp.so 函数边界 + veneer 解析 + 调用图 + 字段访存筛选

用法:
  python3 xref_veneer.py <so路径> graph <root_va_hex> [深度=3]
  python3 xref_veneer.py <so路径> func <va_hex>
  python3 xref_veneer.py <so路径> xref <offset_hex>      # 全 so 找 ldr/str 该偏移的函数

要点（2026-10-25 实测）:
  · 函数边界取自 PT_GNU_EH_FRAME 的 FDE 起始表，**datarel 基址 = eh_frame_hdr 的 sh_addr**
    （本 so = 0xbd82a8，表内 sdata4 + HDR）。
  · 代码段 file_off = VA - 0x4000。
  · 长调用是 `adrp x16; add x16,x16,#imm; br x16` veneer，必须解析。
"""
import struct, sys, bisect
from collections import deque

CODE_DELTA = 0x4000

class So:
    def __init__(self, path):
        self.raw = open(path, 'rb').read()
        self.starts = self._fde_starts()
    # ---- 基础读取（VA 口径）----
    def u32(self, va): return struct.unpack_from('<I', self.raw, va - CODE_DELTA)[0]
    def u8(self, va):  return self.raw[va - CODE_DELTA]
    # ---- FDE 起始表 ----
    def _fde_starts(self):
        raw = self.raw
        eh = None
        e_phoff = struct.unpack_from('<Q', raw, 0x20)[0]
        es = struct.unpack_from('<H', raw, 0x36)[0]
        n = struct.unpack_from('<H', raw, 0x38)[0]
        for i in range(n):
            o = e_phoff + i * es
            t = struct.unpack_from('<I', raw, o)[0]
            if t == 0x6474e550:
                eh = struct.unpack_from('<Q', raw, o + 8)[0]  # p_offset（本 so 与 p_vaddr 同值）
                break
        if eh is None:
            raise SystemExit('找不到 PT_GNU_EH_FRAME')
        cnt = struct.unpack_from('<I', raw, eh + 8)[0]
        base = eh + 12
        vals = [struct.unpack_from('<i', raw, base + i * 8)[0] for i in range(cnt)]
        vals.sort()
        return [eh + v for v in vals]
    def func_of(self, va):
        i = bisect.bisect_right(self.starts, va) - 1
        if i < 0: return None
        s = self.starts[i]
        e = self.starts[i + 1] if i + 1 < len(self.starts) else s + 0x1000
        return (s, e) if va < e else (s, e)
    @staticmethod
    def _sx(v, b):
        m = 1 << (b - 1)
        return (v & (m - 1)) - (v & m)
    def veneer(self, va):
        try:
            w0 = self.u32(va)
            if (w0 & 0x9F00001F) != 0x90000010: return None
            immlo = (w0 >> 29) & 3; immhi = (w0 >> 5) & 0x7FFFF
            page = (va & ~0xFFF) + self._sx((immhi << 2) | immlo, 21) * 0x1000
            w1 = self.u32(va + 4); w2 = self.u32(va + 8)
            if (w1 & 0xFFC003FF) != 0x91000210 or w2 != 0xD61F0200: return None
            return page + ((w1 >> 10) & 0xFFF)
        except Exception:
            return None
    def callees(self, s, e):
        out = set()
        for va in range(s, e, 4):
            w = self.u32(va)
            if (w & 0xFC000000) in (0x94000000, 0x14000000):
                t = va + self._sx(w & 0x3FFFFFF, 26) * 4
                v = self.veneer(t)
                out.add(v if v else t)
        return out
    def field_hits(self, s, e, off):
        r = []
        for va in range(s, e, 4):
            w = self.u32(va)
            if (w & 0xFFC00000) == 0xB9000000 and ((w >> 10) & 0xFFF) * 4 == off: r.append((va, 'str32'))
            elif (w & 0xFFC00000) == 0xB9400000 and ((w >> 10) & 0xFFF) * 4 == off: r.append((va, 'ldr32'))
            elif (w & 0xFFC00000) == 0xF9400000 and ((w >> 10) & 0xFFF) * 8 == off: r.append((va, 'ldr64'))
        return r

def main():
    if len(sys.argv) < 3:
        print(__doc__); return 2
    so = So(sys.argv[1]); cmd = sys.argv[2]
    if cmd == 'func':
        va = int(sys.argv[3], 16); s, e = so.func_of(va)
        print('func 0x%X [0x%X,0x%X) size=0x%X' % (s, s, e, e - s))
        for c in sorted(so.callees(s, e)):
            v = so.veneer(c)
            print('   callee 0x%X%s' % (c, ' (veneer->0x%X)' % v if v else ''))
    elif cmd == 'graph':
        root = int(sys.argv[3], 16); depth = int(sys.argv[4]) if len(sys.argv) > 4 else 3
        off = int(sys.argv[5], 16) if len(sys.argv) > 5 else 0xA0
        seen = {root: 0}; q = deque([root]); found = []
        while q:
            f = q.popleft(); d = seen[f]
            fe = so.func_of(f)
            if not fe: continue
            s, e = fe
            if e - s > 0x20000: continue
            h = so.field_hits(s, e, off)
            if h: found.append((d, f, s, e, h[:4]))
            if d < depth:
                for c in so.callees(s, e):
                    if c not in seen and len(seen) < 30000:
                        seen[c] = d + 1; q.append(c)
        print('遍历函数 %d 个，含 +0x%X 访存的函数 %d 个：' % (len(seen), off, len(found)))
        for d, f, s, e, h in sorted(found)[:40]:
            print('  d=%d func=0x%X [0x%X,0x%X) %s' % (d, f, s, e, [(hex(a), k) for a, k in h]))
    elif cmd == 'xref':
        off = int(sys.argv[3], 16)
        n = 0
        for i in range(len(so.starts) - 1):
            s, e = so.starts[i], so.starts[i + 1]
            h = so.field_hits(s, e, off)
            if h:
                n += 1
                if n <= 30: print('  func 0x%X %s' % (s, [(hex(a), k) for a, k in h[:3]]))
        print('共 %d 个函数访问 +0x%X' % (n, off))
    return 0

if __name__ == '__main__':
    sys.exit(main())
