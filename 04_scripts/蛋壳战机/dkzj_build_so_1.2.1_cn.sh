#!/usr/bin/env bash
# 蛋壳战机 国服 1.2.1 (vc44, taptap) — 星光 native 构建（tag=dkzj，复用 DkzjMod 的 1.2.1 锚点表）。
#
# 与 dkzj_build_so.sh 的差异：
#   构建脚本 Android_dkzj121.mk（= Android_dkzj.mk + -DDKZJ_TARGET_1_2_1），
#   输出 libs_dkzj121 / obj_dkzj121，与私服 libs_dkzjsf 隔离（同轮不互踩）。
#   菜单/标签门禁与私服同（正向 5 项 + dkzj 标签；反向 4 个越界串 = 0，且不得含 dkzjsf）。
#
# 跨机注意（2026-10-28 Windows 实测）：NDK r27 在**非 ASCII 路径**下编译会
#   `unable to open output file ... No such file or directory`
#   ⇒ 当仓库根路径含非 ASCII 字符时，自动把 jni 树 stage 到 $STARRY_STAGE（默认 /c/nxbuild/dkzj121，
#     可用 STARRY_STAGE 覆盖）里编，产物再 .so 拷回仓库 libs_dkzj121。
#
# 用法：bash 04_scripts/蛋壳战机/dkzj_build_so_1.2.1_cn.sh
set -euo pipefail

NIXIANG="$(cd "$(dirname "$0")/../.." && pwd)"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
OUT_LIBS="$STARRY_SRC/libs_dkzj121"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"

md5_of() { if md5 -q /dev/null >/dev/null 2>&1; then md5 -q "$1"; else md5sum "$1" | cut -d' ' -f1; fi; }
sha256_stdin() { if shasum -a 256 /dev/null >/dev/null 2>&1; then shasum -a 256 | awk '{print $1}'; else sha256sum | cut -d' ' -f1; fi; }
size_of() { stat -c '%s' "$1" 2>/dev/null || stat -f '%z' "$1"; }
ncpu() { nproc 2>/dev/null || sysctl -n hw.ncpu 2>/dev/null || echo 8; }

NDK="${ANDROID_NDK_HOME:-}"
if [ -z "$NDK" ]; then
  for c in "$HOME/Library/Android/sdk/ndk/27.0.12077973" \
           "${LOCALAPPDATA:-}/Android/Sdk/ndk/27.0.12077973" \
           "/c/Users/Administrator/AppData/Local/Android/Sdk/ndk/27.0.12077973"; do
    if [ -d "$c" ]; then NDK="$c"; break; fi
  done
fi
[ -n "$NDK" ] && [ -d "$NDK" ] || { echo "ERROR: NDK not found (set ANDROID_NDK_HOME)" >&2; exit 1; }
NDK_BUILD="$NDK/ndk-build"
[ -x "$NDK_BUILD" ] || NDK_BUILD="$NDK/ndk-build.cmd"
[ -e "$NDK_BUILD" ] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }

for f in "$JNI/Android_dkzj121.mk" "$JNI/Games/DkzjMod.cpp" "$JNI/Games/DkzjMod.h" "$JNI/Games/DkzjEntry.cpp"; do
  [ -f "$f" ] || { echo "ERROR: missing $f" >&2; exit 1; }
done
LC_ALL=C grep -q -- "-DDKZJ_TARGET_1_2_1" "$JNI/Android_dkzj121.mk" \
  || { echo "ERROR: Android_dkzj121.mk 缺 -DDKZJ_TARGET_1_2_1（会误用 1.1.8 锚点）" >&2; exit 1; }
LC_ALL=C grep -q -- "kFileEntityCharacterChangeHp = 0x6D1B118" "$JNI/Games/DkzjMod.cpp" \
  || { echo "ERROR: DkzjMod.cpp 缺 1.2.1 锚点表" >&2; exit 1; }

CFG_HASH="$( (cat "$JNI/Android_dkzj121.mk" "$JNI/Application.mk" \
  "$JNI/Games/DkzjMod.cpp" "$JNI/Games/DkzjMod.h" "$JNI/Games/DkzjEntry.cpp"; \
  printf 'TRACE=%s\n' "${STARRY_TRACE:-}") | sha256_stdin)"

# 构建戳（Application.mk 的 STARRY_BUILD_EPOCH）：stage 目录没有 .git，
# 若不在脚本里显式传入，ndk-build 会退化到 1790184000 兜底戳 ⇒ OTA 守卫判据失真
# （pre_install_gate.sh check8 会拦）。这里统一从真实仓库取 commit 时间。
if [ -z "${STARRY_BUILD_EPOCH:-}" ]; then
  STARRY_BUILD_EPOCH="$(git -C "$NIXIANG" log -1 --format=%ct 2>/dev/null | tr -cd '0-9')"
