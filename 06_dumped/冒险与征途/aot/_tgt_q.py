# -*- coding: utf-8 -*-
"""[T2] Metadata + IL query tool for Assembly-CSharp.dll (pure stdlib).

Usage:  _tgt_q.py <cmd> [args...]
  classof <line>                 show class header owning a line in hotupdate_methods.txt
  findtype <regex>               list TypeDef full names matching regex
  findfield <regex>              list Field rows (type, name) matching name regex
  fields <TypeFullNameRegex>     list fields of matching types w/ decoded types
  methods <TypeFullNameRegex>    list methods of matching types
  props <TypeFullNameRegex>      list properties of matching types
  sig <TypeNameRegex> <MethodNameRegex>   full signature + token + RVA
  il <token>                     dump IL body for MethodDef token (0x0600XXXX)
  iluse <TypeRegex> <MethRegex>  dump IL + resolved field/method/string refs
  strrefs <substr>               find #US strings containing substr + their offsets
  callers <TypeRegex> <MethRegex> find methods whose IL references that member
"""
import sys, os, re, struct, collections
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _mdread import Meta, TABLES, CODED

OUT = r'E:\nixiang\06_dumped\冒险与征途\aot'
m = Meta(os.path.join(OUT, 'Assembly-CSharp.dll'))

# ---------- table helpers ----------
def td_fullname_list():
    return [m.td_fullname(i + 1) for i in range(len(m.tbl['TypeDef']))]

TDN = td_fullname_list()

def type_token_of_row(row0):        # 0-based row index -> token
    return 0x02000000 | (row0 + 1)

def type_of_token(tok):
    return TDN[(tok & 0xFFFFFF) - 1]

def field_row(rid):
    r = m.tbl['Field'][rid - 1]
    owner = field_owner_type(rid)
    return owner, m.s(r[1]), r[2]

# ---- precomputed owner index (rid -> owning TypeDef 1-based rid) ----
_TD = m.tbl['TypeDef']
_NTD = len(_TD)
_FIDX = [0] * (len(m.tbl['Field']) + 2)
_MIDX = [0] * (len(m.tbl['MethodDef']) + 2)
for _i, _r in enumerate(_TD):
    _fl = _r[4]; _ml = _r[5]
    _nf = _TD[_i + 1][4] if _i + 1 < _NTD else len(m.tbl['Field']) + 1
    _nm_ = _TD[_i + 1][5] if _i + 1 < _NTD else len(m.tbl['MethodDef']) + 1
    for _j in range(_fl, _nf):
        _FIDX[_j] = _i + 1
    for _j in range(_ml, _nm_):
        _MIDX[_j] = _i + 1


def field_owner_type(field_rid):
    """find TypeDef full name owning this Field rid"""
    o = _FIDX[field_rid]
    return TDN[o - 1] if o else '<unknown>'

def method_owner_type(meth_rid):
    return _MIDX[meth_rid] or None

def method_sig_row(rid):
    r = m.tbl['MethodDef'][rid - 1]
    blob, _ = m.blob(r[4])
    ret, params, gen = m.sig_blob(blob)
    return m.s(r[3]), ret, params, r[0]

def fmt_method(rid, with_token=True):
    o = method_owner_type(rid)
    owner = m.td_fullname(o) if o else '?'
    nm, ret, params, rva = method_sig_row(rid)
    t = ' 0x%08X' % (0x06000000 | rid) if with_token else ''
    return '%s :: %s(%s) -> %s%s' % (owner, nm, ', '.join(params), ret, t)

def bsearch_type(tok_rid):
    return m.td_fullname(tok_rid)

# ---------- signature type resolution for Field ----------
def field_type_str(blobidx):
    """FieldSig = FIELD(0x06) [custommods] Type.  NOTE: prior tool decoded at offset 0
    (the 0x06 marker) and thus reported every field as `short` -- that was a bug.
    Correct: skip the 0x06 marker, then parse the type."""
    blob, _ = m.blob(blobidx)
    if not blob:
        return '<empty>'
    saved = m.data
    m.data = blob
    try:
        p = 0
        assert blob[0] == 0x06, 'not a FieldSig: %02X' % blob[0]
        p = 1
        # skip custom modifiers
        while blob[p] in (0x1F, 0x20):
            _, p = m.cint(p + 1)
        t, _ = m.parse_type(p)
    except Exception as e:
        t = '<err %s>' % e
    finally:
        m.data = saved
    return t

