#!/usr/bin/env bash
# 偶像梦幻祭2：核对真机官方包身份、签名、关键 IL2CPP 输入与 ZIP 结构。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
META_FILE="$SCRIPT_DIR/official_apk.json"
OUT_FILE="$SCRIPT_DIR/official_check.json"
PKG="com.happyelements.es2"
HISTORY_SIGNATURE_TOKEN="16a924db"
ADB_BIN="${ADB_BIN:-adb}"

[[ -f "$META_FILE" ]] || { echo "错误: 缺少 $META_FILE" >&2; exit 1; }
while IFS=$'\t' read -r key value; do
  case "$key" in
    apk_path) APK="$value" ;;
    version_name) VERSION_NAME="$value" ;;
    version_code) VERSION_CODE="$value" ;;
    md5) APK_MD5="$value" ;;
    sha256) APK_SHA256="$value" ;;
    serial) SERIAL="$value" ;;
  esac
done < <(python3 - "$META_FILE" <<'PY'
import json, sys
d = json.load(open(sys.argv[1], encoding='utf-8'))
for k in ('apk_path', 'version_name', 'version_code', 'md5', 'sha256', 'serial'):
    print(f'{k}\t{d[k]}')
PY
)
[[ -f "$APK" ]] || { echo "错误: APK 不存在: $APK" >&2; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/aapt2" ]]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
[[ -n "$BT" ]] || { echo "错误: 找不到 Android build-tools" >&2; exit 1; }

BADGING="$("$BT/aapt2" dump badging "$APK")"
PACKAGE_LINE="$(printf '%s\n' "$BADGING" | sed -n '1p')"
printf '%s\n' "$PACKAGE_LINE" | grep -Fq "name='$PKG'" || { echo "错误: 包名不匹配: $PACKAGE_LINE" >&2; exit 1; }
APK_VERSION_NAME="$(printf '%s\n' "$PACKAGE_LINE" | sed -n "s/.*versionName='\([^']*\)'.*/\1/p")"
APK_VERSION_CODE="$(printf '%s\n' "$PACKAGE_LINE" | sed -n "s/.*versionCode='\([^']*\)'.*/\1/p")"
[[ "$APK_VERSION_NAME" == "$VERSION_NAME" ]] || { echo "错误: versionName 不一致" >&2; exit 1; }
[[ "$APK_VERSION_CODE" == "$VERSION_CODE" ]] || { echo "错误: versionCode 不一致" >&2; exit 1; }

CERT_OUT="$("$BT/apksigner" verify --print-certs "$APK" 2>&1)"
CERT_MD5="$(printf '%s\n' "$CERT_OUT" | sed -n 's/.*certificate MD5 digest: //p' | head -1 | tr 'A-F' 'a-f')"
CERT_SHA256="$(printf '%s\n' "$CERT_OUT" | sed -n 's/.*certificate SHA-256 digest: //p' | head -1 | tr 'A-F' 'a-f')"
[[ -n "$CERT_MD5" ]] || { echo "错误: 无法读取证书 MD5" >&2; exit 1; }

# 先完整读取，避免 awk 提前退出导致 adb 收到 SIGPIPE（pipefail 会误杀）。
PKG_DUMP="$("$ADB_BIN" -s "$SERIAL" shell dumpsys package "$PKG" | tr -d '\r')"
DEVICE_SIGNATURE_TOKEN="$(LC_ALL=C awk 'match($0, /signatures:\[[^]]*\]/) { print substr($0, RSTART+12, RLENGTH-13); exit }' <<< "$PKG_DUMP")"
[[ -n "$DEVICE_SIGNATURE_TOKEN" ]] || { echo "错误: 无法读取设备签名 token" >&2; exit 1; }

unzip -Z1 "$APK" > "$SCRIPT_DIR/apk_entries.txt"
if grep -qx 'lib/arm64-v8a/libstarry_magic.so' "$SCRIPT_DIR/apk_entries.txt"; then
  echo "错误: 输入包已含 libstarry_magic.so，样本身份不干净" >&2
  exit 1
fi

