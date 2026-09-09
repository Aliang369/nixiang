#!/system/bin/sh
# es2 补充 dump：壳载体 so 全段 + il2cpp/xlua 数据段 + 全部 anon .bss + rwx
# Args: PID DEV_DIR
set -e
PID="$1"
D="$2"
[ -n "$PID" ] && [ -n "$D" ] || { echo "usage: dump_es2_ext_device.sh PID DIR"; exit 2; }
rm -rf "$D"; mkdir -p "$D"

dump_hex() {
  s_hex="$1"; e_hex="$2"; name="$3"
  start=$(printf '%d' "0x${s_hex}")
  end=$(printf '%d' "0x${e_hex}")
  size=$((end - start))
  echo "DUMP $name 0x${s_hex}-0x${e_hex} size=$size"
  dd if=/proc/$PID/mem of="$D/$name" bs=1048576 iflag=skip_bytes,count_bytes \
    skip=$start count=$size status=none
}

# 1) 壳载体 so 全段（codec2_server / unityluac / SDKloading 全 perm）
for SO in libcodec2_server.so libunityluac.so libSDKloading.so; do
  IDX=0
  grep " $SO\$" /proc/$PID/maps | while read -r line; do
    range=${line%% *}; s=${range%-*}; e=${range#*-}
    perm=$(echo "$line" | awk '{print $2}')
    case "$perm" in
      r-xp|rw-p|r--p|rwxp)
        IDX=$((IDX+1))
        dump_hex "$s" "$e" "$(echo $SO | sed 's/lib//;s/\.so//')_${perm}_${IDX}.bin"
        ;;
    esac
  done
done

# 2) il2cpp 数据段（rw-p，每段 <=64MB 防爆盘）
IDX=0
grep ' libil2cpp.so$' /proc/$PID/maps | while read -r line; do
  perm=$(echo "$line" | awk '{print $2}')
  [ "$perm" = "rw-p" ] || continue
  range=${line%% *}; s=${range%-*}; e=${range#*-}
  start=$(printf '%d' "0x$s"); end=$(printf '%d' "0x$e"); size=$((end-start))
  [ "$size" -le 67108864 ] || continue
  IDX=$((IDX+1))
  dump_hex "$s" "$e" "il2cpp_rw_${IDX}.bin"
done

# 3) xlua / main r-x + rw
for SO in libxlua.so libmain.so; do
  grep " $SO\$" /proc/$PID/maps | while read -r line; do
    range=${line%% *}; s=${range%-*}; e=${range#*-}
    perm=$(echo "$line" | awk '{print $2}')
    case "$perm" in
      r-xp|rw-p) dump_hex "$s" "$e" "$(echo $SO | sed 's/lib//;s/\.so//')_${perm}.bin" ;;
    esac
  done
done

# 4) 全部 [anon:.bss]（不限位置，rw/r-x 各限 8MB）
IDX=0
grep '\[anon:\.bss\]' /proc/$PID/maps | while read -r line; do
  perm=$(echo "$line" | awk '{print $2}')
  case "$perm" in
    rw-p|r-xp|rwxp) ;;
    *) continue ;;
  esac
  range=${line%% *}; s=${range%-*}; e=${range#*-}
  start=$(printf '%d' "0x$s"); end=$(printf '%d' "0x$e"); size=$((end-start))
  [ "$size" -le 8388608 ] || continue
  IDX=$((IDX+1))
  dump_hex "$s" "$e" "anonbss_${perm}_${IDX}.bin"
done

# 5) 大块匿名 rwx（壳 trampoline 候选）
IDX=0
awk '$2=="rwxp" && NF==5' /proc/$PID/maps | while read -r range rest; do
  s=${range%-*}; e=${range#*-}
  start=$(printf '%d' "0x$s"); end=$(printf '%d' "0x$e"); size=$((end-start))
  [ "$size" -ge 65536 ] || continue
  [ "$size" -le 8388608 ] || continue
  IDX=$((IDX+1))
  dump_hex "$s" "$e" "anon_rwx_${IDX}.bin"
done

ls -lah "$D"
chmod -R a+r "$D" 2>/dev/null || true
echo DONE
