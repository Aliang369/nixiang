#!/usr/bin/env bash
# 霓虹深渊2 — 从「参数化 smali 源码」重建签名杀手 dex（B 轨运行时签名替换用）
# 来源：04_scripts/我在神界刷装备/build_killer_dex_sjzz.sh（同一 MT ApkSignatureKillerEx 配方，只换包名/证书/底包）
#
# 为什么不是直接入库 dex：仓库 .gitignore 排除 *.dex（Git 资产=文本），killer 以可复现 smali 源码入库，
# 本脚本在构建机上烘焙「包名 + 官方证书 DER」后重组 dex。
#
# 证书来源：底包 APK Signing Block 的 v2/v3 签名块（不依赖 META-INF/*.RSA，v3 轮换也适用）；
#   base64 折行宽度 = 76，与 surgical_nhsy2_2026.11.19.sh [4/8] 完全一致 ⇒ 两处产出的 classes6.dex 可逐字节比对。
#
# 产物：04_scripts/霓虹深渊2/assets/nhsy2_signature_killer.dex
# 用法：bash 04_scripts/霓虹深渊2/build_killer_dex_nhsy2.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
BASE_APK="${NHSY2_BASE_APK:-$NIXIANG/02_apks/霓虹深渊2/nhsy2_base_2026.11.19.ad4a3246d7c.apk}"
PKG="com.veewo.neonabyss2"
# 官方 Veewo 证书 DER 的 MD5（与底包指纹一致；变了要先复核 TapTap 双轨前提）
EXPECT_CERT_MD5="${NHSY2_CERT_MD5:-bbe1da8584fb2fb1017a512b2e99ff21}"
SMALI_SRC="$SCRIPT_DIR/assets/nhsy2_killer_smali"
OUT="$SCRIPT_DIR/assets/nhsy2_signature_killer.dex"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$BASE_APK" ]] || { echo "ERROR: 缺少官方底包 $BASE_APK" >&2; exit 1; }
[[ -d "$SMALI_SRC" ]] || { echo "ERROR: 缺少 smali 源码 $SMALI_SRC" >&2; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: 缺少 smali 工具 $SMALI_BIN" >&2; exit 1; }
[[ -x "$BAKSMALI_BIN" ]] || { echo "ERROR: 缺少 baksmali 工具 $BAKSMALI_BIN" >&2; exit 1; }

WORK="$(mktemp -d /tmp/nhsy2_killer.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

python3 - "$BASE_APK" "$EXPECT_CERT_MD5" "$WORK/cert.b64" "$WORK/cert.der" <<'PY'
import base64, hashlib, struct, sys

apk, want_md5, b64_out, der_out = sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4]
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
md5 = hashlib.md5(der).hexdigest()
if md5 != want_md5:
    raise SystemExit('ERROR: 官方证书指纹不符: ' + md5 + ' != ' + want_md5)
b64 = base64.b64encode(der).decode()
wrapped = '\\n'.join(b64[j:j + 76] for j in range(0, len(b64), 76))
open(b64_out, 'w').write(wrapped)
open(der_out, 'wb').write(der)
print('  cert DER %d B, MD5 %s, base64 %d 字符（折行 76，共 %d 行）'
      % (len(der), md5, len(b64), len(wrapped.split('\\n'))))
PY

mkdir -p "$WORK/smali"
cp -R "$SMALI_SRC/." "$WORK/smali/"
python3 - "$WORK/smali/bin/mt/signature/KillerApplication193.smali" "$WORK/cert.b64" "$PKG" <<'PY'
import sys
from pathlib import Path

path, cert_file, pkg = sys.argv[1], sys.argv[2], sys.argv[3]
p = Path(path)
text = p.read_text(encoding='utf-8')
cert = Path(cert_file).read_text(encoding='utf-8')
if text.count('@@CERT_B64_WRAPPED@@') != 1:
    raise SystemExit('ERROR: smali 内 @@CERT_B64_WRAPPED@@ 不是恰好一处')
text = text.replace('@@CERT_B64_WRAPPED@@', cert)
if text.count('"@@PKG@@"') != 1:
    raise SystemExit('ERROR: smali 内 @@PKG@@ 不是恰好一处')
text = text.replace('"@@PKG@@"', '"' + pkg + '"')
if text.count('.super Landroid/app/Application;') != 1:
    raise SystemExit('ERROR: killer 父类不是 android.app.Application')
p.write_text(text, encoding='utf-8')
print('  smali 烘焙完成: pkg=%s cert=%d 字符' % (pkg, len(cert)))
PY

# 与 surgical [4/8] 同一份空壳挂载类（父类指向 killer，manifest android:name 指它）
cat > "$WORK/smali/bin/mt/signature/SgKillerApp.smali" <<'SMALI'
.class public Lbin/mt/signature/SgKillerApp;
.super Lbin/mt/signature/KillerApplication193;

.method public constructor <init>()V
    .registers 1
    invoke-direct {p0}, Lbin/mt/signature/KillerApplication193;-><init>()V
    return-void
.end method
SMALI

"$SMALI_BIN" assemble -o "$OUT" "$WORK/smali"
MD5="$(md5 -q "$OUT")"
SIZE="$(stat -f '%z' "$OUT")"

rm -rf "$WORK/verify"
"$BAKSMALI_BIN" d -o "$WORK/verify" "$OUT" >/dev/null
python3 - "$WORK/verify" "$PKG" "$EXPECT_CERT_MD5" <<'PY'
import base64, hashlib, io, os, re, sys
root, pkg, want = sys.argv[1], sys.argv[2], sys.argv[3]
k = os.path.join(root, 'bin/mt/signature/KillerApplication193.smali')
s = os.path.join(root, 'bin/mt/signature/SgKillerApp.smali')
t = io.open(k, encoding='utf-8').read()
assert re.search(r'(?m)^\.super Landroid/app/Application;$', t), '回读失败: 父类不是 android.app.Application'
assert ('const-string v1, "%s"' % pkg) in t, '回读失败: 包名不是 %s' % pkg
assert os.path.exists(s), '回读失败: 缺 SgKillerApp'
st = io.open(s, encoding='utf-8').read()
assert re.search(r'(?m)^\.super Lbin/mt/signature/KillerApplication193;$', st), '回读失败: SgKillerApp 父类不对'
m = re.search(r'const-string v0, "([A-Za-z0-9+/=\\n]+)"', t)
assert m, '回读失败: 找不到证书 base64 串'
b64 = m.group(1).replace('\\n', '')
der = base64.b64decode(b64)
md5 = hashlib.md5(der).hexdigest()
assert md5 == want, '回读失败: 证书 DER md5 %s != %s' % (md5, want)
assert '@@' not in t, '回读失败: 残留 @@ 占位符'
print('  [回读1/3] 父类 = android.app.Application ✓')
print('  [回读2/3] 包名 = %s ✓' % pkg)
print('  [回读3/3] SgKillerApp 存在（父类 KillerApplication193）✓')
print('  [证书] base64 %d 字符 → DER %d B md5 %s ✓' % (len(b64), len(der), md5))
PY

echo "[killer] ok $OUT ($SIZE B, md5 $MD5)"
