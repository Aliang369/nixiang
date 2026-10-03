#!/usr/bin/env bash
# 境界守望者 真机冒烟/取证：清日志 → 放大环形缓冲 → 重启游戏 → 双路抓 logcat → 落盘 → 摘要
#
# 用法:
#   bash 04_scripts/境界守望者/wor_launch.sh [抓取秒数，默认 180]    全量取证
#   bash 04_scripts/境界守望者/wor_launch.sh gate [秒数，默认 30]   短闸门：只回答"会不会立刻崩"
#   SERIAL=<序列号> bash 04_scripts/境界守望者/wor_launch.sh 240
#
# 环境变量覆盖（2026-10-04 加，Mac 复验提的：跑一次就得往仓库里写日志）:
#   NIXIANG_WOR_LOGDIR  日志落盘目录（默认 06_dumped/境界守望者/device_logs）
#   NIXIANG_WOR_PKG     包名（默认 com.ap.watcherofrealms）
#   SERIAL              设备序列号（默认 96f0d883）
#   例：NIXIANG_WOR_LOGDIR=/tmp/wor_logs bash …/wor_launch.sh gate 30
#
# 两段式验证（为什么这么设计）:
#   ① gate 短闸门 —— 只否掉立刻崩的版本（FATAL/SIGABRT/SIGSEGV/F DEBUG），30 秒够；
#   ② full 长跑 —— 必须走到功能真正触发（开开关 / 进战斗 / 第一次 hook 命中）才算验完。
#   反面教训：本作有一版跑满 2527s 才崩（崩在第一次真正跑布局解析），当年只用
#   200~300s 窗口看，拿到的是假阴性 ⇒ 别把"进程活够久"当成"验过了"。
#
# 产物（06_dumped/境界守望者/device_logs/）:
#   wor_<stamp>_raw.log    全量 logcat（会被游戏自身日志回绕）
#   wor_<stamp>_filt.log   只抓 MOD 自家 tag（永不回绕，是 [Wor] 证据的保险）
#   wor_<stamp>_wor.log    RAW+FILT 合并后只留 [Wor] 相关行
#   wor_<stamp>_tables.txt MOD 落盘的侦察表（若设备侧存在）
#
# 为什么要双路 + 放大缓冲（本作实测）:
#   本作日志量极大，默认 ~256KB 环形缓冲会被战斗日志冲掉早期 [Wor] 行。
#   ① logcat -G 16M 放大设备侧缓冲（老 ROM/部分 MIUI 不支持，失败不致命）
#   ② 再开一路只订阅 MOD 自家 tag 的 logcat，保证证据链不断
set -uo pipefail

SER="${SERIAL:-96f0d883}"
PKG="${NIXIANG_WOR_PKG:-com.ap.watcherofrealms}"
GAME_DIR="境界守望者"
ADB="/c/windows/system32/adb.exe"
[[ -x "$ADB" ]] || ADB="adb"

# 抓取模式（bash 3.2 兼容：只用 case，不用数组/大小写转换）
MODE="full"
DUR="180"
case "${1:-}" in
  "")       ;;
  gate)     MODE="gate"; DUR="${2:-30}" ;;
  *[!0-9]*) echo "用法: bash 04_scripts/境界守望者/wor_launch.sh [秒数|gate [秒数]]" >&2; exit 2 ;;
  *)        DUR="$1" ;;
esac

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"

# 目标路径要交给原生 Windows 的 adb.exe 时必须转成 Windows 形式；mac 没有 cygpath
# ⇒ 必须原样返回。2026-10-04 双机修正：旧写法无条件调 cygpath，mac 上命令替换返回
# 空串，`adb pull <src> ""` 直接失败，侦察表拉不回来（win 侧一直没暴露）。
win_path() {
  if command -v cygpath >/dev/null 2>&1; then cygpath -w "$1"; else printf '%s' "$1"; fi
}
LOGDIR="${NIXIANG_WOR_LOGDIR:-$ROOT/06_dumped/$GAME_DIR/device_logs}"
mkdir -p "$LOGDIR"
STAMP="$(date +%Y%m%d_%H%M%S)"
RAW="$LOGDIR/wor_${STAMP}_raw.log"
FILT="$LOGDIR/wor_${STAMP}_filt.log"
TXT="$LOGDIR/wor_${STAMP}_wor.log"

