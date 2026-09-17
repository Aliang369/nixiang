#!/system/bin/sh
# 动物大联盟：冷启动早期只读采样 libcocos 的 XXTEA key 全局槽。
# 用法: capture_startup_key_device_dwdlm.sh
set -u
PKG="com.fightingstars.games.animal"
ACT="dev.mars.secure.StubActivity"
KEY_OFF=35770408  # 0x221d028

am force-stop "$PKG"
sleep 0.2
monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1 &

i=0
while [ "$i" -lt 1200 ]; do
  pid="$(pidof "$PKG" 2>/dev/null | awk '{print $1}')"
  if [ -n "$pid" ] && [ -r "/proc/$pid/maps" ]; then
    line="$(grep 'libcocos2djs.so' "/proc/$pid/maps" 2>/dev/null | head -n 1)"
    if [ -n "$line" ]; then
      start_hex="${line%%-*}"
      start_hex="$(printf '%s' "$line" | awk '{print $1}' | cut -d- -f1)"
      base_dec="$(printf '%d' "0x${start_hex}" 2>/dev/null || true)"
      if [ -n "$base_dec" ]; then
        addr="$(awk -v b="$base_dec" 'BEGIN { printf "%.0f", b + 35770408 }')"
        bytes="$(dd if="/proc/$pid/mem" bs=1 iflag=skip_bytes,count_bytes skip="$addr" count=64 2>/dev/null | od -An -tx1 | tr -d ' \n')"
        if [ -n "$bytes" ]; then
          first="$(printf '%.2s' "$bytes")"
          if [ "$first" != "00" ]; then
            echo "CAPTURED iter=$i pid=$pid base=0x$start_hex addr=$addr"
            echo "$bytes"
            exit 0
          fi
        fi
      fi
    fi
  fi
  i=$((i + 1))
  usleep 50000 2>/dev/null || sleep 0.05
done
echo "TIMEOUT pid=${pid:-none}"
exit 1
