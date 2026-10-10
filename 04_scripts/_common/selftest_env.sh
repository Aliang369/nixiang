#!/usr/bin/env bash
# ================================================================
#  selftest_env.sh — 自测影响面快照（AGENTS.md P17）
#
#  背景（2026-10-10 邂逅在迷宫第二轮复盘 P17）：
#  本会话三次「自伤」全部发生在**我直接动用户正在用的设备/链路**时：
#    ① DNS 伪造成 10.9.9.9 ⇒ 隧道自关后资源下载黑洞（游戏卡「资源下载中 0%」）；
#    ② 探针 scope 解错 ⇒ 广告面板卡死；③ 我自测 `am force-stop com.starry.magic`
#    ⇒ 菜单球消失，用户以为交付缺了菜单（m05821）。
#  对策：**凡会碰到用户设备的动作（装包/启停助手与游戏/起 VPN/改 DNS·路由/force-stop），
#  动手前先拍一张快照，做完再拍一张，汇报里写「已复原」或未复原的原因。**
#
#  用法:
#    bash 04_scripts/_common/selftest_env.sh before|after <游戏> \
#      [--serial <S>] [--pkg <游戏包名>] [--helper-pkg <助手包名>] [--note "本轮要做什么"]
#  示例:
#    bash 04_scripts/_common/selftest_env.sh before 邂逅在迷宫 \
#      --serial f9a116a2 --pkg cn.alltune.endless --helper-pkg com.starry.magic \
#      --note "重启助手做通道实跑，结束后需恢复菜单球"
#
#  落点: 09_test/<游戏>/<YYYYMMDD>/env_before.txt / env_after.txt（09_test/ 已 gitignore）
#  退出码: 0=已拍快照；1=adb/设备不可用；2=用法错
# ================================================================
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=env.sh
source "$SCRIPT_DIR/env.sh"

usage() {
  sed -n '2,26p' "$0" | sed 's/^# \{0,1\}//'
  exit 2
}

PHASE=""; GAME=""; SERIAL=""; PKG=""; HELPER=""; NOTE=""
while [ $# -gt 0 ]; do
  case "$1" in
    --serial)     SERIAL="${2:-}"; shift 2 ;;
    --pkg)        PKG="${2:-}"; shift 2 ;;
    --helper-pkg) HELPER="${2:-}"; shift 2 ;;
    --note)       NOTE="${2:-}"; shift 2 ;;
    -h|--help) usage ;;
    -*) echo "未知参数: $1" >&2; usage ;;
    *) if [ -z "$PHASE" ]; then PHASE="$1"; elif [ -z "$GAME" ]; then GAME="$1"; fi; shift ;;
  esac
done
case "$PHASE" in before|after) : ;; *) echo "✗ 第一个参数必须是 before 或 after" >&2; usage ;; esac
[ -n "$GAME" ] || usage

ADB="${ADB:-adb}"
command -v "$ADB" >/dev/null 2>&1 || { echo "✗ 找不到 adb（设 ADB=/abs/path/adb）" >&2; exit 1; }
ADBX="$ADB"; [ -n "$SERIAL" ] && ADBX="$ADB -s $SERIAL"
ST="$($ADBX get-state 2>/dev/null || true)"
[ "$ST" = "device" ] || { echo "✗ 设备不可用（get-state=${ST:-空}）" >&2; exit 1; }

DAY="$(date +%Y%m%d)"
OUT_DIR="$NIXIANG_ROOT/09_test/$GAME/$DAY"
mkdir -p "$OUT_DIR"
OUT="$OUT_DIR/env_${PHASE}.txt"

sh_() { $ADBX shell "$1" 2>/dev/null | tr -d '\r'; }

MODEL="$(sh_ 'getprop ro.product.model')"
REL="$(sh_ 'getprop ro.build.version.release')"
HELPER_PID=""; [ -n "$HELPER" ] && HELPER_PID="$(sh_ "pidof $HELPER")"
GAME_PID="";   [ -n "$PKG" ]    && GAME_PID="$(sh_ "pidof $PKG")"
HELPER_VN=""; [ -n "$HELPER" ] && HELPER_VN="$(sh_ "dumpsys package $HELPER" | grep -m1 'versionName=' | sed 's/.*versionName=//')"
GAME_VN="";   [ -n "$PKG" ]    && GAME_VN="$(sh_ "dumpsys package $PKG" | grep -m1 'versionName=' | sed 's/.*versionName=//')"
OVERLAY="";   [ -n "$HELPER" ] && OVERLAY="$(sh_ "appops get $HELPER SYSTEM_ALERT_WINDOW" | head -1)"
TUN0="$(sh_ 'ip addr show tun0' | head -1)"
[ -n "$TUN0" ] || TUN0="（无 tun0）"
RESUMED="$(sh_ 'dumpsys activity activities' | grep -m1 'mResumedActivity')"

{
  echo "# 自测影响面快照（${PHASE}）"
  echo "时间        : $(date '+%Y-%m-%d %H:%M:%S')"
  echo "host        : ${NIXIANG_HOST}（自测执行机）"
  echo "设备        : ${SERIAL:-（唯一设备）}  $MODEL / Android $REL"
  echo "本次自测    : ${NOTE:-（未写 --note，请补：本轮会对用户设备做什么）}"
  echo "助手进程    : ${HELPER_PID:-（未运行 / 未给 --helper-pkg）}"
  echo "助手版本    : ${HELPER_VN:-?}"
  echo "悬浮窗授权  : ${OVERLAY:-（未查 / 未给 --helper-pkg）}"
  echo "游戏进程    : ${GAME_PID:-（未运行 / 未给 --pkg）}"
  echo "游戏版本    : ${GAME_VN:-?}"
  echo "隧道 tun0   : $TUN0"
  echo "前台Activity: ${RESUMED:-?}"
} > "$OUT"

echo "✓ 快照已存：$OUT"
sed 's/^/   /' "$OUT"

# ---------- after：与 before 对照，给出「是否需复原」 ----------
if [ "$PHASE" = "after" ]; then
  BEFORE="$OUT_DIR/env_before.txt"
  if [ ! -f "$BEFORE" ]; then
    echo
    echo "⚠ 找不到 $BEFORE ⇒ 无法自动对照。请自查：助手是否在跑？tun0 是否已撤？"
    exit 0
  fi
  b_of() { sed -n "s/^$1 *: *//p" "$BEFORE" | head -1; }
  b_helper="$(b_of '助手进程')"; a_helper="${HELPER_PID:-（未运行 / 未给 --helper-pkg）}"
  b_tun="$(b_of '隧道 tun0')"
  echo
  echo "── 复原自查（before → after）──"
  need=0
  case "$b_helper" in ''|（*) : ;; *)
    case "$a_helper" in ''|（*) echo "  ⚠ 助手 before 在跑、after 没跑 ⇒ **需复原**：adb shell am start -n <助手包>/.app.MainActivity（或让用户点通知）"; need=1 ;; *) echo "  ✓ 助手进程仍在（${a_helper}）" ;; esac ;;
  esac
  case "$b_tun" in *无\ tun0*|'')
    case "$TUN0" in *无\ tun0*) : ;; *) echo "  ⚠ before 无隧道、after 有 tun0 ⇒ **需复原**：停通道（CDN_STOP / 关助手）"; need=1 ;; esac ;;
  esac
  [ "$need" = "1" ] || echo "  ✓ 未发现需要复原的项（仍建议肉眼确认菜单球在不在）"
  echo "  （汇报里必须写「已复原」或未复原的原因 —— AGENTS.md P17）"
fi
exit 0
