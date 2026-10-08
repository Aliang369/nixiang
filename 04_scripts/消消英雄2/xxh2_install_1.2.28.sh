#!/usr/bin/env bash
# 消消英雄2 1.2.28 — 装机阶段（真机 f9a116a2）
#
# 真机原装的是**官方原版**（CN=Ranhao Kang），星光包是自签（CN=StarryMagic）
# ⇒ 覆盖安装必失败（INSTALL_FAILED_UPDATE_INCOMPATIBLE）。
# 按工作区红线：**不自动卸载**。默认只试覆盖安装并报告；
# 取得用户明确同意后再带 XXH2_ALLOW_UNINSTALL=1 重跑（会清掉官方包的游戏数据）。
#
# 用法:
#   bash 04_scripts/消消英雄2/xxh2_install_1.2.28.sh                 # 只试覆盖安装
#   XXH2_ALLOW_UNINSTALL=1 bash 04_scripts/消消英雄2/xxh2_install_1.2.28.sh   # 卸载官方后安装
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="消消英雄2"
VERSION="1.2.28"
PKG="com.blina.match2.tt"
OUT="$NIXIANG/08_output/$GAME"
# XXH2_APK 可指向 so-only 等变体包（用于诊断/迭代装机；默认装整包外科正式包）
SIGNED="${XXH2_APK:-$OUT/${GAME}_${VERSION}_starry-releaseSigned.apk}"
REMOTE="/data/local/tmp/xxh2_starry_${VERSION}.apk"

[[ -f "$SIGNED" ]] || { echo "ERROR: 缺正式包 $SIGNED" >&2; exit 1; }
adb get-serialno >/dev/null 2>&1 || { echo "ERROR: adb 不可用" >&2; exit 1; }

echo "--- 设备与现状 ---"
adb shell getprop ro.serialno
adb shell dumpsys package "$PKG" 2>/dev/null | grep -E "versionName|versionCode|firstInstallTime|lastUpdateTime" | head -4 || echo "  (未安装 $PKG)"
INSTALLED_MD5="$(adb shell su -c "md5sum \$(pm path $PKG | sed 's/package://')" 2>/dev/null | awk '{print $1}' | head -1 || true)"
echo "  设备当前 base.apk md5: ${INSTALLED_MD5:-未知}"

echo "--- push + 覆盖安装 ---"
adb push "$SIGNED" "$REMOTE" >/dev/null
set +e
OUT_INSTALL="$(adb shell su -c "pm install -r -d --user 0 '$REMOTE'" 2>&1)"
RC=$?
set -e
echo "$OUT_INSTALL"

if echo "$OUT_INSTALL" | grep -qE "INSTALL_FAILED_UPDATE_INCOMPATIBLE|signatures do not match|INSTALL_FAILED_ALREADY_EXISTS|INSTALL_PARSE_FAILED_INCONSISTENT_CERTIFICATES"; then
  echo
  echo "!! 异签名冲突：官方原版（CN=Ranhao Kang）无法被自签星光包覆盖。"
  if [[ "${XXH2_ALLOW_UNINSTALL:-0}" != "1" ]]; then
    echo "!! 默认不卸载（红线：不自动清数据/不卸载）。取得用户同意后重跑："
    echo "!!   XXH2_ALLOW_UNINSTALL=1 bash 04_scripts/消消英雄2/xxh2_install_1.2.28.sh"
    adb shell rm -f "$REMOTE" 2>/dev/null || true
    exit 3
  fi
  echo "!! XXH2_ALLOW_UNINSTALL=1 → 卸载官方包后安装星光包（本地游戏数据随之清空）"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  adb shell su -c "pm uninstall --user 0 '$PKG'" || true
  adb shell su -c "pm install -d --user 0 '$REMOTE'"
fi

adb shell rm -f "$REMOTE" 2>/dev/null || true

echo "--- 完整性校验（设备上的包 == 本地产物）---"
bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$(adb get-serialno)" "$PKG" "$SIGNED" --su || true

echo "--- 装机后公共收尾 ---"
# shellcheck source=/dev/null
source "$NIXIANG/04_scripts/_common/starry_post_install.sh"
starry_post_install "$PKG" || true

echo "--- 装机完成 ---"
ls -lh "$SIGNED"
echo "log: adb logcat -s StarryMagic:V StarrySignSpoof:V NativeMenu:V Match2:V | grep -E 'spoof hit|auth game_id|Match2|xxh2|native menu'"
echo "提示：请用户手动打开游戏（零截图红线），随后只读 logcat 核验锚点。"
