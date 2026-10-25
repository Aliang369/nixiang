#!/usr/bin/env bash
# 弹力果冻 **4399 渠道**（com.ginicat.sweet.m4399，vc4412）— 装机（2026-10-26 雷电轮）
#
# 口径变更（重要）：
#   - 2026-10-07 轮底包是「设备已装星光包」⇒ 只覆盖安装、绝不卸载。
#   - **2026-10-26 轮底包是 4399 官方原版** ⇒ 官方证书（O=ginicat）≠ 星光证书（CN=StarryMagic），
#     `pm install -r` 必然 UPDATE_INCOMPATIBLE。本脚本因此支持
#     **ALLOW_UNINSTALL=1**：先试覆盖安装，失败且该开关打开时，才 `pm uninstall` 再装。
#     该开关**必须**由用户当轮明确授权后才可传（默认 0 = 不卸载，只报告）。
#
# 其它：
#   - 装机前过 pre_install_gate.sh（JNI 配套 / 门禁串 / Manifest 形态 / 构建戳）
#   - 装机后 verify_installed_apk.sh + 设备包内 so/dex 逐项比对
#
# 用法:
#   ALLOW_UNINSTALL=1 SERIAL=emulator-5554 TLGD4399_APK=<apk> bash 04_scripts/弹力果冻4399/tlgd4399_install.sh
set -euo pipefail

NIXIANG="${NIXIANG:-/Users/aliang/Documents/nixiang}"
SERIAL="${SERIAL:-${TLGD4399_SERIAL:-f9a116a2}}"
ALLOW_UNINSTALL="${ALLOW_UNINSTALL:-0}"
PKG="com.ginicat.sweet.m4399"
VC_EXPECT="4412"
STEM="${TLGD_STEM:-弹力果冻4399_2.4.0_雷电星空_20261026}"
APK="${TLGD4399_APK:-$NIXIANG/08_output/弹力果冻4399/${STEM}-releaseSigned.apk}"
BUILT_SO="$NIXIANG/mod菜单/starry-mod-menu/libs_tlgdm4399/arm64-v8a/libstarry_magic.so"
JNI="$NIXIANG/mod菜单/starry-mod-menu/app/src/main/jni"

EXE=""
case "$(uname -s 2>/dev/null || echo unknown)" in
  MINGW*|MSYS*|CYGWIN*) EXE=".exe" ;;
esac
BT="${TLGD_BT:-}"
if [[ -z "$BT" ]]; then
  for v in 36.1.0 36.0.0 35.0.0 34.0.0; do
    for base in "${ANDROID_HOME:-}" "$HOME/Library/Android/sdk"; do
      [[ -n "$base" ]] || continue
      if [[ -x "$base/build-tools/$v/zipalign$EXE" ]]; then BT="$base/build-tools/$v"; break 2; fi
    done
  done
fi
APKSIGNER="$BT/apksigner"
[[ -f "$BT/apksigner.bat" ]] && APKSIGNER="$BT/apksigner.bat"

[[ -f "$APK" ]] || { echo "ERROR: 产物不存在 $APK —— 先跑 tlgd4399_surgical.sh"; exit 1; }
[[ -f "$BUILT_SO" ]] || { echo "ERROR: 缺 build_so 产物 $BUILT_SO"; exit 1; }
[[ -n "$BT" ]] || { echo "ERROR: build-tools not found"; exit 1; }
command -v adb >/dev/null || { echo "ERROR: adb not found"; exit 1; }

echo "=== [0/5] 产物身份与签名 ==="
BADGE="$("$BT/aapt2$EXE" dump badging "$APK" 2>/dev/null | head -1 || true)"
echo "  $BADGE"
echo "$BADGE" | grep -q "name='$PKG'" || { echo "ERROR: 产物包名不是 $PKG"; exit 1; }
echo "$BADGE" | grep -q "versionCode='$VC_EXPECT'" || { echo "ERROR: 产物 versionCode 不是 $VC_EXPECT"; exit 1; }
"$APKSIGNER" verify --print-certs "$APK" 2>&1 | head -4 || true
"$APKSIGNER" verify "$APK" >/dev/null 2>&1 || { echo "ERROR: apksigner verify 失败"; exit 1; }
"$APKSIGNER" verify --print-certs "$APK" 2>/dev/null | grep -q 'StarryMagic' \
  || { echo "ERROR: 证书 CN 不是 StarryMagic"; exit 1; }

echo "=== [1/5] 装机前门禁 pre_install_gate ==="
bash "$NIXIANG/04_scripts/_common/pre_install_gate.sh" \
  --cpp "$JNI/MainModern.cpp" \
  --dex "$APK" \
  --apk "$APK" \
  --needle 'com/starry/magic/StarryApp' \
  --needle 'com/starry/magic/bridge/ModBridge' \
  --needle 'com/starry/magic/menu/MenuService' \
  --needle 'com/starry/magic/menu/NativeMenuController' \
  --needle 'com/starry/magic/sign/SignSpoof' \
  --needle 'com/starry/magic/ota' \
  || { echo "ERROR: 装机前门禁 FAIL —— 禁止装机"; exit 1; }

