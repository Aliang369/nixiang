#!/usr/bin/env bash
# ================================================================
#  绝区零 (com.miHoYo.Nap 3.2.0 / vc429) — build_so 阶段后端
#  modern 注册表版：直编 Android_zzz.mk（不切共享 Android.mk）
#  产物：mod菜单/starry-mod-menu/libs_zzz/arm64-v8a/libstarry_magic.so
#
#  防呆四件套（照范本 04_scripts/魂坠深境/hzsj_build_so.sh）：
#   1 绝对路径 NDK_OUT / NDK_LIBS_OUT（相对路径会被解析到 app/ 下 → 假成功）
#   2 obj stamp：mk / Application.mk / 开关变化 → 清 obj 全量重编
#   3 链接活动保险：本次有新 .o 但 so 字节未变 = 旧 so 顶包 → FAIL
#   4 菜单串按字节校验（LC_ALL=C grep -a；strings 不认 UTF-8 中文）
#
#  Windows 专有两条（2026-10-02 实测，照 mxyzt_build_so.sh）：
#   A. 本机 NDK 27 **只有 ndk-build.cmd**，没有 bash 版 ndk-build
#      → 直调 $NDK/prebuilt/windows-x86_64/bin/make.exe -f build/core/build-local.mk
#   B. 仓库路径含中文（mod菜单）→ Windows 侧 clang 收到 GBK 乱码路径全灭
#      → 在 ASCII 暂存目录构建，再把 so 拷回 libs_zzz
# ================================================================
set -euo pipefail

SELF_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="${NIXIANG:-$(cd "$SELF_DIR/../.." && pwd)}"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
OUT_LIBS="$STARRY_SRC/libs_zzz"
OBJ="$STARRY_SRC/obj_zzz"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"
MK="$JNI/Android_zzz.mk"
APP_MK="$JNI/Application.mk"
TAG="zzz"

hash256() { if command -v sha256sum >/dev/null 2>&1; then sha256sum | awk '{print $1}'; else shasum -a 256 | awk '{print $1}'; fi; }
md5f()    { if command -v md5sum   >/dev/null 2>&1; then md5sum "$1" | awk '{print $1}'; else md5 -q "$1"; fi; }
ncpu()    { if command -v nproc >/dev/null 2>&1; then nproc; else sysctl -n hw.ncpu 2>/dev/null || echo 8; fi; }
winpath() { if command -v cygpath >/dev/null 2>&1; then cygpath -m -- "$1"; else echo "$1"; fi; }

ndk_ok() {
  [ -x "$1/ndk-build" ] && return 0
  [ -x "$1/prebuilt/windows-x86_64/bin/make.exe" ] && [ -f "$1/build/core/build-local.mk" ] && return 0
  return 1
}

