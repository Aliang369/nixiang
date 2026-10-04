#!/usr/bin/env bash
# 雷霆战机 1.25.31：官方原包 zip 注入（禁止 apktool 全量重建）
#
# 背景（2026-08-11）：apktool d→b / 改 dex / 改 lib/*.so 内容 / 往 lib/ 加 so
# 均会触发 ACE SIGBUS。仅允许 zip 改 assets/ + uber 重签。
# 详见：10_docs/雷霆战机/官方星光_卡启动与打包路线_20260811.md
#
# 本脚本：
#   - 从官方 ORIG 复制
#   - 将 libstarry_magic.so + libc++_shared.so 放入 assets/starry_native/
  - 可选：STARRY_WITH_MENU=1（默认）打入 starry.dex + MenuService Manifest
  - uber 重签；可选 root 静默安装
  - **不**改 classes*.dex / lib/arm64-v8a 业务 so
#
# 用法:
#   bash scripts/ltzj_zip_assets_inject.sh              # 只出包
#   STARRY_INSTALL=1 bash scripts/ltzj_zip_assets_inject.sh
#
# 注意：so 在 assets 内不会自动 JNI_OnLoad；需晚加载（开发：phantom-frida 魔改，旧 Florida；交付：delayed shim）。
# so/dex 运行时落点优先：/data/data/<pkg>/files/starry_native/（/data/local/tmp 常 Permission denied）
set -euo pipefail

BUILD_TYPE="${STARRY_BUILD_TYPE:-release}"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
ORIG_APK="${ORIG_APK:-}"
if [[ -z "$ORIG_APK" ]]; then
  for c in \
    "$NIXIANG/02_apks/雷霆战机/雷霆战机_原版.apk" \
    "$NIXIANG/02_apks/雷霆战机/雷霆战机_集结_1.25.31_orig_from_device.apk"; do
    [[ -f "$c" ]] && ORIG_APK="$c" && break
  done
