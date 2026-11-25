#!/usr/bin/env bash
# ================================================================
#  endfield_killer_dex_1.7.2.sh
#  终末地 1.7.2（com.hypergryph.endfield，vc71）— C 轨「运行时证书回灌」killer 烘焙
#
#  决策与证据：10_docs/终末地/交付_1.7.2_20261126.md §二 第 3 项
#   · A 轨（静态改官方 classes2/3/5）实测冷启 ~2s 必死（SIGBUS→SIGSEGV、无 tombstone）；
#   · B 轨（改 ACE 壳 AceApplication 的 .super）必须动 classes.dex ⇒ 同样出局；
#   · C 轨 = 官方 6 份 dex 逐字节不动，killer 只进**我们自己**新增的 classes7。
#
#  产物语义：killer 把 PackageInfo.CREATOR 换成「只对本包回灌官方证书」的包装器，
#    并把 PackageManager / Parcel 的 creator 缓存清掉 ⇒ 进程内任何走 PackageManager
#    的签名查询（TapSignKit.getSign、数美、gameupdate.PackageUtils）都拿到官方值。
#    安装动作**全在 <clinit>** ⇒ 「类被初始化」即「已装好」，不需要 Application 实例，
#    也没有 install() 之类的静态方法可用（这是既有 15 款 killer 的共同形态，见文档 §2）。
#
#  触发点不在本脚本：classes7 里内联的 StarryInitProvider.onCreate 会调
#    SignatureKillerBootstrap.install() → Class.forName("bin.mt.signature.KillerApplication193")；
#    该 Provider 在 manifest 里排第 1，早于 TapTapKitInitProvider（第 12，classes2）。
#
#  用法：
#    bash mod菜单/starry-mod-menu/scripts/endfield_killer_dex_1.7.2.sh
#    bash ... --merge-into <classes7 的 smali 根>     # 烘焙并并入框架 dex 源树
#    bash ... --verify <某个 classesN.dex>            # 只校验已产出/已合并的 dex
#    bash ... --cert-der <cert.der>                   # 不给底包，直接给官方 DER
#    bash ... --out <路径.dex> --keep-work            # 改输出位 / 留工作目录
#
#  退出码：0 = ok；1 = 失败；2 = 用法/环境错
# ================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
NIXIANG="${NIXIANG:-$(cd "${SCRIPT_DIR}/../../.." && pwd)}"

# 目标包名：只对本包回灌（killer 的 killPM 只匹配这一个包名）
PKG="com.hypergryph.endfield"
# 官方证书 DER md5（1.7.2 底包实测；从 v2 签名块与 META-INF/BEYOND.RSA 两条路都得此值）
EXPECT_MD5="1cf808541a17bf218d3dd5a30787e485"
BASE_APK="$NIXIANG/02_apks/终末地/终末地_1.7.2_taptap_official.apk"
SMALI_SRC="$NIXIANG/04_scripts/终末地/assets/endfield_killer_smali"
OUT="$NIXIANG/08_output/终末地/killer/endfield_signature_killer.dex"
CERT_DER=""
MERGE_INTO=""
VERIFY_DEX=""
KEEP_WORK=0

usage() { sed -n "2,29p" "${BASH_SOURCE[0]}" | sed "s/^# \{0,1\}//"; }

while [ $# -gt 0 ]; do
  case "$1" in
    --pkg) PKG="$2"; shift 2 ;;
    --base-apk) BASE_APK="$2"; shift 2 ;;
    --cert-der) CERT_DER="$2"; shift 2 ;;
    --out) OUT="$2"; shift 2 ;;
    --smali-src) SMALI_SRC="$2"; shift 2 ;;
    --merge-into) MERGE_INTO="$2"; shift 2 ;;
    --verify) VERIFY_DEX="$2"; shift 2 ;;
    --expect-cert-md5) EXPECT_MD5="$2"; shift 2 ;;
    --keep-work) KEEP_WORK=1; shift ;;
    -h|--help) usage; exit 0 ;;
    *) echo "ERROR: 未知参数 $1（--help 看用法）" >&2; exit 2 ;;
  esac
done

SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
PY="${PYTHON:-python3}"
export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

die() { echo "ERROR: $*" >&2; exit 1; }

command -v "$PY" >/dev/null 2>&1 || die "缺 $PY"
[ -x "$SMALI_BIN" ] || die "缺 smali 工具 ${SMALI_BIN}（01_tools/smali）"
[ -x "$BAKSMALI_BIN" ] || die "缺 baksmali 工具 ${BAKSMALI_BIN}（01_tools/smali）"

WORK="$(mktemp -d "${TMPDIR:-/tmp}/ef_killer.XXXXXX")"
cleanup() {
  if [ "$KEEP_WORK" = "1" ]; then echo "  work 保留: $WORK"; else rm -rf "$WORK"; fi
}
trap cleanup EXIT

