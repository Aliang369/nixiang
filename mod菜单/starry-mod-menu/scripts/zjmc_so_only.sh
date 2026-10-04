#!/usr/bin/env bash
# 追迹迷城：只重编 so，zip 进已验外科底包（禁止 apktool 整包）
#
# 用法:
#   STARRY_INSTALL=1 bash scripts/zjmc_so_only.sh
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
OUT="$NIXIANG/08_output/追迹迷城"
PKG="com.TeaMelo.DungeonChasers"
BASE="${STARRY_BASE_APK:-$OUT/追迹迷城_1.0.20260824_surgical-releaseSigned.apk}"
SIGNED="$OUT/追迹迷城_1.0.20260824_surgical-soOnly-releaseSigned.apk"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
KS="$NIXIANG/01_tools/starry_release.jks"
export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: no zipalign"; exit 1; }

MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_UNITY_HOOK\s+-DSTARRY_GAME_ZJMC' "$MK" \
  || { echo "ERROR: enable STARRY_GAME_ZJMC in Android.mk"; exit 1; }
[[ -f "$BASE" ]] || { echo "ERROR: missing base $BASE"; exit 1; }

APP="$STARRY_SRC/app"
NDK_BUILD="$NDK/ndk-build"
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK_BUILD" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
SO="$STARRY_SRC/app/src/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO" ]] || { echo "ERROR: missing $SO"; exit 1; }
if ! grep -a -q '\[Zjmc\]' "$SO" || ! grep -a -q 'zjmc' "$SO"; then
  echo "ERROR: so 不是 ZJMC"; exit 1
fi
for dest in \
  "$APP/src/main/jniLibs/arm64-v8a" \
  "$APP/src/main/libs/arm64-v8a" \
  "$APP/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$SO" "$dest/libstarry_magic.so"
done
echo "[so] jniLibs refreshed ([Zjmc])"

WORK="$(mktemp -d /tmp/zjmc_so_only.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
cp -f "$BASE" "$WORK/in.apk"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$SO" "$WORK/lib/arm64-v8a/libstarry_magic.so"
(cd "$WORK" && zip -u in.apk lib/arm64-v8a/libstarry_magic.so)
"$BT/zipalign" -f -p 4 "$WORK/in.apk" "$WORK/aligned.apk"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias starry \
  --ks-pass pass:starry2026 --key-pass pass:starry2026 \
  --out "$SIGNED" "$WORK/aligned.apk"
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  REMOTE=/data/local/tmp/starry_zjmc_so.apk
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  adb shell "su -c \"pm install -r -d --user 0 '$REMOTE'\""
  adb shell "su -c \"rm -f '$REMOTE'\"" 2>/dev/null || true
fi
echo "DONE $SIGNED"
