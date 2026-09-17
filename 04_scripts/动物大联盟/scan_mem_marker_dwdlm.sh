#!/system/bin/sh
# 动物大联盟：在目标进程可写匿名段中只读搜索字符串；运行于设备端。
# 用法: scan_mem_marker_dwdlm.sh <pid> <marker>
set -u
PID="$1"
MARKER="$2"
[ -n "$PID" ] && [ -n "$MARKER" ] || { echo "usage: $0 <pid> <marker>" >&2; exit 2; }

grep 'rw-p' "/proc/$PID/maps" |
awk '$6 == "" || $6 == "[anon:libc_malloc]" || $6 == "[anon:.bss]" { print }' |
while read -r range perms off dev ino path; do
  start_hex="${range%-*}"
  end_hex="${range#*-}"
  start_dec="$(printf '%d' "0x${start_hex}")"
  end_dec="$(printf '%d' "0x${end_hex}")"
  size="$((end_dec - start_dec))"
  [ "$size" -gt 0 ] || continue
  [ "$size" -lt 536870912 ] || continue
  hits="$(dd if="/proc/$PID/mem" bs=1048576 iflag=skip_bytes,count_bytes \
    skip="$start_dec" count="$size" 2>/dev/null |
    grep -a -b -m 3 -F "$MARKER" || true)"
  if [ -n "$hits" ]; then
    echo "HIT range=${range} size=${size} path=${path:-anon}"
    printf '%s\n' "$hits"
  fi
done
echo DONE
