#!/usr/bin/env bash
# ================================================================
#  menu_tpl_gate.sh — 菜单模板/dex 新鲜度门禁（防「用旧菜单 dex 出包」）
#
#  背景（2026-10-26 无限升级 vc1637 事故，两天内同源第三次）：
#    出包脚本是「模板优先，缺了才现编」——只要 templates/<name>/classesN.dex 还在，
#    菜单 dex 就永远不会重编。而 wxsj 线模板停在 2026-09-29，早于 2026-10-07 的
#    `ModBridge` 无条件清功能偏好修复 ⇒ 用户点开的开关重启即被抹掉
#    （用户报障「重启后发现还是默认全关」）。
#    同类既有事故：镇邪人 7f94233e / a3748024（旧 dex 早于同一修复）。
#
#  为什么之前的门禁拦不住：
#    · pre_install_gate 查2 只查「so 注册的 native 名在 dex 里有没有声明」——旧 dex
#      只要还声明同名 native 就 PASS（且没传对应 --cpp 时整项 SKIP）；
#    · so 有 STARRYSTAMP1:EPOCH= 构建戳，dex 侧过去没有任何戳，无从比新旧。
#
#  本脚本的判据（两条，都硬）：
#    ① 模板目录必须有 build_info.txt，且其中 source_commit == 当前 HEAD 里最近一次
#       改动 app/src/main/java 的提交（前 12 位）——不同即「模板落后于源码」。
#    ② dex 内构建戳 `starry-menu:<commit12>`（由 wxsj 两条刷新链注入到 BuildConfig.smali）：
#       存在则必须与①同 commit；不存在时按 WARN 放行（javac+d8 那批游戏的 dex 里没有戳），
#       只有加 `--require-stamp` 才强制要求 —— 防「build_info 被手改/复制但 dex 没重编」。
#
#  用法：
#    bash 04_scripts/_common/menu_tpl_gate.sh --tpl <模板目录> [--dex-name classes4.dex]
#    bash 04_scripts/_common/menu_tpl_gate.sh --apk <成品APK>  --dex-name classes4.dex
#    ... [--require-stamp]   # 强制要求 dex 内有构建戳（wxsj 两条线用）
#    ... [--allow-stale]     # 仅用于复现历史产物/取证，必须在汇报里写明
#    ... [--quiet]
#  退出码：0=通过；1=过期/缺戳（不得出包）；2=用法错；3=环境缺 git/unzip
# ================================================================
set -euo pipefail

ROOT="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)}"
JAVA_SRC_REL="mod菜单/starry-mod-menu/app/src/main/java"

TPL=""
APK=""
DEX_NAME=""
ALLOW_STALE=0
QUIET=0
REQUIRE_STAMP=0

usage() {
  sed -n '28,36p' "$0" | sed 's/^# \{0,1\}//'
  exit 2
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --tpl)        [[ $# -ge 2 ]] || usage; TPL="$2"; shift 2 ;;
    --apk)        [[ $# -ge 2 ]] || usage; APK="$2"; shift 2 ;;
    --dex-name)   [[ $# -ge 2 ]] || usage; DEX_NAME="$2"; shift 2 ;;
    --require-stamp) REQUIRE_STAMP=1; shift ;;
    --allow-stale) ALLOW_STALE=1; shift ;;
    --quiet)      QUIET=1; shift ;;
    -h|--help)    usage ;;
    *) echo "ERROR: 未知参数 $1" >&2; usage ;;
  esac
done

[[ -n "$TPL" || -n "$APK" ]] || usage
command -v git >/dev/null 2>&1 || { echo "ERROR: 缺 git" >&2; exit 3; }

say() { [[ "$QUIET" == "1" ]] || echo "$@"; }
fail() {
  echo "✗ menu_tpl_gate: $*" >&2
  exit 1
}

# ---- 当前源码身份 ----
HEAD_COMMIT="$(git -C "$ROOT" log -1 --format=%H -- "$JAVA_SRC_REL" 2>/dev/null || true)"
if [[ -z "$HEAD_COMMIT" ]]; then
  fail "取不到 $JAVA_SRC_REL 的 git HEAD（仓库/路径不对？）"
fi
HEAD12="${HEAD_COMMIT:0:12}"
say "  源码身份: java 树最近提交 ${HEAD12}"

# ---- 取 dex 临时文件 + 需要检查的 build_info ----
TMPD="$(mktemp -d "${TMPDIR:-/tmp}/menu_tpl_gate.XXXXXX")"
trap 'rm -rf "$TMPD"' EXIT
DEX_FILE=""
INFO_FILE=""

