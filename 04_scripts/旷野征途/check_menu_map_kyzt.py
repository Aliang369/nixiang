#!/usr/bin/env python3
# 旷野征途（kyzt）菜单映射静态回归检查
#
# 为什么需要它：菜单 featureList 的**下标就是 featNum**（MainModern 用 i 直传），
# 一旦为了隐藏功能而删项，后面所有条目会整体前移 ⇒ 点「游戏加速」实际触发「金币」。
# 2026-11-14 曾真实发生过（只留 3 项的写法把 Toggle_游戏加速 放到了 index 1）。
# 因此本检查不查「so 里有没有某个字符串」，而是把
#   Entry.cpp 的数组 → Java 解析规则 → native enum 值 三段串起来做一致性断言。
#
# 用法: python3 04_scripts/旷野征途/check_menu_map_kyzt.py
# 退出码: 0=一致 / 1=不一致
import re, sys, pathlib

ROOT = pathlib.Path(__file__).resolve().parents[2]
JNI = ROOT / "mod菜单/starry-mod-menu/app/src/main/jni"
ENTRY = JNI / "Games/旷野征途Entry.cpp"
MODH = JNI / "Games/旷野征途Mod.h"
JAVA = ROOT / "mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/bridge/ModBridge.java"
MK = JNI / "Android_kyzt.mk"

ECON_TAG = "［2.2待验证］"
BATTLE_TAG = "［2.2待验证］"

def read(p):
    return p.read_text(encoding="utf-8")

def entry_list(src):
    m = re.search(r"k旷野征途_Features\[\]\s*=\s*\{(.*?)\};", src, re.S)
    if not m:
        sys.exit("FAIL 找不到 k旷野征途_Features 数组")
    body = m.group(1)
    items = []
    for line in body.splitlines():
        line = line.split("//")[0].strip()
        if not line.startswith('"'):
            continue
        lit = re.match(r'"([^"]*)"(?:\s*([A-Z_]+))?', line)
        if not lit:
            continue
        text, macro = lit.group(1), lit.group(2)
        if macro == "KYZT_ECON_TAG":
            text += ECON_TAG
        elif macro == "KYZT_BATTLE_TAG":
            text += BATTLE_TAG
        items.append(text)
    return items

def enum_map(src):
    out = {}
    for name, val in re.findall(r"(旷野征途_FEAT_[A-Z]+)\s*=\s*(\d+)", src):
        out[int(val)] = name
    return out

def java_rules(src):
    return {
        "category_prefix": 'startsWith("Category_")' in src,
        "toggle_prefix": 'startsWith("Toggle_")' in src,
        "button_prefix": 'startsWith("Button_")' in src,
        "index_as_featnum": "nativeSetFeature(i, spec, on)" in src,
    }

def main():
    items = entry_list(read(ENTRY))
    enums = enum_map(read(MODH))
    jr = java_rules(read(JAVA))
    mk = read(MK)
    # 「资源已适配」的判据是 Entry 的标签宏 KYZT_ECONOMY22_READY（mk 里的 EXPECTED 只代表
    # 适配层参与编译，不代表功能可交付）—— 两者不可混用，否则会把「编译进去」当「已完成」。
    ready = re.search(r"-DKYZT_ECONOMY22_READY=1", mk) is not None

    ok = True
    def chk(cond, msg):
        nonlocal ok
        print(("PASS " if cond else "FAIL ") + msg)
        ok = ok and cond

    chk(len(items) == 9, "featureList 长度 = 9（实际 %d）" % len(items))
    chk(jr["index_as_featnum"], "Java 以数组下标作 featNum 传给 native")
    chk(jr["category_prefix"] and jr["toggle_prefix"] and jr["button_prefix"],
        "Java 前缀解析规则（Category_/Toggle_/Button_）齐备")

    cats = [i for i, s in enumerate(items) if s.startswith("Category_")]
    chk(cats == [0, 4], "Category 只占 0/4（实际 %s）" % cats)

    func_idx = [i for i, s in enumerate(items) if not s.startswith("Category_")]
    chk(func_idx == [1, 2, 3, 5, 6, 7, 8], "功能位下标 = 1/2/3/5/6/7/8（实际 %s）" % func_idx)
    chk(sorted(enums) == func_idx, "native enum 与功能位一致（enum=%s）" % sorted(enums))

    expect_kind = {1: "Button_", 2: "Button_", 3: "Button_",
                   5: "Toggle_", 6: "Toggle_", 7: "Toggle_", 8: "Toggle_"}
    for i, kind in expect_kind.items():
        chk(items[i].startswith(kind), "index %d 类型 %s（实际 %s）" % (i, kind, items[i][:16]))

    # 三类标签语义必须分开：
    #   资源 1/2/3 = ［2.2待验证］（已实现未上机）
    #   战斗 6/7   = ［2.2未适配］（解释器入口未定位）
    #   5/8        = 无标签（已实现，同样未上机，但属老功能复刻）
    pending = {i for i, s in enumerate(items) if "［2.2待验证］" in s}
    unadapted = {i for i, s in enumerate(items) if "［2.2未适配］" in s}
    chk(pending == {1, 2, 3, 6, 7}, "「待验证」集合 = {1,2,3,6,7}（实际 %s）" % sorted(pending))
    chk(unadapted == set(), "不再有「未适配」项（实际 %s）" % sorted(unadapted))
    chk(not any("［" in items[i] for i in (5, 8)), "5/8 不带适配标签（实际 %s / %s）" % (items[5], items[8]))

    print()
    for i, s in enumerate(items):
        print("  [%d] %s" % (i, s))
    print()
    print("结论：", "一致" if ok else "不一致")
    return 0 if ok else 1

sys.exit(main())
