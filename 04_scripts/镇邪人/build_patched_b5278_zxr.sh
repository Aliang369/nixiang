#!/usr/bin/env bash
# 镇邪人远程热更 src b5278：下载后应用同一套战斗补丁并回加密。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
SRC="$ROOT/06_dumped/镇邪人/remote_b5278/index.b5278.js"
OUT_DIR="$ROOT/07_patched/镇邪人/b5278"
OUT_JS="$OUT_DIR/index.b5278.safe.js"
OUT_JSC="$OUT_DIR/index.b5278.jsc"
SPEC="$SCRIPT_DIR/zxr_patch_spec_b5278.json"
PYTHON_BIN="${NIXIANG_PYTHON:-/opt/homebrew/bin/python3.11}"

[[ -x "$PYTHON_BIN" ]] || PYTHON_BIN="$(command -v python3)"
bash "$SCRIPT_DIR/fetch_remote_b5278.sh"
[[ -f "$SPEC" ]] || { echo "ERROR: spec missing: $SPEC" >&2; exit 1; }

"$PYTHON_BIN" "$SCRIPT_DIR/patch_jsc_zxr.py" "$SRC" "$SPEC" "$OUT_JS" "$OUT_JSC"
cp -f "$ROOT/06_dumped/镇邪人/remote_b5278/config.b5278.json" "$OUT_DIR/config.b5278.json"
cp -f "$OUT_JS" "$OUT_DIR/index.b5278.js"
node --check "$OUT_JS"
node "$SCRIPT_DIR/test_zxr_patch.js" "$SPEC"
for token in 'starry_zxr_safe_v2' '__starryZxrOn(1)' '__starryZxrOn(2)' '__starryZxrOn(3)' '__starryZxrOn(4)'; do
  LC_ALL=C grep -aq "$token" "$OUT_JS" || { echo "ERROR: b5278 patched JS missing token: $token" >&2; exit 1; }
done
printf '[patch_b5278] js=%s\n[patch_b5278] jsc=%s\n[patch_b5278] sha256=%s\n' \
  "$OUT_JS" "$OUT_JSC" "$(shasum -a 256 "$OUT_JSC" | awk '{print $1}')"
