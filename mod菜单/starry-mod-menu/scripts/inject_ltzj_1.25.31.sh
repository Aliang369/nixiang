#!/usr/bin/env bash
# 星光魔法 → 雷霆战机：集结 1.25.31 官方包：构建 + 注入 + SignSpoof + 签名装机
#
# !!! 已废弃（2026-08-11+）：apktool 重建 + lib/ 塞 so + 新 dex → ACE 必杀 / 卡启动。
# 开机请用：bash scripts/ltzj_zip_assets_inject.sh
# 详见：10_docs/雷霆战机/雷霆战机_加固与校验分析.md §4
#
# 若确需考古跑本脚本：FORCE_DEPRECATED_LTZJ_APKTOOL=1
#
# 用法:
#   bash scripts/inject_ltzj_1.25.31.sh
#   STARRY_BUILD_TYPE=debug bash scripts/inject_ltzj_1.25.31.sh
#   STARRY_SKIP_INSTALL=1 bash scripts/inject_ltzj_1.25.31.sh
set -euo pipefail

if [[ "${FORCE_DEPRECATED_LTZJ_APKTOOL:-0}" != "1" ]]; then
  echo "ERROR: inject_ltzj_1.25.31.sh 已废弃（apktool/lib/dex 会杀进程）。"
  echo "       请改用: bash scripts/ltzj_zip_assets_inject.sh"
  echo "       考古强制: FORCE_DEPRECATED_LTZJ_APKTOOL=1 $0"
  exit 2
fi

BUILD_TYPE="${STARRY_BUILD_TYPE:-release}"
if [[ "$BUILD_TYPE" != "release" && "$BUILD_TYPE" != "debug" ]]; then
  echo "ERROR: STARRY_BUILD_TYPE must be release or debug (got: $BUILD_TYPE)"
  exit 1
fi
if [[ "$BUILD_TYPE" == "release" ]]; then
  GRADLE_TASK="assembleRelease"
  APK_NAME="app-release.apk"
else
  GRADLE_TASK="assembleDebug"
  APK_NAME="app-debug.apk"
fi

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="/Users/aliang/Documents/nixiang"
ORIG_APK="$NIXIANG/02_apks/雷霆战机/雷霆战机_集结_1.25.31_orig_from_device.apk"
SRC_DECODE="$NIXIANG/03_decompiled/雷霆战机/apktool_1.25.31_orig"
FINAL_PATCH="$NIXIANG/07_patched/雷霆战机/apktool_1.25.31_modmenu"
OUT="$NIXIANG/08_output/雷霆战机"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
ORIG_MD5="f0c5671a8918185e5d2823eb6b3d6483"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
BT_ZIPALIGN=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT_ZIPALIGN="$HOME/Library/Android/sdk/build-tools/$v/zipalign"
    break
  fi
done
export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

PKG="com.a5game.ltzj.jj"
SIGNED="$OUT/雷霆战机_1.25.31_starry-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/雷霆战机_1.25.31_starry-debugSigned.apk"

WORK="$(mktemp -d /tmp/ltzj_inject.XXXXXX)"
STARRY_BUILD="$WORK/starry"
PATCH="$WORK/apktool_modmenu"
OUT_ASCII="$WORK/out"
UNSIGNED="$OUT_ASCII/ltzj_starry_unsigned.apk"
trap 'rm -rf "$WORK"' EXIT

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing ${ORIG_APK}"; exit 1; }
[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK"; exit 1; }
MK_SRC="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -q 'STARRY_GAME_LTZJ' "$MK_SRC" || { echo "ERROR: Android.mk missing STARRY_GAME_LTZJ"; exit 1; }
grep -E 'STARRY_GAME_LTZJ' "$MK_SRC" \
  || { echo "ERROR: Android.mk Android.mk must enable STARRY_GAME_LTZJ"; exit 1; }
echo "[check] Android.mk target=ltzj"
mkdir -p "$OUT" "$OUT_ASCII" "$(dirname "$SRC_DECODE")"

if [[ ! -f "$SRC_DECODE/AndroidManifest.xml" ]]; then
  echo "[0a] apktool decode orig → ASCII then move"
  DEC_TMP="$(mktemp -d /tmp/ltzj_dec.XXXXXX)"
  apktool d -f -o "$DEC_TMP/apktool_1.25.31_orig" "$ORIG_APK"
  rm -rf "$SRC_DECODE"
  mv "$DEC_TMP/apktool_1.25.31_orig" "$SRC_DECODE"
  rmdir "$DEC_TMP" 2>/dev/null || true
fi

echo "[0/7] rsync starry → ASCII work + copy decode"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$STARRY_BUILD/"
cp -a "$SRC_DECODE" "$PATCH"

APP="$STARRY_BUILD/app"
echo "[1/7] ndk-build + assemble ($GRADLE_TASK) in $STARRY_BUILD"
"$NDK/ndk-build" -C "$APP/src/main/jni" \
  NDK_PROJECT_PATH="$APP" \
  NDK_APPLICATION_MK="$APP/src/main/jni/Application.mk" \
  APP_BUILD_SCRIPT="$APP/src/main/jni/Android.mk" \
  APP_ABI=arm64-v8a \
  NDK_OUT="$APP/build/intermediates/ndk/obj" \
  NDK_LIBS_OUT="$APP/build/intermediates/ndk/libs" \
  -j8
mkdir -p "$APP/src/main/jniLibs/arm64-v8a"
cp -f "$APP/build/intermediates/ndk/libs/arm64-v8a/"*.so "$APP/src/main/jniLibs/arm64-v8a/"
python3.11 - <<PY
from pathlib import Path
import re
p = Path("$APP/build.gradle")
t = p.read_text()
t2 = re.sub(
    r"\n    externalNativeBuild \{[\s\S]*?\n    \}\n",
    "\n    // externalNativeBuild disabled (ASCII CI): so from jniLibs\n",
    t,
    count=1,
)
p.write_text(t2)
PY
(cd "$STARRY_BUILD" && ./gradlew "$GRADLE_TASK" --console=plain)
APK="$STARRY_BUILD/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$APK" ]] || { echo "ERROR: missing $APK"; exit 1; }
echo "using: $APK"

