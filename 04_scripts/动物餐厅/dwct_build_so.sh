#!/usr/bin/env bash
set -euo pipefail
HERE="$(cd "$(dirname "$0")" && pwd)"
source "$HERE/../_common/env.sh"
STAR="$NIXIANG_ROOT/mod菜单/starry-mod-menu"
NDK="${ANDROID_NDK_HOME:-$NIXIANG_ROOT/01_tools/android-sdk/ndk/android-ndk-r27c}"
STAGE="${LOCALAPPDATA:-/tmp}/Temp/dwct1242_build/native"
mkdir -p "$STAGE/jni"
# Isolated ASCII copy; never replace shared jniLibs.
cp -a "$STAR/app/src/main/jni/." "$STAGE/jni/"
NDKB="$NDK/ndk-build"
[[ -f "$NDKB" ]] || NDKB="$NDK/ndk-build.cmd"
"$NDKB" -C "$(nixiang_win_path "$STAGE/jni")" -B NDK_PROJECT_PATH=. APP_BUILD_SCRIPT=Android_dwct1242.mk NDK_APPLICATION_MK=Application.mk STARRY_BUILD_EPOCH="$(git -C "$NIXIANG_ROOT" log -1 --format=%ct)" APP_ABI="arm64-v8a x86_64 armeabi-v7a" NDK_OUT="$(nixiang_win_path "$STAGE/obj")" NDK_LIBS_OUT="$(nixiang_win_path "$STAGE/libs")" -j4
mkdir -p "$STAR/libs_dwct1242"
cp -a "$STAGE/libs/." "$STAR/libs_dwct1242/"
for ABI in arm64-v8a x86_64 armeabi-v7a; do
 SO="$STAR/libs_dwct1242/$ABI/libstarry_magic.so"
 LC_ALL=C grep -aq Dwct1242 "$SO"
 LC_ALL=C grep -aq STARRYSTAMP1:EPOCH= "$SO"
 sha256sum "$SO"
done
