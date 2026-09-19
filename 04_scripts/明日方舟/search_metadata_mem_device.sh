#!/system/bin/sh
# On-device helper: locate runtime-decrypted IL2CPP metadata by its standard magic.
# Args: PID OUT_DIR
set -eu

PID="$1"
D="$2"
[ -n "$PID" ] && [ -n "$D" ] || { echo "usage: search_metadata_mem_device.sh PID DIR"; exit 2; }

METADATA_SIZE=43646828
METADATA_MAP_SIZE=43646976
MAP_LIMIT=536870912
MAGIC="$(printf '\257\033\261\372')"

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

while read -r range perms _ _ _ _ path; do
  case "$perms" in
    *r*) ;;
    *) continue ;;
  esac
  start="$(read_range_start "$range")"
  end="$(read_range_end "$range")"
  size=$((end - start))
  [ "$size" -ge 4096 ] || continue
  [ "$size" -le "$MAP_LIMIT" ] || continue

  if [ "$size" -eq "$METADATA_MAP_SIZE" ]; then
    echo "range=$range perms=$perms path=${path:-} reason=exact_page_aligned_size" >> "$D/magic_hits.txt"
    dump="$D/metadata_runtime_${start}.bin"
    dd if=/proc/$PID/mem of="$dump" bs=1048576 iflag=skip_bytes,count_bytes \
      skip="$start" count="$METADATA_SIZE" status=none 2>/dev/null || true
    if [ -s "$dump" ]; then
      printf 'dumped=0x%x size=%s file=%s reason=size\n' "$start" "$(stat -c %s "$dump")" "$dump" >> "$D/magic_hits.txt"
      break
    fi
  fi

  hits="$(dd if=/proc/$PID/mem bs=1048576 iflag=skip_bytes,count_bytes \
    skip="$start" count="$size" status=none 2>/dev/null | \
    grep -aobF "$MAGIC" 2>/dev/null | head -8 || true)"
  [ -n "$hits" ] || continue

  echo "range=$range perms=$perms path=${path:-}" >> "$D/magic_hits.txt"
  printf '%s\n' "$hits" | while IFS=: read -r off _; do
    [ -n "$off" ] || continue
    va=$((start + off))
    printf '  hit_va=0x%x\n' "$va" >> "$D/magic_hits.txt"
  done

  first_off="$(printf '%s\n' "$hits" | sed -n '1s/:.*//p')"
  [ -n "$first_off" ] || continue
  va=$((start + first_off))
  dump="$D/metadata_runtime_${va}.bin"
  dd if=/proc/$PID/mem of="$dump" bs=1048576 iflag=skip_bytes,count_bytes \
    skip="$va" count="$METADATA_SIZE" status=none 2>/dev/null || true
  if [ -s "$dump" ]; then
    printf 'dumped=0x%x size=%s file=%s\n' "$va" "$(stat -c %s "$dump")" "$dump" >> "$D/magic_hits.txt"
    break
  fi
done < "$D/maps.txt"

find "$D" -maxdepth 1 -type f -print -exec ls -l {} \;
chmod -R a+r "$D" 2>/dev/null || true
echo DONE
