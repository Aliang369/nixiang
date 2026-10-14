#!/usr/bin/env python3.11
"""山海幻境：离线复刻 C++ 拼接规则，核对主 JS 锚点是否仍然命中（薄封装）。

★ 2026-10-15 起实现已升格为公共工具 `04_scripts/_common/cocos_js_splice_check.py`，
  山海幻境的差异全部落在数据文件 `10_docs/山海幻境/shhj_js_anchors.json`（换版本只改数据）。

用法（与旧脚本兼容）:
    python3.11 04_scripts/山海幻境/check_shhj_js_anchors.py <解密后的 assets/main/index.js> \
        [--out <拼接后 JS 路径>] [--quiet]

装机后 logcat 应出现的行由锚点 JSON 的 pair.log 决定（与 ShhjMod.cpp 的 LOGI 逐字一致）。
"""
from __future__ import annotations

import subprocess
import sys
from pathlib import Path

SCRIPT_DIR = Path(__file__).resolve().parent
NIXIANG = SCRIPT_DIR.parent.parent
CHECKER = NIXIANG / "04_scripts/_common/cocos_js_splice_check.py"
ANCHORS = NIXIANG / "10_docs/山海幻境/shhj_js_anchors.json"


def main() -> int:
    if not CHECKER.is_file():
        print(f"ERROR: 缺少公共核对器: {CHECKER}", file=sys.stderr)
        return 2
    if not ANCHORS.is_file():
        print(f"ERROR: 缺少锚点数据: {ANCHORS}", file=sys.stderr)
        return 2
    cmd = [sys.executable, str(CHECKER), *sys.argv[1:], "--anchors", str(ANCHORS)]
    return subprocess.run(cmd).returncode


if __name__ == "__main__":
    raise SystemExit(main())
