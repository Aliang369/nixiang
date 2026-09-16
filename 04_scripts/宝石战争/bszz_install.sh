#!/usr/bin/env bash
# 宝石战争 6.0.012：默认覆盖安装；BSZZ_ALLOW_UNINSTALL=1 时按用户明确授权卸载后安装。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
PKG="com.acing.gow.adv"
SIGNED="$NIXIANG_ROOT/08_output/宝石战争/宝石战争_6.0.012_surgical-releaseSigned.apk"
RESULT="$SCRIPT_DIR/install_result.txt"
ADB_BIN="${ADB_BIN:-adb}"

[[ -f "$STATE_FILE" ]] || { echo "ERROR: missing device serial" >&2; exit 1; }
[[ -f "$SIGNED" ]] || { echo "ERROR: missing $SIGNED" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
LOCAL_MD5="$(md5 -q "$SIGNED")"

{
  echo "time=$(date '+%Y-%m-%d %H:%M:%S %z')"
  echo "serial=$SERIAL"
  echo "apk=$SIGNED"
  echo "local_md5=$LOCAL_MD5"
} > "$RESULT"

if [[ "${BSZZ_ALLOW_UNINSTALL:-0}" == "1" ]]; then
  MODE="uninstall_install_authorized"
  echo "mode=uninstall_install_authorized" >> "$RESULT"
  "$ADB_BIN" -s "$SERIAL" shell am force-stop "$PKG" >/dev/null 2>&1 || true
  set +e
  UNINSTALL_OUT="$("$ADB_BIN" -s "$SERIAL" uninstall "$PKG" 2>&1)"
  UNINSTALL_RC=$?
  set -e
  printf '%s\n' "$UNINSTALL_OUT" >> "$RESULT"
  echo "uninstall_rc=$UNINSTALL_RC" >> "$RESULT"
  if [[ $UNINSTALL_RC -ne 0 ]]; then
    cat "$RESULT"
    exit $UNINSTALL_RC
  fi
else
  MODE="overwrite_only_no_uninstall_no_cleardata"
  echo "mode=overwrite_only_no_uninstall_no_cleardata" >> "$RESULT"
fi

set +e
INSTALL_OUT="$("$ADB_BIN" -s "$SERIAL" install -r -d "$SIGNED" 2>&1)"
RC=$?
set -e
printf '%s\n' "$INSTALL_OUT" >> "$RESULT"
echo "install_rc=$RC" >> "$RESULT"
if [[ $RC -ne 0 ]]; then
  if printf '%s\n' "$INSTALL_OUT" | grep -qi 'UPDATE_INCOMPATIBLE\|signatures do not match\|签名不一致'; then
    echo "覆盖安装被签名校验拒绝；未卸载、未清数据。" >> "$RESULT"
  fi
  if [[ "$MODE" == "uninstall_install_authorized" ]]; then
    echo "卸载安装失败：原版已按授权卸载，星光包安装未成功。" >> "$RESULT"
  fi
  cat "$RESULT"
  exit $RC
fi

bash "$NIXIANG_ROOT/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$SIGNED" --su | tee -a "$RESULT"
if [[ "$MODE" == "uninstall_install_authorized" ]]; then
  echo "安装成功（卸载安装模式：原版本地应用数据已在卸载时清除）。" >> "$RESULT"
else
  echo "覆盖安装成功；未清数据。" >> "$RESULT"
fi
cat "$RESULT"
