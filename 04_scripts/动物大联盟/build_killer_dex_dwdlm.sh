#!/usr/bin/env bash
# 动物大联盟 1.0.25 — 从「参数化 smali 源码」重建签名杀手 dex（B 轨「运行时签名替换」用）。
# 范本：04_scripts/不一样西游/build_killer_dex_byyxy.sh（同一配方，只改包名/证书/底包）。
#
# 为什么不是直接入库 dex：仓库 .gitignore 把 *.dex 排除（Git 资产=文本），故 killer 以
# 可复现的 smali 源码入库（assets/dwdlm_killer_smali，MT ApkSignatureKillerEx 形态，13 个文件），
# 本脚本在构建机上烘焙「包名 + 官方证书」后重组 dex。
#
# 背景（2026-11-05 实测，见 09_test/动物大联盟/sigspoof_20261105/）：
#   APKSecure 壳用「运行时签名」（PackageManager 查询结果）解 assets/apksecurefile 的密钥
#   （libsecret.so 内字符串 "key-unwrap: get runtime signature failed" /
#   "sig-check path: GET_SIGNATURES(fallback) API=%d" / "signature cert cross mismatch"）。
#   纯重签、零注入下同样崩 ⇒ 与菜单/native 注入无关，根因是重签后拿不到官方签名。
#   ⇒ 用 MT ApkSignatureKillerEx 形态在进程最早处把「运行时签名」换回官方值。
#
# 产物：assets/dwdlm_signature_killer.dex（surgical_dwdlm_1.0.25.sh 在 DWDLM_SIGSPOOF=1 时
#       反编译后并入同一个注入 dex classes2.dex，不新增 dex 编号）
# 门禁：产物 md5 必须等于 EXPECT_MD5（同源 smali + 同证书 ⇒ 逐字节可复现）
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
BASE_APK="${DWDLM_BASE_APK:-$NIXIANG/02_apks/动物大联盟/动物大联盟_1.0.25_official.apk}"
PKG="com.fightingstars.games.animal"
# 官方证书 DER 的 SHA-256（以大写登记，脚本内转小写比对）
CERT_SHA256="${DWDLM_CERT_SHA256:-DE8A75D30A1831F3DDC83F38F285513110FDE9AB919FB4E471653F86B5083E0A}"
SMALI_SRC="$SCRIPT_DIR/assets/dwdlm_killer_smali"
OUT="$SCRIPT_DIR/assets/dwdlm_signature_killer.dex"
# 2026-11-05 首次烘焙实测 md5=ec99da8bded10307714ca6d7c3a0c44f（13984 B）；已回填为默认门禁值。
# 同源 smali + 同证书 ⇒ 逐字节可复现，重跑必须命中该值。
EXPECT_MD5="${DWDLM_KILLER_MD5:-ec99da8bded10307714ca6d7c3a0c44f}"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$BASE_APK" ]] || { echo "ERROR: 缺少官方底包 $BASE_APK" >&2; exit 1; }
[[ -d "$SMALI_SRC" ]] || { echo "ERROR: 缺少 smali 源码 $SMALI_SRC" >&2; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: 缺少 smali 工具 $SMALI_BIN" >&2; exit 1; }

WORK="$(mktemp -d /tmp/dwdlm_killer.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

echo "[killer] base=$BASE_APK"
echo "[killer] pkg=$PKG expect_cert_sha256_upper=$CERT_SHA256"

python3 - "$BASE_APK" "$CERT_SHA256" "$WORK/cert.b64" <<'PY'
import base64, hashlib, struct, sys

apk, want_sha, out = sys.argv[1], sys.argv[2], sys.argv[3]
want_sha = want_sha.lower()
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
wrapped = '\\n'.join(b64[j:j + 64] for j in range(0, len(b64), 64)) + '\\n'
open(out, 'w').write(wrapped)
print('  cert DER %d B, SHA-256 %s' % (len(der), sha))
print('  cert base64 %d 字符，每 64 字符换行共 %d 行' % (len(b64), wrapped.count('\\n')))
PY

# smali 源码刻意不以 bin/ 为根目录入库（.gitignore 的 '04_scripts/**/bin/' 会整目录忽略），
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
MAIN="$WORK/verify/bin/mt/signature/KillerApplication193.smali"
grep -q '^\.super Landroid/app/Application;' "$MAIN" \
  || { echo "ERROR: killer 父类不是 Application" >&2; exit 1; }
grep -q "const-string v1, \"$PKG\"" "$MAIN" \
  || { echo "ERROR: killer 包名未烘焙" >&2; exit 1; }
if grep -rq '@@' "$WORK/verify/bin/mt/signature/"; then
  echo "ERROR: 反编译产物残留 @@ 占位符" >&2; exit 1
fi
N_INNER=0
for suf in a b c d e f g h i j k l; do
  [[ -f "$WORK/verify/bin/mt/signature/KillerApplication193\$$suf.smali" ]] || {
    echo "ERROR: 反编译产物缺内部类 KillerApplication193 \$$suf" >&2; exit 1; }
  N_INNER=$((N_INNER + 1))
done
if [[ -n "$EXPECT_MD5" && "$MD5" != "$EXPECT_MD5" ]]; then
  echo "ERROR: killer dex md5 与记录不符: $MD5 != $EXPECT_MD5" >&2; exit 1
fi
echo "  verify ok: .super=Application / pkg 已烘焙 / 内部类 ${N_INNER} 个 / 无 @@ 残留"
echo "[killer] ok $OUT ($SIZE B, md5 $MD5)"
echo "  DWDLM_CERT_SHA256=$CERT_SHA256  DWDLM_KILLER_MD5=$MD5"
