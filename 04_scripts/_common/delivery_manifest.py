#!/usr/bin/env python3
"""Generate and validate the machine-generated delivery manifest.

The manifest is a record of the pipeline state and hashes at archive-close time;
it is not a hand-maintained status document.
"""
import argparse
import hashlib
import json
import os
import subprocess
import sys
from datetime import datetime, timezone
from pathlib import Path


def repo_root():
    return Path(__file__).resolve().parents[2]


def resolve_path(root, value):
    p = Path(value)
    if p.is_file():
        return p
    # pipeline.json may contain the other machine's absolute /Users/... path.
    marker = "/nixiang/"
    text = value.replace("\\", "/")
    if marker in text:
        candidate = root / text.split(marker, 1)[1]
        if candidate.exists():
            return candidate
    if not p.is_absolute():
        return root / p
    return p


def sha256(path):
    h = hashlib.sha256()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def md5(path):
    h = hashlib.md5()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def rel(root, value):
    p = resolve_path(root, value)
    try:
        return p.resolve().relative_to(root.resolve()).as_posix()
    except ValueError:
        return value


def git(root, *args):
    return subprocess.check_output(["git", "-C", str(root), *args], text=True).strip()


def build_manifest(root, game):
    state_path = root / "04_scripts" / game / "pipeline.json"
    if not state_path.is_file():
        raise ValueError(f"缺少管线状态文件: {state_path}")
    data = json.loads(state_path.read_text(encoding="utf-8"))
    stages = data.get("stages", [])
    done = [s for s in stages if s.get("status") == "done"]
    apk = None
    for stage in done:
        for value in (stage.get("artifacts") or {}):
            if value.lower().endswith(".apk"):
                apk = value
    apk_record = None
    if apk:
        p = resolve_path(root, apk)
        apk_record = {"path": rel(root, apk), "available": p.is_file()}
        if p.is_file():
            apk_record.update({"size": p.stat().st_size, "md5": md5(p), "sha256": sha256(p)})

    artifacts = []
    for stage in done:
        for value, record in (stage.get("artifacts") or {}).items():
            p = resolve_path(root, value)
            item = {"stage": stage.get("id"), "path": rel(root, value),
                    "recorded_md5": record.get("md5"), "available": p.is_file()}
            if p.is_file():
                item["current_md5"] = md5(p)
                item["current_sha256"] = sha256(p)
                item["matches_recorded_md5"] = item["current_md5"] == record.get("md5")
            artifacts.append(item)

    return {
        "schema": 1,
        "generated_by": "04_scripts/_common/delivery_manifest.py",
        "generated_at": datetime.now(timezone.utc).isoformat(timespec="seconds"),
        "game": game,
        "version": data.get("version"),
        "package": (data.get("params") or {}).get("package"),
        "pipeline_state": rel(root, str(state_path)),
        "pipeline_status": {s.get("id"): s.get("status") for s in stages},
        "source_commit": git(root, "rev-parse", "HEAD"),
        "source_tree": git(root, "write-tree"),
        "apk": apk_record,
        "artifacts": artifacts,
        "not_proven": ["manifest 只汇总管线与文件指纹；运行证据/NOT PROVEN 需由归档文档人工明确记录"],
        "closure": {"staged_scope_checked": True, "generated_at_archive_close": True},
    }


def main(argv=None):
    ap = argparse.ArgumentParser()
    ap.add_argument("--game", required=True)
    ap.add_argument("--output")
    ap.add_argument("--check", action="store_true")
    args = ap.parse_args(argv)
    root = repo_root()
    output = Path(args.output) if args.output else root / "10_docs" / args.game / "delivery_manifest.json"
    if not output.is_absolute():
        output = root / output
    try:
        manifest = build_manifest(root, args.game)
    except (OSError, ValueError, subprocess.CalledProcessError, json.JSONDecodeError) as exc:
        print(f"ERROR: 生成交付 manifest 失败: {exc}", file=sys.stderr)
        return 1
    if args.check:
        if not output.is_file():
            print(f"ERROR: 缺少交付 manifest: {output}", file=sys.stderr)
            return 1
        old = json.loads(output.read_text(encoding="utf-8"))
        for key in ("game", "version", "package", "pipeline_status", "apk", "artifacts"):
            if old.get(key) != manifest.get(key):
                print(f"ERROR: manifest 已过期，字段不一致: {key}", file=sys.stderr)
                return 1
        print(f"PASS: delivery manifest {output}")
        return 0
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"生成交付 manifest: {output}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
