#!/usr/bin/env bash
# 山海幻境：在隔离副本中构建 legacy SHHJ so，不修改工作区 Android.mk。
# 换版本时用 SHHJ_BUILD_DIR 指定独立产物目录（例：SHHJ_BUILD_DIR=.../build_4.3），
# 避免与上一版本的 so 产物互相覆盖；默认沿用 3.8/4.0 时代的 build_4。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
SRC="$NIXIANG_ROOT/mod菜单/starry-mod-menu"
OUT_DIR="${SHHJ_BUILD_DIR:-$NIXIANG_ROOT/07_patched/山海幻境/build_4}"
OUT_SO="$OUT_DIR/libstarry_magic.so"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_BUILD="$NDK/ndk-build"

[[ -x "$NDK_BUILD" ]] || { echo "错误: NDK ndk-build 不存在: $NDK_BUILD" >&2; exit 1; }
[[ -f "$SRC/app/src/main/jni/Games/ShhjMod.cpp" ]] || { echo "错误: ShhjMod.cpp 不存在" >&2; exit 1; }

WORK="$(mktemp -d /tmp/shhj_build.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
mkdir -p "$WORK/src" "$OUT_DIR" "$WORK/obj" "$WORK/libs"

rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$SRC/" "$WORK/src/"
rm -rf "$WORK/src/obj" "$WORK/src/libs" "$WORK/src/app/src/main/libs" "$WORK/src/app/libs"

python3 - "$WORK/src/app/src/main/jni/Android.mk" <<'PY'
from pathlib import Path
import re
import sys

p = Path(sys.argv[1])
lines = p.read_text(encoding="utf-8").splitlines()
out = []
inserted_flags = False
inserted_src = False
for line in lines:
    if re.match(r"^\s*LOCAL_CFLAGS\s*\+=", line) and "-DSTARRY_GAME_" in line:
        if not inserted_flags:
            out.append("LOCAL_CFLAGS    += -DENABLE_COCOS_HOOK -DSTARRY_GAME_SHHJ")
            inserted_flags = True
        else:
            out.append("# " + line)
        continue
    if re.match(r"^\s*LOCAL_SRC_FILES\s*:=\s*Main\.cpp", line):
        if not inserted_src:
            out.append("LOCAL_SRC_FILES := Main.cpp Games/ShhjMod.cpp")
            inserted_src = True
        else:
            out.append("# " + line)
        continue
    out.append(line)
if not inserted_flags or not inserted_src:
    raise SystemExit("failed to inject SHHJ build flags into Android.mk")
p.write_text("\n".join(out) + "\n", encoding="utf-8")
print("Android.mk -> SHHJ (isolated)")
PY

(
  cd "$WORK/src/app/src/main/jni"
  "$NDK_BUILD" \
    NDK_PROJECT_PATH=../.. \
    APP_BUILD_SCRIPT=./Android.mk \
    NDK_APPLICATION_MK=./Application.mk \
    NDK_OUT="$WORK/obj" \
    NDK_LIBS_OUT="$WORK/libs" \
    -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)"
)

BUILT="$WORK/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$BUILT" ]] || { echo "错误: 构建产物缺失: $BUILT" >&2; exit 1; }
grep -a -q '\[Shhj\]' "$BUILT" || { echo "错误: so 缺少 [Shhj] 标签" >&2; exit 1; }
grep -a -q 'shhj' "$BUILT" || { echo "错误: so 缺少 shhj game_id" >&2; exit 1; }
if grep -a -Eq '\[Zxr\]|\[Dwdlm\]|\[Zjmc\]|\[Es2\]' "$BUILT"; then
  echo "错误: so 混入其他游戏标签" >&2
  exit 1
fi

cp -f "$BUILT" "$OUT_SO"
printf 'output=%s\nmd5=%s\nsize=%s\n' "$OUT_SO" "$(md5 -q "$OUT_SO")" "$(stat -f '%z' "$OUT_SO")"
