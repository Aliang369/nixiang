#!/system/bin/sh
# 动物大联盟：在目标进程全部可读匿名段中只读搜索扩展正则；运行于设备端。
# 用法: scan_mem_all_regex_dwdlm.sh <pid> <extended-regex>
set -u
PID="$1"
PATTERN="$2"
[ -n "$PID" ] && [ -n "$PATTERN" ] || { echo "usage: $0 <pid> <extended-regex>" >&2; exit 2; }

grep -E 'r--p|r-xp|rw-p' "/proc/$PID/maps" |
awk '$6 == "" || $6 ~ /^\[anon:/ { print }' |
while read -r range perms off dev ino path; do
  start_hex="${range%-*}"
  end_hex="${range#*-}"
  start_dec="$(printf '%d' "0x${start_hex}")"
  end_dec="$(printf '%d' "0x${end_hex}")"
  size="$((end_dec - start_dec))"
  [ "$size" -gt 0 ] || continue
  [ "$size" -lt 134217728 ] || continue
  hits="$(dd if="/proc/$PID/mem" bs=1048576 iflag=skip_bytes,count_bytes \
    skip="$start_dec" count="$size" 2>/dev/null |
    grep -a -b -m 20 -E "$PATTERN" || true)"
  if [ -n "$hits" ]; then
    echo "HIT range=${range} perms=${perms} size=${size} path=${path:-anon}"
    printf '%s\n' "$hits"
  fi
done
echo DONE
