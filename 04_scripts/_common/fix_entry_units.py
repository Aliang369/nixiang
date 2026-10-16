#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""FW-22 一次性单位修正：把 Games/*Entry.cpp 里按「毫秒」误写的 usleep 字段值 ×1000。

判据（写死在脚本里，便于复查）：
  · MainModern.cpp:230/238/248/262 一律走 usleep(...) ⇒ 字段单位是**微秒**
  · 框架默认 tickIntervalMs=200000（=200ms）⇒ 反证小值即误写
  · 阈值 <10000 视为误写（<10ms 的 tick 不可能是本意）；>=10000 视为已是微秒，不动
  · 0 不动（0 = 立即/无延迟，两种单位语义相同）

只替换数字字面量，保留原行其它字符（含对齐空格与行尾注释）。
"""
import re
import sys
import pathlib

THRESH = 10000
FIELDS = ("initDelayMs", "initRetryIntervalMs", "tickIntervalMs")
PAT = re.compile(r"(\.(?:" + "|".join(FIELDS) + r")[ \t]*=[ \t]*)(\d+)(,)")

# __file__ = <repo>/04_scripts/_common/fix_entry_units.py ⇒ parents[2] 才是仓库根
GAMES = pathlib.Path(__file__).resolve().parents[2] / "mod菜单/starry-mod-menu/app/src/main/jni/Games"

def main(apply: bool, check: bool) -> int:
    hits = []
    for f in sorted(GAMES.glob("*Entry.cpp")):
        text = f.read_text(encoding="utf-8")
        out, n = [], 0
        for line in text.splitlines(keepends=True):
            m = PAT.search(line)
            if m:
                v = int(m.group(2))
                if 0 < v < THRESH:
                    new = m.group(1) + str(v * 1000) + m.group(3)
                    line = line[:m.start()] + new + line[m.end():]
                    n += 1
                    hits.append((f.name, m.group(1).strip(), v, v * 1000))
            out.append(line)
        if n and apply:
            f.write_text("".join(out), encoding="utf-8")
    for name, field, old, new in hits:
        flag = "  ← 歧义（≥5000，可能是 5ms 而非 5s）" if field == "tickIntervalMs" and old >= 5000 else ""
        print(f"  {name:32s} {field:20s} {old:>6d} -> {new:<9d}{flag}")
    mode = "APPLY" if apply else ("CHECK" if check else "dry-run")
    print(f"\n共 {len(hits)} 处，涉及 {len({h[0] for h in hits})} 个文件；模式={mode}")
    if check and hits:
        print("✗ FW-22：仍有按毫秒误写的 usleep 字段值 ⇒ 跑 `--apply` 修正（或确认是刻意的微秒小值）")
        return 1
    return 0

if __name__ == "__main__":
    sys.exit(main("--apply" in sys.argv, "--check" in sys.argv))
