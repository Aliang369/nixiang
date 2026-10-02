# -*- coding: utf-8 -*-
"""
纯标准库 mini DEX (035) 解析器 + 反汇编器
无第三方依赖，无 jadx/apktool/baksmali。

用法:
  python _dex_disasm.py <dex> --info
  python _dex_disasm.py <dex> --list [--filter SUBSTR]
  python _dex_disasm.py <dex> --class Lcom/android/support/Main;
  python _dex_disasm.py <dex> --method Lcom/android/support/Main;->GetFeatureList(Landroid/content/Context;)[Ljava/lang/String;
  python _dex_disasm.py <dex> --selftest
  python _dex_disasm.py <dex> --dump <outfile.txt>      # 全量反汇编
  python _dex_disasm.py <dex> --strings [--grep X]
  python _dex_disasm.py <dex> --findcall <methodname>   # 全 dex 搜 invoke 目标
"""
import sys, os, io, struct

# ---------------------------------------------------------------- 基础读取
def uleb128(buf, off):
    result = 0; shift = 0
    while True:
        b = buf[off]; off += 1
        result |= (b & 0x7f) << shift
        if b < 0x80: break
        shift += 7
        if shift > 35: break
    return result, off

def sleb128(buf, off):
    result = 0; shift = 0
    while True:
        b = buf[off]; off += 1
        result |= (b & 0x7f) << shift
        shift += 7
        if b < 0x80:
            if b & 0x40:
                result |= -(1 << shift)
            break
    return result, off

def read_mutf8(buf, off):
    _u16len, off = uleb128(buf, off)
    units = []
    end = len(buf)
    while off < end:
        b = buf[off]
        if b == 0:
            off += 1; break
        if b < 0x80:
            units.append(b); off += 1
        elif (b & 0xE0) == 0xC0:
            units.append(((b & 0x1F) << 6) | (buf[off+1] & 0x3F)); off += 2
        elif (b & 0xF0) == 0xE0:
            units.append(((b & 0x0F) << 12) | ((buf[off+1] & 0x3F) << 6) | (buf[off+2] & 0x3F)); off += 3
        else:
            units.append(b); off += 1
    raw = ''.join(chr(c) for c in units)
    try:
        return raw.encode('utf-16-le', 'surrogatepass').decode('utf-16-le'), off
    except Exception:
        return raw, off

# ---------------------------------------------------------------- opcode 表
# opcode -> (name, format)
OPS = {}
def _d(base, names, fmt):
    for i, n in enumerate(names):
        OPS[base + i] = (n, fmt)

LEN_BY_FMT = {
    '10x':1,'12x':1,'11n':1,'11x':1,'10t':1,
    '20t':2,'20bc':2,'22x':2,'21t':2,'21s':2,'21h':2,'21c':2,'23x':2,
    '22b':2,'22t':2,'22s':2,'22c':2,'22cs':2,
    '30t':3,'32x':3,'31i':3,'31t':3,'31c':3,'35c':3,'35ms':3,'35mi':3,
    '3rc':3,'3rms':3,'3rmi':3,
    '45cc':4,'4rcc':4,'51l':5,
}