echo "[launch] serial=$SER pkg=$PKG duration=${DUR}s"
echo "[launch] raw  -> $RAW"
echo "[launch] filt -> $FILT"

"$ADB" -s "$SER" logcat -c || { echo "ERROR: logcat -c 失败（设备未连？）" >&2; exit 1; }
"$ADB" -s "$SER" logcat -G 16M 2>&1 | sed 's/^/[launch] logcat -G: /' || true
"$ADB" -s "$SER" shell am force-stop "$PKG"

"$ADB" -s "$SER" logcat -v time > "$RAW" 2>&1 &
LPID=$!
"$ADB" -s "$SER" logcat -v time -s Mod_Menu:V StarryMagic:V StarryInitProvider:V MenuService:V ModBridge:V StarrySignSpoof:V > "$FILT" 2>&1 &
FPID=$!
sleep 1

"$ADB" -s "$SER" shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1
echo "[launch] 游戏已拉起，等待 ${DUR}s —— 请人工操作悬浮菜单（MIUI 禁 adb input tap，必须手点）"

END=$(( $(date +%s) + DUR ))
while [[ "$(date +%s)" -lt "$END" ]]; do sleep 5; printf '.'; done
echo

kill "$LPID" "$FPID" 2>/dev/null
wait "$LPID" "$FPID" 2>/dev/null

echo "[launch] 抓取结束: 全量 $(wc -l < "$RAW") 行 / 过滤 $(wc -l < "$FILT") 行"

# ---- 0) 把 MOD 落盘的侦察表拉回来（布局/偏移定案的主要证据）----
# MSYS 会把 /sdcard/... 当路径转换吃掉 ⇒ 必须关掉转换，且目标用 Windows 形式路径。
# gate 模式不拉表（只看崩不崩）。
TABLES="$LOGDIR/wor_${STAMP}_tables.txt"
if [ "$MODE" = "full" ]; then
export MSYS_NO_PATHCONV=1 MSYS2_ARG_CONV_EXCL='*'
for p in "/sdcard/Android/data/$PKG/files/wor_tables.txt" \
         "/storage/emulated/0/Android/data/$PKG/files/wor_tables.txt"; do
  if "$ADB" -s "$SER" shell "test -f '$p'" 2>/dev/null; then
    echo "[launch] 发现落盘表: $p"
    "$ADB" -s "$SER" pull "$p" "$(win_path "$TABLES")" 2>&1 | tail -2
    if [[ -s "$TABLES" ]]; then
      echo "[launch] 表已拉回: $TABLES （$(wc -l < "$TABLES") 行, $(wc -c < "$TABLES") 字节）"
      echo; echo "===== 表: SECTION 目录 ====="
      LC_ALL=C grep -an '^=====' "$TABLES" || true
      echo; echo "===== 表: HIT / probe / reconcile（前 60 条）====="
      LC_ALL=C grep -aE 'HIT-|probe |reconcile|off .*cn=' "$TABLES" | head -60 || true
    fi
    break
  fi
done
fi

# ---- 1) 只看 [Wor] 标签（含 Mod_Menu / StarryMagic 转发）----
cat "$RAW" "$FILT" 2>/dev/null | LC_ALL=C grep -aE '\[Wor\]|Wor_|wor-e2|libstarry_magic' > "$TXT" || true
echo "[launch] [Wor] 相关行数: $(wc -l < "$TXT")（RAW+FILT 合并）"

if [ "$MODE" = "full" ]; then
echo; echo "===== [Wor] 全部行（前 140 条）====="
head -140 "$TXT"
fi

echo; echo "===== 关键锚点计数（RAW+FILT 合并）====="
# 2026-10-04 双机修正：macOS 的 mktemp 必须带模板（不带给空串 ⇒ 下一行重定向报
# "No such file or directory"，整块锚点计数失效）。用 ${TMPDIR:-/tmp} 兼容两台机。
COMB="$(mktemp "${TMPDIR:-/tmp}/wor_comb.XXXXXX")"
cat "$RAW" "$FILT" 2>/dev/null > "$COMB"
for s in "off " "match" "DIFF" "keep-cn" "cand HasComponent" "cand ExcuteOnce" "cand " \
         "by-name" "diag cls" "HIT-" "SECTION" "asym" "thread attached" "domain not ready" \
         "EnsureLayout retry" "FATAL" "ENV stage" "feature stage" "lib ready" \
         "INV e=" "DMG x" "P#" "拒绝"; do
  printf '  %-22s %s\n' "$s" "$(LC_ALL=C grep -acF -- "$s" "$COMB" || true)"
