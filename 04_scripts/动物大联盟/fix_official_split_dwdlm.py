#!/usr/bin/env python3
"""动物大联盟 — 修正 official_apk.json 的 split 口径（幂等，不重拉底包）。

背景（2026-11-05 实测）：
  dumpsys package com.fightingstars.games.animal 对非 split 安装也会打印
  splits=[base]（base 是占位符，不是真正的 split）。fetch_official_dwdlm.sh 旧逻辑
  直接拿 splits 字段非空 × pm 路径数判单 base，于是写出 is_single_base=false，
  与 aapt 侧 official_check.json（split_entries=[] / is_single_base=true）自相矛盾。

修正口径：
  split 名 = splits 字段去掉空串与 base 占位符后的余项；
  is_single_base = (pm_path_count == 1) and (split 名为空)。
  原始观测值一个都不覆盖，另存到 split.observed 下。
"""
import json
import sys
from pathlib import Path

PATH = Path(__file__).resolve().parent / "official_apk.json"


def normalize_splits(raw):
    return [s.strip() for s in raw if s.strip().lower() not in ("", "base")]


def main():
    if not PATH.exists():
        print("ERROR: 缺 %s" % PATH, file=sys.stderr)
        return 1
    data = json.loads(PATH.read_text(encoding="utf-8"))
    split = data.get("split") or {}
    raw_field = split.get("splits_field", [])
    if isinstance(raw_field, str):
        raw_field = [p for p in raw_field.split(",")]
    path_count = int(split.get("pm_path_count", 0) or 0)
    named = normalize_splits(raw_field)
    observed = dict(split.get("observed") or {})
    if "splits_field_raw" not in observed:
        observed["splits_field_raw"] = split.get("splits_field")
    if "pm_path_count_raw" not in observed:
        observed["pm_path_count_raw"] = split.get("pm_path_count")
    if "is_single_base_raw" not in observed:
        observed["is_single_base_raw"] = split.get("is_single_base")

    new_split = {
        "pm_path_count": path_count,
        "splits_field": split.get("splits_field"),
        "splits_normalized": named,
        "is_single_base": bool(path_count == 1 and not named),
        "rule": "dumpsys 对非 split 安装同样打印 splits=[base]；判单 base 必须先剔除 base 占位符",
        "observed": observed,
    }
    changed = new_split != split
    data["split"] = new_split
    PATH.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print("[split] pm_path_count=%d splits_field=%s -> normalized=%s is_single_base=%s (%s)"
          % (path_count, split.get("splits_field"), named, new_split["is_single_base"],
             "rewritten" if changed else "unchanged"))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