# ----------------------------------------------------------------
#  公共校验：给定「smali 根」（含 bin/mt/signature/）逐条断言
#    · 13 个类齐（主类 + 12 个内部类）
#    · .super 必须是 Landroid/app/Application;（跨包复用时父类没改 ⇒ ClassNotFound 静默 skip）
#    · 包名已烘焙（killPM 只对该包名生效）
#    · 证书 Base64 解码后 md5 == 期望值（抄错一位就复现 1001「包名、签名错误」且极难定位）
#    · 无 @@ 占位符残留
# ----------------------------------------------------------------
check_smali_dir() {
  local dir="$1"
  [ -d "$dir/bin/mt/signature" ] || die "smali 根缺 bin/mt/signature：$dir"
  "$PY" - "$dir" "$PKG" "$EXPECT_MD5" <<'PY'
import base64, hashlib, os, sys

root, pkg, want_md5 = sys.argv[1], sys.argv[2], sys.argv[3]
d = os.path.join(root, "bin", "mt", "signature")
NAMES = ["KillerApplication193"] + ["KillerApplication193$" + c for c in "abcdefghijkl"]
missing = [n for n in NAMES if not os.path.isfile(os.path.join(d, n + ".smali"))]
if missing:
    raise SystemExit("ERROR: killer 类不全，缺: " + ", ".join(missing))

main = open(os.path.join(d, "KillerApplication193.smali"), encoding="utf-8").read()
if "@@" in main:
    raise SystemExit("ERROR: 产物残留 @@ 占位符（证书/包名未烘焙）")
supers = [ln.strip() for ln in main.splitlines() if ln.startswith(".super ")]
if not supers or supers[0] != ".super Landroid/app/Application;":
    raise SystemExit("ERROR: killer 父类不是 Application: %s" % supers[:2])
if (chr(34) + pkg + chr(34)) not in main:
    raise SystemExit("ERROR: 包名未烘焙（缺 string 常量 %s）" % pkg)

q = chr(34)
cls = [ln for ln in main.splitlines() if ln.strip().startswith("const-string")]
if not cls:
    raise SystemExit("ERROR: <clinit> 里找不到证书 const-string")
lit = cls[0]
b64 = lit[lit.index(q) + 1:lit.rindex(q)].replace(chr(92) + "n", "").replace(chr(92) + "r", "").replace(" ", "")
try:
    der = base64.b64decode(b64, validate=True)
except Exception as e:
    raise SystemExit("ERROR: 证书 Base64 无法解码: %s" % e)
got = hashlib.md5(der).hexdigest()
if want_md5 and got != want_md5:
    raise SystemExit("ERROR: 证书指纹不符: %s != %s（DER %d B）" % (got, want_md5, len(der)))
print("  [ok] 13 类齐 / .super=Application / pkg=%s / cert DER %d B md5=%s" % (pkg, len(der), got))
PY
}

# ----------------------------------------------------------------
#  --verify 模式：反汇编某个已产出的 dex，再走同一套断言
# ----------------------------------------------------------------
if [ -n "$VERIFY_DEX" ]; then
  [ -f "$VERIFY_DEX" ] || die "verify 目标不存在: $VERIFY_DEX"
  echo "[verify] $VERIFY_DEX"
  "$BAKSMALI_BIN" disassemble "$VERIFY_DEX" -o "$WORK/verify" >/dev/null 2>&1 \
    || die "dex 无法被 baksmali 解析（dex 本地自检失败）: $VERIFY_DEX"
  check_smali_dir "$WORK/verify"
  echo "[verify] PASS（dex 可解析 + 类/包名/证书三项均正确）"
  exit 0
fi

echo "[1/4] 官方证书 DER"
if [ -n "$CERT_DER" ]; then
  [ -f "$CERT_DER" ] || die "证书 DER 不存在: $CERT_DER"
  cp "$CERT_DER" "$WORK/cert.der"
  "$PY" - "$WORK/cert.der" "$WORK/cert.b64" "$EXPECT_MD5" <<'PY'
import base64, hashlib, sys

der_path, b64_path, want_md5 = sys.argv[1], sys.argv[2], sys.argv[3]
der = open(der_path, "rb").read()
if not der[:1] == b"0":
    raise SystemExit("ERROR: 不是 DER 编码的证书（首字节应为 0x30）")
md5 = hashlib.md5(der).hexdigest()
if want_md5 and md5 != want_md5:
    raise SystemExit("ERROR: 证书指纹不符: %s != %s" % (md5, want_md5))
b64 = base64.b64encode(der).decode()
open(b64_path, "w").write("\\n".join(b64[j:j + 64] for j in range(0, len(b64), 64)) + "\\n")
print("  cert: %d B DER, md5 %s, base64 %d 字符" % (len(der), md5, len(b64)))
PY
else
  [ -f "$BASE_APK" ] || die "底包不存在: ${BASE_APK}（或用 --cert-der 直接给 DER）"
  "$PY" - "$BASE_APK" "$WORK/cert.der" "$WORK/cert.b64" "$EXPECT_MD5" <<'PY'