_d(0x00, ['nop'], '10x')
_d(0x01, ['move'], '12x')
_d(0x02, ['move/from16'], '22x')
_d(0x03, ['move/16'], '32x')
_d(0x04, ['move-wide'], '12x')
_d(0x05, ['move-wide/from16'], '22x')
_d(0x06, ['move-wide/16'], '32x')
_d(0x07, ['move-object'], '12x')
_d(0x08, ['move-object/from16'], '22x')
_d(0x09, ['move-object/16'], '32x')
_d(0x0a, ['move-result'], '11x')
_d(0x0b, ['move-result-wide'], '11x')
_d(0x0c, ['move-result-object'], '11x')
_d(0x0d, ['move-exception'], '11x')
_d(0x0e, ['return-void'], '10x')
_d(0x0f, ['return'], '11x')
_d(0x10, ['return-wide'], '11x')
_d(0x11, ['return-object'], '11x')
_d(0x12, ['const/4'], '11n')
_d(0x13, ['const/16'], '21s')
_d(0x14, ['const'], '31i')
_d(0x15, ['const/high16'], '21h')
_d(0x16, ['const-wide/16'], '21s')
_d(0x17, ['const-wide/32'], '31i')
_d(0x18, ['const-wide'], '51l')
_d(0x19, ['const-wide/high16'], '21h')
_d(0x1a, ['const-string'], '21c')
_d(0x1b, ['const-string/jumbo'], '31c')
_d(0x1c, ['const-class'], '21c')
_d(0x1d, ['monitor-enter'], '11x')
_d(0x1e, ['monitor-exit'], '11x')
_d(0x1f, ['check-cast'], '21c')
_d(0x20, ['instance-of'], '22c')
_d(0x21, ['array-length'], '12x')
_d(0x22, ['new-instance'], '21c')
_d(0x23, ['new-array'], '22c')
_d(0x24, ['filled-new-array'], '35c')
_d(0x25, ['filled-new-array/range'], '3rc')
_d(0x26, ['fill-array-data'], '31t')
_d(0x27, ['throw'], '11x')
_d(0x28, ['goto'], '10t')
_d(0x29, ['goto/16'], '20t')
_d(0x2a, ['goto/32'], '30t')
_d(0x2b, ['packed-switch'], '31t')
_d(0x2c, ['sparse-switch'], '31t')
_d(0x2d, ['cmpl-float','cmpg-float','cmpl-double','cmpg-double','cmp-long'], '23x')
_d(0x32, ['if-eq','if-ne','if-lt','if-ge','if-gt','if-le'], '22t')
_d(0x38, ['if-eqz','if-nez','if-ltz','if-gez','if-gtz','if-lez'], '21t')
_d(0x44, ['aget','aget-wide','aget-object','aget-boolean','aget-byte','aget-char','aget-short'], '23x')
_d(0x4b, ['aput','aput-wide','aput-object','aput-boolean','aput-byte','aput-char','aput-short'], '23x')
_d(0x52, ['iget','iget-wide','iget-object','iget-boolean','iget-byte','iget-char','iget-short'], '22c')
_d(0x59, ['iput','iput-wide','iput-object','iput-boolean','iput-byte','iput-char','iput-short'], '22c')
_d(0x60, ['sget','sget-wide','sget-object','sget-boolean','sget-byte','sget-char','sget-short'], '21c')
_d(0x67, ['sput','sput-wide','sput-object','sput-boolean','sput-byte','sput-char','sput-short'], '21c')
_d(0x6e, ['invoke-virtual','invoke-super','invoke-direct','invoke-static','invoke-interface'], '35c')
_d(0x74, ['invoke-virtual/range','invoke-super/range','invoke-direct/range','invoke-static/range','invoke-interface/range'], '3rc')

_BIN = ['add-int','sub-int','mul-int','div-int','rem-int','and-int','or-int','xor-int',
        'shl-int','shr-int','ushr-int','add-long','sub-long','mul-long','div-long','rem-long',
        'and-long','or-long','xor-long','shl-long','shr-long','ushr-long',
        'add-float','sub-float','mul-float','div-float','rem-float',
        'add-double','sub-double','mul-double','div-double','rem-double']
_d(0x90, _BIN, '23x')
_d(0xb0, [n + '/2addr' for n in _BIN], '12x')
_d(0xd0, ['add-int/lit16','rsub-int','mul-int/lit16','div-int/lit16','rem-int/lit16',
          'and-int/lit16','or-int/lit16','xor-int/lit16'], '22s')
_d(0xd8, ['add-int/lit8','rsub-int/lit8','mul-int/lit8','div-int/lit8','rem-int/lit8',
          'and-int/lit8','or-int/lit8','xor-int/lit8','shl-int/lit8','shr-int/lit8','ushr-int/lit8'], '22b')
_d(0x7b, ['neg-int','not-int','neg-long','not-long','neg-float','neg-double',
          'int-to-long','int-to-float','int-to-double','long-to-int','long-to-float','long-to-double',
          'float-to-int','float-to-long','float-to-double','double-to-int','double-to-long','double-to-float',
          'int-to-byte','int-to-char','int-to-short'], '12x')
_d(0xfa, ['invoke-polymorphic'], '45cc')
_d(0xfb, ['invoke-polymorphic/range'], '4rcc')
_d(0xfc, ['invoke-custom'], '35c')
_d(0xfd, ['invoke-custom/range'], '3rc')
_d(0xfe, ['const-method-handle'], '21c')
_d(0xff, ['const-method-type'], '21c')

