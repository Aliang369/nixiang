#!/usr/bin/env bash
# 诊断对照：**只重签名、零 mod 内容**的原版包。
#
# 目的：把「重签名导致平台登录失败」与「我方启动期注入导致登录卡住」分开。
#   - 若本产物在设备上同样卡在加载页 → 变量是**签名**（跨进程无法用 SignSpoof 兜）；
#   - 若本产物能正常登录进游戏   → 变量是**我方启动期代码**（Provider/MenuService/dex/so）。
#
# 产物不是交付物，只进 07_patched/ 诊断目录；脚本用闭环断言证明它除了签名以外与原包逐字节相同。
set -euo pipefail

NIXIANG="${NIXIANG:-/Users/aliang/Documents/nixiang}"
GAME=城主别慌张
ORIG="$NIXIANG/02_apks/$GAME/城主别慌张_1.0.10_huawei_base.apk"
OUTDIR="$NIXIANG/07_patched/$GAME/diag_resign"
OUT="$OUTDIR/e2_resign_only_1.0.10.apk"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"

[[ -f "$ORIG" ]] || { echo "ERROR: 原包不存在: $ORIG"; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: 签名库不存在: $KS"; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  [[ -x "$HOME/Library/Android/sdk/build-tools/$v/apksigner" ]] && { BT="$HOME/Library/Android/sdk/build-tools/$v"; break; }
done
[[ -n "$BT" ]] || { echo "ERROR: 找不到 apksigner"; exit 1; }

mkdir -p "$OUTDIR"
echo "[1/4] 原包 md5 = $(md5 -q "$ORIG")"
echo "[2/4] 只重签名（不加 Provider / 不加 dex / 不加 so / 不改 Manifest）"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" --ks-pass "pass:$KS_PASS" \
  --key-pass "pass:$KS_PASS" --out "$OUT" "$ORIG"
echo "      → $OUT (md5 $(md5 -q "$OUT"))"

echo "[3/4] 签名核对（必须是星光密钥，且与原包不同）"
"$BT/apksigner" verify --print-certs "$OUT" 2>/dev/null | grep -E "Signer #1 certificate (DN|SHA-256)" || true
echo "      原包签名："
"$BT/apksigner" verify --print-certs "$ORIG" 2>/dev/null | grep -E "Signer #1 certificate (DN|SHA-256)" || true
"$BT/zipalign" -c -p 4 "$OUT" && echo "      zipalign ✓"

echo "[4/4] 闭环断言：除签名块外与原包逐字节相同"
python3 - "$ORIG" "$OUT" <<'PY'
import hashlib, sys, zipfile

a, b = zipfile.ZipFile(sys.argv[1]), zipfile.ZipFile(sys.argv[2])
na = set(n for n in a.namelist() if not n.startswith("META-INF/"))
nb = set(n for n in b.namelist() if not n.startswith("META-INF/"))
print("      非 META-INF 条目: 原包 %d / 诊断包 %d / 差集 %s"
      % (len(na), len(nb), (na ^ nb) or "空 ✓"))
assert na == nb, "条目集合不一致 —— 这不是纯重签名包"
bad = []
for f in sorted(na):
    if hashlib.sha256(a.read(f)).hexdigest() != hashlib.sha256(b.read(f)).hexdigest():
        bad.append(f)
print("      逐条内容比对: %s" % ("全部字节相同 ✓" if not bad else "★变了: %s" % bad[:5]))
assert not bad, "存在内容不一致的条目 —— 这不是纯重签名包"
# 证明确实没有 mod 内容
for probe in ("classes4.dex", "assets/starry/czbhz_patch.js", "lib/arm64-v8a/libstarry_magic.so"):
    assert probe not in nb, "诊断包里出现了 mod 内容: %s" % probe
print("      无 mod 内容（classes4.dex / starry 补丁 / libstarry_magic.so 均不存在）✓")
PY
echo "✅ 诊断包就绪: $OUT"
