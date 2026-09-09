#!/usr/bin/env bash
# 仅重编 libstarry_magic.so → 塞进魂坠已有正式签包 → 重签 Root 安装
# 用法: STARRY_INSTALL=1 bash scripts/hzsj_so_only.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="/Users/aliang/Documents/nixiang"
OUT="$NIXIANG/08_output/魂坠深境"
SIGNED="$OUT/魂坠深境_1.0.5_starry-releaseSigned.apk"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
PKG="com.chillyroom.containerofsoul.yw"
KS="${STARRY_KS:-$NIXIANG/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$SIGNED" ]] || { echo "ERROR: missing base APK $SIGNED (先跑 inject_hzsj_1.0.5.sh)"; exit 1; }
[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing"; exit 1; }
grep -qE '^[[:space:]]*LOCAL_CFLAGS[[:space:]]*\+=[[:space:]]*.*-DSTARRY_GAME_HZSJ' "$STARRY_SRC/app/src/main/jni/Android.mk" \
  || { echo "ERROR: Android.mk 未激活 STARRY_GAME_HZSJ（当前会编成别的游戏）"; exit 1; }
grep -qE '^[[:space:]]*LOCAL_SRC_FILES[[:space:]]*:=.*HzsjMod\.cpp' "$STARRY_SRC/app/src/main/jni/Android.mk" \
  || { echo "ERROR: Android.mk LOCAL_SRC_FILES 未含 Games/HzsjMod.cpp"; exit 1; }

APP="$STARRY_SRC/app"
WORK="$(mktemp -d /tmp/hzsj_so_only.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

BT_ZIPALIGN=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT_ZIPALIGN="$HOME/Library/Android/sdk/build-tools/$v/zipalign"
    break
  fi
done

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
ls -lh "$SO"

echo "[so-only 2/4] replace so inside APK"
cp -f "$SIGNED" "$WORK/base.apk"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$SO" "$WORK/lib/arm64-v8a/libstarry_magic.so"
( cd "$WORK" && zip -u -q base.apk "lib/arm64-v8a/libstarry_magic.so" )

echo "[so-only 3/4] resign (starry_release.jks)"
UBER_ARGS=(-a "$WORK/base.apk" --overwrite --allowResign)
[[ -n "$BT_ZIPALIGN" ]] && UBER_ARGS+=(--zipAlignPath "$BT_ZIPALIGN")
[[ -f "$KS" ]] && UBER_ARGS+=(--ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS")
java -jar "$SIGNER" "${UBER_ARGS[@]}"
SIGNED_SRC=""
for c in \
  "$WORK/base-aligned-releaseSigned.apk" \
  "$WORK/base-releaseSigned.apk" \
  "$WORK/base-aligned-debugSigned.apk" \
  "$WORK/base-debugSigned.apk" \
  "$WORK/base.apk"; do
  if [[ -f "$c" ]]; then SIGNED_SRC="$c"; break; fi
done
cp -f "$SIGNED_SRC" "$SIGNED"
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" != "1" ]]; then
  echo "skip install (STARRY_INSTALL=1 to install)"
  echo "done → $SIGNED"
  exit 0
fi

echo "[so-only 4/4] root install"
REMOTE=/data/local/tmp/starry_hzsj_so.apk
adb push "$SIGNED" "$REMOTE"
adb shell am force-stop "$PKG" 2>/dev/null || true
# 勿用 -g：会授予官方声明的 READ_LOGS，系统弹「访问所有设备日志」
if ! adb shell "su -c \"pm install -r -d --user 0 '$REMOTE'\""; then
  adb shell "su -c \"pm uninstall --user 0 '$PKG'\"" 2>/dev/null || true
  adb shell "su -c \"pm install -r -d --user 0 '$REMOTE'\"" || adb install --no-incremental "$SIGNED"
fi
adb shell "su -c \"pm revoke $PKG android.permission.READ_LOGS\"" 2>/dev/null || true
adb shell "su -c \"rm -f '$REMOTE'\"" 2>/dev/null || true
# shellcheck source=/dev/null
source "$NIXIANG/04_scripts/_common/starry_post_install.sh"
starry_post_install "$PKG"
adb shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1 || true
echo "done → $SIGNED"
echo "log: adb logcat -s StarryMagic Hzsj Mod_Menu"
