#!/system/bin/sh
# On-device helper. Args: PID DEV_DIR
set -e
PID="$1"
D="$2"
[ -n "$PID" ] && [ -n "$D" ] || { echo "usage: dump_binge_shell_mem_device.sh PID DIR"; exit 2; }
rm -rf "$D"
mkdir -p "$D"

dump_hex() {
  s_hex="$1"; e_hex="$2"; name="$3"
  start=$(printf '%d' "0x${s_hex}")
  end=$(printf '%d' "0x${e_hex}")
  size=$((end - start))
  echo "DUMP $name 0x${s_hex}-0x${e_hex} size=$size"
  dd if=/proc/$PID/mem of="$D/$name" bs=1048576 iflag=skip_bytes,count_bytes \
    skip=$start count=$size status=none
  ls -l "$D/$name"
}

echo "PID=$PID"
grep -E 'SDKloading|libil2cpp|\[anon:\.bss\]' /proc/$PID/maps | head -80

IN=0
RX_IDX=0
SAW_BSS=0
while read -r line; do
  case "$line" in
    *libSDKloading.so*) IN=1 ;;
  esac
  [ "$IN" = "1" ] || continue
  range=${line%% *}
  s=${range%-*}
  e=${range#*-}
  perm=$(echo "$line" | awk '{print $2}')
  path=$(echo "$line" | awk '{print $6}')
  case "$line" in
    *libSDKloading.so*)
      case "$perm" in
        r-xp) RX_IDX=$((RX_IDX+1)); dump_hex "$s" "$e" "sdk_rx_${RX_IDX}.bin" ;;
        r--p) dump_hex "$s" "$e" sdk_ro.bin ;;
        rw-p) dump_hex "$s" "$e" sdk_rw.bin ;;
      esac
      ;;
    *'[anon:.bss]'*)
      SAW_BSS=1
      case "$perm" in
        r-xp) dump_hex "$s" "$e" shell_rx_bss.bin ;;
        r--p) dump_hex "$s" "$e" shell_ro_bss.bin ;;
        rw-p) dump_hex "$s" "$e" shell_rw_bss.bin ;;
        rwxp) dump_hex "$s" "$e" shell_rwx_bss.bin ;;
      esac
      ;;
    *)
      case "$perm" in
        ---p) continue ;;
        r-xp|rwxp)
          if [ -z "$path" ]; then
            start=$(printf '%d' "0x$s"); end=$(printf '%d' "0x$e"); size=$((end-start))
            if [ "$size" -ge 4096 ] && [ "$size" -le 16777216 ]; then
              dump_hex "$s" "$e" "near_exec_${s}.bin"
            fi
          fi
          if [ "$SAW_BSS" = "1" ] && [ -n "$path" ]; then
            case "$path" in
              \[*) ;;
              *) break ;;
            esac
          fi
          ;;
        *)
          if [ "$SAW_BSS" = "1" ] && [ -n "$path" ]; then
            case "$path" in
              \[*) ;;
              *) break ;;
            esac
          fi
          ;;
      esac
      ;;
  esac
done < /proc/$PID/maps

IDX=0
: > "$D/il2cpp_rx_segs.txt"
while read -r line; do
  case "$line" in *libil2cpp.so*) ;; *) continue ;; esac
  echo "$line" | grep -q 'r-xp' || continue
  range=${line%% *}
  s=${range%-*}
  e=${range#*-}
  foff=$(echo "$line" | awk '{print $3}')
  IDX=$((IDX+1))
  dump_hex "$s" "$e" "il2cpp_rx_${IDX}.bin"
  echo "0x${s} 0x${e} 0x${foff}" >> "$D/il2cpp_rx_segs.txt"
done < /proc/$PID/maps

IDX=0
while read -r line; do
  perm=$(echo "$line" | awk '{print $2}')
  [ "$perm" = "rwxp" ] || continue
  path=$(echo "$line" | awk '{print $6}')
  [ -z "$path" ] || continue
  range=${line%% *}
  s=${range%-*}
  e=${range#*-}
  start=$(printf '%d' "0x$s"); end=$(printf '%d' "0x$e"); size=$((end-start))
  [ "$size" -ge 65536 ] || continue
  [ "$size" -le 4194304 ] || continue
  IDX=$((IDX+1))
  dump_hex "$s" "$e" "anon_rwx_${IDX}.bin"
done < /proc/$PID/maps

ls -lah "$D"
chmod -R a+r "$D" 2>/dev/null || true
echo DONE
