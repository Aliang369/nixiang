#!/system/bin/sh
# 动物大联盟：只读转储指定地址区间内的所有可读匿名段。设备端运行。
# 用法: dump_mem_range_dwdlm.sh <pid> <start_hex> <end_hex> <out_dir>
set -u
PID="$1"
START_HEX="$2"
END_HEX="$3"
OUT_DIR="$4"
[ -n "$PID" ] && [ -n "$START_HEX" ] && [ -n "$END_HEX" ] && [ -n "$OUT_DIR" ] || {
  echo "usage: $0 <pid> <start_hex> <end_hex> <out_dir>" >&2
  exit 2
}

START_DEC="$(printf '%d' "$START_HEX")"
END_DEC="$(printf '%d' "$END_HEX")"
mkdir -p "$OUT_DIR"
i=0
grep -E 'r--p|r-xp|rw-p' "/proc/$PID/maps" |
while read -r range perms off dev ino path; do
  start_hex="${range%-*}"
  end_hex="${range#*-}"
  start_dec="$(printf '%d' "0x${start_hex}")"
  end_dec="$(printf '%d' "0x${end_hex}")"
  [ "$start_dec" -ge "$START_DEC" ] || continue
  [ "$end_dec" -le "$END_DEC" ] || continue
  name="$(printf '%03d_%s_%s_%s.bin' "$i" "$start_hex" "$end_hex" "$perms")"
  if /data/local/tmp/dwdlm_memdump64 "$PID" "0x${start_hex}" "0x${end_hex}" "$OUT_DIR/$name" >/dev/null 2>&1; then
    echo "$name"
  else
    echo "SKIP $range $perms" >&2
  fi
  i=$((i + 1))
done
echo DONE
