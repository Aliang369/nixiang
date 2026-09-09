#!/usr/bin/env bash
# 王国大作战：前线 — 外科包装机后必跑（需 root）
#
# kadp 验签绑定安装路径上的 base.apk；open 重定向不够。
# 保留 surgical 的 lib/*.so，把 base.apk 替换成官方 TapTap 包，并推送 classes5 侧载 dex。
#
# 用法:
#   STARRY_INSTALL=1 bash scripts/surgical_wgdzz_1.1.7646.sh
#   bash scripts/wgdzz_post_install_swap.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../../.." && pwd)"
OFFICIAL="${OFFICIAL_APK:-$NIXIANG/02_apks/王国大作战前线/base_1.1.7646.apk}"
PKG="com.xr.wgdzztap"
TMP_DEX="/data/local/tmp/wgdzz_classes5.dex"

SURGICAL="${WGDZZ_SURGICAL_APK:-}"
if [[ -z "$SURGICAL" ]]; then
  OUT="$NIXIANG/08_output/王国大作战前线"
  SURGICAL=$(ls -t "$OUT"/*surgical*Signed*.apk 2>/dev/null | head -1 || true)
fi

[[ -f "$OFFICIAL" ]] || { echo "ERROR: missing official APK $OFFICIAL"; exit 1; }
[[ -n "$SURGICAL" && -f "$SURGICAL" ]] || {
  echo "ERROR: set WGDZZ_SURGICAL_APK or build surgical first"
  exit 1
}

if ! adb shell "su -c 'id'" >/dev/null 2>&1; then
  echo "ERROR: need adb root (su) for base.apk swap"
  exit 1
fi

echo "[1/4] push classes5.dex from $SURGICAL"
unzip -p "$SURGICAL" classes5.dex > /tmp/wgdzz_classes5.dex
adb push /tmp/wgdzz_classes5.dex "$TMP_DEX"
adb shell "su -c 'chmod 644 $TMP_DEX'"

STARRY_SO="${WGDZZ_STARRY_SO:-}"
if [[ -z "$STARRY_SO" || ! -f "$STARRY_SO" ]]; then
  STARRY_SO=$(unzip -l "$SURGICAL" 2>/dev/null | awk '/libstarry_magic\.so/{print $4; exit}')
  [[ -n "$STARRY_SO" ]] || STARRY_SO="lib/arm64-v8a/libstarry_magic.so"
  unzip -p "$SURGICAL" "$STARRY_SO" > /tmp/wgdzz_libstarry_magic.so
else
  cp "$STARRY_SO" /tmp/wgdzz_libstarry_magic.so
  echo "  using WGDZZ_STARRY_SO=$STARRY_SO"
fi
adb push /tmp/wgdzz_libstarry_magic.so /data/local/tmp/wgdzz_libstarry_magic.so
adb push /tmp/wgdzz_libstarry_magic.so /data/local/tmp/wgdzz_libstarry_magic_install.so
adb shell "su -c 'chmod 644 /data/local/tmp/wgdzz_libstarry_magic.so /data/local/tmp/wgdzz_libstarry_magic_install.so'"

echo "[3/5] push official base.apk + root swap (PM cache 仍可能含外科 manifest，可忽略 AppComponentFact CNFE)"
adb push "$OFFICIAL" /data/local/tmp/wgdzz_official_swap.apk

BASE=$(adb shell pm path "$PKG" 2>/dev/null | head -1 | cut -d: -f2 | tr -d '\r')
[[ -n "$BASE" ]] || { echo "ERROR: $PKG not installed"; exit 1; }
echo "  installed base: $BASE"

KWP_DL="${WGDZZ_KWPROTECT_SO:-$NIXIANG/07_patched/王国大作战前线/libKwProtectSDK_sigkill_k1_dlopen.so}"
if [[ ! -f "$KWP_DL" && -f "$NIXIANG/07_patched/王国大作战前线/libKwProtectSDK_sigkill_k1.so" ]]; then
  python3.11 "$SCRIPT_DIR/kwprotect_k1_starry_dlopen_patch.py" \
    "$NIXIANG/07_patched/王国大作战前线/libKwProtectSDK_sigkill_k1.so" "$KWP_DL" 2>/dev/null || true
fi

echo "[4/5] swap base.apk + overlay libstarry_magic.so (+ KwProtect dlopen)"
adb shell "su -c '
  set -e
  cp /data/local/tmp/wgdzz_official_swap.apk \"$BASE\"
  chown root:root \"$BASE\"
  chmod 644 \"$BASE\"
  LIBDIR=\"${BASE%base.apk}/lib/arm64\"
  mkdir -p \"\$LIBDIR\"
  cp /data/local/tmp/wgdzz_libstarry_magic_install.so \"\$LIBDIR/libstarry_magic.so\"
  chmod 755 \"\$LIBDIR/libstarry_magic.so\"
  ls -la \"$BASE\" \"\$LIBDIR/libstarry_magic.so\"
'"
if [[ -f "$KWP_DL" ]]; then
  adb push "$KWP_DL" /data/local/tmp/wgdzz_kwp_dlopen.so
  BASE=$(adb shell pm path "$PKG" 2>/dev/null | head -1 | cut -d: -f2 | tr -d '\r')
  adb shell "su -c 'LIBDIR=\"${BASE%base.apk}/lib/arm64\"; cp /data/local/tmp/wgdzz_kwp_dlopen.so \"\$LIBDIR/libKwProtectSDK.so\"; chmod 755 \"\$LIBDIR/libKwProtectSDK.so\"'"
  echo "  KwProtect dlopen: $KWP_DL"
fi

echo "[verify]"
adb shell "su -c 'ls -la $TMP_DEX; unzip -l \"$BASE\" 2>/dev/null | head -3 || true'"
echo "DONE: swap complete — cold start game; expect WgdzzDex + StarryMagic JNI_OnLoad"
echo "log: adb logcat -c && adb logcat -s WgdzzDex StarryMagic:V StarryTrace MenuService:V"
