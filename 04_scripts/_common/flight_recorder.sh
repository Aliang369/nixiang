#!/usr/bin/env bash
# ================================================================
#  flight_recorder.sh — 飞行记录仪（游玩期连续取证＋用户口述标记）
#
#  背景（2026-10-04）：人工实测比机器判定准——有些问题日志里看不出来，
#  使用时才知道 ⇒ 记录仪把「使用时才知道」的现场留在磁盘上：logcat
#  连续录制 ＋ /proc 周期采样；用户在游玩中发现问题喊「标记」，AI 立刻
#  执行 mark 打时间戳，事后 slice 按标记±窗口切出证据。
#  取证只走 logcat //proc 老三样，不 screencap、不 input tap。
#  与 post_install_precheck.sh 组合：先 start 再装机/预检，全程有记录。
#
#  用法:
#    # 开录（越早越好；默认目录 09_test/_recorder/<时间戳>）
#    bash 04_scripts/_common/flight_recorder.sh start \
#        --pkg <包名> [--serial <S>] [--out <目录>] [--sample-sec 10]
#    # 用户喊「标记」时 AI 立刻执行（note 用用户原话；--pkg 可选，带上则记 pid）
#    bash 04_scripts/_common/flight_recorder.sh mark --dir <目录> --pkg <包名> --note "打Boss没扣血"
#    # 事后切证据（按第 N 条标记或指定时刻；默认 前60s 后60s）
#    bash 04_scripts/_common/flight_recorder.sh slice --dir <目录> --mark 1 [--before 60] [--after 60]
#    bash 04_scripts/_common/flight_recorder.sh slice --dir <目录> --ts "2026-10-04 12:00:00"
#    bash 04_scripts/_common/flight_recorder.sh stop  --dir <目录>
#    bash 04_scripts/_common/flight_recorder.sh status [--dir <目录>]
#
#  产物（目录内；均为文本，按需切片读勿整读）:
#    logcat.txt       连续 logcat（-v threadtime，行首 MM-DD HH:MM:SS.mmm）
#    proc_samples.log `S|<epoch>|<时刻>|<pid>| statm=<..> VmRSS=<..> Threads=<..>`（周期采样）
#    marks.log        `MARK|<epoch>|<时刻>|<pid或->|<用户原话>`
#    events.log       `<epoch>|<时刻>|<事件>`（REC_START/REC_STOP/PROC_GONE/PROC_RESTART）
#    recorder.meta    SUP=<pid> / LOGCAT=<pid>（stop 用）
#    slice_*.txt      slice 产物（窗口内 logcat/marks/proc/events 汇总）
#
#  注意:
#    - mark/slice 不依赖记录仪存活（marks.log 追加随时可用；slice 读存量文件）
#    - 跨午夜窗口 slice 只保同日半窗（时间按 HH:MM:SS 字符串比较）；当日会话无碍
#    - 一次一录：同一 --out 目录已有活记录仪时 start 拒绝
#  ⚠️ macOS Bash 3.2 兼容：不用 mapfile / declare -A / ${v,,} / &>>
# ================================================================
set -u

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

MODE=""
[ $# -gt 0 ] && { MODE="$1"; shift; }
case "$MODE" in
  start|mark|slice|stop|status|__worker) ;;
  *) sed -n '2,/^set -u$/p' "$0"; [ -n "$MODE" ] && echo "✗ 未知模式: $MODE"; exit 1 ;;
esac

# ---------- 公共 ----------
adbx() {
  if [ -n "$SERIAL" ]; then adb -s "$SERIAL" "$@"; else adb "$@"; fi
}
# epoch → 任意格式时刻（mac date -r 与 GNU date -d @ 双兼容；格式须带 + 前缀，mac 无 + 会把 fmt 当 new_date 操作数）
epoch_fmt() {
  if date -r "$1" "+$2" >/dev/null 2>&1; then date -r "$1" "+$2"; else date -d "@$1" "+$2"; fi
}
# "YYYY-MM-DD HH:MM:SS" → epoch（mac -j -f 与 GNU -d 双兼容）
to_epoch() {
  if date -j -f '%Y-%m-%d %H:%M:%S' "$1" +%s >/dev/null 2>&1; then
    date -j -f '%Y-%m-%d %H:%M:%S' "$1" +%s
  else
    date -d "$1" +%s
  fi
}
wall() { date '+%Y-%m-%d %H:%M:%S'; }
ev_log() { echo "$(date +%s)|$(wall)|$1" >> "$OUT/events.log"; }

