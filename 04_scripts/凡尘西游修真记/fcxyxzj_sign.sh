#!/usr/bin/env bash
# 凡尘西游修真记 1.1.0 — 签名阶段（pipeline sign）
# 输入：08_output/凡尘西游修真记/凡尘西游修真记_1.1.0_surgical-unsigned.apk
# 输出：08_output/凡尘西游修真记/凡尘西游修真记_1.1.0_surgical-releaseSigned.apk
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="凡尘西游修真记"
VERSION="1.1.0"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
SIGNED="$OUT/${GAME}_${VERSION}_surgical-releaseSigned.apk"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$UNSIGNED" ]] || { echo "ERROR: missing ${UNSIGNED}（先跑 surgical 阶段）" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing keystore $KS" >&2; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

WORK="$(mktemp -d /tmp/fcxyxzj_sign.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
ALIGNED="$WORK/aligned.apk"

if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$UNSIGNED" "$ALIGNED"
  rm -f "$SIGNED"
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
  java -jar "$SIGNER" -a "$ALIGNED" --out "$WORK/signed" --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS" >/dev/null
  CAND="$(ls -t "$WORK"/signed/*Signed*.apk 2>/dev/null | sed -n '1p' || true)"
  [[ -n "${CAND:-}" ]] || { echo "ERROR: uber-apk-signer produced nothing" >&2; exit 1; }
  cp -f "$CAND" "$SIGNED"
fi

[[ -f "$SIGNED" ]] || { echo "ERROR: sign failed" >&2; exit 1; }

# 签名完整性 + 包内容回读（注意：禁止 `unzip -l | grep -q`——grep 提前退出会给 unzip 发 SIGPIPE，
# 在 set -o pipefail 下整条管道返回 141，误报"缺失"）
if [[ -n "$BT" && -x "$BT/apksigner" ]]; then
  "$BT/apksigner" verify "$SIGNED" >/dev/null || { echo "ERROR: apksigner verify failed" >&2; exit 1; }
fi
unzip -l "$SIGNED" > "$WORK/entries.txt"
grep -q "classes3.dex" "$WORK/entries.txt" || { echo "ERROR: classes3.dex missing in signed apk" >&2; exit 1; }
grep -q "classes2.dex" "$WORK/entries.txt" || { echo "ERROR: classes2.dex missing in signed apk" >&2; exit 1; }
grep -q "lib/arm64-v8a/libstarry_magic.so" "$WORK/entries.txt" \
  || { echo "ERROR: so missing in signed apk" >&2; exit 1; }

# 包内 so 仍须等于 build_so 产物（签名不改变内容，但防手工替换）
SO_MD5="$(md5 -q "$NIXIANG/mod菜单/starry-mod-menu/libs_fcxyxzj/arm64-v8a/libstarry_magic.so" 2>/dev/null || echo none)"
IN_SO_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$SO_MD5" == "$IN_SO_MD5" ]] \
  || { echo "ERROR: 签名包内 so ($IN_SO_MD5) != 构建产物 ($SO_MD5)" >&2; exit 1; }

echo "[sign] done → $SIGNED"
echo "[sign] md5 = $(md5 -q "$SIGNED")"
ls -lh "$SIGNED"
