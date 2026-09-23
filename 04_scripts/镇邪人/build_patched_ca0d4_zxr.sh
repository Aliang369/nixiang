#!/usr/bin/env bash
# 镇邪人远程热更 src ca0d4（2026-09-23 服务器当前版本）：应用同一套战斗补丁并回加密。
# 输入来自真机热更产物（09_test/镇邪人_orig_20260923/decrypt_ca0d4/index.ca0d4.js）。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
SRC="$ROOT/06_dumped/镇邪人/remote_ca0d4/index.ca0d4.js"
CONFIG_SRC="$ROOT/06_dumped/镇邪人/remote_ca0d4/config.ca0d4.json"
OUT_DIR="$ROOT/07_patched/镇邪人/ca0d4"
OUT_JS="$OUT_DIR/index.ca0d4.safe.js"
OUT_JSC="$OUT_DIR/index.ca0d4.jsc"
SPEC="$SCRIPT_DIR/zxr_patch_spec_b5278.json"
PYTHON_BIN="${NIXIANG_PYTHON:-/opt/homebrew/bin/python3.11}"

[[ -x "$PYTHON_BIN" ]] || PYTHON_BIN="$(command -v python3)"
[[ -f "$SRC" ]] || { echo "ERROR: decrypted ca0d4 JS missing: $SRC" >&2; exit 1; }
[[ -f "$CONFIG_SRC" ]] || { echo "ERROR: ca0d4 config missing: $CONFIG_SRC" >&2; exit 1; }
[[ -f "$SPEC" ]] || { echo "ERROR: spec missing: $SPEC" >&2; exit 1; }
command -v node >/dev/null || { echo "ERROR: node missing" >&2; exit 1; }

mkdir -p "$OUT_DIR"
"$PYTHON_BIN" "$SCRIPT_DIR/patch_jsc_zxr.py" "$SRC" "$SPEC" "$OUT_JS" "$OUT_JSC"
cp -f "$CONFIG_SRC" "$OUT_DIR/config.ca0d4.json"
node --check "$OUT_JS"
node "$SCRIPT_DIR/test_zxr_patch.js" "$SPEC"
for token in 'starry_zxr_safe_v2' '__starryZxrOn(1)' '__starryZxrOn(2)' '__starryZxrOn(3)' '__starryZxrOn(4)'; do
  LC_ALL=C grep -aq "$token" "$OUT_JS" || { echo "ERROR: ca0d4 patched JS missing token: $token" >&2; exit 1; }
done
printf '[patch_ca0d4] js=%s\n[patch_ca0d4] jsc=%s\n[patch_ca0d4] config=%s\n[patch_ca0d4] sha256=%s\n' \
  "$OUT_JS" "$OUT_JSC" "$OUT_DIR/config.ca0d4.json" "$(shasum -a 256 "$OUT_JSC" | awk '{print $1}')"
