#!/usr/bin/env bash
# 动物大联盟 1.0.23 - official control run installer.
# Temporarily installs the untouched TapTap APK for a same-device baseline.
set -euo pipefail

MODE="${1:-install}"
case "$MODE" in
  install|verify) ;;
  *) echo "ERROR: mode must be install or verify" >&2; exit 1 ;;
esac

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
ORIG_APK="${DWDLM_BASE_APK:-$NIXIANG/02_apks/动物大联盟/动物大联盟_1.0.23_official.apk}"
ORIG_MD5="${DWDLM_BASE_MD5:-211cfebb2d273360e22ea3593fdaedd6}"
PKG="com.fightingstars.games.animal"
ADB_BIN="${ADB_BIN:-adb}"
SERIAL_FILE="$SCRIPT_DIR/.device_serial"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing official APK $ORIG_APK" >&2; exit 1; }
[[ "$(md5 -q "$ORIG_APK")" == "$ORIG_MD5" ]] || {
  echo "ERROR: official APK fingerprint mismatch" >&2
  exit 1
}
[[ -f "$SERIAL_FILE" ]] || { echo "ERROR: missing $SERIAL_FILE" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$SERIAL_FILE")"
[[ -n "$SERIAL" ]] || { echo "ERROR: empty device serial" >&2; exit 1; }

if [[ "$MODE" == "verify" ]]; then
  bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$ORIG_APK" --su
  exit 0
fi

if "$ADB_BIN" -s "$SERIAL" shell pm path "$PKG" >/dev/null 2>&1; then
  echo "[official-control] uninstall current package for signature-compatible control baseline"
  "$ADB_BIN" -s "$SERIAL" shell su -c "pm uninstall --user 0 $PKG" >/dev/null
fi

"$ADB_BIN" -s "$SERIAL" install -r "$ORIG_APK"
"$ADB_BIN" -s "$SERIAL" shell su -c "pm clear $PKG" >/dev/null
bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$ORIG_APK" --su
echo "[official-control] installed and cleared $PKG"
