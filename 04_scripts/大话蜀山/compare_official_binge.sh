#!/usr/bin/env bash
# 比较《大话蜀山》斌哥改包与 TapTap 官包的游戏核心输入是否一致。
set -euo pipefail

if [[ $# -ne 2 ]]; then
  echo "用法: $0 <binge.apk> <official.apk>" >&2
  exit 1
fi

BINGE="$1"
OFFICIAL="$2"
OUT_DIR="/Users/aliang/Documents/nixiang/06_dumped/大话蜀山"
OUT="$OUT_DIR/official_compare_2.5.txt"

[[ -f "$BINGE" ]] || { echo "错误: 斌哥包不存在: $BINGE" >&2; exit 1; }
[[ -f "$OFFICIAL" ]] || { echo "错误: 官包不存在: $OFFICIAL" >&2; exit 1; }

mkdir -p "$OUT_DIR"

hash_entry() {
  local apk="$1" entry="$2"
  unzip -p "$apk" "$entry" | shasum -a 256 | awk '{print $1}'
}

{
  echo "# 大话蜀山 2.5 斌哥包 / TapTap 官包核心输入对照"
  echo
  echo "- 斌哥包: $(basename "$BINGE") md5=$(md5 -q "$BINGE")"
  echo "- 官包: $(basename "$OFFICIAL") md5=$(md5 -q "$OFFICIAL")"
  echo
  printf '%-52s %-64s %s\n' "entry" "binge_sha256" "match"
  for entry in \
    "lib/arm64-v8a/libil2cpp.so" \
    "lib/arm64-v8a/libunity.so" \
    "assets/bin/Data/Managed/Metadata/global-metadata.dat"; do
    bh="$(hash_entry "$BINGE" "$entry")"
    oh="$(hash_entry "$OFFICIAL" "$entry")"
    if [[ "$bh" == "$oh" ]]; then
      printf '%-52s %-64s yes\n' "$entry" "$bh"
    else
      printf '%-52s %-64s NO official=%s\n' "$entry" "$bh" "$oh"
      exit 1
    fi
  done
} > "$OUT"

cat "$OUT"
echo "完成: $OUT"
