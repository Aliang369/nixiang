#!/system/bin/sh
# On-device helper: locate runtime-decrypted IL2CPP metadata in a live process.
# Args: PID OUT_DIR [METADATA_SIZE]
#
# 两趟策略（版本无关）：
#   第 1 趟（便宜、快）：只看「映射尺寸落在 METADATA_SIZE…METADATA_SIZE+64KiB」的可读段，
#     读 16 字节判首 int32 是否标准 magic(0xFAB11BAF)，或 version∈[16,32] 且 headerSize∈[64,2048]。
#     后者覆盖 2.7.81 这种「sanity/magic 字段被清零」的运行时 metadata。
#   第 2 趟（兜底、慢）：逐段全量 grep 标准 magic。
# 注意：Android mksh 的 $(( )) 是 32 位运算（$((0x754ea38000)) → 0x4ea38000），
#       地址与尺寸运算一律走 awk/od（%d 走 double，48 位以内精确）。
set -eu

PID="$1"
D="$2"
METADATA_SIZE="${3:-44172880}"
[ -n "$PID" ] && [ -n "$D" ] || { echo "usage: search_metadata_mem_device.sh PID DIR [METADATA_SIZE]"; exit 2; }

MAP_LIMIT=536870912
MAGIC="$(printf '\257\033\261\372')"
MAGIC_U32=4207849487   # 0xFAB11BAF

rm -rf "$D"
mkdir -p "$D"
cat /proc/$PID/maps > "$D/maps.txt"
: > "$D/magic_hits.txt"

read_range_start() {
  local range="$1"
  printf '%d' "0x${range%-*}"
}

read_range_end() {
  local range="$1"
  printf '%d' "0x${range#*-}"
}

range_size() {
  awk -v a="$1" -v b="$2" 'BEGIN{printf "%d", b-a}'
}

addr_add() {
  awk -v a="$1" -v b="$2" 'BEGIN{printf "%d", a+b}'
}

addr_hex() {
  awk -v a="$1" 'BEGIN{printf "0x%x", a}'
}

is_uint() {
  case "$1" in
    ''|*[!0-9]*) return 1 ;;
    *) return 0 ;;
  esac
}

dump_at() {
  # $1=start(dec) $2=reason $3=extra
  dump="$D/metadata_runtime_$(addr_hex "$1").bin"
  dd if=/proc/$PID/mem of="$dump" bs=1048576 iflag=skip_bytes,count_bytes \
    skip="$1" count="$METADATA_SIZE" status=none 2>/dev/null || true
  if [ -s "$dump" ]; then
    printf 'dumped=%s size=%s file=%s reason=%s %s\n' \
      "$(addr_hex "$1")" "$(stat -c %s "$dump")" "$dump" "$2" "$3" >> "$D/magic_hits.txt"
    return 0
  fi
  return 1
}

FOUND=0

# ---------- 第 1 趟：尺寸窗口 + 头部自洽 ----------
while read -r range perms _ _ _ _ path; do
  case "$perms" in
    *r*) ;;
    *) continue ;;
  esac
  start="$(read_range_start "$range")"
  end="$(read_range_end "$range")"
  size="$(range_size "$start" "$end")"
  [ "$size" -ge "$METADATA_SIZE" ] || continue
  [ "$size" -le "$(addr_add "$METADATA_SIZE" 65536)" ] || continue

  fields="$(dd if=/proc/$PID/mem bs=16 iflag=skip_bytes,count_bytes skip="$start" count=16 status=none 2>/dev/null |
    od -An -tu4 2>/dev/null | tr -s ' ')"
  [ -n "$(printf '%s' "$fields" | tr -d ' ')" ] || continue
  set -- $fields
  m0="$1"
  ver="$2"
  hsz="$3"

  reason=""
  if [ "$m0" = "$MAGIC_U32" ]; then
    reason="head_magic_size_window"
  elif is_uint "$ver" && is_uint "$hsz"; then
    if [ "$ver" -ge 16 ] && [ "$ver" -le 32 ] && [ "$hsz" -ge 64 ] && [ "$hsz" -le 2048 ]; then
      reason="head_version_size_window"
    fi
  fi
  [ -n "$reason" ] || continue

  echo "range=$range perms=$perms path=${path:-} reason=$reason magic=$m0 version=$ver header_size=$hsz" >> "$D/magic_hits.txt"
  if dump_at "$start" "$reason" "magic=$m0 version=$ver header_size=$hsz"; then
    FOUND=1
    break
  fi
done < "$D/maps.txt"

# ---------- 第 2 趟：全量 magic 扫描兜底 ----------
if [ "$FOUND" = 0 ]; then
  while read -r range perms _ _ _ _ path; do
    case "$perms" in
      *r*) ;;
      *) continue ;;
    esac
    start="$(read_range_start "$range")"
    end="$(read_range_end "$range")"
    size="$(range_size "$start" "$end")"
    [ "$size" -ge 4096 ] || continue
    [ "$size" -le "$MAP_LIMIT" ] || continue

    hits="$(dd if=/proc/$PID/mem bs=1048576 iflag=skip_bytes,count_bytes \
      skip="$start" count="$size" status=none 2>/dev/null | \
      grep -aobF "$MAGIC" 2>/dev/null | head -8 || true)"
    [ -n "$hits" ] || continue

    echo "range=$range perms=$perms path=${path:-} reason=full_magic_scan" >> "$D/magic_hits.txt"
    printf '%s\n' "$hits" | while IFS=: read -r off _; do
      [ -n "$off" ] || continue
      va="$(addr_add "$start" "$off")"
      printf '  hit_va=%s\n' "$(addr_hex "$va")" >> "$D/magic_hits.txt"
    done

    first_off="$(printf '%s\n' "$hits" | sed -n '1s/:.*//p')"
    [ -n "$first_off" ] || continue
    va="$(addr_add "$start" "$first_off")"
    if dump_at "$va" "full_magic_scan" "hit_off=$first_off"; then
      FOUND=1
      break
    fi
  done < "$D/maps.txt"
fi

[ "$FOUND" = 1 ] || echo "not_found metadata_size=$METADATA_SIZE" >> "$D/magic_hits.txt"

find "$D" -maxdepth 1 -type f -print -exec ls -l {} \;
chmod -R a+r "$D" 2>/dev/null || true
echo DONE
