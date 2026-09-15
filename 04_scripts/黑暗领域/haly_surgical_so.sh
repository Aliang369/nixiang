#!/usr/bin/env bash
# 黑暗领域 v2：以真机回收的星光包为底，只替换 libstarry_magic.so 并产出未签名包。
set -euo pipefail

trap 'rc=$?; echo "ERROR: haly_surgical_so.sh line ${LINENO} rc=${rc}" >&2' ERR

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="黑暗领域"
VERSION="1.0"
BASE_APK="$NIXIANG/02_apks/$GAME/${GAME}_${VERSION}_starry_from_device.apk"
SO="$NIXIANG/mod菜单/starry-mod-menu/libs_haly/arm64-v8a/libstarry_magic.so"
OUT_DIR="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT_DIR/${GAME}_${VERSION}_combo_fix-unsigned.apk"

[[ -f "$BASE_APK" ]] || { echo "ERROR: missing base APK $BASE_APK" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: missing build so $SO" >&2; exit 1; }
LC_ALL=C grep -aq 'build=starry_haly_v2' "$SO" || { echo "ERROR: so is not v2" >&2; exit 1; }

mkdir -p "$OUT_DIR"
WORK="$(mktemp -d /tmp/haly_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rc=$?; [ $rc -eq 0 ] || echo "ERROR: haly_surgical_so.sh rc=$rc" >&2' EXIT
cp -f "$BASE_APK" "$WORK/base.apk"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$SO" "$WORK/lib/arm64-v8a/libstarry_magic.so"
(
  cd "$WORK"
  zip -q -u base.apk lib/arm64-v8a/libstarry_magic.so
)

SO_MD5="$(md5 -q "$SO")"
IN_SO_MD5="$(unzip -p "$WORK/base.apk" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$SO_MD5" == "$IN_SO_MD5" ]] || { echo "ERROR: patched so mismatch" >&2; exit 1; }
unzip -tq "$WORK/base.apk" >/dev/null
cp -f "$WORK/base.apk" "$UNSIGNED"
printf '[surgical] output=%s\n[surgical] so_md5=%s\n' "$UNSIGNED" "$IN_SO_MD5"