DEX_COUNT="$(grep -Ec '^classes([0-9]*)\.dex$' "$SCRIPT_DIR/apk_entries.txt" | tr -d ' ')"
DEX_LIST="$(grep -E '^classes([0-9]*)\.dex$' "$SCRIPT_DIR/apk_entries.txt" | sort -V | paste -sd, -)"
SO_COUNT="$(grep -Ec '^lib/arm64-v8a/.*\.so$' "$SCRIPT_DIR/apk_entries.txt" | tr -d ' ')"
RES_METHOD="$(zipinfo -v "$APK" 'resources.arsc' | awk -F': ' '/compression method:/{if (!found) {print $2; found=1}}')"
SO_MD5="$(unzip -p "$APK" lib/arm64-v8a/libil2cpp.so | md5 -q)"
SO_SIZE="$(unzip -l "$APK" lib/arm64-v8a/libil2cpp.so | awk 'NR==4{print $1}')"
META_MD5="$(unzip -p "$APK" assets/bin/Data/Managed/Metadata/global-metadata.dat | md5 -q)"
META_SIZE="$(unzip -l "$APK" assets/bin/Data/Managed/Metadata/global-metadata.dat | awk 'NR==4{print $1}')"
[[ -n "$SO_MD5" && -n "$META_MD5" ]] || { echo "错误: 缺少 libil2cpp.so 或 global-metadata.dat" >&2; exit 1; }
SIGNATURE_TOKEN_MATCH="false"
[[ "$DEVICE_SIGNATURE_TOKEN" == "$HISTORY_SIGNATURE_TOKEN" ]] && SIGNATURE_TOKEN_MATCH="true"

python3 - "$OUT_FILE" "$APK" "$SERIAL" "$VERSION_NAME" "$VERSION_CODE" "$APK_MD5" "$APK_SHA256" \
  "$CERT_MD5" "$CERT_SHA256" "$SO_MD5" "$SO_SIZE" "$META_MD5" "$META_SIZE" \
  "$DEX_COUNT" "$DEX_LIST" "$SO_COUNT" "$RES_METHOD" \
  "$DEVICE_SIGNATURE_TOKEN" "$HISTORY_SIGNATURE_TOKEN" "$SIGNATURE_TOKEN_MATCH" <<'PY'
import json, sys
from pathlib import Path
(out, apk, serial, version_name, version_code, apk_md5, apk_sha256, cert_md5,
 cert_sha256, so_md5, so_size, meta_md5, meta_size,
 dex_count, dex_list, so_count, res_method, device_sig, history_sig,
 sig_match) = sys.argv[1:]
data = {
    "package": "com.happyelements.es2",
    "apk_path": apk,
    "serial": serial,
    "version_name": version_name,
    "version_code": int(version_code),
    "apk_md5": apk_md5,
    "apk_sha256": apk_sha256,
    "signer_cert_md5": cert_md5,
    "signer_cert_sha256": cert_sha256,
    "device_signature_token": device_sig,
    "history_4.4.8684_signature_token": history_sig,
    "signature_token_matches_history": sig_match == "true",
    "libil2cpp_md5": so_md5,
    "libil2cpp_size": int(so_size),
    "global_metadata_md5": meta_md5,
    "global_metadata_size": int(meta_size),
    "dex_count": int(dex_count),
    "dex_list": [x for x in dex_list.split(",") if x],
    "arm64_so_count": int(so_count),
    "resources_arsc_compression": res_method,
    "contains_libstarry_magic": False,
}
Path(out).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

cat <<EOF
package=$PKG
versionName=$VERSION_NAME
versionCode=$VERSION_CODE
cert_md5=$CERT_MD5
device_signature_token=$DEVICE_SIGNATURE_TOKEN
history_signature_token=$HISTORY_SIGNATURE_TOKEN
signature_token_matches_history=$SIGNATURE_TOKEN_MATCH
libil2cpp_md5=$SO_MD5
libil2cpp_size=$SO_SIZE
global_metadata_md5=$META_MD5
global_metadata_size=$META_SIZE
dex_count=$DEX_COUNT
dex_list=$DEX_LIST
arm64_so_count=$SO_COUNT
resources_arsc_compression=$RES_METHOD
summary=$OUT_FILE
EOF
