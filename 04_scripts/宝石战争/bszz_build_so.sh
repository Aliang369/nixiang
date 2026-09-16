#!/usr/bin/env bash
# 宝石战争 6.0.012：构建星光 native 桥接 so 并刷新 jniLibs。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OBJ="$STARRY_SRC/obj_bszz"
LIBS="$STARRY_SRC/libs_bszz"
SO="$LIBS/arm64-v8a/libstarry_magic.so"
MK="$JNI/Android_bszz.mk"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$MK" ]] || { echo "ERROR: missing Android_bszz.mk" >&2; exit 1; }
grep -q 'Games/BszzMod.cpp' "$MK" || { echo "ERROR: BszzMod.cpp not linked" >&2; exit 1; }
grep -q 'Games/BszzEntry.cpp' "$MK" || { echo "ERROR: BszzEntry.cpp not linked" >&2; exit 1; }

if [[ -d "$OBJ" ]]; then
  mv "$OBJ" "${OBJ}.old.$(date +%s)"
fi
if [[ -d "$LIBS" ]]; then
  mv "$LIBS" "${LIBS}.old.$(date +%s)"
fi

"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_bszz.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$LIBS" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 8)"

[[ -f "$SO" ]] || { echo "ERROR: build produced no $SO" >&2; exit 1; }
LC_ALL=C grep -a -q '\[Bszz\]' "$SO" || { echo "ERROR: so 缺少 [Bszz]" >&2; exit 1; }
LC_ALL=C grep -a -q 'bszz' "$SO" || { echo "ERROR: so 缺少 bszz" >&2; exit 1; }
LC_ALL=C grep -a -q 'Slider_攻击倍率:1:100:1' "$SO" || { echo "ERROR: so 缺少攻击倍率菜单" >&2; exit 1; }
for stale in '\[Es2\]' '\[Dwdlm\]' '\[Haly\]' '\[Zjmc\]' '\[Zxr\]'; do
  if LC_ALL=C grep -a -q "$stale" "$SO"; then
    echo "ERROR: so 残留其他游戏标签: $stale" >&2
    exit 1
  fi
done

NM="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-nm"
[[ -x "$NM" ]] || { echo "ERROR: llvm-nm missing" >&2; exit 1; }
for sym in JNI_OnLoad StarryGw_AdjustDamage StarryGw_AdjustHealthValue StarryGw_ShouldKeepMana StarryGw_ShouldBlockRunAway; do
  "$NM" -D --defined-only "$SO" | grep -q " $sym$" || {
    echo "ERROR: missing exported symbol $sym" >&2
    exit 1
  }
done

for dest in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"; do
  mkdir -p "$dest"
  cp -f "$SO" "$dest/libstarry_magic.so"
done

cat <<EOF
[build_so] so=$SO
[build_so] md5=$(md5 -q "$SO")
[build_so] sha256=$(shasum -a 256 "$SO" | awk '{print $1}')
[build_so] size=$(stat -f '%z' "$SO")
[build_so] jniLibs=$STARRY_SRC/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so
EOF
