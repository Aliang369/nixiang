#!/usr/bin/env python3
# script-policy: {"purpose": "吾辈小熊猫 5.0 登录/签名改包 smali 补丁：killer 父类挂载 + TapSDK 门闩恒放行 + toast no-op + 强制网页登录", "inputs": "03_decompiled/吾辈小熊猫/apktool_out/smali_classes2（baksmali 产物）", "outputs": "原地打补丁的 4 个 smali 文件（供 smali assemble 出 classes2.dex）", "boundary": "只改这 4 个文件里的指定方法/两行父类；不动其它类、不动 TapSignKit/SOPackageUtil.MarketP、不打包不签名", "prior_art": "10_docs/吾辈小熊猫/登录改包方案.md（锚点逐字来源）", "difference": "4.10.4 新锚点：InitializerServiceImpl 在 initializer/service/；无 LoginActivityImpl/handlers 包；门闩与两处 toast 一并打", "evidence": "10_docs/吾辈小熊猫/登录改包方案.md#b4"}
"""吾辈小熊猫 5.0（com.wbxxm.wbxxm，TapTap）smali 改包补丁 —— 幂等。

用法：
    python3 patch_wbxxm_smali.py --root 03_decompiled/吾辈小熊猫/apktool_out [--check]

改动（逐条对应 10_docs/吾辈小熊猫/登录改包方案.md）：
  d.2  com/xipu/msdk/XiPuSDKApplication.smali
         .super Landroid/app/Application;  → Lbin/mt/signature/KillerApplication193;
         <init> invoke-direct 同改（父类 <clinit> 必须先于子类，killer 才能早装钩）
  b.1  GatekeeperRepository.isGatekeeperLoadedSuccessfully()Z → const/4 v0,0x1 / return v0
  b.2  GatekeeperRepository.showErrorToast(TapNetworkException)V → 首行 return-void（保留 .registers 5）
  b.3  GatekeeperRepository.showGatekeeperError()V → 首行 return-void（保留 .registers 5）
  b.4  InitializerServiceImpl.checkInitialize(Z)Z → 首行 const/4 v1,0x1 / return v1
  c.1  ClientLoginUtils.isClientLogin(Activity,Actions)Z → const/4 p1,0x0 / return p1
"""
import argparse
import os
import sys

FAIL = []

KEEP_PREFIX = ('.registers', '.locals', '.param', '.end param',
               '.annotation', '.end annotation')
DROP_PREFIX = ('.line', '.prologue', '.local', '.end local', '.restart local',
               '.source', '#')


def fail(msg):
    FAIL.append(msg)
    print('  FAIL ' + msg)


def read(path):
    with open(path, 'r', encoding='utf-8') as f:
        return f.read()


def write(path, text):
    with open(path, 'w', encoding='utf-8') as f:
        f.write(text)


def method_span(text, header):
    start = text.find(header)
    if start < 0:
        return None
    end = text.find('.end method', start)
    if end < 0:
        return None
    return start, end + len('.end method'), header


def body_lines(text, header):
    span = method_span(text, header)
    if span is None:
        return None
    seg = text[span[0]:span[1]]
    # 去掉 .method 那一行与 .end method 那一行
    lines = seg.split('\n')[1:]
    if lines and lines[-1].startswith('.end method'):
        lines = lines[:-1]
    else:
        lines = [l for l in lines if not l.startswith('.end method')]
    return lines


def instrs_of(lines):
    """方法体里的真指令（去掉伪指令/空行）。"""
    return [l.strip() for l in lines
            if l.strip() and not l.strip().startswith('.')]


