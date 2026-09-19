#!/usr/bin/env bash
# 弓箭传说2 1.1.9 — 装机阶段（pipeline install）
#
# 默认只做「同签名覆盖安装」；官方包是 Habby 签名、星光包是 StarryMagic 签名，
# 异签覆盖必然失败 → 卸载/清数据属破坏性操作，需 GJC2_ALLOW_UNINSTALL=1 明确授权。
#
# 用法:
#   bash gjc2_install.sh                          # 只同签覆盖（异签会失败并提示）
#   GJC2_ALLOW_UNINSTALL=1 bash gjc2_install.sh   # 经授权卸载后全新安装
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
source "$NIXIANG/04_scripts/_common/env.sh"
GAME="弓箭传说2"
VERSION="1.1.9"
PKG="com.xq.archeroii.ua"
SIGNED="$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_surgical-releaseSigned.apk"
STATE_FILE="$SCRIPT_DIR/.device_serial"
OK_STAMP="$SCRIPT_DIR/.install_ok"
ADB_BIN="${ADB_BIN:-adb}"

[[ -f "$SIGNED" ]] || { echo "错误: 缺少签名包 $SIGNED" >&2; exit 1; }
[[ -f "$STATE_FILE" ]] || { echo "错误: 缺少设备状态文件，请先跑 device_ready 阶段" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
[[ -n "$SERIAL" ]] || { echo "错误: 设备 serial 为空" >&2; exit 1; }
command -v "$ADB_BIN" >/dev/null || { echo "错误: adb 不可用" >&2; exit 1; }

"$ADB_BIN" -s "$SERIAL" get-state >/dev/null 2>&1 \
  || { echo "错误: 设备 $SERIAL 不在线" >&2; exit 1; }

REMOTE="/data/local/tmp/gjc2_install.apk"
echo "==> push $SIGNED"
"$ADB_BIN" -s "$SERIAL" push "$SIGNED" "$REMOTE"
"$ADB_BIN" -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true

echo "==> 同签名覆盖安装"
if "$ADB_BIN" -s "$SERIAL" shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" 2>&1 | tee /tmp/gjc2_install_out.txt | grep -q "Success"; then
  echo "  覆盖安装成功"
else
  if grep -q "INSTALL_FAILED_UPDATE_INCOMPATIBLE\|signatures do not match" /tmp/gjc2_install_out.txt; then
    if [[ "${GJC2_ALLOW_UNINSTALL:-0}" != "1" ]]; then
      "$ADB_BIN" -s "$SERIAL" shell "rm -f $REMOTE" 2>/dev/null || true
      cat >&2 <<'EOF'
错误: 签名与设备上官方包不一致，覆盖安装被拒。

卸载原版会清除游戏本地数据（账号进度在 Habby 云端，但本地缓存/继续局快照会丢）。
需明确授权后重跑:
  GJC2_ALLOW_UNINSTALL=1 bash 04_scripts/弓箭传说2/gjc2_install.sh
EOF
      exit 2
    fi
    echo "==> 已授权：卸载官方包 + 清数据后全新安装"
    "$ADB_BIN" -s "$SERIAL" shell "pm uninstall $PKG" 2>/dev/null \
      || "$ADB_BIN" -s "$SERIAL" uninstall "$PKG" || true
    "$ADB_BIN" -s "$SERIAL" shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" | grep -q "Success" \
      || { echo "错误: 卸载后安装失败" >&2; exit 1; }
  else
    echo "错误: 安装失败（见上方输出）" >&2
    exit 1
  fi
fi
"$ADB_BIN" -s "$SERIAL" shell "rm -f $REMOTE" 2>/dev/null || true

echo "==> 设备包指纹闭环校验"
bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$SIGNED" --su \
  || { echo "错误: 设备包与本地产物不一致（装的是旧包）" >&2; exit 1; }

# 签名种子（鉴权设备指纹用）
# 签名种子（鉴权设备指纹用；脚本自带设备 serial 解析，无需传参）
SEED_SH="$NIXIANG/04_scripts/_common/ensure_starry_device_id_seed.sh"
[[ -f "$SEED_SH" ]] && bash "$SEED_SH" 2>/dev/null || true

{
  echo "installed_at=$(date '+%Y-%m-%d %H:%M:%S')"
  echo "serial=$SERIAL"
  echo "apk=$SIGNED"
  echo "apk_md5=$(md5 -q "$SIGNED")"
} > "$OK_STAMP"

cat <<EOF
装机完成
  device=$SERIAL
  package=$PKG
  version=$VERSION
  apk_md5=$(md5 -q "$SIGNED")
  log: adb -s $SERIAL logcat -s StarryMagic:V StarryApp:V StarryGjc2:V Gjc2:V MenuService:V
EOF
