#!/usr/bin/env bash
# 境界守望者 真机冒烟/取证：清日志 → 放大环形缓冲 → 重启游戏 → 双路抓 logcat → 落盘 → 摘要
#
# 用法:
#   bash 04_scripts/境界守望者/wor_launch.sh [抓取秒数，默认 180]
#   SERIAL=<序列号> bash 04_scripts/境界守望者/wor_launch.sh 240
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
PKG="com.ap.watcherofrealms"
GAME_DIR="境界守望者"
ADB="/c/windows/system32/adb.exe"
[[ -x "$ADB" ]] || ADB="adb"
DUR="${1:-180}"

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
LOGDIR="$ROOT/06_dumped/$GAME_DIR/device_logs"
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
TABLES="$LOGDIR/wor_${STAMP}_tables.txt"
export MSYS_NO_PATHCONV=1 MSYS2_ARG_CONV_EXCL='*'
for p in "/sdcard/Android/data/$PKG/files/wor_tables.txt" \
         "/storage/emulated/0/Android/data/$PKG/files/wor_tables.txt"; do
  if "$ADB" -s "$SER" shell "test -f '$p'" 2>/dev/null; then
    echo "[launch] 发现落盘表: $p"
    "$ADB" -s "$SER" pull "$p" "$(cygpath -w "$TABLES")" 2>&1 | tail -2
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

# ---- 1) 只看 [Wor] 标签（含 Mod_Menu / StarryMagic 转发）----
cat "$RAW" "$FILT" 2>/dev/null | LC_ALL=C grep -aE '\[Wor\]|Wor_|wor-e2|libstarry_magic' > "$TXT" || true
echo "[launch] [Wor] 相关行数: $(wc -l < "$TXT")（RAW+FILT 合并）"

echo; echo "===== [Wor] 全部行（前 140 条）====="
head -140 "$TXT"

echo; echo "===== 关键锚点计数（RAW+FILT 合并）====="
COMB="$(mktemp)"
cat "$RAW" "$FILT" 2>/dev/null > "$COMB"
for s in "off " "match" "DIFF" "keep-cn" "cand HasComponent" "cand ExcuteOnce" "cand " \
         "by-name" "diag cls" "HIT-" "SECTION" "asym" "thread attached" "domain not ready" \
         "EnsureLayout retry" "FATAL" "ENV stage" "feature stage" "lib ready" \
         "INV e=" "DMG x" "P#" "拒绝"; do
  printf '  %-22s %s\n' "$s" "$(LC_ALL=C grep -acF -- "$s" "$COMB" || true)"
done
rm -f "$COMB"

echo; echo "===== 初始化/挂点行 ====="
cat "$RAW" "$FILT" 2>/dev/null | LC_ALL=C grep -aE 'field |off |cand |hooked=|lib ready|helpers|拒绝|FATAL|asym|diag cls|thread attached|EnsureLayout' | head -100 || true

echo; echo "===== 探针 P# 行（前 20 条）====="
cat "$RAW" "$FILT" 2>/dev/null | LC_ALL=C grep -aF -- "P#" | head -20 || true

echo; echo "===== 改写生效行 INV / DMG（前 20 条）====="
cat "$RAW" "$FILT" 2>/dev/null | LC_ALL=C grep -aE 'INV e=|DMG x' | head -20 || true

echo; echo "===== 崩溃/异常 ====="
cat "$RAW" "$FILT" 2>/dev/null | LC_ALL=C grep -aE 'FATAL EXCEPTION|SIGSEGV|signal 11|backtrace|liblogic.so.*\+0x|DEBUG   :|tombstone' | head -40 || true

echo; echo "[launch] 完成。原始: $RAW"
