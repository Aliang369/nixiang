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
#    --so-tag <串>      断言 jniLibs 的 libstarry_magic.so 含该标记串（可多次），
#                       用于拦「jniLibs 被别的游戏的 so 污染」⇒ 菜单串成别的游戏
#                       （TOOLBOOK 坑 56；例：--so-tag 邂逅在迷宫 --so-tag 伤害倍率）
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
SO_TAGS=""        # --so-tag 累积（换行分隔；不用数组，兼容 macOS bash 3.2 的 set -u）

usage() {
  sed -n '2,42p' "$0" | sed 's/^# \{0,1\}//'
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
    --so-tag)  [ -n "${2:-}" ] || { echo "--so-tag 需要参数"; usage; exit 2; }
               SO_TAGS="$SO_TAGS$2"$'\n'; shift 2 ;;
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
# versionCode 同时兼容 `versionCode 29`（Groovy 空格语法）与 `versionCode = 29` / `versionCode=29`
VC_RAW="$( { grep -m1 -E '^[[:space:]]*versionCode[[:space:]]*=' "$GRADLE_FILE" || true; grep -m1 -E '^[[:space:]]*versionCode[[:space:]]+[0-9]' "$GRADLE_FILE" || true; } | head -1 )"
VC_OLD="$(printf '%s' "$VC_RAW" | sed 's/.*versionCode[[:space:]]*=[[:space:]]*//; s/.*versionCode[[:space:]]*//' | tr -d ' \r"')"
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
    sed -i.tmp "s/^\([[:space:]]*versionCode[[:space:]]*=\{0,1\}[[:space:]]*\)$VC_OLD/\1$VC_NEW/" "$GRADLE_FILE"
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

# --- native so 身份核对 ------------------------------------------------------
# gradle 不编 native（externalNativeBuild disabled，so 原样取自 jniLibs），
# 而 jniLibs 是跨游戏共用路径 ⇒ 并发会话/上一轮别的游戏的 so 会被原样打进包，
# 出包门禁只看可调试位/禁用串、不看 so 身份（TOOLBOOK 坑 56）。
JNI_SO="$(ls -1 "$PROJECT/$MODULE/src/main/jniLibs/"*/libstarry_magic.so 2>/dev/null | head -1)"
if [ -n "$JNI_SO" ]; then
  step "native so 身份核对"
  SO_ABI="$(basename "$(dirname "$JNI_SO")")"
  SO_SIZE="$(wc -c < "$JNI_SO" | tr -d ' ')"
  SO_MD5="$(md5 -q "$JNI_SO" 2>/dev/null || md5sum "$JNI_SO" | cut -d' ' -f1)"
  SO_MTIME="$(stat -f '%Sm' "$JNI_SO" 2>/dev/null || date -r "$JNI_SO" '+%Y-%m-%d %H:%M' 2>/dev/null || echo '?')"
  echo "   jniLibs : $JNI_SO"
  echo "             ${SO_SIZE} B  md5=${SO_MD5}  mtime=${SO_MTIME}"
  if [ "$DRY" = "1" ]; then
    echo "   [dry-run] 跳过包内 so 比对"
  else
    PKG_SO_MD5="$(unzip -p "$APK_SRC" "lib/${SO_ABI}/libstarry_magic.so" 2>/dev/null | md5 -q 2>/dev/null || true)"
    [ -n "$PKG_SO_MD5" ] || PKG_SO_MD5="$(unzip -p "$APK_SRC" "lib/${SO_ABI}/libstarry_magic.so" 2>/dev/null | md5sum | cut -d' ' -f1)"
    echo "   包内 so : md5=${PKG_SO_MD5:-?}（应等于 jniLibs）"
    [ "$PKG_SO_MD5" = "$SO_MD5" ] || die "包内 so 与 jniLibs 不一致（gradle 打包了别的 so？）" 1
  fi
  # 可选：--so-tag <串> 断言 so 含本游戏的标记串（可多次；中文多字节，用 python 计数）
  if [ -n "$SO_TAGS" ]; then
    while IFS= read -r t; do
      [ -n "$t" ] || continue
      if python3 -c 'import sys;d=open(sys.argv[1],"rb").read();sys.exit(0 if d.count(sys.argv[2].encode())>0 else 1)' "$JNI_SO" "$t"; then
        echo "   so 含标记 ✓ $t"
      else
        die "so 不含本游戏标记「${t}」—— jniLibs 里的 so 可能是别的游戏的（TOOLBOOK 坑 56）" 1
      fi
    done <<< "$SO_TAGS"
  else
    echo "   提示  : 未给 --so-tag，未校验 so 属于哪个游戏；本游戏标记示例：--so-tag 邂逅在迷宫 --so-tag 伤害倍率"
  fi
fi

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
