#!/usr/bin/env bash
# 霓虹深渊 — 仅换 libstarry_magic.so（底包须已是外科/星光成品）
# 用法:
#   STARRY_INSTALL=1 bash scripts/neon_so_only.sh
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
NX="$(cd "$ROOT/../.." && pwd)"
PKG=com.shangyoo.neon
OUT_DIR="$NX/08_output/霓虹深渊"
BASE_APK="${NEON_BASE_APK:-$OUT_DIR/霓虹深渊_3.2.860_surgical-releaseSigned.apk}"
if [[ ! -f "$BASE_APK" && -f "$OUT_DIR/霓虹深渊_3.2.860_starry-releaseSigned.apk" ]]; then
  BASE_APK="$OUT_DIR/霓虹深渊_3.2.860_starry-releaseSigned.apk"
fi
NDK="${NDK:-$(ls -d "$HOME/Library/Android/sdk/ndk/"* 2>/dev/null | sort -V | tail -1)}"

if [[ ! -f "$BASE_APK" ]]; then
  echo "ERROR: 缺少底包 $BASE_APK — 先跑 surgical_neon_3.2.860.sh" >&2
  exit 1
fi

echo "[1] ndk-build STARRY_GAME_NEON"
grep -q 'STARRY_GAME_NEON' "$ROOT/app/src/main/jni/Android.mk" || {
  echo "ERROR: Android.mk 未启用 STARRY_GAME_NEON" >&2
  exit 1
}
"$NDK/ndk-build" -C "$ROOT/app/src/main/jni" \
  NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk -j8

SO="$ROOT/app/src/libs/arm64-v8a/libstarry_magic.so"
cp -f "$SO" "$ROOT/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so"
if ! strings "$SO" | grep -q '\[Neon\]'; then
  echo "ERROR: so 自检失败，未见 [Neon]" >&2
  exit 1
fi
if strings "$SO" | grep -qE '\[Wggls\]|\[Shhj\]|\[Gyws\]'; then
  echo "ERROR: so 仍含其它游戏标签" >&2
  exit 1
fi

WORKDIR=$(mktemp -d)
trap 'rm -rf "$WORKDIR"' EXIT
cp -f "$BASE_APK" "$WORKDIR/base.apk"
(
  cd "$WORKDIR"
  mkdir -p lib/arm64-v8a
  cp -f "$SO" lib/arm64-v8a/libstarry_magic.so
  zip -u base.apk lib/arm64-v8a/libstarry_magic.so
)

UNSIGNED="$WORKDIR/base.apk"
SIGNED="$OUT_DIR/霓虹深渊_3.2.860_surgical-releaseSigned.apk"
mkdir -p "$OUT_DIR"
KS="$NX/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/apksigner" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
if [[ -n "$BT" && -f "$KS" ]]; then
  "$BT/zipalign" -f -p 4 "$UNSIGNED" "$WORKDIR/aligned.apk"
  "$BT/apksigner" sign \
    --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$SIGNED" "$WORKDIR/aligned.apk"
else
  UBER_ARGS=(-a "$UNSIGNED" --out "$OUT_DIR" --allowResign --overwrite)
  [[ -n "$BT" ]] && UBER_ARGS+=(--zipAlignPath "$BT/zipalign")
  java -jar "$NX/01_tools/uber-apk-signer.jar" "${UBER_ARGS[@]}"
  CAND=$(ls -t "$OUT_DIR"/*Signed*.apk "$OUT_DIR"/*aligned*.apk 2>/dev/null | head -1 || true)
  [[ -n "${CAND:-}" ]] && cp -f "$CAND" "$SIGNED"
fi
cp -f "$SIGNED" "$OUT_DIR/霓虹深渊_3.2.860_starry-releaseSigned.apk"
echo "signed → $SIGNED"
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  adb push "$SIGNED" /data/local/tmp/neon_so.apk
  adb shell am force-stop "$PKG" 2>/dev/null || true
  adb shell su -c "pm install -r -d --user 0 /data/local/tmp/neon_so.apk" \
    || adb install -r --no-incremental "$SIGNED"
  adb shell rm -f /data/local/tmp/neon_so.apk 2>/dev/null || true
  echo "installed; check: adb logcat -s StarryApp:V StarryMagic:V Neon"
fi
