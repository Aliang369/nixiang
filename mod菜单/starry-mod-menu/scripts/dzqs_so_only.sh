#!/usr/bin/env bash
# 独奏骑士 — 只换 libstarry_magic.so（底包须已是 surgical 成品）
# 用法: STARRY_INSTALL=1 bash scripts/dzqs_so_only.sh
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="/Users/aliang/Documents/nixiang"
OUT="$NIXIANG/08_output/独奏骑士"
SIGNED="$OUT/独奏骑士_1.1.760_surgical-releaseSigned.apk"
PKG="com.Shimmer.SoloKnight.cn"
MK="$STARRY_SRC/app/src/main/jni/Android.mk"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
KS="$NIXIANG/01_tools/starry_release.jks"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$SIGNED" ]] || { echo "ERROR: missing $SIGNED (先跑 surgical_dzqs_1.1.760.sh)"; exit 1; }
grep -q 'STARRY_GAME_DZQS' "$MK" || { echo "ERROR: Android.mk not DZQS"; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

WORK="$(mktemp -d /tmp/dzqs_so_only.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

echo "[1/3] ndk-build"
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK/ndk-build" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")

SO=""
for cand in \
  "$STARRY_SRC/app/src/libs/arm64-v8a/libstarry_magic.so" \
  "$STARRY_SRC/app/src/obj/local/arm64-v8a/libstarry_magic.so"
do
  if [[ -f "$cand" ]] && grep -a -q 'dzqs' "$cand" && grep -a -q '\[Dzqs\]' "$cand"; then
    SO="$cand"; break
  fi
done
[[ -n "$SO" ]] || { echo "ERROR: so not DZQS"; exit 1; }
for dest in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$SO" "$dest/libstarry_magic.so"
done
echo "  so ok: $SO"

echo "[2/3] zip -u so into surgical apk"
cp -f "$SIGNED" "$WORK/base.apk"
(
  cd "$WORK"
  mkdir -p lib/arm64-v8a
  cp -f "$SO" lib/arm64-v8a/libstarry_magic.so
  zip -u base.apk lib/arm64-v8a/libstarry_magic.so
)

KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
ALIGNED="$WORK/aligned.apk"
if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$WORK/base.apk" "$ALIGNED"
  "$BT/apksigner" sign \
    --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$SIGNED" "$ALIGNED"
else
  java -jar "$NIXIANG/01_tools/uber-apk-signer.jar" -a "$WORK/base.apk" --out "$OUT" --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS"
fi
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  REMOTE="/data/local/tmp/dzqs_so_only.apk"
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'"
  adb shell "rm -f $REMOTE" 2>/dev/null || true
  echo "[install] cover ok"
fi
echo "DONE"