# ---------------------------------------------------------------- DEX 解析
class Dex(object):
    def __init__(self, blob, path=''):
        self.buf = blob
        self.path = path
        self.phys = len(blob)
        magic = blob[:8]
        if magic[:4] != b'dex\n':
            raise ValueError('not a dex: %r' % magic)
        self.version = magic[4:7].decode('ascii', 'replace')
        (self.checksum, self.sig, self.file_size, self.header_size,
         self.endian_tag, self.link_size, self.link_off, self.map_off,
         self.string_ids_size, self.string_ids_off,
         self.type_ids_size, self.type_ids_off,
         self.proto_ids_size, self.proto_ids_off,
         self.field_ids_size, self.field_ids_off,
         self.method_ids_size, self.method_ids_off,
         self.class_defs_size, self.class_defs_off,
         self.data_size, self.data_off) = struct.unpack_from('<I20s' + 'I'*20, blob, 8)
        self._ssize = self.string_ids_size
        self._tsize = self.type_ids_size
        self._str_cache = {}

    # --- ids ---
    def string(self, idx):
        if idx in self._str_cache: return self._str_cache[idx]
        if idx < 0 or idx >= self.string_ids_size:
            v = '<string#%d OUT_OF_RANGE>' % idx
        else:
            off = struct.unpack_from('<I', self.buf, self.string_ids_off + 4*idx)[0]
            v, _ = read_mutf8(self.buf, off)
        self._str_cache[idx] = v
        return v

    def type_desc(self, idx):
        if idx == 0xffffffff: return '<NO_INDEX>'
        if idx < 0 or idx >= self.type_ids_size:
            return '<type#%d OUT_OF_RANGE>' % idx
        si = struct.unpack_from('<I', self.buf, self.type_ids_off + 4*idx)[0]
        return self.string(si)

    def type_short(self, idx):
        d = self.type_desc(idx)
        return shorten_type(d)

    def proto(self, idx):
        """return (shorty, ret_type_idx, param_type_idx_list)"""
        if idx < 0 or idx >= self.proto_ids_size:
            return ('<proto#%d OOR>' % idx, 0xffffffff, [])
        so, rt, po = struct.unpack_from('<III', self.buf, self.proto_ids_off + 12*idx)
        params = []
        if po != 0:
            n = struct.unpack_from('<I', self.buf, po)[0]
            for i in range(n):
                params.append(struct.unpack_from('<H', self.buf, po + 4 + 2*i)[0])
        return (self.string(so), rt, params)

    def field(self, idx):
        if idx < 0 or idx >= self.field_ids_size:
            return ('<field#%d OOR>' % idx, '', '')
        c, t, n = struct.unpack_from('<HHI', self.buf, self.field_ids_off + 8*idx)
        return (self.type_desc(c), self.type_desc(t), self.string(n))

    def method(self, idx):
        if idx < 0 or idx >= self.method_ids_size:
            return ('<method#%d OOR>' % idx, '', '', '')
        c, p, n = struct.unpack_from('<HHI', self.buf, self.method_ids_off + 8*idx)
        shorty, rt, params = self.proto(p)
        return (self.type_desc(c), self.string(n), ''.join(self.type_short(x) for x in params), shorty)

    def method_name(self, idx):
        if idx < 0 or idx >= self.method_ids_size: return '<method#%d OOR>' % idx
        return self.string(struct.unpack_from('<I', self.buf, self.method_ids_off + 8*idx + 4)[0])

    def method_full(self, idx):
        c, n, ps, shorty = self.method(idx)
        shorty2, rt, params = self.proto(struct.unpack_from('<HHI', self.buf, self.method_ids_off + 8*idx)[1]) if 0 <= idx < self.method_ids_size else ('', 0xffffffff, [])
        return '%s->%s(%s)%s' % (c, n, ''.join(self.type_desc(x) for x in params), self.type_desc(rt))

    # --- class defs ---
    def class_defs(self):
        out = []
        for i in range(self.class_defs_size):
            base = self.class_defs_off + 32*i
            ci, acc, sci, si, ii, so, ao = struct.unpack_from('<IIIIIII', self.buf, base)
            out.append({'idx': i, 'class_idx': ci, 'access': acc, 'superclass_idx': sci,
                        'interfaces_off': ii, 'source_file_idx': si,
                        'annotations_off': ao, 'class_data_off': None, 'base': base})
        return out

    def class_data(self, cd):
        off = struct.unpack_from('<I', self.buf, cd['base'] + 24)[0]
        cd['class_data_off'] = off
        if off == 0:
            return {'static_fields': [], 'instance_fields': [], 'direct': [], 'virtual': []}
        sf, off = uleb128(self.buf, off)
        inf, off = uleb128(self.buf, off)
        dm, off = uleb128(self.buf, off)
        vm, off = uleb128(self.buf, off)
        res = {'static_fields': [], 'instance_fields': [], 'direct': [], 'virtual': []}
        for key, cnt in (('static_fields', sf), ('instance_fields', inf)):
            fidx = 0
            for _ in range(cnt):
                d, off = uleb128(self.buf, off)
                acc, off = uleb128(self.buf, off)
                fidx += d
                res[key].append((fidx, acc))
        for key, cnt in (('direct', dm), ('virtual', vm)):
            midx = 0
            for _ in range(cnt):
                d, off = uleb128(self.buf, off)
                acc, off = uleb128(self.buf, off)
                code_off, off = uleb128(self.buf, off)
                midx += d
                res[key].append((midx, acc, code_off))
        cd['data'] = res
        return res

    def find_class(self, descriptor):
        for cd in self.class_defs():
            if self.type_desc(cd['class_idx']) == descriptor:
                return cd
        return None

    def all_methods(self, cd):
        """return list of (method_idx, access, code_off, kind)"""
        d = cd.get('data') or self.class_data(cd)
        out = []
        for kind in ('direct', 'virtual'):
            for (mi, acc, co) in d[kind]:
                out.append((mi, acc, co, kind))
        return out

