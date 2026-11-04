#!/usr/bin/env bash
# 黎明前20分钟 — 从「参数化 smali 源码」重建签名杀手 dex（B 轨运行时签名替换用）。
# 来源脚本：04_scripts/追迹迷城/build_killer_dex_zjmc.sh（同一配方，只改包名/证书/底包）。
#
# 为什么不是直接入库 dex：仓库 .gitignore 把 *.dex 排除（Git 资产=文本），故 killer 以
# 可复现的 smali 源码入库（assets/lmq20fz_killer_smali，MT ApkSignatureKillerEx 形态），
# 本脚本在构建机上烘焙「包名 + 官方证书」后重组 dex。
#
# 产物：assets/lmq20fz_signature_killer.dex（surgical 以 classes8.dex 塞进包内，
#       由 Manifest 的 appComponentFactory=com.starry.magic.sign.StarryAppComponentFactory 挂载）
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
BASE_APK="$NIXIANG/02_apks/原版_TapTap_20261103/twenty_minutes.apk"
if [ -n "$(printenv LMQ20FZ_BASE_APK || true)" ]; then BASE_APK="$LMQ20FZ_BASE_APK"; fi
PKG="com.nanostudios.games.twenty.minutes"
# 官方证书 DER 的 SHA-256（首次烘焙后回填到本行，用于跨机复现校验）
CERT_SHA256="$(printenv LMQ20FZ_CERT_SHA256 || true)"
SMALI_SRC="$SCRIPT_DIR/assets/lmq20fz_killer_smali"
OUT="$SCRIPT_DIR/assets/lmq20fz_signature_killer.dex"
EXPECT_MD5="$(printenv LMQ20FZ_KILLER_MD5 || true)"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"

export JAVA_HOME="$(/usr/libexec/java_home -v 17 2>/dev/null || true)"
if [ -n "$JAVA_HOME" ]; then export PATH="$JAVA_HOME/bin:$PATH"; fi

[ -f "$BASE_APK" ] || { echo "ERROR: 缺少官方底包 $BASE_APK" >&2; exit 1; }
[ -d "$SMALI_SRC" ] || { echo "ERROR: 缺少 smali 源码 $SMALI_SRC" >&2; exit 1; }
[ -x "$SMALI_BIN" ] || { echo "ERROR: 缺少 smali 工具 $SMALI_BIN" >&2; exit 1; }

WORK="$(mktemp -d /tmp/lmq20fz_killer.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

python3 - "$BASE_APK" "$CERT_SHA256" "$WORK/cert.b64" <<'PY'
import base64, hashlib, struct, sys

apk, want_sha, out = sys.argv[1], sys.argv[2], sys.argv[3]
data = open(apk, 'rb').read()
magic = b'APK Sig Block 42'
i = data.rfind(magic)
if i <= 0:
    raise SystemExit('ERROR: 底包无 APK Signing Block')
size = struct.unpack('<Q', data[i - 8:i])[0]
block_start = i + 16 - size - 8
block = data[block_start:i + 16]
pos, end, pairs = 8, len(block) - 24, {}
while pos < end:
    plen = struct.unpack('<Q', block[pos:pos + 8])[0]
    pid = struct.unpack('<I', block[pos + 8:pos + 12])[0]
    pairs[pid] = block[pos + 12:pos + 8 + plen]
    pos += 8 + plen

def lp(buf):
    n = struct.unpack('<I', buf[:4])[0]
    return buf[4:4 + n], buf[4 + n:]

v2 = pairs.get(0x7109871a) or pairs.get(0xf05368c0)
if not v2:
    raise SystemExit('ERROR: 底包无 v2/v3 签名块')
signers, _ = lp(v2)
signer, _ = lp(signers)
signed_data, _ = lp(signer)
_, rest = lp(signed_data)
certs, _ = lp(rest)
der, _ = lp(certs)
sha = hashlib.sha256(der).hexdigest()
if want_sha and sha != want_sha:
    raise SystemExit('ERROR: 官方证书指纹不符: ' + sha)
b64 = base64.b64encode(der).decode()
wrapped = b64  # 单行长串
open(out, 'w').write(wrapped)
print('  cert DER %d B, SHA-256 %s' % (len(der), sha))
PY

# smali 源码刻意不以 bin/ 为根目录入库（.gitignore '04_scripts/**/bin/' 会整目录忽略），
# 这里还原成 smali 汇编器期望的 bin/mt/signature/ 结构。
mkdir -p "$WORK/smali/bin"
cp -R "$SMALI_SRC/." "$WORK/smali/bin/"
python3 - "$WORK/smali/bin/mt/signature/KillerApplication193.smali" "$WORK/cert.b64" "$PKG" <<'PY'
import sys
from pathlib import Path

path, cert_file, pkg = sys.argv[1], sys.argv[2], sys.argv[3]
p = Path(path)
text = p.read_text(encoding='utf-8')
cert = Path(cert_file).read_text(encoding='utf-8')
if '@@CERT_B64_WRAPPED@@' not in text:
    raise SystemExit('ERROR: smali 内无 @@CERT_B64_WRAPPED@@ 占位符')
text = text.replace('@@CERT_B64_WRAPPED@@', cert)
if '"@@PKG@@"' not in text:
    raise SystemExit('ERROR: smali 内无 @@PKG@@ 占位符')
text = text.replace('"@@PKG@@"', '"' + pkg + '"')
p.write_text(text, encoding='utf-8')
print('  smali 烘焙完成: pkg=%s cert=%d 字符' % (pkg, len(cert)))
PY

"$SMALI_BIN" assemble -o "$OUT" "$WORK/smali"
MD5="$(md5 -q "$OUT")"
SIZE="$(stat -f '%z' "$OUT")"
"$BAKSMALI_BIN" d -o "$WORK/verify" "$OUT" >/dev/null
grep -q '^\.super Landroid/app/Application;' "$WORK/verify/bin/mt/signature/KillerApplication193.smali" \
  || { echo "ERROR: killer 父类不是 Application" >&2; exit 1; }
grep -q "const-string v1, \"$PKG\"" "$WORK/verify/bin/mt/signature/KillerApplication193.smali" \
  || { echo "ERROR: killer 包名未烘焙" >&2; exit 1; }
if grep -q '@@' "$WORK/verify/bin/mt/signature/KillerApplication193.smali"; then
  echo "ERROR: 产物残留 @@ 占位符" >&2; exit 1
fi
if [ -n "$EXPECT_MD5" ] && [ "$MD5" != "$EXPECT_MD5" ]; then
  echo "ERROR: killer dex md5 与记录不符: $MD5 != $EXPECT_MD5" >&2; exit 1
fi
echo "[killer] ok $OUT ($SIZE B, md5 $MD5)"
echo "  LMQ20FZ_CERT_SHA256=$CERT_SHA256  LMQ20FZ_KILLER_MD5=$MD5"
