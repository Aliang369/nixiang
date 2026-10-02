"""ECMA-335 metadata reader (pure stdlib) for the extracted Assembly-CSharp.dll.

Produces: typedef/methoddef listings, signature decoding, #US heap scan.
"""
import struct, os, hashlib

TABLES = [
    ('Module',        [('Generation','u16'),('Name','s'),('Mvid','g'),('EncId','g'),('EncBaseId','g')]),
    ('TypeRef',       [('ResolutionScope','c:ResolutionScope'),('Name','s'),('Namespace','s')]),
    ('TypeDef',       [('Flags','u32'),('Name','s'),('Namespace','s'),('Extends','c:TypeDefOrRef'),('FieldList','t:Field'),('MethodList','t:MethodDef')]),
    ('FieldPtr',      [('Field','t:Field')]),
    ('Field',         [('Flags','u16'),('Name','s'),('Signature','b')]),
    ('MethodPtr',     [('Method','t:MethodDef')]),
    ('MethodDef',     [('RVA','u32'),('ImplFlags','u16'),('Flags','u16'),('Name','s'),('Signature','b'),('ParamList','t:Param')]),
    ('ParamPtr',      [('Param','t:Param')]),
    ('Param',         [('Flags','u16'),('Sequence','u16'),('Name','s')]),
    ('InterfaceImpl', [('Class','t:TypeDef'),('Interface','c:TypeDefOrRef')]),
    ('MemberRef',     [('Class','c:MemberRefParent'),('Name','s'),('Signature','b')]),
    ('Constant',      [('Type','u16'),('Parent','c:HasConstant'),('Value','b')]),
    ('CustomAttribute',[('Parent','c:HasCustomAttribute'),('Type','c:CustomAttributeType'),('Value','b')]),
    ('FieldMarshal',  [('Parent','c:HasFieldMarshal'),('NativeType','b')]),
    ('DeclSecurity',  [('Action','u16'),('Parent','c:HasDeclSecurity'),('PermissionSet','b')]),
    ('ClassLayout',   [('PackingSize','u16'),('ClassSize','u32'),('Parent','t:TypeDef')]),
    ('FieldLayout',   [('Offset','u32'),('Field','t:Field')]),
    ('StandAloneSig', [('Signature','b')]),
    ('EventMap',      [('Parent','t:TypeDef'),('EventList','t:Event')]),
    ('EventPtr',      [('Event','t:Event')]),
    ('Event',         [('EventFlags','u16'),('Name','s'),('EventType','c:TypeDefOrRef')]),
    ('PropertyMap',   [('Parent','t:TypeDef'),('PropertyList','t:Property')]),
    ('PropertyPtr',   [('Property','t:Property')]),
    ('Property',      [('Flags','u16'),('Name','s'),('Type','b')]),
    ('MethodSemantics',[('Semantics','u16'),('Method','t:MethodDef'),('Association','c:HasSemantics')]),
    ('MethodImpl',    [('Class','t:TypeDef'),('MethodBody','c:MethodDefOrRef'),('MethodDeclaration','c:MethodDefOrRef')]),
    ('ModuleRef',     [('Name','s')]),
    ('TypeSpec',      [('Signature','b')]),
    ('ImplMap',       [('MappingFlags','u16'),('MemberForwarded','c:MemberForwarded'),('ImportName','s'),('ImportScope','t:ModuleRef')]),
    ('FieldRVA',      [('RVA','u32'),('Field','t:Field')]),
    ('ENCLog',        [('Token','u32'),('FuncCode','u32')]),
    ('ENCMap',        [('Token','u32')]),
    ('Assembly',      [('HashAlgId','u32'),('MajorVersion','u16'),('MinorVersion','u16'),('BuildNumber','u16'),('RevisionNumber','u16'),('Flags','u32'),('PublicKey','b'),('Name','s'),('Culture','s')]),
    ('AssemblyProcessor',[('Processor','u32')]),
    ('AssemblyOS',    [('OSPlatformID','u32'),('OSMajorVersion','u32'),('OSMinorVersion','u32')]),
    ('AssemblyRef',   [('MajorVersion','u16'),('MinorVersion','u16'),('BuildNumber','u16'),('RevisionNumber','u16'),('Flags','u32'),('PublicKeyOrToken','b'),('Name','s'),('Culture','s'),('HashValue','b')]),
    ('AssemblyRefProcessor',[('Processor','u32'),('AssemblyRef','t:AssemblyRef')]),
    ('AssemblyRefOS', [('OSPlatformID','u32'),('OSMajorVersion','u32'),('OSMinorVersion','u32'),('AssemblyRef','t:AssemblyRef')]),
    ('File',          [('Flags','u32'),('Name','s'),('HashValue','b')]),
    ('ExportedType',  [('Flags','u32'),('TypeDefId','u32'),('TypeName','s'),('TypeNamespace','s'),('Implementation','c:Implementation')]),
    ('ManifestResource',[('Offset','u32'),('Flags','u32'),('Name','s'),('Implementation','c:Implementation')]),
    ('NestedClass',   [('NestedClass','t:TypeDef'),('EnclosingClass','t:TypeDef')]),
    ('GenericParam',  [('Number','u16'),('Flags','u16'),('Owner','c:TypeOrMethodDef'),('Name','s')]),
    ('MethodSpec',    [('Method','c:MethodDefOrRef'),('Instantiation','b')]),
    ('GenericParamConstraint',[('Owner','t:GenericParam'),('Constraint','c:TypeDefOrRef')]),
]

