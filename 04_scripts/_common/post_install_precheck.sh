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
#        [--idle-min 3] [--launch-timeout 120] [--auth-timeout 300] \
#        [--helper-pkg <宿主助手包名>] [--helper-start] [--helper-delay 20]
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
#    crash_ignore|记录先前处理器      # 崩溃标记的排除项（默认见 RE_CRASH_IGNORE）
#    '#' 开头为注释
#
#  ⚠️ 崩溃误报教训（2026-10-08 我本布衣世家录实测）：
#     CrashTrap 装信号处理器时会打自己的记账日志
#     `[crash] 记录先前处理器 sig=SIGSEGV / sig=SIGABRT`
#     （来源 mod菜单/starry-mod-menu/app/src/main/jni/Games/CrashTrap.cpp:220/224）。
#     裸 `SIGSEGV|SIGABRT` 关键字会命中它，且该行带本进程 pid
#     ⇒ 进程范围校验（CRASH_SCOPE）也拦不住 ⇒ 每次预检都误判 FAIL_A。
#     故崩溃判定一律先过 RE_CRASH_IGNORE 再判范围。
#
#  退出码: 0=PASS_TO_HUMAN（预检过，请用户上手实测）
#          1=FAIL_A（上游没发生：没装上/启动即崩/运行中崩/未见 so 加载）
#          2=FAIL_B（装了没命中：so 加载但菜单未弹出）
#          3=BLOCKED_AUTH（授权墙未过，已发操作请求卡；等用户输卡密后重跑）
#          4=HOOK 未命中（菜单弹出但 hook_hit 标记缺失；菜单层证据齐，游戏层缺证）
#
#  ⚠️ 段=B 假阴性教训（2026-10-10 邂逅在迷宫实测）：
#     独立助手形态下，`native menu shown` 只在**助手 attach 的那一次**打印。
#     若助手在预检**开始录制之前**就已经在跑（典型：上一轮验证后没关），这条唯一证据
#     就落在录制窗口外 ⇒ 预检判 FAIL_B（"装了没命中"），实为假阴性，白跑一整轮。
#     对策：传 `--helper-pkg <助手包名>` ⇒ 起录前先 force-stop 助手；再用 `--helper-start`
#     在**窗口内**拉起它（默认延后 20s，`--helper-delay` 可调）。不传 `--helper-start` 时
#     脚本会打印"请在窗口内自行点开助手"的提示。
#
#  ⚠️ 真机取证纪律：本脚本不 screencap、不 input tap；授权墙一律发操作
#     请求卡等用户处理，脚本不试账号、不改判服务端问题。
#  ⚠️ macOS Bash 3.2 兼容：不用 mapfile / declare -A / ${v,,} / &>>
# ================================================================
set -u

# ★P2-1（2026-10-16）：把退出码打成一行。调用方常写 `bash post_install_precheck.sh … | tail`，
# 管道会把 rc 换成 tail 的 rc ⇒ **FAIL_A 被看成 0**（本轮实测踩到一次，差点漏判一次崩溃）。
# 放在最前面 ⇒ 任何 exit 路径（含 usage / 判定分支 / set -u 中止）都会打印。
trap 'rc=$?; printf "[precheck] PRECHECK_RC=%s\n" "$rc"' EXIT

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

# ---------- 参数 ----------
GAME=""; SERIAL=""; PKG=""; MARKERS=""
HELPER_PKG=""; HELPER_START=0; HELPER_DELAY=20; HELPER_PID=""
IDLE_MIN=3; LAUNCH_TIMEOUT=120; AUTH_TIMEOUT=300

