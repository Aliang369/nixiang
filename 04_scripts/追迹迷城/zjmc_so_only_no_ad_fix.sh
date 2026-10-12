#!/usr/bin/env bash
# 追迹迷城 1.0.20260912 免广告修复：同签名 so-only 重打与可选覆盖安装。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
SO="$NIXIANG_ROOT/mod菜单/starry-mod-menu/libs_zjmc/arm64-v8a/libstarry_magic.so"
OUT_DIR="$NIXIANG_ROOT/08_output/追迹迷城"
OUT="$OUT_DIR/追迹迷城_1.0.20260912_noAdFix-releaseSigned.apk"
PKG="com.TeaMelo.DungeonChasers"
REMOTE_TMP="/data/local/tmp/zjmc_no_ad_fix.apk"
REMOTE_BASE_TMP="/data/local/tmp/zjmc_starry_base.apk"
KS="$NIXIANG_ROOT/01_tools/starry_release.jks"
ADB_BIN="${ADB_BIN:-adb}"
EXPECTED_BASE_MD5="f50a6f15cd438745bdea3714884c7bf1"
EXPECTED_SIGN_SHA1="821c92e404ab5fdac8dbf157c2e467e71440917d"

[[ -f "$STATE_FILE" ]] || { echo "错误: 缺少设备状态文件" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "错误: 缺少新 so: $SO" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "错误: 缺少签名文件" >&2; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"
    break
  fi
done
[[ -n "$BT" ]] || { echo "错误: 找不到 build-tools" >&2; exit 1; }

SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
mkdir -p "$OUT_DIR"
WORK="$(mktemp -d /tmp/zjmc_no_ad_fix.XXXXXX)"
trap 'rm -rf "$WORK"; "$ADB_BIN" -s "$SERIAL" shell rm -f "$REMOTE_BASE_TMP" "$REMOTE_TMP" >/dev/null 2>&1 || true' EXIT

echo "==> 回拉设备当前星空底包"
DEV_BASE="$("$ADB_BIN" -s "$SERIAL" shell pm path "$PKG" | tr -d '\r' | sed -n 's/^package://p' | head -1)"
[[ -n "$DEV_BASE" ]] || { echo "错误: 设备未安装 $PKG" >&2; exit 1; }
if ! "$ADB_BIN" -s "$SERIAL" pull "$DEV_BASE" "$WORK/base.apk"; then
  "$ADB_BIN" -s "$SERIAL" shell "su -c 'cp \"$DEV_BASE\" \"$REMOTE_BASE_TMP\" && chmod 644 \"$REMOTE_BASE_TMP\"'"
  "$ADB_BIN" -s "$SERIAL" pull "$REMOTE_BASE_TMP" "$WORK/base.apk"
  "$ADB_BIN" -s "$SERIAL" shell rm -f "$REMOTE_BASE_TMP" || true
fi

BASE_MD5="$(md5 -q "$WORK/base.apk")"
[[ "$BASE_MD5" == "$EXPECTED_BASE_MD5" ]] || {
  echo "错误: 设备底包不是预期星空版本（$BASE_MD5 != ${EXPECTED_BASE_MD5}）" >&2
  exit 1
}
BASE_SHA1="$("$BT/apksigner" verify --print-certs "$WORK/base.apk" 2>/dev/null | sed -n 's/.*certificate SHA-1 digest: //p' | head -1 | tr 'A-F' 'a-f')"
[[ "$BASE_SHA1" == "$EXPECTED_SIGN_SHA1" ]] || {
  echo "错误: 设备底包签名不是星光包（${BASE_SHA1}）" >&2
  exit 1
}

echo "==> 替换 so 并重签"
cp -f "$WORK/base.apk" "$WORK/unsigned.apk"
zip -d "$WORK/unsigned.apk" 'META-INF/*' >/dev/null || true
STAGE="$WORK/stage"
mkdir -p "$STAGE/lib/arm64-v8a"
cp -f "$SO" "$STAGE/lib/arm64-v8a/libstarry_magic.so"
(cd "$STAGE" && zip -u "$WORK/unsigned.apk" lib/arm64-v8a/libstarry_magic.so >/dev/null)

"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$WORK/aligned.apk"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "${STARRY_KS_ALIAS:-starry}" \
  --ks-pass "pass:${STARRY_KS_PASS:-starry2026}" \
  --key-pass "pass:${STARRY_KS_PASS:-starry2026}" \
  --out "$OUT" "$WORK/aligned.apk"

python3 - "$OUT" "$WORK/base.apk" "$SO" <<'PY'
import hashlib, sys, zipfile
out, base, so_path = sys.argv[1:]
z = zipfile.ZipFile(out)
b = zipfile.ZipFile(base)
for name in ("AndroidManifest.xml", "classes.dex", "classes2.dex", "classes3.dex", "classes4.dex", "classes5.dex", "classes6.dex", "resources.arsc"):
    assert hashlib.sha256(z.read(name)).hexdigest() == hashlib.sha256(b.read(name)).hexdigest(), name
so = z.read("lib/arm64-v8a/libstarry_magic.so")
assert so == open(so_path, "rb").read()
assert b"[Zjmc]" in so
assert b"AT.GetFreeSkipAdsUsed" in so
assert b"AT.UseFreeSkipAds" in so
print("apk validation ok; only so changed")
PY

echo "==> 签名复核"
"$BT/apksigner" verify --print-certs "$OUT" 2>&1 | sed -n '1,12p'

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  echo "==> 覆盖安装"
  "$ADB_BIN" -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true
  "$ADB_BIN" -s "$SERIAL" push "$OUT" "$REMOTE_TMP"
  "$ADB_BIN" -s "$SERIAL" shell "pm install -r -d --user 0 '$REMOTE_TMP'"
  "$ADB_BIN" -s "$SERIAL" shell rm -f "$REMOTE_TMP" || true
  bash "$NIXIANG_ROOT/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$OUT" --su
fi

cat <<EOF
output=$OUT
output_md5=$(md5 -q "$OUT")
output_sha256=$(shasum -a 256 "$OUT" | awk '{print $1}')
so_md5=$(md5 -q "$SO")
installed=${STARRY_INSTALL:-0}
EOF
