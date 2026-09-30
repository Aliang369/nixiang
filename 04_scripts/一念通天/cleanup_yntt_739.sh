#!/usr/bin/env bash
# 一念通天：739 收官清理。只删除本作可再生的本地中间物，不处理 02_apks/08_output。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
GAME="一念通天"
FINAL_APK="$NIXIANG_ROOT/08_output/$GAME/${GAME}_739_surgical-releaseSigned.apk"
FINAL_MD5="c5a01bc79a623dd64f9c8f6239b46b62"
SUMMARY="$SCRIPT_DIR/cleanup_summary.json"
SERIAL_FILE="$SCRIPT_DIR/.device_serial"
ADB_BIN="${ADB_BIN:-adb}"

cd "$NIXIANG_ROOT"

[[ -f "$FINAL_APK" ]] || { echo "错误: 缺少正式包 $FINAL_APK" >&2; exit 1; }
[[ "$(md5 -q "$FINAL_APK")" == "$FINAL_MD5" ]] || { echo "错误: 正式包指纹与收尾记录不符" >&2; exit 1; }

# 06_dumped 整份 dump 按 2026-09-28 收紧规则不入库；这里保留既有 719/720/728 文本档案。
KEEP_FILES=(
  "06_dumped/$GAME/official_728/static/il2cpp_dump/dump.cs"
  "06_dumped/$GAME/official_728/static/il2cpp_dump/script.json"
  "06_dumped/$GAME/official_728/static/il2cpp_dump/stringliteral.json"
  "06_dumped/$GAME/official_728/static/summary.txt"
  "06_dumped/$GAME/official_720/static/il2cpp_dump/dump.cs"
  "06_dumped/$GAME/official_720/static/il2cpp_dump/script.json"
  "06_dumped/$GAME/official_720/static/il2cpp_dump/stringliteral.json"
  "06_dumped/$GAME/official_720/static/summary.txt"
  "06_dumped/$GAME/binge_719/static/il2cpp_dump/dump.cs"
  "06_dumped/$GAME/binge_719/static/il2cpp_dump/script.json"
  "06_dumped/$GAME/binge_719/static/il2cpp_dump/stringliteral.json"
  "06_dumped/$GAME/binge_719/static/summary.txt"
  "06_dumped/$GAME/binge_719/memdump_initial/il2cpp_memdiff_report.txt"
  "06_dumped/$GAME/binge_719/memdump_initial/il2cpp_patch_count.txt"
  "06_dumped/$GAME/binge_719/memdump_kill_on/il2cpp_memdiff_report.txt"
  "06_dumped/$GAME/binge_719/memdump_kill_on/il2cpp_patch_count.txt"
  "06_dumped/$GAME/binge_719/memdump_all_on/il2cpp_memdiff_report.txt"
  "06_dumped/$GAME/binge_719/memdump_all_on/il2cpp_patch_count.txt"
  "06_dumped/$GAME/binge_719/memdump_restart_all_on/il2cpp_memdiff_report.txt"
  "06_dumped/$GAME/binge_719/memdump_restart_all_on/il2cpp_patch_count.txt"
)
for keep in "${KEEP_FILES[@]}"; do
  [[ -f "$keep" ]] || { echo "错误: 缺少需保留文本 $keep" >&2; exit 1; }
  git add -- "$keep"
done

if git diff --cached --name-only -- "06_dumped/$GAME" | grep -E '\.(apk|ap_|so|o|jar|bin|dat|dex|i64|idb)$' >/dev/null; then
  echo "错误: 06_dumped 暂存区出现大二进制，拒绝清理" >&2
  exit 1
fi

BEFORE_KB="$( (du -sk "03_decompiled/$GAME" "06_dumped/$GAME" "07_patched/$GAME" 2>/dev/null || true) | awk '{s+=$1} END {print s+0}')"
BEFORE_AT="$(date '+%Y-%m-%d %H:%M:%S')"

echo "==> dry-run: 本作可再生中间物"
git clean -xdn -- "03_decompiled/$GAME" "06_dumped/$GAME" "07_patched/$GAME" | sed -n '1,240p'

git clean -xdf -- "03_decompiled/$GAME"
git clean -xdf -- "07_patched/$GAME"
git clean -xdf -- "06_dumped/$GAME"

for keep in "${KEEP_FILES[@]}"; do
  [[ -f "$keep" ]] || { echo "错误: 保留文本被误删 $keep" >&2; exit 1; }
done
[[ -d "02_apks/$GAME" ]] || { echo "错误: 02_apks 被误删" >&2; exit 1; }
[[ -d "08_output/$GAME" ]] || { echo "错误: 08_output 被误删" >&2; exit 1; }
[[ -d "09_test/$GAME" ]] || { echo "错误: 09_test 被误删" >&2; exit 1; }

find /tmp -maxdepth 1 -name 'yntt_surgical.*' -print -exec rm -rf {} + 2>/dev/null || true
find /tmp -maxdepth 1 -name 'yntt_sign.*' -print -exec rm -rf {} + 2>/dev/null || true
find /tmp -maxdepth 1 -name 'yntt_gate.*' -print -exec rm -rf {} + 2>/dev/null || true

SERIAL=""
if [[ -f "$SERIAL_FILE" ]]; then
  SERIAL="$(tr -d '[:space:]' < "$SERIAL_FILE")"
fi
if [[ -n "$SERIAL" ]] && "$ADB_BIN" -s "$SERIAL" get-state >/dev/null 2>&1; then
  echo "==> 设备本次临时文件（只清理明确路径）"
  "$ADB_BIN" -s "$SERIAL" shell rm -f /data/local/tmp/yntt_surgical_739.apk || true
  "$ADB_BIN" -s "$SERIAL" shell ls -1 /data/local/tmp 2>/dev/null | tr -d '\r' | grep -E '^yntt_' || true
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
    "version": "739",
    "before_kb": int(before_kb),
    "after_kb": int(after_kb),
    "released_kb": max(0, int(before_kb) - int(after_kb)),
    "before_at": before_at,
    "after_at": after_at,
    "kept": [
        "02_apks/一念通天（用户管理，未动）",
        "08_output/一念通天（正式包由 prune_output_final.sh 单独裁剪）",
        "06_dumped/一念通天 text archives (official_728 + official_720 + binge_719)",
        "06_dumped/一念通天/official_739/rva_targets.json 锚点摘录",
        "09_test/一念通天（728 卸载前备份 + 739 证据）",
        "04_scripts/一念通天",
        "10_docs/一念通天",
        "mod菜单 Yntt sources",
    ],
    "removed_roots": [
        "03_decompiled/一念通天",
        "07_patched/一念通天",
        "06_dumped/一念通天/official_739 ignored binaries (dump.cs/script.json/il2cpp.h/stringliteral.json/DummyDll)",
        "tmp yntt analysis directories",
    ],
}
Path(out).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

cat "$SUMMARY"
