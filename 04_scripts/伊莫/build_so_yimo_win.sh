#!/usr/bin/env bash
# 04_scripts/伊莫/build_so_yimo_win.sh
# 伊莫（com.x.yimoo.official）libstarry_magic.so 构建 —— Windows(Git-Bash) 版
#
#   与 09_test/_t_build_mod_win.sh 同一套姿势：
#     · 工程真名含中文（mod菜单/），clang 写 .o 会失败 ⇒ 一律用 ASCII 目录
#       ① starry_link（Junction → mod菜单/starry-mod-menu）作源码树
#       ② 产物落 09_test/_modobj / 09_test/_modlibs（ASCII）
#     · 本游戏走 modern 注册表：APP_BUILD_SCRIPT=./Android_yimo.mk
#       （Games/YimoMod.cpp + Games/YimoEntry.cpp + Games/NoAceStubs.cpp）
#
# 产物：09_test/_modlibs/arm64-v8a/libstarry_magic.so（+ libc++_shared.so）
#      并自动同步到 starry_link/app/src/main/jniLibs/arm64-v8a/（gradle 打包用）
#
# 用法：
#   bash 04_scripts/伊莫/build_so_yimo_win.sh              # 正式包（干净、零诊断）
#   STARRY_DIAG=1 bash 04_scripts/伊莫/build_so_yimo_win.sh # 诊断包（独立 obj 目录）
set -uo pipefail
export MSYS_NO_PATHCONV=1

HERE="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$HERE/../.." && pwd)"
SRC="$ROOT/starry_link"
[ -d "$SRC" ] || { echo "ERROR: 缺 ${SRC}（Junction → mod菜单/starry-mod-menu）"; exit 1; }
JNI="$SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-/c/Users/Administrator/AppData/Local/Android/Sdk/ndk/27.0.12077973}"
NDK_BUILD="$NDK/ndk-build"
[ -x "$NDK_BUILD" ] || NDK_BUILD="$NDK/ndk-build.cmd"
[ -f "$NDK_BUILD" ] || { echo "ERROR: 找不到 ndk-build(.cmd): $NDK"; exit 1; }

W_ROOT="$(cygpath -m "$ROOT")"
W_JNI="$W_ROOT/starry_link/app/src/main/jni"
TAG="Yimo"
DIAG="${STARRY_DIAG:-0}"
if [ "$DIAG" = "1" ]; then
  W_OBJ="$W_ROOT/09_test/_modobj_yimo_diag"
  W_LIBS="$W_ROOT/09_test/_modlibs_yimo_diag"
  EXTRA_MAKE=(STARRY_DIAG=1)
else
  W_OBJ="$W_ROOT/09_test/_modobj_yimo"
  W_LIBS="$W_ROOT/09_test/_modlibs_yimo"
  EXTRA_MAKE=()
fi
SO_TMP="$W_LIBS/arm64-v8a/libstarry_magic.so"
CXX_TMP="$W_LIBS/arm64-v8a/libc++_shared.so"

[ -f "$JNI/Android_yimo.mk" ] || { echo "ERROR: 缺 Android_yimo.mk"; exit 1; }
grep -q 'Games/YimoMod.cpp' "$JNI/Android_yimo.mk" || { echo "ERROR: mk 缺 YimoMod.cpp"; exit 1; }
echo "[env] ROOT=$W_ROOT"
echo "[env] JNI=$W_JNI"
echo "[env] NDK=$NDK"
echo "[env] OBJ=$W_OBJ  LIBS=$W_LIBS  DIAG=$DIAG"
mkdir -p "$W_OBJ" "$W_LIBS"