def replace_body(text, header, instrs, label):
    span = method_span(text, header)
    if span is None:
        fail('找不到方法: %s' % header)
        return text
    start, end, _ = span
    block = text[start:end]
    lines = block.split('\n')
    out = [lines[0]]
    for ln in lines[1:]:
        s = ln.strip()
        if s == '.end method':
            break
        if s.startswith(DROP_PREFIX):
            continue
        if s.startswith(KEEP_PREFIX) or s == '':
            out.append(ln)
            continue
        break  # 第一条真指令：方法体到此为止
    while out and out[-1].strip() == '':
        out.pop()
    out.extend('    ' + i for i in instrs)
    out.extend(['', '.end method'])
    print('  ok  %s' % label)
    return text[:start] + '\n'.join(out) + text[end:]


def insert_first_line(text, header, instrs, label):
    span = method_span(text, header)
    if span is None:
        fail('找不到方法: %s' % header)
        return text
    start, end, _ = span
    block = text[start:end]
    idx = block.find('.registers')
    if idx < 0:
        idx = block.find('.locals')
    if idx < 0:
        fail('%s 无 .registers/.locals' % label)
        return text
    eol = block.find('\n', idx)
    head, tail = block[:eol + 1], block[eol + 1:]
    ins = ''.join('    %s\n\n' % i for i in instrs)
    print('  ok  %s（首行插入 %d 条，原体保留）' % (label, len(instrs)))
    return text[:start] + head + ins + tail + text[end:]


APP_REL = 'smali_classes2/com/xipu/msdk/XiPuSDKApplication.smali'
GK_REL = 'smali_classes2/com/taptap/sdk/initializer/repository/GatekeeperRepository.smali'
INI_REL = 'smali_classes2/com/taptap/sdk/initializer/service/InitializerServiceImpl.smali'
CLU_REL = 'smali_classes2/com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali'

H_GK = '.method public final isGatekeeperLoadedSuccessfully()Z'
H_TOAST = '.method private final showErrorToast(Lcom/taptap/sdk/common/network/throwable/TapNetworkException;)V'
H_GKERR = '.method public final showGatekeeperError()V'
H_CHK = '.method public checkInitialize(Z)Z'
H_CLU = '.method public final isClientLogin(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;)Z'


