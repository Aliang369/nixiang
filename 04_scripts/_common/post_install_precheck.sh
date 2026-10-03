#!/usr/bin/env bash
# ================================================================
#  post_install_precheck.sh — 装机预检闸门（装机后运行时证据关）
#
#  背景（2026-10-04 用户口述三条禁令根因，禁止再提机器实测/自动点击）：
#    ① 坐标定位会歪，重试烧流程与 token；② 菜单授权墙机器过不去且易
#    误诊成服务端问题，而用户输一次账号即可解决；③ 人工实测比机器判定
#    准——有些问题日志里看不出来，使用时才知道。
#    ⇒ 分工定死：人工＝终审＋标记，机器＝预检员＋记录仪。本脚本只做
#    机器擅长的「装机层证据」：包在不在、启动崩不崩、菜单 so 是否加载、
#    菜单是否弹出、授权墙是否过（过不去只发操作请求卡，不诊断服务端）、
#    空转 N 分钟稳不稳。**预检全过才请用户上手实测；预检挂了直接出
#    A/B 判定，不消耗用户实测轮。** 预检不证明功能好用，终审以用户实测为准。
#
#  与既有脚本分工：
#    pre_install_gate.sh     — 装机前产物静态五查（JS/JNI/PInvoke/控件/门禁串）
#    verify_installed_apk.sh — 装机完整性 md5 校验（防装旧包假成功）
#    本脚本                 — 装机后启动运行时证据（logcat 标记监控＋判定）
#    装机本身仍走 pipeline.sh；本脚本在 install 阶段完成后调用，不替代管线。
#
#  用法:
#    bash 04_scripts/_common/post_install_precheck.sh --game <游戏> \
#        [--serial <S>] [--pkg <包名>] [--markers <标记文件>] \
#        [--idle-min 3] [--launch-timeout 120] [--auth-timeout 300]
#
#  包名解析优先级: --pkg > 标记文件 pkg 行 > 10_docs/<游戏>/*_menu_template.json 的 package 字段
#  标记文件（默认 10_docs/<游戏>/预检标记.txt，可不存在）每行 `类别|ERE`：
#    pkg|com.example.game           # 包名
#    menu_load|StarryMagic          # 菜单 so 加载标记（默认=星光框架 TAG）
#    menu_shown|native menu shown   # 菜单弹出标记
#    hook_hit|<游戏级hook标记>       # 游戏级命中标记（缺省时该项降级为只验菜单层）
#    auth_fail|renew auth fail      # 授权墙标记
#    auth_ok|auth game_id synced    # 授权通过标记
#    crash|FATAL EXCEPTION|...      # 崩溃标记
#    '#' 开头为注释
#
#  退出码: 0=PASS_TO_HUMAN（预检过，请用户上手实测）
#          1=FAIL_A（上游没发生：没装上/启动即崩/运行中崩/未见 so 加载）
#          2=FAIL_B（装了没命中：so 加载但菜单未弹出）
#          3=BLOCKED_AUTH（授权墙未过，已发操作请求卡；等用户输卡密后重跑）
#          4=HOOK 未命中（菜单弹出但 hook_hit 标记缺失；菜单层证据齐，游戏层缺证）
#
#  ⚠️ 真机取证纪律：本脚本不 screencap、不 input tap；授权墙一律发操作
#     请求卡等用户处理，脚本不试账号、不改判服务端问题。
#  ⚠️ macOS Bash 3.2 兼容：不用 mapfile / declare -A / ${v,,} / &>>
# ================================================================
set -u

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

# ---------- 参数 ----------
GAME=""; SERIAL=""; PKG=""; MARKERS=""
IDLE_MIN=3; LAUNCH_TIMEOUT=120; AUTH_TIMEOUT=300

while [ $# -gt 0 ]; do
  case "$1" in
    --game)           GAME="$2"; shift 2 ;;
    --serial)         SERIAL="$2"; shift 2 ;;
    --pkg)            PKG="$2"; shift 2 ;;
    --markers)        MARKERS="$2"; shift 2 ;;
    --idle-min)       IDLE_MIN="$2"; shift 2 ;;
    --launch-timeout) LAUNCH_TIMEOUT="$2"; shift 2 ;;
    --auth-timeout)   AUTH_TIMEOUT="$2"; shift 2 ;;
    -h|--help)        sed -n '2,/^set -u$/p' "$0"; exit 0 ;;
    *) echo "✗ 未知参数: $1（--help 看用法）"; exit 1 ;;
  esac