# ---------------------------------------------------------------- 反汇编核心
def shorten_type(d):
    """Lfoo/bar/Baz; -> LFoo.Baz;  (参数列表里简短显示)"""
    if d.startswith('L') and d.endswith(';'):
        body = d[1:-1]
        return 'L' + body.split('/')[-1] + ';'
    return d

class Disasm(object):
    def __init__(self, dex):
        self.dex = dex
        self.stats = {'insns': 0, 'unimpl': 0, 'fail': 0, 'methods': 0, 'methods_no_code': 0,
                      'payloads': 0, 'opcode_hist': {}}
        self.problems = []

    # --- 单条指令解码 ---
    def insn_at(self, insns, pc, base_byte):
        """返回 (text, length, ok) ; base_byte = code start byte offset（用于跳转显示）"""
        dex = self.dex
        u = insns[pc]
        op = u & 0xFF
        hi = (u >> 8) & 0xFF
        # payload 伪指令
        if op == 0:
            if hi == 0x00:
                return ('nop', 1, True)
            if hi in (0x01, 0x02):     # packed-switch payload / sparse-switch payload
                ident = 'packed-switch-payload' if hi == 1 else 'sparse-switch-payload'
                size = insns[pc+1]
                ln = 4 + size*2 if hi == 1 else 2 + size*4
                return ('%s size=%d' % (ident, size), ln, True)
            if hi == 0x03:             # fill-array-data payload
                elem_width = insns[pc+1]
                size = insns[pc+2] | (insns[pc+3] << 16)
                ln = (size*elem_width + 1)//2 + 4
                return ('array-data-payload elem_width=%d size=%d' % (elem_width, size), ln, True)
            return ('payload? 0x%04x' % u, 1, True)
        ent = OPS.get(op)
        if ent is None:
            return ('opcode=0x%02x (未实现)' % op, 1, False)
        name, fmt = ent
        L = LEN_BY_FMT[fmt]
        try:
            return (self._render(name, fmt, insns, pc, u), L, True)
        except Exception as e:
            return ('%s <DECODE-FAIL %s>' % (name, e), L, False)

    def _render(self, name, fmt, insns, pc, u):
        dex = self.dex
        A = (u >> 8) & 0xF          # 低半字节寄存器 A（12x/11n/22c 族）
        B = (u >> 12) & 0xF         # 高半字节寄存器 B
        AA = (u >> 8) & 0xFF        # 全字节寄存器 AA（11x/21*/22x/31*/51l/10t）
        if fmt == '10x':
            return name
        if fmt in ('12x', '11n', '11x'):
            if fmt == '11n':
                n = (u >> 12) & 0xF
                if n >= 8: n -= 16
                return '%s v%d, #%d' % (name, A, n)
            if fmt == '11x':
                return '%s v%d' % (name, AA)
            return '%s v%d, v%d' % (name, A, B)
        if fmt == '10t':
            off = (u >> 8) & 0xFF
            if off >= 0x80: off -= 0x100
            return '%s %s' % (name, tgt(pc, off))
        if fmt == '20t':
            off = insns[pc+1]
            if off >= 0x8000: off -= 0x10000
            return '%s %s' % (name, tgt(pc, off))
        if fmt == '30t':
            off = insns[pc+1] | (insns[pc+2] << 16)
            if off >= 0x80000000: off -= 0x100000000
            return '%s %s' % (name, tgt(pc, off))
        if fmt == '22x':
            return '%s v%d, v%d' % (name, AA, insns[pc+1])
        if fmt == '32x':
            return '%s v%d, v%d' % (name, insns[pc+1], insns[pc+2])
        if fmt == '21s':
            s = insns[pc+1]
            if s >= 0x8000: s -= 0x10000
            return '%s v%d, #%d' % (name, AA, s)
        if fmt == '21h':
            s = insns[pc+1]
            if s >= 0x8000: s -= 0x10000
            return '%s v%d, #%d (0x%X)' % (name, AA, s << 16, (s << 16) & 0xFFFFFFFF)
        if fmt == '31i':
            v = insns[pc+1] | (insns[pc+2] << 16)
            if v >= 0x80000000: v -= 0x100000000
            return '%s v%d, #%d (0x%X)' % (name, AA, v, v & 0xFFFFFFFF)
        if fmt == '51l':
            v = insns[pc+1] | (insns[pc+2] << 16) | (insns[pc+3] << 32) | (insns[pc+4] << 48)
            sv = v - (1 << 64) if v >= (1 << 63) else v
            return '%s v%d, #%d (0x%X)' % (name, AA, sv, v)
        if fmt == '21c':
            idx = insns[pc+1]
            return '%s v%d, %s' % (name, AA, self.refidx(name, idx))
        if fmt == '31c':
            idx = insns[pc+1] | (insns[pc+2] << 16)
            return '%s v%d, %s' % (name, AA, self.refidx(name, idx))
        if fmt == '22c':
            return '%s v%d, v%d, %s' % (name, A, B, self.refidx(name, insns[pc+1]))
        if fmt == '23x':
            return '%s v%d, v%d, v%d' % (name, A, B, insns[pc+1])
        if fmt == '22b':
            n = insns[pc+1]
            return '%s v%d, v%d, #%d' % (name, A, B, lo2s((n >> 8) & 0xFF))
        if fmt == '22s':
            s = insns[pc+1] & 0xFF
            if s >= 0x80: s -= 0x100
            return '%s v%d, v%d, #%d' % (name, A, B, s)
        if fmt == '22t':
            off = insns[pc+1]
            if off >= 0x8000: off -= 0x10000
            return '%s v%d, v%d, %s' % (name, A, B, tgt(pc, off))
        if fmt == '21t':
            off = insns[pc+1]
            if off >= 0x8000: off -= 0x10000
            return '%s v%d, %s' % (name, AA, tgt(pc, off))
        if fmt == '31t':
            off = insns[pc+1] | (insns[pc+2] << 16)
            if off >= 0x80000000: off -= 0x100000000
            t = tgt(pc, off)
            extra = self.payload_info(insns, pc, off)
            return '%s v%d, %s%s' % (name, AA, t, extra)
        if fmt in ('35c', '35ms', '35mi'):
            cnt = (u >> 12) & 0xF         # A = arg register count (bits 12-15)
            g = (u >> 8) & 0xF            # G = 5th register (bits 8-11)
            w2 = insns[pc+2]
            cregs = [w2 & 0xF, (w2 >> 4) & 0xF, (w2 >> 8) & 0xF, (w2 >> 12) & 0xF, g]
            return '%s {v%s}, %s' % (name, ', v'.join(str(x) for x in cregs[:cnt]),
                                     self.mref(insns[pc+1]))
        if fmt in ('3rc', '3rms', '3rmi'):
            cnt = (u >> 8) & 0xFF         # AA = register count
            first = insns[pc+2]           # CCCC = first register
            rng = 'v%d..v%d' % (first, first + cnt - 1) if cnt else '-'
            return '%s {%s}, %s' % (name, rng, self.mref(insns[pc+1]))
        if fmt == '45cc':
            cnt = (u >> 12) & 0xF
            g = (u >> 8) & 0xF
            w2 = insns[pc+2]
            cregs = [w2 & 0xF, (w2 >> 4) & 0xF, (w2 >> 8) & 0xF, (w2 >> 12) & 0xF, g]
            return '%s {v%s}, %s, %s' % (name, ', v'.join(str(x) for x in cregs[:cnt]),
                                         self.mref(insns[pc+1]), self.mref(insns[pc+3]))
        if fmt == '4rcc':
            cnt = (u >> 8) & 0xFF
            first = insns[pc+2]
            rng = 'v%d..v%d' % (first, first + cnt - 1) if cnt else '-'
            return '%s {%s}, %s, %s' % (name, rng, self.mref(insns[pc+1]), self.mref(insns[pc+3]))
        return '%s <fmt %s 未实现>' % (name, fmt)

    def payload_info(self, insns, pc, rel_off):
        p = pc + rel_off
        if p < 0 or p >= len(insns): return ''
        w = insns[p]
        if (w & 0xFF) != 0 or ((w >> 8) & 0xFF) not in (1, 2): return ''
        size = insns[p+1]
        if (w >> 8) == 1:
            first = insns[p+2] | (insns[p+3] << 16)
            if first >= 0x80000000: first -= 0x100000000
            return '  ; packed size=%d first_key=%d' % (size, first)
        return '  ; sparse size=%d' % size

    def mref(self, idx):
        dex = self.dex
        if idx >= dex.method_ids_size:
            return '<method#%d OOR>' % idx
        c, n, ps, shorty = dex.method(idx)
        try:
            full = dex.method_full(idx)
        except Exception:
            full = '%s->%s' % (c, n)
        return full

    def refidx(self, name, idx):
        dex = self.dex
        if name == 'const-string' or name == 'const-string/jumbo':
            return '"%s"   ; string@%d' % (dex.string(idx), idx)
        if name in ('const-class', 'check-cast', 'new-instance', 'instance-of',
                    'new-array', 'filled-new-array', 'filled-new-array/range'):
            return '%s   ; type@%d' % (dex.type_desc(idx), idx)
        if name.startswith('sget') or name.startswith('sput'):
            c, t, n = dex.field(idx)
            return '%s->%s:%s   ; field@%d' % (c, n, t, idx)
        if name in ('const-method-handle', 'const-method-type'):
            return '<index %d>' % idx
        c, t, n = dex.field(idx)
        return '%s->%s:%s   ; field@%d' % (c, n, t, idx)

