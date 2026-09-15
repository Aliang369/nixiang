#!/usr/bin/env bash
# 偶像梦幻祭2 4.5.8861 收官清理：仅删除本作可再生中间物与设备专属临时 APK。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="偶像梦幻2"
VERSION="4.5.8861"
FINAL="$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_surgical-releaseSigned.apk"
OFFICIAL="$NIXIANG/02_apks/$GAME/${GAME}_${VERSION}_official_from_device.apk"
SERIAL_FILE="$NIXIANG/04_scripts/$GAME/.device_serial"
SUMMARY="$NIXIANG/04_scripts/$GAME/cleanup_summary.json"
SCRIPT_DIR="$NIXIANG/04_scripts/$GAME"

FINAL_MD5_EXPECTED="29598948a08e234b83cca24a4a7fbd1c"
OFFICIAL_MD5_EXPECTED="10c2b8993828ba5cd19ca6cceec8dd06"

[[ -f "$FINAL" ]] || { echo "ERROR: missing final APK: $FINAL" >&2; exit 1; }
[[ -f "$OFFICIAL" ]] || { echo "ERROR: missing official APK: $OFFICIAL" >&2; exit 1; }
[[ "$(md5 -q "$FINAL")" == "$FINAL_MD5_EXPECTED" ]] || { echo "ERROR: final APK md5 mismatch" >&2; exit 1; }
[[ "$(md5 -q "$OFFICIAL")" == "$OFFICIAL_MD5_EXPECTED" ]] || { echo "ERROR: official APK md5 mismatch" >&2; exit 1; }

size_of_path() {
  local p="$1"
  if [[ -e "$p" ]]; then
    du -sk "$p" | awk '{print $1 * 1024}'
  else
    echo 0
  fi
}

TARGETS=(
  "$NIXIANG/03_decompiled/$GAME"
  "$NIXIANG/07_patched/$GAME"
)

FREED_BYTES=0
for p in "${TARGETS[@]}"; do
  size="$(size_of_path "$p")"
  FREED_BYTES=$((FREED_BYTES + size))
done

safe_rm() {
  local p="$1"
  case "$p" in
    "$NIXIANG/03_decompiled/$GAME"|\
    "$NIXIANG/07_patched/$GAME")
      rm -rf -- "$p"
      ;;
    *)
      echo "ERROR: unsafe cleanup target: $p" >&2
      exit 1
      ;;
  esac
}

for p in "${TARGETS[@]}"; do
  safe_rm "$p"
done

DEVICE_TMP_BEFORE=""
[[ -f "$SERIAL_FILE" ]] || { echo "ERROR: missing serial file" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$SERIAL_FILE")"
for remote in /data/local/tmp/es2_starry.apk /data/local/tmp/es2_official_base.apk; do
  if adb -s "$SERIAL" shell "[ -e '$remote' ]"; then
    DEVICE_TMP_BEFORE="$DEVICE_TMP_BEFORE $remote"
    adb -s "$SERIAL" shell rm -f "$remote"
  fi
done

for remote in $DEVICE_TMP_BEFORE; do
  if adb -s "$SERIAL" shell "[ -e '$remote' ]"; then
    echo "ERROR: device temp still exists: $remote" >&2
    exit 1
  fi
done

DEVICE_BASE="$(adb -s "$SERIAL" shell "pm path com.happyelements.es2" | tr -d '\r' | sed -n 's/^package://p' | grep 'base.apk' | head -1)"
[[ -n "$DEVICE_BASE" ]] || { echo "ERROR: starry package is not installed" >&2; exit 1; }
DEVICE_MD5="$(adb -s "$SERIAL" shell "su -c 'md5sum $DEVICE_BASE'" | tr -d '\r\n' | awk '{print $1}')"
[[ "$DEVICE_MD5" == "$FINAL_MD5_EXPECTED" ]] || { echo "ERROR: device base.apk md5 changed: $DEVICE_MD5" >&2; exit 1; }

python3 - "$SUMMARY" "$FINAL" "$FINAL_MD5_EXPECTED" "$OFFICIAL" "$OFFICIAL_MD5_EXPECTED" \
  "$FREED_BYTES" "$SERIAL" "$DEVICE_BASE" "$DEVICE_MD5" "$DEVICE_TMP_BEFORE" <<'PY'
import json, sys
from pathlib import Path
(out, final, final_md5, official, official_md5, freed, serial, device_base,
 device_md5, removed_device_tmp) = sys.argv[1:]
data = {
    "game": "偶像梦幻2",
    "version": "4.5.8861",
    "status": "cleanup_done",
    "final_apk": final,
    "final_apk_md5": final_md5,
    "official_apk": official,
    "official_apk_md5": official_md5,
    "estimated_freed_bytes": int(freed),
    "removed_device_tmp": [x for x in removed_device_tmp.split() if x],
    "serial": serial,
    "device_base_apk": device_base,
    "device_base_apk_md5": device_md5,
    "preserved": [
        "02_apks/偶像梦幻2 official APK",
        "06_dumped/偶像梦幻2 large static inputs until post-archive cleanup",
        "08_output/偶像梦幻2 untouched (user-managed output directory)",
        "mod菜单/starry-mod-menu/libs_es2 build artifact",
        "10_docs/偶像梦幻2 analysis and landing documents",
        "04_scripts/偶像梦幻2 pipeline",
    ],
}
Path(out).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

echo "[cleanup] final_md5=$FINAL_MD5_EXPECTED"
echo "[cleanup] official_md5=$OFFICIAL_MD5_EXPECTED"
echo "[cleanup] estimated_freed_bytes=$FREED_BYTES"
echo "[cleanup] device_base_md5=$DEVICE_MD5"
echo "[cleanup] summary=$SUMMARY"