resolve_ndk() {
  local base v d
  if [ -n "${LOCALAPPDATA:-}" ]; then
    base="$(cygpath -u "$LOCALAPPDATA" 2>/dev/null || echo "$LOCALAPPDATA")/Android/Sdk/ndk"
  else
    base="$HOME/Library/Android/sdk/ndk"
  fi
  for v in 27.0.12077973 26.3.11579264 29.0.14033849; do
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
  NDK="$(resolve_ndk)" || { echo "ERROR: 找不到可用 NDK（试过 \$ANDROID_NDK_HOME 与 \$LOCALAPPDATA/Android/Sdk/ndk）" >&2; exit 1; }
fi

ndk_build() {
  if [ -x "$NDK/ndk-build" ]; then
    "$NDK/ndk-build" "$@"
  else
    "$NDK/prebuilt/windows-x86_64/bin/make.exe" -O \
      -f "$NDK/build/core/build-local.mk" SHELL=cmd "$@"
  fi
}

[ -f "$MK" ]     || { echo "ERROR: 缺 $MK" >&2; exit 1; }
[ -f "$APP_MK" ] || { echo "ERROR: 缺 $APP_MK" >&2; exit 1; }

# ---- 构建工作区：路径含非 ASCII（中文）时必须用 ASCII 暂存目录 ----
BUILD_JNI="$JNI"; BUILD_OUT="$OBJ"; BUILD_LIBS="$OUT_LIBS"; STAGED=0
if [ "$(printf '%s' "$JNI" | LC_ALL=C tr -d '\000-\177' | wc -c | tr -d ' ')" != "0" ]; then
  if command -v cygpath >/dev/null 2>&1; then
    STAGE_ROOT="$(cygpath -u "${LOCALAPPDATA:-/tmp}")/Temp/${TAG}_build"
  else
    STAGE_ROOT="/tmp/${TAG}_build"
  fi
  BUILD_JNI="$STAGE_ROOT/jni"; BUILD_OUT="$STAGE_ROOT/obj"; BUILD_LIBS="$STAGE_ROOT/libs"
  STAGED=1
fi

echo "[build_so] NDK   = $NDK"
echo "[build_so] SRC   = $STARRY_SRC"
echo "[build_so] OUT   = $SO"
if [ "$STAGED" = "1" ]; then
  echo "[build_so] 路径非 ASCII → ASCII 暂存构建：$STAGE_ROOT"
  mkdir -p "$BUILD_JNI"
  cp -a "$JNI"/. "$BUILD_JNI"/
  BUILD_MK="$BUILD_JNI/Android_zzz.mk"; BUILD_APP_MK="$BUILD_JNI/Application.mk"
else
  BUILD_MK="$MK"; BUILD_APP_MK="$APP_MK"
fi

# ---- obj stamp ----
CFG_HASH="$( (cat "$MK" "$APP_MK"; printf 'TRACE=%s\nDIAG=%s\n' \
  "${STARRY_TRACE:-}" "${STARRY_DIAG:-}") | hash256 )"
STAMP="$OBJ/.build_stamp"
if [ -f "$STAMP" ] && [ "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]; then
  echo "[stamp] 构建配置变化（mk/开关）→ 清 obj 强制全量重编"
  rm -rf "$OBJ" "$BUILD_OUT"
fi

SO_MD5_BEFORE="none"
[ -f "$SO" ] && SO_MD5_BEFORE="$(md5f "$SO")"
T0="$(mktemp)"; touch "$T0"

BUILD_RC=0
( cd "$BUILD_JNI" && ndk_build \
  NDK_PROJECT_PATH="$(winpath "$BUILD_JNI/../..")" \
  APP_BUILD_SCRIPT="$(winpath "$BUILD_MK")" \
  NDK_APPLICATION_MK="$(winpath "$BUILD_APP_MK")" \
  NDK_OUT="$(winpath "$BUILD_OUT")" \
  NDK_LIBS_OUT="$(winpath "$BUILD_LIBS")" \
  -j"$(ncpu)" ) || BUILD_RC=$?

NEW_O="$(find "$BUILD_OUT" -name '*.o' -newer "$T0" 2>/dev/null | head -1 || true)"
rm -f "$T0"
[ "$BUILD_RC" -eq 0 ] || { echo "ERROR: ndk-build rc=$BUILD_RC" >&2; exit 1; }

if [ "$STAGED" = "1" ]; then
  mkdir -p "$OUT_LIBS/arm64-v8a"
  for f in "$BUILD_LIBS/arm64-v8a"/*.so; do
    [ -f "$f" ] && cp -f "$f" "$OUT_LIBS/arm64-v8a/$(basename "$f")"
  done
fi

[ -f "$SO" ] || { echo "ERROR: 构建未产出 $SO" >&2; exit 1; }

# ---- 链接活动保险 ----
if [ -n "$NEW_O" ] && [ "$(md5f "$SO")" = "$SO_MD5_BEFORE" ]; then
  echo "ERROR: 本次构建产生了新 .o 但 $SO 字节未变 —— 旧 so 顶包嫌疑，禁止通过" >&2
  exit 1
fi

mkdir -p "$OBJ" && echo "$CFG_HASH" > "$STAMP"

# ---- 菜单串按字节校验（2026-10-07 用户「移动速度功能删除」：四项，禁扩张） ----
check_menu() {
  local s="$1" n
  n="$(LC_ALL=C grep -ac "$s" "$SO" 2>/dev/null || true)"
  [ -n "$n" ] || n=0
  [ "$n" -ge 1 ] || { echo "ERROR: $SO 不含菜单串「$s」—— 源文件未编译进去或旧 so 混入" >&2; exit 1; }
  echo "[check] 菜单串 OK: $s (x$n)"
}
check_menu "Toggle_无敌"
check_menu "Toggle_无限技能"
check_menu "Slider_伤害倍率"
check_menu "Toggle_定怪"
if LC_ALL=C grep -aq "Slider_移动速度" "$SO" 2>/dev/null; then
  echo "ERROR: $SO 仍含已删除的移动速度菜单串 —— 旧 so 混入或菜单表未换" >&2
  exit 1
fi
LC_ALL=C grep -aq '\[jql\]' "$SO" || { echo "ERROR: $SO 缺 [jql] 标签" >&2; exit 1; }
LC_ALL=C grep -aq 'libil2cpp.so' "$SO" || { echo "ERROR: $SO 缺 libil2cpp.so 等待库名" >&2; exit 1; }

echo "[build_so] done → $SO"
echo "[build_so] size = $(wc -c <"$SO" | tr -d ' ') B"
echo "[build_so] md5  = $(md5f "$SO")"
echo "[build_so] sha256 = $( (sha256sum "$SO" 2>/dev/null || shasum -a 256 "$SO") | awk '{print $1}')"
