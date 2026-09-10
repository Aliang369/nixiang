#!/usr/bin/env bash
# 造梦无双 管线 build_so 阶段后端：私有 Android_zmws.mk 直编
# 产物 = mod菜单/starry-mod-menu/libs_zmws/arm64-v8a/libstarry_magic.so
# 防呆设施（2026-09-10 实战教训固化，同 hzsj_build_so.sh）：
#   1. NDK_OUT/NDK_LIBS_OUT 绝对路径（相对路径被 -C 解析到 app/ 下 → 旧 so 假成功）
#   2. obj stamp：mk/探针/trace 开关变化 → 清 obj 强制全量重编（make 不追踪 cflags，探针泄漏教训）
#   3. 链接活动保险：有新 .o 但 so 字节没变 → FAIL（旧 so 顶包嫌疑）
#   4. 菜单串校验：LC_ALL=C grep -a 按字节搜（strings 不认 UTF-8 中文）
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OUT_LIBS="$STARRY_SRC/libs_zmws"
OBJ="$STARRY_SRC/obj_zmws"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$JNI/Android_zmws.mk" ]] || { echo "ERROR: missing $JNI/Android_zmws.mk" >&2; exit 1; }

# ---- obj stamp：mk 内容 / 探针 / trace 开关变化 → 清 obj 强制全量重编 ----
# 背景（2026-09-10 造梦无双教训）：make 只看文件 mtime，不追踪 cflags——
# 探针版构建后去掉 STARRY_ZMWS_PROBE 重编，旧 .o（含 probe dump）被原样复用泄漏进"干净版"。
STAMP="$OBJ/.build_stamp"
CFG_HASH="$( (cat "$JNI/Android_zmws.mk" "$JNI/Application.mk"; \
  printf 'PROBE=%s\nTRACE=%s\n' "${STARRY_ZMWS_PROBE:-}" "${STARRY_TRACE:-}") \
  | shasum -a 256 | awk '{print $1}')"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] 构建配置变化（mk/开关）→ 清 obj_zmws 强制全量重编"
  rm -rf "$OBJ"
fi

SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_zmws.mk" \
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

# 防呆：so 必须含菜单串（中文 UTF-8 多字节，LC_ALL=C grep -a 按字节搜）
for s in "Toggle_无限技能" "Toggle_超级秒杀"; do
  N=$(LC_ALL=C grep -ac "$s" "$SO" || true)
  [[ "$N" -ge 1 ]] || { echo "ERROR: $SO 不含 $s（旧 so 混入或源码未编译）" >&2; exit 1; }
done
echo "[build_so] done → $SO ($(md5 -q "$SO"))"