fi
OUT="$NIXIANG/08_output/雷霆战机"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
PKG="com.a5game.ltzj.jj"
SIGNED="$OUT/雷霆战机_1.25.31_starry-zipAssets-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/雷霆战机_1.25.31_starry-zipAssets-debugSigned.apk"
# 带 MenuService Manifest + starry.dex 时改名，便于区分开机-only 包
WITH_MENU="${STARRY_WITH_MENU:-1}"
MF_CACHE="${STARRY_MF:-$NIXIANG/07_patched/雷霆战机/surgical/AndroidManifest.xml}"
if [[ "$WITH_MENU" == "1" ]]; then
  SIGNED="$OUT/雷霆战机_1.25.31_starry-zipAssets-menu-releaseSigned.apk"
  [[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/雷霆战机_1.25.31_starry-zipAssets-menu-debugSigned.apk"
fi

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT_ZIPALIGN=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT_ZIPALIGN="$HOME/Library/Android/sdk/build-tools/$v/zipalign"
    break
  fi
done

[[ -n "$ORIG_APK" && -f "$ORIG_APK" ]] || { echo "ERROR: missing ORIG APK (set ORIG_APK=...)"; exit 1; }
echo "[*] ORIG=$ORIG_APK"
grep -E '^[[:space:]]*LOCAL_CFLAGS.*STARRY_GAME_LTZJ' "$STARRY_SRC/app/src/main/jni/Android.mk" \
  || { echo "ERROR: Android.mk must enable STARRY_GAME_LTZJ (uncommented)"; exit 1; }

WORK="$(mktemp -d /tmp/ltzj_zip_assets.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
mkdir -p "$OUT" "$WORK/starry" "$WORK/assets/starry_native"

echo "[0] rsync starry → ASCII work"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
APP="$WORK/starry/app"

echo "[1] ndk-build arm64"
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
cp -f "$SO" "$WORK/assets/starry_native/libstarry_magic.so"
[[ -f "$NDK_CXX" ]] || { echo "ERROR: missing libc++_shared at $NDK_CXX"; exit 1; }
cp -f "$NDK_CXX" "$WORK/assets/starry_native/libc++_shared.so"
ls -lh "$WORK/assets/starry_native/"

echo "[2] certs from starry src assets（菜单已原生，不再打 assets/web）"
if [[ -d "$APP/src/main/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$APP/src/main/assets/certs/." "$WORK/assets/certs/"
  echo "certs: copied"
else
  echo "certs: none"
fi

# 可选：把已有 starry.dex 打进 assets（改 APK classes*.dex 会杀；assets 可活）
STARRY_DEX_SRC="${STARRY_DEX:-$NIXIANG/08_output/雷霆战机/starry_dex_build/starry.dex}"
if [[ "${STARRY_SKIP_DEX:-0}" != "1" && -f "$STARRY_DEX_SRC" ]]; then
  mkdir -p "$WORK/assets/starry_dex"
  cp -f "$STARRY_DEX_SRC" "$WORK/assets/starry_dex/starry.dex"
  echo "dex: $STARRY_DEX_SRC → assets/starry_dex/starry.dex"
else
  echo "dex: skipped (STARRY_SKIP_DEX=1 or missing $STARRY_DEX_SRC)"
fi

echo "[3] zip inject into ORIG copy (assets + optional Manifest; never dex/lib)"
cp -f "$ORIG_APK" "$WORK/base.apk"
(
  cd "$WORK"
  zip -u -q base.apk \
    assets/starry_native/libstarry_magic.so \
    assets/starry_native/libc++_shared.so
  if [[ -f assets/starry_dex/starry.dex ]]; then
    zip -u -q base.apk assets/starry_dex/starry.dex
  fi
  if [[ -d assets/certs ]]; then
    find assets/certs -type f | while read -r f; do
      zip -u -q base.apk "$f"
    done
  fi
)
if [[ "$WITH_MENU" == "1" ]]; then
  if [[ ! -f "$MF_CACHE" ]]; then
    echo "ERROR: STARRY_WITH_MENU=1 but missing Manifest cache: $MF_CACHE"
    exit 1
  fi
  cp -f "$MF_CACHE" "$WORK/AndroidManifest.xml"
  (
    cd "$WORK"
    zip -u -q base.apk AndroidManifest.xml
  )
  echo "manifest: MenuService from $MF_CACHE"
fi

# 自检：不得出现 lib/arm64 下的 starry
if unzip -l "$WORK/base.apk" | grep -E -q 'lib/arm64.*/libstarry'; then
  echo "ERROR: starry landed under lib/ — abort"
  exit 1
fi
unzip -l "$WORK/base.apk" | grep -E 'assets/starry_native|assets/starry_dex|assets/certs|AndroidManifest.xml' | head -25 || true

echo "[4] uber sign"
UBER_ARGS=(-a "$WORK/base.apk" --overwrite --allowResign)
[[ -n "$BT_ZIPALIGN" ]] && UBER_ARGS+=(--zipAlignPath "$BT_ZIPALIGN")
java -jar "$SIGNER" "${UBER_ARGS[@]}"
SIGNED_SRC=""
for c in \
  "$WORK/base-aligned-debugSigned.apk" \
  "$WORK/base-aligned-releaseSigned.apk" \
  "$WORK/base-debugSigned.apk" \
  "$WORK/base-releaseSigned.apk" \
  "$WORK/base.apk"; do
  if [[ -f "$c" ]]; then SIGNED_SRC="$c"; break; fi
done
cp -f "$SIGNED_SRC" "$SIGNED"
ls -lh "$SIGNED"
echo "done package → $SIGNED"
echo "NOTE: so in assets only; need delayed loader (Bugly shim / late inject) for JNI_OnLoad."

if [[ "${STARRY_INSTALL:-0}" != "1" ]]; then
  echo "skip install (STARRY_INSTALL=1 to install)"
  exit 0
fi

REMOTE="/data/local/tmp/starry_ltzj_zipassets.apk"
adb push "$SIGNED" "$REMOTE"
# 默认覆盖安装，禁止轻易卸载（大资源）；需干净装才设 STARRY_CLEAN_INSTALL=1（须聊天确认）
if [[ "${STARRY_CLEAN_INSTALL:-0}" == "1" ]]; then
  echo "WARN: STARRY_CLEAN_INSTALL=1 → pm uninstall"
  adb shell su -c "pm uninstall '$PKG'" >/dev/null 2>&1 || true
fi
if ! adb shell su -c "pm install -r -d --user 0 '$REMOTE'"; then
  echo "ERROR: root pm install failed（签名不一致时需用户明确同意卸载后再装）"
  exit 1
fi
adb shell su -c "rm -f '$REMOTE'" >/dev/null 2>&1 || true
# 全游戏卡密种子 + 悬浮窗（勿删；换游戏 403 多因此）
_NIXIANG="$(cd "$(dirname "$0")/../../.." && pwd)"
# shellcheck source=/dev/null
source "$_NIXIANG/04_scripts/_common/starry_post_install.sh"
starry_post_install "$PKG"
set +e
adb shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1
set -e
echo "installed → $SIGNED"
echo "expect: AppActivity survive; no StarryMagic JNI until loader wired"
