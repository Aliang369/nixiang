#!/usr/bin/env bash
# 镇邪人：从最终正式 APK 提取后台 OTA 使用的 libstarry_magic.so。
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
APK="${ZXR_OTA_SOURCE_APK:-$ROOT/08_output/镇邪人/镇邪人_1.0.3_e4278_star-releaseSigned.apk}"
OUT_DIR="$ROOT/08_output/镇邪人/游戏内"
OUT="$OUT_DIR/libstarry_magic.so"
EXPECTED_APK_SHA256="${ZXR_OTA_APK_SHA256:-83d49ea88ac7895cf2c065d0b895c70e5b480591d1c0977518442efb82886c7d}"
EXPECTED_SO_SHA256="${ZXR_OTA_SO_SHA256:-79bfb98cf31a33926a5b3ba1a5d581bcfbd68a6cfad2ee96ffca7ebdf6f0347b}"

[[ -f "$APK" ]] || { echo "ERROR: APK missing: $APK" >&2; exit 1; }
ACTUAL_APK_SHA256="$(shasum -a 256 "$APK" | awk '{print $1}')"
[[ "$ACTUAL_APK_SHA256" == "$EXPECTED_APK_SHA256" ]] || {
  echo "ERROR: APK SHA-256 mismatch: $ACTUAL_APK_SHA256" >&2
  exit 1
}
if [[ -e "$OUT" ]]; then
  EXISTING_SO_SHA256="$(shasum -a 256 "$OUT" | awk '{print $1}')"
  [[ "$EXISTING_SO_SHA256" == "$EXPECTED_SO_SHA256" ]] || {
    echo "ERROR: existing OTA so mismatch, refuse overwrite: $OUT" >&2
    exit 1
  }
fi

mkdir -p "$OUT_DIR"
unzip -p "$APK" lib/arm64-v8a/libstarry_magic.so > "$OUT"
[[ -s "$OUT" ]] || { echo "ERROR: libstarry_magic.so missing in APK" >&2; exit 1; }

SO_SHA256="$(shasum -a 256 "$OUT" | awk '{print $1}')"
[[ "$SO_SHA256" == "$EXPECTED_SO_SHA256" ]] || {
  echo "ERROR: OTA so SHA-256 mismatch: $SO_SHA256" >&2
  exit 1
}
LC_ALL=C grep -aFq '[Zxr]' "$OUT" || { echo "ERROR: OTA so missing [Zxr]" >&2; exit 1; }
LC_ALL=C grep -aFq 'zxr' "$OUT" || { echo "ERROR: OTA so missing zxr" >&2; exit 1; }
LC_ALL=C grep -aFq 'BEGIN CERTIFICATE' "$OUT" || { echo "ERROR: OTA so missing certificate" >&2; exit 1; }

printf 'ota_so=%s\nsize=%s\nmd5=%s\nsha256=%s\n' \
  "$OUT" "$(stat -f '%z' "$OUT")" "$(md5 -q "$OUT")" "$SO_SHA256"
