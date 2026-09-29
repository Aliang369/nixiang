#!/usr/bin/env bash
# 我在神界刷装备 管线 build_so 阶段后端：私有 Android_sjzz.mk 直编
# 产物 = mod菜单/starry-mod-menu/libs_sjzz/arm64-v8a/libstarry_magic.so
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OUT_LIBS="$STARRY_SRC/libs_sjzz"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$JNI/Android_sjzz.mk" ]] || { echo "ERROR: missing $JNI/Android_sjzz.mk" >&2; exit 1; }

OBJ="$STARRY_SRC/obj_sjzz"
STAMP="$OBJ/.build_stamp"
CFG_HASH="$( (cat "$JNI/Android_sjzz.mk" "$JNI/Application.mk"; \
  printf 'TRACE=%s\n' "${STARRY_TRACE:-}") \
  | shasum -a 256 | awk '{print $1}')"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] 构建配置变化（mk/开关）→ 清 obj_sjzz 强制全量重编"
  rm -rf "$OBJ"
fi

SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_sjzz.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$OUT_LIBS" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 8)" || BUILD_RC=$?
NEW_O="$(find "$OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1)"
rm -f "$T0"
[[ $BUILD_RC -eq 0 ]] || { echo "ERROR: ndk-build rc=$BUILD_RC" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: build produced no $SO" >&2; exit 1; }

if [[ -n "$NEW_O" && "$(md5 -q "$SO")" == "$SO_MD5_BEFORE" ]]; then
  echo "ERROR: 本次构建产生了新 .o 但 $SO 未变化——旧 so 顶包嫌疑，禁止通过" >&2
  exit 1
fi
mkdir -p "$OBJ" && echo "$CFG_HASH" > "$STAMP"

# 中文菜单串必须按字节检查，strings 不认 UTF-8。
for token in 'Slider_伤害倍率' 'Toggle_无敌' 'Toggle_免广告' 'Toggle_游戏加速' 'Sjzz'; do
  cnt=$(LC_ALL=C grep -ac "$token" "$SO" || true)
  [[ "$cnt" -ge 1 ]] || { echo "ERROR: $SO 不含 $token（旧 so 混入或源码未编译）" >&2; exit 1; }
done
LC_ALL=C grep -aq 'SjzzHooks' "$SO" \
  || { echo "ERROR: $SO 不含 SjzzHooks（Java 桥未编入）" >&2; exit 1; }

echo "[build_so] done → $SO"
