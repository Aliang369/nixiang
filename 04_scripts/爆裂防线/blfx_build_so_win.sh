#!/usr/bin/env bash
# ================================================================
#  爆裂防线 1.02.01 — build_so（Windows/MSYS 后端）
#
#  与 mac 侧 blfx_build_so.sh 等价，差异只在平台层：
#    A. 本机 NDK 27/29 只有 ndk-build.cmd，没有 bash 版 ndk-build
#       → 直调 prebuilt/windows-x86_64/bin/make.exe -f build/core/build-local.mk
#    B. 仓库路径含中文（mod菜单）→ Windows 侧 clang 收到乱码路径全灭
#       → 在 ASCII 暂存目录（.workbuddy/scratch，gitignored）构建后拷回
#    C. md5 -q / sysctl / stat -f → md5f / ncpu / filesize（blfx_win_common.sh）
#
#  产物：mod菜单/starry-mod-menu/libs_blfx/arm64-v8a/libstarry_magic.so
#  用法：bash 04_scripts/爆裂防线/blfx_build_so_win.sh
#        BLFX_PROBE=1 bash ...（只读探针构建）
# ================================================================
set -euo pipefail

HERE="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=blfx_win_common.sh
. "$HERE/blfx_win_common.sh"

TAG="blfx"
MK="$JNI_DIR/Android_blfx.mk"
APP_MK="$JNI_DIR/Application.mk"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"
OBJ="$STARRY_SRC/obj_$TAG"

[ -f "$MK" ]     || { echo "ERROR: 缺 $MK" >&2; exit 1; }
[ -f "$APP_MK" ] || { echo "ERROR: 缺 $APP_MK" >&2; exit 1; }
[ -n "$NDK_DIR" ] && [ -d "$NDK_DIR" ] || { echo "ERROR: 找不到 NDK（\$ANDROID_NDK_HOME / LOCALAPPDATA/Android/Sdk/ndk）" >&2; exit 1; }

ndk_build() {
  if [ -x "$NDK_DIR/ndk-build" ]; then
    "$NDK_DIR/ndk-build" "$@"
  else
    "$NDK_DIR/prebuilt/windows-x86_64/bin/make.exe" -O \
      -f "$NDK_DIR/build/core/build-local.mk" SHELL=cmd "$@"
  fi
}

# Application.mk 里的 `git log | tr` 在 SHELL=cmd 下必失败 → 先算好导出
if [ -z "${STARRY_BUILD_EPOCH:-}" ]; then
  STARRY_BUILD_EPOCH="$(git -C "$NIXIANG" log -1 --format=%ct 2>/dev/null | tr -cd '0-9' || true)"
  [ -n "$STARRY_BUILD_EPOCH" ] || STARRY_BUILD_EPOCH="$(date +%s)"
fi
export STARRY_BUILD_EPOCH
echo "[build_so] NDK   = $NDK_DIR"
echo "[build_so] epoch = $STARRY_BUILD_EPOCH"

# ---- ASCII 暂存（仅当 jni 路径含非 ASCII）--------------
BUILD_JNI="$JNI_DIR"; BUILD_OUT="$OBJ"; BUILD_LIBS="$OUT_LIBS"; STAGED=0
NONASCII="$(printf '%s' "$JNI_DIR" | LC_ALL=C tr -d '\000-\177' | wc -c | tr -d ' ')"
if [ "$NONASCII" != "0" ]; then
  STAGE_ROOT="$NIXIANG/.workbuddy/scratch/${TAG}_so"
  BUILD_JNI="$STAGE_ROOT/jni"; BUILD_OUT="$STAGE_ROOT/obj"; BUILD_LIBS="$STAGE_ROOT/libs"
  STAGED=1
  echo "[build_so] 路径非 ASCII → ASCII 暂存构建：$STAGE_ROOT"
  rm -rf "$STAGE_ROOT"
  mkdir -p "$BUILD_JNI"
  cp -a "$JNI_DIR"/. "$BUILD_JNI"/
fi
BUILD_MK="$BUILD_JNI/Android_blfx.mk"
BUILD_APP_MK="$BUILD_JNI/Application.mk"

# ---- obj stamp：mk / 开关变化 → 清 obj 强制全量重编 ----
CFG_HASH="$( (cat "$MK" "$APP_MK"; printf 'PROBE=%s\nTRACE=%s\n' \
  "${BLFX_PROBE:-}" "${STARRY_TRACE:-}") | (sha256sum 2>/dev/null || shasum -a 256) | awk '{print $1}')"
STAMP="$OBJ/.build_stamp"
if [ -f "$STAMP" ] && [ "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]; then
  echo "[stamp] 构建配置变化（mk/开关）→ 清 obj 强制全量重编"
  rm -rf "$OBJ" "$BUILD_OUT"
fi

MAKE_ARGS=()
if [ -n "${BLFX_PROBE:-}" ]; then
  MAKE_ARGS+=("BLFX_PROBE=1")
  echo "[build_so] 探针构建 BLFX_PROBE=1（0 hook / 0 写入）"
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
  "${MAKE_ARGS[@]+"${MAKE_ARGS[@]}"}" \
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

# ---- 链接活动保险：本次有新 .o 但 so 字节没变 = 旧 so 顶包 ----
if [ -n "$NEW_O" ] && [ "$(md5f "$SO")" = "$SO_MD5_BEFORE" ]; then
  echo "ERROR: 本次构建产生了新 .o 但 $SO 未变化——旧 so 顶包嫌疑，禁止通过" >&2
  exit 1
fi
mkdir -p "$OBJ" && echo "$CFG_HASH" > "$STAMP"

# ---- 菜单串 / 锚点串按字节校验（LC_ALL=C：中文 UTF-8 多字节）----
check_str() {
  local s="$1" n
  n="$(LC_ALL=C grep -ac "$s" "$SO" 2>/dev/null || true)"
  [ -n "$n" ] || n=0
  [ "$n" -ge 1 ] || { echo "ERROR: $SO 不含「${s}」——源码未编译进去或旧 so 混入" >&2; exit 1; }
  echo "[check] OK: $s (x$n)"
}
for s in "Category_战斗" "SeekBar_伤害倍率_1_100_1" "Toggle_无敌" "SeekBar_战斗加速_1_10_1"; do
  check_str "$s"
done
for s in "blfx_probe.txt" "zbSafeLong" "zbCombatSpeed" "ChangeHp" "zbCheatContext"; do
  check_str "$s"
done
if [ -n "${BLFX_PROBE:-}" ]; then
  check_str "PROBE round"
fi

echo "[build_so] done → $SO ($(md5f "$SO"), $(filesize "$SO") B)"
