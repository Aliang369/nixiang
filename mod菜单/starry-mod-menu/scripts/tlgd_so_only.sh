#!/usr/bin/env bash
# 弹力果冻 — 只换 libstarry_magic.so（底包须已是 surgical 成品）
# 用法: STARRY_INSTALL=1 bash scripts/tlgd_so_only.sh
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
OUT="$NIXIANG/08_output/弹力果冻"
SIGNED="$OUT/弹力果冻_2.4.0_surgical-releaseSigned.apk"
PKG="com.ginicat.sweet.m4399"
MK="$STARRY_SRC/app/src/main/jni/Android.mk"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
KS="$NIXIANG/01_tools/starry_release.jks"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$SIGNED" ]] || { echo "ERROR: missing $SIGNED (先跑 surgical_tlgd_2.4.0.sh)"; exit 1; }
grep -q 'STARRY_GAME_TLGD' "$MK" || { echo "ERROR: Android.mk not TLGD"; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

WORK="$(mktemp -d /tmp/tlgd_so_only.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

echo "[1/3] ndk-build"
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK/ndk-build" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")

SO=""
for cand in \
  "$STARRY_SRC/app/src/libs/arm64-v8a/libstarry_magic.so" \
  "$STARRY_SRC/app/src/obj/local/arm64-v8a/libstarry_magic.so"
do
  if [[ -f "$cand" ]] && grep -a -q 'tlgd' "$cand" && grep -a -q '\[Tlgd\]' "$cand"; then
    SO="$cand"
    break
  fi
done
[[ -n "$SO" ]] || { echo "ERROR: no TLGD libstarry_magic.so"; exit 1; }
if grep -a -q '\[Dwct\]' "$SO"; then
  echo "ERROR: so still has [Dwct]"; exit 1
fi
echo "  so=$SO"
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
  REMOTE=/data/local/tmp/starry_tlgd_so.apk
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  adb shell su -c "pm install -r -d --user 0 $REMOTE" \
    || { echo "ERROR: cover install failed (do not uninstall ~2GB game)"; exit 2; }
  adb shell "rm -f $REMOTE" 2>/dev/null || true
fi
echo "DONE: $SIGNED"
