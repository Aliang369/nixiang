#!/usr/bin/env bash
# 我的技能无限触发 1.6.3 (com.SmallMill.EnMaster, vc163) — libstarry_magic.so 构建（arm64-v8a）
#
# 底包 ABI：python3 04_scripts/_common/detect_abis.py <底包.apk> → arm64-v8a（只此一套）
# 产出：libs_wdjnwcf/arm64-v8a/libstarry_magic.so（同步 jniLibs + libs，Gradle 只打包 jniLibs）
#
# 用法：bash 04_scripts/我的技能无限触发/wdjnwcf_build_so.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
TAG="wdjnwcf"
ABI="arm64-v8a"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build 不存在: $NDK" >&2; exit 1; }

# NDK_OUT / NDK_LIBS_OUT 必须绝对路径（相对路径被 -C 解析到 app/ 下 → 旧 so 顶包假成功）
"$NDK/ndk-build" \
  -C "$STARRY_SRC/app/src/main/jni" \
  NDK_PROJECT_PATH=../.. \
  APP_BUILD_SCRIPT=./Android_wdjnwcf.mk \
  NDK_APPLICATION_MK=./Application.mk \
  APP_ABI="$ABI" \
  NDK_OUT="$STARRY_SRC/obj_${TAG}" \
  NDK_LIBS_OUT="$STARRY_SRC/libs_${TAG}" \
  -j8

SO="$STARRY_SRC/libs_${TAG}/$ABI/libstarry_magic.so"
[[ -f "$SO" ]] || { echo "ERROR: 未产出 $SO" >&2; exit 1; }

mkdir -p "$STARRY_SRC/app/src/main/jniLibs/$ABI" "$STARRY_SRC/app/src/main/libs/$ABI"
cp -f "$SO" "$STARRY_SRC/app/src/main/jniLibs/$ABI/libstarry_magic.so"
cp -f "$SO" "$STARRY_SRC/app/src/main/libs/$ABI/libstarry_magic.so"

echo "[selfcheck] so = $SO ($(stat -f%z "$SO") B, md5=$(md5 -q "$SO"))"
CNT_TAG=$(LC_ALL=C grep -ac "Wdjnwcf" "$SO" || true)
CNT_ID=$(LC_ALL=C grep -ac "wdjnwcf" "$SO" || true)
[[ "$CNT_TAG" -ge 1 && "$CNT_ID" -ge 1 ]] || { echo "ERROR: so 缺 Wdjnwcf/wdjnwcf 标签" >&2; exit 1; }
c=$(LC_ALL=C grep -ac "Category_功能" "$SO" || true)
[[ "$c" -ge 1 ]] || { echo "ERROR: so 缺菜单串 Category_功能" >&2; exit 1; }
if LC_ALL=C grep -aq 'Bysxy\]\|Byyxx2\]\|Nhsy2\]\|Zjmc\]' "$SO"; then
  echo "ERROR: so 残留上一款游戏标签" >&2; exit 1
fi
echo "[done] $SO"