done
[ -n "$GAME" ] || { echo "✗ 必填 --game <游戏>"; exit 1; }
[ -n "$MARKERS" ] || MARKERS="$NIXIANG_ROOT/10_docs/$GAME/预检标记.txt"

adbx() {
  if [ -n "$SERIAL" ]; then adb -s "$SERIAL" "$@"; else adb "$@"; fi
}

# 设备可用性（多设备不传 --serial 时 adb 会报 more than one device，先拦住）
ST="$(adbx get-state 2>/dev/null | tr -d '\r')"
[ "$ST" = "device" ] || { echo "✗ 设备不可用（get-state=${ST:-空}）；多设备请传 --serial"; exit 1; }

# ---------- 标记正则（默认=星光框架 2026-10 校准，来源 skill/starry-mod/SKILL.md 验证节） ----------
RE_MENU_LOAD='StarryApp|StarryMagic'
RE_MENU_SHOWN='native menu shown'
RE_HOOK=''
RE_AUTH_FAIL='renew auth fail|authoritative=.*deny'
RE_AUTH_OK='auth game_id synced from native|renew ok'
RE_CRASH='FATAL EXCEPTION|Fatal signal|SIGSEGV|SIGABRT|tombstone'

if [ -f "$MARKERS" ]; then
  echo "- 标记文件已加载: $MARKERS"
  while IFS= read -r line; do
    case "$line" in ''|'#'*) continue ;; esac
    k="${line%%|*}"; v="${line#*|}"
    case "$k" in
      pkg)        PKG="$v" ;;
      menu_load)  RE_MENU_LOAD="$v" ;;
      menu_shown) RE_MENU_SHOWN="$v" ;;
      hook_hit)   RE_HOOK="$v" ;;
      auth_fail)  RE_AUTH_FAIL="$v" ;;
      auth_ok)    RE_AUTH_OK="$v" ;;
      crash)      RE_CRASH="$v" ;;
      *) echo "! WARN 未知标记类别: $k（可用: pkg|menu_load|menu_shown|hook_hit|auth_fail|auth_ok|crash）" ;;
    esac
  done < "$MARKERS"
fi

# ---------- 包名兜底：菜单模板 json 的 package 字段 ----------
if [ -z "$PKG" ]; then
  for mj in "$NIXIANG_ROOT/10_docs/$GAME/"*_menu_template.json; do
    [ -f "$mj" ] || continue
    p="$(sed -n 's/.*"package"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' "$mj" | head -1)"
    if [ -n "$p" ]; then
      PKG="$p"
      echo "- 包名取自菜单模板: $(basename "$mj") → $PKG"
      break
    fi
  done
fi
[ -n "$PKG" ] || { echo "✗ 包名未定：传 --pkg，或建 $MARKERS 写 pkg| 行，或放 10_docs/$GAME/*_menu_template.json"; exit 1; }

# ---------- 产物目录 ----------
TS="$(date +%Y%m%d_%H%M%S)"
OUT="$NIXIANG_ROOT/09_test/$GAME/precheck_$TS"
mkdir -p "$OUT" || { echo "✗ 建目录失败: $OUT"; exit 1; }
LOG="$OUT/logcat.txt"
echo "== 装机预检 game=$GAME pkg=$PKG out=$OUT =="

# ---------- 1. 包在不在 ----------
PMPATH="$(adbx shell "pm path $PKG" 2>/dev/null | tr -d '\r' | head -1)"
if [ -z "$PMPATH" ]; then
  echo "判定段=A 依据=pm path 无返回（$PKG 未安装）"
  exit 1
fi
echo "✓ 包已安装: $PMPATH"

# ---------- 2. 起录（不 wipe 设备缓冲，-T 1 只取新行）＋干净启动 ----------
adbx logcat -T 1 -v threadtime > "$LOG" 2>/dev/null &
LOGCAT_PID=$!
cleanup() { kill "$LOGCAT_PID" 2>/dev/null; }
trap cleanup EXIT

