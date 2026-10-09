#!/usr/bin/env bash
# ================================================================
#  check_release_apk.sh — 出包门禁（Java/APK 侧，P6 的半步）
#
#  背景（2026-10-10 邂逅在迷宫会话复盘 P6）：
#  本会话助手包曾带着"只在调试时才该存在"的东西一路装到真机：
#     · 自测开关（`--ez starry_selftest true` / `BuildConfig.DEBUG` 分支）
#     · 手写诊断文案（装机日志里的临时排查字串）
#     · 未关的 debuggable
#  这些一旦随正式包交付，就是把内部脚手架当产品发出去。native 侧已有
#  `check_release_clean.sh`（扫 jni/*.cpp 的裸日志/诊断宏）；本脚本补 **Java/APK 侧**：
#    查1 manifest：aapt2 dump badging 不得出现 application-debuggable
#    查2 禁用串：classes*.dex / assets/* / lib/* 内不得出现清单里的字串
#                （清单 = 内置默认项 + 10_docs/<游戏>/出包禁用串.txt + --forbid 追加）
#    查3 版本一致性：badging 的 versionCode/versionName 与 --expect-vc/--expect-vn 相符
#
#  用法:
#    bash 04_scripts/_common/check_release_apk.sh --apk 08_output/<游戏>/xxx.apk [--game <游戏>]
#    bash 04_scripts/_common/check_release_apk.sh --apk x.apk --forbid 'starry_selftest' --forbid '--ez '
#    bash 04_scripts/_common/check_release_apk.sh --list          # 打印生效的禁用串清单
#
#  退出码: 0=干净；1=有违规（**不要交付这个包**）；2=用法错
#  铁律: 出包门禁只认"产物里没有"，不认"我记得删了"。
# ================================================================
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=env.sh
source "$SCRIPT_DIR/env.sh"

APK=""; GAME=""; LIST=0; EXPECT_VC=""; EXPECT_VN=""
FORBID_FILE=""
# 内置禁用串（诊断脚手架专用；正式包出现即违规）
FORBID_DEFAULT='starry_selftest
starry_diag
starry_debug
--ez starry
MOD_DIAG
DIAG_ONLY
TEMP_DEBUG'
FORBID_EXTRA=""

usage() {
  sed -n '2,26p' "$0" | sed 's/^# \{0,1\}//'
}

while [ $# -gt 0 ]; do
  case "$1" in
    --apk)       APK="$2"; shift 2 ;;
    --game)      GAME="$2"; shift 2 ;;
    --forbid)    FORBID_EXTRA="$FORBID_EXTRA
$2"; shift 2 ;;
    --forbid-file) FORBID_FILE="$2"; shift 2 ;;
    --expect-vc) EXPECT_VC="$2"; shift 2 ;;
    --expect-vn) EXPECT_VN="$2"; shift 2 ;;
    --list)      LIST=1; shift ;;
    -h|--help)   usage; exit 0 ;;
    *) echo "未知参数: $1"; usage; exit 2 ;;
  esac
done

# 汇总禁用串
NEEDLES="$FORBID_DEFAULT"
if [ -n "$GAME" ] && [ -f "$NIXIANG_ROOT/10_docs/$GAME/出包禁用串.txt" ]; then
  NEEDLES="$NEEDLES
$(grep -v '^#' "$NIXIANG_ROOT/10_docs/$GAME/出包禁用串.txt" | grep -v '^[[:space:]]*$')"
  FORBID_FILE="$NIXIANG_ROOT/10_docs/$GAME/出包禁用串.txt"
fi
if [ -n "$FORBID_FILE" ] && [ -f "$FORBID_FILE" ]; then
  NEEDLES="$NEEDLES
$(grep -v '^#' "$FORBID_FILE" | grep -v '^[[:space:]]*$')"
fi
NEEDLES="$NEEDLES$FORBID_EXTRA"
# 去空行/去重（bash 3.2 无 sort -u 之外的麻烦，直接用 sort -u）
NEEDLES="$(printf '%s\n' "$NEEDLES" | grep -v '^[[:space:]]*$' | sort -u)"

if [ "$LIST" = "1" ]; then
  echo "生效的禁用串清单（内置 + 游戏清单 + --forbid）："
  printf '%s\n' "$NEEDLES" | nl -w2 -s'. '
  [ -n "$FORBID_FILE" ] && echo "（游戏清单文件：${FORBID_FILE}）"
  exit 0
