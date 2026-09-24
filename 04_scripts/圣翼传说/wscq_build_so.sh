#!/usr/bin/env bash
# 圣翼传说 管线 build_so 阶段后端：私有 Android_wscq.mk 直编（modern 注册表，不切共享 mk）
# 产物 = mod菜单/starry-mod-menu/libs_wscq/arm64-v8a/libstarry_magic.so
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OUT_LIBS="$STARRY_SRC/libs_wscq"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$JNI/Android_wscq.mk" ]] || { echo "ERROR: missing $JNI/Android_wscq.mk" >&2; exit 1; }

OBJ="$STARRY_SRC/obj_wscq"
STAMP="$OBJ/.build_stamp"
# ---- obj stamp：mk 内容 / 探针 / trace 开关变化 → 清 obj 强制全量重编 ----
CFG_HASH="$( (cat "$JNI/Android_wscq.mk" "$JNI/Application.mk"; \
  printf 'PROBE=%s\nTRACE=%s\nSMOKE=%s\n' "${STARRY_WSCQ_PROBE:-}" "${STARRY_TRACE:-}" "${WSCQ_SMOKE_ALL_ON:-}") \
  | shasum -a 256 | awk '{print $1}')"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] 构建配置变化（mk/开关）→ 清 obj_wscq 强制全量重编"
  rm -rf "$OBJ"
fi

# 独立 obj_wscq（并行会话互踩防护）；切游戏无需清共享 app/obj
# NDK_OUT/NDK_LIBS_OUT 用绝对路径：相对路径会被 -C 的 cwd 解析到 app/ 下（假成功教训）
SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_wscq.mk" \
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

# 防呆：so 必须含本作菜单串（旧 so 混装假成功教训）。
# 中文是 UTF-8 多字节，strings 默认 ASCII 会漏报——必须 LC_ALL=C grep -a 按字节搜
for s in "SeekBar_伤害倍率_1_1000_1" "Toggle_玩家无敌" "SeekBar_战斗加速_1_10_1" \
         "Toggle_怪物BOSS瞬刷" "Toggle_全屏攻击"; do
  n=$(LC_ALL=C grep -ac "$s" "$SO" || true)
  [[ "$n" -ge 1 ]] || { echo "ERROR: $SO 不含菜单串 $s（源码未编译或旧 so 混入）" >&2; exit 1; }
done
# 校验 hook 锚点标签已编译进 so（防 Mod.cpp 没被链进去）
for t in "EnemyGetHurt" "ReviveBoss" "EnemyCtr2Fixed" "AntiSpeed"; do
  n=$(LC_ALL=C grep -ac "$t" "$SO" || true)
  [[ "$n" -ge 1 ]] || { echo "ERROR: $SO 不含锚点标签 $t" >&2; exit 1; }
done
echo "[build_so] done → $SO ($(md5 -q "$SO"))"
