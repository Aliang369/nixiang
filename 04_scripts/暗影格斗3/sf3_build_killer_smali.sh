#!/usr/bin/env bash
# =============================================================================
# 暗影格斗3 1.45.3 — 从「参数化 smali 源码」重建 killer smali 树（B 轨运行时签名替换）
#
# 配方来源（同一 MT ApkSignatureKillerEx 形态）：04_scripts/追迹迷城/build_killer_dex_zjmc.sh
# 为什么不入库 dex：仓库 .gitignore 把 *.dex 排除（:164）。killer 的可变输入只有两项——
#   包名 与 官方证书 DER（base64 每 64 列一行）—— 故以带占位符的 smali 源码入库，
#   构建机现场烘焙。本作包里没有 v2 签名块以外的证书来源也一样：证书取自 META-INF/AYGD3.RSA。
#
# 产物：07_patched/暗影格斗3/surgical/killer_smali/bin/mt/signature/*.smali（已烘焙）
#       该目录由 surgical_sf3_1.45.3.sh 的 [2.6/6] 直接 smali assemble 成 classes9.dex
#
# 用法：bash 04_scripts/暗影格斗3/sf3_build_killer_smali.sh
# =============================================================================
set -euo pipefail

NIXIANG="${NIXIANG_ROOT:-/Users/aliang/Documents/nixiang}"
# 非登录 shell（脚本直跑）不会加载 ~/.bashrc 的 NIXIANG_ROOT ⇒ 缺 workspace 时以脚本位置为准（双机同解）
if [ ! -d "$NIXIANG/01_tools" ]; then
  NIXIANG="$(cd "$(dirname "$0")/../.." && pwd)"
fi
if command -v md5sum >/dev/null 2>&1; then
  md5() { if [ "${1:-}" = "-q" ]; then shift; md5sum "$@" | awk '{print $1}'; else md5sum "$@"; fi; }
fi
fsize() { stat -c '%s' "$1" 2>/dev/null || stat -f '%z' "$1"; }
export PYTHONUTF8=1

GAME="暗影格斗3"
VERSION="1.45.3"
PKG="com.wanda.sf3.wd"
ORIG_APK="${SF3_BASE_APK:-$NIXIANG/02_apks/$GAME/${GAME}_${VERSION}_taptap_official_from_device.apk}"
SMALI_SRC="$NIXIANG/04_scripts/$GAME/assets/sf3_killer_smali"
OUT_DIR="$NIXIANG/07_patched/$GAME/surgical/killer_smali"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"

# 官方证书指纹（2026-11-24 从本包 META-INF/AYGD3.RSA 现算；与 01_static.md D3 一致）
CERT_DER_BYTES=845
CERT_MD5="c9950f9c3790803c7ccf42470142afe7"
CERT_SHA1="9078e787fb5246bfd147193343cf7944dadc8fd8"
CERT_SHA256="b147f6a485dfaa6dcb9154e5e5190d342264b0568566749df0aa38cfe55e76e3"

[ -f "$ORIG_APK" ] || { echo "ERROR: 缺官方底包 $ORIG_APK" >&2; exit 1; }
[ -d "$SMALI_SRC" ] || { echo "ERROR: 缺 smali 源码 $SMALI_SRC" >&2; exit 1; }
[ -x "$SMALI_BIN" ] || { echo "ERROR: 缺 smali 工具 $SMALI_BIN" >&2; exit 1; }

WORK="$(mktemp -d "${TMPDIR:-/tmp}/sf3_killer.XXXXXX")"
trap 'rm -rf "$WORK"' EXIT

echo "[1/4] 取官方证书（META-INF/AYGD3.RSA → PKCS#7 → x509 DER → base64）"
unzip -p "$ORIG_APK" META-INF/AYGD3.RSA > "$WORK/aygd3.p7"
openssl pkcs7 -inform DER -in "$WORK/aygd3.p7" -print_certs -out "$WORK/cert.pem" >/dev/null 2>&1 \
  || { echo "ERROR: AYGD3.RSA 不是合法 PKCS#7" >&2; exit 1; }
openssl x509 -in "$WORK/cert.pem" -outform DER -out "$WORK/cert.der"
DER_BYTES="$(fsize "$WORK/cert.der")"
DER_MD5="$(md5 -q "$WORK/cert.der")"
[ "$DER_BYTES" = "$CERT_DER_BYTES" ] || { echo "ERROR: 证书 DER 大小不符（${DER_BYTES} != ${CERT_DER_BYTES}）" >&2; exit 1; }
[ "$DER_MD5" = "$CERT_MD5" ] || { echo "ERROR: 证书 DER md5 不符（${DER_MD5}）" >&2; exit 1; }
echo "  DER ${DER_BYTES} B / md5 ${DER_MD5}（= 记录值）"
python3 - "$WORK/cert.der" "$WORK/cert.b64" <<'PY'
import base64, sys
from pathlib import Path
der = Path(sys.argv[1]).read_bytes()
b64 = base64.b64encode(der).decode()
# 与 MT 产物一致：每 64 字符一行，换行写成 smali 字符串里的 \n 转义（两个字符）
wrapped = '\\n'.join(b64[i:i + 64] for i in range(0, len(b64), 64)) + '\\n'
Path(sys.argv[2]).write_text(wrapped, encoding='utf-8')
print('  base64 %d 字符 / %d 行（单常量 1146 字符 << 65535 上限，无需拆分）'
      % (len(b64), len(wrapped.split('\\n')) - 1))
