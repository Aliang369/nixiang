#!/usr/bin/env bash
# 旷野征途 (kyzt) — 从「参数化 smali 源码」重建签名杀手 dex（B 轨运行时签名替换用）。
#
# 用途：3.2 起游戏新增 C# AOT 反篡改 ET.AndroidPackageIntegrity（VerifyInstalledPackageAsync /
#       VerifyCurrentSigner / VerifyNativeLibraries），比对 base64(SHA256(DER)) 全等 ⇒ 必须让
#       Java 侧 PackageManager 返回官方 DER。本脚本重建那个 killer dex（注入为 classes9.dex，
#       序号在框架 dex 之后连续）。
#
# 为什么不是直接入库 dex：仓库 .gitignore 排除 *.dex（Git 资产=文本），故 killer 以可复现的
# 参数化 smali 源码入库（assets/kyzt_killer_smali，MT ApkSignatureKillerEx 形态），本脚本在
# 构建机上烘焙「包名 + 官方证书 DER」后汇编成 dex。
#
# 配方来源：04_scripts/墨守孤城/msgc_build_killer_dex.sh（同源参数化 smali；3.2 首版在
#           13_scratch/kyzt32_killer/rebuild_kyzt_killer.sh 内跑通，本条已提升入库）。
#
# 前置条件：
#   1. 官方底包在位：02_apks/旷野征途/旷野征途_3.2_taptap_official.apk（可用 KYZT_BASE_APK 覆盖）
#      ——必须与装机底包同版，否则 #6155/DER 对应关系错、反篡改仍会失败。
#   2. smali 工具：01_tools/smali/smali-fat.jar + baksmali-fat.jar
#   3. JDK 17（JAVA_HOME 或 NIXIANG_JAVA17 指向），python3 可执行
#
# 产物：13_scratch/kyzt32_killer/kyzt_signature_killer.dex（**只写 13_scratch**，不碰 04_scripts/08_output）
#       2026-11-24 实测 13,696 B，内嵌 DER 678 B（sha256 0a3b7825…048f）
# 用法：
#   bash 04_scripts/旷野征途/rebuild_killer_dex_kyzt.sh
#   KYZT_BASE_APK=/path/to/base.apk bash 04_scripts/旷野征途/rebuild_killer_dex_kyzt.sh
#   出货时把它交给组包脚本：KYZT_KILLER_DEX=<产物> bash 04_scripts/旷野征途/surgical_kyzt_3.2.sh
# 门禁：官方证书 SHA-256 不匹配即退出（防止把错的 DER 烘进 killer）。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"

BASE_APK="${KYZT_BASE_APK:-$NIXIANG/02_apks/旷野征途/旷野征途_3.2_taptap_official.apk}"
PKG="com.xf.kyzt"
# 官方证书 DER 的 SHA-256（3.2 底包 apksigner verify --print-certs 实测）
CERT_SHA256_EXPECT="0a3b782591c7895437472a4768a4754e6487fd27cbe1887697ebc38de41a048f"

SMALI_SRC="$SCRIPT_DIR/assets/kyzt_killer_smali"
SMALI_JAR="$NIXIANG/01_tools/smali/smali-fat.jar"
BAKSMALI_JAR="$NIXIANG/01_tools/smali/baksmali-fat.jar"
JAVA="${NIXIANG_JAVA17:-${JAVA_HOME:-/c/Program Files/Microsoft/jdk-17.0.20.101-hotspot}}/bin/java"

# 输出目录固定留在 13_scratch（可重建工作区，不入库）
OUTDIR="$NIXIANG/13_scratch/kyzt32_killer"
OUT="$OUTDIR/kyzt_signature_killer.dex"

[[ -f "$BASE_APK" ]] || { echo "ERROR: 缺少官方底包 $BASE_APK" >&2; exit 1; }
[[ -d "$SMALI_SRC/mt/signature" ]] || { echo "ERROR: 缺少 smali 源码 $SMALI_SRC/mt/signature" >&2; exit 1; }
[[ -f "$SMALI_JAR" ]] || { echo "ERROR: 缺少 smali 工具 $SMALI_JAR" >&2; exit 1; }
[[ -x "$JAVA" ]] || { echo "ERROR: 找不到 JDK17 java（JAVA_HOME / NIXIANG_JAVA17）: $JAVA" >&2; exit 1; }

