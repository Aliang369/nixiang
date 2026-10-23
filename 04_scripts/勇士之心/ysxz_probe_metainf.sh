#!/usr/bin/env bash
# 勇士之心 1.0.6：探针——把 META-INF 里的 v1 证书换成【官方证书 DER】，v2 仍用星空密钥签。
#
# 目的（2026-10-24）：TapSDK 打印的指纹是「星空证书 161fca32…」而不是我们 Java 层伪造的官方
# （79a6a62f…），说明它没走 PackageManager，而是自己解析 APK。open() 追踪显示 base.apk 被
# libopenjdkjvm/libjavacore（Java ZipFile/JarFile）打开 ⇒ 很可能在读 META-INF 里的 v1 证书。
# 本探针：META-INF/CERT.RSA 放入官方证书 DER，然后用 apksigner **只开 v2**（不开 v1，避免
# 覆盖我们放进去的 META-INF 条目）签名。Android 装机只看 v2/v3，所以安装不受影响。
#
# 用法：bash 04_scripts/勇士之心/ysxz_probe_metainf.sh
# 产物：08_output/勇士之心/勇士之心_1.0.6_metainf-probe-signed.apk（**探针包，不作交付**）
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
GAME="勇士之心"
OUT="$ROOT/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_1.0.6_surgical-unsigned.apk"
PROBE="$OUT/${GAME}_1.0.6_metainf-probe-unsigned.apk"
SIGNED="$OUT/${GAME}_1.0.6_metainf-probe-signed.apk"
KS="$ROOT/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
CERT_DER="${CERT_DER:-/tmp/official.der}"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/apksigner" ]]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
[[ -n "$BT" ]] || { echo "ERROR: apksigner 未找到" >&2; exit 1; }
[[ -f "$UNSIGNED" ]] || { echo "ERROR: 缺 $UNSIGNED" >&2; exit 1; }
[[ -f "$CERT_DER" ]] || { echo "ERROR: 缺官方证书 DER $CERT_DER" >&2; exit 1; }

python3 - "$UNSIGNED" "$PROBE" "$CERT_DER" <<'PY'
import sys, zipfile
src, dst, der = sys.argv[1], sys.argv[2], open(sys.argv[3], 'rb').read()
zin = zipfile.ZipFile(src)
zout = zipfile.ZipFile(dst, 'w')
for i in zin.infolist():
    if i.filename.startswith('META-INF/') and i.filename.upper().endswith(('.RSA', '.DSA', '.EC', '.SF', '.MF')):
        continue
    info = zipfile.ZipInfo(i.filename, i.date_time)
    info.compress_type = zipfile.ZipInfo(i.filename).compress_type if False else i.compress_type
    info.external_attr = i.external_attr
    zout.writestr(info, zin.read(i))
# 官方 v1 证书（DER）；再加一个空 .SF 占位，让 JarFile 风格的解析器有配对项
zout.writestr('META-INF/CERT.RSA', der)
zout.writestr('META-INF/CERT.SF', b'Signature-Version: 1.0\r\nCreated-By: starry-probe\r\n\r\n')
zout.close(); zin.close()
print('probe unsigned built, cert bytes =', len(der))
PY

"$BT/zipalign" -f -p 4 "$PROBE" "$PROBE.aligned"
export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" --ks-pass "pass:$KS_PASS" \
  --v1-signing-enabled false --v2-signing-enabled true --v3-signing-enabled false \
  --out "$SIGNED" "$PROBE.aligned"
"$BT/apksigner" verify --print-certs "$SIGNED" | head -4
rm -f "$PROBE.aligned"
echo "[probe] signed=$SIGNED"
md5 -q "$SIGNED" | sed 's/^/md5=/'
