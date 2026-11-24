#!/usr/bin/env bash
# 我的勇者 7.6.5 (com.rsg.wdyz) — 从「参数化 smali 源码」重建签名 Killer dex。
# 配方来源：04_scripts/追迹迷城/build_killer_dex_zjmc.sh（同一 MT ApkSignatureKillerEx 形态，
#   只改包名 / 官方证书 / 底包）。用途：A 轨运行时签名替换——TapSDK 取到的是星光证书，
#   由本 dex 的 bin.mt.signature.KillerApplication193 回灌官方证书 DER。
#
# 为什么不是直接入库 dex：仓库 .gitignore 把 *.dex 排除（Git 资产=文本），故 killer 以
#   可复现的 smali 源码入库（assets/wdyz_killer_smali/mt/signature/*.smali，13 个类），
#   本脚本在构建机上烘焙「包名 + 官方证书」后重组 dex。
#
# 触发方：com.starry.magic.StarryInitProvider（StarryInitProvider.java:79）
#   → com.starry.magic.sign.SignatureKillerBootstrap.install()
#   → Class.forName("bin.mt.signature.KillerApplication193") 触发 <clinit>（killPM + killOpen）。
#
# 产物：assets/wdyz_signature_killer.dex（surgical_wdyz_7.6.5.sh 以 classes3.dex 装进 APK）
# 门禁：产物 md5 必须等于 EXPECT_MD5（同源 smali + 同证书 ⇒ 逐字节可复现）
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
BASE_APK="${WDYZ_BASE_APK:-$NIXIANG/02_apks/我的勇者/我的勇者_7.6.5_base.apk}"
PKG="com.rsg.wdyz"
# 官方 CN=rn 证书 DER（从底包 META-INF/RN.RSA 导出，865 B）
CERT_MD5="60908df93a89b1130e265b448bd944bc"
CERT_SHA256="ce7afa489c2227258642e42a903e99a4824620f4f9ccf127b372b0919dfa6375"
CERT_SIZE=865
SMALI_SRC="$SCRIPT_DIR/assets/wdyz_killer_smali"
OUT="$SCRIPT_DIR/assets/wdyz_signature_killer.dex"
EXPECT_MD5="${WDYZ_KILLER_MD5-91895287b09e4a8ad86d3a36321eddba}"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$BASE_APK" ]] || { echo "ERROR: 缺少官方底包 $BASE_APK" >&2; exit 1; }
[[ -d "$SMALI_SRC" ]] || { echo "ERROR: 缺少 smali 源码 $SMALI_SRC" >&2; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: 缺少 smali 工具 $SMALI_BIN" >&2; exit 1; }
[[ -x "$BAKSMALI_BIN" ]] || { echo "ERROR: 缺少 baksmali 工具 $BAKSMALI_BIN" >&2; exit 1; }

WORK="$(mktemp -d /tmp/wdyz_killer.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

echo "[1/4] 从底包 META-INF/RN.RSA 导出官方证书 DER 并自验"
# 底包 1.4 GB，只抽 META-INF/RN.RSA（PKCS7，1330 B），再用 openssl 解出证书。
unzip -p "$BASE_APK" META-INF/RN.RSA > "$WORK/RN.RSA"
openssl pkcs7 -inform DER -in "$WORK/RN.RSA" -print_certs -out "$WORK/cert.pem"
N_CERT="$(grep -c 'BEGIN CERTIFICATE' "$WORK/cert.pem" || true)"
[[ "${N_CERT:-0}" -eq 1 ]] || { echo "ERROR: RN.RSA 内证书数 $N_CERT != 1" >&2; exit 1; }
openssl x509 -in "$WORK/cert.pem" -outform DER -out "$WORK/cert.der"
GOT_SIZE="$(stat -f '%z' "$WORK/cert.der")"
GOT_MD5="$(md5 -q "$WORK/cert.der")"
GOT_SHA="$(shasum -a 256 "$WORK/cert.der" | awk '{print $1}')"
[[ "$GOT_SIZE" == "$CERT_SIZE" ]] || { echo "ERROR: 官方证书大小 $GOT_SIZE != $CERT_SIZE" >&2; exit 1; }
[[ "$GOT_MD5" == "$CERT_MD5" ]] || { echo "ERROR: 官方证书 DER md5 不符: $GOT_MD5 != $CERT_MD5" >&2; exit 1; }
[[ "$GOT_SHA" == "$CERT_SHA256" ]] || { echo "ERROR: 官方证书 DER sha256 不符: $GOT_SHA != $CERT_SHA256" >&2; exit 1; }
echo "  cert DER $GOT_SIZE B, md5 $GOT_MD5, sha256 $GOT_SHA"

# base64 折 64 列：字面量 \n 由 smali 汇编还原成真换行，killer 走 Base64.decode(s, 0) 可容忍换行
python3 - "$WORK/cert.der" "$WORK/cert.b64" <<'PY'
import base64, sys
from pathlib import Path
der = Path(sys.argv[1]).read_bytes()
b64 = base64.b64encode(der).decode()
wrapped = '\\n'.join(b64[j:j + 64] for j in range(0, len(b64), 64)) + '\\n'
Path(sys.argv[2]).write_text(wrapped, encoding='utf-8')
print('  cert.b64 %d 字符（折 64 列）' % len(wrapped))
PY

echo "[2/4] 烘焙 smali（包名 $PKG + 官方证书）"
# smali 源码刻意不以 bin/ 为根目录入库（.gitignore 会整目录忽略 04_scripts/**/bin/），
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
if '@@PKG@@' in text:
    text = text.replace('"@@PKG@@"', '"' + pkg + '"')
if '"' + pkg + '"' not in text:
    raise SystemExit('ERROR: smali 内无目标包名 ' + pkg)
p.write_text(text, encoding='utf-8')
print('  smali 烘焙完成: pkg=%s cert=%d 字符' % (pkg, len(cert)))
PY

echo "[3/4] smali assemble → $OUT"
"$SMALI_BIN" assemble -o "$OUT" "$WORK/smali"
MD5="$(md5 -q "$OUT")"
SHA="$(shasum -a 256 "$OUT" | awk '{print $1}')"
SIZE="$(stat -f '%z' "$OUT")"

echo "[4/4] 反汇编 + 二进制自检"
"$BAKSMALI_BIN" d -o "$WORK/verify" "$OUT" >/dev/null
grep -q '^\.super Landroid/app/Application;' "$WORK/verify/bin/mt/signature/KillerApplication193.smali" \
  || { echo "ERROR: killer 父类不是 Application" >&2; exit 1; }
grep -q "const-string v1, \"$PKG\"" "$WORK/verify/bin/mt/signature/KillerApplication193.smali" \
  || { echo "ERROR: killer 包名未烘焙（缺 const-string ${PKG}）" >&2; exit 1; }
if grep -q '@@' "$WORK/verify/bin/mt/signature/KillerApplication193.smali"; then
  echo "ERROR: 产物残留 @@ 占位符" >&2; exit 1
fi
N_CLS="$(find "$WORK/verify/bin/mt/signature" -name '*.smali' | wc -l | tr -d ' ')"
[[ "$N_CLS" == "13" ]] || { echo "ERROR: killer 类数 $N_CLS != 13" >&2; exit 1; }
grep -a -q 'KillerApplication193' "$OUT" || { echo "ERROR: dex 内缺 KillerApplication193" >&2; exit 1; }
grep -a -q "$PKG" "$OUT" || { echo "ERROR: dex 内缺包名 $PKG" >&2; exit 1; }
if LC_ALL=C grep -a -q 'TeaMelo' "$OUT"; then echo "ERROR: dex 内残留 zjmc 包名" >&2; exit 1; fi
if grep -a -q 'Lcom/starry/magic/sign/SignSpoof;' "$OUT"; then echo "ERROR: dex 内含 SignSpoof（禁用）" >&2; exit 1; fi
# 证书烘焙断言：dex 内必须内嵌折 64 列的官方证书 b64，回读 base64 后 md5 == CERT_MD5
python3 - "$OUT" "$WORK/cert.b64" "$CERT_MD5" <<'PY'
import base64, hashlib, sys
from pathlib import Path
dex = Path(sys.argv[1]).read_bytes()
wrapped = Path(sys.argv[2]).read_text(encoding='utf-8')
want = wrapped.replace('\\n', '').replace('\n', '')   # 865 B DER ⇒ 1156 字符 b64
i = dex.find(want[:64].encode())
if i < 0:
    raise SystemExit('ERROR: dex 内未找到烘焙的证书 b64')
region = dex[i:i + len(wrapped) + 64].replace(b'\n', b'').replace(b'\r', b'')
if not region.startswith(want.encode()):
    raise SystemExit('ERROR: dex 内证书 b64 与烘焙值不一致')
der = base64.b64decode(want)
if hashlib.md5(der).hexdigest() != sys.argv[3]:
    raise SystemExit('ERROR: dex 内证书 DER md5 不符: ' + hashlib.md5(der).hexdigest())
print('  dex 内证书回读: DER %d B, md5 %s' % (len(der), hashlib.md5(der).hexdigest()))
PY

if [[ "$EXPECT_MD5" == "__PENDING__" ]]; then
  echo "ERROR: 脚本内 EXPECT_MD5 仍是占位符，请把实测 md5 回填后重跑" >&2
  echo "  实测 md5=$MD5" >&2; exit 1
fi
if [[ -n "$EXPECT_MD5" && "$MD5" != "$EXPECT_MD5" ]]; then
  echo "ERROR: killer dex md5 与记录不符: $MD5 != $EXPECT_MD5" >&2; exit 1
fi
echo "[killer] ok $OUT ($SIZE B, md5 $MD5)"
echo "  sha256=$SHA"
echo "  WDYZ_CERT_MD5=$CERT_MD5  WDYZ_KILLER_MD5=$MD5"
