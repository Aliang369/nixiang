#!/usr/bin/env bash
# 勇士之心 1.0.6 — 从「参数化 smali 源码」重建签名杀手 dex（B 轨运行时签名替换用）
# 来源：04_scripts/我在神界刷装备/build_killer_dex_sjzz.sh（同一 MT ApkSignatureKillerEx 配方，
#       只换包名/证书/输出目录；与 04_scripts/不一样西游/build_killer_dex_byyxy.sh 的差异同样只有这三项）。
#
# 为什么不是直接入库 dex：仓库 .gitignore 排除 *.dex（Git 资产=文本），killer 以可复现 smali 源码
# 入库（assets/ysxz_killer_smali，MT ApkSignatureKillerEx 形态），本脚本烘焙「包名 + 官方证书 DER」后重组。
#
# 为什么需要它：libsecret.so 用「内嵌常量 XOR f(运行时签名,包名)」派生引擎解密密钥
# （见 10_docs/勇士之心/_work_20261123_壳可行性.md E5/E6）。改签后 PackageManager 返回星空证书
# ⇒ key unwrap failed ⇒ refuse empty mlib ⇒ 游戏起不来。本 dex 把 PackageInfo.CREATOR 包一层，
# 对目标包名回灌官方证书 ⇒ 之后任何 PackageManager 查询都拿到官方签名值。
#
# 产物：08_output/勇士之心/killer/ysxz_signature_killer.dex
# 用法：bash 04_scripts/勇士之心/build_killer_dex_ysxz.sh
# 退出码：0 成功；1 前置/断言失败；2 证书指纹不符
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
GAME="勇士之心"

PKG="com.fightingstars.games.warrior"
BASE_APK="${YSXZ_BASE_APK:-$NIXIANG/02_apks/${GAME}_1.0.6_official.apk}"
# 官方证书 DER 的 SHA-256（2026-11-23 实测：apksigner --print-certs 与 v2 签名块解析双路一致）
CERT_SHA256="${YSXZ_CERT_SHA256:-de8a75d30a1831f3ddc83f38f285513110fde9ab919fb4e471653f86b5083e0a}"
CERT_DIR="$SCRIPT_DIR/assets/ysxz_cert"
CERT_DER="$CERT_DIR/ysxz_official_cert.der"
SMALI_SRC="$SCRIPT_DIR/assets/ysxz_killer_smali"
OUT_DIR="$NIXIANG/08_output/$GAME/killer"
OUT="$OUT_DIR/ysxz_signature_killer.dex"
# 逐字节 md5 在 Windows 上不稳定（smali 写 dex 时类/字符串次序不固定，实测 c60b7d03…
# 与 f39d2033… 两值交替），所以门禁用「内容指纹」：烘焙后的 smali 树（含包名+证书 base64）
# 排序后逐文件 sha256 再摘要。EXPECT_MD5 保留为可选（mac 上可逐字节复现时用）。
EXPECT_FP="${YSXZ_KILLER_FP:-a86dead85b5a724d3ef3c64099d24bd83af962b54ba55657be52db473b4e2a5b}"
EXPECT_MD5="${YSXZ_KILLER_MD5:-}"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
# framework 侧触发点（只读核对，不改）
FACTORY_JAVA="$NIXIANG/mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/StarryAppComponentFactory.java"
BOOTSTRAP_JAVA="$NIXIANG/mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/SignatureKillerBootstrap.java"
KILLER_CLASS="bin.mt.signature.KillerApplication193"

# mac 靠 /usr/libexec/java_home；win 上没有该命令，回落到已有 JAVA_HOME 或 PATH 里的 java
if [[ -z "${JAVA_HOME:-}" && -x /usr/libexec/java_home ]]; then
  export JAVA_HOME="$(/usr/libexec/java_home -v 17 2>/dev/null || true)"
fi
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

PY_BIN="$(command -v python3 || command -v python || true)"
[[ -n "$PY_BIN" ]] || { echo "ERROR: 找不到 python3/python" >&2; exit 1; }
command -v java >/dev/null 2>&1 || { echo "ERROR: java 不在 PATH（设 JAVA_HOME）" >&2; exit 1; }

