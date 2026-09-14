#!/system/bin/sh
# 4399 无悔华夏：只读扫描进程内存中的 dex magic，输出候选地址。
# 不读设备文件、不修改进程；由本地脚本 push 后以 root 执行。
set -u

PID="$1"
OUT="${2:-/data/local/tmp/whrhx4399_dexscan}"
TMP="$OUT/chunk.bin"

[ -n "$PID" ] || { echo "usage: $0 PID [OUT]" >&2; exit 2; }
[ -r "/proc/$PID/maps" ] || { echo "process not found: $PID" >&2; exit 1; }

rm -rf "$OUT"
mkdir -p "$OUT"
cp "/proc/$PID/maps" "$OUT/maps"
: > "$OUT/dex_candidates.txt"

grep -E ' r.-- ' "/proc/$PID/maps" | while read -r line; do
  set -- $line
  range="$1"
  perm="$2"
  path="$6"
  start_hex="${range%-*}"
  end_hex="${range#*-}"
  start=$(printf '%d' "0x$start_hex")
  end=$(printf '%d' "0x$end_hex")
  size=$((end - start))

  # 只扫可能承载运行时 dex 的中等大小区域，避开巨大图形/资源映射。
  [ "$size" -ge 65536 ] || continue
  [ "$size" -le 268435456 ] || continue

  off=0
  while [ "$off" -lt "$size" ]; do
    chunk=$((size - off))
    [ "$chunk" -gt 4194304 ] && chunk=4194304
    dd if="/proc/$PID/mem" of="$TMP" bs=1048576 \
      iflag=skip_bytes,count_bytes skip=$((start + off)) count="$chunk" \
      status=none 2>/dev/null || break
    pos=$(grep -abo $'\x64\x65\x78\x0a\x30\x33\x35\x00' "$TMP" 2>/dev/null | cut -d: -f1)
    while [ -n "$pos" ]; do
      abs=$((start + off + pos))
      printf '0x%x\t%s\t%s\t%s\n' "$abs" "$perm" "$path" "$range" >> "$OUT/dex_candidates.txt"
      next=$((pos + 1))
      rest=$(dd if="$TMP" bs=1 skip="$next" status=none 2>/dev/null | grep -abo $'\x64\x65\x78\x0a\x30\x33\x35\x00' 2>/dev/null | cut -d: -f1)
      [ -n "$rest" ] || break
      pos=$((next + rest))
    done
    off=$((off + chunk - 8))
  done
done

rm -f "$TMP"
sort -u "$OUT/dex_candidates.txt" > "$OUT/dex_candidates.sorted.txt" 2>/dev/null || true
mv "$OUT/dex_candidates.sorted.txt" "$OUT/dex_candidates.txt" 2>/dev/null || true
wc -l "$OUT/dex_candidates.txt"
chmod -R a+r "$OUT" 2>/dev/null || true
echo DEX_SCAN_DONE
