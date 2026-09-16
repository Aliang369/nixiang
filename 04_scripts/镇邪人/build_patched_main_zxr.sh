#!/usr/bin/env bash
# 生成镇邪人本地 src bundle 路由保护版 main.js。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
APK="$ROOT/02_apks/镇邪人/镇邪人_1.0.3_starry_from_downloads_20260916.apk"
OUT_JS="$ROOT/07_patched/镇邪人/main.js"
PYTHON_BIN="${NIXIANG_PYTHON:-/opt/homebrew/bin/python3.11}"

[[ -x "$PYTHON_BIN" ]] || PYTHON_BIN="$(command -v python3)"
"$PYTHON_BIN" "$SCRIPT_DIR/patch_main_zxr.py" "$APK" "$OUT_JS"
node --check "$OUT_JS"
LC_ALL=C grep -aq 'starry_zxr_main_guard_v1' "$OUT_JS"
printf '[patch_main] js=%s\n[patch_main] sha256=%s\n' \
  "$OUT_JS" "$(shasum -a 256 "$OUT_JS" | awk '{print $1}')"
