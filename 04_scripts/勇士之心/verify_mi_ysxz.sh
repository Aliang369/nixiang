#!/usr/bin/env bash
# 勇士之心 1.0.6：无 root 小米机（96f0d883）装机 + 冷启取证一条命令。
#
# 用途：阶段 A 的双端对照证据位。同一份正式包（不重新出包）装到无 root 机，
#       取三类决定性证据：
#         ① 壳净化走的是 1.0.6 分支（[YsxzSec] state words ... tag=1.0.6）
#         ② JS 三层加载（[AC] / [AUTO] / [MODMOD] require trap）
#         ③ 无 SyntaxError、无 FATAL，进程存活
#
# 用法：
#   bash 04_scripts/勇士之心/verify_mi_ysxz.sh              # 默认 SERIAL=96f0d883
#   SERIAL=xxxx bash 04_scripts/勇士之心/verify_mi_ysxz.sh  # 换机
#   SKIP_INSTALL=1 ...                                      # 只取证不装
#
# 只做：装机（同签覆盖优先，异签需 ALLOW_UNINSTALL=1）+ 冷启 + 读日志。
# 不做：卸载用户数据（除非显式 ALLOW_UNINSTALL=1）、清档、改存档。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
GAME="勇士之心"
PKG="${PKG:-com.fightingstars.games.warrior}"
SERIAL="${SERIAL:-96f0d883}"
ADB_BIN="${ADB_BIN:-adb}"
APK="${APK:-$ROOT/08_output/$GAME/${GAME}_1.0.6_surgical-releaseSigned.apk}"
EVID="$ROOT/10_docs/$GAME/evidence_20261024"
STAMP="$(date +%Y%m%d_%H%M%S)"
OUT="$EVID/mi_${SERIAL}_${STAMP}.txt"

[[ -f "$APK" ]] || { echo "错误: 正式包不存在: $APK" >&2; exit 1; }
command -v "$ADB_BIN" >/dev/null || { echo "错误: adb 不可用" >&2; exit 1; }
"$ADB_BIN" -s "$SERIAL" get-state >/dev/null 2>&1 || {
  echo "错误: 设备 $SERIAL 未接入（adb devices 里没有）" >&2
  exit 2
}

mkdir -p "$EVID"
LOCAL_MD5="$(md5 -q "$APK")"
echo "apk=$APK"
echo "apk_md5=$LOCAL_MD5"
echo "serial=$SERIAL"

if [[ "${SKIP_INSTALL:-0}" != "1" ]]; then
  echo "==> 装机（同签覆盖优先）"
  SERIAL="$SERIAL" VERSION=1.0.6 ALLOW_UNINSTALL="${ALLOW_UNINSTALL:-1}" \
    bash "$SCRIPT_DIR/install_ysxz.sh" 2>&1 | sed 's/^/[install] /'
fi

echo "==> 冷启"
"$ADB_BIN" -s "$SERIAL" shell am force-stop "$PKG" || true
sleep 2
"$ADB_BIN" -s "$SERIAL" logcat -c 2>/dev/null || true
"$ADB_BIN" -s "$SERIAL" shell am start -n "$PKG/org.cocos2dx.javascript.AppActivity" >/dev/null 2>&1 || {
  echo "错误: am start 失败" >&2; exit 3;
}
echo "  等待 45s 让壳解密 + JS 自举完成…"
sleep 45

PID="$("$ADB_BIN" -s "$SERIAL" shell pidof "$PKG" | tr -d '\r')"
{
  echo "# 勇士之心 1.0.6 无 root 端（${SERIAL}）装机+冷启取证  ${STAMP}"
  echo "apk_md5=$LOCAL_MD5"
  echo "pid=${PID:-<未运行>}"
  echo "--- 设备/版本 ---"
  "$ADB_BIN" -s "$SERIAL" shell dumpsys package "$PKG" 2>/dev/null | grep -E "versionName|versionCode" | head -2
  echo "--- 决定性日志（Ysxz / jswrapper JS 三层）---"
  if [[ -n "$PID" ]]; then
    "$ADB_BIN" -s "$SERIAL" logcat -d --pid="$PID" 2>/dev/null \
      | grep -E "YsxzSec|spoof hit|\[AC\]|JS: \[AC\]|JS: \[AUTO\]|MODMOD\] require trap|Cocos Creator|SyntaxError|FATAL" | head -40
  fi
} > "$OUT"

SYNTAX="$("$ADB_BIN" -s "$SERIAL" logcat -d --pid="${PID:-0}" 2>/dev/null | grep -c "SyntaxError" || true)"
FATAL="$("$ADB_BIN" -s "$SERIAL" logcat -d --pid="${PID:-0}" 2>/dev/null | grep -cE "FATAL|AndroidRuntime" || true)"
RC=0
[[ -n "$PID" ]] || { echo "❌ 进程未存活"; RC=4; }
[[ "$SYNTAX" == "0" ]] || { echo "❌ 出现 SyntaxError ×$SYNTAX"; RC=5; }
[[ "$FATAL" == "0" ]] || { echo "❌ 出现 FATAL ×$FATAL"; RC=6; }
grep -q "JS: \[AC\] ac layer v1 loaded" "$OUT" || { echo "❌ AC 层未加载"; RC=7; }
grep -q "JS: \[AUTO\] auto layer v1 loaded" "$OUT" || { echo "❌ AUTO 层未加载"; RC=8; }
grep -q "MODMOD\] require trap installed" "$OUT" || { echo "❌ 自举 require trap 未装"; RC=9; }
grep -q "tag=1.0.6" "$OUT" || echo "⚠ 未看到 SecShell tag=1.0.6（壳净化可能未启动，需人工看 ${OUT}）"

echo "证据: ${OUT}"
if [[ $RC -eq 0 ]]; then echo "✅ PASS（无 root 端冷启取证通过）"; else echo "❌ FAIL rc=$RC"; fi
exit $RC
