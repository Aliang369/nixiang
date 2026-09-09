#!/usr/bin/env bash
# 从 apktool 解出的 AndroidManifest.xml 抽包名/权限/组件
# 用法: ./manifest_summary.sh <AndroidManifest.xml>
set -euo pipefail

[[ $# -ge 1 ]] || { echo "用法: $0 <AndroidManifest.xml>"; exit 1; }
MF="$1"
[[ -f "$MF" ]] || { echo "文件不存在: $MF" >&2; exit 1; }

PKG="$(grep -oE 'package="[^"]+"' "$MF" | head -1 | sed 's/package="//;s/"//')" || true
echo "package: ${PKG:-?}"
echo

echo "== permissions =="
grep -oE 'android:name="android\.permission\.[^"]+"' "$MF" 2>/dev/null \
  | sed 's/android:name="//;s/"//' | sort -u | sed 's/^/  /' || echo "  (none)"
echo

extract_components() {
  local tag="$1"
  # rough: lines with <tag ... android:name="...
  grep -E "<${tag}[[:space:]]" "$MF" 2>/dev/null \
    | grep -oE 'android:name="[^"]+"' \
    | sed 's/android:name="//;s/"//' \
    | sort -u \
    | sed 's/^/  /' || true
}

echo "== activities =="
ACTS="$(extract_components activity)"
[[ -n "$ACTS" ]] && echo "$ACTS" || echo "  (none)"
echo

echo "== services =="
SVCS="$(extract_components service)"
[[ -n "$SVCS" ]] && echo "$SVCS" || echo "  (none)"
echo

echo "== receivers =="
RCV="$(extract_components receiver)"
[[ -n "$RCV" ]] && echo "$RCV" || echo "  (none)"
echo

echo "== providers =="
PROV="$(extract_components provider)"
[[ -n "$PROV" ]] && echo "$PROV" || echo "  (none)"
echo

echo "== launcher (MAIN + LAUNCHER) =="
# best-effort: activity-alias or activity near MAIN
LAUNCH="$(grep -B8 'android.intent.action.MAIN' "$MF" | grep -oE 'android:name="[^"]+"' | head -1 | sed 's/android:name="//;s/"//')" || true
echo "  ${LAUNCH:-?}"
