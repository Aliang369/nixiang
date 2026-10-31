#!/usr/bin/env bash
# 我本布衣：世家录 星空包签名（VERSION 可用 WBJY_VERSION 覆盖；默认 1.1.66）
# 跨机版：路径/工具全部走 04_scripts/_common/env.sh（mac / Windows 同一份）
# 注意：env.sh 必须在 set -e 之前 source —— 它内部有非零返回的探测语句，
#       在 errexit 下会被当成致命错误提前退出（表现为「apksigner 未解析」）。
HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=../_common/env.sh
. "$HERE/../_common/env.sh"
set -euo pipefail

NIXIANG="$NIXIANG_ROOT"
GAME="我本布衣世家录"
VERSION="${WBJY_VERSION:-1.1.66}"
PKG="com.JAGames.TalesoftheClan"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="${WBJY_UNSIGNED:-$OUT/${GAME}_${VERSION}_surgical-unsigned.apk}"
SIGNED="${WBJY_SIGNED:-$OUT/${GAME}_${VERSION}_surgical-releaseSigned.apk}"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
ZIPALIGN="${NIXIANG_ZIPALIGN:-}"
APKSIGNER="${NIXIANG_APKSIGNER:-}"

md5f() { if command -v md5sum >/dev/null 2>&1; then md5sum "$1" | awk '{print $1}'; else md5 -q "$1"; fi; }
sha256f() { if command -v sha256sum >/dev/null 2>&1; then sha256sum "$1" | awk '{print $1}'; else shasum -a 256 "$1" | awk '{print $1}'; fi; }

[[ -f "$UNSIGNED" ]] || { echo "ERROR: missing ${UNSIGNED}" >&2; exit 1; }
[[ -f "$KS" ]]       || { echo "ERROR: missing keystore $KS" >&2; exit 1; }
# Win 上 apksigner 只有 .bat（MSYS 对 .bat 的 -x 恒假）⇒ 一律用 -f 判存在
[[ -n "$ZIPALIGN" && -f "$ZIPALIGN" ]] || { echo "ERROR: zipalign 未解析（env.sh）" >&2; exit 1; }
[[ -n "$APKSIGNER" && -f "$APKSIGNER" ]] || { echo "ERROR: apksigner 未解析（env.sh）" >&2; exit 1; }

WORK="$(mktemp -d "$NIXIANG/.workbuddy/scratch/wbjy_sign.XXXXXX")" 2>/dev/null \
  || { mkdir -p "$NIXIANG/.workbuddy/scratch"; WORK="$(mktemp -d "$NIXIANG/.workbuddy/scratch/wbjy_sign.XXXXXX")"; }
trap 'rm -rf "$WORK"' EXIT
ALIGNED="$WORK/aligned.apk"

"$ZIPALIGN" -f -p 4 "$UNSIGNED" "$ALIGNED"
rm -f "$SIGNED" "$SIGNED.idsig"

"$APKSIGNER" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$ALIGNED"

"$APKSIGNER" verify --print-certs "$SIGNED" 2>&1 | sed -n '1,6p' || true
[[ -f "$SIGNED" ]] || { echo "ERROR: sign failed" >&2; exit 1; }
"$APKSIGNER" verify "$SIGNED" >/dev/null 2>&1 || { echo "ERROR: apksigner verify failed" >&2; exit 1; }

# 同签名检测：与上次证书指纹一致 ⇒ 覆盖安装即可，不必卸载丢存档
CURRENT_SHA1="$("$APKSIGNER" verify --print-certs "$SIGNED" 2>/dev/null | grep 'SHA-1 digest' | awk '{print $NF}' | head -1)"
PREV_SHA1_FILE="$NIXIANG/.workbuddy/scratch/.wbjy_last_cert_sha1"
if [[ -f "$PREV_SHA1_FILE" ]]; then
  PREV="$(cat "$PREV_SHA1_FILE")"
  if [[ "$CURRENT_SHA1" == "$PREV" ]]; then
    echo ""
    echo "[同签名提示] 本次与上次证书 SHA-1 一致 ($CURRENT_SHA1)"
    echo "           若设备上已是同签名星空包 ⇒ 覆盖安装：adb install -r --no-incremental $SIGNED"
  fi
fi
echo -n "$CURRENT_SHA1" > "$PREV_SHA1_FILE"

echo "signed_md5=$(md5f "$SIGNED")"
echo "signed_sha256=$(sha256f "$SIGNED")"
ls -lh "$SIGNED"
echo "DONE: $SIGNED"
