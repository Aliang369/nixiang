#!/usr/bin/env bash
# 战魂铭人 — 只换 libstarry_magic.so（底包须已是 starry 成品）
# 用法: STARRY_INSTALL=1 bash scripts/zhmr_so_only.sh
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="/Users/aliang/Documents/nixiang"
OUT="$NIXIANG/08_output/战魂铭人"
# 真机当前为 3.5.0 外科包（starry 签名）
SIGNED="$OUT/战魂铭人_3.5.0_surgical-releaseSigned.apk"
[[ -f "$SIGNED" ]] || SIGNED="$OUT/战魂铭人_3.5.0_surgical-debugSigned.apk"
PKG="com.chillyroom.zhmr.yw"
MK="$STARRY_SRC/app/src/main/jni/Android.mk"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
KS="$NIXIANG/01_tools/starry_release.jks"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$SIGNED" ]] || { echo "ERROR: missing base apk under $OUT"; exit 1; }
grep -q 'STARRY_GAME_ZHMR' "$MK" || { echo "ERROR: Android.mk not ZHMR"; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: no build-tools"; exit 1; }

WORK="$(mktemp -d /tmp/zhmr_so_only.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

echo "[1/3] ndk-build"
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK/ndk-build" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")

SO=""
for cand in \
  "$STARRY_SRC/app/src/libs/arm64-v8a/libstarry_magic.so" \
  "$STARRY_SRC/app/src/obj/local/arm64-v8a/libstarry_magic.so"
do
  if [[ -f "$cand" ]] && grep -a -q 'zhmr' "$cand" && grep -a -q '\[Zhmr\]' "$cand"; then
    SO="$cand"
    break
  fi
done
[[ -n "$SO" ]] || { echo "ERROR: no ZHMR libstarry_magic.so"; exit 1; }
echo "  so=$SO"

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

# 签名必须与底包一致（starry keystore），否则 pm install -r 报签名不匹配。
# uber-apk-signer 不带 --ks 会用其 debug key，仅作无 keystore 时的兜底。
KS="${STARRY_KS:-$NIXIANG/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
if [[ -f "$KS" && -x "$BT/apksigner" ]]; then
  "$BT/apksigner" sign \
    --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$SIGNED" "$ALIGNED"
else
  SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
  java -jar "$SIGNER" -a "$ALIGNED" -o "$WORK/out" --allowResign 2>/dev/null \
    || java -jar "$SIGNER" --apks "$ALIGNED" --out "$WORK/out" --allowResign
  NEW="$(find "$WORK/out" -name '*.apk' | head -1)"
  [[ -n "$NEW" ]] || { echo "ERROR: uber sign failed"; exit 1; }
  cp "$NEW" "$SIGNED"
fi
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  REMOTE=/data/local/tmp/starry_zhmr_so.apk
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  adb shell su -c "pm install -r -d --user 0 $REMOTE" \
    || adb install -r "$SIGNED"
fi
echo "DONE"
