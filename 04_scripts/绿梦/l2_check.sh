#!/usr/bin/env bash
# ============================================================================
#  l2_check.sh — 绿梦：时空之声 的 L2（机器侧）验收闸门
#
#  为什么不用 post_install_precheck.sh：本游戏的「装机」不是装 APK，而是**替换游戏
#  cache 里的热更 bundle**（见 10_docs/绿梦/侦察笔记.md §4）。所以机器侧判据是
#  「进程活着 + 热更 dll 真的被加载 + 没有加载期错误码 + 我们的文件没被改写」。
#
#  用法:
#    bash 04_scripts/绿梦/l2_check.sh [--serial S] [--launch] [--wait 60]
#         [--expect-data-md5 <md5>] [--tag <日志前缀，默认 StarryLvmeng>]
#
#  不带 --launch 时只做静态面（设备文件 md5 / 状态文件 / mod dll 是否就位），
#  **不启动游戏、不打断用户**；带 --launch 才走飞行记录仪冷启并判运行面。
#
#  ⚠ 取证通道铁律（本游戏吞 Debug.Log/Debug.LogWarning，只转发 Error/Exception）：
#     游戏侧的证据日志**必须用 Debug.LogError / Debug.LogException**，否则本闸门永远 0 命中。
#     源码证据见 10_docs/绿梦/侦察笔记.md §8。
#
#  退出码: 0 = PASS；4 = 静态面 FAIL；5 = 运行面 FAIL；2 = 用法/环境错误
# ============================================================================
set -u

SELF_DIR="$(cd "$(dirname "$0")" && pwd -P)"
ROOT="${NIXIANG_ROOT:-$(cd "$SELF_DIR/../.." && pwd -P)}"
SERIAL="${ANDROID_SERIAL:-}"
PKG="com.zhuziplay.lvmengnew.op"
GUID="02341ed99641ed1b023420439a78fc9d"
LAUNCH=0
WAIT=60
EXPECT_DATA_MD5=""
TAG="StarryLvmeng"
ACTIVITY="com.bamboo.game.SplashActivity"

while [ $# -gt 0 ]; do
  case "$1" in
    --serial) SERIAL="${2:-}"; shift 2 ;;
    --launch) LAUNCH=1; shift ;;
    --wait)   WAIT="${2:-60}"; shift 2 ;;
    --expect-data-md5) EXPECT_DATA_MD5="${2:-}"; shift 2 ;;
    --tag)    TAG="${2:-}"; shift 2 ;;
    -h|--help) sed -n '2,20p' "$0"; exit 0 ;;
    *) echo "未知参数: $1" >&2; exit 2 ;;
  esac
done

ADB=(adb)
[ -n "$SERIAL" ] && ADB=(adb -s "$SERIAL")
DEV_BASE="/sdcard/Android/data/$PKG/files/yoo/Dll"
DEV_BDIR="$DEV_BASE/BundleFiles/02/$GUID"
MEDIA_DIR="/sdcard/Android/media/$PKG"

sh_su() { "${ADB[@]}" shell "su -c \"$1\"" 2>&1 | tr -d '\r'; }

FAILS=0
say()  { echo "   $*"; }
ok()   { echo "   [PASS] $*"; }
bad()  { echo "   [FAIL] $*"; FAILS=$((FAILS + 1)); }

echo "== 静态面 =="
DEV_MD5="$(sh_su "md5sum '$DEV_BDIR/__data' 2>/dev/null" | awk '{print $1}')"
if [ -z "$DEV_MD5" ]; then
  bad "设备上找不到 $DEV_BDIR/__data"
else
  say "device __data md5 = $DEV_MD5"
  if [ -n "$EXPECT_DATA_MD5" ]; then
    if [ "$DEV_MD5" = "$EXPECT_DATA_MD5" ]; then ok "bundle md5 与期望一致"; else bad "bundle md5 与期望不一致（期望 ${EXPECT_DATA_MD5}）"; fi
  else
    say "（未给 --expect-data-md5，跳过比对）"
  fi
fi

INFO_HEX="$(sh_su "od -An -tx1 '$DEV_BDIR/__info' 2>/dev/null" | tr -d ' \n')"
if [ -z "$INFO_HEX" ]; then
  bad "__info 读不到"
else
  say "__info = $INFO_HEX"
  DEV_SIZE="$(sh_su "stat -c %s '$DEV_BDIR/__data' 2>/dev/null" | tr -dc '0-9')"
  # __info: 2B 长度 + CRC 字符串 + 8B LE 大小
  INFO_SIZE_LE="$(printf '%s' "$INFO_HEX" | tail -c 16)"
  say "设备 __data 大小 = ${DEV_SIZE:-?}；__info 里的 LE 大小字节 = $INFO_SIZE_LE"
  if [ -n "$DEV_SIZE" ]; then
    CALC="$(python3 -c "import struct; print(struct.pack('<q', $DEV_SIZE).hex())" 2>/dev/null)"
    if [ "$CALC" = "$INFO_SIZE_LE" ]; then ok "__info 的大小字段与 __data 实际大小自洽（YooAsset Middle 级校验只看这个）"; else bad "__info 大小字段与 __data 实际大小不符（$INFO_SIZE_LE vs ${CALC}）"; fi
  fi
fi

