#!/usr/bin/env bash
# 明日方舟 2.7.71 — pipeline build_so 后端（modern 注册表 + 私有 Android_mrfzz.mk）
# 产物：mod菜单/starry-mod-menu/libs_mrfzz/arm64-v8a/libstarry_magic.so
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OUT_LIBS="$STARRY_SRC/libs_mrfzz"
OBJ="$STARRY_SRC/obj_mrfzz"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"

[[ -x "$NDK/ndk-build" ]] || {
  echo "ERROR: ndk-build missing at $NDK" >&2
  exit 1
}
[[ -f "$JNI/Android_mrfzz.mk" ]] || {
  echo "ERROR: missing $JNI/Android_mrfzz.mk" >&2
  exit 1
}
[[ -f "$JNI/Games/ArknightsMod.cpp" ]] || {
  echo "ERROR: missing ArknightsMod.cpp" >&2
  exit 1
}
[[ -f "$JNI/Games/ArknightsEntry.cpp" ]] || {
  echo "ERROR: missing ArknightsEntry.cpp" >&2
  exit 1
}

STAMP="$OBJ/.build_stamp"
CFG_HASH="$(
  (
    cat "$JNI/Android_mrfzz.mk" "$JNI/Application.mk" \
      "$JNI/Games/ArknightsMod.h" "$JNI/Games/ArknightsMod.cpp" \
      "$JNI/Games/ArknightsEntry.cpp"
    printf 'TRACE=%s\n' "${STARRY_TRACE:-}"
  ) | shasum -a 256 | awk '{print $1}'
)"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] 构建配置/模块源码变化 -> 清 obj_mrfzz 强制全量重编"
  rm -rf "$OBJ"
fi

SO_STAT_BEFORE="$(stat -f '%m:%z' "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"
touch "$T0"
BUILD_RC=0
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_mrfzz.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$OUT_LIBS" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 8)" || BUILD_RC=$?

NEW_O="$(find "$OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1)"
rm -f "$T0"
[[ $BUILD_RC -eq 0 ]] || {
  echo "ERROR: ndk-build rc=$BUILD_RC" >&2
  exit 1
}
[[ -f "$SO" ]] || {
  echo "ERROR: build produced no $SO" >&2
  exit 1
}

if [[ -n "$NEW_O" && "$(stat -f '%m:%z' "$SO" 2>/dev/null || echo none)" == "$SO_STAT_BEFORE" ]]; then
  echo "ERROR: 本次构建产生了新 .o 但 $SO 未被重新链接——旧 so 顶包嫌疑" >&2
  exit 1
fi
mkdir -p "$OBJ"
echo "$CFG_HASH" > "$STAMP"

TAG_HIT="$(LC_ALL=C grep -ac "mrfzz" "$SO" || true)"
[[ "$TAG_HIT" -ge 1 ]] || {
  echo "ERROR: $SO 不含 mrfzz 标签" >&2
  exit 1
}
TAG_HIT="$(LC_ALL=C grep -ac "\\[mrfzz\\]" "$SO" || true)"
[[ "$TAG_HIT" -ge 1 ]] || {
  echo "ERROR: $SO 不含 [mrfzz] 日志标签" >&2
  exit 1
}

while IFS= read -r s; do
  HIT="$(LC_ALL=C grep -ac "$s" "$SO" || true)"
  [[ "$HIT" -ge 1 ]] || {
    echo "ERROR: $SO 不含菜单串 $s" >&2
    exit 1
  }
done <<'STRINGS'
Category_战斗
Toggle_超级攻速
Toggle_超级无敌
Toggle_基地锁血
Toggle_十倍伤害
Toggle_全图放置
Toggle_无限技能
Toggle_零部署费用
STRINGS

for bad in "[Endfield]" "Toggle_秒杀" "Toggle_无敌" "Toggle_玩家隐身" \
           "Toggle_全屏攻击" "Toggle_自动开技能"; do
  HIT="$(LC_ALL=C grep -Fac "$bad" "$SO" || true)"
  [[ "$HIT" -eq 0 ]] || {
    echo "ERROR: $SO 含其它游戏/旧模板菜单串 $bad" >&2
    exit 1
  }
done

echo "[build_so] done → $SO"
echo "[build_so] md5 = $(md5 -q "$SO")"
echo "[build_so] size = $(stat -f '%z' "$SO")"
