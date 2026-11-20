#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""mxzz 功能下标一致性门禁（b6 起随出包必跑）。

问题背景：Java 侧 featNum = featureList 数组下标（0 基，见 NativeMenuController.java:699
FeatureListParser.parse(i, specs[i]) → new Item(index, …)），MainModern.cpp:290 原样透传。
所以 native 侧 MXZZ_FEAT_* 常量必须等于「该 Toggle 在列表里跳过 Category_/非 Toggle 项后的 1 基序号」。
b5 之前缺占位项 ⇒ 秒杀=0/无敌=1，开无敌反而秒杀。
"""
import re, sys, pathlib

ROOT = pathlib.Path(__file__).resolve().parents[2]
ENTRY = ROOT / "mod菜单/starry-mod-menu/app/src/main/jni/Games/MxzzEntry.cpp"
HEADER = ROOT / "mod菜单/starry-mod-menu/app/src/main/jni/Games/MxzzMod.h"

# Toggle 中文名 -> 常量后缀（新增功能时同步维护）
TOGGLE_TO_CONST = {"秒杀": "KILL", "无敌": "GOD"}


def read(p):
    return p.read_text(encoding="utf-8", errors="ignore")


def features(text):
    m = re.search(r"kMxzz_Features\[\]\s*=\s*\{(.*?)\};", text, re.S)
    if not m:
        sys.exit("FAIL: 找不到 kMxzz_Features[]")
    body = m.group(1)
    body = re.sub(r"//[^\n]*", "", body)
    return re.findall(r'"([^"]+)"', body)


def consts(text):
    out = {}
    for name, val in re.findall(r"MXZZ_FEAT_([A-Z0-9_]+)\s*=\s*(\d+)", text):
        out[name] = int(val)
    return out


def main():
    feats = features(read(ENTRY))
    consts_ = consts(read(HEADER))
    toggles = [f for f in feats if f.startswith("Toggle_")]
    problems = []
    if not toggles:
        problems.append("featureList 里没有任何 Toggle_ 项")
    for i, name in enumerate(toggles, start=1):     # 1 基序号 = Java featNum
        cn = name[len("Toggle_"):]
        suffix = TOGGLE_TO_CONST.get(cn)
        if not suffix:
            problems.append(f"{name}: 未在 TOGGLE_TO_CONST 登记，无法核对常量（新增功能请同步维护）")
            continue
        if suffix not in consts_:
            problems.append(f"{name}: MxzzMod.h 里没有 MXZZ_FEAT_{suffix}")
            continue
        if consts_[suffix] != i:
            problems.append(
                f"{name}: Java 会发 featNum={i}，但 MXZZ_FEAT_{suffix}={consts_[suffix]} ⇒ 错位"
            )
    # 占位检查：第 0 项必须是 Category_*（否则 0 基下标整体左移 1）
    if not feats or not feats[0].startswith("Category_"):
        problems.append(
            f'featureList[0] = "{feats[0] if feats else ""}"，必须以 Category_* 占位（Java 侧 0 基下标）'
        )
    if problems:
        print("FAIL [功能下标] mxzz featureList 与 MXZZ_FEAT_* 不一致：")
        for p in problems:
            print("   -", p)
        print("   参考：HkldEntry.cpp 首项 \"Category_功能\" // 0，其 Toggle_无敌 在 1")
        sys.exit(1)
    print(f"PASS [功能下标] {len(toggles)} 项 Toggle 与 MXZZ_FEAT_* 对齐：" +
          ", ".join(f"{t}->{i}" for i, t in enumerate(toggles, start=1)))


if __name__ == "__main__":
    main()