def prop_type_str(blobidx):
    """PropertySig = PROPERTY(0x08) | HASTHIS(0x20) [|EXPLICITTHIS] count RetType Params..."""
    blob, _ = m.blob(blobidx)
    saved = m.data
    m.data = blob
    try:
        p = 1                       # skip 0x08 PROPERTY marker
        if blob[p] & 0x20:
            p += 1
        cnt, p = m.cint(p)
        t, _ = m.parse_type(p)
    except Exception as e:
        t = '<err %s>' % e
    finally:
        m.data = saved
    return t

# ---- reverse index: Property rid -> (getter tok, setter tok) ----
_PSEM = {}
for _sem, _mid, _assoc in m.tbl['MethodSemantics']:
    if (_assoc & 1) == 1:                     # tag 1 = Property
        _pr = _assoc >> 1
        _e = _PSEM.setdefault(_pr, [None, None, None])
        if _sem == 0x02:
            _e[0] = 0x06000000 | _mid
        elif _sem == 0x01:
            _e[1] = 0x06000000 | _mid
        elif _sem == 0x04:                    # Other (e.g. add/remove)
            pass

# ---- reverse index: PropertyMap row -> owning TypeDef 0-based idx ----
_PMAP_BY_TYPE = {}
for _j, _row in enumerate(m.tbl['PropertyMap']):
    _PMAP_BY_TYPE[_row[0] - 1] = _j
_NPROP = len(m.tbl['Property'])
_PMAP = m.tbl['PropertyMap']


def prop_getset(rid):
    """(name, type, (getter_token, setter_token)) for Property rid"""
    r = m.tbl['Property'][rid - 1]
    nm = m.s(r[1])
    ty = prop_type_str(r[2])
    g, s, _ = _PSEM.get(rid, (None, None, None))
    return nm, ty, g, s