adbx shell "am force-stop $PKG" >/dev/null 2>&1
sleep 1
adbx shell "monkey -p $PKG -c android.intent.category.LAUNCHER 1" > "$OUT/monkey.log" 2>&1
echo "- 已干净启动（force-stop 后 monkey 冷启）"

# ---------- 3. 监控循环 ----------
START=$(date +%s)
GRACE=15
LAUNCH_DL=$((START + LAUNCH_TIMEOUT))
AUTH_DL=0
IDLE_DL=0
so_ok=0; shown_ok=0; hook_ok=0; auth_ok_f=0; auth_card=0
seen=0; CUR_PID=""

say_fail_a() { echo "判定段=A 依据=$1"; exit 1; }

while :; do
  NOW=$(date +%s)

  # 3a. 进程消失检测（宽限后）
  NEWPID="$(adbx shell "pidof $PKG" 2>/dev/null | tr -d '\r' | awk '{print $1}')"
  if [ -z "$NEWPID" ] && [ $((NOW - START)) -ge "$GRACE" ]; then
    if grep -q "$PKG" "$LOG" 2>/dev/null && grep -qE "$RE_CRASH" "$LOG" 2>/dev/null; then
      echo "== 崩溃上下文（logcat 尾 40 行）=="
      tail -40 "$LOG"
      say_fail_a "进程消失且 logcat 含 $PKG 崩溃标记"
    fi
    say_fail_a "启动 ${GRACE}s 后进程消失/未起来（pidof 空，无崩溃日志则为启动即退或被杀）"
  fi
  [ -n "$NEWPID" ] && CUR_PID="$NEWPID"

  # 3b. 增量扫描 logcat（行基分块，单行不会跨块）
  TOTAL="$(wc -l < "$LOG" 2>/dev/null || echo 0)"
  TOTAL="${TOTAL//[^0-9]/}"
  [ -n "$TOTAL" ] || TOTAL=0
  if [ "$TOTAL" -gt "$seen" ]; then
    CHUNK="$OUT/chunk.$$.txt"
    sed -n "$((seen + 1)),${TOTAL}p" "$LOG" > "$CHUNK"
    seen="$TOTAL"

    # 崩溃：先限本游戏范围（pid 或包名），防别的 app 崩误报；
    # java 崩的 FATAL EXCEPTION 行不带 pid，往后 6 行找 "Process: 包名"
    if grep -qE "$RE_CRASH" "$CHUNK" 2>/dev/null; then
      CRASH_SCOPE="$PKG"
      [ -n "$CUR_PID" ] && CRASH_SCOPE="$CUR_PID|$CRASH_SCOPE"
      if grep -A6 -E "$RE_CRASH" "$CHUNK" 2>/dev/null | grep -qE "$CRASH_SCOPE"; then
        echo "== 崩溃标记（限 $PKG）=="
        grep -E "$RE_CRASH" "$CHUNK" | head -3
        LN="$(grep -nE "$RE_CRASH" "$CHUNK" | head -1 | cut -d: -f1)"
        if [ -n "$LN" ]; then
          L0=$((LN - 20)); [ "$L0" -lt 1 ] && L0=1
          echo "== 上下文 ±20 行 =="
          sed -n "${L0},$((LN + 20))p" "$CHUNK"
        fi
        say_fail_a "运行中崩溃: $(grep -E "$RE_CRASH" "$CHUNK" | head -1)"
      fi
    fi

    # 菜单 so 加载
    if [ "$so_ok" -eq 0 ] && grep -qE "$RE_MENU_LOAD" "$CHUNK" 2>/dev/null; then
      so_ok=1
      echo "✓ 菜单 so 加载标记: $(grep -m1 -E "$RE_MENU_LOAD" "$CHUNK" | tail -c 120)"
    fi

    # 菜单弹出
    if [ "$shown_ok" -eq 0 ] && grep -qE "$RE_MENU_SHOWN" "$CHUNK" 2>/dev/null; then
      shown_ok=1
      IDLE_DL=$((NOW + IDLE_MIN * 60))
      echo "✓ 菜单弹出标记: $(grep -m1 -E "$RE_MENU_SHOWN" "$CHUNK" | tail -c 120)"
    fi

    # 游戏级 hook 命中（配置了才验）
    if [ -n "$RE_HOOK" ] && [ "$hook_ok" -eq 0 ] && grep -qE "$RE_HOOK" "$CHUNK" 2>/dev/null; then
      hook_ok=1
      echo "✓ hook 命中标记: $(grep -m1 -E "$RE_HOOK" "$CHUNK" | tail -c 120)"
    fi

    # 授权墙（只发卡不诊断服务端）
    if [ "$auth_ok_f" -eq 0 ] && grep -qE "$RE_AUTH_FAIL" "$CHUNK" 2>/dev/null; then
      [ "$AUTH_DL" -eq 0 ] && AUTH_DL=$((NOW + AUTH_TIMEOUT))
      if [ "$auth_card" -eq 0 ]; then
        auth_card=1
        cat <<'CARD'