while [ $# -gt 0 ]; do
  case "$1" in
    --game)           GAME="$2"; shift 2 ;;
    --serial)         SERIAL="$2"; shift 2 ;;
    --pkg)            PKG="$2"; shift 2 ;;
    --markers)        MARKERS="$2"; shift 2 ;;
    --helper-pkg)     HELPER_PKG="$2"; shift 2 ;;
    --helper-start)   HELPER_START=1; shift ;;
    --helper-delay)   HELPER_DELAY="$2"; shift 2 ;;
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
# 崩溃排除项：星光框架 CrashTrap 的「记录先前处理器」记账行含 sig=SIGSEGV/SIGABRT 字样，
# 那是初始化日志不是崩溃（详见文件头 ⚠️ 教训）。可用标记文件 crash_ignore| 覆盖。
# ⚠️ 崩溃误报教训 2（2026-10-12 疯狂水世界实测）：native 侧致命信号兜底装处理器时会打
#     `[Fkssj] fault guard armed (SIGSEGV/SIGBUS/SIGILL, altstack=65536)`——这行同样含
#     裸 SIGSEGV 且带本进程 pid，进程范围校验也拦不住 ⇒ 每次预检都误判「运行中崩溃」。
#     该行是**所有星光改包游戏**共有的初始化日志（不只是本作），故在默认排除项里全局兜掉。
RE_CRASH_IGNORE='记录先前处理器|\[crash\]|CrashTrap|fault guard armed|kDebuggerdJavaBacktrace|Wrote stack traces to tombstoned|skipping tombstone file creation|received crash request for pid|found intercept fd'
# ⚠️ 崩溃误报教训 3（2026-10-16 终末地 1.6.4 小米机实测）：`RE_CRASH` 里的**裸 `tombstone`**
#     会被 MIUI/HyperOS 的系统遥测误命中 —— MIUI 的 `ScoutManager`/`DumpScoutTraceThread` 会为
#     前台 App 抓 Java 栈，`tombstoned` 随即连打四行：
#       `registered intercept for pid … and type kDebuggerdJavaBacktrace`
#       `received crash request for pid …` / `found intercept fd …`
#       `Wrote stack traces to tombstoned` / `skipping tombstone file creation due to intercept`
#     ⇒ 每次 MIUI 抓栈都判「运行中崩溃」（本轮连吃两次假 FAIL_A）。反证判据：同窗口
#     `Fatal signal|E CRASH|ANR in` 计数 0 + `dumpsys activity exit-info` 无崩溃项 +
#     进程存活 + **无 tombstone 落盘**（真崩必然落盘）。上面五条已在默认排除项里全局兜掉。

# 崩溃行提取：命中 RE_CRASH 且不在 RE_CRASH_IGNORE 里的行（其余判定与展示一律走它）。
crash_hits() { grep -E "$RE_CRASH" "$1" 2>/dev/null | grep -vE "$RE_CRASH_IGNORE"; }

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
      # ★P0-1（2026-10-16）：**追加**语义。原为覆盖（`RE_CRASH_IGNORE="$v"`）⇒ 标记文件里
      # 只要加一条，默认排除项就被整体冲掉，游戏侧被迫把手抄的默认值也写进去
      # （实例：10_docs/终末地/预检标记.txt 的 crash_ignore 行不得不带默认四项）。
      crash_ignore) RE_CRASH_IGNORE="${RE_CRASH_IGNORE}|$v" ;;
      *) echo "! WARN 未知标记类别: ${k}（可用: pkg|menu_load|menu_shown|hook_hit|auth_fail|auth_ok|crash|crash_ignore）" ;;
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

# ---------- 1b. 宿主助手：起录前先 force-stop（消段=B 假阴性，见文件头教训） ----------
if [ -n "$HELPER_PKG" ]; then
  HP="$(adbx shell "pm path $HELPER_PKG" 2>/dev/null | tr -d '\r' | head -1)"
  if [ -z "$HP" ]; then
    echo "⚠ --helper-pkg $HELPER_PKG 未安装 ⇒ 跳过助手处理（菜单标记将由游戏内形态提供？）"
  else
    adbx shell "am force-stop $HELPER_PKG" >/dev/null 2>&1
    echo "- 已 force-stop 宿主助手 ${HELPER_PKG}（避免其一次性 attach 日志落在录制窗口外）"
  fi
fi

# ---------- 2. 起录（不 wipe 设备缓冲，-T 1 只取新行）＋干净启动 ----------
# 设备端 logcat 的 PID：直接 kill 本地 adb 客户端**不会**结束设备端 logcat 进程（它继续占内存/电池，
# 2026-10-09 实测一轮预检漏 1 个；本轮消消英雄2 收尾时一次清出 9 个历史泄漏）。
# 做法：远端 shell 先把自身 PID 写进设备侧 pid 文件，再 `exec logcat`（PID 不变），
# 收尾时只 kill 这一个 PID + 删 pid 文件 —— 不做设备端 `pkill -f logcat`（会误伤并行会话正在录的证据）。
# 取不到 PID 时自动退回旧行为（只杀本地客户端），不阻塞预检。
DEVLOG_PIDFILE="/data/local/tmp/starry_precheck_$$.pid"   # 非 root 可写；EXIT 时删除
adbx shell "sh -c 'echo \$\$ > ${DEVLOG_PIDFILE}; exec logcat -T 1 -v threadtime'" > "$LOG" 2>/dev/null &
LOGCAT_PID=$!
DEVLOG_PID=""
for _i in 1 2 3 4; do
  sleep 0.3
  DEVLOG_PID="$(adbx shell "cat ${DEVLOG_PIDFILE} 2>/dev/null" 2>/dev/null | tr -dc '0-9')"
  [ -n "$DEVLOG_PID" ] && break
