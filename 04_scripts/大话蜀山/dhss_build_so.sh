#!/usr/bin/env bash
# 大话蜀山 管线 build_so 阶段后端：私有 Android_dhss.mk 直编（modern 注册表）
# 产物 = mod菜单/starry-mod-menu/libs_dhss/arm64-v8a/libstarry_magic.so
# 范本：04_scripts/魂坠深境/hzsj_build_so.sh（绝对路径 NDK_OUT + obj stamp + 链接活动保险 + 菜单串字节校验）
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OUT_LIBS="$STARRY_SRC/libs_dhss"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$JNI/Android_dhss.mk" ]] || { echo "ERROR: missing $JNI/Android_dhss.mk" >&2; exit 1; }

OBJ="$STARRY_SRC/obj_dhss"
STAMP="$OBJ/.build_stamp"
# ---- obj stamp：mk / Application.mk / 探针开关变化 → 清 obj 强制全量重编 ----
CFG_HASH="$( (cat "$JNI/Android_dhss.mk" "$JNI/Application.mk"; \
  printf 'PROBE=%s\nTRACE=%s\n' "${STARRY_DHSS_PROBE:-}" "${STARRY_TRACE:-}") \
  | shasum -a 256 | awk '{print $1}')"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] 构建配置变化（mk/开关）→ 清 obj_dhss 强制全量重编"
  rm -rf "$OBJ"
fi

SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_dhss.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$OUT_LIBS" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 8)" || BUILD_RC=$?
NEW_O="$(find "$OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1)"
rm -f "$T0"
[[ $BUILD_RC -eq 0 ]] || { echo "ERROR: ndk-build rc=$BUILD_RC" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: build produced no $SO" >&2; exit 1; }

# ---- 链接活动保险：有新 .o 但 so 未变 → 旧 so 顶包嫌疑 ----
if [[ -n "$NEW_O" && "$(md5 -q "$SO")" == "$SO_MD5_BEFORE" ]]; then
  echo "ERROR: 本次构建产生了新 .o 但 $SO 未变化——旧 so 顶包嫌疑，禁止通过" >&2
  exit 1
fi
mkdir -p "$OBJ" && echo "$CFG_HASH" > "$STAMP"

# ---- 防呆：本游戏标签与菜单串必须编进 so（中文按字节搜，strings 默认 ASCII 会漏）----
TAG_HITS=$(LC_ALL=C grep -ac "dhss" "$SO" || true)
[[ "$TAG_HITS" -ge 1 ]] || { echo "ERROR: $SO 不含 dhss 标签（旧 so 混入或源码未编译）" >&2; exit 1; }
for menu in "Toggle_秒杀" "Toggle_无敌" "Toggle_免广告" "ServerStep" "ApplyDamage" "OnGameAdNotify"; do
  HITS=$(LC_ALL=C grep -ac "$menu" "$SO" || true)
  [[ "$HITS" -ge 1 ]] || { echo "ERROR: $SO 不含 ${menu}（源码未编译或挂点表缺失）" >&2; exit 1; }
done

echo "[build_so] so=$SO"
echo "[build_so] md5=$(md5 -q "$SO")"
echo "[build_so] sha256=$(shasum -a 256 "$SO" | awk '{print $1}')"
echo "[build_so] done"
