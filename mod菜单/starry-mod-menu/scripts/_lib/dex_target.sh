#!/usr/bin/env bash
# dex_target.sh —— 选「星光 smali / 新增类可以合并进去」的 dex 落点（**禁止硬编码 classesN**）
#
# 背景（2026-10-10 追迹迷城 1.0.20261009 实测）：同一游戏换版本会整段改变 dex 布局
# （0912 版是 classes.dex…classes6.dex，1009 版只剩 4 个），按旧版写死 classes5/classes6
# 的脚本会直接扑空；而 classes3.dex 常贴着 65533/65536 的方法数上限，合进去必然溢出。
# 所以落点必须**按余量算出来**，并在余量不足时硬失败。
#
# 用法：
#   bash dex_target.sh --apk <apk> [--need-methods N] [--print-entry]
#   bash dex_target.sh --dex <classesN.dex> [--need-methods N] [--print-entry]
#
#   --need-methods N  需要的方法数余量（默认 2048）；不足则退出码 2
#   --print-entry     只向 stdout 打印选中的条目名（诊断信息走 stderr），便于脚本取值：
#                       MERGE_DEX="$(bash dex_target.sh --apk "$APK" --need-methods 8192 --print-entry)"
#
# 退出码：0 选定成功 / 1 用法或解析错误 / 2 无候选满足 --need-methods
set -uo pipefail

MAX_METHODS=65536
NEED=2048
PRINT_ENTRY=0
APK=""
DEXX=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    --apk) APK="${2:-}"; shift 2 ;;
    --dex) DEXX="${2:-}"; shift 2 ;;
    --need-methods) NEED="${2:-}"; shift 2 ;;
    --print-entry) PRINT_ENTRY=1; shift ;;
    -h|--help) sed -n '2,20p' "$0"; exit 0 ;;
    *) echo "ERROR: unknown arg $1" >&2; exit 1 ;;
  esac
done

if [[ -z "$APK" && -z "$DEXX" ]]; then
  echo "ERROR: 需要 --apk <apk> 或 --dex <classesN.dex>" >&2
  exit 1
fi
if [[ -n "$APK" && ! -f "$APK" ]]; then
  echo "ERROR: 找不到 APK: $APK" >&2
  exit 1
fi
if [[ -n "$DEXX" && ! -f "$DEXX" ]]; then
  echo "ERROR: 找不到 dex: $DEXX" >&2
  exit 1
fi
case "$NEED" in
  ''|*[!0-9]*) echo "ERROR: --need-methods 需要非负整数，收到 '$NEED'" >&2; exit 1 ;;
esac

# dex 头字节 -> "file_size method_ids class_defs"
# 偏移：file_size=0x20, method_ids_size=0x58, class_defs_size=0x60（u32 小端）
read_header() {
  od -An -tu4 | tr -s ' \n' ' ' | awk '{
    if (NF < 25) { print "0 0 0"; exit }
    printf "%s %s %s\n", $9, $23, $25
  }'
}

stats_of_entry() {  # $1 = apk 内条目名；输出三值
  unzip -p "$APK" "$1" 2>/dev/null | head -c 128 | read_header
}

list_entries() {  # 按 dex 编号升序输出条目名（避免依赖 sort -V）
  unzip -Z1 "$APK" 2>/dev/null | grep -E '^classes[0-9]*\.dex$' \
    | awk '{ n = ($0 == "classes.dex") ? 1 : substr($0, 8) + 0; printf "%d %s\n", n, $0 }' \
    | sort -n | awk '{ print $2 }'
}

best_name=""
best_spare=-1
best_mid=0
best_cls=0
lines=""

if [[ -n "$APK" ]]; then
  entries="$(list_entries)"
  if [[ -z "$entries" ]]; then
    echo "ERROR: 包内找不到 classes*.dex（${APK}）" >&2
    exit 1
  fi
else
  entries="$(basename "$DEXX")"
fi

for name in $entries; do
  if [[ -n "$APK" ]]; then
    set -- $(stats_of_entry "$name")
  else
    set -- $(head -c 128 "$DEXX" | read_header)
  fi
  fsize="${1:-0}"; mid="${2:-0}"; cls="${3:-0}"
  if [[ "$mid" = "0" && "$fsize" = "0" ]]; then
    echo "WARN: 无法解析 $name 头部，跳过" >&2
    continue
  fi
  spare=$(( MAX_METHODS - mid ))
  lines="$lines$(printf '  %-14s size=%-10s method_ids=%-6s class_defs=%-6s spare=%s\n' "$name" "$fsize" "$mid" "$cls" "$spare")"$'\n'
  if [[ "$spare" -gt "$best_spare" ]]; then
    best_spare="$spare"; best_name="$name"; best_mid="$mid"; best_cls="$cls"
  fi
done

if [[ -z "$best_name" ]]; then
  echo "ERROR: 没有可用的 dex 候选" >&2
  exit 1
fi

if [[ "$PRINT_ENTRY" = "1" ]]; then
  if [[ "$best_spare" -lt "$NEED" ]]; then
    printf '%s\n' "$lines" >&2
    echo "ERROR: 无候选满足 --need-methods ${NEED}（最大余量 $best_spare 在 ${best_name}）" >&2
    exit 2
  fi
  printf '%s\n' "$best_name"
  exit 0
fi

echo "[dex_target] ${APK:-$DEXX}"
printf '%s' "$lines"
if [[ "$best_spare" -lt "$NEED" ]]; then
  echo "[dex_target] ERROR: 无候选满足 need-methods=${NEED}（最大余量 $best_spare 在 ${best_name}）"
  exit 2
fi
echo "[dex_target] 选定 ${best_name}（method_ids=$best_mid class_defs=$best_cls spare=$best_spare ≥ need=${NEED}）"
echo "[dex_target] 结论：合并落点 = ${best_name}（禁止在脚本里硬编码 classesN）"
exit 0
