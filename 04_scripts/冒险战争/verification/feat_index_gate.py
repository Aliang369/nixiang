#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""feat_index_gate — featureList 下标 ↔ *_FEAT_* 常量 机器门禁（static-verifier）

背景（已实测钉死）：
  NativeMenuController.java:699  for (i...) FeatureListParser.parse(i, specs[i])
  FeatureListParser.Item(index,...)   ⇒ featNum = **0 基数组下标**（Category/SKIP 也占位）
  MainModern.cpp:300                 g_mod->onFeatureChanged(featNum, enabled) 原样透传
  ⇒ 常量必须等于「该 Toggle 在数组中的位置」，不是「第几个功能」。

三种要能扫出的情形：
  1) Category 占位缺失   → 常量整体 -1，点「无敌」发到 KILL 位（实测 b4）
  2) Category 混排在中间 → 其后所有常量需整体 +1
  3) 功能项缺项 / 常量越界 → 常量指向 Category 位或越界

用法：
  PYTHONUTF8=1 python feat_index_gate.py \
     --entry   mod菜单/starry-mod-menu/app/src/main/jni/Games/MxzzEntry.cpp \
     --header  mod菜单/starry-mod-menu/app/src/main/jni/Games/MxzzMod.h \
     --template 04_scripts/冒险战争/mxzz_menu_template.json \
     --prefix  MXZZ_FEAT_
