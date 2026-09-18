#!/usr/bin/env bash
# 燃烧纪元 1.13.09 管线 build_so 阶段后端：私有 Android_rsjy.mk 直编。
# 产物 = mod菜单/starry-mod-menu/libs_rsjy/arm64-v8a/libstarry_magic.so
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OUT_LIBS="$STARRY_SRC/libs_rsjy"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"
PATCH_JS="$NIXIANG/04_scripts/燃烧纪元/patch_rsjy.js"
OBJ="$STARRY_SRC/obj_rsjy"
STAMP="$OBJ/.build_stamp"
GEN="$OBJ/generated"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$JNI/Android_rsjy.mk" ]] || { echo "ERROR: missing $JNI/Android_rsjy.mk" >&2; exit 1; }
[[ -f "$PATCH_JS" ]] || { echo "ERROR: missing $PATCH_JS" >&2; exit 1; }

CFG_HASH="$( (cat "$JNI/Android_rsjy.mk" "$JNI/Application.mk" \
  "$JNI/Games/RsjyMod.h" "$JNI/Games/RsjyMod.cpp" "$JNI/Games/RsjyEntry.cpp" \
  "$PATCH_JS"; printf 'TRACE=%s\n' "${STARRY_TRACE:-}") | shasum -a 256 | awk '{print $1}')"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] 构建配置变化 → 清 obj_rsjy 强制全量重编"
  rm -rf "$OBJ"
fi

mkdir -p "$GEN"
python3 - "$PATCH_JS" "$GEN/RsjyPatchJs.h" <<'PY'
import sys
from pathlib import Path
text = Path(sys.argv[1]).read_text(encoding='utf-8')
if ')RSJYJS"' in text:
    raise SystemExit('patch JS contains raw-string terminator')
new = 'static const char kPatchJs[] = R"RSJYJS(' + text + ')RSJYJS";\n'
dst = Path(sys.argv[2])
if not dst.exists() or dst.read_text(encoding='utf-8') != new:
    dst.write_text(new, encoding='utf-8')
PY
export STARRY_RSJY_GEN="$GEN"

SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_rsjy.mk" \
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

LC_ALL=C grep -aq "rsjy" "$SO" || { echo "ERROR: so 缺少 rsjy 标签" >&2; exit 1; }
LC_ALL=C grep -aq "Slider_攻击倍率:1:100:1" "$SO" || { echo "ERROR: so 缺少攻击倍率滑块" >&2; exit 1; }
LC_ALL=C grep -aq "Toggle_上帝模式" "$SO" || { echo "ERROR: so 缺少上帝模式" >&2; exit 1; }
LC_ALL=C grep -aq "Slider_战斗速度:1:10:1" "$SO" || { echo "ERROR: so 缺少战斗速度滑块" >&2; exit 1; }
LC_ALL=C grep -aq "Toggle_免广告" "$SO" || { echo "ERROR: so 缺少免广告开关" >&2; exit 1; }
LC_ALL=C grep -aq "starry_rsjy_v1" "$SO" || { echo "ERROR: so 缺少 JS 补丁标记" >&2; exit 1; }

echo "[build_so] done → $SO"
echo "[build_so] md5=$(md5 -q "$SO")"
