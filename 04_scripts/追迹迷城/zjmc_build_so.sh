#!/usr/bin/env bash
# 追迹迷城：构建 libstarry_magic.so，强制刷新 jniLibs 并校验游戏标签。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
STARRY_SRC="$NIXIANG_ROOT/mod菜单/starry-mod-menu"
JNI_DIR="$STARRY_SRC/app/src/main/jni"
MK="$JNI_DIR/Android.mk"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_BUILD="$NDK/ndk-build"
OBJ_DIR="$STARRY_SRC/obj_zjmc"
LIBS_DIR="$STARRY_SRC/libs_zjmc"

[[ -x "$NDK_BUILD" ]] || { echo "错误: ndk-build 不存在: $NDK_BUILD" >&2; exit 1; }
grep -Eq '^[[:space:]]*LOCAL_CFLAGS[[:space:]]*\+=[[:space:]]*-DENABLE_UNITY_HOOK[[:space:]]+-DSTARRY_GAME_ZJMC' "$MK" \
  || { echo "错误: Android.mk 未激活 STARRY_GAME_ZJMC" >&2; exit 1; }
grep -Eq '^[[:space:]]*LOCAL_SRC_FILES[[:space:]]*:=[[:space:]]*Main.cpp Games/ZjmcMod.cpp' "$MK" \
  || { echo "错误: Android.mk 未编入 Games/ZjmcMod.cpp" >&2; exit 1; }

rm -rf "$OBJ_DIR" "$LIBS_DIR"
(cd "$JNI_DIR" && \
  "$NDK_BUILD" \
    NDK_PROJECT_PATH="$STARRY_SRC/app" \
    APP_BUILD_SCRIPT="$JNI_DIR/Android.mk" \
    NDK_APPLICATION_MK="$JNI_DIR/Application.mk" \
    NDK_OUT="$OBJ_DIR" \
    NDK_LIBS_OUT="$LIBS_DIR" \
    -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")

SO="$LIBS_DIR/arm64-v8a/libstarry_magic.so"
[[ -f "$SO" ]] || { echo "错误: so 未产出: $SO" >&2; exit 1; }
LC_ALL=C grep -a -q '\[Zjmc\]' "$SO" || { echo "错误: so 缺少 [Zjmc]" >&2; exit 1; }
LC_ALL=C grep -a -q 'zjmc' "$SO" || { echo "错误: so 缺少 zjmc" >&2; exit 1; }
if LC_ALL=C grep -a -q '\[Cstjjs\]' "$SO"; then
  echo "错误: so 残留 [Cstjjs]" >&2
  exit 1
fi

for dest in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$SO" "$dest/libstarry_magic.so"
done

NM="${NDK}/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-nm"
if [[ -x "$NM" ]]; then
  "$NM" -D --defined-only "$SO" | grep -q 'JNI_OnLoad' \
    || { echo "错误: so 未导出 JNI_OnLoad" >&2; exit 1; }
fi

cat <<EOF
so=$SO
md5=$(md5 -q "$SO")
sha256=$(shasum -a 256 "$SO" | awk '{print $1}')
size=$(stat -f '%z' "$SO")
jniLibs=$STARRY_SRC/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so
EOF
