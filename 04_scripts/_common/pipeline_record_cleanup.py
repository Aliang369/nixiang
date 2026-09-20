#!/usr/bin/env python3
"""Record already-performed cleanup in a game's pipeline.json.

The cleanup step may delete build outputs before archive.  This helper keeps
the pipeline snapshot consistent by changing matching verify rules to
not_exists and removing matching artifact/input entries.
"""

from __future__ import annotations

import argparse
import fnmatch
import json
import os
import sys
import tempfile
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]


def normalize_item(item: object, game: str) -> str | None:
    if isinstance(item, str):
        raw = item
    elif isinstance(item, dict):
        raw = item.get("path")
        if not raw and item.get("name"):
            raw = f"08_output/{game}/{item['name']}"
    else:
        return None
    if not raw:
        return None
    path = Path(raw)
    if not path.is_absolute():
        path = ROOT / path
    return str(path.resolve(strict=False))


def load_manifest(path: Path, game: str) -> list[str]:
    data = json.loads(path.read_text(encoding="utf-8"))
    values: list[object] = []
    for key in ("removed_paths", "removed_roots", "removed"):
        item = data.get(key)
        if isinstance(item, list):
            values.extend(item)
    return [p for p in (normalize_item(v, game) for v in values) if p]


def matches(candidate: object, removed: str) -> bool:
    if not isinstance(candidate, str) or not candidate:
        return False
    current = candidate
    if not os.path.isabs(current):
        current = str((ROOT / current).resolve(strict=False))
    else:
        current = str(Path(current).resolve(strict=False))
    if any(ch in removed for ch in "*?["):
        return fnmatch.fnmatch(current, removed)
    removed_norm = removed.rstrip("/")
    return current == removed_norm or current.startswith(removed_norm + "/")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--game", required=True)
    parser.add_argument("--pipeline-json", type=Path)
    parser.add_argument("--manifest", action="append", default=[], type=Path)
    parser.add_argument("--removed", action="append", default=[])
    parser.add_argument("--date", default=None)
    parser.add_argument("--apply", action="store_true")
    args = parser.parse_args()

    pipeline_path = args.pipeline_json or (ROOT / "04_scripts" / args.game / "pipeline.json")
    if not pipeline_path.is_file():
        print(f"错误: 管线状态文件不存在: {pipeline_path}", file=sys.stderr)
        return 1

    removed = [p for p in (normalize_item(v, args.game) for v in args.removed) if p]
    for manifest in args.manifest:
        removed.extend(load_manifest(manifest, args.game))
    removed = list(dict.fromkeys(removed))
    if not removed:
        print("错误: 至少需要一个 --removed 或 --manifest 条目", file=sys.stderr)
        return 1

    data = json.loads(pipeline_path.read_text(encoding="utf-8"))
    date = args.date or __import__("time").strftime("%Y-%m-%d")
    changed = 0

    for stage in data.get("stages", []):
        sid = stage.get("id", "?")
        notes: list[str] = []

        for rule in stage.get("verify", []):
            for path in removed:
                if matches(rule.get("path"), path):
                    if rule.get("type") != "not_exists":
                        print(f"{sid}: verify {rule.get('type')} -> not_exists: {rule.get('path')}")
                        rule["type"] = "not_exists"
                        changed += 1
                        notes.append(path)

        artifacts = stage.get("artifacts")
        if isinstance(artifacts, dict):
            for path in list(artifacts):
                for removed_path in removed:
                    if matches(path, removed_path):
                        md5 = artifacts[path].get("md5") if isinstance(artifacts[path], dict) else None
                        print(f"{sid}: remove artifact: {path}" + (f" (md5={md5})" if md5 else ""))
                        del artifacts[path]
                        changed += 1
                        notes.append(f"{path}" + (f"; 原 md5={md5}" if md5 else ""))
            if not artifacts:
                stage.pop("artifacts", None)

        inputs = stage.get("inputs")
        if isinstance(inputs, dict):
            for path in list(inputs):
                for removed_path in removed:
                    if matches(path, removed_path):
                        print(f"{sid}: remove input: {path}")
                        del inputs[path]
                        changed += 1
            if not inputs:
                stage.pop("inputs", None)

        if notes:
            existing = str(stage.get("note") or "").rstrip()
            additions = []
            for note in notes:
                marker = f"{date} 收尾清理: {note}"
                if marker not in existing and marker not in additions:
                    additions.append(marker)
            if additions:
                stage["note"] = "\n".join([x for x in (existing, *additions) if x])
                changed += len(additions)

    if not changed:
        print("无需修改: 指定路径未出现在 pipeline 快照中")
        return 0

    if not args.apply:
        print(f"[dry-run] 将修改 {changed} 处；加 --apply 写入 {pipeline_path}")
        return 0

    pipeline_path.parent.mkdir(parents=True, exist_ok=True)
    with tempfile.NamedTemporaryFile(
        "w", encoding="utf-8", dir=pipeline_path.parent, delete=False
    ) as tmp:
        json.dump(data, tmp, ensure_ascii=False, indent=2)
        tmp.write("\n")
        tmp_name = tmp.name
    os.replace(tmp_name, pipeline_path)
    print(f"已更新: {pipeline_path}（{changed} 处）")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
