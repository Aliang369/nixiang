#!/usr/bin/env python3
# script-policy: {"purpose": "吾辈小熊猫主玩法 jsc 单点补丁：VersionConfig.IsGmLogin=false→true（内购 GM 分支）", "inputs": "assets/assets/main/index.jsc（XXTEA 密文）+ xxtea_jsc.py", "outputs": "07_patched/吾辈小熊猫/assets/assets/main/index.jsc（同密钥重加密）", "boundary": "只改 1 个布尔字面量；改前必须通过「解密→重加密 == 原文」往返自检，否则拒绝写出", "prior_art": "04_scripts/吾辈小熊猫/xxtea_jsc.py（本作 padding=False 变体）", "difference": "改完用同 key 重加密并反向解密复核；断言 IsGmMenu 保持 !1（不扩张功能面）", "evidence": "10_docs/吾辈小熊猫/功能挂点分析.md §5.2 候选切口 A"}
"""吾辈小熊猫 5.0 主玩法 jsc 补丁：VersionConfig.IsGmLogin false→true。

用法：
    python3 patch_wbxxm_jsc.py [--src <index.jsc>] [--out <index.jsc>] [--check]

安全闸（任一不过就拒绝写出）：
  1) encrypt(decrypt(原密文)) 必须与原文逐字节相同 —— 证明 enc/dec 互逆，改完引擎能读；
  2) IsGmLogin:!1 在明文里恰好出现 1 次；
  3) IsGmMenu:!1 保持不动（GM 菜单面板属额外功能面，不在本次点名范围）；
  4) 写出后反向解密，确认只有目标字面量变化、长度不变。
"""
import argparse
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import xxtea_jsc as X            # noqa: E402

ROOT = os.path.abspath(os.path.join(HERE, '..', '..'))
DEFAULT_SRC = os.path.join(ROOT, '03_decompiled/吾辈小熊猫/js_assets/assets/assets/main/index.jsc')
DEFAULT_OUT = os.path.join(ROOT, '07_patched/吾辈小熊猫/assets/assets/main/index.jsc')

OLD = b'IsGmLogin:!1'
NEW = b'IsGmLogin:!0'


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--src', default=DEFAULT_SRC)
    ap.add_argument('--out', default=DEFAULT_OUT)
    ap.add_argument('--check', action='store_true')
    args = ap.parse_args()

    raw = open(args.src, 'rb').read()
    plain = X.decrypt(raw)

    # 闸 1：往返自检（改之前就证明工具链正确）
    if X.encrypt(plain) != raw:
        sys.exit('ERROR: 往返自检失败（重加密 != 原文），拒绝改包')
    print('[jsc] 往返自检通过：%d B 密文 ↔ %d B 明文' % (len(raw), len(plain)))

    n_old = plain.count(OLD)
    n_new = plain.count(NEW)
    if n_old + n_new == 0:
        sys.exit('ERROR: 明文里找不到 IsGmLogin 字面量（版本换代？）')
    if n_new:
        print('[jsc] 已是 IsGmLogin:!0（跳过改写）')
        patched = plain
    else:
        if n_old != 1:
            sys.exit('ERROR: IsGmLogin:!1 出现 %d 次（期望 1），锚点不唯一，拒绝改' % n_old)
        if args.check:
            print('[jsc] --check：会把 IsGmLogin:!1 → !0（未写出）')
            return
        patched = plain.replace(OLD, NEW, 1)
        print('[jsc] 已改 IsGmLogin:!1 → !0')

    # 闸 3：IsGmMenu 不动
    if patched.count(b'IsGmMenu:!1') != plain.count(b'IsGmMenu:!1'):
        sys.exit('ERROR: IsGmMenu 被意外改动（不在本次范围）')
    if len(patched) != len(plain):
        sys.exit('ERROR: 明文长度变化（%d → %d），字面量替换必须等长' % (len(plain), len(patched)))

    out_bytes = X.encrypt(patched)
    # 闸 4：反向复核
    back = X.decrypt(out_bytes)
    if back != patched:
        sys.exit('ERROR: 写出前反向解密不一致')
    if back.count(NEW) != 1 or back.count(OLD):
        sys.exit('ERROR: 反向复核未看到唯一 IsGmLogin:!0')

    if args.check:
        print('[jsc] --check：产物 %d B（未写出）' % len(out_bytes))
        return

    os.makedirs(os.path.dirname(args.out), exist_ok=True)
    open(args.out, 'wb').write(out_bytes)
    import difflib
    diff = list(difflib.unified_diff(
        plain.decode('utf-8', 'replace').splitlines(),
        patched.decode('utf-8', 'replace').splitlines(), lineterm='', n=0))
    print('[jsc] 写出 %s（%d B，与原密文同长）' % (args.out, len(out_bytes)))
    print('[jsc] 明文差异行数 %d（应只涉及 IsGmLogin 那一处）' % len(diff))


if __name__ == '__main__':
    main()
