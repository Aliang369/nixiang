#!/usr/bin/env bash
# 幻想武将模拟器单包用 so：libc++ 静态链接，避免再占一个 APK 条目。
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
STARRY_SRC="$ROOT/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OUT_LIBS="$STARRY_SRC/libs_hxwj_static"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"
READELF="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-readelf"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$JNI/Android_hxwj.mk" ]] || { echo "ERROR: missing Android_hxwj.mk" >&2; exit 1; }
[[ -f "$JNI/Application_hxwj_static.mk" ]] || { echo "ERROR: missing Application_hxwj_static.mk" >&2; exit 1; }
[[ -x "$READELF" ]] || { echo "ERROR: llvm-readelf missing at $READELF" >&2; exit 1; }

"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_hxwj.mk" \
  NDK_APPLICATION_MK="./Application_hxwj_static.mk" \
  NDK_OUT="$STARRY_SRC/obj_hxwj_static" \
  NDK_LIBS_OUT="$OUT_LIBS" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 8)"

[[ -f "$SO" ]] || { echo "ERROR: build produced no $SO" >&2; exit 1; }

for s in "Toggle_秒杀" "Toggle_百分百掉落" "Toggle_免广告" \
         "hxwjmlq" "FightGwItem" "ToponVideo" "[hxwj]"; do
  LC_ALL=C grep -aqF -- "$s" "$SO" || { echo "ERROR: $SO 缺归属串 $s" >&2; exit 1; }
done
for s in "[Gyzw]" "[Ysxz]" "[Dwct]" "[Tlgd]" "[Blfx]"; do
  if LC_ALL=C grep -aqF -- "$s" "$SO"; then
    echo "ERROR: $SO 含别家标签 $s" >&2
    exit 1
  fi
done
if "$READELF" -d "$SO" | grep -q "libc++_shared.so"; then
  echo "ERROR: 静态 so 仍依赖 libc++_shared.so" >&2
  exit 1
fi

echo "[build_so_static] done -> $SO ($(stat -f%z "$SO") B, md5=$(md5 -q "$SO"))"
