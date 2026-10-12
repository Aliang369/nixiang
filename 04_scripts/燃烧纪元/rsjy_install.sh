#!/usr/bin/env bash
# 燃烧纪元星光候选包装机脚本。
# 星光同签包可直接覆盖；仅在当前为官方异签包时，必须显式设置 RSJY_ALLOW_UNINSTALL=1 才允许卸载并清数据。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
SERIAL="${SERIAL:-f9a116a2}"
PKG="com.rsjy.yf"
APK="$NIXIANG/08_output/燃烧纪元/燃烧纪元_1.13.09_starry-releaseSigned.apk"
OFFICIAL_SHA1="9dae8d5cc8fba8b23b702e06b87e6cd225d503ea"
STARRY_SHA1="821c92e404ab5fdac8dbf157c2e467e71440917d"
BT="$HOME/Library/Android/sdk/build-tools/36.0.0"

[[ -f "$APK" ]] || { echo "ERROR: missing $APK" >&2; exit 1; }
[[ -x "$BT/apksigner" ]] || { echo "ERROR: apksigner missing" >&2; exit 1; }

adb -s "$SERIAL" get-state >/dev/null
CUR_PATH="$(adb -s "$SERIAL" shell pm path "$PKG" | head -1 | sed 's/^package://' | tr -d '\r')"
if [[ -n "$CUR_PATH" ]]; then
  TMP_APK="$(mktemp /tmp/rsjy_current.XXXXXX.apk)"
  adb -s "$SERIAL" exec-out "su -c 'cat $CUR_PATH'" > "$TMP_APK"
  CUR_SHA1="$("$BT/apksigner" verify --print-certs "$TMP_APK" 2>/dev/null | sed -n 's/.*certificate SHA-1 digest: //p' | head -1 | tr 'A-F' 'a-f')"
  rm -f "$TMP_APK"
  if [[ "$CUR_SHA1" == "$OFFICIAL_SHA1" ]]; then
    if [[ "${RSJY_ALLOW_UNINSTALL:-0}" != "1" ]]; then
      echo "PAUSED: 当前为官方 ${PKG}，覆盖安装会因签名不一致失败。若确认允许卸载并清数据，请以 RSJY_ALLOW_UNINSTALL=1 重跑。" >&2
      exit 2
    fi
    echo "[install] 卸载已知官方包 ${PKG}（会清数据）"
    adb -s "$SERIAL" shell am force-stop "$PKG"
    adb -s "$SERIAL" shell pm uninstall --user 0 "$PKG"
  elif [[ "$CUR_SHA1" == "$STARRY_SHA1" ]]; then
    echo "[install] 当前已是星光同签包，force-stop 后覆盖安装（不清数据）"
    adb -s "$SERIAL" shell am force-stop "$PKG"
  else
    echo "ERROR: 当前已安装包签名 SHA-1=${CUR_SHA1}，既非官方也非星光，拒绝自动处理。" >&2
    exit 1
  fi
else
  echo "[install] 设备上无 ${PKG}，直接安装"
fi

adb -s "$SERIAL" install -r -d "$APK"
bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" --su
echo "[install] done"
