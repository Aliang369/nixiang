# -*- coding: utf-8 -*-
"""菜单 feat 号 ↔ 枚举 ↔ 回调 一致性校验（无限升级 11 线，2026-11-04）

为什么要有这个：feat 号 = featureList 的**下标**，插一行就整体位移。
2026-11-04 用户要求「开关放到对应输入框上面」⇒ 必须重排，手改号极易错位
（开关拨了却改到别人的键）。本脚本把三处对齐关系机器校验：

  ① Entry.cpp 数组下标  ==  Mod.h 枚举值（逐项）
  ② 每个菜单项 → 预期绑定的枚举名（映射表写死，防「顺序对了但绑错功能」）
  ③ 每个枚举都有对应的 case 分支（OnFeatureChanged / OnValueChanged / OnInputChanged）

用法：python 04_scripts/无限升级/check_feat_order.py
"""
import io
import os
import re
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
JNI = os.path.join(ROOT, 'mod菜单', 'starry-mod-menu', 'app', 'src', 'main', 'jni', 'Games')
ENTRY = os.path.join(JNI, 'WxsjVc1637Entry.cpp')
HDR = os.path.join(JNI, 'WxsjVc1637Mod.h')
CPP = os.path.join(JNI, 'WxsjVc1637Mod.cpp')

# 菜单项 -> 必须绑定的枚举名（去掉 WXSJVC_FEAT_ 前缀）
EXPECT = {
    'Toggle_无敌': 'GOD',
    'Toggle_秒杀': 'ONESHOT',
    'Toggle_免广告': 'FREEADS',
    'Toggle_物品不减': 'ITEMKEEP',
    'Toggle_技能免费升级': 'SKILLFREE',
    'Toggle_广告奖励倍数生效': 'ADSREWARD_ON',
    'InputValue_1:广告奖励倍数': 'ADSREWARD',
    'Toggle_物品倍数生效': 'ITEMDOUBLE_ON',
    'InputValue_1:物品倍数': 'ITEMDOUBLE',
    'Toggle_无限挑战秘境': 'MIJINGFREE',
    'Toggle_皮肤全解锁': 'SKINUNLOCK',
    'Toggle_功能全解锁': 'FUNCUNLOCK',
    'Toggle_经验倍数生效': 'EXPMULTI_ON',
    'InputValue_1:经验倍数': 'EXPMULTI',
    'Toggle_商店广告无限': 'SHOPADS',
    'Toggle_物品使用反增': 'ITEMREVERSE',
    'InputValue_1000:物品反增倍率': 'ITEMREVERSEX',
    'Toggle_防封': 'ANTIBAN',
    'Toggle_转生时间无限制': 'ZHUANSHENG',
    'Slider_技能投掷物倍数:1:100': 'PROJECTILE',
    'Slider_技能伤害倍数:1:1000': 'SKILLDMG',
    'Toggle_转生无条件升级': 'LEGACYFREE',
}


def flavor_lists(path, pattern, start_mark, end_mark):
    """返回 {'screen': [(idx, val)], 'normal': [...]}；#ifdef WXSJ_ANTIBAN_MENU 段只进 normal。"""
    lines = io.open(path, 'r', encoding='utf-8').read().split('\n')
    out = {'screen': [], 'normal': []}
    mode = None
    inside = False
    for ln in lines:
        if start_mark in ln:
            inside = True
            continue
        if not inside:
            continue
        if end_mark and end_mark in ln:
            break
        s = ln.strip()
        if s.startswith('#ifdef WXSJ_ANTIBAN_MENU'):
            mode = 'normal'
            continue
        if s.startswith('#else'):
            mode = 'screen'
            continue
        if s.startswith('#endif'):
            mode = None
            continue
        m = re.search(pattern, ln)
        if not m:
            continue
        val = m.group(1)
        if mode is None:
            out['screen'].append(val)
            out['normal'].append(val)
        else:
            out[mode].append(val)
    return out


def enum_maps(path):
    lines = io.open(path, 'r', encoding='utf-8').read().split('\n')
    out = {'screen': {}, 'normal': {}}
    mode = None
    inside = False
    for ln in lines:
        if 'enum WxsjVc1637Feat' in ln:
            inside = True
            continue
        if not inside:
            continue
        if ln.strip().startswith('};'):
            break
        s = ln.strip()
        if s.startswith('#ifdef WXSJ_ANTIBAN_MENU'):
            mode = 'normal'
            continue
        if s.startswith('#else'):
            mode = 'screen'
            continue
        if s.startswith('#endif'):
            mode = None
            continue
        m = re.search(r'(WXSJVC_FEAT_\w+)\s*=\s*(\d+)', ln)
        if not m:
            continue
        name = m.group(1)[len('WXSJVC_FEAT_'):]
        num = int(m.group(2))
        if mode is None:
            out['screen'][name] = num
            out['normal'][name] = num
        else:
            out[mode][name] = num
    return out


def main():
    specs = flavor_lists(ENTRY, r'"([^"]+)"', 'kWxsjVc1637_Features[]', '};')
    enums = enum_maps(HDR)
    cpp = io.open(CPP, 'r', encoding='utf-8').read()
    cases = set(re.findall(r'case\s+WXSJVC_FEAT_(\w+)\s*:', cpp))

    bad = []
    for fl in ('screen', 'normal'):
        sl = specs[fl]
        em = enums[fl]
        for i, spec in enumerate(sl):
            if spec.startswith('Category_'):
                continue
            exp = EXPECT.get(spec)
            if exp is None:
                bad.append('[%s] 未知菜单项 %r（没在 EXPECT 表里登记）' % (fl, spec))
                continue
            if exp not in em:
                bad.append('[%s] 枚举缺 WXSJVC_FEAT_%s' % (fl, exp))
                continue
            if em[exp] != i:
                bad.append('[%s] 错位：%r 在下标 %d，但 WXSJVC_FEAT_%s=%d'
                           % (fl, spec, i, exp, em[exp]))
            if exp not in cases:
                bad.append('[%s] 没有回调分支：case WXSJVC_FEAT_%s 缺失（功能会哑）' % (fl, exp))
        # 枚举里多出来的项（有号但没菜单项）
        for name, num in em.items():
            if num >= len(sl):
                bad.append('[%s] 枚举 WXSJVC_FEAT_%s=%d 超出菜单长度 %d' % (fl, name, num, len(sl)))

    print('菜单长度：屏蔽版 %d / 正常版 %d' % (len(specs['screen']), len(specs['normal'])))
    if bad:
        print('FAIL %d 项：' % len(bad))
        for b in bad:
            print('  - ' + b)
        return 1
    print('PASS feat 号 / 枚举 / 回调 三者一致（%d 个菜单项全对齐）' % len(specs['normal']))
    return 0


if __name__ == '__main__':
    sys.exit(main())
