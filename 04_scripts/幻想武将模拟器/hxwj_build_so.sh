#!/usr/bin/env bash
# 幻想武将模拟器 1.0.8 管线 build_so 后端：私有 Android_hxwj.mk 直编（modern 注册表）
# 产物 = mod菜单/starry-mod-menu/libs_hxwj/arm64-v8a/libstarry_magic.so
#
# 注意：jniLibs/arm64-v8a/libstarry_magic.so 是 gradle 打包时实际取用的那份，
# 且被所有并行游戏会话共享 —— surgical 脚本会在 gradle 之后按字节校验归属，
# 此处只负责产出 libs_hxwj 并同步到 jniLibs。
#
#  普通构建： bash 04_scripts/幻想武将模拟器/hxwj_build_so.sh
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OUT_LIBS="$STARRY_SRC/libs_hxwj"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$JNI/Android_hxwj.mk" ]] || { echo "ERROR: missing $JNI/Android_hxwj.mk" >&2; exit 1; }

OBJ="$STARRY_SRC/obj_hxwj"
STAMP="$OBJ/.build_stamp"
CFG_HASH="$( (cat "$JNI/Android_hxwj.mk" "$JNI/Application.mk"; \
  printf 'TRACE=%s\n' "${STARRY_TRACE:-}") \
  | shasum -a 256 | awk '{print $1}')"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] 构建配置变化（mk/开关）→ 清 obj_hxwj 强制全量重编"
  rm -rf "$OBJ"
fi

SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_hxwj.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$OUT_LIBS" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 8)" || BUILD_RC=$?
NEW_O="$(find "$OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1)"
rm -f "$T0"
[[ $BUILD_RC -eq 0 ]] || { echo "ERROR: ndk-build rc=$BUILD_RC" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: build produced no $SO" >&2; exit 1; }

# 链接活动保险：有新 .o 但 so 字节未变 → 旧 so 顶包嫌疑
if [[ -n "$NEW_O" && "$(md5 -q "$SO")" == "$SO_MD5_BEFORE" ]]; then
  echo "ERROR: 本次构建产生了新 .o 但 $SO 未变化——旧 so 顶包嫌疑，禁止通过" >&2
  exit 1
fi
mkdir -p "$OBJ" && echo "$CFG_HASH" > "$STAMP"

# ---- 归属防呆：本作串必须在、别家串必须不在（跨会话共享目录，禁止凭 mtime 判断）----
# 中文 UTF-8 多字节，strings 默认 ASCII 会漏报 ⇒ LC_ALL=C grep -a 按字节搜
for s in "Toggle_秒杀" "Toggle_百分百掉落" "Toggle_免广告" \
         "hxwjmlq" "FightGwItem" "ToponVideo"; do
  n="$(LC_ALL=C grep -ac -- "$s" "$SO" || true)"
  [[ "$n" -ge 1 ]] || { echo "ERROR: $SO 不含本作串 ${s}（旧 so 混入或源码未编译）" >&2; exit 1; }
done
n="$(LC_ALL=C grep -ac -- "\[hxwj\]" "$SO" || true)"
[[ "$n" -ge 1 ]] || { echo "ERROR: $SO 不含 [hxwj] 日志标签" >&2; exit 1; }
for s in "[Gyzw]" "[Ysxz]" "[Dwct]" "[Tlgd]" "[Blfx]"; do
  if LC_ALL=C grep -aqF -- "$s" "$SO"; then
    echo "ERROR: $SO 含别家标签 ${s}（libs_hxwj 被并行会话覆盖）" >&2
    exit 1
  fi
done

# ---- 同步到 gradle 实际取用的 jniLibs（共享路径，必须字节一致）----
SO_MD5="$(md5 -q "$SO")"
for d in "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
         "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
         "$STARRY_SRC/app/libs/arm64-v8a"; do
  [[ -d "$d" ]] || continue
  cp -f "$SO" "$d/libstarry_magic.so"
done
for d in "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
         "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
         "$STARRY_SRC/app/libs/arm64-v8a"; do
  [[ -f "$d/libstarry_magic.so" ]] || continue
  m="$(md5 -q "$d/libstarry_magic.so")"
  [[ "$m" == "$SO_MD5" ]] || { echo "ERROR: $d 同步失败（$m != ${SO_MD5}）" >&2; exit 1; }
done

echo "[build_so] done → $SO ($SO_MD5, $(stat -f%z "$SO") B)；jniLibs 已同步"
