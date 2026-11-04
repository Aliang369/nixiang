# -*- coding: utf-8 -*-
"""生成 11 线明文载荷头（无加固版，2026-11-04）

加固/载荷密文按用户 2026-11-04 决定全部去掉 ⇒ so 里直接内嵌明文补丁 DLL。
本脚本把三份变体的**长度与 sha256**写成编译期常量，供 WxsjVc1637Mod.cpp 选变体与日志用；
同时给构建脚本一个可校验的清单（sha 不符 = 拿错 DLL）。

用法：
  python 04_scripts/无限升级/gen_plain_header.py [--dir 07_patched/无限升级] [--out <header>]
"""
import argparse
import hashlib
import io
import os

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
DEFAULT_OUT = os.path.join(ROOT, 'mod菜单', 'starry-mod-menu', 'app', 'src', 'main', 'jni',
                           'Includes', 'sg_plain_wxsj_vc1637.h')
VARIANTS = [
    ('hf2', 'targetfuntional_vc1637_hf2_feat3.dll', 2030080),
    ('hf4', 'targetfuntional_vc1637_hf4_feat3.dll', 2028544),
    ('hf3', 'targetfuntional_vc1637_hf3_feat3.dll', 2028544),
]


def main(argv=None):
    ap = argparse.ArgumentParser()
    ap.add_argument('--dir', default=os.path.join(ROOT, '07_patched', '无限升级'))
    ap.add_argument('--out', default=DEFAULT_OUT)
    a = ap.parse_args(argv)

    lines = [
        '#pragma once',
        '/* 自动生成，勿手改：python 04_scripts/无限升级/gen_plain_header.py',
        ' * 无加固形态：so 内嵌**明文**补丁 DLL（2026-11-04 用户定：加固与载荷 sha 全去）。',
        ' * 变体按运行时下发的官方 DLL 长度 + fnv 选择，选不到一律 PASSTHROUGH 官方。',
        ' */',
        '#include <stdint.h>',
        'static const char* kWxsjvcPlainPolicy = "plain-inline";',
    ]
    total = 0
    for tag, fn, official in VARIANTS:
        p = os.path.join(a.dir, fn)
        if not os.path.exists(p):
            raise SystemExit('缺输入: %s（先跑 ilpatch_vc1637_feat3）' % p)
        data = io.open(p, 'rb').read()
        if len(data) > official:
            raise SystemExit('%s 长度 %d 超过官方数组 %d' % (fn, len(data), official))
        sha = hashlib.sha256(data).hexdigest()
        total += len(data)
        lines.append('')
        lines.append('/* %s：%s  %d B（官方数组 %d，余量 %d）*/'
                     % (tag, fn, len(data), official, official - len(data)))
        lines.append('static const uint32_t kWxsjvc%sPlainLen = %du;' % (tag.capitalize(), len(data)))
        lines.append('static const char* kWxsjvc%sPlainSha = "%s";' % (tag.capitalize(), sha))
    lines.append('')
    lines.append('/* 三份明文合计 %d B */' % total)
    lines.append('')
    io.open(a.out, 'w', encoding='utf-8', newline='\n').write('\n'.join(lines))
    print('已写 %s' % os.path.relpath(a.out, ROOT))
    return 0


if __name__ == '__main__':
    raise SystemExit(main())
