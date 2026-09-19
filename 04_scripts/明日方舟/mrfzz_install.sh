#!/usr/bin/env bash
# 明日方舟 2.7.71 - 装机阶段（pipeline install）
#
# 默认只做同签名覆盖安装。TapTap 官方包为 HyperGryph 签名，星光包为
# StarryMagic 签名，异签覆盖会失败；卸载/清数据需 MRFZZ_ALLOW_UNINSTALL=1。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
source "$NIXIANG/04_scripts/_common/env.sh"
GAME="明日方舟"
VERSION="2.7.71"
PKG="com.hypergryph.arknights"
SIGNED="$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_surgical-releaseSigned.apk"
STATE_FILE="$SCRIPT_DIR/.device_serial"
OK_STAMP="$SCRIPT_DIR/.install_ok"
ADB_BIN="${ADB_BIN:-adb}"

[[ -f "$SIGNED" ]] || { echo "ERROR: missing signed apk $SIGNED" >&2; exit 1; }
[[ -f "$STATE_FILE" ]] || { echo "ERROR: missing $STATE_FILE; run device_ready first" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
[[ -n "$SERIAL" ]] || { echo "ERROR: empty device serial" >&2; exit 1; }
command -v "$ADB_BIN" >/dev/null || { echo "ERROR: adb unavailable" >&2; exit 1; }
"$ADB_BIN" -s "$SERIAL" get-state >/dev/null 2>&1 \
  || { echo "ERROR: device $SERIAL offline" >&2; exit 1; }

REMOTE="/data/local/tmp/mrfzz_install.apk"
echo "==> push $SIGNED"
"$ADB_BIN" -s "$SERIAL" push "$SIGNED" "$REMOTE"
"$ADB_BIN" -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true

echo "==> same-sign cover install"
if "$ADB_BIN" -s "$SERIAL" shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" \
     2>&1 | tee /tmp/mrfzz_install_out.txt | grep -q "Success"; then
  echo "  cover install ok"
else
  if grep -q "INSTALL_FAILED_UPDATE_INCOMPATIBLE\|signatures do not match" \
       /tmp/mrfzz_install_out.txt; then
    if [[ "${MRFZZ_ALLOW_UNINSTALL:-0}" != "1" ]]; then
      "$ADB_BIN" -s "$SERIAL" shell "rm -f $REMOTE" 2>/dev/null || true
      cat >&2 <<'EOF'
ERROR: official package has a different signer; cover install refused.

Uninstalling clears local game data. Re-run only with explicit approval:
  MRFZZ_ALLOW_UNINSTALL=1 bash 04_scripts/明日方舟/mrfzz_install.sh
EOF
      exit 2
    fi
    echo "==> explicit approval: uninstall official package and install clean"
    "$ADB_BIN" -s "$SERIAL" shell "pm uninstall --user 0 $PKG" 2>/dev/null \
      || "$ADB_BIN" -s "$SERIAL" uninstall "$PKG" || true
    "$ADB_BIN" -s "$SERIAL" shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" \
      | grep -q "Success" \
      || { echo "ERROR: install after uninstall failed" >&2; exit 1; }
  else
    echo "ERROR: install failed; see output above" >&2
    exit 1
  fi
fi
"$ADB_BIN" -s "$SERIAL" shell "rm -f $REMOTE" 2>/dev/null || true

echo "==> verify installed apk"
bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" \
  "$SERIAL" "$PKG" "$SIGNED" --su \
  || { echo "ERROR: installed package does not match local apk" >&2; exit 1; }

SEED_SH="$NIXIANG/04_scripts/_common/ensure_starry_device_id_seed.sh"
[[ -f "$SEED_SH" ]] && bash "$SEED_SH" 2>/dev/null || true

{
  echo "installed_at=$(date '+%Y-%m-%d %H:%M:%S')"
  echo "serial=$SERIAL"
  echo "apk=$SIGNED"
  echo "apk_md5=$(md5 -q "$SIGNED")"
} > "$OK_STAMP"

echo "install complete"
echo "  device=$SERIAL"
echo "  package=$PKG"
echo "  version=$VERSION"
echo "  apk_md5=$(md5 -q "$SIGNED")"
