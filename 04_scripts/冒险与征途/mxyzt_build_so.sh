#!/usr/bin/env bash
# ================================================================
#  冒险与征途 (com.mxyzt.gzmy 1.5.9 / vc159) — build_so 阶段后端
#  modern 注册表版：直编 Android_mxyzt.mk（不切共享 Android.mk）
#  产物：mod菜单/starry-mod-menu/libs_mxyzt/arm64-v8a/libstarry_magic.so
#
#  防呆四件套（照范本 04_scripts/魂坠深境/hzsj_build_so.sh）：
#   1 绝对路径 NDK_OUT / NDK_LIBS_OUT（相对路径会被解析到 app/ 下 → 假成功）
#   2 obj stamp：mk / Application.mk / 探针开关变化 → 清 obj 全量重编
#      （2026-09-10 造梦无双教训：make 只认 mtime，去探针后旧 .o 被原样复用 → 泄漏）
#   3 链接活动保险：本次有新 .o 但 so 字节未变 = 旧 so 顶包 → FAIL
#   4 菜单串按字节校验（LC_ALL=C grep -a；strings 不认 UTF-8 中文）
#
#  Windows 专有两条（2026-10-02 本机实测新增）：
#   A. 本机 Android SDK 的 NDK 27 **只有 ndk-build.cmd**，没有 bash 版 ndk-build
#      → 按 $NDK/build/ndk-build.cmd 的做法直调 prebuilt/windows-x86_64/bin/make.exe
#   B. 仓库路径含中文（mod菜单），Windows 版 clang 收到 GBK 乱码路径 →
#      "unable to open output file" 全灭 → 一律在 **ASCII 暂存目录** 里构建，
#      再把 so 拷回 libs_mxyzt（源树用 cp -a 保留 mtime，增量编译照常）
# ================================================================
set -euo pipefail

SELF_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="${NIXIANG:-$(cd "$SELF_DIR/../.." && pwd)}"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
OUT_LIBS="$STARRY_SRC/libs_mxyzt"
OBJ="$STARRY_SRC/obj_mxyzt"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"
MK="$JNI/Android_mxyzt.mk"
APP_MK="$JNI/Application.mk"

hash256() { if command -v sha256sum >/dev/null 2>&1; then sha256sum | awk '{print $1}'; else shasum -a 256 | awk '{print $1}'; fi; }
md5f()    { if command -v md5sum   >/dev/null 2>&1; then md5sum "$1" | awk '{print $1}'; else md5 -q "$1"; fi; }
ncpu()    { if command -v nproc >/dev/null 2>&1; then nproc; else sysctl -n hw.ncpu 2>/dev/null || echo 8; fi; }
winpath() { if command -v cygpath >/dev/null 2>&1; then cygpath -m -- "$1"; else echo "$1"; fi; }

# 一个 NDK 是否可用：bash 版 ndk-build（mac/Linux/完整 NDK）
# 或 Windows 版 build/core/build-local.mk + prebuilt make.exe（本机 win SDK 只带 .cmd）
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
  NDK="$(resolve_ndk)" || { echo "ERROR: 找不到可用 NDK（试过 \$ANDROID_NDK_HOME 与 \$LOCALAPPDATA/Android/Sdk/ndk；需要 ndk-build 或 prebuilt/windows-x86_64/bin/make.exe）" >&2; exit 1; }
fi

# ---- ndk_build：统一入口。有 bash 版 ndk-build 就用它；只有 Windows 版时
#      按 $NDK/build/ndk-build.cmd 的做法直调 prebuilt make.exe ----
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
# 注意 1：本机 Git Bash 的 grep 不接受 `[^ -~]`（Invalid regular expression）→ 用 tr 数非 ASCII 字节
# 注意 2：中文在 $STARRY_SRC（mod菜单）而不在 $NIXIANG（仓库根是 ASCII）→ 必须测 $JNI
if [ "$(printf '%s' "$JNI" | LC_ALL=C tr -d '\000-\177' | wc -c | tr -d ' ')" != "0" ]; then
  # 双机：win 用 cygpath 把 %LOCALAPPDATA% 转 POSIX；mac/linux 直接用 /tmp（无 cygpath）。
  if command -v cygpath >/dev/null 2>&1; then
    STAGE_ROOT="$(cygpath -u "${LOCALAPPDATA:-/tmp}")/Temp/mxyzt_build"
  else
    STAGE_ROOT="/tmp/mxyzt_build"
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
  BUILD_MK="$BUILD_JNI/Android_mxyzt.mk"; BUILD_APP_MK="$BUILD_JNI/Application.mk"
else
  BUILD_MK="$MK"; BUILD_APP_MK="$APP_MK"
fi

# ---- obj stamp ----
CFG_HASH="$( (cat "$MK" "$APP_MK"; printf 'PROBE=%s\nTRACE=%s\nDIAG=%s\n' \
  "${STARRY_MXYTZ_PROBE:-}" "${STARRY_TRACE:-}" "${STARRY_DIAG:-}") | hash256 )"
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

# ---- 暂存构建：把 so 拷回真仓库 ----
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

# ---- 菜单串按字节校验（防旧 so 混入 / featureList 未编进去） ----
check_menu() {
  local s="$1" n
  n="$(LC_ALL=C grep -ac "$s" "$SO" 2>/dev/null || true)"
  [ -n "$n" ] || n=0
  [ "$n" -ge 1 ] || { echo "ERROR: $SO 不含菜单串「${s}」—— 源文件未编译进去或旧 so 混入" >&2; exit 1; }
  echo "[check] 菜单串 OK: $s (x$n)"
}
check_menu "Toggle_秒杀小怪"
check_menu "Toggle_无敌"
check_menu "Toggle_全模式技能伤害增加"
check_menu "Toggle_免广告"

echo "[build_so] done → $SO"
echo "[build_so] md5 = $(md5f "$SO")"
