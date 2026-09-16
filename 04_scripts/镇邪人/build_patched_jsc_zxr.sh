#!/usr/bin/env bash
# 镇邪人 1.0.3：生成静态战斗补丁 JSC，并同步生成 native 热更兜底头文件。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
JNI="$NIXIANG_ROOT/mod菜单/starry-mod-menu/app/src/main/jni"
DUMP="$NIXIANG_ROOT/06_dumped/镇邪人/starry_1.0.3/jsc_decrypted/assets/src/index.d5507.js"
OUT_DIR="$NIXIANG_ROOT/07_patched/镇邪人"
OUT_JS="$OUT_DIR/index.d5507.safe.js"
OUT_JSC="$OUT_DIR/assets/src/index.d5507.jsc"
SPEC="$SCRIPT_DIR/zxr_patch_spec.json"
HEADER_SPEC="$SCRIPT_DIR/zxr_patch_spec_b5278.json"
HEADER="$JNI/Games/ZxrPatchJs.h"
PYTHON_BIN="${NIXIANG_PYTHON:-/opt/homebrew/bin/python3.11}"

[[ -x "$PYTHON_BIN" ]] || PYTHON_BIN="$(command -v python3)"
[[ -f "$DUMP" ]] || { echo "ERROR: decrypted JS missing: $DUMP" >&2; exit 1; }
[[ -f "$SPEC" ]] || { echo "ERROR: patch spec missing: $SPEC" >&2; exit 1; }
command -v node >/dev/null || { echo "ERROR: node missing" >&2; exit 1; }

"$PYTHON_BIN" "$SCRIPT_DIR/gen_zxr_patch_header.py" "$HEADER_SPEC" "$HEADER"
node "$SCRIPT_DIR/test_zxr_patch.js" "$SPEC"
"$PYTHON_BIN" "$SCRIPT_DIR/patch_jsc_zxr.py" "$DUMP" "$SPEC" "$OUT_JS" "$OUT_JSC"

node --check "$OUT_JS"
for token in 'starry_zxr_safe_v2' '__starryZxrOn(1)' '__starryZxrOn(2)' '__starryZxrOn(3)' '__starryZxrOn(4)'; do
  LC_ALL=C grep -aq "$token" "$OUT_JS" || { echo "ERROR: patched JS missing token: $token" >&2; exit 1; }
done
printf '[patch_jsc] js=%s\n[patch_jsc] jsc=%s\n[patch_jsc] sha256=%s\n' \
  "$OUT_JS" "$OUT_JSC" "$(shasum -a 256 "$OUT_JSC" | awk '{print $1}')"
