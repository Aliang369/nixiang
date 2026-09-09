#!/usr/bin/env bash
# 消消英雄2：只换 libstarry_magic.so（外科 zip -u，禁止 apktool 整包重建）
#
# 用法:
#   STARRY_INSTALL=1 bash scripts/xxh2_so_only.sh
#   STARRY_BASE_APK=... STARRY_INSTALL=1 bash scripts/xxh2_so_only.sh
#
# 默认底包：native-menu 外科成品；默认签名 uber-debug（与真机旧包同证）。
# 正式证：STARRY_KS=.../starry_release.jks
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="/Users/aliang/Documents/nixiang"
OUT="$NIXIANG/08_output/消消英雄2"
PKG="com.blina.match2.tt"
BASE="${STARRY_BASE_APK:-$OUT/消消英雄2_1.2.26_native-menu-releaseSigned.apk}"
SIGNED="$OUT/消消英雄2_1.2.26_starry-soOnly-releaseSigned.apk"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
KS="${STARRY_KS:-$NIXIANG/01_tools/uber-debug.keystore}"
export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_UNITY_HOOK\s+-DSTARRY_GAME_MATCH2' "$MK" \
  || { echo "ERROR: enable STARRY_GAME_MATCH2 in Android.mk"; exit 1; }
[[ -f "$BASE" ]] || { echo "ERROR: missing base apk: $BASE"; exit 1; }

APP="$STARRY_SRC/app"
LUA_TRACE="${STARRY_LUA_DUMP_TRACE:-0}"
EXTRA_CFLAGS=""
if [[ "$LUA_TRACE" == "1" ]]; then
  EXTRA_CFLAGS="-DMATCH2_LUA_DUMP_TRACE=1"
  echo "build: MATCH2_LUA_DUMP_TRACE=1 (lua dump to starry_lua_dump/)"
fi
"$NDK/ndk-build" -C "$APP/src/main/jni" \
  NDK_PROJECT_PATH="$APP" \
  NDK_APPLICATION_MK="$APP/src/main/jni/Application.mk" \
  APP_BUILD_SCRIPT="$APP/src/main/jni/Android.mk" \
  APP_ABI=arm64-v8a \
  NDK_OUT="$APP/build/intermediates/ndk/obj" \
  NDK_LIBS_OUT="$APP/build/intermediates/ndk/libs" \
  APP_CFLAGS="$EXTRA_CFLAGS" \
  -j8

SO="$APP/build/intermediates/ndk/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO" ]] || { echo "ERROR: ndk-build did not produce $SO"; exit 1; }

mkdir -p "$APP/src/main/jniLibs/arm64-v8a"
cp -f "$SO" "$APP/src/main/jniLibs/arm64-v8a/libstarry_magic.so"

if ! strings "$SO" | grep -q '\[Match2\]'; then
  echo "ERROR: so missing [Match2] tag"
  exit 1
fi
if strings "$SO" | grep -q '__starry_ac_ok'; then
  echo "ERROR: so 仍含过检测标记 __starry_ac_ok（本版应关）"
  exit 1
fi

WORK="$(mktemp -d /tmp/xxh2_so_only.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
cp -f "$BASE" "$WORK/in.apk"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$SO" "$WORK/lib/arm64-v8a/libstarry_magic.so"
(cd "$WORK" && zip -u in.apk lib/arm64-v8a/libstarry_magic.so)

if [[ "$KS" == *"starry_release"* ]]; then
  java -jar "$SIGNER" -a "$WORK/in.apk" --overwrite --allowResign \
    --ks "$KS" --ksAlias starry --ksPass starry2026 --ksKeyPass starry2026
else
  java -jar "$SIGNER" -a "$WORK/in.apk" --overwrite --allowResign \
    --ks "$KS" --ksAlias androiddebugkey --ksPass android --ksKeyPass android
fi

mkdir -p "$OUT"
if [[ -f "$WORK/in-aligned-releaseSigned.apk" ]]; then
  cp -f "$WORK/in-aligned-releaseSigned.apk" "$SIGNED"
elif [[ -f "$WORK/in-releaseSigned.apk" ]]; then
  cp -f "$WORK/in-releaseSigned.apk" "$SIGNED"
else
  cp -f "$WORK/in.apk" "$SIGNED"
fi
# 同步覆盖正式别名，避免下次底包指到旧文件
cp -f "$SIGNED" "$OUT/消消英雄2_1.2.26_native-menu-releaseSigned.apk"
cp -f "$SIGNED" "$OUT/消消英雄2_1.2.26_starry.apk"
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  REMOTE="/data/local/tmp/starry_xxh2_so.apk"
  adb push "$SIGNED" "$REMOTE"
  adb shell su -c "pm install -r -d --user 0 '$REMOTE'" || adb install -r -d "$SIGNED"
  if [[ -f "$NIXIANG/04_scripts/_common/starry_post_install.sh" ]]; then
    # shellcheck disable=SC1091
    source "$NIXIANG/04_scripts/_common/starry_post_install.sh"
    starry_post_install "$PKG" || true
  fi
  bash "$NIXIANG/04_scripts/_common/ensure_starry_device_id_seed.sh" || true
fi
echo "done → $SIGNED"
echo "log: adb logcat -s StarryMagic:V NativeMenu:V Match2:V | grep -E 'xxh2|Match2|NativeMenu'"
