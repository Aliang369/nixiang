#!/usr/bin/env bash
# ================================================================
#  暗影猎手:失落的世界 (com.enigma.shadowhunter.m4399 30.125.2.0 / vc3012502)
#  build_so 阶段后端 — modern 注册表版（直编 Android_ayxs.mk，不切共享 Android.mk）
#  产物：mod菜单/starry-mod-menu/libs_ayxs/arm64-v8a/libstarry_magic.so
#  防呆四件套：绝对路径 OUT / obj stamp / 链接活动保险 / 菜单串按字节校验
# ================================================================
set -euo pipefail

SELF_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="${NIXIANG:-$(cd "$SELF_DIR/../.." && pwd)}"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
OUT_LIBS="$STARRY_SRC/libs_ayxs"
OBJ="$STARRY_SRC/obj_ayxs"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"
MK="$JNI/Android_ayxs.mk"
APP_MK="$JNI/Application.mk"

resolve_ndk() {
  local base v d
  base="$HOME/Library/Android/sdk/ndk"
  for v in 27.0.12077973 26.3.11579264 29.0.14033849; do
    [ -x "$base/$v/ndk-build" ] && { echo "$base/$v"; return 0; }
  done
  for d in "$base"/*; do
    [ -x "$d/ndk-build" ] && { echo "$d"; return 0; }
  done
  return 1
}

if [ -n "${ANDROID_NDK_HOME:-}" ] && [ -x "${ANDROID_NDK_HOME}/ndk-build" ]; then
  NDK="$ANDROID_NDK_HOME"
else
  NDK="$(resolve_ndk)" || { echo "ERROR: 找不到可用 NDK" >&2; exit 1; }
fi

[ -f "$MK" ]     || { echo "ERROR: 缺 $MK" >&2; exit 1; }
[ -f "$APP_MK" ] || { echo "ERROR: 缺 $APP_MK" >&2; exit 1; }
[ -f "$JNI/Games/AyxsMod.cpp" ]   || { echo "ERROR: 缺 Games/AyxsMod.cpp" >&2; exit 1; }
[ -f "$JNI/Games/AyxsEntry.cpp" ] || { echo "ERROR: 缺 Games/AyxsEntry.cpp" >&2; exit 1; }

echo "[build_so] NDK = $NDK"
echo "[build_so] OUT = $SO"

mkdir -p "$OBJ" "$OUT_LIBS"
STAMP="${OBJ}/.stamp"
NEW_STAMP="$(cat "$MK" "$APP_MK" "$JNI/Games/AyxsMod.cpp" "$JNI/Games/AyxsMod.h" "$JNI/Games/AyxsEntry.cpp" | shasum -a 256 | cut -c1-64)"
if [ -f "$STAMP" ] && [ "$(cat "$STAMP")" = "$NEW_STAMP" ] && [ -f "$SO" ]; then
  echo "[build_so] 源码未变，复用已有 so（强制重编：rm -f $STAMP）"
else
  echo "[build_so] 源码变化 → 清 obj 全量重编"
  rm -rf "$OBJ"
  mkdir -p "$OBJ"
  echo "$NEW_STAMP" > "$STAMP"
fi

OLDM=0
[ -f "$SO" ] && OLDM="$(stat -f %m "$SO" 2>/dev/null || stat -c %Y "$SO")"

cd "$JNI"
"$NDK/ndk-build" NDK_PROJECT_PATH=../.. \
  APP_BUILD_SCRIPT=./Android_ayxs.mk \
  NDK_APPLICATION_MK=./Application.mk \
  APP_ABI="arm64-v8a" \
  NDK_OUT="$OBJ" NDK_LIBS_OUT="$OUT_LIBS" 2>&1 | tail -6

[ -f "$SO" ] || { echo "ERROR: 未产出 $SO" >&2; exit 1; }
NEWM="$(stat -f %m "$SO" 2>/dev/null || stat -c %Y "$SO")"
if [ "$NEWM" = "$OLDM" ]; then
  echo "ERROR: so mtime 未变化 —— 本次编译没有真正产出（旧 so 顶包）" >&2; exit 1
fi

fail=0
LC_ALL=C grep -aq '\[Ayxs\]' "$SO"        || { echo "ERROR: so 缺 [Ayxs] 标签" >&2; fail=1; }
LC_ALL=C grep -aq 'ayxssldsj' "$SO"       || { echo "ERROR: so 缺 authGameId(ayxssldsj)" >&2; fail=1; }
LC_ALL=C grep -aq 'Toggle_超级定怪' "$SO" || { echo "ERROR: so 缺菜单项 超级定怪" >&2; fail=1; }
LC_ALL=C grep -aq 'Toggle_超级恢复' "$SO" || { echo "ERROR: so 缺菜单项 超级恢复" >&2; fail=1; }
LC_ALL=C grep -aq 'Toggle_超级秒杀' "$SO" || { echo "ERROR: so 缺菜单项 超级秒杀" >&2; fail=1; }
if LC_ALL=C grep -aq '\[Tlgd\]' "$SO"; then
  echo "ERROR: so 仍含上一款 [Tlgd] 标签" >&2; fail=1
fi
[ "$fail" = "0" ] || exit 1

SIZE=$(stat -f %z "$SO" 2>/dev/null || stat -c %s "$SO")
MD5=$(shasum -a 256 "$SO" | cut -c1-64)
echo "[build_so] OK  $SO  ${SIZE} B  md5=$MD5"

for dest in "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" "$STARRY_SRC/app/src/main/libs/arm64-v8a" "$STARRY_SRC/app/libs/arm64-v8a"; do
  mkdir -p "$dest"; cp -f "$SO" "$dest/libstarry_magic.so"
done
echo "[build_so] jniLibs 已同步"