import base64, hashlib, struct, sys

apk, der_path, b64_path, want_md5 = sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4]
data = open(apk, "rb").read()
magic = b"APK Sig Block 42"
i = data.rfind(magic)
if i <= 0:
    raise SystemExit("ERROR: 底包无 APK Signing Block（v2/v3）")
size = struct.unpack("<Q", data[i - 8:i])[0]
block = data[i + 16 - size - 8:i + 16]
pos, end, pairs = 8, len(block) - 24, {}
while pos < end:
    plen = struct.unpack("<Q", block[pos:pos + 8])[0]
    pid = struct.unpack("<I", block[pos + 8:pos + 12])[0]
    pairs[pid] = block[pos + 12:pos + 8 + plen]
    pos += 8 + plen

def lp(buf):
    n = struct.unpack("<I", buf[:4])[0]
    return buf[4:4 + n], buf[4 + n:]

v2 = pairs.get(0x7109871a) or pairs.get(0xf05368c0)
if not v2:
    raise SystemExit("ERROR: 底包无 v2/v3 签名块")
signers, _ = lp(v2)
signer, _ = lp(signers)
signed_data, _ = lp(signer)
_digests, rest = lp(signed_data)
certs, _ = lp(rest)
der, _ = lp(certs)
md5 = hashlib.md5(der).hexdigest()
if want_md5 and md5 != want_md5:
    raise SystemExit("ERROR: 官方证书指纹不符: %s != %s" % (md5, want_md5))
open(der_path, "wb").write(der)
b64 = base64.b64encode(der).decode()
open(b64_path, "w").write("\\n".join(b64[j:j + 64] for j in range(0, len(b64), 64)) + "\\n")
print("  cert: %d B DER, md5 %s, base64 %d 字符" % (len(der), md5, len(b64)))
PY
fi

echo "[2/4] 烘焙 smali（pkg=${PKG}）"
[ -d "$SMALI_SRC/mt/signature" ] || die "killer smali 模板缺失: $SMALI_SRC/mt/signature"
mkdir -p "$WORK/smali/bin"
cp -R "$SMALI_SRC/." "$WORK/smali/bin/"
"$PY" - "$WORK/smali/bin/mt/signature/KillerApplication193.smali" "$WORK/cert.b64" "$PKG" <<'PY'
import sys
from pathlib import Path

path, cert_file, pkg = sys.argv[1], sys.argv[2], sys.argv[3]
p = Path(path)
text = p.read_text(encoding="utf-8")
cert = Path(cert_file).read_text(encoding="utf-8")
if "@@CERT_B64_WRAPPED@@" not in text:
    raise SystemExit("ERROR: smali 内无 @@CERT_B64_WRAPPED@@ 占位符")
text = text.replace("@@CERT_B64_WRAPPED@@", cert)
if chr(34) + "@@PKG@@" + chr(34) not in text:
    raise SystemExit("ERROR: smali 内无 @@PKG@@ 占位符")
text = text.replace(chr(34) + "@@PKG@@" + chr(34), chr(34) + pkg + chr(34))
p.write_text(text, encoding="utf-8")
print("  smali 烘焙完成: pkg=%s cert=%d 字符" % (pkg, len(cert)))
PY
check_smali_dir "$WORK/smali"

echo "[3/4] assemble → $OUT"
mkdir -p "$(dirname "$OUT")"
"$SMALI_BIN" assemble -o "$OUT" "$WORK/smali"
echo "  dex $(stat -f %z "$OUT") B, md5 $(md5 -q "$OUT")"

echo "[4/4] 产物 dex 自检（反汇编回读 + 同一套断言）"
"$BAKSMALI_BIN" disassemble "$OUT" -o "$WORK/verify" >/dev/null 2>&1 \
  || die "产物 dex 无法被 baksmali 解析"
check_smali_dir "$WORK/verify"
"$BAKSMALI_BIN" list classes "$OUT" > "$WORK/classes.txt" 2>/dev/null || true
grep -q "^Lbin/mt/signature/KillerApplication193" "$WORK/classes.txt" \
  || die "产物 dex 类表里没有 bin/mt/signature/KillerApplication193"

if [ -n "$MERGE_INTO" ]; then
  echo "[merge] 并入 smali 根: $MERGE_INTO"
  mkdir -p "$MERGE_INTO"
  cp -R "$WORK/smali/bin" "$MERGE_INTO/"
  [ -f "$MERGE_INTO/bin/mt/signature/KillerApplication193.smali" ] \
    || die "合并后目标缺 KillerApplication193.smali"
  echo "  [ok] 已并入（smali assemble 该根即得到含 killer 的 classes7）"
fi

echo "[endfield_killer_dex] ok  pkg=$PKG  dex=$OUT"
echo "  装机后判据：files/sigkiller.txt 首行应为 ok bin.mt.signature.KillerApplication193 t=…"
echo "  并且 logcat 不得出现 TapTap 1001「包名、签名错误」"
