#!/usr/bin/env bash
# ================================================================
#  无限升级 11.266018 / vc1637 星空版 — 装机（含卸载官方包）
#
#  背景：星空包签名 CN=StarryMagic ≠ 官方 CN=Cui，pm install -r 必然
#  报 INSTALL_FAILED_UPDATE_INCOMPATIBLE，只能先卸载。卸载会清 app
#  私有数据（存档）。因此本脚本把卸载做成**显式一步**，需 --yes 才执行。
#
#  用法:
#    bash scripts/install_wxsj_11.266018_vc1637.sh --yes
#    STARRY_SERIAL=xxxx bash scripts/install_wxsj_11.266018_vc1637.sh --yes
#
#  同签名升级（已在星空包上迭代，不需卸载/不丢存档）:
#    bash scripts/install_wxsj_11.266018_vc1637.sh --upgrade
#
#  环境变量: STARRY_SERIAL（默认 f9a116a2）
# ================================================================
set -euo pipefail

SERIAL="${STARRY_SERIAL:-f9a116a2}"
PKG="com.xingzuodaoshi.infinityUpgrade"
NIXIANG="/Users/aliang/Documents/nixiang"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"

APK="$NIXIANG/08_output/无限升级/无限升级_11.266018_vc1637_starry-releaseSigned.apk"
EXPECT_SHA="32bb13cdc79b098539de18186091bfdf041656d7606bb679d429206febcfc176"
SO="$STARRY_SRC/app/src/libs_wxsj_vc1637/arm64-v8a/libstarry_magic.so"
CPP="$STARRY_SRC/app/src/main/jni/Games/WxsjVc1637Mod.cpp"
MENU_JSON="$NIXIANG/10_docs/无限升级/wxsj_vc1637_menu_template.json"
REMOTE="/data/local/tmp/starry_wxsj_vc1637.apk"

DO_UNINSTALL=0
for a in "$@"; do
  case "$a" in
    --yes) DO_UNINSTALL=1 ;;
    --upgrade) DO_UNINSTALL=0; UPGRADE=1 ;;
    -h|--help) sed -n '2,20p' "$0"; exit 0 ;;
    *) echo "未知参数: $a（--help）" >&2; exit 1 ;;
  esac
done
UPGRADE="${UPGRADE:-0}"
if [[ "$DO_UNINSTALL" != "1" && "$UPGRADE" != "1" ]]; then
  echo "拒绝执行：本脚本会卸载官方包并清除其私有数据（存档）。"
  echo "确认后加 --yes 重跑；已在星空包上迭代用 --upgrade（不卸载、保留存档）。"
  exit 2
fi

echo "[0/8] 设备 / 产物预检"
adb -s "$SERIAL" get-state >/dev/null
[[ -f "$APK" ]] || { echo "ERROR: 缺 APK $APK"; exit 1; }
ACT_SHA="$(shasum -a 256 "$APK" | awk '{print $1}')"
[[ "$ACT_SHA" == "$EXPECT_SHA" ]] || {
  echo "ERROR: APK sha256 不符"; echo "  expect $EXPECT_SHA"; echo "  actual $ACT_SHA"; exit 1; }
echo "  APK sha256 ✓ $ACT_SHA"

WORK="$(mktemp -d /tmp/wxsj_vc1637_install.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

echo "[1/8] 装机前产物门禁（JNI 配套 / 控件读取 / so）"
unzip -p "$APK" classes4.dex > "$WORK/classes4.dex"
[[ -s "$WORK/classes4.dex" ]] || { echo "ERROR: 包内无 classes4.dex"; exit 1; }
bash "$NIXIANG/04_scripts/_common/pre_install_gate.sh" \
  --cpp "$CPP" --dex "$WORK/classes4.dex" \
  --so "$SO" --menu-json "$MENU_JSON"

echo "[2/8] 记录卸载前官方包现场（仅只读）"
adb -s "$SERIAL" shell "dumpsys package $PKG" 2>/dev/null \
  | grep -E 'versionCode=|versionName=|firstInstallTime=' | sed 's/^/  /' || true
adb -s "$SERIAL" shell "su -c 'du -sh /data/data/$PKG 2>/dev/null'" 2>/dev/null | sed 's/^/  data: /' || true

echo "[3/8] force-stop 官方包"
adb -s "$SERIAL" shell "am force-stop $PKG" || true

echo "[4/8] 卸载官方包（--yes 已确认，私有数据将被清除）"
if [[ "$UPGRADE" == "1" ]]; then
  echo "  跳过：--upgrade 同签名覆盖升级，保留存档"
else
  adb -s "$SERIAL" shell "su -c 'pm uninstall $PKG'" | sed 's/^/  /'
  if adb -s "$SERIAL" shell "pm path $PKG" 2>/dev/null | grep -q 'package:'; then
    echo "ERROR: 卸载后仍能查到 $PKG，中止"; exit 1
  fi
  echo "  已卸载 ✓"
fi

echo "[5/8] 推送并安装星空包"
adb -s "$SERIAL" push "$APK" "$REMOTE"
adb -s "$SERIAL" shell "su -c \"pm install -r -d --user 0 '$REMOTE'\""
adb -s "$SERIAL" shell "su -c \"rm -f '$REMOTE'\"" 2>/dev/null || true

echo "[6/8] 装机完整性校验（设备 base.apk md5 == 本地产物）"
bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" --su

echo "[7/8] 装机后收尾（悬浮窗权限 + 卡密种子）"
# shellcheck source=/dev/null
source "$NIXIANG/04_scripts/_common/starry_post_install.sh"
starry_post_install "$PKG"

echo "[8/8] 安装后包信息"
adb -s "$SERIAL" shell "dumpsys package $PKG" 2>/dev/null \
  | grep -E 'versionCode=|versionName=|firstInstallTime=' | sed 's/^/  /' || true
echo "DONE $APK"
