#!/system/bin/sh
# 扫进程 rw 段找解密后的 global-metadata 魔数 0xFAB11BAF，报告段与偏移
# Args: PID
PID="$1"
[ -n "$PID" ] || { echo "usage: $0 PID"; exit 2; }
TMP=/data/local/tmp/meta_scan.bin
grep -E 'rw-p' /proc/$PID/maps | grep -vE '(/\[|dev/|/dev)' | while read -r line; do
  range=${line%% *}; s=${range%-*}; e=${range#*-}
  start=$(printf '%d' "0x$s"); end=$(printf '%d' "0x$e"); size=$((end-start))
  [ "$size" -ge 1048576 ] || continue
  [ "$size" -le 134217728 ] || continue
  # 1MB 分块扫，带 4 字节重叠
  off=0
  while [ "$off" -lt "$size" ]; do
    chunk=$((size - off)); [ "$chunk" -gt 4194304 ] && chunk=4194304
    dd if=/proc/$PID/mem of=$TMP bs=1048576 iflag=skip_bytes,count_bytes \
      skip=$((start+off)) count=$chunk status=none 2>/dev/null
    pos=$(grep -abo $'\xfa\xb1\xb1\xfa' $TMP 2>/dev/null | head -1 | cut -d: -f1)
    if [ -n "$pos" ]; then
      abs=$((start+off+pos))
      echo "METADATA_MAGIC at 0x$(printf '%x' $abs) (seg 0x$s size=$size)"
    fi
    off=$((off + chunk - 4))
  done
done
rm -f $TMP
echo SCAN_DONE