PY

echo "[2/4] 烘焙占位符（@@PKG@@ / @@CERT_B64_WRAPPED@@）"
rm -rf "$OUT_DIR"
mkdir -p "$OUT_DIR/bin"
cp -R "$SMALI_SRC/." "$OUT_DIR/bin/"
python3 - "$OUT_DIR/bin/mt/signature/KillerApplication193.smali" "$WORK/cert.b64" "$PKG" <<'PY'
import sys
from pathlib import Path
path, cert_file, pkg = sys.argv[1], sys.argv[2], sys.argv[3]
p = Path(path)
text = p.read_text(encoding='utf-8')
cert = Path(cert_file).read_text(encoding='utf-8')
if text.count('@@CERT_B64_WRAPPED@@') != 1:
    raise SystemExit('ERROR: 证书占位符出现 %d 次（期望 1 次）' % text.count('@@CERT_B64_WRAPPED@@'))
if text.count('"@@PKG@@"') != 1:
    raise SystemExit('ERROR: 包名占位符出现 %d 次（期望 1 次）' % text.count('"@@PKG@@"'))
if '.super Landroid/app/Application;' not in text:
    raise SystemExit('ERROR: 模板 .super 不是 android.app.Application')
text = text.replace('@@CERT_B64_WRAPPED@@', cert, 1)
text = text.replace('"@@PKG@@"', '"' + pkg + '"', 1)
p.write_text(text, encoding='utf-8')
print('  baked: pkg=%s cert=%d 字符' % (pkg, len(cert)))
PY

echo "[3/4] 三处必改项逐项断言（源文件）"
K="$OUT_DIR/bin/mt/signature/KillerApplication193.smali"
grep -q '^\.super Landroid/app/Application;$' "$K" || { echo "ERROR: ① 父类不是 android.app.Application" >&2; exit 1; }
echo "  ① .super = Landroid/app/Application;  OK（无别家游戏壳父类）"
grep -qF "const-string v1, \"$PKG\"" "$K" || { echo "ERROR: ② 目标包名未烘焙" >&2; exit 1; }
echo "  ② 目标包名 = $PKG  OK"
grep -qF 'MIIDSTCCAjGgAwIBAgIEJhYcejAN' "$K" || { echo "ERROR: ③ 官方证书 base64 未烘焙" >&2; exit 1; }
if grep -q '@@' "$K"; then echo "ERROR: 残留 @@ 占位符" >&2; exit 1; fi
echo "  ③ 官方证书 base64 已烘焙（头 MIIDSTCCAjGgAwIBAgIEJhYcejAN）OK"
# 防串包：不得残留别家游戏的包名 / 壳类
for bad in 'com.TeaMelo.DungeonChasers' 'PkjNluikmwxGUJv' 'com.wingjoy.toWest'; do
  if grep -qF "$bad" "$K"; then echo "ERROR: 残留他包标识 $bad" >&2; exit 1; fi
done
echo "  ④ 无别家游戏包名/壳类残留  OK"

echo "[4/4] smali assemble → baksmali 回读验证（3 处必须都在）"
"$SMALI_BIN" assemble -o "$WORK/classes9.dex" "$OUT_DIR"
"$BAKSMALI_BIN" d "$WORK/classes9.dex" -o "$WORK/verify" >/dev/null
V="$WORK/verify/bin/mt/signature/KillerApplication193.smali"
[ -f "$V" ] || { echo "ERROR: 回读缺 KillerApplication193.smali" >&2; exit 1; }
grep -q '^\.super Landroid/app/Application;$' "$V" || { echo "ERROR: 回读 ① 失败" >&2; exit 1; }
echo "  回读 ①：.super Landroid/app/Application;"
grep -qF "const-string v1, \"$PKG\"" "$V" || { echo "ERROR: 回读 ② 失败" >&2; exit 1; }
echo "  回读 ②：const-string v1, \"$PKG\""
LC_ALL=C grep -aq 'MIIDSTCCAjGgAwIBAgIEJhYcejAN' "$WORK/classes9.dex" \
  || { echo "ERROR: 回读 ③ 失败（dex 缺证书 base64）" >&2; exit 1; }
echo "  回读 ③：classes9.dex 内 grep -a 'MIIDSTCCAjGgAwIBAgIEJhYcejAN' 命中"

echo "[killer] dex md5 = $(md5 -q "$WORK/classes9.dex") / size = $(fsize "$WORK/classes9.dex") B（不落盘留证，每次同源可复现）"
echo "[killer] smali 树 → $OUT_DIR"
echo "  CERT_MD5=$CERT_MD5 CERT_SHA1=$CERT_SHA1 CERT_SHA256=$CERT_SHA256"
