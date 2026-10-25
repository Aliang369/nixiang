#!/usr/bin/env bash
# 勇士之心 1.0.4 - 签名阶段（pipeline sign）
set -euo pipefail

NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)}"
GAME="勇士之心"
VERSION="${VERSION:-1.0.4}"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
SIGNED="$OUT/${GAME}_${VERSION}_surgical-releaseSigned.apk"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$UNSIGNED" ]] || { echo "ERROR: missing $UNSIGNED" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing keystore $KS" >&2; exit 1; }

# 2026-10-25 win：build-tools 目录/可执行名随平台变（mac 是 zipalign，win 是 zipalign.exe），
# 且 SDK 根在 ANDROID_HOME。这里按「先 ANDROID_HOME，再 mac 老路径」逐个探测，取最高版本。
BT=""
ZA=""
APKS=""
_pick_bt() {
  for cand in "$1"; do
    [ -d "$cand" ] || continue
    # 用 -f 而不是 -x：MSYS 的 test -x 会把 "zipalign" 当 "zipalign.exe" 解析成真，
    # 而 apksigner.bat 又不是可执行位，混用会一半命中一半落空（2026-10-25 实测）。
    if [ -f "$cand/zipalign.exe" ]; then ZA="$cand/zipalign.exe";
    elif [ -f "$cand/zipalign" ]; then ZA="$cand/zipalign";
    else continue; fi
    if [ -f "$cand/apksigner.bat" ]; then APKS="$cand/apksigner.bat";
    elif [ -f "$cand/apksigner" ]; then APKS="$cand/apksigner";
    elif [ -f "$cand/apksigner.jar" ]; then APKS="$cand/apksigner.jar";
    else ZA=""; continue; fi
    BT="$cand"
    return 0
  done
  return 1
}
if [ -n "${ANDROID_HOME:-}${ANDROID_SDK_ROOT:-}" ]; then
  for v in 36.1.0 36.0.0 35.0.0 34.0.0; do
    _pick_bt "${ANDROID_HOME:-$ANDROID_SDK_ROOT}/build-tools/$v" && break
  done
fi
if [ -z "$BT" ]; then
  for v in 36.0.0 35.0.0 34.0.0; do
    _pick_bt "$HOME/Library/Android/sdk/build-tools/$v" && break
  done
fi

WORK="$(mktemp -d /tmp/ysxz_sign.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
ALIGNED="$WORK/aligned.apk"

# 分发：.bat/.exe 直接执行；只有 jar 时用 java -jar
_apksign() {
  case "$APKS" in
    *.jar) java -jar "$APKS" "$@" ;;
    *)     "$APKS" "$@" ;;
  esac
}
if [[ -n "$BT" ]]; then
  "$ZA" -f -p 4 "$UNSIGNED" "$ALIGNED"
  _apksign sign \
    --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$SIGNED" "$ALIGNED"
  _apksign verify --print-certs "$SIGNED" 2>&1 | sed -n '1,6p' || true
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
if [[ -n "$BT" ]]; then
  _apksign verify "$SIGNED" >/dev/null || { echo "ERROR: apksigner verify failed" >&2; exit 1; }
fi

unzip -l "$SIGNED" > "$WORK/entries.txt"
for e in classes.dex classes2.dex \
         lib/arm64-v8a/libstarry_magic.so resources.arsc; do
  grep -q " $e\$" "$WORK/entries.txt" || { echo "ERROR: $e missing in signed apk" >&2; exit 1; }
done

SO_MD5="$(md5 -q "$NIXIANG/mod菜单/starry-mod-menu/libs_ysxz/arm64-v8a/libstarry_magic.so")"
IN_SO_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$SO_MD5" == "$IN_SO_MD5" ]] || { echo "ERROR: signed so mismatch" >&2; exit 1; }

echo "[sign] done -> $SIGNED"
echo "[sign] size=$(stat -f '%z' "$SIGNED")"
echo "[sign] md5=$(md5 -q "$SIGNED")"
echo "[sign] sha256=$(shasum -a 256 "$SIGNED" | awk '{print $1}')"
echo "[sign] so_md5=$IN_SO_MD5"