done
rm -f "$COMB"

if [ "$MODE" = "full" ]; then
echo; echo "===== 初始化/挂点行 ====="
cat "$RAW" "$FILT" 2>/dev/null | LC_ALL=C grep -aE 'field |off |cand |hooked=|lib ready|helpers|拒绝|FATAL|asym|diag cls|thread attached|EnsureLayout' | head -100 || true

echo; echo "===== 探针 P# 行（前 20 条）====="
cat "$RAW" "$FILT" 2>/dev/null | LC_ALL=C grep -aF -- "P#" | head -20 || true

echo; echo "===== 改写生效行 INV / DMG（前 20 条）====="
cat "$RAW" "$FILT" 2>/dev/null | LC_ALL=C grep -aE 'INV e=|DMG x' | head -20 || true
fi

echo; echo "===== 崩溃/异常 ====="
cat "$RAW" "$FILT" 2>/dev/null | LC_ALL=C grep -aE 'FATAL EXCEPTION|SIGSEGV|signal 11|backtrace|liblogic.so.*\+0x|DEBUG   :|tombstone' | head -40 || true

# ---- 自动结论 ----
# 2026-10-04 加的：本作"无限重启"那轮是用户先发现的，不是我看日志发现的 —— 因为
# 当时只人工扫了几眼。结论必须由脚本强制打印，不能靠"我觉得没问题"。
cnt() { cat "$RAW" "$FILT" 2>/dev/null | LC_ALL=C grep -acF -- "$1" || true; }
_V_FATAL="$(cnt 'FATAL EXCEPTION')"
_V_SEGV="$(cnt 'SIGSEGV')"
_V_ABRT="$(cnt 'SIGABRT')"
_V_LIBOK="$(cnt '[Wor] lib ready')"
_V_INV="$(cnt 'INV e=')"
_V_DMG="$(cnt 'DMG x')"
_V_ASM="$(cnt 'img#')"
echo; echo "===== 自动结论（报告前必读；别拿\"进程还活着\"当结论）====="
echo "  FATAL=$_V_FATAL SIGSEGV=$_V_SEGV SIGABRT=$_V_ABRT | [Wor] lib ready=$_V_LIBOK | INV=$_V_INV DMG=$_V_DMG | img#=$_V_ASM"
if [ "$_V_FATAL" -eq 0 ] && [ "$_V_SEGV" -eq 0 ] && [ "$_V_ABRT" -eq 0 ]; then
  if [ "$MODE" = "gate" ]; then
    echo "VERDICT: GATE-PASS — 无 FATAL/SIGSEGV/SIGABRT，这一版不会立刻崩。"
    echo "         但 gate 通过 ≠ 可用：必须再跑 full 长跑，并走到功能真正触发（开开关/进战斗/第一次 hook 命中）。"
  elif [ "$_V_LIBOK" -gt 0 ]; then
    echo "VERDICT: FULL-PASS(取证完成) — 无崩溃且 [Wor] lib ready 已出现；功能是否生效看 INV=${_V_INV} / DMG=${_V_DMG}。"
  else
    echo "VERDICT: FULL-NO-INIT — 无崩溃但没等到 [Wor] lib ready（抓取窗口太短，或库没被游戏 dlopen）。"
  fi
else
  echo "VERDICT: CRASH — 有崩溃（FATAL=$_V_FATAL SIGSEGV=$_V_SEGV SIGABRT=${_V_ABRT}）⇒ 立刻回滚本版，不要往下走。"
  echo "         崩溃栈看上面的「崩溃/异常」段；需要符号化就用 NDK llvm-symbolizer + 未 strip 的 obj/ 产物。"
fi

echo; echo "[launch] 完成。原始: $RAW"
