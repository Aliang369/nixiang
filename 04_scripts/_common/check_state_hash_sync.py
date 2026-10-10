#!/usr/bin/env python3
"""Check that game state/current index hashes match the final APK/prune manifest.

清单/输入契约（2026-10-10 追迹迷城复盘 P0-4 补注）:
  --manifest 吃 prune 清单（removed:[{name,size,md5}]），必须含 md5 与 size，
  且 state 文件（10_docs/_state/games/{游戏}.md）须写出**完整** md5 + sha256；
  10_docs/_state/current.md 的本游戏行须含 md5/sha256 的 **8 位前缀**，
  且整行 ≤400 **字符**（不是字节）。缺 sha256 前缀 ⇒ 报
  「hash sync failed: current sha256 prefix」。
"""
import argparse
import hashlib
import json
import re
import sys
from pathlib import Path


def md5_file(path: Path) -> str:
    h = hashlib.md5()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def sha256_file(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--game", required=True)
    ap.add_argument("--manifest")
    ap.add_argument("--apk")
    ap.add_argument("--state")
    ap.add_argument("--current", default="10_docs/_state/current.md")
    args = ap.parse_args()

    root = Path(__file__).resolve().parents[2]
    state = Path(args.state) if args.state else root / "10_docs/_state/games" / f"{args.game}.md"
    current = Path(args.current)
    if not current.is_absolute():
        current = root / current
    if not state.is_absolute():
        state = root / state

    apk = None
    keep_md5 = None
    keep_size = None
    if args.manifest:
        manifest_path = Path(args.manifest)
        if not manifest_path.is_absolute():
            manifest_path = root / manifest_path
        data = json.loads(manifest_path.read_text(encoding="utf-8"))
        keep = data["keep"]
        keep_md5 = data["keep_md5"]
        keep_size = data.get("keep_size")
        apk = root / "08_output" / args.game / keep
    elif args.apk:
        apk = Path(args.apk)
        if not apk.is_absolute():
            apk = root / apk
    else:
        print("ERROR: need --manifest or --apk", file=sys.stderr)
        return 2

    if not apk.exists():
        print(f"ERROR: APK not found: {apk}", file=sys.stderr)
        return 1
    if not state.exists():
        print(f"ERROR: state file not found: {state}", file=sys.stderr)
        return 1
    if not current.exists():
        print(f"ERROR: current index not found: {current}", file=sys.stderr)
        return 1

    actual_md5 = md5_file(apk)
    actual_sha256 = sha256_file(apk)
    actual_size = apk.stat().st_size
    if keep_md5 and actual_md5 != keep_md5:
        print(f"ERROR: APK md5 mismatch: actual={actual_md5} manifest={keep_md5}", file=sys.stderr)
        return 1
    if keep_size is not None and actual_size != keep_size:
        print(f"ERROR: APK size mismatch: actual={actual_size} manifest={keep_size}", file=sys.stderr)
        return 1

    state_text = state.read_text(encoding="utf-8")
    current_text = current.read_text(encoding="utf-8")
    row = None
    for line in current_text.splitlines():
        if line.startswith(f"| {args.game} |"):
            row = line
            break
    if row is None:
        print(f"ERROR: current.md has no row for {args.game}", file=sys.stderr)
        return 1
    if len(row) > 400:
        print(f"ERROR: current row too long: {len(row)} > 400 chars", file=sys.stderr)
        return 1

    checks = [
        ("state md5", actual_md5 in state_text),
        ("state sha256", actual_sha256 in state_text),
        ("current md5 prefix", actual_md5[:8] in row),
        ("current sha256 prefix", actual_sha256[:8] in row),
    ]
    failed = [name for name, ok in checks if not ok]
    if failed:
        print("ERROR: hash sync failed: " + ", ".join(failed), file=sys.stderr)
        print(f"  apk={apk}", file=sys.stderr)
        print(f"  md5={actual_md5}", file=sys.stderr)
        print(f"  sha256={actual_sha256}", file=sys.stderr)
        print(f"  state={state}", file=sys.stderr)
        print(f"  current={current}", file=sys.stderr)
        return 1

    print(f"OK {args.game} md5={actual_md5} sha256={actual_sha256} row_chars={len(row)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
