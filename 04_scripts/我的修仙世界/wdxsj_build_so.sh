#!/usr/bin/env bash
# 我的修仙世界 4.5 — pipeline build_so 后端（私有 Android_wdxsj.mk 直编，modern 注册表）
# 产物 = mod菜单/starry-mod-menu/libs_wdxsj/arm64-v8a/libstarry_magic.so
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OUT_LIBS="$STARRY_SRC/libs_wdxsj"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$JNI/Android_wdxsj.mk" ]] || { echo "ERROR: missing $JNI/Android_wdxsj.mk" >&2; exit 1; }

OBJ="$STARRY_SRC/obj_wdxsj"
STAMP="$OBJ/.build_stamp"
CFG_HASH="$( (cat "$JNI/Android_wdxsj.mk" "$JNI/Application.mk"; \
  printf 'TRACE=%s\n' "${STARRY_TRACE:-}") \
  | shasum -a 256 | awk '{print $1}')"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] 构建配置变化 → 清 obj_wdxsj 强制全量重编"
  rm -rf "$OBJ"
fi

SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_wdxsj.mk" \
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

TAG_HIT=$(LC_ALL=C grep -ac "wdxsj" "$SO" || true)
[[ "$TAG_HIT" -ge 1 ]] || { echo "ERROR: $SO 不含 wdxsj 标签" >&2; exit 1; }
for s in "Toggle_百倍伤害" "Toggle_无敌" "Toggle_无限货币道具"; do
  HIT=$(LC_ALL=C grep -ac "$s" "$SO" || true)
  [[ "$HIT" -ge 1 ]] || { echo "ERROR: $SO 不含菜单串 $s" >&2; exit 1; }
done
for bad in "Toggle_秒杀" "Toggle_掉落必掉" "Toggle_无限技能"; do
  HIT=$(LC_ALL=C grep -ac "$bad" "$SO" || true)
  [[ "$HIT" -eq 0 ]] || { echo "ERROR: $SO 含其它游戏菜单串 $bad" >&2; exit 1; }
done

echo "[build_so] done → $SO ($(md5 -q "$SO"))"