# ---------- IL decode ----------
SIMPLE_OPS = {0x00:'nop',0x01:'break',0x02:'ldarg.0',0x03:'ldarg.1',0x04:'ldarg.2',0x05:'ldarg.3',
 0x06:'ldloc.0',0x07:'ldloc.1',0x08:'ldloc.2',0x09:'ldloc.3',0x0A:'stloc.0',0x0B:'stloc.1',0x0C:'stloc.2',0x0D:'stloc.3',
 0x0E:'ldarg.s',0x0F:'ldarga.s',0x10:'starg.s',0x11:'ldloc.s',0x12:'ldloca.s',0x13:'stloc.s',0x14:'ldnull',
 0x15:'ldc.i4.m1',0x16:'ldc.i4.0',0x17:'ldc.i4.1',0x18:'ldc.i4.2',0x19:'ldc.i4.3',0x1A:'ldc.i4.4',0x1B:'ldc.i4.5',
 0x1C:'ldc.i4.6',0x1D:'ldc.i4.7',0x1E:'ldc.i4.8',0x1F:'ldc.i4.s',0x20:'ldc.i4',0x21:'ldc.i8',0x22:'ldc.r4',0x23:'ldc.r8',
 0x25:'dup',0x26:'pop',0x27:'jmp',0x28:'call',0x29:'calli',0x2A:'ret',0x2B:'br.s',0x2C:'brfalse.s',0x2D:'brtrue.s',
 0x2E:'beq.s',0x2F:'bge.s',0x30:'bgt.s',0x31:'ble.s',0x32:'blt.s',0x33:'bne.un.s',0x34:'bge.un.s',0x35:'bgt.un.s',
 0x36:'ble.un.s',0x37:'blt.un.s',0x38:'br',0x39:'brfalse',0x3A:'brtrue',0x3B:'beq',0x3C:'bge',0x3D:'bgt',0x3E:'ble',
 0x3F:'blt',0x40:'bne.un',0x41:'bge.un',0x42:'bgt.un',0x43:'ble.un',0x44:'blt.un',0x45:'switch',0x46:'ldind.i1',
 0x47:'ldind.u1',0x48:'ldind.i2',0x49:'ldind.u2',0x4A:'ldind.i4',0x4B:'ldind.u4',0x4C:'ldind.i8',0x4D:'ldind.i',
 0x4E:'ldind.r4',0x4F:'ldind.r8',0x50:'ldind.ref',0x51:'stind.ref',0x52:'stind.i1',0x53:'stind.i2',0x54:'stind.i4',
 0x55:'stind.i8',0x56:'stind.r4',0x57:'stind.r8',0x58:'add',0x59:'sub',0x5A:'mul',0x5B:'div',0x5C:'div.un',0x5D:'rem',
 0x5E:'rem.un',0x5F:'and',0x60:'or',0x61:'xor',0x62:'shl',0x63:'shr',0x64:'shr.un',0x65:'neg',0x66:'not',
 0x67:'conv.i1',0x68:'conv.i2',0x69:'conv.i4',0x6A:'conv.i8',0x6B:'conv.r4',0x6C:'conv.r8',0x6D:'conv.u4',
 0x6E:'conv.u8',0x6F:'callvirt',0x70:'cpobj',0x71:'ldobj',0x72:'ldstr',0x73:'newobj',0x74:'castclass',
 0x75:'isinst',0x76:'conv.r.un',0x79:'unbox',0x7A:'throw',0x7B:'ldfld',0x7C:'ldflda',0x7D:'stfld',0x7E:'ldsfld',
 0x7F:'ldsflda',0x80:'stsfld',0x81:'stobj',0x82:'conv.ovf.i1.un',0x8C:'box',0x8D:'newarr',0x8E:'ldlen',
 0x8F:'ldelema',0x90:'ldelem.i1',0x91:'ldelem.u1',0x92:'ldelem.i2',0x93:'ldelem.u2',0x94:'ldelem.i4',
 0x95:'ldelem.u4',0x96:'ldelem.i8',0x97:'ldelem.i',0x98:'ldelem.r4',0x99:'ldelem.r8',0x9A:'ldelem.ref',
 0x9B:'stelem.i',0x9C:'stelem.i1',0x9D:'stelem.i2',0x9E:'stelem.i4',0x9F:'stelem.i8',0xA0:'stelem.r4',
 0xA1:'stelem.r8',0xA2:'stelem.ref',0xA3:'ldelem',0xA4:'stelem',0xA5:'unbox.any',0xB3:'conv.ovf.i1',
 0xB4:'conv.ovf.u1',0xB5:'conv.ovf.i2',0xB6:'conv.ovf.u2',0xB7:'conv.ovf.i4',0xB8:'conv.ovf.u4',
 0xB9:'conv.ovf.i8',0xBA:'conv.ovf.u8',0xC2:'refanyval',0xC3:'ckfinite',0xC6:'mkrefany',0xD0:'ldtoken',
 0xD1:'conv.u2',0xD2:'conv.u1',0xD3:'conv.i',0xD4:'conv.ovf.i',0xD5:'conv.ovf.u',0xD6:'add.ovf',
 0xD7:'add.ovf.un',0xD8:'mul.ovf',0xD9:'mul.ovf.un',0xDA:'sub.ovf',0xDB:'sub.ovf.un',0xDC:'endfinally',
 0xDD:'leave',0xDE:'leave.s',0xDF:'stind.i',0xE0:'conv.u'}

OP2 = {0x00:'arglist',0x01:'ceq',0x02:'cgt',0x03:'cgt.un',0x04:'clt',0x05:'clt.un',0x06:'ldftn',0x07:'ldvirtftn',
 0x09:'ldarg',0x0A:'ldarga',0x0B:'starg',0x0C:'ldloc',0x0D:'ldloca',0x0E:'stloc',0x0F:'localloc',0x11:'endfilter',
 0x12:'unaligned.',0x13:'volatile.',0x14:'tail.',0x15:'initobj',0x16:'constrained.',0x17:'cpblk',0x18:'initblk',
 0x19:'no.',0x1A:'rethrow',0x1C:'sizeof',0x1D:'refanytype',0x1E:'readonly.'}

def read_us(data, p):
    v = struct.unpack_from('<H', data, p)[0]; return v, p + 2
def read_i4(data, p):
    v = struct.unpack_from('<i', data, p)[0]; return v, p + 4
def read_u4(data, p):
    v = struct.unpack_from('<I', data, p)[0]; return v, p + 4
def read_i8(data, p):
    v = struct.unpack_from('<q', data, p)[0]; return v, p + 8
def read_r4(data, p):
    v = struct.unpack_from('<f', data, p)[0]; return v, p + 4
def read_r8(data, p):
    v = struct.unpack_from('<d', data, p)[0]; return v, p + 8

def us_string(idx):
    b, sz = m.streams['#US']
    p = b + idx
    n, p = m.cint(p, m._file)
    raw = m._file[p:p+n]
    if raw and raw[-1] in (0, 1):
        raw = raw[:-1]
    return raw.decode('utf-16-le', 'replace') if len(raw) % 2 == 0 else raw.decode('utf-8', 'replace')