echo "=== [2/5] 设备在线检查 ==="
adb -s "$SERIAL" get-state >/dev/null 2>&1 || { echo "ERROR: 设备 $SERIAL 不在线"; exit 1; }
CUR_VC="$(adb -s "$SERIAL" shell dumpsys package "$PKG" 2>/dev/null | tr -d '\r' | grep -m1 -o 'versionCode=[0-9]*' | tr -dc '0-9' || true)"
CUR_CERT="$(adb -s "$SERIAL" shell dumpsys package "$PKG" 2>/dev/null | tr -d '\r' | grep -m1 -o 'signatures:\[[0-9a-f]*\]' || true)"
echo "  设备已装 $PKG versionCode=${CUR_VC:-未安装} $CUR_CERT"

echo "=== [3/5] 安装（先试覆盖；签名冲突且 ALLOW_UNINSTALL=1 才卸载）==="
adb -s "$SERIAL" push "$APK" /data/local/tmp/tlgd4399_new.apk 2>&1 | tail -1
RC=0
OUT="$(adb -s "$SERIAL" shell "su -c 'pm install -r -d --user 0 /data/local/tmp/tlgd4399_new.apk'" 2>&1 | tail -3)" || RC=$?
echo "  $OUT"
if [[ $RC -ne 0 ]] || echo "$OUT" | grep -qi 'fail'; then
  if [[ "$ALLOW_UNINSTALL" == "1" ]]; then
    echo "  → 覆盖安装失败（预期：异签名）。ALLOW_UNINSTALL=1 已授权，执行卸载"
    adb -s "$SERIAL" shell "su -c 'pm uninstall $PKG'" 2>&1 | tail -2
    sleep 2
    RC=0
    OUT="$(adb -s "$SERIAL" shell "su -c 'pm install -d --user 0 /data/local/tmp/tlgd4399_new.apk'" 2>&1 | tail -3)" || RC=$?
    echo "  $OUT"
    if [[ $RC -ne 0 ]] || echo "$OUT" | grep -qi 'fail'; then
      RC=0
      adb -s "$SERIAL" install -d /data/local/tmp/tlgd4399_new.apk 2>&1 | tail -3 || RC=$?
    fi
  else
    echo "ERROR: 覆盖安装失败且未授权卸载（ALLOW_UNINSTALL=0）。异签名时报用户，不自动卸载。"
    adb -s "$SERIAL" shell rm -f /data/local/tmp/tlgd4399_new.apk >/dev/null 2>&1 || true
    exit 1
  fi
fi
adb -s "$SERIAL" shell rm -f /data/local/tmp/tlgd4399_new.apk >/dev/null 2>&1 || true

echo "=== [4/5] 装机完整性（设备 base.apk md5 == 本地产物）==="
bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" \
  || bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" --su

echo "=== [5/5] 设备包内 so / 菜单 dex 逐项比对 ==="
BASE="$(adb -s "$SERIAL" shell "pm path $PKG" 2>/dev/null | tr -d '\r' | grep 'base\.apk' | head -1 | sed 's/^package://' || true)"
[[ -n "$BASE" ]] || { echo "ERROR: 取不到设备 base.apk 路径"; exit 1; }
echo "  设备 base.apk = $BASE"
DEV_SO_MD5="$(adb -s "$SERIAL" exec-out "unzip -p '$BASE' lib/arm64-v8a/libstarry_magic.so" 2>/dev/null | md5 -q)"
LOC_SO_MD5="$(md5 -q "$BUILT_SO")"
echo "  so:   设备=$DEV_SO_MD5  本地构建=$LOC_SO_MD5"
[[ "$DEV_SO_MD5" == "$LOC_SO_MD5" ]] \
  || { echo "ERROR: 设备上不是本次构建的 so —— 装机没换新"; exit 1; }

DEV_DEX_SHA="$(adb -s "$SERIAL" exec-out "unzip -p '$BASE' classes2.dex" 2>/dev/null | shasum -a 256 | awk '{print $1}')"
LOC_DEX_SHA="$(unzip -p "$APK" classes2.dex | shasum -a 256 | awk '{print $1}')"
echo "  dex:  设备=${DEV_DEX_SHA:0:16}…  本地产物=${LOC_DEX_SHA:0:16}…"
[[ "$DEV_DEX_SHA" == "$LOC_DEX_SHA" ]] \
  || { echo "ERROR: 设备上不是本次构建的菜单 dex"; exit 1; }

echo "  设备 dex 集合：$(adb -s "$SERIAL" exec-out "unzip -l '$BASE'" 2>/dev/null | tr -d '\r' | awk '{print $NF}' | grep -c '^classes.*\.dex$') 个（应为 2）"

echo "DONE: 安装 + 三项闭环校验通过（base.apk md5 / so / 菜单 dex）"
