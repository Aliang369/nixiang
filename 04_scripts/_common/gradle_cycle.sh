#!/usr/bin/env bash
# ================================================================
#  gradle_cycle.sh — 助手类（gradle 工程）一键出包链路（P5）
#
#  背景（2026-10-10 邂逅在迷宫会话复盘 P5）：
#  本会话同一套"改代码 → versionCode+1 → assembleRelease → zipalign → apksigner
#  → verify → install → 拉服务自测 → 抓日志"手工敲了 **9 遍**（v2.1→v2.9），
#  每遍 6~10 条命令，其中 3 次因忘 bump versionCode / 忘 verify / 签名参数写错
#  而返工。全是机器可拦的错误 ⇒ 收成一条命令。
#
#  用法:
#    bash 04_scripts/_common/gradle_cycle.sh [选项]
#  常用:
#    bash 04_scripts/_common/gradle_cycle.sh --tag v2.10 --bump-version --install
#    bash 04_scripts/_common/gradle_cycle.sh --tag v2.10 --bump-version \
#        --install --arm-service com.starry.magic/.menu.net.CdnVpnService \
#        --watch 6 --logs '注入完成|load endless dll'
#
#  选项:
#    --project <目录>   gradle 工程根（默认 mod菜单/starry-mod-menu）
#    --module <名>      module 名（默认 app）
#    --tag <标签>       版本标签，同时作为输出文件名后缀（默认沿用当前 versionName）
#    --bump-version     把 versionCode+1；给了 --tag 则 versionName 一并改成 tag
#    --ks <jks>         签名库（默认 01_tools/starry_release.jks）
#    --alias <别名>     默认 starry
#    --ks-pass <pass>   默认 starry2026（也可传 pass:xxx / file:xxx 交给 apksigner）
#    --out <apk>        签名产物落点（默认 /tmp/<工程名>_build/<AppName>_<tag>_signed.apk）
#    --serial <S>       指定 adb 设备
#    --install          装机（adb install -r）
#    --arm-service <组件> 装机后用 root 拉起前台服务做自测（**仅诊断**；正式流程仍需用户点按钮）
#    --watch <秒>       arm-service 后等待秒数（默认 5）
#    --logs <ERE>       logcat 过滤正则（配合 --watch 打印，形如 '注入完成|load endless dll'）
#    --no-gate          跳过 check_release_apk.sh 出包门禁（默认跑）
#    --dry-run          只打印将要执行的步骤（不构建、不装机）
#
#  退出码: 0=全链通过；1=构建/签名/门禁失败；2=用法或环境错；3=装机失败
#  铁律: 出包门禁（P6）不许默认跳过 —— 干净正式包是交付底线（见
#        10_docs/_templates/经验候选卡_诊断脚手架隔离与干净正式包_20260930.md）。
# ================================================================
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=env.sh
source "$SCRIPT_DIR/env.sh"

PACKAGE_NAME="com.starry.magic"   # 默认助手包名（输出文件名用）
PROJECT="mod菜单/starry-mod-menu"
MODULE="app"
TAG=""; BUMP=0
KS="$NIXIANG_ROOT/01_tools/starry_release.jks"
KS_ALIAS="starry"
KS_PASS="starry2026"
OUT=""
SERIAL=""
DO_INSTALL=0
ARM_SERVICE=""
WATCH=5
LOGS=""
GATE=1
DRY=0

usage() {
  sed -n '2,40p' "$0" | sed 's/^# \{0,1\}//'
}

while [ $# -gt 0 ]; do
  case "$1" in
    --project) PROJECT="$2"; shift 2 ;;
    --module)  MODULE="$2"; shift 2 ;;
    --tag)     TAG="$2"; shift 2 ;;
    --bump-version) BUMP=1; shift ;;
    --ks)      KS="$2"; shift 2 ;;
    --alias)   KS_ALIAS="$2"; shift 2 ;;
    --ks-pass) KS_PASS="$2"; shift 2 ;;
    --out)     OUT="$2"; shift 2 ;;
    --serial)  SERIAL="$2"; shift 2 ;;
    --install) DO_INSTALL=1; shift ;;
    --arm-service) ARM_SERVICE="$2"; shift 2 ;;
    --watch)   WATCH="$2"; shift 2 ;;
    --logs)    LOGS="$2"; shift 2 ;;
    --no-gate) GATE=0; shift ;;
    --dry-run) DRY=1; shift ;;
    -h|--help) usage; exit 0 ;;
    *) echo "未知参数: $1"; usage; exit 2 ;;
  esac