# win（Git Bash）下 01_tools/smali 的 bash 包装可能没有可执行位 ⇒ 退回 bash 调用
smali_run()    { if [[ -x "$SMALI_BIN" ]];    then "$SMALI_BIN" "$@";    else bash "$SMALI_BIN" "$@";    fi; }
baksmali_run() { if [[ -x "$BAKSMALI_BIN" ]]; then "$BAKSMALI_BIN" "$@"; else bash "$BAKSMALI_BIN" "$@"; fi; }

md5_of()    { "$PY_BIN" -c 'import hashlib,sys;print(hashlib.md5(open(sys.argv[1],"rb").read()).hexdigest())' "$1"; }
sha256_of() { "$PY_BIN" -c 'import hashlib,sys;print(hashlib.sha256(open(sys.argv[1],"rb").read()).hexdigest())' "$1"; }
size_of()   { "$PY_BIN" -c 'import os,sys;print(os.path.getsize(sys.argv[1]))' "$1"; }

[[ -d "$SMALI_SRC/mt/signature" ]] || { echo "ERROR: 缺少 smali 源码 $SMALI_SRC" >&2; exit 1; }
[[ -f "$SMALI_BIN" ]] || { echo "ERROR: 缺少 smali 工具 $SMALI_BIN" >&2; exit 1; }

echo "[1/5] 官方证书 DER"
if [[ ! -f "$CERT_DER" ]]; then
  [[ -f "$BASE_APK" ]] || { echo "ERROR: 缺证书且缺底包 $BASE_APK" >&2; exit 1; }
  echo "  证书缺失，从底包 v2 签名块现取：$BASE_APK"
  "$PY_BIN" "$CERT_DIR/extract_v2_cert.py" --apk "$BASE_APK" --out-dir "$CERT_DIR" \
      --expect-sha256 "$CERT_SHA256" || exit $?
fi
GOT_SHA="$(sha256_of "$CERT_DER")"
if [[ "$GOT_SHA" != "$CERT_SHA256" ]]; then
  echo "ERROR: 证书 DER 指纹不符: $GOT_SHA != $CERT_SHA256" >&2
  exit 2
fi
echo "  $CERT_DER ($(size_of "$CERT_DER") B) sha256=$GOT_SHA"

echo "[2/5] framework 触发点与类名核对（只读）"
for j in "$FACTORY_JAVA" "$BOOTSTRAP_JAVA"; do
  [[ -f "$j" ]] || { echo "WARN: 缺 framework 源码 ${j}（跳过该项核对）" >&2; continue; }
  grep -q "\"$KILLER_CLASS\"" "$j" || {
    echo "ERROR: $j 里的 KILLER_CLASS 不是 $KILLER_CLASS —— 两侧类名必须逐字相同" >&2; exit 1; }
  echo "  ok $(basename "$j"): KILLER_CLASS=$KILLER_CLASS"
done

echo "[3/5] 烘焙包名 + 证书 base64"
WORK="$(mktemp -d "${TMPDIR:-/tmp}/ysxz_killer.XXXXXX")"
trap 'rm -rf "$WORK"' EXIT
mkdir -p "$WORK/smali/bin"
cp -R "$SMALI_SRC/." "$WORK/smali/bin/"
"$PY_BIN" - "$WORK/smali/bin/mt/signature/KillerApplication193.smali" "$CERT_DER" "$PKG" <<'PY'
import base64, sys
from pathlib import Path

path, der_file, pkg = sys.argv[1], sys.argv[2], sys.argv[3]
p = Path(path)
text = p.read_text(encoding='utf-8')
b64 = base64.b64encode(Path(der_file).read_bytes()).decode()
# 注意：smali 字符串字面量里换行必须写成两个字符 \n（转义序列），不能是真换行
cert = '\\n'.join(b64[j:j + 64] for j in range(0, len(b64), 64)) + '\\n'
if '@@CERT_B64_WRAPPED@@' not in text:
    raise SystemExit('ERROR: smali 内无 @@CERT_B64_WRAPPED@@ 占位符')
text = text.replace('@@CERT_B64_WRAPPED@@', cert)
if '"@@PKG@@"' not in text:
    raise SystemExit('ERROR: smali 内无 @@PKG@@ 占位符')
text = text.replace('"@@PKG@@"', '"' + pkg + '"')
p.write_text(text, encoding='utf-8')
print('  smali 烘焙完成: pkg=%s cert=%d 字符' % (pkg, len(cert)))
PY

