#!/usr/bin/env bash
# 凡尘西游修真记 — dump 阶段后端
# 用法: bash 04_scripts/凡尘西游修真记/dump_fcxyxzj.sh [APK路径]
#
# 产物:
#   03_decompiled/凡尘西游修真记/{jadx_1.1.0,apktool_1.1.0,decode_summary_1.1.0.md}
#   06_dumped/凡尘西游修真记/libil2cpp.so            （静态分析用）
#   06_dumped/凡尘西游修真记/global-metadata.packed.dat （包内混淆版，仅作对照）
#   06_dumped/凡尘西游修真记/global-metadata.runtime.dat（运行时解码版；需先前已 dump 到 /tmp，缺失则只告警）
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=../_common/env.sh
source "$SCRIPT_DIR/../_common/env.sh"

GAME="凡尘西游修真记"
VERSION="1.1.0"
APK="${1:-$NIXIANG_ROOT/02_apks/$GAME/${GAME}_taptap_official.apk}"

if [[ ! -f "$APK" ]]; then
  echo "错误: APK 不存在: $APK" >&2
  exit 1
fi

echo "==> [1/3] jadx + apktool 拆包"
bash "$NIXIANG_ROOT/04_scripts/_common/decode_apk.sh" "$GAME" "$APK" "$VERSION"

DUMP_DIR="$NIXIANG_ROOT/06_dumped/$GAME"
mkdir -p "$DUMP_DIR"

echo "==> [2/3] 提取 IL2CPP 静态分析输入"
unzip -p "$APK" "lib/arm64-v8a/libil2cpp.so" > "$DUMP_DIR/libil2cpp.so"
unzip -p "$APK" "assets/bin/Data/Managed/Metadata/global-metadata.dat" > "$DUMP_DIR/global-metadata.packed.dat"
ls -l "$DUMP_DIR/libil2cpp.so" "$DUMP_DIR/global-metadata.packed.dat"

echo "==> [3/3] 运行时解码元数据（可选对照）"
RUNTIME_META="${FCXYXZJ_RUNTIME_METADATA:-/tmp/fcxyxzj_global_metadata_runtime.dat}"
if [[ -f "$RUNTIME_META" ]]; then
  cp "$RUNTIME_META" "$DUMP_DIR/global-metadata.runtime.dat"
  echo "已复制运行时解码元数据: $DUMP_DIR/global-metadata.runtime.dat"
else
  echo "警告: 未找到运行时解码元数据（${RUNTIME_META}）；包内元数据为混淆版，"
  echo "      静态 dump 前需先在运行中的游戏进程内 dump 解码后的 global-metadata.dat。"
fi

echo
echo "完成。dump 目录: $DUMP_DIR"
