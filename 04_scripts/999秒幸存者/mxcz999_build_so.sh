#!/usr/bin/env bash
# ================================================================
#  999秒!幸存者 (com.HaloGames.Survivors999 V0.0866 / vc866) — build_so 阶段后端
#  modern 注册表版：直编私有 mk Android_mxcz999.mk（不切共享 Android.mk）
#  产物：mod菜单/starry-mod-menu/libs_mxcz999/arm64-v8a/libstarry_magic.so
#
#  ★ 绝不写入 app/src/main/jniLibs/（2026-11-15 现状：那里是并行会话 abssnmxt 的 [Abssnmxt] 产物）。
#    本游戏出包走外科 zip 直接取 libs_mxcz999 的 so，不依赖 gradle。
#
#  防呆四件套（照 04_scripts/魂坠深境/hzsj_build_so.sh 范本，勿自创）：
#   1 绝对路径 NDK_OUT / NDK_LIBS_OUT（相对路径被 ndk-build -C 解析到 app/ 下 → 校验到旧 so 假成功）
#   2 obj stamp：mk / Application.mk / 开关变化 → 清 obj 全量重编
#   3 链接活动保险：本次有新 .o 但 so 字节未变 = 旧 so 顶包 → FAIL
#   4 菜单串按字节校验（LC_ALL=C grep -a；strings 不认 UTF-8 中文）
#
#  Windows 专有两条（照 hzsj_build_so.sh / zzz_build_so.sh）：
#   A 本机 NDK r27 只有 ndk-build.cmd，没有 bash 版 ndk-build
#     → 直调 $NDK/prebuilt/windows-x86_64/bin/make.exe -f build/core/build-local.mk
#   B 仓库路径含中文（mod菜单）→ Windows 侧 clang 收到 GBK 乱码路径全灭
#     → 在 ASCII 暂存目录构建，再把 so 拷回 libs_mxcz999
#
#  用法:
#    bash 04_scripts/999秒幸存者/mxcz999_build_so.sh              # 真构建
#    bash 04_scripts/999秒幸存者/mxcz999_build_so.sh --dry-run     # 只打印解析结果，不构建
# ================================================================
set -euo pipefail

SELF_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="${NIXIANG:-$(cd "$SELF_DIR/../.." && pwd)}"
[ -f "$NIXIANG/04_scripts/_common/env.sh" ] && source "$NIXIANG/04_scripts/_common/env.sh"

DRY_RUN=0
for a in "$@"; do
  case "$a" in
    --dry-run) DRY_RUN=1 ;;
    *) echo "ERROR: 未知参数 ${a}（只支持 --dry-run）" >&2; exit 2 ;;
  esac
done

STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
MK="$JNI/Android_mxcz999.mk"
APP_MK="$JNI/Application.mk"
OUT_LIBS="$STARRY_SRC/libs_mxcz999"
OBJ="$STARRY_SRC/obj_mxcz999"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"
TAG="Mxcz999"
GAME_ID="999mxcz"
ABI="arm64-v8a"        # 底包只带 lib/arm64-v8a；Application.mk APP_ABI 同值

# ---- 菜单串：必须与 Mxcz999Entry.cpp 的 kMxcz999_Features[] 逐字一致（改菜单＝改两处）----
MENU_STRINGS=(
  "Category_战斗"
  "Toggle_无敌"
  "Slider_伤害倍率:1:100:1"
  "Slider_移动速度:1:5:1"
  "Toggle_解锁所有皮肤"
  "Toggle_无限解锁券"
  "Toggle_免广告"
)

hash256() { if command -v sha256sum >/dev/null 2>&1; then sha256sum | awk '{print $1}'; else shasum -a 256 | awk '{print $1}'; fi; }
md5f()    { if command -v md5sum   >/dev/null 2>&1; then md5sum "$1" | awk '{print $1}'; else md5 -q "$1"; fi; }
ncpu()    { if command -v nproc >/dev/null 2>&1; then nproc; else sysctl -n hw.ncpu 2>/dev/null || echo 8; fi; }
winpath() { if command -v cygpath >/dev/null 2>&1; then cygpath -m -- "$1"; else echo "$1"; fi; }
nonascii() { if [ "$(printf '%s' "$1" | LC_ALL=C tr -d '\000-\177' | wc -c | tr -d ' ')" != "0" ]; then echo 1; else echo 0; fi; }

ndk_ok() {
  [ -x "$1/ndk-build" ] && return 0
  [ -x "$1/prebuilt/windows-x86_64/bin/make.exe" ] && [ -f "$1/build/core/build-local.mk" ] && return 0
  return 1
}

