#!/usr/bin/env python3
"""Record already-performed cleanup in a game's pipeline.json.

The cleanup step may delete build outputs before archive.  This helper keeps
the pipeline snapshot consistent by changing matching verify rules to
not_exists and removing matching artifact/input entries.

清单契约（2026-10-10 追迹迷城复盘 P0-4 补注）:
  --manifest 只吃 **prune 清单**（prune_output_final.sh 写出的
  removed:[{name,size,md5}]，即 10_docs/{游戏}/正式包裁剪清单_*.json）。
  传 trash_paths.sh 的清理清单（--path 列表、无 removed 键）会报
  「错误: 至少需要一个 --removed 或 --manifest 条目」。
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


def _rel(path: str) -> str:
    """把绝对路径压成仓库相对路径（便于人读与跨机对比）。"""
    try:
        return str(Path(path).resolve(strict=False).relative_to(ROOT))
    except ValueError:
        return path


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
    parser.add_argument(
        "--restore-verify",
        action="store_true",
        help="修复模式：把生产阶段被改成 not_exists 的 verify 还原成 file_exists"
             "（这些条目是历史轮次收尾时被旧版脚本改写的；不还原则该阶段下次重跑时"
             "「产物刚生成就被判失败」）。配合 --stages 指定阶段，默认只动生产阶段。",
    )
    parser.add_argument(
        "--stages",
        default="build_so,surgical,sign,install,so_only,patch,compile",
        help="--restore-verify 的作用范围（逗号分隔 stage id；默认不含 cleanup/archive）",
    )
    parser.add_argument(
        "--flip-verify",
        action="store_true",
        help="旧行为：把命中产物的 verify 改成 not_exists。默认关闭——"
             "改写会让下一轮该阶段重跑时「产物刚生成就被 not_exists 判失败」"
             "（2026-11-05 一念通天 779 实测），且与 stages.md「verify 永远保持真产物语义」冲突。",
    )
    args = parser.parse_args()

    pipeline_path = args.pipeline_json or (ROOT / "04_scripts" / args.game / "pipeline.json")
    if not pipeline_path.is_file():
        print(f"错误: 管线状态文件不存在: {pipeline_path}", file=sys.stderr)
        return 1

    removed = [p for p in (normalize_item(v, args.game) for v in args.removed) if p]
    for manifest in args.manifest:
        removed.extend(load_manifest(manifest, args.game))
    removed = list(dict.fromkeys(removed))
    if not removed and not args.restore_verify:
        print("错误: 至少需要一个 --removed 或 --manifest 条目", file=sys.stderr)
        return 1

    data = json.loads(pipeline_path.read_text(encoding="utf-8"))
    date = args.date or __import__("time").strftime("%Y-%m-%d")
    changed = 0

    if args.restore_verify:
        allow = {x.strip() for x in str(args.stages).split(",") if x.strip()}
        restored = 0
        for stage in data.get("stages", []):
            sid = stage.get("id", "?")
            if sid not in allow:
                continue
            hit = 0
            for rule in stage.get("verify", []):
                if isinstance(rule, dict) and rule.get("type") == "not_exists":
                    print(f"{sid}: verify not_exists -> file_exists: {_rel(str(rule.get('path')))}")
                    rule["type"] = "file_exists"
                    hit += 1
            if hit:
                restored += hit
                existing = str(stage.get("note") or "").rstrip()
                marker = f"{date} 修正: verify 由 not_exists 还原为 file_exists（真产物语义，{hit} 条）"
                if marker not in existing:
                    stage["note"] = "\n".join([x for x in (existing, marker) if x])
                changed += 1
        if not restored:
            print("无需修改: 指定阶段没有 not_exists 的 verify")
            return 0
        if not args.apply:
            print(f"[dry-run] 将还原 {restored} 条 verify；加 --apply 写入 {pipeline_path}")
            return 0
        print(f"将还原 {restored} 条 verify")
        # 走统一的落盘分支

    for stage in data.get("stages", []):
        sid = stage.get("id", "?")
        notes: list[str] = []

        cleaned: list[str] = []

        for rule in stage.get("verify", []):
            if not isinstance(rule, dict):
                continue
            for path in removed:
                if matches(rule.get("path"), path):
                    if args.flip_verify:
                        if rule.get("type") != "not_exists":
                            print(f"{sid}: verify {rule.get('type')} -> not_exists: {rule.get('path')}")
                            rule["type"] = "not_exists"
                            changed += 1
                            notes.append(path)
                    else:
                        # 2026-11-05 修正（一念通天 779 实测）：默认不再改写 verify 语义。
                        # 依据 stages.md「禁止手改 stage 的 verify 语义；verify 永远保持
                        # "该阶段的真产物"语义」。改写会让下一轮该阶段重跑时产物刚生成
                        # 就被 not_exists 判失败，必须靠人记着翻回来 —— 是把债转嫁给下一轮。
                        rel = _rel(path)
                        print(f"{sid}: verify 保持 {rule.get('type')}（真产物语义不变；该产物已在收尾清理中删除）: {rel}")
                        if rel not in cleaned:
                            cleaned.append(rel)

        artifacts = stage.get("artifacts")
        if isinstance(artifacts, dict):
            for path in list(artifacts):
                for removed_path in removed:
                    # 同一 artifact 可能同时命中父目录与具体文件两条 removed 条目；
                    # 前一条已把它删掉，这里必须跳过，否则 artifacts[path] 抛 KeyError。
                    if path not in artifacts:
                        break
                    if matches(path, removed_path):
                        entry = artifacts[path]
                        md5 = entry.get("md5") if isinstance(entry, dict) else None
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

        if cleaned:
            bucket = stage.get("cleaned")
            if not isinstance(bucket, dict):
                bucket = {}
                stage["cleaned"] = bucket
            bucket[date] = sorted(set(list(bucket.get(date, [])) + cleaned))
            changed += 1

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
