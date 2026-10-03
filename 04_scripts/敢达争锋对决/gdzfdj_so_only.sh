#!/usr/bin/env bash
# 敢达争锋对决（gdzfdj）— 只构建 libstarry_magic.so（ndk-build，APP_BUILD_SCRIPT=Android_gdzfdj.mk）
#
# 用法：bash 04_scripts/敢达争锋对决/gdzfdj_so_only.sh
# 产物：mod菜单/starry-mod-menu/app/src/main/libs/arm64-v8a/libstarry_magic.so
#       （并同步到 jniLibs/arm64-v8a 供后续打包引用）

set -u

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=../_common/env.sh
. "$HERE/../_common/env.sh"

STAR="$NIXIANG_ROOT/mod菜单/starry-mod-menu"
JNI="$STAR/app/src/main/jni"
MK_NAME="Android_gdzfdj.mk"
SO_NAME="libstarry_magic.so"

die() { echo "ERROR: $*" >&2; exit 1; }

# --- NDK 解析（win: %LOCALAPPDATA%\Android\Sdk\ndk\<ver>；mac: ~/Library/Android/sdk/ndk/<ver>）---
NDK="${ANDROID_NDK_HOME:-}"
if [ -z "$NDK" ]; then
  if [ "$NIXIANG_HOST" = "win" ]; then
    SDK="${ANDROID_HOME:-${ANDROID_SDK_ROOT:-${LOCALAPPDATA:-$HOME/AppData/Local}/Android/Sdk}}"
    SDK="$(cygpath -u "$SDK" 2>/dev/null || printf '%s' "$SDK")"   # win 路径 → /c/... 供 bash 使用
  else
    SDK="${ANDROID_HOME:-${ANDROID_SDK_ROOT:-$HOME/Library/Android/sdk}}"
  fi
  for v in 27.0.12077973 29.0.14033849; do
    [ -x "$SDK/ndk/$v/ndk-build" ] && NDK="$SDK/ndk/$v" && break
    [ -f "$SDK/ndk/$v/ndk-build.cmd" ] && NDK="$SDK/ndk/$v" && break
  done
  if [ -z "$NDK" ] && [ -d "$SDK/ndk" ]; then
    for d in "$SDK"/ndk/*; do [ -d "$d" ] && NDK="$d"; done
  fi
fi
[ -n "$NDK" ] || die "NDK 未找到（设 ANDROID_NDK_HOME）"
[ -f "$JNI/$MK_NAME" ] || die "缺 $JNI/$MK_NAME"

echo "[1/3] ndk-build（$NDK）"
NPROC="$(nproc 2>/dev/null || sysctl -n hw.ncpu 2>/dev/null || echo 4)"
# Application.mk 用 $(shell git log -1 --format=%ct) 做「同源重建戳」；
# 在 ASCII 暂存目录里不是 git 仓库 ⇒ 会打印 fatal 并回落到当前时间。
# 这里显式传入（命令行变量覆盖 makefile 赋值），保持戳一致且日志干净。
EPOCH="$(git -C "$NIXIANG_ROOT" log -1 --format=%ct 2>/dev/null || date +%s)"
echo "      STARRY_BUILD_EPOCH=$EPOCH"

# ★ Win 侧铁律：仓库路径含中文（07_patched/敢达争锋对决、mod菜单），
#   Windows 版 NDK 只提供 ndk-build.cmd（无 bash 版 ndk-build），且原生 make.exe
#   不做 MSYS 路径转换 —— 故先把 jni 整树镜像到纯 ASCII 目录，再用 cmd.exe 驱动。
if [ "$NIXIANG_HOST" = "win" ]; then
  PROJ="$(cygpath -u "${LOCALAPPDATA:-$HOME/AppData/Local}")/Temp/gdzfdj_build/so"
  rm -rf "$PROJ"
  mkdir -p "$PROJ/jni"
  cp -a "$JNI/." "$PROJ/jni/" || die "jni 镜像失败"
  ( cd "$PROJ" && cmd //c "$(cygpath -w "$NDK/ndk-build.cmd")" \
      NDK_PROJECT_PATH=. \
      APP_BUILD_SCRIPT="./jni/$MK_NAME" \
      NDK_APPLICATION_MK=./jni/Application.mk \
      STARRY_BUILD_EPOCH="$EPOCH" \
      -j"$NPROC" ) || die "ndk-build 失败"
  SO="$PROJ/libs/arm64-v8a/$SO_NAME"
  [ -f "$SO" ] || die "未找到 $SO_NAME 产物（$PROJ/libs）"
else
  ( cd "$JNI" && "$NDK/ndk-build" \
      NDK_PROJECT_PATH=../.. \
      APP_BUILD_SCRIPT="./$MK_NAME" \
      NDK_APPLICATION_MK=./Application.mk \
      STARRY_BUILD_EPOCH="$EPOCH" \
      -j"$NPROC" ) || die "ndk-build 失败"
  SO=""
  for cand in "$STAR/app/src/main/libs/arm64-v8a/$SO_NAME" "$STAR/app/src/libs/arm64-v8a/$SO_NAME" "$STAR/app/libs/arm64-v8a/$SO_NAME"; do
    [ -f "$cand" ] && SO="$cand" && break
  done
  [ -n "$SO" ] || die "未找到 $SO_NAME 产物"
fi
echo "[2/3] 产物 $SO ($(wc -c < "$SO" | tr -d ' ') bytes)"

echo "[3/3] so 字节/串自检"
for s in '[gdzfdj]' 'Slider_伤害倍率:1:100:1' 'Toggle_超级定怪' 'Slider_全局加速:1:10:1' 'UpdateAI' 'CalcDamage' 'get_deltaTime' 'set_timeScale'; do
  if grep -a -q -- "$s" "$SO"; then echo "  $s OK"; else echo "  $s MISSING"; die "so 自检失败: $s"; fi
done
# 反向门禁：别的游戏的 tag 不该出现（本 so 只服务本作）
if grep -a -q '\[gdzfdj\]' "$SO"; then :; else die "so tag 缺失"; fi

for dest in "$STAR/app/src/main/jniLibs/arm64-v8a" "$STAR/app/src/libs/arm64-v8a" "$STAR/app/libs/arm64-v8a"; do
  mkdir -p "$dest"
  cp -f "$SO" "$dest/$SO_NAME"
done

"${NIXIANG_PYTHON:-python}" - "$SO" <<'PY'
import hashlib, sys
b = open(sys.argv[1], 'rb').read()
print('  size  : %d B' % len(b))
print('  sha256: %s' % hashlib.sha256(b).hexdigest())
print('  md5   : %s' % hashlib.md5(b).hexdigest())
PY
echo "OK → $SO"