def method_body(rid):
    r = m.tbl['MethodDef'][rid - 1]
    rva = r[0]
    if rva == 0:
        return None, 'no body (RVA=0)'
    off = m.r2o(rva)
    d = m._file
    b0 = d[off]
    if b0 & 3 == 2:      # tiny
        size = b0 >> 2
        code = d[off + 1: off + 1 + size]
    elif b0 & 3 == 3:    # fat
        flags_size = struct.unpack_from('<H', d, off)[0]
        hdr = (flags_size >> 12) * 4
        csize = struct.unpack_from('<I', d, off + 4)[0]
        code = d[off + hdr: off + hdr + csize]
    else:
        return None, 'bad header 0x%02X' % b0
    return code, 'ok'

def resolve_token(tok):
    t = tok >> 24
    rid = tok & 0xFFFFFF
    try:
        if t == 0x06:
            return 'method ' + fmt_method(rid)
        if t == 0x04:
            owner, nm, sig = field_row(rid)
            return 'field %s::%s : %s' % (owner, nm, field_type_str(sig))
        if t == 0x0A:
            r = m.tbl['MemberRef'][rid - 1]
            cls = memberref_parent(r[0])
            blob, _ = m.blob(r[2])
            return 'memberref %s::%s' % (cls, m.s(r[1]))
        if t == 0x02:
            return 'type ' + bsearch_type(rid)
        if t == 0x01:
            return 'typespec ' + m.typespec_str(rid)
        if t == 0x1B:
            r = m.tbl['MethodSpec'][rid - 1]
            return 'methodspec ' + resolve_token_field_method(r[0])
        if t == 0x70:
            return 'string "%s"' % us_string(rid)
        if t == 0x11:
            r = m.tbl['StandAloneSig'][rid - 1]
            blob, _ = m.blob(r[0])
            saved = m.data; m.data = blob
            try:
                ret, params, g = m.parse_methodsig(0)
            except Exception:
                ret, params, g = ('?', ['?'], [])
            finally:
                m.data = saved
            return 'standalonesig %s(%s)' % (ret, ', '.join(params))
    except Exception as e:
        return 'TOKEN 0x%08X (err %s)' % (tok, e)
    return 'TOKEN 0x%08X' % tok

def resolve_token_field_method(coded):
    tag = coded & 3
    rid = coded >> 2
    if tag == 0:
        return fmt_method(rid)
    r = m.tbl['MemberRef'][rid - 1]
    return '%s::%s' % (memberref_parent(r[0]), m.s(r[1]))

def memberref_parent(coded):
    tag = coded & 7
    rid = coded >> 3
    if tag == 0:
        return bsearch_type(rid)
    if tag == 1:
        r = m.tbl['TypeRef'][rid - 1]
        nm = m.s(r[1]); ns = m.s(r[2])
        return (ns + '.' + nm) if ns else nm
    if tag == 2:
        return 'ModuleRef#' + str(rid)
    if tag == 3:
        return 'MethodDef#' + fmt_method(rid)
    if tag == 4:
        return m.typespec_str(rid)
    return '?tag%d' % tag

