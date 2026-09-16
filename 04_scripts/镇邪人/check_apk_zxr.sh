#!/usr/bin/env bash
# 镇邪人星光底包身份与签名核对。
set -euo pipefail

ROOT="/Users/aliang/Documents/nixiang"
APK="${1:-$ROOT/02_apks/镇邪人/镇邪人_1.0.3_starry_from_downloads_20260916.apk}"
AAPT2="${AAPT2:-/Users/aliang/Library/Android/sdk/build-tools/36.0.0/aapt2}"
APKSIGNER="${APKSIGNER:-/Users/aliang/Library/Android/sdk/build-tools/36.0.0/apksigner}"
EXPECT_SHA="ec124c8e7a7d54af5e48ad636bd9d6b2d10973e2dfde653b883da4b3f8c4d7f6"

[[ -f "$APK" ]] || { echo "ERROR: APK missing: $APK" >&2; exit 1; }
ACTUAL_SHA="$(shasum -a 256 "$APK" | awk '{print $1}')"
[[ "$ACTUAL_SHA" == "$EXPECT_SHA" ]] || { echo "ERROR: SHA mismatch: $ACTUAL_SHA" >&2; exit 1; }
BADGING="$("$AAPT2" dump badging "$APK")"
printf '%s\n' "$BADGING" | grep -Fq "package: name='com.zxr.dlxmz.zxrwz37zgx10.gg9b' versionCode='11' versionName='1.0.3'"
"$APKSIGNER" verify "$APK" >/dev/null
echo "base_sha256=$ACTUAL_SHA"
echo "package=com.zxr.dlxmz.zxrwz37zgx10.gg9b"
echo "version=1.0.3/11"
echo "signature=verified"
