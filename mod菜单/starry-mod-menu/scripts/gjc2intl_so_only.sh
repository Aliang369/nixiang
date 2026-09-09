#!/usr/bin/env bash
# 弓箭传说2 — 只换 libstarry_magic.so（底包须已是 surgical 成品）
# 用法: STARRY_INSTALL=1 bash scripts/gjc2intl_so_only.sh
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="/Users/aliang/Documents/nixiang"
OUT="$NIXIANG/08_output/弓箭传说2国际版"
SIGNED="$OUT/弓箭传说2国际版_1.0.1_surgical-releaseSigned.apk"
PKG="com.facai.archeroii"
MK="$STARRY_SRC/app/src/main/jni/Android.mk"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
KS="$NIXIANG/01_tools/starry_release.jks"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$SIGNED" ]] || { echo "ERROR: missing $SIGNED (先跑 surgical_gjc2intl_1.0.1.sh)"; exit 1; }
grep -q 'STARRY_GAME_GJC2INTL' "$MK" || { echo "ERROR: Android.mk not GJC2INTL"; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

WORK="$(mktemp -d /tmp/gjc2intl_so_only.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

echo "[1/3] ndk-build"
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK/ndk-build" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")

SO=""
for cand in \
  "$STARRY_SRC/app/src/libs/arm64-v8a/libstarry_magic.so" \
  "$STARRY_SRC/app/src/obj/local/arm64-v8a/libstarry_magic.so"
do
  if [[ -f "$cand" ]] && grep -a -q 'gjc2intl' "$cand" && grep -a -q '\[Gjc2Intl\]' "$cand"; then
    SO="$cand"
    break
  fi
done
[[ -n "$SO" ]] || { echo "ERROR: no GJC2INTL libstarry_magic.so"; exit 1; }
echo "  so=$SO"
# 同步 jniLibs，避免下次 surgical/gradle 又打出旧游戏 so
for dest in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$SO" "$dest/libstarry_magic.so"
done

echo "[2/3] zip -u so + resign"
cp "$SIGNED" "$WORK/base.apk"
(
  cd "$WORK"
  mkdir -p lib/arm64-v8a
  cp "$SO" lib/arm64-v8a/libstarry_magic.so
  zip -u base.apk lib/arm64-v8a/libstarry_magic.so
)
ALIGNED="$WORK/aligned.apk"
"$BT/zipalign" -f -p 4 "$WORK/base.apk" "$ALIGNED"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "${STARRY_KS_ALIAS:-starry}" \
  --ks-pass "pass:${STARRY_KS_PASS:-starry2026}" --key-pass "pass:${STARRY_KS_PASS:-starry2026}" \
  --out "$SIGNED" "$ALIGNED"
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  REMOTE=/data/local/tmp/starry_gjc2_so.apk
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  adb shell su -c "pm install -r -d --user 0 $REMOTE" \
    || adb install -r -d "$SIGNED"
  adb shell "rm -f $REMOTE" 2>/dev/null || true
fi
echo "DONE: $SIGNED"
