#!/usr/bin/env bash
# 魂坠深境 管线 build_so 阶段后端：私有 Android_hzsj.mk 直编（modern 注册表迁移后不再切共享 mk）
# 产物 = mod菜单/starry-mod-menu/libs_hzsj/arm64-v8a/libstarry_magic.so
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OUT_LIBS="$STARRY_SRC/libs_hzsj"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$JNI/Android_hzsj.mk" ]] || { echo "ERROR: missing $JNI/Android_hzsj.mk" >&2; exit 1; }

OBJ="$STARRY_SRC/obj_hzsj"
STAMP="$OBJ/.build_stamp"
# ---- obj stamp：mk 内容 / 探针 / trace 开关变化 → 清 obj 强制全量重编 ----
# 背景（2026-09-10 造梦无双教训）：make 只看文件 mtime，不追踪 cflags——
# 探针版构建后去掉开关重编，旧 .o（含探针代码）被原样复用泄漏进"干净版"。
CFG_HASH="$( (cat "$JNI/Android_hzsj.mk" "$JNI/Application.mk"; \
  printf 'PROBE=%s\nTRACE=%s\n' "${STARRY_HZSJ_PROBE:-}" "${STARRY_TRACE:-}") \
  | shasum -a 256 | awk '{print $1}')"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] 构建配置变化（mk/开关）→ 清 obj_hzsj 强制全量重编"
  rm -rf "$OBJ"
fi

# 独立 obj_hzsj（并行会话互踩防护）；切游戏无需清共享 app/obj
# NDK_OUT/NDK_LIBS_OUT 用绝对路径：相对路径会被 -C 的 cwd 解析到 app/ 下（2026-09-10 假成功教训）
SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_hzsj.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$OUT_LIBS" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 8)" || BUILD_RC=$?
NEW_O="$(find "$OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1)"
rm -f "$T0"
[[ $BUILD_RC -eq 0 ]] || { echo "ERROR: ndk-build rc=$BUILD_RC" >&2; exit 1; }

[[ -f "$SO" ]] || { echo "ERROR: build produced no $SO" >&2; exit 1; }

# ---- 链接活动保险：本次确有编译活动（新 .o）但 so 字节没变 → 假成功嫌疑 ----
# 防的是"编了但没链到预期位置/旧 so 顶包"这类静默失败（无改动全量跳过的 no-op 构建不受影响）
if [[ -n "$NEW_O" && "$(md5 -q "$SO")" == "$SO_MD5_BEFORE" ]]; then
  echo "ERROR: 本次构建产生了新 .o 但 $SO 未变化——旧 so 顶包嫌疑，禁止通过" >&2
  exit 1
fi
mkdir -p "$OBJ" && echo "$CFG_HASH" > "$STAMP"
# 防呆：so 必须含新功能菜单串（旧 so 混装假成功教训 2026-09-10）
# 注意：中文是 UTF-8 多字节，strings 默认 ASCII 会漏报——必须 LC_ALL=C grep -a 按字节搜
NEW_MENU=$(LC_ALL=C grep -ac "Toggle_秒杀" "$SO" || true)
[[ "$NEW_MENU" -ge 1 ]] || { echo "ERROR: $SO 不含 Toggle_秒杀（旧 so 混入或源码未编译）" >&2; exit 1; }
NEW_AW=$(LC_ALL=C grep -ac "Toggle_自动胜利" "$SO" || true)
[[ "$NEW_AW" -ge 1 ]] || { echo "ERROR: $SO 不含 Toggle_自动胜利（feat7 未编译进 so）" >&2; exit 1; }
echo "[build_so] done → $SO"