CODED = {
    'TypeDefOrRef':      (['TypeDef','TypeRef','TypeSpec'], 2),
    'HasConstant':       (['Field','Param','Property'], 2),
    'HasCustomAttribute':(['MethodDef','Field','TypeRef','TypeDef','Param','InterfaceImpl','MemberRef','Module',
                           'DeclSecurity','Property','Event','StandAloneSig','ModuleRef','TypeSpec','Assembly',
                           'AssemblyRef','File','ExportedType','ManifestResource','GenericParam',
                           'GenericParamConstraint','MethodSpec'], 5),
    'HasFieldMarshal':   (['Field','Param'], 1),
    'HasDeclSecurity':   (['TypeDef','MethodDef','Assembly'], 2),
    'MemberRefParent':   (['TypeDef','TypeRef','ModuleRef','MethodDef','TypeSpec'], 3),
    'HasSemantics':      (['Event','Property'], 1),
    'MethodDefOrRef':    (['MethodDef','MemberRef'], 1),
    'MemberForwarded':   (['Field','MethodDef'], 1),
    'Implementation':    (['File','AssemblyRef','ExportedType'], 2),
    'CustomAttributeType':(['<none>','<none>','MethodDef','MemberRef','<none>'], 3),
    'ResolutionScope':   (['Module','ModuleRef','AssemblyRef','TypeRef'], 2),
    'TypeOrMethodDef':   (['TypeDef','MethodDef'], 1),
}


