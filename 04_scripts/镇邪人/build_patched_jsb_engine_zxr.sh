#!/usr/bin/env bash
# 生成镇邪人本地 src bundle 路由版 jsb-engine.js。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
APK="$ROOT/02_apks/镇邪人/镇邪人_1.0.3_starry_from_downloads_20260916.apk"
OUT="$ROOT/07_patched/镇邪人/jsb-engine.js"
PYTHON_BIN="${NIXIANG_PYTHON:-/opt/homebrew/bin/python3.11}"

[[ -x "$PYTHON_BIN" ]] || PYTHON_BIN="$(command -v python3)"
"$PYTHON_BIN" "$SCRIPT_DIR/patch_jsb_engine_zxr.py" "$APK" "$OUT"
node --check "$OUT"
LC_ALL=C grep -aq 'if (bundleName === "src")' "$OUT"
printf '[patch_jsb_engine] js=%s\n[patch_jsb_engine] sha256=%s\n' \
  "$OUT" "$(shasum -a 256 "$OUT" | awk '{print $1}')"
