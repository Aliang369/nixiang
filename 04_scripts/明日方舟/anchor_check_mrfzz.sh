#!/usr/bin/env bash
# 明日方舟：锚点核验（版本无关）
#
# 背景：官方包 assets/bin/Data/Managed/Metadata/global-metadata.dat 静态加密
#   （首 4 字节非 af1bb1fa），必须先由 dump_official_runtime_mrfzz.sh 从运行时
#   内存导出明文副本；本脚本用自研解析器 anchor_check_mrfzz.py 逐条核验
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

PYTHON_BIN="${PYTHON_BIN:-/opt/homebrew/bin/python3.11}"
[[ -x "$PYTHON_BIN" ]] || PYTHON_BIN="$(command -v python3)"

META_FILE="$SCRIPT_DIR/official_apk.json"
[[ -f "$META_FILE" ]] || { echo "错误: 缺 ${META_FILE}，先跑 fetch_official_mrfzz.sh" >&2; exit 2; }

VERSION="$(python3 -c 'import json,sys;print(json.load(open(sys.argv[1]))["version_name"])' "$META_FILE")"
VERSION_CODE="$(python3 -c 'import json,sys;print(json.load(open(sys.argv[1]))["version_code"])' "$META_FILE")"
SERIAL="$(python3 -c 'import json,sys;print(json.load(open(sys.argv[1])).get("serial",""))' "$META_FILE")"

RUNTIME_DIR="$ROOT/06_dumped/明日方舟/official/runtime_current"
METADATA="$RUNTIME_DIR/global-metadata.runtime.magicfixed.dat"
[[ -f "$METADATA" ]] || METADATA="$RUNTIME_DIR/global-metadata.runtime.dat"
[[ -f "$METADATA" ]] || { echo "错误: 缺运行时 metadata（${RUNTIME_DIR}），先跑 dump_official_runtime_mrfzz.sh" >&2; exit 2; }

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
  0) echo "[anchor] 全部锚点在 2.7.81 核验通过（详见 ${OUT_MD}）" ;;
  3) echo "[anchor] 有锚点缺失或参数个数不一致 —— 禁止继续装机，先改锚点表" >&2 ;;
  *) echo "[anchor] 核验器输入错误（rc=${rc}）" >&2 ;;
esac
exit "$rc"
