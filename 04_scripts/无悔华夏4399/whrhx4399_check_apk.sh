#!/usr/bin/env bash
# 无悔华夏 4399 3.8.280：核对从真机提取的官方包身份与关键组件。
set -eu

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
META_FILE="$SCRIPT_DIR/fetched_apk.json"
OUT_JSON="$SCRIPT_DIR/check_summary.json"
AAPT2="${AAPT2:-$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2}"
APKSIGNER="${APKSIGNER:-$HOME/Library/Android/sdk/build-tools/36.0.0/apksigner}"

[[ -f "$META_FILE" ]] || { echo "错误: 缺少 $(basename "$META_FILE")" >&2; exit 1; }
[[ -x "$AAPT2" ]] || { echo "错误: aapt2 不可用: $AAPT2" >&2; exit 1; }
[[ -x "$APKSIGNER" ]] || { echo "错误: apksigner 不可用: $APKSIGNER" >&2; exit 1; }

APK="$(python3 - "$META_FILE" <<'PY'
import json, sys
print(json.load(open(sys.argv[1], encoding="utf-8"))["apk_path"])
PY
)"
[[ -f "$APK" ]] || { echo "错误: APK 不存在: $APK" >&2; exit 1; }

ACTUAL_MD5="$(md5 -q "$APK")"
EXPECTED_MD5="$(python3 - "$META_FILE" <<'PY'
import json, sys
print(json.load(open(sys.argv[1], encoding="utf-8"))["md5"])
PY
)"
[[ "$ACTUAL_MD5" = "$EXPECTED_MD5" ]] || {
  echo "错误: APK MD5 与提取记录不一致: $ACTUAL_MD5 != $EXPECTED_MD5" >&2
  exit 1
}

BADGING="$("$AAPT2" dump badging "$APK")"
PKG="$(printf '%s\n' "$BADGING" | sed -n "s/^package: name='\([^']*\)'.*/\1/p" | head -1)"
VERSION_NAME="$(printf '%s\n' "$BADGING" | sed -n "s/^package:.* versionName='\([^']*\)'.*/\1/p" | head -1)"
VERSION_CODE="$(printf '%s\n' "$BADGING" | sed -n "s/^package:.* versionCode='\([^']*\)'.*/\1/p" | head -1)"
[[ "$PKG" = "com.yhhy.whrhx.m4399" ]] || { echo "错误: 包名不符: $PKG" >&2; exit 1; }
[[ "$VERSION_NAME" = "3.8.280" && "$VERSION_CODE" = "33" ]] || {
  echo "错误: 版本不符: $VERSION_NAME / $VERSION_CODE" >&2
  exit 1
}

CERT_TEXT="$("$APKSIGNER" verify --verbose --print-certs "$APK")"
CERT_DN="$(printf '%s\n' "$CERT_TEXT" | sed -n 's/^Signer #1 certificate DN: //p' | head -1)"
CERT_SHA256="$(printf '%s\n' "$CERT_TEXT" | sed -n 's/^Signer #1 certificate SHA-256 digest: //p' | head -1)"
CERT_SHA1="$(printf '%s\n' "$CERT_TEXT" | sed -n 's/^Signer #1 certificate SHA-1 digest: //p' | head -1)"
V1="$(printf '%s\n' "$CERT_TEXT" | sed -n 's/^Verified using v1 scheme (JAR signing): //p' | head -1)"
V2="$(printf '%s\n' "$CERT_TEXT" | sed -n 's/^Verified using v2 scheme (APK Signature Scheme v2): //p' | head -1)"

file_sha256() {
  local entry="$1"
  unzip -p "$APK" "$entry" 2>/dev/null | shasum -a 256 | awk '{print $1}'
}

file_size() {
  local entry="$1"
  unzip -l "$APK" "$entry" 2>/dev/null | awk -v name="$entry" '$4 == name {print $1; exit}'
}

LIBIL2CPP_SIZE="$(file_size 'lib/arm64-v8a/libil2cpp.so')"
LIBIL2CPP_SHA256="$(file_sha256 'lib/arm64-v8a/libil2cpp.so')"
METADATA_SIZE="$(file_size 'assets/bin/Data/Managed/Metadata/global-metadata.dat')"
METADATA_SHA256="$(file_sha256 'assets/bin/Data/Managed/Metadata/global-metadata.dat')"
LIBUNITY_SHA256="$(file_sha256 'lib/arm64-v8a/libunity.so')"
LIBXLUA_SHA256="$(file_sha256 'lib/arm64-v8a/libxlua.so')"
LIBNESEC_SHA256="$(file_sha256 'lib/arm64-v8a/libnesec.so')"
NETHT_SHA256="$(file_sha256 'lib/arm64-v8a/libNetHTProtect.so')"
METADATA_MAGIC="$(unzip -p "$APK" 'assets/bin/Data/Managed/Metadata/global-metadata.dat' | xxd -p -l 4)"

ENTRY_LIST="$(unzip -Z1 "$APK")"
HAS_BAIDU="$(printf '%s\n' "$ENTRY_LIST" | grep -c 'libbaiduprotect\.so' || true)"
HAS_BINGE="$(printf '%s\n' "$ENTRY_LIST" | grep -Ec 'libSDKloading\.so|libtoolChecker\.so' || true)"
DEX_COUNT="$(printf '%s\n' "$ENTRY_LIST" | grep -Ec '^classes[0-9]*\.dex$' || true)"

python3 - "$OUT_JSON" "$APK" "$ACTUAL_MD5" "$PKG" "$VERSION_NAME" "$VERSION_CODE" \
  "$CERT_DN" "$CERT_SHA256" "$CERT_SHA1" "$V1" "$V2" "$LIBIL2CPP_SIZE" \
  "$LIBIL2CPP_SHA256" "$METADATA_SIZE" "$METADATA_SHA256" "$METADATA_MAGIC" \
  "$LIBUNITY_SHA256" "$LIBXLUA_SHA256" "$LIBNESEC_SHA256" "$NETHT_SHA256" \
  "$HAS_BAIDU" "$HAS_BINGE" "$DEX_COUNT" <<'PY'
import json
import sys
from pathlib import Path

(
    out,
    apk,
    md5,
    package,
    version_name,
    version_code,
    cert_dn,
    cert_sha256,
    cert_sha1,
    v1,
    v2,
    il2cpp_size,
    il2cpp_sha256,
    metadata_size,
    metadata_sha256,
    metadata_magic,
    unity_sha256,
    xlua_sha256,
    nesec_sha256,
    netht_sha256,
    has_baidu,
    has_binge,
    dex_count,
) = sys.argv[1:]
data = {
    "apk": apk,
    "md5": md5,
    "package": package,
    "version_name": version_name,
    "version_code": int(version_code),
    "signer_dn": cert_dn,
    "signer_sha256": cert_sha256,
    "signer_sha1": cert_sha1,
    "signature_v1": v1 == "true",
    "signature_v2": v2 == "true",
    "dex_count": int(dex_count),
    "libil2cpp": {
        "size": int(il2cpp_size),
        "sha256": il2cpp_sha256,
    },
    "global_metadata": {
        "size": int(metadata_size),
        "sha256": metadata_sha256,
        "magic_hex": metadata_magic,
    },
    "libunity_sha256": unity_sha256,
    "libxlua_sha256": xlua_sha256,
    "libnesec_sha256": nesec_sha256,
    "libNetHTProtect_sha256": netht_sha256,
    "has_baiduprotect": int(has_baidu) > 0,
    "has_binge_shell": int(has_binge) > 0,
}
Path(out).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

cat "$OUT_JSON"