退出码：0 PASS / 1 FAIL / 2 用法或解析错误
"""
import argparse, json, os, re, sys

SKIP_PREFIXES = ("RichWebView_", "RichTextView_", "Collapse_", "LaunchLink_")

def strip_comments(text):
    text = re.sub(r"/\*.*?\*/", "", text, flags=re.S)
    text = re.sub(r"//[^\n]*", "", text)
    return text

def parse_specs(entry_path):
    src = open(entry_path, encoding="utf-8", errors="replace").read()
    m = re.search(r"static\s+const\s+char\s*\*\s*(\w+)\s*\[\s*\]\s*=\s*\{(.*?)\}\s*;", src, re.S)
    if not m:
        raise SystemExit(f"ERROR: 找不到 featureList 数组声明: {entry_path}")
    arr_name, body = m.group(1), strip_comments(m.group(2))
    specs = re.findall(r'"((?:[^"\\]|\\.)*)"', body)
    return arr_name, specs

def classify(spec):
    s = spec.strip()
    if not s:
        return "SKIP", ""
    if s.startswith("Category_"):
        return "CATEGORY", s[len("Category_"):]
    if s.startswith(SKIP_PREFIXES):
        return "SKIP", s
    if s.startswith("Toggle_"):
        return "TOGGLE", s[len("Toggle_"):]
    if s.startswith("CheckBox_"):
        return "CHECKBOX", s[len("CheckBox_"):]
    if s.startswith("Button_"):
        return "BUTTON", s[len("Button_"):]
    if s.startswith("Slider_"):
        return "SEEK", s[len("Slider_"):].split(":")[0]
    if s.startswith("SeekBar_"):
        return "SEEK", s[len("SeekBar_"):].split("_")[0]
    if s.startswith("Spinner_"):
        return "SPINNER", s[len("Spinner_"):].split(":")[0]
    if s.startswith("InputText_"):
        return "INPUT", s[len("InputText_"):]
    if s.startswith("InputValue_"):
        return "INPUT", s[len("InputValue_"):].split(":")[-1]
    return "TOGGLE", s  # 与 FeatureListParser 末尾 default 一致

def parse_consts(header_path, prefix):
    src = strip_comments(open(header_path, encoding="utf-8", errors="replace").read())
    out = {}
    for name, val in re.findall(r"\b(" + re.escape(prefix) + r"\w+)\s*=\s*(\d+)", src):
        out[name] = int(val)
    for name, val in re.findall(r"#define\s+(" + re.escape(prefix) + r"\w+)\s+(\d+)", src):
        out[name] = int(val)
    return out

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--entry", required=True)
    ap.add_argument("--header", required=True)
    ap.add_argument("--template", required=False)
    ap.add_argument("--prefix", default="MXZZ_FEAT_")
    ap.add_argument("--max-features", type=int, default=64, help="MainModern.cpp: g_features[64]")
    a = ap.parse_args()

    arr, specs = parse_specs(a.entry)
    items = [classify(s) for s in specs]
    consts = parse_consts(a.header, a.prefix)
    fails, warns = [], []

    print(f"== {os.path.basename(a.entry)} :: {arr} ({len(specs)} 项) ==")
    for i, (spec, (kind, name)) in enumerate(zip(specs, items)):
        print(f"  [{i}] {kind:<8} {name:<14} -> {spec}")

    if not consts:
        fails.append(f"头文件里没找到 {a.prefix}* 常量: {a.header}")

    # C5 上限
    if len(specs) > a.max_features:
        fails.append(f"C5 功能项 {len(specs)} 超过 native g_features 上限 {a.max_features}")

    # C1 常量必须指向 TOGGLE/CHECKBOX，且不越界
    for cname, idx in sorted(consts.items(), key=lambda kv: kv[1]):
        if idx >= len(specs):
            fails.append(f"C1 {cname}={idx} 越界（数组只有 {len(specs)} 项）")
            continue
        kind, name = items[idx]
        if kind not in ("TOGGLE", "CHECKBOX"):
            fails.append(f"C1 {cname}={idx} 指向 {kind}「{name}」，不是可点击功能项"
                         f"（缺占位/混排均会落到这里）")
            continue
        print(f"  ✓ {cname:<18} = {idx}  ->  {kind}「{name}」")

    # C2 0 号位占位
    min_idx = min(consts.values()) if consts else 0
    if min_idx > 0:
        bad = [(i, items[i]) for i in range(min_idx) if items[i][0] not in ("CATEGORY", "SKIP")]
        if bad:
            fails.append("C2 常量从 %d 起，但 %s 不是占位项"
                         % (min_idx, ", ".join(f"[{i}]{k}「{n}」" for i, (k, n) in bad)))
        else:
            print(f"  ✓ C2 0..{min_idx-1} 均为占位项（Category_/SKIP），符合「featNum=0 基下标」约定")
    else:
        warns.append("C2 无可占位（常量都 >= 1 时才需要占位；当前最小常量 = 0）")

    # C3 中间混入 Category
    for i, (kind, name) in enumerate(items):
        if kind == "CATEGORY" and i >= min_idx:
            drift = sum(1 for j in range(i, len(specs)) if items[j][0] == "CATEGORY")
            fails.append(f"C3 [{i}] 是 Category「{name}」，位于功能项之后 ⇒ 其后常量需整体 +{drift}")

    # C4 每个功能项都被常量覆盖
    used = set(consts.values())
    for i, (kind, name) in enumerate(items):
        if kind in ("TOGGLE", "CHECKBOX") and i not in used:
            fails.append(f"C4 [{i}] {kind}「{name}」没有被任何 {a.prefix}* 常量引用"
                         f"（漏定义或常量错位）")

    # C6 模板对照
    if a.template and os.path.exists(a.template):
        tpl = json.load(open(a.template, encoding="utf-8"))
        tfeats = tpl.get("features") or tpl.get("featureList") or []
        tnames = [f.get("name", "") for f in tfeats if isinstance(f, dict)]
        toggles = [n for k, n in items if k == "TOGGLE"]
        if tnames and len(tnames) != len(toggles):
            fails.append(f"C6 模板 {len(tnames)} 项 vs SO 侧 Toggle {len(toggles)} 项，数量不一致")
        for t in tnames:
            if t and t not in toggles:
                fails.append(f"C6 模板项「{t}」在 featureList 里找不到同名 Toggle")
        for t, f in zip(tnames, tfeats):
            fn = f.get("feat_num")
            if isinstance(fn, int):
                if fn >= len(specs):
                    fails.append(f"C6 模板 feat_num={fn} 越界")
                elif items[fn][1] != t:
                    fails.append(f"C6 模板「{t}」标 feat_num={fn}，但数组 [{fn}] 是 {items[fn][0]}「{items[fn][1]}」")
        print(f"  ✓ C6 已对照模板: {a.template}")

    print()
    for w in warns:
        print("WARN:", w)
    if fails:
        for f in fails:
            print("FAIL:", f)
        print("\nRESULT: FAIL")
        return 1
    print("RESULT: PASS（下标与常量一致，占位/混排/缺项三项均无问题）")
    return 0

if __name__ == "__main__":
    sys.exit(main())
