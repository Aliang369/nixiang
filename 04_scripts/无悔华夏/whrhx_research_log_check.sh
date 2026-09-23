#!/usr/bin/env bash
# 无悔华夏 · 科研热更修复 运行时校验
# 用途：清 logcat → 拉起游戏（不点屏、不卸载、不清数据）→ 抓 [Whrhx] 日志 → 判定科研钩子是否全就绪。
# 只读诊断，不修改设备任何状态。
set -uo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
SERIAL="${WHRHX_SERIAL:-f9a116a2}"
PKG="${WHRHX_PKG:-com.yhhy.whrhx.aligames}"
WAIT="${WHRHX_WAIT:-30}"
OUT_DIR="$NIXIANG/06_dumped/无悔华夏/current_20260923/logs"

mkdir -p "$OUT_DIR"
STAMP="$(date +%Y%m%d_%H%M%S)"
OUT="$OUT_DIR/whrhx_runtime_${STAMP}.log"

echo "[runtime-check] serial=$SERIAL pkg=$PKG wait=${WAIT}s"
adb -s "$SERIAL" logcat -c || { echo "ERROR: logcat -c failed" >&2; exit 1; }

echo "[runtime-check] launching app (monkey, 不点屏)…"
adb -s "$SERIAL" shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1

sleep "$WAIT"

adb -s "$SERIAL" logcat -d 2>/dev/null | LC_ALL=C grep -F "[Whrhx]" > "$OUT" || true
echo "[runtime-check] captured $(wc -l < "$OUT" | tr -d ' ') lines -> $OUT"
echo "---- 科研相关 ----"
LC_ALL=C grep -E "research|Policy|rva miss|minister|extra minister|ready|incomplete" "$OUT" || echo "(无科研相关行)"

echo "---- 判定 ----"
if LC_ALL=C grep -q "research hooks ready" "$OUT"; then
  LINE="$(LC_ALL=C grep "research hooks ready" "$OUT" | tail -1)"
  echo "READY: $LINE"
  echo "$LINE" | LC_ALL=C grep -q "execute=1" || echo "WARN: execute != 1"
  echo "$LINE" | LC_ALL=C grep -q "update=1"  || echo "WARN: update  != 1"
  echo "$LINE" | LC_ALL=C grep -q "min=1"     || echo "NOTE: 名臣入口 min=0（未命中，走按名兜底或待重校）"
  echo "$LINE" | LC_ALL=C grep -q "extra=1"   || echo "NOTE: 额外名臣入口 extra=0"
  exit 0
fi
if LC_ALL=C grep -q "research hooks incomplete" "$OUT"; then
  echo "INCOMPLETE: $(LC_ALL=C grep 'research hooks incomplete' "$OUT" | tail -1)"
  exit 2
fi
echo "NORUN: 未见 InstallResearchHooks 日志——菜单第 9 项可能未开启，或游戏尚未进到主流程"
exit 3
