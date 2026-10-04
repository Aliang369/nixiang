#!/usr/bin/env bash
# Phigros：zip 外科换 so；改 Java 菜单时 baksmali 只合并 com/starry 进 classes2.dex
#
# 用法:
#   STARRY_INSTALL=1 bash scripts/phigros_so_only.sh              # 默认：换 so + 合并菜单 dex
#   STARRY_UPDATE_MENU=0 STARRY_INSTALL=1 bash scripts/phigros_so_only.sh  # 仅 native（快，会丢 UI 默认值）
#   STARRY_USE_CLEAN_BASE=1 …  # 从 02_apks/phigros 干净底包重来（救砖）
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
OUT="$NIXIANG/08_output/phigros"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
KS="${STARRY_KS:-$NIXIANG/01_tools/starry_release.jks}"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
PKG="com.PigeonGames.Phigros"
SIGNED="$OUT/phigros_3.20.0_starry-releaseSigned.apk"
ORIG="$NIXIANG/02_apks/phigros/phigros_3.20.0_orig_base.apk"

if [[ -n "${STARRY_UPDATE_MENU_DEX:-}" ]]; then
  echo "WARN: STARRY_UPDATE_MENU_DEX 已废弃，请用 STARRY_UPDATE_MENU=1"
  STARRY_UPDATE_MENU=1
fi

# 底包：默认用已有成品（保留菜单 dex）；仅首包或救砖时 STARRY_USE_CLEAN_BASE=1
# 默认合并菜单 dex（滑条默认值/分类展开），纯改 native 可 STARRY_UPDATE_MENU=0
STARRY_UPDATE_MENU="${STARRY_UPDATE_MENU:-1}"

if [[ -z "${STARRY_BASE_APK:-}" ]]; then
  if [[ "${STARRY_USE_CLEAN_BASE:-0}" == "1" && -f "$ORIG" ]]; then
    STARRY_BASE_APK="$ORIG"
    echo "[base] clean orig (STARRY_USE_CLEAN_BASE=1)"
  elif [[ -f "$SIGNED" ]]; then
    STARRY_BASE_APK="$SIGNED"
    echo "[base] 已有成品 $SIGNED"
  elif [[ -f "$ORIG" ]]; then
    STARRY_BASE_APK="$ORIG"
    echo "[base] clean orig（尚无成品）"
  else
    echo "ERROR: no base apk"; exit 1
  fi
fi

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -qE '^[[:space:]]*LOCAL_CFLAGS[[:space:]]*\+=[[:space:]]*.*-DSTARRY_GAME_PHIGROS' "$MK" \
  || { echo "ERROR: Android.mk 未激活 STARRY_GAME_PHIGROS"; exit 1; }

APP="$STARRY_SRC/app"
echo "[1] ndk-build"
"$NDK/ndk-build" -C "$APP/src/main/jni" \
  NDK_PROJECT_PATH="$APP" \
  NDK_APPLICATION_MK="$APP/src/main/jni/Application.mk" \
  APP_BUILD_SCRIPT="$APP/src/main/jni/Android.mk" \
  APP_ABI=arm64-v8a \
  NDK_OUT="$APP/build/intermediates/ndk/obj" \
  NDK_LIBS_OUT="$APP/build/intermediates/ndk/libs" -j8

SO="$APP/build/intermediates/ndk/libs/arm64-v8a/libstarry_magic.so"
strings "$SO" | grep -q '\[Phigros\]'
mkdir -p "$APP/src/main/jniLibs/arm64-v8a"
cp -f "$SO" "$APP/src/main/jniLibs/arm64-v8a/libstarry_magic.so"

DEX_SZ="$(unzip -l "$STARRY_BASE_APK" classes2.dex 2>/dev/null | awk '/classes2\.dex/{print $1}')"
[[ -n "$DEX_SZ" && "$DEX_SZ" -gt 100000 ]] || {
  echo "ERROR: base classes2.dex 异常，请用: $ORIG"; exit 1
}

WORK="$(mktemp -d /tmp/phigros_zip.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
cp -f "$STARRY_BASE_APK" "$WORK/in.apk"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$SO" "$WORK/lib/arm64-v8a/libstarry_magic.so"
ZIP_ARGS=(lib/arm64-v8a/libstarry_magic.so)

if [[ "${STARRY_UPDATE_MENU:-0}" == "1" ]]; then
  [[ -x "$SMALI_BIN" && -x "$BAKSMALI_BIN" ]] || { echo "ERROR: smali/baksmali missing"; exit 1; }
  echo "[1b] gradle + apktool d starry 模板（小 APK，快）"
  (cd "$STARRY_SRC" && ./gradlew assembleRelease --console=plain -q)
  STAR_APK="$STARRY_SRC/app/build/outputs/apk/release/app-release.apk"
  [[ -f "$STAR_APK" ]] || STAR_APK="$STARRY_SRC/app/build/outputs/apk/release/app-release-unsigned.apk"
  apktool d -f -o "$WORK/starry_dec" "$STAR_APK" >/dev/null

  echo "[2] baksmali 合并 com/starry → classes2.dex（保留游戏 dex）"
  unzip -qo "$WORK/in.apk" classes2.dex -d "$WORK"
  "$BAKSMALI_BIN" d -o "$WORK/c2smali" "$WORK/classes2.dex"
  rm -rf "$WORK/c2smali/com/starry"
  mkdir -p "$WORK/c2smali/com"
  for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
    [[ -d "$d/com/starry" ]] || continue
    rsync -a "$d/com/starry" "$WORK/c2smali/com/"
  done
  [[ -f "$WORK/c2smali/com/starry/magic/menu/FeatureListParser.smali" ]] \
    || { echo "ERROR: FeatureListParser.smali missing"; exit 1; }
  "$SMALI_BIN" assemble -o "$WORK/classes2.dex" "$WORK/c2smali"
  NEW_SZ=$(wc -c < "$WORK/classes2.dex" | tr -d ' ')
  [[ "$NEW_SZ" -gt 1000000 ]] || { echo "ERROR: merged classes2 too small ($NEW_SZ)"; exit 1; }
  ZIP_ARGS+=(classes2.dex)
else
  echo "[2] zip replace so only（菜单 UI 未改；要默认值请 STARRY_UPDATE_MENU=1）"
fi

(cd "$WORK" && zip -u in.apk "${ZIP_ARGS[@]}")

echo "[3] sign"
java -jar "$SIGNER" -a "$WORK/in.apk" --overwrite --allowResign \
  --ks "$KS" --ksAlias starry --ksPass starry2026 --ksKeyPass starry2026

mkdir -p "$OUT"
if [[ -f "$WORK/in-aligned-releaseSigned.apk" ]]; then
  cp -f "$WORK/in-aligned-releaseSigned.apk" "$SIGNED"
elif [[ -f "$WORK/in-releaseSigned.apk" ]]; then
  cp -f "$WORK/in-releaseSigned.apk" "$SIGNED"
else
  cp -f "$WORK/in.apk" "$SIGNED"
fi
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  echo "[4] install"
  REMOTE="/data/local/tmp/starry_phigros_install.apk"
  adb push "$SIGNED" "$REMOTE"
  adb shell su -c "pm install -r -d --user 0 '$REMOTE'" || adb install -r -d "$SIGNED"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  adb shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1 || true
fi
echo "done → $SIGNED"