resolve_ndk() {
  local d
  if [ -n "${ANDROID_NDK_HOME:-}" ] && ndk_ok "$ANDROID_NDK_HOME"; then echo "$ANDROID_NDK_HOME"; return 0; fi
  for d in "$NIXIANG"/01_tools/android-sdk/ndk/*; do
    if [ -d "$d" ] && ndk_ok "$d"; then echo "$d"; return 0; fi
  done
  if [ -n "${LOCALAPPDATA:-}" ]; then
    for d in "$(cygpath -u "$LOCALAPPDATA" 2>/dev/null || echo "$LOCALAPPDATA")/Android/Sdk/ndk"/*; do
      if [ -d "$d" ] && ndk_ok "$d"; then echo "$d"; return 0; fi
    done
  fi
  return 1
}

NDK="$(resolve_ndk)" || { echo "ERROR: 找不到可用 NDK（试过 \$ANDROID_NDK_HOME 与 01_tools/android-sdk/ndk）" >&2; exit 1; }

ndk_build() {
  if [ -x "$NDK/ndk-build" ]; then
    "$NDK/ndk-build" "$@"
  else
    "$NDK/prebuilt/windows-x86_64/bin/make.exe" -O \
      -f "$NDK/build/core/build-local.mk" SHELL=cmd "$@"
  fi
}

[ -f "$MK" ]     || { echo "ERROR: 缺 ${MK}（先跑 scripts/new_game_mod.sh Mxcz999）" >&2; exit 1; }
[ -f "$APP_MK" ] || { echo "ERROR: 缺 $APP_MK" >&2; exit 1; }
grep -q 'Games/Mxcz999Mod.cpp' "$MK" || { echo "ERROR: $MK 未编入 Games/Mxcz999Mod.cpp" >&2; exit 1; }
grep -q 'Games/Mxcz999Entry.cpp' "$MK" || { echo "ERROR: $MK 未编入 Games/Mxcz999Entry.cpp" >&2; exit 1; }
grep -q 'MainModern.cpp' "$MK" || { echo "ERROR: $MK 未走 modern 入口（MainModern.cpp）" >&2; exit 1; }

# ---- 构建工作区：仓库路径含中文时必须用 ASCII 暂存目录 ----
BUILD_JNI="$JNI"; BUILD_OUT="$OBJ"; BUILD_LIBS="$OUT_LIBS"; STAGED=0
if [ "$(nonascii "$JNI")" = "1" ]; then
  if [ -n "${LOCALAPPDATA:-}" ] && command -v cygpath >/dev/null 2>&1; then
    STAGE_ROOT="$(cygpath -u "$LOCALAPPDATA")/Temp/${TAG}_build"
  else
    STAGE_ROOT="${TMPDIR:-/tmp}/${TAG}_build"
  fi
  BUILD_JNI="$STAGE_ROOT/jni"; BUILD_OUT="$STAGE_ROOT/obj"; BUILD_LIBS="$STAGE_ROOT/libs"
  STAGED=1
fi

# 构建戳 epoch：必须在仓库根取（ASCII 暂存目录里没有 .git ⇒ Application.mk 的 $(shell git log …) 取空
# ⇒ 兜底 1790184000 ⇒ OTA 守卫判不出新旧，pre_install_gate 查8 FAIL）。
EPOCH="$(git -C "$NIXIANG" log -1 --format=%ct 2>/dev/null | tr -cd '0-9')"
[ -n "$EPOCH" ] || { echo "ERROR: 取不到 git commit 时间（git log -1 --format=%ct）。禁止用兜底 1790184000 出包。" >&2; exit 1; }
echo "[build_so] epoch     = ${EPOCH}（git log -1 --format=%ct @ ${NIXIANG}）"

CFG_HASH="$( (cat "$MK" "$APP_MK"; printf 'TRACE=%s\nDIAG=%s\nNDK=%s\nEPOCH=%s\n' \
  "${STARRY_TRACE:-}" "${STARRY_DIAG:-}" "$NDK" "$EPOCH") | hash256 )"
STAMP="$OBJ/.build_stamp"

echo "[build_so] game      = 999秒!幸存者 / $GAME_ID / tag=$TAG"
echo "[build_so] NDK       = $NDK"
echo "[build_so] MK        = $MK"
echo "[build_so] ABI       = ${ABI}（单 ABI：底包只带 arm64-v8a；雷电走 houdini 转译）"
echo "[build_so] NDK_OUT   = $OBJ  (绝对路径)"
echo "[build_so] NDK_LIBS  = $OUT_LIBS  (绝对路径)"
echo "[build_so] 产物      = $SO"
if [ "$STAGED" = "1" ]; then
  echo "[build_so] 路径非 ASCII → ASCII 暂存构建：$STAGE_ROOT"
fi
echo "[build_so] jniLibs   = 不写入（app/src/main/jniLibs 属并行会话 abssnmxt，禁止覆盖）"

if [ "$DRY_RUN" = "1" ]; then
  echo "[dry-run] 配置解析完成，未构建。真构建去掉 --dry-run。"
  echo "[dry-run] 菜单串待校验：${MENU_STRINGS[*]}"
  echo "[dry-run] 必查标签：[Mxcz999] ≥1；禁含：[Abssnmxt]；必含 authGameId 串：${GAME_ID}"
  exit 0
fi

if [ "$STAGED" = "1" ]; then
  mkdir -p "$BUILD_JNI"
  cp -a "$JNI"/. "$BUILD_JNI"/
  BUILD_MK="$BUILD_JNI/Android_mxcz999.mk"; BUILD_APP_MK="$BUILD_JNI/Application.mk"
else
  BUILD_MK="$MK"; BUILD_APP_MK="$APP_MK"
fi

# ---- obj stamp ----
if [ -f "$STAMP" ] && [ "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]; then
  echo "[stamp] 构建配置变化（mk/开关/NDK）→ 清 obj 强制全量重编"
  rm -rf "$OBJ" "$BUILD_OUT"
fi

SO_MD5_BEFORE="none"
[ -f "$SO" ] && SO_MD5_BEFORE="$(md5f "$SO")"
T0="$(mktemp)"; touch "$T0"

BUILD_RC=0
( cd "$BUILD_JNI" && ndk_build \
  NDK_PROJECT_PATH="$(winpath "$BUILD_JNI/../..")" \
  APP_BUILD_SCRIPT="$(winpath "$BUILD_MK")" \
  NDK_APPLICATION_MK="$(winpath "$BUILD_APP_MK")" \
  NDK_OUT="$(winpath "$BUILD_OUT")" \
  NDK_LIBS_OUT="$(winpath "$BUILD_LIBS")" \
  STARRY_BUILD_EPOCH="$EPOCH" \
  -j"$(ncpu)" ) || BUILD_RC=$?

NEW_O="$(find "$BUILD_OUT" -name '*.o' -newer "$T0" 2>/dev/null | head -1 || true)"
rm -f "$T0"
[ "$BUILD_RC" -eq 0 ] || { echo "ERROR: ndk-build rc=${BUILD_RC}（读上面日志，别换命令试错）" >&2; exit 1; }

if [ "$STAGED" = "1" ]; then
  mkdir -p "$OUT_LIBS/$ABI"
  for f in "$BUILD_LIBS/$ABI"/*.so; do
    [ -f "$f" ] && cp -f "$f" "$OUT_LIBS/$ABI/$(basename "$f")"
  done
fi

[ -f "$SO" ] || { echo "ERROR: 构建未产出 $SO" >&2; exit 1; }

# ---- 链接活动保险 ----
if [ -n "$NEW_O" ] && [ "$(md5f "$SO")" = "$SO_MD5_BEFORE" ]; then
  echo "ERROR: 本次构建产生了新 .o 但 $SO 字节未变 —— 旧 so 顶包嫌疑，禁止通过" >&2
  exit 1
fi

mkdir -p "$OBJ" && echo "$CFG_HASH" > "$STAMP"

# ---- 菜单串 / 身份串按字节校验 ----
check_has() {
  local s="$1" n
  n="$(LC_ALL=C grep -ac "$s" "$SO" 2>/dev/null || true)"
  [ -n "$n" ] || n=0
  [ "$n" -ge 1 ] || { echo "ERROR: $SO 不含「${s}」—— 源文件未编进去或旧 so 混入" >&2; exit 1; }
  echo "[check] OK  $s (x$n)"
}
for s in ${MENU_STRINGS[@]+"${MENU_STRINGS[@]}"}; do
  check_has "$s"
done
check_has "[$TAG]"
check_has "$GAME_ID"

# 串味检查：不得残留上一款游戏的标签或别的游戏 tag（jniLibs 顶包的典型症状）
if LC_ALL=C grep -aqF '[Abssnmxt]' "$SO"; then
  echo "ERROR: $SO 含 [Abssnmxt] 标签 —— 构建到别人会话的产物上，禁止通过" >&2
  exit 1
fi
echo "[check] OK  未含 [Abssnmxt]"
LC_ALL=C grep -aqF 'libil2cpp.so' "$SO" || { echo "ERROR: $SO 缺 libil2cpp.so 等待库名" >&2; exit 1; }

echo "[build_so] done → $SO"
echo "[build_so] size = $(wc -c <"$SO" | tr -d ' ') B"
echo "[build_so] md5  = $(md5f "$SO")"
echo "[build_so] sha256 = $(hash256 <"$SO")"
echo "[build_so] 提醒：不要拷进 app/src/main/jniLibs（属 abssnmxt 会话）；外科脚本直接取本产物。"