def il_disasm(code, base=0):
    p = 0
    out = []
    n = len(code)
    while p < n:
        ip = p
        op = code[p]; p += 1
        name = SIMPLE_OPS.get(op)
        pre_arg = None
        if name is None and op == 0xFE:
            op2 = code[p]; p += 1
            name = 'FE' + (OP2.get(op2) or ('%02X' % op2))
            if op2 in (0x06, 0x07, 0x15, 0x16, 0x1C):
                v, p = read_u4(code, p); pre_arg = '0x%08X %s' % (v, resolve_token(v))
            elif op2 in (0x09, 0x0B, 0x0C, 0x0E):
                v, p = struct.unpack_from('<H', code, p)[0], p + 2; pre_arg = str(v)
            elif op2 == 0x11:
                v, p = struct.unpack_from('<b', code, p)[0], p + 1; pre_arg = str(v)
            elif op2 == 0x12:
                v, p = read_i8(code, p); pre_arg = str(v)
            elif op2 == 0x13:
                v, p = read_r4(code, p); pre_arg = repr(v)
            elif op2 == 0x14:
                v, p = read_r8(code, p); pre_arg = repr(v)
        elif name is None:
            name = '0x%02X' % op
        arg = pre_arg if pre_arg is not None else ''
        if op in (0x0E,0x0F,0x10,0x11,0x12,0x13,0x1F,0x2B,0x2C,0x2D,0x2E,0x2F,0x30,0x31,0x32,0x33,0x34,0x35,
                  0x36,0x37,0xDE):
            v, p = struct.unpack_from('<b', code, p)[0], p + 1
            arg = str(v)
        elif op in (0x20,):
            v, p = read_i4(code, p); arg = str(v)
        elif op == 0x21:
            v, p = read_i8(code, p); arg = str(v)
        elif op == 0x22:
            v, p = read_r4(code, p); arg = repr(v)
        elif op == 0x23:
            v, p = read_r8(code, p); arg = repr(v)
        elif op in (0x28,0x29,0x6F,0x73,0x7B,0x7C,0x7D,0x7E,0x7F,0x80,0x72,0x74,0x75,0x79,0x8C,0x8D,0x8F,
                    0x81,0x70,0x71,0xA3,0xA4,0xA5,0xD0,0x27):
            v, p = read_u4(code, p); arg = '0x%08X %s' % (v, resolve_token(v))
        elif op in (0x38,0x39,0x3A,0x3B,0x3C,0x3D,0x3E,0x3F,0x40,0x41,0x42,0x43,0x44,0xDD):
            v, p = read_i4(code, p); arg = 'IL_%04X' % (p + v)
        elif op == 0x45:
            n2, p = read_u4(code, p)
            tg = [read_i4(code, p + 4 * i)[0] for i in range(n2)]
            p += 4 * n2
            arg = ' '.join('IL_%04X' % (p + v) for v in tg)
        elif op == 0xFE:
            pass
        elif op in (0x0E,):
            pass
        out.append('IL_%04X: %-12s %s' % (ip, name, arg))
        if p >= n:
            break
    return out

# ---------- commands ----------
def cmd_classof(line):
    line = int(line)
    lines = open(os.path.join(OUT, 'hotupdate_methods.txt'), encoding='utf-8', errors='replace').read().split('\n')
    for i in range(line - 1, -1, -1):
        if lines[i].startswith('=== '):
            print(lines[i]); return
    print('not found')

def cmd_findtype(pat):
    rx = re.compile(pat)
    hits = [(i + 1, n) for i, n in enumerate(TDN) if rx.search(n)]
    print('types:', len(hits))
    for rid, n in hits[:400]:
        print('  0x%08X  %s' % (0x02000000 | rid, n))

def cmd_findfield(pat):
    rx = re.compile(pat)
    cnt = 0
    for rid in range(1, len(m.tbl['Field']) + 1):
        r = m.tbl['Field'][rid - 1]
        nm = m.s(r[1])
        if rx.search(nm):
            cnt += 1
            print('  0x%08X  %s :: %s : %s' % (0x04000000 | rid, field_owner_type(rid), nm, field_type_str(r[2])))
            if cnt > 300:
                print('  ...more'); break
    print('field hits:', cnt)

def cmd_fields(pat):
    rx = re.compile(pat)
    for i, r in enumerate(m.tbl['TypeDef']):
        full = TDN[i]
        if not rx.search(full):
            continue
        fl, ml = r[4], r[5]
        nxt_f = m.tbl['TypeDef'][i + 1][4] if i + 1 < len(m.tbl['TypeDef']) else len(m.tbl['Field']) + 1
        nxt_m = m.tbl['TypeDef'][i + 1][5] if i + 1 < len(m.tbl['TypeDef']) else len(m.tbl['MethodDef']) + 1
        print('=== %s   [0x%08X]  fields=%d methods=%d' % (full, 0x02000000 | (i + 1), nxt_f - fl, nxt_m - ml))
        for fid in range(fl, nxt_f):
            fr = m.tbl['Field'][fid - 1]
            print('    F 0x%08X  %-30s %-46s flags=0x%04X' % (0x04000000 | fid, m.s(fr[1]), field_type_str(fr[2]), fr[0]))

def cmd_props(pat):
    rx = re.compile(pat)
    for i, r in enumerate(m.tbl['TypeDef']):
        full = TDN[i]
        if not rx.search(full):
            continue
        print('=== %s   [0x%08X]' % (full, 0x02000000 | (i + 1)))
        j = _PMAP_BY_TYPE.get(i)
        if j is None:
            print('    (no PropertyMap)')
            continue
        pl = _PMAP[j][1]
        nxt = _PMAP[j + 1][1] if j + 1 < len(_PMAP) else _NPROP + 1
        for pid in range(pl, nxt):
            nm, ty, g, s = prop_getset(pid)
            print('    P 0x%08X  %-30s %-46s get=%s set=%s' % (
                0x17000000 | pid, nm, ty,
                ('0x%08X' % g) if g else '-', ('0x%08X' % s) if s else '-'))

