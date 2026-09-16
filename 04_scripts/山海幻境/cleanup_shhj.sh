#!/usr/bin/env bash
# 山海幻境 4.0 收官清理：唯一正式包裁剪 + 本作滚动物/设备临时文件回收。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="山海幻境"
VERSION="4"
FINAL="$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_loginfix-releaseSigned.apk"
OFFICIAL="$NIXIANG/02_apks/$GAME/${GAME}_${VERSION}_official_from_device.apk"
SERIAL_FILE="$NIXIANG/04_scripts/$GAME/.device_serial"
PRUNE_MANIFEST="$NIXIANG/10_docs/$GAME/正式包裁剪清单_4_20260916.json"
SUMMARY="$NIXIANG/04_scripts/$GAME/cleanup_summary.json"
FINAL_MD5_EXPECTED="6bd976050b7571d71a9cd3597513427d"
OFFICIAL_MD5_EXPECTED="7d8a7085a271b4a6a649b56e758b8c6b"
PKG="com.chx.shhj"

[[ -f "$FINAL" ]] || { echo "错误: 正式包不存在: $FINAL" >&2; exit 1; }
[[ -f "$OFFICIAL" ]] || { echo "错误: 官包不存在: $OFFICIAL" >&2; exit 1; }
[[ "$(md5 -q "$FINAL")" == "$FINAL_MD5_EXPECTED" ]] || { echo "错误: 正式包 md5 不符" >&2; exit 1; }
[[ "$(md5 -q "$OFFICIAL")" == "$OFFICIAL_MD5_EXPECTED" ]] || { echo "错误: 官包 md5 不符" >&2; exit 1; }

TARGETS=(
  "$NIXIANG/03_decompiled/$GAME"
  "$NIXIANG/06_dumped/$GAME"
  "$NIXIANG/07_patched/$GAME"
)
OUT_DIR="$NIXIANG/08_output/$GAME"

for p in "${TARGETS[@]}"; do
  while IFS= read -r tracked; do
    [[ -z "$tracked" ]] || { echo "错误: 清理目标含 Git 跟踪文件，拒绝删除: $tracked" >&2; exit 1; }
  done < <(git -C "$NIXIANG" ls-files -- "$p")
done

measure_bytes() {
  local total=0 p size
  for p in "$@"; do
    if [[ -e "$p" ]]; then
      size="$(du -sk "$p" | awk '{print $1 * 1024}')"
      total=$((total + size))
    fi
  done
  printf '%s\n' "$total"
}

BEFORE_BYTES="$(measure_bytes "$OUT_DIR" "${TARGETS[@]}")"

bash "$NIXIANG/04_scripts/_common/prune_output_final.sh" \
  "$OUT_DIR" \
  "$(basename "$FINAL")" \
  --apply \
  --manifest "$PRUNE_MANIFEST"

for p in "${TARGETS[@]}"; do
  [[ ! -e "$p" ]] || rm -rf -- "$p"
done

DEVICE_TMP_BEFORE=""
[[ -f "$SERIAL_FILE" ]] || { echo "错误: 缺少设备状态文件" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$SERIAL_FILE")"
for remote in \
  /data/local/tmp/shhj_official_base.apk \
  /data/local/tmp/shhj_4_surgical.apk \
  /data/local/tmp/shhj_surgical.apk \
  /data/local/tmp/shhj_index_runtime.jsc
do
  if adb -s "$SERIAL" shell "[ -e '$remote' ]"; then
    DEVICE_TMP_BEFORE="$DEVICE_TMP_BEFORE $remote"
    adb -s "$SERIAL" shell rm -f "$remote"
  fi
done
for remote in $DEVICE_TMP_BEFORE; do
  if adb -s "$SERIAL" shell "[ -e '$remote' ]"; then
    echo "错误: 设备临时文件仍存在: $remote" >&2
    exit 1
  fi
done

DEVICE_BASE="$(adb -s "$SERIAL" shell pm path "$PKG" | tr -d '\r' | sed -n 's/^package://p' | grep 'base.apk' | head -1)"
[[ -n "$DEVICE_BASE" ]] || { echo "错误: 设备未安装 $PKG" >&2; exit 1; }
DEVICE_MD5="$(adb -s "$SERIAL" shell "su -c 'md5sum $DEVICE_BASE'" | tr -d '\r\n' | awk '{print $1}')"
[[ "$DEVICE_MD5" == "$FINAL_MD5_EXPECTED" ]] || { echo "错误: 设备 base.apk md5 不符: $DEVICE_MD5" >&2; exit 1; }

AFTER_BYTES="$(measure_bytes "$OUT_DIR" "${TARGETS[@]}")"
FREED_BYTES=$((BEFORE_BYTES - AFTER_BYTES))

python3 - "$SUMMARY" "$FINAL" "$FINAL_MD5_EXPECTED" "$OFFICIAL" "$OFFICIAL_MD5_EXPECTED" \
  "$PRUNE_MANIFEST" "$BEFORE_BYTES" "$AFTER_BYTES" "$FREED_BYTES" "$SERIAL" \
  "$DEVICE_BASE" "$DEVICE_MD5" "$DEVICE_TMP_BEFORE" <<'PY'
import json
import sys
from pathlib import Path

(out, final, final_md5, official, official_md5, prune_manifest,
 before, after, freed, serial, device_base, device_md5,
 removed_device_tmp) = sys.argv[1:]
data = {
    "game": "山海幻境",
    "version": "4 / vc54",
    "status": "cleanup_done",
    "final_apk": final,
    "final_apk_md5": final_md5,
    "official_apk": official,
    "official_apk_md5": official_md5,
    "prune_manifest": prune_manifest,
    "bytes_before": int(before),
    "bytes_after": int(after),
    "measured_freed_bytes": int(freed),
    "removed_device_tmp": [x for x in removed_device_tmp.split() if x],
    "serial": serial,
    "device_base_apk": device_base,
    "device_base_apk_md5": device_md5,
    "preserved": [
        "02_apks/山海幻境 official APK",
        "08_output/山海幻境 unique final APK",
        "04_scripts/山海幻境 pipeline and scripts",
        "10_docs/山海幻境 landing and acceptance documents",
        "mod菜单/starry-mod-menu/scripts/surgical_shhj_3.8.sh",
    ],
}
Path(out).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

echo "[cleanup] final_md5=$FINAL_MD5_EXPECTED"
echo "[cleanup] measured_freed_bytes=$FREED_BYTES"
echo "[cleanup] device_base_md5=$DEVICE_MD5"
echo "[cleanup] summary=$SUMMARY"
