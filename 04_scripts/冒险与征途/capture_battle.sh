#!/usr/bin/env bash
# 被动崩溃监视 + 战斗期日志落盘（AI 不点屏、不截图；用户手动测）
# 用法: bash capture_battle.sh <tag> <seconds>
set -u
TAG="${1:-26h}"
SECS="${2:-1800}"
SERIAL="${SERIAL:-127.0.0.1:16384}"
OUT="${LOCALAPPDATA:-/c/Users/Administrator/AppData/Local}/Temp/mxyzt_battle_${TAG}.txt"
ADB="${ADB:-/c/windows/system32/adb}"
"$ADB" -s "$SERIAL" logcat -c
echo "[capture] tag=${TAG} secs=${SECS} -> ${OUT}"
timeout "$SECS" "$ADB" -s "$SERIAL" logcat -v threadtime \
  '*:S' Mod_Menu:V CRASH:V libc:V DEBUG:V scudo:V Zygote:V ActivityManager:V Unity:V > "$OUT" 2>&1
echo "[capture] done: $(wc -l < "$OUT") lines"
for p in CRASH 'Fatal signal' 'ACT+' 'ACT?' KILLSKIP SKILLHP SKILLDMG INVULN HPLOCK APPLY 'feat '; do
  printf '%s = %s\n' "$p" "$(grep -c "$p" "$OUT" 2>/dev/null || echo 0)"
done
