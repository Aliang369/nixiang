#!/usr/bin/env bash
# 追迹迷城：核对真机提取包的包名、版本、ABI、签名和 IL2CPP 输入完整性。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
META_FILE="$SCRIPT_DIR/official_apk.json"
SUMMARY_FILE="$SCRIPT_DIR/check_summary.json"
PKG="com.TeaMelo.DungeonChasers"
OLD_CERT_MD5="4684a2b2249e978c74230f8895635894"

[[ -f "$META_FILE" ]] || { echo "错误: 缺少 $META_FILE" >&2; exit 1; }

while IFS=$'\t' read -r key value; do
  case "$key" in
    apk_path) APK="$value" ;;
    version_name) VERSION_NAME="$value" ;;
    version_code) VERSION_CODE="$value" ;;
    serial) SERIAL="$value" ;;
    md5) APK_MD5="$value" ;;
    sha256) APK_SHA256="$value" ;;
  esac
done < <(python3 - "$META_FILE" <<'PY'
import json, sys
d = json.load(open(sys.argv[1], encoding="utf-8"))
for k in ("apk_path", "version_name", "version_code", "serial", "md5", "sha256"):
    print(f"{k}\t{d[k]}")
PY
)

[[ -f "$APK" ]] || { echo "错误: APK 不存在: $APK" >&2; exit 1; }

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
if ! printf '%s\n' "$PACKAGE_LINE" | grep -Fq "name='$PKG'"; then
  echo "错误: 包名不匹配: $PACKAGE_LINE" >&2
  exit 1
fi

APK_VERSION_NAME="$(printf '%s\n' "$PACKAGE_LINE" | sed -n "s/.*versionName='\([^']*\)'.*/\1/p")"
APK_VERSION_CODE="$(printf '%s\n' "$PACKAGE_LINE" | sed -n "s/.*versionCode='\([^']*\)'.*/\1/p")"
[[ "$APK_VERSION_NAME" == "$VERSION_NAME" ]] || { echo "错误: aapt2 versionName 与设备记录不一致" >&2; exit 1; }
[[ "$APK_VERSION_CODE" == "$VERSION_CODE" ]] || { echo "错误: aapt2 versionCode 与设备记录不一致" >&2; exit 1; }

CERT_OUT="$("$BT/apksigner" verify --print-certs "$APK" 2>&1)"
CERT_MD5="$(printf '%s\n' "$CERT_OUT" | sed -n 's/.*certificate MD5 digest: //p' | head -1 | tr 'A-F' 'a-f')"
[[ -n "$CERT_MD5" ]] || { echo "错误: 无法读取 APK 签名证书 MD5" >&2; printf '%s\n' "$CERT_OUT" >&2; exit 1; }

SO_MD5="$(unzip -p "$APK" lib/arm64-v8a/libil2cpp.so | md5 -q)"
META_MD5="$(unzip -p "$APK" assets/bin/Data/Managed/Metadata/global-metadata.dat | md5 -q)"
if [[ -z "$SO_MD5" || -z "$META_MD5" ]]; then
  echo "错误: APK 缺少 libil2cpp.so 或 global-metadata.dat" >&2
  exit 1
fi
if unzip -Z1 "$APK" | grep -qx 'lib/arm64-v8a/libstarry_magic.so'; then
  echo "错误: 官方包已含 libstarry_magic.so，样本身份不干净" >&2
  exit 1
fi

CERT_CHANGED="false"
[[ "$CERT_MD5" != "$OLD_CERT_MD5" ]] && CERT_CHANGED="true"
DOC="$NIXIANG_ROOT/10_docs/追迹迷城/官方包核对_${VERSION_NAME}.md"
mkdir -p "$(dirname "$DOC")"

python3 - "$SUMMARY_FILE" "$APK" "$SERIAL" "$VERSION_NAME" "$VERSION_CODE" \
  "$APK_MD5" "$APK_SHA256" "$CERT_MD5" "$OLD_CERT_MD5" "$CERT_CHANGED" "$SO_MD5" "$META_MD5" <<'PY'
import json
import sys
from pathlib import Path

(out, apk, serial, version_name, version_code, md5, sha256, cert_md5,
 old_cert_md5, cert_changed, so_md5, meta_md5) = sys.argv[1:]
data = {
    "package": "com.TeaMelo.DungeonChasers",
    "apk_path": apk,
    "serial": serial,
    "version_name": version_name,
    "version_code": int(version_code),
    "apk_md5": md5,
    "apk_sha256": sha256,
    "signer_cert_md5": cert_md5,
    "old_20260906_cert_md5": old_cert_md5,
    "cert_changed_vs_20260906": cert_changed == "true",
    "libil2cpp_md5": so_md5,
    "global_metadata_md5": meta_md5,
}
Path(out).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

cat > "$DOC" <<EOF
# 追迹迷城官方包核对 — $VERSION_NAME

- 提取时间: $(date '+%Y-%m-%d %H:%M:%S')
- 设备: \`$SERIAL\`
- 包名: \`$PKG\`
- 版本: \`$VERSION_NAME\` / vc \`$VERSION_CODE\`
- APK: \`$APK\`
- APK 大小: $(stat -f '%z' "$APK") bytes
- APK MD5: \`$APK_MD5\`
- APK SHA-256: \`$APK_SHA256\`
- 官方签名证书 MD5: \`$CERT_MD5\`
- 20260906 证书 MD5: \`$OLD_CERT_MD5\`
- 证书相对 20260906 是否变化: **$CERT_CHANGED**
- 包内 libil2cpp.so MD5: \`$SO_MD5\`
- 包内 global-metadata.dat MD5: \`$META_MD5\`
- 包内 lib/arm64-v8a/libstarry_magic.so: 不存在

## 结论

- 单一 base APK、包名、设备版本、arm64 IL2CPP 输入均一致。
- 该样本可作为本版静态重定位底包。
EOF

echo "==> 官方包核对"
printf '%s\n' "$CERT_OUT" | sed -n '1,24p'
cat <<EOF
package=$PKG
versionName=$VERSION_NAME
versionCode=$VERSION_CODE
cert_md5=$CERT_MD5
cert_changed_vs_20260906=$CERT_CHANGED
libil2cpp_md5=$SO_MD5
global_metadata_md5=$META_MD5
summary=$SUMMARY_FILE
doc=$DOC
EOF
