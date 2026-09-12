#!/usr/bin/env bash
# 弹力果冻 TapTap 渠道 管线 build_so 阶段后端：私有 Android_tlgd.mk 直编
# 产物 = mod菜单/starry-mod-menu/libs_tlgd/arm64-v8a/libstarry_magic.so
#
# 范本 = 04_scripts/魂坠深境/hzsj_build_so.sh（四件套：绝对路径 / obj stamp /
#        链接活动保险 / 菜单串按字节校验），不自行发明。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OUT_LIBS="$STARRY_SRC/libs_tlgd"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"
OBJ="$STARRY_SRC/obj_tlgd"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$JNI/Android_tlgd.mk" ]] || { echo "ERROR: missing $JNI/Android_tlgd.mk" >&2; exit 1; }

STAMP="$OBJ/.build_stamp"
# ---- obj stamp：mk 内容 / 开关变化 → 清 obj 强制全量重编 ----
CFG_HASH="$( (cat "$JNI/Android_tlgd.mk" "$JNI/Application.mk"; \
  printf 'PROBE=%s\nTRACE=%s\n' "${STARRY_TLGD_PROBE:-1}" "${STARRY_TRACE:-}") \
  | shasum -a 256 | awk '{print $1}')"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] 构建配置变化（mk/开关）→ 清 obj_tlgd 强制全量重编"
  rm -rf "$OBJ"
fi

# NDK_OUT/NDK_LIBS_OUT 必须绝对路径（相对路径会被 ndk-build -C 解析到 app/ 下 → 假成功）
SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_tlgd.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$OUT_LIBS" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 8)" || BUILD_RC=$?
NEW_O="$(find "$OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1)"
rm -f "$T0"
[[ $BUILD_RC -eq 0 ]] || { echo "ERROR: ndk-build rc=$BUILD_RC" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: build produced no $SO" >&2; exit 1; }

# ---- 链接活动保险：有新 .o 但 so 未变 → 假成功 ----
if [[ -n "$NEW_O" && "$(md5 -q "$SO")" == "$SO_MD5_BEFORE" ]]; then
  echo "ERROR: 本次构建产生了新 .o 但 $SO 未变化——旧 so 顶包嫌疑，禁止通过" >&2
  exit 1
fi
mkdir -p "$OBJ" && echo "$CFG_HASH" > "$STAMP"

# ---- 防呆：按字节校验菜单串/标签（strings 不认 UTF-8 中文）----
NEW_MENU=$(LC_ALL=C grep -ac "Category_战斗" "$SO" || true)
[[ "$NEW_MENU" -ge 1 ]] || { echo "ERROR: $SO 不含 Category_战斗（旧 so 混入或源码未编译）" >&2; exit 1; }
NEW_FEAT=$(LC_ALL=C grep -ac "Toggle_全屏秒杀" "$SO" || true)
[[ "$NEW_FEAT" -ge 1 ]] || { echo "ERROR: $SO 不含功能菜单串（5 功能未编入）" >&2; exit 1; }
NEW_TAG=$(LC_ALL=C grep -ac "\[TLGDF\]" "$SO" || true)
[[ "$NEW_TAG" -ge 1 ]] || { echo "ERROR: $SO 不含 [TLGDF] 日志前缀" >&2; exit 1; }
NEW_FTAG=$(LC_ALL=C grep -ac "\[Tlgd\]" "$SO" || true)
[[ "$NEW_FTAG" -ge 1 ]] || { echo "ERROR: $SO 不含 [Tlgd] 框架标签字面量" >&2; exit 1; }
if LC_ALL=C grep -aq "\[Dwct\]" "$SO"; then
  echo "ERROR: $SO 含 [Dwct]（上一款游戏残留，jniLibs/mk 接错）" >&2; exit 1
fi
echo "[build_so] done → $SO"
md5 -q "$SO"
