#!/usr/bin/env bash
# Fetch the currently installed official Misty Continent 15.0.0 APK from the
# authorized test device into the local analysis staging directory.
set -euo pipefail

SERIAL="${SERIAL:-f9a116a2}"
PKG="com.hortor.mwdl"
NIXIANG="/Users/aliang/Documents/nixiang"
OUT_DIR="$NIXIANG/06_dumped/迷雾大陆"
OUT="$OUT_DIR/迷雾大陆_15.0.0_official.apk"
META="$OUT_DIR/device_package_15.0.0.txt"
CERT="$OUT_DIR/official_15.0.0_cert.txt"

mkdir -p "$OUT_DIR"

version_name="$(adb -s "$SERIAL" shell dumpsys package "$PKG" | sed -n 's/^[[:space:]]*versionName=//p' | head -1 | tr -d '\r')"
version_code="$(adb -s "$SERIAL" shell dumpsys package "$PKG" | sed -n 's/^[[:space:]]*versionCode=//p' | head -1 | awk '{print $1}' | tr -d '\r')"
[[ "$version_name" == "15.0.0" ]] || { echo "ERROR: device versionName=$version_name"; exit 1; }
[[ "$version_code" == "20" ]] || { echo "ERROR: device versionCode=$version_code"; exit 1; }

remote_paths="$(adb -s "$SERIAL" shell pm path "$PKG" | tr -d '\r' | sed -n 's/^package://p')"
remote_count="$(printf '%s\n' "$remote_paths" | sed '/^$/d' | wc -l | tr -d ' ')"
[[ "$remote_count" == "1" ]] || { echo "ERROR: expected one APK path, got $remote_count"; exit 1; }
remote_apk="$remote_paths"
[[ "$remote_apk" == *.apk ]] || { echo "ERROR: unexpected remote path: $remote_apk"; exit 1; }

tmp="$OUT.tmp"
rm -f "$tmp"
adb -s "$SERIAL" pull "$remote_apk" "$tmp"
mv -f "$tmp" "$OUT"

if unzip -Z1 "$OUT" | grep -qx 'lib/arm64-v8a/libstarry_magic.so'; then
  echo "ERROR: installed base.apk already contains libstarry_magic.so"
  exit 1
fi

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/aapt" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"
    break
  fi
done

if [[ -n "$BT" ]]; then
  "$BT/aapt" dump badging "$OUT" | sed -n '1,8p'
  "$BT/apksigner" verify --print-certs "$OUT" > "$CERT"
fi

{
  echo "serial=$SERIAL"
  echo "package=$PKG"
  echo "versionName=$version_name"
  echo "versionCode=$version_code"
  echo "remote_apk=$remote_apk"
  echo "local_apk=$OUT"
  echo "size=$(stat -f '%z' "$OUT")"
  echo "md5=$(md5 -q "$OUT")"
  echo "sha256=$(shasum -a 256 "$OUT" | awk '{print $1}')"
} | tee "$META"

echo "DONE: $OUT"
