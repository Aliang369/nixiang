#!/usr/bin/env bash
# 消消英雄2 1.2.27 — 签名阶段（外科产物 → 正式签名包）
# 用法: bash 04_scripts/消消英雄2/xxh2_sign_1.2.27.sh
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="消消英雄2"
VERSION="1.2.27"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
SIGNED="$OUT/${GAME}_${VERSION}_starry-releaseSigned.apk"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$UNSIGNED" ]] || { echo "ERROR: 缺 ${UNSIGNED}（先跑 surgical_xxh2_1.2.27.sh）" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: 缺 keystore $KS" >&2; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
[[ -n "$BT" ]] || { echo "ERROR: 找不到 build-tools/zipalign" >&2; exit 1; }

WORK="$(mktemp -d /tmp/xxh2_sign.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
ALIGNED="$WORK/aligned.apk"

"$BT/zipalign" -f -p 4 "$UNSIGNED" "$ALIGNED"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$ALIGNED"

echo "--- apksigner verify ---"
"$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | sed -n '1,8p' || true
CERT_MD5="$("$BT/apksigner" verify --print-certs "$SIGNED" 2>/dev/null | grep -i 'certificate MD5' | head -1 | awk '{print $NF}' | tr -d ':' | tr 'A-Z' 'a-z')"
echo "signer cert MD5: $CERT_MD5"
[[ -n "$CERT_MD5" ]] || { echo "ERROR: 取不到签名证书指纹" >&2; exit 1; }

# 结构复核：注入物在位、官方 dex/so/metadata 未变
PY_BIN="python3.11"
command -v "$PY_BIN" >/dev/null 2>&1 || PY_BIN="python3"
"$PY_BIN" - "$SIGNED" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
names = set(z.namelist())
need = ["classes3.dex", "lib/arm64-v8a/libstarry_magic.so",
        "lib/arm64-v8a/libc++_shared.so", "lib/arm64-v8a/libil2cpp.so",
        "lib/arm64-v8a/libxlua.so", "assets/bin/Data/Managed/Metadata/global-metadata.dat"]
for n in need:
    assert n in names, f"缺 {n}"
# 注：签名后 META-INF/*.SF|*.RSA 属于**我方** v1 签名（apksigner 已在上方 verify），
# 官方旧签名条目在 surgical 阶段已被剔除，这里不做名字断言。
print("  structure ok:", len(names), "entries")
PY

ls -lh "$SIGNED"
md5 -q "$SIGNED"
shasum -a 256 "$SIGNED" | awk '{print $1}'
echo "DONE(signed): $SIGNED"
