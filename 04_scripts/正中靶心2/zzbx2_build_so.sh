#!/usr/bin/env bash
# 正中靶心2 管线 build_so 阶段后端：私有 Android_zzbx2.mk 直编。
# 产物 = mod菜单/starry-mod-menu/libs_zzbx2/arm64-v8a/libstarry_magic.so
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OUT_LIBS="$STARRY_SRC/libs_zzbx2"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$JNI/Android_zzbx2.mk" ]] || { echo "ERROR: missing $JNI/Android_zzbx2.mk" >&2; exit 1; }

OBJ="$STARRY_SRC/obj_zzbx2"
STAMP="$OBJ/.build_stamp"
GEN="$OBJ/generated"
PATCH_JS="$NIXIANG/04_scripts/正中靶心2/patch_main.js"
CFG_HASH="$( (cat "$JNI/Android_zzbx2.mk" "$JNI/Application.mk" "$PATCH_JS"; \
  printf 'TRACE=%s\n' "${STARRY_TRACE:-}") | shasum -a 256 | awk '{print $1}')"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] 构建配置变化 → 清 obj_zzbx2 强制全量重编"
  rm -rf "$OBJ"
fi

mkdir -p "$GEN"
python3 - "$PATCH_JS" "$GEN/Zzbx2PatchJs.h" <<'PY'
import sys
from pathlib import Path
text = Path(sys.argv[1]).read_text(encoding='utf-8')
if ')ZZBX2JS"' in text:
    raise SystemExit('patch JS contains raw-string terminator')
new = 'static const char kPatchJs[] = R"ZZBX2JS(' + text + ')ZZBX2JS";\n'
dst = Path(sys.argv[2])
if not dst.exists() or dst.read_text(encoding='utf-8') != new:
    dst.write_text(new, encoding='utf-8')
PY
export STARRY_ZZBX2_GEN="$GEN"

SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_zzbx2.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$OUT_LIBS" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 8)" || BUILD_RC=$?
NEW_O="$(find "$OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1)"
rm -f "$T0"
[[ $BUILD_RC -eq 0 ]] || { echo "ERROR: ndk-build rc=$BUILD_RC" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: build produced no $SO" >&2; exit 1; }

if [[ -n "$NEW_O" && "$(md5 -q "$SO")" == "$SO_MD5_BEFORE" ]]; then
  echo "ERROR: 本次构建产生了新 .o 但 $SO 未变化——旧 so 顶包嫌疑" >&2
  exit 1
fi
mkdir -p "$OBJ" && echo "$CFG_HASH" > "$STAMP"

LC_ALL=C grep -aq "zzbx2" "$SO" || { echo "ERROR: so 缺少 zzbx2 标签" >&2; exit 1; }
LC_ALL=C grep -aq "Toggle_秒杀" "$SO" || { echo "ERROR: so 缺少 Toggle_秒杀" >&2; exit 1; }
LC_ALL=C grep -aq "Slider_战斗加速:1:5:1" "$SO" || { echo "ERROR: so 缺少 Slider_战斗加速:1:5:1" >&2; exit 1; }
LC_ALL=C grep -aq "Toggle_免广告" "$SO" || { echo "ERROR: so 缺少 Toggle_免广告" >&2; exit 1; }
LC_ALL=C grep -aq "starry_zzbx2_v1" "$SO" || { echo "ERROR: so 缺少 JS 补丁标记" >&2; exit 1; }

echo "[build_so] done → $SO"
echo "[build_so] md5=$(md5 -q "$SO")"
