#!/usr/bin/env bash
# 追迹迷城：收官清理。只删除本作可再生中间物和本次设备临时件。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
GAME="追迹迷城"
STATE_FILE="$SCRIPT_DIR/.device_serial"
SUMMARY="$SCRIPT_DIR/cleanup_summary.json"
ADB_BIN="${ADB_BIN:-adb}"

cd "$NIXIANG_ROOT"

[[ -d "02_apks/$GAME" ]] || { echo "错误: 缺少官方包目录 02_apks/$GAME" >&2; exit 1; }
[[ -d "08_output/$GAME" ]] || { echo "错误: 缺少正式包目录 08_output/$GAME" >&2; exit 1; }
[[ -f "06_dumped/$GAME/1.0.20260912_il2cpp/dump.cs" ]] || { echo "错误: 缺少需要保留的 dump.cs" >&2; exit 1; }
[[ -f "06_dumped/$GAME/1.0.20260912_il2cpp/stringliteral.json" ]] || { echo "错误: 缺少需要保留的 stringliteral.json" >&2; exit 1; }
[[ -f "06_dumped/$GAME/1.0.20260912_il2cpp/rva_targets.json" ]] || { echo "错误: 缺少需要保留的 rva_targets.json" >&2; exit 1; }

BEFORE_KB="$((du -sk 03_decompiled/$GAME 06_dumped/$GAME 07_patched/$GAME 2>/dev/null || true) | awk '{s+=$1} END {print s+0}')"
BEFORE_AT="$(date '+%Y-%m-%d %H:%M:%S')"

# Staged text survives scoped git clean below.
git add -f \
  "06_dumped/$GAME/1.0.20260912_il2cpp/dump.cs" \
  "06_dumped/$GAME/1.0.20260912_il2cpp/stringliteral.json" \
  "06_dumped/$GAME/1.0.20260912_il2cpp/rva_targets.json"

echo "==> dry-run: 06_dumped/$GAME"
git clean -xdn -- "06_dumped/$GAME" | sed -n '1,120p'

git clean -xdf -- "03_decompiled/$GAME"
git clean -xdf -- "07_patched/$GAME"
git clean -xdf -- "06_dumped/$GAME"
git clean -xdf -- "09_test/$GAME" 2>/dev/null || true

[[ -f "06_dumped/$GAME/1.0.20260912_il2cpp/dump.cs" ]] || { echo "错误: dump.cs 被误删" >&2; exit 1; }
[[ -f "06_dumped/$GAME/1.0.20260912_il2cpp/stringliteral.json" ]] || { echo "错误: stringliteral.json 被误删" >&2; exit 1; }
[[ -f "06_dumped/$GAME/1.0.20260912_il2cpp/rva_targets.json" ]] || { echo "错误: rva_targets.json 被误删" >&2; exit 1; }
[[ -d "02_apks/$GAME" ]] || { echo "错误: 02_apks 被误删" >&2; exit 1; }
[[ -d "08_output/$GAME" ]] || { echo "错误: 08_output 被误删" >&2; exit 1; }

find /tmp -maxdepth 1 -name 'zjmc_surgical.*' -print -exec rm -rf {} + 2>/dev/null || true
find /tmp -maxdepth 1 -name 'zjmc_manifest.*' -print -exec rm -f {} + 2>/dev/null || true

SERIAL=""
if [[ -f "$STATE_FILE" ]]; then
  SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
fi
if [[ -n "$SERIAL" ]] && "$ADB_BIN" -s "$SERIAL" get-state >/dev/null 2>&1; then
  echo "==> 设备本次临时文件（只清理本次两个明确路径）"
  "$ADB_BIN" -s "$SERIAL" shell ls -1 /data/local/tmp 2>/dev/null | tr -d '\r' | grep -E '^zjmc_' || true
  "$ADB_BIN" -s "$SERIAL" shell rm -f /data/local/tmp/zjmc_surgical.apk /data/local/tmp/zjmc_official_base.apk || true
  "$ADB_BIN" -s "$SERIAL" shell ls -1 /data/local/tmp 2>/dev/null | tr -d '\r' | grep -E '^zjmc_' || true
else
  echo "设备不在线或无 serial；跳过设备临时件清理"
fi

AFTER_KB="$((du -sk 03_decompiled/$GAME 06_dumped/$GAME 07_patched/$GAME 2>/dev/null || true) | awk '{s+=$1} END {print s+0}')"
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
        "02_apks/追迹迷城",
        "08_output/追迹迷城",
        "06_dumped/追迹迷城/1.0.20260912_il2cpp/dump.cs",
        "06_dumped/追迹迷城/1.0.20260912_il2cpp/stringliteral.json",
        "06_dumped/追迹迷城/1.0.20260912_il2cpp/rva_targets.json",
    ],
    "removed_roots": [
        "03_decompiled/追迹迷城",
        "07_patched/追迹迷城",
        "06_dumped/追迹迷城 ignored binaries",
    ],
}
Path(out).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

cat "$SUMMARY"
