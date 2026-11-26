#!/usr/bin/env bash
# ================================================================
#  明日方舟 2.7.71 (mrfzz) — build_so 阶段后端（modern 注册表 + 私有 Android_mrfzz.mk）
#  产物：mod菜单/starry-mod-menu/libs_mrfzz/arm64-v8a/libstarry_magic.so
#
#  ★ 绝不写入 app/src/main/jniLibs/（那里是并行会话的产物）；出包走外科 zip 直接取
#    libs_mrfzz 的 so，不依赖 gradle。
#
#  防呆四件套（照 04_scripts/魂坠深境/hzsj_build_so.sh 范本，勿自创）：
#   1 绝对路径 NDK_OUT / NDK_LIBS_OUT（相对路径被 -C 解析到 app/ 下 → 校验到旧 so 假成功）
#   2 obj stamp：mk / Application.mk / 模块源码 / 开关变化 → 清 obj 全量重编
#   3 链接活动保险：本次有新 .o 但 so 字节未变 = 旧 so 顶包 → FAIL
#   4 菜单串按字节校验（LC_ALL=C grep -a；strings 不认 UTF-8 中文）
#
#  Windows 专有两条（照 04_scripts/999秒幸存者/mxcz999_build_so.sh 范本）：
#   A 本机 NDK r27c 只有 ndk-build.cmd，没有 bash 版 ndk-build
#     → 直调 $NDK/prebuilt/windows-x86_64/bin/make.exe -O -f build/core/build-local.mk SHELL=cmd
#       （与 NDK 自带 build/ndk-build.cmd 逐字同形）
#   B 仓库路径含中文（mod菜单）→ Windows 侧 make/clang 把中文转成 ? 后写不出产物
#     → 在 13_scratch/mrfzz_ndk（纯 ASCII、仍在工作区内）构建，编完拷回 libs_mrfzz
#
#  用法:
#    bash 04_scripts/明日方舟/mrfzz_build_so.sh              # 真构建
#    bash 04_scripts/明日方舟/mrfzz_build_so.sh --dry-run     # 只打印解析结果，不构建
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
MK="$JNI/Android_mrfzz.mk"
APP_MK="$JNI/Application.mk"
OUT_LIBS="$STARRY_SRC/libs_mrfzz"
OBJ="$STARRY_SRC/obj_mrfzz"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"
TAG="mrfzz"
GAME_ID="mrfzz"
ABI="arm64-v8a"        # 单 ABI：底包只带 lib/arm64-v8a；Application.mk APP_ABI 同值
STAGE_ROOT="$NIXIANG/13_scratch/mrfzz_ndk"   # 纯 ASCII 且在工作区内（禁止 /tmp、禁止工作区外）

