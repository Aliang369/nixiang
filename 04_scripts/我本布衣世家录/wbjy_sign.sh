#!/usr/bin/env bash
# 我本布衣：世家录 星空包签名（1.1.56/vc71 起，VERSION 可用 WBJY_VERSION 覆盖）
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="我本布衣世家录"
VERSION="${WBJY_VERSION:-1.1.56}"
PKG="com.JAGames.TalesoftheClan"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="${WBJY_UNSIGNED:-$OUT/${GAME}_${VERSION}_surgical-unsigned.apk}"
SIGNED="$OUT/${GAME}_${VERSION}_surgical-releaseSigned.apk"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$UNSIGNED" ]] || { echo "ERROR: missing ${UNSIGNED}" >&2; exit 1; }
[[ -f "$KS" ]]       || { echo "ERROR: missing keystore $KS" >&2; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

WORK="$(mktemp -d /tmp/wbjy_sign.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
ALIGNED="$WORK/aligned.apk"

if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$UNSIGNED" "$ALIGNED"
  rm -f "$SIGNED" "$SIGNED.idsig"
  if [[ -x "$BT/apksigner" ]]; then
    "$BT/apksigner" sign \
      --ks "$KS" --ks-key-alias "$KS_ALIAS" \
      --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
      --out "$SIGNED" "$ALIGNED"
    "$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | sed -n '1,6p' || true
  fi
fi

if [[ ! -f "$SIGNED" ]]; then
  [[ -n "$BT" ]] || cp "$UNSIGNED" "$ALIGNED"
  java -jar "$NIXIANG/01_tools/uber-apk-signer.jar" -a "$ALIGNED" --out "$WORK/signed" --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS" >/dev/null
  CAND="$(ls -t "$WORK"/signed/*Signed*.apk 2>/dev/null | sed -n '1p' || true)"
  [[ -n "${CAND:-}" ]] || { echo "ERROR: uber-apk-signer produced nothing" >&2; exit 1; }
  cp -f "$CAND" "$SIGNED"
fi
[[ -f "$SIGNED" ]] || { echo "ERROR: sign failed" >&2; exit 1; }

if [[ -n "$BT" && -x "$BT/apksigner" ]]; then
  "$BT/apksigner" verify "$SIGNED" >/dev/null 2>&1 || { echo "ERROR: apksigner verify failed" >&2; exit 1; }
fi

# 同签名检测：如果上次证书指纹一致，提示用 install -r 覆盖安装，避免误卸载丢数据
if [[ -n "$BT" && -x "$BT/apksigner" ]]; then
  CURRENT_SHA1=$("$BT/apksigner" verify --print-certs "$SIGNED" 2>/dev/null | grep 'SHA-1 digest' | awk '{print $NF}' | head -1)
  PREV_SHA1_FILE="/tmp/.wbjy_last_cert_sha1"
  if [[ -f "$PREV_SHA1_FILE" ]]; then
    PREV=$(cat "$PREV_SHA1_FILE")
    if [[ "$CURRENT_SHA1" == "$PREV" ]]; then
      echo ""
      echo "[同签名提示] 本次与上次证书 SHA-1 一致 ($CURRENT_SHA1)"
      echo "           无需卸载，用覆盖安装：adb install -r --no-incremental $SIGNED"
    fi
  fi
  echo -n "$CURRENT_SHA1" > "$PREV_SHA1_FILE"
fi

echo "signed_md5=$(md5 -q "$SIGNED")"
echo "signed_sha256=$(shasum -a 256 "$SIGNED" | awk '{print $1}')"
ls -lh "$SIGNED"
echo "DONE: $SIGNED"
