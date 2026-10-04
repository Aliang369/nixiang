#!/usr/bin/env bash
# 青鸾繁华录：仅重编 libstarry_magic.so → 塞进已有签名包 → 重签（可选安装）
# 前提：磁盘上已有 surgical_qlfh_9.8.146.sh 出的正式包
# 用法: STARRY_INSTALL=1 bash scripts/qlfh_so_only.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
OUT="$NIXIANG/08_output/青鸾繁华录"
SIGNED="$OUT/青鸾繁华录_9.8.146_surgical-releaseSigned.apk"
ALIAS_APK="$OUT/青鸾繁华录_9.8.146_starry.apk"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
PKG="com.hdg4.g4qlfh"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$SIGNED" ]] || { echo "ERROR: missing $SIGNED (先跑 surgical_qlfh_9.8.146.sh)"; exit 1; }
[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing"; exit 1; }
grep -q 'STARRY_GAME_QLFH' "$STARRY_SRC/app/src/main/jni/Android.mk" \
  || { echo "ERROR: Android.mk not QLFH"; exit 1; }

WORK="$(mktemp -d /tmp/qlfh_so_only.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

# 中文路径下 ndk-build 偶发异常：拷到 ASCII 再编
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' \
  "$STARRY_SRC/" "$WORK/starry/"
APP="$WORK/starry/app"

echo "[so-only 1/4] ndk-build arm64"
"$NDK/ndk-build" -C "$APP/src/main/jni" \
  NDK_PROJECT_PATH="$APP" \
  NDK_APPLICATION_MK="$APP/src/main/jni/Application.mk" \
  APP_BUILD_SCRIPT="$APP/src/main/jni/Android.mk" \
  APP_ABI=arm64-v8a \
  NDK_OUT="$APP/build/intermediates/ndk/obj" \
  NDK_LIBS_OUT="$APP/build/intermediates/ndk/libs" \
  -j8

SO="$APP/build/intermediates/ndk/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO" ]] || { echo "ERROR: missing $SO"; exit 1; }
grep -a -q '\[Qlfh\]' "$SO" || { echo "ERROR: so 不是 QLFH"; exit 1; }
ls -lh "$SO"

echo "[so-only 2/4] replace so inside APK"
cp -f "$SIGNED" "$WORK/base.apk"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$SO" "$WORK/lib/arm64-v8a/libstarry_magic.so"
(cd "$WORK" && zip -u -q base.apk "lib/arm64-v8a/libstarry_magic.so")

echo "[so-only 3/4] resign (与 surgical 包同 key：starry_release.jks)"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/apksigner" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
ALIGNED="$WORK/base_aligned.apk"
if [[ -n "$BT" && -x "$BT/zipalign" ]]; then
  "$BT/zipalign" -f -p 4 "$WORK/base.apk" "$ALIGNED"
  "$BT/apksigner" sign \
    --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$WORK/base_signed.apk" "$ALIGNED"
else
  java -jar "$SIGNER" -a "$WORK/base.apk" --overwrite --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS" >/dev/null
  cp -f "$WORK/base.apk" "$WORK/base_signed.apk"
fi
SIGNED_SRC="$WORK/base_signed.apk"
cp -f "$SIGNED_SRC" "$SIGNED"
cp -f "$SIGNED" "$ALIAS_APK"
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" != "1" ]]; then
  echo "skip install (STARRY_INSTALL=1 to install)"
  echo "done → $SIGNED"
  exit 0
fi

echo "[so-only 4/4] adb install (同签名覆盖)"
adb push "$SIGNED" /data/local/tmp/starry_qlfh_so.apk
adb shell am force-stop "$PKG" 2>/dev/null || true
if ! adb shell su -c "pm install -r -d --user 0 /data/local/tmp/starry_qlfh_so.apk"; then
  adb install -r --no-incremental "$SIGNED"
fi
adb shell rm -f /data/local/tmp/starry_qlfh_so.apk 2>/dev/null || true
adb shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1 || true
echo "done → $SIGNED (so-only)"
echo "log: adb logcat -s Mod_Menu:I StarryMagic:I"