echo "[4/5] smali assemble -> $OUT"
mkdir -p "$OUT_DIR"
smali_run assemble -o "$OUT" "$WORK/smali"

echo "[5/5] 回读断言（baksmali 反查产物）"
rm -rf "$WORK/verify"
baksmali_run d -o "$WORK/verify" "$OUT" >/dev/null
V="$WORK/verify/bin/mt/signature/KillerApplication193.smali"
[[ -f "$V" ]] || { echo "ERROR: 产物里没有 ${KILLER_CLASS}（smali 根目录进错才会这样）" >&2; exit 1; }
grep -q '^\.super Landroid/app/Application;' "$V" || { echo "ERROR: killer 父类不是 Application" >&2; exit 1; }
grep -q "const-string v1, \"$PKG\"" "$V" || { echo "ERROR: killer 包名未烘焙" >&2; exit 1; }
if grep -q '@@' "$V"; then echo "ERROR: 产物残留 @@ 占位符" >&2; exit 1; fi
# 证书 base64 首 64 字符必须原样出现在产物里（证明 DER 真的进了 dex，不是空串）
HEAD64="$("$PY_BIN" -c 'import base64,sys;print(base64.b64encode(open(sys.argv[1],"rb").read()).decode()[:64])' "$CERT_DER")"
grep -qF "$HEAD64" "$V" || { echo "ERROR: 产物里没有官方证书 DER（首块 $HEAD64 未命中）" >&2; exit 1; }
grep -q 'killPM' "$V" || { echo "ERROR: 产物里没有 killPM（不是 killer 本体）" >&2; exit 1; }
grep -qF 'KillerApplication193$l;' "$V" || { echo "ERROR: killPM 未引用 Creator 包装类 l" >&2; exit 1; }
# API28+ 走 SigningInfo.getApkContentsSigners —— 本作 libsecret.so 的首选路径，必须存在
L="$WORK/verify/bin/mt/signature/KillerApplication193\$l.smali"
[[ -f "$L" ]] || { echo "ERROR: 产物里没有 Creator 包装类 l（signatures 不会被改写）" >&2; exit 1; }
grep -q 'SigningInfo' "$L" || { echo "ERROR: l 类里没有 SigningInfo 分支（API28+ getApkContentsSigners 不会被改写）" >&2; exit 1; }
grep -q 'PackageInfo;->signatures' "$L" || { echo "ERROR: l 类里没有 signatures 数组改写" >&2; exit 1; }

FP="$(cd "$WORK/smali" && "$PY_BIN" -c '
import hashlib
from pathlib import Path
items = sorted((q.as_posix(), hashlib.sha256(q.read_bytes()).hexdigest())
               for q in Path(".").rglob("*.smali"))
h = hashlib.sha256()
for name, sha in items:
    h.update(("%s %s\\n" % (name, sha)).encode())
print(h.hexdigest())
')"
if [[ -n "$EXPECT_FP" && "$FP" != "$EXPECT_FP" ]]; then
  echo "ERROR: killer 内容指纹不符: $FP != ${EXPECT_FP}（包名/证书/源码之一被改）" >&2; exit 1
fi

MD5="$(md5_of "$OUT")"
SHA="$(sha256_of "$OUT")"
SIZE="$(size_of "$OUT")"
[[ "$SIZE" -gt 8000 ]] || { echo "ERROR: 产物体积异常 $SIZE B" >&2; exit 1; }
if [[ -n "$EXPECT_MD5" && "$MD5" != "$EXPECT_MD5" ]]; then
  echo "ERROR: killer dex md5 与记录不符: $MD5 != $EXPECT_MD5" >&2; exit 1
fi
echo "[killer] ok $OUT"
echo "  size        = $SIZE B"
echo "  content fp  = $FP   (门禁项：烘焙后 smali 树排序摘要，跨机稳定)"
echo "  md5         = $MD5   (逐字节；Windows 上可能两值交替，不作门禁)"
echo "  sha256      = $SHA"
echo "  pkg         = $PKG"
echo "  cert sha256 = $CERT_SHA256"
echo "  复现参数    = YSXZ_CERT_SHA256=$CERT_SHA256 YSXZ_KILLER_FP=$FP"
