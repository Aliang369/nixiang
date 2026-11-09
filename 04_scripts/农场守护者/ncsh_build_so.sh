#!/usr/bin/env bash
# ================================================================
#  农场守护者 (com.qld.ncsh.xiyou 2.1.21 / vc21) — build_so 阶段
#  modern 注册表版：直编 Android_ncsh.mk（不切共享 Android.mk）
#  产物：mod菜单/starry-mod-menu/libs_ncsh/arm64-v8a/libstarry_magic.so
#
#  防呆（照 04_scripts/魂坠深境/hzsj_build_so.sh 范本）：
#   1 绝对路径 NDK_OUT / NDK_LIBS_OUT（相对路径被解析到 app/ 下 → 假成功）
#   2 obj stamp：mk / Application.mk 变化 → 清 obj 全量重编
#   3 链接活动保险：本次有新 .o 但 so 字节未变 = 旧 so 顶包 → FAIL
#   4 菜单串按字节校验（LC_ALL=C grep -a；strings 不认 UTF-8 中文）
# ================================================================
set -euo pipefail

SELF_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="${NIXIANG:-$(cd "$SELF_DIR/../.." && pwd)}"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
OUT_LIBS="$STARRY_SRC/libs_ncsh"
OBJ="$STARRY_SRC/obj_ncsh"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"
MK="$JNI/Android_ncsh.mk"
APP_MK="$JNI/Application.mk"

hash256() { if command -v sha256sum >/dev/null 2>&1; then sha256sum | awk '{print $1}'; else shasum -a 256 | awk '{print $1}'; fi; }
md5f()    { if command -v md5sum   >/dev/null 2>&1; then md5sum "$1" | awk '{print $1}'; else md5 -q "$1"; fi; }
ncpu()    { if command -v nproc >/dev/null 2>&1; then nproc; else sysctl -n hw.ncpu 2>/dev/null || echo 8; fi; }

ndk_ok() { [ -x "$1/ndk-build" ]; }

resolve_ndk() {
  local base v d
  base="${HOME:-/Users/aliang}/Library/Android/sdk/ndk"
  for v in 27.0.12077973 26.3.11579264 26.1.10909125; do
    if ndk_ok "$base/$v"; then echo "$base/$v"; return 0; fi
  done
  for d in "$base"/*; do
    if ndk_ok "$d"; then echo "$d"; return 0; fi
  done
  return 1
}

if [ -n "${ANDROID_NDK_HOME:-}" ] && ndk_ok "$ANDROID_NDK_HOME"; then
  NDK="$ANDROID_NDK_HOME"
else
  NDK="$(resolve_ndk)" || { echo "ERROR: 找不到可用 NDK" >&2; exit 1; }
fi

[ -f "$MK" ]     || { echo "ERROR: 缺 $MK" >&2; exit 1; }
[ -f "$APP_MK" ] || { echo "ERROR: 缺 $APP_MK" >&2; exit 1; }

echo "[build_so] NDK = $NDK"
echo "[build_so] SRC = $STARRY_SRC"
echo "[build_so] OUT = $SO"

CFG_HASH="$( (cat "$MK" "$APP_MK") | hash256 )"
STAMP="$OBJ/.build_stamp"
if [ -f "$STAMP" ] && [ "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]; then
  echo "[stamp] 构建配置变化 → 清 obj 全量重编"
  rm -rf "$OBJ"
fi

SO_MD5_BEFORE="none"
[ -f "$SO" ] && SO_MD5_BEFORE="$(md5f "$SO")"
T0="$(mktemp)"; touch "$T0"

BUILD_RC=0
( cd "$JNI" && "$NDK/ndk-build" \
  NDK_PROJECT_PATH="$JNI/../.." \
  APP_BUILD_SCRIPT="$MK" \
  NDK_APPLICATION_MK="$APP_MK" \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$OUT_LIBS" \
  -j"$(ncpu)" ) || BUILD_RC=$?

NEW_O="$(find "$OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1 || true)"
rm -f "$T0"
[ "$BUILD_RC" -eq 0 ] || { echo "ERROR: ndk-build rc=$BUILD_RC" >&2; exit 1; }
[ -f "$SO" ] || { echo "ERROR: 构建未产出 $SO" >&2; exit 1; }

if [ -n "$NEW_O" ] && [ "$(md5f "$SO")" = "$SO_MD5_BEFORE" ]; then
  echo "ERROR: 本次产生新 .o 但 $SO 字节未变 —— 旧 so 顶包，禁止通过" >&2
  exit 1
fi

mkdir -p "$OBJ" && echo "$CFG_HASH" > "$STAMP"

check_menu() {
  local s="$1" n
  n="$(LC_ALL=C grep -ac "$s" "$SO" 2>/dev/null || true)"
  [ -n "$n" ] || n=0
  [ "$n" -ge 1 ] || { echo "ERROR: $SO 不含菜单串「${s}」" >&2; exit 1; }
  echo "[check] 菜单串 OK: $s (x$n)"
}
check_menu "Toggle_百倍伤害"
check_menu "Toggle_无敌"
check_menu "Toggle_无限技能"
check_menu "Toggle_解锁加速"
LC_ALL=C grep -aq '\[NCSH\]' "$SO" || { echo "ERROR: $SO 缺 [NCSH] 标签" >&2; exit 1; }

echo "[build_so] done → $SO"
echo "[build_so] size = $(wc -c <"$SO" | tr -d ' ') B"
echo "[build_so] md5  = $(md5f "$SO")"
