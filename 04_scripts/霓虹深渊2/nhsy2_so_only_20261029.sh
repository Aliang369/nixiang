#!/usr/bin/env bash
# ================================================================
#  nhsy2_so_only_20261029.sh — 霓虹深渊2「只换 native so」快速迭代轮
#
#  依据（硬约束 A#7）：同签名包之间禁止卸载；**更小的改动（只换
#  lib/arm64-v8a/libstarry_magic.so）优先走 root 覆盖已释放的 so**
#  （/data/app/~~xxxx/<pkg>-yyyy/lib/arm64/，覆盖后 chmod + chcon 对齐
#  + am force-stop 冷启），**完全不碰包安装**。本脚本即该路径的唯一实现。
#
#  适用：r30（2026-10-29）「无敌关掉后仍无敌」字段回滚修复 —— 只改
#  Games/nhsy2Mod.cpp 的钩子体，dex/manifest/资源零改动 ⇒ 不需要重打
#  1.3 GB 的 APK，也不需要官方底包 APK（本机没有，且禁止自行下载）。
#
#  步骤：
#    1) ndk-build（私有 mk Android_nhsy2.mk，独立 obj_nhsy2/libs_nhsy2）
#    2) ASCII 唯一串闸：确认本轮新代码真的进了产物（防「陈旧 so 直装」）
#    3) root 覆盖设备上已释放的 so + chmod/chcon 对齐
#    4) 尺寸 + md5 双向核验
#    5) am force-stop + 冷启（不装/不卸 APK，不清 data）
#
#  用法：
#    bash 04_scripts/霓虹深渊2/nhsy2_so_only_20261029.sh [--serial <SN>] [--dry-run]
#  序列号优先级：--serial > $DEV_SERIAL > emulator-5554（雷电模拟器）
#  bash 3.2 兼容（不用 mapfile / 关联数组 / local -n / ${v^^}）
# ================================================================
set -euo pipefail

PKG="com.veewo.neonabyss2"
LIB_NAME="libstarry_magic.so"
DEF_SERIAL="emulator-5554"
MARK="inv: 无敌残留回滚"            # 本轮唯一 ASCII/UTF-8 唯一串闸
SERIAL="${DEV_SERIAL:-$DEF_SERIAL}"
DRY_RUN=0

REPO="$(cd "$(dirname "$0")/../.." && pwd -P)"
SRC="$REPO/mod菜单/starry-mod-menu"
JNI="$SRC/app/src/main/jni"
SO="$SRC/libs_nhsy2/arm64-v8a/$LIB_NAME"          # 规范位置（外科脚本也用这个）
# 构建产出位置：相对 NDK_LIBS_OUT=../../libs_nhsy2（cwd=jni ⇒ app/src/libs_nhsy2）
SO_BUILD="$SRC/app/src/libs_nhsy2/arm64-v8a/$LIB_NAME"

usage() { sed -n '2,30p' "$0"; exit 2; }
die()   { echo "✗ FAIL  $*" >&2; exit 1; }
log()   { echo "· $*"; }

while [ $# -gt 0 ]; do
  case "$1" in
    --serial) SERIAL="$2"; shift 2 ;;
    --dry-run) DRY_RUN=1; shift ;;
    -h|--help) usage ;;
    *) usage ;;
  esac
done