def cmd_methods(pat):
    rx = re.compile(pat)
    for i, r in enumerate(m.tbl['TypeDef']):
        full = TDN[i]
        if not rx.search(full):
            continue
        ml = r[5]
        nxt_m = m.tbl['TypeDef'][i + 1][5] if i + 1 < len(m.tbl['TypeDef']) else len(m.tbl['MethodDef']) + 1
        print('=== %s   [0x%08X]  methods=%d' % (full, 0x02000000 | (i + 1), nxt_m - ml))
        for mid in range(ml, nxt_m):
            print('    M ' + fmt_method(mid))

def cmd_sig(tpat, mpat):
    trx = re.compile(tpat); mrx = re.compile(mpat)
    for rid in range(1, len(m.tbl['MethodDef']) + 1):
        o = method_owner_type(rid)
        if o is None:
            continue
        full = m.td_fullname(o)
        nm = m.s(m.tbl['MethodDef'][rid - 1][3])
        if trx.search(full) and mrx.search(nm):
            print(fmt_method(rid))

def cmd_il(tok):
    rid = int(tok, 16) & 0xFFFFFF
    print(fmt_method(rid))
    code, st = method_body(rid)
    print('body:', st, 'len', len(code) if code else 0)
    if code:
        for l in il_disasm(code):
            print('   ', l)

def cmd_iluse(tpat, mpat):
    trx = re.compile(tpat); mrx = re.compile(mpat)
    for rid in range(1, len(m.tbl['MethodDef']) + 1):
        o = method_owner_type(rid)
        if o is None:
            continue
        full = m.td_fullname(o)
        nm = m.s(m.tbl['MethodDef'][rid - 1][3])
        if trx.search(full) and mrx.search(nm):
            print('\n' + fmt_method(rid))
            code, st = method_body(rid)
            print('body:', st, 'len', len(code) if code else 0)
            if code:
                for l in il_disasm(code):
                    print('   ' + l)

def cmd_strrefs(sub):
    b, sz = m.streams['#US']
    d = m._file
    p = 1
    hits = 0
    while p < sz:
        start = p
        n, p2 = m.cint(p, d)
        raw = d[p2:p2+n]
        val = raw[:-1] if raw and raw[-1] in (0, 1) else raw
        try:
            s = val.decode('utf-16-le')
        except Exception:
            s = ''
        if sub in s:
            hits += 1
            print('  #US+0x%X  %r' % (start, s[:300]))
        p = p2 + n
    print('hits:', hits)

def cmd_callers(tpat, mpat):
    """find methods whose IL references a member owned by matching types / named mpat"""
    trx = re.compile(tpat); mrx = re.compile(mpat)
    targets = set()
    for rid in range(1, len(m.tbl['MethodDef']) + 1):
        o = method_owner_type(rid)
        if o is None: continue
        if trx.search(m.td_fullname(o)) and mrx.search(m.s(m.tbl['MethodDef'][rid - 1][3])):
            targets.add(('m', rid))
    for fid in range(1, len(m.tbl['Field']) + 1):
        if trx.search(field_owner_type(fid)) and mrx.search(m.s(m.tbl['Field'][fid - 1][1])):
            targets.add(('f', fid))
    print('target members:', len(targets))
    found = 0
    for rid in range(1, len(m.tbl['MethodDef']) + 1):
        code, st = method_body(rid)
        if not code: continue
        hit = False
        p = 0
        while p < len(code) - 4:
            op = code[p]
            if op in (0x28,0x6F,0x73,0x7B,0x7C,0x7D,0x7E,0x7F,0x80,0x06,0xFE):
                base = p + 1
                if op == 0xFE:
                    base = p + 2
                if base + 4 <= len(code):
                    v = struct.unpack_from('<I', code, base)[0]
                    t = v >> 24; rr = v & 0xFFFFFF
                    if t == 0x06 and ('m', rr) in targets:
                        hit = True; break
                    if t == 0x04 and ('f', rr) in targets:
                        hit = True; break
            p += 1
        if hit:
            found += 1
            print('  ' + fmt_method(rid))
    print('callers:', found)

if __name__ == '__main__':
    cmd = sys.argv[1]
    a = sys.argv[2:]
    globals()['cmd_' + cmd](*a)
