#!/usr/bin/env bash
# 长生天机降世 0.9.10 — 只换 libstarry_magic.so（底包须已是 surgical）
# 用法: STARRY_INSTALL=1 bash scripts/cstjjs_so_only.sh
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
OUT="$NIXIANG/08_output/长生天机降世"
SIGNED="$OUT/长生天机降世_0.9.10_surgical-releaseSigned.apk"
PKG="com.netkuu.changsheng.tjjs"
MK="$STARRY_SRC/app/src/main/jni/Android.mk"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
KS="$NIXIANG/01_tools/starry_release.jks"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$SIGNED" ]] || { echo "ERROR: missing $SIGNED"; exit 1; }
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_UNITY_HOOK\s+-DSTARRY_GAME_CSTJJS' "$MK" \
  || { echo "ERROR: Android.mk not CSTJJS"; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: zipalign missing"; exit 1; }

WORK="$(mktemp -d /tmp/cstjjs_so_only.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

echo "[1/3] ndk-build"
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK/ndk-build" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")

SO="$STARRY_SRC/app/src/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO" ]] || { echo "ERROR: missing $SO"; exit 1; }
if ! grep -a -q '\[Cstjjs\]' "$SO"; then
  echo "ERROR: so 不是 CSTJJS"
  strings "$SO" | grep -E 'Cstjjs|Ltzj|Pcr' | head
  exit 1
fi
for dest in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$SO" "$dest/libstarry_magic.so"
done
echo "  so ok"

echo "[2/3] zip -u + sign"
cp "$SIGNED" "$WORK/base.apk"
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO" "$WORK/lib/arm64-v8a/libstarry_magic.so"
(cd "$WORK" && zip -u base.apk lib/arm64-v8a/libstarry_magic.so)
ALIGNED="$WORK/aligned.apk"
"$BT/zipalign" -f -p 4 "$WORK/base.apk" "$ALIGNED"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "${STARRY_KS_ALIAS:-starry}" \
  --ks-pass "pass:${STARRY_KS_PASS:-starry2026}" --key-pass "pass:${STARRY_KS_PASS:-starry2026}" \
  --out "$SIGNED" "$ALIGNED"
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  echo "[3/3] cover install"
  REMOTE=/data/local/tmp/starry_cstjjs_so.apk
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  adb shell "su -c \"pm install -r -d --user 0 '$REMOTE'\"" \
    || { echo "ERROR: pm install failed"; exit 1; }
  adb shell "su -c \"rm -f '$REMOTE'\"" 2>/dev/null || true
fi
echo "DONE $SIGNED"
