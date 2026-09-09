#!/usr/bin/env bash
# 无悔华夏 — 仅换 libstarry_magic.so（底包须已是外科成品）
# STARRY_TRACE=1 时编 Trace 包；默认 release
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
NX="$(cd "$ROOT/../.." && pwd)"
PKG=com.yhhy.whrhx.aligames
OUT_DIR="$NX/08_output/无悔华夏"
BASE_APK="${WHRHX_BASE_APK:-$OUT_DIR/无悔华夏_3.8.280_r.apk}"
if [[ ! -f "$BASE_APK" && -f "$OUT_DIR/无悔华夏_3.8.280_surgical-releaseSigned.apk" ]]; then
  BASE_APK="$OUT_DIR/无悔华夏_3.8.280_surgical-releaseSigned.apk"
fi
if [[ ! -f "$BASE_APK" && -f "$OUT_DIR/无悔华夏_3.8.280.apk" ]]; then
  BASE_APK="$OUT_DIR/无悔华夏_3.8.280.apk"
fi
NDK="${NDK:-$(ls -d "$HOME/Library/Android/sdk/ndk/"* 2>/dev/null | sort -V | tail -1)}"

if [[ ! -f "$BASE_APK" ]]; then
  echo "ERROR: 缺少底包 — 先跑 surgical_whrhx_3.8.280.sh" >&2
  exit 1
fi

grep -q 'STARRY_GAME_WHRHX' "$ROOT/app/src/main/jni/Android.mk" || {
  echo "ERROR: Android.mk 未启用 STARRY_GAME_WHRHX" >&2
  exit 1
}

TRACE_ARGS=()
SUFFIX=release
if [[ "${STARRY_TRACE:-0}" == "1" ]]; then
  TRACE_ARGS+=(STARRY_TRACE=1)
  SUFFIX=trace
fi

echo "[1] ndk-build STARRY_GAME_WHRHX ${SUFFIX}"
(cd "$ROOT/app/src/main/jni" && \
  "$NDK/ndk-build" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk \
  NDK_APPLICATION_MK=./Application.mk ${TRACE_ARGS[@]+"${TRACE_ARGS[@]}"} -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")

SO="$ROOT/app/src/libs/arm64-v8a/libstarry_magic.so"
for dest in \
  "$ROOT/app/src/main/jniLibs/arm64-v8a" \
  "$ROOT/app/src/main/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$SO" "$dest/libstarry_magic.so"
done
if ! strings "$SO" | grep -q '\[Whrhx\]'; then
  echo "ERROR: so 自检失败，未见 [Whrhx]" >&2
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
SIGNED="$OUT_DIR/无悔华夏_3.8.280_surgical-${SUFFIX}Signed.apk"
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
  java -jar "$NX/01_tools/uber-apk-signer.jar" -a "$UNSIGNED" --out "$OUT_DIR" --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS"
  CAND=$(ls -t "$OUT_DIR"/*${SUFFIX}*Signed*.apk 2>/dev/null | head -1 || true)
  [[ -n "${CAND:-}" ]] && cp -f "$CAND" "$SIGNED"
fi
echo "signed → $SIGNED"
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  REMOTE="/data/local/tmp/whrhx_so.apk"
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  adb shell su -c "pm install -r -d --user 0 $REMOTE"
  adb shell rm -f "$REMOTE" 2>/dev/null || true
  echo "installed; log: adb logcat -s StarryMagic:V"
fi
