#!/usr/bin/env bash
# 弹壳战机私服（com.wxdmxga.dkzj）1.2.1 — 星光 native 构建（tag=dkzjsf，复用 DkzjMod）。
#
# 与 dkzj_build_so.sh 的差异：
#   构建脚本 Android_dkzjsf.mk（入口 Games/DkzjSfEntry.cpp）、输出 libs_dkzjsf、obj_dkzjsf；
#   正向串增 "dkzjsf"（game_id），反向坏串同国服（宝箱全拿/拿全部技能/伤害倍率）。
#
# 用法：bash 04_scripts/蛋壳战机/dkzj_sf_build_so.sh
set -euo pipefail

NIXIANG="$(cd "$(dirname "$0")/../.." && pwd)"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OUT_LIBS="$STARRY_SRC/libs_dkzjsf"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
LC_ALL=C grep -q -- "-DDKZJ_TARGET_1_2_1" "$JNI/Android_dkzjsf.mk" \
  || { echo "ERROR: Android_dkzjsf.mk 缺 -DDKZJ_TARGET_1_2_1（会误用 1.1.8 锚点）" >&2; exit 1; }
LC_ALL=C grep -q -- "kFileEntityCharacterChangeHp = 0x6D1B118" "$JNI/Games/DkzjMod.cpp" \
  || { echo "ERROR: DkzjMod.cpp 缺 1.2.1 锚点表" >&2; exit 1; }
for f in "$JNI/Android_dkzjsf.mk" "$JNI/Games/DkzjMod.cpp" "$JNI/Games/DkzjSfEntry.cpp"; do
  [[ -f "$f" ]] || { echo "ERROR: missing $f" >&2; exit 1; }
done

OBJ="$STARRY_SRC/obj_dkzjsf"
STAMP="$OBJ/.build_stamp"
CFG_HASH="$( (cat "$JNI/Android_dkzjsf.mk" "$JNI/Application.mk" \
  "$JNI/Games/DkzjMod.cpp" "$JNI/Games/DkzjMod.h" "$JNI/Games/DkzjSfEntry.cpp"; \
  printf 'TRACE=%s\n' "${STARRY_TRACE:-}") \
  | shasum -a 256 | awk '{print $1}')"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] 构建配置变化 → 清 obj_dkzjsf 强制全量重编"
  rm -rf "$OBJ"
fi

SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_dkzjsf.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$OUT_LIBS" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 8)" || BUILD_RC=$?
NEW_O="$(find "$OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1)"
rm -f "$T0"
[[ $BUILD_RC -eq 0 ]] || { echo "ERROR: ndk-build rc=$BUILD_RC" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: build produced no $SO" >&2; exit 1; }

if [[ -n "$NEW_O" && "$(md5 -q "$SO")" == "$SO_MD5_BEFORE" ]]; then
  echo "ERROR: 本次构建产生了新 .o 但 $SO 未变化——旧 so 顶包嫌疑" >&2
  exit 1
fi
mkdir -p "$OBJ" && echo "$CFG_HASH" > "$STAMP"

for s in "Category_功能" "Slider_攻击倍率:1:100:1" "Toggle_上帝模式" \
         "Toggle_偷盗技能" "Toggle_免广告"; do
  HIT="$(LC_ALL=C grep -ac "$s" "$SO" || true)"
  [[ "$HIT" -ge 1 ]] || { echo "ERROR: $SO 不含菜单串 $s" >&2; exit 1; }
done
for tag in "dkzjsf" "[dkzj]"; do
  HIT="$(LC_ALL=C grep -ac "$tag" "$SO" || true)"
  [[ "$HIT" -ge 1 ]] || { echo "ERROR: $SO 不含标签 $tag" >&2; exit 1; }
done
for bad in "Toggle_宝箱全拿" "Toggle_拿全部技能" "Slider_伤害倍率:1:10:1" "Toggle_立即胜利"; do
  HIT="$(LC_ALL=C grep -ac "$bad" "$SO" || true)"
  [[ "$HIT" -eq 0 ]] || { echo "ERROR: $SO 含越界菜单串 $bad" >&2; exit 1; }
done

echo "[build_so sf] target=1.2.1"
echo "[build_so sf] done → $SO ($(md5 -q "$SO"))"
echo "[build_so sf] size = $(stat -f '%z' "$SO")"