fi
[ -n "${STARRY_BUILD_EPOCH:-}" ] || { echo "ERROR: 取不到 git commit 时间，请显式传 STARRY_BUILD_EPOCH" >&2; exit 1; }
export STARRY_BUILD_EPOCH
echo "[build_so 1.2.1-cn] STARRY_BUILD_EPOCH=$STARRY_BUILD_EPOCH"

# ---- 非 ASCII 路径：stage 到 ASCII 目录编译 ----
NONASCII=""
LC_ALL=C; case "$NIXIANG" in *[!-~]*) NONASCII=1 ;; esac; unset LC_ALL
if [ -n "$NONASCII" ] && [ "$(uname -s)" != "Darwin" ]; then
  STAGE="${STARRY_STAGE:-/c/nxbuild/dkzj121}"
  echo "[stage] 仓库路径含非 ASCII → 在 $STAGE 编译 jni 树"
  rm -rf "$STAGE"; mkdir -p "$STAGE/jni"
  ( cd "$JNI" && tar -cf - . ) | ( cd "$STAGE/jni" && tar -xf - )
  BUILD_JNI="$STAGE/jni"
  OBJ="$STAGE/obj"
  LIBS="$STAGE/libs"
  STAMP="$OBJ/.build_stamp"
  if [ -f "$STAMP" ] && [ "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]; then
    echo "[stamp] 构建配置变化 → 清 obj 强制全量重编"; rm -rf "$OBJ"
  fi
else
  BUILD_JNI="$JNI"
  OBJ="$STARRY_SRC/obj_dkzj121"
  LIBS="$OUT_LIBS"
  STAMP="$OBJ/.build_stamp"
  if [ -f "$STAMP" ] && [ "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]; then
    echo "[stamp] 构建配置变化 → 清 obj_dkzj121 强制全量重编"; rm -rf "$OBJ"
  fi
fi

SO_MD5_BEFORE="$(md5_of "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK_BUILD" -C "$BUILD_JNI" \
  NDK_PROJECT_PATH="$(cd "$BUILD_JNI/../.." && pwd)" \
  APP_BUILD_SCRIPT="./Android_dkzj121.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$LIBS" \
  -j"$(ncpu)" || BUILD_RC=$?
NEW_O="$(find "$OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1)"
rm -f "$T0"
[ "$BUILD_RC" -eq 0 ] || { echo "ERROR: ndk-build rc=$BUILD_RC" >&2; exit 1; }

BUILT="$LIBS/arm64-v8a/libstarry_magic.so"
[ -f "$BUILT" ] || { echo "ERROR: build produced no $BUILT" >&2; exit 1; }
if [ "$BUILT" != "$SO" ]; then
  mkdir -p "$OUT_LIBS/arm64-v8a"
  cp -f "$BUILT" "$SO"
  echo "[stage] 产物回填 → $SO"
fi
[ -f "$SO" ] || { echo "ERROR: missing $SO" >&2; exit 1; }

if [ -n "$NEW_O" ] && [ "$(md5_of "$SO")" = "$SO_MD5_BEFORE" ]; then
  echo "ERROR: 本次构建产生了新 .o 但 $SO 未变化——旧 so 顶包嫌疑" >&2
  exit 1
fi
mkdir -p "$OBJ" && echo "$CFG_HASH" > "$STAMP"

for s in "Category_功能" "Slider_攻击倍率:1:100:1" "Toggle_上帝模式" \
         "Toggle_偷盗技能" "Toggle_免广告"; do
  HIT="$(LC_ALL=C grep -ac "$s" "$SO" || true)"
  [ "${HIT:-0}" -ge 1 ] || { echo "ERROR: $SO 不含菜单串 $s" >&2; exit 1; }
done
for tag in "dkzj" "[dkzj]"; do
  HIT="$(LC_ALL=C grep -ac "$tag" "$SO" || true)"
  [ "${HIT:-0}" -ge 1 ] || { echo "ERROR: $SO 不含标签 $tag" >&2; exit 1; }
done
HIT="$(LC_ALL=C grep -ac "dkzjsf" "$SO" || true)"
[ "${HIT:-0}" -eq 0 ] || { echo "ERROR: $SO 含私服 game_id dkzjsf（串包）" >&2; exit 1; }
for bad in "Toggle_宝箱全拿" "Toggle_拿全部技能" "Slider_伤害倍率:1:10:1" "Toggle_立即胜利"; do
  HIT="$(LC_ALL=C grep -ac "$bad" "$SO" || true)"
  [ "${HIT:-0}" -eq 0 ] || { echo "ERROR: $SO 含越界菜单串 $bad" >&2; exit 1; }
done

echo "[build_so 1.2.1-cn] target=DKZJ_TARGET_1_2_1 (国服 vc44)"
echo "[build_so 1.2.1-cn] so   = $SO"
echo "[build_so 1.2.1-cn] md5  = $(md5_of "$SO")"
echo "[build_so 1.2.1-cn] size = $(size_of "$SO")"
