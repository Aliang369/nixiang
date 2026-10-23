#!/usr/bin/env bash
# 勇士之心 1.0.4 管线 build_so 后端：Android_ysxz.mk 直编。
set -euo pipefail

NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)}"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OUT_LIBS="$STARRY_SRC/libs_ysxz"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$JNI/Android_ysxz.mk" ]] || { echo "ERROR: missing $JNI/Android_ysxz.mk" >&2; exit 1; }
[[ -f "$JNI/Games/YsxzPatchJs.h" ]] || { echo "ERROR: missing YsxzPatchJs.h" >&2; exit 1; }

# YSXZ_MENU_SRC=<dir>：出包前把该目录下的菜单四件套同步进 jni/Games。
# 用途：加密壳代（1.0.4 / 1.0.6）用本树的 SecShell/ExitTrace 骨架，但菜单与 JS 逻辑要走
# 功能最全的那份源码（04_scripts/勇士之心/ysxz_1.0.3_menu）。不设则构建现有树内文件。
if [[ -n "${YSXZ_MENU_SRC:-}" ]]; then
  [[ -d "$YSXZ_MENU_SRC" ]] || { echo "ERROR: YSXZ_MENU_SRC 不是目录: $YSXZ_MENU_SRC" >&2; exit 1; }
  for f in YsxzEntry.cpp YsxzMod.cpp YsxzMod.h YsxzPatchJs.h; do
    [[ -f "$YSXZ_MENU_SRC/$f" ]] || { echo "ERROR: 缺少 $YSXZ_MENU_SRC/$f" >&2; exit 1; }
    cp -f "$YSXZ_MENU_SRC/$f" "$JNI/Games/$f"
  done
  echo "[build_so] menu sources synced from $YSXZ_MENU_SRC"
fi

# YSXZ_SKIP_GEN=1：不从 斌哥注入脚本 重新生成 patch 头（该脚本是 1.0.4 线的单一基线）。
# 启用时要求 YsxzPatchJs.h 由 ysxz_anticheat/build_ac_patch.py 产出（含 ac_layer + auto_layer
# 与全部功能 SHIM），随后仍跑 verify 门禁，防止菜单序号与 SHIM 串号。
if [[ "${YSXZ_SKIP_GEN:-0}" == "1" ]]; then
  echo "[build_so] 跳过 gen_ysxz_patch_header（使用 build_ac_patch.py 产出的 patch 头）"
  python3.11 "$NIXIANG/04_scripts/勇士之心/verify_ysxz_patch_js.py"
else
  python3.11 "$NIXIANG/04_scripts/勇士之心/gen_ysxz_patch_header.py"
  python3.11 "$NIXIANG/04_scripts/勇士之心/verify_ysxz_patch_js.py"
fi

OBJ="$STARRY_SRC/obj_ysxz"
STAMP="$OBJ/.build_stamp"
CFG_HASH="$( (cat "$JNI/Android_ysxz.mk" "$JNI/Application.mk" \
  "$JNI/Games/YsxzMod.cpp" "$JNI/Games/YsxzMod.h" "$JNI/Games/YsxzEntry.cpp" \
  "$JNI/Games/YsxzPatchJs.h"; printf 'TRACE=%s\n' "${STARRY_TRACE:-}") \
  | shasum -a 256 | awk '{print $1}')"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] 构建配置变化 → 清 obj_ysxz 强制全量重编"
  rm -rf "$OBJ"
fi

SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_ysxz.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$OUT_LIBS" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 8)" || BUILD_RC=$?
NEW_O="$(find "$OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1)"
rm -f "$T0"
[[ $BUILD_RC -eq 0 ]] || { echo "ERROR: ndk-build rc=$BUILD_RC" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: build produced no $SO" >&2; exit 1; }

if [[ -n "$NEW_O" && "$(md5 -q "$SO")" == "$SO_MD5_BEFORE" ]]; then
  # 区分两种「so 没变」：
  #   a) 源码/配置没变（CFG_HASH 与上次一致）→ 重编产出同一字节，属**正常可复现**，放行；
  #   b) 配置变了却还产出同一个 so → 多半没真正重编（旧产物/路径错），必须拦。
  if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" == "$CFG_HASH" ]]; then
    echo "[build_so] 源码/配置未变，重编产出同一 so（字节可复现）→ 放行"
  else
    echo "ERROR: 配置变化后仍产出同一 so，疑似未真正重编，禁止通过" >&2
    exit 1
  fi
fi
mkdir -p "$OBJ" && echo "$CFG_HASH" > "$STAMP"

for token in "ysxz" "yszzx" "starry_ysxz_v1" "Slider_伤害倍率" "Toggle_秒杀" "Toggle_无敌" "Toggle_无限技能" "Slider_移动速度" "Toggle_自动战斗" \
             "Toggle_解锁金卡" "Category_自动化" "Toggle_自动重复挑战" "Toggle_自动出售" "Toggle_自动分解" "Toggle_自动穿戴" \
             "Slider_战斗加速"; do
  HITS=$(LC_ALL=C grep -ac "$token" "$SO" || true)
  [[ "$HITS" -ge 1 ]] || { echo "ERROR: $SO 不含 $token" >&2; exit 1; }
done

cp -f "$SO" "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so"

echo "[build_so] so=$SO"
echo "[build_so] md5=$(md5 -q "$SO")"
echo "[build_so] sha256=$(shasum -a 256 "$SO" | awk '{print $1}')"
echo "[build_so] done"
