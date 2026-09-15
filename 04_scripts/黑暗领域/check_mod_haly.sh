#!/usr/bin/env bash
# 黑暗领域：核对当前星光包身份、签名、引擎和 mod so 标签。只读本地 APK。
set -euo pipefail

trap 'rc=$?; echo "错误: check_mod_haly.sh 第 ${LINENO} 行失败 rc=${rc}" >&2' ERR

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
META="$SCRIPT_DIR/installed_mod_apk.json"
OUT="$SCRIPT_DIR/mod_check.json"
MANIFEST="$SCRIPT_DIR/mod_manifest.txt"
SIGNATURE="$SCRIPT_DIR/mod_signature.txt"
ENTRIES="$SCRIPT_DIR/mod_entries.txt"
AAPT2="${AAPT2:-/Users/aliang/Library/Android/sdk/build-tools/36.0.0/aapt2}"
APKSIGNER="${APKSIGNER:-/Users/aliang/Library/Android/sdk/build-tools/36.0.0/apksigner}"

[[ -f "$META" ]] || { echo "错误: 缺少 installed_mod_apk.json" >&2; exit 1; }
APK="$(python3 -c 'import json,sys; print(json.load(open(sys.argv[1],encoding="utf-8"))["apk_path"])' "$META")"
[[ -f "$APK" ]] || { echo "错误: APK 不存在: $APK" >&2; exit 1; }
[[ -x "$AAPT2" ]] || { echo "错误: aapt2 不可执行: $AAPT2" >&2; exit 1; }
[[ -x "$APKSIGNER" ]] || { echo "错误: apksigner 不可执行: $APKSIGNER" >&2; exit 1; }

"$AAPT2" dump badging "$APK" > "$MANIFEST"
"$APKSIGNER" verify --print-certs "$APK" > "$SIGNATURE"
unzip -Z1 "$APK" > "$ENTRIES"

SO_TMP="$(mktemp -t haly_so.XXXXXX)"
trap 'rm -f "$SO_TMP"' EXIT
unzip -p "$APK" lib/arm64-v8a/libstarry_magic.so > "$SO_TMP"
grep -a -q '\[Haly\]' "$SO_TMP" || { echo "错误: 包内 so 未找到 [Haly] 标签" >&2; exit 1; }

PACKAGE="$(sed -n "s/^package: name='\([^']*\)'.*/\1/p" "$MANIFEST" | head -1)"
VERSION_NAME="$(sed -n "s/^package: .* versionName='\([^']*\)'.*/\1/p" "$MANIFEST" | head -1)"
VERSION_CODE="$(sed -n "s/^package: .* versionCode='\([^']*\)'.*/\1/p" "$MANIFEST" | head -1)"
SIGNER_DN="$(sed -n 's/^Signer #1 certificate DN: //p' "$SIGNATURE" | head -1)"
SIGNER_SHA256="$(sed -n 's/^Signer #1 certificate SHA-256 digest: //p' "$SIGNATURE" | head -1)"

python3 - "$OUT" "$APK" "$MANIFEST" "$SIGNATURE" "$ENTRIES" "$PACKAGE" "$VERSION_NAME" "$VERSION_CODE" "$SIGNER_DN" "$SIGNER_SHA256" <<'PY'
import hashlib
import json
import sys
from pathlib import Path

out, apk, manifest_path, signature_path, entries_path, package, version_name, version_code, signer_dn, signer_sha256 = sys.argv[1:]
entries = [x.strip() for x in Path(entries_path).read_text(encoding="utf-8", errors="replace").splitlines() if x.strip()]
manifest = Path(manifest_path).read_text(encoding="utf-8", errors="replace")
signature = Path(signature_path).read_text(encoding="utf-8", errors="replace")
data = {
    "game": "黑暗领域",
    "apk_path": apk,
    "apk_md5": hashlib.md5(Path(apk).read_bytes()).hexdigest(),
    "apk_sha256": hashlib.sha256(Path(apk).read_bytes()).hexdigest(),
    "apk_size": Path(apk).stat().st_size,
    "package": package,
    "version_name": version_name,
    "version_code": version_code,
    "engine": "cocos",
    "cocos_native": [x for x in entries if x.startswith("lib/arm64-v8a/libcocos")],
    "starry_so_present": "lib/arm64-v8a/libstarry_magic.so" in entries,
    "starry_haly_tag": True,
    "signer_dn": signer_dn,
    "signer_sha256": signer_sha256,
    "manifest_badging": manifest.splitlines()[:70],
    "signature_verify": signature.splitlines()[:50],
}
Path(out).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

cat "$OUT"