# ---------- start ----------
do_start() {
  [ -n "$PKG" ] || { echo "✗ start 必填 --pkg <包名>"; exit 1; }
  [ -n "$OUT" ] || OUT="$NIXIANG_ROOT/09_test/_recorder/$(date +%Y%m%d_%H%M%S)"
  if [ -f "$OUT/recorder.meta" ]; then
    SUP0="$(sed -n 's/^SUP=//p' "$OUT/recorder.meta" | head -1)"
    if [ -n "$SUP0" ] && kill -0 "$SUP0" 2>/dev/null; then
      echo "✗ 该目录已有记录仪在跑（SUP=${SUP0}），一次一录；先 stop 或换 --out"
      exit 1
    fi
  fi
  if [ -s "$OUT/logcat.txt" ]; then
    echo "✗ $OUT/logcat.txt 已有旧证据（非空）——防覆盖拒绝重录；换 --out 新目录"
    exit 1
  fi
  mkdir -p "$OUT" || { echo "✗ 建目录失败: $OUT"; exit 1; }
  : > "$OUT/logcat.txt"; : > "$OUT/proc_samples.log"
  [ -f "$OUT/marks.log" ] || : > "$OUT/marks.log"
  [ -f "$OUT/events.log" ] || : > "$OUT/events.log"
  ARGS="--pkg $PKG --out $OUT --sample-sec $SAMPLE_SEC"
  [ -n "$SERIAL" ] && ARGS="--serial $SERIAL $ARGS"
  nohup bash "$0" __worker $ARGS >> "$OUT/recorder.worker.log" 2>&1 &
  SUP=$!
  echo "SUP=$SUP" > "$OUT/recorder.meta"
  ev_log "REC_START pkg=$PKG sample_sec=$SAMPLE_SEC out=$OUT"
  echo "✓ 记录仪已开 SUP=$SUP"
  echo "  目录: $OUT"
  echo "  用户喊「标记」时: bash \"$0\" mark --dir '$OUT' ${PKG:+--pkg $PKG} --note \"<用户原话>\""
  echo "  收工: bash \"$0\" stop --dir '$OUT'"
}

# ---------- __worker（后台常驻） ----------
do_worker() {
  [ -n "$PKG" ] && [ -n "$OUT" ] || { echo "✗ worker 内部参数缺失"; exit 1; }
  adbx logcat -v threadtime > "$OUT/logcat.txt" 2>/dev/null &
  LOGCAT_PID=$!
  echo "LOGCAT=$LOGCAT_PID" >> "$OUT/recorder.meta"
  trap 'kill "$LOGCAT_PID" 2>/dev/null; exit 0' INT TERM
  trap 'kill "$LOGCAT_PID" 2>/dev/null' EXIT
  PREV=""
  while :; do
    TSE=$(date +%s); TSW=$(wall)
    PID="$(adbx shell "pidof $PKG" 2>/dev/null | tr -d '\r' | awk '{print $1}')"
    if [ -n "$PID" ]; then
      SAMP="$(adbx shell "cat /proc/$PID/statm 2>/dev/null; grep -E '^VmRSS|^Threads' /proc/$PID/status 2>/dev/null" 2>/dev/null | tr -d '\r' | tr '\n' ' ')"
      echo "S|$TSE|$TSW|$PID| ${SAMP:-read_fail}" >> "$OUT/proc_samples.log"
      if [ -n "$PREV" ] && [ "$PREV" != "$PID" ]; then
        echo "$TSE|$TSW|PROC_RESTART $PREV -> $PID" >> "$OUT/events.log"
      fi
    else
      if [ -n "$PREV" ]; then
        echo "$TSE|$TSW|PROC_GONE pid=$PREV" >> "$OUT/events.log"
      fi
    fi
    PREV="$PID"
    sleep "$SAMPLE_SEC"
  done
}

