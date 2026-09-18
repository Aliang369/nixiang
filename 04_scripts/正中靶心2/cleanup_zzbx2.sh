#!/usr/bin/env bash
# 正中靶心2：收官清理。只删除本作可再生中间物和本次设备临时 APK。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
GAME="正中靶心2"
STATE_FILE="$SCRIPT_DIR/.device_serial"
SUMMARY="$SCRIPT_DIR/cleanup_summary.json"
FINAL_APK="$NIXIANG_ROOT/08_output/$GAME/${GAME}_1.7_starry-releaseSigned.apk"
FINAL_MD5="4a27592877bed5e9ce149e1acfa5da62"
ADB_BIN="${ADB_BIN:-adb}"

cd "$NIXIANG_ROOT"

[[ -d "08_output/$GAME" ]] || { echo "错误: 缺少正式包目录 08_output/$GAME" >&2; exit 1; }
[[ -f "$FINAL_APK" ]] || { echo "错误: 缺少正式包 $FINAL_APK" >&2; exit 1; }
[[ "$(md5 -q "$FINAL_APK")" == "$FINAL_MD5" ]] || { echo "错误: 正式包指纹与收尾记录不符" >&2; exit 1; }

for keep in \
  "06_dumped/$GAME/official_1.7/decrypted_js/src/assets/Script/src/GameLogic.js" \
  "06_dumped/$GAME/official_1.7/decrypted_js/decrypt_summary.json" \
  "06_dumped/$GAME/official_1.7/zip_manifest.txt"; do
  [[ -f "$keep" ]] || { echo "错误: 缺少需保留文本 $keep" >&2; exit 1; }
done

BEFORE_KB="$( (du -sk "03_decompiled/$GAME" "06_dumped/$GAME" "07_patched/$GAME" 2>/dev/null || true) | awk '{s+=$1} END {print s+0}')"
BEFORE_AT="$(date '+%Y-%m-%d %H:%M:%S')"

# 先把文本白名单纳入 index；git clean 不会删除 tracked/staged 文件。
git add -A -- "06_dumped/$GAME"

if git diff --cached --name-only -- "06_dumped/$GAME" | grep -E '\.(apk|ap_|so|o|jar|bin|dat|dex|i64|idb)$' >/dev/null; then
  echo "错误: 06_dumped 暂存区出现大二进制，拒绝清理" >&2
  exit 1
fi

echo "==> dry-run: 06_dumped/$GAME ignored 大件"
git clean -xdn -- "06_dumped/$GAME" | sed -n '1,160p'

git clean -xdf -- "03_decompiled/$GAME"
git clean -xdf -- "07_patched/$GAME"
git clean -xdf -- "06_dumped/$GAME"
git clean -xdf -- "09_test/$GAME" 2>/dev/null || true

[[ -f "06_dumped/$GAME/official_1.7/decrypted_js/src/assets/Script/src/GameLogic.js" ]] || { echo "错误: GameLogic.js 被误删" >&2; exit 1; }
[[ -f "06_dumped/$GAME/official_1.7/decrypted_js/decrypt_summary.json" ]] || { echo "错误: decrypt_summary.json 被误删" >&2; exit 1; }
[[ -f "06_dumped/$GAME/official_1.7/zip_manifest.txt" ]] || { echo "错误: zip_manifest.txt 被误删" >&2; exit 1; }
[[ -d "08_output/$GAME" ]] || { echo "错误: 08_output 被误删" >&2; exit 1; }

find /tmp -maxdepth 1 -name 'zzbx2_surgical.*' -print -exec rm -rf {} + 2>/dev/null || true
find /tmp -maxdepth 1 -name 'zzbx2_manifest.*' -print -exec rm -f {} + 2>/dev/null || true

SERIAL=""
if [[ -f "$STATE_FILE" ]]; then
  SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
fi
if [[ -n "$SERIAL" ]] && "$ADB_BIN" -s "$SERIAL" get-state >/dev/null 2>&1; then
  echo "==> 设备本次临时文件（只清理明确路径）"
  "$ADB_BIN" -s "$SERIAL" shell rm -f /data/local/tmp/zzbx2_1.7_starry.apk || true
  "$ADB_BIN" -s "$SERIAL" shell rm -f /data/local/tmp/zzbx2_probe.bin || true
  "$ADB_BIN" -s "$SERIAL" shell ls -1 /data/local/tmp 2>/dev/null | tr -d '\r' | grep -E '^zzbx2_' || true
else
  echo "设备不在线或无 serial；跳过设备临时件清理"
fi

AFTER_KB="$( (du -sk "03_decompiled/$GAME" "06_dumped/$GAME" "07_patched/$GAME" 2>/dev/null || true) | awk '{s+=$1} END {print s+0}')"
AFTER_AT="$(date '+%Y-%m-%d %H:%M:%S')"
python3 - "$SUMMARY" "$GAME" "$BEFORE_KB" "$AFTER_KB" "$BEFORE_AT" "$AFTER_AT" <<'PY'
import json
import sys
from pathlib import Path

out, game, before_kb, after_kb, before_at, after_at = sys.argv[1:]
data = {
    "game": game,
    "before_kb": int(before_kb),
    "after_kb": int(after_kb),
    "released_kb": max(0, int(before_kb) - int(after_kb)),
    "before_at": before_at,
    "after_at": after_at,
    "kept": [
        "02_apks/正中靶心2 (user-managed, untouched or absent)",
        "08_output/正中靶心2",
        "06_dumped/正中靶心2 text assets",
        "04_scripts/正中靶心2",
        "10_docs/正中靶心2",
        "mod菜单 Zzbx2 sources",
    ],
    "removed_roots": [
        "03_decompiled/正中靶心2",
        "07_patched/正中靶心2",
        "06_dumped/正中靶心2 ignored binaries",
    ],
}
Path(out).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

cat "$SUMMARY"