def lo2s(b):
    return b - 0x100 if b >= 0x80 else b

def tgt(pc, off):
    return '@%+d -> insn[%d]' % (off, pc + off)

# ---------------------------------------------------------------- 方法反汇编
def dump_method(dex, dis, cd, mi, acc, code_off, kind, out, indent='  '):
    mfull = dex.method_full(mi)
    c, n, ps, shorty = dex.method(mi)
    if code_off == 0:
        out.append('%s%s [%s access=0x%X]  NO-CODE (abstract/native)' % (indent, mfull, kind, acc))
        dis.stats['methods_no_code'] += 1
        return []
    dis.stats['methods'] += 1
    buf = dex.buf
    regs, ins, outs, tries, dbg, n_insns = struct.unpack_from('<HHHHII', buf, code_off)
    code_start = code_off + 16
    insns = list(struct.unpack_from('<%dH' % n_insns, buf, code_start))
    out.append('%s%s [%s access=0x%X] code_off=0x%X regs=%d ins=%d outs=%d insns=%d' %
               (indent, mfull, kind, acc, code_off, regs, ins, outs, n_insns))
    pc = 0
    lines = []
    while pc < n_insns:
        text, L, ok = dis.insn_at(insns, pc, code_start)
        b = code_start + pc*2
        raw = ' '.join('%04x' % insns[pc+i] for i in range(min(L, n_insns-pc)))
        lines.append('%s  %04x: %-22s %s' % (indent, pc, raw, text))
        dis.stats['insns'] += 1
        dis.stats['opcode_hist'][(insns[pc] & 0xFF)] = dis.stats['opcode_hist'].get((insns[pc] & 0xFF), 0) + 1
        if (insns[pc] & 0xFF) == 0 and ((insns[pc] >> 8) & 0xFF) in (1, 2, 3):
            dis.stats['payloads'] += 1
        if not ok:
            if 'opcode=0x' in text:
                dis.stats['unimpl'] += 1
            else:
                dis.stats['fail'] += 1
            dis.problems.append((mfull, pc, text))
        if L <= 0: L = 1
        pc += L
    if pc != n_insns:
        dis.stats['misalign'] = dis.stats.get('misalign', 0) + 1
        dis.problems.append((mfull, pc, 'MISALIGN: 解码结束 pc=%d != insns_size=%d' % (pc, n_insns)))
        lines.append('%s  !! MISALIGN pc=%d insns_size=%d' % (indent, pc, n_insns))
    lines.append('%s  --- 本方法共 %d 条指令 ---' % (indent, len(lines)))
    out.extend(lines)
    return lines