# ---------- mark ----------
do_mark() {
  [ -n "$OUT" ] || { echo "✗ mark 必填 --dir"; exit 1; }
  [ -n "$NOTE" ] || { echo "✗ mark 必填 --note（用用户原话）"; exit 1; }
  mkdir -p "$OUT"
  PID="-"
  if [ -n "$PKG" ] && [ -n "$SERIAL" ]; then
    PID="$(adbx shell "pidof $PKG" 2>/dev/null | tr -d '\r' | awk '{print $1}')"
  elif [ -n "$PKG" ]; then
    PID="$(adb shell "pidof $PKG" 2>/dev/null | tr -d '\r' | awk '{print $1}')"
  fi
  [ -n "$PID" ] || PID="-"
  TSE=$(date +%s); TSW=$(wall)
  echo "MARK|$TSE|$TSW|$PID|$NOTE" >> "$OUT/marks.log"
  N="$(grep -c '^MARK|' "$OUT/marks.log" 2>/dev/null || echo 0)"
  echo "✓ 已标记 第${N}条 $TSW pid=$PID 「${NOTE}」"
}

# ---------- slice ----------
do_slice() {
  [ -n "$OUT" ] || { echo "✗ slice 必填 --dir"; exit 1; }
  [ -f "$OUT/logcat.txt" ] || { echo "✗ 目录无 logcat.txt: $OUT"; exit 1; }
  if [ -n "$MARKN" ]; then
    ML="$(grep '^MARK|' "$OUT/marks.log" 2>/dev/null | sed -n "${MARKN}p")"
    [ -n "$ML" ] || { echo "✗ 第 ${MARKN} 条标记不存在（共 $(grep -c '^MARK|' "$OUT/marks.log" 2>/dev/null || echo 0) 条）"; exit 1; }
    EPOCH="$(echo "$ML" | cut -d'|' -f2)"
    TSW="$(echo "$ML" | cut -d'|' -f3)"
    TAG="M${MARKN}"
  elif [ -n "$TS" ]; then
    EPOCH="$(to_epoch "$TS")" || { echo "✗ --ts 解析失败: $TS"; exit 1; }
    TSW="$TS"; TAG="ts"
  else
    echo "✗ slice 需 --mark N 或 --ts \"YYYY-MM-DD HH:MM:SS\""; exit 1
  fi
  A=$((EPOCH - BEFORE)); B=$((EPOCH + AFTER))
  SL="$OUT/slice_${TAG}_${BEFORE}s_${AFTER}s.txt"
  DAY="$(epoch_fmt "$A" '%m-%d')"
  T0="$(epoch_fmt "$A" '%H:%M:%S')"
  T1="$(epoch_fmt "$B" '%H:%M:%S')"
  CROSS=0
  # 跨午夜：零补位 HH:MM:SS 字符串比较（-lt 是数值比较会报错，必须用 \<）
  if [ "$T1" \< "$T0" ]; then CROSS=1; fi

  {
    echo "== slice $TAG [$TSW] 窗口 [$A,$B] day=$DAY $T0..$T1 跨午夜=$CROSS =="
    echo
    echo "---- 窗口内标记 (marks.log) ----"
    awk -F'|' -v a="$A" -v b="$B" '$1=="MARK" && $2>=a && $2<=b' "$OUT/marks.log" 2>/dev/null
    echo
    echo "---- 窗口内事件 (events.log) ----"
    awk -F'|' -v a="$A" -v b="$B" '$1+0>=a && $1+0<=b' "$OUT/events.log" 2>/dev/null
    echo
    echo "---- 窗口内 /proc 采样 (proc_samples.log) ----"
    awk -F'|' -v a="$A" -v b="$B" '$1=="S" && $2>=a && $2<=b' "$OUT/proc_samples.log" 2>/dev/null
    echo
    echo "---- 窗口内 logcat ----"
    if [ "$CROSS" -eq 0 ]; then
      awk -v d="$DAY" -v t0="$T0" -v t1="$T1" \
        'substr($0,1,5)==d && substr($0,7,8)>=t0 && substr($0,7,8)<=t1' "$OUT/logcat.txt"
    else
      echo "（跨午夜，仅保留同日半窗 ${T0}..23:59:59）"
      awk -v d="$DAY" -v t0="$T0" \
        'substr($0,1,5)==d && substr($0,7,8)>=t0' "$OUT/logcat.txt"
    fi
  } > "$SL" 2>&1

  NLINES="$(wc -l < "$SL" | tr -d ' ')"
  echo "✓ 切片完成: ${SL}（共 ${NLINES} 行）——按需分段读，勿整读进上下文"
  echo "---- 预览（前 40 行）----"
  head -40 "$SL"
}

