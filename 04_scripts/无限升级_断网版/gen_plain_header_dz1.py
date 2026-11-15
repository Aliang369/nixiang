# -*- coding: utf-8 -*-
"""生成 wxsjdz1（无限升级·断网版）明文载荷头 + 校验清单

底包：第三方断网包 com.xingzuodaoshi.infinityOffline / vc1647
基线 DLL：06_dumped/无限升级_断网版/diag_dump/targetfuntional_OFFLINE_20261115.dll
          1,995,776 B / sha256 087fd1c3… / fnv1a64 0xaca5988be25f5119
补丁 DLL：ilpatch_dz1（30 patches）→ ilpatch_delta_dz1（+1）→ 2,008,576 B

⚠ 本号补丁 **大于** 官方数组 12,800 B（Cecil 重写固有膨胀 + 补丁代码），
   框架不能 memcpy 进官方数组 ⇒ native 侧走「自建数组」注入（il2cpp_array_new_specific）。
   本脚本把「补丁长度 / fnv / sha」写成编译期常量，供 so 内变体表与日志用。

用法：
  python3 04_scripts/无限升级_断网版/gen_plain_header_dz1.py \
      --src 07_patched/无限升级_断网版/build/dz1_v2.dll
"""
import argparse
import hashlib
import io
import os

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
DEFAULT_OUT = os.path.join(ROOT, 'mod菜单', 'starry-mod-menu', 'app', 'src', 'main', 'jni',
                           'Includes', 'sg_plain_wxsjdz1.h')
OFFICIAL_LEN = 1995776
OFFICIAL_FNV = 0xaca5988be25f5119


def fnv1a64(data):
    h = 0xcbf29ce484222325
    for b in data:
        h = ((h ^ b) * 0x100000001b3) & 0xFFFFFFFFFFFFFFFF
    return h


def main(argv=None):
    ap = argparse.ArgumentParser()
    ap.add_argument('--src', required=True, help='补丁 DLL（dz1_v2.dll）')
    ap.add_argument('--out', default=DEFAULT_OUT)
    a = ap.parse_args(argv)

    data = io.open(a.src, 'rb').read()
    if data[:2] != b'MZ':
        raise SystemExit('不是 PE: %s' % a.src)
    if data.find(b'BSJB') < 0:
        raise SystemExit('缺 BSJB 元数据签名: %s' % a.src)
    if data.find(b'CodexOfflineBridge') < 0:
        raise SystemExit('缺本包特征串 CodexOfflineBridge（拿错 DLL？）')
    ln = len(data)
    sha = hashlib.sha256(data).hexdigest()
    fnv = fnv1a64(data)
    pad = OFFICIAL_LEN - ln

    lines = [
        '#pragma once',
        '/* 自动生成，勿手改：python3 04_scripts/无限升级_断网版/gen_plain_header_dz1.py',
        ' * 无限升级·断网版 wxsjdz1（vc1647）明文补丁载荷常量。',
        ' * 官方基线 %d B / fnv 0x%016x；补丁 %d B（%s官方数组 %+d B）。'
        % (OFFICIAL_LEN, OFFICIAL_FNV, ln, '超出' if pad < 0 else '小于', pad),
        ' * 超长 ⇒ native 侧自建数组注入（il2cpp_array_new_specific + pinned gchandle）。',
        ' */',
        '#include <stdint.h>',
        'static const char* kWxsjdz1PlainPolicy = "%s";'
        % ('plain-inline-selfarray' if pad < 0 else 'plain-inline-inplace'),
        '',
        '/* %d B，sha256 %s */' % (ln, sha),
        'static const uint32_t kWxsjdz1PayloadLen = %du;' % ln,
        'static const char* kWxsjdz1PayloadSha = "%s";' % sha,
        'static const uint64_t kWXSJDZ1_PATCHED_FNV = 0x%016xULL;' % fnv,
        '/* pad = 官方数组 - 补丁 = %d（负数=需自建更大数组）*/' % pad,
        'static const int32_t kWxsjdz1PayloadPad = %d;' % pad,
        '',
    ]
    io.open(a.out, 'w', encoding='utf-8', newline='\n').write('\n'.join(lines))
    print('payload  : %s' % a.src)
    print('len      : %d (official %d, pad %+d)' % (ln, OFFICIAL_LEN, pad))
    print('sha256   : %s' % sha)
    print('fnv1a64  : 0x%016x' % fnv)
    print('header   : %s' % os.path.relpath(a.out, ROOT))
    return 0


if __name__ == '__main__':
    raise SystemExit(main())