done

step() { echo; echo "==> $*"; }
die()  { echo "✗ $*" >&2; exit "${2:-2}"; }
run()  { if [ "$DRY" = "1" ]; then echo "   [dry-run] $*"; else eval "$@"; fi; }

case "$PROJECT" in /*) : ;; *) PROJECT="$NIXIANG_ROOT/$PROJECT" ;; esac
[ -d "$PROJECT" ] || die "gradle 工程不存在: $PROJECT"
[ -f "$PROJECT/$MODULE/build.gradle" ] || die "找不到 $PROJECT/$MODULE/build.gradle"
[ -f "$PROJECT/gradlew" ] || die "找不到 gradlew（不是 gradle 工程？）: $PROJECT/gradlew"

GRADLE_FILE="$PROJECT/$MODULE/build.gradle"
VC_OLD="$(grep -m1 -E '^[[:space:]]*versionCode[[:space:]]' "$GRADLE_FILE" | tr -d ' \r' | cut -d= -f2)"
VN_OLD="$(grep -m1 -E '^[[:space:]]*versionName[[:space:]]' "$GRADLE_FILE" | sed 's/.*versionName[[:space:]]*//' | tr -d ' \r"')"
[ -n "$VC_OLD" ] || die "读不到 versionCode（${GRADLE_FILE}）"

echo "================ gradle_cycle ================"
echo "工程    : $PROJECT   module=$MODULE"
echo "版本    : versionCode=$VC_OLD versionName=${VN_OLD:-?}"
echo "签名库  : $KS (alias=$KS_ALIAS)"

if [ "$BUMP" = "1" ]; then
  VC_NEW=$((VC_OLD + 1))
  step "bump versionCode $VC_OLD → $VC_NEW${TAG:+ ，versionName → $TAG}"
  if [ "$DRY" = "1" ]; then
    echo "   [dry-run] 改写 $GRADLE_FILE"
  else
    cp "$GRADLE_FILE" "$GRADLE_FILE.bak"
    sed -i.tmp "s/^\([[:space:]]*versionCode[[:space:]]*\)$VC_OLD/\1$VC_NEW/" "$GRADLE_FILE"
    if [ -n "$TAG" ]; then
      sed -i.tmp "s/^\([[:space:]]*versionName[[:space:]]*\).*/\1\"$TAG\"/" "$GRADLE_FILE"
    fi
    rm -f "$GRADLE_FILE.tmp"
    grep -m1 -E 'versionCode|versionName' "$GRADLE_FILE" | sed 's/^/   /'
  fi
else
  VC_NEW="$VC_OLD"
fi

# JAVA_HOME（JDK 17）
if [ -z "${JAVA_HOME:-}" ] && command -v /usr/libexec/java_home >/dev/null 2>&1; then
  export JAVA_HOME="$(/usr/libexec/java_home -v 17 2>/dev/null || true)"
fi
[ -n "${JAVA_HOME:-}" ] || die "JAVA_HOME 未设置且找不到 JDK 17"

step "assembleRelease"
( cd "$PROJECT" && JAVA_HOME="$JAVA_HOME" ./gradlew ":$MODULE:assembleRelease" --console=plain -q ) || die "gradle 构建失败" 1
APK_SRC="$PROJECT/$MODULE/build/outputs/apk/release/$MODULE-release.apk"
[ -f "$APK_SRC" ] || APK_SRC="$(ls -1 "$PROJECT/$MODULE/build/outputs/apk/release/"*.apk 2>/dev/null | head -1)"
[ -n "$APK_SRC" ] && [ -f "$APK_SRC" ] || die "找不到构建产物 apk" 1
echo "   产物: $APK_SRC ($(wc -c < "$APK_SRC" | tr -d ' ') B)"

