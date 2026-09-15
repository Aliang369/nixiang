#!/usr/bin/env bash
# 追迹迷城 1.0.20260912 免广告修复包追加清理。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
SUMMARY="$SCRIPT_DIR/cleanup_fix_summary.json"
APK="$NIXIANG_ROOT/08_output/追迹迷城/追迹迷城_1.0.20260912_noAdFix-releaseSigned.apk"
OTA="$NIXIANG_ROOT/08_output/追迹迷城/ota/zjmc/libstarry_magic.so"
ADB_BIN="${ADB_BIN:-adb}"

[[ -f "$APK" ]] || { echo "错误: 修复 APK 不存在" >&2; exit 1; }
[[ -f "$OTA" ]] || { echo "错误: OTA so 不存在" >&2; exit 1; }

rm -rf "$NIXIANG_ROOT/mod菜单/starry-mod-menu/obj_zjmc"
rm -rf "$NIXIANG_ROOT/mod菜单/starry-mod-menu/libs_zjmc"
rm -rf /tmp/zjmc_no_ad_fix.* /tmp/zjmc_surgical.* /tmp/zjmc_manifest.* 2>/dev/null || true

SERIAL=""
if [[ -f "$STATE_FILE" ]]; then
  SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
fi
if [[ -n "$SERIAL" ]] && "$ADB_BIN" -s "$SERIAL" get-state >/dev/null 2>&1; then
  "$ADB_BIN" -s "$SERIAL" shell rm -f \
    /data/local/tmp/zjmc_no_ad_fix.apk \
    /data/local/tmp/zjmc_starry_base.apk \
    /data/local/tmp/zjmc_surgical.apk \
    /data/local/tmp/zjmc_official_base.apk || true
fi

python3 - "$SUMMARY" "$APK" "$OTA" <<'PY'
import hashlib, json, sys

def md5(path):
    h = hashlib.md5()
    with open(path, 'rb') as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b''):
            h.update(chunk)
    return h.hexdigest()
from pathlib import Path

out, apk, ota = sys.argv[1:]
data = {
    "game": "追迹迷城",
    "version": "1.0.20260912",
    "fix": "noAdFix",
    "kept": [apk, ota],
    "removed": [
        "mod菜单/starry-mod-menu/obj_zjmc",
        "mod菜单/starry-mod-menu/libs_zjmc",
        "/tmp/zjmc_* temporary files",
        "/data/local/tmp/zjmc_* temporary APKs",
    ],
    "apk_md5": md5(apk),
    "ota_so_md5": md5(ota),
}
Path(out).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

cat "$SUMMARY"
