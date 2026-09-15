#!/system/bin/sh
# 勇士之心：在进程 writable 匿名段中只读搜索指定标记。
# 用法: scan_runtime_marker.sh <pid> <marker>
set -e
PID="$1"
MARKER="$2"
[ -n "$PID" ] && [ -n "$MARKER" ] || { echo "usage: $0 <pid> <marker>" >&2; exit 2; }

grep -E 'rw-p .*\[anon:libc_malloc\]|rw-p .*\[anon:\.bss\]' "/proc/$PID/maps" |
while read -r range perms off dev ino path; do
  start_hex="${range%-*}"
  end_hex="${range#*-}"
  start_dec="$(printf '%d' "0x${start_hex}")"
  end_dec="$(printf '%d' "0x${end_hex}")"
  size="$((end_dec - start_dec))"
  [ "$size" -gt 0 ] || continue
  hits="$(dd if="/proc/$PID/mem" bs=1048576 iflag=skip_bytes,count_bytes \
    skip="$start_dec" count="$size" 2>/dev/null |
    grep -a -b -m 3 -F "$MARKER" || true)"
  if [ -n "$hits" ]; then
    echo "HIT range=${range} size=${size} path=${path}"
    printf '%s\n' "$hits"
  fi
done
echo DONE