def patch_app(root, check):
    p = os.path.join(root, APP_REL)
    t = read(p)
    old_super = '.super Landroid/app/Application;'
    new_super = '.super Lbin/mt/signature/KillerApplication193;'
    old_init = 'invoke-direct {p0}, Landroid/app/Application;-><init>()V'
    new_init = 'invoke-direct {p0}, Lbin/mt/signature/KillerApplication193;-><init>()V'
    if new_super in t and new_init in t:
        print('  skip XiPuSDKApplication（已打）')
        return
    if old_super not in t:
        fail('XiPuSDKApplication 无 %s（底包换代？）' % old_super)
        return
    if t.count(old_init) != 1:
        fail('XiPuSDKApplication <init> invoke-direct 出现 %d 次（期望 1）' % t.count(old_init))
        return
    if check:
        print('  TODO XiPuSDKApplication（--check 不写入）')
        return
    t = t.replace(old_super, new_super, 1).replace(old_init, new_init, 1)
    write(p, t)
    print('  ok  XiPuSDKApplication .super → killer（L2/L21）')


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--root', required=True)
    ap.add_argument('--check', action='store_true')
    args = ap.parse_args()
    root = args.root

    print('[1/6] XiPuSDKApplication 父类 → killer')
    patch_app(root, args.check)

    print('[2/6] GatekeeperRepository.isGatekeeperLoadedSuccessfully → true')
    gk = os.path.join(root, GK_REL)
    t = read(gk)
    bl = body_lines(t, H_GK)
    if bl is None:
        fail('找不到 %s' % H_GK)
    elif instrs_of(bl) == ['const/4 v0, 0x1', 'return v0']:
        print('  skip（已打）')
    elif args.check:
        print('  TODO isGatekeeperLoadedSuccessfully（原体 %r）' % instrs_of(bl))
    else:
        t = replace_body(t, H_GK, ['const/4 v0, 0x1', '', 'return v0'],
                         'isGatekeeperLoadedSuccessfully()Z → true')
        write(gk, t)

    print('[3/6] showErrorToast / showGatekeeperError → 首行 return-void')
    t = read(gk)
    for header, label in ((H_TOAST, 'showErrorToast(TapNetworkException)V'),
                          (H_GKERR, 'showGatekeeperError()V')):
        bl = body_lines(t, header)
        if bl is None:
            fail('找不到 %s' % header)
            continue
        ins = instrs_of(bl)
        if ins[:1] == ['return-void']:
            print('  skip %s（已打）' % label)
            continue
        if args.check:
            print('  TODO %s' % label)
            continue
        t = insert_first_line(t, header, ['return-void'], label)
    if not args.check:
        write(gk, t)

    print('[4/6] InitializerServiceImpl.checkInitialize(Z)Z → true')
    ini = os.path.join(root, INI_REL)
    t = read(ini)
    bl = body_lines(t, H_CHK)
    if bl is None:
        fail('找不到 %s' % H_CHK)
    elif instrs_of(bl)[:2] == ['const/4 v1, 0x1', 'return v1']:
        print('  skip（已打）')
    elif args.check:
        print('  TODO checkInitialize(Z)Z')
    else:
        t = insert_first_line(t, H_CHK, ['const/4 v1, 0x1', '', 'return v1'],
                              'checkInitialize(Z)Z → true')
        write(ini, t)

    print('[5/6] ClientLoginUtils.isClientLogin → false（强制网页登录）')
    clu = os.path.join(root, CLU_REL)
    t = read(clu)
    bl = body_lines(t, H_CLU)
    if bl is None:
        fail('找不到 %s' % H_CLU)
    elif instrs_of(bl) == ['const/4 p1, 0x0', 'return p1']:
        print('  skip（已打）')
    elif args.check:
        print('  TODO isClientLogin（原体 %r）' % instrs_of(bl))
    else:
        t = replace_body(t, H_CLU, ['const/4 p1, 0x0', '', 'return p1'],
                         'isClientLogin(Activity,Actions)Z → false')
        write(clu, t)

    print('[6/6] 复读断言')
    t = read(os.path.join(root, APP_REL))
    if '.super Lbin/mt/signature/KillerApplication193;' not in t:
        fail('XiPuSDKApplication .super 未生效')
    if 'invoke-direct {p0}, Lbin/mt/signature/KillerApplication193;-><init>()V' not in t:
        fail('XiPuSDKApplication <init> 未生效')

    gk_t = read(gk)
    if instrs_of(body_lines(gk_t, H_GK) or []) != ['const/4 v0, 0x1', 'return v0']:
        fail('isGatekeeperLoadedSuccessfully 未恒 true')
    for header, label in ((H_TOAST, 'showErrorToast'), (H_GKERR, 'showGatekeeperError')):
        bl = body_lines(gk_t, header) or []
        if instrs_of(bl)[:1] != ['return-void']:
            fail('%s 首条指令不是 return-void' % label)
        seg = gk_t[method_span(gk_t, header)[0]:method_span(gk_t, header)[1]]
        if '.registers 5' not in seg:
            fail('%s .registers 5 被改动' % label)

    ini_t = read(ini)
    if instrs_of(body_lines(ini_t, H_CHK) or [])[:2] != ['const/4 v1, 0x1', 'return v1']:
        fail('checkInitialize 未恒 true')

    clu_t = read(clu)
    if instrs_of(body_lines(clu_t, H_CLU) or []) != ['const/4 p1, 0x0', 'return p1']:
        fail('isClientLogin 未恒 false')

    if FAIL:
        print('\n[FAIL] %d 项' % len(FAIL))
        sys.exit(1)
    print('\n[OK] 6 处补丁到位（killer 父类 / 门闩 / 两处 toast / checkInitialize / 网页登录）')


if __name__ == '__main__':
    main()
