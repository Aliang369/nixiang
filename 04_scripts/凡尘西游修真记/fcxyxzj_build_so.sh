#!/usr/bin/env bash
# 凡尘西游修真记 管线 build_so 阶段后端：私有 Android_fcxyxzj.mk 直编（modern 注册表）
# 产物 = mod菜单/starry-mod-menu/libs_fcxyxzj/arm64-v8a/libstarry_magic.so
#
# 防呆四件套（照 04_scripts/魂坠深境/hzsj_build_so.sh 范本）：
#   绝对路径 NDK_OUT/LIBS_OUT + obj stamp（mk/开关变化清 obj 全量重编）
#   + 链接活动保险（有新 .o 而 so 未变 = 假成功 FAIL）+ 菜单串按字节校验
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OUT_LIBS="$STARRY_SRC/libs_fcxyxzj"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$JNI/Android_fcxyxzj.mk" ]] || { echo "ERROR: missing $JNI/Android_fcxyxzj.mk" >&2; exit 1; }

OBJ="$STARRY_SRC/obj_fcxyxzj"
STAMP="$OBJ/.build_stamp"
# ---- obj stamp：mk 内容 / trace 开关变化 → 清 obj 强制全量重编 ----
# make 只看 mtime 不追踪 cflags，旧 .o 复用会泄漏过期代码（造梦无双 2026-09-10 教训）
CFG_HASH="$( (cat "$JNI/Android_fcxyxzj.mk" "$JNI/Application.mk"; \
  printf 'TRACE=%s\n' "${STARRY_TRACE:-}") \
  | shasum -a 256 | awk '{print $1}')"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] 构建配置变化（mk/开关）→ 清 obj_fcxyxzj 强制全量重编"
  rm -rf "$OBJ"
fi

# 独立 obj_fcxyxzj（并行会话互踩防护）；NDK_OUT/NDK_LIBS_OUT 必须绝对路径
# （相对路径被 -C 的 cwd 解析到 app/ 下 → 校验到旧 so 假成功，2026-09-10 事故）
SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_fcxyxzj.mk" \
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

# ---- 防呆：so 必须含本游戏标签与菜单串（旧 so 混装假成功教训）----
# 中文 UTF-8 多字节：strings 默认 ASCII 会漏报 → LC_ALL=C grep -a 按字节搜
TAG_HIT=$(LC_ALL=C grep -ac "fcxyxzj" "$SO" || true)
[[ "$TAG_HIT" -ge 1 ]] || { echo "ERROR: $SO 不含 fcxyxzj 标签（旧 so 混入或源码未编译）" >&2; exit 1; }
for s in "Toggle_免广告" "Toggle_无限资源" "Toggle_物品不减" "Toggle_掉落翻倍"; do
  HIT=$(LC_ALL=C grep -ac "$s" "$SO" || true)
  [[ "$HIT" -ge 1 ]] || { echo "ERROR: $SO 不含菜单串 $s" >&2; exit 1; }
done
# 反查上一款游戏标签不得出现（菜单串错游戏防线）
for bad in "Toggle_秒杀" "Toggle_自动胜利"; do
  HIT=$(LC_ALL=C grep -ac "$bad" "$SO" || true)
  [[ "$HIT" -eq 0 ]] || { echo "ERROR: $SO 含其它游戏菜单串 $bad（mk 未切或旧 so）" >&2; exit 1; }
done

echo "[build_so] done → $SO ($(md5 -q "$SO"))"
