#!/usr/bin/env bash
# 霓虹深渊2 (com.veewo.neonabyss2 TapTap 渠道) 2026.10.10 — 签名阶段
# 输入：08_output/霓虹深渊2/霓虹深渊2_2026.10.5_surgical-unsigned.apk
# 输出：08_output/霓虹深渊2/霓虹深渊2_2026.10.5_surgical-releaseSigned.apk
#
# 证书铁律：必须用 01_tools/starry_release.jks（星光统一 release 证书）。
# 菜单 v5 的设备指纹只用 SSAID，而 SSAID 按签名证书隔离 —— 换证书会算出不同 dev_*，
# 服务端按卡密绑 device_id ⇒ 403「卡密已绑定其他设备」。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="霓虹深渊2"
VERSION="2026.10.10"
PKG="com.veewo.neonabyss2"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
SIGNED="$OUT/${GAME}_${VERSION}_surgical-releaseSigned.apk"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
SO_SRC="${NHSY2_SO:-$NIXIANG/mod菜单/starry-mod-menu/libs_nhsy2/arm64-v8a/libstarry_magic.so}"
DO_INSTALL="${NHSY2_INSTALL:-0}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$UNSIGNED" ]] || { echo "ERROR: missing $UNSIGNED（先跑 scripts/surgical_nhsy2_2026.10.10.sh）" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing keystore $KS" >&2; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: 找不到 build-tools 下的 zipalign" >&2; exit 1; }

WORK="$(mktemp -d /tmp/nhsy2_sign.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
ALIGNED="$WORK/aligned.apk"

echo "[sign 1/4] zipalign -p 4"
"$BT/zipalign" -f -p 4 "$UNSIGNED" "$ALIGNED"

echo "[sign 2/4] apksigner sign（starry_release.jks）"
rm -f "$SIGNED"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$ALIGNED"
[[ -f "$SIGNED" ]] || { echo "ERROR: apksigner produced nothing" >&2; exit 1; }
"$BT/apksigner" verify "$SIGNED" >/dev/null || { echo "ERROR: apksigner verify failed" >&2; exit 1; }
"$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | sed -n '1,6p'

echo "[sign 3/4] 包内容 + so 一致性回读"
unzip -l "$SIGNED" > "$WORK/entries.txt"
for e in "classes.dex" "classes2.dex" "classes3.dex" "classes4.dex" "classes5.dex" \
         "lib/arm64-v8a/libstarry_magic.so" "lib/arm64-v8a/libil2cpp.so" \
         "assets/certs/starlight.crt"; do
  grep -q "$e" "$WORK/entries.txt" || { echo "ERROR: 缺 $e" >&2; exit 1; }
done
if [[ -f "$SO_SRC" ]]; then
  SO_MD5="$(md5 -q "$SO_SRC")"
  IN_SO_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
  [[ "$SO_MD5" == "$IN_SO_MD5" ]] \
    || { echo "ERROR: 包内 so ($IN_SO_MD5) != 构建产物 ($SO_MD5)——旧 so 混包" >&2; exit 1; }
  echo "  so md5 == $SO_MD5 ✓"
fi

echo "[sign 4/4] 结果"
echo "[sign] done → $SIGNED"
echo "[sign] md5 = $(md5 -q "$SIGNED")"
ls -lh "$SIGNED"

if [[ "$DO_INSTALL" == "1" ]]; then
  ADB="$HOME/Library/Android/sdk/platform-tools/adb"
  echo "[install] adb push + pm install -r -d"
  "$ADB" push "$SIGNED" /data/local/tmp/nhsy2_signed.apk
  if ! "$ADB" shell pm install -r -d --user 0 /data/local/tmp/nhsy2_signed.apk; then
    echo "[install] 回退 adb install -r --no-incremental"
    if ! "$ADB" install -r --no-incremental "$SIGNED"; then
      echo "ERROR: 安装失败（若报 INSTALL_FAILED_UPDATE_INCOMPATIBLE / signatures do not match，需先卸载官方包）" >&2
      exit 2
    fi
  fi
  "$ADB" shell rm -f /data/local/tmp/nhsy2_signed.apk || true
  echo "[install] 已安装 $PKG"
  if [[ -f "$NIXIANG/04_scripts/_common/starry_post_install.sh" ]]; then
    # shellcheck disable=SC1090
    source "$NIXIANG/04_scripts/_common/starry_post_install.sh"
    starry_post_install "$PKG" || true
  fi
fi
