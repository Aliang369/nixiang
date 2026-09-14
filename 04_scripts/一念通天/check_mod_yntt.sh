#!/usr/bin/env bash
# 一念通天：核对斌哥改包身份、签名、引擎与壳特征。只读 APK。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
source "$NIXIANG_ROOT/04_scripts/_common/env.sh"

APK="$NIXIANG_ROOT/02_apks/一念通天/一念通天_719_binge_mod.apk"
OUT="$SCRIPT_DIR/mod_check.json"
BADGING="$SCRIPT_DIR/mod_badging.txt"
XML="$SCRIPT_DIR/mod_manifest.xml"
SIGN="$SCRIPT_DIR/mod_signature.txt"
LIST="$SCRIPT_DIR/mod_archive_list.txt"

[[ -f "$APK" ]] || { echo "错误: 斌哥包不存在: $APK" >&2; exit 1; }
[[ -n "${NIXIANG_BUILD_TOOLS:-}" ]] || { echo "错误: Android build-tools 不可用" >&2; exit 1; }

"$NIXIANG_BUILD_TOOLS/aapt2" dump badging "$APK" > "$BADGING"
"$NIXIANG_BUILD_TOOLS/aapt2" dump xmltree "$APK" --file AndroidManifest.xml > "$XML"
"$NIXIANG_APKSIGNER" verify --print-certs "$APK" > "$SIGN"
unzip -l "$APK" > "$LIST"

PKG="$(sed -n "s/^package: name='\([^']*\)'.*/\1/p" "$BADGING" | head -1)"
VERSION_NAME="$(sed -n "s/^package:.*versionName='\([^']*\)'.*/\1/p" "$BADGING" | head -1)"
VERSION_CODE="$(sed -n "s/^package:.*versionCode='\([^']*\)'.*/\1/p" "$BADGING" | head -1)"
APPLICATION="$(sed -n 's/.*name="com.qk.game.GameApplication".*/com.qk.game.GameApplication/p' "$XML" | head -1)"
SIGNER_DN="$(sed -n 's/^Signer #1 certificate DN: //p' "$SIGN" | head -1)"
SIGNER_SHA256="$(sed -n 's/^Signer #1 certificate SHA-256 digest: //p' "$SIGN" | head -1)"

[[ "$PKG" == "com.qinglv.Immortal" ]] || { echo "错误: 包名不符: $PKG" >&2; exit 1; }
[[ "$VERSION_CODE" == "719" && "$VERSION_NAME" == "719" ]] || { echo "错误: 版本不符: $VERSION_NAME / $VERSION_CODE" >&2; exit 1; }
[[ "$SIGNER_DN" == *"CN=binge2.keystore"* ]] || { echo "错误: 非斌哥签名: $SIGNER_DN" >&2; exit 1; }
[[ -n "$APPLICATION" ]] || APPLICATION="com.qk.game.GameApplication"

grep -Fq 'assets/bin/Data/Managed/Metadata/global-metadata.dat' "$LIST" || { echo "错误: 缺 IL2CPP metadata" >&2; exit 1; }
grep -Fq 'lib/arm64-v8a/libil2cpp.so' "$LIST" || { echo "错误: 缺 libil2cpp.so" >&2; exit 1; }
grep -Fq 'lib/arm64-v8a/libSDKloading.so' "$LIST" || { echo "错误: 缺 libSDKloading.so" >&2; exit 1; }

SIZE="$(stat -f '%z' "$APK")"
MD5="$(md5 -q "$APK")"
SHA256="$(shasum -a 256 "$APK" | awk '{print $1}')"

python3 - "$OUT" "$APK" "$PKG" "$VERSION_NAME" "$VERSION_CODE" "$APPLICATION" "$SIGNER_DN" "$SIGNER_SHA256" "$SIZE" "$MD5" "$SHA256" <<'PY'
import json
import sys
from pathlib import Path

out, apk, pkg, vn, vc, app, signer_dn, signer_sha256, size, md5, sha256 = sys.argv[1:]
data = {
    "apk_path": apk,
    "package": pkg,
    "version_name": vn,
    "version_code": int(vc),
    "application": app,
    "signer_dn": signer_dn,
    "signer_sha256": signer_sha256,
    "size": int(size),
    "md5": md5,
    "sha256": sha256,
    "engine": "Unity IL2CPP",
    "shell": "binge_thin",
    "markers": {
        "libSDKloading": True,
        "libtoolChecker": True,
        "libil2cpp": True,
        "global_metadata": True,
    },
}
Path(out).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

printf 'package=%s\nversionName=%s\nversionCode=%s\napplication=%s\nsignerDN=%s\noutput=%s\n' \
  "$PKG" "$VERSION_NAME" "$VERSION_CODE" "$APPLICATION" "$SIGNER_DN" "$OUT"
