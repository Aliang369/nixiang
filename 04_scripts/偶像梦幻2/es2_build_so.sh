#!/usr/bin/env bash
# 偶像梦幻祭2：构建 libstarry_magic.so，强制刷新 jniLibs 并校验游戏标签。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OBJ="$STARRY_SRC/obj_es2"
LIBS="$STARRY_SRC/libs_es2"
SO="$LIBS/arm64-v8a/libstarry_magic.so"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$JNI/Android_es2.mk" ]] || { echo "ERROR: missing Android_es2.mk" >&2; exit 1; }

grep -Eq '^[[:space:]]*LOCAL_CFLAGS[[:space:]]*\+=[[:space:]]*-DENABLE_UNITY_HOOK[[:space:]]+-DSTARRY_GAME_ES2' "$JNI/Android_es2.mk" \
  || { echo "ERROR: Android_es2.mk 未启用 STARRY_GAME_ES2" >&2; exit 1; }
grep -q 'Games/Es2Mod.cpp' "$JNI/Android_es2.mk" || { echo "ERROR: Android_es2.mk 未编入 Es2Mod.cpp" >&2; exit 1; }

rm -rf "$OBJ" "$LIBS"
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_es2.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$LIBS" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 8)"

[[ -f "$SO" ]] || { echo "ERROR: build produced no $SO" >&2; exit 1; }
LC_ALL=C grep -a -q '\[Es2\]' "$SO" || { echo "ERROR: so 缺少 [Es2]" >&2; exit 1; }
LC_ALL=C grep -a -q 'oxmhj2' "$SO" || { echo "ERROR: so 缺少 oxmhj2" >&2; exit 1; }
LC_ALL=C grep -a -q 'Toggle_自动跳舞' "$SO" || { echo "ERROR: so 缺少 Toggle_自动跳舞" >&2; exit 1; }
for stale in '\[Zjmc\]' '\[Yntt\]' '\[Zzbx2\]'; do
  if LC_ALL=C grep -a -q "$stale" "$SO"; then echo "ERROR: so 残留其他游戏标签: $stale" >&2; exit 1; fi
done

for dest in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"; do
  mkdir -p "$dest"
  cp -f "$SO" "$dest/libstarry_magic.so"
done

NM="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-nm"
if [[ -x "$NM" ]]; then
  "$NM" -D --defined-only "$SO" | grep -q 'JNI_OnLoad' || { echo "ERROR: so 未导出 JNI_OnLoad" >&2; exit 1; }
fi

cat <<EOF
[build_so] so=$SO
[build_so] md5=$(md5 -q "$SO")
[build_so] sha256=$(shasum -a 256 "$SO" | awk '{print $1}')
[build_so] size=$(stat -f '%z' "$SO")
[build_so] jniLibs=$STARRY_SRC/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so
EOF
