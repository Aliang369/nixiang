#!/usr/bin/env bash
# ================================================================
#  我本布衣：世家录 (com.JAGames.TalesoftheClan) — build_so 阶段
#  直编 Android_wbjy.mk（不切共享 Android.mk）
#  产物：mod菜单/starry-mod-menu/libs_wbjy/arm64-v8a/libstarry_magic.so
#
#  防呆四件套（照 04_scripts/魂坠深境/hzsj_build_so.sh）：
#   1 绝对路径 NDK_OUT / NDK_LIBS_OUT（相对路径会被解析到 app/ 下 → 假成功）
#   2 obj stamp：mk / Application.mk / 开关变化 → 清 obj 全量重编
#   3 链接活动保险：本次有新 .o 但 so 字节未变 = 旧 so 顶包 → FAIL
#   4 菜单串按字节校验（LC_ALL=C grep -a；strings 不认 UTF-8 中文）
#
#  Windows 专有两条：
#   A. 本机 NDK 27 **只有 ndk-build.cmd**，没有 bash 版 ndk-build
#      → 直调 $NDK/prebuilt/windows-x86_64/bin/make.exe -f build/core/build-local.mk
#   B. 仓库路径含中文（mod菜单）→ Windows 侧 clang 收到 GBK 乱码路径全灭
#      → 在 ASCII 暂存目录构建，再把 so 拷回 libs_wbjy
# ================================================================
set -euo pipefail

SELF_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="${NIXIANG:-$(cd "$SELF_DIR/../.." && pwd)}"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
OUT_LIBS="$STARRY_SRC/libs_wbjy"
OBJ="$STARRY_SRC/obj_wbjy"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"
MK="$JNI/Android_wbjy.mk"
APP_MK="$JNI/Application.mk"
TAG="wbjy"

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

# 构建戳：Windows 侧 ndk-build 以 SHELL=cmd 跑，Application.mk 里的 `git log | tr` 必失败
# ⇒ 在这里先算好并导出（git commit 时间；非 git 目录退化为当前时间），供 OTA 守卫比较。
if [ -z "${STARRY_BUILD_EPOCH:-}" ]; then
  STARRY_BUILD_EPOCH="$(git -C "$NIXIANG" log -1 --format=%ct 2>/dev/null | tr -cd '0-9' || true)"
  [ -n "$STARRY_BUILD_EPOCH" ] || STARRY_BUILD_EPOCH="$(date +%s)"
  export STARRY_BUILD_EPOCH
fi
echo "[build_so] epoch = $STARRY_BUILD_EPOCH"

# ---- 构建工作区：路径含非 ASCII（中文）时必须用 ASCII 暂存目录 ----
# 暂存根固定在工作区 .workbuddy/scratch（gitignored），不写系统盘
BUILD_JNI="$JNI"; BUILD_OUT="$OBJ"; BUILD_LIBS="$OUT_LIBS"; STAGED=0
if [ "$(printf '%s' "$JNI" | LC_ALL=C tr -d '\000-\177' | wc -c | tr -d ' ')" != "0" ]; then
  STAGE_ROOT="$NIXIANG/.workbuddy/scratch/${TAG}_build"
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
  # Windows 下 GNU make 匹配不到非 ASCII 目标名（E:/…/Games/我本布衣世家录Mod.cpp）
  # ⇒ 暂存区把源文件名 ASCII 化并改引用；只改暂存副本，仓库文件名不动。
  G="$BUILD_JNI/Games"
  cp -f "$G/我本布衣世家录Mod.cpp"   "$G/WbjyMod.cpp"
  cp -f "$G/我本布衣世家录Mod.h"     "$G/WbjyMod.h"
  cp -f "$G/我本布衣世家录Entry.cpp" "$G/WbjyEntry.cpp"
  sed -i 's/我本布衣世家录Mod\.h/WbjyMod.h/g' "$G/WbjyMod.cpp" "$G/WbjyEntry.cpp"
  sed -i 's#Games/我本布衣世家录Mod\.cpp#Games/WbjyMod.cpp#g; s#Games/我本布衣世家录Entry\.cpp#Games/WbjyEntry.cpp#g' \
    "$BUILD_JNI/Android_wbjy.mk"
  BUILD_MK="$BUILD_JNI/Android_wbjy.mk"; BUILD_APP_MK="$BUILD_JNI/Application.mk"
else
  BUILD_MK="$MK"; BUILD_APP_MK="$APP_MK"
fi

# ---- obj stamp：mk / Application.mk / 开关变化 → 清 obj 强制全量重编 ----
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

# ---- 菜单串按字节校验（六项功能，禁扩张） ----
check_menu() {
  local s="$1" n
  n="$(LC_ALL=C grep -ac "$s" "$SO" 2>/dev/null || true)"
  [ -n "$n" ] || n=0
  [ "$n" -ge 1 ] || { echo "ERROR: $SO 不含菜单串「${s}」—— 源文件未编译进去或旧 so 混入" >&2; exit 1; }
  echo "[check] 菜单串 OK: $s (x$n)"
}
check_menu "Toggle_免广告"
check_menu "Toggle_无限铜币"
check_menu "Toggle_无限元宝"
check_menu "Toggle_无限行动力"
check_menu "Toggle_无限科研点"
check_menu "Toggle_土地上限增加"
LC_ALL=C grep -aq '\[wbjy\]' "$SO" || { echo "ERROR: $SO 缺 [wbjy] 标签" >&2; exit 1; }
LC_ALL=C grep -aq 'libil2cpp.so' "$SO" || { echo "ERROR: $SO 缺 libil2cpp.so 等待库名" >&2; exit 1; }

echo "[build_so] done → $SO"
echo "[build_so] size = $(wc -c <"$SO" | tr -d ' ') B"
echo "[build_so] md5  = $(md5f "$SO")"
echo "[build_so] sha256 = $( (sha256sum "$SO" 2>/dev/null || shasum -a 256 "$SO") | awk '{print $1}')"
