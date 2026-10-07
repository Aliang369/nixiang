#!/usr/bin/env bash
# 弹力果冻 **4399 渠道**（com.ginicat.sweet.m4399）— 底包身份核对
#
# 用途：在动任何外科步骤之前，把「底包是不是 4399 星光包、版本对不对、dex/so/Manifest 布局」
#       一次性落成可复查的 JSON。任何一项不符即 exit 1，禁止往下走。
#
# 用法: bash 04_scripts/弹力果冻4399/tlgd4399_check_apk.sh
set -euo pipefail

NIXIANG="${NIXIANG:-/Users/aliang/Documents/nixiang}"
APK="${TLGD4399_BASE:-$NIXIANG/02_apks/弹力果冻4399/弹力果冻4399_2.4.0_vc4412_device_base.apk}"
SUMMARY="$NIXIANG/04_scripts/弹力果冻4399/check_summary.json"
PKG="com.ginicat.sweet.m4399"
VC_EXPECT="4412"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/aapt2" ]]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
[[ -n "$BT" ]] || { echo "ERROR: build-tools not found"; exit 1; }
[[ -f "$APK" ]] || { echo "ERROR: 底包不存在 ${APK}（先跑 tlgd4399_fetch_apk.sh）"; exit 1; }

BADGE="$("$BT/aapt2" dump badging "$APK" 2>/dev/null)"
FIRST="$(echo "$BADGE" | head -1)"
echo "[check] $FIRST"
echo "$BADGE" | grep -q "name='$PKG'" || { echo "ERROR: 包名不是 ${PKG}（底包渠道错）"; exit 1; }
echo "$BADGE" | grep -q "versionCode='$VC_EXPECT'" || { echo "ERROR: versionCode 不是 $VC_EXPECT"; exit 1; }
"$BT/apksigner" verify --print-certs "$APK" 2>&1 | head -4 || true

APK_MD5="$(md5 -q "$APK")"
APK_SHA="$(shasum -a 256 "$APK" | awk '{print $1}')"
SO_MD5="$(unzip -p "$APK" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
IL2CPP_MD5="$(unzip -p "$APK" lib/arm64-v8a/libil2cpp.so | md5 -q)"
DEX2_SHA="$(unzip -p "$APK" classes2.dex | shasum -a 256 | awk '{print $1}')"
DEX2_SIZE="$(unzip -p "$APK" classes2.dex | wc -c | tr -d ' ')"
DEX_CONTAIN_MENU="$(unzip -p "$APK" classes2.dex | grep -c -F -a 'com/starry/magic/menu/MenuService' || true)"
DEX_HAS_NEW_CATEGORY="$(unzip -p "$APK" classes2.dex | grep -c -F -a 'Category_战斗' || true)"

# 旧菜单 dex 的判据：不含新版功能分组串 Category_战斗
OLD_MENU="no"
[[ "$DEX_HAS_NEW_CATEGORY" == "0" ]] && OLD_MENU="yes"

python3.11 - "$APK" <<'PY'
import sys, zipfile
names = zipfile.ZipFile(sys.argv[1]).namelist()
dexs = sorted(n for n in names if n.endswith(".dex"))
assert dexs == ["classes.dex", "classes2.dex"], f"dex 布局异常 {dexs}"
print(f"  [check] {len(names)} 条目；dex={dexs}")
PY

cat > "$SUMMARY" <<JSON
{
  "package": "$PKG",
  "aapt_first_line": "${FIRST//\"/\\\"}",
  "apk": "$APK",
  "apk_size": $(stat -f%z "$APK"),
  "apk_md5": "$APK_MD5",
  "apk_sha256": "$APK_SHA",
  "pkg_so_md5": "$SO_MD5",
  "pkg_libil2cpp_md5": "$IL2CPP_MD5",
  "classes2_dex_size": $DEX2_SIZE,
  "classes2_dex_sha256": "$DEX2_SHA",
  "classes2_is_old_menu": "$OLD_MENU",
  "classes2_MenuService_hits": $DEX_CONTAIN_MENU,
  "classes2_has_new_category_label": $DEX_HAS_NEW_CATEGORY,
  "checked_at": "$(date '+%Y-%m-%d %H:%M:%S')"
}
JSON
echo "[check] classes2.dex=$DEX2_SIZE B sha256=${DEX2_SHA:0:16}… 旧菜单=$OLD_MENU"
echo "[check] 包内 libstarry_magic.so md5=$SO_MD5 / libil2cpp.so md5=$IL2CPP_MD5"
echo "[check] summary → $SUMMARY"
