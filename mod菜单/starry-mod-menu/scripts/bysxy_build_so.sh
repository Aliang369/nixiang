#!/usr/bin/env bash
# 不一样西游 — libstarry_magic.so 构建（arm64-v8a）
#
# 产出：libs_bysxy/arm64-v8a/libstarry_magic.so
#      同步 app/src/main/jniLibs/arm64-v8a/libstarry_magic.so（Gradle 只打包 jniLibs）
#
# 用法：bash mod菜单/starry-mod-menu/scripts/bysxy_build_so.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
TAG="bysxy"
ABI="arm64-v8a"          # 底包 detect_abis.py → arm64-v8a

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build 不存在: $NDK" >&2; exit 1; }

# NDK_OUT / NDK_LIBS_OUT 必须绝对路径（相对路径被 -C 解析到 app/ 下 → 旧 so 顶包假成功）
"$NDK/ndk-build" \
  -C "$JNI" \
  NDK_PROJECT_PATH=../.. \
  APP_BUILD_SCRIPT=./Android_bysxy.mk \
  NDK_APPLICATION_MK=./Application.mk \
  APP_ABI="$ABI" \
  NDK_OUT="$STARRY_SRC/obj_$TAG" \
  NDK_LIBS_OUT="$STARRY_SRC/libs_$TAG" \
  -j8

SO="$STARRY_SRC/libs_$TAG/$ABI/libstarry_magic.so"
[[ -f "$SO" ]] || { echo "ERROR: 未产出 $SO" >&2; exit 1; }

mkdir -p "$STARRY_SRC/app/src/main/jniLibs/$ABI"
cp -f "$SO" "$STARRY_SRC/app/src/main/jniLibs/$ABI/libstarry_magic.so"
mkdir -p "$STARRY_SRC/app/src/main/libs/$ABI"
cp -f "$SO" "$STARRY_SRC/app/src/main/libs/$ABI/libstarry_magic.so"

# 自检（strings 默认 ASCII 不认 UTF-8 中文 → 一律按字节 grep -a）
echo "[selfcheck] so = $SO ($(stat -f%z "$SO") B, md5=$(md5 -q "$SO"))"
CNT_TAG=$(LC_ALL=C grep -ac "Bysxy" "$SO" || true)
CNT_ID=$(LC_ALL=C grep -ac "byyxy" "$SO" || true)
[[ "$CNT_TAG" -ge 1 && "$CNT_ID" -ge 1 ]] || { echo "ERROR: so 缺 Bysxy/byyxy 标签" >&2; exit 1; }
for s in 单次加成 一键四维 免广告 属性; do
  c=$(LC_ALL=C grep -ac "$s" "$SO" || true)
  [[ "$c" -ge 1 ]] || { echo "ERROR: so 缺菜单串 $s" >&2; exit 1; }
done
echo "[selfcheck] 标签与菜单串齐全（Bysxy=$CNT_TAG bysxy=${CNT_ID}）"
if LC_ALL=C grep -aq 'Dkzj\]\|Zzz\]\|Nhsy2\]\|Gjc2\]\|Byyxx2\]' "$SO"; then
  echo "ERROR: so 残留上一款游戏标签" >&2; exit 1
fi
echo "[done] $SO"
