#!/usr/bin/env bash
# 黑暗领域 fix2：重新对齐并签名。
set -euo pipefail

trap 'rc=$?; echo "ERROR: haly_sign_fix2.sh line ${LINENO} rc=${rc}" >&2' ERR

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="黑暗领域"
OUT_DIR="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT_DIR/${GAME}_1.0_combo_fix2-unsigned.apk"
SIGNED="$OUT_DIR/${GAME}_1.0_combo_fix2-releaseSigned.apk"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
DEXDUMP="$HOME/Library/Android/sdk/build-tools/36.0.0/dexdump"

[[ -f "$UNSIGNED" ]] || { echo "ERROR: missing fix2 unsigned APK" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing keystore" >&2; exit 1; }
BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
 done
[[ -n "$BT" ]] || { echo "ERROR: build-tools missing" >&2; exit 1; }

WORK="$(mktemp -d /tmp/haly_sign_fix2.XXXXXX)"
trap 'rm -rf "$WORK"; rc=$?; [ $rc -eq 0 ] || echo "ERROR: haly_sign_fix2.sh rc=$rc" >&2' EXIT
"$BT/zipalign" -f -p 4 "$UNSIGNED" "$WORK/aligned.apk"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$WORK/aligned.apk"
"$BT/apksigner" verify "$SIGNED" >/dev/null

unzip -p "$SIGNED" classes4.dex > "$WORK/classes4.dex"
DEX_DUMP="$("$DEXDUMP" -f "$WORK/classes4.dex" 2>&1 || true)"
grep -Fq "'nativeGetPinnedCert'" <<< "$DEX_DUMP" || { echo "ERROR: signed dex missing nativeGetPinnedCert" >&2; exit 1; }
grep -Fq "'nativeBridgeRegistered'" <<< "$DEX_DUMP" || { echo "ERROR: signed dex missing nativeBridgeRegistered" >&2; exit 1; }
BUILD_SO="$NIXIANG/mod菜单/starry-mod-menu/libs_haly/arm64-v8a/libstarry_magic.so"
SO_MD5="$(md5 -q "$BUILD_SO")"
IN_SO_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$SO_MD5" == "$IN_SO_MD5" ]] || { echo "ERROR: signed package contains wrong so" >&2; exit 1; }

printf '[sign_fix2] output=%s\n[sign_fix2] md5=%s\n[sign_fix2] sha256=%s\n[sign_fix2] so_md5=%s\n' \
  "$SIGNED" "$(md5 -q "$SIGNED")" "$(shasum -a 256 "$SIGNED" | awk '{print $1}')" "$IN_SO_MD5"