class Meta:
    def __init__(self, path):
        self.data = open(path, 'rb').read()
        self._file = self.data          # 永久引用整文件；self.data 可被临时换成 blob 做签名解码
        self.parse_pe()
        self.parse_root()
        self.parse_tables()
        self.build_names()

    # ---------- PE ----------
    def parse_pe(self):
        d = self.data
        pe = struct.unpack_from('<I', d, 0x3C)[0]
        assert d[pe:pe+4] == b'PE\0\0'
        nsec = struct.unpack_from('<H', d, pe + 6)[0]
        opt_size = struct.unpack_from('<H', d, pe + 20)[0]
        opt = pe + 24
        self.sects = []
        for i in range(nsec):
            s = opt + opt_size + i * 40
            name = d[s:s+8].rstrip(b'\0').decode('latin-1')
            vsize, vaddr, rsize, raddr = struct.unpack_from('<IIII', d, s + 8)
            self.sects.append((name, vsize, vaddr, rsize, raddr))
        clr_rva, clr_sz = struct.unpack_from('<II', d, opt + 96 + 14 * 8)
        co = self.r2o(clr_rva)
        _cb, maj, mnr, mdrva, mdsz = struct.unpack_from('<IHHII', d, co)
        self.runtime = '%d.%d' % (maj, mnr)
        self.md_off = self.r2o(mdrva)
        self.md_size = mdsz

    def r2o(self, rva):
        for name, vsize, vaddr, rsize, raddr in self.sects:
            if vaddr <= rva < vaddr + max(vsize, rsize):
                return raddr + (rva - vaddr)
        raise ValueError('rva 0x%X unmapped' % rva)

    # ---------- metadata root ----------
    def parse_root(self):
        d, o = self.data, self.md_off
        assert d[o:o+4] == b'BSJB'
        verlen = struct.unpack_from('<I', d, o + 12)[0]
        self.md_version = d[o+16:o+16+verlen].rstrip(b'\0').decode('latin-1')
        p = o + 16 + ((verlen + 3) // 4) * 4
        flags, nstreams = struct.unpack_from('<HH', d, p); p += 4
        self.streams = {}
        for i in range(nstreams):
            so, ss = struct.unpack_from('<II', d, p); p += 8
            e = d.index(b'\0', p)
            nm = d[p:e].decode('latin-1'); p = (e + 1 + 3) & ~3
            self.streams[nm] = (o + so, ss)

    def heap(self, nm):
        return self.streams.get(nm)

    def s(self, idx):
        """#Strings index -> str（始终读整文件，与 self.data 临时替换无关）"""
        if idx == 0:
            return ''
        b, sz = self.str_heap
        d = self._file
        e = d.index(b'\0', b + idx)
        return d[b + idx:e].decode('utf-8', 'replace')

    def blob(self, idx):
        b, sz = self.blob_heap
        d = self._file
        p = b + idx
        n, p = self.cint(p, d)
        return d[p:p+n], p + n

    def cint(self, p, d=None):
        """compressed unsigned int"""
        d = self.data if d is None else d
        b0 = d[p]
        if b0 & 0x80 == 0:
            return b0, p + 1
        if b0 & 0xC0 == 0x80:
            return ((b0 & 0x3F) << 8) | d[p+1], p + 2
        return ((b0 & 0x1F) << 24) | (d[p+1] << 16) | (d[p+2] << 8) | d[p+3], p + 4

    # ---------- tables ----------
    def parse_tables(self):
        toff, tsize = self.streams['#~']
        d = self.data
        p = toff
        self.heapsizes = d[p + 6]
        valid = struct.unpack_from('<Q', d, p + 8)[0]
        sorted_ = struct.unpack_from('<Q', d, p + 16)[0]
        p += 24
        self.valid = valid
        self.rows = {}
        for i in range(64):
            if valid >> i & 1:
                self.rows[i] = struct.unpack_from('<I', d, p)[0]; p += 4
        # compute column sizes
        self._sizes = {}
        for i, (nm, cols) in enumerate(TABLES):
            if i not in self.rows:
                continue
            self._sizes[i] = self._col_sizes(cols)
        # read rows
        self.tbl = {}
        for i, (nm, cols) in enumerate(TABLES):
            if i not in self.rows:
                self.tbl[nm] = []
                continue
            n = self.rows[i]
            widths = self._sizes[i]
            rowlen = sum(w for _, w in widths)
            rows = []
            base = p
            for r in range(n):
                ro = base + r * rowlen
                vals = []
                for (cn, w) in widths:
                    if w == 2:
                        vals.append(struct.unpack_from('<H', d, ro)[0])
                    elif w == 4:
                        vals.append(struct.unpack_from('<I', d, ro)[0])
                    else:
                        vals.append((ro, w))
                    ro += w
                rows.append(vals)
            self.tbl[nm] = rows
            p += n * rowlen
        self.tbl_end = p

    def _col_sizes(self, cols):
        out = []
        for name, kind in cols:
            if kind == 'u16': out.append((name, 2))
            elif kind == 'u32': out.append((name, 4))
            elif kind == 's': out.append((name, 4 if self.heapsizes & 1 else 2))
            elif kind == 'g': out.append((name, 4 if self.heapsizes & 2 else 2))
            elif kind == 'b': out.append((name, 4 if self.heapsizes & 4 else 2))
            elif kind.startswith('t:'):
                t = kind[2:]
                out.append((name, 4 if self.rows.get(self._tid(t), 0) >= 65536 else 2))
            elif kind.startswith('c:'):
                cn = kind[2:]
                tabs, bits = CODED[cn]
                mx = max(self.rows.get(self._tid(t), 0) for t in tabs if t in [x[0] for x in TABLES])
                out.append((name, 4 if mx >= (1 << (16 - bits)) else 2))
            else:
                raise ValueError(kind)
        return out

    def _tid(self, name):
        for i, (nm, _) in enumerate(TABLES):
            if nm == name:
                return i
        raise KeyError(name)

    def get(self, tname, rid, col):
        cols = [c[0] for c in TABLES[self._tid(tname)][1]]
        return self.tbl[tname][rid - 1][cols.index(col)]

    # ---------- names ----------
    def build_names(self):
        self.str_heap = self.streams['#Strings']
        self.blob_heap = self.streams['#Blob']
        # TypeDef names + nesting
        tds = self.tbl['TypeDef']
        self.td_name = []
        self.td_ns = []
        for r in tds:
            self.td_name.append(self.s(r[1]))
            self.td_ns.append(self.s(r[2]))
        self.nested_of = {}
        for r in self.tbl['NestedClass']:
            self.nested_of[r[0] - 1] = r[1]
        self.asm_name = ''
        if self.tbl.get('Assembly'):
            self.asm_name = self.s(self.tbl['Assembly'][0][7])

    def td_fullname(self, rid):
        i = rid - 1
        parts = []
        cur = i
        guard = 0
        while True:
            parts.append(self.td_name[cur])
            if cur in self.nested_of:
                cur = self.nested_of[cur] - 1
                guard += 1
                if guard > 32: break
            else:
                ns = self.td_ns[cur]
                break
        full = '.'.join(reversed(parts))
        return (ns + '.' + full) if ns else full

    # ---------- signature ----------
    PRIM = {0x01:'void',0x02:'bool',0x03:'char',0x04:'sbyte',0x05:'byte',0x06:'short',0x07:'ushort',
            0x08:'int',0x09:'uint',0x0A:'long',0x0B:'ulong',0x0C:'float',0x0D:'double',0x0E:'string',
            0x16:'System.TypedReference',0x18:'IntPtr',0x19:'UIntPtr',0x1C:'object'}

    def type_from_defref(self, coded):
        tag = coded & 0x3
        rid = coded >> 2
        if tag == 0:
            return self.td_fullname(rid)
        if tag == 1:
            r = self.tbl['TypeRef'][rid - 1]
            nm = self.s(r[1]); ns = self.s(r[2])
            return (ns + '.' + nm) if ns else nm
        if tag == 2:
            return self.typespec_str(rid)
        return '?'

    def typespec_str(self, rid):
        """TypeSpec row -> 解析其 Signature blob（如 List`1<Foo>）"""
        try:
            r = self.tbl['TypeSpec'][rid - 1]
            blob, _ = self.blob(r[0])
        except Exception:
            return 'TypeSpec#%d' % rid
        saved = self.data
        self.data = blob
        try:
            t, _p = self.parse_type(0)
        except Exception:
            t = 'TypeSpec#%d' % rid
        finally:
            self.data = saved
        return t

    def sig_blob(self, blob):
        """在给定 blob 上解码方法签名，不影响字符串堆读取"""
        saved = self.data
        self.data = blob
        try:
            return self.parse_methodsig(0)
        finally:
            self.data = saved

    def parse_type(self, p, depth=0):
        d = self.data
        et = d[p]; p += 1
        if et in self.PRIM:
            return self.PRIM[et], p
        if et == 0x0F:
            t, p = self.parse_type(p, depth + 1); return t + '*', p
        if et == 0x10:
            t, p = self.parse_type(p, depth + 1); return 'ref ' + t, p
        if et in (0x11, 0x12):
            v, p = self.cint(p)
            return self.type_from_defref(v), p
        if et == 0x13:
            v, p = self.cint(p); return '!%d' % v, p
        if et == 0x1E:
            v, p = self.cint(p); return '!!%d' % v, p
        if et == 0x1D:
            t, p = self.parse_type(p, depth + 1); return t + '[]', p
        if et == 0x14:
            t, p = self.parse_type(p, depth + 1)
            rank, p = self.cint(p)
            ns, p = self.cint(p)
            for _ in range(ns):
                _x, p = self.cint(p)
            nl, p = self.cint(p)
            for _ in range(nl):
                _x, p = self.cint(p)
            return '%s[%s]' % (t, ',' * (rank - 1)), p
        if et == 0x15:
            kind = d[p]; p += 1
            v, p = self.cint(p)
            base = self.type_from_defref(v) if kind in (0x11, 0x12) else '?'
            n, p = self.cint(p)
            args = []
            for _ in range(n):
                a, p = self.parse_type(p, depth + 1); args.append(a)
            return '%s<%s>' % (base, ','.join(args)), p
        if et == 0x1B:
            return self.parse_methodsig(p)
        if et in (0x1F, 0x20):
            v, p = self.cint(p)
            inner, p = self.parse_type(p, depth + 1)
            mod = 'modreq' if et == 0x1F else 'modopt'
            return '%s(%s)' % (inner, mod), p
        if et == 0x45:
            t, p = self.parse_type(p, depth + 1); return t + ' pinned', p
        if et == 0x41:
            return '...', p
        if et == 0x21:
            return 'internal', p
        return 'ELEMENT_0x%02X' % et, p

    def parse_methodsig(self, p):
        d = self.data
        cc = d[p]; p += 1
        gen = []
        if cc & 0x10:
            n, p = self.cint(p)
            gen = ['!%d' % i for i in range(n)]
        n, p = self.cint(p)
        ret, p = self.parse_type(p)
        params = []
        for _ in range(n):
            if d[p] == 0x41:
                p += 1
            t, p = self.parse_type(p); params.append(t)
        return (ret, params, gen)


def main():
    OUT = r'E:\nixiang\06_dumped\冒险与征途\aot'
    m = Meta(os.path.join(OUT, 'Assembly-CSharp.dll'))
    print('runtime      =', m.runtime)
    print('md version   =', m.md_version)
    print('assembly     =', repr(m.asm_name))
    print('heapsizes    = 0x%X' % m.heapsizes)
    print('streams      =', {k: (hex(v[0]), v[1]) for k, v in m.streams.items()})
    print('table rows:')
    for i in sorted(m.rows):
        print('   %-24s %d' % (TABLES[i][0], m.rows[i]))
    print('tables end at 0x%X (stream #~ at 0x%X size %d)' % (m.tbl_end, m.streams['#~'][0], m.streams['#~'][1]))
    print('TypeDef count =', len(m.tbl['TypeDef']), 'MethodDef count =', len(m.tbl['MethodDef']))
    # sample
    for i in range(1, 4):
        print('  td%d %s' % (i, m.td_fullname(i)))
    import pickle
    print('OK')


if __name__ == '__main__':
    main()
