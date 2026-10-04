#!/usr/bin/env bash
# 勇士之心 1.0.3 专用：用「1.0.3 源码 + 1.0.4 菜单序号」重建 libstarry_magic.so。
#
# 为什么需要这个脚本：
#   1.0.3 的出包脚本 scripts/surgical_ysxz_1.0.3.sh 只组装 dex，.so 是**预置产物**
#   （$STARRY_SRC/libs_ysxz/arm64-v8a/libstarry_magic.so）。libs_*/ 被 .gitignore 忽略，
#   所以换机器后这份 so 不存在（或残留旧菜单那份），1.0.3 就会带上旧菜单。
#   本脚本从 git 里的 1.0.3 源码提交重建，并覆盖 04_scripts/勇士之心/ysxz_1.0.3_menu/
#   里那份「1.0.3 源码 + 新菜单表」的三个文件，产物直接落回 libs_ysxz。
#
# 用法：
#   bash 04_scripts/勇士之心/ysxz_build_so_103.sh
#   环境变量：ANDROID_NDK_HOME / YSXZ_NDK 指定 NDK；YSXZ_103_BASE 换基线提交（默认 81c70279）；
#            YSXZ_103_WORK 换临时构建目录；YSXZ_103_NO_COPY=1 只构建不落 libs_ysxz。
set -euo pipefail

NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)}"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI_REL="mod菜单/starry-mod-menu/app/src/main/jni"
MENU_DIR="$NIXIANG/04_scripts/勇士之心/ysxz_1.0.3_menu"
BASE_COMMIT="${YSXZ_103_BASE:-81c70279}"
WORK="${YSXZ_103_WORK:-$NIXIANG/.ysxz_103_build}"
OUT_LIBS="$STARRY_SRC/libs_ysxz"
JNIW="$WORK/jni"

# ---------- NDK / SDK 探测（macOS / Windows 都能跑，不写死机器路径） ----------
# ANDROID_NDK_HOME / YSXZ_NDK 优先；否则在 ANDROID_SDK_ROOT|ANDROID_HOME 和系统默认 SDK 目录下找 27.0.12077973
LOCALAPPDATA_U=""
[[ -n "${LOCALAPPDATA:-}" ]] && LOCALAPPDATA_U="$(cygpath -u "$LOCALAPPDATA" 2>/dev/null || echo "$LOCALAPPDATA")"
NDK=""
for cand in "${YSXZ_NDK:-}" "${ANDROID_NDK_HOME:-}" \
            "${ANDROID_SDK_ROOT:-}/ndk/27.0.12077973" "${ANDROID_HOME:-}/ndk/27.0.12077973" \
            "$HOME/Library/Android/sdk/ndk/27.0.12077973" \
            "$LOCALAPPDATA_U/Android/Sdk/ndk/27.0.12077973"; do
  [[ -n "$cand" && "$cand" == /* ]] || continue
  if [[ -x "$cand/ndk-build" || -f "$cand/ndk-build.cmd" ]]; then NDK="$cand"; break; fi
done
[[ -n "$NDK" ]] || { echo "ERROR: 找不到 NDK，请设 ANDROID_NDK_HOME（需要含 ndk-build 或 ndk-build.cmd）" >&2; exit 1; }
if [[ -x "$NDK/ndk-build" ]]; then NDKBIN="$NDK/ndk-build"; else NDKBIN="$NDK/ndk-build.cmd"; fi

JOBS="$(nproc 2>/dev/null || sysctl -n hw.ncpu 2>/dev/null || echo 8)"
# ndk-build.cmd 是 Windows 原生程序，路径必须用 Windows 形式
W_WIN="$(cygpath -m "$WORK" 2>/dev/null || echo "$WORK")"

echo "[103] NIXIANG   = $NIXIANG"
echo "[103] NDK       = $NDK"
echo "[103] base      = $BASE_COMMIT"
echo "[103] work      = $W_WIN"

for f in YsxzEntry.cpp YsxzMod.h YsxzPatchJs.h; do
  [[ -f "$MENU_DIR/$f" ]] || { echo "ERROR: missing $MENU_DIR/$f" >&2; exit 1; }
done

# ---------- 1. 从 1.0.3 源码提交导出 jni（落到 ASCII 路径，避免中文路径进 clang） ----------
rm -rf "$WORK"
mkdir -p "$WORK"
git -C "$NIXIANG" archive "$BASE_COMMIT" "$JNI_REL" | tar -x -C "$WORK" --strip-components=5
[[ -f "$JNIW/Android_ysxz.mk" ]] || { echo "ERROR: 导出失败，缺 $JNIW/Android_ysxz.mk" >&2; exit 1; }

# ---------- 2. 覆盖「新菜单」三个文件 ----------
cp -f "$MENU_DIR/YsxzEntry.cpp" "$JNIW/Games/YsxzEntry.cpp"
cp -f "$MENU_DIR/YsxzMod.h"     "$JNIW/Games/YsxzMod.h"
cp -f "$MENU_DIR/YsxzPatchJs.h" "$JNIW/Games/YsxzPatchJs.h"

# ---------- 3. 构建 ----------
"$NDKBIN" -C "$W_WIN/jni" \
  NDK_PROJECT_PATH="$W_WIN" \
  APP_BUILD_SCRIPT="./Android_ysxz.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$W_WIN/obj" \
  NDK_LIBS_OUT="$W_WIN/libs" \
  -j"$JOBS"

SO="$WORK/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO" ]] || { echo "ERROR: 构建产物缺失 $SO" >&2; exit 1; }

# ---------- 4. 断言：必须带新菜单，绝不能带旧菜单 ----------
for token in "Slider_伤害倍率:1:50:1" "Slider_移动速度:1:10:1" "Toggle_秒杀" "Toggle_无敌" "Toggle_无限技能" "Toggle_自动战斗"; do
  HITS="$(LC_ALL=C grep -ac -- "$token" "$SO" || true)"
  [[ "${HITS:-0}" -ge 1 ]] || { echo "ERROR: $SO 不含菜单串 $token" >&2; exit 1; }
done
if LC_ALL=C grep -aq -- "Slider_移动速度:1:5:1" "$SO"; then
  echo "ERROR: $SO 仍含旧菜单串 Slider_移动速度:1:5:1 —— 菜单表没换成功" >&2
  exit 1
fi

SZ="$(wc -c <"$SO" | tr -d ' ')"
if [[ "$SZ" -lt 120000 || "$SZ" -gt 160000 ]]; then
  echo "ERROR: $SO 体积异常（$SZ B），疑似编错目标" >&2; exit 1
fi
MD5="$(md5sum "$SO" 2>/dev/null | awk '{print $1}' || md5 -q "$SO")"
SHA="$(sha256sum "$SO" 2>/dev/null | awk '{print $1}' || shasum -a 256 "$SO" | awk '{print $1}')"
echo "[103] so     = $SO"
echo "[103] size   = $SZ B"
echo "[103] md5    = $MD5"
echo "[103] sha256 = $SHA"

# ---------- 5. 落回 1.0.3 出包脚本读取的预置 so 位置 ----------
if [[ "${YSXZ_103_NO_COPY:-0}" != "1" ]]; then
  mkdir -p "$OUT_LIBS/arm64-v8a"
  cp -f "$SO" "$OUT_LIBS/arm64-v8a/libstarry_magic.so"
  echo "[103] 已落 $OUT_LIBS/arm64-v8a/libstarry_magic.so"
fi
echo "[103] done"
