#!/usr/bin/env bash
# check_win_compat.sh — 扫描脚本里未豁免的 mac 专有写法（win 上会崩/静默降级）
#
# 豁免（不算命中，已由双机机制兜底）：
#   /Users/aliang/Documents/nixiang   -> junction 垫片
#   md5 -q / stat -f / shasum -a      -> ~/.nx_shims.sh 垫片
# 检测（命中即 win 不兼容，改到再修）：
#   \$HOME/Library/Android/sdk        写死 mac SDK 路径（改用 NIXIANG_BUILD_TOOLS / NIXIANG_NDK_HOME）
#   /usr/libexec/java_home            mac 专有 JDK 探测（改用 NIXIANG_JAVA17）
#   /opt/homebrew                     mac brew 路径
#   darwin-x86_64                     mac NDK prebuilt（win 要 windows-x86_64）
#   sed -i ''                         BSD sed 空后缀（GNU 报 rc=2）
#
# 用法：--all 全仓 / <文件...> 指定文件；退出码 0=干净 1=有命中
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
[ -n "$NIXIANG_ROOT" ] && ROOT="$NIXIANG_ROOT"

if [ "$1" = "--all" ]; then
  shift
  FILES="$(ls "$ROOT"/04_scripts/*/*.sh "$ROOT"/04_scripts/_common/*.sh "$ROOT"/mod菜单/*/scripts/*.sh 2>/dev/null | grep -v "check_win_compat.sh" | sort -u || true)"
else
  FILES="$*"
fi

HITS="$( { grep -n -E '\$HOME/Library/Android/sdk|/usr/libexec/java_home|/opt/homebrew|darwin-x86_64' $FILES 2>/dev/null || true; grep -n -F "sed -i ''" $FILES 2>/dev/null || true; } | sed "s#^$ROOT/##" )"

if [ -n "$HITS" ]; then
  printf '%s\n' "$HITS"
  echo "win-compat-hits=$(printf '%s\n' "$HITS" | wc -l | tr -d ' ')"
  exit 1
fi
echo "win-compat-hits=0"
exit 0
