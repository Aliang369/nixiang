#!/usr/bin/env bash
# ================================================================
#  爆裂防线（com.muyegame.blfx）— Windows/MSYS 侧公共环境垫层
#
#  背景：仓库原有 blfx_*.sh 是 mac 专用（/Users/aliang、md5 -q、
#  darwin-x86_64、/usr/libexec/java_home、gradle assembleRelease）。
#  Windows 端不改原脚本（留作 mac 口径对照），另起 *_win.sh，
#  统一由本文件解析路径/工具/跨平台命令。
#
#  用法：脚本里 `. "$HERE/blfx_win_common.sh"`（HERE = 本目录）
#  被引入后提供：NIXIANG / GAME / VERSION / PKG / HOST
#                 md5f sha256f filesize ncpu winpath u2w
#                 STARRY_SRC JNI_DIR OUT_LIBS BUILD_TOOLS JAVA_HOME_J17
#                 NDK_DIR APKTOOL_JAR SMALI_BIN
# ================================================================
set -u

_BLFX_COMMON_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
NIXIANG="${NIXIANG:-$(cd "$_BLFX_COMMON_DIR/../.." && pwd)}"
GAME="爆裂防线"
VERSION="1.02.01"
PKG="com.muyegame.blfx"
TAG="blfx"

case "$(uname -s)" in
  MINGW*|MSYS*|CYGWIN*) HOST=win ;;
  *)                    HOST=unix ;;
esac

# env.sh 提供 NIXIANG_BUILD_TOOLS / NIXIANG_JAVA17 / NIXIANG_APKSIGNER / NIXIANG_ZIPALIGN
if [ "$HOST" = "win" ]; then
  # shellcheck source=../_common/env.sh
  . "$NIXIANG/04_scripts/_common/env.sh"
fi

STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI_DIR="$STARRY_SRC/app/src/main/jni"
OUT_LIBS="$STARRY_SRC/libs_blfx"
SURGICAL_DIR="$NIXIANG/07_patched/$GAME/surgical"

md5f() {
  if command -v md5sum >/dev/null 2>&1; then md5sum "$1" | awk '{print $1}'
  else md5 -q "$1"; fi
}
sha256f() {
  if command -v sha256sum >/dev/null 2>&1; then sha256sum "$1" | awk '{print $1}'
  else shasum -a 256 "$1" | awk '{print $1}'; fi
}
filesize() {
  if [ "$HOST" = "win" ]; then stat -c %s "$1"; else stat -f %z "$1"; fi
}
ncpu() {
  if command -v nproc >/dev/null 2>&1; then nproc
  else sysctl -n hw.ncpu 2>/dev/null || echo 8; fi
}
# 交给原生 Windows 程序的路径形式（C:/...）；mac 原样
winpath() {
  if command -v cygpath >/dev/null 2>&1; then cygpath -m -- "$1"; else printf '%s' "$1"; fi
}

# ---- 工具解析 --------------------------------------------------
resolve_build_tools() {
  if [ -n "${NIXIANG_BUILD_TOOLS:-}" ] && [ -d "$NIXIANG_BUILD_TOOLS" ]; then
    printf '%s' "$NIXIANG_BUILD_TOOLS"; return 0
  fi
  local base d
  if [ "$HOST" = "win" ] && [ -n "${LOCALAPPDATA:-}" ]; then
    base="$(cygpath -u "$LOCALAPPDATA" 2>/dev/null || echo "$LOCALAPPDATA")/Android/Sdk/build-tools"
  else
    base="$HOME/Library/Android/sdk/build-tools"
  fi
  for d in "$base"/*; do
    [ -d "$d" ] || continue
    if [ -f "$d/aapt2.exe" ] || [ -f "$d/aapt2" ]; then printf '%s' "$d"; return 0; fi
  done
  return 1
}

resolve_ndk() {
  local base v d
  if [ "$HOST" = "win" ] && [ -n "${LOCALAPPDATA:-}" ]; then
    base="$(cygpath -u "$LOCALAPPDATA" 2>/dev/null || echo "$LOCALAPPDATA")/Android/Sdk/ndk"
  else
    base="$HOME/Library/Android/sdk/ndk"
  fi
  for v in 27.0.12077973 29.0.14033849 26.3.11579264; do
    [ -d "$base/$v" ] && { printf '%s' "$base/$v"; return 0; }
  done
  for d in "$base"/*; do
    [ -d "$d" ] && { printf '%s' "$d"; return 0; }
  done
  return 1
}

BUILD_TOOLS="$(resolve_build_tools || true)"
JAVA_HOME_J17="${NIXIANG_JAVA17:-${JAVA_HOME:-}}"
if [ -n "${ANDROID_NDK_HOME:-}" ] && [ -d "$ANDROID_NDK_HOME" ]; then
  NDK_DIR="$ANDROID_NDK_HOME"
else
  NDK_DIR="$(resolve_ndk || true)"
fi
APKTOOL_JAR="$NIXIANG/01_tools/apktool/apktool.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
AAPT2_BIN=""
if [ -n "$BUILD_TOOLS" ]; then
  if [ -f "$BUILD_TOOLS/aapt2.exe" ]; then AAPT2_BIN="$BUILD_TOOLS/aapt2.exe"
  elif [ -f "$BUILD_TOOLS/aapt2" ]; then AAPT2_BIN="$BUILD_TOOLS/aapt2"; fi
fi

export NIXIANG GAME VERSION PKG HOST
export STARRY_SRC JNI_DIR OUT_LIBS SURGICAL_DIR
export BUILD_TOOLS NDK_DIR APKTOOL_JAR SMALI_BIN AAPT2_BIN JAVA_HOME_J17
