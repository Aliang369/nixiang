#!/usr/bin/env bash
# 山海幻境：核对真机官方包身份、签名、ZIP 结构与 Cocos 引擎标记。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
META_FILE="$SCRIPT_DIR/official_apk.json"
OUT_FILE="$SCRIPT_DIR/official_check.json"
TXT_FILE="$SCRIPT_DIR/official_check.txt"
PKG="com.chx.shhj"
HISTORY_SIGNER_MD5="86d1959875555021642fcf3b0d9c161d"

[[ -f "$META_FILE" ]] || { echo "错误: 缺少 $META_FILE" >&2; exit 1; }
APK="$(python3 - "$META_FILE" <<'PY'
import json
import sys
print(json.load(open(sys.argv[1], encoding="utf-8"))["apk_path"])
PY
)"
META_VERSION_NAME="$(python3 - "$META_FILE" <<'PY'
import json
import sys
print(json.load(open(sys.argv[1], encoding="utf-8"))["version_name"])
PY
)"
META_VERSION_CODE="$(python3 - "$META_FILE" <<'PY'
import json
import sys
print(json.load(open(sys.argv[1], encoding="utf-8"))["version_code"])
PY
)"
META_MD5="$(python3 - "$META_FILE" <<'PY'
import json
import sys
print(json.load(open(sys.argv[1], encoding="utf-8"))["md5"])
PY
)"
META_SHA256="$(python3 - "$META_FILE" <<'PY'
import json
import sys
print(json.load(open(sys.argv[1], encoding="utf-8"))["sha256"])
PY
)"
[[ -f "$APK" ]] || { echo "错误: APK 不存在: $APK" >&2; exit 1; }
[[ "$(md5 -q "$APK")" == "$META_MD5" ]] || { echo "错误: APK MD5 与元数据不一致" >&2; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/aapt2" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"
    break
  fi
done
[[ -n "$BT" ]] || { echo "错误: 找不到 Android build-tools" >&2; exit 1; }

BADGING="$("$BT/aapt2" dump badging "$APK")"
PACKAGE_LINE="$(printf '%s\n' "$BADGING" | sed -n '1p')"
printf '%s\n' "$PACKAGE_LINE" | grep -Fq "name='$PKG'" || { echo "错误: 包名不匹配: $PACKAGE_LINE" >&2; exit 1; }
APK_VERSION_NAME="$(printf '%s\n' "$PACKAGE_LINE" | sed -n "s/.*versionName='\([^']*\)'.*/\1/p")"
APK_VERSION_CODE="$(printf '%s\n' "$PACKAGE_LINE" | sed -n "s/.*versionCode='\([^']*\)'.*/\1/p")"
[[ "$APK_VERSION_NAME" == "$META_VERSION_NAME" ]] || { echo "错误: versionName 不一致" >&2; exit 1; }
[[ "$APK_VERSION_CODE" == "$META_VERSION_CODE" ]] || { echo "错误: versionCode 不一致" >&2; exit 1; }

CERT_OUT="$("$BT/apksigner" verify --print-certs "$APK" 2>&1)"
CERT_MD5="$(printf '%s\n' "$CERT_OUT" | sed -n 's/.*certificate MD5 digest: //p' | head -1 | tr 'A-F' 'a-f')"
CERT_SHA256="$(printf '%s\n' "$CERT_OUT" | sed -n 's/.*certificate SHA-256 digest: //p' | head -1 | tr 'A-F' 'a-f')"
CERT_DN="$(printf '%s\n' "$CERT_OUT" | sed -n 's/^Signer #1 certificate DN: //p' | head -1)"
[[ -n "$CERT_MD5" ]] || { echo "错误: 无法读取证书指纹" >&2; exit 1; }
if [[ "$CERT_MD5" != "$HISTORY_SIGNER_MD5" ]]; then
  echo "警告: 官方签名已变化: history=$HISTORY_SIGNER_MD5 current=$CERT_MD5" >&2
fi

ENTRIES="$SCRIPT_DIR/apk_entries.txt"
unzip -Z1 "$APK" > "$ENTRIES"
if grep -qx 'lib/arm64-v8a/libstarry_magic.so' "$ENTRIES"; then
  echo "错误: 输入包已含 libstarry_magic.so，样本身份不干净" >&2
  exit 1
fi

