#!/usr/bin/env bash
# 明日方舟：锚点核验（版本无关）
#
# 背景：官方包 assets/bin/Data/Managed/Metadata/global-metadata.dat 是【弱混淆】
#   （首 8 字节固定标记 + 每 64 KiB 内 16 KiB 窗口异或 0x66），静态修复即可解析：
#     bash 04_scripts/明日方舟/metadata_fix_mrfzz.py --apk-from 04_scripts/明日方舟/official_apk.json
#   产出的 06_dumped/明日方舟/official/static_meta/global-metadata.fixed.dat 是本脚本
#   默认首选的输入；运行时内存导出（dump_official_runtime_mrfzz.sh）降级为兜底，
#   仅在静态修复自检不自洽时才用。解析器为自研 anchor_check_mrfzz.py，逐条核验
#   ArknightsMod.cpp 依赖的 11 个 (命名空间, 类, 嵌套类, 方法, 参数个数) 锚点。
#   注：01_tools/Il2CppDumper 吃本游戏运行时 metadata 会抛
#   KeyNotFoundException('kIl2CppMetadataUsageInvalid')，不可用
#   （见 10_docs/明日方舟/台账.md「已否证」区）。
#
# 用法: bash 04_scripts/明日方舟/anchor_check_mrfzz.sh
# 退出码: 0=11 条全通过；3=有缺失或 argc 不一致；2=输入错误
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"

# 解释器自适配：显式 PYTHON_BIN > python3 > python（Windows 端 python3 指向商店别名时用 python）
PYTHON_BIN="${PYTHON_BIN:-}"
if [[ -z "$PYTHON_BIN" ]]; then
  for cand in python3 python; do
    if command -v "$cand" >/dev/null 2>&1; then PYTHON_BIN="$(command -v "$cand")"; break; fi
  done
fi
[[ -n "$PYTHON_BIN" ]] || { echo "错误: 未找到 python 解释器（可用 PYTHON_BIN 指定）" >&2; exit 2; }

META_FILE="$SCRIPT_DIR/official_apk.json"
[[ -f "$META_FILE" ]] || { echo "错误: 缺 ${META_FILE}，先跑 fetch_official_mrfzz.sh" >&2; exit 2; }

VERSION="$("$PYTHON_BIN" -c 'import json,sys;print(json.load(open(sys.argv[1]))["version_name"])' "$META_FILE")"
VERSION_CODE="$("$PYTHON_BIN" -c 'import json,sys;print(json.load(open(sys.argv[1]))["version_code"])' "$META_FILE")"
SERIAL="$("$PYTHON_BIN" -c 'import json,sys;print(json.load(open(sys.argv[1])).get("serial",""))' "$META_FILE")"

STATIC_DIR="$ROOT/06_dumped/明日方舟/official/static_meta"
RUNTIME_DIR="$ROOT/06_dumped/明日方舟/official/runtime_current"
# MRFZZ_METADATA：显式指定 metadata 文件，优先于以下默认顺位
# 默认顺位：①静态修复产物（首选） ②运行时导出（兜底）
METADATA="${MRFZZ_METADATA:-}"
if [[ -z "$METADATA" ]]; then
  METADATA="$STATIC_DIR/global-metadata.fixed.dat"
  [[ -f "$METADATA" ]] || METADATA="$RUNTIME_DIR/global-metadata.runtime.magicfixed.dat"
  [[ -f "$METADATA" ]] || METADATA="$RUNTIME_DIR/global-metadata.runtime.dat"
fi
[[ -f "$METADATA" ]] || { echo "错误: 缺 metadata（${METADATA}）：先跑 metadata_fix_mrfzz.py 做静态修复（首选），或 dump_official_runtime_mrfzz.sh 兜底，或设 MRFZZ_METADATA" >&2; exit 2; }

SPEC="$SCRIPT_DIR/anchors_mrfzz.json"
CPP="$ROOT/mod菜单/starry-mod-menu/app/src/main/jni/Games/ArknightsMod.cpp"
OUT_JSON="$ROOT/10_docs/明日方舟/抓点_锚点_${VERSION}.json"
OUT_MD="$ROOT/10_docs/明日方舟/锚点核验_${VERSION}.md"

DUMP_CS="${MRFZZ_DUMP_CS:-}"
DUMP_ARGS=()
if [[ -n "$DUMP_CS" ]]; then
  [[ -f "$DUMP_CS" ]] || { echo "错误: MRFZZ_DUMP_CS 指向的文件不存在: $DUMP_CS" >&2; exit 2; }
  DUMP_ARGS=(--dump-cs "$DUMP_CS")
fi

printf '[meta] version=%s vc=%s serial=%s\n' "$VERSION" "$VERSION_CODE" "$SERIAL"
printf '[meta] metadata=%s\n' "$METADATA"
printf '[meta] spec=%s\n' "$SPEC"

set +e
"$PYTHON_BIN" "$SCRIPT_DIR/anchor_check_mrfzz.py" \
  --metadata "$METADATA" \
  ${DUMP_ARGS[@]+"${DUMP_ARGS[@]}"} \
  --cpp "$CPP" \
  --spec "$SPEC" \
  --out-json "$OUT_JSON" \
  --out-md "$OUT_MD" \
  --version-name "$VERSION" \
  --version-code "$VERSION_CODE" \
  --serial "$SERIAL"
rc=$?
set -e

case "$rc" in
  0) echo "[anchor] 全部锚点在 ${VERSION} 核验通过（详见 ${OUT_MD}）" ;;
  3) echo "[anchor] 有锚点缺失或参数个数不一致 —— 禁止继续装机，先改锚点表" >&2 ;;
  *) echo "[anchor] 核验器输入错误（rc=${rc}）" >&2 ;;
esac
exit "$rc"
