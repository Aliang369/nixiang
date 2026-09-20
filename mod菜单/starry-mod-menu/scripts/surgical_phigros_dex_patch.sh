#!/usr/bin/env bash
# Phigros — 外科替换 classes2.dex + 重签 + 覆盖安装
#
# 背景（2026-09-21 实测）：
#   游戏内「账号与统计 → TapTap 登录」走 TapSDK 客户端 SSO，被 TapTap 客户端以
#   `signature not match` 拒绝（code=-1 / error=permission_result），游戏显示
#   「登录失败，请检查网络」；LoginBlock 收 resultCode=-1 后直接 finish，不降级网页。
#   修复 = TapLoginForceWeb 白名单加 com.PigeonGames.Phigros，并在 onPreCreated 阶段把
#   intent extra "request" 换成 YnttLoginRequest（getRequestCode() 抛异常），迫使
#   LoginClient.sendLoginRequest 走 catch 分支 → 官方 WebViewHandler 内嵌网页登录。
#
# 本脚本只换 classes2.dex（mod 自有 dex），不碰官方 classes.dex / libil2cpp.so / 资源，
# 属「外科 zip 组装」，不做 apktool 整包重建。
#
# 用法:
#   ./surgical_phigros_dex_patch.sh <base.apk> <new_classes2.dex> <out.apk> [--install]
set -euo pipefail

BASE="${1:?base apk}"
NEWDEX="${2:?new classes2.dex}"
OUT="${3:?output apk}"
INSTALL="${4:-}"
SERIAL="${PHIGROS_SERIAL:-f9a116a2}"

NIXIANG="/Users/aliang/Documents/nixiang"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/apksigner" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"
    break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: apksigner not found" >&2; exit 1; }

[[ -f "$BASE" && -f "$NEWDEX" ]] || { echo "ERROR: missing input" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing keystore: $KS" >&2; exit 1; }

WORK="$(mktemp -d /tmp/phig_dex.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

echo "==> [1/6] 备份基准包"
BAK="${BASE%.apk}_backup.apk"
[[ -f "$BAK" ]] || cp "$BASE" "$BAK"
echo "    base sha256=$(shasum -a 256 "$BASE" | awk '{print $1}')"

echo "==> [2/6] 外科换 classes2.dex"
cp -f "$BASE" "$WORK/base.apk"
zip -q -d "$WORK/base.apk" classes2.dex
cp -f "$NEWDEX" "$WORK/classes2.dex"
( cd "$WORK" && zip -q -X "$WORK/base.apk" classes2.dex )

echo "==> [3/6] zip 条目审计（只允许 classes2.dex 变化）"
python3 - "$BASE" "$WORK/base.apk" <<'PY'
import sys, zipfile
a = zipfile.ZipFile(sys.argv[1]); b = zipfile.ZipFile(sys.argv[2])
ia = {i.filename: (i.CRC, i.file_size) for i in a.infolist()}
ib = {i.filename: (i.CRC, i.file_size) for i in b.infolist()}
if set(ia) != set(ib):
    raise SystemExit(f"ERROR: entry set changed: +{set(ib)-set(ia)} -{set(ia)-set(ib)}")
changed = [n for n in ia if ia[n] != ib[n]]
if changed != ["classes2.dex"]:
    raise SystemExit(f"ERROR: changed entries unexpected: {changed}")
print("    zip entry audit: only classes2.dex changed")
PY

echo "==> [4/6] zipalign + 签名"
mkdir -p "$(dirname "$OUT")"
"$BT/zipalign" -f -p 4 "$WORK/base.apk" "$WORK/aligned.apk"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$OUT" "$WORK/aligned.apk"

echo "==> [5/6] 校验签名 + 包内 dex"
"$BT/apksigner" verify --verbose --print-certs "$OUT" > "$WORK/verify.txt"
LC_ALL=C grep -q "CN=StarryMagic" "$WORK/verify.txt" || { echo "ERROR: 非 StarryMagic 证书" >&2; cat "$WORK/verify.txt" >&2; exit 1; }
LC_ALL=C grep -q "SHA-1 digest: 821c92e404ab5fdac8dbf157c2e467e71440917d" "$WORK/verify.txt" || { echo "ERROR: 签名指纹不符" >&2; exit 1; }
unzip -p "$OUT" classes2.dex > "$WORK/final.dex"
NEW_SHA="$(shasum -a 256 "$NEWDEX" | awk '{print $1}')"
FIN_SHA="$(shasum -a 256 "$WORK/final.dex" | awk '{print $1}')"
[[ "$NEW_SHA" == "$FIN_SHA" ]] || { echo "ERROR: 包内 dex 与输入不一致" >&2; exit 1; }
echo "    dex sha256=$FIN_SHA (一致)"
ls -la "$OUT"

if [[ "$INSTALL" == "--install" ]]; then
  echo "==> [6/6] 覆盖安装 serial=$SERIAL"
  adb -s "$SERIAL" install -r -d "$OUT"
else
  echo "==> [6/6] 跳过安装（未传 --install）"
fi
