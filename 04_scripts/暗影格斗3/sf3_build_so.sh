#!/usr/bin/env bash
# 暗影格斗3 1.45.3 — pipeline build_so 后端（私有 Android_sf3.mk 直编，modern 注册表）
# 产物 = mod菜单/starry-mod-menu/libs_sf3/arm64-v8a/libstarry_magic.so
set -euo pipefail

# ============================================================================
# 平台垫层（2026-11-24 builder：Windows Git Bash / macOS 双跑）
# 只改「路径解析 + 工具探测」，构建命令与全部校验条目与原脚本逐条一致。
# ============================================================================
NIXIANG="${NIXIANG_ROOT:-/Users/aliang/Documents/nixiang}"
# 非登录 shell（脚本直跑）不会加载 ~/.bashrc 的 NIXIANG_ROOT ⇒ 缺 workspace 时以脚本位置为准（双机同解）
if [ ! -d "$NIXIANG/01_tools" ]; then
  NIXIANG="$(cd "$(dirname "$0")/../.." && pwd)"
fi
case "$(uname -s)" in
  MINGW*|MSYS*|CYGWIN*) HOST_OS=win ;;
  *)                    HOST_OS=unix ;;
esac
# md5 -q：mac 自带；Git Bash 只有 md5sum
if command -v md5sum >/dev/null 2>&1; then
  md5() { if [ "${1:-}" = "-q" ]; then shift; md5sum "$@" | awk '{print $1}'; else md5sum "$@"; fi; }
fi
sha256stream() { if command -v sha256sum >/dev/null 2>&1; then sha256sum; else shasum -a 256; fi; }
fsize() { stat -c '%s' "$1" 2>/dev/null || stat -f '%z' "$1"; }
ncpu() { if command -v nproc >/dev/null 2>&1; then nproc; else sysctl -n hw.ncpu 2>/dev/null || echo 8; fi; }
winpath() { if [ "$HOST_OS" = "win" ] && command -v cygpath >/dev/null 2>&1; then cygpath -m -- "$1"; else printf '%s' "$1"; fi; }

STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
OUT_LIBS="$STARRY_SRC/libs_sf3"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"

# NDK 解析顺序：$ANDROID_NDK_HOME → 本仓 01_tools/android-sdk/ndk/android-ndk-r27c → mac 旧路径
# （取第一个带 ndk-build 的；Windows 包根目录只有 ndk-build.cmd，另有 build/ndk-build）
NDK=""
for cand in "${ANDROID_NDK_HOME:-}" \
            "$NIXIANG/01_tools/android-sdk/ndk/android-ndk-r27c" \
            "$HOME/Library/Android/sdk/ndk/27.0.12077973"; do
  [ -n "$cand" ] || continue
  if [ -x "$cand/ndk-build" ] || [ -f "$cand/ndk-build.cmd" ] || [ -f "$cand/build/ndk-build" ]; then
    NDK="$cand"; break
  fi
done
[ -n "$NDK" ] || { echo "ERROR: 找不到 NDK（ANDROID_NDK_HOME / 01_tools/android-sdk/ndk/android-ndk-r27c / mac 旧路径 均无）" >&2; exit 1; }
# Windows 侧 NDK 只带 ndk-build.cmd，sh 版跑不起来 ⇒ 沿 04_scripts/爆裂防线/blfx_build_so_win.sh 的配方
# 直调 prebuilt/windows-x86_64/bin/make.exe -f build/core/build-local.mk。
ndk_build() {
  if [ -x "$NDK/ndk-build" ]; then
    "$NDK/ndk-build" "$@"
  else
    "$NDK/prebuilt/windows-x86_64/bin/make.exe" -O -f "$NDK/build/core/build-local.mk" SHELL=cmd "$@"
  fi
}
echo "[build_so] NDK = $NDK ($HOST_OS)"

[ -f "$JNI/Android_sf3.mk" ] || { echo "ERROR: missing $JNI/Android_sf3.mk" >&2; exit 1; }

OBJ="$STARRY_SRC/obj_sf3"
STAMP="$OBJ/.build_stamp"
CFG_HASH="$( (cat "$JNI/Android_sf3.mk" "$JNI/Application.mk"; \
  printf 'TRACE=%s\n' "${STARRY_TRACE:-}") \
  | sha256stream | awk '{print $1}')"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] 构建配置变化 → 清 obj_sf3 强制全量重编"
  rm -rf "$OBJ"