# ---- 七功能菜单串：必须与 Games/ArknightsEntry.cpp 的 kArknights_Features[] 逐字一致 ----
# （改菜单＝改两处：Entry 的数组 + 这里）
MENU_STRINGS=(
  "Category_战斗"
  "Toggle_超级攻速"
  "Toggle_超级无敌"
  "Toggle_基地锁血"
  "Toggle_十倍伤害"
  "Toggle_全图放置"
  "Toggle_无限技能"
  "Toggle_零部署费用"
)
# ---- 负向串：其它游戏 / 旧模板的菜单串，一条都不许进包 ----
BAD_STRINGS=(
  "[Endfield]"
  "Toggle_秒杀"
  "Toggle_无敌"
  "Toggle_玩家隐身"
  "Toggle_全屏攻击"
  "Toggle_自动开技能"
  "AutoSkillTrigger"
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

# Windows：make.exe 直调（与 NDK 自带 build/ndk-build.cmd 同形）；mac/Linux：原生 ndk-build
ndk_build() {
  if [ -x "$NDK/ndk-build" ]; then
    "$NDK/ndk-build" "$@"
  else
    PYTHONHOME= PYTHONPATH= NDK_ROOT="$(winpath "$NDK")" \
      "$NDK/prebuilt/windows-x86_64/bin/make.exe" -O \
      -f "$(winpath "$NDK/build/core/build-local.mk")" SHELL=cmd "$@"
  fi
}

[ -f "$MK" ]     || { echo "ERROR: 缺 $MK" >&2; exit 1; }
[ -f "$APP_MK" ] || { echo "ERROR: 缺 $APP_MK" >&2; exit 1; }
[ -f "$JNI/Games/ArknightsMod.cpp" ]   || { echo "ERROR: 缺 Games/ArknightsMod.cpp" >&2; exit 1; }
[ -f "$JNI/Games/ArknightsEntry.cpp" ] || { echo "ERROR: 缺 Games/ArknightsEntry.cpp" >&2; exit 1; }
grep -q 'Games/ArknightsMod.cpp' "$MK"    || { echo "ERROR: $MK 未编入 Games/ArknightsMod.cpp" >&2; exit 1; }
grep -q 'Games/ArknightsEntry.cpp' "$MK"  || { echo "ERROR: $MK 未编入 Games/ArknightsEntry.cpp" >&2; exit 1; }
grep -q 'MainModern.cpp' "$MK"            || { echo "ERROR: $MK 未走 modern 入口（MainModern.cpp）" >&2; exit 1; }
grep -q 'APP_ABI *:= *arm64-v8a' "$APP_MK" || { echo "ERROR: $APP_MK 的 APP_ABI 不是单 arm64-v8a" >&2; exit 1; }

# ---- 构建工作区：仓库路径含中文时必须用 ASCII 暂存目录 ----
BUILD_JNI="$JNI"; BUILD_OUT="$OBJ"; BUILD_LIBS="$OUT_LIBS"; PROJECT_PATH="$JNI/../.."; STAGED=0
if [ "$(nonascii "$JNI")" = "1" ]; then
  BUILD_JNI="$STAGE_ROOT/jni"; BUILD_OUT="$STAGE_ROOT/obj"; BUILD_LIBS="$STAGE_ROOT/libs"
  PROJECT_PATH="$STAGE_ROOT"
  STAGED=1
fi

CFG_HASH="$( (cat "$MK" "$APP_MK" \
  "$JNI/Games/ArknightsMod.h" "$JNI/Games/ArknightsMod.cpp" "$JNI/Games/ArknightsEntry.cpp"; \
  printf 'TRACE=%s\nDIAG=%s\nNDK=%s\n' "${STARRY_TRACE:-}" "${STARRY_DIAG:-1}" "$NDK") | hash256 )"
STAMP="$OBJ/.build_stamp"

echo "[build_so] game      = 明日方舟 2.7.71 / $GAME_ID / tag=$TAG"
echo "[build_so] host      = ${NIXIANG_HOST:-unknown} / $(uname -s)"
echo "[build_so] NDK       = $NDK"
if [ -x "$NDK/ndk-build" ]; then
  echo "[build_so] 入口      = \$NDK/ndk-build"
else
  echo "[build_so] 入口      = \$NDK/prebuilt/windows-x86_64/bin/make.exe -O -f build/core/build-local.mk SHELL=cmd"
fi
echo "[build_so] MK        = $MK"
echo "[build_so] ABI       = ${ABI}（单 ABI：底包只带 arm64-v8a/v7a，无 x86_64）"
echo "[build_so] NDK_OUT   = $OBJ  (绝对路径)"
echo "[build_so] NDK_LIBS  = $OUT_LIBS  (绝对路径)"
echo "[build_so] 产物      = $SO"
echo "[build_so] 菜单串    = ${MENU_STRINGS[*]}"
echo "[build_so] 负向串    = ${BAD_STRINGS[*]}"

if [ "$STAGED" = "1" ]; then
  echo "[build_so] 路径非 ASCII → ASCII 暂存构建：$STAGE_ROOT"
else
  echo "[build_so] 路径纯 ASCII → 原地构建：$JNI"
fi

if [ "$DRY_RUN" = "1" ]; then
  echo "[dry-run] NDK_PROJECT_PATH   = $(winpath "$PROJECT_PATH")"
  echo "[dry-run] APP_BUILD_SCRIPT   = $(winpath "$BUILD_JNI/Android_mrfzz.mk")"
  echo "[dry-run] NDK_APPLICATION_MK = $(winpath "$BUILD_JNI/Application.mk")"
  echo "[dry-run] NDK_OUT            = $(winpath "$BUILD_OUT")"
  echo "[dry-run] NDK_LIBS_OUT       = $(winpath "$BUILD_LIBS")"
  echo "[dry-run] 回拷目标            = $OUT_LIBS/$ABI/"
  echo "[dry-run] 配置解析完成，未构建。真构建去掉 --dry-run。"
  exit 0
fi

if [ "$STAGED" = "1" ]; then
  rm -rf "$BUILD_JNI"
  mkdir -p "$BUILD_JNI"
  cp -a "$JNI"/. "$BUILD_JNI"/
fi
BUILD_MK="$BUILD_JNI/Android_mrfzz.mk"; BUILD_APP_MK="$BUILD_JNI/Application.mk"

# ---- obj stamp ----
if [ -f "$STAMP" ] && [ "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]; then
  echo "[stamp] 构建配置/模块源码变化 → 清 obj 强制全量重编"
  rm -rf "$OBJ" "$BUILD_OUT"
fi

SO_MD5_BEFORE="none"
[ -f "$SO" ] && SO_MD5_BEFORE="$(md5f "$SO")"
T0="$(mktemp)"; touch "$T0"

BUILD_RC=0
( cd "$BUILD_JNI" && ndk_build \
  NDK_PROJECT_PATH="$(winpath "$PROJECT_PATH")" \
  APP_BUILD_SCRIPT="$(winpath "$BUILD_MK")" \
  NDK_APPLICATION_MK="$(winpath "$BUILD_APP_MK")" \
  NDK_OUT="$(winpath "$BUILD_OUT")" \
  NDK_LIBS_OUT="$(winpath "$BUILD_LIBS")" \
  ${STARRY_TRACE:+STARRY_TRACE="$STARRY_TRACE"} \
  ${STARRY_DIAG:+STARRY_DIAG="$STARRY_DIAG"} \
  -j"$(ncpu)" ) || BUILD_RC=$?

NEW_O="$(find "$BUILD_OUT" -name '*.o' -newer "$T0" 2>/dev/null | head -1 || true)"
T0_KEEP="$T0"   # 构建起点戳：下方链接活动保险用它判断"本次是否重新链接"，收尾删
[ "$BUILD_RC" -eq 0 ] || { echo "ERROR: ndk-build rc=${BUILD_RC}（读上面日志，别换命令试错）" >&2; exit 1; }

if [ "$STAGED" = "1" ]; then
  mkdir -p "$OUT_LIBS/$ABI"
  for f in "$BUILD_LIBS/$ABI"/*.so; do
    [ -f "$f" ] && cp -f "$f" "$OUT_LIBS/$ABI/$(basename "$f")"
  done
fi

[ -f "$SO" ] || { echo "ERROR: 构建未产出 $SO" >&2; exit 1; }

# ---- 链接活动保险 ----
# 判据＝「本次是否真的重新链接」，而不是「md5 是否变化」：
# 本工具链实测可复现（obj 全清后重编，so 字节与上一轮完全一致），拿 md5 当判据会在
# 合法的全量重编上误报「旧 so 顶包」。mtime 早于构建起点却又有新 .o ⇒ 才是真顶包。
if [ -n "$NEW_O" ] && [ ! "$SO" -nt "$T0_KEEP" ]; then
  echo "ERROR: 本次编译产生了新 .o，但 $SO 的 mtime 早于构建起点（未重新链接）—— 旧 so 顶包嫌疑，禁止通过" >&2
  exit 1
fi
[ -n "$NEW_O" ] && [ "$(md5f "$SO")" = "$SO_MD5_BEFORE" ] && \
  echo "[stamp] 提示：本轮重新链接但 so 字节与上一轮一致（本工具链可复现构建，非顶包）"
rm -f "$T0_KEEP"

mkdir -p "$OBJ" && echo "$CFG_HASH" > "$STAMP"

# ---- 身份串 / 菜单串按字节校验（LC_ALL=C grep -a；strings 不认 UTF-8 中文）----
check_has() {
  local s="$1" n
  n="$(LC_ALL=C grep -ac -- "$s" "$SO" 2>/dev/null || true)"
  [ -n "$n" ] || n=0
  [ "$n" -ge 1 ] || { echo "ERROR: $SO 不含「${s}」—— 源文件未编进去或旧 so 混入" >&2; exit 1; }
  echo "[check] OK  必含 $s (x$n)"
}
check_has "mrfzz"
check_has "[mrfzz]"
for s in ${MENU_STRINGS[@]+"${MENU_STRINGS[@]}"}; do
  check_has "$s"
done
for b in ${BAD_STRINGS[@]+"${BAD_STRINGS[@]}"}; do
  HIT="$(LC_ALL=C grep -Fac -- "$b" "$SO" 2>/dev/null || true)"
  [ -n "$HIT" ] || HIT=0
  [ "$HIT" -eq 0 ] || { echo "ERROR: $SO 含其它游戏/旧模板菜单串「${b}」(x$HIT)" >&2; exit 1; }
  echo "[check] OK  禁含 $b (0)"
done
# 产物架构按字节校验（od 可移植；明日方舟的 libil2cpp.so 名走 SecStrings 混淆，文件里无字面，不能拿它当判据）
ELF_CLASS="$(od -An -tx1 -j4 -N1 "$SO" | tr -d ' \n')"
ELF_MACH="$(od -An -tx1 -j18 -N2 "$SO" | tr -d ' \n')"
[ "$ELF_CLASS" = "02" ] || { echo "ERROR: $SO 不是 ELF64（e_ident[EI_CLASS]=${ELF_CLASS}）" >&2; exit 1; }
[ "$ELF_MACH" = "b700" ] || { echo "ERROR: $SO 不是 AArch64（e_machine=${ELF_MACH}，期望 b700）" >&2; exit 1; }
echo "[check] OK  ELF64 / AArch64（e_machine=0x00b7）"

# ---------- 诊断总闸断言（P0-2，沿用 mac 版口径）----------
# 诊断包：诊断串必须在；交付包（STARRY_DIAG=0）：诊断串必须一条不剩
DIAG_VAL="${STARRY_DIAG:-1}"
if [ "$DIAG_VAL" = "0" ]; then
  for d in 'diag: ready=' 'calib: anchor=' 'calib: guard2 ' 'OkPtr probe' ' by-name ok: ' \
           'ext api assignable=' 'SELFCHECK'; do
    HIT="$(LC_ALL=C grep -Fac -- "$d" "$SO" 2>/dev/null || true)"
    [ -n "$HIT" ] || HIT=0
    [ "$HIT" -eq 0 ] || {
      echo "ERROR: STARRY_DIAG=0（交付包）但 $SO 仍含诊断串: ${d}（检查 SDIAG 是否被写成了 if）" >&2
      exit 1
    }
  done
  echo "[check] OK  诊断串负向断言通过（交付包：0 条诊断串）"
else
  for d in 'diag: ready=' 'calib: guard2 ' 'SELFCHECK'; do
    HIT="$(LC_ALL=C grep -Fac -- "$d" "$SO" 2>/dev/null || true)"
    [ -n "$HIT" ] || HIT=0
    [ "$HIT" -ge 1 ] || { echo "ERROR: STARRY_DIAG=${DIAG_VAL}（诊断包）但 $SO 缺诊断串: $d" >&2; exit 1; }
  done
  echo "[check] OK  诊断串正向断言通过（诊断包）"
fi

echo "[build_so] done → $SO"
echo "[build_so] size = $(wc -c <"$SO" | tr -d ' ') B"
echo "[build_so] md5  = $(md5f "$SO")"
echo "[build_so] sha256 = $(hash256 <"$SO")"
echo "[build_so] 提醒：不要拷进 app/src/main/jniLibs；外科脚本直接取本产物。"