mkdir -p "$OUTDIR/logs"

# [1/3] 从 APK Sig Block v2 取官方证书 DER，校验指纹后写 base64
echo "[1/3] 取官方证书 DER（校验 sha256）"
python3 - "$BASE_APK" "$OUTDIR/logs/cert.b64" "$CERT_SHA256_EXPECT" "$OUTDIR/logs/cert.wrapped" <<'PY'
import base64, hashlib, struct, sys
from pathlib import Path

apk, out_raw, want, out_wrapped = sys.argv[1:5]
data = open(apk, 'rb').read()
i = data.rfind(b'APK Sig Block 42')
if i < 0:
    raise SystemExit('找不到 APK Sig Block 42（底包未做 v2/v3 签名？）')
size = struct.unpack('<Q', data[i-8:i])[0]
block = data[i+16-size-8:i+16]
pos, end, pairs = 8, len(block) - 24, {}
while pos < end:
    plen = struct.unpack('<Q', block[pos:pos+8])[0]
    pid = struct.unpack('<I', block[pos+8:pos+12])[0]
    pairs[pid] = block[pos+12:pos+8+plen]
    pos += 8 + plen

def lp(b):
    n = struct.unpack('<I', b[:4])[0]
    return b[4:4+n], b[4+n:]

v2 = pairs.get(0x7109871a) or pairs.get(0xf05368c0)
signers, _ = lp(v2)
signer, _ = lp(signers)
signed, _ = lp(signer)
_, rest = lp(signed)
certs, _ = lp(rest)
der, _ = lp(certs)
sha = hashlib.sha256(der).hexdigest()
if sha != want:
    raise SystemExit('证书指纹不符: %s（期望 %s）' % (sha, want))
b64 = base64.b64encode(der).decode()
Path(out_raw).write_text(b64, encoding='utf-8')
# smali 侧用续行拼长字符串，每行 64 字符
Path(out_wrapped).write_text((chr(92) + 'n').join(b64[j:j+64] for j in range(0, len(b64), 64)) + (chr(92) + 'n'), encoding='utf-8')
print('      cert DER %d B  sha256 %s' % (len(der), sha))
PY

# [2/3] 烘焙包名 + 证书进 smali
echo "[2/3] 烘焙 smali（pkg=${PKG}）"
rm -rf "$OUTDIR/work"
mkdir -p "$OUTDIR/work/smali/bin"
cp -R "$SMALI_SRC/." "$OUTDIR/work/smali/bin/"
python3 - "$OUTDIR/work/smali/bin/mt/signature/KillerApplication193.smali" "$OUTDIR/logs/cert.wrapped" "$PKG" <<'PY'
import sys
from pathlib import Path

p = Path(sys.argv[1])
cert = Path(sys.argv[2]).read_text(encoding='utf-8')
pkg = sys.argv[3]
t = p.read_text(encoding='utf-8')
if '@@CERT_B64_WRAPPED@@' not in t:
    raise SystemExit('ERROR: smali 内无 @@CERT_B64_WRAPPED@@ 占位符')
if '"@@PKG@@"' not in t:
    raise SystemExit('ERROR: smali 内无 @@PKG@@ 占位符')
p.write_text(t.replace('@@CERT_B64_WRAPPED@@', cert).replace('"@@PKG@@"', '"' + pkg + '"'), encoding='utf-8')
print('      smali 烘焙完成: pkg=%s cert=%d 字符' % (pkg, len(cert)))
PY

# [3/3] 汇编成 dex + 打印指纹
echo "[3/3] 汇编 dex"
"$JAVA" -jar "$SMALI_JAR" assemble -o "$OUT" "$OUTDIR/work/smali"
"$JAVA" -jar "$BAKSMALI_JAR" list classes "$OUT" | sed -n '1,20p'
stat -c '  %s B  %n' "$OUT"
sha256sum "$OUT"
md5sum "$OUT"
echo
echo "完成。装机版组包：KYZT_KILLER_DEX=$OUT bash 04_scripts/旷野征途/surgical_kyzt_3.2.sh"
