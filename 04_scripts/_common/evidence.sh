#!/usr/bin/env bash
# ================================================================
#  evidence.sh — 参数化取证（把「现场 logcat 检索」收敛成一条命令）
#
#  用法:
#    bash 04_scripts/_common/evidence.sh --game 霓虹深渊2 \
#         [--serial f9a116a2] [--pkg com.veewo.neonabyss2] \
#         [--needle 'nhsy2|Starry'] [--needle 'install (pass|skip)'] ... \
#         [--source <已存的 logcat.txt>] [--buf-lines 200000] \
#         [--lines 40] [--dir <输出目录>] [--quiet]
#
#  动作（每次都落文件，终端只回显摘要）:
#    1) 取原始日志 → <dir>/logcat.txt   （默认 `adb -s <SERIAL> logcat -d`；--source 则复制该文件）
#    2) 每个 --needle 单独切一份 → <dir>/slice_<n>_<关键字>.txt
#    3) 打印 MOD 关键行统计（install pass / skip / 崩溃 / fault addr / nhsy2 / 菜单）
#    4) 只回显 <= --lines 行匹配内容（防刷屏）
#
#  为什么要有它（2026-10-12 霓虹深渊2 会话复盘 P1-11）:
#    那一轮在 bash 里现场 grep logcat **176 次 / 658 KB 输出**，同一份证据被重复取、重复刷屏，
#    结论却没落盘 —— 下一轮又从头 grep。现在：一条命令 → 一份带时间戳的证据目录 + 一屏摘要。
#
#  退出码: 0=至少一个 needle 命中（或未给 needle 且成功取到日志）；1=无命中；2=用法错
# ================================================================
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="${NIXIANG_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"
GAME=""; SERIAL=""; PKG=""; SRC=""; BUF=200000; SHOW=40; DIR=""; QUIET=0
NEEDLES=()

while [ $# -gt 0 ]; do
  case "$1" in
    --game) GAME="${2:-}"; shift 2 ;;
    --serial) SERIAL="${2:-}"; shift 2 ;;
    --pkg) PKG="${2:-}"; shift 2 ;;
    --needle) NEEDLES+=("${2:-}"); shift 2 ;;
    --source) SRC="${2:-}"; shift 2 ;;
    --buf-lines) BUF="${2:-200000}"; shift 2 ;;
    --lines) SHOW="${2:-40}"; shift 2 ;;
    --dir) DIR="${2:-}"; shift 2 ;;
    --quiet) QUIET=1; shift ;;
    -h|--help) sed -n '2,28p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
    *) echo "未知参数: $1" >&2; exit 2 ;;
  esac
done
[ -n "$GAME" ] || { echo "用法: bash 04_scripts/_common/evidence.sh --game <游戏> [--needle 正则]…" >&2; exit 2; }

TS="$(date +%Y%m%d_%H%M%S)"
[ -n "$DIR" ] || DIR="$ROOT/09_test/$GAME/evidence_$TS"
mkdir -p "$DIR"
RAW="$DIR/logcat.txt"

# ---------- 取日志 ----------
if [ -n "$SRC" ]; then
  [ -f "$SRC" ] || { echo "✗ --source 不存在: $SRC" >&2; exit 2; }
  cp -f "$SRC" "$RAW"
  echo "▶ 源：$SRC"
else
  command -v adb >/dev/null 2>&1 || { echo "✗ 找不到 adb（用 --source 指定已存日志）" >&2; exit 2; }
  SERIAL="${SERIAL:-$(adb devices | awk 'NR>1 && $2=="device"{print $1; exit}')}"
  [ -n "$SERIAL" ] || { echo "✗ 无在线设备（adb devices 空）；或传 --source" >&2; exit 2; }
  echo "▶ 源：adb -s $SERIAL logcat -d -t $BUF"
  adb -s "$SERIAL" logcat -d -t "$BUF" > "$RAW" 2>/dev/null
fi
LINES_TOTAL="$(wc -l < "$RAW" | tr -d ' ')"
BYTES="$(wc -c < "$RAW" | tr -d ' ')"
echo "▶ 原始日志：${RAW}（${LINES_TOTAL} 行 / ${BYTES} B）"

# ---------- 摘要（固定关注项）----------
summ() {
  local pat="$1" label="$2"
  local n
  n="$(grep -cE "$pat" "$RAW" 2>/dev/null | tr -d ' ' || true)"
  [ -n "$n" ] || n=0
  printf '  %-34s %s\n' "$label" "$n"
}
echo "── MOD / 崩溃 关键行计数 ──"
summ 'install pass' 'install pass（槽位安装）'
summ 'install skip' 'install skip（入口形态不安全被跳过）'
summ 'nhsy2' '[nhsy2] 自有日志'
summ 'libstarry_magic' 'libstarry_magic.so'
summ 'Fatal signal|SIGSEGV|SIGABRT|fault addr' '致命信号 / fault addr'
summ 'Cause: ' 'tombstone Cause:'
summ 'Start proc|MenuService|ModBridge' '菜单 / 桥接'
summ 'async_safe|护盾' 'async_safe 护盾'
[ -n "$PKG" ] && summ "$(printf '%s' "$PKG" | sed 's/\./\\./g')" "包名 $PKG"

# ---------- 逐 needle 切片 ----------
RC=0; i=0
for pat in ${NEEDLES+"${NEEDLES[@]}"}; do
  i=$((i+1))
  safe="$(printf '%s' "$pat" | tr -c 'A-Za-z0-9._-' '_' | cut -c1-32)"
  OUT="$DIR/slice_${i}_${safe}.txt"
  grep -nE "$pat" "$RAW" > "$OUT" 2>/dev/null || true
  N="$(wc -l < "$OUT" | tr -d ' ')"
  echo "── needle[$i] 「${pat}」 → ${OUT}（${N} 行）"
  [ "$N" = "0" ] && RC=1
  if [ "$QUIET" != "1" ] && [ "$N" != "0" ]; then
    sed -n "1,${SHOW}p" "$OUT" | sed 's/^/    /'
    [ "$N" -gt "$SHOW" ] && echo "    …（共 ${N} 行，全量见上文件）"
  fi
done

{
  echo "t=$TS"
  echo "game=$GAME"
  echo "serial=${SERIAL:-或 --source}"
  echo "pkg=${PKG:-未传}"
  echo "raw=$RAW lines=$LINES_TOTAL bytes=$BYTES"
  echo "needles=${NEEDLES[*]:-（无）}"
} > "$DIR/meta.txt"

echo
echo "▶ 证据目录：${DIR}（logcat.txt / slice_*.txt / meta.txt）"
echo "▶ 结论请回写：bash 04_scripts/_common/rootcause_note.sh --game $GAME --feature <功能#> --round <n> --cause '…' --evidence '$DIR/slice_1_….txt:<行号>' --verify '…'"
[ "$i" = "0" ] && RC=0
exit "$RC"