done
cleanup() {
  if [ -n "$DEVLOG_PID" ]; then adbx shell "kill -9 $DEVLOG_PID" >/dev/null 2>&1 || true; fi
  if [ -n "${HELPER_PID:-}" ]; then kill "$HELPER_PID" 2>/dev/null; fi
  kill "$LOGCAT_PID" 2>/dev/null
  adbx shell "rm -f ${DEVLOG_PIDFILE}" >/dev/null 2>&1 || true
}
trap cleanup EXIT

adbx shell "am force-stop $PKG" >/dev/null 2>&1
sleep 1
adbx shell "monkey -p $PKG -c android.intent.category.LAUNCHER 1" > "$OUT/monkey.log" 2>&1
echo "- 已干净启动（force-stop 后 monkey 冷启）"

# 宿主助手必须在**录制窗口内**启动，否则其一次性 attach 日志（native menu shown）录不到。
if [ -n "$HELPER_PKG" ]; then
  if [ "$HELPER_START" = "1" ]; then
    (
      sleep "$HELPER_DELAY"
      adbx shell "monkey -p $HELPER_PKG -c android.intent.category.LAUNCHER 1" >/dev/null 2>&1
    ) &
    HELPER_PID=$!
    echo "- ${HELPER_DELAY}s 后将拉起宿主助手 ${HELPER_PKG}（窗口内 attach ⇒ 菜单标记可被录到）"
  else
    echo "- 请在**预检结束前**自行点开宿主助手 ${HELPER_PKG}（否则 native menu shown 不在窗口内 ⇒ 段=B 假阴性）"
  fi
fi

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
    if grep -q "$PKG" "$LOG" 2>/dev/null && [ -n "$(crash_hits "$LOG")" ]; then
      echo "== 崩溃上下文（logcat 尾 40 行）=="
      tail -40 "$LOG"
      say_fail_a "进程消失且 logcat 含 ${PKG} 崩溃标记"
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

    # 崩溃：先剔框架自身记账日志（CrashTrap 装处理器会打 sig=SIGSEGV/SIGABRT，
    # 那不是崩溃且带本进程 pid，进程范围校验拦不住），再限本游戏范围
    # （pid 或包名），防别的 app 崩误报；
    # java 崩的 FATAL EXCEPTION 行不带 pid，往后 6 行找 "Process: 包名"
    LN="$(grep -nE "$RE_CRASH" "$CHUNK" 2>/dev/null | grep -vE "$RE_CRASH_IGNORE" | head -1 | cut -d: -f1)"
    if [ -n "$LN" ]; then
      CRASH_SCOPE="$PKG"
      [ -n "$CUR_PID" ] && CRASH_SCOPE="$CUR_PID|$CRASH_SCOPE"
      if sed -n "${LN},$((LN + 6))p" "$CHUNK" | grep -qE "$CRASH_SCOPE"; then
        echo "== 崩溃标记（限 ${PKG}）=="
        crash_hits "$CHUNK" | head -3
        L0=$((LN - 20)); [ "$L0" -lt 1 ] && L0=1
        echo "== 上下文 ±20 行 =="
        sed -n "${L0},$((LN + 20))p" "$CHUNK"
        say_fail_a "运行中崩溃: $(crash_hits "$CHUNK" | head -1)"
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
      cleanup   # 本地客户端 + 设备端 logcat 一起收（EXIT trap 会再跑一次，幂等）
      if [ -n "$RE_HOOK" ] && [ "$hook_ok" -eq 0 ]; then
        echo "判定段=B 依据=菜单弹出但 hook_hit 标记（${RE_HOOK}）未命中"
        echo "证据: ${OUT}（logcat.txt 已留存）"
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
    echo "判定段=B 依据=so 加载标记已现但未见菜单弹出标记（${RE_MENU_SHOWN}），logcat 尾 20 行："
    tail -20 "$LOG"
    echo "证据: $OUT"
    exit 2
  elif [ "$AUTH_DL" -eq 0 ] && [ "$NOW" -ge "$LAUNCH_DL" ]; then
    echo "判定段=A 依据=${LAUNCH_TIMEOUT}s 内未见菜单 so 加载标记（${RE_MENU_LOAD}），logcat 尾 20 行："
    tail -20 "$LOG"
    echo "证据: $OUT"
    exit 1
  fi

  sleep 2
done