fi

[ -n "$APK" ] || { usage; exit 2; }
[ -f "$APK" ] || { echo "✗ APK 不存在: $APK" >&2; exit 2; }

viol=0
echo "================ 出包门禁 check_release_apk ================"
echo "APK : $APK ($(wc -c < "$APK" | tr -d ' ') B  md5 $(md5 -q "$APK" 2>/dev/null || md5sum "$APK" | cut -d' ' -f1))"

# ---------- 查1/查3 manifest ----------
BT=""
for c in "${ANDROID_HOME:-}" "${ANDROID_SDK_ROOT:-}" "$HOME/Library/Android/sdk" "$HOME/Android/Sdk"; do
  [ -n "$c" ] && [ -d "$c/build-tools" ] || continue
  BT="$c/build-tools/$(ls -1 "$c/build-tools" | sort -V | tail -1)"; break
done
if [ -n "$BT" ] && [ -x "$BT/aapt2" ]; then
  BAD="$("$BT/aapt2" dump badging "$APK" 2>/dev/null || true)"
  echo
  echo "── 查1/查3 manifest ──"
  if printf '%s' "$BAD" | grep -q 'application-debuggable'; then
    echo "  ✗ FAIL 带 application-debuggable（debug 包被当正式包发）"; viol=$((viol+1))
  else
    echo "  ✓ 无 application-debuggable"
  fi
  PKGLINE="$(printf '%s\n' "$BAD" | grep -m1 "^package:")"
  VC="$(printf '%s' "$PKGLINE" | sed -n "s/.*versionCode='\([0-9]*\)'.*/\1/p")"
  VN="$(printf '%s' "$PKGLINE" | sed -n "s/.*versionName='\([^']*\)'.*/\1/p")"
  echo "  · versionCode=$VC versionName=$VN"
  if [ -n "$EXPECT_VC" ] && [ "$VC" != "$EXPECT_VC" ]; then
    echo "  ✗ FAIL versionCode 期望 $EXPECT_VC 实际 ${VC}（改完代码忘 bump？）"; viol=$((viol+1))
  fi
  if [ -n "$EXPECT_VN" ] && [ "$VN" != "$EXPECT_VN" ]; then
    echo "  ✗ FAIL versionName 期望 $EXPECT_VN 实际 $VN"; viol=$((viol+1))
  fi
else
  echo "  ⚠ 无 aapt2（设 ANDROID_HOME），跳过 manifest 检查"
fi

# ---------- 查2 禁用串 ----------
echo
echo "── 查2 禁用串扫描（dex/assets/lib）──"
TMP="$(mktemp -d 2>/dev/null || mktemp -d -t xapk)"
trap 'rm -rf "$TMP"' EXIT
unzip -o -q "$APK" 'classes*.dex' 'assets/*' 'lib/*' -d "$TMP" 2>/dev/null || true
filescnt=0
for f in $(find "$TMP" -type f 2>/dev/null); do filescnt=$((filescnt+1)); done
echo "  · 扫描文件 $filescnt 个"
hit=0
printf '%s\n' "$NEEDLES" | while IFS= read -r n; do
  [ -n "$n" ] || continue
  cnt=$(grep -a -o -F "$n" -r "$TMP" 2>/dev/null | wc -l | tr -d ' ')
  if [ "$cnt" != "0" ]; then
    echo "  ✗ 命中「${n}」×$cnt"
    grep -a -o -F -l "$n" -r "$TMP" 2>/dev/null | sed "s|^$TMP/|       |" | head -5
    echo "$cnt" > "$TMP/.hit"
  fi
done
if [ -f "$TMP/.hit" ]; then
  viol=$((viol+1))
  echo "  ✗ FAIL 禁用串命中（正式包禁止携带调试脚手架）"
else
  echo "  ✓ 无禁用串命中"
fi

echo
if [ "$viol" -gt 0 ]; then
  echo "结论：✗ FAIL $viol 项 ⇒ 不要交付这个包；清掉脚手架后重新出包"
  echo "      （清单可用 --list 查看；游戏专属清单放 10_docs/<游戏>/出包禁用串.txt）"
  exit 1
fi
echo "结论：✓ 干净正式包"
exit 0
