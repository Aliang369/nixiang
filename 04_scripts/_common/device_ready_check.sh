#!/usr/bin/env bash
# ================================================================
#  device_ready_check.sh — 真机测试「就绪门禁」（装机后、请用户上手前跑一次）
#
#  为什么要有（2026-10-14 潮汐守望者会话复盘 P0-1）:
#    那一轮白跑了 3 次：① 设备锁屏 ⇒ `MenuService: no host within 25s → zombie service`、
#    `NativeMenu: show failed`、全程零条 mod 日志，被误读成「功能失效」；② 焦点不在游戏；
#    ③ 收尾时设备掉线，logcat 采集 job 以 exit 255 结束，没人知道缺了哪一段。
#    这些全是「跑之前一条命令就能查出来」的前置条件。
#
#  用法:
#    bash 04_scripts/_common/device_ready_check.sh --serial 96f0d883 [--pkg com.gf.cxswz]
#         [--min-free-mb 500] [--strict] [--quiet]
#    --pkg     给出时额外校验「当前前台焦点」是否就是该包（未前台 ⇒ WARN，不阻断）
#    --strict  出现任意 FAIL 时 exit 4（用于 CI / 装机流水线；默认只报告）
#    --quiet   不打印逐项 OK 行，只打印 WARN/FAIL 与末行结论
#
#  检查项: adb 可见 / 命令稳定性(3 连) / 唤醒态 / 锁屏 / 前台焦点 / 与宿主时间差 /
#          /data 可用空间 / 电量与温度 / root 可用性（只报告，不阻断）
#  退出码: 0=READY（可能有 WARN）；4=NOT_READY（有 FAIL，或 --strict 下的 FAIL）；2=用法错
# ================================================================
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="${NIXIANG_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"

SERIAL=""; PKG=""; MIN_FREE_MB=500; STRICT=0; QUIET=0
while [ $# -gt 0 ]; do
  case "$1" in
    --serial) SERIAL="${2:-}"; shift 2 ;;
    --pkg) PKG="${2:-}"; shift 2 ;;
    --min-free-mb) MIN_FREE_MB="${2:-500}"; shift 2 ;;
    --strict) STRICT=1; shift ;;
    --quiet) QUIET=1; shift ;;
    -h|--help) sed -n '2,26p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
    *) echo "未知参数: $1" >&2; exit 2 ;;
  esac
done

command -v adb >/dev/null 2>&1 || { echo "[GATE] NOT_READY fails=1 reason=no-adb"; exit 4; }
if [ -z "$SERIAL" ]; then
  SERIAL="$(adb devices | awk 'NR>1 && $2=="device"{print $1; exit}')"
fi
[ -n "$SERIAL" ] || { echo "[GATE] NOT_READY fails=1 reason=no-online-device"; exit 4; }

FAILS=0; WARNS=0
ok()   { [ "$QUIET" = "1" ] || echo "[CHECK] $1 status=OK detail=$2"; }
warn() { WARNS=$((WARNS + 1)); echo "[CHECK] $1 status=WARN detail=$2"; }
fail() { FAILS=$((FAILS + 1)); echo "[CHECK] $1 status=FAIL detail=$2"; }

sh() { adb -s "$SERIAL" shell "$@" 2>/dev/null | LC_ALL=C tr -d '\r'; }

# ---------- 1. 设备可见 + 命令稳定性（掉线最常见的形态是「有时能连」）----------
STABLE=0; i=0
while [ "$i" -lt 3 ]; do
  i=$((i + 1))
  if [ "$(sh echo ok)" = "ok" ]; then STABLE=$((STABLE + 1)); fi
done
if [ "$STABLE" = "3" ]; then ok adb_stable "3/3"; else fail adb_stable "$STABLE/3（设备可能掉线或 adb server 不稳）"; fi
if [ "$STABLE" = "0" ]; then echo "[GATE] NOT_READY fails=$FAILS reason=device-unreachable serial=$SERIAL"; exit 4; fi

# ---------- 2. 唤醒态 / 锁屏 ----------
WAKE="$(dumpsys power 2>/dev/null | grep -m1 -E 'mWakefulness=' | awk -F= '{print $2}')"
[ -z "$WAKE" ] && WAKE="$(sh dumpsys power | grep -m1 -E 'mWakefulness=' | awk -F= '{print $2}')"
case "$WAKE" in
  Awake) ok wakefulness "$WAKE" ;;
  "") warn wakefulness "取不到 mWakefulness（接口差异，请在设备上确认屏幕已点亮）" ;;
  *) fail wakefulness "${WAKE}（请点亮并解锁屏幕）" ;;
