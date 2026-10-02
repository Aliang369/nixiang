# -*- coding: utf-8 -*-
"""批量查询驱动：从 UTF-8 spec 文件读目标，绕开 PowerShell 重编码混淆类名。
spec 行格式：
  CLS   <class-descriptor>            列该类所有方法
  CLSF  <class-descriptor> <子串>      过滤
  CLSHEX <descriptor-utf8-hex>        按字节定位类
  METH  <class-descriptor>-><name>(<params>)<ret>   反汇编该方法
  FINDP <proto>                       全库找 rest 以该 proto 结尾的方法并反汇编
  FINDD <proto>                       同上但只列签名
用法: python _q.py <dex> <spec.txt> <out.txt> [--maxlines N]
"""
import sys, io
sys.stdout.reconfigure(encoding='utf-8', errors='replace')
import _dex_disasm as D


def main():
    dexpath, specpath, outpath = sys.argv[1], sys.argv[2], sys.argv[3]
    maxlines = 0
    if '--maxlines' in sys.argv:
        maxlines = int(sys.argv[sys.argv.index('--maxlines') + 1])
    blob = open(dexpath, 'rb').read()
    dex = D.Dex(blob, dexpath)
    dis = D.Disasm(dex)
    out = []

    def list_cls(cd, label):
        out.append('=== %s (class_def idx=%s access=0x%X super=%s) ===' %
                   (label, cd['idx'], cd['access'], dex.type_desc(cd['superclass_idx'])))
        for (mi, acc, co, kind) in dex.all_methods(cd):
            out.append('   [%s acc=0x%X code_off=0x%06X] %s' % (kind, acc, co, dex.method_full(mi)))
        out.append('')

    def dump1(cd, mi, acc, co, kind, tag):
        out.append('### %s' % tag)
        before = len(out)
        D.dump_method(dex, dis, cd, mi, acc, co, kind, out, indent='  ')
        if maxlines and len(out) - before > maxlines:
            del out[before + maxlines:]
            out.append('  ... (截断)')
        out.append('')

    lines = [l.rstrip('\n') for l in io.open(specpath, encoding='utf-8')
             if l.strip() and not l.lstrip().startswith('#')]
    for ln in lines:
        parts = ln.split(None, 2)
        cmd = parts[0].upper()
        if cmd in ('CLS', 'CLSF'):
            desc = parts[1].strip()
            filt = parts[2].strip() if len(parts) > 2 else ''
            cd = dex.find_class(desc)
            if cd is None:
                out.append('=== CLS %r : NOT FOUND ===' % desc); out.append(''); continue
            out.append('=== CLS %r (idx=%s access=0x%X super=%s) ===' %
                       (desc, cd['idx'], cd['access'], dex.type_desc(cd['superclass_idx'])))
            for (mi, acc, co, kind) in dex.all_methods(cd):
                mf = dex.method_full(mi)
                if filt and filt not in mf:
                    continue
                out.append('   [%s acc=0x%X code_off=0x%06X] %s' % (kind, acc, co, mf))
            d = cd.get('data') or dex.class_data(cd)
            for key in ('static_fields', 'instance_fields'):
                for (fi, acc) in d[key]:
                    c, t, n = dex.field(fi)
                    out.append('   FIELD[%s acc=0x%X] %s %s ; decl=%s' % (key, acc, D.shorten_type(t), n, c))
            out.append('')
        elif cmd == 'CLSHEX':
            want = bytes.fromhex(parts[1].strip()).decode('utf-8')
            cd = dex.find_class(want)
            if cd is None:
                out.append('=== CLSHEX %r : NOT FOUND ===' % want); out.append(''); continue
            list_cls(cd, 'CLSHEX %r' % want)
        elif cmd == 'METH':
            sig = parts[1].strip() if len(parts) > 1 else ''
            if '->' not in sig:
                out.append('=== METH %r : BAD SIG ===' % sig); continue
            cdesc, _rest = sig.split('->', 1)
            cd = dex.find_class(cdesc.strip())
            if cd is None:
                out.append('=== METH %r : CLASS NOT FOUND ===' % sig); out.append(''); continue
            hit = None
            for (mi, acc, co, kind) in dex.all_methods(cd):
                if dex.method_full(mi) == sig:
                    hit = (mi, acc, co, kind); break
            if hit is None:
                out.append('=== METH %r : NOT FOUND ===' % sig); out.append(''); continue
            dump1(cd, hit[0], hit[1], hit[2], hit[3], sig)
        elif cmd == 'COFF':
            want = int(parts[1].strip(), 0)
            hit = None
            for cd in dex.class_defs():
                for (mi, acc, co, kind) in dex.all_methods(cd):
                    if co == want:
                        hit = (cd, mi, acc, co, kind); break
                if hit: break
            if hit is None:
                out.append('=== COFF 0x%X : NOT FOUND ===' % want); out.append(''); continue
            dump1(hit[0], hit[1], hit[2], hit[3], hit[4],
                  'COFF 0x%X %s' % (want, dex.method_full(hit[1])))
        elif cmd in ('FINDP', 'FINDD'):
            proto = parts[1].strip()
            clsflt = parts[2].strip().lower() if len(parts) > 2 else ''
            dolist = (cmd == 'FINDD')
            n = 0
            for cd in dex.class_defs():
                cdesc = dex.type_desc(cd['class_idx'])
                for (mi, acc, co, kind) in dex.all_methods(cd):
                    mfull = dex.method_full(mi)
                    if '->' not in mfull:
                        continue
                    if mfull.split('->', 1)[1].endswith(proto):
                        if clsflt and clsflt not in cdesc.encode('utf-8').hex():
                            continue
                        n += 1
                        if dolist:
                            out.append('### [%d] cls_hex=%s cls=%r code_off=0x%06X acc=0x%X [%s]' %
                                       (n, cdesc.encode('utf-8').hex(), cdesc, co, acc, kind))
                        else:
                            dump1(cd, mi, acc, co, kind,
                                  '[%d] cls_hex=%s cls=%r co=0x%06X %s' %
                                  (n, cdesc.encode('utf-8').hex(), cdesc, co, mfull))
            out.append('=== %s %s : %d 个候选 ===' % (cmd, proto, n))
            out.append('')
        else:
            out.append('=== 未知命令 %r ===' % ln)

    io.open(outpath, 'w', encoding='utf-8', newline='\n').write('\n'.join(out) + '\n')
    print('WROTE', outpath, len(out), 'lines')


if __name__ == '__main__':
    main()