echo "[2/7] decode starry"
apktool d -f -o "$WORK/starry_dec" "$APK" >/dev/null

echo "[3/7] inject so/assets/smali"
mkdir -p "$PATCH/lib/arm64-v8a"
cp "$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so" "$PATCH/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$PATCH/lib/arm64-v8a/libc++_shared.so"
# 原生菜单：不再拷贝 assets/web；清掉旧包残留 HTML
rm -rf "$PATCH/assets/web" 2>/dev/null || true
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$PATCH/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$PATCH/assets/certs/"
fi
# 新 dex：原包到 smali_classes5，星光用 smali_classes6
rm -rf "$PATCH/smali_classes6/com/starry"
mkdir -p "$PATCH/smali_classes6/com"
[[ -d "$WORK/starry_dec/smali/com/starry" ]] && cp -R "$WORK/starry_dec/smali/com/starry" "$PATCH/smali_classes6/com/"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali_classes*/com/starry; do
  cp -R "$d" "$PATCH/smali_classes6/com/"
done
[[ -d "$PATCH/smali_classes6/com/starry/magic" ]] || { echo "ERROR: starry smali missing"; exit 1; }

echo "[4/7] patch Manifest / CloverApplication / AppActivity / TopOn"
python3.11 "$STARRY_BUILD/scripts/_patch_ltzj_smali.py" "$PATCH" "$ORIG_MD5"

echo "[5/7] (combat = CocosJsBridge + get_ret; ad = TopOn bridge)"
echo "[6/7] apktool rebuild (ASCII) — large APK, wait"
apktool b -f "$PATCH" -o "$UNSIGNED"

echo "[7/7] sign + install"
UBER_ARGS=(-a "$UNSIGNED" --overwrite --allowResign)
[[ -n "$BT_ZIPALIGN" ]] && UBER_ARGS+=(--zipAlignPath "$BT_ZIPALIGN")
java -jar "$SIGNER" "${UBER_ARGS[@]}"
mkdir -p "$OUT"
SIGNED_SRC=""
for c in \
  "${UNSIGNED%.apk}-aligned-debugSigned.apk" \
  "${UNSIGNED%.apk}-aligned-releaseSigned.apk" \
  "${UNSIGNED%.apk}-debugSigned.apk" \
  "${UNSIGNED%.apk}-releaseSigned.apk" \
  "$UNSIGNED"; do
  if [[ -f "$c" ]]; then SIGNED_SRC="$c"; break; fi
done
cp -f "$SIGNED_SRC" "$SIGNED"
rm -rf "$FINAL_PATCH"
mkdir -p "$(dirname "$FINAL_PATCH")"
cp -a "$PATCH" "$FINAL_PATCH"
ls -lh "$SIGNED"

if [[ "${STARRY_SKIP_INSTALL:-0}" == "1" ]]; then
  echo "skip install"
  echo "done → $SIGNED"
  exit 0
fi
if [[ "${STARRY_INSTALL:-1}" != "1" ]]; then
  echo "STARRY_INSTALL!=1 skip install"
  echo "done → $SIGNED"
  exit 0
fi

# 只用 root pm 静默装（禁止 adb install，ColorOS 会弹手点确认）
REMOTE_APK="/data/local/tmp/starry_ltzj_install.apk"
adb push "$SIGNED" "$REMOTE_APK"
adb shell su -c "pm uninstall '$PKG'" >/dev/null 2>&1 || true
if ! adb shell su -c "pm install -r -d --user 0 '$REMOTE_APK'"; then
  echo "ERROR: root pm install failed（勿回退 adb install）"
  exit 1
fi
adb shell su -c "rm -f '$REMOTE_APK'" >/dev/null 2>&1 || true
# 全游戏卡密种子 + 悬浮窗（勿删；换游戏 403 多因此）
_NIXIANG="$(cd "$(dirname "$0")/../../.." && pwd)"
# shellcheck source=/dev/null
source "$_NIXIANG/04_scripts/_common/starry_post_install.sh"
starry_post_install "$PKG"
set +e
adb shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1
set -e
echo "done → $SIGNED (root silent install)"
echo "log: adb logcat -s StarrySignSpoof:V StarryMagic:V Ltzj:V Mod_Menu:V"
