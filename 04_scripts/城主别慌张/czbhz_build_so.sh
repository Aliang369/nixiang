#!/usr/bin/env bash
# 城主别慌张 管线 build_so 阶段后端：私有 Android_czbhz.mk 直编（modern 注册表，不切共享 mk）
# 产物 = mod菜单/starry-mod-menu/libs_czbhz/arm64-v8a/libstarry_magic.so
#
# 本作特殊性：无原生游戏库（H5/WebView），so 只提供菜单框架 + 功能位状态。
# 因此没有 RVA/序言校验可做，但"旧 so 混包"防线（obj stamp + 链接活动保险 +
# 菜单串按字节校验）一个都不能少。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OUT_LIBS="$STARRY_SRC/libs_czbhz"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$JNI/Android_czbhz.mk" ]] || { echo "ERROR: missing $JNI/Android_czbhz.mk" >&2; exit 1; }

OBJ="$STARRY_SRC/obj_czbhz"
STAMP="$OBJ/.build_stamp"
# ---- obj stamp：mk 内容 / 开关变化 → 清 obj 强制全量重编 ----
# 背景（2026-09-10 造梦无双教训）：make 只看 mtime 不追踪 cflags，
# 开关变化后旧 .o 会被原样复用，旧行为泄漏进新 so。
CFG_HASH="$( (cat "$JNI/Android_czbhz.mk" "$JNI/Application.mk"; \
  printf 'TRACE=%s\n' "${STARRY_TRACE:-}") \
  | shasum -a 256 | awk '{print $1}')"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] 构建配置变化（mk/开关）→ 清 obj_czbhz 强制全量重编"
  rm -rf "$OBJ"
fi

# 独立 obj_czbhz（并行会话互踩防护）；NDK_OUT/NDK_LIBS_OUT 必须绝对路径，
# 相对路径会被 -C 的 cwd 解析到 app/ 下 → 旧 so 顶包"假成功"（2026-09-10 事故）
SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_czbhz.mk" \
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

# ---- 防呆：so 必须含本作游戏标签与菜单串（旧 so 混装假成功教训 2026-09-10）----
# 中文是 UTF-8 多字节，strings 默认 ASCII 会漏报 → 必须 LC_ALL=C grep -a 按字节搜
chk() {  # chk <字节串> <说明>
  local n
  n=$(LC_ALL=C grep -ac "$1" "$SO" || true)
  [[ "$n" -ge 1 ]] || { echo "ERROR: $SO 不含 '$1'（$2；旧 so 混入或源码未编译）" >&2; exit 1; }
}
chk "czbhz"            "游戏标签未编译进 so"
chk "Toggle_免广告"     "feat1 未编译进 so"
chk "Toggle_无限技能"   "feat6 未编译进 so"
chk "SeekBar_移速_1_10_1" "feat5 菜单串未编译进 so"

echo "[build_so] done → $SO"
echo "[build_so] md5 = $(md5 -q "$SO")  size = $(wc -c < "$SO" | tr -d ' ')"