esac

DREAM="$(sh dumpsys window | grep -m1 -E 'mDreamingLockscreen=' | awk -F= '{print $2}' | awk '{print $1}')"
case "$DREAM" in
  false) ok dreaming_lockscreen "false" ;;
  true) fail dreaming_lockscreen "true（锁屏中：mod 菜单会拿不到 activity ⇒ 白跑一轮）" ;;
  *) warn dreaming_lockscreen "取不到 mDreamingLockscreen" ;;
esac

# ---------- 3. 前台焦点 ----------
FOCUS="$(sh dumpsys window | grep -m1 -E 'mCurrentFocus=' | sed 's/.*mCurrentFocus=//')"
FOCUS_PKG="$(printf '%s' "$FOCUS" | sed -n 's/.* \([a-zA-Z0-9_.]*\)\/.*/\1/p')"
if [ -z "$FOCUS" ]; then
  warn current_focus "取不到 mCurrentFocus"
elif [ -n "$PKG" ] && [ "$FOCUS_PKG" = "$PKG" ]; then
  ok current_focus "$FOCUS_PKG"
elif [ -n "$PKG" ]; then
  warn current_focus "当前=${FOCUS_PKG:-未知} 期望=${PKG}（未前台，日志会缺 mod 行）"
else
  ok current_focus "${FOCUS_PKG:-未知}"
fi

# ---------- 4. 与宿主时间差（日志切片按时间戳对齐，差 >5s 会让切片错窗）----------
HOST_EPOCH="$(date +%s)"
DEV_EPOCH="$(sh date +%s | tr -d ' ')"
if printf '%s' "$DEV_EPOCH" | grep -qE '^[0-9]+$'; then
  DIFF="$((DEV_EPOCH - HOST_EPOCH))"
  [ "$DIFF" -lt 0 ] && DIFF="$((0 - DIFF))"
  if [ "$DIFF" -le 5 ]; then ok clock_skew "${DIFF}s"; else warn clock_skew "${DIFF}s（>5s，切片窗口需按设备时钟算）"; fi
else
  warn clock_skew "取不到设备时间"
fi

# ---------- 5. /data 可用空间 ----------
FREE_KB="$(sh df /data | awk 'NR>1{print $4; exit}' | tr -d ' ')"
case "$FREE_KB" in
  ''|*[!0-9]*) warn free_space "取不到 df /data" ;;
  *)
    FREE_MB=$((FREE_KB / 1024))
    if [ "$FREE_MB" -ge "$MIN_FREE_MB" ]; then ok free_space "${FREE_MB}MB"; else warn free_space "${FREE_MB}MB < ${MIN_FREE_MB}MB"; fi
    ;;
esac

# ---------- 6. 电量 / 温度（过热降频会让「掉帧」被误读成 mod 卡顿）----------
BATT="$(sh dumpsys battery | grep -m1 -E '^  level:' | awk '{print $2}')"
TEMP="$(sh dumpsys battery | grep -m1 -E '^  temperature:' | awk '{print $2}')"
if printf '%s' "$BATT" | grep -qE '^[0-9]+$'; then
  if [ "$BATT" -le 15 ]; then warn battery "${BATT}%（低电可能导致降频）"; else ok battery "${BATT}%"; fi
else
  warn battery "取不到电量"
fi
if printf '%s' "$TEMP" | grep -qE '^[0-9]+$'; then
  if [ "$TEMP" -ge 430 ]; then warn temperature "$((TEMP / 10)).$((TEMP % 10))C（偏高，可能降频）"; else ok temperature "$((TEMP / 10)).$((TEMP % 10))C"; fi
else
  warn temperature "取不到温度"
fi

# ---------- 7. root（只报告：影响 tombstone 可读性与 su 类操作）----------
if sh su -c id | grep -q 'uid=0'; then ok root "yes"; else ok root "no（tombstone / dumpsys dropbox 可能不可读，走 App 自身 crash_logs）"; fi

echo "[GATE] serial=$SERIAL warns=$WARNS fails=$FAILS"
if [ "$FAILS" -gt 0 ]; then
  echo "[GATE] NOT_READY fails=$FAILS ⇒ 先按上面 FAIL 处置，再请用户上手（不要拿锁屏/后台轮次当功能结论）"
  exit 4
fi
echo "[GATE] READY"
exit 0
