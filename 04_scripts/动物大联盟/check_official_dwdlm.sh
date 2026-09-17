#!/usr/bin/env bash
# 动物大联盟：核对官方 1.0.23 原包的身份、签名、ABI 与关键资源。
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
source "$NIXIANG_ROOT/04_scripts/_common/env.sh"
META="$SCRIPT_DIR/official_apk.json"
OUT="$SCRIPT_DIR/official_check.json"
TXT="$SCRIPT_DIR/official_check.txt"
[[ -f "$META" ]] || { echo "错误: 缺少 $META" >&2; exit 1; }
APK="$(python3 - "$META" <<'PY'
import json,sys
print(json.load(open(sys.argv[1],encoding='utf-8'))['apk_path'])
PY
)"
[[ -f "$APK" ]] || { echo "错误: APK 不存在: $APK" >&2; exit 1; }
[[ -n "${NIXIANG_BUILD_TOOLS:-}" ]] || { echo "错误: 未找到 Android build-tools" >&2; exit 1; }
BAD="$(mktemp -t dwdlm_official_bad.XXXXXX)"
SIG="$(mktemp -t dwdlm_official_sig.XXXXXX)"
trap 'rm -f "$BAD" "$SIG"' EXIT
"$NIXIANG_BUILD_TOOLS/aapt2" dump badging "$APK" > "$BAD"
"$NIXIANG_BUILD_TOOLS/apksigner" verify --verbose --print-certs "$APK" > "$SIG" 2>&1
PKG=$(sed -n "s/^package: name='\([^']*\)'.*/\1/p" "$BAD" | head -1)
VN=$(sed -n "s/^package:.*versionName='\([^']*\)'.*/\1/p" "$BAD" | head -1)
VC=$(sed -n "s/^package:.*versionCode='\([^']*\)'.*/\1/p" "$BAD" | head -1)
DN=$(sed -n 's/^Signer #1 certificate DN: //p' "$SIG" | head -1)
SHA=$(sed -n 's/^Signer #1 certificate SHA-256 digest: //p' "$SIG" | head -1)
[[ "$PKG" == "com.fightingstars.games.animal" && "$VN" == "1.0.23" ]] || { echo "错误: 包名/版本不符: $PKG $VN" >&2; exit 1; }
python3 - "$APK" "$OUT" "$TXT" "$PKG" "$VN" "$VC" "$DN" "$SHA" <<'PY'
import json, sys, zipfile
from pathlib import Path
apk, out, txt, pkg, vn, vc, dn, sha = sys.argv[1:]
with zipfile.ZipFile(apk) as z:
    names = z.namelist()
markers = {
    'has_libcocos2djs': any(n.endswith('/libcocos2djs.so') for n in names),
    'has_main_js': 'assets/main.js' in names,
    'has_src_jsc': any(n.startswith('assets/src/') and n.endswith('.jsc') for n in names),
    'has_libtapsdkcore': any(n.endswith('/libtapsdkcore.so') for n in names),
    'has_libEncryptorP': any(n.endswith('/libEncryptorP.so') for n in names),
    'has_libsecret': any(n.endswith('/libsecret.so') for n in names),
    'has_libBugly': any(n.endswith('/libBugly.so') for n in names),
}
data = {
    'apk': apk,
    'package': pkg,
    'version_name': vn,
    'version_code': int(vc),
    'signer_dn': dn,
    'signer_sha256': sha,
    'markers': markers,
}
Path(out).write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
Path(txt).write_text('\n'.join([
    f'package={pkg}',
    f'versionName={vn}',
    f'versionCode={vc}',
    f'signerDN={dn}',
    f'signerSHA256={sha}',
] + [f'{k}={1 if v else 0}' for k, v in markers.items()]) + '\n', encoding='utf-8')
print(Path(txt).read_text(encoding='utf-8'))
PY