DEX_COUNT="$(grep -Ec '^classes([0-9]*)\.dex$' "$ENTRIES" | tr -d ' ')"
DEX_LIST="$(grep -E '^classes([0-9]*)\.dex$' "$ENTRIES" | sort -V | paste -sd, -)"
COCOS_COUNT="$(grep -Ec '^lib/arm64-v8a/libcocos2djs\.so$' "$ENTRIES" | tr -d ' ')"
SDKLOADING_COUNT="$(grep -Ec '^lib/arm64-v8a/libSDKloading\.so$' "$ENTRIES" || true)"
TOOLCHECKER_COUNT="$(grep -Ec '^lib/arm64-v8a/libtoolChecker\.so$' "$ENTRIES" || true)"
JSC_COUNT="$(grep -Ec '\.jsc$' "$ENTRIES" || true)"
MAIN_JS_COUNT="$(grep -Ec '(^|/)main\.js$' "$ENTRIES" || true)"
[[ "$COCOS_COUNT" -gt 0 ]] || { echo "错误: 未发现 libcocos2djs.so" >&2; exit 1; }

COCOS_MD5="$(unzip -p "$APK" lib/arm64-v8a/libcocos2djs.so | md5 -q)"
COCOS_SIZE="$(unzip -l "$APK" lib/arm64-v8a/libcocos2djs.so | awk 'NR==4{print $1}')"
RES_METHOD="$(zipinfo -v "$APK" 'resources.arsc' | awk -F': ' '/compression method:/{if (!found) {print $2; found=1}}')"

python3 - "$OUT_FILE" "$APK" "$META_MD5" "$META_SHA256" \
  "$APK_VERSION_NAME" "$APK_VERSION_CODE" "$CERT_MD5" "$CERT_SHA256" "$CERT_DN" \
  "$DEX_COUNT" "$DEX_LIST" "$COCOS_MD5" "$COCOS_SIZE" \
  "$SDKLOADING_COUNT" "$TOOLCHECKER_COUNT" "$JSC_COUNT" "$MAIN_JS_COUNT" \
  "$RES_METHOD" "$HISTORY_SIGNER_MD5" <<'PY'
import json
import sys
from pathlib import Path

(out, apk, md5, sha256, version_name, version_code, cert_md5,
 cert_sha256, cert_dn, dex_count, dex_list, cocos_md5, cocos_size,
 sdkloading_count, toolchecker_count, jsc_count, main_js_count,
 res_method, history_md5) = sys.argv[1:]
data = {
    "apk_path": apk,
    "apk_md5": md5,
    "apk_sha256": sha256,
    "package": "com.chx.shhj",
    "version_name": version_name,
    "version_code": int(version_code),
    "signer_cert_md5": cert_md5,
    "signer_cert_sha256": cert_sha256,
    "signer_dn": cert_dn,
    "history_signer_md5": history_md5,
    "signer_matches_history": cert_md5 == history_md5,
    "dex_count": int(dex_count),
    "dex_list": [x for x in dex_list.split(",") if x],
    "libcocos2djs_md5": cocos_md5,
    "libcocos2djs_size": int(cocos_size),
    "has_libSDKloading": sdkloading_count == "1",
    "has_libtoolChecker": toolchecker_count == "1",
    "jsc_entry_count": int(jsc_count),
    "main_js_entry_count": int(main_js_count),
    "resources_arsc_compression": res_method,
    "contains_libstarry_magic": False,
}
Path(out).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

{
  echo "package=$PKG"
  echo "versionName=$APK_VERSION_NAME"
  echo "versionCode=$APK_VERSION_CODE"
  echo "cert_md5=$CERT_MD5"
  echo "cert_dn=$CERT_DN"
  echo "signer_matches_history=$([[ "$CERT_MD5" == "$HISTORY_SIGNER_MD5" ]] && echo true || echo false)"
  echo "dex_count=$DEX_COUNT"
  echo "dex_list=$DEX_LIST"
  echo "libcocos2djs_md5=$COCOS_MD5"
  echo "libcocos2djs_size=$COCOS_SIZE"
  echo "has_libSDKloading=$SDKLOADING_COUNT"
  echo "has_libtoolChecker=$TOOLCHECKER_COUNT"
  echo "jsc_entry_count=$JSC_COUNT"
  echo "main_js_entry_count=$MAIN_JS_COUNT"
  echo "resources_arsc_compression=$RES_METHOD"
  echo "summary=$OUT_FILE"
} | tee "$TXT_FILE"
