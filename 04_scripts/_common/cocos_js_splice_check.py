#!/usr/bin/env python3.11
"""cocos_js_splice_check.py — 通用「离线复刻 C++ JS 拼接规则」核对器。

为什么有它（2026-10-15 山海幻境 4.3 复盘）：
  starry 框架的 cocos 类 hook 都是「在 se::evalString 里对主 JS 做字符串替换」。
  换版本时唯一要证的静态命题是「这些 from/to 在本版 JS 里仍然逐字命中」。
  4.0/4.2 各写一份一次性脚本、收尾删掉，4.3 又写一遍 ⇒ 规则抽成 JSON 数据，
  本器只做执行：读 JS + 读锚点 JSON → 逐条 ReplaceAll → 报命中 + node --check。

用法:
  python3.11 04_scripts/_common/cocos_js_splice_check.py <解密后的 js> \
      --anchors 10_docs/{游戏}/{game}_js_anchors.json [--out <拼接后 js>] [--quiet]

锚点 JSON 协议（framework: starry-cocos-splice/v1）
  {
    "game":       "山海幻境",
    "entry":      "assets/main/index.js",
    "source_of_truth": "Games/ShhjMod.cpp:179-250 (SpliceCombatJs)",
    "stats_needles": [["标签", "要统计的原文锚点"], ...],
    "preamble":   {"marker": "...", "anchor": "window.__require", "text": "..."},
    "rules": [
      {"label": "getHurt dmg5",
       "guard_absent":  ["window.GX5("],          # 出现则整条跳过（对应 C++ 的 if）
       "guard_present": ["this.view.isBoss"],     # 未出现则整条跳过
       "pairs": [{"tag": "compact", "from": "...", "to": "..."},
                 {"tag": "spaced",  "from": "...", "to": "..."}]},
      ...
    ],
    "required": ["preamble", "getHurt dmg5", "curHp", "tHurt inv"]   # 缺一即 rc=3
  }

退出码: 0=全部必需命中；2=用法/文件错；3=必需锚点缺失；4=node --check 失败
"""
from __future__ import annotations

import argparse
import json
import subprocess
import sys
from pathlib import Path


def replace_all(s: str, frm: str, to: str) -> tuple[str, int]:
    n = s.count(frm)
    return (s.replace(frm, to) if n else s), n


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("js", type=Path)
    ap.add_argument("--anchors", type=Path, required=True)
    ap.add_argument("--out", type=Path, default=None)
    ap.add_argument("--quiet", action="store_true")
    a = ap.parse_args()

    if not a.js.is_file():
        print(f"ERROR: JS 不存在: {a.js}", file=sys.stderr)
        return 2
    if not a.anchors.is_file():
        print(f"ERROR: 锚点 JSON 不存在: {a.anchors}", file=sys.stderr)
        return 2
    spec = json.loads(a.anchors.read_text(encoding="utf-8"))
    if spec.get("framework") not in (None, "starry-cocos-splice/v1"):
        print(f"ERROR: 未知锚点协议: {spec.get('framework')}", file=sys.stderr)
        return 2

    s = a.js.read_text(encoding="utf-8", errors="replace")
    orig_len = len(s)
    lines = s.splitlines()
    print(f"  [锚点表] {a.anchors}（{spec.get('game','?')} · 出处 {spec.get('source_of_truth','?')}）")

    for label, needle in spec.get("stats_needles", []):
        cnt = s.count(needle)
        first = next((i + 1 for i, ln in enumerate(lines) if needle in ln), 0)
        if not a.quiet:
            print(f"  [原文] {label}: 命中 {cnt} 处" + (f"，首行 {first}" if first else ""))

    hits: list[str] = []
    hit_labels: set[str] = set()

    pre = spec.get("preamble")
    if pre:
        if pre["marker"] in s:
            hit_labels.add("preamble")
        elif pre["anchor"] in s:
            p = s.find(pre["anchor"])
            s = s[:p] + pre["text"] + s[p:]
            hits.append(pre.get("log", "spliced preamble"))
            hit_labels.add("preamble")
        else:
            print(f"  MISS preamble 锚点 {pre['anchor']!r} 未出现", file=sys.stderr)

    for rule in spec.get("rules", []):
        label = rule["label"]
        if any(g in s for g in rule.get("guard_absent", [])):
            continue
        if rule.get("guard_present") and not all(g in s for g in rule["guard_present"]):
            continue
        for pair in rule.get("pairs", []):
            s, n = replace_all(s, pair["from"], pair["to"])
            if n:
                hits.append(f"{pair.get('log', rule.get('log', label))} ×{n}")
                hit_labels.add(label)

    print(f"  原文 {orig_len} B → 拼接后 {len(s)} B（Δ{len(s) - orig_len:+d}）")
    for h in hits:
        print(f"  HIT  {h}")

    missing = [r for r in spec.get("required", []) if r not in hit_labels]
    if missing:
        print(f"  MISS 必需锚点缺失: {', '.join(missing)}", file=sys.stderr)
        return 3

    if a.out:
        a.out.parent.mkdir(parents=True, exist_ok=True)
        a.out.write_text(s, encoding="utf-8")
        print(f"  拼接结果 → {a.out}")
        node = subprocess.run(["node", "--check", str(a.out)], capture_output=True, text=True)
        if node.returncode != 0:
            print(f"  node --check FAIL: {node.stderr.strip()[:400]}", file=sys.stderr)
            return 4
        print("  node --check PASS")

    print("  期望运行时日志（装机后 logcat 应出现的行）：")
    for h in hits:
        print(f"    Mod_Menu: [Shhj] {h.split(' ×')[0]}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