if [[ -n "$TPL" ]]; then
  [[ -d "$TPL" ]] || fail "模板目录不存在: $TPL"
  if [[ -z "$DEX_NAME" ]]; then
    DEX_NAME="$(ls "$TPL" 2>/dev/null | grep -E '^classes[0-9]*\.dex$' | head -1 || true)"
    [[ -n "$DEX_NAME" ]] || fail "模板目录内没有 classesN.dex: $TPL"
  fi
  [[ -s "$TPL/$DEX_NAME" ]] || fail "模板缺 dex: $TPL/$DEX_NAME"
  DEX_FILE="$TPL/$DEX_NAME"
  INFO_FILE="$TPL/build_info.txt"
  [[ -f "$INFO_FILE" ]] || fail "模板缺 build_info.txt（无法证明新鲜度）: $TPL
    ⇒ 先跑: bash mod菜单/starry-mod-menu/scripts/wxsj_refresh_menu_tpl.sh"
else
  [[ -f "$APK" ]] || fail "APK 不存在: $APK"
  if [[ -z "$DEX_NAME" ]]; then
    DEX_NAME="$(unzip -l "$APK" 2>/dev/null | awk '{print $4}' | grep -E '^classes[0-9]*\.dex$' | tail -1 || true)"
    [[ -n "$DEX_NAME" ]] || fail "APK 内没有 dex: $APK"
  fi
  unzip -p "$APK" "$DEX_NAME" > "$TMPD/target.dex" 2>/dev/null \
    || fail "APK 内取不到 $DEX_NAME: $APK"
  DEX_FILE="$TMPD/target.dex"
  say "  目标: $APK :: $DEX_NAME"
fi

# ---- ① build_info 的 source_commit ----
if [[ -n "$INFO_FILE" ]]; then
  INFO_COMMIT="$(grep -E '^source_commit' "$INFO_FILE" 2>/dev/null | head -1 | awk '{print $NF}' || true)"
  if [[ -z "$INFO_COMMIT" ]]; then
    fail "build_info.txt 缺 source_commit（模板未纳入新鲜度机制）: $INFO_FILE
    ⇒ 用该游戏自己的构建脚本重产模板（04_scripts/<游戏>/<tag>_build_dex.sh，已内置该字段）
      或 wxsj 两条线: bash mod菜单/starry-mod-menu/scripts/wxsj_refresh_menu_tpl.sh"
  fi
  INFO12="${INFO_COMMIT:0:12}"
  say "  build_info: source_commit ${INFO12}"
  if [[ "$INFO12" != "$HEAD12" ]]; then
    if [[ "$ALLOW_STALE" == "1" ]]; then
      echo "  ⚠ --allow-stale: 模板 ${INFO12} != 源码 ${HEAD12}（仅在复现历史产物时允许）" >&2
    else
      fail "菜单模板落后于源码：build_info=${INFO12} / HEAD=${HEAD12}
    ⇒ 重新生成: bash mod菜单/starry-mod-menu/scripts/wxsj_refresh_menu_tpl.sh
      （改过 app/src/main/java 就必须重编，否则出的是旧菜单 dex）"
    fi
  fi
fi

# ---- ② dex 内构建戳 ----
STAMP_LINE="$(LC_ALL=C grep -aoE 'starry-menu:[0-9a-f]{7,40}' "$DEX_FILE" 2>/dev/null | head -1 || true)"
if [[ -z "$STAMP_LINE" ]]; then
  if [[ "$ALLOW_STALE" == "1" || "$REQUIRE_STAMP" == "0" ]]; then
    echo "  ⚠ dex 内无构建戳（javac+d8 管线不带戳；本次以 build_info.source_commit 为准）" >&2
  else
    fail "dex 内没有构建戳 starry-menu:<commit>（无法证明与源码同步）: ${DEX_FILE}
    ⇒ 用刷新脚本重编（它会注入戳）: bash mod菜单/starry-mod-menu/scripts/wxsj_refresh_menu_tpl.sh"
  fi
else
  STAMP_COMMIT="${STAMP_LINE#starry-menu:}"
  STAMP12="${STAMP_COMMIT:0:12}"
  say "  dex 构建戳: ${STAMP12}"
  if [[ "$STAMP12" != "$HEAD12" ]]; then
    if [[ "$ALLOW_STALE" == "1" ]]; then
      echo "  ⚠ --allow-stale: dex 戳 ${STAMP12} != 源码 ${HEAD12}" >&2
    else
      fail "dex 构建戳与源码不符：dex=${STAMP12} / HEAD=${HEAD12}
    ⇒ 重编菜单 dex（模板里的 dex 是旧的，改了 Java 没重编）"
    fi
  fi
fi

say "✓ menu_tpl_gate: dex 与源码同步（${HEAD12}）"
exit 0
