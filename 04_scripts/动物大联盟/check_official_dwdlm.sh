#!/usr/bin/env bash
# 动物大联盟：核对官方原包身份（包名/版本/SDK/ABI/split）与签名身份（证书 DN + 三种摘要）。
# 只读：只读 02_apks 下已登记的输入 APK，不修改 APK、不接触设备。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
source "${NIXIANG_ROOT}/04_scripts/_common/env.sh"
META="${SCRIPT_DIR}/official_apk.json"
OUT_JSON="${SCRIPT_DIR}/official_check.json"
OUT_TXT="${SCRIPT_DIR}/official_check.txt"

[ -f "${META}" ] || { echo "错误: 缺少 ${META}（先跑 pull_official）" >&2; exit 1; }
[ -n "${NIXIANG_BUILD_TOOLS:-}" ] || { echo "错误: 未找到 Android build-tools" >&2; exit 1; }
[ -x "${NIXIANG_BUILD_TOOLS}/aapt2" ] || { echo "错误: 缺 aapt2" >&2; exit 1; }

APK="$(python3 - "${META}" <<'PY'
import json, sys
print(json.load(open(sys.argv[1], encoding='utf-8'))['apk_path'])
PY
)"
[ -f "${APK}" ] || { echo "错误: APK 不存在: ${APK}" >&2; exit 1; }

BAD="$(mktemp -t dwdlm_bad.XXXXXX)"
SIG="$(mktemp -t dwdlm_sig.XXXXXX)"
XTREE="${SCRIPT_DIR}/official_manifest.xmltree"
trap 'rm -f "${BAD}" "${SIG}"' EXIT

"${NIXIANG_BUILD_TOOLS}/aapt2" dump badging "${APK}" > "${BAD}"
SIG_RC=0
"${NIXIANG_BUILD_TOOLS}/apksigner" verify --verbose --print-certs "${APK}" > "${SIG}" 2>&1 || SIG_RC=$?
"${NIXIANG_BUILD_TOOLS}/aapt2" dump xmltree --file AndroidManifest.xml "${APK}" > "${XTREE}"

python3 - "${APK}" "${BAD}" "${SIG}" "${XTREE}" "${OUT_JSON}" "${OUT_TXT}" "${SIG_RC}" <<'PY'
import json, re, sys
from pathlib import Path
apk, bad_p, sig_p, xt_p, out_json, out_txt, sig_rc = sys.argv[1:]
bad = Path(bad_p).read_text(encoding='utf-8', errors='replace')
sig = Path(sig_p).read_text(encoding='utf-8', errors='replace')
xt = Path(xt_p).read_text(encoding='utf-8', errors='replace')

def first(pat, text, grp=1):
    m = re.search(pat, text, re.M)
    return m.group(grp) if m else None

pkg_line = first(r"^package: (.*)$", bad) or ''
package = first(r"name='([^']*)'", pkg_line)
version_name = first(r"versionName='([^']*)'", pkg_line)
version_code = first(r"versionCode='([^']*)'", pkg_line)
platform_build = first(r"platformBuildVersionName='([^']*)'", pkg_line)
sdk_version = first(r"^minSdkVersion:'([^']*)'", bad) or first(r"^sdkVersion:'([^']*)'", bad)
target_sdk = first(r"^targetSdkVersion:'([^']*)'", bad)
native_codes = re.findall(r"^native-code: (.*)$", bad, re.M)
launchable = first(r"^launchable-activity: name='([^']*)'", bad)
splits = re.findall(r"^split='([^']*)'", bad, re.M)

dn = first(r"^Signer #1 certificate DN: (.*)$", sig)
sha256 = first(r"^Signer #1 certificate SHA-256 digest: (.*)$", sig)
sha1 = first(r"^Signer #1 certificate SHA-1 digest: (.*)$", sig)
md5 = first(r"^Signer #1 certificate MD5 digest: (.*)$", sig)
verified = [l for l in sig.splitlines() if l.startswith('Verifies')]

# AndroidManifest：application@name 与 provider authorities（注入点判定的输入，本阶段只记录不决策）
app_name = None
line_list = xt.splitlines()
for i, line in enumerate(line_list):
    if line.strip().startswith('E: application'):
        depth = len(line) - len(line.lstrip())
        for nxt in line_list[i+1:]:
            if not nxt.strip():
                continue
            if len(nxt) - len(nxt.lstrip()) <= depth and nxt.lstrip().startswith('E: '):
                break
            m = re.search(r':name\(0x[0-9a-f]+\)="([^"]*)"', nxt)
            if m and app_name is None and nxt.lstrip().startswith('A: '):
                app_name = m.group(1)
        break
providers = re.findall(r':authorities\(0x[0-9a-f]+\)="([^"]*)"', xt)
debuggable = first(r':debuggable\(0x[0-9a-f]+\)=(\S+)', xt)

data = {
    'apk': apk,
    'identity': {
        'package': package,
        'version_name': version_name,
        'version_code': int(version_code) if version_code and version_code.isdigit() else version_code,
        'sdk_version': sdk_version,
        'target_sdk_version': target_sdk,
        'platform_build_version': platform_build,
        'native_code': [n.strip() for n in native_codes],
        'launchable_activity': launchable,
        'split_entries': splits,
        'is_single_base': len(splits) == 0,
    },
    'signature': {
        'signer_dn': dn,
        'cert_sha256': sha256,
        'cert_sha1': sha1,
        'cert_md5': md5,
        'verify_lines': verified,
        'apksigner_rc': int(sig_rc),
    },
    'manifest': {
        'application_name': app_name,
        'debuggable': debuggable,
        'provider_authorities': providers[:20],
    },
}
Path(out_json).write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
lines = [
    'apk=' + apk,
    'package=' + str(package),
    'versionName=' + str(version_name),
    'versionCode=' + str(version_code),
    'sdkVersion=' + str(sdk_version),
    'targetSdkVersion=' + str(target_sdk),
    'native-code=' + ','.join(n.strip() for n in native_codes),
    'launchable-activity=' + str(launchable),
    'split_entries=' + (','.join(splits) if splits else '<none>'),
    'is_single_base=' + str(1 if not splits else 0),
    'signerDN=' + str(dn),
    'certSHA256=' + str(sha256).upper(),
    'certSHA1=' + str(sha1).upper(),
    'certMD5=' + str(md5).upper(),
    'apksigner_rc=' + str(sig_rc),
    'application_name=' + str(app_name),
    'debuggable=' + str(debuggable),
    'provider_count=' + str(len(providers)),
]
Path(out_txt).write_text('\n'.join(lines) + '\n', encoding='utf-8')
print('\n'.join(lines))
PY

echo "[check_official] done → ${OUT_TXT}"
