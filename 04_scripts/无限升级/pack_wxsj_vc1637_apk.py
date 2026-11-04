# -*- coding: utf-8 -*-
"""无限升级 11.266018 / vc1637 —— 外科重打（只换 so，2026-11-04）

底包 = 云端已装 11 定制包（正常版 md5 ca704cc7…）或官方底包；
只替换 lib/arm64-v8a/libstarry_magic.so，其余条目逐字节透传（保持原压缩方式/顺序），
删掉旧签名块（META-INF/*.SF|*.RSA|*.DSA|*.EC 与 MANIFEST.MF 由重签覆盖）。

用法：
  python 04_scripts/无限升级/pack_wxsj_vc1637_apk.py --base <base.apk> --so <libstarry_magic.so> --out <out.apk>
"""
import argparse
import os
import shutil
import sys
import zipfile

SO_ENTRY = 'lib/arm64-v8a/libstarry_magic.so'
SIG_SUFFIX = ('.SF', '.RSA', '.DSA', '.EC')


def main(argv=None):
    ap = argparse.ArgumentParser()
    ap.add_argument('--base', required=True)
    ap.add_argument('--so', required=True)
    ap.add_argument('--dex', default=None, help='可选：替换 classes4.dex（菜单 dex）')
    ap.add_argument('--out', required=True)
    a = ap.parse_args(argv)

    so = open(a.so, 'rb').read()
    dex = open(a.dex, 'rb').read() if a.dex else None
    dex_entry = 'classes4.dex'
    tmp = a.out + '.unsigned'
    n_copy = n_so = n_drop = n_dex = 0
    with zipfile.ZipFile(a.base) as zi, zipfile.ZipFile(tmp, 'w') as zo:
        for info in zi.infolist():
            name = info.filename
            if dex is not None and name == dex_entry:
                zi2 = zipfile.ZipInfo(name, date_time=info.date_time)
                zi2.compress_type = info.compress_type
                zi2.external_attr = info.external_attr
                zo.writestr(zi2, dex)
                n_dex = n_dex + 1
                continue
            if name == SO_ENTRY:
                zi2 = zipfile.ZipInfo(name, date_time=info.date_time)
                zi2.compress_type = info.compress_type
                zi2.external_attr = info.external_attr
                zo.writestr(zi2, so)
                n_so += 1
                continue
            if name.startswith('META-INF/') and (name.upper().endswith(SIG_SUFFIX) or name.upper().endswith('MANIFEST.MF')):
                n_drop += 1
                continue
            data = zi.read(name)
            zi2 = zipfile.ZipInfo(name, date_time=info.date_time)
            zi2.compress_type = info.compress_type
            zi2.external_attr = info.external_attr
            zo.writestr(zi2, data)
            n_copy += 1
    if dex is not None and n_dex != 1:
        raise SystemExit('替换 dex 条目数 = %d（必须恰好 1）' % n_dex)
    if n_so != 1:
        raise SystemExit('替换 so 条目数 = %d（必须恰好 1）' % n_so)
    with zipfile.ZipFile(tmp) as z:
        bad = z.testzip()
        if bad:
            raise SystemExit('重打后 zip 校验失败：%s' % bad)
    if os.path.exists(a.out):
        os.remove(a.out)
    os.rename(tmp, a.out)
    print('重打完成: %s  透传 %d 条 / 替换 so 1 条 / 替换 dex %d 条 / 丢旧签名 %d 条 / so %d B'
          % (a.out, n_copy, n_dex, n_drop, len(so)))
    return 0


if __name__ == '__main__':
    sys.exit(main())