T0="$(date +%s)"
# OTA 时间戳：必须来自 git 提交时间（Application.mk 回落值会让覆盖装不清掉玩家侧旧 so，见装机门 查8）
# ★git 是原生程序：喂 MSYS 路径 /e/... 会报 "cannot change to"（实测），必须用 Windows 形式
EPOCH="${STARRY_BUILD_EPOCH:-$(git -C "$(cygpath -m "$ROOT")" log -1 --format=%ct 2>/dev/null | tr -cd "0-9")}"
[ -n "$EPOCH" ] || EPOCH=1790184000
echo "[env] STARRY_BUILD_EPOCH=$EPOCH"
"$NDK_BUILD" -C "$W_JNI" \
  NDK_PROJECT_PATH="$W_ROOT/starry_link/app/src/main" \
  APP_BUILD_SCRIPT="./Android_yimo.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  APP_ABI=arm64-v8a \
  NDK_OUT="$W_OBJ" \
  NDK_LIBS_OUT="$W_LIBS" \
  STARRY_BUILD_EPOCH="$EPOCH" \
  ${EXTRA_MAKE[@]+"${EXTRA_MAKE[@]}"} \
  -j"$(nproc 2>/dev/null || echo 8)" 2>&1 | tail -25
RC=${PIPESTATUS[0]}
echo "ndk-build rc=$RC (耗时 $(( $(date +%s) - T0 ))s)"
[ "$RC" -eq 0 ] || { echo "!! 编译失败"; exit 1; }
[ -f "$SO_TMP" ] || { echo "!! 没有产物 $SO_TMP"; exit 1; }

echo "=== 产物校验 ==="
SIZE=$(wc -c < "$SO_TMP")
MD5=$(md5sum "$SO_TMP" | awk '{print $1}')
echo "so=$SO_TMP size=$SIZE md5=$MD5"
FAIL=0
# ① 本游戏标签必须在、上一款标签必须不在
if LC_ALL=C grep -aq "\[$TAG\]" "$SO_TMP"; then echo "  [OK] tag [$TAG]"; else echo "  [FAIL] 缺 tag [$TAG]"; FAIL=1; fi
for stale in '\[Nhsy2\]' '\[Sf3\]' '\[Jjdm\]' '\[Hxwj\]' '\[Ysxz\]' '\[Zmws\]'; do
  if LC_ALL=C grep -aq "$stale" "$SO_TMP"; then echo "  [FAIL] 含上一款标签 $stale"; FAIL=1; fi
done
# ② 授权证书必须烘焙进 so（fail-closed 授权链依赖）
CERT_N=$(LC_ALL=C grep -ac 'BEGIN CERTIFICATE' "$SO_TMP" || true)
if [ "${CERT_N:-0}" -ge 1 ]; then echo "  [OK] 烘焙证书 x$CERT_N"; else echo "  [FAIL] so 内无烘焙证书"; FAIL=1; fi
# ③ 六项菜单串（用户点名的功能集，多一项/少一项都不行）
for m in 'Toggle_精灵攻击秒杀' 'Slider_移速' 'Toggle_玩家无敌' 'Toggle_无限跳跃' 'Toggle_无限体力' 'Toggle_瞬移采集'; do
  N=$(LC_ALL=C grep -ac "$m" "$SO_TMP" || true)
  if [ "${N:-0}" -ge 1 ]; then echo "  [OK] 菜单串 $m"; else echo "  [FAIL] 缺菜单串 $m"; FAIL=1; fi
done
# ④ 干净包门禁：正式包内不得有诊断通道
if [ "$DIAG" != "1" ]; then
  if bash "$ROOT/04_scripts/_common/check_release_clean.sh" --so "$SO_TMP"; then
    echo "  [OK] 干净包门禁"
  else
    echo "  [FAIL] 干净包门禁未过（诊断/日志泄漏进正式包）"; FAIL=1
  fi
  if LC_ALL=C grep -aq 'moddiag\.log' "$SO_TMP"; then echo "  [FAIL] 正式包含诊断 chunk 标记"; FAIL=1; fi
fi
[ "$FAIL" -eq 0 ] || { echo "!! so 自检失败"; exit 1; }

# ⑤ 同步进 jniLibs（gradle 打包源；漏这步 = 打出上一款游戏的 so）
JL="$SRC/app/src/main/jniLibs/arm64-v8a"
mkdir -p "$JL"
cp -f "$SO_TMP" "$JL/libstarry_magic.so"
echo "  [sync] $JL/libstarry_magic.so"
if [ -f "$CXX_TMP" ]; then
  cp -f "$CXX_TMP" "$JL/libc++_shared.so"
  echo "  [sync] $JL/libc++_shared.so ($(wc -c < "$CXX_TMP") B)"
fi
echo "[done] md5=$MD5 size=$SIZE"