# ---------- stop ----------
do_stop() {
  [ -n "$OUT" ] || { echo "✗ stop 必填 --dir"; exit 1; }
  [ -f "$OUT/recorder.meta" ] || { echo "✗ 无 recorder.meta: ${OUT}（未开录？）"; exit 1; }
  SUP="$(sed -n 's/^SUP=//p' "$OUT/recorder.meta" | head -1)"
  LCP="$(sed -n 's/^LOGCAT=//p' "$OUT/recorder.meta" | head -1)"
  [ -n "$SUP" ] && kill "$SUP" 2>/dev/null
  sleep 1
  [ -n "$LCP" ] && kill "$LCP" 2>/dev/null
  ev_log "REC_STOP"
  NM="$(grep -c '^MARK|' "$OUT/marks.log" 2>/dev/null || echo 0)"
  ST="$(awk -F'|' '$3 ~ /REC_START/ {print $1; exit}' "$OUT/events.log" 2>/dev/null)"
  echo "✓ 记录仪已停: $OUT"
  echo "  标记 ${NM} 条；产物:"
  ls -lh "$OUT" 2>/dev/null | awk 'NR>1 {printf "  %-24s %s\n", $9, $5}'
  if [ -n "$ST" ]; then
    echo "  时长: $(( ($(date +%s) - ST) / 60 )) 分钟"
  fi
  [ "$NM" -gt 0 ] && echo "  切证据: bash \"$0\" slice --dir '$OUT' --mark 1..$NM"
}

# ---------- status ----------
do_status() {
  if [ -z "$OUT" ]; then
    OUT="$(ls -1d "$NIXIANG_ROOT"/09_test/_recorder/*/ 2>/dev/null | tail -1)"
    [ -n "$OUT" ] || { echo "（尚无任何记录目录）"; exit 0; }
  fi
  [ -f "$OUT/recorder.meta" ] || { echo "✗ 无 recorder.meta: $OUT"; exit 1; }
  SUP="$(sed -n 's/^SUP=//p' "$OUT/recorder.meta" | head -1)"
  if [ -n "$SUP" ] && kill -0 "$SUP" 2>/dev/null; then RUN="运行中(SUP=$SUP)"; else RUN="已停"; fi
  NM="$(grep -c '^MARK|' "$OUT/marks.log" 2>/dev/null || echo 0)"
  echo "目录: $OUT  状态: $RUN  标记: ${NM} 条"
  [ "$NM" -gt 0 ] && tail -3 "$OUT/marks.log"
}

# ---------- 各模式参数 ----------
SERIAL=""; PKG=""; OUT=""; SAMPLE_SEC=10; NOTE=""; MARKN=""; TS=""; BEFORE=60; AFTER=60
while [ $# -gt 0 ]; do
  case "$1" in
    --serial)     SERIAL="$2"; shift 2 ;;
    --pkg)        PKG="$2"; shift 2 ;;
    --out|--dir)  OUT="$2"; shift 2 ;;
    --sample-sec) SAMPLE_SEC="$2"; shift 2 ;;
    --note)       NOTE="$2"; shift 2 ;;
    --mark)       MARKN="$2"; shift 2 ;;
    --ts)         TS="$2"; shift 2 ;;
    --before)     BEFORE="$2"; shift 2 ;;
    --after)      AFTER="$2"; shift 2 ;;
    *) echo "✗ 未知参数: $1"; exit 1 ;;
  esac
done

case "$MODE" in
  start)    do_start ;;
  __worker) do_worker ;;
  mark)     do_mark ;;
  slice)    do_slice ;;
  stop)     do_stop ;;
  status)   do_status ;;
esac