# ---------------- NDK ----------------
# ★ Win 版 NDK 包**没有**顶层 ndk-build（只有 ndk-build.cmd），bash 入口在 build/ndk-build。
detect_ndk() {
  if [ -n "${ANDROID_NDK_HOME:-}" ] && { [ -f "$ANDROID_NDK_HOME/build/ndk-build" ] || [ -f "$ANDROID_NDK_HOME/ndk-build" ]; }; then
    echo "$ANDROID_NDK_HOME"; return 0
  fi
  local c
  for c in "$HOME/AppData/Local/Android/Sdk/ndk/27.0.12077973" \
           "$HOME/AppData/Local/Android/Sdk/ndk"/* \
           "$HOME/Library/Android/sdk/ndk/27.0.12077973"; do
    { [ -f "$c/build/ndk-build" ] || [ -f "$c/ndk-build" ]; } && { echo "$c"; return 0; }
  done
  return 1
}
NDK="$(detect_ndk)" || die "找不到 NDK（设 ANDROID_NDK_HOME 指向含 build/ndk-build 的 NDK 根目录）"
if [ -f "$NDK/build/ndk-build" ]; then NDB="$NDK/build/ndk-build"; else NDB="$NDK/ndk-build"; fi
log "NDK=$NDK"
log "ndk-build=$NDB"

# ---------------- 1) 构建 ----------------
# ★ Windows 实测（2026-10-29）：Git Bash 里**不能**用 bash 版 `build/ndk-build`：
#   ① 先死在 "Cannot find 'make'"（MSYS 无 make）；② 即便塞 GNUMAKE=NDK 自带 make.exe，
#   产物规则里仍是 cmd 语法（`rem ...`）⇒ 到处 `line 1: rem: command not found`（Error 127）。
#   正解＝走官方 Windows 入口 `ndk-build.cmd`（它自己把 SHELL 设成 cmd 并带上自带 make.exe）。
#   同时把 STARRY_BUILD_EPOCH 显式传进去（否则 cmd 里 `2>/dev/null | tr` 不可用 ⇒ 戳变垃圾）。
build_so() {
  local epoch
  epoch="$(cd "$REPO" && git log -1 --format=%ct 2>/dev/null | tr -cd '0-9')"
  [ -n "$epoch" ] || epoch=1790184000
  local argline="NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android_nhsy2.mk NDK_APPLICATION_MK=./Application.mk NDK_OUT=../../obj_nhsy2 NDK_LIBS_OUT=../../libs_nhsy2 STARRY_BUILD_EPOCH=$epoch -j4"
  case "$(uname -s)" in
    MINGW*|MSYS*|CYGWIN*)
      log "Windows 入口: ndk-build.cmd（epoch=${epoch}）"
      ( cd "$JNI" && cmd //c "$(cygpath -w "$NDK/ndk-build.cmd")" $argline )
      ;;
    *)
      ( cd "$JNI" && "$NDB" $argline )
      ;;
  esac
}

log "ndk-build → $SO"
if [ "$DRY_RUN" = "0" ]; then
  rm -f "$SO" "$SO_BUILD"
  build_so > "$REPO/04_scripts/霓虹深渊2/so_build_20261029.log" 2>&1 \
    || { tail -40 "$REPO/04_scripts/霓虹深渊2/so_build_20261029.log"; die "ndk-build 失败"; }
  [ -f "$SO_BUILD" ] || { tail -20 "$REPO/04_scripts/霓虹深渊2/so_build_20261029.log"; die "ndk-build 报成功但产物不在 $SO_BUILD"; }
  mkdir -p "$(dirname "$SO")" && cp "$SO_BUILD" "$SO"
fi
[ -f "$SO" ] || die "产物缺失 $SO"
log "so 产物: $(wc -c < "$SO" | tr -d ' ') B"

# ---------------- 2) 唯一串闸（防陈旧 so） ----------------
# ★ 教训：不要用 strings 找中文（默认不输出 UTF-8 中文）；grep -a 直接扫原始字节
if ! LC_ALL=C grep -aq "$MARK" "$SO"; then
  die "产物里没有本轮唯一串「${MARK}」⇒ 跑的是陈旧 so，拒绝刷机"
fi
log "唯一串闸通过：$MARK"
NM="$NDK/toolchains/llvm/prebuilt/$(ls "$NDK/toolchains/llvm/prebuilt" | head -1)/bin/llvm-nm"
[ -f "$NM" ] || NM="$NM.exe"
if [ -f "$NM" ]; then
  if "$NM" -D --undefined-only "$SO" | grep -q emutls; then die "so 里有 emutls 未定义符号（dlopen 会失败）"; fi
  log "emutls 闸通过"
fi
SO_MD5="$(md5 -q "$SO" 2>/dev/null || md5sum "$SO" | cut -d' ' -f1)"
SO_SIZE="$(wc -c < "$SO" | tr -d ' ')"
log "本地 so md5=$SO_MD5 size=$SO_SIZE"

# ---------------- 3) 设备侧覆盖 ----------------
DEVDIR="$(adb -s "$SERIAL" shell pm path "$PKG" | tr -d '\r' | sed -n 's|^package:||p' | head -1)"
[ -n "$DEVDIR" ] || die "设备 $SERIAL 上没找到 $PKG"
APPDIR="$(dirname "$DEVDIR")"                       # /data/app/~~x/<pkg>-y
LIBDIR="$APPDIR/lib/arm64"
REMOTE_TMP="/data/local/tmp/nhsy2_so_$$"
log "设备: $SERIAL  LIBDIR=$LIBDIR"
[ "$DRY_RUN" = "1" ] && { log "dry-run：只打印不执行"; exit 0; }

# ★ MSYS 路径转换闸：Git Bash 会把 `/data/local/tmp/x` 这类**远端**路径自动改写成
#   `C:\Program Files\Git\data\local\tmp\x` ⇒ adb push 直接失败（2026-10-29 实测踩到）。
#   ⇒ 关掉自动转换；同时把本地文件路径用 cygpath -w 转成 Windows 形式再交给 adb。
case "$(uname -s)" in
  MINGW*|MSYS*|CYGWIN*) export MSYS_NO_PATHCONV=1; IS_WIN=1 ;;
  *) IS_WIN=0 ;;
esac
LOCAL_SO="$SO"
[ "$IS_WIN" = "1" ] && LOCAL_SO="$(cygpath -w "$SO")"

adb -s "$SERIAL" push "$LOCAL_SO" "$REMOTE_TMP" >/dev/null || die "push 失败（local=$LOCAL_SO remote=${REMOTE_TMP}）"
# ★ SELinux 上下文：Android 的 toybox chcon **不支持 --reference**（2026-10-29 实测报
#   "Unknown option 'reference=…'"）⇒ 先从 libil2cpp.so 读出上下文，再显式 chcon。
CTX="$(adb -s "$SERIAL" shell "su -c 'ls -Z \"$LIBDIR/libil2cpp.so\"'" | tr -d '\r' | awk '{print $1}')"
[ -n "$CTX" ] || CTX="u:object_r:apk_data_file:s0"
log "SELinux 上下文基准: $CTX"
adb -s "$SERIAL" shell "su -c 'cp \"$REMOTE_TMP\" \"$LIBDIR/$LIB_NAME\"'" >/dev/null || die "cp 失败"
adb -s "$SERIAL" shell "su -c 'chmod 644 \"$LIBDIR/$LIB_NAME\"'" >/dev/null || die "chmod 失败"
adb -s "$SERIAL" shell "su -c 'chcon $CTX \"$LIBDIR/$LIB_NAME\"'" >/dev/null || log "⚠ chcon 未成功（SELinux=$(adb -s "$SERIAL" shell getenforce | tr -d '\r')，permissive 下不影响）"
adb -s "$SERIAL" shell "rm -f \"$REMOTE_TMP\"" >/dev/null || true
RC_DEV="$(adb -s "$SERIAL" shell "su -c 'wc -c < \"$LIBDIR/$LIB_NAME\"'" | tr -d '\r ')"
[ "$RC_DEV" = "$SO_SIZE" ] || die "设备侧尺寸不符（$RC_DEV ≠ ${SO_SIZE}）"
RC_MD5="$(adb -s "$SERIAL" shell "su -c 'md5sum \"$LIBDIR/$LIB_NAME\"'" | tr -d '\r' | cut -d' ' -f1)"
[ "$RC_MD5" = "$SO_MD5" ] || die "设备侧 md5 不符（$RC_MD5 ≠ ${SO_MD5}）"
log "覆盖完成并双向核验一致（md5=$RC_MD5 size=${RC_DEV}）"

# ---------------- 5) 冷启 ----------------
adb -s "$SERIAL" shell am force-stop "$PKG"
ACT="$(adb -s "$SERIAL" shell cmd package resolve-activity --brief "$PKG" | tr -d '\r' | tail -1)"
log "冷启: $ACT"
adb -s "$SERIAL" shell am start -n "$ACT" >/dev/null
log "已冷启（不装/不卸/不清 data；日志判据：adb -s $SERIAL logcat -d | grep '\\[nhsy2\\]'）"