for d in "$MEDIA_DIR" "/sdcard/Android/data/$PKG/files"; do
  SF="$(sh_su "cat '$d/starry_state.txt' 2>/dev/null" | head -1)"
  if [ -n "$SF" ]; then ok "状态文件在 $d"; say "    内容: ${SF}"; else
    if [ "$LAUNCH" = 1 ]; then bad "状态文件缺失: $d/starry_state.txt"; else say "状态文件缺失: $d/starry_state.txt（静态面不算 FAIL）"; fi
  fi
  ML="$(sh_su "ls -l '$d/starry_lvmeng.dll' 2>/dev/null")"
  if [ -n "$ML" ]; then ok "mod dll 在 $d"; say "    ${ML}"; else
    if [ "$LAUNCH" = 1 ]; then bad "mod dll 缺失: $d/starry_lvmeng.dll"; else say "mod dll 缺失: $d/starry_lvmeng.dll（静态面不算 FAIL）"; fi
  fi
done

if [ "$LAUNCH" != 1 ]; then
  echo "== 汇总（仅静态面）=="
  echo "FAIL=$FAILS"
  [ "$FAILS" = 0 ] && { echo "结论: 静态面 PASS（未启动游戏）"; exit 0; } || { echo "结论: 静态面 FAIL"; exit 4; }
fi

echo "== 运行面（飞行记录仪冷启）=="
REC="09_test/绿梦/l2_$(date +%Y%m%d_%H%M%S)"
( cd "$ROOT" && bash 04_scripts/_common/flight_recorder.sh start --pkg "$PKG" --serial "$SERIAL" --out "$REC" >/dev/null 2>&1 )
say "记录目录 $REC"
"${ADB[@]}" shell "am force-stop $PKG" >/dev/null 2>&1
sleep 2
"${ADB[@]}" shell "am start -n $PKG/$ACTIVITY" >/dev/null 2>&1
say "已冷启，等待 ${WAIT}s ..."
sleep "$WAIT"

PID="$( "${ADB[@]}" shell "pidof $PKG" 2>/dev/null | tr -d '\r' )"
( cd "$ROOT" && bash 04_scripts/_common/flight_recorder.sh stop --dir "$REC" >/dev/null 2>&1 )
LOG="$ROOT/$REC/logcat.txt"
if [ ! -f "$LOG" ]; then bad "记录文件缺失: $LOG"; else
  N_TAG="$(grep -ac "\[$TAG\]" "$LOG" 2>/dev/null || true)"
  # ★ 真正的判据走**文件证据**：游戏用 LuminApplication 的日志器且 LogEnabled=false 时
  #   **连 LogError 都被吞掉**（LuminApplication.cs:68），所以 logcat 对本游戏是不可靠通道。
  #   引导链证据 = /data/data/<pkg>/files/starry_loader.log（注入代码写）
  #   mod 证据   = <media dir>/starry_log.txt（热更 dll 写）
  LOADER_LOG="$(sh_su "cat /data/data/$PKG/files/starry_loader.log 2>/dev/null")"
  MOD_LOG="$(sh_su "cat '$MEDIA_DIR/starry_log.txt' 2>/dev/null")"
  if printf '%s' "$LOADER_LOG" | grep -q "\[boot\] enter"; then
    ok "引导证据：Hello::Run prefix 执行了（starry_loader.log 有 [boot] enter）"
  else
    bad "starry_loader.log 里没有 [boot] enter —— 注入的 prefix 没执行"
  fi
  if printf '%s' "$LOADER_LOG" | grep -q "\[load\] ok"; then
    ok "引导证据：mod dll 加载成功（$(printf '%s' "$LOADER_LOG" | grep -a '\[load\] ok' | head -1)）"
  else
    bad "mod dll 未加载成功；引导日志尾部：$(printf '%s' "$LOADER_LOG" | tail -2 | tr '\n' ' ')"
  fi
  if printf '%s' "$MOD_LOG" | grep -q "StarryLvmeng"; then
    ok "mod 证据：$(printf '%s' "$MOD_LOG" | head -1)"
  else
    bad "mod 未落证据（starry_log.txt 空或缺失）"
  fi
  N_C03="$(grep -ac "C03-0" "$LOG" 2>/dev/null || true)"
  N_AB="$(grep -ac "Failed to load asset bundle" "$LOG" 2>/dev/null || true)"
  say "日志行数 $(wc -l < "$LOG")"
  # logcat 只作**参考**：本游戏 LogEnabled=false 时连 LogError 都被吞（侦察笔记 §8），
  # 所以 0 命中**不算 FAIL**；真正的判据是上面的文件证据。
  if [ "${N_TAG:-0}" -gt 0 ]; then
    ok "logcat 命中 [$TAG] $N_TAG 行（额外旁证）"
  else
    say "logcat 0 行 [$TAG]（本游戏日志通道可能被 LogEnabled 关闭，按设计不算 FAIL；以文件证据为准）"
  fi
  [ "${N_C03:-0}" = 0 ] && ok "无 C03-0 加载错误码" || bad "出现 $N_C03 行 C03-0 加载错误"
  [ "${N_AB:-0}" = 0 ] && ok "无 asset bundle 加载失败" || bad "出现 $N_AB 行 bundle 加载失败"
fi
if [ -n "$PID" ]; then ok "进程存活 pid=${PID}（冷启后 ${WAIT}s）"; else bad "进程已退出（冷启后 ${WAIT}s）"; fi

AFTER_MD5="$(sh_su "md5sum '$DEV_BDIR/__data' 2>/dev/null" | awk '{print $1}')"
if [ "$AFTER_MD5" = "$DEV_MD5" ]; then ok "运行后 bundle 未被改写/重下"; else bad "运行后 bundle md5 变了：$DEV_MD5 → $AFTER_MD5"; fi

echo "== 汇总 =="
echo "FAIL=$FAILS  记录目录: $REC"
[ "$FAILS" = 0 ] && { echo "结论: L2 PASS"; exit 0; } || { echo "结论: L2 FAIL"; exit 5; }