fi

# Windows 侧 clang 收到非 ASCII 路径会全灭（仓库路径含「mod菜单」）⇒ ASCII 暂存目录构建后拷回。
# mac 上路径本来就是 ASCII，STAGED=0 走原地编译，行为与原脚本一致。
BUILD_JNI="$JNI"; BUILD_OBJ="$OBJ"; BUILD_LIBS="$OUT_LIBS"; STAGED=0
if [ "$HOST_OS" = "win" ] && [ "$(printf '%s' "$JNI" | LC_ALL=C tr -d '\000-\177' | wc -c | tr -d ' ')" != "0" ]; then
  STAGE_ROOT="$NIXIANG/.workbuddy/scratch/sf3_so"
  BUILD_JNI="$STAGE_ROOT/jni"; BUILD_OBJ="$STAGE_ROOT/obj"; BUILD_LIBS="$STAGE_ROOT/libs"
  STAGED=1
  echo "[build_so] 路径非 ASCII → ASCII 暂存构建：$STAGE_ROOT"
  rm -rf "$STAGE_ROOT"
  mkdir -p "$BUILD_JNI"
  cp -a "$JNI"/. "$BUILD_JNI"/
fi

SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
( cd "$BUILD_JNI" && ndk_build \
  NDK_PROJECT_PATH="$(winpath "$BUILD_JNI/../..")" \
  APP_BUILD_SCRIPT="$(winpath "$BUILD_JNI/Android_sf3.mk")" \
  NDK_APPLICATION_MK="$(winpath "$BUILD_JNI/Application.mk")" \
  NDK_OUT="$(winpath "$BUILD_OBJ")" \
  NDK_LIBS_OUT="$(winpath "$BUILD_LIBS")" \
  -j"$(ncpu)" ) || BUILD_RC=$?
NEW_O="$(find "$BUILD_OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1 || true)"
rm -f "$T0"
if [ "$STAGED" = "1" ]; then
  mkdir -p "$OUT_LIBS/arm64-v8a"
  for f in "$BUILD_LIBS/arm64-v8a"/*.so; do
    [ -f "$f" ] && cp -f "$f" "$OUT_LIBS/arm64-v8a/$(basename "$f")"
  done
fi
[[ $BUILD_RC -eq 0 ]] || { echo "ERROR: ndk-build rc=$BUILD_RC" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: build produced no $SO" >&2; exit 1; }

if [[ -n "$NEW_O" && "$(md5 -q "$SO")" == "$SO_MD5_BEFORE" ]]; then
  echo "ERROR: 本次构建产生了新 .o 但 $SO 未变化——旧 so 顶包嫌疑" >&2
  exit 1
fi
mkdir -p "$OBJ" && echo "$CFG_HASH" > "$STAMP"

# 标签/菜单串硬校验（防串包：so 必须只含本游戏菜单）
TAG_HIT=$(LC_ALL=C grep -ac "sf3" "$SO" || true)
[[ "$TAG_HIT" -ge 1 ]] || { echo "ERROR: $SO 不含 sf3 标签" >&2; exit 1; }
while IFS= read -r s; do
  HIT=$(LC_ALL=C grep -ac "$s" "$SO" || true)
  [[ "$HIT" -ge 1 ]] || { echo "ERROR: $SO 不含菜单串 $s" >&2; exit 1; }
done <<'STRINGS'
Category_功能
Slider_伤害倍率:1:100:1
Toggle_上帝模式
Slider_战斗速度:1:10:1
STRINGS
for bad in "Slider_攻击倍率" "Slider_战斗加速" "Toggle_百倍伤害" "Toggle_免广告"; do
  HIT=$(LC_ALL=C grep -ac "$bad" "$SO" || true)
  [[ "$HIT" -eq 0 ]] || { echo "ERROR: $SO 含其它游戏菜单串 $bad" >&2; exit 1; }
done

echo "[build_so] done → $SO ($(md5 -q "$SO"))"
echo "[build_so] size = $(fsize "$SO")"
