#!/usr/bin/env bash
# 正中靶心2：核对真机拉取包的包名、版本、签名、引擎与斌哥壳特征。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
# shellcheck source=../_common/env.sh
source "$NIXIANG_ROOT/04_scripts/_common/env.sh"

META_FILE="$SCRIPT_DIR/binge_apk.json"
OUT_FILE="$SCRIPT_DIR/mod_check.json"
TEXT_FILE="$SCRIPT_DIR/mod_check.txt"
EXPECTED_PKG="${PKG:-com.openew.monster}"

[[ -f "$META_FILE" ]] || { echo "错误: 缺少 ${META_FILE}，请先跑 pull_mod" >&2; exit 1; }
APK="$(python3 - "$META_FILE" <<'PY'
import json, sys
print(json.load(open(sys.argv[1], encoding='utf-8'))['apk_path'])
PY
)"
[[ -f "$APK" ]] || { echo "错误: APK 不存在: $APK" >&2; exit 1; }
[[ -n "${NIXIANG_BUILD_TOOLS:-}" && -x "$NIXIANG_BUILD_TOOLS/aapt2" ]] || {
  echo "错误: 找不到 aapt2（NIXIANG_BUILD_TOOLS=${NIXIANG_BUILD_TOOLS}）" >&2
  exit 1
}

BADGING_TMP="$(mktemp -t zzbx2_badging.XXXXXX)"
SIGN_TMP="$(mktemp -t zzbx2_sign.XXXXXX)"
trap 'rm -f "$BADGING_TMP" "$SIGN_TMP"' EXIT

"$NIXIANG_BUILD_TOOLS/aapt2" dump badging "$APK" > "$BADGING_TMP"
"$NIXIANG_BUILD_TOOLS/apksigner" verify --verbose --print-certs "$APK" > "$SIGN_TMP" 2>&1

PACKAGE="$(sed -n "s/^package: name='\([^']*\)'.*/\1/p" "$BADGING_TMP" | head -1)"
VERSION_NAME="$(sed -n "s/^package:.*versionName='\([^']*\)'.*/\1/p" "$BADGING_TMP" | head -1)"
VERSION_CODE="$(sed -n "s/^package:.*versionCode='\([^']*\)'.*/\1/p" "$BADGING_TMP" | head -1)"
SIGNER_DN="$(sed -n 's/^Signer #1 certificate DN: //p' "$SIGN_TMP" | head -1)"
SIGNER_SHA256="$(sed -n 's/^Signer #1 certificate SHA-256 digest: //p' "$SIGN_TMP" | head -1)"

[[ "$PACKAGE" == "$EXPECTED_PKG" ]] || { echo "错误: 包名不匹配: $PACKAGE != $EXPECTED_PKG" >&2; exit 1; }

python3 - "$APK" "$BADGING_TMP" "$SIGN_TMP" "$OUT_FILE" "$TEXT_FILE" "$PACKAGE" "$VERSION_NAME" "$VERSION_CODE" "$SIGNER_DN" "$SIGNER_SHA256" <<'PY'
import json
import re
import sys
import zipfile
from pathlib import Path

apk, badging_path, sign_path, out_path, text_path, package, version_name, version_code, signer_dn, signer_sha256 = sys.argv[1:]

with zipfile.ZipFile(apk) as zf:
    names = zf.namelist()

libs = sorted(n for n in names if n.startswith('lib/') and n.endswith('.so'))
dexes = sorted(n for n in names if re.fullmatch(r'classes\d*\.dex', n))
assets_apk = sorted(n for n in names if n.startswith('assets/') and n.endswith('.apk'))
assets_jsc = sorted(n for n in names if n.startswith('assets/') and n.endswith(('.jsc', '.js', '.json')))

markers = {
    'libSDKloading.so': any(n.endswith('/libSDKloading.so') for n in libs),
    'libtoolChecker.so': any(n.endswith('/libtoolChecker.so') for n in libs),
    'libcodec2_server.so': any(n.endswith('/libcodec2_server.so') for n in libs),
    'libunityluac.so': any(n.endswith('/libunityluac.so') for n in libs),
    'nested_assets_base_apk': any(n.startswith('assets/') and n.endswith('.apk') for n in names),
    'cocos2djs': any(n.endswith('/libcocos2djs.so') for n in libs),
    'cocos': any(n.endswith('/libcocos.so') for n in libs),
}

with zipfile.ZipFile(apk) as zf:
    candidates = [n for n in names if n in ('AndroidManifest.xml', 'resources.arsc', 'assets/application.js', 'assets/main.js') or n.endswith('/application.js') or n.endswith('/main.js')]
    blob = b''
    for n in candidates[:40]:
        try:
            blob += zf.read(n)
        except Exception:
            pass

needles = {
    'binge_cn': b'binge2.keystore',
    'menu_tencent_url': b'menu-1255602259.file.myqcloud.com',
    'binge_email': b'bg888@188.com',
    'binge_prefs_bridge': b'kaiguan',
}
markers.update({k: (v in blob) for k, v in needles.items()})

mod_score = sum(bool(markers[k]) for k in (
    'libSDKloading.so', 'libtoolChecker.so', 'libcodec2_server.so', 'libunityluac.so',
    'nested_assets_base_apk', 'binge_cn', 'menu_tencent_url', 'binge_email', 'binge_prefs_bridge'
))
hits = sum(1 for k in ('binge_cn', 'menu_tencent_url', 'binge_email', 'binge_prefs_bridge') if markers[k])
if 'binge2.keystore' in signer_dn:
    hits += 1
verdict = 'binge_strong' if hits >= 2 else ('binge_candidate' if hits == 1 or mod_score >= 2 else 'unknown')

summary = {
    'apk': apk,
    'package': package,
    'version_name': version_name,
    'version_code': int(version_code) if version_code.isdigit() else version_code,
    'signer_dn': signer_dn,
    'signer_sha256': signer_sha256,
    'dexes': dexes,
    'native_libs': libs,
    'asset_apks': assets_apk,
    'asset_script_samples': assets_jsc[:80],
    'markers': markers,
    'mod_score': mod_score,
    'verdict': verdict,
}
Path(out_path).write_text(json.dumps(summary, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')

lines = [
    f'package={package}',
    f'versionName={version_name}',
    f'versionCode={version_code}',
    f'signerDN={signer_dn}',
    f'signerSHA256={signer_sha256}',
    f'verdict={verdict}',
    f'modScore={mod_score}',
    f'dexCount={len(dexes)}',
    f'nativeLibCount={len(libs)}',
]
for key, value in markers.items():
    lines.append(f'{key}={1 if value else 0}')
lines.append('nativeLibs=' + ','.join(libs))
lines.append('assetAPKs=' + ','.join(assets_apk))
lines.append('assetScriptSamples=' + ','.join(assets_jsc[:40]))
Path(text_path).write_text('\n'.join(lines) + '\n', encoding='utf-8')
print('\n'.join(lines))
PY

echo "json=$OUT_FILE"
echo "text=$TEXT_FILE"