# build-tools
BT=""
for c in "${ANDROID_HOME:-}" "${ANDROID_SDK_ROOT:-}" "$HOME/Library/Android/sdk" "$HOME/Android/Sdk"; do
  [ -n "$c" ] || continue
  [ -d "$c/build-tools" ] || continue
  BT="$(ls -1 "$c/build-tools" 2>/dev/null | sort -V | tail -1)"
  BT="$c/build-tools/$BT"
  break
done
[ -n "$BT" ] && [ -x "$BT/zipalign" ] || die "找不到 build-tools（设 ANDROID_HOME 或 ANDROID_SDK_ROOT）"
echo "   build-tools: $BT"

WORK="/tmp/$(basename "$PROJECT")_build"; mkdir -p "$WORK"
ALIGNED="$WORK/aligned.apk"
[ -n "$OUT" ] || OUT="$WORK/$(basename "$PROJECT")_${TAG:-$VN_OLD}_signed.apk"

step "zipalign"
run "\"$BT/zipalign\" -f -p 4 \"$APK_SRC\" \"$ALIGNED\"" || die "zipalign 失败" 1

step "apksigner 签名"
[ -f "$KS" ] || die "签名库不存在: $KS"
run "\"$BT/apksigner\" sign --ks \"$KS\" --ks-key-alias \"$KS_ALIAS\" --ks-pass pass:$KS_PASS --key-pass pass:$KS_PASS --v1-signing-enabled true --v2-signing-enabled true --v3-signing-enabled true --out \"$OUT\" \"$ALIGNED\"" || die "签名失败" 1

step "verify（签名 + 证书）"
[ "$DRY" = "1" ] || {
  "$BT/apksigner" verify --print-certs "$OUT" | sed 's/^/   /' || die "verify 失败" 1
  echo "   产物: $OUT ($(wc -c < "$OUT" | tr -d ' ') B  md5 $(md5 -q "$OUT" 2>/dev/null || md5sum "$OUT" | cut -d' ' -f1))"
}

# ---------- P6 出包门禁 ----------
if [ "$GATE" = "1" ] && [ "$DRY" != "1" ]; then
  if [ -x "$SCRIPT_DIR/check_release_apk.sh" ] || [ -f "$SCRIPT_DIR/check_release_apk.sh" ]; then
    step "出包门禁 check_release_apk.sh"
    if ! bash "$SCRIPT_DIR/check_release_apk.sh" --apk "$OUT"; then
      die "出包门禁 FAIL ⇒ 不要交付这个包（--no-gate 才能跳过，需写明理由）" 1
    fi
  else
    echo "   ⚠ check_release_apk.sh 不存在，跳过出包门禁"
  fi
fi

# ---------- 装机 ----------
ADB="${ADB:-adb}"
if [ "$DO_INSTALL" = "1" ]; then
  step "install -r"
  ADBX="$ADB"; [ -n "$SERIAL" ] && ADBX="$ADB -s $SERIAL"
  run "$ADBX install -r \"$OUT\"" || die "装机失败" 3
fi

# ---------- 诊断自测（可选） ----------
if [ -n "$ARM_SERVICE" ]; then
  step "root 拉起服务（诊断用）: $ARM_SERVICE"
  ADBX="$ADB"; [ -n "$SERIAL" ] && ADBX="$ADB -s $SERIAL"
  run "$ADBX shell \"su -c 'am start-foreground-service -n $ARM_SERVICE -a com.starry.magic.action.CDN_START'\"" || echo "   ⚠ 拉起失败（VPN 授权缺失？正式流程需用户点按钮）"
  if [ -n "$LOGS" ] && [ "$DRY" != "1" ]; then
    step "等待 ${WATCH}s 后抓日志: $LOGS"
    sleep "$WATCH"
    ADBX="$ADB"; [ -n "$SERIAL" ] && ADBX="$ADB -s $SERIAL"
    $ADBX logcat -d 2>/dev/null | grep -E "$LOGS" | tail -30 | sed 's/^/   /'
  fi
fi

echo
echo "================ 完成 ================"
echo "APK      : $OUT"
echo "versionCode=$VC_NEW versionName=${TAG:-$VN_OLD}"
echo "记账提醒：过门禁当轮记一行 —— bash 04_scripts/_common/gate_note.sh <游戏> G2 通过 \"gradle_cycle 出包+verify 通过\""
exit 0