# ---------------------------------------------------------------- 主程序
def main():
    try:
        sys.stdout.reconfigure(encoding='utf-8', errors='replace')
        sys.stderr.reconfigure(encoding='utf-8', errors='replace')
    except Exception:
        pass
    if len(sys.argv) < 2:
        print(__doc__); return
    path = sys.argv[1]
    args = sys.argv[2:]
    blob = open(path, 'rb').read()
    dex = Dex(blob, path)
    dis = Disasm(dex)

    if '--info' in args or not args:
        print('file=%s size=%d version=%s header=0x%X endian=0x%X' %
              (path, dex.file_size, dex.version, dex.header_size, dex.endian_tag))
        print('strings=%d types=%d protos=%d fields=%d methods=%d classes=%d' %
              (dex.string_ids_size, dex.type_ids_size, dex.proto_ids_size,
               dex.field_ids_size, dex.method_ids_size, dex.class_defs_size))
        print('物理大小=%d 头部file_size=%d %s' % (dex.phys, dex.file_size,
              '一致' if dex.phys == dex.file_size else '不一致(可能被追加)'))
        return

    def opt(flag, default=None):
        if flag in args:
            i = args.index(flag)
            return args[i+1] if i+1 < len(args) else default
        return default

    if '--strings' in args:
        g = opt('--grep')
        for i in range(dex.string_ids_size):
            s = dex.string(i)
            if g is None or g.lower() in s.lower():
                print('%6d\t%s' % (i, s))
        return

    if '--list' in args:
        f = opt('--filter')
        for cd in dex.class_defs():
            d = dex.type_desc(cd['class_idx'])
            if f and f not in d: continue
            print('%s  (super=%s access=0x%X)' % (d, dex.type_desc(cd['superclass_idx']), cd['access']))
            for (mi, acc, co, kind) in dex.all_methods(cd):
                print('    [%s] 0x%06X %s' % (kind, co, dex.method_full(mi)))
        return

    if '--findcall' in args:
        needle = opt('--findcall')
        hits = 0
        for cd in dex.class_defs():
            cname = dex.type_desc(cd['class_idx'])
            for (mi, acc, co, kind) in dex.all_methods(cd):
                if co == 0: continue
                regs, ins, outs, tries, dbg, n_insns = struct.unpack_from('<HHHHII', blob, co)
                insns = list(struct.unpack_from('<%dH' % n_insns, blob, co+16))
                pc = 0
                while pc < n_insns:
                    text, L, ok = dis.insn_at(insns, pc, co+16)
                    if needle in text:
                        print('%s :: %s\n    insn[%d] %s' % (cname, dex.method_full(mi), pc, text))
                        hits += 1
                    if L <= 0: L = 1
                    pc += L
        print('--findcall done, hits=%d' % hits)
        return

    if '--methodname' in args:
        nm = opt('--methodname')
        for cd in dex.class_defs():
            for (mi, acc, co, kind) in dex.all_methods(cd):
                c, n, ps, shorty = dex.method(mi)
                if n == nm:
                    out = []
                    print('### %s' % dex.method_full(mi))
                    dump_method(dex, dis, cd, mi, acc, co, kind, out)
                    print('\n'.join(out))
        return

    if '--strref' in args:
        idx = int(opt('--strref'))
        need = '"%s"' % dex.string(idx)
        for cd in dex.class_defs():
            for (mi, acc, co, kind) in dex.all_methods(cd):
                if co == 0: continue
                regs, ins, outs, tries, dbg, n_insns = struct.unpack_from('<HHHHII', blob, co)
                insns = list(struct.unpack_from('<%dH' % n_insns, blob, co+16))
                pc = 0
                while pc < n_insns:
                    text, L, ok = dis.insn_at(insns, pc, co+16)
                    if need in text:
                        print('%s :: %s\n    insn[%d] %s' % (dex.type_desc(cd['class_idx']), dex.method_full(mi), pc, text))
                    if L <= 0: L = 1
                    pc += L
        return

    if '--nonnum' in args:
        seen = set()
        mnames = {}
        for i in range(dex.method_ids_size):
            c, n, ps, shorty = dex.method(i)
            mnames.setdefault(n, []).append((c, i))
        for n in sorted(mnames, key=lambda x: (not x.isdigit(), x)):
            if n.isdigit(): continue
            print('METHOD-NAME  %-40s  count=%d  e.g. %s' % (n, len(mnames[n]), mnames[n][0][0]))
        fnames = {}
        for i in range(dex.field_ids_size):
            c, t, n = dex.field(i)
            fnames.setdefault(n, []).append((c, i))
        for n in sorted(fnames, key=lambda x: (not x.isdigit(), x)):
            if n.isdigit(): continue
            print('FIELD-NAME   %-40s  count=%d  e.g. %s' % (n, len(fnames[n]), fnames[n][0][0]))
        # 非数字类名
        for i in range(dex.type_ids_size):
            d = dex.type_desc(i)
            last = d.rsplit('/', 1)[-1]
            if any(ch.isdigit() for ch in last[:1]):
                pass
        return

    if '--class' in args:
        d = opt('--class')
        cd = dex.find_class(d)
        if cd is None:
            print('class not found: %s' % d); return
        print('%s  (super=%s access=0x%X class_data_off=0x%X)' %
              (d, dex.type_desc(cd['superclass_idx']), cd['access'],
               struct.unpack_from('<I', blob, cd['base']+24)[0]))
        for (mi, acc, co, kind) in dex.all_methods(cd):
            print('    [%s] access=0x%04X code_off=0x%06X  %s' % (kind, acc, co, dex.method_full(mi)))
        return

    if '--method' in args:
        sig = opt('--method')
        found = False
        for cd in dex.class_defs():
            for (mi, acc, co, kind) in dex.all_methods(cd):
                fsig = dex.method_full(mi)
                if fsig == sig or (sig in fsig and sig.startswith(dex.type_desc(cd['class_idx']))):
                    found = True
                    out = []
                    dump_method(dex, dis, cd, mi, acc, co, kind, out)
                    print('\n'.join(out))
                    print('')
        if not found:
            print('method not found: %s' % sig)
        return

    if '--selftest' in args:
        out = []
        for cd in dex.class_defs():
            cn = dex.type_desc(cd['class_idx'])
            data = dex.class_data(cd)
            for (mi, acc, co, kind) in dex.all_methods(cd):
                dump_method(dex, dis, cd, mi, acc, co, kind, out)
        s = dis.stats
        print('=== SELFTEST %s ===' % os.path.basename(path))
        print('classes        = %d' % dex.class_defs_size)
        print('methods w/code = %d' % s['methods'])
        print('methods no-code= %d' % s['methods_no_code'])
        print('code units     = %d' % s['insns'])
        print('payload 伪指令 = %d' % s['payloads'])
        print('未实现 opcode  = %d' % s['unimpl'])
        print('解码异常       = %d' % s['fail'])
        print('指令边界错位   = %d  (0 = 长度表完全正确，所有方法都精确走到 insns_size)' % s.get('misalign', 0))
        # opcode 覆盖分布
        miss = sorted(set(range(0x100)) - set(OPS.keys()))
        print('OPS 表未覆盖的 opcode 字节: %s' % ', '.join('0x%02x' % m for m in miss))
        used_ops = sorted(s['opcode_hist'].items())
        print('实际出现的 opcode 数 = %d' % len(used_ops))
        hist = {OPS.get(o, ('?',))[0] if o in OPS else 'opcode=0x%02x(未实现)' % o: n
                for o, n in used_ops}
        print('--- opcode 直方图（Top 40）---')
        for k, v in sorted(hist.items(), key=lambda x: -x[1])[:40]:
            print('  %-28s %d' % (k, v))
        if dis.problems:
            print('--- 前 20 条问题指令 ---')
            for (m, pc, t) in dis.problems[:20]:
                print('  %s insn[%d] %s' % (m, pc, t))
        else:
            print('--- 无问题指令 ---')
        return

    if '--dump' in args:
        outp = opt('--dump')
        out = []
        for cd in dex.class_defs():
            cn = dex.type_desc(cd['class_idx'])
            out.append('')
            out.append('#' * 70)
            out.append('# CLASS %s  (super=%s)' % (cn, dex.type_desc(cd['superclass_idx'])))
            out.append('#' * 70)
            data = dex.class_data(cd)
            for (mi, acc, co, kind) in dex.all_methods(cd):
                out.append('')
                dump_method(dex, dis, cd, mi, acc, co, kind, out)
        s = dis.stats
        tail = ['', '=== DUMP STATS ===',
                'classes=%d methods=%d no_code=%d insns=%d unimpl=%d fail=%d' %
                (dex.class_defs_size, s['methods'], s['methods_no_code'], s['insns'],
                 s['unimpl'], s['fail'])]
        io.open(outp, 'w', encoding='utf-8', errors='replace').write('\n'.join(out + tail))
        print('dumped -> %s  (%d lines, insns=%d unimpl=%d fail=%d)' %
              (outp, len(out), s['insns'], s['unimpl'], s['fail']))
        return

    print('unknown args: %s' % args)
    print(__doc__)

if __name__ == '__main__':
    main()
