# -*- coding: utf-8 -*-
"""汉克旅店 1.1.0 — 生成 so 内嵌载荷头（明文补丁 DLL 的长度 / sha256 / 官方基线）

用法:
  python3 04_scripts/汉克旅店/gen_hkld_payload_header.py [--dll <patched.dll>] [--out <header>]

产物: mod菜单/starry-mod-menu/app/src/main/jni/Includes/sg_plain_hkld.h
判据: 补丁 DLL 的 sha256 / 长度写进编译期常量；native 侧装载时比对「运行时装到的官方 DLL 长度」
      决定是否替换（长度不符 = 版本不匹配 -> PASSTHROUGH 官方，不崩不闪退）。
"""
import argparse
import hashlib
import io
import os

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
DEFAULT_DLL = os.path.join(ROOT, '07_patched', '汉克旅店', 'Assembly-CSharp_hkld.dll')
DEFAULT_OUT = os.path.join(ROOT, 'mod菜单', 'starry-mod-menu', 'app', 'src', 'main', 'jni',
                           'Includes', 'sg_plain_hkld.h')
# 官方（1.1.0 vc337）热更程序集基线 —— 分析报告附录 B
# ★运行时基线（2026-11-13 真机实锤）：热更下发并真正被 Assembly.Load 装载的是这份。
OFFICIAL_LEN = 5328384
OFFICIAL_SHA = '263e11ebfca73cfc3a014a4dbae170ffafb20c0a30bef4964fac1f8d92ca1d31'
# 包内 bundle 里那份（离线首启可能用到；内容与运行时基线从偏移 136 起就不同）
BUNDLED_LEN = 5328896
BUNDLED_SHA = 'd2e7edf03f24f276db78d240e147ed3398263ee81923db47d9cb2e94ded33277'


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--dll', default=DEFAULT_DLL)
    ap.add_argument('--out', default=DEFAULT_OUT)
    a = ap.parse_args()

    if not os.path.isfile(a.dll):
        raise SystemExit('缺输入: ' + a.dll + '（先跑 04_scripts/汉克旅店/ilpatch）')
    data = io.open(a.dll, 'rb').read()
    sha = hashlib.sha256(data).hexdigest()
    lines = [
        '#pragma once',
        '/* 自动生成，勿手改：python3 04_scripts/汉克旅店/gen_hkld_payload_header.py',
        ' * 汉克旅店 1.1.0 (vc337) 热更程序集补丁载荷（明文内嵌，无加固/无票）。',
        ' * native 侧策略：运行时装配长度 == kHkldOfficialLen 才替换；否则 PASSTHROUGH 官方版。',
        ' */',
        '#include <stdint.h>',
        '',
        'static const char* kHkldPayloadPolicy = "plain-inline";',
        '',
        '/* 官方基线 Assembly-CSharp.dll：%d B */' % OFFICIAL_LEN,
        'static const uint32_t kHkldOfficialLen = %du;' % OFFICIAL_LEN,
        'static const char* kHkldOfficialSha = "%s";' % OFFICIAL_SHA,
        '',
        '/* 包内 bundle 版本（离线首启可能命中）。补丁载荷按运行时基线制作，' ,
        ' * 命中该长度时同样替换，但日志会标 alt —— 两者内容不同，需真机复核。 */',
        'static const uint32_t kHkldBundledLen = %du;' % BUNDLED_LEN,
        'static const char* kHkldBundledSha = "%s";' % BUNDLED_SHA,
        '',
        '/* 补丁产物：%d B（比官方 %+d B）sha256=%s */' % (len(data), len(data) - OFFICIAL_LEN, sha),
        'static const uint32_t kHkldPatchedLen = %du;' % len(data),
        'static const char* kHkldPatchedSha = "%s";' % sha,
        '',
    ]
    io.open(a.out, 'w', encoding='utf-8', newline='\n').write('\n'.join(lines))
    print('已写 %s  (patched=%d B, sha=%s)' % (os.path.relpath(a.out, ROOT), len(data), sha[:16]))
    return 0


if __name__ == '__main__':
    raise SystemExit(main())