──────────── 操作请求卡 ────────────
① 请你做什么: 打开游戏里的星光菜单授权入口，输入你的卡密/账号完成授权
② 成功标志: 菜单功能变为可点（浮球/菜单正常显示）
③ 我接着看: logcat 中 "auth game_id synced from native"（预检自动续检）
（超时 --auth-timeout 秒退出；授权问题不诊断服务端）
CARD
      fi
    fi

    # 授权通过
    if [ "$auth_ok_f" -eq 0 ] && grep -qE "$RE_AUTH_OK" "$CHUNK" 2>/dev/null; then
      auth_ok_f=1; AUTH_DL=0
      echo "✓ 授权通过标记: $(grep -m1 -E "$RE_AUTH_OK" "$CHUNK" | tail -c 120)"
    fi

    rm -f "$CHUNK"
  fi

  # 3c. 判定
  if [ "$shown_ok" -eq 1 ]; then
    if [ "$IDLE_DL" -gt 0 ] && [ "$NOW" -ge "$IDLE_DL" ]; then
      if [ "$auth_card" -eq 1 ] && [ "$auth_ok_f" -eq 0 ]; then
        echo "判定段=BLOCKED_AUTH 依据=菜单已弹但授权未过（auth fail 后未见 auth_ok，功能不可点）"
        echo "等用户完成授权后重跑本预检；不诊断服务端。证据: $OUT"
        exit 3
      fi
      kill "$LOGCAT_PID" 2>/dev/null
      if [ -n "$RE_HOOK" ] && [ "$hook_ok" -eq 0 ]; then
        echo "判定段=B 依据=菜单弹出但 hook_hit 标记（$RE_HOOK）未命中"
        echo "证据: $OUT（logcat.txt 已留存）"
        exit 4
      fi
      echo "✓ PASS_TO_HUMAN —— so 加载/菜单弹出/空转 ${IDLE_MIN} 分钟无崩溃，装机层证据齐"
      echo "  预检不证明功能好用（使用层问题日志未必显形），请你上手实测——人工＝终审。"
      echo "  建议实测期间飞行记录仪保持运行，发现问题在聊天里喊「标记」。"
      echo "  证据: $OUT"
      exit 0
    fi
  elif [ "$AUTH_DL" -gt 0 ] && [ "$NOW" -ge "$AUTH_DL" ]; then
    echo "判定段=BLOCKED_AUTH 依据=授权墙未过（auth fail 后 ${AUTH_TIMEOUT}s 未见 auth_ok 标记）"
    echo "已发操作请求卡；等用户完成授权后重跑本预检，不诊断服务端。"
    echo "证据: $OUT"
    exit 3
  elif [ "$so_ok" -eq 1 ] && [ "$AUTH_DL" -eq 0 ] && [ "$NOW" -ge "$LAUNCH_DL" ]; then
    echo "判定段=B 依据=so 加载标记已现但未见菜单弹出标记（$RE_MENU_SHOWN），logcat 尾 20 行："
    tail -20 "$LOG"
    echo "证据: $OUT"
    exit 2
  elif [ "$AUTH_DL" -eq 0 ] && [ "$NOW" -ge "$LAUNCH_DL" ]; then
    echo "判定段=A 依据=${LAUNCH_TIMEOUT}s 内未见菜单 so 加载标记（$RE_MENU_LOAD），logcat 尾 20 行："
    tail -20 "$LOG"
    echo "证据: $OUT"
    exit 1
  fi

  sleep 2
done
