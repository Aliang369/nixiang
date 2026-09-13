#!/usr/bin/env bash
# 一键 jadx + apktool 拆包，按游戏名落到 03_decompiled/
# 用法:
#   ./decode_apk.sh <游戏名> <apk路径> [版本号]
# 示例:
#   ./decode_apk.sh 战魂铭人 ../../02_apks/战魂铭人/战魂铭人_3.4.0.apk 3.4.0
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=env.sh
source "$SCRIPT_DIR/env.sh"

usage() {
  echo "用法: $0 <游戏名> <apk路径> [版本号]"
  echo "  版本号可省略：从文件名里取最后一个 _ 后的片段（去 .apk）"
  exit 1
}

[[ $# -ge 2 ]] || usage
GAME="$1"
APK="$2"
VERSION="${3:-}"

if [[ ! -f "$APK" ]]; then
  echo "错误: APK 不存在: $APK" >&2
  exit 1
fi
APK="$(cd "$(dirname "$APK")" && pwd)/$(basename "$APK")"

if [[ -z "$VERSION" ]]; then
  base="$(basename "$APK" .apk)"
  VERSION="${base##*_}"
  [[ -n "$VERSION" && "$VERSION" != "$base" ]] || VERSION="unknown"
fi

need() { command -v "$1" >/dev/null || { echo "缺少工具: $1"; exit 1; }; }
need jadx
need apktool

OUT_ROOT="$NIXIANG_ROOT/03_decompiled/$GAME"
JADX_OUT="$OUT_ROOT/jadx_$VERSION"
APKTOOL_OUT="$OUT_ROOT/apktool_$VERSION"
SUMMARY="$OUT_ROOT/decode_summary_$VERSION.md"

mkdir -p "$OUT_ROOT"
rm -rf "$JADX_OUT" "$APKTOOL_OUT"
mkdir -p "$JADX_OUT" "$APKTOOL_OUT"

echo "==> jadx → $JADX_OUT"
set +e
jadx -d "$JADX_OUT" "$APK"
JADX_RC=$?
set -e
if [[ $JADX_RC -ne 0 ]]; then
  echo "警告: jadx 退出码 ${JADX_RC}（部分类失败时仍可能有可用产物）" >&2
fi

echo "==> apktool → $APKTOOL_OUT"
apktool d -f "$APK" -o "$APKTOOL_OUT"

# 摘要
PKG=""
LAUNCH=""
if [[ -f "$APKTOOL_OUT/AndroidManifest.xml" ]]; then
  PKG="$(grep -oE 'package="[^"]+"' "$APKTOOL_OUT/AndroidManifest.xml" | head -1 | sed 's/package="//;s/"//')" || true
  LAUNCH="$(grep -B5 'android.intent.action.MAIN' "$APKTOOL_OUT/AndroidManifest.xml" | grep -oE 'android:name="[^"]+"' | head -1 | sed 's/android:name="//;s/"//')" || true
fi

JAVA_COUNT="$(find "$JADX_OUT" -name '*.java' 2>/dev/null | wc -l | tr -d ' ')"
SMALI_DIRS="$(find "$APKTOOL_OUT" -maxdepth 1 -type d -name 'smali*' 2>/dev/null | wc -l | tr -d ' ')"
SO_LIST="$(find "$APKTOOL_OUT/lib" -name '*.so' 2>/dev/null | sed "s|$APKTOOL_OUT/||" | sort || true)"
SO_COUNT="$(printf '%s\n' "$SO_LIST" | grep -c '\.so$' || true)"
HAS_IL2CPP="no"
HAS_METADATA="no"
printf '%s\n' "$SO_LIST" | grep -q 'libil2cpp.so' && HAS_IL2CPP="yes"
[[ -f "$APKTOOL_OUT/assets/bin/Data/Managed/Metadata/global-metadata.dat" ]] && HAS_METADATA="yes"

{
  echo "# 拆包摘要 — $GAME $VERSION"
  echo
  echo "- 时间: $(date '+%Y-%m-%d %H:%M:%S')"
  echo "- APK: \`$APK\`"
  echo "- jadx: \`$JADX_OUT\` (exit=$JADX_RC, java≈$JAVA_COUNT)"
  echo "- apktool: \`$APKTOOL_OUT\` (smali_dirs=$SMALI_DIRS)"
  echo "- package: \`$PKG\`"
  echo "- launcher activity: \`$LAUNCH\`"
  echo "- .so 数量: $SO_COUNT"
  echo "- Unity IL2CPP: libil2cpp=$HAS_IL2CPP / global-metadata=$HAS_METADATA"
  echo
  echo "## .so 列表"
  if [[ -n "$SO_LIST" ]]; then
    printf '%s\n' "$SO_LIST" | sed 's/^/- `/' | sed 's/$/`/'
  else
    echo "- （无）"
  fi
  echo
  echo "## 下一步检查点"
  echo "- [ ] 读 AndroidManifest（权限/组件/exported）"
  echo "- [ ] 判断壳 / 加固"
  echo "- [ ] 有 .so → IDA + FindCrypt；Unity → Il2CppDumper"
  echo "- [ ] 笔记写到 \`10_docs/$GAME/\`"
} > "$SUMMARY"

# 可选：跑 manifest 摘要
if [[ -x "$SCRIPT_DIR/manifest_summary.sh" && -f "$APKTOOL_OUT/AndroidManifest.xml" ]]; then
  "$SCRIPT_DIR/manifest_summary.sh" "$APKTOOL_OUT/AndroidManifest.xml" > "$OUT_ROOT/manifest_$VERSION.txt" || true
fi

echo
echo "完成。"
echo "  jadx:    $JADX_OUT"
echo "  apktool: $APKTOOL_OUT"
echo "  摘要:    $SUMMARY"
[[ -f "$OUT_ROOT/manifest_$VERSION.txt" ]] && echo "  manifest:$OUT_ROOT/manifest_$VERSION.txt"
