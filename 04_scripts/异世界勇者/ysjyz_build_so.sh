#!/usr/bin/env bash
# 异世界勇者 管线 build_so 阶段后端：私有 Android_ysjyz.mk 直编（modern 注册表）
# 产物 = mod菜单/starry-mod-menu/libs_ysjyz/arm64-v8a/libstarry_magic.so
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OUT_LIBS="$STARRY_SRC/libs_ysjyz"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$JNI/Android_ysjyz.mk" ]] || { echo "ERROR: missing $JNI/Android_ysjyz.mk" >&2; exit 1; }

OBJ="$STARRY_SRC/obj_ysjyz"
STAMP="$OBJ/.build_stamp"
# ---- obj stamp：mk 内容 / trace 开关变化 → 清 obj 强制全量重编 ----
# （make 只看 mtime，不追踪 cflags；不改 stamp 会把旧 .o 原样复用到新配置）
CFG_HASH="$( (cat "$JNI/Android_ysjyz.mk" "$JNI/Application.mk"; \
  printf 'TRACE=%s\n' "${STARRY_TRACE:-}") \
  | shasum -a 256 | awk '{print $1}')"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] 构建配置变化（mk/开关）→ 清 obj_ysjyz 强制全量重编"
  rm -rf "$OBJ"
fi

# NDK_OUT/NDK_LIBS_OUT 必须绝对路径：相对路径会被 -C 的 cwd 解析到 app/ 下（假成功教训）
SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_ysjyz.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$OUT_LIBS" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 8)" || BUILD_RC=$?
NEW_O="$(find "$OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1)"
rm -f "$T0"
[[ $BUILD_RC -eq 0 ]] || { echo "ERROR: ndk-build rc=$BUILD_RC" >&2; exit 1; }

[[ -f "$SO" ]] || { echo "ERROR: build produced no $SO" >&2; exit 1; }

# ---- 链接活动保险：本次确有编译活动（新 .o）但 so 字节没变 → 假成功嫌疑 ----
if [[ -n "$NEW_O" && "$(md5 -q "$SO")" == "$SO_MD5_BEFORE" ]]; then
  echo "ERROR: 本次构建产生了新 .o 但 $SO 未变化——旧 so 顶包嫌疑，禁止通过" >&2
  exit 1
fi
mkdir -p "$OBJ" && echo "$CFG_HASH" > "$STAMP"

# ---- 防呆：so 必须含本游戏标签与新功能菜单串（旧 so 混装假成功防线）----
# 注意：中文是 UTF-8 多字节，strings 默认 ASCII 会漏报 —— 必须 LC_ALL=C grep -a 按字节搜
chk() {
  local n
  n="$(LC_ALL=C grep -ac "$1" "$SO" || true)"
  [[ "$n" -ge 1 ]] || { echo "ERROR: $SO 不含 [$1]（旧 so 混入或源码未编译）" >&2; exit 1; }
}
chk "ysjyz"
chk "Toggle_免广告"
chk "Toggle_无限技能"
chk "Toggle_无敌"
chk "SeekBar_伤害倍率"
chk "SeekBar_游戏倍速"
# v7 负向防呆：删除项/旧串绝不能残留（旧名还在 = 拿了旧源码或没改干净）
no() {
  local n
  n="$(LC_ALL=C grep -ac "$1" "$SO" || true)"
  [[ "$n" -eq 0 ]] || { echo "ERROR: $SO 仍含已删除串 [$1] ×$n" >&2; exit 1; }
}
no "Toggle_吸血拉满"
no "伤害倍率秒杀"
no "S.leech"
chk "clampResult"
chk "YsjYzHooks"
chk "starry_ysjyz_v1"

echo "[build_so] done → $SO"
echo "[build_so] md5 = $(md5 -q "$SO")"
